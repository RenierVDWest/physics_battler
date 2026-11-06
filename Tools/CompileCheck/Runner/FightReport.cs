// `fight [dir] [seed]`: a whole 3 v 3 fight on the board with no help (milestone 6's gate), stepped
// as the smoke run is (the stand-in integrator for PhysX), teams starting at opposite ends. Records
// a playtest session in <dir> (analyze it with Tools/Playtest/analyze.py) and prints how it went.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhysicsBattler.Battle;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class FightReport
{
    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>Called after every step of a fight (the `arms` runner measures the poses here).</summary>
    public static Action<List<UnitController>, float> OnStep;

    public static int Run(string output, int seed)
    {
        Directory.CreateDirectory(output);
        var rigs = new Dictionary<string, RigFile>();
        foreach (var body in new[] { "Small", "Medium", "Large" })
        {
            rigs[body] = RigFile.Parse(File.ReadAllText(Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json")));
        }

        string assets = Application.dataPath;
        Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");
        var recorder = new GameObject("Recorder").AddComponent<PlaytestRecorder>();
        Call(recorder, "OnEnable");
        if (UnitController.ClipMotion)
        {
            // (No physics headless: the clip fighters' bodies are the lab runners', posed only.)
            UnitController.ClipPhysical = false;
        }

        float seconds = float.TryParse(Environment.GetEnvironmentVariable("FIGHT_SECONDS"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fs) ? fs : 240f;
        var result = Play(rigs, seed, seconds, recorder, out var stats);
        Call(recorder, "OnDisable");
        Application.dataPath = assets;
        Console.WriteLine(result);
        Console.WriteLine(stats);
        Console.WriteLine("fight session: " + recorder.SessionPath);
        return 0;
    }

    /// <summary>Plays a fight to its end (or the time limit); the result line, and per-unit figures.</summary>
    private static readonly bool Smalls = Environment.GetEnvironmentVariable("FIGHT_SMALLS") != null;

    public static string Play(Dictionary<string, RigFile> rigs, int seed, float limit, PlaytestRecorder recorder, out string stats)
    {
        var grid = new BoardGrid();
        var lineup = new[]
        {
            // (The small units left the battle on 6 November: a medium with a sword only in their place, "Swordsman";
            // FIGHT_SMALLS=1 has them back.)
            new[] { (Smalls ? "Small" : "Swordsman", new Tile(2, 1)), ("Medium", new Tile(3, 1)), ("Large", new Tile(4, 1)) },
            new[] { ("Medium", new Tile(4, 6)), ("Large", new Tile(2, 6)), (Smalls ? "Small" : "Swordsman", new Tile(3, 6)) },
        };
        // (The plan "Fighting off several attackers": FIGHT_LINEUP=gang2 or gang3 sets one medium with sword and shield
        // in the middle on 2 or 3 swordsmen coming from in front, the side and behind.)
        string gang = Environment.GetEnvironmentVariable("FIGHT_LINEUP");
        if (gang == "gang2" || gang == "gang3")
        {
            lineup = new[]
            {
                new[] { ("Medium", new Tile(3, 3)) },
                gang == "gang2"
                    ? new[] { ("Swordsman", new Tile(3, 6)), ("Swordsman", new Tile(6, 3)) }
                    : new[] { ("Swordsman", new Tile(3, 6)), ("Swordsman", new Tile(6, 3)), ("Swordsman", new Tile(3, 0)) },
            };
        }

        if (Environment.GetEnvironmentVariable("FIGHT_MIRROR") != null)
        {
            // (Each team in the other's places, to see whether a side or a slot wins, not the units.)
            var swap = new[] { lineup[1], lineup[0] };
            lineup = new[]
            {
                Array.ConvertAll(swap[0], e => (e.Item1, new Tile(e.Item2.column, 7 - e.Item2.row))),
                Array.ConvertAll(swap[1], e => (e.Item1, new Tile(e.Item2.column, 7 - e.Item2.row))),
            };
        }

        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising), MoveDefinition.Create(MoveStyle.ClubBackhand), MoveDefinition.Create(MoveStyle.ClubFan), MoveDefinition.Create(MoveStyle.ClubFanBackhand) };
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        var units = new List<UnitController>();
        for (int team = 0; team < 2; team++)
        {
            foreach (var (named, tile) in lineup[team])
            {
                string body = named == "Swordsman" ? "Medium" : named;
                var profile = UnitProfile.CreateDefault();
                profile.name = "UP_" + body;
                profile.body = body;
                profile.style = CombatStyle.CreateFor(body);
                if (body == "Small") { profile.maxHealth = 60f; profile.maxPoise = 25f; }
                if (body == "Large") { profile.maxHealth = 220f; profile.maxPoise = 120f; }
                var root = UnitBuilder.Build(rigs[body], profile, $"{(team == 0 ? "A" : "B")} {named}", _ => null, 8);
                root.transform.position = grid.Center(tile);
                root.transform.rotation = Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f);
                var unit = root.GetComponent<UnitController>();
                unit.Team = team;
                unit.Profile = profile;
                Call(unit, "Awake");
                if (named == "Swordsman") unit.Equip(sword, null); else if (body == "Medium") unit.Equip(sword, shield); else unit.Equip(club, null);
                if (Environment.GetEnvironmentVariable("FIGHT_NO_COLLIDERS") == null && !unit.UsesClips) Smoke.GiveColliders(unit);   // (milestone 11: blocks and parries headless)
                units.Add(unit);
            }
        }

        var loop = new FightLoop(seed) { Grid = grid };
        // (The plan "Bringing the lab into the fight", step 4: CLIP_FIGHT=1 moves the units as clip fighters, their
        // walk measured as the lab's is.)
        bool clips = UnitController.ClipMotion;
        var feet = new Dictionary<UnitController, PhysicsBattler.Locomotion.LocoMeasures>();
        var lastRoot = new Dictionary<UnitController, Vector3>();
        var rootJump = new Dictionary<UnitController, (float most, float at)>();
        var reachedAt = new Dictionary<UnitController, float>();
        var overlapPairs = new Dictionary<string, (int steps, float nearest)>();
        var mateGaps = new List<float>();
        var moving = new Dictionary<UnitController, int>();
        var running = new Dictionary<UnitController, int>();
        var cameAt = new Dictionary<UnitController, float>();
        int targetSteps = 0, aloneSteps = 0;
        var offs = new Dictionary<UnitController, List<float>>();
        if (clips)
        {
            foreach (var u in units)
            {
                if (u.UsesClips)
                {
                    feet[u] = new PhysicsBattler.Locomotion.LocoMeasures(u.Clip.Anim, u.Clip.Loco, u.Clip.Poser);
                    lastRoot[u] = u.Root.position;
                    offs[u] = new List<float>();
                }
            }
        }

        float dt = Time.fixedDeltaTime;
        int shared = 0, attacks = 0;
        var started = new Dictionary<UnitController, int>();
        var blowsAt = new List<(float t, UnitController by, UnitController at, float duration)>();
        var tempoTime = new Dictionary<string, float>();
        void OnMove(UnitController u, MoveState s)
        {
            started[u] = (started.TryGetValue(u, out int n) ? n : 0) + 1;
            attacks++;
            blowsAt.Add((Time.time, u, s.Target as UnitController, s.Duration));
        }
        UnitController.MoveStarted += OnMove;
        // (Clip fighters' blows, which have no MoveState, and what every blow did: step 5 of the plan.)
        var clipBlows = new Dictionary<UnitController, List<string>>();
        var startedFrom = new Dictionary<string, int>();
        var turning = new List<(UnitController who, UnitController to, float since)>();
        var turnTimes = new List<float>();
        int turnsMissed = 0, outnumberedSteps = 0, surroundedSteps = 0;
        void OnClipStrike(UnitController u, string what, UnitController at)
        {
            started[u] = (started.TryGetValue(u, out int n) ? n : 0) + 1;
            attacks++;
            blowsAt.Add((Time.time, u, at, 1f));
            if (!clipBlows.TryGetValue(u, out var l)) clipBlows[u] = l = new List<string>();
            l.Add(what);
            if (at != null && at.UsesClips)
            {
                Vector3 from = u.Root.position - at.Root.position;
                from.y = 0f;
                float angle = Vector3.Angle(Quaternion.Euler(0f, at.FacingYaw, 0f) * Vector3.forward, from);
                string sector = angle < 45f ? "front" : angle < 135f ? "side" : "back";
                startedFrom[sector] = (startedFrom.TryGetValue(sector, out int sn) ? sn : 0) + 1;
                if (angle > 45f) turning.Add((at, u, Time.time));
            }
        }
        var hitKinds = new Dictionary<UnitController, Dictionary<string, int>>();
        var sectors = new Dictionary<string, int>();
        float damageAll = 0f;
        void OnHit(HitReport h)
        {
            if (h.Attacker == null) return;
            if (h.Struck != null && h.Struck.UsesClips && h.Result.Impulse.sqrMagnitude > 1e-6f)
            {
                // (Where the blow came from, against the way the one struck faced: in front, beside, behind.)
                Vector3 from = h.Attacker.Root.position - h.Struck.Root.position;
                from.y = 0f;
                float angle = Vector3.Angle(Quaternion.Euler(0f, h.Struck.FacingYaw, 0f) * Vector3.forward, from);
                string sector = angle < 45f ? "front" : angle < 135f ? "side" : "back";
                string what = h.Parried ? "parried" : h.Shield ? "blocked" : "landed";
                var seen = h.Struck.ClipThreats != null ? ((List<UnitController.ClipThreat>)null) : null;
                bool sawIt = false;
                foreach (var th in h.Struck.ClipThreats) if (th.Attacker == h.Attacker && th.Seen) sawIt = true;
                what += sawIt ? " (seen)" : " (unseen)";
                string key = sector + " " + what;
                sectors[key] = (sectors.TryGetValue(key, out int sn) ? sn : 0) + 1;
            }

            if (!hitKinds.TryGetValue(h.Attacker, out var k)) hitKinds[h.Attacker] = k = new Dictionary<string, int>();
            string kind = h.Kind ?? "?";
            k[kind] = (k.TryGetValue(kind, out int n) ? n : 0) + 1;
            damageAll += h.Result.Damage;
            if (Environment.GetEnvironmentVariable("FIGHT_HITS") != null)
            {
                Console.WriteLine($"  {Time.time:0.00} {h.Attacker.name} > {h.Struck.name}: {kind}, {h.Part}, {h.Result.Damage:0} damage, {h.Result.Reaction}; {h.Struck.Health:0} hp left");
            }
        }
        UnitController.ClipStrikeStarted += OnClipStrike;
        UnitController.Hit += OnHit;
        // Milestone 10: blows inside combos, and the time from one link's start to the next.
        int comboBlows = 0, combos = 0;
        var linkGaps = new List<float>();
        var contactGaps = new List<float>();
        var comboEnds = new Dictionary<string, int>();
        void OnCombo(UnitController u, ComboRun c, string what)
        {
            if (what == "started")
            {
                combos++;
                comboBlows++;
            }
            else if (what.StartsWith("link"))
            {
                comboBlows++;
                if (c.LinkStarts.Count >= 2)
                {
                    linkGaps.Add(c.LinkStarts[c.LinkStarts.Count - 1] - c.LinkStarts[c.LinkStarts.Count - 2]);
                }
            }
            else
            {
                // (The strikes' rhythm: from one link's striking moment to the next's.)
                for (int i = 1; i < c.Contacts.Count; i++)
                {
                    contactGaps.Add(c.Contacts[i] - c.Contacts[i - 1]);
                }

                string why = what.Replace("ended: ", "");
                why = why.StartsWith("stopped by") ? (why.EndsWith("'s shield") ? "blocked" : "stopped in a body") : why;
                comboEnds[why] = (comboEnds.TryGetValue(why, out int k) ? k : 0) + 1;
            }
        }
        UnitController.ComboChanged += OnCombo;
        int steps = (int)(limit * 60f) + 600;
        float startTime = Time.time;
        try
        {
            for (int step = 0; step < steps && !loop.Over; step++)
            {
                Time.time += dt;
                if (recorder != null)
                {
                    Call(recorder, "FixedUpdate");
                }

                loop.TimeLimit = limit;
                loop.Step(units, dt, Time.time);
                foreach (var u in units)
                {
                    if (u.IsAlive && u.State == UnitState.Standing && u.Tempo != null && u.TempoOn)
                    {
                        string k = u.Tempo.State.ToString();
                        tempoTime[k] = (tempoTime.TryGetValue(k, out float sec) ? sec : 0f) + dt;
                    }
                }

                foreach (var unit in units)
                {
                    Call(unit, "FixedUpdate");
                    if (!unit.UsesClips)
                    {
                        PhysicsBattler.Tests.BalanceTests.Integrate(unit, dt);
                    }
                }

                if (clips)
                {
                    // (How long a unit took to face an attacker that started a blow from its side or behind.)
                    for (int k = turning.Count - 1; k >= 0; k--)
                    {
                        var (who, to, since) = turning[k];
                        Vector3 from = to.Root.position - who.Root.position;
                        from.y = 0f;
                        float off = Vector3.Angle(Quaternion.Euler(0f, who.FacingYaw, 0f) * Vector3.forward, from);
                        if (off < 30f) { turnTimes.Add(Time.time - since); turning.RemoveAt(k); }
                        else if (!who.IsAlive || !to.IsAlive || Time.time - since > 3f) { turnsMissed++; turning.RemoveAt(k); }
                    }

                    // (How spread round a unit its attackers stood: the plan "Fighting off several attackers", step 5.)
                    foreach (var u in units)
                    {
                        if (!u.IsAlive || !u.UsesClips || u.IsDown) continue;
                        var on = FightLoop.OnIt(units, u, UnitController.ClipEngagedWithin * u.Body.SizeFactor);
                        if (on.Count < 2) continue;
                        outnumberedSteps++;
                        if (FightLoop.Spread(u.Root.position, on) > 120f) surroundedSteps++;
                    }

                    // (How they spread: each standing one's nearest teammate, and whether its target has others of its side on it.)
                    foreach (var u in units)
                    {
                        if (!u.IsAlive || u.State != UnitState.Standing) continue;
                        float nearestMate = 99f;
                        foreach (var o in units)
                        {
                            if (o != u && o.IsAlive && o.Team == u.Team && o.State == UnitState.Standing)
                                nearestMate = Mathf.Min(nearestMate, Vector3.Distance(FightLoop.BodyAt(u), FightLoop.BodyAt(o)));
                        }
                        if (nearestMate < 90f) mateGaps.Add(nearestMate);
                        if (u.Opponent != null && u.Opponent.IsAlive)
                        {
                            int sharing = 0;
                            foreach (var o in units)
                            {
                                if (o != u && o.IsAlive && o.Team == u.Team && o.Opponent == u.Opponent) sharing++;
                            }
                            targetSteps++;
                            if (sharing == 0) aloneSteps++;
                        }
                    }

                    float t = Time.time - startTime;
                    foreach (var u in units)
                    {
                        if (!feet.TryGetValue(u, out var m))
                        {
                            continue;
                        }

                        if (u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.Cycle && u.Clip.Loco.Velocity.magnitude > 0.3f)
                        {
                            moving[u] = (moving.TryGetValue(u, out var mv) ? mv : 0) + 1;
                            if (u.Clip.Loco.Running) running[u] = (running.TryGetValue(u, out var rn) ? rn : 0) + 1;
                        }

                        // (The side it first came at its target from: the angle between its way in and the line between the teams.)
                        if (!cameAt.ContainsKey(u) && u.Opponent != null && Vector3.Distance(u.Root.position, u.Opponent.Root.position) < 2.2f * u.Body.SizeFactor)
                        {
                            Vector3 axis = (u.Team == 0 ? 1f : -1f) * Vector3.forward;
                            Vector3 way = u.Opponent.Root.position - u.Root.position;
                            way.y = 0f;
                            cameAt[u] = Vector3.Angle(axis, way);
                        }

                        m.Sample(dt, t, u.Clip.Loco.Current + (u.Clip.Loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot && u.Clip.Loco.Shot != null ? " " + u.Clip.Loco.Shot.Name : "") + (u.Clip.Fallen ? " fallen" : "") + (u.Clip.Striking ? " striking" : "") + (u.Clip.Stumbling ? " stumbling" : ""));
                        float jump = Vector3.Distance(u.Root.position, lastRoot[u]);
                        lastRoot[u] = u.Root.position;
                        if (!rootJump.TryGetValue(u, out var j) || jump > j.most)
                        {
                            rootJump[u] = (jump, t);
                        }

                        // (How far it is from where it would strike its opponent from: the fight's engage distance.)
                        if (u.Opponent != null)
                        {
                            Vector3 d = u.Opponent.Root.position - u.Root.position;
                            float off = new Vector2(d.x, d.z).magnitude - (u.Style.engageDistance * u.Body.SizeFactor + 0.25f * u.Opponent.Body.SizeFactor);
                            if (off < 0.35f * u.Body.SizeFactor && !reachedAt.ContainsKey(u))
                            {
                                reachedAt[u] = t;
                            }

                            if (reachedAt.ContainsKey(u))
                            {
                                offs[u].Add(off);
                            }
                        }
                    }
                }

                OnStep?.Invoke(units, dt);

                // Two standing bodies overlapping (free roam: the steering should keep them apart).
                int overlapping = FightLoop.Overlapping(units);
                if (overlapping > 0 && clips)
                {
                    for (int i = 0; i < units.Count; i++)
                    {
                        for (int j = i + 1; j < units.Count; j++)
                        {
                            float apart = Vector3.Distance(FightLoop.BodyAt(units[i]), FightLoop.BodyAt(units[j]));
                            if (units[i].IsAlive && units[j].IsAlive && apart < 0.8f * (FightLoop.BodyRadius(units[i]) + FightLoop.BodyRadius(units[j])))
                            {
                                string key = units[i].name + " & " + units[j].name;
                                if (Environment.GetEnvironmentVariable("FIGHT_OVERLAP_TRACE") != null && step % 6 == 0)
                                {
                                    string W(UnitController u) => $"{u.name} want {u.ClipWant.magnitude:0.00} ({Vector3.Dot(u.ClipWant, (FightLoop.BodyAt(u) - FightLoop.BodyAt(u == units[i] ? units[j] : units[i])).normalized):+0.00;-0.00} away) walks {u.Clip.Loco.Velocity.magnitude:0.00} {u.Clip.Loco.Current} circling {u.CircleOpponent}";
                                    Console.WriteLine($"  {Time.time - startTime:0.00} {key} {apart:0.00} m: {W(units[i])} | {W(units[j])}");
                                }

                                overlapPairs[key] = (overlapPairs.TryGetValue(key, out var op) ? op.steps : 0) + 1 is int k ? (k, Mathf.Min(op.nearest == 0f ? 9f : op.nearest, apart)) : op;
                            }
                        }
                    }
                }
                if (overlapping > 0 && shared++ % 120 == 0 && Environment.GetEnvironmentVariable("FIGHT_DEBUG") != null)
                {
                    Console.WriteLine($"  t {Time.time - startTime:0.0}: {overlapping} pairs of bodies overlapping");
                }
            }
        }
        finally
        {
            UnitController.MoveStarted -= OnMove;
            UnitController.ClipStrikeStarted -= OnClipStrike;
            UnitController.Hit -= OnHit;
            UnitController.ComboChanged -= OnCombo;
        }

        var sb = new System.Text.StringBuilder();
        if (clips)
        {
            sb.AppendLine($"  clip fighters ({(UnitController.ClipBlows ? "step 5: striking" : "step 4: moving only")}): {damageAll:0} damage dealt in all; bodies moved straight out of each other {UnitController.ClipDeepMoves} times, {UnitController.ClipDeepMoved:0.00} m in all");
            if (sectors.Count > 0)
            {
                var keys = new List<string>(sectors.Keys);
                keys.Sort();
                sb.AppendLine("    blows met, by where they came from (against the facing of the one struck): " + string.Join(", ", keys.ConvertAll(k => $"{k} {sectors[k]}")));
                sb.AppendLine($"    outnumbered (two or more on it): {outnumberedSteps * dt:0.0} s in all; with them more than 120 degrees apart round it {100f * surroundedSteps / Mathf.Max(1, outnumberedSteps):0}% of that");
                turnTimes.Sort();
                sb.AppendLine($"    turning to an attacker starting a blow from beside or behind: {turnTimes.Count} faced it (median {(turnTimes.Count > 0 ? turnTimes[turnTimes.Count / 2] : 0f):0.00} s, 90% {(turnTimes.Count > 0 ? turnTimes[turnTimes.Count * 9 / 10] : 0f):0.00} s), {turnsMissed} not within 3 s");
                sb.AppendLine("    blows started, by where the attacker stood (against its target's facing): " + string.Join(", ", new List<string>(startedFrom.Keys).ConvertAll(k => $"{k} {startedFrom[k]}")));
            }

            mateGaps.Sort();
            if (mateGaps.Count > 0)
            {
                sb.AppendLine($"    spread: nearest teammate median {mateGaps[mateGaps.Count / 2]:0.00} m (10% {mateGaps[mateGaps.Count / 10]:0.00}, least {mateGaps[0]:0.00}); a unit alone on its target {100f * aloneSteps / Mathf.Max(1, targetSteps):0}% of the time");
            }
            foreach (var u in units)
            {
                if (hitKinds.TryGetValue(u, out var k) || clipBlows.ContainsKey(u))
                {
                    var parts = new List<string>();
                    if (k != null) foreach (var pair in k) parts.Add($"{pair.Key} {pair.Value}");
                    int combosThrown = clipBlows.TryGetValue(u, out var l) ? l.FindAll(x => x.StartsWith("a combo")).Count : 0;
                    sb.AppendLine($"    {u.name}: {(l != null ? l.Count : 0)} blows ({combosThrown} combos); its hits: {(parts.Count > 0 ? string.Join(", ", parts) : "none")}; it fell {u.Clip.Falls - (u.IsAlive ? 0 : 1)} times alive, was thrown back stumbling {u.Clip.Stumbles} times{(u.IsAlive ? "" : ", killed")}");
                }
            }

            foreach (var pair in overlapPairs)
            {
                sb.AppendLine($"    overlapping: {pair.Key}, {pair.Value.steps} steps, as near as {pair.Value.nearest:0.00} m");
            }

            foreach (var u in units)
            {
                if (!feet.TryGetValue(u, out var m))
                {
                    continue;
                }

                var o = offs[u];
                o.Sort();
                string near = reachedAt.TryGetValue(u, out float at)
                    ? $"reached its target at {at:0.0} s, then off its striking distance median {(o.Count > 0 ? o[o.Count / 2] : 0f):+0.00;-0.00} m (10% {(o.Count > 0 ? o[o.Count / 10] : 0f):+0.00;-0.00}, 90% {(o.Count > 0 ? o[o.Count * 9 / 10] : 0f):+0.00;-0.00})"
                    : "never reached its target";
                if (Environment.GetEnvironmentVariable("FIGHT_FEET") != null)
                {
                    foreach (var line in m.Report().Split('\n'))
                    {
                        if (line.Contains("while planted")) sb.AppendLine($"      {u.name}: {line.Trim()}");
                    }
                }

                sb.AppendLine($"    {u.name}: ran {(moving.TryGetValue(u, out var mvn) && mvn > 0 ? 100f * (running.TryGetValue(u, out var rnn) ? rnn : 0) / mvn : 0f):0}% of its walking; came at its first target {(cameAt.TryGetValue(u, out var ca) ? $"{ca:0}° off head-on" : "never")}");
                sb.AppendLine($"    {u.name}: {near}; planted feet moved at most {m.PlantDriftWorst * 100f:0.0} cm ({m.PlantsDone} plants); the root's biggest step {rootJump[u].most:0.00} m at {rootJump[u].at:0.0} s; feet travelled {m.FeetTravel:0.0} m for the body's {m.BodyTravel:0.0}");
            }
        }

        foreach (var u in units)
        {
            sb.AppendLine($"  {u.name}: {(u.IsAlive ? $"alive, {u.Health:0}/{u.Profile.maxHealth:0} hp" : "dead")}{(u.Loadout?.Main == null ? ", unarmed" : "")}{(u.LegLost ? ", legless" : "")}, "
                          + $"{(started.TryGetValue(u, out int n) ? n : 0)} blows, {u.SeveredLimbs.Count} limbs lost");
        }

        // The tempo's headline (milestone 7a; Tools/Playtest/tempo.py has the rest): bursts, gaps, taking turns.
        var gaps = new List<float>();
        var afterEnd = new List<float>();
        var lastBy = new Dictionary<UnitController, float>();
        var lastEnd = new Dictionary<UnitController, float>();
        var lastInPair = new Dictionary<(UnitController, UnitController), UnitController>();
        int alternate = 0, same = 0;
        foreach (var (t, by, at, duration) in blowsAt)
        {
            if (lastBy.TryGetValue(by, out float before))
            {
                gaps.Add(t - before);
                afterEnd.Add(t - lastEnd[by]);
            }

            lastBy[by] = t;
            lastEnd[by] = t + duration;
            if (at == null)
            {
                continue;
            }

            var key = units.IndexOf(by) < units.IndexOf(at) ? (by, at) : (at, by);
            if (lastInPair.TryGetValue(key, out var previous))
            {
                if (previous == by) same++; else alternate++;
            }

            lastInPair[key] = by;
        }

        gaps.Sort();
        if (gaps.Count > 0)
        {
            int bursts = afterEnd.FindAll(g => g < 0.6f).Count;
            float total = 0f;
            foreach (var v in tempoTime.Values) total += v;
            var shares = new List<string>();
            foreach (var pair in tempoTime) shares.Add($"{pair.Key.ToLowerInvariant()} {100f * pair.Value / Mathf.Max(1e-3f, total):0}%");
            if (shares.Count > 0) sb.AppendLine("  tempo (time on their feet): " + string.Join(", ", shares));
            sb.AppendLine($"  gaps between a unit's own blows: median {gaps[gaps.Count / 2]:0.0} s; in bursts (next within 0.6 s of the last ending): {100f * bursts / afterEnd.Count:0}%; "
                          + $"within pairs the next blow was the other's {100f * alternate / Mathf.Max(1, alternate + same):0}% of the time");
        }

        linkGaps.Sort();
        var ends = new List<string>();
        foreach (var pair in comboEnds) ends.Add($"{pair.Key} {pair.Value}");
        contactGaps.Sort();
        sb.AppendLine($"  combos: {combos}, {100f * comboBlows / Mathf.Max(1, attacks):0}% of blows inside them (target 25-40%); striking moment to the next: median {(contactGaps.Count > 0 ? contactGaps[contactGaps.Count / 2] : 0f):0.00} s "
                      + $"(target 0.4-0.7, {contactGaps.Count}); link start to the next: median {(linkGaps.Count > 0 ? linkGaps[linkGaps.Count / 2] : 0f):0.00} s; ended: {string.Join(", ", ends)}");
        sb.Append($"  steps with standing bodies overlapping: {shared}; times a unit made no headway and went round: {loop.Stalls}");
        stats = sb.ToString();
        string result = loop.Over ? $"fight over: {loop.Result}" : $"not over after {Time.time - startTime:0} s";
        foreach (var u in units)
        {
            Call(u, "OnDestroy");
            UnityEngine.Object.DestroyImmediate(u.gameObject);
        }

        return result + $"; {attacks} blows in all";
    }

    private static void Call(object target, string method)
    {
        target?.GetType().GetMethod(method, Any)?.Invoke(target, null);
    }
}
