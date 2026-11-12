using System;
using System.Collections.Generic;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Authority
{
    /// <summary>
    /// A unit's upper body as one physical body (the plan "A different approach", step 5): the pelvis, spine, chest,
    /// head and both arms as an articulation, each part on a ball joint driven toward the pose's own joint turn by a
    /// motor of limited strength. The pelvis is locked to a body the pose carries, so the legs stay the animation's.
    /// What it can't follow it doesn't: a blade that meets a shield stops there, a struck arm gives.
    ///
    /// Built as the body is at rest under the pelvis (every joint at zero is the rest pose), each part with the same
    /// collider its rig part has; the rig's own upper parts are left as its shadows (moved onto it after each step, so
    /// everything that reads a part's place still can). What the experiments found is built in: a motor is sized by
    /// the inertia of everything that hangs from its joint, a part's inertia is given in the part's own frame with no
    /// axis much easier to turn about than the others, and the motors' targets are fed a share of the way to the new
    /// pose at each physics step.
    /// </summary>
    public sealed class UpperBodyArticulation
    {
        /// <summary>The parts it is made of, each after the one it hangs from.</summary>
        public static readonly PartId[] Parts =
        {
            PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head,
            PartId.UpperArmL, PartId.ForearmL, PartId.HandL, PartId.UpperArmR, PartId.ForearmR, PartId.HandR,
        };

        /// <summary>The furthest the pose's pelvis is followed in one game step (metres; 15 m/s) and the most it is turned after (degrees; 1200 a second): past either the body is put where the pose is.</summary>
        public const float MostCarry = 0.25f, MostCarryTurn = 20f;

        /// <summary>Times the pose jumped and the body was put on it (for the reports).</summary>
        public int Jumps;

        /// <summary>No axis of a part, or of what hangs from a joint, turns with less than this share of the most (a thin blade about its own length rings on a stiff motor).</summary>
        public const float InertiaFloor = 0.2f;

        private readonly UnitRig rig;
        private readonly AnimRig anim;
        private readonly PartId[] parts;
        private readonly int[] parent;
        private readonly ArticulationBody[] links;
        private readonly Vector3[] was, now, hangs;
        private readonly float[] hz, most;
        private readonly Quaternion[] slack;             // each joint: how far aside of its pose it has been left (a turn on the part's own axes, after the pose's)
        private readonly bool[] gripped;                 // each joint: a hand with a shield strapped on it
        private readonly bool[] put;                     // each joint: put where the pose turned it at once, this step (where it is is not yet to be read)
        private readonly float[] tight;                  // each joint: 0 with slack (an arm), 1 held to its pose (the torso; an arm inside its own body or past its joint's range)
        private readonly Dictionary<Collider, int> linkOf = new Dictionary<Collider, int>();   // each solid's part (what a hand holds counts as the hand)

        // (What each joint's muscle is allowed over its own strength, to follow what the pose itself does.)
        private Vector3[] turning;                       // each joint's speed of turn in the last step (rad/s)
        private Vector3[] rate;                          // each joint's speed of turn in the pose this step (rad/s, on the part's own axes)
        private float[] carried, lever, reach, more;     // what hangs from it (kg), its centre from the joint and from the hips (m), the allowance (N m)
        private Vector3 rootSpeed, rootSpin;
        private readonly Quaternion[] lastWorld = new Quaternion[PartInfo.Count];
        private float stepWas;
        private readonly List<Piece>[] pieces;
        private readonly List<Collider> colliders = new List<Collider>();
        private readonly Rigidbody carrier;
        private Vector3 rootWas;
        private Quaternion rootTurnWas = Quaternion.identity;
        private bool known;

        /// <summary>The object its parts are under (destroyed with it).</summary>
        public readonly GameObject Root;

        private struct Piece
        {
            public float Mass;
            public Vector3 At, Turns;   // its centre and its inertia about it on the link's axes
        }

        /// <param name="under">What to build it under (the unit's detached body).</param>
        /// <param name="muscle">Each part's motor: how quick (Hz: the frequency what hangs from its joint would swing at undamped) and the most it turns with (N m).</param>
        public UpperBodyArticulation(UnitRig rig, AnimRig anim, Transform under, Func<PartId, (float hz, float most)> muscle)
        {
            this.rig = rig;
            this.anim = anim;
            // (What is left of it: a part cut off, and what hung from it, is not its own.)
            var own = new List<PartId>();
            foreach (var id in Parts)
            {
                if (!rig.IsSevered(id) && (id == PartId.Pelvis || own.Contains(PartInfo.Parent(id))))
                {
                    own.Add(id);
                }
            }

            parts = own.ToArray();
            int n = parts.Length;
            parent = new int[n];
            links = new ArticulationBody[n];
            was = new Vector3[n];
            now = new Vector3[n];
            hangs = new Vector3[n];
            hz = new float[n];
            tight = new float[parts.Length];
            put = new bool[parts.Length];
            gripped = new bool[parts.Length];
            slack = new Quaternion[parts.Length];
            for (int j = 0; j < slack.Length; j++) slack[j] = Quaternion.identity;
            most = new float[n];
            pieces = new List<Piece>[n];
            var pelvis = anim.Part(PartId.Pelvis);
            rootWas = pelvis.position;
            rootTurnWas = pelvis.rotation;

            Root = new GameObject(rig.name + " (upper body)");
            Root.transform.SetParent(under, false);
            for (int i = 0; i < n; i++)
            {
                var part = rig.Part(parts[i]);
                parent[i] = i == 0 ? -1 : Array.IndexOf(parts, PartInfo.Parent(parts[i]));
                var go = new GameObject(parts[i] + " (link)") { layer = part.gameObject.layer };
                if (i == 0)
                {
                    go.transform.SetParent(Root.transform, false);
                    go.transform.SetPositionAndRotation(rootWas, rootTurnWas);
                }
                else
                {
                    // (As the body is at rest: every part turned as the pelvis is, so a joint at zero is the rest pose.)
                    go.transform.SetParent(links[parent[i]].transform, false);
                    go.transform.localPosition = part.restLocalPosition;
                    go.transform.localRotation = Quaternion.identity;
                }

                var link = go.AddComponent<ArticulationBody>();
                links[i] = link;
                link.useGravity = !Weightless;
                link.linearDamping = 0f;
                link.angularDamping = 0.05f;
                link.jointFriction = 0f;
                link.maxAngularVelocity = 100f;
                // (What it will meet in the coming step is found ahead: a blade at 30 m/s moves 12 cm in a physics step, further than a shield is thick.)
                link.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                link.maxJointVelocity = MostJointSpeed;
                link.maxDepenetrationVelocity = MostPushOut;
                if (i == 0)
                {
                    link.solverIterations = SolverSteps;
                    link.solverVelocityIterations = SolverSpeedSteps;
                    link.sleepThreshold = 0f;
                }
                else
                {
                    link.jointType = ArticulationJointType.SphericalJoint;
                    link.anchorPosition = Vector3.zero;
                    link.anchorRotation = Quaternion.identity;
                    link.twistLock = ArticulationDofLock.FreeMotion;
                    link.swingYLock = ArticulationDofLock.FreeMotion;
                    link.swingZLock = ArticulationDofLock.FreeMotion;
                }

                pieces[i] = new List<Piece>();
                Collider copy = CopyCollider(part.partCollider, part.transform, go.transform);
                if (copy != null)
                {
                    colliders.Add(copy);
                    linkOf[copy] = i;
                    pieces[i].Add(PieceOf(copy, go.transform, part.body.mass));
                }
                else
                {
                    pieces[i].Add(new Piece { Mass = part.body.mass, At = Vector3.zero, Turns = Vector3.one * (0.4f * part.body.mass * 0.05f * 0.05f) });
                }

                (hz[i], most[i]) = i > 0 ? muscle(parts[i]) : (0f, 0f);
            }

            // (Its own parts don't stop each other, as the pose's don't; nor do the rig's parts they stand in for.)
            for (int a = 0; a < colliders.Count; a++)
            {
                for (int b = a + 1; b < colliders.Count; b++)
                {
                    Physics.IgnoreCollision(colliders[a], colliders[b], true);
                }

                foreach (var part in rig.Parts)
                {
                    if (part.partCollider != null)
                    {
                        Physics.IgnoreCollision(colliders[a], part.partCollider, true);
                    }
                }
            }

            RefreshMass();

            // The pelvis carried by the pose: locked to a body that goes where the posed pelvis goes.
            var carrierObject = new GameObject("Pelvis (carrier)");
            carrierObject.transform.SetParent(Root.transform, false);
            carrierObject.transform.SetPositionAndRotation(rootWas, rootTurnWas);
            carrier = carrierObject.AddComponent<Rigidbody>();
            carrier.isKinematic = true;
            carrier.useGravity = false;
            var hold = carrierObject.AddComponent<ConfigurableJoint>();
            hold.connectedArticulationBody = links[0];
            hold.anchor = Vector3.zero;
            hold.autoConfigureConnectedAnchor = false;
            hold.connectedAnchor = Vector3.zero;
            hold.xMotion = hold.yMotion = hold.zMotion = ConfigurableJointMotion.Locked;
            hold.angularXMotion = hold.angularYMotion = hold.angularZMotion = ConfigurableJointMotion.Locked;
            SnapJoints();
        }

        /// <summary>
        /// Once everything is in its hands: every joint put where the pose has it again. A part added (a shield's
        /// grip) may have the engine build the whole body anew, with every joint back at its zero.
        /// </summary>
        public void Settle()
        {
            SnapJoints();
            ClearOfGround();
        }

        /// <summary>UPPER_DEBUG=1 in the environment: each joint the pose turns at once is logged with what kind of turn it was.</summary>
        public static readonly bool Debugging = System.Environment.GetEnvironmentVariable("UPPER_DEBUG") == "1";

        /// <summary>How much of the pose's own speed of turn each motor is told to turn at (1: all of it).</summary>
        public static float FeedForward = 1f;

        /// <summary>For measuring how the muscles alone follow the pose: the body touches nothing.</summary>
        public static bool Ghost;

        private Vector3 groundAt;
        private readonly HashSet<Collider> grounds = new HashSet<Collider>();

        /// <summary>
        /// Nothing of the body or in its hands is stopped by the ground it stands on. The clips swing a blade low,
        /// and through the ground at the bottom of a rising cut; stopped there, the blade held its arm back a metre
        /// from the pose and its body with it. (A unit on its feet has no other business with the ground; fallen,
        /// it is the rig's own parts that lie on it.) Looked for again as the body moves on.
        /// </summary>
        private void ClearOfGround()
        {
            if (Ghost)
            {
                foreach (var c in colliders)
                {
                    if (c != null)
                    {
                        c.enabled = false;
                    }
                }
            }

            groundAt = Root != null && links[0] != null ? links[0].transform.position : Vector3.zero;
            // (A project without the layer has -1 here, and nothing is found on it.)
            int layer = BodyPart.GroundLayer;
            foreach (var ground in Physics.OverlapSphere(groundAt, 6f, 1 << layer, QueryTriggerInteraction.Ignore))
            {
                if (ground == null || !grounds.Add(ground))
                {
                    continue;
                }

                foreach (var c in colliders)
                {
                    if (c != null)
                    {
                        Physics.IgnoreCollision(c, ground, true);
                    }
                }
            }
        }

        /// <summary>Every joint put where the pose has it, at once and still (as it is built, and when the body is put somewhere).</summary>
        private void SnapJoints()
        {
            Vector3 sign = ArticulationConventions.TargetSign;
            for (int i = 1; i < parts.Length; i++)
            {
                var local = Quaternion.Inverse(anim.Part(parts[parent[i]]).rotation) * anim.Part(parts[i]).rotation;
                now[i] = was[i] = JointMath.TurnVector(local);
                slack[i] = Quaternion.identity;
                Vector3 at = Vector3.Scale(now[i], sign);
                links[i].jointPosition = new ArticulationReducedSpace(at.x, at.y, at.z);
                links[i].jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
                for (int axis = 0; axis < 3; axis++)
                {
                    links[i].SetDriveTarget((ArticulationDriveAxis)axis, at[axis] * Mathf.Rad2Deg);
                    links[i].SetDriveTargetVelocity((ArticulationDriveAxis)axis, 0f);
                }
            }

            foreach (var g in grips)
            {
                SnapGrip(g);
            }

            known = true;
        }

        /// <summary>A part's link.</summary>
        public ArticulationBody Link(PartId id)
        {
            int i = Array.IndexOf(parts, id);
            return i >= 0 ? links[i] : null;
        }

        /// <summary>Whether a part is one of its own (not cut off when it was built).</summary>
        public bool Holds(PartId id) => Array.IndexOf(parts, id) >= 0;

        /// <summary>Its own parts, each after the one it hangs from.</summary>
        public IReadOnlyList<PartId> Own => parts;

        /// <summary>Every collider on it (its parts', and what its hands hold).</summary>
        public IReadOnlyList<Collider> Colliders => colliders;

        /// <summary>What hangs from a part's joint, as an inertia about it on the part's three axes (kg m2).</summary>
        public Vector3 Hangs(PartId id) => hangs[Array.IndexOf(parts, id)];

        /// <summary>A part's motor: how quick, and the most it turns with (N m; 0: no limit).</summary>
        public void SetMuscle(PartId id, float quick, float mostTorque)
        {
            int i = Array.IndexOf(parts, id);
            if (i <= 0)
            {
                return;
            }

            hz[i] = quick;
            most[i] = mostTorque;
            SetDrives(i);
        }

        private void SetDrives(int i)
        {
            float w = 2f * Mathf.PI * hz[i], scale = ArticulationConventions.StiffnessScale;
            // (A wrist bears what turns the shield in its hand: the grip's motor has the arm's whole strength and
            // pushes back on the hand with it, four times what a wrist has. Without this the wrist gave way each
            // time the pose turned the shield to face a blow.)
            float borne = Returns && GripStrength > 0f && gripped[i] ? GripStrength : 0f;
            for (int axis = 0; axis < 3; axis++)
            {
                // (A damper alone is as strong about every axis as about the heaviest: what hangs from a joint
                // is measured at rest, and about its own length a straight arm is nothing; bent at the elbow, with
                // a sword out, it is as much to turn that way as any. Sized by the rest, an upper arm's damper let
                // it slip 15 degrees about its length each time the forearm swung.)
                float turns = Returns && EvenDamping ? Mathf.Max(hangs[i].x, Mathf.Max(hangs[i].y, hangs[i].z)) : hangs[i][axis];
                links[i].SetDriveStiffness((ArticulationDriveAxis)axis, Returns ? 0f : w * w * turns * scale);
                links[i].SetDriveDamping((ArticulationDriveAxis)axis, 2f * w * turns * scale * ArticulationConventions.DampingScale * (Returns ? DampTimes : 1f));
                links[i].SetDriveForceLimit((ArticulationDriveAxis)axis, most[i] > 0f ? most[i] + (more != null ? AllowShare * more[i] : 0f) + borne : float.MaxValue);
            }

            // (What the hand grips is turned as quick as the hand, and held with all the grip has: a shield is
            // strapped to its arm, and pressed on it is the arm that gives, not the board that turns in the fist.)
            foreach (var g in grips)
            {
                if (g.Hand != i)
                {
                    continue;
                }

                for (int axis = 0; axis < 3; axis++)
                {
                    float turns = Returns && EvenDamping ? Mathf.Max(g.Hangs.x, Mathf.Max(g.Hangs.y, g.Hangs.z)) : g.Hangs[axis];
                    g.Link.SetDriveStiffness((ArticulationDriveAxis)axis, Returns ? 0f : w * w * turns * scale);
                    g.Link.SetDriveDamping((ArticulationDriveAxis)axis, 2f * w * turns * scale * ArticulationConventions.DampingScale * (Returns ? DampTimes : 1f));
                    g.Link.SetDriveForceLimit((ArticulationDriveAxis)axis, GripStrength > 0f ? GripStrength : float.MaxValue);
                }
            }
        }

        /// <summary>
        /// Something solid put in a hand (a sword's blade, a shield's board): its collider copied onto the hand's
        /// link where it is now against the rig's hand, with its weight.
        /// </summary>
        /// <param name="balance">Where its weight is, against the rig's hand (null: its collider's middle).</param>
        /// <param name="gyration">Its radius of gyration about that point across its length (metres; 0: as a solid of its collider's shape).</param>
        public Collider Hold(PartId hand, Collider solid, float mass, Vector3? balance = null, float gyration = 0f)
        {
            int i = Array.IndexOf(parts, hand);
            if (i < 0)
            {
                return null;
            }

            var part = rig.Part(hand);
            var copy = CopyCollider(solid, part.transform, links[i].transform);
            if (copy == null)
            {
                return null;
            }

            copy.isTrigger = false;
            foreach (var other in colliders)
            {
                Physics.IgnoreCollision(copy, other, true);
            }

            foreach (var p in rig.Parts)
            {
                if (p.partCollider != null)
                {
                    Physics.IgnoreCollision(copy, p.partCollider, true);
                }
            }

            colliders.Add(copy);
            linkOf[copy] = i;
            var piece = PieceOf(copy, links[i].transform, mass);
            Vector3 middle = piece.At;
            if (balance.HasValue)
            {
                piece.At = balance.Value;
            }

            if (gyration > 0f)
            {
                // (A sword turns about its balance like a rod a third its length, whatever its collider's shape.)
                Vector3 way = (middle - piece.At).sqrMagnitude > 1e-8f ? (middle - piece.At).normalized : Vector3.forward;
                Vector3 across = new Vector3(1f - way.x * way.x, 1f - way.y * way.y, 1f - way.z * way.z);
                piece.Turns = mass * gyration * gyration * across;
            }

            pieces[i].Add(piece);
            held.Add((i, copy, pieces[i].Count - 1, solid));
            RefreshMass();
            return copy;
        }

        /// <summary>
        /// What a hand holds that is turned in it (a shield, turned to face a blow): a part of its own on a joint at
        /// the grip, driven to where the pose turns it. Turned by being put there it would pass through whatever
        /// stood in its way (a board turned 47° in a step went through the arm that struck at it).
        /// </summary>
        private sealed class Grip
        {
            public int Hand;
            public ArticulationBody Link;
            public Transform Posed, Shown;      // the pose's slot (where it is turned to), the rig's (what is seen)
            public Quaternion Rest;             // the slot's turn in the hand where the grip's joint is at zero
            public Vector3 Was, Now, Hangs, Centre, Rate;
            public readonly List<Piece> Pieces = new List<Piece>();
        }

        private readonly List<Grip> grips = new List<Grip>();

        /// <summary>The most a grip turns what it holds with (N m; 0: no limit).</summary>
        public float GripStrength;

        /// <summary>Whether what a hand holds is a part of its own, turned by its grip's motor (not by the rig's slot).</summary>
        public bool Turns(PartId hand)
        {
            int i = Array.IndexOf(parts, hand);
            return i >= 0 && grips.Exists(g => g.Hand == i);
        }

        /// <summary>
        /// Something solid a hand holds and turns in its grip (a shield's board): a part of its own under the hand's
        /// link, on a ball joint where the slot is. To be held as the body is built, before the engine first steps:
        /// a part added to a body that has stepped would have the engine build the whole body anew.
        /// </summary>
        /// <param name="posed">The pose's slot: its turn against the pose's hand is where the grip's motor is driven.</param>
        /// <param name="shown">The rig's slot: what is seen, and what the solid is copied from; turned with the part after each step.</param>
        /// <param name="rest">The slot's turn in the hand as it is gripped, unturned: the grip's joint is at zero there, so that no turn the pose asks for comes near half a turn about (where a ball joint's turn has two names and the part flips between them).</param>
        public Collider HoldTurned(PartId hand, Collider solid, float mass, Transform posed, Transform shown, Quaternion rest)
        {
            int i = Array.IndexOf(parts, hand);
            if (i < 0 || posed == null || shown == null)
            {
                return null;
            }

            var grip = grips.Find(g => g.Hand == i);
            if (grip == null)
            {
                var part = rig.Part(hand);
                var go = new GameObject(hand + " grip (link)") { layer = part.gameObject.layer };
                go.transform.SetParent(links[i].transform, false);
                go.transform.localPosition = part.transform.InverseTransformPoint(shown.position);
                go.transform.localRotation = rest;
                var link = go.AddComponent<ArticulationBody>();
                link.useGravity = !Weightless;
                link.linearDamping = 0f;
                link.angularDamping = 0.05f;
                link.jointFriction = 0f;
                link.maxAngularVelocity = 100f;
                link.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                link.maxJointVelocity = MostJointSpeed;
                link.maxDepenetrationVelocity = MostPushOut;
                link.jointType = ArticulationJointType.SphericalJoint;
                link.anchorPosition = Vector3.zero;
                link.anchorRotation = Quaternion.identity;
                link.twistLock = ArticulationDofLock.FreeMotion;
                link.swingYLock = ArticulationDofLock.FreeMotion;
                link.swingZLock = ArticulationDofLock.FreeMotion;
                grip = new Grip { Hand = i, Link = link, Posed = posed, Shown = shown, Rest = rest };
                grips.Add(grip);
                gripped[i] = true;
            }

            // (Where it sits against the slot it is held by: the link is the slot, turned as the hand at its joint's zero.)
            var copy = CopyCollider(solid, shown, grip.Link.transform);
            if (copy == null)
            {
                return null;
            }

            copy.isTrigger = false;
            foreach (var other in colliders)
            {
                Physics.IgnoreCollision(copy, other, true);
            }

            foreach (var p in rig.Parts)
            {
                if (p.partCollider != null)
                {
                    Physics.IgnoreCollision(copy, p.partCollider, true);
                }
            }

            colliders.Add(copy);
            linkOf[copy] = i;
            // (Its weight on the link's axes: the slot's.)
            var piece = PieceOf(copy, grip.Link.transform, mass);
            grip.Pieces.Add(piece);
            RefreshMass();
            SnapGrip(grip);
            return copy;
        }

        /// <summary>A grip's turn as the pose has it: its slot in its hand, from where it is gripped unturned (a turn vector on the slot's own axes, radians).</summary>
        private Vector3 GripTurn(Grip g)
        {
            var inHand = Quaternion.Inverse(anim.Part(parts[g.Hand]).rotation) * g.Posed.rotation;
            // (As this engine takes the turn of a part built turned: after how it was built, or before; measured.)
            return JointMath.TurnVector(ArticulationConventions.TurnOnOwnAxes ? Quaternion.Inverse(g.Rest) * inHand : inHand * Quaternion.Inverse(g.Rest));
        }

        private void SnapGrip(Grip g)
        {
            Vector3 sign = ArticulationConventions.TargetSign;
            g.Now = g.Was = GripTurn(g);
            Vector3 at = Vector3.Scale(g.Now, sign);
            g.Link.jointPosition = new ArticulationReducedSpace(at.x, at.y, at.z);
            g.Link.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
            for (int axis = 0; axis < 3; axis++)
            {
                g.Link.SetDriveTarget((ArticulationDriveAxis)axis, at[axis] * Mathf.Rad2Deg);
                g.Link.SetDriveTargetVelocity((ArticulationDriveAxis)axis, 0f);
            }
        }

        /// <summary>The most the pose may turn a joint in one game step and have its muscle follow (degrees); past it the joint is put there.</summary>
        public const float MostJointTurn = 70f;

        /// <summary>A joint the pose turns at once is put there (see <see cref="BeginStep"/>); off, its muscle brings it there as fast as it brings any joint back to its pose.</summary>
        public static bool PutFlips = true;

        /// <summary>Times a joint was put where the pose turned it at once, and the last such joint (for the reports).</summary>
        public int JointJumps;

        public PartId LastJointJump;

        /// <summary>The most anything held was turned in its hand this step (degrees; for the reports).</summary>
        public float HeldTurn;

        private readonly List<(int link, Collider collider, int piece, Collider source)> held = new List<(int, Collider, int, Collider)>();

        /// <summary>What the hands hold kept where the rig's hands have it (a shield is turned in its hand as it is raised).</summary>
        private void FollowHeld()
        {
            HeldTurn = 0f;
            foreach (var h in held)
            {
                if (h.source == null || h.collider == null)
                {
                    continue;
                }

                var hand = rig.Part(parts[h.link]).transform;
                var t = h.collider.transform;
                var turn = Quaternion.Inverse(hand.rotation) * h.source.transform.rotation;
                HeldTurn = Mathf.Max(HeldTurn, Quaternion.Angle(t.localRotation, turn));
                t.localPosition = hand.InverseTransformPoint(h.source.transform.position);
                t.localRotation = turn;
            }
        }

        /// <summary>What a hand held is let go: its collider and its weight taken off the hand's link.</summary>
        public void LetGo(PartId hand)
        {
            int i = Array.IndexOf(parts, hand);
            if (i < 0)
            {
                return;
            }

            for (int k = held.Count - 1; k >= 0; k--)
            {
                if (held[k].link != i)
                {
                    continue;
                }

                colliders.Remove(held[k].collider);
                held[k].collider.enabled = false;
                UnityEngine.Object.Destroy(held[k].collider.gameObject);
                held.RemoveAt(k);
            }

            if (pieces[i].Count > 1)
            {
                pieces[i].RemoveRange(1, pieces[i].Count - 1);
                RefreshMass();
            }
        }

        /// <summary>Each link's mass, centre and inertia from what is on it, then what hangs from each joint, and the motors sized by it.</summary>
        private void RefreshMass()
        {
            int n = parts.Length;
            var centre = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                float mass = 0f;
                Vector3 at = Vector3.zero;
                foreach (var p in pieces[i])
                {
                    mass += p.Mass;
                    at += p.Mass * p.At;
                }

                at /= Mathf.Max(1e-5f, mass);
                Vector3 turns = Vector3.zero;
                foreach (var p in pieces[i])
                {
                    turns += p.Turns + Spread(p.Mass, p.At - at);
                }

                turns = Floored(turns);
                centre[i] = at;
                var link = links[i];
                link.automaticCenterOfMass = false;
                link.automaticInertiaTensor = false;
                link.mass = mass;
                link.centerOfMass = at;
                link.inertiaTensor = turns;
                link.inertiaTensorRotation = Quaternion.identity;
            }

            foreach (var g in grips)
            {
                float mass = 0f;
                Vector3 at = Vector3.zero;
                foreach (var p in g.Pieces)
                {
                    mass += p.Mass;
                    at += p.Mass * p.At;
                }

                at /= Mathf.Max(1e-5f, mass);
                Vector3 turns = Vector3.zero;
                foreach (var p in g.Pieces)
                {
                    turns += p.Turns + Spread(p.Mass, p.At - at);
                }

                g.Centre = at;
                g.Link.automaticCenterOfMass = false;
                g.Link.automaticInertiaTensor = false;
                g.Link.mass = mass;
                g.Link.centerOfMass = at;
                g.Link.inertiaTensor = Floored(turns);
                g.Link.inertiaTensorRotation = Quaternion.identity;
                // (What its own joint turns: itself about its grip.)
                g.Hangs = Floored(g.Link.inertiaTensor + Spread(mass, at));
            }

            // (At rest every link is turned as the pelvis is: a link's place against another is their rest places'.)
            var rest = new Vector3[n];
            for (int i = 1; i < n; i++)
            {
                rest[i] = rest[parent[i]] + rig.Part(parts[i]).restLocalPosition;
            }

            for (int j = 1; j < n; j++)
            {
                Vector3 sum = Vector3.zero;
                for (int k = j; k < n; k++)
                {
                    int up = k;
                    while (up > j)
                    {
                        up = parent[up];
                    }

                    if (up != j)
                    {
                        continue;
                    }

                    sum += links[k].inertiaTensor + Spread(links[k].mass, rest[k] - rest[j] + centre[k]);
                    foreach (var g in grips)
                    {
                        if (g.Hand == k)
                        {
                            sum += OnAxes(g.Link.inertiaTensor, g.Rest) + Spread(g.Link.mass, rest[k] - rest[j] + g.Link.transform.localPosition + g.Rest * g.Centre);
                        }
                    }
                }

                hangs[j] = Floored(sum);
                SetDrives(j);
            }

            // (And how much hangs from each joint, and where its centre is: what a move of the hips costs it.)
            if (carried == null || carried.Length != n)
            {
                carried = new float[n];
                lever = new float[n];
                reach = new float[n];
                more = new float[n];
                turning = new Vector3[n];
                rate = new Vector3[n];
            }

            for (int j = 1; j < n; j++)
            {
                float mass = 0f;
                Vector3 at = Vector3.zero;
                for (int k = j; k < n; k++)
                {
                    int up = k;
                    while (up > j)
                    {
                        up = parent[up];
                    }

                    if (up != j)
                    {
                        continue;
                    }

                    mass += links[k].mass;
                    at += links[k].mass * (rest[k] + centre[k]);
                    foreach (var g in grips)
                    {
                        if (g.Hand == k)
                        {
                            mass += g.Link.mass;
                            at += g.Link.mass * (rest[k] + g.Link.transform.localPosition + g.Rest * g.Centre);
                        }
                    }
                }

                at /= Mathf.Max(1e-5f, mass);
                carried[j] = mass;
                lever[j] = (at - rest[j]).magnitude;
                reach[j] = at.magnitude;
            }
        }

        /// <summary>
        /// What each muscle may turn with over its own strength this step: what it takes to do what the pose
        /// itself does. A muscle's strength (<see cref="SetMuscle"/>) is what it holds against the world with: a
        /// blade pressed on a shield, a body leant on. But the pose asks more of it than any such strength gives:
        /// a hit reaction snaps the torso over at 880 rad/s², a combo's cut carries the hips 18 cm in a step, and
        /// a muscle held to its strength then falls behind, and at its limit no longer damps: it overshoots and
        /// swings. So each is allowed, on top, the torque that turns what hangs from it as fast as the pose turns
        /// it, and that carries it as the hips are carried and turned, and what the joints further out are allowed
        /// (their muscles pull on this one).
        /// </summary>
        private void Allow(float dt)
        {
            int n = parts.Length;
            var pelvis = anim.Part(PartId.Pelvis);
            Vector3 speed = (pelvis.position - rootWas) / dt;
            var turned = pelvis.rotation * Quaternion.Inverse(rootTurnWas);
            Vector3 spin = JointMath.TurnVector(turned) / dt;
            float pushed = known && stepWas > 0f ? (speed - rootSpeed).magnitude / dt : 0f;
            float spun = known && stepWas > 0f ? (spin - rootSpin).magnitude / dt : 0f;
            rootSpeed = speed;
            rootSpin = spin;
            for (int i = 1; i < n; i++)
            {
                // (How fast the pose turns it: the turn itself, not its numbers' change. A turn past half a turn
                // is written with numbers that move several times as fast as it does.)
                Vector3 turns = rate[i];
                float quickens = known && stepWas > 0f ? (turns - turning[i]).magnitude / dt : 0f;
                turning[i] = turns;
                float heaviest = Mathf.Max(hangs[i].x, Mathf.Max(hangs[i].y, hangs[i].z));
                // (Its own turn; the hips carried: its load's weight on its lever; the hips turned: its load swung
                // about them, and turned with them.)
                more[i] = heaviest * quickens + carried[i] * lever[i] * (pushed + spun * reach[i]) + heaviest * spun;
            }

            for (int i = n - 1; i >= 1; i--)
            {
                if (parent[i] > 0)
                {
                    more[parent[i]] += more[i];
                }
            }

            for (int i = 1; i < n; i++)
            {
                // (And a wrist what turns the shield in its hand: see SetDrives.)
                float limit = most[i] > 0f ? most[i] + AllowShare * more[i] + (Returns && GripStrength > 0f && gripped[i] ? GripStrength : 0f) : float.MaxValue;
                for (int axis = 0; axis < 3; axis++)
                {
                    links[i].SetDriveForceLimit((ArticulationDriveAxis)axis, limit);
                }
            }

            stepWas = dt;
        }

        /// <summary>How much of what the pose's own motion takes each muscle is allowed over its strength (1.25: with a margin; 0: none, each held to its strength).</summary>
        public static float AllowShare = 1.25f;

        private static Vector3 Spread(float m, Vector3 d) => m * new Vector3(d.y * d.y + d.z * d.z, d.x * d.x + d.z * d.z, d.x * d.x + d.y * d.y);

        private static Vector3 Floored(Vector3 turns)
        {
            float least = InertiaFloor * Mathf.Max(turns.x, Mathf.Max(turns.y, turns.z));
            return new Vector3(Mathf.Max(turns.x, least), Mathf.Max(turns.y, least), Mathf.Max(turns.z, least));
        }

        /// <summary>A collider like <paramref name="source"/> under <paramref name="onto"/>, where the source sits against <paramref name="against"/>.</summary>
        private static Collider CopyCollider(Collider source, Transform against, Transform onto)
        {
            if (source == null)
            {
                return null;
            }

            var go = new GameObject(source.name + " (solid)") { layer = source.gameObject.layer };
            go.transform.SetParent(onto, false);
            go.transform.localPosition = against.InverseTransformPoint(source.transform.position);
            go.transform.localRotation = Quaternion.Inverse(against.rotation) * source.transform.rotation;
            go.transform.localScale = source.transform.lossyScale;
            Collider copy;
            if (source is BoxCollider box)
            {
                var c = go.AddComponent<BoxCollider>();
                c.center = box.center;
                c.size = box.size;
                copy = c;
            }
            else if (source is CapsuleCollider capsule)
            {
                var c = go.AddComponent<CapsuleCollider>();
                c.center = capsule.center;
                c.radius = capsule.radius;
                c.height = capsule.height;
                c.direction = capsule.direction;
                copy = c;
            }
            else if (source is SphereCollider sphere)
            {
                var c = go.AddComponent<SphereCollider>();
                c.center = sphere.center;
                c.radius = sphere.radius;
                copy = c;
            }
            else
            {
                UnityEngine.Object.Destroy(go);
                return null;
            }

            copy.sharedMaterial = source.sharedMaterial;
            return copy;
        }

        /// <summary>A collider as a solid of a given mass: its centre and its inertia about it, on the link's axes.</summary>
        private static Piece PieceOf(Collider collider, Transform link, float mass)
        {
            var t = collider.transform;
            Vector3 s = t.lossyScale;
            Vector3 centre, own;   // its centre (collider's frame, unscaled), its inertia on its own axes
            if (collider is BoxCollider box)
            {
                Vector3 size = Vector3.Scale(box.size, s);
                centre = box.center;
                own = mass / 12f * new Vector3(size.y * size.y + size.z * size.z, size.x * size.x + size.z * size.z, size.x * size.x + size.y * size.y);
            }
            else if (collider is CapsuleCollider capsule)
            {
                int along = capsule.direction;
                float r = capsule.radius * Mathf.Max(s[(along + 1) % 3], s[(along + 2) % 3]);
                float length = Mathf.Max(0f, capsule.height * s[along] - 2f * r);
                float tube = Mathf.PI * r * r * length, caps = 4f / 3f * Mathf.PI * r * r * r;
                float mTube = mass * tube / Mathf.Max(1e-9f, tube + caps), mCap = 0.5f * (mass - mTube);
                float about = 0.5f * mTube * r * r + 2f * mCap * 0.4f * r * r;
                float across = mTube * (length * length / 12f + r * r / 4f) + 2f * mCap * (0.4f * r * r + 0.25f * length * length + 0.375f * length * r);
                own = Vector3.one * across;
                own[along] = about;
                centre = capsule.center;
            }
            else
            {
                var sphere = (SphereCollider)collider;
                float r = sphere.radius * Mathf.Max(s.x, Mathf.Max(s.y, s.z));
                own = Vector3.one * (0.4f * mass * r * r);
                centre = sphere.center;
            }

            // (Onto the link's axes: the diagonal of the turned tensor.)
            var turns = OnAxes(own, Quaternion.Inverse(link.rotation) * t.rotation);
            return new Piece { Mass = mass, At = link.InverseTransformPoint(t.TransformPoint(centre)), Turns = turns };
        }

        /// <summary>An inertia on its own axes, turned by <paramref name="q"/>, as it is about the axes it is turned against (the diagonal of the turned tensor).</summary>
        private static Vector3 OnAxes(Vector3 own, Quaternion q)
        {
            Vector3 ax = q * Vector3.right, ay = q * Vector3.up, az = q * Vector3.forward;
            return new Vector3(
                ax.x * ax.x * own.x + ay.x * ay.x * own.y + az.x * az.x * own.z,
                ax.y * ax.y * own.x + ay.y * ay.y * own.y + az.y * az.y * own.z,
                ax.z * ax.z * own.x + ay.z * ay.z * own.y + az.z * az.z * own.z);
        }

        /// <summary>The pose's joint turns now: what the motors are driven to over the coming physics steps. Once a game step, after the pose.</summary>
        /// <param name="dt">The game step (seconds).</param>
        public void BeginStep(float dt)
        {
            // (A pose that jumps: a body turned at once onto another enemy, moved at once. Carried there in one game
            // step the whole upper body would be swung through whatever stands about it; it is put there instead.)
            var pelvis = anim.Part(PartId.Pelvis);
            if (known && ((pelvis.position - rootWas).sqrMagnitude > MostCarry * MostCarry || Quaternion.Angle(rootTurnWas, pelvis.rotation) > MostCarryTurn))
            {
                Jumps++;
                Teleport();
            }

            FollowHeld();
            for (int i = 1; i < parts.Length; i++)
            {
                var local = Quaternion.Inverse(anim.Part(parts[parent[i]]).rotation) * anim.Part(parts[i]).rotation;
                now[i] = JointMath.TurnVectorNear(JointMath.TurnVector(local), was[i]);
                // (A joint the pose turns at once: an arm's roll flipped as its hand is placed anew, more than any
                // swing turns in a step (the fastest clip, 42 degrees). No muscle turns that: driven there the arm
                // is whipped round and takes the body with it. It is put there, as the whole body is when the
                // pose moves it at once.)
                if (known && Quaternion.Angle(JointMath.FromTurnVector(was[i]), local) > MostJointTurn)
                {
                    JointJumps++;
                    LastJointJump = parts[i];
                    if (Debugging)
                    {
                        // (What kind of jump: its angle, and how far its axis is from the bone's own length and from the parent's.)
                        var before = JointMath.FromTurnVector(was[i]);
                        Vector3 own = JointMath.TurnVector(Quaternion.Inverse(before) * local);       // on the part's own axes
                        Vector3 parents = JointMath.TurnVector(local * Quaternion.Inverse(before));   // on its parent's
                        Vector3 bone = Vector3.zero, parentBone = rig.Part(parts[i]).restLocalPosition.normalized;
                        for (int c = i + 1; c < parts.Length; c++)
                        {
                            if (parent[c] == i) bone = rig.Part(parts[c]).restLocalPosition.normalized;
                        }

                        Debug.Log($"JUMPKIND {parts[i]}: {own.magnitude * Mathf.Rad2Deg:0}°, its axis {Vector3.Angle(own, bone):0}° from its own bone, {Vector3.Angle(parents, parentBone):0}° from its parent's; "
                            + $"world turn of the part {Quaternion.Angle(lastWorld[i], anim.Part(parts[i]).rotation):0}°, of its parent {Quaternion.Angle(lastWorld[parent[i]], anim.Part(parts[parent[i]]).rotation):0}°");
                    }
                    now[i] = JointMath.TurnVector(local);
                    was[i] = now[i];
                    turning[i] = Vector3.zero;
                    put[i] = true;
                    if (PutFlips)
                    {
                        // (Where the pose is now, as far aside of it as the joint was left.)
                        Vector3 at = Vector3.Scale(JointMath.TurnVector(local * slack[i]), ArticulationConventions.TargetSign);
                        links[i].jointPosition = new ArticulationReducedSpace(at.x, at.y, at.z);
                        links[i].jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
                    }
                }
            }

            for (int i = 1; i < parts.Length; i++)
            {
                rate[i] = known && dt > 1e-6f ? TurnBetween(was[i], now[i]) / dt : Vector3.zero;
                // (What a joint was left aside by comes back out of it: slowly while it is little, quicker when
                // it is much, or when the joint is held.)
                float aside = Quaternion.Angle(slack[i], Quaternion.identity) * Mathf.Deg2Rad;
                slack[i] = aside < 1e-4f ? Quaternion.identity
                    : Quaternion.Slerp(slack[i], Quaternion.identity, Mathf.Clamp01(ReturnRate(aside, tight[i], TautTime) * dt));
            }

            foreach (var g in grips)
            {
                g.Now = JointMath.TurnVectorNear(GripTurn(g), g.Was);
                g.Rate = known && dt > 1e-6f ? TurnBetween(g.Was, g.Now) / dt : Vector3.zero;
            }

            if (Debugging)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    lastWorld[i] = anim.Part(parts[i]).rotation;
                }
            }

            if ((links[0].transform.position - groundAt).sqrMagnitude > 3f * 3f)
            {
                ClearOfGround();
            }

            if (dt > 1e-6f)
            {
                Allow(dt);
            }

            if (!known)
            {
                // (Its first step: nothing to come from.)
                foreach (var g in grips)
                {
                    g.Was = g.Now;
                }

                for (int i = 1; i < parts.Length; i++)
                {
                    was[i] = now[i];
                    put[i] = false;
                }

                known = true;
            }
        }

        /// <summary>
        /// Before physics step <paramref name="k"/> of <paramref name="n"/> in this game step: the carried pelvis
        /// and each motor's target a share of the way to the new pose, the pose's own speed fed forward.
        /// </summary>
        /// <param name="step">The whole game step (seconds).</param>
        public void Substep(int k, int n, float step)
        {
            float share = k / (float)n;
            // (Where the pose has a joint as this physics step begins: what the joint is measured against. Where
            // the pose has it by the step's end is what its speed is for; measured against that, a joint the
            // pose turns 40 degrees in a step would be 10 degrees "behind" at each quarter of it.)
            float begun = Returns ? (k - 1) / (float)n : share;
            var pelvis = anim.Part(PartId.Pelvis);
            carrier.MovePosition(Vector3.Lerp(rootWas, pelvis.position, share));
            carrier.MoveRotation(Quaternion.Slerp(rootTurnWas, pelvis.rotation, share));
            Vector3 sign = ArticulationConventions.TargetSign;
            for (int i = 1; i < parts.Length; i++)
            {
                // (The turn asked for, named the way nearest where the joint is. A motor pulls each of its three
                // numbers toward the target's, and a turn has many names: one half a turn about and the same one
                // the other way round are 360 apart in numbers. Named by where the pose last was, a shoulder
                // that passed half a turn was sent the long way round at full strength, and round again: the
                // arm whirled, and its body with it.)
                Vector3 at = Where(links[i], links[parent[i]].transform, Quaternion.identity);
                Vector3 asked = JointMath.TurnVectorNear(Vector3.Lerp(was[i], now[i], begun), at);
                // (A wrist with a shield on it gives way to nothing: the shield is strapped along the forearm, and
                // a hand between its forearm's motor and its shield's is too light to be told from either. Left
                // its slack it was turned by the shield's motor each time the pose turned the board in the fist.)
                // (Nor does a joint the pose has just turned at once: it is not behind, it has been moved.)
                float gives = put[i] || gripped[i] ? float.MaxValue : GiveWay + GiveLag * rate[i].magnitude;
                if (Ask(links[i], asked, ref slack[i], gives, MostAside * (tight[i] > 0.5f ? HeldAside : 1f), at, rate[i], Mathf.Max(hangs[i].x, Mathf.Max(hangs[i].y, hangs[i].z)), most[i], sign))
                {
                    GaveWay++;
                    if (Debugging)
                    {
                        Debug.Log($"GAVE {rig.name} {parts[i]}: {lastGave * Mathf.Rad2Deg:0}° from where it was wanted, the pose turning it at {rate[i].magnitude:0} rad/s, quickening {(rate[i] - turning[i]).magnitude / Mathf.Max(1e-4f, step * n):0} rad/s2, most {links[i].xDrive.forceLimit:0} N m, substep {k}");
                    }
                }
            }

            foreach (var g in grips)
            {
                Vector3 at = Where(g.Link, links[g.Hand].transform, g.Rest);
                Vector3 asked = JointMath.TurnVectorNear(Vector3.Lerp(g.Was, g.Now, begun), at);
                // (A shield is strapped on: its grip gives way to nothing.)
                var none = Quaternion.identity;
                Ask(g.Link, asked, ref none, float.MaxValue, 0f, at, g.Rate, Mathf.Max(g.Hangs.x, Mathf.Max(g.Hangs.y, g.Hangs.z)), GripStrength, sign);

                if (k == n)
                {
                    g.Was = g.Now;
                }
            }

            if (k == n)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    was[i] = now[i];
                }

                rootWas = pelvis.position;
                rootTurnWas = pelvis.rotation;
            }
        }

        /// <summary>
        /// A joint's motor told what to do for the coming physics step: a speed, and no place. The motor is a
        /// damper alone: it takes off whatever speed the joint has beyond what it is asked, with all the muscle
        /// has and no more. The speed asked is the pose's own speed of turn, and on top of it what brings the
        /// joint to where it is wanted in a sixtieth of a second.
        ///
        /// Where it is wanted is its pose and its slack: how far aside of the pose it has been left. Pushed
        /// further from there than <paramref name="givesAt"/> (a blow, a blade that met a shield, the muscle not
        /// keeping up) the joint gives way: where it now is becomes where it is wanted, and its slack is that
        /// much more. The slack comes back out slowly (<see cref="BeginStep"/>, <see cref="ReturnRate"/>).
        ///
        /// So a blow that turns an arm 15 degrees leaves it 15 degrees aside: it goes on doing what the animation
        /// does, that much off, and comes back over a second or so. There is no spring to throw it back and past.
        ///
        /// (It had a spring before, and that was two faults. A spring strong enough to hold a pose asks for
        /// hundreds of times the muscle's strength a degree out; the motor gave its strength and the damper's
        /// share was lost in it, so a joint knocked off its pose swung to and fro about it. And Unity's engine
        /// writes a ball joint's turn anywhere from 0 to 360 degrees where the headless one keeps it under 180:
        /// past 180 the numbers move the other way to the turn, sideways of its axis, and the engine's spring,
        /// which works on the numbers, pushed the joint away from where it was sent. That was the whirl the
        /// headless fights never showed, and what was left of it on 12 November: a sword hand swinging 80 degrees
        /// to and fro for three seconds while its engine wrote it at 300.
        /// The speed alone, with nothing to bring the joint to its place, was tried and is not enough: the engine
        /// does not give a chain of joints quite the speeds they are asked, and an upper arm slipped 15 degrees
        /// about its own length in a windup with nothing touching it.)
        /// </summary>
        /// <param name="asked">Where the pose has the joint as the step begins (a turn vector, radians).</param>
        /// <param name="aside">The joint's slack (a turn on the part's own axes, after the pose's); made more when it gives way.</param>
        /// <param name="givesAt">How far from where it is wanted the joint gives way (radians).</param>
        /// <param name="mostAside">The most slack it is ever left (radians).</param>
        /// <param name="at">Where the joint is.</param>
        /// <param name="turns">How fast the pose turns it (rad/s, on the part's own axes).</param>
        /// <param name="inertia">What hangs from it (kg m2, about its heaviest axis).</param>
        /// <param name="strength">Its muscle's strength (N m; 0: no limit).</param>
        /// <returns>Whether it gave way.</returns>
        private static bool Ask(ArticulationBody link, Vector3 asked, ref Quaternion aside, float givesAt, float mostAside, Vector3 at, Vector3 turns, float inertia, float strength, Vector3 sign)
        {
            Vector3 target = asked, speed = turns * FeedForward;
            bool gave = false;
            if (Returns)
            {
                // (No spring: the place it is sent is the place it is, written as the engine writes it.)
                target = at;
                var here = JointMath.FromTurnVector(at);
                var pose = JointMath.FromTurnVector(asked);
                var wanted = pose * aside;
                // (The turn that takes it from where it is to where it is wanted, as a turn, on the part's own
                // axes or its parent's as the engine counts a speed.)
                Vector3 off = JointMath.TurnVector(OwnAxes ? Quaternion.Inverse(here) * wanted : wanted * Quaternion.Inverse(here));
                float far = off.magnitude;
                if (far > givesAt)
                {
                    lastGave = far;
                    // (As far aside as it now is, and no further than a joint is ever left: past that it is
                    // held where it is with all the muscle has, and whatever pushes it has that to push against.)
                    aside = Quaternion.Inverse(pose) * here;
                    float left = Quaternion.Angle(aside, Quaternion.identity) * Mathf.Deg2Rad;
                    if (left > mostAside)
                    {
                        aside = Quaternion.Slerp(Quaternion.identity, aside, mostAside / left);
                    }

                    wanted = pose * aside;
                    off = JointMath.TurnVector(OwnAxes ? Quaternion.Inverse(here) * wanted : wanted * Quaternion.Inverse(here));
                    far = off.magnitude;
                    gave = true;
                }

                // (The pose's speed of turn, on the axes of a part that much aside of it.)
                if (OwnAxes)
                {
                    speed = Quaternion.Inverse(aside) * speed;
                }

                if (far > 1e-5f)
                {
                    float back = Mathf.Min(MostReturn, far * Follow);
                    if (strength > 0f && inertia > 0f)
                    {
                        // (And no faster than it can still stop from before it is there.)
                        back = Mathf.Min(back, Mathf.Sqrt(2f * BrakeShare * strength / inertia * far));
                    }

                    speed += off * (back / far);
                }
            }

            target = Vector3.Scale(target, sign) * Mathf.Rad2Deg;
            speed = Vector3.Scale(speed, sign) * (Mathf.Rad2Deg * ArticulationConventions.SpeedScale);
            for (int axis = 0; axis < 3; axis++)
            {
                link.SetDriveTarget((ArticulationDriveAxis)axis, target[axis]);
                link.SetDriveTargetVelocity((ArticulationDriveAxis)axis, speed[axis]);
            }

            return gave;
        }

        /// <summary>
        /// How fast a joint's slack comes back out of it, as a share of the slack a second (1/s): 1 over
        /// <see cref="SlackTime"/> up to <see cref="SlackAngle"/> off, rising to 1 over <paramref name="within"/> at
        /// <see cref="TautAngle"/> and beyond; and 1 over <paramref name="within"/> throughout for a joint that is held.
        /// </summary>
        public static float ReturnRate(float far, float held, float within)
        {
            float taut = Mathf.Max(Mathf.Clamp01(held), Mathf.InverseLerp(SlackAngle, Mathf.Max(SlackAngle + 1e-3f, TautAngle), far));
            // (Smoothly: no step in its speed as it comes in through the angle.)
            taut = taut * taut * (3f - 2f * taut);
            return Mathf.Lerp(1f / Mathf.Max(0.05f, SlackTime), 1f / Mathf.Max(0.05f, within), taut);
        }

        /// <summary>The turn from one of a joint's turns to another, on the part's own axes (a turn vector, radians).</summary>
        private static Vector3 TurnBetween(Vector3 from, Vector3 to)
            => JointMath.TurnVector(OwnAxes ? Quaternion.Inverse(JointMath.FromTurnVector(from)) * JointMath.FromTurnVector(to)
                : JointMath.FromTurnVector(to) * Quaternion.Inverse(JointMath.FromTurnVector(from)));

        /// <summary>For the tests: a joint's speed of turn given to its motor on the other axes than the engine was measured to count it on.</summary>
        public static bool OtherAxes;

        /// <summary>A joint's speed of turn is given to its motor on the part's own axes, as the engine counts it (measured at start-up), or on its parent's.</summary>
        private static bool OwnAxes => ArticulationConventions.SpeedOnOwnAxes != OtherAxes;

        /// <summary>A joint's motor is a damper asked for a speed (see <see cref="Ask"/>); off, for the tests, a spring sent the pose itself however far off it is, as it first was.</summary>
        public static bool Returns = true;

        /// <summary>
        /// The slack in an arm (radians; 15 degrees). A joint this far off its pose or less is in no hurry: it
        /// comes back over <see cref="SlackTime"/>. Further off it comes back quicker, up to <see cref="TautAngle"/>.
        /// </summary>
        public static float SlackAngle = 15f * Mathf.Deg2Rad;

        /// <summary>The time a joint within its slack comes back in (seconds; 1: two thirds of the way in a second, nearly all of it in two).</summary>
        public static float SlackTime = 1f;

        /// <summary>How far off its pose a joint is taut (radians; 40 degrees): from here on it comes back in <see cref="TautTime"/>.</summary>
        public static float TautAngle = 40f * Mathf.Deg2Rad;

        /// <summary>The time a taut joint comes back in (seconds; 0.3): one far off its pose, and the torso and head, an arm inside its own body or past its joint's range at any angle.</summary>
        public static float TautTime = 0.3f;

        /// <summary>
        /// How far a joint is pushed from where it is wanted before it gives way and stays where it was pushed
        /// (radians; 8 degrees, and see <see cref="GiveLag"/>). Under it the push is undone at once (a sixtieth of a second): it has to be more
        /// than a muscle keeping up with its pose is ever behind by, or a fast cut is left behind.
        /// </summary>
        public static float GiveWay = 8f * Mathf.Deg2Rad;

        /// <summary>The most a joint is ever left aside of its pose (radians; 50 degrees), and the share of that for a joint that is held (0.4: the spine, chest and head, 20 degrees).</summary>
        public static float MostAside = 50f * Mathf.Deg2Rad, HeldAside = 0.4f;

        /// <summary>
        /// And this much further for each radian a second the pose turns the joint (seconds; 0.01: a joint turned
        /// at 30 rad/s gives way 17 degrees further out). A joint is behind a fast pose by a few thousandths of
        /// a second's turn with nothing in its way.
        /// </summary>
        public static float GiveLag = 0.01f;

        /// <summary>How fast a joint is brought to where it is wanted, as a share of how far from it it is a second (1/s; 60: all of it in a step).</summary>
        public static float Follow = 60f;

        private static float lastGave;

        /// <summary>Times a joint gave way (for the reports).</summary>
        public int GaveWay { get; private set; }

        /// <summary>
        /// How many times the damping of a spring of the motor's quickness the damper has, now that it is a damper
        /// alone. It is what makes a joint take up the speed it is asked for: the weaker it is, the longer a joint
        /// is behind a pose that quickens (at 1, a sword hand was 8 cm behind its pose half the time in a strike,
        /// touching nothing). It has no spring to be matched to, and the engine holds any damping steady.
        /// </summary>
        public static float DampTimes = 3f;

        /// <summary>The damper as strong about each of a joint's axes as about its heaviest (see <see cref="SetDrives"/>); off, for the tests, each axis by what hangs about it at rest.</summary>
        public static bool EvenDamping = true;

        /// <summary>How many times over the engine works the body's joints and contacts out each physics step, and its speeds.</summary>
        public static int SolverSteps = 32, SolverSpeedSteps = 4;

        /// <summary>The parts weigh nothing on their joints. A muscle holds its limb up without being asked; a motor that is a damper alone would let it sink until its drift back held it, a sword's point a hand's breadth low.</summary>
        public static bool Weightless = true;

        /// <summary>The share of a muscle's strength counted on to stop a joint coming back to its pose (0.5): the rest is left for the load and the spring.</summary>
        public static float BrakeShare = 0.5f;

        /// <summary>The fastest a joint is ever brought back to its pose (rad/s, on top of the pose's own turn; 20). Only one a long way off comes near it.</summary>
        public static float MostReturn = 20f;

        /// <summary>The fastest a joint may turn at all (rad/s). The fastest clip turns one at 44; nothing a muscle does is three times that, and a joint that is, is out of hand.</summary>
        public static float MostJointSpeed = 90f;

        /// <summary>The fastest a part found inside another is pushed out of it (m/s). Left to the engine a part put deep in a leg is thrown out at whatever speed clears it in one step.</summary>
        public static float MostPushOut = 3f;

        /// <summary>The body put where the pose is at once (a teleport, a spawn): carried there, its joints still driven to the pose.</summary>
        public void Teleport()
        {
            var pelvis = anim.Part(PartId.Pelvis);
            rootWas = pelvis.position;
            rootTurnWas = pelvis.rotation;
            carrier.position = rootWas;
            carrier.rotation = rootTurnWas;
            links[0].TeleportRoot(rootWas, rootTurnWas);
            // (And still: put back because it was out of hand, it is not to go on as it was going.)
            links[0].linearVelocity = Vector3.zero;
            links[0].angularVelocity = Vector3.zero;
            SnapJoints();
        }

        /// <summary>The rig's own upper parts moved onto its links (its shadows): what reads a part's place, and what is drawn, follows the physical body.</summary>
        public void MoveShadows()
        {
            for (int i = 0; i < parts.Length; i++)
            {
                var part = rig.Part(parts[i]);
                var t = links[i].transform;
                part.transform.SetPositionAndRotation(t.position, t.rotation);
            }

            // (What a hand turns in its grip is seen turned as its part is.)
            foreach (var g in grips)
            {
                g.Shown.rotation = g.Link.transform.rotation;
            }
        }

        /// <summary>How far what a hand grips is turned from where the pose turns it in the hand (degrees; 0: it grips nothing that turns).</summary>
        public float GripOff(PartId hand)
        {
            int i = Array.IndexOf(parts, hand);
            var g = i >= 0 ? grips.Find(x => x.Hand == i) : null;
            return g == null ? 0f : Quaternion.Angle(
                Quaternion.Inverse(links[i].transform.rotation) * g.Link.transform.rotation,
                Quaternion.Inverse(anim.Part(parts[i]).rotation) * g.Posed.rotation);
        }

        /// <summary>
        /// Where a joint is, as the turn vector its motor's target is compared with (radians, on the game's axes):
        /// the engine's own number for it where that reads as the targets do (measured at start-up), else the
        /// link's turn against its parent, from where it was built.
        /// </summary>
        private static Vector3 Where(ArticulationBody link, Transform parent, Quaternion built)
        {
            if (ArticulationConventions.PositionReadable)
            {
                var at = link.jointPosition;
                if (at.dofCount == 3)
                {
                    return Vector3.Scale(new Vector3(at[0], at[1], at[2]), ArticulationConventions.TargetSign);
                }
            }

            var turned = Quaternion.Inverse(parent.rotation) * link.transform.rotation;
            return JointMath.TurnVector(ArticulationConventions.TurnOnOwnAxes ? Quaternion.Inverse(built) * turned : turned * Quaternion.Inverse(built));
        }

        /// <summary>For the reports: a joint's target and where the engine has it (turn vectors, degrees), and its motor's stiffness and limit.</summary>
        public string Describe(PartId id)
        {
            int i = Array.IndexOf(parts, id);
            if (i < 1)
            {
                return "";
            }

            var at = links[i].jointPosition;
            var d = links[i].xDrive;
            Vector3 t = now[i] * Mathf.Rad2Deg;
            return $"{id} asked {t.x:0} {t.y:0} {t.z:0} ({t.magnitude:0}°) is {at[0] * Mathf.Rad2Deg:0} {at[1] * Mathf.Rad2Deg:0} {at[2] * Mathf.Rad2Deg:0}, {JointSpeed(i):0} rad/s, left {Quaternion.Angle(slack[i], Quaternion.identity):0}° aside{(tight[i] > 0.5f ? " (held)" : "")}, damping {d.damping:0} most {d.forceLimit:0}";
        }

        /// <summary>How fast a joint turns: its part's spin against the part it hangs from (rad/s).</summary>
        private float JointSpeed(int i) => (links[i].angularVelocity - links[parent[i]].angularVelocity).magnitude;

        /// <summary>The fastest any of its joints turns (rad/s), and which.</summary>
        public float FastestJoint(out PartId which)
        {
            float fastest = 0f;
            which = PartId.Pelvis;
            for (int i = 1; i < parts.Length; i++)
            {
                float speed = JointSpeed(i);
                if (speed > fastest)
                {
                    fastest = speed;
                    which = parts[i];
                }
            }

            return fastest;
        }

        /// <summary>
        /// For the tests: every joint set turning at <paramref name="speed"/> (rad/s), each about another axis,
        /// as a body thrown out of hand is. One that is sound comes back to its pose and stays.
        /// </summary>
        public void Kick(float speed)
        {
            for (int i = 1; i < parts.Length; i++)
            {
                Vector3 axis = i % 3 == 0 ? Vector3.right : i % 3 == 1 ? Vector3.up : Vector3.forward;
                float heaviest = Mathf.Max(hangs[i].x, Mathf.Max(hangs[i].y, hangs[i].z));
                links[i].AddTorque(links[i].transform.rotation * axis * (heaviest * speed * (i % 2 == 0 ? 1f : -1f)), ForceMode.Impulse);
            }
        }

        /// <summary>Every joint's target and where it is, for a trace.</summary>
        public string DescribeAll()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 1; i < parts.Length; i++)
            {
                sb.Append(Describe(parts[i])).Append("; ");
            }

            sb.Append(DescribeGrips());
            return sb.ToString();
        }

        /// <summary>A joint held to its pose (1) or left its slack (0): see <see cref="Ask"/>.</summary>
        public void SetTight(PartId id, float held)
        {
            int i = Array.IndexOf(parts, id);
            if (i > 0)
            {
                tight[i] = held;
            }
        }

        /// <summary>
        /// How deep any of some parts' solids (and what those hands hold) are in any of some others' (metres; 0:
        /// clear). Its own parts pass through each other, as the pose's do; an arm left off its pose is not to be
        /// left inside its own chest.
        /// </summary>
        public float Deepest(PartId[] these, PartId[] those)
        {
            float deepest = 0f;
            foreach (var a in linkOf)
            {
                if (a.Key == null || !a.Key.enabled || Array.IndexOf(these, parts[a.Value]) < 0)
                {
                    continue;
                }

                foreach (var b in linkOf)
                {
                    if (b.Key == null || !b.Key.enabled || Array.IndexOf(those, parts[b.Value]) < 0)
                    {
                        continue;
                    }

                    if (Physics.ComputePenetration(a.Key, a.Key.transform.position, a.Key.transform.rotation, b.Key, b.Key.transform.position, b.Key.transform.rotation, out _, out float depth))
                    {
                        deepest = Mathf.Max(deepest, depth);
                    }
                }
            }

            return deepest;
        }

        /// <summary>What its grips (a shield turned in its hand) are asked and where they are, for a trace.</summary>
        public string DescribeGrips()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var g in grips)
            {
                var at = g.Link.jointPosition;
                var d = g.Link.xDrive;
                Vector3 t = g.Now * Mathf.Rad2Deg;
                sb.Append($"grip asked {t.x:0} {t.y:0} {t.z:0} ({t.magnitude:0}°) is {at[0] * Mathf.Rad2Deg:0} {at[1] * Mathf.Rad2Deg:0} {at[2] * Mathf.Rad2Deg:0}, k {d.stiffness:0} most {d.forceLimit:0}");
            }

            return sb.ToString();
        }

        /// <summary>How far a part is from where the pose has it (metres).</summary>
        public float Off(PartId id) => Vector3.Distance(Link(id).transform.position, anim.Part(id).position);

        public void Destroy()
        {
            if (Root != null)
            {
                // (Gone from the engine at once: destroyed only at the frame's end, it would stand a step in the way of the body built in its place.)
                foreach (var c in colliders)
                {
                    if (c != null)
                    {
                        c.enabled = false;
                    }
                }

                UnityEngine.Object.Destroy(Root);
            }
        }
    }
}
