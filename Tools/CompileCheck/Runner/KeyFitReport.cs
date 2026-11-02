// `keyfit`: fits a blow's arm keys (the hand at wind-up and follow-through, the wrist and the elbow
// null at all three keys) by playing it: a medium unit at a medium's parts, scored on what the swing
// tests and a playtest ask of a blow. The arc the striking part sweeps from the release to the
// follow-through's end (up to the big swing's 280 and a little), landing on the aim, the arm and blade
// clear of the attacker's own body, the wrist and shoulder inside their limits, no arm part turning
// fast in a step, fast through contact against the wind-up, and the wrist easy (well inside its
// range). A (1+8) evolution strategy from the move's keys. Prints the
// keys found as C# for MoveDefinition's defaults.
//   KEY_MOVE=SwordCut (a MoveStyle name; all swings when unset), KEY_ROUNDS (default 6)
using System;
using System.Collections.Generic;
using System.Globalization;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

internal static class KeyFitReport
{
    internal sealed class Metrics
    {
        public bool ok;
        public Vector3 bladeWindUp, bladeFollow, handWindUpAt, handFollowAt;   // (aim frame: the blade's way, the hand from the shoulder)
        public float arc, error = 1f, clear = 1f, wrist, shoulder, fastest, contactSpeed, windUpSpeed, depth, comfort;
        public string where = "";

        public override string ToString() => ok
            ? $"arc {arc:0}°, lands {error * 100f:0.0} cm, clear {clear * 100f:0.0} cm, wrist past {wrist:0}, shoulder past {shoulder:0}, fastest {fastest:0}°/step{where}, contact {contactSpeed:0.0} m/s (wind-up {windUpSpeed:0.0}), depth {depth * 100f:0} cm"
            : "not planned";
    }

    private static readonly float[] Steps = { 0.05f, 0.05f, 0.05f, 10f, 10f, 5f, 15f, 10f, 10f, 5f, 15f, 0.05f, 0.05f, 0.05f, 10f, 10f, 5f, 15f };

    private static float[] Get(MoveDefinition m) => new[]
    {
        m.handWindUp.x, m.handWindUp.y, m.handWindUp.z, m.wristWindUp.x, m.wristWindUp.y, m.wristWindUp.z, m.elbowWindUp,
        m.wristContact.x, m.wristContact.y, m.wristContact.z, m.elbowContact,
        m.handFollow.x, m.handFollow.y, m.handFollow.z, m.wristFollow.x, m.wristFollow.y, m.wristFollow.z, m.elbowFollow,
    };

    /// <summary>How far a medium arm reaches from the shoulder to the wrist (a hand key further out only stretches it straight).</summary>
    private static float reach = 0.57f;

    private static Vector3 Hand(float x, float y, float z) => Vector3.ClampMagnitude(new Vector3(x, y, z), 0.97f * reach);

    private static void Set(MoveDefinition m, float[] p)
    {
        m.handWindUp = Hand(p[0], p[1], p[2]);
        m.wristWindUp = Wrist(p[3], p[4], p[5]);
        m.elbowWindUp = Mathf.Clamp(p[6], -180f, 180f);
        m.wristContact = Wrist(p[7], p[8], p[9]);
        m.elbowContact = Mathf.Clamp(p[10], -180f, 180f);
        m.handFollow = Hand(p[11], p[12], p[13]);
        m.wristFollow = Wrist(p[14], p[15], p[16]);
        m.elbowFollow = Mathf.Clamp(p[17], -180f, 180f);
    }

    private static Vector3 Wrist(float t, float f, float d) => new Vector3(
        Mathf.Clamp(t, -ArmRig.TwistLimit, ArmRig.TwistLimit), Mathf.Clamp(f, -ArmRig.FlexLimit, ArmRig.FlexLimit), Mathf.Clamp(d, -ArmRig.DeviationLimit, ArmRig.RadialLimit));

