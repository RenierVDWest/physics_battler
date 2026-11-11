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
                link.useGravity = true;
                link.linearDamping = 0f;
                link.angularDamping = 0.05f;
                link.jointFriction = 0f;
                link.maxAngularVelocity = 100f;
                // (What it will meet in the coming step is found ahead: a blade at 30 m/s moves 12 cm in a physics step, further than a shield is thick.)
                link.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                link.maxJointVelocity = 200f;
                if (i == 0)
                {
                    link.solverIterations = 32;
                    link.solverVelocityIterations = 4;
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
        public void Settle() => SnapJoints();

        /// <summary>Every joint put where the pose has it, at once and still (as it is built, and when the body is put somewhere).</summary>
        private void SnapJoints()
        {
            Vector3 sign = ArticulationConventions.TargetSign;
            for (int i = 1; i < parts.Length; i++)
            {
                var local = Quaternion.Inverse(anim.Part(parts[parent[i]]).rotation) * anim.Part(parts[i]).rotation;
                now[i] = was[i] = JointMath.TurnVector(local);
                Vector3 at = Vector3.Scale(now[i], sign);
                links[i].jointPosition = new ArticulationReducedSpace(at.x, at.y, at.z);
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
            for (int axis = 0; axis < 3; axis++)
            {
                float turns = hangs[i][axis];
                links[i].SetDriveStiffness((ArticulationDriveAxis)axis, w * w * turns * scale);
                links[i].SetDriveDamping((ArticulationDriveAxis)axis, 2f * w * turns * scale);
                links[i].SetDriveForceLimit((ArticulationDriveAxis)axis, most[i] > 0f ? most[i] : float.MaxValue);
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
                    float turns = g.Hangs[axis];
                    g.Link.SetDriveStiffness((ArticulationDriveAxis)axis, w * w * turns * scale);
                    g.Link.SetDriveDamping((ArticulationDriveAxis)axis, 2f * w * turns * scale);
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
            public Vector3 Was, Now, Hangs, Centre;
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
                link.useGravity = true;
                link.linearDamping = 0f;
                link.angularDamping = 0.05f;
                link.jointFriction = 0f;
                link.maxAngularVelocity = 100f;
                link.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                link.maxJointVelocity = 200f;
                link.jointType = ArticulationJointType.SphericalJoint;
                link.anchorPosition = Vector3.zero;
                link.anchorRotation = Quaternion.identity;
                link.twistLock = ArticulationDofLock.FreeMotion;
                link.swingYLock = ArticulationDofLock.FreeMotion;
                link.swingZLock = ArticulationDofLock.FreeMotion;
                grip = new Grip { Hand = i, Link = link, Posed = posed, Shown = shown, Rest = rest };
                grips.Add(grip);
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
            for (int axis = 0; axis < 3; axis++)
            {
                g.Link.SetDriveTarget((ArticulationDriveAxis)axis, at[axis] * Mathf.Rad2Deg);
                g.Link.SetDriveTargetVelocity((ArticulationDriveAxis)axis, 0f);
            }
        }

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
        }

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
        public void BeginStep()
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
            }

            foreach (var g in grips)
            {
                g.Now = JointMath.TurnVectorNear(GripTurn(g), g.Was);
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
            var pelvis = anim.Part(PartId.Pelvis);
            carrier.MovePosition(Vector3.Lerp(rootWas, pelvis.position, share));
            carrier.MoveRotation(Quaternion.Slerp(rootTurnWas, pelvis.rotation, share));
            Vector3 sign = ArticulationConventions.TargetSign;
            for (int i = 1; i < parts.Length; i++)
            {
                Vector3 target = Vector3.Scale(Vector3.Lerp(was[i], now[i], share), sign) * Mathf.Rad2Deg;
                Vector3 speed = step > 1e-6f ? Vector3.Scale(now[i] - was[i], sign) * (Mathf.Rad2Deg / step) : Vector3.zero;
                for (int axis = 0; axis < 3; axis++)
                {
                    links[i].SetDriveTarget((ArticulationDriveAxis)axis, target[axis]);
                    links[i].SetDriveTargetVelocity((ArticulationDriveAxis)axis, speed[axis]);
                }
            }

            foreach (var g in grips)
            {
                Vector3 target = Vector3.Scale(Vector3.Lerp(g.Was, g.Now, share), sign) * Mathf.Rad2Deg;
                Vector3 speed = step > 1e-6f ? Vector3.Scale(g.Now - g.Was, sign) * (Mathf.Rad2Deg / step) : Vector3.zero;
                for (int axis = 0; axis < 3; axis++)
                {
                    g.Link.SetDriveTarget((ArticulationDriveAxis)axis, target[axis]);
                    g.Link.SetDriveTargetVelocity((ArticulationDriveAxis)axis, speed[axis]);
                }

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

        /// <summary>The body put where the pose is at once (a teleport, a spawn): carried there, its joints still driven to the pose.</summary>
        public void Teleport()
        {
            var pelvis = anim.Part(PartId.Pelvis);
            rootWas = pelvis.position;
            rootTurnWas = pelvis.rotation;
            carrier.position = rootWas;
            carrier.rotation = rootTurnWas;
            links[0].TeleportRoot(rootWas, rootTurnWas);
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
