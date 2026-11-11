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

        /// <summary>No axis of a part, or of what hangs from a joint, turns with less than this share of the most (a thin blade about its own length rings on a stiff motor).</summary>
        public const float InertiaFloor = 0.2f;

        private readonly UnitRig rig;
        private readonly AnimRig anim;
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
            int n = Parts.Length;
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
                var part = rig.Part(Parts[i]);
                parent[i] = i == 0 ? -1 : Array.IndexOf(Parts, PartInfo.Parent(Parts[i]));
                var go = new GameObject(Parts[i] + " (link)") { layer = part.gameObject.layer };
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

                (hz[i], most[i]) = i > 0 ? muscle(Parts[i]) : (0f, 0f);
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
        }

        /// <summary>A part's link.</summary>
        public ArticulationBody Link(PartId id) => links[Array.IndexOf(Parts, id)];

        /// <summary>Whether a part is one of its own.</summary>
        public static bool Has(PartId id) => Array.IndexOf(Parts, id) >= 0;

        /// <summary>Every collider on it (its parts', and what its hands hold).</summary>
        public IReadOnlyList<Collider> Colliders => colliders;

        /// <summary>What hangs from a part's joint, as an inertia about it on the part's three axes (kg m2).</summary>
        public Vector3 Hangs(PartId id) => hangs[Array.IndexOf(Parts, id)];

        /// <summary>A part's motor: how quick, and the most it turns with (N m; 0: no limit).</summary>
        public void SetMuscle(PartId id, float quick, float mostTorque)
        {
            int i = Array.IndexOf(Parts, id);
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
        }

        /// <summary>
        /// Something solid put in a hand (a sword's blade, a shield's board): its collider copied onto the hand's
        /// link where it is now against the rig's hand, with its weight.
        /// </summary>
        /// <param name="balance">Where its weight is, against the rig's hand (null: its collider's middle).</param>
        /// <param name="gyration">Its radius of gyration about that point across its length (metres; 0: as a solid of its collider's shape).</param>
        public Collider Hold(PartId hand, Collider solid, float mass, Vector3? balance = null, float gyration = 0f)
        {
            int i = Array.IndexOf(Parts, hand);
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
            held.Add((i, copy, pieces[i].Count - 1));
            RefreshMass();
            return copy;
        }

        private readonly List<(int link, Collider collider, int piece)> held = new List<(int, Collider, int)>();

        /// <summary>What a hand held is let go: its collider and its weight taken off the hand's link.</summary>
        public void LetGo(PartId hand)
        {
            int i = Array.IndexOf(Parts, hand);
            for (int k = held.Count - 1; k >= 0; k--)
            {
                if (held[k].link != i)
                {
                    continue;
                }

                colliders.Remove(held[k].collider);
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
            int n = Parts.Length;
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

            // (At rest every link is turned as the pelvis is: a link's place against another is their rest places'.)
            var rest = new Vector3[n];
            for (int i = 1; i < n; i++)
            {
                rest[i] = rest[parent[i]] + rig.Part(Parts[i]).restLocalPosition;
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
            var q = Quaternion.Inverse(link.rotation) * t.rotation;
            Vector3 ax = q * Vector3.right, ay = q * Vector3.up, az = q * Vector3.forward;
            var turns = new Vector3(
                ax.x * ax.x * own.x + ay.x * ay.x * own.y + az.x * az.x * own.z,
                ax.y * ax.y * own.x + ay.y * ay.y * own.y + az.y * az.y * own.z,
                ax.z * ax.z * own.x + ay.z * ay.z * own.y + az.z * az.z * own.z);
            return new Piece { Mass = mass, At = link.InverseTransformPoint(t.TransformPoint(centre)), Turns = turns };
        }

        /// <summary>The pose's joint turns now: what the motors are driven to over the coming physics steps. Once a game step, after the pose.</summary>
        public void BeginStep()
        {
            for (int i = 1; i < Parts.Length; i++)
            {
                var local = Quaternion.Inverse(anim.Part(Parts[parent[i]]).rotation) * anim.Part(Parts[i]).rotation;
                now[i] = JointMath.TurnVectorNear(JointMath.TurnVector(local), was[i]);
            }

            if (!known)
            {
                // (Its first step: nothing to come from.)
                for (int i = 1; i < Parts.Length; i++)
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
            for (int i = 1; i < Parts.Length; i++)
            {
                Vector3 target = Vector3.Scale(Vector3.Lerp(was[i], now[i], share), sign) * Mathf.Rad2Deg;
                Vector3 speed = step > 1e-6f ? Vector3.Scale(now[i] - was[i], sign) * (Mathf.Rad2Deg / step) : Vector3.zero;
                for (int axis = 0; axis < 3; axis++)
                {
                    links[i].SetDriveTarget((ArticulationDriveAxis)axis, target[axis]);
                    links[i].SetDriveTargetVelocity((ArticulationDriveAxis)axis, speed[axis]);
                }
            }

            if (k == n)
            {
                for (int i = 1; i < Parts.Length; i++)
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
        }

        /// <summary>The rig's own upper parts moved onto its links (its shadows): what reads a part's place, and what is drawn, follows the physical body.</summary>
        public void MoveShadows()
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                var part = rig.Part(Parts[i]);
                var t = links[i].transform;
                part.transform.SetPositionAndRotation(t.position, t.rotation);
            }
        }

        /// <summary>How far a part is from where the pose has it (metres).</summary>
        public float Off(PartId id) => Vector3.Distance(Link(id).transform.position, anim.Part(id).position);

        public void Destroy()
        {
            if (Root != null)
            {
                UnityEngine.Object.Destroy(Root);
            }
        }
    }
}