    /// <summary>Plays a blow (a body with a weapon at a medium's part) and measures it.</summary>
    internal static Metrics Play(MoveDefinition move, WeaponDefinition weapon, string body, AimPart part, string targetBody = "Medium")
    {
        var m = new Metrics();
        weapon.moves = new[] { move };
        var a = AimMatrix.Build(body, Vector3.zero, 0f, 0);
        var target = AimMatrix.Build(targetBody, Vector3.forward * 5f, 180f, 1);
        void OnContact(UnitController unit, MoveState s, ContactReport report)
        {
            if (unit == a) m.error = report.AnimError;
        }

        UnitController.MoveContact += OnContact;
        try
        {
            float distance = a.Style.engageDistance * a.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            target.Place(Vector3.forward * distance, 180f);
            a.DealsHits = false;
            a.Equip(weapon, null);
            for (int i = 0; i < 20; i++)
            {
                a.PreviewPose(1f / 60f);
                target.PreviewPose(1f / 60f);
            }

            var state = MovePlanner.Prepare(a, target, move, part);
            if (state == null || !a.StartMove(state))
            {
                return m;
            }

            m.ok = true;
            float released = (move.SwingTravel(0) + move.SwingTravel(1)) / move.SwingTotal;
            float length = weapon.length * 0.98f;
            var parts = new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR };
            var was = new Quaternion[3];
            Vector3 last = Vector3.zero;
            bool have = false;
            for (int i = 0; i < 600 && a.IsAttacking; i++)
            {
                a.PreviewPose(1f / 60f);
                target.PreviewPose(1f / 60f);
                Vector3 strike = a.Pose.StrikePoint;
                if (have && state.Progress < released && state.T > 0.1f)
                {
                    m.windUpSpeed = Mathf.Max(m.windUpSpeed, Vector3.Distance(strike, last) * 60f);
                }

                float c = BodyClearance.Measure(a.Rig, a.Anim, length, a.Loadout.Scale, out _, out _);
                m.clear = Mathf.Min(m.clear, c);
                var arm = a.Pose.LastWeaponArm;
                m.wrist = Mathf.Max(m.wrist, arm.wristExcess);
                m.shoulder = Mathf.Max(m.shoulder, arm.shoulderPast);
                for (int k = 0; k < 3; k++)
                {
                    var q = a.Anim.Part(parts[k]).rotation;
                    if (have)
                    {
                        float turn = Quaternion.Angle(was[k], q);
                        if (turn > m.fastest)
                        {
                            m.fastest = turn;
                            m.where = $" ({parts[k]} at T {state.T:0.00})";
                        }
                    }

                    was[k] = q;
                }

                // (The pose at the wind-up's end and the follow-through's end, as the seeds describe them.)
                foreach (var (t, isWindUp) in new[] { (move.WindUpEnd, true), (move.SwingFollowEnd, false) })
                {
                    if (state.T >= t && (isWindUp ? m.bladeWindUp : m.bladeFollow) == Vector3.zero)
                    {
                        Vector3 blade = Quaternion.Inverse(state.Frame) * (a.Anim.SlotWeaponR.rotation * Vector3.forward);
                        Vector3 handAt = Quaternion.Inverse(state.Frame) * (a.Anim.Part(PartId.HandR).position - a.Anim.Part(PartId.UpperArmR).position) / state.Scale;
                        if (isWindUp) { m.bladeWindUp = blade; m.handWindUpAt = handAt; }
                        else { m.bladeFollow = blade; m.handFollowAt = handAt; }
                    }
                }

                last = strike;
                have = true;
            }

            m.arc = state.Arc;
            m.contactSpeed = state.ContactSpeed;
            m.depth = state.Depth;
            return m;
        }
        finally
        {
            UnitController.MoveContact -= OnContact;
            UnityEngine.Object.DestroyImmediate(a.gameObject);
            UnityEngine.Object.DestroyImmediate(target.gameObject);
        }
    }

    private static float Comfort(MoveDefinition m)
    {
        float W(Vector3 w) => Mathf.Pow(Mathf.Abs(w.x) / ArmRig.TwistLimit, 2f) + Mathf.Pow(Mathf.Abs(w.y) / ArmRig.FlexLimit, 2f)
                              + Mathf.Pow(w.z >= 0f ? w.z / ArmRig.RadialLimit : -w.z / ArmRig.DeviationLimit, 2f);
        return W(m.wristWindUp) + W(m.wristContact) + W(m.wristFollow);
    }

    /// <summary>The designed poses the fit keeps near (none: free).</summary>
    private static bool haveSeeds;
    private static Pose seedWindUp, seedFollow;

    internal static float Score(List<Metrics> runs, MoveDefinition move, float arcWanted)
    {
        float s = 0f;
        if (haveSeeds)
        {
            // (Looking as designed: the blade's way at the wind-up and follow-through within 25° of the
            // design, the hand within 8 cm of it; past that it costs, and dearly.)
            foreach (var r in runs)
            {
                if (!r.ok) continue;
                s -= (3f * Mathf.Max(0f, Vector3.Angle(r.bladeWindUp, seedWindUp.blade) - 25f) + 3f * Mathf.Max(0f, Vector3.Angle(r.bladeFollow, seedFollow.blade) - 25f)
                      + 600f * Mathf.Max(0f, Vector3.Distance(r.handWindUpAt, seedWindUp.hand) - 0.08f) + 600f * Mathf.Max(0f, Vector3.Distance(r.handFollowAt, seedFollow.hand) - 0.08f)) / runs.Count;
            }
        }

        foreach (var r in runs)
        {
            if (!r.ok)
            {
                s -= 500f;
                continue;
            }

            s += Mathf.Min(r.arc, arcWanted + 20f)
                 - 40f * Mathf.Max(0f, r.error * 100f - 1.5f)
                 - 40f * Mathf.Max(0f, 2.5f - r.clear * 100f)
                 - 30f * r.wrist - 30f * r.shoulder
                 - 4f * Mathf.Max(0f, r.fastest - 30f)
                 - 2f * Mathf.Max(0f, r.arc - 330f)
                 - 20f * Mathf.Max(0f, 12f - r.contactSpeed)
                 - 20f * Mathf.Max(0f, 1.3f * r.windUpSpeed - r.contactSpeed)
                 - 4f * Mathf.Max(0f, 15f - r.depth * 100f);
        }

        // (Elbows kept to where people hold them: hanging, out or up for a forehand, across for a
        // backhand; far round behind is the arm wound up as no one does.)
        float elbows = 0f;
        foreach (float e in new[] { move.elbowWindUp, move.elbowContact, move.elbowFollow })
        {
            elbows += Mathf.Pow(Mathf.Max(0f, Mathf.Abs(e) - 100f) / 40f, 2f);
        }

        return s / runs.Count - 25f * Comfort(move) - 20f * elbows;
    }

    public static int Run()
    {
        string only = Environment.GetEnvironmentVariable("KEY_MOVE");
        int rounds = int.TryParse(Environment.GetEnvironmentVariable("KEY_ROUNDS"), out int r) ? r : 40;
        foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle)))
        {
            if (only != null && style.ToString() != only)
            {
                continue;
            }

            var move = MoveDefinition.Create(style);
            if (move == null || move.shape != MoveShape.Swing)
            {
                continue;
            }

            var weapon = style.ToString().StartsWith("Sword") ? SwingReport.Sword() : SwingReport.Club();
            string body = move.UsableBy(1f) ? "Medium" : "Small";
            var parts = new List<AimPart>();
            foreach (var p in new[] { AimPart.Chest, AimPart.KneeL, AimPart.Head, AimPart.Belly, AimPart.Neck, AimPart.ElbowR })
            {
                if (move.targets.Contains(p) && parts.Count < 3)
                {
                    parts.Add(p);
                }
            }

            float arcWanted = style == MoveStyle.SwordBackhand || style == MoveStyle.ClubBackhand ? 250f : 280f;
            List<Metrics> Evaluate()
            {
                var list = new List<Metrics>();
                foreach (var p in parts)
                {
                    list.Add(Play(move, weapon, body, p));
                }

                return list;
            }

            {
                var probe = AimMatrix.Build("Medium", Vector3.zero, 0f, 0);
                reach = probe.Anim.RestLocalPosition(PartId.ForearmR).magnitude + probe.Anim.RestLocalPosition(PartId.HandR).magnitude;
                UnityEngine.Object.DestroyImmediate(probe.gameObject);
            }

            if (Environment.GetEnvironmentVariable("KEY_SEED") != null)
            {
                Seed(move, weapon, body, parts[0]);
            }

            ArmFilmReport.LoadKeys(move, style, Environment.GetEnvironmentVariable("KEY_FROM"));

            var x = Get(move);
            if (int.TryParse(Environment.GetEnvironmentVariable("KEY_REPEAT"), out int repeat))
            {
                for (int k = 0; k < repeat; k++)
                {
                    Console.WriteLine($"  again: {string.Join(" | ", Evaluate())}");
                }
            }

            var runs = Evaluate();
            float best = Score(runs, move, arcWanted);
            Console.WriteLine($"{style} ({body}, at {string.Join(", ", parts)}) from {best:0}: {runs[0]}");
            // A (1+8) evolution strategy: every key nudged at random round the best so far, the
            // nudges grown on success and shrunk otherwise (coordinate steps alone stall in a swing's
            // lumpy score).
            var random = new System.Random(1);
            float sigma = 1f;
            float Gauss()
            {
                double u = 1.0 - random.NextDouble(), v = random.NextDouble();
                return (float)(Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v));
            }

            for (int round = 0; round < rounds; round++)
            {
                float[] genBest = null;
                List<Metrics> genRuns = null;
                float genScore = float.MinValue;
                for (int k = 0; k < 8; k++)
                {
                    var y = (float[])x.Clone();
                    for (int i = 0; i < y.Length; i++)
                    {
                        if (random.NextDouble() < 0.5)
                        {
                            y[i] += Gauss() * 2f * Steps[i] * sigma;
                        }
                    }

                    Set(move, y);
                    var tried = Evaluate();
                    float sc = Score(tried, move, arcWanted);
                    if (sc > genScore)
                    {
                        genScore = sc;
                        genBest = Get(move);
                        genRuns = tried;
                    }
                }

                if (genScore > best)
                {
                    best = genScore;
                    x = genBest;
                    runs = genRuns;
                    sigma = Mathf.Min(2f, sigma * 1.3f);
                }
                else
                {
                    sigma *= 0.75f;
                }

                Set(move, x);
                if (round % 5 == 4 || round == rounds - 1)
                {
                    Console.WriteLine($"  round {round}: {best:0} (sigma {sigma:0.00}): {string.Join(" | ", runs)}");
                }

                if (sigma < 0.1f)
                {
                    break;
                }
            }

            Set(move, x);
            string V(Vector3 v) => $"new Vector3({F(v.x)}, {F(v.y)}, {F(v.z)})";
            Console.WriteLine($"  {style}: hand windUp = {V(move.handWindUp)}; follow = {V(move.handFollow)};");
            Console.WriteLine($"  {style}: wrist windUp = {V(move.wristWindUp)}; contact = {V(move.wristContact)}; follow = {V(move.wristFollow)};");
            Console.WriteLine($"  {style}: elbow windUp = {F(move.elbowWindUp)}; contact = {F(move.elbowContact)}; follow = {F(move.elbowFollow)};");
        }

        return 0;
    }

    // ---- Seeds: a blow posed as a person holds it at its keys -------------------------------------
    // Where the hand is (from the shoulder, aim frame), where the elbow null points, and where the blade
    // points (aim frame); the wrist found that points the blade so, as easily as it can.
    private struct Pose
    {
        public Vector3 hand, blade;
        public float elbow;
        public Pose(Vector3 hand, float elbow, Vector3 blade) { this.hand = hand; this.elbow = elbow; this.blade = blade; }
    }

    private static bool Seeds(MoveStyle style, out Pose windUp, out Pose follow)
    {
        switch (style)
        {
            case MoveStyle.SwordCut:
            case MoveStyle.ClubSwing:
                // Drawn back high behind the right shoulder, the elbow out, the blade pointing back;
                // through to the left hip.
                windUp = new Pose(new Vector3(0.28f, 0.3f, -0.25f), 100f, new Vector3(0.3f, 0f, -1f));
                follow = new Pose(new Vector3(-0.35f, -0.4f, 0.2f), 10f, new Vector3(-0.6f, -0.3f, -0.8f));
                return true;
            case MoveStyle.SwordCleave:
            case MoveStyle.ClubSmash:
                // Raised over the head, the blade hanging behind it; down through to the front of the
                // hips, the blade carried on past them and back (not into the ground).
                windUp = new Pose(new Vector3(0.05f, 0.42f, -0.05f), 150f, new Vector3(0.05f, -0.2f, -1f));
                follow = new Pose(new Vector3(-0.15f, -0.45f, 0.25f), 30f, new Vector3(-0.2f, -0.4f, -0.9f));
                return true;
            case MoveStyle.ClubRising:
                // Low behind the right hip; up through to above the left shoulder.
                windUp = new Pose(new Vector3(0.2f, -0.45f, -0.15f), -10f, new Vector3(0.3f, -0.6f, -0.7f));
                follow = new Pose(new Vector3(-0.15f, 0.35f, 0.15f), 60f, new Vector3(-0.3f, 0.7f, -0.6f));
                return true;
            case MoveStyle.SwordBackhand:
            case MoveStyle.ClubBackhand:
                // Wound across in front of the left shoulder, the elbow across; out past the right side.
                windUp = new Pose(new Vector3(-0.25f, 0.15f, 0.2f), -60f, new Vector3(-0.35f, 0.4f, -0.85f));
                follow = new Pose(new Vector3(0.45f, 0f, 0.05f), 60f, new Vector3(0.7f, 0.1f, -0.7f));
                return true;
            default:
                windUp = follow = default;
                return false;
        }
    }

    internal static void Seed(MoveDefinition move, WeaponDefinition weapon, string body, AimPart part)
    {
        if (!Seeds((MoveStyle)Enum.Parse(typeof(MoveStyle), move.name.Replace("MD_", "")), out var windUp, out var follow))
        {
            return;
        }

        weapon.moves = new[] { move };
        var a = AimMatrix.Build(body, Vector3.zero, 0f, 0);
        var target = AimMatrix.Build("Medium", Vector3.forward * 5f, 180f, 1);
        try
        {
            float distance = a.Style.engageDistance * a.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            target.Place(Vector3.forward * distance, 180f);
            a.Equip(weapon, null);
            for (int i = 0; i < 20; i++)
            {
                a.PreviewPose(1f / 60f);
                target.PreviewPose(1f / 60f);
            }

            var state = MovePlanner.Prepare(a, target, move, part);
            if (state == null)
            {
                return;
            }

            seedWindUp = windUp;
            seedFollow = follow;
            haveSeeds = true;
            foreach (var (pose, t, isWindUp) in new[] { (windUp, move.WindUpEnd, true), (follow, move.SwingFollowEnd, false) })
            {
                a.PoseDryAt(state, t);
                var anim = a.Anim;
                Vector3 shoulder = anim.Part(PartId.UpperArmR).position;
                Vector3 want = (state.Frame * pose.blade).normalized;
                float best = float.MaxValue;
                Vector3 bestWrist = Vector3.zero, bestHand = pose.hand;
                float bestElbow = pose.elbow, bestOff = 0f;
                // (The hand within 10 cm of the design and the elbow within 40°: the wrist that points the
                // blade the designed way most easily.)
                for (float hx = -0.1f; hx <= 0.101f; hx += 0.05f)
                for (float hy = -0.1f; hy <= 0.101f; hy += 0.05f)
                for (float hz = -0.1f; hz <= 0.101f; hz += 0.05f)
                for (float de = -40f; de <= 40.1f; de += 20f)
                {
                    Vector3 hand = pose.hand + new Vector3(hx, hy, hz);
                    for (float tw = -80f; tw <= 80f; tw += 10f)
                    for (float fl = -60f; fl <= 60f; fl += 15f)
                    for (float dv = -35f; dv <= 20f; dv += 11f)
                    {
                        var key = new Vector3(tw, fl, dv);
                        var goal = new ArmGoal { point = shoulder + state.Frame * (hand * state.Scale), wristGiven = true, wristLocal = ArmRig.Wrist(anim, true, key), elbowSwivel = pose.elbow + de };
                        ArmRig.Solve(anim, true, goal);
                        float off = Vector3.Angle(anim.SlotWeaponR.rotation * Vector3.forward, want);
                        float cost = off + 20f * (Mathf.Pow(tw / 80f, 2f) + Mathf.Pow(fl / 60f, 2f) + Mathf.Pow(dv >= 0f ? dv / 20f : -dv / 35f, 2f))
                                     + 50f * (hand - pose.hand).magnitude + 0.1f * Mathf.Abs(de);
                        if (cost < best)
                        {
                            best = cost;
                            bestWrist = key;
                            bestHand = hand;
                            bestElbow = pose.elbow + de;
                            bestOff = off;
                        }
                    }
                }

                if (isWindUp)
                {
                    move.handWindUp = bestHand;
                    move.elbowWindUp = bestElbow;
                    move.wristWindUp = bestWrist;
                }
                else
                {
                    move.handFollow = bestHand;
                    move.elbowFollow = bestElbow;
                    move.wristFollow = bestWrist;
                }

                Console.WriteLine($"  seed {(isWindUp ? "wind-up" : "follow")}: hand {bestHand}, elbow {bestElbow:0}, wrist {bestWrist}, the blade {bestOff:0}° off the design");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(a.gameObject);
            UnityEngine.Object.DestroyImmediate(target.gameObject);
        }
    }

    private static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture) + "f";
}
