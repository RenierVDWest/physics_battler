// The real PhysX engine under the Unity stubs (the plan "A different approach", step 1: eyes on real physics).
// Built only when Tools/CompileCheck/ThirdParty/MagicPhysX is there (get-physx.sh fetches it): PhysX 5 through
// Cysharp's MagicPhysX bindings. Every stub Rigidbody, Collider and ConfigurableJoint is mirrored into PhysX as Unity
// itself would set it up (Unity's joints are PhysX D6 joints), each game step simulated in one or more physics steps,
// and the result written back to the stub transforms and velocities.
#if PHYSX
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicPhysX;
using UnityEngine;
using static MagicPhysX.NativeMethods;

namespace PhysicsBattler.Engine
{
    /// <summary>One contact the engine solved in the last game step.</summary>
    public struct EngineContact
    {
        public Collider A, B;
        public Vector3 Point, Normal, Impulse;
        public float Separation;
    }

    public sealed unsafe partial class PhysXWorld : IPhysicsBackend
    {
        /// <summary>The world the engine's callbacks speak to (one at a time).</summary>
        public static PhysXWorld Current { get; private set; }

        /// <summary>Physics steps in each game step (1: Unity's own 60 a second; 4: 240).</summary>
        public int Substeps = 1;

        /// <summary>The contacts of the last game step (every physics step's, in order).</summary>
        public readonly List<EngineContact> Contacts = new List<EngineContact>();

        /// <summary>Physics steps simulated so far.</summary>
        public long Steps { get; private set; }

        // (One foundation and one physics for the process, as PhysX has it; a world is a scene of its own.)
        private static PxFoundation* foundation;
        private static PxPhysics* physics;
        private readonly PxScene* scene;
        private readonly PxMaterial* material;

        private sealed class BodyState
        {
            public Rigidbody Body;
            public PxRigidDynamic* Actor;
            public Vector3 Pos, Vel, AngVel, Before;
            public Quaternion Rot;
            public bool Kinematic, Gravity, Seen;
            public float Mass, LinDamp, AngDamp, MaxAng, MaxDepen;
            public int Iters, VelIters, Depth;
            public CollisionDetectionMode Ccd;
            public bool MassDirty;
        }

        private sealed class ShapeState
        {
            public Collider Collider;
            public PxShape* Shape;
            public PxRigidActor* Actor;
            public BodyState Owner;
            public uint Id;
            public bool Trigger, Enabled, Seen;
            public int Layer;
        }

        private sealed class JointState
        {
            public ConfigurableJoint Joint;
            public PxD6Joint* Px;
            public Rigidbody Connected;
            public Vector3 Axis, Secondary, Anchor;
            public ConfigurableJointMotion[] Motion = new ConfigurableJointMotion[6];
            public float LowX, HighX, LimY, LimZ;
            public JointDrive Slerp, AngX, AngYZ;
            public RotationDriveMode Mode;
            public Quaternion Target;
            public Vector3 TargetSpin;
            public bool Collide, Preprocess, Seen, First = true;
        }

        private struct Push
        {
            public BodyState Body;
            public Vector3 Force, At;
            public ForceMode Mode;
            public int Kind;   // 0 force, 1 force at a point, 2 torque
        }

        private readonly Dictionary<Rigidbody, BodyState> bodies = new Dictionary<Rigidbody, BodyState>();
        private readonly Dictionary<Collider, ShapeState> shapes = new Dictionary<Collider, ShapeState>();
        private readonly Dictionary<IntPtr, ShapeState> byShape = new Dictionary<IntPtr, ShapeState>();
        private readonly Dictionary<Joint, JointState> joints = new Dictionary<Joint, JointState>();
        private readonly List<BodyState> ordered = new List<BodyState>();
        private readonly List<Push> pushes = new List<Push>();
        private readonly HashSet<ulong> ignored = new HashSet<ulong>();
        private readonly HashSet<(Collider, Collider)> ignoredPairs = new HashSet<(Collider, Collider)>();
        private readonly bool[,] layerIgnored = new bool[32, 32];
        private uint nextId = 1;
        private bool filteringDirty;

        /// <summary>
        /// The engine put under the stubs, with flat ground at height 0 on <paramref name="groundLayer"/> (a collider of
        /// the stubs', so the game's scripts hear of bodies meeting it).
        /// </summary>
        public static PhysXWorld Install(int groundLayer, int substeps = 1)
        {
            var world = new PhysXWorld(false) { Substeps = substeps };
            Physics.Backend = world;
            var ground = new GameObject("Ground") { layer = groundLayer };
            var box = ground.AddComponent<BoxCollider>();
            box.size = new Vector3(400f, 2f, 400f);
            box.center = new Vector3(0f, -1f, 0f);
            return world;
        }

        public PhysXWorld(bool ground = true)
        {
            if (physics == null)
            {
                foundation = physx_create_foundation();
                physics = physx_create_physics(foundation);
            }

            if (Current != null)
            {
                Current.Close();
            }

            var desc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));
            var g = Physics.gravity;
            desc.gravity = V(g);
            // (No worker threads: everything on the caller's thread, so a run repeats exactly and the callbacks are safe.)
            desc.cpuDispatcher = (PxCpuDispatcher*)phys_PxDefaultCpuDispatcherCreate(0, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
            desc.solverType = PxSolverType.Tgs;                       // the project's Physics settings: Temporal Gauss Seidel
            desc.bounceThresholdVelocity = Physics.bounceThreshold;
            desc.flags |= PxSceneFlags.EnableEnhancedDeterminism | PxSceneFlags.EnablePcm | PxSceneFlags.EnableCcd;
            enable_custom_filter_shader(&desc, &Filter, 1);
            var events = new SimulationEventCallbackInfo { collision_callback = &OnContact };
            desc.simulationEventCallback = create_simulation_event_callbacks(&events);
            scene = PxPhysics_createScene_mut(physics, &desc);
            material = PxPhysics_createMaterial_mut(physics, 0.6f, 0.6f, 0f);   // Unity's default material
            if (ground)
            {
                var plane = PxPlane_new_1(0f, 1f, 0f, 0f);
                var actor = phys_PxCreatePlane(physics, &plane, material);
                PxScene_addActor_mut(scene, (PxActor*)actor, null);
            }

            Current = this;
        }

