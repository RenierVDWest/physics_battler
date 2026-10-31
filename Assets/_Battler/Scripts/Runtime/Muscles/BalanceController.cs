using PhysicsBattler.Diagnostics;
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

        /// <summary>How long the body has stayed off its feet with no catch steps left.</summary>
        private float stuckTime;
        private bool bonusStepUsed;

        /// <summary>Out of steps and still off balance for this long: take one more step, then give up and fall.</summary>
        private const float StuckGiveUp = 0.4f;

        public Vector3 CenterOfMass { get; private set; }
        public Vector3 CapturePoint { get; private set; }
        public Vector3 SupportCenter { get; private set; }

        /// <summary>How far the capture point is outside the support area (0 = balanced over the feet).</summary>
        public float BalanceError { get; private set; }

        /// <summary>Why <see cref="UpdateCatchSteps"/> last gave up (the knockdown reason).</summary>
        public string LastVerdict { get; private set; } = "";

        // What the assist applied last step (for the playtest log).
        public float LastVerticalForce { get; private set; }
        public float LastHorizontalForce { get; private set; }
        public float LastUprightPelvis { get; private set; }
        public float LastUprightChest { get; private set; }

        private UnitController owner;
        private UnitController Owner => owner != null ? owner : (owner = rig.GetComponent<UnitController>());

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

            // The support area: the soles of the feet that are down (both if neither is, e.g. airborne).
            soles.Clear();
            bool left = FootDown(PartId.FootL);
            bool right = FootDown(PartId.FootR);
            float scale = profile.scale;
            foreach (var foot in new[] { PartId.FootL, PartId.FootR })
            {
                if ((foot == PartId.FootL ? left : right) || (!left && !right))
                {
                    var body = rig.Part(foot).body;
                    SupportPolygon.AddSole(soles, body.position, body.rotation, SoleLength * scale, SoleWidth * scale, SoleForward * scale);
                }
            }

            SupportPolygon.Hull(soles, supportHull);
            Vector2 centre = SupportPolygon.Centroid(supportHull);
            SupportCenter = new Vector3(centre.x, groundY, centre.y);
            BalanceError = SupportPolygon.DistanceOutside(supportHull, new Vector2(capture.x, capture.z));
        }

        // A foot's sole, about the size of the placeholder orc's at scale 1.
        private const float SoleLength = 0.30f;
        private const float SoleWidth = 0.14f;
        private const float SoleForward = 0.06f;

        private readonly System.Collections.Generic.List<Vector2> soles = new System.Collections.Generic.List<Vector2>(8);
        private readonly System.Collections.Generic.List<Vector2> supportHull = new System.Collections.Generic.List<Vector2>(8);

        /// <summary>The support area's outline on the ground (counter-clockwise; x, z).</summary>
        public System.Collections.Generic.IReadOnlyList<Vector2> SupportHull => supportHull;

        /// <summary>Touching the ground this step (or just now), and not in the middle of a step.</summary>
        private bool FootDown(PartId foot)
        {
            if (stance.Stepping && stance.SteppingFoot == foot)
            {
                return false;
            }

            var part = rig.Part(foot);
            return Time.time - Mathf.Max(part.LastGroundContactTime, part.LastOtherContactTime) < 0.1f;
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
        public bool UpdateCatchSteps(float groundY, float scale, float thresholdFactor = 1f)
        {
            if (stance.StepsTaken == 0)
            {
                bonusStepUsed = false;
            }

            if (stance.Stepping)
            {
                stuckTime = 0f;
                return true;
            }

            float threshold = profile.stepThreshold * scale * thresholdFactor;
            if (BalanceError <= threshold)
            {
                stuckTime = 0f;
                return true;
            }

            if (stance.StepsTaken >= profile.maxSteps)
            {
                if (BalanceError >= threshold * 3f)
                {
                    LastVerdict = $"out of catch steps ({stance.StepsTaken}/{profile.maxSteps}) and falling: balance error {BalanceError:0.00} m ≥ 3× step threshold {threshold:0.00} m";
                    return false;
                }

                // Held off its feet by the assist but never caught: without this it could hang in
                // Stagger forever. One more step, and if that doesn't do it, fall.
                stuckTime += Time.fixedDeltaTime;
                if (stuckTime < StuckGiveUp)
                {
                    return true;
                }

                // Only a little off its feet: keep shuffling rather than fall over (second playtest: an orc
                // went down for a balance error of 0.25 m against a 0.24 m threshold).
                if (BalanceError < threshold * 2f)
                {
                    bonusStepUsed = false;
                }

                if (bonusStepUsed)
                {
                    LastVerdict = $"out of catch steps and stuck off balance for {StuckGiveUp:0.0} s after a bonus step (error {BalanceError:0.00} m > {threshold:0.00} m)";
                    return false;
                }

                bonusStepUsed = true;
                stance.StepsTaken = profile.maxSteps - 1;
                if (PlaytestTrace.Enabled)
                {
                    PlaytestTrace.Note(Owner, "step", $"stuck off balance {StuckGiveUp:0.0} s with no steps left (error {BalanceError:0.00} m): granting one bonus step");
                }
            }

            stuckTime = 0f;
            Vector3 left = stance.Planted(PartId.FootL);
            Vector3 right = stance.Planted(PartId.FootR);
            Vector3 capture = CapturePoint;

            // Falling mostly sideways: step with the foot on the falling side, so the legs never cross.
            // Otherwise (forward or back): step with the foot farther from where the weight is going.
            Vector3 across = Flat(right - left);
            Vector3 fall = Flat(capture - SupportCenter);
            bool moveLeft;
            bool fallingSideways = across.sqrMagnitude > 1e-6f && fall.sqrMagnitude > 1e-6f
                            && Mathf.Abs(Vector3.Dot(fall.normalized, across.normalized)) > 0.7f;
            if (fallingSideways)
            {
                moveLeft = Vector3.Dot(fall, across) < 0f;
            }
            else
            {
                moveLeft = Flat(left - capture).sqrMagnitude > Flat(right - capture).sqrMagnitude;
            }

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
            if (PlaytestTrace.Enabled)
            {
                var root = Owner != null ? Owner.Root : null;
                string direction = root != null ? DescribeDirection(fall, root) : "";
                PlaytestTrace.Note(Owner, "step",
                    $"catch step {(moveLeft ? "FootL" : "FootR")} #{stance.StepsTaken}/{profile.maxSteps}: falling {direction}"
                    + $"{(fallingSideways ? " (sideways: falling-side foot)" : " (farther foot)")}, balance error {BalanceError:0.00} m > {threshold:0.00} m, "
                    + $"step {Flat(target - moving).magnitude:0.00} m{(fromStaying.magnitude > maxLength ? " (clamped to maxStepLength)" : "")}");
            }

            return true;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>Applies the support force and the upright torques, scaled by <paramref name="balance"/>.</summary>
        public void ApplyForces(float balance, bool grounded)
        {
            LastVerticalForce = 0f;
            LastHorizontalForce = 0f;
            LastUprightPelvis = 0f;
            LastUprightChest = 0f;
            if (balance <= 0.001f)
            {
                DriveUpright(pelvisAnchor, rig.Part(PartId.Pelvis).body, ghost.Part(PartId.Pelvis).rotation, 0f, 0f);
                DriveUpright(chestAnchor, rig.Part(PartId.Chest).body, ghost.Part(PartId.Chest).rotation, 0f, 0f);
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
            // While the weight is over the feet the legs keep the pelvis where it belongs; the sideways
            // pull only comes in fully as the body falls out of its support area.
            float threshold = Mathf.Max(0.01f, profile.stepThreshold * profile.scale);
            float pull = Mathf.Lerp(profile.horizontalAssistInside, 1f, Mathf.Clamp01(BalanceError / threshold));
            Vector3 horizontal = balance * pull * mass * (profile.kpHorizontal * horizontalError - profile.kdHorizontal * horizontalVelocity);
            pelvis.AddForce(Vector3.up * vertical + horizontal, ForceMode.Force);
            LastVerticalForce = vertical;
            LastHorizontalForce = horizontal.magnitude;

            // Upright: a share of the whole body's inertia about its feet, split between pelvis and chest,
            // applied through joints to kinematic anchors that carry the ghost's rotation. The joints'
            // drives are solved implicitly by PhysX. (The second playtest applied this as an explicit
            // torque on the pelvis and chest alone, far stiffer than those light bodies can take in one
            // step: everything shook at 30 Hz.)
            float comHeight = Mathf.Max(0.2f, ghost.RestLocalPosition(PartId.Pelvis).y * 1.1f);   // standing, about the feet
            float inertia = Mathf.Max(0f, profile.uprightBodyShare) * mass * comHeight * comHeight * 0.5f;
            LastUprightPelvis = DriveUpright(pelvisAnchor, rig.Part(PartId.Pelvis).body, ghost.Part(PartId.Pelvis).rotation, balance, inertia);
            LastUprightChest = DriveUpright(chestAnchor, rig.Part(PartId.Chest).body, ghost.Part(PartId.Chest).rotation, balance, inertia);

            if (profile.headLift > 0f)
            {
                rig.Part(PartId.Head).body.AddForce(Vector3.up * (profile.headLift * balance * g), ForceMode.Acceleration);
            }
        }

        private sealed class Anchor
        {
            public Rigidbody body;
            public ConfigurableJoint joint;
        }

        private Anchor pelvisAnchor;
        private Anchor chestAnchor;

        /// <summary>
        /// Creates the kinematic anchors the upright drives pull toward. Call once, with the body still
        /// in its rest pose (the joints take the current relative rotation as their zero).
        /// </summary>
        public void CreateUprightAnchors(Transform container, string unitName)
        {
            pelvisAnchor = CreateAnchor(container, unitName + " upright (pelvis)", rig.Part(PartId.Pelvis).body);
            chestAnchor = CreateAnchor(container, unitName + " upright (chest)", rig.Part(PartId.Chest).body);
        }

        public void DestroyUprightAnchors()
        {
            foreach (var anchor in new[] { pelvisAnchor, chestAnchor })
            {
                if (anchor != null && anchor.body != null)
                {
                    Object.Destroy(anchor.body.gameObject);
                }
            }
        }

        private static Anchor CreateAnchor(Transform container, string name, Rigidbody part)
        {
            var anchorObject = new GameObject(name);
            anchorObject.transform.SetParent(container, false);
            anchorObject.transform.SetPositionAndRotation(part.position, part.rotation);
            var body = anchorObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            var joint = anchorObject.AddComponent<ConfigurableJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;
            joint.axis = Vector3.right;
            joint.secondaryAxis = Vector3.up;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.targetRotation = Quaternion.identity;   // keep the rotation the two had when joined
            joint.enablePreprocessing = false;
            joint.connectedBody = part;
            return new Anchor { body = body, joint = joint };
        }

        /// <summary>
        /// Moves an anchor to the ghost's rotation and sets its drive: kp and kd per kg·m² of
        /// <paramref name="inertia"/> plus the part's own. Returns roughly the torque it pulls with (N·m).
        /// </summary>
        private float DriveUpright(Anchor anchor, Rigidbody part, Quaternion wanted, float balance, float inertia)
        {
            if (anchor == null || anchor.body == null)
            {
                return 0f;
            }

            anchor.body.MovePosition(part.position);
            anchor.body.MoveRotation(wanted);
            Vector3 own = part.inertiaTensor;
            float total = inertia + (own.x + own.y + own.z) / 3f;
            float strength = Mathf.Max(0f, balance) * total;
            anchor.joint.slerpDrive = new JointDrive
            {
                positionSpring = profile.kpUpright * strength,
                positionDamper = profile.kdUpright * strength,
                maximumForce = float.MaxValue,
                useAcceleration = false,
            };
            return profile.kpUpright * strength * JointMath.RotationError(part.rotation, wanted).magnitude;
        }

        /// <summary>"forward-left" etc., relative to the unit's facing.</summary>
        public static string DescribeDirection(Vector3 flat, Transform root)
        {
            if (flat.sqrMagnitude < 1e-6f)
            {
                return "nowhere";
            }

            float forward = Vector3.Dot(flat.normalized, root.forward);
            float right = Vector3.Dot(flat.normalized, root.right);
            string fb = forward > 0.38f ? "forward" : forward < -0.38f ? "back" : "";
            string lr = right > 0.38f ? "right" : right < -0.38f ? "left" : "";
            return fb.Length > 0 && lr.Length > 0 ? fb + "-" + lr : fb + lr;
        }
    }
}
