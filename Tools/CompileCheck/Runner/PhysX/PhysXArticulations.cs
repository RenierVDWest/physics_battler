// The stub ArticulationBody mirrored into PhysX: each root and what hangs from it is one reduced-coordinate
// articulation, built at the first step after it was set up (its links where their transforms are, its joints'
// frames from their anchors), driven each step by the stubs' drives and read back into their transforms.
#if PHYSX
using System;
using System.Collections.Generic;
using MagicPhysX;
using UnityEngine;
using static MagicPhysX.NativeMethods;

namespace PhysicsBattler.Engine
{
    public sealed unsafe partial class PhysXWorld
    {
        private sealed class LinkState
        {
            public ArticulationBody Body;
            public PxArticulationLink* Link;
            public ArtState Art;
            public bool Gravity = true;
            public float Mass;
            public Vector3 Centre, Turns;
        }

        private sealed class ArtState
        {
            public ArticulationBody Root;
            public PxArticulationReducedCoordinate* Art;
            public readonly List<LinkState> Links = new List<LinkState>();
            public bool InScene;
        }

        private readonly Dictionary<ArticulationBody, LinkState> links = new Dictionary<ArticulationBody, LinkState>();
        private readonly List<ArtState> arts = new List<ArtState>();

        /// <summary>New articulations made (not yet in the scene: their colliders go on first), gone ones released.</summary>
        private void SyncArticulations()
        {
            for (int i = arts.Count - 1; i >= 0; i--)
            {
                var a = arts[i];
                if (a.Root == null || a.Root.gameObject == null || !Physics.Articulations.Contains(a.Root))
                {
                    foreach (var l in a.Links)
                    {
                        links.Remove(l.Body);
                        l.Body.Engine = null;
                    }

                    if (a.InScene)
                    {
                        PxScene_removeArticulation_mut(scene, a.Art, true);
                    }

                    PxArticulationReducedCoordinate_release_mut(a.Art);
                    arts.RemoveAt(i);
                }
            }

            foreach (var body in Physics.Articulations)
            {
                if (links.ContainsKey(body) || !body.isRoot)
                {
                    continue;
                }

                var a = new ArtState { Root = body, Art = PxPhysics_createArticulationReducedCoordinate_mut(physics) };
                PxArticulationReducedCoordinate_setSolverIterationCounts_mut(a.Art, (uint)Mathf.Max(1, body.solverIterations), (uint)Mathf.Max(1, body.solverVelocityIterations));
                PxArticulationReducedCoordinate_setArticulationFlag_mut(a.Art, PxArticulationFlag.DriveLimitsAreForces, true);
                PxArticulationReducedCoordinate_setArticulationFlag_mut(a.Art, PxArticulationFlag.FixBase, body.immovable);
                PxArticulationReducedCoordinate_setSleepThreshold_mut(a.Art, body.sleepThreshold);
                AddLink(a, body, null);
                arts.Add(a);
            }
        }

        private void AddLink(ArtState a, ArticulationBody body, LinkState parent)
        {
            var t = body.transform;
            var pose = T(t.position, t.rotation);
            var l = new LinkState { Body = body, Art = a, Link = PxArticulationReducedCoordinate_createLink_mut(a.Art, parent != null ? parent.Link : null, &pose) };
            a.Links.Add(l);
            links[body] = l;
            body.Engine = l;
            if (parent != null)
            {
                var joint = PxArticulationLink_getInboundJoint(l.Link);
                var kind = body.jointType == ArticulationJointType.SphericalJoint ? PxArticulationJointType.Spherical
                    : body.jointType == ArticulationJointType.RevoluteJoint ? PxArticulationJointType.Revolute
                    : body.jointType == ArticulationJointType.PrismaticJoint ? PxArticulationJointType.Prismatic : PxArticulationJointType.Fix;
                PxArticulationJointReducedCoordinate_setJointType_mut(joint, kind);
                // (Its anchor on itself; on its parent the same place and turn, or as it was given.)
                var p = parent.Body.transform;
                Vector3 scale = t.lossyScale;
                Vector3 own = Vector3.Scale(body.anchorPosition, scale);
                Vector3 onParentAt;
                Quaternion onParentTurn;
                if (body.matchAnchors)
                {
                    var inv = Quaternion.Inverse(p.rotation);
                    onParentAt = inv * (t.position + t.rotation * own - p.position);
                    onParentTurn = inv * (t.rotation * body.anchorRotation);
                    body.parentAnchorPosition = onParentAt;
                    body.parentAnchorRotation = onParentTurn;
                }
                else
                {
                    onParentAt = Vector3.Scale(body.parentAnchorPosition, p.lossyScale);
                    onParentTurn = body.parentAnchorRotation;
                }

                var onParent = T(onParentAt, onParentTurn);
                var onChild = T(own, body.anchorRotation);
                PxArticulationJointReducedCoordinate_setParentPose_mut(joint, &onParent);
                PxArticulationJointReducedCoordinate_setChildPose_mut(joint, &onChild);
                if (kind == PxArticulationJointType.Spherical || kind == PxArticulationJointType.Revolute)
                {
                    PxArticulationJointReducedCoordinate_setMotion_mut(joint, PxArticulationAxis.Twist, Motion(body.twistLock));
                    if (kind == PxArticulationJointType.Spherical)
                    {
                        PxArticulationJointReducedCoordinate_setMotion_mut(joint, PxArticulationAxis.Swing1, Motion(body.swingYLock));
                        PxArticulationJointReducedCoordinate_setMotion_mut(joint, PxArticulationAxis.Swing2, Motion(body.swingZLock));
                    }
                }

                PxArticulationJointReducedCoordinate_setMaxJointVelocity_mut(joint, body.maxJointVelocity);
                PxArticulationJointReducedCoordinate_setFrictionCoefficient_mut(joint, body.jointFriction);
            }

            for (int i = 0; i < t.childCount; i++)
            {
                AddChildLinks(a, t.GetChild(i), l);
            }
        }