        // ---- For what is built straight in the engine (the articulation experiment) ---------------------------------

        internal PxPhysics* Px => physics;
        internal PxScene* Scene => scene;
        internal PxMaterial* Material => material;

        /// <summary>Called before each physics step of a game step: which one (from 1), of how many, and the physics step's length.</summary>
        public Action<int, int, float> BeforeSubstep;

        internal uint NewId() => nextId++;

        internal void IgnoreIds(uint a, uint b) => extraIgnored.Add(Key(a, b));

        /// <summary>A collider of the stubs' and a shape built straight in the engine don't stop each other.</summary>
        internal void IgnoreWith(Collider stub, uint id)
        {
            pendingIgnore.Add((stub, id));
            filteringDirty = true;
        }

        private readonly List<(Collider, uint)> pendingIgnore = new List<(Collider, uint)>();

        private readonly HashSet<ulong> extraIgnored = new HashSet<ulong>();

        internal static PxVec3 Vec(Vector3 v) => V(v);
        internal static Vector3 Vec(PxVec3 v) => U(v);
        internal static Quaternion Quat(PxQuat q) => U(q);
        internal static PxTransform Pose(Vector3 p, Quaternion q) => T(p, q);

        /// <summary>A collider's shape for the engine, with where it sits against the collider's own transform.</summary>
        internal static bool ShapeOf(Collider collider, PxGeometry* into, out Vector3 offset, out Quaternion turn) => MakeGeo(collider, (Geo*)into, out offset, out turn);

        internal const int GeoSize = 64;

        /// <summary>The world let go (a new one takes its place): its scene released, with everything in it.</summary>
        private void Close()
        {
            foreach (var j in joints.Values)
            {
                Release(j);
            }

            joints.Clear();
            PxScene_release_mut(scene);
        }

        // ---- The stubs' calls ------------------------------------------------------------------------------------

        public void AddForce(Rigidbody body, Vector3 force, ForceMode mode) => Queue(body, force, default, mode, 0);

        public void AddForceAtPosition(Rigidbody body, Vector3 force, Vector3 position, ForceMode mode) => Queue(body, force, position, mode, 1);

        public void AddTorque(Rigidbody body, Vector3 torque, ForceMode mode) => Queue(body, torque, default, mode, 2);

        private void Queue(Rigidbody body, Vector3 force, Vector3 at, ForceMode mode, int kind)
        {
            // (A body the engine hasn't met yet is met at the next step, with the push waiting for it.)
            pushes.Add(new Push { Body = StateOf(body), Force = force, At = at, Mode = mode, Kind = kind });
        }

        public Vector3 PointVelocity(Rigidbody body, Vector3 worldPoint)
        {
            if (!bodies.TryGetValue(body, out var s) || body.isKinematic)
            {
                return body.linearVelocity;
            }

            return body.linearVelocity + Vector3.Cross(body.angularVelocity, worldPoint - WorldCenterOfMass(body));
        }

        public Vector3 InertiaTensor(Rigidbody body)
        {
            if (!bodies.TryGetValue(body, out var s))
            {
                Sync();
                s = bodies[body];
            }

            return U(PxRigidBody_getMassSpaceInertiaTensor((PxRigidBody*)s.Actor));
        }

        public Vector3 WorldCenterOfMass(Rigidbody body)
        {
            if (!bodies.TryGetValue(body, out var s))
            {
                return body.position;
            }

            var local = PxRigidBody_getCMassLocalPose((PxRigidBody*)s.Actor);
            return body.position + body.rotation * U(local.p);
        }

        public void IgnoreCollision(Collider a, Collider b, bool ignore)
        {
            if (a == null || b == null)
            {
                return;
            }

            if (ignore ? ignoredPairs.Add((a, b)) : (ignoredPairs.Remove((a, b)) | ignoredPairs.Remove((b, a))))
            {
                filteringDirty = true;
            }
        }

        public void IgnoreLayerCollision(int a, int b, bool ignore)
        {
            layerIgnored[a, b] = layerIgnored[b, a] = ignore;
            filteringDirty = true;
        }

        public bool ComputePenetration(Collider a, Vector3 positionA, Quaternion rotationA, Collider b, Vector3 positionB, Quaternion rotationB,
            out Vector3 direction, out float distance)
        {
            direction = Vector3.zero;
            distance = 0f;
            Geo ga, gb;
            if (!MakeGeo(a, &ga, out var offA, out var turnA) || !MakeGeo(b, &gb, out var offB, out var turnB))
            {
                return false;
            }

            var pa = T(positionA + rotationA * offA, rotationA * turnA);
            var pb = T(positionB + rotationB * offB, rotationB * turnB);
            PxVec3 dir;
            float depth;
            if (!PxGeometryQuery_computePenetration(&dir, &depth, (PxGeometry*)&ga, &pa, (PxGeometry*)&gb, &pb, (PxGeometryQueryFlags)0))
            {
                return false;
            }

            direction = U(dir);
            distance = depth;
            return true;
        }

        /// <summary>How deep two colliders overlap where they are now (metres; 0: not at all).</summary>
        public float Overlap(Collider a, Collider b) =>
            ComputePenetration(a, a.transform.position, a.transform.rotation, b, b.transform.position, b.transform.rotation, out _, out float d) ? d : 0f;

