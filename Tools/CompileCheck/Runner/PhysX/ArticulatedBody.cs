// The plan "A different approach", step 3: a body's upper half as one PhysX articulation, on legs the pose carries.
// The pelvis is its root, locked to a body that goes where the posed pelvis goes; the spine, chest, head and both
// arms hang from it on ball joints, each driven toward the pose's own joint turn by a motor of limited strength.
// What was found building the sword arm (ArticulatedArm) holds here: a joint's position is the child's turn against
// its parent as a turn vector; a part's inertia in the part's own frame; a little armature on each joint; the motors'
// limits told to be torques.
#if PHYSX
using System;
using System.Collections.Generic;
using MagicPhysX;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using UnityEngine;
using static MagicPhysX.NativeMethods;

namespace PhysicsBattler.Engine
{
    internal sealed unsafe class ArticulatedBody
    {
        /// <summary>The upper body, each part after the one it hangs from.</summary>
        public static readonly PartId[] UpperBody =
        {
            PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head,
            PartId.UpperArmL, PartId.ForearmL, PartId.HandL, PartId.UpperArmR, PartId.ForearmR, PartId.HandR,
        };

        private readonly PhysXWorld world;
        private readonly AnimRig anim;
        private readonly PartId[] parts;
        private readonly int[] parent;
        private readonly PxArticulationLink*[] links;
        private readonly PxArticulationReducedCoordinate* art;
        private readonly PxRigidDynamic* carrier;
        private readonly PxD6Joint* hold;
        private readonly Vector3[] was, now;
        private readonly Vector3[] hangs;
        private readonly uint[] ids;
        private Vector3 rootWas;
        private Quaternion rootTurnWas;

        /// <summary>The push the pelvis has passed on to what carries it (the legs) since it was last cleared: N s, world.</summary>
        public Vector3 PassedOn;

        /// <summary>The most its pelvis pulled on what carries it (N).</summary>
        public float MostHold;

        public const float Armature = 0.02f, InertiaFloor = 0.2f;

        /// <param name="muscle">Each part's motor: how quick (the frequency it would swing at undamped, Hz) and the most it turns with (N m; 0: no limit).</param>
        public ArticulatedBody(PhysXWorld world, AnimRig anim, UnitRig rig, PartId[] parts, Func<PartId, (float hz, float most)> muscle)
        {
            this.world = world;
            this.anim = anim;
            this.parts = parts;
            int n = parts.Length;
            parent = new int[n];
            links = new PxArticulationLink*[n];
            was = new Vector3[n];
            now = new Vector3[n];
            ids = new uint[n];
            var physics = world.Px;
            art = PxPhysics_createArticulationReducedCoordinate_mut(physics);
            PxArticulationReducedCoordinate_setSolverIterationCounts_mut(art, 32, 4);
            var root = anim.Part(parts[0]);
            rootWas = root.position;
            rootTurnWas = root.rotation;
            var at = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                var part = rig.Part(parts[i]);
                parent[i] = i == 0 ? -1 : Array.IndexOf(parts, PartInfo.Parent(parts[i]));
                if (i > 0 && parent[i] < 0)
                {
                    throw new ArgumentException($"{parts[i]} hangs from a part that isn't in the body");
                }

                // (Built as the body is at rest, every part turned as the root is: the joints' turns are then the pose's.)
                at[i] = i == 0 ? rootWas : at[parent[i]] + rootTurnWas * part.restLocalPosition;
                var pose = PhysXWorld.Pose(at[i], rootTurnWas);
                links[i] = PxArticulationReducedCoordinate_createLink_mut(art, i > 0 ? links[parent[i]] : null, &pose);
                ids[i] = world.NewId();
                bool shaped = false;
                if (part.partCollider != null)
                {
                    byte* geo = stackalloc byte[PhysXWorld.GeoSize];
                    if (PhysXWorld.ShapeOf(part.partCollider, (PxGeometry*)geo, out var offset, out var geoTurn))
                    {
                        var t = part.partCollider.transform;
                        var inv = Quaternion.Inverse(part.transform.rotation);
                        Shape(links[i], (PxGeometry*)geo, inv * (t.position + t.rotation * offset - part.transform.position), inv * t.rotation * geoTurn, ids[i]);
                        shaped = true;
                    }
                }

                float mass = part.body.mass;
                if (!shaped || !PxRigidBodyExt_setMassAndUpdateInertia_1((PxRigidBody*)links[i], mass, null, false))
                {
                    PxRigidBody_setMass_mut((PxRigidBody*)links[i], mass);
                    var plainTensor = PhysXWorld.Vec(Vector3.one * (0.1f * mass));
                    PxRigidBody_setMassSpaceInertiaTensor_mut((PxRigidBody*)links[i], &plainTensor);
                }

                Straighten(links[i]);
                if (i > 0)
                {
                    var joint = PxArticulationLink_getInboundJoint(links[i]);
                    PxArticulationJointReducedCoordinate_setJointType_mut(joint, PxArticulationJointType.Spherical);
                    var onParent = PhysXWorld.Pose(part.restLocalPosition, Quaternion.identity);
                    var onChild = PhysXWorld.Pose(Vector3.zero, Quaternion.identity);
                    PxArticulationJointReducedCoordinate_setParentPose_mut(joint, &onParent);
                    PxArticulationJointReducedCoordinate_setChildPose_mut(joint, &onChild);
                    for (int axis = 0; axis < 3; axis++)
                    {
                        PxArticulationJointReducedCoordinate_setMotion_mut(joint, (PxArticulationAxis)axis, PxArticulationMotion.Free);
                        PxArticulationJointReducedCoordinate_setArmature_mut(joint, (PxArticulationAxis)axis, Armature);
                    }

                    PxArticulationJointReducedCoordinate_setMaxJointVelocity_mut(joint, 200f);
                }
            }

