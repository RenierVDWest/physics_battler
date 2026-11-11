// The plan "A different approach", step 2, the second build: the sword arm as a PhysX articulation (reduced
// coordinates: its joints can't come apart, its motors are sized by the engine for everything that hangs from them and
// hold very stiff gains). Built straight in the engine for the experiment: the chest carried by the pose, the upper
// arm, the forearm and the hand with its sword on ball joints driven to the pose's own joint turns.
#if PHYSX
using System;
using MagicPhysX;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using UnityEngine;
using static MagicPhysX.NativeMethods;

namespace PhysicsBattler.Engine
{
    internal sealed unsafe class ArticulatedArm
    {
        private static readonly PartId[] Chain = { PartId.Chest, PartId.UpperArmR, PartId.ForearmR, PartId.HandR };
        private readonly PhysXWorld world;
        private readonly AnimRig anim;
        private readonly PxArticulationLink*[] links = new PxArticulationLink*[4];
        private readonly PxRigidDynamic* carrier;
        private readonly Vector3[] was = new Vector3[4], now = new Vector3[4];
        private Vector3 chestWas;
        private Quaternion chestTurnWas;
        private readonly Vector3 bladeFrom;
        private readonly Quaternion bladeTurn;
        private readonly float length;

        /// <summary>The blade's shape id in the engine's filter (to tell it from other things).</summary>
        public readonly uint BladeId;

