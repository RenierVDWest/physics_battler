using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The places on a body a blow can be asked for (the plan "Aimed attacks"): its sixteen parts, the neck and the two
    /// shoulders. Left and right are the struck body's own.
    /// </summary>
    public enum AimSpot
    {
        Head,
        Neck,
        ShoulderL,
        ShoulderR,
        Chest,
        Belly,
        Hips,
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

    /// <summary>Where each <see cref="AimSpot"/> is on a fighter, what it is called, and which struck part counts as it.</summary>
    public static class AimSpots
    {
        public static readonly AimSpot[] All =
        {
            AimSpot.Head, AimSpot.Neck, AimSpot.ShoulderL, AimSpot.ShoulderR, AimSpot.Chest, AimSpot.Belly, AimSpot.Hips,
            AimSpot.UpperArmL, AimSpot.ForearmL, AimSpot.HandL, AimSpot.UpperArmR, AimSpot.ForearmR, AimSpot.HandR,
            AimSpot.ThighL, AimSpot.ShinL, AimSpot.FootL, AimSpot.ThighR, AimSpot.ShinR, AimSpot.FootR,
        };

        /// <summary>A neck or a shoulder is this far over the joint it sits on (metres at a Medium's size).</summary>
        public const float OverJoint = 0.04f;

        /// <summary>A blow asked for a neck or a shoulder is on it when it lands this near the spot (metres at a Medium's size) on a part round it.</summary>
        public const float JointSpotWithin = 0.12f;

        /// <summary>The spot has no solid of its own: it lies over a joint, between two parts (the neck, the shoulders).</summary>
        public static bool OverAJoint(AimSpot spot) => spot == AimSpot.Neck || spot == AimSpot.ShoulderL || spot == AimSpot.ShoulderR;

        /// <summary>The spot in words, as the logs say it.</summary>
        public static string Name(AimSpot spot)
        {
            switch (spot)
            {
                case AimSpot.Head: return "head";
                case AimSpot.Neck: return "neck";
                case AimSpot.ShoulderL: return "left shoulder";
                case AimSpot.ShoulderR: return "right shoulder";
                case AimSpot.Chest: return "chest";
                case AimSpot.Belly: return "belly";
                case AimSpot.Hips: return "hips";
                case AimSpot.UpperArmL: return "left upper arm";
                case AimSpot.ForearmL: return "left forearm";
                case AimSpot.HandL: return "left hand";
                case AimSpot.UpperArmR: return "right upper arm";
                case AimSpot.ForearmR: return "right forearm";
                case AimSpot.HandR: return "right hand";
                case AimSpot.ThighL: return "left thigh";
                case AimSpot.ShinL: return "left shin";
                case AimSpot.FootL: return "left foot";
                case AimSpot.ThighR: return "right thigh";
                case AimSpot.ShinR: return "right shin";
                default: return "right foot";
            }
        }

        /// <summary>The part a spot is on (the neck is the head's foot, a shoulder the top of its upper arm).</summary>
        public static PartId PartOf(AimSpot spot)
        {
            switch (spot)
            {
                case AimSpot.Head:
                case AimSpot.Neck: return PartId.Head;
                case AimSpot.ShoulderL:
                case AimSpot.UpperArmL: return PartId.UpperArmL;
                case AimSpot.ShoulderR:
                case AimSpot.UpperArmR: return PartId.UpperArmR;
                case AimSpot.Chest: return PartId.Chest;
                case AimSpot.Belly: return PartId.Spine;
                case AimSpot.Hips: return PartId.Pelvis;
                case AimSpot.ForearmL: return PartId.ForearmL;
                case AimSpot.HandL: return PartId.HandL;
                case AimSpot.ForearmR: return PartId.ForearmR;
                case AimSpot.HandR: return PartId.HandR;
                case AimSpot.ThighL: return PartId.ThighL;
                case AimSpot.ShinL: return PartId.ShinL;
                case AimSpot.FootL: return PartId.FootL;
                case AimSpot.ThighR: return PartId.ThighR;
                case AimSpot.ShinR: return PartId.ShinR;
                default: return PartId.FootR;
            }
        }

        /// <summary>
        /// A blow that struck <paramref name="struck"/> may have landed on the spot asked for: the part itself; for the neck,
        /// the head or the chest (it lies between their two solids); for a shoulder, its upper arm or the chest. For those
        /// three it must also have landed near the spot (<see cref="JointSpotWithin"/>).
        /// </summary>
        public static bool Counts(AimSpot spot, PartId struck)
        {
            if (struck == PartOf(spot))
            {
                return true;
            }

            return (spot == AimSpot.Neck || spot == AimSpot.ShoulderL || spot == AimSpot.ShoulderR) && struck == PartId.Chest;
        }

        /// <summary>Where the spot is on <paramref name="fighter"/> now, as its body is seen (world): the middle of the part's solid; the neck and the shoulders a little over their joints.</summary>
        public static Vector3 Point(ClipFighter fighter, AimSpot spot)
        {
            var rig = fighter.Rig;
            if (spot == AimSpot.Neck)
            {
                var head = rig.Part(PartId.Head).transform;
                return head.position + head.up * (0.5f * OverJoint * fighter.Size);
            }

            if (spot == AimSpot.ShoulderL || spot == AimSpot.ShoulderR)
            {
                var arm = rig.Part(spot == AimSpot.ShoulderL ? PartId.UpperArmL : PartId.UpperArmR).transform;
                return arm.position + rig.Part(PartId.Chest).transform.up * (OverJoint * fighter.Size);
            }

            var part = rig.Part(PartOf(spot));
            return Middle(part.partCollider, part.transform);
        }

        /// <summary>The middle of a part's solid (world); with none, the part's own joint.</summary>
        public static Vector3 Middle(Collider solid, Transform part)
        {
            switch (solid)
            {
                case BoxCollider box: return box.transform.TransformPoint(box.center);
                case SphereCollider sphere: return sphere.transform.TransformPoint(sphere.center);
                case CapsuleCollider capsule: return capsule.transform.TransformPoint(capsule.center);
                default: return solid != null ? solid.transform.position : part.position;
            }
        }
    }
}
