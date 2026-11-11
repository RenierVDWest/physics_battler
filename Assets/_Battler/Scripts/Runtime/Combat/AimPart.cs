using System;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>The body parts a blow can be aimed at: the aim nulls, and the elbow and knee joints.</summary>
    public enum AimPart
    {
        Head,
        Neck,
        Chest,
        Belly,
        ElbowL,
        ElbowR,
        KneeL,
        KneeR,
    }

    [Flags]
    public enum AimParts
    {
        None = 0,
        Head = 1 << AimPart.Head,
        Neck = 1 << AimPart.Neck,
        Chest = 1 << AimPart.Chest,
        Belly = 1 << AimPart.Belly,
        ElbowL = 1 << AimPart.ElbowL,
        ElbowR = 1 << AimPart.ElbowR,
        KneeL = 1 << AimPart.KneeL,
        KneeR = 1 << AimPart.KneeR,
        Elbows = ElbowL | ElbowR,
        Knees = KneeL | KneeR,
        Body = Head | Neck | Chest | Belly,
        All = Body | Elbows | Knees,
    }

    public static class AimPartInfo
    {
        public static readonly AimPart[] All = (AimPart[])Enum.GetValues(typeof(AimPart));

        public static bool Contains(this AimParts set, AimPart part) => (set & (AimParts)(1 << (int)part)) != 0;

        /// <summary>The body part a null belongs to (the part it moves with).</summary>
        public static PartId BodyPartOf(AimPart part)
        {
            switch (part)
            {
                case AimPart.Head: return PartId.Head;
                case AimPart.Neck: return PartId.Head;
                case AimPart.Chest: return PartId.Chest;
                case AimPart.Belly: return PartId.Spine;
                case AimPart.ElbowL: return PartId.ForearmL;
                case AimPart.ElbowR: return PartId.ForearmR;
                case AimPart.KneeL: return PartId.ShinL;
                default: return PartId.ShinR;
            }
        }

        /// <summary>The null on the physical body (where the part really is).</summary>
        public static Transform Physical(UnitRig rig, AimPart part)
        {
            switch (part)
            {
                case AimPart.Head: return rig.AimHead;
                case AimPart.Neck: return rig.AimNeck;
                case AimPart.Chest: return rig.AimChest;
                case AimPart.Belly: return rig.AimBelly;
                default: return rig.Part(BodyPartOf(part)).transform;   // the joint's pivot
            }
        }

        /// <summary>
        /// Where the null is in the physics simulation now: from its part's rigidbody, since the
        /// transforms of interpolated bodies trail the simulation inside FixedUpdate.
        /// </summary>
        public static Vector3 PhysicalPoint(UnitRig rig, AimPart part)
        {
            return rig.Part(BodyPartOf(part)).PhysicalPoint(Physical(rig, part).position);
        }
    }
}