        /// <summary>The links under a transform: its own if it has one, else those of what is under it (a link's children needn't be its direct ones).</summary>
        private void AddChildLinks(ArtState a, Transform t, LinkState parent)
        {
            var body = t.GetComponent<ArticulationBody>();
            if (body != null)
            {
                if (!links.ContainsKey(body))
                {
                    AddLink(a, body, parent);
                }

                return;
            }

            for (int i = 0; i < t.childCount; i++)
            {
                AddChildLinks(a, t.GetChild(i), parent);
            }
        }

        private static PxArticulationMotion Motion(ArticulationDofLock dofLock) =>
            dofLock == ArticulationDofLock.FreeMotion ? PxArticulationMotion.Free : dofLock == ArticulationDofLock.LimitedMotion ? PxArticulationMotion.Limited : PxArticulationMotion.Locked;

        /// <summary>New articulations, their colliders now on them: their masses set, and into the scene.</summary>
        private void FinishArticulations()
        {
            foreach (var a in arts)
            {
                if (a.InScene)
                {
                    continue;
                }

                foreach (var l in a.Links)
                {
                    var rigid = (PxRigidBody*)l.Link;
                    var b = l.Body;
                    if (b.automaticCenterOfMass && b.automaticInertiaTensor && PxRigidActor_getNbShapes((PxRigidActor*)l.Link) > 0
                        && PxRigidBodyExt_setMassAndUpdateInertia_1(rigid, Mathf.Max(1e-4f, b.mass), null, false))
                    {
                        // (As the engine works it out from its colliders; told back to the stub.)
                        var cm = PxRigidBody_getCMassLocalPose(rigid);
                        b.centerOfMass = U(cm.p);
                        b.inertiaTensor = U(PxRigidBody_getMassSpaceInertiaTensor(rigid));
                        b.inertiaTensorRotation = U(cm.q);
                    }
                    else
                    {
                        SetMass(l);
                    }

                    l.Mass = b.mass;
                    l.Centre = b.centerOfMass;
                    l.Turns = b.inertiaTensor;

                    PxRigidBody_setLinearDamping_mut(rigid, b.linearDamping);
                    PxRigidBody_setAngularDamping_mut(rigid, b.angularDamping);
                    PxRigidBody_setMaxAngularVelocity_mut(rigid, b.maxAngularVelocity);
                    PxRigidBody_setMaxLinearVelocity_mut(rigid, b.maxLinearVelocity);
                    PxRigidBody_setMaxDepenetrationVelocity_mut(rigid, Mathf.Min(b.maxDepenetrationVelocity, 1e30f));
                    b.DrivesChanged = true;
                }

                PxScene_addArticulation_mut(scene, a.Art);
                a.InScene = true;
            }
        }

        private static void SetMass(LinkState l)
        {
            var rigid = (PxRigidBody*)l.Link;
            var b = l.Body;
            PxRigidBody_setMass_mut(rigid, Mathf.Max(1e-4f, b.mass));
            var cm = T(b.centerOfMass, b.inertiaTensorRotation);
            PxRigidBody_setCMassLocalPose_mut(rigid, &cm);
            var tensor = V(b.inertiaTensor);
            PxRigidBody_setMassSpaceInertiaTensor_mut(rigid, &tensor);
            l.Mass = b.mass;
            l.Centre = b.centerOfMass;
            l.Turns = b.inertiaTensor;
        }

