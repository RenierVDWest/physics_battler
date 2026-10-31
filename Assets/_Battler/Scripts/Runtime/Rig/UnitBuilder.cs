using System;
using System.Collections.Generic;
using PhysicsBattler.Muscles;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// Builds a unit from a rig file: the null tree, a mesh under each part's null at zero offset,
    /// and on each part a rigidbody, a collider (on a child object, so tilted capsules work) and a
    /// ConfigurableJoint to its parent part. Used by the editor to bake prefabs, and usable at runtime.
    ///
    /// Result:
    ///   Unit (UnitRig, UnitController)        the logical root, on the ground
    ///     Pelvis (BodyPart, Rigidbody)
    ///       Mesh, Collider, Spine, Thigh_L ...  (and so on down the tree; markers are plain nulls)
    /// </summary>
    public static class UnitBuilder
    {
        public struct Visual
        {
            public Mesh mesh;
            public Material[] materials;

            /// <summary>The mesh node's pose inside its model file (identity when the export baked it in).</summary>
            public Vector3 offset;
            public Quaternion rotation;
            public Vector3 scale;

            public static Visual Of(Mesh mesh, Material[] materials)
            {
                return new Visual { mesh = mesh, materials = materials, rotation = Quaternion.identity, scale = Vector3.one };
            }
        }

        /// <param name="visuals">Returns the mesh and materials for a mesh name, or null to leave a part without one.</param>
        /// <param name="bodyLayer">Layer for the part colliders (the project's "Body" layer).</param>
        public static GameObject Build(RigFile rig, UnitProfile profile, string unitName,
            Func<string, Visual?> visuals, int bodyLayer)
        {
            if (rig == null)
            {
                throw new ArgumentNullException(nameof(rig));
            }

            if (profile == null)
            {
                profile = UnitProfile.CreateDefault();
            }

            float scale = Mathf.Max(0.05f, profile.scale);
            float massFactor = rig.totalMassKg > 0f && profile.totalMassKg > 0f ? profile.totalMassKg / rig.totalMassKg : 1f;

            var root = new GameObject(unitName);
            var nulls = new Dictionary<string, Transform>();
            var rootEntry = Array.Find(rig.nulls, n => !n.HasParent);
            nulls[rootEntry != null ? rootEntry.name : "Root"] = root.transform;

            // The file lists parents before children, but don't rely on it.
            var pending = new List<NullEntry>(rig.nulls);
            pending.RemoveAll(n => !n.HasParent);
            while (pending.Count > 0)
            {
                int before = pending.Count;
                for (int i = pending.Count - 1; i >= 0; i--)
                {
                    var entry = pending[i];
                    if (!nulls.TryGetValue(entry.parent, out var parent))
                    {
                        continue;
                    }

                    var node = new GameObject(entry.name).transform;
                    node.SetParent(parent, false);
                    node.localPosition = entry.LocalPosition * scale;
                    node.localRotation = Quaternion.identity;
                    nulls[entry.name] = node;
                    pending.RemoveAt(i);
                }

                if (pending.Count == before)
                {
                    throw new FormatException($"Rig nulls with missing parents: {pending[0].name} (parent {pending[0].parent}).");
                }
            }

            var parts = new BodyPart[PartInfo.Count];
            foreach (var id in PartInfo.All)
            {
                string nullName = PartInfo.NullName(id);
                if (!nulls.TryGetValue(nullName, out var node))
                {
                    throw new FormatException($"The rig has no null called {nullName}.");
                }

                var entry = rig.FindPart(nullName);
                if (entry == null)
                {
                    throw new FormatException($"The rig has no part entry for {nullName}.");
                }

                parts[(int)id] = BuildPart(id, node, entry, profile, scale, massFactor, visuals, bodyLayer);
            }

            // Joints after every rigidbody exists.
            foreach (var id in PartInfo.All)
            {
                if (PartInfo.HasParent(id))
                {
                    var part = parts[(int)id];
                    part.joint = AddJoint(part, parts[(int)PartInfo.Parent(id)].body);
                }
            }

            var rigComponent = root.AddComponent<UnitRig>();
            rigComponent.Initialize(parts, Find(nulls, "Slot_Weapon_R"), Find(nulls, "Slot_Weapon_L"),
                Find(nulls, "Aim_Chest"), Find(nulls, "Aim_Head"), Find(nulls, "Aim_Neck"), Find(nulls, "Aim_Belly"), scale);

            var controller = root.AddComponent<UnitController>();
            controller.Profile = profile;
            return root;
        }

        private static Transform Find(Dictionary<string, Transform> nulls, string name)
        {
            return nulls.TryGetValue(name, out var node) ? node : null;
        }

        private static BodyPart BuildPart(PartId id, Transform node, PartEntry entry, UnitProfile profile,
            float scale, float massFactor, Func<string, Visual?> visuals, int bodyLayer)
        {
            var part = node.gameObject.AddComponent<BodyPart>();
            part.id = id;
            part.restLocalPosition = node.localPosition;
            part.restLocalRotation = node.localRotation;
            part.limits = JointLimitTable.For(id);

            var visual = string.IsNullOrEmpty(entry.mesh) ? null : visuals?.Invoke(entry.mesh);
            if (visual.HasValue && visual.Value.mesh != null)
            {
                var v = visual.Value;
                var meshObject = new GameObject("Mesh");
                meshObject.transform.SetParent(node, false);
                meshObject.transform.localPosition = v.offset * scale;
                meshObject.transform.localRotation = v.rotation.w == 0f && v.rotation.x == 0f ? Quaternion.identity : v.rotation;
                meshObject.transform.localScale = (v.scale == Vector3.zero ? Vector3.one : v.scale) * scale;
                meshObject.AddComponent<MeshFilter>().sharedMesh = visual.Value.mesh;
                meshObject.AddComponent<MeshRenderer>().sharedMaterials = visual.Value.materials ?? new Material[0];
            }

            part.partCollider = AddCollider(node, entry.collider, scale, bodyLayer, "Collider");

            var body = node.gameObject.AddComponent<Rigidbody>();
            body.mass = Mathf.Max(0.1f, entry.massKg * massFactor);
            ConfigureBody(body, profile);
            part.body = body;
            return part;
        }

        public static void ConfigureBody(Rigidbody body, UnitProfile profile)
        {
            body.useGravity = true;
            body.linearDamping = profile.linearDamping;
            body.angularDamping = profile.angularDamping;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.solverIterations = profile.solverIterations;
            body.solverVelocityIterations = profile.solverVelocityIterations;
            body.maxAngularVelocity = profile.maxAngularVelocity;
            body.maxDepenetrationVelocity = profile.maxDepenetrationVelocity;
            body.automaticCenterOfMass = true;
            body.automaticInertiaTensor = true;
        }

        /// <summary>
        /// Adds one collider on a child of <paramref name="parent"/>. Capsules are given by the centres of
        /// their two rounded ends, so the child is rotated to lie along start-to-end.
        /// </summary>
        public static Collider AddCollider(Transform parent, ColliderEntry entry, float scale, int layer, string name)
        {
            if (entry == null || string.IsNullOrEmpty(entry.type))
            {
                return null;
            }

            var holder = new GameObject(name);
            holder.layer = layer;
            holder.transform.SetParent(parent, false);

            switch (entry.type)
            {
                case "box":
                {
                    holder.transform.localPosition = RigMath.ToVector(entry.center) * scale;
                    var box = holder.AddComponent<BoxCollider>();
                    box.size = RigMath.ToVector(entry.size) * scale;
                    return box;
                }
                case "sphere":
                {
                    holder.transform.localPosition = RigMath.ToVector(entry.center) * scale;
                    var sphere = holder.AddComponent<SphereCollider>();
                    sphere.radius = entry.radius * scale;
                    return sphere;
                }
                case "capsule":
                {
                    var start = RigMath.ToVector(entry.start) * scale;
                    var end = RigMath.ToVector(entry.end) * scale;
                    var axis = end - start;
                    holder.transform.localPosition = (start + end) * 0.5f;
                    holder.transform.localRotation = axis.sqrMagnitude > 1e-8f
                        ? Quaternion.FromToRotation(Vector3.up, axis.normalized)
                        : Quaternion.identity;
                    var capsule = holder.AddComponent<CapsuleCollider>();
                    capsule.direction = 1;
                    capsule.radius = entry.radius * scale;
                    capsule.height = axis.magnitude + 2f * entry.radius * scale;
                    return capsule;
                }
                default:
                    UnityEngine.Object.DestroyImmediate(holder);
                    throw new FormatException($"Unknown collider type '{entry.type}'.");
            }
        }

        private static ConfigurableJoint AddJoint(BodyPart part, Rigidbody parentBody)
        {
            var joint = part.gameObject.AddComponent<ConfigurableJoint>();
            joint.connectedBody = parentBody;
            joint.autoConfigureConnectedAnchor = true;
            joint.anchor = Vector3.zero;                     // the part's origin is the joint
            joint.axis = Vector3.right;                      // joint space == the part's local space
            joint.secondaryAxis = Vector3.up;
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;   // never Locked: PhysX drops the slerp
            joint.angularZMotion = ConfigurableJointMotion.Limited;   // drive if any angular axis is locked
            JointMath.ApplyLimits(joint, part.limits);
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.slerpDrive = new JointDrive { positionSpring = 0f, positionDamper = 0f, maximumForce = float.MaxValue, useAcceleration = true };
            joint.targetRotation = Quaternion.identity;
            joint.enableCollision = false;
            joint.enablePreprocessing = false;
            return joint;
        }
    }
}
