using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// One fighter's own levers (the plan "Fight Rhythm from Recordings", step 8, the dance, its first part): how he
    /// moves and how big his postures are, against everyone's (Combat Feel's own levers, <see cref="MoveScore.Levers"/>
    /// and <see cref="PostureMaker"/>'s sizes). With every one of them at rest he moves as everyone does.
    ///
    /// Until now there was one set of levers for every fighter on the field. Two fighters who are to dance need to
    /// be told apart at a glance, by how each carries himself and how he gestures: so each has his own, kept on his
    /// walk (<see cref="Locomotor.Own"/>).
    /// </summary>
    public sealed class FighterLevers
    {
        /// <summary>Each fighter has his own levers (Combat Feel "Each Fighter Has His Own Levers"). Off: every fighter moves by everyone's, as it was.</summary>
        public static bool On = true;

        /// <summary>What he is called on the panel.</summary>
        public string Name = "Plain";

        /// <summary>His movement, each times everyone's (1: as everyone): the hips' rise and fall, their sway, their turning, the chest's turning, the arms' swing, the feet's lift.</summary>
        public float Bounce = 1f, Sway = 1f, HipTurn = 1f, ChestTurn = 1f, ArmSwing = 1f, StepLift = 1f;

        /// <summary>His lean forward from the hips (degrees) and how much lower he carries his hips (cm at a Medium's size), added to everyone's.</summary>
        public float Lean, Crouch;

        /// <summary>His postures, each times everyone's (1: as everyone): how deep his feints go, how far his fake lunges carry him, how far he sinks in a drop, how big his blade's bluffs and his shield's press are, how far a change of guard carries his sword arm.</summary>
        public float Feint = 1f, Lunge = 1f, Drop = 1f, Bluff = 1f, Guard = 1f;

        /// <summary>
        /// How he paces the fight (the dance's third part; 0 to 1, 0.5: middling). Restless: how much he is on the move,
        /// going round the other in long spells with short stands between (1), or standing long and moving little (0).
        /// Aggressive: the distance he likes, inside reach (1) or just outside it (0); how soon he is back in after a
        /// blow; and at 0.6 and more he stands and covers for a blow he expects, where a less aggressive one steps out.
        /// </summary>
        public float Restless = 0.5f, Aggressive = 0.5f;

        /// <summary>
        /// How readily he leaps where a step would do (0: never; 1: every time): in, to close the last of the gap for
        /// his blow when he is still out of reach as it is due; and out, away from a blow he expects, where another
        /// would walk out.
        /// </summary>
        public float Leaps = 0.3f;

        /// <summary>
        /// How many of his pattern's blows are heavy ones (0: all light; 1: all heavy; <see cref="AttackClasses"/>), and
        /// how readily he dodges a high blow he sees coming (ducks aside under it) where another would trust his shield
        /// (0: never; 1: every time).
        /// </summary>
        public float Heavy = 0.5f, Dodges = 0.2f;

        /// <summary>
        /// In the dance: how many fake attacks he throws for each real one (0: none; 0.5: one for every two real
        /// attacks; 2: two for each), whatever his pattern has: a fake it has that he may not throw is left out; and
        /// how many of his pattern's blows are a hard bash to break the other's guard instead of a cut (0: none; with a
        /// shield only).
        /// </summary>
        public float Fakes = 0.5f, GuardBreaks = 0.2f;

        /// <summary>Every lever of his is at rest: he moves as everyone does.</summary>
        public bool AtRest => Bounce == 1f && Sway == 1f && HipTurn == 1f && ChestTurn == 1f && ArmSwing == 1f && StepLift == 1f && Lean == 0f && Crouch == 0f
            && Feint == 1f && Lunge == 1f && Drop == 1f && Bluff == 1f && Guard == 1f && Restless == 0.5f && Aggressive == 0.5f && Leaps == 0.3f && Heavy == 0.5f && Dodges == 0.2f && Fakes == 0.5f && GuardBreaks == 0.2f;

        /// <summary>Everyone's movement levers with his own laid on them.</summary>
        public MoveLevers Over(MoveLevers all)
        {
            all.Bounce *= Bounce;
            all.Sway *= Sway;
            all.HipTurn *= HipTurn;
            all.ChestTurn *= ChestTurn;
            all.ArmSwing *= ArmSwing;
            all.StepLift *= StepLift;
            all.Lean += Lean;
            all.Crouch += 0.01f * Crouch / Locomotor.ClipLeg;
            return all;
        }

        /// <summary>A fighter's own (null: he has none), or nothing with the switch off: what the sizes are multiplied by.</summary>
        public static float FeintOf(FighterLevers own) => On && own != null ? own.Feint : 1f;
        public static float LungeOf(FighterLevers own) => On && own != null ? own.Lunge : 1f;
        public static float DropOf(FighterLevers own) => On && own != null ? own.Drop : 1f;
        public static float BluffOf(FighterLevers own) => On && own != null ? own.Bluff : 1f;
        public static float GuardOf(FighterLevers own) => On && own != null ? own.Guard : 1f;

        /// <summary>As everyone.</summary>
        public static FighterLevers Plain() => new FighterLevers();

        /// <summary>Low and leaning in, level, his hips swaying and turning, his feet kept near the ground; small quick gestures, a deep drop.</summary>
        public static FighterLevers Prowler() => new FighterLevers
        {
            Name = "Prowler", Bounce = 0.6f, Sway = 1.6f, HipTurn = 1.6f, ChestTurn = 1.4f, StepLift = 0.7f, Lean = 10f, Crouch = 8f,
            Feint = 0.7f, Lunge = 1.2f, Drop = 1.5f, Bluff = 0.7f, Guard = 0.7f, Restless = 0.85f, Aggressive = 0.3f, Leaps = 0.8f, Heavy = 0.2f, Dodges = 0.7f, Fakes = 0.5f, GuardBreaks = 0.1f,
        };

        /// <summary>Upright and bouncing, his chest turning and his arms swinging wide, his feet lifted high; big showy gestures.</summary>
        public static FighterLevers Strutter() => new FighterLevers
        {
            Name = "Strutter", Bounce = 1.8f, Sway = 1.2f, HipTurn = 1.3f, ChestTurn = 2f, ArmSwing = 1.8f, StepLift = 1.6f, Lean = -5f,
            Feint = 1.15f, Lunge = 0.8f, Drop = 0.6f, Bluff = 1.5f, Guard = 1.3f, Restless = 0.6f, Aggressive = 0.55f, Leaps = 0.4f, Heavy = 0.5f, Dodges = 0.3f, Fakes = 1f, GuardBreaks = 0.15f,
        };

        /// <summary>Heavy and square: leaning in a little, his hips and chest turning little, his feet low; few frills, a long lunge.</summary>
        public static FighterLevers Bull() => new FighterLevers
        {
            Name = "Bull", Bounce = 0.8f, Sway = 0.7f, HipTurn = 0.6f, ChestTurn = 0.5f, ArmSwing = 0.6f, StepLift = 0.85f, Lean = 6f, Crouch = 3f,
            Feint = 1f, Lunge = 1.5f, Drop = 1f, Bluff = 0.5f, Guard = 1f, Restless = 0.2f, Aggressive = 0.9f, Leaps = 0f, Heavy = 0.85f, Dodges = 0f, Fakes = 0.25f, GuardBreaks = 0.3f,
        };

        public static readonly string[] Names = { "Plain", "Prowler", "Strutter", "Bull" };

        /// <summary>One of the made-up fighters by name (null: there is none of that name).</summary>
        public static FighterLevers Named(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "plain": return Plain();
                case "prowler": return Prowler();
                case "strutter": return Strutter();
                case "bull": return Bull();
                default: return null;
            }
        }
    }
}