        /// <summary>Before each physics step: the drives as the stubs have them now, the pushes asked for, a root put somewhere.</summary>
        private void DriveArticulations(int k)
        {
            foreach (var a in arts)
            {
                if (!a.InScene)
                {
                    continue;
                }

                if (a.Root.Teleported)
                {
                    a.Root.Teleported = false;
                    var to = T(a.Root.TeleportTo, a.Root.TeleportTurn);
                    PxArticulationReducedCoordinate_setRootGlobalPose_mut(a.Art, &to, true);
                }

                foreach (var l in a.Links)
                {
                    var b = l.Body;
                    if (b.useGravity != l.Gravity)
                    {
                        l.Gravity = b.useGravity;
                        PxActor_setActorFlag_mut((PxActor*)l.Link, PxActorFlag.DisableGravity, !l.Gravity);
                    }

                    if (l != a.Links[0] && b.jointType != ArticulationJointType.FixedJoint)
                    {
                        var joint = PxArticulationLink_getInboundJoint(l.Link);
                        int axes = b.jointType == ArticulationJointType.SphericalJoint ? 3 : 1;
                        for (int axis = 0; axis < axes; axis++)
                        {
                            var d = b.drives[axis];
                            if (b.DrivesChanged)
                            {
                                var drive = new PxArticulationDrive
                                {
                                    stiffness = d.stiffness, damping = d.damping, maxForce = d.forceLimit,
                                    driveType = d.driveType == ArticulationDriveType.Acceleration ? PxArticulationDriveType.Acceleration : PxArticulationDriveType.Force,
                                };
                                PxArticulationJointReducedCoordinate_setDriveParams_mut(joint, (PxArticulationAxis)axis, &drive);
                            }

                            // (A drive's target in degrees, its speed in degrees a second, as Unity's are.)
                            PxArticulationJointReducedCoordinate_setDriveTarget_mut(joint, (PxArticulationAxis)axis, d.target * Mathf.Deg2Rad, true);
                            PxArticulationJointReducedCoordinate_setDriveVelocity_mut(joint, (PxArticulationAxis)axis, d.targetVelocity * Mathf.Deg2Rad, true);
                        }

                        b.DrivesChanged = false;
                    }

                    if (b.mass != l.Mass || Differs(b.centerOfMass, l.Centre) || Differs(b.inertiaTensor, l.Turns))
                    {
                        // (Its weight changed: something put in its hand.)
                        SetMass(l);
                    }

                    foreach (var p in b.Pushes)
                    {
                        bool once = p.Mode == ForceMode.Impulse || p.Mode == ForceMode.VelocityChange;
                        if (once && k > 1)
                        {
                            continue;
                        }

                        var f = V(p.Force);
                        var mode = p.Mode == ForceMode.Impulse ? PxForceMode.Impulse : p.Mode == ForceMode.VelocityChange ? PxForceMode.VelocityChange
                            : p.Mode == ForceMode.Acceleration ? PxForceMode.Acceleration : PxForceMode.Force;
                        if (p.Kind == 2)
                        {
                            PxRigidBody_addTorque_mut((PxRigidBody*)l.Link, &f, mode, true);
                        }
                        else if (p.Kind == 1 && (mode == PxForceMode.Force || mode == PxForceMode.Impulse))
                        {
                            var at = V(p.At);
                            PxRigidBodyExt_addForceAtPos((PxRigidBody*)l.Link, &f, &at, mode, true);
                        }
                        else
                        {
                            PxRigidBody_addForce_mut((PxRigidBody*)l.Link, &f, mode, true);
                        }
                    }
                }
            }
        }

        /// <summary>After the step: each link's place into its stub's transform (parents first), its speeds and its joint's position.</summary>
        private void ReadBackArticulations()
        {
            foreach (var a in arts)
            {
                if (!a.InScene)
                {
                    continue;
                }

                foreach (var l in a.Links)
                {
                    var pose = PxRigidActor_getGlobalPose((PxRigidActor*)l.Link);
                    l.Body.transform.SetPositionAndRotation(U(pose.p), U(pose.q));
                    l.Body.linearVelocity = U(PxRigidBody_getLinearVelocity((PxRigidBody*)l.Link));
                    l.Body.angularVelocity = U(PxRigidBody_getAngularVelocity((PxRigidBody*)l.Link));
                    l.Body.Pushes.Clear();
                    if (l != a.Links[0] && l.Body.jointType != ArticulationJointType.FixedJoint)
                    {
                        var joint = PxArticulationLink_getInboundJoint(l.Link);
                        bool ball = l.Body.jointType == ArticulationJointType.SphericalJoint;
                        l.Body.jointPosition = new ArticulationReducedSpace(
                            PxArticulationJointReducedCoordinate_getJointPosition(joint, PxArticulationAxis.Twist),
                            ball ? PxArticulationJointReducedCoordinate_getJointPosition(joint, PxArticulationAxis.Swing1) : 0f,
                            ball ? PxArticulationJointReducedCoordinate_getJointPosition(joint, PxArticulationAxis.Swing2) : 0f) { dofCount = ball ? 3 : 1 };
                    }
                }
            }
        }
    }
}
#endif
