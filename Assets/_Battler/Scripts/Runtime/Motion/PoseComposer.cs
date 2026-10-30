using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>What the composer needs to know this step.</summary>
    public struct PoseRequest
    {
        public Transform root;
        public UnitStateKind state;
        /// <summary>0..1 through getting up.</summary>
        public float getUpProgress;
        public ActionPose action;
        public float time;
        public float scale;
        /// <summary>Copy the physical weapon arm into the ghost, so its muscles stop steering (abandoned swing).</summary>
        public bool releaseWeaponArm;
        /// <summary>A weapon is held in the right hand: idle, the hand carries it angled down and out.</summary>
        public bool holdingWeapon;
    }

    /// <summary>
    /// Builds the ghost's pose every physics step: the stance (pelvis, breathing, legs reaching
    /// their planted feet by IK), the action layered on top (torso twist and lean, the weapon arm
    /// reaching its grip by IK), the free arm and the head looking at the target.
    /// </summary>
    public sealed class PoseComposer
    {
        /// <summary>
        /// How the right hand carries a weapon at rest, relative to the unit root: tipped 15° down and
        /// turned 15° out to the right, so a long weapon points away from whoever stands in front.
        /// </summary>
        public static readonly Quaternion WeaponCarry = Quaternion.Euler(15f, 15f, 0f);

        private readonly GhostRig ghost;
        private readonly UnitRig rig;
        private readonly Stance stance;

        public PoseComposer(GhostRig ghost, UnitRig rig, Stance stance)
        {
            this.ghost = ghost;
            this.rig = rig;
            this.stance = stance;
        }

        public void Compose(in PoseRequest request)
        {
            ghost.ResetToRest();
            if (request.state == UnitStateKind.Knockdown || request.state == UnitStateKind.Dead)
            {
                BracePose();
                return;
            }

            var action = request.action;
            var root = request.root;
            bool staggered = request.state == UnitStateKind.Stagger;
            float rise = request.state == UnitStateKind.GettingUp ? Ease.InOutCubic(request.getUpProgress) : 1f;

            // Torso.
            float breathe = Mathf.Sin(request.time * 2f * Mathf.PI * 0.35f);
            var pelvis = ghost.Part(PartId.Pelvis);
            float drop = (1f - rise) * 0.45f * request.scale + (staggered ? 0.04f * request.scale : 0f);
            Vector3 offset = action.active ? action.pelvisOffset : Vector3.zero;
            pelvis.localPosition = ghost.RestLocalPosition(PartId.Pelvis) + offset
                                   + Vector3.up * (breathe * 0.006f * request.scale - drop);
            pelvis.localRotation = Quaternion.Euler(0f, action.active ? action.pelvisYaw : 0f, 0f);
            ghost.Part(PartId.Spine).localRotation =
                Quaternion.Euler(action.spinePitch + (1f - rise) * 30f + breathe * 0.8f, action.spineYaw, 0f);
            ghost.Part(PartId.Chest).localRotation =
                Quaternion.Euler(action.chestPitch + breathe * 1.2f, action.chestYaw, 0f);

            // Legs reach the planted feet; feet stay flat and face the root's way.
            SolveLeg(PartId.ThighL, PartId.ShinL, PartId.FootL, root);
            SolveLeg(PartId.ThighR, PartId.ShinR, PartId.FootR, root);

            // Arms: relaxed, held out a little more when staggered.
            float sway = Mathf.Sin(request.time * 2f * Mathf.PI * 0.35f + 1.3f) * 2f;
            float spread = staggered ? 28f : 6f;
            ghost.Part(PartId.UpperArmR).localRotation = Quaternion.Euler(-6f + sway, 0f, spread);
            ghost.Part(PartId.ForearmR).localRotation = Quaternion.Euler(-14f, 0f, 0f);
            float freePitch = action.active ? action.freeArmPitch : 0f;
            float freeOut = action.active ? action.freeArmOut : 0f;
            ghost.Part(PartId.UpperArmL).localRotation = Quaternion.Euler(-6f - sway + freePitch, 0f, -(spread + freeOut));
            ghost.Part(PartId.ForearmL).localRotation = Quaternion.Euler(-14f - Mathf.Max(0f, -freePitch) * 0.5f, 0f, 0f);
            if (request.holdingWeapon)
            {
                ghost.Part(PartId.HandR).rotation = root.rotation * WeaponCarry;
            }

            if (action.active && action.weaponArmWeight > 0f)
            {
                SolveWeaponArm(action);
            }

            if (request.releaseWeaponArm)
            {
                CopyPhysical(PartId.UpperArmR);
                CopyPhysical(PartId.ForearmR);
                CopyPhysical(PartId.HandR);
            }

            if (action.active && action.hasLookTarget)
            {
                LookAt(action.lookTarget, 0.8f);
            }
        }

        private void SolveLeg(PartId thigh, PartId shin, PartId foot, Transform root)
        {
            TwoBoneIK.Solve(ghost.Part(thigh), ghost.Part(shin), ghost.Part(foot), stance.Current(foot), root.forward);
            ghost.Part(foot).rotation = root.rotation;
        }

        private void SolveWeaponArm(in ActionPose action)
        {
            var upper = ghost.Part(PartId.UpperArmR);
            var lower = ghost.Part(PartId.ForearmR);
            var hand = ghost.Part(PartId.HandR);
            Quaternion idleUpper = upper.localRotation;
            Quaternion idleLower = lower.localRotation;
            Quaternion idleHand = hand.localRotation;

            // The grip is offset from the wrist by the slot's position in the hand.
            Vector3 slotOffset = ghost.SlotWeaponR != null ? ghost.SlotWeaponR.localPosition : Vector3.zero;
            Vector3 wrist = action.gripTarget - action.handRotation * slotOffset;
            TwoBoneIK.Solve(upper, lower, hand, wrist, action.elbowPole);
            hand.rotation = action.handRotation;

            float w = Mathf.Clamp01(action.weaponArmWeight);
            if (w < 1f)
            {
                upper.localRotation = Quaternion.Slerp(idleUpper, upper.localRotation, w);
                lower.localRotation = Quaternion.Slerp(idleLower, lower.localRotation, w);
                hand.localRotation = Quaternion.Slerp(idleHand, hand.localRotation, w);
            }
        }

        private void CopyPhysical(PartId id)
        {
            ghost.Part(id).localRotation = rig.Part(id).transform.localRotation;
        }

        /// <summary>Turns the head toward a point, within a comfortable range of the chest.</summary>
        private void LookAt(Vector3 point, float weight)
        {
            var head = ghost.Part(PartId.Head);
            var chest = ghost.Part(PartId.Chest);
            Vector3 direction = point - head.position;
            if (direction.sqrMagnitude < 1e-4f)
            {
                return;
            }

            Vector3 local = Quaternion.Inverse(chest.rotation) * direction.normalized;
            float yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -70f, 70f);
            float pitch = Mathf.Clamp(-Mathf.Asin(Mathf.Clamp(local.y, -1f, 1f)) * Mathf.Rad2Deg, -40f, 40f);
            head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(pitch, yaw, 0f), weight);
        }

        /// <summary>Knocked down: curl up a little and brace, arms forward, knees bent.</summary>
        private void BracePose()
        {
            ghost.Part(PartId.Spine).localRotation = Quaternion.Euler(20f, 0f, 0f);
            ghost.Part(PartId.Chest).localRotation = Quaternion.Euler(10f, 0f, 0f);
            ghost.Part(PartId.Head).localRotation = Quaternion.Euler(25f, 0f, 0f);
            ghost.Part(PartId.UpperArmR).localRotation = Quaternion.Euler(-60f, 0f, 20f);
            ghost.Part(PartId.UpperArmL).localRotation = Quaternion.Euler(-60f, 0f, -20f);
            ghost.Part(PartId.ForearmR).localRotation = Quaternion.Euler(-60f, 0f, 0f);
            ghost.Part(PartId.ForearmL).localRotation = Quaternion.Euler(-60f, 0f, 0f);
            ghost.Part(PartId.ThighR).localRotation = Quaternion.Euler(-45f, 0f, 8f);
            ghost.Part(PartId.ThighL).localRotation = Quaternion.Euler(-45f, 0f, -8f);
            ghost.Part(PartId.ShinR).localRotation = Quaternion.Euler(70f, 0f, 0f);
            ghost.Part(PartId.ShinL).localRotation = Quaternion.Euler(70f, 0f, 0f);
        }
    }
}
