using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// The animation rig: an invisible copy of the unit's null tree that the pose pipeline poses
    /// every physics step. It is the authority over what the body should look like; the physical
    /// parts follow it as far as the authority layer allows. Built from the physical rig while that
    /// is still in its rest pose.
    /// </summary>
    public sealed class AnimRig
    {
        private readonly Transform[] parts = new Transform[PartInfo.Count];
        private readonly Vector3[] restPositions = new Vector3[PartInfo.Count];
        private readonly Quaternion[] restRotations = new Quaternion[PartInfo.Count];

        public Transform Root { get; }
        public Transform SlotWeaponR { get; }
        public Transform SlotWeaponL { get; }
        public Transform AimChest { get; }
        public Transform AimHead { get; }
        public Transform AimNeck { get; }
        public Transform AimBelly { get; }

        public AnimRig(Transform unitRoot, UnitRig rig)
        {
            Root = new GameObject("Anim").transform;
            Root.SetParent(unitRoot, false);

            var map = new Dictionary<Transform, Transform>();
            Clone(rig.Part(PartId.Pelvis).transform, Root, map, rig);
            foreach (var id in PartInfo.All)
            {
                var copy = map[rig.Part(id).transform];
                parts[(int)id] = copy;
                restPositions[(int)id] = copy.localPosition;
                restRotations[(int)id] = copy.localRotation;
            }

            SlotWeaponR = Lookup(map, rig.SlotWeaponR);
            SlotWeaponL = Lookup(map, rig.SlotWeaponL);
            AimChest = Lookup(map, rig.AimChest);
            AimHead = Lookup(map, rig.AimHead);
            AimNeck = Lookup(map, rig.AimNeck);
            AimBelly = Lookup(map, rig.AimBelly);
        }

        private static Transform Lookup(Dictionary<Transform, Transform> map, Transform source)
        {
            return source != null && map.TryGetValue(source, out var copy) ? copy : null;
        }

        private static void Clone(Transform source, Transform parent, Dictionary<Transform, Transform> map, UnitRig rig)
        {
            var copy = new GameObject("A_" + source.name).transform;
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

        private static bool IsNull(Transform child, UnitRig rig)
        {
            return child.GetComponent<BodyPart>() != null
                   || child == rig.SlotWeaponR || child == rig.SlotWeaponL
                   || child == rig.AimChest || child == rig.AimHead || child == rig.AimNeck || child == rig.AimBelly;
        }

        public Transform Part(PartId id) => parts[(int)id];

        public Vector3 RestLocalPosition(PartId id) => restPositions[(int)id];

        public Quaternion RestLocalRotation(PartId id) => restRotations[(int)id];

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
