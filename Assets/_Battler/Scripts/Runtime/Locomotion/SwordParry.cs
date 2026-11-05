using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A sword without a shield meeting the blows that come at it (the defence plan's sword parry, stage 1): from
    /// a medium guard (the hand before the belly, the point at the opponent's face), it reads each blow from the
    /// attacker's wind-up (<see cref="BlowRead"/>), takes the sabre and broadsword manuals' parry for where the blow
    /// will land (St George, hanging, tierce, quarte, prime, seconde, septime, octave and the rest), and moves its
    /// sword there: the forte (the strong third, by the hilt) on the incoming blade's path just before it lands,
    /// the blade square to the plane the incoming blade sweeps, its edge to the blow. The trunk turns and leans
    /// with every parry. The arm is the fight's arm rig (<see cref="ArmRig.Solve"/>): the elbow on the parry's
    /// null (where the manuals put it), the wrist within its limits, the arm clear of the torso. The hand, the
    /// blade, the elbow and the trunk move to each new place at a human's speed, never jumping.
    /// </summary>
    public sealed class SwordParry
    {
        /// <summary>A held position: the grip (metres at a real Medium's size, against the ground under the body: x right, y up, z forward), the blade's way, the edge's face, the elbow's null (degrees: 0 down, +90 out, 180 up, -90 in), and the trunk's turn (+ to its right) and lean (+ forward, degrees).</summary>
        public struct Position
        {
            public string Name;
            public Vector3 Hand, Blade, Edge;
            public float Swivel, Turn, Lean;

            public Position(string name, Vector3 hand, Vector3 blade, Vector3 edge, float swivel, float turn, float lean)
            {
                Name = name;
                Hand = hand;
                Blade = blade;
                Edge = edge;
                Swivel = swivel;
                Turn = turn;
                Lean = lean;
            }
        }

        /// <summary>The medium guard every parry starts from and goes back to (Hutton, Castle): the hand before the belly, the point at the opponent's face, the edge down.</summary>
        public static readonly Position Guard = new Position("medium guard", new Vector3(0.18f, 1.05f, 0.32f), new Vector3(-0.1f, 0.35f, 1f), Vector3.down, 10f, 0f, 0f);

        /// <summary>The parries (the manuals' positions, as the parryreach study put them on the arm rig).</summary>
        public static readonly Position[] Parries =
        {
            new Position("St George", new Vector3(0.25f, 1.85f, 0.30f), new Vector3(-1f, 0.05f, 0.35f), Vector3.up, 80f, 5f, -4f),
            new Position("hanging", new Vector3(0.10f, 1.80f, 0.35f), new Vector3(-0.8f, -0.5f, 0.3f), new Vector3(0f, 0.6f, 0.8f), 120f, -8f, -4f),
            new Position("high tierce", new Vector3(0.30f, 1.60f, 0.35f), new Vector3(-0.6f, 0.7f, 0.2f), new Vector3(0.7f, 0.5f, 0.3f), 60f, 8f, 0f),
            new Position("tierce", new Vector3(0.30f, 1.15f, 0.35f), new Vector3(0.2f, 0.9f, 0.35f), Vector3.right, 20f, 15f, 2f),
            new Position("quarte", new Vector3(-0.05f, 1.15f, 0.35f), new Vector3(-0.15f, 0.9f, 0.35f), Vector3.left, -15f, -12f, 2f),
            new Position("high quarte", new Vector3(-0.15f, 1.60f, 0.35f), new Vector3(0.6f, 0.7f, 0.2f), new Vector3(-0.6f, 0.6f, 0.4f), 10f, -10f, 0f),
            new Position("prime", new Vector3(-0.20f, 1.45f, 0.35f), new Vector3(0.05f, -1f, 0.15f), Vector3.left, 110f, -15f, 2f),
            new Position("seconde", new Vector3(0.30f, 1.00f, 0.35f), new Vector3(0.3f, -0.8f, 0.5f), Vector3.right, 40f, 10f, 5f),
            new Position("low tierce", new Vector3(0.25f, 0.95f, 0.30f), new Vector3(0.3f, 0.9f, 0.2f), Vector3.right, 10f, 20f, 6f),
            new Position("low quarte", new Vector3(0.15f, 0.95f, 0.30f), new Vector3(-0.5f, 0.75f, 0.15f), new Vector3(-0.3f, -0.2f, 0.9f), -10f, -8f, 6f),
            new Position("septime", new Vector3(0.00f, 0.95f, 0.35f), new Vector3(-0.2f, -0.6f, 0.75f), Vector3.left, -20f, -15f, 10f),
            new Position("octave", new Vector3(0.15f, 1.00f, 0.40f), new Vector3(0.3f, -0.75f, 0.55f), Vector3.right, 0f, 8f, 8f),
            // (The plan "Parries that divert": point down along a leg, for a blow below the knees, the blade hanging
            // nearly straight to the ground. Low prime over the left leg, the lead leg, low seconde over the right.)
            new Position("low prime", new Vector3(-0.15f, 0.92f, 0.35f), new Vector3(-0.15f, -1f, 0.25f), Vector3.left, 110f, -15f, 12f),
            new Position("low seconde", new Vector3(0.30f, 0.90f, 0.35f), new Vector3(0.15f, -1f, 0.25f), Vector3.right, 40f, 12f, 12f),
        };

        /// <summary>Our Medium against a real one (the positions are a real body's; the parryreach study scaled them so).</summary>
        public const float BodyScale = 0.9f;

        /// <summary>Where the forte is along the blade (share of its length from the grip), where on it the parry meets the blow.</summary>
        public const float ForteAt = 0.3f;

        /// <summary>How far the forte may go from its parry's own place to meet the incoming blade's path, and the blade turn from the parry's own way (metres at a Medium's size, degrees).</summary>
        public const float PlaceMost = 0.35f, MostBladeOff = 35f;

        /// <summary>The fastest the hand moves, the blade turns, the elbow swivels and the trunk turns (m/s, degrees a second); and how quickly each closes on where it is going (1/s).</summary>
        public const float HandSpeed = 6f, BladeTurnSpeed = 1200f, SwivelSpeed = 540f, TrunkSpeed = 300f, Close = 18f;

        /// <summary>How long after a blow starts it is read (seconds), how long a parry is held after the blow has passed, and the wrist's share of its range it uses.</summary>
        public const float ReadAfter = 0.1f, HoldAfter = 0.15f, WristEase = 0.85f;

        /// <summary>How long before contact the parry starts out (seconds): the hand's way takes about 0.15-0.2 s.</summary>
        public const float MoveLead = 0.32f;

        /// <summary>How far the hand may roll about the grip from the edge asked (degrees each way).</summary>
        public const float RollAllowance = 70f;

        private readonly AnimRig anim;
        private readonly Locomotor loco;
        private readonly UnitRig rig;
        private readonly float size;
        public float WeaponLength;

        private bool started, handKnown, resyncElbow;
        private Vector3 forte;
        private Quaternion handRot, heldHand;
        private float swivel, elbow, turn, lean, sinceBlow = 99f;
        private int edgeSign = 1, lastChosen = -2;
        private Vector3 lastSlot;
        private float lastSolvedSwivel;
        private Vector3 lastUpperArm, lastForte;
        private bool lastForteKnown;

        /// <summary>The most the upper arm turns at the shoulder (degrees a second): a fast parry's, and no faster.</summary>
        public static float UpperArmSpeed = 900f;

        /// <summary>How fast the arm goes back to the guard after a parry, against the parry's own speeds: unhurried.</summary>
        public const float GuardPace = 0.5f;

        private float Pace => Current >= 0 ? 1f : GuardPace;

        /// <summary>How near the body's middle the blow's path must come to be at the body (metres at a Medium's size), and how high the head reaches (real metres).</summary>
        public const float BodyRadius = 0.3f, HeadTop = 1.75f;

        /// <summary>The parry being made now (-1: the guard).</summary>
        public int Current { get; private set; } = -1;

        public string CurrentName => Current >= 0 ? Parries[Current].Name : Guard.Name;

        /// <summary>The worst the arm did since <see cref="ResetMeasures"/>: the wrist's shortfall of the turn asked (degrees), the elbow's biggest jump round its circle in a step (degrees), the fastest hand (m/s), the shoulder past its limits (degrees), the arm into the torso (metres).</summary>
        public float MostWristShort, MostSwivelJump, MostHandSpeed, MostShoulderPast, MostIntoTorso;

        /// <summary>The parries made since the measures were reset, in order (for the logs).</summary>
        public string Made = "";

        /// <summary>A line each step, for the runner's trace (null: none).</summary>
        public static System.Action<string> Trace;

        // ---- Parry skill (the defence plan's stage 3): 0 a novice, 1 an expert -------------------------------------

        /// <summary>
        /// How well it parries, 0 to 1 (the defence plan's table of levers): how soon into a blow it moves (0.4 s, on the
        /// blade, to 0.1 s, on the wind-up), how far off it reads where the blow lands (25 cm to none) and how often it
        /// reads it again as it comes (never to every step), how often it takes the wrong parry (30% to never), its hand's
        /// speed (2.5 to 6 m/s), and how it meets the blade (a shallow cross, often out on its own foible, to square on
        /// its forte: the blow then glances off, or is stopped). Each blow's slips are drawn as it starts, the same each run.
        /// </summary>
        public float Skill = 1f;

        /// <summary>The levers at skill 0 (at 1: 0.1 s, none, every step, never, 6 m/s, square on the forte).</summary>
        public const float NoviceReadAfter = 0.4f, NoviceMisread = 0.25f, NoviceRereadEvery = 0.3f, NoviceWrongPick = 0.3f, NoviceHandSpeed = 2.5f,
            NoviceSquare = 0.15f, NoviceFoible = 0.4f, NoviceMoveLead = 0.12f;

        private readonly System.Random luck = new System.Random(20261104);
        private float lastComing = -1f, rereadIn, foibleBy;
        private int lastSerial = -1, takenFor = -1;
        private Vector3 tracedAim;
        private bool takenAhead, readAhead;
        private float toLandNow = -1f;

        /// <summary>How late its hand will be to the blow it is meeting (seconds: what it still needs to get there less what the blow gives it; 0 or less, in time).</summary>
        public float Late { get; private set; }

        /// <summary>A blow met on the blade drives the sword back along it (world, metres): the hand gives and comes back at the hand's pace, and the next parry starts from where it was knocked.</summary>
        public void Knock(Vector3 by)
        {
            forte += by;
        }

        /// <summary>
        /// The set-aside (the plan "Parries that divert"): the forte carried this much further at once, the blade with
        /// it, the blow it is bound to carried along. Held there, not eased back to the parry, until <see cref="EndSweep"/>.
        /// </summary>
        public void Sweep(Vector3 by)
        {
            forte += by;
            sweeping = true;
        }

        /// <summary>The sweep over: the sword goes back to its parry at the hand's own speed.</summary>
        public void EndSweep() => sweeping = false;

        /// <summary>Sweeping a blow aside (<see cref="Sweep"/>).</summary>
        public bool Sweeping => sweeping;

        private bool sweeping;
        private Vector3 misread, heldLand, heldMotion;
        private bool wrongPick, readHeld, playsOn = true;

        /// <summary>Blows read and how many of them it took the wrong parry for (for the logs).</summary>
        public int BlowsRead, WrongPicks;

        /// <summary>How far from an expert, 0 to 1: a curve, so the middle of the scale parries about midway between a novice and an expert (in a straight line, skill 0.5 already parried nine in ten).</summary>
        private float Lack => 1f - Mathf.Clamp01(Skill) * Mathf.Clamp01(Skill);

        /// <summary>A new blow: this one's slips drawn.</summary>
        private void NewBlow()
        {
            BlowsRead++;
            float lack = Lack;
            Vector3 dir = new Vector3((float)luck.NextDouble() * 2f - 1f, (float)luck.NextDouble() * 2f - 1f, (float)luck.NextDouble() * 2f - 1f);
            misread = (dir.sqrMagnitude > 1e-4f ? dir.normalized : Vector3.up) * (NoviceMisread * lack * (0.4f + 0.6f * (float)luck.NextDouble()) * size);
            wrongPick = luck.NextDouble() < NoviceWrongPick * lack;
            WrongPicks += wrongPick ? 1 : 0;
            foibleBy = NoviceFoible * lack * (float)luck.NextDouble();
            // (An expert plays its parry on against the blade it sees coming; a novice goes where it read it would be.)
            playsOn = luck.NextDouble() >= lack;
            rereadIn = 0f;
            readHeld = false;
        }

        public SwordParry(AnimRig anim, Locomotor loco, UnitRig rig, float size, float weaponLength)
        {
            this.anim = anim;
            this.loco = loco;
            this.rig = rig;
            this.size = size;
            WeaponLength = weaponLength;
        }

        public void ResetMeasures()
        {
            MostWristShort = MostSwivelJump = MostHandSpeed = MostShoulderPast = MostIntoTorso = 0f;
            Made = "";
        }

        /// <summary>The sword's blade now (world): from the grip to the tip.</summary>
        public void BladeNow(out Vector3 grip, out Vector3 tip)
        {
            var slot = anim.SlotWeaponR;
            grip = slot.position;
            tip = grip + slot.rotation * Vector3.forward * WeaponLength;
        }

        private Quaternion BodyTurn => Quaternion.Euler(0f, loco.Facing, 0f);

        private Vector3 Root => loco.Position;

        private Vector3 World(Vector3 local) => Root + BodyTurn * (local * (BodyScale * size));

        /// <summary>The parry for a blow: by where it will land on the body (high, middle, low; inside or outside) and how it comes in.</summary>
        /// <summary>
        /// Where the blow will first be at the body, and which way it moves there: the first point of its path that
        /// comes within <see cref="BodyRadius"/> of the body's middle (below the head's top), else where it is aimed.
        /// </summary>
        private void Landing(in BlowRead blow, out Vector3 land, out Vector3 motion)
        {
            land = blow.Aim;
            motion = -blow.ComesFrom;
            landHeight = blow.Aim.y;
            landPart = 0;
            landK = -1;
            if (blow.Path == null)
            {
                return;
            }

            // (The body as the blade meets it, as it stands now: the head, the torso, each thigh and shin; looked for
            // along each step of the blade's path, since a blade crosses a head between two of its points.)
            Vector3 P(PartId id) => anim.Part(id).position;

            // (The whole blade, grip to tip, against each part: a chop at the chest passes the head with its middle.)
            bool Inside(Vector3 g, Vector3 tp)
            {
                float r = size;
                Vector3 headAt = P(PartId.Head) + Vector3.up * (0.08f * r);
                float Seg(Vector3 a2, Vector3 b2) => StrikeHits.SegmentsNearest(g, tp, a2, b2, out _, out _, out _, out _);
                if (Seg(headAt, headAt) <= 0.14f * r)
                {
                    landPart = 0;
                    landHeight = headAt.y;
                    return true;
                }

                if (Seg(P(PartId.Pelvis), P(PartId.Chest)) <= 0.17f * r || Seg(P(PartId.Chest), P(PartId.Head)) <= 0.15f * r)
                {
                    landPart = 0;
                    StrikeHits.SegmentsNearest(g, tp, P(PartId.Pelvis), headAt, out _, out _, out _, out Vector3 onBody);
                    landHeight = onBody.y;
                    return true;
                }

                for (int leg = 0; leg < 2; leg++)
                {
                    var thigh = P(leg == 0 ? PartId.ThighL : PartId.ThighR);
                    var shin = P(leg == 0 ? PartId.ShinL : PartId.ShinR);
                    var foot = P(leg == 0 ? PartId.FootL : PartId.FootR);
                    if (Seg(thigh, shin) <= 0.1f * r || Seg(shin, foot) <= 0.08f * r)
                    {
                        landPart = leg == 0 ? -1 : 1;
                        StrikeHits.SegmentsNearest(g, tp, thigh, foot, out _, out _, out _, out Vector3 onLeg);
                        landHeight = onLeg.y;
                        return true;
                    }
                }

                return false;
            }

            var grips = blow.GripPath ?? blow.Path;
            var tips = blow.TipPath ?? blow.Path;
            for (int k = 1; k < blow.Path.Length; k++)
            {
                for (int j = 1; j <= 8; j++)
                {
                    float u = j / 8f;
                    Vector3 g = Vector3.Lerp(grips[k - 1], grips[k], u), tp = Vector3.Lerp(tips[k - 1], tips[k], u);
                    if (Inside(g, tp))
                    {
                        landK = k;
                        landU = u;
                        // (Where the striking point is then: the forte goes across its path there.)
                        land = Vector3.Lerp(blow.Path[k - 1], blow.Path[k], u);
                        Vector3 step = Vector3.Lerp(tips[k - 1], tips[k], u) - Vector3.Lerp(tips[k - 1], tips[k], Mathf.Max(0f, u - 0.125f));
                        if (step.sqrMagnitude < 1e-8f)
                        {
                            step = tips[k] - tips[k - 1];
                        }

                        if (step.sqrMagnitude > 1e-8f)
                        {
                            motion = step.normalized;
                        }

                        return;
                    }
                }
            }
        }

        private int Choose(in BlowRead blow, Vector3 land, Vector3 motion, out Vector3 local)
        {
            var back = Quaternion.Inverse(BodyTurn);
            local = back * (land - Root) / (BodyScale * size);
            Vector3 comes = back * -motion;
            // (The height where the blade first reaches the body.)
            float x = local.x, y = (landHeight - Root.y) / (BodyScale * size);
            // (Which side: a cut that comes in from one side is met on that side, wherever its point first touches;
            // one that comes straight in, by where it lands.)
            bool outside = comes.x > 0.25f ? true : comes.x < -0.25f ? false : x > 0.05f;
            if (landPart != 0 && y < 1.0f)
            {
                // (A leg first: its own side's low parry.)
                // (Below the knee, point down along it.)
                return landPart < 0 ? (y >= KneeLine ? 10 : 12) : (y >= KneeLine ? 7 : 13);
            }

            if (blow.Thrust)
            {
                outside = x > 0.05f;
                return y >= 1.45f ? (x < -0.1f ? 1 : 2) : y >= 0.95f ? (outside ? 3 : 4) : y >= KneeLine ? (outside ? 11 : 10) : (outside ? 13 : 12);
            }

            if (y >= 1.5f)
            {
                // (The head: from above, St George, or hanging when it comes from the inside; from the side, high tierce or high quarte.)
                if (comes.y > 0.45f)
                {
                    return comes.x < -0.3f ? 1 : 0;
                }

                return x >= 0f ? 2 : 5;
            }

            if (y >= 1.15f)
            {
                return outside ? (y >= 1.4f ? 2 : 3) : (y >= 1.35f && comes.y > 0.3f ? 6 : 4);
            }

            // (A rising cut at the flank or the belly: met point down, seconde outside, septime inside, as Hutton has it.)
            if (y < 1.15f && comes.y < -0.3f)
            {
                return outside ? 7 : 10;
            }

            if (y >= 0.88f)
            {
                return outside ? 8 : 9;
            }

            return y >= KneeLine ? (outside ? 7 : 10) : (outside ? 13 : 12);
        }

        /// <summary>Below this height a blow is met point down along the leg (metres at a Medium's size: about its knees).</summary>
        public const float KneeLine = 0.6f;

        /// <summary>
        /// One step, after the body is posed (the guard clip's arms on it): the trunk turned, and the sword arm set to
        /// the guard or the parry for the blow coming (<paramref name="blow"/>, null: none).
        /// </summary>
        private Vector3 landAt, landMotion;
        private float landHeight, feasible = 1f, pendingFor;
        private int pending = -1;

        /// <summary>How long a new read must hold before the parry changes to it (seconds).</summary>
        public const float ChangeAfter = 0.06f;

        /// <summary>How near the blow's landing the parry taken is no longer changed for another (seconds): too late to go round to it.</summary>
        public const float LockWithin = 0.15f;

        /// <summary>What the blow first meets: 0 the torso or head, -1 the left leg, +1 the right leg.</summary>
        private int landPart, landK = -1;
        private float landU;

        /// <summary>How far out from the body's middle the blades are to meet (metres at a Medium's size: a forearm and more, well clear of the head and the body).</summary>
        public static float CrossOut = 0.45f;

        /// <summary>The parries for each line a blow lands in (indices into <see cref="Parries"/>): the head's, the body's, the hips' and legs'.</summary>
        private static readonly int[] HighLine = { 0, 1, 2, 5, 6 }, MiddleLine = { 2, 3, 4, 5, 6, 7, 8, 9 }, LowLine = { 3, 4, 7, 8, 9, 10, 11, 12, 13 };

        /// <summary>The least way along the attacker's blade (grip to tip) a clean parry takes it.</summary>
        public static float TheirsFrom = 0.25f;

        /// <summary>The nearest the sword hand comes to its shoulder in a parry, of the arm's length (the elbow bent no more than about 110 degrees).</summary>
        public const float LeastReach = 0.6f;

        /// <summary>The furthest the grip goes from the shoulder in a parry, of the arm's length (shoulder to wrist): the elbow kept a little bent.</summary>
        public const float MostReach = 0.92f;

        /// <summary>How far before the face a parry at head height meets the blade (metres at a Medium's size).</summary>
        public static float HeadFront = 0.3f;

        /// <summary>
        /// Where up the blow's path (from where it would reach the body, back toward the attacker) it is first a
        /// <see cref="CrossOut"/> out from the body's middle: there the blades are to cross.
        /// </summary>
        private Vector3 CrossingOnPath(in BlowRead blow, Vector3 fallback)
        {
            if (blow.Path == null || landK < 1)
            {
                return fallback;
            }

            Vector3 pelvis = anim.Part(PartId.Pelvis).position, head = anim.Part(PartId.Head).position;
            float Out(Vector3 p)
            {
                Vector3 ab = head - pelvis;
                // (Above the head, out from the line up through it: a blow from above is met out in front, not over the crown.)
                float t = Mathf.Max(0f, Vector3.Dot(p - pelvis, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                return (pelvis + ab * t - p).magnitude;
            }

            Vector3 at = Vector3.Lerp(blow.Path[landK - 1], blow.Path[landK], landU);
            for (int k = landK; k >= 1; k--)
            {
                for (int j = 8; j >= 0; j--)
                {
                    float u = k == landK ? Mathf.Min(landU, j / 8f) : j / 8f;
                    Vector3 p = Vector3.Lerp(blow.Path[k - 1], blow.Path[k], u);
                    if (Out(p) >= CrossOut * size)
                    {
                        return p;
                    }

                    at = p;
                }
            }

            return at;
        }

        /// <summary>The other body as solid shapes (its torso, head, arms and shield), for keeping this sword out of it; null: none.</summary>
        public OpponentShape Opponent;

        /// <summary>How far the sword keeps from the other body and its shield (metres).</summary>
        public const float KeepClear = 0.04f;

        /// <summary>How far the guard keeps its sword from the other body and its shield (metres): room for the shield to move before it meets it.</summary>
        public const float GuardClear = 0.15f;

        /// <summary>How far the guard keeps its blade off the other's (metres).</summary>
        public const float GuardBladeClear = 0.05f;

        /// <summary>The most the sword is pushed off the other body in a step (degrees): a fast shield knocks it aside over a few steps, not in one.</summary>
        public const float MostPushStep = 25f;

        /// <summary>For the runner's comparisons: the keeping clear of the other body off.</summary>
        public static bool NoClear;

        private static string V(Vector3 v) => $"({v.x:0.00},{v.y:0.00},{v.z:0.00})";

        /// <summary>
        /// The sword arm was the fighter's own a while (a strike of its own, posed by its clip): the parry takes it up
        /// again from where the strike left it (the blade, the hand's turn, the trunk's), not from where it let go of it,
        /// which would whip the arm back there.
        /// </summary>
        public void Resync()
        {
            var hand = anim.Part(PartId.HandR);
            var slot = anim.SlotWeaponR;
            Vector3 grip = slot != null ? slot.position : hand.position;
            Vector3 blade = slot != null ? slot.rotation * Vector3.forward : hand.rotation * Vector3.forward;
            forte = grip + blade.normalized * (ForteAt * WeaponLength);
            handRot = heldHand = hand.rotation;
            turn = lean = 0f;
            lastForte = forte;
            lastForteKnown = true;
            lastUpperArm = Quaternion.Inverse(anim.Part(PartId.Chest).rotation) * (anim.Part(PartId.ForearmR).position - anim.Part(PartId.UpperArmR).position);
            lastSlot = slot != null ? slot.position : Vector3.zero;
            started = true;
            resyncElbow = handKnown;
            tucked = 0f;
        }

        public void Step(float dt, BlowRead? blow, Vector3 opponent)
        {
            var bodyTurn = BodyTurn;
            Position aim = Guard;
            Vector3 wantForte, wantBlade, wantEdge;
            float wantSwivel, wantTurn, wantLean;
            sinceBlow = blow.HasValue ? 0f : sinceBlow + dt;
            // (A new blow, a combo's next one too: read afresh, its parry chosen again.)
            if (blow.HasValue && blow.Value.Serial != lastSerial)
            {
                lastSerial = blow.Value.Serial;
                NewBlow();
            }

            lastComing = blow.HasValue ? blow.Value.Coming : -1f;
            // (Read from early, moved late: the parry starts out only so long before contact as it takes to get there,
            // as an expert's does. Up early, it would show the attacker where to cut instead. A novice only sees the blow
            // once the blade is well on its way.)
            float readAfter = Mathf.Lerp(ReadAfter, NoviceReadAfter, Lack);
            bool reading = blow.HasValue && blow.Value.Coming >= readAfter && blow.Value.ToContact <= Mathf.Lerp(MoveLead, NoviceMoveLead, Lack);
            if (reading || (Current >= 0 && sinceBlow <= HoldAfter))
            {
                if (reading)
                {
                    Landing(blow.Value, out landAt, out landMotion);
                    // (Where a less skilled eye reads it landing: off by its misread, and read again only now and then; and
                    // always again as a blow read ahead comes into view.)
                    rereadIn -= dt;
                    if (readAhead && !blow.Value.Predicted)
                    {
                        readHeld = false;
                    }

                    readAhead = blow.Value.Predicted;
                    if (readHeld && rereadIn > 0f)
                    {
                        landAt = heldLand;
                        landMotion = heldMotion;
                    }
                    else
                    {
                        landAt += misread;
                        heldLand = landAt;
                        heldMotion = landMotion;
                        readHeld = true;
                        rereadIn = Lack >= 0.999f ? float.MaxValue : NoviceRereadEvery * Lack;
                    }

                    int chosen = Choose(blow.Value, landAt, landMotion, out _);
                    if (Trace != null && takenFor == lastSerial && (blow.Value.Aim - tracedAim).magnitude > 0.15f)
                    {
                        Trace($"blow {lastSerial} read again: {blow.Value.ToContact:0.00} s to contact, aim {V(Quaternion.Inverse(bodyTurn) * (blow.Value.Aim - Root))}, would choose {Parries[chosen].Name}");
                    }

                    if (Trace != null)
                    {
                        if (takenFor != lastSerial || (blow.Value.Aim - tracedAim).magnitude > 0.15f)
                        {
                            tracedAim = blow.Value.Aim;
                        }
                    }

                    if (Trace != null && takenFor != lastSerial)
                    {
                        var back0 = Quaternion.Inverse(bodyTurn);
                        Trace($"blow {lastSerial}: {blow.Value.ToContact:0.00} s to contact, aim {V(back0 * (blow.Value.Aim - Root))}, land {V(back0 * (landAt - Root))}, chose {Parries[chosen].Name} (holding {(Current >= 0 ? Parries[Current].Name : "none")})");
                    }
                    // (Too late to change: the parry taken for this blow is held, only placed on the blade as it comes. The one
                    // still held from the last blow of a combo is no parry for this one: a combo's next blow comes into
                    // view only 0.15 to 0.3 s before its contact, and held so, the parry stayed high for a low cut.)
                    float toLand = landK >= 1 ? blow.Value.SecondsTo(landK - 1 + landU) : blow.Value.ToContact;
                    toLandNow = toLand;
                    bool takenForIt = Current >= 0 && takenFor == lastSerial;
                    // (Taken on the blow read ahead, the parry is looked at once more as the blow itself comes into view:
                    // the read ahead gives the line, not the place.)
                    bool firstSight = takenForIt && takenAhead && !blow.Value.Predicted;
                    if (takenForIt && toLand < LockWithin && !firstSight)
                    {
                        chosen = Current;
                    }
                    else if (wrongPick || !playsOn)
                    {
                        // (The wrong parry for this blow, taken and kept: a novice's guess, not played on against the blow.)
                        chosen = takenForIt ? Current : wrongPick ? WrongFor(chosen) : chosen;
                    }
                    else
                    {
                        // (Played on against the blow read: the parry that meets it on the blade, not the arm or the body.)
                        chosen = Pick(blow.Value, chosen, opponent, takenForIt);
                    }

                    // (A change of mind holds a moment before it is taken: the read wavers between two near parries. Not a
                    // new blow's first: that one is taken at once.)
                    if (takenForIt && chosen != Current && !firstSight)
                    {
                        pendingFor = chosen == pending ? pendingFor + dt : 0f;
                        pending = chosen;
                        if (pendingFor < ChangeAfter)
                        {
                            chosen = Current;
                        }
                    }

                    if (Trace != null && chosen != Current && blow.Value.Path != null)
                    {
                        var back = Quaternion.Inverse(bodyTurn);
                        var sb = new System.Text.StringBuilder("read: aim " + V(back * (blow.Value.Aim - Root)) + " land " + V(back * (landAt - Root)) + "; tip path");
                        foreach (var q in blow.Value.TipPath)
                        {
                            sb.Append(' ').Append(V(back * (q - Root)));
                        }

                        Trace(sb.ToString());
                    }

                    if (chosen != Current)
                    {
                        Made += (Made.Length > 0 ? ", " : "") + Parries[chosen].Name;
                    }

                    Current = chosen;
                    takenFor = lastSerial;
                    takenAhead = blow.Value.Predicted;
                }

                aim = Parries[Current];
            }
            else
            {
                Current = -1;
            }

            wantSwivel = aim.Swivel;
            wantTurn = aim.Turn;
            wantLean = aim.Lean;
            if (Current >= 0 && blow.HasValue)
            {
                Place(Current, blow.Value, opponent, out wantForte, out wantBlade, out wantEdge, dt);
                if (Trace != null)
                {
                    Vector3 hw = wantForte - wantBlade * (ForteAt * WeaponLength);
                    Trace($"placed: the hand {Vector3.Distance(hw, anim.Part(PartId.UpperArmR).position):0.00} m from the shoulder (arm {anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude:0.00}), the forte {V(Quaternion.Inverse(bodyTurn) * (wantForte - anim.Part(PartId.Head).position))} from the head");
                }
            }
            else
            {
                Vector3 hand = World(aim.Hand);
                wantBlade = (bodyTurn * aim.Blade).normalized;
                wantForte = hand + wantBlade * (ForteAt * WeaponLength);
                wantEdge = Vector3.ProjectOnPlane(bodyTurn * aim.Edge, wantBlade).normalized;
                if (Current < 0)
                {
                    // (The guard's point at the opponent's face.)
                    Vector3 toFace = opponent + Vector3.up * (0.2f * size) - hand;
                    if (toFace.sqrMagnitude > 0.01f)
                    {
                        wantBlade = Vector3.RotateTowards(wantBlade, toFace.normalized, 25f * Mathf.Deg2Rad, 0f);
                        wantForte = hand + wantBlade * (ForteAt * WeaponLength);
                        wantEdge = Vector3.ProjectOnPlane(Vector3.down, wantBlade).normalized;
                    }
                }

                KeepOffOpponent(opponent, ref wantForte, ref wantBlade, dt);
            }

            if (!started)
            {
                forte = wantForte;
                swivel = wantSwivel;
                turn = wantTurn;
                lean = wantLean;
                started = true;
            }

            // Each control closes on where it is going at a human's speed (an eased approach, capped).
            float close = 1f - Mathf.Exp(-Close * dt);
            // (A novice's hand is slower to get there.)
            float quick = Mathf.Lerp(1f, NoviceHandSpeed / HandSpeed, Lack);
            // (How late the hand will be: the time it still needs to get the forte there, against the time the blade gives it.)
            Late = Current >= 0 && blow.HasValue && toLandNow >= 0f
                ? (wantForte - forte).magnitude / Mathf.Max(0.1f, HandSpeed * quick * Pace * size) - toLandNow
                : 0f;
            toLandNow = -1f;
            if (!sweeping)
            {
                forte += Vector3.ClampMagnitude((wantForte - forte) * close, HandSpeed * quick * Pace * size * dt);
            }
            swivel += Mathf.Clamp(Mathf.DeltaAngle(swivel, wantSwivel) * close, -SwivelSpeed * quick * Pace * dt, SwivelSpeed * quick * Pace * dt);
            turn += Mathf.Clamp((wantTurn - turn) * close, -TrunkSpeed * dt, TrunkSpeed * dt);
            lean += Mathf.Clamp((wantLean - lean) * close, -TrunkSpeed * dt, TrunkSpeed * dt);

            // The trunk: the spine and the chest share the turn and the lean; the head kept on the opponent.
            Vector3 up = Vector3.up, right = bodyTurn * Vector3.right;
            var spine = anim.Part(PartId.Spine);
            var chest = anim.Part(PartId.Chest);
            var head = anim.Part(PartId.Head);
            Quaternion headWas = head.rotation;
            spine.rotation = Quaternion.AngleAxis(0.45f * turn, up) * Quaternion.AngleAxis(0.45f * lean, right) * spine.rotation;
            chest.rotation = Quaternion.AngleAxis(0.55f * turn, up) * Quaternion.AngleAxis(0.55f * lean, right) * chest.rotation;
            head.rotation = Quaternion.Slerp(head.rotation, headWas, 0.8f);

            TuckFreeArm(bodyTurn, dt);
            SolveArm(dt, close, wantBlade, wantEdge);
            PushedOffOpponent(opponent, dt);
        }

        /// <summary>A parry other than the right one, drawn for this blow: the neighbouring line most often.</summary>
        private int WrongFor(int right)
        {
            int n = Parries.Length;
            if (n < 2)
            {
                return right;
            }

            int pick = luck.Next(n - 1);
            return pick >= right ? pick + 1 : pick;
        }

        /// <summary>
        /// Where a parry puts the sword for the blow read (<see cref="landAt"/>, <see cref="landMotion"/>): the forte onto
        /// the incoming blade's path where it is still a forearm and more out from the body, the blade square to the
        /// plane that blade sweeps (its way and its motion), turned from the parry's own way as little as that needs,
        /// the edge to the blow; in front of the body, and never into the other body or its shield.
        /// </summary>
        private void Place(int parry, in BlowRead b, Vector3 opponent, out Vector3 wantForte, out Vector3 wantBlade, out Vector3 wantEdge, float dt = -1f)
        {
            var bodyTurn = BodyTurn;
            var aim = Parries[parry];
            Vector3 hand = World(aim.Hand);
            wantBlade = (bodyTurn * aim.Blade).normalized;
            wantForte = hand + wantBlade * (ForteAt * WeaponLength);
            wantEdge = Vector3.ProjectOnPlane(bodyTurn * aim.Edge, wantBlade).normalized;
            Vector3 motion = landMotion;
            Vector3 across = CrossingOnPath(b, landAt + motion * Mathf.Clamp(Vector3.Dot(wantForte - landAt, motion), -0.6f * size, -0.06f * size));
            // (Not played on against the blade itself: put where it was read to be, misread and all.)
            if (!playsOn)
            {
                across += misread;
            }

            wantForte += Vector3.ClampMagnitude(across - wantForte, PlaceMost * size);
            Vector3 square = Vector3.Cross(b.BladeWay, motion);
            if (square.sqrMagnitude > 1e-4f)
            {
                square.Normalize();
                if (Vector3.Dot(square, wantBlade) < 0f)
                {
                    square = -square;
                }

                // (Less of it while the arm can't take the turn: the parry's own way is one it can. A novice hardly
                // squares it at all: a shallow cross the blow glances along.)
                float squaring = Mathf.Lerp(1f, NoviceSquare, Lack);
                wantBlade = Vector3.RotateTowards(wantBlade, square, MostBladeOff * squaring * (parry == Current ? feasible : 1f) * Mathf.Deg2Rad, 0f);
            }

            // (A novice meets the blow out on its own foible: the whole blade drawn back along itself, the meeting further out.)
            wantForte -= wantBlade * (foibleBy * WeaponLength);

            Vector3 toBlow = Vector3.ProjectOnPlane(-motion, wantBlade);
            wantEdge = toBlow.sqrMagnitude > 1e-4f ? toBlow.normalized : Vector3.ProjectOnPlane(wantEdge, wantBlade).normalized;

            // (In front of the body, never through it; a blow at the head met well out before the face, where the
            // blade it stops is still clear of the head beyond the crossing.)
            Vector3 ahead = bodyTurn * Vector3.forward;
            float front = Vector3.Dot(wantForte - Root, ahead);
            if (front < 0.22f * size)
            {
                wantForte += ahead * (0.22f * size - front);
            }

            Vector3 headAt = anim.Part(PartId.Head).position;
            if (wantForte.y > headAt.y - 0.15f * size)
            {
                float beforeFace = Vector3.Dot(wantForte - headAt, ahead);
                if (beforeFace < HeadFront * size)
                {
                    wantForte += ahead * (HeadFront * size - beforeFace);
                }
            }

            // (The hand never drawn in so close to its shoulder that the elbow folds shut, nor out past the arm's reach.)
            {
                Vector3 shoulderAt = anim.Part(PartId.UpperArmR).position;
                float armLength = anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude;
                Vector3 handAt = wantForte - wantBlade * (ForteAt * WeaponLength);
                Vector3 fromShoulder = handAt - shoulderAt;
                float least = LeastReach * armLength, most = MostReach * armLength;
                if (fromShoulder.magnitude < least)
                {
                    Vector3 outWay = fromShoulder.sqrMagnitude > 1e-6f ? fromShoulder.normalized : ahead;
                    outWay = (outWay + ahead).normalized;
                    wantForte += outWay * (least - fromShoulder.magnitude);
                }
                else if (fromShoulder.magnitude > most)
                {
                    // (Nor out past the arm's reach: a hand asked further than the arm goes leaves the elbow straight,
                    // and a straight elbow has no way round it to keep.)
                    wantForte -= fromShoulder.normalized * (fromShoulder.magnitude - most);
                }
            }

            KeepOffOpponent(opponent, ref wantForte, ref wantBlade, dt);
        }

        /// <summary>
        /// The parry to take: the one held while it still meets the blow on the blade (or the blow misses); else the one
        /// read (<paramref name="chosen"/>) if it does; else whichever does, the nearest to where the sword is; else the one read.
        /// One held from the last blow (<paramref name="ownBlow"/> false: a combo's next) is kept only on the blade: the
        /// blow read missing it is no reason to stay in the last one's parry.
        /// </summary>
        private int Pick(in BlowRead b, int chosen, Vector3 opponent, bool ownBlow = true)
        {
            int Rank(Meeting m) => m == Meeting.Blade || m == Meeting.Nothing ? 0 : m == Meeting.Glance ? 1 : 2;
            Meeting Try(int p, in BlowRead read, out Vector3 at)
            {
                Place(p, read, opponent, out at, out Vector3 way, out _);
                return Meets(read, at, way, Parries[p].Swivel);
            }

            if (Current >= 0)
            {
                var held = Try(Current, b, out Vector3 heldAt);
                Trace?.Invoke($"pick: holding {Parries[Current].Name} meets {held} @{meetAlong:0.00}/{meetCross:0}, the forte {V(Quaternion.Inverse(BodyTurn) * (heldAt - anim.Part(PartId.Head).position))} from the head");
                if (held == Meeting.Blade || (ownBlow && held == Meeting.Nothing))
                {
                    return Current;
                }
            }

            var firstMeets = Try(chosen, b, out Vector3 firstAt);
            int firstRank = Rank(firstMeets);
            if (firstRank == 0)
            {
                Trace?.Invoke($"pick: read {Parries[chosen].Name} meets {firstMeets} @{meetAlong:0.00}/{meetCross:0}, the forte {V(Quaternion.Inverse(BodyTurn) * (firstAt - anim.Part(PartId.Head).position))} from the head");
                return chosen;
            }

            if (Trace != null)
            {
                var sb = new System.Text.StringBuilder("pick: read " + Parries[chosen].Name + " meets");
                for (int p = 0; p < Parries.Length; p++)
                {
                    var m = Try(p, b, out _);
                    sb.Append($" {Parries[p].Name}:{m}{(m == Meeting.Blade || m == Meeting.Glance ? $"@{meetAlong:0.00}/{meetCross:0}" : "")}");
                }

                Trace(sb.ToString());
            }

            int best = chosen, bestRank = firstRank;
            float bestCost = float.MaxValue;
            Vector3 from = started ? forte : World(Guard.Hand);
            // (Only the parries for the height the blow lands at: one from another line folds the arm up to get there.)
            float y = (landHeight - Root.y) / (BodyScale * size);
            int[] band = y >= 1.4f ? HighLine : y >= 1.0f ? MiddleLine : LowLine;
            for (int p = 0; p < Parries.Length; p++)
            {
                if (p == chosen || System.Array.IndexOf(band, p) < 0)
                {
                    continue;
                }

                int rank = Rank(Try(p, b, out Vector3 at));
                float cost = Vector3.Distance(at, from);
                if (rank < bestRank || (rank == bestRank && rank < firstRank && cost < bestCost))
                {
                    best = p;
                    bestRank = rank;
                    bestCost = cost;
                }
            }

            return best;
        }

        /// <summary>
        /// Where the sword as posed is in the other body or its shield (a shield swung into the guard's point, a body
        /// come too close), it is pushed out, as a blade is: turned at the wrist, the point up and away. The push is
        /// kept from step to step and eased off only as far as the sword stays clear, so the blade rides the shield
        /// smoothly rather than being snapped out and pulled back in each step.
        /// </summary>
        private void PushedOffOpponent(Vector3 opponent, float dt)
        {
            if (Opponent == null || NoClear || anim.SlotWeaponR == null)
            {
                pushAngle = 0f;
                return;
            }

            var hand = anim.Part(PartId.HandR);
            Vector3 toOpponent = opponent - Root;
            toOpponent.y = 0f;
            toOpponent = toOpponent.sqrMagnitude > 1e-4f ? toOpponent.normalized : BodyTurn * Vector3.forward;
            Vector3 away = (Vector3.up - 0.6f * toOpponent).normalized;
            Quaternion solvedHand = hand.rotation;
            bool Clear(float angle, float margin)
            {
                BladeNow(out Vector3 g0, out Vector3 t0);
                Vector3 way0 = (t0 - g0).normalized;
                var turn0 = Quaternion.FromToRotation(way0, Vector3.RotateTowards(way0, away, angle * Mathf.Deg2Rad, 0f));
                hand.rotation = turn0 * solvedHand;
                BladeNow(out Vector3 g, out Vector3 tp);
                return Opponent.Clearance(g, tp) >= margin;
            }

            // (Eased off at a wrist's pace while it stays clear; pushed further, at once, while it is in.)
            float eased = Mathf.Max(0f, pushAngle - PushEase * dt);
            if (pushAngle > 0f && Clear(eased, 0.01f * size))
            {
                pushAngle = eased;
            }

            float pushedFrom = pushAngle;
            for (int i = 0; i < 50 && pushAngle - pushedFrom < MostPushStep && !Clear(pushAngle, 0f); i++)
            {
                pushAngle += 1f;
                Pushed++;
            }

            // (Posed with the push; the arm's own turn, held for the next step, stays the solved one.)
            Clear(pushAngle, 0f);
            if (Trace != null && pushAngle > 0f)
            {
                Trace($"pushed {pushAngle:0} deg off the opponent ({CurrentName})");
            }
        }

        private float pushAngle;

        /// <summary>How fast a push off the other body eases off once the sword is clear (degrees a second).</summary>
        public const float PushEase = 60f;

        /// <summary>How many times the sword was pushed out of the other body or its shield (5 degrees each).</summary>
        public int Pushed;

        /// <summary>Never into the other body or its shield: the sword drawn back toward its own body until it is clear, its point raised a little, as a sword held off a body too close.</summary>
        private void KeepOffOpponent(Vector3 opponent, ref Vector3 wantForte, ref Vector3 wantBlade, float dt = -1f)
        {
            if (Opponent == null || NoClear)
            {
                return;
            }

            Vector3 back = Root - opponent;
            back.y = 0f;
            back = back.sqrMagnitude > 1e-4f ? back.normalized : -(BodyTurn * Vector3.forward);
            // (Out to the sword's side more than back: drawn straight back, the hand folds in behind the body.)
            Vector3 swordSide = Vector3.Cross(Vector3.up, -back).normalized;
            back = (0.75f * swordSide + 0.45f * back + 0.2f * Vector3.up).normalized;
            float margin = (Current < 0 ? GuardClear : KeepClear) * size;
            Vector3 forte0 = wantForte, blade0 = wantBlade;
            // (How far it is drawn back and raised, 0 to 1 of the most: the least that clears, found by halving, so a
            // body that moves a little moves the sword a little, never by a whole step at a time.)
            void At(float t, out Vector3 f, out Vector3 w)
            {
                f = forte0 + back * (t * MostDrawBack * size);
                w = Vector3.RotateTowards(blade0, Vector3.up, t * MostRaise * Mathf.Deg2Rad, 0f);
            }

            Vector3 elbowNow = anim.Part(PartId.ForearmR).position;
            bool ClearAt(float t)
            {
                At(t, out Vector3 f, out Vector3 w);
                Vector3 g = f - w * (ForteAt * WeaponLength);
                // (The sword and the forearm holding it: a shield pressed on the arm throws it about as on the blade.)
                float clear = Mathf.Min(Opponent.Clearance(g, g + w * WeaponLength), Opponent.Clearance(elbowNow, g) + 0.5f * margin);
                // (At rest in the guard, off the other's blade too: two guards' blades resting on each other buzz.)
                if (Current < 0)
                {
                    clear = Mathf.Min(clear, Opponent.WeaponClearance(g, g + w * WeaponLength) + (GuardClear - GuardBladeClear) * size);
                }

                return clear >= margin;
            }

            float need = 0f;
            if (!ClearAt(0f))
            {
                need = 1f;
                if (ClearAt(1f))
                {
                    float lo = 0f, hi = 1f;
                    for (int i = 0; i < 8; i++)
                    {
                        float mid = 0.5f * (lo + hi);
                        if (ClearAt(mid)) hi = mid; else lo = mid;
                    }

                    need = hi;
                }
            }

            // (The sword's own step: drawn off promptly, let back slowly, so the guard rests rather than hunts.)
            float t2 = need;
            if (dt >= 0f)
            {
                // (A parry lets it go as quickly as it takes it up: it has a blow to meet.)
                float fall = Current >= 0 ? DrawBackRise : DrawBackFall;
                drawnBack = need > drawnBack ? Mathf.MoveTowards(drawnBack, need, DrawBackRise * dt) : Mathf.MoveTowards(drawnBack, need, fall * dt);
                t2 = drawnBack;
            }

            At(t2, out wantForte, out wantBlade);
        }

        private float drawnBack;

        /// <summary>The most the sword is drawn off the other body, out to its side and back (metres at a Medium's size), and its point raised toward upright (degrees).</summary>
        public const float MostDrawBack = 0.4f, MostRaise = 80f;

        /// <summary>How fast the drawing back is taken up, and let go (of the most, a second).</summary>
        public const float DrawBackRise = 6f, DrawBackFall = 0.8f;

        /// <summary>What the blow read would meet first with the sword placed so (forte, way).</summary>
        private enum Meeting { Nothing, Body, Glance, Blade }

        /// <summary>Where on the sword (from the hilt, of its length) and at what crossing (degrees) a meeting found by <see cref="Meets"/> was.</summary>
        private float meetAlong, meetCross;

        /// <summary>
        /// The blow read played on against the sword held at a forte and a way (the arm out to it from the shoulder,
        /// bent at its elbow as the parry bends it): which the incoming blade meets first, the sword (on its forte,
        /// well crossed: a clean parry; else a glance) or the body (the sword arm, the head, the torso, the legs).
        /// </summary>
        private Meeting Meets(in BlowRead b, Vector3 atForte, Vector3 way, float swivelAt)
        {
            if (b.Path == null)
            {
                return Meeting.Nothing;
            }

            float L = WeaponLength, r = size;
            Vector3 hand = atForte - way * (ForteAt * L);
            Vector3 bladeFrom = hand + way * (0.05f * L), bladeTo = hand + way * L;
            Vector3 shoulder = anim.Part(PartId.UpperArmR).position;
            float upper = anim.RestLocalPosition(PartId.ForearmR).magnitude, fore = anim.RestLocalPosition(PartId.HandR).magnitude;
            Vector3 axis = hand - shoulder;
            float d = axis.magnitude;
            axis = d > 1e-4f ? axis / d : Vector3.forward;
            Vector3 down0 = Vector3.ProjectOnPlane(Vector3.down, axis);
            down0 = down0.sqrMagnitude > 1e-6f ? down0.normalized : Vector3.ProjectOnPlane(BodyTurn * Vector3.back, axis).normalized;
            // (The elbow off the shoulder-to-hand line by what the arm's bend makes of the distance, round the line by the parry's swivel.)
            float along = Mathf.Clamp((upper * upper - fore * fore + d * d) / Mathf.Max(1e-4f, 2f * d), 0f, upper);
            float off = Mathf.Sqrt(Mathf.Max(0f, upper * upper - along * along));
            Vector3 elbow = shoulder + axis * along + Quaternion.AngleAxis(swivelAt, axis) * down0 * off;
            var grips = b.GripPath ?? b.Path;
            var tips = b.TipPath ?? b.Path;
            const int Sub = 16;
            for (int k = 1; k < b.Path.Length; k++)
            {
                for (int j = 1; j <= Sub; j++)
                {
                    float u = j / (float)Sub;
                    Vector3 g = Vector3.Lerp(grips[k - 1], grips[k], u), tp = Vector3.Lerp(tips[k - 1], tips[k], u);
                    // (The body first: a blade already at the head or the arm when the swords meet has not been stopped in time.)
                    if (StrikeHits.SegmentsNearest(g, tp, shoulder, elbow, out _, out _, out _, out _) <= 0.06f * r
                        || StrikeHits.SegmentsNearest(g, tp, elbow, hand, out _, out _, out _, out _) <= 0.055f * r
                        || BodyTouches(g, tp))
                    {
                        return Meeting.Body;
                    }

                    if (StrikeHits.SegmentsNearest(g, tp, bladeFrom, bladeTo, out float onTheirs, out float s, out _, out _) <= 0.04f * r)
                    {
                        Vector3 theirs = tp - g;
                        meetAlong = 0.05f + 0.95f * s;
                        meetCross = Vector3.Angle(theirs, way);
                        meetCross = Mathf.Min(meetCross, 180f - meetCross);
                        // (A clean parry takes their blade on its middle or beyond: at its hilt it is a jam into the swing, not a parry.)
                        return meetAlong <= 0.6f && meetCross >= 40f && onTheirs >= TheirsFrom ? Meeting.Blade : Meeting.Glance;
                    }
                }
            }

            return Meeting.Nothing;
        }

        /// <summary>Whether a blade (grip to tip) is at the body as it stands now: its head, its torso, its legs.</summary>
        private bool BodyTouches(Vector3 g, Vector3 tp)
        {
            float r = size;
            Vector3 P(PartId id) => anim.Part(id).position;
            float Seg(Vector3 a2, Vector3 b2) => StrikeHits.SegmentsNearest(g, tp, a2, b2, out _, out _, out _, out _);
            Vector3 headAt = P(PartId.Head) + Vector3.up * (0.08f * r);
            if (Seg(headAt, headAt) <= 0.14f * r || Seg(P(PartId.Pelvis), P(PartId.Chest)) <= 0.17f * r || Seg(P(PartId.Chest), P(PartId.Head)) <= 0.15f * r)
            {
                return true;
            }

            for (int leg = 0; leg < 2; leg++)
            {
                if (Seg(P(leg == 0 ? PartId.ThighL : PartId.ThighR), P(leg == 0 ? PartId.ShinL : PartId.ShinR)) <= 0.1f * r
                    || Seg(P(leg == 0 ? PartId.ShinL : PartId.ShinR), P(leg == 0 ? PartId.FootL : PartId.FootR)) <= 0.08f * r)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Where the free (left) hand is kept: behind the left hip, the elbow out and back (the sabreur's off hand, out of the way of blows).</summary>
        public static readonly Vector3 FreeHand = new Vector3(-0.12f, 1.0f, -0.22f);

        /// <summary>How long the free arm takes to go back to its tuck after the fighter's own strike had it (seconds).</summary>
        public const float TuckBackOver = 0.3f;

        private float tucked = 1f;

        private void TuckFreeArm(Quaternion bodyTurn, float dt)
        {
            var upper = anim.Part(PartId.UpperArmL);
            var fore = anim.Part(PartId.ForearmL);
            var handL = anim.Part(PartId.HandL);
            var keep = handL.rotation;
            // (Eased back from where its own strike left it, not snapped there.)
            tucked = Mathf.MoveTowards(tucked, 1f, dt / TuckBackOver);
            float ease = tucked * tucked * (3f - 2f * tucked);
            Vector3 tuck = World(FreeHand);
            TwoBoneIK.Solve(upper, fore, handL, ease >= 1f ? tuck : Vector3.Lerp(handL.position, tuck, ease), bodyTurn * new Vector3(-1f, -0.6f, -0.5f));
            handL.rotation = keep;
        }

        private void SolveArm(float dt, float close, Vector3 wantBlade, Vector3 wantEdge)
        {
            var hand = anim.Part(PartId.HandR);
            var slot = anim.SlotWeaponR;
            if (slot == null)
            {
                return;
            }

            Quaternion slotInHand = Quaternion.Inverse(hand.rotation) * slot.rotation;
            Vector3 bladeInHand = slotInHand * Vector3.forward;
            Vector3 forteInHand = hand.InverseTransformPoint(slot.position + slot.rotation * Vector3.forward * (ForteAt * WeaponLength));
            var torso = rig != null ? new BodyClearance.Posed(rig, anim, size) : null;
            Quaternion Target(int sign) => Quaternion.LookRotation(wantBlade, wantEdge)
                * Quaternion.Inverse(Quaternion.LookRotation(bladeInHand, slotInHand * (sign > 0 ? Vector3.up : Vector3.down)));
            // (The grip is what is placed: where the hand goes is then smooth whatever the wrist can take of the turn;
            // a turn it can't take shows in the blade's angle, not as the hand swinging round the forte.)
            Vector3 gripInHand = hand.InverseTransformPoint(slot.position);
            ArmGoal Goal(Quaternion turnTo, float swivelAt, float weight) => new ArmGoal
            {
                point = forte - (turnTo * bladeInHand) * (ForteAt * WeaponLength),
                pointInHand = gripInHand,
                handTurn = turnTo,
                elbowSwivel = swivelAt,
                turnWeight = weight,
                nearNull = true,
                // (The hand may roll about the grip: which way the edge faces matters less than where the blade lies.)
                rollAxisInHand = bladeInHand,
                rollAllowance = RollAllowance,
                wristEase = WristEase,
                clearance = 0.04f * size,
            };

            // (Which of the sword's two edges meets the blow: chosen as each parry is taken up, whichever the arm takes
            // the better, then kept, so the hand never rolls over in the middle of a parry.)
            if (Current != lastChosen || !handKnown)
            {
                float up = ArmRig.Solve(anim, true, Goal(Target(1), swivel, 1f), torso, apply: false).wristExcess;
                float down = ArmRig.Solve(anim, true, Goal(Target(-1), swivel, 1f), torso, apply: false).wristExcess;
                edgeSign = up <= down ? 1 : -1;
                lastChosen = Current;
            }

            // The hand turns toward the parry's turn at a human's speed; the elbow goes round its circle toward where
            // the arm would put it for that turn, at most SwivelSpeed; the arm is then solved with the elbow there.
            Quaternion target = Target(edgeSign);
            if (!handKnown)
            {
                handRot = target;
            }

            // (From the turn the hand really has: asked no further ahead of it than a step's turn, so an ask the wrist
            // can't take never runs on ahead of the arm.)
            if (handKnown)
            {
                handRot = Quaternion.RotateTowards(heldHand, handRot, BladeTurnSpeed * Pace * dt);
            }

            handRot = Quaternion.RotateTowards(handRot, target, Mathf.Min(Quaternion.Angle(handRot, target) * close, BladeTurnSpeed * Pace * dt));
            var keep = ArmRig.Solve(anim, true, Goal(handRot, swivel, 1f), torso, apply: false);
            if (!handKnown || resyncElbow)
            {
                elbow = keep.swivel;
                handKnown = true;
                resyncElbow = false;
            }

            elbow += Mathf.Clamp(Mathf.DeltaAngle(elbow, keep.swivel) * close, -SwivelSpeed * Pace * dt, SwivelSpeed * Pace * dt);
            feasible = keep.wristExcess > 20f ? Mathf.Max(0f, feasible - 4f * dt) : Mathf.Min(1f, feasible + 2f * dt);
            var solved = ArmRig.Solve(anim, true, Goal(handRot, elbow, 0f), torso, apply: true);
            // (How far the elbow went round the shoulder this step: the upper arm's turn against the chest. More than a
            // shoulder turns (UpperArmSpeed), the step is taken only so far: the forte and the hand's turn held back.)
            var chestTurn = Quaternion.Inverse(anim.Part(PartId.Chest).rotation);
            float UpperTurn() => lastUpperArm == Vector3.zero ? 0f : Vector3.Angle(chestTurn * (anim.Part(PartId.ForearmR).position - anim.Part(PartId.UpperArmR).position), lastUpperArm);
            float jump = UpperTurn();
            float mostTurn = UpperArmSpeed * Pace * dt;
            for (int i = 0; i < 2 && jump > mostTurn && handKnown && lastForteKnown; i++)
            {
                float share = mostTurn / jump;
                forte = lastForte + (forte - lastForte) * share;
                handRot = Quaternion.Slerp(heldHand, handRot, share);
                solved = ArmRig.Solve(anim, true, Goal(handRot, elbow, 0f), torso, apply: true);
                jump = UpperTurn();
            }

            heldHand = hand.rotation;
            lastForte = forte;
            lastForteKnown = true;
            lastUpperArm = chestTurn * (anim.Part(PartId.ForearmR).position - anim.Part(PartId.UpperArmR).position);
            Vector3 slotNow = slot.position;
            if (lastSlot != Vector3.zero)
            {
                MostHandSpeed = Mathf.Max(MostHandSpeed, Vector3.Distance(slotNow, lastSlot) / Mathf.Max(1e-4f, dt));
                MostSwivelJump = Mathf.Max(MostSwivelJump, jump);
            }

            if (Trace != null && (Current >= 0 || jump > 12f))
            {
                Trace($"{CurrentName,-12} jump {jump,3:0} edge {edgeSign:+0;-0} null {swivel,5:0} wants {keep.swivel,5:0} elbow {elbow,5:0} solved {solved.swivel,5:0} short {solved.wristExcess,3:0} miss {solved.pointMiss:0.00} bend {solved.elbowBend,3:0} hand {Vector3.Distance(slotNow, lastSlot) / Mathf.Max(1e-4f, dt),5:0.0} m/s");
            }

            lastSlot = slotNow;
            lastSolvedSwivel = solved.swivel;
            MostWristShort = Mathf.Max(MostWristShort, solved.wristExcess);
            MostShoulderPast = Mathf.Max(MostShoulderPast, solved.shoulderPast);
            MostIntoTorso = Mathf.Max(MostIntoTorso, solved.clearanceShort);
        }
    }

    /// <summary>
    /// The other body, as a sword sees it: its torso, head, arms and legs as capsules, its shield as a disc (each
    /// step from its posed rig). <see cref="Clearance"/> is how far a blade is from all of it (less than 0: inside).
    /// </summary>
    public sealed class OpponentShape
    {
        private readonly AnimRig anim;
        private readonly Transform shield;
        private readonly float size, shieldRadius;

        public OpponentShape(AnimRig anim, float size, Transform shieldSlot = null, float shieldRadius = 0.26f)
        {
            this.anim = anim;
            this.size = size;
            shield = shieldSlot;
            this.shieldRadius = shieldRadius;
        }

        /// <summary>Its weapon (the slot, the blade's length in metres): kept off by a guard at rest, met by a parry.</summary>
        public Transform WeaponSlot;
        public float WeaponLength;

        /// <summary>How far a segment is from the other's weapon (metres; a large number with none).</summary>
        public float WeaponClearance(Vector3 p, Vector3 q)
        {
            if (WeaponSlot == null || WeaponLength <= 0f)
            {
                return float.MaxValue;
            }

            Vector3 g = WeaponSlot.position, t = g + WeaponSlot.rotation * Vector3.forward * WeaponLength;
            return StrikeHits.SegmentsNearest(p, q, g, t, out _, out _, out _, out _) - 0.02f * size;
        }

        private static readonly (PartId a, PartId b, float r)[] Capsules =
        {
            (PartId.Pelvis, PartId.Chest, 0.17f), (PartId.Chest, PartId.Head, 0.15f), (PartId.Head, PartId.Head, 0.13f),
            (PartId.UpperArmL, PartId.ForearmL, 0.06f), (PartId.ForearmL, PartId.HandL, 0.05f),
            (PartId.UpperArmR, PartId.ForearmR, 0.06f), (PartId.ForearmR, PartId.HandR, 0.05f),
            (PartId.ThighL, PartId.ShinL, 0.09f), (PartId.ShinL, PartId.FootL, 0.07f),
            (PartId.ThighR, PartId.ShinR, 0.09f), (PartId.ShinR, PartId.FootR, 0.07f),
        };

        public bool HasShield => shield != null;

        /// <summary>How far the segment from <paramref name="p"/> to <paramref name="q"/> is from the body and its shield (metres; less than 0: inside).</summary>
        public float Clearance(Vector3 p, Vector3 q)
        {
            float least = float.MaxValue;
            foreach (var c in Capsules)
            {
                Vector3 a = anim.Part(c.a).position, b = anim.Part(c.b).position;
                float d = StrikeHits.SegmentsNearest(p, q, a, b, out _, out _, out _, out _) - c.r * size;
                least = Mathf.Min(least, d);
            }

            if (shield != null)
            {
                // (The shield as a disc: its face the slot's forward, its boss a little in front of the fist.)
                Vector3 n = shield.rotation * Vector3.forward;
                Vector3 centre = shield.position + n * (0.075f * size);
                // (Where the segment passes the disc's plane, or its nearest point to the centre.)
                StrikeHits.SegmentsNearest(p, q, centre, centre, out float s, out _, out Vector3 near, out _);
                Vector3 off = near - centre;
                float across = Vector3.ProjectOnPlane(off, n).magnitude;
                float through = Mathf.Abs(Vector3.Dot(off, n));
                float d = across <= shieldRadius * size ? through - 0.03f * size : Mathf.Sqrt(through * through + (across - shieldRadius * size) * (across - shieldRadius * size));
                // (A segment crossing the plane inside the rim goes through it.)
                float dp = Vector3.Dot(p - centre, n), dq = Vector3.Dot(q - centre, n);
                if (dp * dq < 0f)
                {
                    Vector3 hit = p + (q - p) * (dp / (dp - dq));
                    if ((hit - centre).magnitude <= shieldRadius * size)
                    {
                        d = -0.01f;
                    }
                }

                least = Mathf.Min(least, d);
            }

            return least;
        }
    }
}
