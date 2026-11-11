using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Motion;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Records a playtest so it can be read afterwards (by Claude, through Tools/Playtest/analyze.py,
    /// or by hand). Every Play session writes a folder under PlaytestLogs/ in the project:
    ///
    ///   session.md          what was recorded, the settings, and the summary per unit
    ///   events.log          one line per thing that happened, with the reasons the code gave
    ///   meta.json           machine-readable index: units, files, parts, marks
    ///   config/*.json       the unit profiles and combat styles as they were (and if changed in play)
    ///   units/NN_Name.frames.csv.gz   one row per physics step: stance, balance, authority, pose errors, the blow under way
    ///   units/NN_Name.pose.csv.gz     every part's physical position/rotation and its animated position
    ///   world.csv.gz        props (crates, barrels) positions
    ///   frames/NNNNNN.jpg   small square pictures of the game view, 8 per second (frames/frames.csv says when)
    ///
    /// Samples are taken at the start of each physics step, before the units run: the body as PhysX
    /// left it, against the pose it was following. Nothing here changes how the game plays.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class PlaytestRecorder : MonoBehaviour
    {
        [Tooltip("Start recording when Play starts.")]
        public bool recordOnPlay = true;

        [Tooltip("Write the full pose every N physics steps (1 = every step, 60 per second).")]
        [Range(1, 6)] public int poseEvery = 1;

        [Header("Pictures of the game view")]
        [Tooltip("Save small pictures of what the main camera sees while recording.")]
        public bool captureFrames = true;
        [Tooltip("Picture size in pixels (square).")]
        public int frameSize = 448;

        [Tooltip("The pictures move in to frame the fighters still standing (the game camera's side and angle, as close as fits them). Off: the game view as it is.")]
        public bool framesFollowFight = true;
        [Tooltip("Pictures per second of game time.")]
        public float framesPerSecond = 8f;
        [Range(10, 100)] public int frameQuality = 70;
        [Tooltip("Stop taking pictures after this many (about 15 KB each).")]
        public int maxFrames = 2400;

        private FrameCapture frames;

        public static PlaytestRecorder Active { get; private set; }

        private static float lastSeenTime;

        /// <summary>
        /// Time.time, except when Play is stopping: Unity has already reset Time.time to 0 by the time
        /// OnDisable runs, so the last time seen is used instead.
        /// </summary>
        public static float Now => Mathf.Max(Time.time, lastSeenTime);

        private CultureInfo previousCulture;

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
        private ShieldContacts shieldContacts = new ShieldContacts();
        private readonly List<ShieldContacts.Contact> shieldEnded = new List<ShieldContacts.Contact>();

        private StreamWriter events;
        private PerfTrack perf;

        /// <summary>Milestone 11: the defensive posture's gate numbers (session.md's Defence section).</summary>
        private DefenceStats defence = new DefenceStats();
        private readonly List<UnitController> sampledUnits = new List<UnitController>();
        private Action<string> onHitch;
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

        /// <summary>The living units' bodies: their middle (hip height) and how far out from it they reach.</summary>
        private (Vector3 centre, float radius)? FightFocus()
        {
            Vector3 lo = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), hi = -lo;
            int n = 0;
            foreach (var unit in UnitController.All)
            {
                if (unit == null || !unit.IsAlive || !unit.IsInitialized)
                {
                    continue;
                }

                Vector3 p = unit.Root.position;
                float size = unit.Body != null ? unit.Body.SizeFactor : 1f;
                lo = Vector3.Min(lo, p - new Vector3(0.6f, 0f, 0.6f) * size);
                hi = Vector3.Max(hi, p + new Vector3(0.6f, 1.9f, 0.6f) * size);
                n++;
            }

            if (n == 0)
            {
                return null;
            }

            Vector3 centre = 0.5f * (lo + hi);
            return (centre, 0.5f * Vector3.Distance(lo, hi));
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

            // Numbers in the log always use a decimal point, whatever the machine's region settings.
            previousCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            lastSeenTime = Time.time;
            try
            {
                startedAt = DateTime.Now;
                SessionName = startedAt.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
                sessionPath = Path.Combine(LogRoot, SessionName);
                Directory.CreateDirectory(Path.Combine(sessionPath, "units"));
                Directory.CreateDirectory(Path.Combine(sessionPath, "config"));
                events = new StreamWriter(Path.Combine(sessionPath, "events.log"), false, new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Playtest] Could not start recording in {sessionPath}: {exception.Message}");
                events = null;
                return;
            }

            startTime = Now;
            step = 0;
            eventCount = 0;
            unitCount = 0;
            tracks.Clear();
            shieldContacts = new ShieldContacts();
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
            UnitController.Initialized += OnUnitInitialized;
            UnitController.MoveStarted += OnMoveStarted;
            UnitController.MoveContact += OnMoveContact;
            UnitController.MoveEnded += OnMoveEnded;
            UnitController.Hit += OnHit;
            UnitController.Died += OnDied;
            UnitController.Severed += OnSevered;
            UnitController.ComboChanged += OnCombo;
            UnitController.BalanceChanged += OnBalance;
            UnitController.PostureStarted += OnPosture;
            UnitController.PostureLedIn += OnPostureLedIn;
            UnitController.Defending += OnDefending;
            UnitController.Noticed += OnNoticed;
            UnitController.Forgot += OnForgot;
            UnitController.TempoChanged += OnTempo;
            UnitController.GuardBroken += OnGuardBroken;
            UnitController.ClipStrikeStarted += OnClipStrike;
            UnitController.ClipComboTurnedOn += OnClipComboTurned;
            defence = new DefenceStats();
            Application.logMessageReceived += OnConsole;

            foreach (var unit in FindObjectsByType<UnitController>())
            {
                if (unit.IsInitialized)
                {
                    Follow(unit);
                }
            }

            if (captureFrames)
            {
                frames = GetComponent<FrameCapture>();
                if (frames == null)
                {
                    frames = gameObject.AddComponent<FrameCapture>();
                }

                frames.CurrentStep = () => step;
                frames.Focus = framesFollowFight ? FightFocus : null;
                frames.Begin(sessionPath, startTime, frameSize, framesPerSecond, frameQuality, maxFrames);
                if (frames.Capturing)
                {
                    Event((string)null, "session", $"saving {frames.Size}x{frames.Size} pictures of the game view, {frames.Rate:0.#} per second, to frames/");
                }
            }

            // Performance analytics (perf.csv.gz, perf_counters.txt; hitches in this log as "perf").
            try
            {
                perf = new PerfTrack(sessionPath);
                Event((string)null, "session", "performance: a row a frame in perf.csv.gz; Unity's counters recorded: "
                                               + (perf.Counters.Length > 0 ? perf.Counters : "none") + $"; frames over {PerfTrack.HitchMs:0} ms noted as \"perf\""
                                               + $"; fixed step {Time.fixedDeltaTime * 1000f:0.0} ms, at most {Time.maximumDeltaTime * 1000f:0} ms of physics a frame, vSync {QualitySettings.vSyncCount}, target fps {Application.targetFrameRate}");
            }
            catch (Exception exception)
            {
                Event((string)null, "session", "performance analytics could not start: " + exception.Message);
                perf = null;
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
            UnitController.Initialized -= OnUnitInitialized;
            UnitController.MoveStarted -= OnMoveStarted;
            UnitController.MoveContact -= OnMoveContact;
            UnitController.MoveEnded -= OnMoveEnded;
            UnitController.Hit -= OnHit;
            UnitController.Died -= OnDied;
            UnitController.Severed -= OnSevered;
            UnitController.ComboChanged -= OnCombo;
            UnitController.BalanceChanged -= OnBalance;
            UnitController.PostureStarted -= OnPosture;
            UnitController.PostureLedIn -= OnPostureLedIn;
            UnitController.Defending -= OnDefending;
            UnitController.Noticed -= OnNoticed;
            UnitController.Forgot -= OnForgot;
            UnitController.TempoChanged -= OnTempo;
            UnitController.GuardBroken -= OnGuardBroken;
            UnitController.ClipStrikeStarted -= OnClipStrike;
            UnitController.ClipComboTurnedOn -= OnClipComboTurned;
            Application.logMessageReceived -= OnConsole;

            // (Whatever goes wrong summing the session up, its files are closed: one left open stays locked until the editor
            // quits.)
            try
            {
                foreach (var track in tracks.ToArray())
                {
                    try
                    {
                        CloseTrack(track, "recording stopped");
                    }
                    catch (Exception exception)
                    {
                        Debug.LogWarning($"[Playtest] Closing a unit's track: {exception}");
                    }
                }

                shieldEnded.Clear();
                shieldContacts.CloseAll(Time.fixedDeltaTime, shieldEnded);
                LogShieldContacts();
                CheckConfigChanges();
                if (frames != null && (frames.Capturing || frames.Saved > 0))
                {
                    Event((string)null, "session", $"{frames.Saved} pictures saved" + (frames.Failed > 0 ? $", {frames.Failed} failed" : ""));
                    frames.End();
                }

                Event((string)null, "session", $"recording stopped after {Now - startTime:0.0} s ({step} physics steps)");
                WriteMeta(true);
                WriteSummary(true);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Playtest] Summing the session up: {exception}");
            }
            finally
            {
                perf?.Dispose();
                perf = null;
            }

            try
            {
                events.Dispose();
            }
            catch (Exception)
            {
                // The editor is shutting down; the file is as complete as it will get.
            }

            world?.Dispose();

            events = null;
            world = null;
            if (previousCulture != null)
            {
                CultureInfo.CurrentCulture = previousCulture;
            }

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
            marks.Add($"{number}|{Now:0.000}|{note}");
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

        private UnitTrack TrackOf(UnitController unit)
        {
            foreach (var track in tracks)
            {
                if (track.unit == unit)
                {
                    return track;
                }
            }

            return null;
        }

        private static string NameOf(object unit) => unit is UnitController u && u != null ? u.name : "a point";

        private static string Cm(float metres) => (metres * 100f).ToString("0.0", CultureInfo.InvariantCulture);

        private void OnMoveStarted(UnitController unit, MoveState move)
        {
            NoteConfig(move.Move);
            NoteConfig(move.Weapon);
            Event(TrackOf(unit), "attack", $"{move.Move.name.Replace("MD_", "")} at {NameOf(move.Target)}'s {move.Part}: aim locked at "
                                           + $"({move.Aim.x:0.00}, {move.Aim.y:0.00}, {move.Aim.z:0.00}), lunge {move.Lunge.magnitude:0.00} m, "
                                           + $"{move.Duration:0.00} s, contact at {move.ContactTime * move.Duration:0.00} s");
        }

        private void OnMoveContact(UnitController unit, MoveState move, ContactReport report)
        {
            string struck = report.StruckUnit != null
                ? $"nearest part {report.StruckUnit.name}'s {report.StruckPart} ({Cm(report.StruckDistance)} cm)"
                : "nothing near";
            string line = $"{move.Move.name.Replace("MD_", "")} at {NameOf(move.Target)}'s {move.Part}: passed {Cm(report.AnimError)} cm from the aim point; "
                          + $"{struck}; the {move.Part} had moved {Cm(report.TargetMoved)} cm since the aim was locked";
            var track = TrackOf(unit);
            Event(track, "contact", line);
            track?.NoteContact(move, report, $"{Now:0.00} s: {line}");
        }

        private void OnMoveEnded(UnitController unit, MoveState move)
        {
            var track = TrackOf(unit);
            if (move.Cancelled != null)
            {
                Event(track, "attack", $"{move.Move.name.Replace("MD_", "")} cut short: {move.Cancelled}");
                return;
            }

            string physical = move.PhysicalClosest >= 0f ? $"the physical weapon passed {Cm(move.PhysicalClosest)} cm from the aim point at its closest" : "no physical weapon";
            string swing = move.Generated ? $"; swing: {move.ContactSpeed:0.0} m/s at contact, {move.PeakSpeed:0.0} at its fastest, {Cm(move.Depth)} cm past the aim point, a {move.Arc:0}° arc" : "";
            Event(track, "attack", $"{move.Move.name.Replace("MD_", "")} done: {physical}{swing}");
            track?.NotePhysical(move.PhysicalClosest);
        }

        private void OnHit(HitReport hit)
        {
            defence.OnHit(hit);
            var r = hit.Result;
            string what = hit.Shield
                ? $"blocked by {hit.Struck.name}'s shield ({r.PoiseDamage:0} poise)"
                : hit.Parried
                ? $"parried by {hit.Struck.name}'s {hit.Kind.Replace("parried by the ", "")} ({r.PoiseDamage:0} poise)"
                : $"{r.Damage:0.0} damage, {r.PoiseDamage:0} poise, {r.Reaction.ToString().ToLowerInvariant()}";
            string swing = r.StopsSwing ? "the swing stopped" : $"cleaved through ({r.MomentumLeft * 100f:0}% of the swing left)";
            if (hit.Impact)
            {
                string impact = $"thrown into {hit.Struck.name}: its {hit.ImpactPart} hit the {hit.Part} at {hit.Speed:0.0} m/s, "
                                + $"{r.Damage:0.0} damage, {r.PoiseDamage:0} poise, {r.Reaction.ToString().ToLowerInvariant()}; impulse {r.Impulse.magnitude:0.0} N·s ({hit.Kind})";
                var thrower = TrackOf(hit.Attacker);
                if (r.Impulse.sqrMagnitude > 0f)
                {
                    Event(thrower, "impact", impact);
                    thrower?.NoteHitDealt(hit);
                }

                var hurt = TrackOf(hit.Struck);
                string by = r.Impulse.sqrMagnitude > 0f ? $"by {hit.Attacker.name}'s body" : $"thrown into {hit.Attacker.name}";
                Event(hurt, "struck", $"{by} on the {hit.Part}: health {hit.Struck.Health:0}/{hit.Struck.Profile.maxHealth:0}, "
                                      + $"poise {Mathf.Max(0f, hit.Struck.Poise):0}/{hit.Struck.Profile.maxPoise:0}, {StateOf(hit.Struck)}");
                hurt?.NoteHitTaken(hit);
                return;
            }

            string line = $"{hit.What} struck {hit.Struck.name}'s {hit.Part} at {hit.Speed:0.0} m/s: {what}; "
                          + $"impulse {r.Impulse.magnitude:0.0} N·s ({r.PartShare * 100f:0}% into the part); {swing}; {hit.Kind}"
                          + (hit.Severed.HasValue ? $"; cut through the {PartInfo.JointName(hit.Severed.Value)}" : hit.SeverMiss != null ? $"; no sever: {hit.SeverMiss}" : "");
            var track = TrackOf(hit.Attacker);
            Event(track, "hit", line);
            track?.NoteHitDealt(hit);
            var struck = TrackOf(hit.Struck);
            Event(struck, "struck", $"by {hit.Attacker.name} on the {hit.Part}: health {hit.Struck.Health:0}/{hit.Struck.Profile.maxHealth:0}, "
                                    + $"poise {Mathf.Max(0f, hit.Struck.Poise):0}/{hit.Struck.Profile.maxPoise:0}, {r.Reaction.ToString().ToLowerInvariant()}"
                                    + $"{(hit.Struck.Knockback.magnitude > 0.05f ? $", knocked back at {hit.Struck.Knockback.magnitude:0.00} m/s" : "")}"
                                    + $", {StateOf(hit.Struck)}"
                                    + (!string.IsNullOrEmpty(hit.Struck.Clip.LastReaction) ? $"; {hit.Struck.Clip.LastReaction}" : "")
                                    + (hit.Struck.Concentration > 0.01f ? $"; concentration {hit.Struck.Concentration:0.00}" : ""));
            struck?.NoteHitTaken(hit);
        }

        /// <summary>A clip fighter started a strike or a combo (the plan "Bringing the lab into the fight", step 5).</summary>
        private void OnClipStrike(UnitController unit, string what, UnitController target)
        {
            Event(TrackOf(unit), "attack", $"{what} at {LabelOf(target)}{(unit.Concentration > 0.01f ? $", concentration {unit.Concentration:0.00}" : "")}");
        }

        /// <summary>A combo's next blow turned on another enemy (the plan "Fighting off several attackers", step 8).</summary>
        private void OnClipComboTurned(UnitController unit, UnitController was, UnitController now)
        {
            Event(TrackOf(unit), "attack", $"its combo's next blow turned from {LabelOf(was)} to {LabelOf(now)}");
        }

        private static string StateOf(UnitController unit)
        {
            if (!unit.IsAlive)
            {
                return "dead";
            }

            switch (unit.State)
            {
                case UnitState.Stumbling: return $"stumbling at {unit.StumbleVelocity.magnitude:0.00} m/s";
                case UnitState.Down: return "down";
                case UnitState.GettingUp: return "getting up";
                default: return "standing";
            }
        }

        private void OnPosture(UnitController unit, PostureDefinition posture, bool leadIn)
        {
            var track = TrackOf(unit);
            float length = unit.Rhythm != null ? unit.Rhythm.Length(posture) : posture.duration;
            var open = new System.Text.StringBuilder();
            foreach (var w in posture.openings)
            {
                open.Append(open.Length > 0 ? "; " : "").Append($"{w.parts.ToString().Replace(", ", " ")} {w.from * length:0.00}–{w.to * length:0.00} s");
            }

            string mood = unit.Rhythm != null ? $"energy {unit.Rhythm.Energy:0.00}, aggression {unit.Rhythm.Aggression:0.00}" : "";
            Event(track, "rhythm", $"posture {posture.Label}{(leadIn ? " (lead-in to a blow)" : "")}, {length:0.00} s; open: {(open.Length > 0 ? open.ToString() : "nothing")}; {mood}");
            track?.NotePosture(posture, leadIn);
        }

        private void OnTempo(UnitController unit, Motion.TempoState from, Motion.TempoState to, string why)
        {
            var track = TrackOf(unit);
            float energy = unit.Rhythm != null ? unit.Rhythm.Energy : 0f, aggression = unit.Rhythm != null ? unit.Rhythm.Aggression : 0f;
            Event(track, "tempo", $"{from.ToString().ToLowerInvariant()} → {to.ToString().ToLowerInvariant()}: {why}; energy {energy:0.00}, aggression {aggression:0.00}");
            defence.OnTempo(unit, from, to, why);
        }

        /// <summary>Milestone 11: a guard broken under a barrage.</summary>
        private void OnGuardBroken(UnitController unit, HitReport hit)
        {
            Event(TrackOf(unit), "defence", $"guard broken by {(hit.Attacker != null ? hit.Attacker.name : "?")}'s {hit.What} (pressure {(unit.Tempo != null ? unit.Tempo.Pressure : 0f):0.00}, "
                                            + $"poise {unit.Poise:0}/{unit.Profile.maxPoise:0}): open for {UnitController.GuardOpenTime:0.0} s");
            defence.OnGuardBroken(unit, hit);
        }

        private void OnDefending(UnitController unit, UnitController attacker, MoveState move, string what)
        {
            var track = TrackOf(unit);
            float contact = move.ContactTime * move.Duration;
            Event(track, "defence", $"{what} against {attacker.name}'s {move.Move.name.Replace("MD_", "")} at its {move.Part} (contact in {contact:0.00} s)");
            track?.NoteDefence(what);
        }

        private void OnNoticed(UnitController unit, UnitController other, Combat.Sight sight)
        {
            string how = sight == Combat.Sight.Clear ? "in clear view" : sight == Combat.Sight.Edge ? "at the edge of its view" : "by touch";
            Event(TrackOf(unit), "sight", $"noticed {other.name} {how} ({Vector3.Distance(unit.Root.position, other.Root.position):0.0} m)");
        }

        private void OnForgot(UnitController unit, UnitController other)
        {
            Event(TrackOf(unit), "sight", $"lost track of {other.name}");
        }

        private void OnPostureLedIn(UnitController unit, PostureDefinition posture)
        {
            var track = TrackOf(unit);
            Event(track, "rhythm", $"lead-in: posture {posture.Label} under way leads into a blow");
            track?.NoteLedIn();
        }

        private void OnBalance(UnitController unit, string text)
        {
            var track = TrackOf(unit);
            Event(track, "balance", text);
            track?.NoteBalance(text);
        }

        private void OnDied(UnitController unit, HitReport hit)
        {
            string by = hit.Attacker != null ? hit.Attacker.name : "nobody";
            Event(TrackOf(unit), "death", hit.Kind == "bled out" || hit.Kind == "beheaded"
                ? $"{hit.Kind} (wounded by {by})"
                : $"killed by {by}'s {hit.What} to the {hit.Part}");
        }

        /// <summary>Milestone 10: a combo started, linked or ended.</summary>
        private void OnCombo(UnitController unit, ComboRun combo, string what)
        {
            defence.OnCombo(unit, combo, what);
            string head = what == "started"
                ? $"combo \"{combo.Definition.name}\" ({combo.Why}, {combo.Length} links) at {LabelOf(combo.Target)}"
                : $"combo \"{combo.Definition.name}\" {what}";
            if (what.StartsWith("ended"))
            {
                head += $" after {combo.Link + 1} of {combo.Length} links";
            }

            Event(LabelOf(unit), "combo", head);
        }

        private void OnSevered(UnitController unit, PartId at, HitReport hit)
        {
            var track = TrackOf(unit);
            string how = hit != null && hit.Attacker != null ? $" by {hit.Attacker.name}'s {hit.What} at {hit.Speed:0.0} m/s ({hit.Result.Damage:0.0} damage)" : "";
            Event(track, "sever", $"severed at the {PartInfo.JointName(at)}{how}; health {unit.Health:0}/{unit.Profile.maxHealth:0}, bleeding {unit.Bleeding:0.0}/s"
                                  + (unit.Loadout?.Main == null ? ", unarmed" : "") + (unit.LegLost ? ", can't stand" : ""));
            track?.NoteSevered();
        }

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
                                  + $"{(unit.Profile != null ? unit.Profile.body : "?")} body, mass {rig.TotalMass():0.0} kg, "
                                  + $"leg length {unit.Body.LegLength:0.00} m, rest pelvis height {unit.RestPelvisHeight:0.00} m, "
                                  + $"profile {(unit.Profile != null ? unit.Profile.name : "default")}, style {(unit.Style != null ? unit.Style.name : "default")}");
            NoteConfig(unit.Profile);
            NoteConfig(unit.Style);

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

        // ---- Sampling --------------------------------------------------------------------------

        private void FixedUpdate()
        {
            if (!Recording)
            {
                return;
            }

            step++;
            lastSeenTime = Time.time;
            perf?.NoteFixedStep();
            using (Perf.Begin(PerfSection.RecorderSample))
            {
                SampleStep();
            }
        }

        private void SampleStep()
        {
            if (pending.Count > 0)
            {
                foreach (var unit in pending)
                {
                    Follow(unit);
                }

                pending.Clear();
            }

            sampledUnits.Clear();
            foreach (var t in tracks)
            {
                if (t.Alive && t.unit != null)
                {
                    sampledUnits.Add(t.unit);
                }
            }

            foreach (var u in sampledUnits)
            {
                defence.Sample(u, sampledUnits, Time.fixedDeltaTime, Now);
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

            SampleShields();
            if (step % 30 == 1)
            {
                FindProps();
            }

            if (step % 2 == 0)
            {
                SampleProps();
            }

            // (The performance pass: one file a step, in turn, rather than every file at once every
            // half second: written all together they took 27 ms in one step, a stutter.)
            FlushOne();
        }

        private void LateUpdate()
        {
            if (Recording && perf != null)
            {
                onHitch ??= text => Event((string)null, "perf", text);
                perf.EndFrame(Now - startTime, step, tracks.Count, onHitch);
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

        /// <summary>Milestone 9c: each shield's overlaps with other bodies; each one in the log as it ends.</summary>
        private void SampleShields()
        {
            shieldContacts.BeginStep();
            foreach (var track in tracks)
            {
                if (track.unit != null)
                {
                    shieldContacts.Measure(track.unit, LabelOf, Now);
                }
            }

            shieldEnded.Clear();
            shieldContacts.EndStep(Time.fixedDeltaTime, shieldEnded);
            LogShieldContacts();
        }

        private void LogShieldContacts()
        {
            foreach (var c in shieldEnded)
            {
                Event((string)null, "shield", $"{c.shield} in {c.other} for {c.duration:0.000} s, {c.deepest * 100f:0.0} cm deep at most");
            }

            shieldEnded.Clear();
        }

        private void FindProps()
        {
            int propLayer = LayerMask.NameToLayer(Layers.Prop);
            foreach (var body in FindObjectsByType<Rigidbody>())
            {
                if (props.ContainsKey(body) || body.gameObject.layer != propLayer
                    || body.GetComponentInParent<BodyPart>() != null)
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

                if (world == null)
                {
                    world = new TableWriter(Path.Combine(sessionPath, "world.csv.gz"));   // only once there are props
                }

                world.Begin();
                world.Add("t", Now, 4);
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
            using (Perf.Begin(PerfSection.RecorderFlush))
            {
                FlushFiles();
            }
        }

        private int flushTurn;

        /// <summary>Writes out one of the recording's files (in turn: the events, the props, the performance table, then each unit's).</summary>
        private void FlushOne()
        {
            using (Perf.Begin(PerfSection.RecorderFlush))
            {
                int files = 3 + tracks.Count;
                int turn = flushTurn++ % Mathf.Max(1, files);
                try
                {
                    switch (turn)
                    {
                        case 0:
                            // (A unit described in meta.json only once it is set up.)
                            foreach (var track in tracks)
                            {
                                if (track.metaFields == null && track.unit != null && track.unit.IsInitialized)
                                {
                                    WriteMeta(false);
                                    break;
                                }
                            }

                            events.Flush();
                            break;
                        case 1:
                            world?.Flush();
                            break;
                        case 2:
                            perf?.Flush();
                            break;
                        default:
                            tracks[turn - 3].Flush();
                            break;
                    }
                }
                catch (Exception)
                {
                    // (Next time round.)
                }
            }
        }

        private void FlushFiles()
        {
            perf?.Flush();
            // A unit described in meta.json only once it is set up (it announces itself before).
            foreach (var track in tracks)
            {
                if (track.metaFields == null && track.unit != null && track.unit.IsInitialized)
                {
                    WriteMeta(false);
                    break;
                }
            }

            // Each file on its own: one failing must not stop the others being flushed.
            try { events.Flush(); } catch (Exception) { /* next time */ }
            try { world?.Flush(); } catch (Exception) { /* next time */ }
            foreach (var track in tracks)
            {
                try { track.Flush(); } catch (Exception) { /* next time */ }
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

                    consoleProblems.Add($"{Now:0.00} s {type}: {text}");
                    Event((string)null, "console", $"{type.ToString().ToUpperInvariant()}: {text}{where}");
                }
                else
                {
                    if (type == LogType.Warning)
                    {
                        consoleProblems.Add($"{Now:0.00} s warning: {text}");
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
            line.Append(Now.ToString("0.000", CultureInfo.InvariantCulture).PadLeft(9));
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
            json.Append($"  \"endTime\": {(ended ? Num(Now) : "null")},\n  \"physicsSteps\": {step},\n");
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
                    // Kept: when Play stops the units are gone before the last write.
                    t.metaFields = $", \"team\": {unit.Team}, \"mass\": {Num(unit.Rig.TotalMass())}, \"scale\": {Num(unit.Scale)}, "
                                   + $"\"restPelvisHeight\": {Num(unit.RestPelvisHeight)}, \"legLength\": {Num(unit.Body.LegLength)}, "
                                   + $"\"profile\": \"{Escape(unit.Profile != null ? unit.Profile.name : "")}\"";
                }

                json.Append(t.metaFields ?? "");

                json.Append(i < all.Count - 1 ? "},\n" : "}\n");
            }

            json.Append("  ],\n");
            if (frames != null)
            {
                json.Append($"  \"frames\": {{\"index\": \"frames/frames.csv\", \"size\": {frames.Size}, \"perSecond\": {Num(frames.Rate)}, \"saved\": {frames.Saved}}},\n");
            }

            json.Append("  \"marks\": [");
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
                ? $"Recorded {Now - startTime:0.0} s of play ({step} physics steps), {eventCount} events, {marks.Count} marks."
                : "Recording… (this file is completed when Play stops).");
            md.AppendLine();
            md.AppendLine("Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), "
                          + "`units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, "
                          + "`perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.");
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

                if (defence.Sides.Count > 0)
                {
                    md.AppendLine("## Defence (milestone 11)");
                    md.AppendLine();
                    defence.Write(md);
                    md.AppendLine();
                }

                var (count, p95, deepest, longest, under) = ShieldContacts.Summary(shieldContacts.Done);
                if (count > 0)
                {
                    md.AppendLine("## Shield contacts");
                    md.AppendLine();
                    md.AppendLine($"{count} times a shield overlapped another unit's body: 95% under {p95 * 100f:0.0} cm deep ({under * 100f:0}% under "
                                  + $"{ShieldContacts.DepthTarget * 100f:0} cm; target 95%), the deepest {deepest * 100f:0.0} cm, the longest {longest:0.00} s "
                                  + $"(target under {ShieldContacts.LongestTarget:0.0} s). Each one is in events.log (\"shield\").");
                    md.AppendLine();
                }

                perf?.AppendSummary(md);

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

        private static void AppendUnitSummary(StringBuilder md, UnitTrack track) => track.AppendSummary(md);

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