        // ---- A game step -----------------------------------------------------------------------------------------

        public void Simulate(float step)
        {
            Sync();
            Contacts.Clear();
            int n = Mathf.Max(1, Substeps);
            for (int k = 1; k <= n; k++)
            {
                float share = k / (float)n;
                foreach (var s in ordered)
                {
                    if (!s.Kinematic)
                    {
                        continue;
                    }

                    // (A kinematic body goes to where its stub was moved, a physics step at a time: moved by
                    // MovePosition it sweeps what it meets along; placed by its position, it is there at once.)
                    var to = T(Vector3.Lerp(s.Pos, s.Body.position, share), Quaternion.Slerp(s.Rot, s.Body.rotation, share));
                    if (s.Body.MovedThisStep)
                    {
                        PxRigidDynamic_setKinematicTarget_mut(s.Actor, &to);
                    }
                    else if (k == 1 && (Differs(s.Pos, s.Body.position) || Differs(s.Rot, s.Body.rotation)))
                    {
                        var at = T(s.Body.position, s.Body.rotation);
                        PxRigidActor_setGlobalPose_mut((PxRigidActor*)s.Actor, &at, true);
                    }
                }

                foreach (var p in pushes)
                {
                    bool once = p.Mode == ForceMode.Impulse || p.Mode == ForceMode.VelocityChange;
                    if (p.Body == null || p.Body.Kinematic || p.Body.Actor == null || (once && k > 1))
                    {
                        continue;
                    }

                    var f = V(p.Force);
                    var mode = p.Mode == ForceMode.Impulse ? PxForceMode.Impulse : p.Mode == ForceMode.VelocityChange ? PxForceMode.VelocityChange
                        : p.Mode == ForceMode.Acceleration ? PxForceMode.Acceleration : PxForceMode.Force;
                    if (p.Kind == 2)
                    {
                        PxRigidBody_addTorque_mut((PxRigidBody*)p.Body.Actor, &f, mode, true);
                    }
                    else if (p.Kind == 1 && (mode == PxForceMode.Force || mode == PxForceMode.Impulse))
                    {
                        var at = V(p.At);
                        PxRigidBodyExt_addForceAtPos((PxRigidBody*)p.Body.Actor, &f, &at, mode, true);
                    }
                    else
                    {
                        PxRigidBody_addForce_mut((PxRigidBody*)p.Body.Actor, &f, mode, true);
                    }
                }

                BeforeSubstep?.Invoke(k, n, step / n);
                DriveArticulations(k);
                PxScene_simulate_mut(scene, step / n, null, null, 0, true);
                uint error = 0;
                PxScene_fetchResults_mut(scene, true, &error);
                Steps++;
            }

            pushes.Clear();
            ReadBack();
            ReadBackArticulations();
            Tell();
        }

        // ---- Collisions told to the game's scripts, as Unity tells them ----------------------------------------------

        private HashSet<(Collider, Collider)> touching = new HashSet<(Collider, Collider)>();
        private readonly Dictionary<Type, System.Reflection.MethodInfo[]> handlers = new Dictionary<Type, System.Reflection.MethodInfo[]>();

        /// <summary>
        /// OnCollisionEnter for each pair of colliders that came to touch this game step, OnCollisionStay for each still
        /// touching, to the scripts on each one's body and on the collider itself: the pair's impulse, the speed they met
        /// at and the points.
        /// </summary>
        private void Tell()
        {
            if (Contacts.Count == 0 && touching.Count == 0)
            {
                return;
            }

            var pairs = new Dictionary<(Collider, Collider), List<EngineContact>>();
            foreach (var c in Contacts)
            {
                if (c.A == null || c.B == null)
                {
                    continue;
                }

                if (!pairs.TryGetValue((c.A, c.B), out var list))
                {
                    pairs[(c.A, c.B)] = list = new List<EngineContact>();
                }

                list.Add(c);
            }

            var now = new HashSet<(Collider, Collider)>();
            foreach (var pair in pairs)
            {
                var (a, b) = pair.Key;
                now.Add(pair.Key);
                bool entered = !touching.Contains(pair.Key);
                Vector3 impulse = Vector3.zero;
                foreach (var c in pair.Value)
                {
                    impulse += c.Impulse;
                }

                Vector3 va = SpeedBefore(a), vb = SpeedBefore(b);
                Send(a, b, pair.Value, impulse, vb - va, entered, 1f);
                Send(b, a, pair.Value, -impulse, va - vb, entered, -1f);
            }

            touching = now;
        }

        private Vector3 SpeedBefore(Collider collider)
        {
            var body = collider.attachedRigidbody;
            if (body == null && collider.attachedArticulationBody != null)
            {
                return collider.attachedArticulationBody.linearVelocity;
            }

            return body != null && bodies.TryGetValue(body, out var s) ? s.Before : Vector3.zero;
        }

        private void Send(Collider mine, Collider other, List<EngineContact> contacts, Vector3 impulse, Vector3 relative, bool entered, float sign)
        {
            var body = mine.attachedRigidbody;
            var points = new ContactPoint[contacts.Count];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new ContactPoint
                {
                    point = contacts[i].Point, normal = contacts[i].Normal * sign, impulse = contacts[i].Impulse * sign, separation = contacts[i].Separation,
                    thisCollider = mine, otherCollider = other,
                };
            }

