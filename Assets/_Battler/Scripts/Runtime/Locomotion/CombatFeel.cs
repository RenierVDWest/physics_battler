using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The fight's feel, in one place to tweak live (the scene's "Combat Feel" object): how a blow is felt by the body
    /// it lands on, the shield or the blade that meets it, and the attacker it is stopped on. Its values are pushed to
    /// the statics the clip fighters read every frame, in play mode and out of it; with none in the scene (headless)
    /// the defaults hold. More of the fight's feel joins it as it is made.
    /// </summary>
    [ExecuteAlways]
    public sealed class CombatFeel : MonoBehaviour
    {
        [Header("Hit bend: a blow bends the body round where it landed")]
        [Tooltip("How far the bend pushes the part the blow struck, per m/s of the blow's weight (metres, at a Medium's size; bigger bodies scale up). The parts around it are pushed less the further they are from the blow, so the body curves round it. For scale, a Medium's sword weighs about 0.4 m/s, its club 0.65, the Large's club 1.2 to 3. Raise it for a more violent bend on every blow; 0 turns the bend off. Default 0.35.")]
        [Range(0f, 1.5f)] public float bendPerWeight = DefaultBendPerWeight;

        [Tooltip("The furthest any one blow's bend can push the struck part (metres, at a Medium's size), however heavy the blow. Keeps the Large's club from folding the body in half. Default 0.4.")]
        [Range(0f, 1f)] public float mostBend = DefaultMostBend;

        [Tooltip("How much harder a blow that doesn't push the body straight back bends it and throws its upper body: one that throws it sideways or forward, as a second or third attacker's from its side or behind does (the plan \"Fighting off several attackers\", step 6), so it reads as contact. 1: the same as from in front. Default 1.35.")]
        [Range(1f, 2.5f)] public float offFrontBend = DefaultOffFrontBend;

        [Tooltip("The longest a reaction to a blow from the side or behind takes (seconds), its recovery hurried to end in time. The side hit clips are long (1.3 to 1.6 s); a unit in one can't step, turn or strike back, so against two or three attackers a full one left it open to the next blow. Its first moments, the blow landing, play at their own speed. Default 1.0.")]
        [Range(0.6f, 2f)] public float sideHitTime = DefaultSideHitTime;

        [Tooltip("How far from the point of contact the bend spreads before it fades to nothing (metres, at a Medium's size). Smaller: only the struck part and its close neighbours move, a sharp local jolt. Larger: the whole body goes with the blow. Default 1.2.")]
        [Range(0.2f, 3f)] public float bendReach = DefaultBendReach;

        [Tooltip("How long the bend takes to reach its furthest (seconds). It then springs back and is mostly gone after about five times this. Smaller is snappier, larger is a slower, heavier sway. Default 0.08.")]
        [Range(0.02f, 0.3f)] public float bendPeak = DefaultBendPeak;

        [Header("Hit lean and head snap (the older whole-body reaction)")]
        [Tooltip("Scales how far a blow tips the whole upper body away from it (the lean that came before the bend, on top of it). 1 is as it was tuned, 0 turns it off, 2 doubles it.")]
        [Range(0f, 3f)] public float leanScale = 1f;

        [Tooltip("Scales how far a blow to the head snaps the head back or aside. 1 is as it was tuned, 0 turns it off, 2 doubles it.")]
        [Range(0f, 3f)] public float headSnapScale = 1f;

        [Header("Hit stop: both stand still a moment as a blow lands")]
        [Tooltip("The part of the hit stop every blow gets, whatever its weight (seconds). The weight then adds to it. Default 0.03.")]
        [Range(0f, 0.2f)] public float hitStopBase = DefaultHitStopBase;

        [Tooltip("How much longer the hit stop gets per m/s of the blow's weight (seconds), so heavy blows freeze the moment longer. With the defaults a Medium's sword stops about 0.05 s, its club 0.07, the Large's club 0.10 to 0.16. Default 0.05.")]
        [Range(0f, 0.2f)] public float hitStopPerWeight = DefaultHitStopPerWeight;

        [Tooltip("The shortest hit stop any blow that lands gets (seconds), so even the lightest blow registers. Default 0.04.")]
        [Range(0f, 0.2f)] public float leastHitStop = DefaultLeastHitStop;

        [Tooltip("The longest hit stop a blow can get (seconds), however heavy. A combo's final blow (the showstopper) has its own hold, 0.13 to 0.17 s. Default 0.16.")]
        [Range(0f, 0.4f)] public float mostHitStop = DefaultMostHitStop;

        [Header("Driven back: what the struck part doesn't take moves the whole body")]
        [Tooltip("How hard a blow drives the struck body back across the ground: the share of the blow's momentum the struck part doesn't absorb, over the body's mass, times this, becomes the speed its steps carry it back at. Raise it to make every hit move the target further. Default 3.5.")]
        [Range(0f, 8f)] public float driveBack = DefaultDriveBack;

        [Tooltip("The fastest a blow can drive a body back (m/s), when it doesn't stumble. Default 2.2.")]
        [Range(0f, 4f)] public float mostDrive = DefaultMostDrive;

        [Tooltip("The weight (m/s) from which a blow throws the body back stumbling at once, its feet scrambling to catch it, instead of playing a reaction first and being driven back after. The Large's club on a Medium is about 1.2 to 3, a Medium's club 0.65, a sword 0.4. Lower it to make more blows stumble. Default 1.")]
        [Range(0.2f, 4f)] public float stumbleFrom = DefaultStumbleFrom;

        [Tooltip("How much harder a stumbling blow throws the body back than an ordinary drive (times the drive-back speed). Default 1.3.")]
        [Range(1f, 3f)] public float stumbleBoost = DefaultStumbleBoost;

        [Tooltip("The fastest a stumble can throw a body back (m/s). Higher sends it further but its feet may slide trying to keep up. Default 2.8.")]
        [Range(0f, 5f)] public float mostStumble = DefaultMostStumble;

        [Tooltip("How long a stumbling body needs to find its feet before it can strike or steer again (seconds). Default 0.7.")]
        [Range(0f, 2f)] public float stumbleRecovery = DefaultStumbleRecovery;

        [Header("Block and parry: the defender feels the blow's weight too")]
        [Tooltip("How far a parried blow drives the parrying sword back toward the body before it sweeps the blow aside, per m/s of the blow's weight (metres, at a Medium's size). The attacking weapon stays on the blade as it gives. A Medium's sword (about 0.4 m/s) gives it 0.04 m, the Large's club (about 1.4) 0.14. Default 0.1.")]
        [Range(0f, 0.4f)] public float parryGivePerWeight = DefaultParryGivePerWeight;

        [Tooltip("The furthest a held parry gives (metres, at a Medium's size), however heavy the blow. Default 0.2.")]
        [Range(0f, 0.5f)] public float mostParryGive = DefaultMostParryGive;

        [Tooltip("The heaviest blow (m/s of weight) a novice's square parry can hold. Anything heavier beats the parry: the blade is crushed back into the defender and part of the blow lands through it. A blow taken at a slant (a glance) is never beaten. A combo's final blow counts double. Default 0.8: a novice holds a Medium's club but not the Large's.")]
        [Range(0.2f, 4f)] public float parryHolds = DefaultParryHolds;

        [Tooltip("The heaviest blow (m/s of weight) an expert's square parry can hold; skill in between holds in proportion. Default 2: an expert holds the Large's single blows, not its combo's final blow.")]
        [Range(0.2f, 5f)] public float expertParryHolds = DefaultExpertParryHolds;

        [Tooltip("When a parry is beaten, the share of the blow's weight that lands through it on the body behind the blade (the rest is spent on the sword). Default 0.6.")]
        [Range(0f, 1f)] public float beatenShare = DefaultBeatenShare;

        [Tooltip("How far a blocked blow knocks the shield arm back, per m/s of the blow's weight on the shield (metres, at a Medium's size). A sword on a shield is about 0.5 m/s, the Large's club taken square on about 1.2. Default 0.12.")]
        [Range(0f, 0.4f)] public float shieldGivePerWeight = DefaultShieldGivePerWeight;

        [Tooltip("The furthest a block knocks the shield arm back (metres, at a Medium's size). Default 0.25.")]
        [Range(0f, 0.6f)] public float mostShieldGive = DefaultMostShieldGive;

        [Tooltip("A blow this heavy on the shield or more (m/s of weight) knocks the guard open: the shield arm is thrown wide, comes back slowly, and the shield doesn't move to meet another blow until the guard is back. A shield slanted to turn the blow takes less of its weight. Default 1.")]
        [Range(0.2f, 4f)] public float guardOpenFrom = DefaultGuardOpenFrom;

        [Tooltip("How long a guard knocked open stays open (seconds). Default 0.5.")]
        [Range(0f, 2f)] public float guardOpenFor = DefaultGuardOpenFor;

        [Header("The attacker's rebound off a block or a parry")]
        [Tooltip("How fast the attacker's weapon hand is thrown back off a block or a parry, per m/s of the blow's weight (m/s). The heavier the blow, the harder it is stopped and the more it jars the arm. Default 2.4.")]
        [Range(0f, 6f)] public float reboundPerWeight = DefaultReboundPerWeight;

        [Tooltip("The least the attacker's hand is thrown back off a block or a parry (m/s), so even a light blow visibly bounces. Default 0.8.")]
        [Range(0f, 3f)] public float leastRebound = DefaultLeastRebound;

        [Tooltip("The most the attacker's hand is thrown back off a block or a parry (m/s). Default 3.5.")]
        [Range(0f, 8f)] public float mostRebound = DefaultMostRebound;

        [Tooltip("How much a blow stopped on a shield or a blade jars the attacker's own upper body (its lean back, against the lean a blow gives the one it hits). 0 none. Default 0.3.")]
        [Range(0f, 1.5f)] public float jarScale = DefaultJarScale;

        [Header("Shield: how it moves to meet a blow")]
        [Tooltip("How long the shield takes to settle where it is going (seconds): it speeds up and slows in like an arm, never jumping. Larger is calmer and slower to arrive, smaller snappier. Default 0.08.")]
        [Range(0.02f, 0.5f)] public float shieldSettle = DefaultShieldSettle;

        [Tooltip("How long the shield's face takes to settle into the way it is turning (seconds). Larger is calmer, smaller snappier. Default 0.1.")]
        [Range(0.02f, 0.5f)] public float shieldFaceSettle = DefaultShieldFaceSettle;

        [Tooltip("Once the shield knows where to meet a blow, a later read moves that place only if it changes by more than this (metres, at a Medium's size). Larger: it commits to its first read and holds still; smaller: it follows the blade more closely but moves more. Default 0.2.")]
        [Range(0f, 0.6f)] public float shieldReplanBy = DefaultShieldReplanBy;

        [Tooltip("The same for the way the face looks (degrees): a later read turns it only if it changes by more than this. Default 40.")]
        [Range(0f, 90f)] public float shieldReplanTurn = DefaultShieldReplanTurn;

        [Tooltip("How skill sets the shield against a blow on its own: the curve from novice to expert (skill to the power of this). 1: a shield of skill 0.5 is halfway to an expert; higher: it needs more skill to block well. Tuned so skill 0.5 misses about a quarter of single blows. Default 1.")]
        [Range(0.3f, 4f)] public float shieldSingleCurve = DefaultShieldSingleCurve;

        [Tooltip("The same against a combo's next blow, coming on the heels of the last: harder to read. Tuned so skill 0.5 misses about half of a combo's blows. Default 4.")]
        [Range(0.3f, 6f)] public float shieldComboCurve = DefaultShieldComboCurve;

        [Tooltip("The shield starts to move to meet a blow no sooner than this before contact (seconds). Earlier, the wind-up still swings the blade's path about and the shield would follow it round. Default 0.6.")]
        [Range(0.2f, 2f)] public float shieldMovesWithin = DefaultShieldMovesWithin;

        [Header("Giving ground: a quick step back to meet a heavy or deep blow")]
        [Tooltip("A fighter steps back to block or parry a blow it reads coming at least this heavy: the weight it would be met with (m/s, the momentum a block takes over the defender's mass; a combo's blow counts 1.5 times, its showstopper twice). Met further out, earlier in its arc, it lands lighter. A parry steps back only from a blow heavier than it holds as well (its skill's \"parry holds\"): stepped back from lighter ones, its arm lost the blade. An expert always steps; a novice now and then, and late. Default 0.9.")]
        [Range(0.2f, 4f)] public float giveGroundFrom = DefaultGiveGroundFrom;

        [Tooltip("A sword fighter steps back to a blow whose attacker's hand will come within this of its body at contact (metres at a Medium's size): too deep to parry on the strong of the blade. 0: never for depth. Default 0: off. Tried at 0.45 m, the step back took the body from where the parry's arm was putting the sword and let blows through it (against the Large's single blows, 2 more landed); against the Small's combos it parried 2 more. Turn it on to try.")]
        [Range(0f, 1.2f)] public float giveGroundDeep = DefaultGiveGroundDeep;

        [Tooltip("How long before the blow's contact an expert starts its step back (seconds). The step takes about 0.1 s; started much earlier the attacker would see it and follow. A novice steps later, by how much it lacks. Default 0.22.")]
        [Range(0.05f, 0.5f)] public float giveGroundLead = DefaultGiveGroundLead;

        [Tooltip("How far an expert steps back (metres at a Medium's size), at the least (a blow just heavy enough) and at the most (one twice as heavy, or as deep as it gets). A novice steps a little over half as far. The attacker follows it: the blow still meets the shield, further out. Defaults 0.1 and 0.2 (0.25 to 0.5 was far too much in the playtest).")]
        [Range(0f, 1f)] public float leastGiveGround = DefaultLeastGiveGround;

        [Tooltip("See the least step back: the most (metres at a Medium's size). Default 0.2.")]
        [Range(0f, 1.2f)] public float mostGiveGround = DefaultMostGiveGround;

        [Tooltip("The share of a blow's weight a full step back takes off as it is met (its block or parry knocked back less, the parry beaten less often). 0: stepping back only looks right. Default 0.3.")]
        [Range(0f, 0.8f)] public float giveGroundEases = DefaultGiveGroundEases;

        [Header("Concentration: outnumbered, a unit fights sharper")]
        [Tooltip("How fast concentration builds while two or more enemies are on a unit (per second, for each attacker past the first: against three it builds twice as fast as against two). Concentration runs from 0 to 1. Default 0.3: against two, full in a little over 3 seconds.")]
        [Range(0f, 2f)] public float concentrationRise = DefaultConcentrationRise;

        [Tooltip("How fast concentration fades once it has one enemy or none on it (per second). Default 0.15: from full to nothing in about 7 seconds, so a unit that has just broken out of a gang keeps some of its edge a while.")]
        [Range(0f, 2f)] public float concentrationFall = DefaultConcentrationFall;

        [Tooltip("What full concentration adds to a unit's own blocks and parries: this much on its shield and parry skill (0 to 1; it reads blows truer and misses fewer), and as much again of the weight its parry holds and its guard takes before it is knocked open, its shield knocked back less. 0: no help to its defence. Default 0.5.")]
        [Range(0f, 1f)] public float concentrationGuard = DefaultConcentrationGuard;

        [Tooltip("What full concentration takes off the block and parry skill of whoever it strikes (0 to 1): its blows are aimed truer, so they are misread and slip past guards more often. An expert (skill 1) struck by a fully concentrated unit defends as one of skill 1 minus this. 0: no effect on its blows. Default 0.3.")]
        [Range(0f, 1f)] public float concentrationAccuracy = DefaultConcentrationAccuracy;

        [Header("Combos across several enemies")]
        [Tooltip("A combo's next blow may turn on another enemy fighting the unit, if that one stands within this of where the unit faces (degrees), so a combo is strung across the enemies on it (one, the next, back to the first) instead of all its blows at one. 0: every blow at the one it opened on, as before. Default 100.")]
        [Range(0f, 180f)] public float comboSpreadTurn = DefaultComboSpreadTurn;

        [Tooltip("How near another enemy must stand for a combo's next blow to turn on it (metres from the unit, at a Medium's size): the follow-on steps in after it, at most about 2 m. Default 3.2, the distance at which an enemy counts as on it.")]
        [Range(1f, 4f)] public float comboSpreadReach = DefaultComboSpreadReach;

        [Tooltip("With two or more enemies fighting it within that reach and turn, the chance a blow it starts opens a combo strung across them, whatever its tempo would say (its tempo's own combos come as well). 0: only its tempo's combos. Default 0.25 (more commits it to long strings with others round it).")]
        [Range(0f, 1f)] public float comboSweepChance = DefaultComboSweepChance;

        [Header("The physical upper body")]
        [Tooltip("Clip fighters' upper bodies are physical (the plan \"A different approach\", step 5): the hips, torso, head and both arms are one jointed body on muscles of limited strength, with a solid sword and shield in its hands, on legs the animation still moves. A blade stops on what it meets, and bodies can't pass through each other. The game then steps the physics itself, 4 times for each of its own steps (240 a second), and a strike's blade is limited to 30 m/s unless Most Blade Speed says otherwise. Takes effect at the next Spawn. The hits themselves are still decided as before (by the pose). Off: held to the pose by pins, as it has been. Default off.")]
        public bool physicalUpperBody = DefaultPhysicalUpperBody;

        [Tooltip("How strong the physical upper body's muscles are against the world: the most a shoulder, the spine or a shield's grip holds with against what presses on it, a blade on a shield, a body leant on (newton metres, for a Medium; an elbow has 0.7 of it, a wrist and the neck a quarter; a heavier, larger body has as much more as it is heavier to turn). To follow the animation itself, however fast it moves, each muscle is given what that takes on top of this, so this no longer limits how fast a sword is swung: it sets how hard a stopped blade pushes and how firmly a body stands against a shove. Lower, fighters give way to each other more; higher, they shove each other about. Takes effect at the next Spawn. Default 600.")]
        [Range(100f, 1500f)] public float muscleStrength = DefaultMuscleStrength;

        [Tooltip("A weapon arm that is stopped gives: once its hand is held 22 cm from where the animation has it and is hardly moving (its blade stopped on a shield or a body), it presses on with only this share of its strength; under 10 cm, or while the hand is still on its way, it has all of it. 1: it never gives, and a stopped strike goes on shoving with all of its strength. Lower, a blocked blade rests on the shield until the strike is over. The arm a shield is on never gives: it holds against what presses on it. Default 0.2.")]
        [Range(0.05f, 1f)] public float blockedArmStrength = DefaultBlockedArmStrength;

        [Tooltip("A body pressed on gives way with its feet: for each metre its chest is held off where the animation has it (by a body it stands against, by a blade pressed on it), it is moved this many metres a second the way it is pressed (2 m/s at most). 0: never, and all of it is taken up by its spine, the body folded back over its own hips. Higher, crowded fighters push each other about more. Default 6.")]
        [Range(0f, 15f)] public float pressedBackSpeed = DefaultPressedBackSpeed;

        [Tooltip("A body borne down falls: its chest held this many centimetres (at a Medium's size) from where the animation has it for a tenth of a second, by what its muscles can't shift (a fighter landing on it from a leap), it goes down as one knocked off its feet and gets up again. 0: never, and it is folded where it stands. Default 28.")]
        [Range(0f, 80f)] public float borneDownFrom = DefaultBorneDownFrom;

        [Tooltip("How fast a joint of the physical upper body that is off where the animation has it comes back (radians a second, on top of the animation's own turn; 6.3 is one turn a second): an arm knocked aside by a parry, a head thrown back by a blow, a shield arm let go by what held it. It is brought back at this speed at most, and slower the nearer it gets, so that it stops where the animation has it and does not swing past. Higher: it is back sooner and whips more; lower: a struck body hangs off its pose longer and looks softer. Default 20 (an arm knocked a quarter turn off is back in about a tenth of a second).")]
        [Range(4f, 60f)] public float returnSpeed = DefaultReturnSpeed;

        [Header("Feet")]
        [Tooltip("How fast a combo's strike may carry the body in after its target through the strike's wind-up (metres a second, at a Medium's size): the pull that keeps a combo's blows landing on an enemy the last blow drove back. It was 7.2: a body moved that fast is not stepping, its feet were dragged after it. At 3 it goes as fast as a quick step and as many blows still land (measured over three fights); lower and blows start to fall short (at 2 the body is carried 40% less far). Default 3.")]
        [Range(0.5f, 8f)] public float comboPullSpeed = DefaultComboPullSpeed;

        [Tooltip("A foot moving over the ground is lifted, as a stepping foot is: this many centimetres at most (at a Medium's size), from 0.4 m/s up. A foot is then either held where it stands or in the air, never slid: without it, a body snapped away under a combo dragged a foot 55 cm along the ground. A walk's own swing is higher and is left as it is. 0: off, as before. Default 7.")]
        [Range(0f, 12f)] public float movingFootLift = DefaultMovingFootLift;

        [Header("A blow's speed")]
        [Tooltip("The fastest a strike may swing its blade's point (metres a second). The strike clips whip the blade through their two fastest frames at 45 to 67 m/s, two or three times what an arm can swing a sword at (a real cut's point goes 20 to 30 m/s), which no body made of muscles can follow. With a limit set, the blade's point never goes faster than this, and the strike's clip is slowed over those frames so the body waits for its blade: a blow lands about 0.07 s later at 30, about 0.12 s later at 25. 0: the clips play at their own pace, as before. Default 0.")]
        [Range(0f, 80f)] public float mostBladeSpeed = DefaultMostBladeSpeed;

        public const float DefaultBendPerWeight = 0.35f, DefaultMostBend = 0.4f, DefaultBendReach = 1.2f, DefaultBendPeak = 0.08f;
        public const float DefaultOffFrontBend = 1.35f, DefaultSideHitTime = 1.0f;
        public const float DefaultMostBladeSpeed = 0f;
        public const bool DefaultPhysicalUpperBody = false;
        public const float DefaultMuscleStrength = 600f, DefaultBlockedArmStrength = 0.2f, DefaultPressedBackSpeed = 6f, DefaultBorneDownFrom = 28f, DefaultReturnSpeed = 20f;

        /// <summary>The blade's speed limit a physical upper body is given when none is set (m/s).</summary>
        public const float PhysicalBladeSpeed = 30f;
        public const float DefaultComboPullSpeed = 3f, DefaultMovingFootLift = 7f;
        public const float DefaultComboSpreadTurn = 100f, DefaultComboSpreadReach = 3.2f, DefaultComboSweepChance = 0.25f;
        public const float DefaultConcentrationRise = 0.3f, DefaultConcentrationFall = 0.15f, DefaultConcentrationGuard = 0.5f, DefaultConcentrationAccuracy = 0.3f;
        public const float DefaultHitStopBase = 0.03f, DefaultHitStopPerWeight = 0.05f, DefaultLeastHitStop = 0.04f, DefaultMostHitStop = 0.16f;
        public const float DefaultDriveBack = 3.5f, DefaultMostDrive = 2.2f, DefaultStumbleFrom = 1.0f, DefaultStumbleBoost = 1.3f, DefaultMostStumble = 2.8f, DefaultStumbleRecovery = 0.7f;
        public const float DefaultParryGivePerWeight = 0.1f, DefaultMostParryGive = 0.2f, DefaultParryHolds = 0.8f, DefaultExpertParryHolds = 2f, DefaultBeatenShare = 0.6f,
            DefaultShieldGivePerWeight = 0.12f, DefaultMostShieldGive = 0.25f, DefaultGuardOpenFrom = 1f, DefaultGuardOpenFor = 0.5f,
            DefaultReboundPerWeight = 2.4f, DefaultLeastRebound = 0.8f, DefaultMostRebound = 3.5f, DefaultJarScale = 0.3f;
        public const float DefaultShieldSettle = 0.08f, DefaultShieldFaceSettle = 0.1f, DefaultShieldReplanBy = 0.2f, DefaultShieldReplanTurn = 40f, DefaultShieldMovesWithin = 0.6f,
            DefaultShieldSingleCurve = 1f, DefaultShieldComboCurve = 4f;
        public const float DefaultGiveGroundFrom = 0.9f, DefaultGiveGroundDeep = 0f, DefaultGiveGroundLead = 0.22f, DefaultLeastGiveGround = 0.1f, DefaultMostGiveGround = 0.2f, DefaultGiveGroundEases = 0.3f;

        /// <summary>The values in force (the scene's Combat Feel's, else the defaults).</summary>
        public static float OffFrontBend = DefaultOffFrontBend, SideHitTime = DefaultSideHitTime;

        public static float MostBladeSpeed = DefaultMostBladeSpeed;

        public static float ComboPullSpeed = DefaultComboPullSpeed, MovingFootLift = DefaultMovingFootLift;

        public static float ComboSpreadTurn = DefaultComboSpreadTurn, ComboSpreadReach = DefaultComboSpreadReach, ComboSweepChance = DefaultComboSweepChance;

        public static float ConcentrationRise = DefaultConcentrationRise, ConcentrationFall = DefaultConcentrationFall, ConcentrationGuard = DefaultConcentrationGuard, ConcentrationAccuracy = DefaultConcentrationAccuracy;

        public static float BendPerWeight = DefaultBendPerWeight, MostBend = DefaultMostBend, BendReach = DefaultBendReach, BendPeak = DefaultBendPeak,
            LeanScale = 1f, HeadSnapScale = 1f,
            HitStopBase = DefaultHitStopBase, HitStopPerWeight = DefaultHitStopPerWeight, LeastHitStop = DefaultLeastHitStop, MostHitStop = DefaultMostHitStop,
            DriveBack = DefaultDriveBack, MostDrive = DefaultMostDrive, StumbleFrom = DefaultStumbleFrom, StumbleBoost = DefaultStumbleBoost, MostStumble = DefaultMostStumble, StumbleRecovery = DefaultStumbleRecovery,
            ParryGivePerWeight = DefaultParryGivePerWeight, MostParryGive = DefaultMostParryGive, ParryHolds = DefaultParryHolds, ExpertParryHolds = DefaultExpertParryHolds, BeatenShare = DefaultBeatenShare, ShieldGivePerWeight = DefaultShieldGivePerWeight, MostShieldGive = DefaultMostShieldGive, GuardOpenFrom = DefaultGuardOpenFrom, GuardOpenFor = DefaultGuardOpenFor, ReboundPerWeight = DefaultReboundPerWeight, LeastRebound = DefaultLeastRebound, MostRebound = DefaultMostRebound, JarScale = DefaultJarScale,
            ShieldSettle = DefaultShieldSettle, ShieldFaceSettle = DefaultShieldFaceSettle, ShieldReplanBy = DefaultShieldReplanBy, ShieldReplanTurn = DefaultShieldReplanTurn, ShieldMovesWithin = DefaultShieldMovesWithin,
            ShieldSingleCurve = DefaultShieldSingleCurve, ShieldComboCurve = DefaultShieldComboCurve,
            GiveGroundFrom = DefaultGiveGroundFrom, GiveGroundDeep = DefaultGiveGroundDeep, GiveGroundLead = DefaultGiveGroundLead, LeastGiveGround = DefaultLeastGiveGround, MostGiveGround = DefaultMostGiveGround, GiveGroundEases = DefaultGiveGroundEases;

        private void OnEnable() => Push();

        private void OnValidate() => Push();

        private void Update() => Push();

        private void OnDisable()
        {
            BendPerWeight = DefaultBendPerWeight;
            OffFrontBend = DefaultOffFrontBend;
            SideHitTime = DefaultSideHitTime;
            MostBend = DefaultMostBend;
            BendReach = DefaultBendReach;
            BendPeak = DefaultBendPeak;
            LeanScale = HeadSnapScale = 1f;
            HitStopBase = DefaultHitStopBase;
            HitStopPerWeight = DefaultHitStopPerWeight;
            LeastHitStop = DefaultLeastHitStop;
            MostHitStop = DefaultMostHitStop;
            DriveBack = DefaultDriveBack;
            MostDrive = DefaultMostDrive;
            StumbleFrom = DefaultStumbleFrom;
            StumbleBoost = DefaultStumbleBoost;
            MostStumble = DefaultMostStumble;
            StumbleRecovery = DefaultStumbleRecovery;
            ParryGivePerWeight = DefaultParryGivePerWeight;
            MostParryGive = DefaultMostParryGive;
            ParryHolds = DefaultParryHolds;
            ExpertParryHolds = DefaultExpertParryHolds;
            BeatenShare = DefaultBeatenShare;
            ShieldGivePerWeight = DefaultShieldGivePerWeight;
            MostShieldGive = DefaultMostShieldGive;
            GuardOpenFrom = DefaultGuardOpenFrom;
            GuardOpenFor = DefaultGuardOpenFor;
            ReboundPerWeight = DefaultReboundPerWeight;
            LeastRebound = DefaultLeastRebound;
            MostRebound = DefaultMostRebound;
            JarScale = DefaultJarScale;
            ShieldSettle = DefaultShieldSettle;
            ShieldFaceSettle = DefaultShieldFaceSettle;
            ShieldReplanBy = DefaultShieldReplanBy;
            ShieldReplanTurn = DefaultShieldReplanTurn;
            ShieldMovesWithin = DefaultShieldMovesWithin;
            ShieldSingleCurve = DefaultShieldSingleCurve;
            ShieldComboCurve = DefaultShieldComboCurve;
            GiveGroundFrom = DefaultGiveGroundFrom;
            GiveGroundDeep = DefaultGiveGroundDeep;
            GiveGroundLead = DefaultGiveGroundLead;
            LeastGiveGround = DefaultLeastGiveGround;
            MostGiveGround = DefaultMostGiveGround;
            GiveGroundEases = DefaultGiveGroundEases;
            ConcentrationRise = DefaultConcentrationRise;
            ConcentrationFall = DefaultConcentrationFall;
            ConcentrationGuard = DefaultConcentrationGuard;
            ConcentrationAccuracy = DefaultConcentrationAccuracy;
            ComboSpreadTurn = DefaultComboSpreadTurn;
            ComboSpreadReach = DefaultComboSpreadReach;
            ComboSweepChance = DefaultComboSweepChance;
        }

        private void Push()
        {
            BendPerWeight = bendPerWeight;
            OffFrontBend = offFrontBend;
            SideHitTime = Mathf.Max(0.3f, sideHitTime);
            MostBend = mostBend;
            BendReach = bendReach;
            BendPeak = Mathf.Max(0.01f, bendPeak);
            LeanScale = leanScale;
            HeadSnapScale = headSnapScale;
            HitStopBase = hitStopBase;
            HitStopPerWeight = hitStopPerWeight;
            LeastHitStop = leastHitStop;
            MostHitStop = Mathf.Max(leastHitStop, mostHitStop);
            DriveBack = driveBack;
            MostDrive = mostDrive;
            StumbleFrom = stumbleFrom;
            StumbleBoost = stumbleBoost;
            MostStumble = mostStumble;
            StumbleRecovery = stumbleRecovery;
            ParryGivePerWeight = parryGivePerWeight;
            MostParryGive = mostParryGive;
            ParryHolds = parryHolds;
            ExpertParryHolds = expertParryHolds;
            BeatenShare = beatenShare;
            ShieldGivePerWeight = shieldGivePerWeight;
            MostShieldGive = mostShieldGive;
            GuardOpenFrom = guardOpenFrom;
            GuardOpenFor = guardOpenFor;
            ReboundPerWeight = reboundPerWeight;
            LeastRebound = leastRebound;
            MostRebound = mostRebound;
            JarScale = jarScale;
            ShieldSettle = shieldSettle;
            ShieldFaceSettle = shieldFaceSettle;
            ShieldReplanBy = shieldReplanBy;
            ShieldReplanTurn = shieldReplanTurn;
            ShieldMovesWithin = shieldMovesWithin;
            ShieldSingleCurve = shieldSingleCurve;
            ShieldComboCurve = shieldComboCurve;
            GiveGroundFrom = giveGroundFrom;
            GiveGroundDeep = giveGroundDeep;
            GiveGroundLead = giveGroundLead;
            LeastGiveGround = leastGiveGround;
            MostGiveGround = mostGiveGround;
            GiveGroundEases = giveGroundEases;
            ConcentrationRise = concentrationRise;
            ConcentrationFall = concentrationFall;
            ConcentrationGuard = concentrationGuard;
            ConcentrationAccuracy = concentrationAccuracy;
            ComboSpreadTurn = comboSpreadTurn;
            ComboSpreadReach = comboSpreadReach;
            ComboSweepChance = comboSweepChance;
            MostBladeSpeed = mostBladeSpeed > 0f ? mostBladeSpeed : physicalUpperBody ? PhysicalBladeSpeed : 0f;
            ClipFighter.PhysicalUpper = physicalUpperBody;
            ClipFighter.MuscleStrength = muscleStrength;
            ClipFighter.BlockedStrength = blockedArmStrength;
            ClipFighter.PressedGain = pressedBackSpeed;
            ClipFighter.BorneDownFrom = borneDownFrom * 0.01f;
            PhysicsBattler.Authority.UpperBodyArticulation.MostReturn = returnSpeed;
            ComboPullSpeed = comboPullSpeed;
            MovingFootLift = movingFootLift;
        }
    }
}
