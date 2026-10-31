using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Builds the unit's pose on its animation rig, once per physics step, in a fixed order:
    ///
    ///  1. Stance: the feet, planted or stepping.
    ///  2. Idle: the hips drift and bob, the chest and head weave (CombatIdle).
    ///  3. Upper body: hip turn, lean, chest counter-turn, head held toward the front, arms in guard.
    ///  4. Weight: the hips move so the centre of mass sits over the support point between the feet
    ///     (more over the lead foot as the style asks). Leaning forward pushes the hips back.
    ///  5. Legs: the hips are lowered if a foot can't be reached, then each leg is solved (two-bone
    ///     IK) so the ankle sits over its planted foot, and the foot lies flat, turned with its step.
    ///
    /// The legs always end on the feet, so they always carry the body: nothing holds it up.
    /// </summary>
    public sealed class PosePipeline
    {
        private readonly Transform root;
        private readonly AnimRig anim;
        private readonly BodyMeasure body;
        private readonly Stance stance;
        private readonly CombatIdle idle;

        public CombatStyle Style { get; set; }

        // What the last step produced, for the log.
        public Vector3 CenterOfMass { get; private set; }
        public Vector3 SupportPoint { get; private set; }
        public float PelvisDrop { get; private set; }
        public float WeightShift { get; private set; }
        public IdleSample LastIdle { get; private set; }

        /// <summary>How far the hips may move to put the weight over the support point (fraction of leg length).</summary>
        public const float MaxWeightShift = 0.14f;

        public PosePipeline(Transform root, AnimRig anim, BodyMeasure body, Stance stance, CombatIdle idle, CombatStyle style)
        {
            this.root = root;
            this.anim = anim;
            this.body = body;
            this.stance = stance;
            this.idle = idle;
            Style = style;
        }

        public void Compose(float time, float dt)
        {
            var style = Style;
            float leg = body.LegLength;
            float yaw = root.eulerAngles.y;
            var facing = Quaternion.Euler(0f, yaw, 0f);
            float leadSign = stance.LeadLeft ? 1f : -1f;

            // 1. Feet
            stance.GroundY = root.position.y;
            stance.Update(root.position, yaw, dt);

            // 2. Idle
            var s = idle.Sample(time, style, leg);
            LastIdle = s;

            anim.ResetToRest();

            // 3. Upper body. Hips over the weighted point between the feet, sunk by the crouch.
            int lead = stance.LeadLeft ? Stance.Left : Stance.Right;
            int rear = 1 - lead;
            Vector3 support = Vector3.Lerp(stance.Position(rear), stance.Position(lead), style.leadWeight);
            support.y = root.position.y;
            SupportPoint = support;

            var pelvis = anim.Part(PartId.Pelvis);
            pelvis.rotation = facing * Quaternion.Euler(s.hips.x, leadSign * style.hipTurn + s.hips.y, s.hips.z);
            Vector3 hips = support + facing * s.hipShift;
            hips.y = root.position.y + body.PelvisHeight - style.crouch * leg + s.bob;
            pelvis.position = hips;

            anim.Part(PartId.Spine).localRotation = Quaternion.Euler(style.lean * 0.5f, 0f, 0f);
            anim.Part(PartId.Chest).localRotation = Quaternion.Euler(style.lean * 0.5f + s.chest.x,
                leadSign * style.chestCounterTurn + s.chest.y, s.chest.z);
            // The head keeps looking ahead: it undoes most of the turn below it.
            float turnBelow = leadSign * (style.hipTurn + style.chestCounterTurn) + s.hips.y + s.chest.y;
            anim.Part(PartId.Head).localRotation = Quaternion.Euler(-style.lean + s.head.x, -0.9f * turnBelow + s.head.y, s.head.z);
            Guard(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, +1f, stance.LeadLeft, style);
            Guard(PartId.UpperArmR, PartId.ForearmR, PartId.HandR, -1f, !stance.LeadLeft, style);

            // 4. Weight over the support point (plus the sway, which is meant to move it).
            Vector3 wanted = support + facing * s.hipShift;
            Vector3 com = CenterOfMassNow();
            Vector3 shift = wanted - com;
            shift.y = 0f;
            shift = Vector3.ClampMagnitude(shift * 0.85f, MaxWeightShift * leg);
            WeightShift = shift.magnitude;
            pelvis.position += shift;

            // 5. Legs: lower the hips if a foot is out of reach, then solve each leg.
            PelvisDrop = 0f;
            for (int f = 0; f < 2; f++)
            {
                var thigh = anim.Part(f == Stance.Left ? PartId.ThighL : PartId.ThighR);
                Vector3 ankle = AnkleTarget(f);
                Vector3 hip = thigh.position;
                Vector3 flat = ankle - hip;
                flat.y = 0f;
                float reach = 0.985f * leg;
                float vertical = Mathf.Sqrt(Mathf.Max(0f, reach * reach - flat.sqrMagnitude));
                float excess = (hip.y - ankle.y) - vertical;
                PelvisDrop = Mathf.Max(PelvisDrop, excess);
            }

            if (PelvisDrop > 0f)
            {
                pelvis.position += Vector3.down * PelvisDrop;
            }

            for (int f = 0; f < 2; f++)
            {
                SolveLeg(f);
            }

            CenterOfMass = CenterOfMassNow();
        }

        private void Guard(PartId upper, PartId fore, PartId hand, float inSign, bool lead, CombatStyle style)
        {
            float raise = style.guardRaise + (lead ? style.leadArmReach : 0f);
            anim.Part(upper).localRotation = Quaternion.Euler(raise, 0f, inSign * style.guardIn);
            anim.Part(fore).localRotation = Quaternion.Euler(style.guardElbow - (lead ? 0.5f * style.leadArmReach : 0f), 0f, 0f);
            anim.Part(hand).localRotation = Quaternion.Euler(-10f, 0f, 0f);
        }

        public Vector3 AnkleTarget(int foot)
        {
            Vector3 ground = stance.Position(foot);
            return ground + Vector3.up * (body.AnkleHeight + stance.Lift(foot));
        }

        private void SolveLeg(int foot)
        {
            bool left = foot == Stance.Left;
            var thigh = anim.Part(left ? PartId.ThighL : PartId.ThighR);
            var shin = anim.Part(left ? PartId.ShinL : PartId.ShinR);
            var ankle = anim.Part(left ? PartId.FootL : PartId.FootR);
            var footYaw = Quaternion.Euler(0f, stance.Yaw(foot), 0f);
            // Knees point over the toes, a little outward.
            Vector3 pole = footYaw * new Vector3(left ? -0.15f : 0.15f, 0f, 1f);
            TwoBoneIK.Solve(thigh, shin, ankle, AnkleTarget(foot), pole);
            // Flat on the ground, toes down a little as it lifts.
            float t = stance.StepProgress(foot);
            float pitch = stance.Stepping(foot) ? 18f * Mathf.Sin(Mathf.PI * t) * (t < 0.5f ? 1f : -0.6f) : 0f;
            ankle.rotation = footYaw * Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>
        /// Mass-weighted centre of the posed parts. Each limb part's mass sits at the middle of its
        /// bone (between its pivot and its child's); hands, feet and the head at their own pivots.
        /// </summary>
        public Vector3 CenterOfMassNow()
        {
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (var id in PartInfo.All)
            {
                Vector3 p = anim.Part(id).position;
                var child = BoneEnd(id);
                if (child.HasValue)
                {
                    p = 0.5f * (p + anim.Part(child.Value).position);
                }

                float m = body.PartMass[(int)id];
                sum += p * m;
                mass += m;
            }

            return mass > 0f ? sum / mass : root.position;
        }

        private static PartId? BoneEnd(PartId id)
        {
            switch (id)
            {
                case PartId.Pelvis: return PartId.Spine;
                case PartId.Spine: return PartId.Chest;
                case PartId.Chest: return PartId.Head;
                case PartId.UpperArmL: return PartId.ForearmL;
                case PartId.ForearmL: return PartId.HandL;
                case PartId.UpperArmR: return PartId.ForearmR;
                case PartId.ForearmR: return PartId.HandR;
                case PartId.ThighL: return PartId.ShinL;
                case PartId.ShinL: return PartId.FootL;
                case PartId.ThighR: return PartId.ShinR;
                case PartId.ShinR: return PartId.FootR;
                default: return null;
            }
        }
    }
}
