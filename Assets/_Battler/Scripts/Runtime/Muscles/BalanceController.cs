using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// Keeps a unit on its feet without physically honest balance: a tunable spring-damper
    /// (the "hand of god") holds the pelvis at the ghost's height and turns the pelvis and chest
    /// upright, scaled by the state's balance value, so hits take the support away. It also lets the
    /// unit's root follow its body when shoved (re-anchoring), and plans catch steps toward the
    /// capture point when the body is falling.
    /// </summary>
    public sealed class BalanceController
    {
        private readonly UnitRig rig;
        private readonly GhostRig ghost;
        private readonly UnitProfile profile;
        private readonly Stance stance;

        public Vector3 CenterOfMass { get; private set; }
        public Vector3 CapturePoint { get; private set; }
        public Vector3 SupportCenter { get; private set; }

        /// <summary>Horizontal distance from the support centre to the capture point.</summary>
        public float BalanceError { get; private set; }

        public BalanceController(UnitRig rig, GhostRig ghost, UnitProfile profile, Stance stance)
        {
            this.rig = rig;
            this.ghost = ghost;
            this.profile = profile;
            this.stance = stance;
        }

        /// <summary>Measures where the body's weight is going. Call once per step before the others.</summary>
        public void Sense(float groundY)
        {
            CenterOfMass = rig.CenterOfMass();
            Vector3 velocity = rig.CenterOfMassVelocity();
            float height = Mathf.Max(0.1f, CenterOfMass.y - groundY);
            Vector3 capture = CenterOfMass + velocity * Mathf.Sqrt(height / Mathf.Abs(Physics.gravity.y));
            capture.y = groundY;
            CapturePoint = capture;

            Vector3 support = (rig.Part(PartId.FootL).body.position + rig.Part(PartId.FootR).body.position) * 0.5f;
            support.y = groundY;
            SupportCenter = support;
            BalanceError = Vector3.Distance(CapturePoint, SupportCenter);
        }

        /// <summary>
        /// Moves the unit's root so the body is never more than the anchor radius from where the unit
        /// thinks it is. Being knocked aside then really moves the unit, instead of the balance
        /// assist dragging it back.
        /// </summary>
        public void Reanchor(Transform root, float groundY, float radius)
        {
            Vector3 pelvis = rig.Part(PartId.Pelvis).body.position;
            Vector3 ghostOffset = ghost.Part(PartId.Pelvis).localPosition;
            ghostOffset.y = 0f;
            Vector3 wanted = pelvis - root.rotation * ghostOffset;
            Vector3 delta = wanted - root.position;
            delta.y = 0f;
            float distance = delta.magnitude;
            Vector3 position = root.position;
            if (distance > radius)
            {
                position += delta * ((distance - radius) / distance);
            }

            position.y = groundY;
            root.position = position;
        }

        /// <summary>
        /// Starts a catch step when the capture point leaves the feet. Returns false when the unit is out
        /// of steps and still falling (it should go down).
        /// </summary>
        public bool UpdateCatchSteps(float groundY, float scale)
        {
            if (stance.Stepping)
            {
                return true;
            }

            float threshold = profile.stepThreshold * scale;
            if (BalanceError <= threshold)
            {
                return true;
            }

            if (stance.StepsTaken >= profile.maxSteps)
            {
                return BalanceError < threshold * 3f;
            }

            Vector3 left = stance.Planted(PartId.FootL);
            Vector3 right = stance.Planted(PartId.FootR);
            Vector3 capture = CapturePoint;
            bool moveLeft = Flat(left - capture).sqrMagnitude > Flat(right - capture).sqrMagnitude;
            Vector3 staying = moveLeft ? right : left;
            Vector3 moving = moveLeft ? left : right;

            // Land the foot just beside the capture point, keeping some stance width.
            Vector3 sideways = Flat(moving - staying);
            Vector3 target = capture + (sideways.sqrMagnitude > 1e-6f ? sideways.normalized : Vector3.zero) * (0.1f * scale);
            Vector3 fromStaying = Flat(target - staying);
            float maxLength = profile.maxStepLength * scale;
            if (fromStaying.magnitude > maxLength)
            {
                target = staying + fromStaying.normalized * maxLength;
            }

            target.y = moving.y;
            stance.BeginStep(moveLeft ? PartId.FootL : PartId.FootR, target, profile.stepDuration, profile.stepHeight * scale);
            return true;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>Applies the support force and the upright torques, scaled by <paramref name="balance"/>.</summary>
        public void ApplyForces(float balance, bool grounded)
        {
            if (balance <= 0.001f)
            {
                return;
            }

            var pelvis = rig.Part(PartId.Pelvis).body;
            float mass = rig.TotalMass();
            float g = Mathf.Abs(Physics.gravity.y);

            Vector3 error = ghost.Part(PartId.Pelvis).position - pelvis.position;
            Vector3 velocity = pelvis.linearVelocity;

            float vertical = mass * (profile.kpVertical * error.y - profile.kdVertical * velocity.y);
            if (grounded)
            {
                vertical += mass * g * profile.supportFeedForward;
            }
            else
            {
                vertical = Mathf.Min(vertical, 0f);   // no holding the unit up in mid-air
            }

            vertical = Mathf.Clamp(vertical * balance, -mass * g, mass * g * 2f);
            Vector3 horizontalError = new Vector3(error.x, 0f, error.z);
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 horizontal = balance * mass * (profile.kpHorizontal * horizontalError - profile.kdHorizontal * horizontalVelocity);
            pelvis.AddForce(Vector3.up * vertical + horizontal, ForceMode.Force);

            Upright(pelvis, ghost.Part(PartId.Pelvis).rotation, balance);
            Upright(rig.Part(PartId.Chest).body, ghost.Part(PartId.Chest).rotation, balance);

            if (profile.headLift > 0f)
            {
                rig.Part(PartId.Head).body.AddForce(Vector3.up * (profile.headLift * balance * g), ForceMode.Acceleration);
            }
        }

        private void Upright(Rigidbody body, Quaternion wanted, float balance)
        {
            Vector3 error = JointMath.RotationError(body.rotation, wanted);
            Vector3 torque = balance * (profile.kpUpright * error - profile.kdUpright * body.angularVelocity);
            body.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}