            // What each joint turns: everything that hangs from it, as an inertia about the joint, on the part's own
            // three axes. The motors are sized by it: the engine's own "acceleration" motors size themselves for the
            // one part below the joint, not for all that hangs from it (a spine asked for 6 Hz let the body sag 63
            // degrees under its own weight: the chest, head and arms are a hundred times the spine's own inertia).
            hangs = new Vector3[n];
            var inv0 = Quaternion.Inverse(rootTurnWas);
            for (int j = 1; j < n; j++)
            {
                Vector3 sum = Vector3.zero;
                for (int k = j; k < n; k++)
                {
                    int up = k;
                    while (up > j && up >= 0) up = parent[up];
                    if (up != j) continue;
                    float m = PxRigidBody_getMass((PxRigidBody*)links[k]);
                    var own = PhysXWorld.Vec(PxRigidBody_getMassSpaceInertiaTensor((PxRigidBody*)links[k]));
                    Vector3 d = at[k] - at[j] + rootTurnWas * PhysXWorld.Vec(PxRigidBody_getCMassLocalPose((PxRigidBody*)links[k]).p);
                    d = inv0 * d;
                    sum += own + m * new Vector3(d.y * d.y + d.z * d.z, d.x * d.x + d.z * d.z, d.x * d.x + d.y * d.y);
                }

                float least = InertiaFloor * Mathf.Max(sum.x, Mathf.Max(sum.y, sum.z));
                hangs[j] = new Vector3(Mathf.Max(sum.x, least), Mathf.Max(sum.y, least), Mathf.Max(sum.z, least));
                var (hz, most) = muscle(parts[j]);
                SetMuscle(parts[j], hz, most);
            }