            var collision = new Collision { impulse = impulse, relativeVelocity = relative, rigidbody = other.attachedRigidbody, collider = other, Points = points };
            string name = entered ? "OnCollisionEnter" : "OnCollisionStay";
            var onLink = body == null ? mine.attachedArticulationBody : null;
            Call(body != null ? body.gameObject : onLink != null ? onLink.gameObject : null, name, collision);
            if ((body == null && onLink == null) || mine.gameObject != (body != null ? body.gameObject : onLink.gameObject))
            {
                Call(mine.gameObject, name, collision);
            }
        }

        private void Call(GameObject target, string name, Collision collision)
        {
            if (target == null)
            {
                return;
            }

            foreach (var script in target.GetComponents<MonoBehaviour>())
            {
                var type = script.GetType();
                if (!handlers.TryGetValue(type, out var found))
                {
                    const System.Reflection.BindingFlags any = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
                    handlers[type] = found = new[] { type.GetMethod("OnCollisionEnter", any), type.GetMethod("OnCollisionStay", any) };
                }

                var method = name == "OnCollisionEnter" ? found[0] : found[1];
                if (method != null)
                {
                    try
                    {
                        method.Invoke(script, new object[] { collision });
                    }
                    catch (System.Reflection.TargetInvocationException e) when (e.InnerException != null)
                    {
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    }
                }
            }
        }

        /// <summary>The engine's state onto the stubs: every dynamic body's pose and speeds, parents before children.</summary>
        private void ReadBack()
        {
            foreach (var s in ordered)
            {
                s.Body.MovedThisStep = false;
                if (s.Kinematic)
                {
                    s.Pos = s.Body.position;
                    s.Rot = s.Body.rotation;
                    continue;
                }

                var pose = PxRigidActor_getGlobalPose((PxRigidActor*)s.Actor);
                s.Pos = U(pose.p);
                s.Rot = U(pose.q);
                s.Vel = U(PxRigidBody_getLinearVelocity((PxRigidBody*)s.Actor));
                s.AngVel = U(PxRigidBody_getAngularVelocity((PxRigidBody*)s.Actor));
                s.Body.position = s.Pos;
                s.Body.rotation = s.Rot;
                s.Body.linearVelocity = s.Vel;
                s.Body.angularVelocity = s.AngVel;
                // (What the stub's transform makes of them is what is compared next step: no teleport read into rounding.)
                s.Pos = s.Body.position;
                s.Rot = s.Body.rotation;
            }
        }

        // ---- The stubs mirrored into the engine ------------------------------------------------------------------

        private BodyState StateOf(Rigidbody body)
        {
            if (!bodies.TryGetValue(body, out var s))
            {
                s = new BodyState { Body = body };
                var pose = T(body.position, body.rotation);
                s.Actor = PxPhysics_createRigidDynamic_mut(physics, &pose);
                s.Pos = body.position;
                s.Rot = body.rotation;
                s.Vel = body.linearVelocity;
                s.AngVel = body.angularVelocity;
                s.Mass = -1f;
                s.LinDamp = s.AngDamp = s.MaxAng = s.MaxDepen = -1f;
                s.Gravity = true;
                s.Iters = -1;
                s.MassDirty = true;
                for (var t = body.transform; t != null; t = t.parent)
                {
                    s.Depth++;
                }

                var v = V(s.Vel);
                var w = V(s.AngVel);
                PxRigidDynamic_setLinearVelocity_mut(s.Actor, &v, true);
                PxRigidDynamic_setAngularVelocity_mut(s.Actor, &w, true);
                PxScene_addActor_mut(scene, (PxActor*)s.Actor, null);
                bodies[body] = s;
                ordered.Add(s);
                ordered.Sort((a, b) => a.Depth.CompareTo(b.Depth));
            }

            return s;
        }

        /// <summary>Everything the game changed on the stubs since the last step, taken into the engine.</summary>
        private void Sync()
        {
            // Bodies: new ones made, destroyed ones released.
            foreach (var s in ordered)
            {
                s.Seen = false;
            }

            foreach (var body in Physics.Bodies)
            {
                StateOf(body).Seen = true;
            }

            for (int i = ordered.Count - 1; i >= 0; i--)
            {
                var s = ordered[i];
                if (!s.Seen)
                {
                    PxScene_removeActor_mut(scene, (PxActor*)s.Actor, true);
                    s.Actor = null;
                    bodies.Remove(s.Body);
                    ordered.RemoveAt(i);
                }
            }

            SyncArticulations();

            // Colliders: new shapes on their bodies (or standing still in the world), gone ones taken off.
            foreach (var sh in shapes.Values)
            {
                sh.Seen = false;
            }

            foreach (var collider in Physics.Colliders)
            {
                if (!shapes.TryGetValue(collider, out var sh))
                {
                    sh = MakeShape(collider);
                    if (sh == null)
                    {
                        continue;
                    }
                }

                sh.Seen = true;
                bool trigger = collider.isTrigger, enabled = collider.enabled;
                int layer = collider.gameObject.layer;
                if (trigger != sh.Trigger || enabled != sh.Enabled)
                {
                    sh.Trigger = trigger;
                    sh.Enabled = enabled;
                    // (Off both before either goes on: PhysX refuses a shape that is a trigger and a solid at once.)
                    PxShape_setFlag_mut(sh.Shape, PxShapeFlag.SimulationShape, false);
                    PxShape_setFlag_mut(sh.Shape, PxShapeFlag.TriggerShape, false);
                    PxShape_setFlag_mut(sh.Shape, PxShapeFlag.TriggerShape, enabled && trigger);
                    PxShape_setFlag_mut(sh.Shape, PxShapeFlag.SimulationShape, enabled && !trigger);
                    PxShape_setFlag_mut(sh.Shape, PxShapeFlag.SceneQueryShape, enabled);
                    if (sh.Owner != null)
                    {
                        sh.Owner.MassDirty = true;
                    }
                }

                if (layer != sh.Layer)
                {
                    sh.Layer = layer;
                    var data = new PxFilterData { word0 = sh.Id, word1 = (uint)layer };
                    PxShape_setSimulationFilterData_mut(sh.Shape, &data);
                    filteringDirty = true;
                }
            }

            var gone = new List<ShapeState>();
            foreach (var sh in shapes.Values)
            {
                if (!sh.Seen || (sh.Owner != null && sh.Owner.Actor == null))
                {
                    gone.Add(sh);
                }
            }

            foreach (var sh in gone)
            {
                if (sh.Owner == null || sh.Owner.Actor != null)
                {
                    PxRigidActor_detachShape_mut(sh.Actor, sh.Shape, true);
                    if (sh.Owner != null)
                    {
                        sh.Owner.MassDirty = true;
                    }
                }

                byShape.Remove((IntPtr)sh.Shape);
                shapes.Remove(sh.Collider);
            }

            // Each body's own settings, its pose where the game put it, its speeds where the game set them.
            foreach (var s in ordered)
            {
                var b = s.Body;
                var rigid = (PxRigidBody*)s.Actor;
                if (b.isKinematic != s.Kinematic)
                {
                    s.Kinematic = b.isKinematic;
                    PxRigidBody_setRigidBodyFlag_mut(rigid, PxRigidBodyFlag.Kinematic, s.Kinematic);
                    if (!s.Kinematic)
                    {
                        // (Let go: it starts from the speeds the game gives it.)
                        s.Vel = s.AngVel = new Vector3(float.NaN, 0f, 0f);
                        PxRigidDynamic_wakeUp_mut(s.Actor);
                    }
                }

                if (b.mass != s.Mass || s.MassDirty)
                {
                    s.Mass = b.mass;
                    s.MassDirty = false;
                    if (!PxRigidBodyExt_setMassAndUpdateInertia_1(rigid, Mathf.Max(1e-4f, b.mass), null, false))
                    {
                        // (No solid shape on it: a small ball's inertia, so it still turns sanely.)
                        PxRigidBody_setMass_mut(rigid, Mathf.Max(1e-4f, b.mass));
                        var inertia = V(Vector3.one * (0.4f * Mathf.Max(1e-4f, b.mass) * 0.05f * 0.05f));
                        PxRigidBody_setMassSpaceInertiaTensor_mut(rigid, &inertia);
                    }
                }

                if (b.useGravity != s.Gravity)
                {
                    s.Gravity = b.useGravity;
                    PxActor_setActorFlag_mut((PxActor*)s.Actor, PxActorFlag.DisableGravity, !s.Gravity);
                }

                if (b.linearDamping != s.LinDamp)
                {
                    PxRigidBody_setLinearDamping_mut(rigid, s.LinDamp = b.linearDamping);
                }

                if (b.angularDamping != s.AngDamp)
                {
                    PxRigidBody_setAngularDamping_mut(rigid, s.AngDamp = b.angularDamping);
                }

                if (b.maxAngularVelocity != s.MaxAng)
                {
                    PxRigidBody_setMaxAngularVelocity_mut(rigid, s.MaxAng = b.maxAngularVelocity);
                }

                if (b.maxDepenetrationVelocity != s.MaxDepen)
                {
                    PxRigidBody_setMaxDepenetrationVelocity_mut(rigid, Mathf.Min(s.MaxDepen = b.maxDepenetrationVelocity, 1e30f));
                }

                if (b.solverIterations != s.Iters || b.solverVelocityIterations != s.VelIters)
                {
                    s.Iters = b.solverIterations;
                    s.VelIters = b.solverVelocityIterations;
                    PxRigidDynamic_setSolverIterationCounts_mut(s.Actor, (uint)Mathf.Max(1, s.Iters), (uint)Mathf.Max(1, s.VelIters));
                }

                if (b.collisionDetectionMode != s.Ccd)
                {
                    s.Ccd = b.collisionDetectionMode;
                    PxRigidBody_setRigidBodyFlag_mut(rigid, PxRigidBodyFlag.EnableSpeculativeCcd, s.Ccd == CollisionDetectionMode.ContinuousSpeculative);
                    PxRigidBody_setRigidBodyFlag_mut(rigid, PxRigidBodyFlag.EnableCcd,
                        s.Ccd == CollisionDetectionMode.Continuous || s.Ccd == CollisionDetectionMode.ContinuousDynamic);
                }

                if (s.Kinematic)
                {
                    continue;   // (moved in Simulate, a physics step at a time)
                }

                if (Differs(b.position, s.Pos) || Differs(b.rotation, s.Rot))
                {
                    // (Placed by the game: a teleport, its speeds kept.)
                    var at = T(b.position, b.rotation);
                    PxRigidActor_setGlobalPose_mut((PxRigidActor*)s.Actor, &at, true);
                    s.Pos = b.position;
                    s.Rot = b.rotation;
                }

                if (Differs(b.linearVelocity, s.Vel))
                {
                    var v = V(s.Vel = b.linearVelocity);
                    PxRigidDynamic_setLinearVelocity_mut(s.Actor, &v, true);
                }

                if (Differs(b.angularVelocity, s.AngVel))
                {
                    var w = V(s.AngVel = b.angularVelocity);
                    PxRigidDynamic_setAngularVelocity_mut(s.Actor, &w, true);
                }
            }

            foreach (var s in ordered)
            {
                s.Before = s.Body.linearVelocity;
            }

            FinishArticulations();
            SyncJoints();
            if (filteringDirty)
            {
                filteringDirty = false;
                ignored.Clear();
                foreach (var (stub, id) in pendingIgnore)
                {
                    if (shapes.TryGetValue(stub, out var st))
                    {
                        extraIgnored.Add(Key(st.Id, id));
                    }
                }

                foreach (var (a, b) in ignoredPairs)
                {
                    if (shapes.TryGetValue(a, out var sa) && shapes.TryGetValue(b, out var sb))
                    {
                        ignored.Add(Key(sa.Id, sb.Id));
                    }
                }

                foreach (var s in ordered)
                {
                    PxScene_resetFiltering_mut(scene, (PxActor*)s.Actor);
                }
            }
        }

        private ShapeState MakeShape(Collider collider)
        {
            Geo geo;
            if (!MakeGeo(collider, &geo, out var offset, out var turn))
            {
                return null;
            }

            var body = collider.attachedRigidbody;
            var sh = new ShapeState { Collider = collider, Id = nextId++, Layer = int.MinValue };
            sh.Trigger = collider.isTrigger;
            sh.Enabled = collider.enabled;
            var flags = !sh.Enabled ? (PxShapeFlags)0 : sh.Trigger ? PxShapeFlags.TriggerShape | PxShapeFlags.SceneQueryShape : PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape;
            sh.Shape = PxPhysics_createShape_mut(physics, (PxGeometry*)&geo, material, true, flags);
            PxShape_setContactOffset_mut(sh.Shape, Physics.defaultContactOffset);
            PxShape_setRestOffset_mut(sh.Shape, 0f);
            var t = collider.transform;
            var onLink = body == null ? collider.attachedArticulationBody : null;
            if (onLink != null && links.TryGetValue(onLink, out var link))
            {
                // (A link's collider: on its link, where it sits against it.)
                sh.Actor = (PxRigidActor*)link.Link;
                var inv = Quaternion.Inverse(onLink.transform.rotation);
                var local = T(inv * (t.position + t.rotation * offset - onLink.transform.position), inv * t.rotation * turn);
                PxShape_setLocalPose_mut(sh.Shape, &local);
            }
            else if (body != null)
            {
                sh.Owner = StateOf(body);
                sh.Actor = (PxRigidActor*)sh.Owner.Actor;
                var inv = Quaternion.Inverse(body.rotation);
                var local = T(inv * (t.position + t.rotation * offset - body.position), inv * t.rotation * turn);
                PxShape_setLocalPose_mut(sh.Shape, &local);
                sh.Owner.MassDirty = true;
            }
            else
            {
                // (No body: it stands still in the world where it is.)
                var pose = T(t.position + t.rotation * offset, t.rotation * turn);
                sh.Actor = (PxRigidActor*)PxPhysics_createRigidStatic_mut(physics, &pose);
                PxScene_addActor_mut(scene, (PxActor*)sh.Actor, null);
            }

            PxRigidActor_attachShape_mut(sh.Actor, sh.Shape);
            shapes[collider] = sh;
            byShape[(IntPtr)sh.Shape] = sh;
            filteringDirty = true;
            return sh;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Geo
        {
            [FieldOffset(0)] public PxBoxGeometry Box;
            [FieldOffset(0)] public PxSphereGeometry Sphere;
            [FieldOffset(0)] public PxCapsuleGeometry Capsule;

        }

        /// <summary>A collider's shape as the engine has it, with where its centre sits and how it is turned against the collider's own transform.</summary>
        private static bool MakeGeo(Collider collider, Geo* geo, out Vector3 offset, out Quaternion turn)
        {
            offset = Vector3.zero;
            turn = Quaternion.identity;
            Vector3 s = collider.transform.lossyScale;
            s = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
            switch (collider)
            {
                case BoxCollider box:
                    offset = Vector3.Scale(box.center, s);
                    geo->Box = PxBoxGeometry_new(Mathf.Max(1e-4f, 0.5f * box.size.x * s.x), Mathf.Max(1e-4f, 0.5f * box.size.y * s.y), Mathf.Max(1e-4f, 0.5f * box.size.z * s.z));
                    return true;
                case SphereCollider sphere:
                    offset = Vector3.Scale(sphere.center, s);
                    geo->Sphere = PxSphereGeometry_new(Mathf.Max(1e-4f, sphere.radius * Mathf.Max(s.x, Mathf.Max(s.y, s.z))));
                    return true;
                case CapsuleCollider capsule:
                {
                    offset = Vector3.Scale(capsule.center, s);
                    int d = capsule.direction;
                    float along = d == 0 ? s.x : d == 1 ? s.y : s.z;
                    float across = d == 0 ? Mathf.Max(s.y, s.z) : d == 1 ? Mathf.Max(s.x, s.z) : Mathf.Max(s.x, s.y);
                    float radius = Mathf.Max(1e-4f, capsule.radius * across);
                    float half = Mathf.Max(1e-4f, 0.5f * capsule.height * along - radius);
                    // (PhysX's capsule lies along its x.)
                    turn = d == 0 ? Quaternion.identity : Quaternion.FromToRotation(Vector3.right, d == 1 ? Vector3.up : Vector3.forward);
                    geo->Capsule = PxCapsuleGeometry_new(radius, half);
                    return true;
                }
                default:
                    return false;
            }
        }

        // ---- Joints: Unity's ConfigurableJoint as the D6 joint it is ---------------------------------------------

        private void SyncJoints()
        {
            foreach (var j in joints.Values)
            {
                j.Seen = false;
            }

            foreach (var joint in Physics.Joints)
            {
                if (!(joint is ConfigurableJoint cj) || cj.Destroyed)
                {
                    continue;
                }

                var own = cj.GetComponent<Rigidbody>();
                if (own == null || !bodies.ContainsKey(own) || (cj.connectedBody != null && !bodies.ContainsKey(cj.connectedBody))
                    || (cj.connectedArticulationBody != null && !links.ContainsKey(cj.connectedArticulationBody)))
                {
                    continue;
                }

                if (!joints.TryGetValue(cj, out var j))
                {
                    j = new JointState { Joint = cj };
                    joints[cj] = j;
                }

                j.Seen = true;
                if (j.Px == null || j.Connected != cj.connectedBody || Differs(j.Axis, cj.axis) || Differs(j.Secondary, cj.secondaryAxis) || Differs(j.Anchor, cj.anchor))
                {
                    // (Made, or its frame changed: set up again from the pose its bodies are in, as Unity does.)
                    Release(j);
                    Make(j, own);
                }

                Update(j);
            }

            var gone = new List<Joint>();
            foreach (var pair in joints)
            {
                if (!pair.Value.Seen)
                {
                    Release(pair.Value);
                    gone.Add(pair.Key);
                }
            }

            foreach (var key in gone)
            {
                joints.Remove(key);
            }
        }

        private static void Release(JointState j)
        {
            if (j.Px != null)
            {
                PxJoint_release_mut((PxJoint*)j.Px);
                j.Px = null;
            }
        }

        private void Make(JointState j, Rigidbody own)
        {
            var cj = j.Joint;
            j.Connected = cj.connectedBody;
            j.Axis = cj.axis;
            j.Secondary = cj.secondaryAxis;
            j.Anchor = cj.anchor;
            j.First = true;
            // The joint's frame on its own body: x along the axis, y along the secondary axis (squared up to it).
            Vector3 x = cj.axis.sqrMagnitude > 1e-12f ? cj.axis.normalized : Vector3.right;
            Vector3 y = cj.secondaryAxis - Vector3.Dot(cj.secondaryAxis, x) * x;
            y = y.sqrMagnitude > 1e-12f ? y.normalized : Vector3.Cross(x, Mathf.Abs(x.y) < 0.9f ? Vector3.up : Vector3.forward).normalized;
            Vector3 z = Vector3.Cross(x, y);
            var frame = Quaternion.LookRotation(z, y);
            Vector3 anchor = Vector3.Scale(cj.anchor, own.transform.lossyScale);
            var local0 = T(anchor, frame);
            // (From the pose its bodies were in when it was set up, as Unity takes it: not the one they are in now.)
            Vector3 ownAt = cj.FramesTaken ? cj.OwnAt : own.position;
            Quaternion ownTurn = cj.FramesTaken ? cj.OwnTurn : own.rotation;
            Vector3 worldAt = ownAt + ownTurn * anchor;
            Quaternion worldTurn = ownTurn * frame;
            PxTransform local1;
            PxRigidActor* other = null;
            if (cj.connectedArticulationBody != null)
            {
                // (Joined to a link of an articulation: where the link is now.)
                var c = cj.connectedArticulationBody.transform;
                var inv = Quaternion.Inverse(c.rotation);
                local1 = T(inv * (worldAt - c.position), inv * worldTurn);
                other = (PxRigidActor*)links[cj.connectedArticulationBody].Link;
            }
            else if (cj.connectedBody != null)
            {
                var c = cj.connectedBody;
                var inv = Quaternion.Inverse(cj.FramesTaken ? cj.ConnectedTurn : c.rotation);
                local1 = T(inv * (worldAt - (cj.FramesTaken ? cj.ConnectedAt : c.position)), inv * worldTurn);
                other = (PxRigidActor*)bodies[c].Actor;
            }
            else
            {
                local1 = T(worldAt, worldTurn);
            }

            // (Unity's joint: its own body first, the connected one second. Its drive target, its X limits and its
            // target speed are then the connected body's against its own, which is why a part is driven with the
            // inverse of its turn, its X limits bound it mirrored and it turns against its target speed: what the
            // game's probe measures in Unity, and here.)
            j.Px = phys_PxD6JointCreate(physics, (PxRigidActor*)bodies[own].Actor, &local0, other, &local1);
        }

        private static void Update(JointState j)
        {
            var cj = j.Joint;
            var px = j.Px;
            bool first = j.First;
            j.First = false;
            for (int i = 0; i < 6; i++)
            {
                var m = i == 0 ? cj.xMotion : i == 1 ? cj.yMotion : i == 2 ? cj.zMotion : i == 3 ? cj.angularXMotion : i == 4 ? cj.angularYMotion : cj.angularZMotion;
                if (first || m != j.Motion[i])
                {
                    j.Motion[i] = m;
                    PxD6Joint_setMotion_mut(px, (PxD6Axis)i, m == ConfigurableJointMotion.Locked ? PxD6Motion.Locked : m == ConfigurableJointMotion.Limited ? PxD6Motion.Limited : PxD6Motion.Free);
                }
            }

            float low = cj.lowAngularXLimit.limit, high = cj.highAngularXLimit.limit;
            if (first || low != j.LowX || high != j.HighX)
            {
                j.LowX = low;
                j.HighX = high;
                float a = Mathf.Min(low, high) * Mathf.Deg2Rad, b = Mathf.Max(low, high) * Mathf.Deg2Rad;
                var pair = PxJointAngularLimitPair_new(a, Mathf.Max(b, a + 1e-3f), -1f);
                PxD6Joint_setTwistLimit_mut(px, &pair);
            }

            float ly = cj.angularYLimit.limit, lz = cj.angularZLimit.limit;
            if (first || ly != j.LimY || lz != j.LimZ)
            {
                j.LimY = ly;
                j.LimZ = lz;
                var cone = PxJointLimitCone_new(Mathf.Clamp(ly, 0.5f, 179f) * Mathf.Deg2Rad, Mathf.Clamp(lz, 0.5f, 179f) * Mathf.Deg2Rad, -1f);
                PxD6Joint_setSwingLimit_mut(px, &cone);
            }

            if (first || cj.rotationDriveMode != j.Mode || !Same(cj.slerpDrive, j.Slerp) || !Same(cj.angularXDrive, j.AngX) || !Same(cj.angularYZDrive, j.AngYZ))
            {
                j.Mode = cj.rotationDriveMode;
                j.Slerp = cj.slerpDrive;
                j.AngX = cj.angularXDrive;
                j.AngYZ = cj.angularYZDrive;
                bool slerp = j.Mode == RotationDriveMode.Slerp;
                SetDrive(px, PxD6Drive.Slerp, slerp ? j.Slerp : default);
                SetDrive(px, PxD6Drive.Twist, slerp ? default : j.AngX);
                SetDrive(px, PxD6Drive.Swing, slerp ? default : j.AngYZ);
            }

            if (first || Differs(cj.targetRotation, j.Target))
            {
                j.Target = cj.targetRotation;
                var to = T(cj.targetPosition, cj.targetRotation);
                PxD6Joint_setDrivePosition_mut(px, &to, true);
            }

            if (first || Differs(cj.targetAngularVelocity, j.TargetSpin))
            {
                j.TargetSpin = cj.targetAngularVelocity;
                var none = V(Vector3.zero);
                var spin = V(j.TargetSpin);
                PxD6Joint_setDriveVelocity_mut(px, &none, &spin, true);
            }

            if (first || cj.enableCollision != j.Collide || cj.enablePreprocessing != j.Preprocess)
            {
                j.Collide = cj.enableCollision;
                j.Preprocess = cj.enablePreprocessing;
                PxJoint_setConstraintFlag_mut((PxJoint*)px, PxConstraintFlag.CollisionEnabled, j.Collide);
                PxJoint_setConstraintFlag_mut((PxJoint*)px, PxConstraintFlag.DisablePreprocessing, !j.Preprocess);
            }
        }

        private static bool Same(JointDrive a, JointDrive b) =>
            a.positionSpring == b.positionSpring && a.positionDamper == b.positionDamper && a.maximumForce == b.maximumForce && a.useAcceleration == b.useAcceleration;

        private static void SetDrive(PxD6Joint* px, PxD6Drive which, JointDrive d)
        {
            var drive = PxD6JointDrive_new_1(d.positionSpring, d.positionDamper, d.maximumForce <= 0f ? 0f : Mathf.Min(d.maximumForce, 3.4e38f), d.useAcceleration);
            PxD6Joint_setDrive_mut(px, which, &drive);
        }

        public static bool TraceUnknown = Environment.GetEnvironmentVariable("PHYSX_CONTACT_TRACE") != null;

        // ---- The engine's callbacks ------------------------------------------------------------------------------

        private static ulong Key(uint a, uint b) => a < b ? ((ulong)a << 32) | b : ((ulong)b << 32) | a;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static PxFilterFlags Filter(FilterShaderCallbackInfo* info)
        {
            var world = Current;
            uint a = info->filterData0.word0, b = info->filterData1.word0;
            if (world != null && a != 0 && b != 0 && (world.ignored.Contains(Key(a, b)) || world.extraIgnored.Contains(Key(a, b)) || world.layerIgnored[info->filterData0.word1 & 31, info->filterData1.word1 & 31]))
            {
                return PxFilterFlags.Suppress;
            }

            *info->pairFlags |= PxPairFlags.NotifyTouchFound | PxPairFlags.NotifyTouchPersists | PxPairFlags.NotifyContactPoints | PxPairFlags.DetectCcdContact;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static void OnContact(void* user, PxContactPairHeader* header, PxContactPair* pairs, uint count)
        {
            var world = Current;
            if (world == null)
            {
                return;
            }

            // (The binding's PxContactPair has its two shape pointers as two bytes, so its fields sit wrong: read by
            // their real places. 64 bytes a pair: the shapes at 0 and 8.)
            var points = stackalloc PxContactPairPoint[16];
            for (uint i = 0; i < count; i++)
            {
                byte* pair = (byte*)pairs + i * 64;
                IntPtr s0 = *(IntPtr*)pair, s1 = *(IntPtr*)(pair + 8);
                if (!world.byShape.TryGetValue(s0, out var a) || !world.byShape.TryGetValue(s1, out var b))
                {
                    if (TraceUnknown)
                    {
                        var d0 = PxShape_getSimulationFilterData((PxShape*)s0);
                        var d1 = PxShape_getSimulationFilterData((PxShape*)s1);
                        Console.WriteLine($"    [engine contact] shapes {d0.word0} and {d1.word0}");
                    }

                    // (The ground's plane has no collider of the stubs': the other is still told.)
                    world.byShape.TryGetValue(s0, out a);
                    world.byShape.TryGetValue(s1, out b);
                    if (a == null && b == null)
                    {
                        continue;
                    }
                }

                uint n = PxContactPair_extractContacts((PxContactPair*)pair, points, 16);
                for (uint k = 0; k < n; k++)
                {
                    world.Contacts.Add(new EngineContact
                    {
                        A = a?.Collider, B = b?.Collider, Point = U(points[k].position), Normal = U(points[k].normal), Impulse = U(points[k].impulse),
                        Separation = points[k].separation,
                    });
                }
            }
        }

        // ---- Unity's vectors and the engine's --------------------------------------------------------------------

        // (Exactly: Unity's own == takes nearly equal for equal, and a body placed a hair off is still placed.)
        private static bool Differs(Vector3 a, Vector3 b) => a.x != b.x || a.y != b.y || a.z != b.z;

        private static bool Differs(Quaternion a, Quaternion b) => a.x != b.x || a.y != b.y || a.z != b.z || a.w != b.w;

        private static PxVec3 V(Vector3 v) => new PxVec3 { x = v.x, y = v.y, z = v.z };

        private static Vector3 U(PxVec3 v) => new Vector3(v.x, v.y, v.z);

        private static Quaternion U(PxQuat q) => new Quaternion(q.x, q.y, q.z, q.w);

        private static PxTransform T(Vector3 p, Quaternion q)
        {
            float n = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            if (n < 1e-6f)
            {
                q = Quaternion.identity;
                n = 1f;
            }

            return new PxTransform { p = V(p), q = new PxQuat { x = q.x / n, y = q.y / n, z = q.z / n, w = q.w / n } };
        }
    }
}
#endif
