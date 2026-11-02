using System;
using PhysicsBattler.Combat;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>How the weapon moves in the base loop.</summary>
    public enum WeaponLoop
    {
        None,
        /// <summary>Up and down with the bounce (a boxer's cocked right hand).</summary>
        Pump,
        /// <summary>A figure-eight from the wrist (a stick fighter).</summary>
        FigureEight,
        /// <summary>The point circling slowly (a fencer's blade).</summary>
        TipCircle,
    }

    /// <summary>The ready-made fighting styles.</summary>
    public enum RhythmStyle
    {
        /// <summary>The large unit: a pressure boxer, the club for a right hand.</summary>
        Boxing,
        /// <summary>The medium unit: early medieval round-shield and sword.</summary>
        ShieldAndSword,
        /// <summary>The small unit: Filipino stick fighting (Eskrima/Kali), the club as a single stick.</summary>
        Eskrima,
    }

    /// <summary>The ready-made rhythms, made once and shared, for units whose style names none.</summary>
    public static class RhythmLibrary
    {
        private static readonly RhythmDefinition[] made = new RhythmDefinition[3];

        public static RhythmDefinition For(string body) => For(RhythmDefinition.StyleFor(body));

        public static RhythmDefinition For(RhythmStyle style)
        {
            int i = (int)style;
            if (made[i] == null)
            {
                made[i] = RhythmDefinition.Create(style);
            }

            return made[i];
        }
    }

    /// <summary>
    /// A Fight Rhythm: how a fighting style moves between blows. A base loop that always plays (a
    /// bounce, a sway, the weapon's own motion, the guard), postures mixed in over it (feints,
    /// footwork, guard changes, flourishes, defensive moves), how often and in what patterns
    /// (phrases, lead-ins before blows), and the mood that changes the mix as the fight goes on.
    /// Lengths are for the medium body (leg lengths, or metres for the hands), scaled by size.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Fight Rhythm", fileName = "RD_Rhythm")]
    public class RhythmDefinition : ScriptableObject
    {
        [Header("Base: a captured loop")]
        [Tooltip("A loop from motion capture (Tools/Mocap): when it has keys, it plays instead of the bounce, sway and weapon loop below.")]
        public RhythmClip clip = new RhythmClip();

        [Header("Base: the bounce")]
        [Tooltip("Bounces a second (0: none).")]
        public float bounceRate;
        [Tooltip("How far the hips drop each bounce, in leg lengths.")]
        public float bounceHeight;

        [Header("Base: sway")]
        [Tooltip("Hips side to side, in leg lengths.")]
        public float hipSway;
        public float swayRate = 0.5f;
        [Tooltip("Head side to side (yaw and roll), degrees.")]
        public float headSway;

        [Header("Base: weapon and guard")]
        public WeaponLoop weaponLoop = WeaponLoop.None;
        [Tooltip("The loop's size, metres.")]
        public float loopSize = 0.03f;
        [Tooltip("Loops a second.")]
        public float loopRate = 0.8f;
        [Tooltip("How much the weapon turns in the loop, degrees.")]
        public float loopTurn;
        [Tooltip("The weapon hand's guard moved, metres (x right, y up, z forward), and the weapon turned (degrees).")]
        public Vector3 mainGuard;
        public Vector3 mainGuardTurn;
        [Tooltip("The shield's guard moved, metres, and turned (degrees); an empty off hand is placed here against the chest instead (x right, y up, z forward).")]
        public Vector3 offGuard;
        public Vector3 offGuardTurn;

        [Header("Base: moving round the opponent")]
        [Tooltip("Sideways speed circling the opponent, m/s (0: doesn't circle).")]
        public float circling;
        [Tooltip("Seconds between changes of direction, on average.")]
        public float circleFlip = 3f;

        [Header("Postures and phrases")]
        public PostureDefinition[] postures = Array.Empty<PostureDefinition>();
        [Tooltip("Seconds between postures when fresh (a random time in this range; longer when tired).")]
        public Vector2 gap = new Vector2(1f, 2f);
        [Tooltip("Chance a blow is led into with a lead-in posture (a feint first).")]
        [Range(0f, 1f)] public float leadInChance = 0.5f;

        [Header("Mood")]
        [Tooltip("Aggression at the start (0 to 1): more feints and attacking postures, fewer defensive ones.")]
        [Range(0f, 1f)] public float aggression = 0.5f;
        [Tooltip("Energy each posture, blow and share of health lost costs; energy back per second. A busy fighter is about half spent after a minute and a half.")]
        public float postureCost = 0.006f;
        [Tooltip("(With a tempo, its blows cost what the temperament says; this is on top.)")]
        public float blowCost;
        public float hitCost = 0.35f;
        [Tooltip("(With a tempo, energy comes back as the temperament says, not at this rate.)")]
        public float energyRegen = 0.0015f;
        [Range(0f, 1f)] public float minEnergy = 0.05f;
        [Tooltip("How far both hands sag when spent (energy 0), metres. Below 35% energy the head is open.")]
        public float tiredGuardDrop = 0.1f;

        [Header("Tempo (milestone 8)")]
        [Tooltip("When it gathers, presses and shells up, what its energy costs and earns, and the stance it holds in each.")]
        public Temperament temperament = new Temperament();

        public string Label => name.StartsWith("RD_") ? name.Substring(3) : name;

        public PostureDefinition Find(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return null;
            }

            foreach (var p in postures)
            {
                if (p != null && p.Label == label)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>The ready-made style for a body (large: boxing, medium: shield and sword, small: Eskrima).</summary>
        public static RhythmStyle StyleFor(string body)
        {
            switch ((body ?? "").ToLowerInvariant())
            {
                case "large": return RhythmStyle.Boxing;
                case "small": return RhythmStyle.Eskrima;
                default: return RhythmStyle.ShieldAndSword;
            }
        }

        public static RhythmDefinition Create(RhythmStyle style)
        {
            var rhythm = CreateInstance<RhythmDefinition>();
            rhythm.name = "RD_" + style;
            rhythm.ApplyDefaults(style);
            return rhythm;
        }

        /// <summary>Fills in a ready-made style, its postures made fresh (the setup saves them as their own assets).</summary>
        public void ApplyDefaults(RhythmStyle style)
        {
            switch (style)
            {
                case RhythmStyle.Boxing:
                    Boxing();
                    break;
                case RhythmStyle.ShieldAndSword:
                    ShieldAndSword();
                    break;
                default:
                    Eskrima();
                    break;
            }
        }

        // ---- The ready-made styles ---------------------------------------------------------------

        private static PostureDefinition Posture(string name, PostureKind kind, float duration, bool leadIn, string followedBy, float weight = 1f)
        {
            var p = CreateInstance<PostureDefinition>();
            p.name = "PD_" + name;
            p.kind = kind;
            p.duration = duration;
            p.leadIn = leadIn;
            p.followedBy = followedBy;
            p.weight = weight;
            p.use = PostureDefinition.DefaultUse(kind);
            return p;
        }

        private static OpeningWindow Open(float from, float to, AimParts parts) => new OpeningWindow { from = from, to = to, parts = parts };

        /// <summary>
        /// Gathering (milestone 8): a long breath out. The shoulders drop and the hips sink, the guard
        /// comes in and is set again, the chin goes down; then back up. Plays only while gathering.
        /// </summary>
        private static PostureDefinition Settle(string name, Vector3 hands, float chin, string followedBy)
        {
            var settle = Posture(name, PostureKind.Guard, 1.2f, false, followedBy, 1.2f);
            settle.use = TempoUse.Gather;
            settle.keys = new[]
            {
                new PostureKey { at = 0.35f, crouch = 0.1f, lean = -5f, roll = 5f, head = new Vector2(chin, 0f), main = hands, off = hands + new Vector3(0f, -0.06f, 0f) },
                new PostureKey { at = 0.7f, crouch = 0.03f, lean = 2f, head = new Vector2(0.5f * chin, 0f), main = 0.4f * hands, off = 0.4f * hands },
            };
            settle.openings = new[] { Open(0.25f, 0.55f, AimParts.Head) };   // (the guard comes down for a breath)
            return settle;
        }

        /// <summary>
        /// Pressing (milestone 8): a stalking step in, weight onto the front foot, the guard pushed
        /// forward. Plays only in a press, between its blows.
        /// </summary>
        private static PostureDefinition StalkIn(string name, float distance, string followedBy)
        {
            var stalk = Posture(name, PostureKind.Footwork, 0.55f, true, followedBy, 1.4f);
            stalk.use = TempoUse.Press;
            stalk.keys = new[]
            {
                new PostureKey { at = 0.45f, hips = new Vector3(0f, 0f, 0.08f), crouch = 0.06f, lean = 11f, head = new Vector2(5f, 0f), off = new Vector3(0f, 0.06f, 0.2f), main = new Vector3(0f, 0.06f, 0.1f), travel = new Vector3(0f, 0f, 0.7f * distance) },
                new PostureKey { at = 1f, travel = new Vector3(0f, 0f, distance) },
            };
            stalk.openings = new[] { Open(0.2f, 0.5f, AimParts.Belly) };
            return stalk;
        }

        /// <summary>
        /// The large unit: a pressure boxer in Tyson's mould. The base loop is a captured boxing
        /// idle (Boxing.fbx): bladed, rocking forward and down onto the lead leg and back up, the
        /// body turning with it, the lead hand up in front of the shoulder, the club at the shoulder
        /// bobbing with the rear hand. Feints just before it really swings. (Without the clip: a low
        /// bounce, a sway and the club pumping.)
        /// </summary>
        // The boxer's guards, where the capture holds its hands (measured against the shoulder in
        // arm lengths): the lead hand up in front of its shoulder, the rear hand (the club's) in front
        // of the chin, the club laid back over the right shoulder like a cocked fist.
        public static readonly Vector3 BoxingOffGuard = new Vector3(-0.14f, 0.19f, 0.36f);
        public static readonly Vector3 BoxingMainGuard = new Vector3(-0.08f, 0.48f, -0.04f);
        public static readonly Vector3 BoxingMainGuardTurn = new Vector3(-67f, 0f, -9f);

        // The medium and small units' guards, where their captured idles hold the hands (against
        // the shoulder, in arm lengths): the shield out toward the opponent and the sword hand up in
        // front of the chest; the stick fighter's hands low, the alive hand out in front.
        public static readonly Vector3 MediumMainGuard = new Vector3(0.084f, 0.257f, -0.102f);
        public static readonly Vector3 MediumOffGuard = new Vector3(0.011f, 0.039f, 0.425f);
        public static readonly Vector3 SmallMainGuard = new Vector3(0.166f, -0.053f, -0.212f);
        public static readonly Vector3 SmallOffGuard = new Vector3(-0.595f, -0.4f, 0.29f);
        public static readonly Vector3 SmallMainGuardTurn = new Vector3(21f, 15f, 0f);   // (the stick held low, pointing forward and up)
        public static readonly Vector3 OldSmallMainGuardTurn = new Vector3(-15f, 0f, 0f);

        // Before the captures (setup 16).
        public static readonly Vector3 OldMediumMainGuard = new Vector3(0.04f, -0.1f, -0.12f);
        public static readonly Vector3 OldMediumOffGuard = new Vector3(0.02f, 0.02f, 0.14f);
        public static readonly Vector3 OldSmallMainGuard = new Vector3(-0.02f, 0.05f, 0.04f);
        public static readonly Vector3 OldSmallOffGuard = new Vector3(-0.06f, 0.02f, 0.3f);

        // Before the capture (setup 15).
        public static readonly Vector3 OldBoxingOffGuard = new Vector3(-0.1f, 0.3f, 0.24f);
        public static readonly Vector3 OldBoxingMainGuard = new Vector3(0.04f, 0.1f, -0.06f);
        public static readonly Vector3 OldBoxingMainGuardTurn = new Vector3(-25f, 0f, 0f);

        private void Boxing()
        {
            clip = RhythmClips.Boxing();
            bounceRate = 1.8f;
            bounceHeight = 0.03f;
            hipSway = 0.03f;
            swayRate = 0.45f;
            headSway = 6f;
            weaponLoop = WeaponLoop.None;   // (the club rides the captured rear hand)
            loopSize = 0.035f;
            loopRate = 0.9f;
            loopTurn = 4f;
            mainGuard = BoxingMainGuard;
            mainGuardTurn = BoxingMainGuardTurn;
            offGuard = BoxingOffGuard;
            circling = 0f;
            gap = new Vector2(1f, 2f);
            leadInChance = 0.85f;
            aggression = 0.65f;
            tiredGuardDrop = 0.14f;

            // A pressure fighter: short breathers, long bursts walking forward, hard to rattle.
            temperament = new Temperament
            {
                probeAt = 0.45f, pressAt = 0.7f, pressAggression = 0.4f, burstMax = 4, composure = 0.8f, holdChance = 0.35f, fullCharge = 0.85f, shellGuard = ShellGuard.High,
                probeInterval = new Vector2(0.8f, 1.6f), pressGap = new Vector2(0.15f, 0.35f),
                // Gathering: sits down into the stance, chin tucked, the guard pulled in, breathing deep.
                gather = new MoodStance { crouch = 0.05f, lean = -2f, forward = -0.05f, chin = 8f, main = new Vector3(0.02f, -0.03f, 0.03f), off = new Vector3(0.02f, 0f, -0.08f), loopSpeed = 0.75f, loopSize = 1.25f },
                // Pressing: leaning in on the front foot, hands up and out, bouncing quicker.
                press = new MoodStance { crouch = 0.03f, lean = 9f, forward = 0.07f, chin = 5f, main = new Vector3(0f, 0.05f, 0.08f), off = new Vector3(0f, 0.06f, 0.14f), loopSpeed = 1.35f, loopSize = 1.2f },
                // The high shell: forearms up beside the head, elbows tight, chin down, crouched in behind them.
                shell = new MoodStance { crouch = 0.06f, lean = 3f, forward = -0.02f, chin = 14f, main = new Vector3(-0.06f, 0.12f, 0.04f), off = new Vector3(0.12f, 0.22f, -0.1f), loopSpeed = 1.4f, loopSize = 0.5f },
            };

            // The club cocked back as if to swing, and stopped: the real swing's own wind-up, a third of the way.
            var clubFeint = Posture("ClubFeint", PostureKind.Feint, 0.5f, true, "JabFeint", 1.2f);
            clubFeint.feintMove = "ClubSwing";
            clubFeint.feintDepth = 0.4f;
            clubFeint.keys = new[]
            {
                new PostureKey { at = 0.4f, crouch = 0.06f, lean = 8f, turn = 8f, off = new Vector3(0f, -0.1f, 0.04f) },
            };
            clubFeint.openings = new[] { Open(0.3f, 0.6f, AimParts.Head) };

            // The off hand flicks half out at the face and back, with a small step in.
            var jabFeint = Posture("JabFeint", PostureKind.Feint, 0.38f, true, "BobAndWeave");
            jabFeint.keys = new[]
            {
                new PostureKey { at = 0.35f, hips = new Vector3(0f, 0f, 0.06f), lean = 7f, turn = -14f, off = new Vector3(0.08f, -0.04f, 0.42f) },
                new PostureKey { at = 0.7f, hips = new Vector3(0f, 0f, 0.03f), lean = 3f, turn = -5f, off = new Vector3(0.03f, -0.01f, 0.1f) },
            };
            jabFeint.steps = new[]
            {
                new PostureStep { at = 0f, foot = PostureFoot.Lead, to = PostureStepTo.OffsetFromHome, offset = new Vector3(0f, 0f, 0.08f), length = 0.35f },
                new PostureStep { at = 0.6f, foot = PostureFoot.Lead, to = PostureStepTo.Home, length = 0.35f },
            };
            jabFeint.openings = new[] { Open(0.25f, 0.7f, AimParts.Head | AimParts.ElbowL) };

            // Dips into a U under an imagined hook and rises on the other side.
            var bob = Posture("BobAndWeave", PostureKind.Defence, 0.75f, false, "ClubFeint");
            bob.keys = new[]
            {
                new PostureKey { at = 0.25f, hips = new Vector3(0.1f, 0f, 0f), crouch = 0.1f, lean = 10f, roll = 14f, turn = 12f, head = new Vector2(10f, 0f) },
                new PostureKey { at = 0.5f, hips = new Vector3(0f, 0f, 0.03f), crouch = 0.21f, lean = 21f, head = new Vector2(12f, 0f) },
                new PostureKey { at = 0.75f, hips = new Vector3(-0.1f, 0f, 0f), crouch = 0.1f, lean = 10f, roll = -14f, turn = -12f, head = new Vector2(10f, 0f) },
            };
            bob.openings = new[] { Open(0.4f, 0.62f, AimParts.Head | AimParts.Neck) };

            // The rear foot steps through to the front and back: a stance switch (Tyson's shift).
            var shift = Posture("Shift", PostureKind.Footwork, 0.7f, false, "JabFeint", 0.8f);
            shift.keys = new[]
            {
                new PostureKey { at = 0.45f, hips = new Vector3(0.04f, 0f, 0.08f), lean = 7f, turn = 30f, off = new Vector3(0f, 0.04f, 0.12f) },
            };
            shift.steps = new[]
            {
                new PostureStep { at = 0.02f, foot = PostureFoot.Rear, to = PostureStepTo.OffsetFromHome, offset = new Vector3(0f, 0f, 0.36f), length = 0.35f },
                new PostureStep { at = 0.6f, foot = PostureFoot.Rear, to = PostureStepTo.Home, length = 0.35f },
            };
            shift.openings = new[] { Open(0.35f, 0.6f, AimParts.Belly | AimParts.Knees) };
            shift.use = TempoUse.Gather | TempoUse.Probe;   // (not pressing: leaning in on a switched stance tips it over)

            postures = new[] { clubFeint, jabFeint, bob, shift, Settle("BoxerSettle", new Vector3(0.02f, -0.1f, -0.06f), 10f, "Shift"), StalkIn("BoxerStalk", 0.24f, "JabFeint") };
        }

        /// <summary>
        /// The medium unit: early medieval round-shield and sword. The shield out at arm's length, the
        /// sword drawn back by the hip, point forward and circling a little; low, circling to the
        /// opponent's shield side. Feints with the sword, presses with the shield.
        /// </summary>
        private void ShieldAndSword()
        {
            clip = RhythmClips.ShieldAndSword();
            bounceRate = 0f;
            bounceHeight = 0f;
            hipSway = 0.02f;
            swayRate = 0.32f;
            headSway = 3f;
            weaponLoop = WeaponLoop.TipCircle;
            loopSize = 0.02f;
            loopRate = 0.45f;
            loopTurn = 6f;
            mainGuard = MediumMainGuard;
            mainGuardTurn = new Vector3(22f, 18f, 0f);
            offGuard = MediumOffGuard;
            offGuardTurn = new Vector3(0f, -12f, 0f);
            circling = 0.16f;
            circleFlip = 4f;
            gap = new Vector2(1.2f, 2.4f);
            leadInChance = 0.5f;
            aggression = 0.5f;
            tiredGuardDrop = 0.1f;

            // Patient: long breathers behind the shield, short sharp bursts, shells up early.
            temperament = new Temperament
            {
                probeAt = 0.5f, pressAt = 0.85f, pressAggression = 0.45f, burstMax = 3, composure = 0.5f, holdChance = 0.5f, fullCharge = 0.95f,
                probeInterval = new Vector2(1.0f, 2.0f), pressGap = new Vector2(0.2f, 0.45f),
                // Gathering: low behind the shield pulled in, the sword drawn back, weight back.
                gather = new MoodStance { crouch = 0.06f, lean = -3f, forward = -0.06f, chin = 5f, main = new Vector3(0f, -0.06f, -0.08f), off = new Vector3(0f, -0.04f, -0.12f), loopSpeed = 0.75f, loopSize = 1.2f },
                // Pressing: the shield driven forward, weight on the front foot, the sword up ready.
                press = new MoodStance { crouch = 0.02f, lean = 8f, forward = 0.07f, chin = 3f, main = new Vector3(0f, 0.08f, 0.04f), off = new Vector3(0f, 0.04f, 0.16f), loopSpeed = 1.3f, loopSize = 1.2f },
                // Shelled: crouched right down behind the shield raised to the eyes, the sword back.
                shell = new MoodStance { crouch = 0.11f, lean = 8f, forward = 0f, chin = 10f, main = new Vector3(0f, 0.04f, -0.1f), off = new Vector3(0.04f, 0.22f, 0.04f), loopSpeed = 1.3f, loopSize = 0.5f },
            };

            // The sword rises as if for a high cut over the shield, and the line breaks off.
            var high = Posture("HighFeint", PostureKind.Feint, 0.55f, true, "ShieldPress", 1.2f);
            high.feintMove = "SwordCut";
            high.feintDepth = 0.45f;
            high.keys = new[]
            {
                new PostureKey { at = 0.4f, crouch = -0.02f, off = new Vector3(0f, 0.04f, 0.02f) },
            };
            high.openings = new[] { Open(0.35f, 0.65f, AimParts.ElbowR) };

            // The shield punches forward with a half step, then pulls back.
            var press = Posture("ShieldPress", PostureKind.Feint, 0.6f, true, "HighFeint");
            press.keys = new[]
            {
                new PostureKey { at = 0.45f, hips = new Vector3(0f, 0f, 0.1f), lean = 12f, turn = -16f, off = new Vector3(0f, 0.06f, 0.42f) },
                new PostureKey { at = 0.75f, hips = new Vector3(0f, 0f, 0.05f), lean = 5f, turn = -6f, off = new Vector3(0f, 0.02f, 0.14f) },
            };
            press.steps = new[]
            {
                new PostureStep { at = 0.1f, foot = PostureFoot.Lead, to = PostureStepTo.OffsetFromHome, offset = new Vector3(0f, 0f, 0.18f), length = 0.3f },
                new PostureStep { at = 0.65f, foot = PostureFoot.Lead, to = PostureStepTo.Home, length = 0.3f },
            };
            press.openings = new[] { Open(0.4f, 0.75f, AimParts.Knees | AimParts.ElbowR) };

            // The sword over the head (hanging guard), the shield down by the knee; then the sword on
            // the shoulder and the shield up to it; then back.
            var change = Posture("GuardChange", PostureKind.Guard, 0.9f, false, "Peek");
            change.keys = new[]
            {
                new PostureKey { at = 0.35f, turn = 10f, lean = -3f, main = new Vector3(0.02f, 0.46f, 0.04f), mainTurn = new Vector3(-60f, 0f, 0f), off = new Vector3(0f, -0.3f, 0.02f), offTurn = new Vector3(20f, 0f, 0f) },
                new PostureKey { at = 0.7f, turn = -8f, lean = 4f, main = new Vector3(0.08f, 0.3f, -0.1f), mainTurn = new Vector3(-45f, 8f, 0f), off = new Vector3(0f, 0.16f, -0.06f) },
            };
            change.openings = new[] { Open(0.3f, 0.5f, AimParts.Chest | AimParts.Belly), Open(0.6f, 0.8f, AimParts.Knees) };

            // The shield dips to look over it, and comes back up: the classic moment to strike.
            var peek = Posture("Peek", PostureKind.Defence, 0.45f, false, "HighFeint", 0.9f);
            peek.keys = new[]
            {
                new PostureKey { at = 0.45f, crouch = -0.04f, lean = -4f, head = new Vector2(-12f, 0f), off = new Vector3(0f, -0.32f, -0.08f), offTurn = new Vector3(20f, 0f, 0f) },
            };
            peek.openings = new[] { Open(0.25f, 0.7f, AimParts.Head | AimParts.Neck) };

            postures = new[] { high, press, change, peek, Settle("ShieldSettle", new Vector3(0f, -0.08f, -0.14f), 6f, "GuardChange"), StalkIn("ShieldAdvance", 0.2f, "ShieldPress") };
        }

        /// <summary>
        /// The small unit: Filipino stick fighting (Eskrima/Kali), the club as a single stick. Light
        /// and upright, a quick bounce, the stick tracing a figure-eight from the wrist, the alive hand
        /// up in front; restless, stepping round to new angles.
        /// </summary>
        private void Eskrima()
        {
            clip = RhythmClips.Eskrima();
            bounceRate = 2.4f;
            bounceHeight = 0.02f;
            hipSway = 0.03f;
            swayRate = 0.8f;
            headSway = 4f;
            weaponLoop = WeaponLoop.FigureEight;
            loopSize = 0.07f;
            loopRate = 0.9f;
            loopTurn = 22f;
            mainGuard = SmallMainGuard;
            mainGuardTurn = SmallMainGuardTurn;
            offGuard = SmallOffGuard;
            circling = 0.12f;
            circleFlip = 2.5f;
            gap = new Vector2(0.6f, 1.4f);
            leadInChance = 0.55f;
            aggression = 0.55f;
            tiredGuardDrop = 0.08f;

            // Timing: short breathers circling out of reach, quick flurries in and out, would rather move than shell.
            temperament = new Temperament
            {
                probeAt = 0.45f, pressAt = 0.7f, pressAggression = 0.45f, burstMax = 6, composure = 0.6f, pressCost = 0.1f, holdChance = 0.3f, fullCharge = 0.85f, shellGuard = ShellGuard.Across,
                probeInterval = new Vector2(0.6f, 1.4f), pressGap = new Vector2(0.1f, 0.3f),
                // Gathering: light and back on the rear foot, the stick low, the alive hand in.
                gather = new MoodStance { crouch = 0.04f, lean = -3f, forward = -0.06f, chin = 4f, main = new Vector3(0f, -0.06f, -0.06f), off = new Vector3(0.04f, -0.06f, -0.1f), loopSpeed = 0.8f, loopSize = 1.15f },
                // Pressing: low and forward, the stick up and out, the alive hand reaching.
                press = new MoodStance { crouch = 0.06f, lean = 9f, forward = 0.07f, chin = 3f, main = new Vector3(0f, 0.1f, 0.1f), off = new Vector3(0f, 0.08f, 0.14f), loopSpeed = 1.4f, loopSize = 1.3f },
                // Shelled: very low, the stick across the body, ready to slip out.
                shell = new MoodStance { crouch = 0.13f, lean = 5f, forward = -0.06f, chin = 6f, main = new Vector3(-0.1f, 0.04f, 0.1f), off = new Vector3(0.06f, 0.12f, 0.08f), loopSpeed = 1.4f, loopSize = 0.6f },
            };

            // Fast fan flicks of the stick, side to side from the wrist, driven by the hips.
            var abanico = Posture("AbanicoFlurry", PostureKind.Flourish, 0.95f, true, "TriangleStep", 1.2f);
            abanico.keys = new[]
            {
                new PostureKey { at = 0.2f, hips = new Vector3(0.04f, 0f, 0f), turn = 12f, main = new Vector3(0.2f, 0.15f, 0.12f), mainTurn = new Vector3(10f, 0f, -60f) },
                new PostureKey { at = 0.4f, hips = new Vector3(-0.04f, 0f, 0f), turn = -12f, main = new Vector3(-0.1f, 0.15f, 0.14f), mainTurn = new Vector3(10f, 0f, 55f) },
                new PostureKey { at = 0.6f, hips = new Vector3(0.04f, 0f, 0f), turn = 12f, main = new Vector3(0.2f, 0.16f, 0.12f), mainTurn = new Vector3(10f, 0f, -60f) },
                new PostureKey { at = 0.8f, hips = new Vector3(-0.03f, 0f, 0f), turn = -10f, main = new Vector3(-0.06f, 0.12f, 0.12f), mainTurn = new Vector3(10f, 0f, 50f) },
            };
            abanico.openings = new[] { Open(0.3f, 0.8f, AimParts.Knees | AimParts.Belly) };

            // Off the line to a new angle: forward and out along a triangle, the feet stepping after.
            var triangle = Posture("TriangleStep", PostureKind.Footwork, 0.55f, true, "AbanicoFlurry");
            triangle.keys = new[]
            {
                new PostureKey { at = 0.45f, hips = new Vector3(-0.06f, 0f, 0.04f), crouch = 0.07f, lean = 6f, turn = 22f, off = new Vector3(0f, 0.06f, 0.1f), travel = new Vector3(-0.22f, 0f, 0.17f) },
                new PostureKey { at = 1f, travel = new Vector3(-0.28f, 0f, 0.22f) },
            };
            triangle.openings = new[] { Open(0.35f, 0.6f, AimParts.Knees) };

            // One big circle of the stick overhead and round: a flourish, and a wind-up in disguise.
            var redonda = Posture("RedondaTwirl", PostureKind.Flourish, 1f, true, "TriangleStep", 0.9f);
            redonda.keys = new[]
            {
                new PostureKey { at = 0.25f, hips = new Vector3(0.04f, 0f, 0f), crouch = 0.04f, turn = 8f, main = new Vector3(0.18f, 0.24f, 0.04f), mainTurn = new Vector3(-25f, 0f, -18f) },
                new PostureKey { at = 0.5f, hips = new Vector3(0.06f, 0f, -0.03f), turn = 18f, lean = -6f, main = new Vector3(0.2f, 0.32f, -0.04f), mainTurn = new Vector3(-80f, -35f, 0f) },
                new PostureKey { at = 0.75f, hips = new Vector3(0.02f, 0f, 0.04f), crouch = 0.05f, lean = 6f, turn = 4f, main = new Vector3(0.14f, 0.12f, 0.14f), mainTurn = new Vector3(50f, 20f, 0f) },
            };
            redonda.openings = new[] { Open(0.4f, 0.75f, AimParts.Chest | AimParts.Belly | AimParts.ElbowR) };

            // A step back, the stick pulled back and the alive hand out: inviting a blow (a trap).
            var bait = Posture("BaitRetreat", PostureKind.Defence, 0.75f, false, "AbanicoFlurry", 0.8f);
            bait.keys = new[]
            {
                new PostureKey { at = 0.5f, lean = -10f, hips = new Vector3(0f, 0f, -0.05f), main = new Vector3(0.04f, -0.1f, -0.26f), mainTurn = new Vector3(15f, 0f, 0f), off = new Vector3(0.04f, 0.22f, 0.3f), travel = new Vector3(0f, 0f, -0.32f) },
                new PostureKey { at = 1f, travel = new Vector3(0f, 0f, -0.4f) },
            };
            bait.openings = new[] { Open(0.15f, 0.95f, AimParts.Head | AimParts.Chest | AimParts.Belly) };

            postures = new[] { abanico, triangle, redonda, bait, Settle("StickSettle", new Vector3(0.04f, -0.1f, -0.12f), 6f, "TriangleStep"), StalkIn("StickStalk", 0.3f, "AbanicoFlurry") };
        }
    }
}