            // (Its own parts don't stop each other, as the pose's don't; nor do the posed parts they stand in for.)
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    world.IgnoreIds(ids[i], ids[j]);
                }
            }

            foreach (var part in rig.Parts)
            {
                if (part.partCollider != null)
                {
                    for (int i = 0; i < n; i++)
                    {
                        world.IgnoreWith(part.partCollider, ids[i]);
                    }
                }
            }

            PxArticulationReducedCoordinate_setArticulationFlag_mut(art, PxArticulationFlag.DriveLimitsAreForces, true);
            PxArticulationReducedCoordinate_setSleepThreshold_mut(art, 0f);
            PxScene_addArticulation_mut(world.Scene, art);

            // The pelvis carried by the pose: locked to a body that goes where the posed pelvis goes.
            var start = PhysXWorld.Pose(rootWas, rootTurnWas);
            carrier = PxPhysics_createRigidDynamic_mut(physics, &start);
            PxRigidBody_setRigidBodyFlag_mut((PxRigidBody*)carrier, PxRigidBodyFlag.Kinematic, true);
            PxScene_addActor_mut(world.Scene, (PxActor*)carrier, null);
            var same = PhysXWorld.Pose(Vector3.zero, Quaternion.identity);
            hold = phys_PxD6JointCreate(physics, (PxRigidActor*)carrier, &same, (PxRigidActor*)links[0], &same);
            world.BeforeSubstep += Drive;
        }

        /// <summary>Lets go of the engine's step (the experiment's next world is another's).</summary>
        public void Release() => world.BeforeSubstep -= Drive;

        /// <summary>What hangs from a part's joint, as an inertia about it on the part's three axes (kg m2).</summary>
        public Vector3 Hangs(PartId id) => hangs[Array.IndexOf(parts, id)];

        /// <summary>A part's motor: how quick (the frequency all that hangs from its joint would swing at undamped; it is damped just enough not to swing), and the most it turns with (N m; 0: no limit).</summary>
        public void SetMuscle(PartId id, float hz, float most)
        {
            int i = Array.IndexOf(parts, id);
            if (i <= 0)
            {
                return;
            }

            var joint = PxArticulationLink_getInboundJoint(links[i]);
            float w = 2f * Mathf.PI * hz;
            for (int axis = 0; axis < 3; axis++)
            {
                float turns = hangs[i][axis] + Armature;
                var drive = new PxArticulationDrive { stiffness = w * w * turns, damping = 2f * w * turns, maxForce = most > 0f ? most : float.MaxValue, driveType = PxArticulationDriveType.Force };
                PxArticulationJointReducedCoordinate_setDriveParams_mut(joint, (PxArticulationAxis)axis, &drive);
            }
        }

        /// <summary>A part's mass turned as the part is, and no axis much easier to turn about than the others (see ArticulatedArm).</summary>
        private static void Straighten(PxArticulationLink* link)
        {
            var cm = PxRigidBody_getCMassLocalPose((PxRigidBody*)link);
            var q = PhysXWorld.Quat(cm.q);
            var d = PhysXWorld.Vec(PxRigidBody_getMassSpaceInertiaTensor((PxRigidBody*)link));
            Vector3 ax = q * Vector3.right, ay = q * Vector3.up, az = q * Vector3.forward;
            var straight = new Vector3(
                ax.x * ax.x * d.x + ay.x * ay.x * d.y + az.x * az.x * d.z,
                ax.y * ax.y * d.x + ay.y * ay.y * d.y + az.y * az.y * d.z,
                ax.z * ax.z * d.x + ay.z * ay.z * d.y + az.z * az.z * d.z);
            float least = InertiaFloor * Mathf.Max(straight.x, Mathf.Max(straight.y, straight.z));
            straight = new Vector3(Mathf.Max(straight.x, least), Mathf.Max(straight.y, least), Mathf.Max(straight.z, least));
            var plain = PhysXWorld.Pose(PhysXWorld.Vec(cm.p), Quaternion.identity);
            PxRigidBody_setCMassLocalPose_mut((PxRigidBody*)link, &plain);
            var tensor = PhysXWorld.Vec(straight);
            PxRigidBody_setMassSpaceInertiaTensor_mut((PxRigidBody*)link, &tensor);
        }

        private void Shape(PxArticulationLink* link, PxGeometry* geo, Vector3 at, Quaternion turn, uint id)
        {
            var shape = PxPhysics_createShape_mut(world.Px, geo, world.Material, true, PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
            var local = PhysXWorld.Pose(at, turn);
            PxShape_setLocalPose_mut(shape, &local);
            PxShape_setContactOffset_mut(shape, Physics.defaultContactOffset);
            PxShape_setRestOffset_mut(shape, 0f);
            var data = new PxFilterData { word0 = id, word1 = 0 };
            PxShape_setSimulationFilterData_mut(shape, &data);
            PxRigidActor_attachShape_mut((PxRigidActor*)link, shape);
        }

        private void Drive(int k, int n, float dt)
        {
            // (What the pelvis pulled on its carrier with over the last physics step: what the legs are asked to take.)
            PxVec3 pull, twist;
            PxConstraint_getForce(PxJoint_getConstraint((PxJoint*)hold), &pull, &twist);
            var force = PhysXWorld.Vec(pull);
            if (!float.IsNaN(force.x) && force.sqrMagnitude < 1e16f)
            {
                PassedOn += force * dt;
                MostHold = Mathf.Max(MostHold, force.magnitude);
            }

            float share = k / (float)n;
            if (k == 1)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    var local = Quaternion.Inverse(anim.Part(parts[parent[i]]).rotation) * anim.Part(parts[i]).rotation;
                    now[i] = Near(TurnVector(local), was[i]);
                }
            }

            var root = anim.Part(parts[0]);
            var to = PhysXWorld.Pose(Vector3.Lerp(rootWas, root.position, share), Quaternion.Slerp(rootTurnWas, root.rotation, share));
            PxRigidDynamic_setKinematicTarget_mut(carrier, &to);
            float step = dt * n;
            for (int i = 1; i < parts.Length; i++)
            {
                var joint = PxArticulationLink_getInboundJoint(links[i]);
                Vector3 target = Vector3.Lerp(was[i], now[i], share), speed = (now[i] - was[i]) / step;
                for (int axis = 0; axis < 3; axis++)
                {
                    PxArticulationJointReducedCoordinate_setDriveTarget_mut(joint, (PxArticulationAxis)axis, target[axis], true);
                    PxArticulationJointReducedCoordinate_setDriveVelocity_mut(joint, (PxArticulationAxis)axis, speed[axis], true);
                }
            }

            if (k == n)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    was[i] = now[i];
                }

                rootWas = root.position;
                rootTurnWas = root.rotation;
            }
        }

        /// <summary>Where a part is now, and how it is turned.</summary>
        public Vector3 PartAt(PartId id, out Quaternion turn)
        {
            var pose = PxRigidActor_getGlobalPose((PxRigidActor*)links[Array.IndexOf(parts, id)]);
            turn = PhysXWorld.Quat(pose.q);
            return PhysXWorld.Vec(pose.p);
        }

        /// <summary>How far a part is from where the pose has it (metres), and by what (world).</summary>
        public Vector3 Off(PartId id) => PartAt(id, out _) - anim.Part(id).position;

        /// <summary>How far a part's joint is turned from the pose's (degrees).</summary>
        public float JointOff(PartId id)
        {
            int i = Array.IndexOf(parts, id);
            PartAt(parts[parent[i]], out var up);
            PartAt(id, out var mine);
            var posed = Quaternion.Inverse(anim.Part(parts[parent[i]]).rotation) * anim.Part(id).rotation;
            return Quaternion.Angle(posed, Quaternion.Inverse(up) * mine);
        }

        public Vector3 Velocity(PartId id) => PhysXWorld.Vec(PxRigidBody_getLinearVelocity((PxRigidBody*)links[Array.IndexOf(parts, id)]));

        public IReadOnlyList<PartId> Parts => parts;

        private static Vector3 TurnVector(Quaternion q)
        {
            if (q.w < 0f)
            {
                q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
            }

            var v = new Vector3(q.x, q.y, q.z);
            float s = v.magnitude;
            return s < 1e-7f ? Vector3.zero : v / s * (2f * Mathf.Atan2(s, q.w));
        }

        /// <summary>The same turn as <paramref name="v"/>, written the way nearest <paramref name="last"/>.</summary>
        private static Vector3 Near(Vector3 v, Vector3 last)
        {
            float angle = v.magnitude;
            if (angle < 1e-6f)
            {
                return v;
            }

            Vector3 axis = v / angle, best = v;
            for (int k = -1; k <= 1; k += 2)
            {
                Vector3 other = axis * (angle + k * 2f * Mathf.PI);
                if ((other - last).sqrMagnitude < (best - last).sqrMagnitude)
                {
                    best = other;
                }
            }

            return best;
        }
    }
}
#endif