        /// <param name="hz">How quick its motors are (the frequency they would swing at undamped).</param>
        /// <param name="mostTorque">The most the shoulder's motor turns with (N·m; 0: no limit); the elbow's and wrist's their <see cref="JointShare"/> of it.</param>
        public ArticulatedArm(PhysXWorld world, AnimRig anim, UnitRig rig, float bladeLength, float hz, float mostTorque, float swordMass = 1.2f)
        {
            this.world = world;
            this.anim = anim;
            length = bladeLength;
            var physics = world.Px;
            var art = PxPhysics_createArticulationReducedCoordinate_mut(physics);
            PxArticulationReducedCoordinate_setSolverIterationCounts_mut(art, 32, 4);
            var chest = anim.Part(PartId.Chest);
            Vector3 at = chest.position;
            Quaternion turn = chest.rotation;
            chestWas = at;
            chestTurnWas = turn;
            uint[] ids = new uint[4];
            for (int i = 0; i < 4; i++)
            {
                var part = rig.Part(Chain[i]);
                if (i > 0)
                {
                    at += turn * part.restLocalPosition;
                }

                var pose = PhysXWorld.Pose(at, turn);
                links[i] = PxArticulationReducedCoordinate_createLink_mut(art, i > 0 ? links[i - 1] : null, &pose);
                ids[i] = world.NewId();
                bool shaped = false;
                if (i > 0 && part.partCollider != null)
                {
                    byte* geo = stackalloc byte[PhysXWorld.GeoSize];
                    if (PhysXWorld.ShapeOf(part.partCollider, (PxGeometry*)geo, out var offset, out var geoTurn))
                    {
                        var t = part.partCollider.transform;
                        // (Where the collider sits on its part: the two at rest.)
                        var inv = Quaternion.Inverse(part.transform.rotation);
                        Shape(links[i], (PxGeometry*)geo, inv * (t.position + t.rotation * offset - part.transform.position), inv * t.rotation * geoTurn, ids[i]);
                        shaped = true;
                    }
                }

                float mass = part.body.mass + (Chain[i] == PartId.HandR ? swordMass : 0f);
                if (Chain[i] == PartId.HandR)
                {
                    // Its sword: a solid blade from above the grip to the point, where the hand's slot holds it.
                    var slot = rig.SlotWeaponR;
                    bladeFrom = slot.localPosition;
                    bladeTurn = slot.localRotation;
                    var capsule = PxCapsuleGeometry_new(0.012f, 0.5f * (length - 0.12f));
                    BladeId = world.NewId();
                    Shape(links[i], (PxGeometry*)&capsule, bladeFrom + bladeTurn * new Vector3(0f, 0f, 0.5f * (length + 0.12f)),
                        bladeTurn * Quaternion.FromToRotation(Vector3.right, Vector3.forward), BladeId);
                    shaped = true;
                }

                if (Environment.GetEnvironmentVariable("ART_NOGRAV") != null)
                {
                    PxActor_setActorFlag_mut((PxActor*)links[i], PxActorFlag.DisableGravity, true);
                }

                if (!shaped || !PxRigidBodyExt_setMassAndUpdateInertia_1((PxRigidBody*)links[i], mass, null, false))
                {
                    PxRigidBody_setMass_mut((PxRigidBody*)links[i], mass);
                    var inertia = PhysXWorld.Vec(Vector3.one * (0.1f * mass));
                    PxRigidBody_setMassSpaceInertiaTensor_mut((PxRigidBody*)links[i], &inertia);
                }

                if (Chain[i] == PartId.HandR && SwordBalance >= 0f)
                {
                    // The hand and its sword, their weight where a sword's is: a sword balances a hand's breadth past its
                    // guard, not halfway along its blade, and turns about that point like a rod a third its length.
                    Vector3 way = (bladeTurn * Vector3.forward).normalized;
                    float handMass = part.body.mass;
                    Vector3 handAt = 0.5f * bladeFrom, swordAt = bladeFrom + way * SwordBalance;
                    Vector3 centre = (handMass * handAt + swordMass * swordAt) / (handMass + swordMass);
                    float handTurns = 0.4f * handMass * 0.05f * 0.05f, swordTurns = swordMass * SwordGyration * SwordGyration;
                    Vector3 Spread(float m, Vector3 d) => m * new Vector3(d.y * d.y + d.z * d.z, d.x * d.x + d.z * d.z, d.x * d.x + d.y * d.y);
                    Vector3 across = new Vector3(1f - way.x * way.x, 1f - way.y * way.y, 1f - way.z * way.z);
                    Vector3 whole = Vector3.one * handTurns + swordTurns * across + Spread(handMass, handAt - centre) + Spread(swordMass, swordAt - centre);
                    PxRigidBody_setMass_mut((PxRigidBody*)links[i], handMass + swordMass);
                    var centred = PhysXWorld.Pose(centre, Quaternion.identity);
                    PxRigidBody_setCMassLocalPose_mut((PxRigidBody*)links[i], &centred);
                    var wholeTensor = PhysXWorld.Vec(whole);
                    PxRigidBody_setMassSpaceInertiaTensor_mut((PxRigidBody*)links[i], &wholeTensor);
                    if (Environment.GetEnvironmentVariable("ARM_TRACE") != null && !toldSword)
                    {
                        toldSword = true;
                        Console.WriteLine($"    the hand and sword: {handMass + swordMass:0.00} kg, centred {centre} from the wrist (the blade's way {way}), turning {whole} kg m2");
                    }
                }
                else if (StraightMass)
                {
                    // (Its mass's own frame turned back onto the part's: the engine's ball joint goes wrong when a part's
                    // inertia is given in a frame turned against the part, as a capsule's is.)
                    var cm = PxRigidBody_getCMassLocalPose((PxRigidBody*)links[i]);
                    var q = PhysXWorld.Quat(cm.q);
                    var d = PhysXWorld.Vec(PxRigidBody_getMassSpaceInertiaTensor((PxRigidBody*)links[i]));
                    Vector3 ax = q * Vector3.right, ay = q * Vector3.up, az = q * Vector3.forward;
                    var straight = new Vector3(
                        ax.x * ax.x * d.x + ay.x * ay.x * d.y + az.x * az.x * d.z,
                        ax.y * ax.y * d.x + ay.y * ay.y * d.y + az.y * az.y * d.z,
                        ax.z * ax.z * d.x + ay.z * ay.z * d.y + az.z * az.z * d.z);
                    var plain = PhysXWorld.Pose(PhysXWorld.Vec(cm.p), Quaternion.identity);
                    PxRigidBody_setCMassLocalPose_mut((PxRigidBody*)links[i], &plain);
                    var tensor0 = PhysXWorld.Vec(straight);
                    PxRigidBody_setMassSpaceInertiaTensor_mut((PxRigidBody*)links[i], &tensor0);
                }

                // (No part turns about one of its axes much more easily than about the others: a thin blade's inertia about
                // its own length is next to nothing, and a stiff motor on next to nothing rings.)
                if (InertiaFloor > 0f)
                {
                    var tensor = PhysXWorld.Vec(PxRigidBody_getMassSpaceInertiaTensor((PxRigidBody*)links[i]));
                    float most = Mathf.Max(tensor.x, Mathf.Max(tensor.y, tensor.z)), least = InertiaFloor * most;
                    var floored = PhysXWorld.Vec(new Vector3(Mathf.Max(tensor.x, least), Mathf.Max(tensor.y, least), Mathf.Max(tensor.z, least)));
                    PxRigidBody_setMassSpaceInertiaTensor_mut((PxRigidBody*)links[i], &floored);
                }

                if (i > 0)
                {
                    var joint = PxArticulationLink_getInboundJoint(links[i]);
                    PxArticulationJointReducedCoordinate_setJointType_mut(joint, PxArticulationJointType.Spherical);
                    var onParent = PhysXWorld.Pose(part.restLocalPosition, Quaternion.identity);
                    var onChild = PhysXWorld.Pose(Vector3.zero, Quaternion.identity);
                    PxArticulationJointReducedCoordinate_setParentPose_mut(joint, &onParent);
                    PxArticulationJointReducedCoordinate_setChildPose_mut(joint, &onChild);
                    float w = 2f * Mathf.PI * hz;
                    var drive = new PxArticulationDrive { stiffness = w * w, damping = 2f * w, maxForce = mostTorque > 0f ? mostTorque * JointShare[i] : float.MaxValue, driveType = PxArticulationDriveType.Acceleration };
                    for (int axis = 0; axis < 3; axis++)
                    {
                        PxArticulationJointReducedCoordinate_setMotion_mut(joint, (PxArticulationAxis)axis, PxArticulationMotion.Free);
                        PxArticulationJointReducedCoordinate_setDriveParams_mut(joint, (PxArticulationAxis)axis, &drive);
                    }

                    PxArticulationJointReducedCoordinate_setMaxJointVelocity_mut(joint, 200f);
                    if (Armature > 0f)
                    {
                        for (int axis = 0; axis < 3; axis++)
                        {
                            PxArticulationJointReducedCoordinate_setArmature_mut(joint, (PxArticulationAxis)axis, Armature);
                        }
                    }
                }
            }

            // (Its own parts don't stop each other: the blade passes its own arm, as the pose's does.)
            for (int i = 0; i < 4; i++)
            {
                world.IgnoreIds(BladeId, ids[i]);
                for (int j = i + 1; j < 4; j++)
                {
                    world.IgnoreIds(ids[i], ids[j]);
                }
            }

            // (Nor does the body it is the arm of: its own posed parts are where this arm is.)
            foreach (var part in rig.Parts)
            {
                if (part.partCollider != null)
                {
                    world.IgnoreWith(part.partCollider, BladeId);
                    for (int i = 0; i < 4; i++)
                    {
                        world.IgnoreWith(part.partCollider, ids[i]);
                    }
                }
            }

            // (Its motors' limits are torques, as Unity's are, not the engine's default: a turn's worth of push a step.)
            PxArticulationReducedCoordinate_setArticulationFlag_mut(art, PxArticulationFlag.DriveLimitsAreForces, true);
            PxScene_addArticulation_mut(world.Scene, art);

            // The chest carried by the pose: locked to a kinematic body that goes where the posed chest goes.
            var start = PhysXWorld.Pose(chest.position, chest.rotation);
            carrier = PxPhysics_createRigidDynamic_mut(physics, &start);
            PxRigidBody_setRigidBodyFlag_mut((PxRigidBody*)carrier, PxRigidBodyFlag.Kinematic, true);
            PxScene_addActor_mut(world.Scene, (PxActor*)carrier, null);
            var same = PhysXWorld.Pose(Vector3.zero, Quaternion.identity);
            phys_PxD6JointCreate(physics, (PxRigidActor*)carrier, &same, (PxRigidActor*)links[0], &same);
            world.BeforeSubstep = Drive;
        }

