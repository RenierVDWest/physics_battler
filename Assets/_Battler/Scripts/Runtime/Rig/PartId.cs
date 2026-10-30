using System.Collections.Generic;

namespace PhysicsBattler.Rig
{
    /// <summary>The 15 rigid body parts of a humanoid unit, one per null in the rig.</summary>
    public enum PartId
    {
        Pelvis,
        Spine,
        Chest,
        Head,
        UpperArmL,
        ForearmL,
        HandL,
        UpperArmR,
        ForearmR,
        HandR,
        ThighL,
        ShinL,
        FootL,
        ThighR,
        ShinR,
        FootR,
    }

    /// <summary>Muscle groups: each group's strength is scaled together by states and actions.</summary>
    public enum MuscleGroup
    {
        Legs,
        Core,
        Neck,
        ArmL,
        ArmR,
    }

    /// <summary>
    /// Static facts about the parts: their null names in the rig, parents, muscle groups and
    /// neighbours. The rig's tree is fixed, so this is plain data rather than something read
    /// from the prefab.
    /// </summary>
    public static class PartInfo
    {
        public const int Count = 16;

        public static readonly PartId[] All =
        {
            PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head,
            PartId.UpperArmL, PartId.ForearmL, PartId.HandL,
            PartId.UpperArmR, PartId.ForearmR, PartId.HandR,
            PartId.ThighL, PartId.ShinL, PartId.FootL,
            PartId.ThighR, PartId.ShinR, PartId.FootR,
        };

        private static readonly string[] NullNames =
        {
            "Pelvis", "Spine", "Chest", "Head",
            "UpperArm_L", "Forearm_L", "Hand_L",
            "UpperArm_R", "Forearm_R", "Hand_R",
            "Thigh_L", "Shin_L", "Foot_L",
            "Thigh_R", "Shin_R", "Foot_R",
        };

        // Parent part of each part, or -1 for the pelvis (the root body).
        private static readonly int[] Parents =
        {
            -1, (int)PartId.Pelvis, (int)PartId.Spine, (int)PartId.Chest,
            (int)PartId.Chest, (int)PartId.UpperArmL, (int)PartId.ForearmL,
            (int)PartId.Chest, (int)PartId.UpperArmR, (int)PartId.ForearmR,
            (int)PartId.Pelvis, (int)PartId.ThighL, (int)PartId.ShinL,
            (int)PartId.Pelvis, (int)PartId.ThighR, (int)PartId.ShinR,
        };

        private static readonly MuscleGroup[] Groups =
        {
            MuscleGroup.Core, MuscleGroup.Core, MuscleGroup.Core, MuscleGroup.Neck,
            MuscleGroup.ArmL, MuscleGroup.ArmL, MuscleGroup.ArmL,
            MuscleGroup.ArmR, MuscleGroup.ArmR, MuscleGroup.ArmR,
            MuscleGroup.Legs, MuscleGroup.Legs, MuscleGroup.Legs,
            MuscleGroup.Legs, MuscleGroup.Legs, MuscleGroup.Legs,
        };

        // How much more (or less) damage a hit on this part does. Head hits hurt most.
        private static readonly float[] DamageMultipliers =
        {
            1.0f, 1.0f, 1.0f, 1.5f,
            0.7f, 0.7f, 0.6f,
            0.7f, 0.7f, 0.6f,
            0.8f, 0.7f, 0.6f,
            0.8f, 0.7f, 0.6f,
        };

        public static string NullName(PartId part) => NullNames[(int)part];

        public static bool TryParse(string nullName, out PartId part)
        {
            for (int i = 0; i < NullNames.Length; i++)
            {
                if (NullNames[i] == nullName)
                {
                    part = (PartId)i;
                    return true;
                }
            }

            part = PartId.Pelvis;
            return false;
        }

        public static bool HasParent(PartId part) => Parents[(int)part] >= 0;

        /// <summary>The parent part. Only valid when <see cref="HasParent"/> is true.</summary>
        public static PartId Parent(PartId part) => (PartId)Parents[(int)part];

        public static MuscleGroup Group(PartId part) => Groups[(int)part];

        public static float DamageMultiplier(PartId part) => DamageMultipliers[(int)part];

        public static bool IsFoot(PartId part) => part == PartId.FootL || part == PartId.FootR;

        public static bool IsHand(PartId part) => part == PartId.HandL || part == PartId.HandR;

        /// <summary>The part itself, its parent and its children: what a hit there softens.</summary>
        public static IEnumerable<PartId> SelfAndNeighbours(PartId part)
        {
            yield return part;
            if (HasParent(part))
            {
                yield return Parent(part);
            }

            for (int i = 0; i < Parents.Length; i++)
            {
                if (Parents[i] == (int)part)
                {
                    yield return (PartId)i;
                }
            }
        }
    }
}
