using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Records a playtest so it can be read afterwards (by Claude, through Tools/Playtest/analyze.py,
    /// or by hand). Every Play session writes a folder under PlaytestLogs/ in the project:
    ///
    ///   session.md          what was recorded, the settings, and the summary (swings, falls, tracking)
    ///   events.log          one line per thing that happened, with the reasons the code gave
    ///   meta.json           machine-readable index: units, files, parts, marks
    ///   config/*.json       the unit profiles, attacks and weapons as they were (and if changed in play)
    ///   units/NN_Name.frames.csv.gz   one row per physics step: state, balance, swing, joint errors
    ///   units/NN_Name.pose.csv.gz     every part's physical position/rotation and its ghost position
    ///   world.csv.gz        props (crates, barrels) positions
    ///
    /// Samples are taken at the start of each physics step, before the units run: the body as PhysX
    /// left it, against the ghost pose it was chasing. Nothing here changes how the game plays.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class PlaytestRecorder : MonoBehaviour
    {
        [Tooltip("Start recording when Play starts.")]
        public bool recordOnPlay = true;

        [Tooltip("Write the full pose every N physics steps (1 = every step, 60 per second).")]
        [Range(1, 6)] public int poseEvery = 1;

        public static PlaytestRecorder Active { get; private set; }

        private readonly List<UnitTrack> tracks = new List<UnitTrack>();
        private readonly List<UnitTrack> closedTracks = new List<UnitTrack>();
        private readonly Dictionary<UnitController, UnitTrack> byUnit = new Dictionary<UnitController, UnitTrack>();
        private readonly Dictionary<string, int> nameUses = new Dictionary<string, int>();
        private readonly Dictionary<ScriptableObject, string> configs = new Dictionary<ScriptableObject, string>();
        private readonly List<string> marks = new List<string>();
        private readonly List<string> consoleProblems = new List<string>();
        private readonly Dictionary<Rigidbody, int> props = new Dictionary<Rigidbody, int>();
        private readonly StringBuilder line = new StringBuilder(512);
        private readonly List<UnitController> pending = new List<UnitController>();

        private StreamWriter events;
        private TableWriter world;
        private string sessionPath;
        private DateTime startedAt;
        private float startTime;
        private int step;
        private int eventCount;
        private int unitCount;
        private float lastTimeScale = 1f;
        private bool inLogCallback;

        public bool Recording => events != null;
        public string SessionName { get; private set; } = "";
        public string SessionPath => sessionPath;
        public int MarkCount => marks.Count;
        public int EventCount => eventCount;

        /// <summary>PlaytestLogs/ next to Assets/.</summary>
        public static string LogRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "PlaytestLogs"));

        private void OnEnable()
        {
            Active = this;
            if (recordOnPlay)
            {
                StartSession();
            }
        }

        private void OnDisable()
        {
            StopSession();
            if (Active == this)
            {
                Active = null;
            }
        }

        // ---- Session ---------------------------------------------------------------------------

        public void StartSession()
        {
            if (Recording)
            {
                return;
            }

            try
            {
                startedAt = DateTime.Now;
                SessionName = startedAt.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
                sessionPath = Path.Combine(LogRoot, SessionName);
                Directory.CreateDirectory(Path.Combine(sessionPath, "units"));
                Directory.CreateDirectory(Path.Combine(sessionPath, "config"));
                events = new StreamWriter(Path.Combine(sessionPath, "events.log"), false, new UTF8Encoding(false));
                world = new TableWriter(Path.Combine(sessionPath, "world.csv.gz"));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Playtest] Could not start recording in {sessionPath}: {exception.Message}");
                events = null;
                return;
            }

            startTime = Time.time;
            step = 0;
            eventCount = 0;
            unitCount = 0;
            tracks.Clear();
            closedTracks.Clear();
            byUnit.Clear();
            nameUses.Clear();
            configs.Clear();
            marks.Clear();
            consoleProblems.Clear();
            props.Clear();
            pending.Clear();
            lastTimeScale = Time.timeScale;

            events.WriteLine("# Physics Battler playtest " + SessionName);
            events.WriteLine("# time (s since Play) | physics step | who | what | details");
            Event((string)null, "session", $"recording started: Unity {Application.unityVersion}, physics {1f / Time.fixedDeltaTime:0} Hz, "
                                           + $"solver {Physics.defaultSolverIterations}/{Physics.defaultSolverVelocityIterations} iterations, time scale {Time.timeScale:0.00}");

            PlaytestTrace.Noted += OnTrace;
            CombatEvents.HitLanded += OnHit;
            UnitController.Initialized += OnUnitInitialized;
            Application.logMessageReceived += OnConsole;

            foreach (var unit in FindObjectsByType<UnitController>(FindObjectsSortMode.None))
            {
                if (unit.IsInitialized)
                {
                    Follow(unit);
                }
            }

            WriteMeta(false);
            WriteSummary(false);
        }

        public void StopSession()
        {
            if (!Recording)
            {
                return;
            }

            PlaytestTrace.Noted -= OnTrace;
            CombatEvents.HitLanded -= OnHit;
            UnitController.Initialized -= OnUnitInitialized;
            Application.logMessageReceived -= OnConsole;

            foreach (var track in tracks.ToArray())
            {
                CloseTrack(track, "recording stopped");
            }

            CheckConfigChanges();
            Event((string)null, "session", $"recording stopped after {Time.time - startTime:0.0} s ({step} physics steps)");
            WriteMeta(true);
            WriteSummary(true);

            try
            {
                events.Dispose();
                world.Dispose();
            }
            catch (Exception)
            {
                // The editor is shutting down; the files are as complete as they will get.
            }

            events = null;
            world = null;
            Debug.Log($"[Playtest] Saved {SessionPath}");
        }

        /// <summary>Ends this log and starts a fresh one.</summary>
        public void NewSession()
        {
            StopSession();
            StartSession();
        }

        /// <summary>Drops a numbered marker into the log, with what every unit is doing at that moment.</summary>
        public void Mark(string note = null)
        {
            if (!Recording)
            {
                return;
            }

            int number = marks.Count + 1;
            marks.Add($"{number}|{Time.time:0.000}|{note}");
            Event((string)null, "MARK", $"===== mark {number} ====={(string.IsNullOrEmpty(note) ? "" : " " + note)}");
            foreach (var track in tracks)
            {
                Event(track, "mark", track.Snapshot());
            }

            WriteMeta(false);
        }

        // ---- Following units -------------------------------------------------------------------

        // Units announce themselves from Awake, inside Instantiate, before the spawner has named them
        // and given them a weapon: start following them at the next physics step.
        private void OnUnitInitialized(UnitController unit) => pending.Add(unit);

        private void Follow(UnitController unit)
        {
            if (!Recording || unit == null || byUnit.ContainsKey(unit))
            {
                return;
            }

            string baseName = unit.name;
            nameUses.TryGetValue(baseName, out int uses);
            nameUses[baseName] = ++uses;
            string label = uses > 1 ? $"{baseName} #{uses}" : baseName;
            int id = ++unitCount;
            string stem = $"{id:00}_{Sanitize(label)}";
            var track = new UnitTrack(this, unit, id, label, stem,
                Path.Combine(sessionPath, "units", stem + ".frames.csv.gz"),
                Path.Combine(sessionPath, "units", stem + ".pose.csv.gz"));
            tracks.Add(track);
            byUnit[unit] = track;

            var rig = unit.Rig;
            Event(track, "spawn", $"at ({unit.Root.position.x:0.00}, {unit.Root.position.z:0.00}) facing {Heading(unit.Root.forward)}, team {unit.Team}, "
                                  + $"mass {rig.TotalMass():0.0} kg, scale {unit.Scale:0.00}, rest pelvis height {unit.RestPelvisHeight:0.00} m, "
                                  + $"profile {(unit.Profile != null ? unit.Profile.name : "default")}"
                                  + (unit.Weapon != null ? $", holding {unit.Weapon.name.Replace("(Clone)", "")}" : ""));
            NoteConfig(unit.Profile);
            if (unit.Weapon != null)
            {
                NoteConfig(unit.Weapon.Definition);
            }

            WriteMeta(false);
        }

        private void CloseTrack(UnitTrack track, string why)
        {
            Event(track, "despawn", why);
            track.Close(why);
            tracks.Remove(track);
            closedTracks.Add(track);
            if (track.unit != null)
            {
                byUnit.Remove(track.unit);
            }
        }

        public string LabelOf(UnitController unit)
        {
            if (unit == null)
            {
                return "?";
            }

            return byUnit.TryGetValue(unit, out var track) ? track.label : unit.name;
        }

        /// <summary>A short description of a collider for the log: "Orc B Head", "Orc B's club", "the ground", "Crate".</summary>
        public string Describe(Collider collider)
        {
            if (collider == null)
            {
                return "?";
            }

            var weapon = collider.GetComponentInParent<Weapon>();
            if (weapon != null)
            {
                return weapon.Owner != null ? LabelOf(weapon.Owner) + "'s club" : "a dropped club";
            }

            var part = collider.GetComponentInParent<BodyPart>();
            if (part != null && part.Owner != null)
            {
                return LabelOf(part.Owner) + " " + part.id;
            }

            if (collider.gameObject.layer == LayerMask.NameToLayer(Layers.Ground))
            {
                return "the ground";
            }

            return collider.attachedRigidbody != null ? collider.attachedRigidbody.name.Replace("(Clone)", "") : collider.name;
        }

        // ---- Sampling --------------------------------------------------------------------------

        private void FixedUpdate()
        {
            if (!Recording)
            {
                return;
            }

            step++;
            if (pending.Count > 0)
            {
                foreach (var unit in pending)
                {
                    Follow(unit);
                }

                pending.Clear();
            }

            bool writePose = step % Mathf.Max(1, poseEvery) == 0;
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                var track = tracks[i];
                if (!track.Alive)
                {
                    CloseTrack(track, "removed from the scene");
                    continue;
                }

                try
                {
                    track.Sample(step, writePose);
                }
                catch (Exception exception)
                {
                    Event(track, "recorder", "sampling failed (recording of this unit stops): " + exception.Message);
                    CloseTrack(track, "recorder error");
                }
            }

            if (step % 30 == 1)
            {
                FindProps();
            }

            if (step % 2 == 0)
            {
                SampleProps();
            }

            if (step % 30 == 0)
            {
                Flush();
            }
        }

        private void Update()
        {
            if (Recording && !Mathf.Approximately(Time.timeScale, lastTimeScale))
            {
                Event((string)null, "time", Time.timeScale == 0f ? "paused" : $"time scale {Time.timeScale:0.00}");
                lastTimeScale = Time.timeScale;
            }
        }

        private void FindProps()
        {
            int propLayer = LayerMask.NameToLayer(Layers.Prop);
            foreach (var body in FindObjectsByType<Rigidbody>(FindObjectsSortMode.None))
            {
                if (props.ContainsKey(body) || body.gameObject.layer != propLayer
                    || body.GetComponentInParent<BodyPart>() != null || body.GetComponentInParent<Weapon>() != null)
                {
                    continue;
                }

                props[body] = props.Count + 1;
                var collider = body.GetComponentInChildren<Collider>();
                Vector3 size = collider != null ? collider.bounds.size : Vector3.zero;
                Event((string)null, "prop", $"{body.name.Replace("(Clone)", "")} #{props[body]} appeared at ({body.position.x:0.00}, {body.position.y:0.00}, {body.position.z:0.00}), "
                                            + $"{body.mass:0} kg, size {size.x:0.00}×{size.y:0.00}×{size.z:0.00} m");
            }
        }

        private void SampleProps()
        {
            foreach (var pair in props)
            {
                var body = pair.Key;
                if (body == null)
                {
                    continue;
                }

                world.Begin();
                world.Add("t", Time.time, 4);
                world.Add("prop", pair.Value);
                world.Add("name", Sanitize(body.name.Replace("(Clone)", "")));
                Vector3 p = body.position;
                Quaternion q = body.rotation;
                world.Add("x", p.x, 3);
                world.Add("y", p.y, 3);
                world.Add("z", p.z, 3);
                world.Add("qx", q.x, 3);
                world.Add("qy", q.y, 3);
                world.Add("qz", q.z, 3);
                world.Add("qw", q.w, 3);
                world.Add("speed", body.linearVelocity.magnitude, 2);
                world.End();
            }
        }

        private void Flush()
        {
            try
            {
                events.Flush();
                world.Flush();
                foreach (var track in tracks)
                {
                    track.Flush();
                }
            }
            catch (Exception)
            {
                // Best effort; the next flush or the stop will try again.
            }
        }

        // ---- Events ----------------------------------------------------------------------------

        private void OnTrace(UnitController unit, string category, string text)
        {
            UnitTrack track = null;
            if (unit != null)
            {
                byUnit.TryGetValue(unit, out track);
            }

            track?.OnTrace(category, text);
            if (track != null)
            {
                Event(track, category, text);
            }
            else
            {
                Event(unit != null ? unit.name : null, category, text);
            }
        }

        private void OnHit(HitEvent hit)
        {
            foreach (var track in tracks)
            {
                track.OnHitLanded(hit);
            }

            Event(hit.attacker != null && byUnit.TryGetValue(hit.attacker, out var track2) ? track2 : null, "HIT",
                $"→ {LabelOf(hit.victim)} {hit.part}: {hit.speed:0.0} m/s, {hit.damage:0.0} damage, -{hit.stabilityLoss:0.0} stability, "
                + $"intent {hit.intent:0.00}, impulse {hit.impulse:0} N·s{(hit.friendly ? " (ally)" : "")}");
        }

        private void OnConsole(string condition, string stackTrace, LogType type)
        {
            if (inLogCallback || condition.StartsWith("[Playtest]"))
            {
                return;
            }

            inLogCallback = true;
            try
            {
                string text = condition.Replace('\n', ' ');
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    string where = "";
                    if (!string.IsNullOrEmpty(stackTrace))
                    {
                        int end = stackTrace.IndexOf('\n');
                        where = " @ " + (end > 0 ? stackTrace.Substring(0, end) : stackTrace).Trim();
                    }

                    consoleProblems.Add($"{Time.time:0.00} s {type}: {text}");
                    Event((string)null, "console", $"{type.ToString().ToUpperInvariant()}: {text}{where}");
                }
                else
                {
                    if (type == LogType.Warning)
                    {
                        consoleProblems.Add($"{Time.time:0.00} s warning: {text}");
                    }

                    Event((string)null, "console", $"{(type == LogType.Warning ? "warning: " : "")}{text}");
                }
            }
            finally
            {
                inLogCallback = false;
            }
        }

        internal void Event(UnitTrack track, string category, string text) => Event(track != null ? track.label : null, category, text);

        /// <summary>Writes one line to events.log.</summary>
        public void Event(string who, string category, string text)
        {
            if (events == null)
            {
                return;
            }

            line.Clear();
            line.Append(Time.time.ToString("0.000", CultureInfo.InvariantCulture).PadLeft(9));
            line.Append(" | ");
            line.Append(step.ToString(CultureInfo.InvariantCulture).PadLeft(6));
            line.Append(" | ");
            line.Append((who ?? "-").PadRight(14));
            line.Append(" | ");
            line.Append((category ?? "").PadRight(8));
            line.Append(" | ");
            line.Append((text ?? "").Replace('\n', ' '));
            events.WriteLine(line.ToString());
            eventCount++;
        }

        // ---- Config ----------------------------------------------------------------------------

        /// <summary>Saves a tuning asset's values the first time it is seen.</summary>
        public void NoteConfig(ScriptableObject asset)
        {
            if (!Recording || asset == null || configs.ContainsKey(asset))
            {
                return;
            }

            string json = JsonUtility.ToJson(asset, true);
            configs[asset] = json;
            try
            {
                File.WriteAllText(Path.Combine(sessionPath, "config", Sanitize(asset.name) + ".json"), json);
            }
            catch (Exception)
            {
                // Not essential.
            }
        }

        /// <summary>Tuning assets edited during Play: saved again, with the changed lines in the log.</summary>
        private void CheckConfigChanges()
        {
            foreach (var pair in configs)
            {
                if (pair.Key == null)
                {
                    continue;
                }

                string now = JsonUtility.ToJson(pair.Key, true);
                if (now == pair.Value)
                {
                    continue;
                }

                var before = new HashSet<string>(pair.Value.Split('\n'));
                var changed = new List<string>();
                foreach (var row in now.Split('\n'))
                {
                    if (!before.Contains(row))
                    {
                        changed.Add(row.Trim().TrimEnd(','));
                    }
                }

                Event((string)null, "config", $"{pair.Key.name} was changed during play; now: {string.Join("; ", changed)}");
                try
                {
                    File.WriteAllText(Path.Combine(sessionPath, "config", Sanitize(pair.Key.name) + ".end.json"), now);
                }
                catch (Exception)
                {
                    // Not essential.
                }
            }
        }

        // ---- Files -----------------------------------------------------------------------------

        private void WriteMeta(bool ended)
        {
            var json = new StringBuilder();
            json.Append("{\n");
            json.Append($"  \"format\": 1,\n  \"session\": \"{SessionName}\",\n  \"unity\": \"{Escape(Application.unityVersion)}\",\n");
            json.Append($"  \"fixedDeltaTime\": {Num(Time.fixedDeltaTime)},\n  \"startTime\": {Num(startTime)},\n");
            json.Append($"  \"endTime\": {(ended ? Num(Time.time) : "null")},\n  \"physicsSteps\": {step},\n");
            json.Append("  \"parts\": [");
            for (int i = 0; i < PartInfo.Count; i++)
            {
                var id = PartInfo.All[i];
                json.Append(i > 0 ? ", " : "").Append($"{{\"name\": \"{id}\", \"parent\": \"{(PartInfo.HasParent(id) ? PartInfo.Parent(id).ToString() : "")}\", \"bit\": {(int)id}}}");
            }

            json.Append("],\n  \"units\": [\n");
            var all = new List<UnitTrack>(closedTracks);
            all.AddRange(tracks);
            all.Sort((a, b) => a.id.CompareTo(b.id));
            for (int i = 0; i < all.Count; i++)
            {
                var t = all[i];
                var unit = t.unit;
                json.Append($"    {{\"id\": {t.id}, \"label\": \"{Escape(t.label)}\", \"stem\": \"{t.stem}\", ");
                json.Append($"\"frames\": \"units/{t.stem}.frames.csv.gz\", \"pose\": \"units/{t.stem}.pose.csv.gz\", ");
                json.Append($"\"spawnTime\": {Num(t.spawnTime)}, \"endTime\": {(t.endTime >= 0f ? Num(t.endTime) : "null")}");
                if (unit != null && unit.IsInitialized)
                {
                    json.Append($", \"team\": {unit.Team}, \"mass\": {Num(unit.Rig.TotalMass())}, \"scale\": {Num(unit.Scale)}, ");
                    json.Append($"\"restPelvisHeight\": {Num(unit.RestPelvisHeight)}, \"weapon\": {(unit.Weapon != null ? "true" : "false")}, ");
                    json.Append($"\"profile\": \"{Escape(unit.Profile != null ? unit.Profile.name : "")}\"");
                }

                json.Append(i < all.Count - 1 ? "},\n" : "}\n");
            }

            json.Append("  ],\n  \"marks\": [");
            for (int i = 0; i < marks.Count; i++)
            {
                var parts = marks[i].Split('|');
                json.Append(i > 0 ? ", " : "").Append($"{{\"n\": {parts[0]}, \"time\": {parts[1]}, \"note\": \"{Escape(parts[2])}\"}}");
            }

            json.Append("]\n}\n");
            try
            {
                File.WriteAllText(Path.Combine(sessionPath, "meta.json"), json.ToString());
            }
            catch (Exception)
            {
                // Rewritten at the end.
            }
        }

        private void WriteSummary(bool ended)
        {
            var md = new StringBuilder();
            md.AppendLine($"# Playtest {SessionName}");
            md.AppendLine();
            md.AppendLine($"Unity {Application.unityVersion}, scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}, "
                          + $"physics {1f / Time.fixedDeltaTime:0} Hz, solver {Physics.defaultSolverIterations}/{Physics.defaultSolverVelocityIterations}.");
            md.AppendLine(ended
                ? $"Recorded {Time.time - startTime:0.0} s of play ({step} physics steps), {eventCount} events, {marks.Count} marks."
                : "Recording… (this file is completed when Play stops).");
            md.AppendLine();
            md.AppendLine("Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), "
                          + "`units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.");
            md.AppendLine("Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/" + SessionName + "`.");

            if (ended)
            {
                var all = new List<UnitTrack>(closedTracks);
                all.AddRange(tracks);
                all.Sort((a, b) => a.id.CompareTo(b.id));
                foreach (var track in all)
                {
                    AppendUnitSummary(md, track);
                }

                if (marks.Count > 0)
                {
                    md.AppendLine("## Marks");
                    md.AppendLine();
                    foreach (var mark in marks)
                    {
                        var parts = mark.Split('|');
                        md.AppendLine($"- mark {parts[0]} at {parts[1]} s {parts[2]}");
                    }

                    md.AppendLine();
                }

                if (consoleProblems.Count > 0)
                {
                    md.AppendLine("## Console warnings and errors");
                    md.AppendLine();
                    for (int i = 0; i < consoleProblems.Count && i < 30; i++)
                    {
                        md.AppendLine("- " + consoleProblems[i]);
                    }

                    if (consoleProblems.Count > 30)
                    {
                        md.AppendLine($"- … and {consoleProblems.Count - 30} more (see events.log)");
                    }

                    md.AppendLine();
                }
            }

            try
            {
                File.WriteAllText(Path.Combine(sessionPath, "session.md"), md.ToString());
            }
            catch (Exception)
            {
                // Rewritten at the end.
            }
        }

        private static void AppendUnitSummary(StringBuilder md, UnitTrack track)
        {
            md.AppendLine();
            md.AppendLine($"## {track.label}");
            md.AppendLine();
            md.AppendLine($"Present {track.spawnTime:0.0}–{(track.endTime >= 0f ? track.endTime : Time.time):0.0} s. Time in states: {track.StateTimes()}.");
            md.AppendLine($"Hits taken {track.hitsTaken} ({track.damageTaken:0} damage). Catch steps {track.catchSteps}, settle steps {track.settleSteps}.");
            md.AppendLine();

            if (track.attacks.Count > 0)
            {
                md.AppendLine("| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |");
                md.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
                foreach (var a in track.attacks)
                {
                    md.AppendLine($"| {a.number} | {a.start:0.0} | {a.victim}, {a.planDistance:0.00} m | {a.furthestPhase} | {a.maxTipSpeed:0.0} m/s | "
                                  + $"{a.maxStrikeLag:0}° | {(a.minClubToTarget < float.MaxValue ? $"{a.minClubToTarget:0.00} m ({a.phaseAtClosest})" : "-")} | {a.Outcome} |");
                }

                md.AppendLine();
            }

            if (track.falls.Count > 0)
            {
                md.AppendLine("| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |");
                md.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
                foreach (var f in track.falls)
                {
                    md.AppendLine($"| {f.number} | {f.start:0.0} | {f.reason} | {(f.landedAt >= 0f ? $"{f.firstGroundParts} after {f.landedAt - f.start:0.0} s" : "-")} | "
                                  + $"{(f.lying.Length > 0 ? f.lying : "-")} | {(f.getUpStart >= 0f ? $"{f.getUpMaxPelvis:0.00} / {(f.getUpEndPelvis >= 0f ? f.getUpEndPelvis.ToString("0.00") : "-")} m" : "never started")} | "
                                  + $"{f.result ?? "-"} |");
                }

                md.AppendLine();
            }

            md.AppendLine("Joint tracking error, physical vs ghost (mean / max degrees):");
            md.AppendLine();
            var header = new StringBuilder("| Part |");
            var rule = new StringBuilder("| --- |");
            var used = new List<int>();
            for (int c = 0; c < UnitTrack.Contexts; c++)
            {
                if (track.ContextSeconds(c) >= 0.2f)
                {
                    used.Add(c);
                    header.Append($" {UnitTrack.ContextNames[c]} ({track.ContextSeconds(c):0} s) |");
                    rule.Append(" --- |");
                }
            }

            md.AppendLine(header.ToString());
            md.AppendLine(rule.ToString());
            foreach (var id in PartInfo.All)
            {
                var row = new StringBuilder($"| {id} |");
                foreach (int c in used)
                {
                    track.Tracking(c, id, out float mean, out float max);
                    row.Append($" {mean:0} / {max:0} |");
                }

                md.AppendLine(row.ToString());
            }

            md.AppendLine();
        }

        // ---- Helpers ---------------------------------------------------------------------------

        private static string Num(float value) => value.ToString("0.#####", CultureInfo.InvariantCulture);

        private static string Escape(string text) => (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string Sanitize(string text)
        {
            var clean = new StringBuilder();
            foreach (char c in text)
            {
                clean.Append(char.IsLetterOrDigit(c) ? c : '_');
            }

            return clean.ToString().Trim('_').Replace("__", "_");
        }

        private static string Heading(Vector3 forward)
        {
            float yaw = Mathf.Repeat(Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg, 360f);
            return $"{yaw:0}°";
        }
    }
}
