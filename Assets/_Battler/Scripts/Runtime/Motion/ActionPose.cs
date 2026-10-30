using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// What an action (an attack, for now) asks of the body this step, layered on top of the
    /// stance by the <see cref="PoseComposer"/>. Offsets are in the unit root's space.
    /// </summary>
    public struct ActionPose
    {
        public bool active;

        // Body: pelvis offset (x right, y up, z forward) and yaw/pitch per torso part, in degrees.
        public Vector3 pelvisOffset;
        public float pelvisYaw;
        public float spineYaw;
        public float chestYaw;
        public float spinePitch;
        public float chestPitch;

        // Weapon arm: blend weight, where the weapon's grip should be and how the hand turns (world).
        public float weaponArmWeight;
        public Vector3 gripTarget;
        public Quaternion handRotation;
        public Vector3 elbowPole;

        // The free arm swings for balance (shoulder pitch in degrees, negative = forward).
        public float freeArmPitch;
        public float freeArmOut;

        public bool hasLookTarget;
        public Vector3 lookTarget;

        public static ActionPose None => new ActionPose { handRotation = Quaternion.identity };
    }
}
