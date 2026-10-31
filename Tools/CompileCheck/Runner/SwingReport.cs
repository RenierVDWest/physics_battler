// Measures every move as it is actually played: arc, speed, depth past the aim point, body turn,
// steps, accuracy, clearance of its own body, hip drop. Against every target size.
//
//   dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll swings [film output folder]
//
// With a folder, also records a gallery of blows (each move once, by a medium sword unit and a
// small and a large club unit, at a medium target) with the playtest recorder, so
// Tools/Playtest/analyze.py draws their film strips.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class SwingReport
{
    private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public sealed class Measure
    {
        public float PlayClear = float.MaxValue, PlayClearT;
        public string PlayClearPart = "";
        public float Error = -1f, ContactSpeed, PeakSpeed, Arc, Depth, HipTurnRange, HipDrop, Clearance, TipSpeed;
        public int StepsBeforeContact;
        public bool Planned;
        public string Why = "";
    }

    public static WeaponDefinition Sword()
    {
        var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
        sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
        return sword;
    }

    public static WeaponDefinition Club()
    {
        var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
        club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising) };
        return club;
    }

    public static AimPart PartFor(MoveDefinition move)
    {
        foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Head, AimPart.KneeL })
        {
            if (move.targets.Contains(part))
            {
                return part;
            }
        }

        return AimPart.Chest;
    }

    public static int Run(string film)
    {
        var sword = Sword();
        var club = Club();
        var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
        Console.WriteLine("move              attacker->target part   | err cm | contact m/s | peak m/s | tip m/s | arc deg | depth cm | chest deg| steps | drop cm | clear cm");
        foreach (var weapon in new[] { sword, club })
        {
            foreach (var move in weapon.moves)
            {
                foreach (var a in new[] { "Small", "Medium", "Large" })
                {
                    foreach (var b in new[] { "Small", "Medium", "Large" })
                    {
                        string only = Environment.GetEnvironmentVariable("SWING_ONLY");
                        if (!string.IsNullOrEmpty(only) && only != $"{move.name} {a[0]}{b[0]}")
                        {
                            continue;
                        }

                        // The first part (body first) the planner takes; else the reason the first one fails.
                        var part = PartFor(move);
                        Measure m = null;
                        var order = new[] { AimPart.Chest, AimPart.Belly, AimPart.Neck, AimPart.Head, AimPart.ElbowL, AimPart.KneeL };
                        string onlyPart = Environment.GetEnvironmentVariable("SWING_PART");
                        if (!string.IsNullOrEmpty(onlyPart))
                        {
                            order = new[] { (AimPart)Enum.Parse(typeof(AimPart), onlyPart) };
                        }

                        foreach (var p in order)
                        {
                            if (!move.targets.Contains(p))
                            {
                                continue;
                            }

                            var tried = Play(a, b, weapon, weapon == sword ? shield : null, move, p, null);
                            if (m == null || tried.Planned && !m.Planned)
                            {
                                m = tried;
                                part = p;
                            }

                            if (m.Planned)
                            {
                                break;
                            }
                        }

                        string head = $"{move.name,-17} {a[0]}->{b[0]} {part,-7}";
                        if (!m.Planned)
                        {
                            Console.WriteLine($"{head} | not planned: {m.Why}");
                            continue;
                        }

                        Console.WriteLine($"{head} | {m.Error * 100f,6:0.0} | {m.ContactSpeed,11:0.0} | {m.PeakSpeed,8:0.0} | {m.TipSpeed,7:0.0} | {m.Arc,7:0} | {m.Depth * 100f,8:0} | {m.HipTurnRange,8:0} | {m.StepsBeforeContact,5} | {m.HipDrop * 100f,7:0.0} | {m.Clearance * 100f,8:0.0} | played {m.PlayClear * 100f:0.0} ({m.PlayClearPart} at t {m.PlayClearT:0.00})");
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(film))
        {
            Gallery(film, sword, club, shield);
        }

        return 0;
    }

    public static UnitController Build(string body, Vector3 at, float yaw, int team, int layer = 0)
    {
        var unit = AimMatrix.Build(body, at, yaw, team);
        unit.DealsHits = false;
        return unit;
    }

    /// <summary>Plays one blow (attacker at the origin facing +Z, the target where it would engage) and measures it.</summary>
    public static Measure Play(string a, string b, WeaponDefinition weapon, WeaponDefinition off, MoveDefinition move, AimPart part, Action<UnitController, UnitController> each)
    {
        var m = new Measure();
        var attacker = Build(a, Vector3.zero, 0f, 0);
        var target = Build(b, Vector3.forward * 5f, 180f, 1);
        try
        {
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            target.Place(Vector3.forward * distance, 180f);
            attacker.Equip(weapon, off);
            for (int i = 0; i < 20; i++)
            {
                attacker.PreviewPose(1f / 60f);
                target.PreviewPose(1f / 60f);
            }

            var state = MovePlanner.Prepare(attacker, target, move, part);
            if (state == null)
            {
                m.Why = "out of lunge range or reach";
                return m;
            }

            float tryErr = attacker.TryOut(state);
            float clear = attacker.Clearance(state, MovePlanner.ClearanceSamples, out var closest, out float when, out bool blade);
            m.Clearance = clear;
            if (tryErr > MovePlanner.Tolerance || clear < -MovePlanner.OwnBodyClearance * attacker.Body.SizeFactor)
            {
                m.Why = tryErr > MovePlanner.Tolerance ? $"misses by {tryErr * 100f:0.0} cm in the dry run" : $"{(blade ? "weapon" : "arm")} {-clear * 100f:0} cm into its own {closest} at t={when:0.00}";
                if (each == null)
                {
                    return m;
                }
            }
            else
            {
                m.Planned = true;
            }

            Vector3 pivotC = state.PivotAt(move.ContactProgress);
            Vector3 e1 = (state.Aim - pivotC).normalized;
            Vector3 last = Vector3.zero, lastTip = Vector3.zero, lastRadial = Vector3.zero;
            bool have = false;
            float minYaw = float.MaxValue, maxYaw = float.MinValue;
            int stepsAtStart = attacker.Stance.StepsTaken;
            bool contactSeen = false;
            void OnContact(UnitController unit, MoveState s, ContactReport report)
            {
                if (unit == attacker)
                {
                    m.Error = report.AnimError;
                }
            }

            UnitController.MoveContact += OnContact;
            try
            {
                attacker.StartMove(state);
                float dt = 1f / 60f;
                for (int i = 0; i < 600 && attacker.IsAttacking; i++)
                {
                    float before = state.T;
                    attacker.PreviewPose(dt);
                    target.PreviewPose(dt);
                    each?.Invoke(attacker, target);
                    Vector3 strike = attacker.Pose.StrikePoint;
                    Vector3 tip = attacker.MainWeapon.WorldPoint(weapon.tip);
                    Vector3 radial = strike - state.PivotAt(state.Progress);
                    if (have)
                    {
                        float speed = Vector3.Distance(strike, last) / dt;
                        if (Environment.GetEnvironmentVariable("SWING_TRACE") == "1")
                        {
                            Console.WriteLine($"   t {state.T:0.000} p {state.Progress:0.000} blend {state.Blend:0.00} body {state.Body:0.00} speed {speed:0.0} err {Vector3.Distance(strike, state.PathPoint(state.Progress)) * 100f:0.0} cm aim {Vector3.Distance(strike, state.Aim) * 100f:0.0} pathaim {Vector3.Distance(state.PathPoint(move.ContactProgress), state.Aim) * 100f:0.0} tc {state.ContactTime:0.000} drop {attacker.Pose.PelvisDrop * 100f:0} bend {attacker.Pose.SpineBend:0} feet {attacker.Stance.Position(0).x:0.00},{attacker.Stance.Position(0).z:0.00}{(attacker.Stance.Stepping(0) ? "*" : "")} {attacker.Stance.Position(1).x:0.00},{attacker.Stance.Position(1).z:0.00}{(attacker.Stance.Stepping(1) ? "*" : "")} pelvis {attacker.Anim.Part(PartId.Pelvis).position.x:0.00},{attacker.Anim.Part(PartId.Pelvis).position.y:0.00},{attacker.Anim.Part(PartId.Pelvis).position.z:0.00} root {attacker.Root.position.z:0.00}");
                        }

                        m.PeakSpeed = Mathf.Max(m.PeakSpeed, speed);
                        m.TipSpeed = Mathf.Max(m.TipSpeed, Vector3.Distance(tip, lastTip) / dt);
                        if (!contactSeen && state.T >= state.ContactTime)
                        {
                            m.ContactSpeed = speed;
                            contactSeen = true;
                            m.StepsBeforeContact = attacker.Stance.StepsTaken - stepsAtStart;
                        }

                        if (state.Blend > 0.9f && state.Progress >= (move.SwingTravel(0) + move.SwingTravel(1)) / move.SwingTotal && state.T <= move.SwingFollowEnd)
                        {
                            m.Arc += Vector3.Angle(lastRadial, radial);
                        }
                    }

                    if (state.T >= state.ContactTime)
                    {
                        m.Depth = Mathf.Max(m.Depth, Vector3.Dot(strike - state.Aim, e1));
                    }

                    var chest = attacker.Anim.Part(PartId.Chest);
                    Vector3 chestFwd = chest.rotation * Vector3.forward;
                    float yaw = Mathf.DeltaAngle(attacker.FacingYaw, Mathf.Atan2(chestFwd.x, chestFwd.z) * Mathf.Rad2Deg);
                    minYaw = Mathf.Min(minYaw, yaw);
                    maxYaw = Mathf.Max(maxYaw, yaw);
                    m.HipDrop = Mathf.Max(m.HipDrop, attacker.Pose.PelvisDrop);
                    float c = BodyClearance.Measure(attacker.Rig, attacker.Anim, weapon.length * 0.98f, attacker.Loadout.Scale, out var cp, out bool cb);
                    if (c < m.PlayClear)
                    {
                        m.PlayClear = c;
                        m.PlayClearPart = (cb ? "weapon into " : "arm into ") + cp;
                        m.PlayClearT = state.T;
                    }
                    last = strike;
                    lastTip = tip;
                    lastRadial = radial;
                    have = true;
                }
            }
            finally
            {
                UnitController.MoveContact -= OnContact;
            }

            m.HipTurnRange = maxYaw - minYaw;
            return m;
        }
        finally
        {
            if (each == null)
            {
                UnityEngine.Object.DestroyImmediate(attacker.gameObject);
                UnityEngine.Object.DestroyImmediate(target.gameObject);
            }
        }
    }

    /// <summary>Records each move once with the playtest recorder, for film strips.</summary>
    private static void Gallery(string output, WeaponDefinition sword, WeaponDefinition club, WeaponDefinition shield)
    {
        Directory.CreateDirectory(output);
        string assets = Application.dataPath;
        Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");
        var recorderObject = new GameObject("Recorder");
        var recorder = recorderObject.AddComponent<PlaytestRecorder>();
        Call(recorder, "OnEnable");
        float dt = Time.fixedDeltaTime;
        var lineup = new List<(string a, WeaponDefinition w, WeaponDefinition off)> { ("Medium", sword, shield), ("Large", club, null), ("Small", club, null) };
        try
        {
            Application.dataPath = assets;
            foreach (var (a, weapon, off) in lineup)
            {
                foreach (var move in weapon.moves)
                {
                    var attacker = AimMatrix.Build(a, Vector3.zero, 0f, 0);
                    attacker.name = $"{a} {move.name.Replace("MD_", "")}";
                    attacker.DealsHits = false;
                    var target = AimMatrix.Build("Medium", Vector3.forward * 5f, 180f, 1);
                    target.name = $"Target of {attacker.name}";
                    float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                    target.Place(Vector3.forward * distance, 180f);
                    attacker.Equip(weapon, off);
                    Call(recorder, "Follow", attacker);
                    Call(recorder, "Follow", target);
                    for (int i = 0; i < 20; i++)
                    {
                        Time.time += dt;
                        Call(recorder, "FixedUpdate");
                        attacker.PreviewPose(dt);
                        target.PreviewPose(dt);
                    }

                    var state = MovePlanner.Prepare(attacker, target, move, PartFor(move));
                    if (state != null && attacker.StartMove(state))
                    {
                        for (int i = 0; i < 200 && attacker.IsAttacking; i++)
                        {
                            Time.time += dt;
                            Call(recorder, "FixedUpdate");
                            attacker.PreviewPose(dt);
                            target.PreviewPose(dt);
                        }
                    }

                    for (int i = 0; i < 20; i++)
                    {
                        Time.time += dt;
                        Call(recorder, "FixedUpdate");
                        attacker.PreviewPose(dt);
                        target.PreviewPose(dt);
                    }

                    UnityEngine.Object.DestroyImmediate(attacker.gameObject);
                    UnityEngine.Object.DestroyImmediate(target.gameObject);
                }
            }
        }
        finally
        {
            Application.dataPath = Path.Combine(Path.GetFullPath(output), "Assets");
            Call(recorder, "OnDisable");
            Application.dataPath = assets;
        }

        Console.WriteLine("gallery session: " + recorder.SessionPath);
    }

    private static void Call(object target, string method, params object[] args)
    {
        target.GetType().GetMethod(method, Any)?.Invoke(target, args.Length > 0 ? args : null);
    }
}
