using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// The invisible animation rig: a copy of the unit's null tree (parts and markers only, no meshes,
    /// colliders or physics) that the procedural animation poses each step. The muscles then try to
    /// make the physical body match it. "The animation is a wish."
    /// </summary>
    public sealed class GhostRig
    {
        private readonly Transform[] parts = new Transform[PartInfo.Count];
        private readonly Vector3[] restPositions = new Vector3[PartInfo.Count];
        private readonly Quaternion[] restRotations = new Quaternion[PartInfo.Count];

        public Transform Root { get; }
        public Transform SlotWeaponR { get; private set; }
        public Transform SlotWeaponL { get; private set; }
        public Transform AimChest { get; private set; }

        /// <summary>
        /// Clones the physical rig's nulls under a "Ghost" object parented to <paramref name="unitRoot"/>.
        /// Call before the physical body is detached from the unit root.
        /// </summary>
        public GhostRig(Transform unitRoot, UnitRig rig)
        {
            Root = new GameObject("Ghost").transform;
            Root.SetParent(unitRoot, false);

            var map = new Dictionary<Transform, Transform>();
            var pelvis = rig.Part(PartId.Pelvis).transform;
            Clone(pelvis, Root, map, rig);

            foreach (var id in PartInfo.All)
            {
                var ghost = map[rig.Part(id).transform];
                parts[(int)id] = ghost;
                restPositions[(int)id] = ghost.localPosition;
                restRotations[(int)id] = ghost.localRotation;
            }

            SlotWeaponR = rig.SlotWeaponR != null && map.TryGetValue(rig.SlotWeaponR, out var slotR) ? slotR : null;
            SlotWeaponL = rig.SlotWeaponL != null && map.TryGetValue(rig.SlotWeaponL, out var slotL) ? slotL : null;
            AimChest = rig.AimChest != null && map.TryGetValue(rig.AimChest, out var aim) ? aim : null;
        }

        private static void Clone(Transform source, Transform parent, Dictionary<Transform, Transform> map, UnitRig rig)
        {
            var copy = new GameObject("G_" + source.name).transform;
            copy.SetParent(parent, false);
            copy.localPosition = source.localPosition;
            copy.localRotation = source.localRotation;
            map[source] = copy;

            for (int i = 0; i < source.childCount; i++)
            {
                var child = source.GetChild(i);
                if (IsNull(child, rig))
                {
                    Clone(child, copy, map, rig);
                }
            }
        }

        /// <summary>Body parts and marker nulls are copied; mesh and collider children are not.</summary>
        private static bool IsNull(Transform child, UnitRig rig)
        {
            return child.GetComponent<BodyPart>() != null
                   || child == rig.SlotWeaponR || child == rig.SlotWeaponL
                   || child == rig.AimChest || child == rig.AimHead;
        }

        public Transform Part(PartId id) => parts[(int)id];

        public Vector3 RestLocalPosition(PartId id) => restPositions[(int)id];

        public Quaternion RestLocalRotation(PartId id) => restRotations[(int)id];

        /// <summary>Puts every part back in the rest pose (identity rotations, rest offsets).</summary>
        public void ResetToRest()
        {
            foreach (var id in PartInfo.All)
            {
                parts[(int)id].localPosition = restPositions[(int)id];
                parts[(int)id].localRotation = restRotations[(int)id];
            }
        }
    }
}