        private void Shape(PxArticulationLink* link, PxGeometry* geo, Vector3 at, Quaternion turn, uint id)
        {
            var shape = PxPhysics_createShape_mut(world.Px, geo, world.Material, true,
                Environment.GetEnvironmentVariable("ART_NOSHAPES") != null ? PxShapeFlags.SceneQueryShape : PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);
            var local = PhysXWorld.Pose(at, turn);
            PxShape_setLocalPose_mut(shape, &local);
            PxShape_setContactOffset_mut(shape, Physics.defaultContactOffset);
            PxShape_setRestOffset_mut(shape, 0f);
            var data = new PxFilterData { word0 = id, word1 = 0 };
            PxShape_setSimulationFilterData_mut(shape, &data);
            PxRigidActor_attachShape_mut((PxRigidActor*)link, shape);
        }

        /// <summary>Which way a joint's turn is given to its motor (+1: the child's against its parent; found by trying).</summary>
        public static float Sign = 1f;

        private void Drive(int k, int n, float dt)
        {
            float share = k / (float)n;
            if (k == 1)
            {
                // The pose's joint turns now, each as a turn vector (its axis times its angle), kept near the last.
                for (int i = 1; i < 4; i++)
                {
                    var local = Quaternion.Inverse(anim.Part(Chain[i - 1]).rotation) * anim.Part(Chain[i]).rotation;
                    now[i] = Map == 0 ? Near(TurnVector(local) * Sign, was[i]) : Angles(local);
                }
            }

            var chest = anim.Part(PartId.Chest);
            var to = PhysXWorld.Pose(Vector3.Lerp(chestWas, chest.position, share), Quaternion.Slerp(chestTurnWas, chest.rotation, share));
            PxRigidDynamic_setKinematicTarget_mut(carrier, &to);
            float step = dt * n;
            for (int i = 1; i < 4; i++)
            {
                var joint = PxArticulationLink_getInboundJoint(links[i]);
                Vector3 target = Vector3.Lerp(was[i], now[i], share), speed = (now[i] - was[i]) / step;
                for (int axis = 0; axis < 3; axis++)
                {
                    PxArticulationJointReducedCoordinate_setDriveTarget_mut(joint, (PxArticulationAxis)axis, target[axis], true);
                    PxArticulationJointReducedCoordinate_setDriveVelocity_mut(joint, (PxArticulationAxis)axis, FeedForward ? speed[axis] : 0f, true);
                }
            }

            if (k == n)
            {
                for (int i = 1; i < 4; i++)
                {
                    was[i] = now[i];
                }

                chestWas = chest.position;
                chestTurnWas = chest.rotation;
            }
        }

