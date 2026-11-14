using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The line a cut travels on, as the one who cuts sees it (Meyer's eight lines through one point): where the
    /// blade comes from. Right and left are the attacker's own.
    /// </summary>
    public enum CutLine
    {
        Down,
        DownFromRight,
        DownFromLeft,
        LevelFromRight,
        LevelFromLeft,
        UpFromRight,
        UpFromLeft,
        Up,
    }

    /// <summary>A cut's size: what leads it, how far the blade turns, how long it takes, how hard it lands (the plan "Aimed attacks", "The four sizes").</summary>
    public enum CutSize
    {
        /// <summary>From the wrist: the hand snaps the blade over, the forearm hardly swings. The quickest and the lightest.</summary>
        Wrist,

        /// <summary>From the elbow: the forearm swings the sword and the wrist finishes it. Most blows.</summary>
        Elbow,
    }

    /// <summary>
    /// A blow built in code for one part of one body as it stands now (the plan "Aimed attacks", steps 1 and 2: the
    /// blade, the arm and the trunk). Nothing is played: the blow is worked out backward from the blade meeting the part.
    ///
    /// <para><b>The arm is a hammer's.</b> The upper arm, the forearm and the blade swing in one plane, about the
    /// elbow's own hinge: the shoulder turns the upper arm in it, the elbow straightens, and the wrist snaps the blade
    /// over, edge first. Each joint stays inside a body's range because the blow is made of the joints' own turns, not
    /// of a path the arm then has to find a way to.</para>
    /// <para><b>At the target.</b> The arm rig is asked for the arm that lays the blade's striking part on the spot
    /// with the wrist snapped forward (<see cref="ArmRig.Solve"/> with a keyed wrist); round the line from the shoulder
    /// to the spot, the arm is turned to where its swing comes in along the line asked for.</para>
    /// <para><b>Wound up.</b> The same arm with each joint turned back: the elbow bent, the wrist cocked, the upper
    /// arm lifted back; less far if the blade would be wound into its own body.</para>
    /// <para><b>In time.</b> From wherever the sword is held the joints go to the wound-up arm, are still a moment,
    /// then go one after another: the shoulder first, the elbow after it, the wrist last. The elbow is at a steady
    /// speed for the last moment into the spot. Each joint carries on past the spot, slowing, and the arm goes back to
    /// where the sword was held.</para>
    ///
    /// <para><b>The trunk</b> (<see cref="UsesTrunk"/>) turns away as the blow is wound up and into it before the arm
    /// goes, so the sword shoulder is forward as the blade lands, and leans in as far as the reach needs. The arm is
    /// planned on the trunk as it will be at the spot, and carried by it: the trunk's turn is the first part of the cut.</para>
    ///
    /// The legs are left as they stand (the step and the full cuts are the plan's next steps).
    /// What the blade meets is judged as a strike clip's is (<see cref="StrikeHits"/>), with the blade live from the
    /// start of the cut to the end of the carry-through.
    /// </summary>
    public sealed class BuiltBlow
    {
        /// <summary>The aim test's blows are built ones (the scene's Combat Feel, "Aim Test Blows Are Built"); off, today's strike clips.</summary>
        public static bool On = true;

        /// <summary>
        /// How far each blow is pushed past the fencing book (the scene's Combat Feel, "Blows Are Pushed"; 0 the book's, 1
        /// pushed): wound further back, a stillness at the top, carried further through. The moment and the place it
        /// lands do not move with it.
        /// </summary>
        public static float Pushed = 1f;

        /// <summary>
        /// The trunk is in a built blow (the scene's Combat Feel, "Blows Use The Trunk"; the plan's step 2): it turns away
        /// as the blow is wound up and into it as it is cut, ahead of the arm, and leans as far as the reach needs. Off:
        /// the arm alone, the body as it stands.
        /// </summary>
        public static bool UsesTrunk = true;

        /// <summary>The trunk's turn into a blow (degrees to its left, the sword shoulder forward) and the most it leans into one for reach (degrees forward).</summary>
        public const float TrunkIn = 22f, LeastLean = 5f, MostLean = 22f;

        /// <summary>For the headless reports: each blow's plan as it begins.</summary>
        public static System.Action<string> Trace = System.Environment.GetEnvironmentVariable("BLOW_TRACE") != null ? (System.Action<string>)(l => System.Console.WriteLine("    blow " + l)) : null;

        /// <summary>A size's numbers at a push of 0, and what a full push adds (seconds; degrees).</summary>
        private struct Shape
        {
            public float WindUp, Hold, HoldPushed, Cut, Back;

            // How far each joint is turned back when wound up, and how far it carries on past the spot.
            public float Shoulder, ShoulderPushed, Elbow, ElbowPushed, ShoulderOn, ShoulderOnPushed, ElbowOn;

            // How much of the wrist's room each way is used: cocked back, at the spot, at the end.
            public float WristCocked, WristAtSpot, WristOn;

            // When in the cut each joint starts (a share of the cut's time): the shoulder first, the wrist last.
            public float ElbowFrom, WristFrom;
        }

        // (The times are the plan's starting values: a wrist cut lands in 0.2 to 0.3 s, an elbow cut in 0.3 to 0.45.)
        private static readonly Shape WristShape = new Shape
        {
            WindUp = 0.13f, Hold = 0.02f, HoldPushed = 0.02f, Cut = 0.10f, Back = 0.22f,
            Shoulder = 4f, ShoulderPushed = 4f, Elbow = 22f, ElbowPushed = 10f, ShoulderOn = 4f, ShoulderOnPushed = 3f, ElbowOn = 10f,
            WristCocked = 0.9f, WristAtSpot = 0.55f, WristOn = 0.95f,
            ElbowFrom = 0f, WristFrom = 0.15f,
        };

        private static readonly Shape ElbowShape = new Shape
        {
            WindUp = 0.19f, Hold = 0.03f, HoldPushed = 0.03f, Cut = 0.14f, Back = 0.30f,
            Shoulder = 12f, ShoulderPushed = 12f, Elbow = 70f, ElbowPushed = 20f, ShoulderOn = 12f, ShoulderOnPushed = 10f, ElbowOn = 25f,
            WristCocked = 0.85f, WristAtSpot = 0.5f, WristOn = 0.95f,
            ElbowFrom = 0.1f, WristFrom = 0.35f,
        };

        /// <summary>The last of the cut, into the spot, the elbow is at a steady speed for this long (seconds), so the edge arrives true.</summary>
        public const float SteadyFor = 0.05f;

        /// <summary>From this long before it lands nothing changes: the spot is where it was then (seconds).</summary>
        public const float NoReturn = 0.1f;

        /// <summary>The blade stays live this long into the way back (seconds): the body carries a blow out a little behind its pose.</summary>
        public const float LiveAfter = 0.06f;

        /// <summary>A joint carries on past the spot for no longer than this (seconds).</summary>
        public const float MostCarry = 0.16f;

        /// <summary>The elbow bends no further than this in a blow, and straightens no further than this (degrees).</summary>
        public const float MostBend = 138f, LeastBend = 6f;

        /// <summary>The wound-up blade is kept this far from its own body (metres at a Medium's size): wound less, if it has to be.</summary>
        public const float OwnClearance = 0.04f;

        public readonly CutLine Line;
        public readonly CutSize Size;

        /// <summary>Where the spot is now (world): read each step until the point of no return.</summary>
        public System.Func<Vector3> Spot;

        /// <summary>The blow in words ("an elbow cut down from the right").</summary>
        public string Name { get; }

        /// <summary>It is under way: from the moment it is begun to the end of its way back.</summary>
        public bool Playing { get; private set; }

        /// <summary>Its own time (seconds; it stops with the blade and runs back as a stopped blade backs out), and how fast that runs (1 as it passes, 0 held).</summary>
        public float Time { get; private set; }
        public float Speed = 1f;

        /// <summary>Its moments, in its own time: the cut begins, the blade is on the spot, the carry-through ends.</summary>
        public float CutFrom { get; private set; }
        public float ContactAt { get; private set; }
        public float CarriedAt { get; private set; }

        /// <summary>On its way back to where the sword was held.</summary>
        public bool ComingBack => comingBack;

        /// <summary>The blade can strike now: from the start of the cut to the end of the carry-through and a moment into the way back; not once it was stopped.</summary>
        public bool Live => Playing && !rewinding && !wasStopped && Time >= CutFrom && (!comingBack || backTime <= LiveAfter);

        /// <summary>How far short of the spot the blade's striking part falls (metres; 0: it reaches).</summary>
        public float Short { get; private set; }

        /// <summary>How far the blade turns in the cut as planned (degrees): the shoulder's, the elbow's and the wrist's turns together.</summary>
        public float Wound { get; private set; }

        /// <summary>The blade's turning as it meets the spot (degrees a second), and its striking part's speed then (m/s).</summary>
        public float TurnSpeed { get; private set; }
        public float StrikeSpeed { get; private set; }

        /// <summary>How far the way the blade comes in is off the line asked for (degrees, seen along the reach): the arm could not be turned to it.</summary>
        public float LineOff { get; private set; }

        /// <summary>How far the edge is turned off the way the blade is going as it meets the spot, as planned (degrees).</summary>
        public float EdgeOff { get; private set; }

        /// <summary>How far the arm is past what a body's arm does as the blade meets the spot, as planned (degrees; 0: a body can stand so).</summary>
        public float PastAtSpot { get; private set; }

        /// <summary>What the arm made of it, at the worst over the blow: the wrist past its range (degrees), the blade into its own body (metres).</summary>
        public float MostWristShort { get; private set; }
        public float MostBladeInBody { get; private set; }

        private readonly AnimRig anim;
        private readonly UnitRig rig;
        private readonly Locomotor loco;
        private readonly LocoBody poser;
        private readonly float size;
        private readonly Shape shape;
        private readonly float strikeAlong, bladeLength;

        /// <summary>An arm: the upper arm's turn against the chest, the forearm's on it and the hand's on the forearm.</summary>
        private struct Arm
        {
            public Quaternion Upper, Fore, Hand;
        }

        // The arm's own measures (the same for every blow of this arm).
        private Transform upperPart, forePart, handPart, chestPart, spinePart, headPart;

        // The trunk as the body's own pose has it this step, before the blow turns it.
        private Quaternion spineBase, chestLocalBase, headLocalBase, headBase;

        // The trunk's turn (degrees, over 0 to its right) and lean (over 0 forward): wound up, at the spot, at the end; now; and where the way back began.
        private float turnBack, turnAtSpot, turnOn, leanBack, leanAtSpot, leanOn, turnNow, leanNow, turnFrom, leanFrom;
        private Run turnRun, leanRun;

        /// <summary>How far the trunk turns in the cut, from wound up to the spot (degrees), and how far it leans in at the spot.</summary>
        public float TrunkTurn => Mathf.Abs(turnBack - turnAtSpot);
        public float TrunkLean => leanAtSpot;
        private Quaternion slotInHand, foreRest, handRest, wristFrame;
        private Vector3 bladeInHand, gripInHand, strikeInHand, elbowOff, wristOff, elbowAxis, snapAxis;
        private float wristTwist, cockMost, snapMost;

        // Where the sword was held when the blow began: the arm against the chest.
        private Arm held;

        // The plan: the spot, the arm as the blade meets it, and how far each joint is turned back and carried on.
        private Vector3 spot;
        private Arm atSpot;
        private Vector3 hinge;
        private float swivel, bendAtSpot;
        private float shoulderBack, elbowBack, shoulderOn, elbowOn, wristCocked, wristAtSpot, wristOn;
        private int edgeSign = 1;

        // Stopped: backing out, then the way back from where it is.
        private bool rewinding, comingBack, wasStopped;
        private float rewindFloor, rewindSpeed, backTime;
        private Arm backFrom;

        // The hand thrown by what stopped the blade, and held on a blade it is bound to.
        private Vector3 knock, knockSpeed, bindOffset, bindNow;
        private bool bound;

        public BuiltBlow(ClipFighter fighter, CutLine line, CutSize cutSize, System.Func<Vector3> spot)
        {
            anim = fighter.Anim;
            rig = fighter.Rig;
            loco = fighter.Loco;
            poser = fighter.Poser;
            size = fighter.Size;
            Line = line;
            Size = cutSize;
            Spot = spot;
            shape = cutSize == CutSize.Wrist ? WristShape : ElbowShape;
            Name = $"{(cutSize == CutSize.Wrist ? "a wrist cut" : "an elbow cut")} {LineName(line)}";
            var weapon = fighter.Strikes != null ? fighter.Strikes.Weapon : null;
            bladeLength = weapon != null ? weapon.tip.magnitude * size : poser.WeaponLength;
            strikeAlong = weapon != null ? weapon.StrikePoint(StrikeWith.Edge).magnitude * size : poser.WeaponLength * poser.SweetSpot;
        }

        /// <summary>The line in words, as the logs say it.</summary>
        public static string LineName(CutLine line)
        {
            switch (line)
            {
                case CutLine.Down: return "straight down";
                case CutLine.DownFromRight: return "down from the right";
                case CutLine.DownFromLeft: return "down from the left";
                case CutLine.LevelFromRight: return "level from the right";
                case CutLine.LevelFromLeft: return "level from the left";
                case CutLine.UpFromRight: return "up from the right";
                case CutLine.UpFromLeft: return "up from the left";
                default: return "straight up";
            }
        }

        /// <summary>
        /// The lines a spot may be cut on, the likeliest first, and the size, when none is chosen by hand. A first, simple
        /// choice (the plan's step 5 chooses in earnest, against what is in the way): a part on the attacker's right is
        /// cut from the right; the small, near parts with the wrist. Of these the one the arm can make best from where
        /// it stands is taken (<see cref="Badness"/>).
        /// </summary>
        public static CutLine[] Choices(ClipFighter attacker, ClipFighter target, AimSpot at, out CutSize cutSize)
        {
            Vector3 point = AimSpots.Point(target, at);
            Vector3 shoulder = attacker.Anim.Part(PartId.UpperArmR).position;
            Vector3 to = point - shoulder;
            Vector3 right = Vector3.Cross(Vector3.up, to);
            // (Which side of its own middle the part is on, as the attacker sees it.)
            Vector3 middle = target.Rig.Part(PartId.Chest).transform.position;
            bool onRight = Vector3.Dot(point - middle, right) >= 0f;
            CutLine down = onRight ? CutLine.DownFromRight : CutLine.DownFromLeft, downOther = onRight ? CutLine.DownFromLeft : CutLine.DownFromRight;
            CutLine level = onRight ? CutLine.LevelFromRight : CutLine.LevelFromLeft, up = onRight ? CutLine.UpFromRight : CutLine.UpFromLeft;
            cutSize = CutSize.Elbow;
            switch (at)
            {
                case AimSpot.Head:
                    return new[] { CutLine.Down, CutLine.DownFromRight, CutLine.DownFromLeft };
                case AimSpot.Neck:
                    return new[] { CutLine.DownFromRight, CutLine.DownFromLeft, CutLine.LevelFromRight, CutLine.LevelFromLeft };
                case AimSpot.Chest:
                case AimSpot.Hips:
                    // (Not straight down: a blade coming down on the chest meets the head first.)
                    return new[] { CutLine.DownFromRight, CutLine.DownFromLeft };
                case AimSpot.Belly:
                    return new[] { CutLine.DownFromRight, CutLine.DownFromLeft, CutLine.LevelFromLeft, CutLine.LevelFromRight };
                case AimSpot.ShoulderL:
                case AimSpot.ShoulderR:
                    return new[] { down, CutLine.Down, downOther };
                case AimSpot.UpperArmL:
                case AimSpot.UpperArmR:
                    return new[] { level, down, CutLine.Down };
                case AimSpot.ForearmL:
                case AimSpot.ForearmR:
                    cutSize = CutSize.Wrist;
                    return new[] { down, CutLine.Down, up, downOther };
                case AimSpot.HandL:
                case AimSpot.HandR:
                    cutSize = CutSize.Wrist;
                    return new[] { CutLine.Down, down, downOther };
                case AimSpot.ThighL:
                case AimSpot.ThighR:
                    return new[] { down, level, downOther };
                default:
                    return new[] { level, down, downOther };
            }
        }

        /// <summary>
        /// How badly the arm makes this blow from where it stands (0: as asked): how far the blade comes in off the line
        /// (degrees), how far the arm is past a body's range at the spot (three for each degree), how far it falls short
        /// (three for each centimetre), and a cut it has not the room to wind up half way.
        /// </summary>
        public float Badness => LineOff + 3f * PastAtSpot + 300f * Short + (Wound < 0.5f * FullWind ? 25f : 0f);

        /// <summary>How far the blade would turn in the cut with all the room it wants (degrees).</summary>
        public float FullWind { get; private set; }

        private float Push => Mathf.Clamp01(Pushed);

        private float Hold => shape.Hold + shape.HoldPushed * Push;

        /// <summary>
        /// Begun: the sword's hold now is where it starts from and comes back to, and the blow is planned for the spot
        /// as it is. False (and nothing begun) with no sword hand or no sword in it.
        /// </summary>
        public bool Begin()
        {
            if (!Prepare())
            {
                return false;
            }

            Time = 0f;
            Speed = 1f;
            rewinding = comingBack = wasStopped = false;
            knock = knockSpeed = bindOffset = bindNow = Vector3.zero;
            bound = false;
            MostWristShort = MostBladeInBody = 0f;
            Playing = true;
            return true;
        }

        private bool prepared;

        /// <summary>
        /// Planned for the spot as it is and the sword as it is held, without beginning: how well the arm can make it
        /// from here is then known (<see cref="Badness"/>, <see cref="Short"/>, <see cref="LineOff"/>). The arm is left
        /// as it was. False with no sword hand or no sword in it.
        /// </summary>
        public bool Prepare()
        {
            if (prepared)
            {
                return true;
            }

            upperPart = anim.Part(PartId.UpperArmR);
            forePart = anim.Part(PartId.ForearmR);
            handPart = anim.Part(PartId.HandR);
            chestPart = anim.Part(PartId.Chest);
            spinePart = anim.Part(PartId.Spine);
            headPart = anim.Part(PartId.Head);
            var slot = anim.SlotWeaponR;
            if (upperPart == null || forePart == null || handPart == null || chestPart == null || spinePart == null || headPart == null || slot == null || Spot == null)
            {
                return false;
            }

            // The arm's own measures: its bones, the sword in the hand.
            foreRest = anim.RestLocalRotation(PartId.ForearmR);
            handRest = anim.RestLocalRotation(PartId.HandR);
            wristFrame = ArmRig.WristFrame(anim, true);
            elbowAxis = ArmRig.ElbowAxis(anim, true);
            elbowOff = Vector3.Scale(anim.RestLocalPosition(PartId.ForearmR), upperPart.lossyScale);
            wristOff = Vector3.Scale(anim.RestLocalPosition(PartId.HandR), forePart.lossyScale);
            slotInHand = Quaternion.Inverse(handPart.rotation) * slot.rotation;
            bladeInHand = slotInHand * Vector3.forward;
            gripInHand = Quaternion.Inverse(handPart.rotation) * (slot.position - handPart.position);
            strikeInHand = gripInHand + bladeInHand * strikeAlong;
            MeasureWrist();

            held = new Arm
            {
                Upper = Quaternion.Inverse(chestPart.rotation) * upperPart.rotation,
                Fore = forePart.localRotation,
                Hand = handPart.localRotation,
            };

            spot = Spot();
            shoulderBack = shape.Shoulder + shape.ShoulderPushed * Push;
            elbowBack = shape.Elbow + shape.ElbowPushed * Push;
            shoulderOn = shape.ShoulderOn + shape.ShoulderOnPushed * Push;
            elbowOn = shape.ElbowOn;
            wristCocked = shape.WristCocked * cockMost;
            wristAtSpot = -shape.WristAtSpot * snapMost;
            wristOn = -shape.WristOn * snapMost;
            CaptureTrunk();
            PlanTrunk();
            var trunkWas = SaveTrunk();
            TrunkTo(turnAtSpot, leanAtSpot);
            FindSwivel();
            Measure(PlanAtSpot());
            // (Leaning in as far as the reach needs, and no further.)
            for (int i = 0; i < 6 && UsesTrunk && Short > 0.01f && leanAtSpot < MostLean - 0.5f; i++)
            {
                leanAtSpot = Mathf.Min(MostLean, leanAtSpot + 3.5f);
                leanOn = leanAtSpot + 3f;
                TrunkTo(turnAtSpot, leanAtSpot);
                Measure(PlanAtSpot());
            }

            FullWind = TrunkTurn + shoulderBack + (Mathf.Clamp(bendAtSpot + elbowBack, LeastBend, MostBend) - bendAtSpot) + (wristCocked - wristAtSpot);
            FitToOwnBody();
            TrunkTo(turnAtSpot, leanAtSpot);
            SetTimes();
            RestoreTrunk(trunkWas);
            prepared = true;

            if (Trace != null)
            {
                Vector3 along = wristOff.normalized;
                string lies = "";
                foreach (float turn in new[] { cockMost, 0f, -snapMost })
                {
                    Vector3 blade = WristAt(turn) * bladeInHand;
                    lies += $" {turn:0}: {Vector3.Angle(along, blade):0} from the forearm, {90f - Vector3.Angle(Quaternion.Inverse(foreRest) * elbowAxis, blade):0} out of the swing;";
                }

                SwordOf(atSpot, out Vector3 g0, out _);
                Trace($"the blade against the forearm, by the wrist's turn:{lies} the grip {Vector3.Distance(upperPart.position, g0):0.00} m from the shoulder, the spot {Vector3.Distance(upperPart.position, spot):0.00} m, bones {elbowOff.magnitude:0.00} and {wristOff.magnitude:0.00}, grip in hand {gripInHand.magnitude:0.00}, striking part {strikeAlong:0.00} along the blade");
            }

            Trace?.Invoke($"{Name}: wrist twist {wristTwist:0}, room {cockMost:0} back and {snapMost:0} on; swivel {swivel:0}, elbow {bendAtSpot:0} at the spot, back {elbowRun.Back:0} on {elbowRun.On:0}; shoulder back {shoulderBack:0}; wrist {wristCocked:0} to {wristAtSpot:0} to {wristOn:0}; wound {Wound:0}, {TurnSpeed:0} deg/s, {StrikeSpeed:0.0} m/s; trunk turn {turnBack:0} to {turnAtSpot:0} to {turnOn:0}, lean {leanBack:0} to {leanAtSpot:0}; line off {LineOff:0}, edge off {EdgeOff:0}, past range {PastAtSpot:0} at the spot, short {100f * Short:0} cm; cut from {CutFrom:0.00}, at the spot {ContactAt:0.00}, carried {CarriedAt:0.00}");
            return true;
        }

        // ---- The trunk -----------------------------------------------------------------------------------------------

        /// <summary>The trunk as the body's own pose has it now: what the blow's turn and lean are laid over this step.</summary>
        private void CaptureTrunk()
        {
            spineBase = spinePart.rotation;
            chestLocalBase = chestPart.localRotation;
            headLocalBase = headPart.localRotation;
            headBase = headPart.rotation;
        }

        private (Quaternion spine, Quaternion chest, Quaternion head) SaveTrunk() => (spinePart.rotation, chestPart.localRotation, headPart.localRotation);

        private void RestoreTrunk((Quaternion spine, Quaternion chest, Quaternion head) was)
        {
            spinePart.rotation = was.spine;
            chestPart.localRotation = was.chest;
            headPart.localRotation = was.head;
        }

        /// <summary>
        /// The trunk turned and leant so from how the body's own pose has it: the spine and the chest share the turn and
        /// the lean, the head kept on the opponent (as the sword parry's trunk is).
        /// </summary>
        private void TrunkTo(float turn, float lean)
        {
            Vector3 right = Quaternion.Euler(0f, loco.Facing, 0f) * Vector3.right;
            spinePart.rotation = Quaternion.AngleAxis(0.45f * turn, Vector3.up) * Quaternion.AngleAxis(0.45f * lean, right) * spineBase;
            chestPart.localRotation = chestLocalBase;
            chestPart.rotation = Quaternion.AngleAxis(0.55f * turn, Vector3.up) * Quaternion.AngleAxis(0.55f * lean, right) * chestPart.rotation;
            headPart.localRotation = headLocalBase;
            headPart.rotation = Quaternion.Slerp(headPart.rotation, headBase, 0.8f);
        }

        /// <summary>
        /// How far the trunk turns and leans: turned away as the blow is wound up (to its right for a blow from the
        /// right, further to its left for one from the left), turned in at the spot with the sword shoulder forward,
        /// and on past it; a wrist cut has half of it.
        /// </summary>
        private void PlanTrunk()
        {
            turnBack = turnAtSpot = turnOn = leanBack = leanAtSpot = leanOn = 0f;
            if (!UsesTrunk)
            {
                return;
            }

            float share = Size == CutSize.Wrist ? 0.5f : 1f;
            // (Which side the blow comes from, against the body: across the chest.)
            float side = Vector3.Dot(ComesFrom(out _), chestPart.right);
            turnAtSpot = -TrunkIn * share;
            if (side > 0.25f)
            {
                turnBack = (12f + 8f * Push) * share;
                turnOn = turnAtSpot - 10f * share;
            }
            else if (side < -0.25f)
            {
                turnBack = turnAtSpot - (14f + 6f * Push) * share;
                turnOn = turnAtSpot + 12f * share;
            }
            else
            {
                turnBack = (6f + 4f * Push) * share;
                turnOn = turnAtSpot - 6f * share;
            }

            leanBack = -(3f + 2f * Push) * share;
            leanAtSpot = LeastLean * share;
            leanOn = leanAtSpot + 3f;
        }

        /// <summary>The trunk's turn and lean at a moment of the blow's own time, before it is on its way back.</summary>
        private void TrunkAt(float t, out float turn, out float lean)
        {
            if (t < shape.WindUp)
            {
                float u = Mathf.Clamp01(t / shape.WindUp);
                float w = u * u * (3f - 2f * u);
                turn = turnBack * w;
                lean = leanBack * w;
                return;
            }

            turn = turnRun.At(t, ContactAt);
            lean = leanRun.At(t, ContactAt);
        }

        // ---- The wrist ------------------------------------------------------------------------------------------------

        /// <summary>The hand's turn on the forearm with the wrist turned so far about the swing's axis (degrees; over 0 cocked back, under 0 snapped forward).</summary>
        private Quaternion WristAt(float turn) =>
            handRest * (wristFrame * (Quaternion.AngleAxis(turn, snapAxis) * Quaternion.AngleAxis(wristTwist, Vector3.right)) * Quaternion.Inverse(wristFrame));

        /// <summary>
        /// How this hand holds this sword against its own forearm: the wrist's axis that turns the blade in the plane
        /// the elbow swings it in, the forearm's twist that lays the blade's flat across that plane (so the edge
        /// leads), and how far the wrist turns each way about that axis inside its range.
        /// </summary>
        private void MeasureWrist()
        {
            // (The elbow's hinge as the wrist's own frame sees it; the wrist's swing has no part along the forearm.)
            Quaternion toWrist = Quaternion.Inverse(handRest * wristFrame);
            Vector3 hingeInWrist = toWrist * (Quaternion.Inverse(foreRest) * elbowAxis);
            snapAxis = new Vector3(0f, hingeInWrist.y, hingeInWrist.z);
            snapAxis = snapAxis.sqrMagnitude > 1e-6f ? snapAxis.normalized : Vector3.forward;
            // (The blade's flat, turned about the forearm until it lies across the swing: either face will do, the
            // nearer one is taken.)
            Vector3 flat = Quaternion.Inverse(wristFrame) * (slotInHand * Vector3.right);
            flat = new Vector3(0f, flat.y, flat.z);
            wristTwist = 0f;
            if (flat.sqrMagnitude > 1e-6f)
            {
                float a = Vector3.SignedAngle(flat, snapAxis, Vector3.right), b = Vector3.SignedAngle(flat, -snapAxis, Vector3.right);
                wristTwist = Mathf.Clamp(Mathf.Abs(a) <= Mathf.Abs(b) ? a : b, -ArmRig.TwistLimit + 5f, ArmRig.TwistLimit - 5f);
            }

            float Room(int way)
            {
                float most = 0f;
                for (float turn = 2.5f; turn <= 70f; turn += 2.5f)
                {
                    var local = wristFrame * (Quaternion.AngleAxis(way * turn, snapAxis) * Quaternion.AngleAxis(wristTwist, Vector3.right)) * Quaternion.Inverse(wristFrame);
                    if (ArmRig.WristAngles(local, wristFrame, out _, out _, out _) > 0.5f)
                    {
                        break;
                    }

                    most = turn;
                }

                return most;
            }

            cockMost = Room(1);
            snapMost = Room(-1);
        }

        // ---- The arm, worked out without posing it ------------------------------------------------------------------

        private Quaternion ForeAt(float bend) => foreRest * Quaternion.AngleAxis(bend, elbowAxis);

        /// <summary>Where an arm puts the sword (world), the shoulder where it is now: the grip and the hand's turn.</summary>
        private void SwordOf(in Arm arm, out Vector3 grip, out Quaternion hand)
        {
            Quaternion upper = chestPart.rotation * arm.Upper;
            Vector3 elbow = upperPart.position + upper * elbowOff;
            Quaternion fore = upper * arm.Fore;
            hand = fore * arm.Hand;
            grip = elbow + fore * wristOff + hand * gripInHand;
        }

        /// <summary>How far an arm is past what a body's arm does (degrees; 0: inside), as the game's own gate reads it (<see cref="ArmLimits.Read"/>).</summary>
        private float Past(in Arm arm)
        {
            Quaternion upper = chestPart.rotation * arm.Upper;
            Vector3 shoulder = upperPart.position, elbow = shoulder + upper * elbowOff;
            Quaternion fore = upper * arm.Fore;
            return ArmLimits.Read(chestPart.rotation, shoulder, elbow, elbow + fore * wristOff, fore * arm.Hand, false).Worst;
        }

        private Vector3 StrikeOf(in Arm arm)
        {
            SwordOf(arm, out Vector3 grip, out Quaternion hand);
            return grip + hand * bladeInHand * strikeAlong;
        }

        /// <summary>The arm with each joint turned from where it is as the blade meets the spot: the shoulder and the elbow by so many degrees back (over 0) or on (under 0), the wrist at its own turn.</summary>
        private Arm Turned(float shoulder, float elbow, float wrist) => new Arm
        {
            Upper = Quaternion.AngleAxis(shoulder, hinge) * atSpot.Upper,
            Fore = ForeAt(Mathf.Clamp(bendAtSpot + elbow, LeastBend, MostBend)),
            Hand = WristAt(wrist),
        };

        // ---- The plan --------------------------------------------------------------------------------------------------

        /// <summary>
        /// The arm as the blade meets the spot, for where the spot and the shoulder are now: the arm rig's, with the
        /// wrist keyed and the elbow at the swivel found for the line. The arm is left as it was.
        /// </summary>
        private ArmSolve PlanAtSpot() => PlanAtSpot(swivel, out atSpot, out hinge, out bendAtSpot);

        private ArmSolve PlanAtSpot(float swivelAt, out Arm arm, out Vector3 hingeAt, out float bend)
        {
            // (With the trunk as it will be when the blade meets the spot: the shoulder is then where the trunk has brought it.)
            var trunkWas = SaveTrunk();
            TrunkTo(turnAtSpot, leanAtSpot);
            Quaternion upperWas = upperPart.rotation, foreWas = forePart.localRotation, handWas = handPart.localRotation;
            var goal = new ArmGoal
            {
                point = spot,
                pointInHand = strikeInHand,
                wristGiven = true,
                wristLocal = Quaternion.Inverse(handRest) * WristAt(wristAtSpot),
                elbowSwivel = swivelAt,
                clearance = 0.04f * size,
            };
            var solved = ArmRig.Solve(anim, true, goal, rig != null ? new BodyClearance.Posed(rig, anim, size) : null, apply: true);
            arm = new Arm { Upper = Quaternion.Inverse(chestPart.rotation) * upperPart.rotation, Fore = forePart.localRotation, Hand = handPart.localRotation };
            hingeAt = arm.Upper * elbowAxis;
            // (The elbow's bend as the forearm has it: its turn from rest about the hinge.)
            (Quaternion.Inverse(foreRest) * arm.Fore).ToAngleAxis(out float angle, out Vector3 axis);
            bend = Mathf.DeltaAngle(0f, angle) * (Vector3.Dot(axis, elbowAxis) >= 0f ? 1f : -1f);
            upperPart.rotation = upperWas;
            forePart.localRotation = foreWas;
            handPart.localRotation = handWas;
            RestoreTrunk(trunkWas);
            return solved;
        }

        /// <summary>The way the line comes from, seen along the reach to the spot (world, unit, across the reach).</summary>
        private Vector3 ComesFrom(out Vector3 reachWay)
        {
            Vector3 to = spot - upperPart.position;
            reachWay = to.sqrMagnitude > 1e-6f ? to.normalized : chestPart.forward;
            Vector3 right = Vector3.Cross(Vector3.up, reachWay);
            right = right.sqrMagnitude > 1e-6f ? right.normalized : chestPart.right;
            Vector3 up = Vector3.Cross(reachWay, right);
            switch (Line)
            {
                case CutLine.Down: return up;
                case CutLine.DownFromRight: return (up + right).normalized;
                case CutLine.DownFromLeft: return (up - right).normalized;
                case CutLine.LevelFromRight: return right;
                case CutLine.LevelFromLeft: return -right;
                case CutLine.UpFromRight: return (right - up).normalized;
                case CutLine.UpFromLeft: return (-right - up).normalized;
                default: return -up;
            }
        }

        /// <summary>
        /// Round the line from the shoulder to the spot the whole arm can be turned (the elbow's swivel), and with it
        /// the way its swing brings the blade in. The swivel is found at which the blade comes in along the line asked
        /// for, as near as the shoulder and the trunk let the arm be turned. Kept for the whole blow.
        /// </summary>
        private void FindSwivel()
        {
            Vector3 from = ComesFrom(out Vector3 reachWay);
            float Off(float at, out ArmSolve solved)
            {
                solved = PlanAtSpot(at, out Arm arm, out Vector3 hingeAt, out float bend);
                // (The way the striking part is going as it meets the spot: where it is with every joint a little back.)
                var was = (atSpot, hinge, bendAtSpot);
                atSpot = arm;
                hinge = hingeAt;
                bendAtSpot = bend;
                Vector3 going = StrikeOf(Turned(0f, 0f, wristAtSpot)) - StrikeOf(Turned(1f, 2f, wristAtSpot + 1f));
                // (An arm no body has, at the spot or half wound up, is no answer; nor one in its own trunk.)
                float past = 2f * Past(Turned(0f, 0f, wristAtSpot)) + Past(Turned(0.5f * shoulderBack, 0.5f * elbowBack, 0.5f * (wristCocked + wristAtSpot)));
                (atSpot, hinge, bendAtSpot) = was;
                going = Vector3.ProjectOnPlane(going, reachWay);
                float off = going.sqrMagnitude > 1e-10f ? Vector3.Angle(going, -from) : 180f;
                return off + past + 2f * solved.shoulderPast + 400f * solved.clearanceShort;
            }

            float best = float.MaxValue;
            swivel = 0f;
            for (float at = -180f; at < 180f; at += 15f)
            {
                float off = Off(at, out _);
                if (off < best)
                {
                    best = off;
                    swivel = at;
                }
            }

            for (float step = 7.5f; step >= 0.9f; step *= 0.5f)
            {
                foreach (float at in new[] { swivel - step, swivel + step })
                {
                    float off = Off(at, out _);
                    if (off < best)
                    {
                        best = off;
                        swivel = at;
                    }
                }
            }
        }

        /// <summary>What the plan comes to, for the logs: how far it reaches, how true its line and its edge are.</summary>
        private void Measure(in ArmSolve solved)
        {
            Short = solved.pointMiss;
            Vector3 from = ComesFrom(out Vector3 reachWay);
            Vector3 going = StrikeOf(Turned(0f, 0f, wristAtSpot)) - StrikeOf(Turned(1f, 2f, wristAtSpot + 1f));
            Vector3 across = Vector3.ProjectOnPlane(going, reachWay);
            LineOff = across.sqrMagnitude > 1e-10f ? Vector3.Angle(across, -from) : 0f;
            SwordOf(atSpot, out _, out Quaternion hand);
            Vector3 blade = hand * bladeInHand, edge = hand * (slotInHand * Vector3.up);
            Vector3 acrossBlade = Vector3.ProjectOnPlane(going, blade);
            float off = acrossBlade.sqrMagnitude > 1e-10f ? Vector3.Angle(edge, acrossBlade) : 0f;
            edgeSign = off <= 90f ? 1 : -1;
            EdgeOff = Mathf.Min(off, 180f - off);
            PastAtSpot = Past(atSpot);
        }

        // Its own body as the blade sees it: the trunk, the head, the other arm and the legs (not the sword arm).
        private static readonly (PartId a, PartId b, float r)[] OwnBody =
        {
            (PartId.Pelvis, PartId.Chest, 0.17f), (PartId.Chest, PartId.Head, 0.15f), (PartId.Head, PartId.Head, 0.13f),
            (PartId.UpperArmL, PartId.ForearmL, 0.06f), (PartId.ForearmL, PartId.HandL, 0.05f),
            (PartId.ThighL, PartId.ShinL, 0.09f), (PartId.ShinL, PartId.FootL, 0.07f),
            (PartId.ThighR, PartId.ShinR, 0.09f), (PartId.ShinR, PartId.FootR, 0.07f),
        };

        /// <summary>How far an arm's blade is from its own body (metres; under 0: in it).</summary>
        private float OwnRoom(in Arm arm)
        {
            SwordOf(arm, out Vector3 grip, out Quaternion hand);
            Vector3 blade = hand * bladeInHand;
            Vector3 from = grip + blade * (0.12f * size), tip = grip + blade * bladeLength;
            float least = float.MaxValue;
            foreach (var c in OwnBody)
            {
                least = Mathf.Min(least, StrikeHits.SegmentsNearest(from, tip, anim.Part(c.a).position, anim.Part(c.b).position, out _, out _, out _, out _) - c.r * size);
            }

            return least;
        }

        /// <summary>
        /// Wound no further back, and carried no further on, than a body's arm goes and its own body leaves room for: a
        /// blade wound into its own head, or an arm wound past its shoulder's range, is wound less.
        /// </summary>
        private void FitToOwnBody()
        {
            // (No worse than the arm at the spot is: an arm already at the edge of its range there is not held to more.)
            float allowed = PastAtSpot + 1.5f;
            float shoulder = shoulderBack, elbow = elbowBack, wrist = wristCocked;
            for (int i = 0; i < 9; i++)
            {
                TrunkTo(turnBack, leanBack);
                Arm wound = Turned(shoulderBack, elbowBack, wristCocked);
                if (OwnRoom(wound) >= OwnClearance * size && Past(wound) <= allowed)
                {
                    break;
                }

                float less = 1f - 0.1f * (i + 1);
                shoulderBack = shoulder * less;
                elbowBack = elbow * less;
                wristCocked = Mathf.Lerp(wristAtSpot, wrist, less);
            }

            float on = shoulderOn, elbowOnFull = elbowOn, wristFull = wristOn;
            for (int i = 0; i < 9; i++)
            {
                TrunkTo(turnOn, leanOn);
                Arm carried = Turned(-shoulderOn, -elbowOn, wristOn);
                if (OwnRoom(carried) >= OwnClearance * size && Past(carried) <= allowed)
                {
                    break;
                }

                float less = 1f - 0.1f * (i + 1);
                shoulderOn = on * less;
                elbowOn = elbowOnFull * less;
                wristOn = Mathf.Lerp(wristAtSpot, wristFull, less);
            }
        }

        // ---- In time -----------------------------------------------------------------------------------------------------

        /// <summary>One joint's turn through the cut and past it: still until it starts, faster and faster, steady for its last moment into the spot, then slowing evenly to its end.</summary>
        private struct Run
        {
            public float Back, AtSpot, On, From, Steady;

            public float Speed(float contactAt) => (AtSpot - Back) / Mathf.Max(1e-4f, 0.5f * (contactAt - From - Steady) + Steady);

            public float Carry(float contactAt) => Mathf.Abs(Speed(contactAt)) > 1e-3f ? Mathf.Min(MostCarry, 2f * Mathf.Abs((On - AtSpot) / Speed(contactAt))) : 0f;

            public float At(float t, float contactAt)
            {
                if (t <= From)
                {
                    return Back;
                }

                float speed = Speed(contactAt);
                if (t <= contactAt)
                {
                    float rise = Mathf.Max(1e-4f, contactAt - From - Steady), into = t - From;
                    return Back + (into <= rise ? 0.5f * speed * into * into / rise : 0.5f * speed * rise + speed * (into - rise));
                }

                float carry = Carry(contactAt);
                if (carry <= 1e-4f)
                {
                    return AtSpot;
                }

                // (Slowing evenly from its speed at the spot; as far as that takes it, and no further than its end.)
                float past = Mathf.Min(t - contactAt, carry);
                return AtSpot + speed * past - 0.5f * speed * past * past / carry;
            }
        }

        private Run shoulderRun, elbowRun, wristRun;

        /// <summary>The times of its phases, and each joint's run through the cut: the shoulder from the start, the elbow after it, the wrist last.</summary>
        private void SetTimes()
        {
            CutFrom = shape.WindUp + Hold;
            ContactAt = CutFrom + shape.Cut;
            // (The elbow's turn back is what its range leaves: a near spot's arm is bent already.)
            float elbowFrom = Mathf.Clamp(bendAtSpot + elbowBack, LeastBend, MostBend) - bendAtSpot;
            float elbowTo = Mathf.Clamp(bendAtSpot - elbowOn, LeastBend, MostBend) - bendAtSpot;
            shoulderRun = new Run { Back = shoulderBack, AtSpot = 0f, On = -shoulderOn, From = CutFrom, Steady = 0.5f * shape.Cut };
            elbowRun = new Run { Back = elbowFrom, AtSpot = 0f, On = elbowTo, From = CutFrom + shape.ElbowFrom * shape.Cut, Steady = Mathf.Min(SteadyFor, 0.5f * shape.Cut) };
            wristRun = new Run { Back = wristCocked, AtSpot = wristAtSpot, On = wristOn, From = CutFrom + shape.WristFrom * shape.Cut, Steady = Mathf.Min(0.03f, 0.3f * shape.Cut) };
            // (The trunk goes first: it starts in the stillness at the top, before the arm, and is steady well before the spot.)
            float trunkFrom = Mathf.Max(shape.WindUp, CutFrom - 0.05f);
            turnRun = new Run { Back = turnBack, AtSpot = turnAtSpot, On = turnOn, From = trunkFrom, Steady = 0.5f * (ContactAt - trunkFrom) };
            leanRun = new Run { Back = leanBack, AtSpot = leanAtSpot, On = leanOn, From = trunkFrom, Steady = 0.5f * (ContactAt - trunkFrom) };
            CarriedAt = ContactAt + Mathf.Max(0.04f, Mathf.Max(shoulderRun.Carry(ContactAt), Mathf.Max(elbowRun.Carry(ContactAt), wristRun.Carry(ContactAt))));
            Wound = TrunkTurn + shoulderBack + elbowFrom + (wristCocked - wristAtSpot);
            TurnSpeed = -(shoulderRun.Speed(ContactAt) + elbowRun.Speed(ContactAt) + wristRun.Speed(ContactAt));
            StrikeSpeed = Vector3.Distance(StrikeOf(ArmAt(ContactAt)), StrikeOf(ArmAt(ContactAt - 0.004f))) / 0.004f;
        }

        /// <summary>The arm at a moment of the blow's own time, before it is on its way back.</summary>
        private Arm ArmAt(float t)
        {
            if (t < shape.WindUp)
            {
                // To the wound-up arm from how the sword was held, each joint eased in and out.
                Arm to = Turned(shoulderBack, elbowRun.Back, wristCocked);
                float u = Mathf.Clamp01(t / shape.WindUp);
                float w = u * u * (3f - 2f * u);
                return new Arm
                {
                    Upper = Quaternion.Slerp(held.Upper, to.Upper, w),
                    Fore = Quaternion.Slerp(held.Fore, to.Fore, w),
                    Hand = Quaternion.Slerp(held.Hand, to.Hand, w),
                };
            }

            return Turned(shoulderRun.At(t, ContactAt), elbowRun.At(t, ContactAt), wristRun.At(t, ContactAt));
        }

        /// <summary>
        /// Where the sword is planned to be at a moment of the blow's own time (world), with the body where it is now:
        /// the grip, the tip and the striking part. What a defender reads, and what the blade as seen is measured against.
        /// </summary>
        public void BladeAtTime(float t, out Vector3 grip, out Vector3 tip, out Vector3 strike)
        {
            t = Mathf.Min(t, CarriedAt);
            var trunkWas = SaveTrunk();
            TrunkAt(t, out float turn, out float lean);
            TrunkTo(turn, lean);
            SwordOf(ArmAt(t), out grip, out Quaternion hand);
            RestoreTrunk(trunkWas);
            Vector3 blade = hand * bladeInHand;
            tip = grip + blade * bladeLength;
            strike = grip + blade * strikeAlong;
        }

        /// <summary>One step of its own time (after the walk's step, before the pose).</summary>
        public void Step(float dt)
        {
            if (!Playing)
            {
                return;
            }

            float pace = dt * Mathf.Max(0.05f, loco.TimeScale);
            // (The hand thrown by what stopped the blade comes back, critically damped.)
            float k = 2f * Mathf.PI * LocoBody.HandKnockFrequency;
            knockSpeed += (-k * k * knock - 2f * k * knockSpeed) * dt;
            knock = Vector3.ClampMagnitude(knock + knockSpeed * dt, LocoBody.MostHandKnock * size);
            bindNow = bound ? bindOffset : Vector3.Lerp(bindNow, Vector3.zero, 1f - Mathf.Exp(-dt / 0.05f));

            if (comingBack)
            {
                backTime += pace;
                if (backTime >= shape.Back)
                {
                    Playing = false;
                }

                return;
            }

            if (rewinding)
            {
                Time = Mathf.Max(rewindFloor, Time - pace * rewindSpeed);
                if (Time <= rewindFloor + 1e-4f)
                {
                    rewinding = false;
                    ComeBack();
                }

                return;
            }

            Time += pace * Mathf.Max(0f, Speed);
            if (Time >= CarriedAt)
            {
                Time = CarriedAt;
                ComeBack();
            }
        }

        /// <summary>The way back begun from the arm as it is now.</summary>
        private void ComeBack()
        {
            backFrom = new Arm
            {
                Upper = Quaternion.Inverse(chestPart.rotation) * upperPart.rotation,
                Fore = forePart.localRotation,
                Hand = handPart.localRotation,
            };
            turnFrom = turnNow;
            leanFrom = leanNow;
            comingBack = true;
            backTime = 0f;
        }

        /// <summary>
        /// The sword arm posed for this step (after the body's own pose): the plan taken again for where the spot and
        /// the shoulder are now, and the arm put where the blow has it.
        /// </summary>
        /// <param name="again">Posed a second time in one step (put back to where its blade was stopped): the trunk under it is the one already read this step.</param>
        public void Pose(float dt, bool again = false)
        {
            if (!Playing)
            {
                return;
            }

            if (again)
            {
                // (Back to the body's own trunk first: the blow's turn is laid over that, not over itself.)
                TrunkTo(0f, 0f);
            }
            else
            {
                CaptureTrunk();
            }

            Arm arm;
            if (comingBack)
            {
                float u = Mathf.Clamp01(backTime / shape.Back);
                float w = u * u * (3f - 2f * u);
                turnNow = Mathf.Lerp(turnFrom, 0f, w);
                leanNow = Mathf.Lerp(leanFrom, 0f, w);
                TrunkTo(turnNow, leanNow);
                arm = new Arm
                {
                    Upper = Quaternion.Slerp(backFrom.Upper, held.Upper, w),
                    Fore = Quaternion.Slerp(backFrom.Fore, held.Fore, w),
                    Hand = Quaternion.Slerp(backFrom.Hand, held.Hand, w),
                };
            }
            else
            {
                if (Time < ContactAt - NoReturn && !rewinding)
                {
                    spot = Spot();
                }

                // (The plan taken again with the trunk as it will be at the spot; then the trunk as it is now.)
                var solved = PlanAtSpot();
                TrunkTo(turnAtSpot, leanAtSpot);
                Measure(solved);
                TrunkAt(Time, out turnNow, out leanNow);
                TrunkTo(turnNow, leanNow);
                arm = ArmAt(Time);
            }

            upperPart.rotation = chestPart.rotation * arm.Upper;
            forePart.localRotation = arm.Fore;
            handPart.localRotation = arm.Hand;

            // (The hand thrown by what stopped the blade or held on a blade it is bound to, and its weapon resting
            // against what it touches outside the cut, as a clip's hand is held off it: the whole arm turned about the
            // shoulder so the grip goes that way, the arm's own shape kept.)
            Vector3 off = knock + bindNow + poser.RestOffset;
            if (off.sqrMagnitude > 1e-8f)
            {
                Vector3 shoulder = upperPart.position, grip = anim.SlotWeaponR.position;
                upperPart.rotation = Quaternion.FromToRotation(grip - shoulder, grip + off - shoulder) * upperPart.rotation;
            }

            if (Trace != null)
            {
                var r = ArmLimits.Read(chestPart.rotation, upperPart.position, forePart.position, handPart.position, handPart.rotation, false);
                if (r.Worst > 3f)
                {
                    Trace($"  t {Time:0.000}{(comingBack ? $" back {backTime:0.00}" : "")}{(rewinding ? " rewinding" : "")}{(off.sqrMagnitude > 1e-8f ? $" hand off {100f * off.magnitude:0} cm" : "")}: past reach {r.PastReach:0} turn {r.PastTurn:0} bend {r.PastBend:0} roll {r.PastRoll:0} wrist {r.PastWrist:0} | raised {r.Raised:0} toward {r.Toward:0} turn {r.Turn:0} bend {r.Bend:0} roll {r.Roll:0} flex {r.Flex:0} thumb {r.Thumbward:0}");
                }
            }

            MostWristShort = Mathf.Max(MostWristShort, ArmRig.WristAngles(Quaternion.Inverse(handRest) * handPart.localRotation, wristFrame, out _, out _, out _));
            if (!comingBack)
            {
                MostBladeInBody = Mathf.Max(MostBladeInBody, -OwnRoom(arm));
            }
        }

        // ---- What stops it (as a strike clip is stopped: StrikeHits) -------------------------------------------------

        /// <summary>Put back to an earlier moment of itself, at once: a blade stopped by what it met is shown where it met it.</summary>
        public void WindBack(float to)
        {
            if (Playing && !comingBack && to < Time)
            {
                Time = Mathf.Max(0f, to);
            }
        }

        /// <summary>It met something it can't go through: it backs out a little (so much of its own time, at so much of its speed), then goes back to where the sword was held from there.</summary>
        public void CutShort(float back, float speed)
        {
            if (!Playing || comingBack)
            {
                return;
            }

            rewinding = true;
            wasStopped = true;
            rewindFloor = Mathf.Max(CutFrom, Time - back);
            rewindSpeed = Mathf.Max(0.05f, speed);
        }

        /// <summary>The weapon hand thrown by a meeting of blades or a shield (m/s at the hand).</summary>
        public void KnockHand(Vector3 velocity) => knockSpeed += velocity;

        /// <summary>The weapon held on a blade it is bound to: the hand this far off where the blow has it (world) while the bind lasts.</summary>
        public void Bind(Vector3 offset)
        {
            bindOffset = offset;
            bindNow = offset;
            bound = true;
        }

        public void LetGo() => bound = false;

        /// <summary>Ended at once (its fighter fell, or is struck out of it): the arm is left where it is.</summary>
        public void End() => Playing = false;
    }
}
