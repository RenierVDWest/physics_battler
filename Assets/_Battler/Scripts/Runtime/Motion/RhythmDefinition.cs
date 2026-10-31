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
        public float blowCost = 0.012f;
        public float hitCost = 0.35f;
        public float energyRegen = 0.0015f;
        [Range(0f, 1f)] public float minEnergy = 0.15f;
        [Tooltip("How far both hands sag when spent (energy 0), metres. Below 35% energy the head is open.")]
        public float tiredGuardDrop = 0.1f;

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
            return p;
        }

        private static OpeningWindow Open(float from, float to, AimParts parts) => new OpeningWindow { from = from, to = to, parts = parts };

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
            weaponLoop = WeaponLoop.Pump;
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

            // The club cocked back as if to swing, and stopped: the real swing's own wind-up, a third of the way.
            var clubFeint = Posture("ClubFeint", PostureKind.Feint, 0.5f, true, "JabFeint", 1.2f);
            clubFeint.feintMove = "ClubSwing";
            clubFeint.feintDepth = 0.4f;
            clubFeint.keys = new[]
            {
                new PostureKey { at = 0.4f, crouch = 0.04f, lean = 4f, off = new Vector3(0f, -0.08f, 0f) },
            };
            clubFeint.openings = new[] { Open(0.3f, 0.6f, AimParts.Head) };

            // The off hand flicks half out at the face and back, with a small step in.
            var jabFeint = Posture("JabFeint", PostureKind.Feint, 0.38f, true, "BobAndWeave");
            jabFeint.keys = new[]
            {
                new PostureKey { at = 0.35f, hips = new Vector3(0f, 0f, 0.04f), lean = 4f, turn = -10f, off = new Vector3(0.06f, -0.04f, 0.3f) },
                new PostureKey { at = 0.7f, hips = new Vector3(0f, 0f, 0.02f), lean = 2f, turn = -4f, off = new Vector3(0.02f, -0.01f, 0.08f) },
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
                new PostureKey { at = 0.25f, hips = new Vector3(0.06f, 0f, 0f), crouch = 0.07f, lean = 8f, roll = 10f, turn = 8f, head = new Vector2(8f, 0f) },
                new PostureKey { at = 0.5f, hips = new Vector3(0f, 0f, 0.03f), crouch = 0.17f, lean = 18f, head = new Vector2(10f, 0f) },
                new PostureKey { at = 0.75f, hips = new Vector3(-0.06f, 0f, 0f), crouch = 0.07f, lean = 8f, roll = -10f, turn = -8f, head = new Vector2(8f, 0f) },
            };
            bob.openings = new[] { Open(0.4f, 0.62f, AimParts.Head | AimParts.Neck) };

            // The rear foot steps through to the front and back: a stance switch (Tyson's shift).
            var shift = Posture("Shift", PostureKind.Footwork, 0.7f, false, "JabFeint", 0.8f);
            shift.keys = new[]
            {
                new PostureKey { at = 0.45f, hips = new Vector3(0.03f, 0f, 0.06f), lean = 5f, turn = 25f },
            };
            shift.steps = new[]
            {
                new PostureStep { at = 0.02f, foot = PostureFoot.Rear, to = PostureStepTo.OffsetFromHome, offset = new Vector3(0f, 0f, 0.36f), length = 0.35f },
                new PostureStep { at = 0.6f, foot = PostureFoot.Rear, to = PostureStepTo.Home, length = 0.35f },
            };
            shift.openings = new[] { Open(0.35f, 0.6f, AimParts.Belly | AimParts.Knees) };

            postures = new[] { clubFeint, jabFeint, bob, shift };
        }

        /// <summary>
        /// The medium unit: early medieval round-shield and sword. The shield out at arm's length, the
        /// sword drawn back by the hip, point forward and circling a little; low, circling to the
        /// opponent's shield side. Feints with the sword, presses with the shield.
        /// </summary>
        private void ShieldAndSword()
        {
            bounceRate = 0f;
            bounceHeight = 0f;
            hipSway = 0.02f;
            swayRate = 0.32f;
            headSway = 3f;
            weaponLoop = WeaponLoop.TipCircle;
            loopSize = 0.02f;
            loopRate = 0.45f;
            loopTurn = 6f;
            mainGuard = new Vector3(0.04f, -0.1f, -0.12f);
            mainGuardTurn = new Vector3(22f, 18f, 0f);
            offGuard = new Vector3(0.02f, 0.02f, 0.14f);
            offGuardTurn = new Vector3(0f, -12f, 0f);
            circling = 0.16f;
            circleFlip = 4f;
            gap = new Vector2(1.2f, 2.4f);
            leadInChance = 0.5f;
            aggression = 0.5f;
            tiredGuardDrop = 0.1f;

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
                new PostureKey { at = 0.45f, hips = new Vector3(0f, 0f, 0.06f), lean = 6f, turn = -10f, off = new Vector3(0f, 0.02f, 0.2f) },
                new PostureKey { at = 0.75f, hips = new Vector3(0f, 0f, 0.03f), lean = 3f, turn = -4f, off = new Vector3(0f, 0f, 0.08f) },
            };
            press.steps = new[]
            {
                new PostureStep { at = 0.1f, foot = PostureFoot.Lead, to = PostureStepTo.OffsetFromHome, offset = new Vector3(0f, 0f, 0.12f), length = 0.3f },
                new PostureStep { at = 0.65f, foot = PostureFoot.Lead, to = PostureStepTo.Home, length = 0.3f },
            };
            press.openings = new[] { Open(0.4f, 0.75f, AimParts.Knees | AimParts.ElbowR) };

            // The sword over the head (hanging guard), the shield down by the knee; then the sword on
            // the shoulder and the shield up to it; then back.
            var change = Posture("GuardChange", PostureKind.Guard, 0.9f, false, "Peek");
            change.keys = new[]
            {
                new PostureKey { at = 0.35f, main = new Vector3(0.02f, 0.38f, 0.04f), mainTurn = new Vector3(-55f, 0f, 0f), off = new Vector3(0f, -0.14f, 0f), offTurn = new Vector3(15f, 0f, 0f) },
                new PostureKey { at = 0.7f, main = new Vector3(0.06f, 0.24f, -0.06f), mainTurn = new Vector3(-40f, 8f, 0f), off = new Vector3(0f, 0.08f, -0.04f) },
            };
            change.openings = new[] { Open(0.3f, 0.5f, AimParts.Chest | AimParts.Belly), Open(0.6f, 0.8f, AimParts.Knees) };

            // The shield dips to look over it, and comes back up: the classic moment to strike.
            var peek = Posture("Peek", PostureKind.Defence, 0.45f, false, "HighFeint", 0.9f);
            peek.keys = new[]
            {
                new PostureKey { at = 0.45f, crouch = -0.02f, head = new Vector2(-6f, 0f), off = new Vector3(0f, -0.15f, -0.03f), offTurn = new Vector3(12f, 0f, 0f) },
            };
            peek.openings = new[] { Open(0.25f, 0.7f, AimParts.Head | AimParts.Neck) };

            postures = new[] { high, press, change, peek };
        }

        /// <summary>
        /// The small unit: Filipino stick fighting (Eskrima/Kali), the club as a single stick. Light
        /// and upright, a quick bounce, the stick tracing a figure-eight from the wrist, the alive hand
        /// up in front; restless, stepping round to new angles.
        /// </summary>
        private void Eskrima()
        {
            bounceRate = 2.4f;
            bounceHeight = 0.02f;
            hipSway = 0.03f;
            swayRate = 0.8f;
            headSway = 4f;
            weaponLoop = WeaponLoop.FigureEight;
            loopSize = 0.07f;
            loopRate = 0.9f;
            loopTurn = 22f;
            mainGuard = new Vector3(-0.02f, 0.05f, 0.04f);
            mainGuardTurn = new Vector3(-15f, 0f, 0f);
            offGuard = new Vector3(-0.06f, 0.02f, 0.3f);
            circling = 0.12f;
            circleFlip = 2.5f;
            gap = new Vector2(0.6f, 1.4f);
            leadInChance = 0.55f;
            aggression = 0.55f;
            tiredGuardDrop = 0.08f;

            // Fast fan flicks of the stick, side to side from the wrist, driven by the hips.
            var abanico = Posture("AbanicoFlurry", PostureKind.Flourish, 0.65f, true, "TriangleStep", 1.2f);
            abanico.keys = new[]
            {
                new PostureKey { at = 0.15f, turn = 6f, main = new Vector3(0.12f, 0.08f, 0.06f), mainTurn = new Vector3(10f, 0f, -55f) },
                new PostureKey { at = 0.35f, turn = -6f, main = new Vector3(-0.06f, 0.1f, 0.08f), mainTurn = new Vector3(10f, 0f, 50f) },
                new PostureKey { at = 0.55f, turn = 6f, main = new Vector3(0.12f, 0.1f, 0.06f), mainTurn = new Vector3(10f, 0f, -55f) },
                new PostureKey { at = 0.75f, turn = -5f, main = new Vector3(-0.04f, 0.08f, 0.08f), mainTurn = new Vector3(10f, 0f, 45f) },
            };
            abanico.openings = new[] { Open(0.3f, 0.8f, AimParts.Knees | AimParts.Belly) };

            // Off the line to a new angle: forward and out along a triangle, the feet stepping after.
            var triangle = Posture("TriangleStep", PostureKind.Footwork, 0.55f, true, "AbanicoFlurry");
            triangle.keys = new[]
            {
                new PostureKey { at = 0.45f, hips = new Vector3(-0.03f, 0f, 0.02f), crouch = 0.03f, turn = 12f, travel = new Vector3(-0.18f, 0f, 0.14f) },
                new PostureKey { at = 1f, travel = new Vector3(-0.24f, 0f, 0.18f) },
            };
            triangle.openings = new[] { Open(0.35f, 0.6f, AimParts.Knees) };

            // One big circle of the stick overhead and round: a flourish, and a wind-up in disguise.
            var redonda = Posture("RedondaTwirl", PostureKind.Flourish, 0.85f, true, "TriangleStep", 0.9f);
            redonda.keys = new[]
            {
                new PostureKey { at = 0.25f, turn = 6f, main = new Vector3(0.18f, 0.24f, 0.04f), mainTurn = new Vector3(-25f, 0f, -18f) },
                new PostureKey { at = 0.5f, turn = 14f, lean = -4f, main = new Vector3(0.2f, 0.32f, -0.04f), mainTurn = new Vector3(-95f, -45f, 0f) },
                new PostureKey { at = 0.75f, turn = 4f, main = new Vector3(0.14f, 0.12f, 0.14f), mainTurn = new Vector3(50f, 20f, 0f) },
            };
            redonda.openings = new[] { Open(0.4f, 0.75f, AimParts.Chest | AimParts.Belly | AimParts.ElbowR) };

            // A step back, the stick pulled back and the alive hand out: inviting a blow (a trap).
            var bait = Posture("BaitRetreat", PostureKind.Defence, 0.75f, false, "AbanicoFlurry", 0.8f);
            bait.keys = new[]
            {
                new PostureKey { at = 0.5f, lean = -5f, main = new Vector3(0.02f, -0.04f, -0.12f), mainTurn = new Vector3(15f, 0f, 0f), off = new Vector3(0.02f, 0.1f, 0.14f), travel = new Vector3(0f, 0f, -0.22f) },
                new PostureKey { at = 1f, travel = new Vector3(0f, 0f, -0.28f) },
            };
            bait.openings = new[] { Open(0.15f, 0.95f, AimParts.Head | AimParts.Chest | AimParts.Belly) };

            postures = new[] { abanico, triangle, redonda, bait };
        }
    }
}