        public static bool FeedForward = true;

        /// <summary>How far past the grip a sword balances (metres; below 0: its weight spread evenly along its blade), and its radius of gyration about that point.</summary>
        public static float SwordBalance = float.TryParse(Environment.GetEnvironmentVariable("ART_BALANCE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float sb) ? sb : 0.18f;

        public static float SwordGyration = 0.26f;

        private static bool toldSword;

        /// <summary>Each joint's share of the arm's strength (the chest, then shoulder, elbow, wrist): a person's shoulder turns with about 110 N m, the elbow 80, the wrist 25.</summary>
        public static readonly float[] JointShare = { 1f, 1f, 0.7f, 0.25f };

        public static bool StraightMass = Environment.GetEnvironmentVariable("ART_TURNED_MASS") == null;

        public static float InertiaFloor = float.TryParse(Environment.GetEnvironmentVariable("ART_FLOOR"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fl) ? fl : 0.2f;

        public static float Armature = float.TryParse(Environment.GetEnvironmentVariable("ART_ARMATURE"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float ar) ? ar : 0.02f;

        /// <summary>How a joint's turn is written for its motor (found by trying): 0 a turn vector; 1 to 3 its twist and swings, in turn.</summary>
        public static int Map;

        private static Vector3 Angles(Quaternion q)
        {
            if (Map == 3)
            {
                // Twist about x, then the swing left over, as a turn vector in y and z.
                var twist = new Quaternion(q.x, 0f, 0f, q.w);
                float n = Mathf.Sqrt(twist.x * twist.x + twist.w * twist.w);
                twist = n > 1e-6f ? new Quaternion(twist.x / n, 0f, 0f, twist.w / n) : Quaternion.identity;
                var swing = q * Quaternion.Inverse(twist);
                var sv = TurnVector(swing);
                return new Vector3(2f * Mathf.Atan2(twist.x, twist.w), sv.y, sv.z);
            }

            // Euler angles about x, y, z: q = Rx Ry Rz (1) or q = Rz Ry Rx (2).
            Vector3 r0 = q * Vector3.right, r1 = q * Vector3.up, r2 = q * Vector3.forward;   // the columns
            float m00 = r0.x, m01 = r1.x, m02 = r2.x, m10 = r0.y, m11 = r1.y, m12 = r2.y, m20 = r0.z, m21 = r1.z, m22 = r2.z;
            if (Map == 1)
            {
                float y = Mathf.Asin(Mathf.Clamp(m02, -1f, 1f));
                return new Vector3(Mathf.Atan2(-m12, m22), y, Mathf.Atan2(-m01, m00));
            }

            float yy = Mathf.Asin(Mathf.Clamp(-m20, -1f, 1f));
            return new Vector3(Mathf.Atan2(m21, m22), yy, Mathf.Atan2(m10, m00));
        }

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

        /// <summary>The same turn as <paramref name="v"/>, written the way nearest <paramref name="last"/> (a turn and it plus a whole turn about its axis are one).</summary>
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

        public Vector3 PartAt(int i, out Quaternion turn)
        {
            var pose = PxRigidActor_getGlobalPose((PxRigidActor*)links[i]);
            turn = PhysXWorld.Quat(pose.q);
            return PhysXWorld.Vec(pose.p);
        }

        /// <summary>The blade now: its grip end (the hand's slot), its way and its point.</summary>
        public void Blade(out Vector3 grip, out Quaternion turn, out Vector3 tip)
        {
            Vector3 hand = PartAt(3, out var handTurn);
            grip = hand + handTurn * bladeFrom;
            turn = handTurn * bladeTurn;
            tip = grip + turn * Vector3.forward * length;
        }

        /// <summary>For finding the joints' convention: each joint's target and where the engine says it is.</summary>
        public string Debug()
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 1; i < 4; i++)
            {
                var joint = PxArticulationLink_getInboundJoint(links[i]);
                var at = new Vector3(PxArticulationJointReducedCoordinate_getJointPosition(joint, PxArticulationAxis.Twist),
                    PxArticulationJointReducedCoordinate_getJointPosition(joint, PxArticulationAxis.Swing1), PxArticulationJointReducedCoordinate_getJointPosition(joint, PxArticulationAxis.Swing2));
                PartAt(i - 1, out var parent);
                PartAt(i, out var child);
                sb.Append($"\n      {Chain[i]}: target {now[i] * Mathf.Rad2Deg} deg, the engine's joint position {at * Mathf.Rad2Deg} deg, its real turn as a turn vector {TurnVector(Quaternion.Inverse(parent) * child) * Mathf.Rad2Deg}");
            }

            return sb.ToString();
        }

        public Vector3 HandSpeed => PhysXWorld.Vec(PxRigidBody_getLinearVelocity((PxRigidBody*)links[3]));

        /// <summary>How far each joint is off the pose's (degrees): shoulder, elbow, wrist.</summary>
        public Vector3 JointsOff()
        {
            var off = Vector3.zero;
            for (int i = 1; i < 4; i++)
            {
                PartAt(i - 1, out var parent);
                PartAt(i, out var child);
                var posed = Quaternion.Inverse(anim.Part(Chain[i - 1]).rotation) * anim.Part(Chain[i]).rotation;
                off[i - 1] = Quaternion.Angle(posed, Quaternion.Inverse(parent) * child);
            }

            return off;
        }
    }
}
#endif
