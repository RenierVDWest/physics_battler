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
        };

        /// <summary>Our Medium against a real one (the positions are a real body's; the parryreach study scaled them so).</summary>
        public const float BodyScale = 0.9f;

        /// <summary>Where the forte is along the blade (share of its length from the grip), where on it the parry meets the blow.</summary>
        public const float ForteAt = 0.3f;

        /// <summary>How far the forte may go from its parry's own place to meet the incoming blade's path, and the blade turn from the parry's own way (metres at a Medium's size, degrees).</summary>
        public const float PlaceMost = 0.25f, MostBladeOff = 35f;

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

        private bool started, handKnown;
        private Vector3 forte;
        private Quaternion handRot, heldHand;
        private float swivel, elbow, turn, lean, sinceBlow = 99f;
        private int edgeSign = 1, lastChosen = -2;
        private Vector3 lastSlot;
        private float lastSolvedSwivel;

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
            if (blow.Path == null)
            {
                return;
            }

            // (The striking point's path and the tip's, the deeper: whichever reaches the body first, looked for along
            // each step of the path, since a blade crosses a head between two of its points.)
            // (The body as the blade meets it: the torso, the head, each thigh and shin, as they stand now.)
            Vector3 P(PartId id) => anim.Part(id).position;
            float Near(Vector3 p, Vector3 a, Vector3 b)
            {
                Vector3 ab = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
                return (a + ab * t - p).magnitude;
            }

            bool Inside(Vector3 p)
            {
                float r = size;
                if (Near(p, P(PartId.Pelvis), P(PartId.Chest)) <= 0.18f * r || Near(p, P(PartId.Chest), P(PartId.Head)) <= 0.17f * r
                    || (p - P(PartId.Head)).magnitude <= 0.16f * r)
                {
                    landPart = 0;
                    return true;
                }

                for (int leg = 0; leg < 2; leg++)
                {
                    var thigh = P(leg == 0 ? PartId.ThighL : PartId.ThighR);
                    var shin = P(leg == 0 ? PartId.ShinL : PartId.ShinR);
                    var foot = P(leg == 0 ? PartId.FootL : PartId.FootR);
                    if (Near(p, thigh, shin) <= 0.11f * r || Near(p, shin, foot) <= 0.09f * r)
                    {
                        landPart = leg == 0 ? -1 : 1;
                        return true;
                    }
                }

                return false;
            }

            for (int k = 1; k < blow.Path.Length; k++)
            {
                for (int j = 1; j <= 8; j++)
                {
                    float u = j / 8f;
                    foreach (var path in new[] { blow.TipPath, blow.Path })
                    {
                        if (path == null)
                        {
                            continue;
                        }

                        Vector3 p = Vector3.Lerp(path[k - 1], path[k], u);
                        if (Inside(p))
                        {
                            // (Where the striking point is then: the forte goes across its path there.)
                            land = Vector3.Lerp(blow.Path[k - 1], blow.Path[k], u);
                            if ((path[k] - path[k - 1]).sqrMagnitude > 1e-6f)
                            {
                                motion = (path[k] - path[k - 1]).normalized;
                            }

                            landHeight = p.y;
                            return;
                        }
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
                return landPart < 0 ? 10 : (y >= 0.6f ? 7 : 11);
            }

            if (blow.Thrust)
            {
                outside = x > 0.05f;
                return y >= 1.45f ? (x < -0.1f ? 1 : 2) : y >= 0.95f ? (outside ? 3 : 4) : (outside ? 11 : 10);
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

            return outside ? (y >= 0.6f ? 7 : 11) : 10;
        }

        /// <summary>
        /// One step, after the body is posed (the guard clip's arms on it): the trunk turned, and the sword arm set to
        /// the guard or the parry for the blow coming (<paramref name="blow"/>, null: none).
        /// </summary>
        private Vector3 landAt, landMotion;
        private float landHeight, feasible = 1f, pendingFor;
        private int pending = -1;

        /// <summary>How long a new read must hold before the parry changes to it (seconds).</summary>
        public const float ChangeAfter = 0.06f;

        /// <summary>What the blow first meets: 0 the torso or head, -1 the left leg, +1 the right leg.</summary>
        private int landPart;

        private static string V(Vector3 v) => $"({v.x:0.00},{v.y:0.00},{v.z:0.00})";

        public void Step(float dt, BlowRead? blow, Vector3 opponent)
        {
            var bodyTurn = BodyTurn;
            Position aim = Guard;
            Vector3 wantForte, wantBlade, wantEdge;
            float wantSwivel, wantTurn, wantLean;
            sinceBlow = blow.HasValue ? 0f : sinceBlow + dt;
            // (Read from early, moved late: the parry starts out only so long before contact as it takes to get there,
            // as an expert's does. Up early, it would show the attacker where to cut instead.)
            bool reading = blow.HasValue && blow.Value.Coming >= ReadAfter && blow.Value.ToContact <= MoveLead;
            if (reading || (Current >= 0 && sinceBlow <= HoldAfter))
            {
                if (reading)
                {
                    Landing(blow.Value, out landAt, out landMotion);
                    int chosen = Choose(blow.Value, landAt, landMotion, out _);
                    // (A change of mind holds a moment before it is taken: the read wavers between two near parries.)
                    if (Current >= 0 && chosen != Current)
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
                }

                aim = Parries[Current];
            }
            else
            {
                Current = -1;
            }

            Vector3 hand = World(aim.Hand);
            wantBlade = (bodyTurn * aim.Blade).normalized;
            wantForte = hand + wantBlade * (ForteAt * WeaponLength);
            wantEdge = Vector3.ProjectOnPlane(bodyTurn * aim.Edge, wantBlade).normalized;
            wantSwivel = aim.Swivel;
            wantTurn = aim.Turn;
            wantLean = aim.Lean;
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
            else if (blow.HasValue)
            {
                // The forte onto the incoming blade's path just before it lands; the blade square to the plane that
                // blade sweeps (its way and its motion), turned from the parry's own way as little as that needs; the
                // edge to the blow.
                var b = blow.Value;
                Vector3 motion = landMotion;
                float t = Mathf.Clamp(Vector3.Dot(wantForte - landAt, motion), -0.6f * size, -0.06f * size);
                Vector3 across = landAt + motion * t;
                wantForte += Vector3.ClampMagnitude(across - wantForte, PlaceMost * size);
                Vector3 square = Vector3.Cross(b.BladeWay, motion);
                if (square.sqrMagnitude > 1e-4f)
                {
                    square.Normalize();
                    if (Vector3.Dot(square, wantBlade) < 0f)
                    {
                        square = -square;
                    }

                    // (Less of it while the arm can't take the turn: the parry's own way is one it can.)
                    wantBlade = Vector3.RotateTowards(wantBlade, square, MostBladeOff * feasible * Mathf.Deg2Rad, 0f);
                }

                Vector3 toBlow = Vector3.ProjectOnPlane(-motion, wantBlade);
                wantEdge = toBlow.sqrMagnitude > 1e-4f ? toBlow.normalized : Vector3.ProjectOnPlane(wantEdge, wantBlade).normalized;

                // (In front of the body, never through it.)
                Vector3 ahead = bodyTurn * Vector3.forward;
                float front = Vector3.Dot(wantForte - Root, ahead);
                if (front < 0.22f * size)
                {
                    wantForte += ahead * (0.22f * size - front);
                }
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
            forte += Vector3.ClampMagnitude((wantForte - forte) * close, HandSpeed * size * dt);
            swivel += Mathf.Clamp(Mathf.DeltaAngle(swivel, wantSwivel) * close, -SwivelSpeed * dt, SwivelSpeed * dt);
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

            TuckFreeArm(bodyTurn);
            SolveArm(dt, close, wantBlade, wantEdge);
        }

        /// <summary>Where the free (left) hand is kept: behind the left hip, the elbow out and back (the sabreur's off hand, out of the way of blows).</summary>
        public static readonly Vector3 FreeHand = new Vector3(-0.12f, 1.0f, -0.22f);

        private void TuckFreeArm(Quaternion bodyTurn)
        {
            var upper = anim.Part(PartId.UpperArmL);
            var fore = anim.Part(PartId.ForearmL);
            var handL = anim.Part(PartId.HandL);
            var keep = handL.rotation;
            TwoBoneIK.Solve(upper, fore, handL, World(FreeHand), bodyTurn * new Vector3(-1f, -0.6f, -0.5f));
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
                handRot = Quaternion.RotateTowards(heldHand, handRot, BladeTurnSpeed * dt);
            }

            handRot = Quaternion.RotateTowards(handRot, target, Mathf.Min(Quaternion.Angle(handRot, target) * close, BladeTurnSpeed * dt));
            var keep = ArmRig.Solve(anim, true, Goal(handRot, swivel, 1f), torso, apply: false);
            if (!handKnown)
            {
                elbow = keep.swivel;
                handKnown = true;
            }

            elbow += Mathf.Clamp(Mathf.DeltaAngle(elbow, keep.swivel) * close, -SwivelSpeed * dt, SwivelSpeed * dt);
            feasible = keep.wristExcess > 20f ? Mathf.Max(0f, feasible - 4f * dt) : Mathf.Min(1f, feasible + 2f * dt);
            var solved = ArmRig.Solve(anim, true, Goal(handRot, elbow, 0f), torso, apply: true);
            heldHand = hand.rotation;
            float jump = Mathf.Abs(Mathf.DeltaAngle(lastSolvedSwivel, solved.swivel));
            Vector3 slotNow = slot.position;
            if (lastSlot != Vector3.zero)
            {
                MostHandSpeed = Mathf.Max(MostHandSpeed, Vector3.Distance(slotNow, lastSlot) / Mathf.Max(1e-4f, dt));
                MostSwivelJump = Mathf.Max(MostSwivelJump, jump);
            }

            if (Trace != null)
            {
                Trace($"{CurrentName,-12} edge {edgeSign:+0;-0} null {swivel,5:0} wants {keep.swivel,5:0} elbow {elbow,5:0} solved {solved.swivel,5:0} short {solved.wristExcess,3:0} miss {solved.pointMiss:0.00} bend {solved.elbowBend,3:0} hand {Vector3.Distance(slotNow, lastSlot) / Mathf.Max(1e-4f, dt),5:0.0} m/s");
            }

            lastSlot = slotNow;
            lastSolvedSwivel = solved.swivel;
            MostWristShort = Mathf.Max(MostWristShort, solved.wristExcess);
            MostShoulderPast = Mathf.Max(MostShoulderPast, solved.shoulderPast);
            MostIntoTorso = Mathf.Max(MostIntoTorso, solved.clearanceShort);
        }
    }
}
