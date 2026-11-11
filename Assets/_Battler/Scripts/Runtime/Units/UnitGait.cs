using PhysicsBattler.Motion;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// The unit's walk (<see cref="GaitPlayer"/>): while the game walks its root, the feet step to the
    /// style's captured walk cycle (its cadence, lift and width; each foot lands half a stance ahead
    /// of where the hips will be) and the body moves as the capture's does. Standing still, the stance
    /// takes the feet back.
    /// </summary>
    public partial class UnitController
    {
        public GaitPlayer Gait { get; private set; }

        /// <summary>
        /// How fast it walks across the board, m/s: the style's walking speed, by the square root of
        /// its size (as animals of different sizes walk: a small one isn't left behind in proportion).
        /// </summary>
        public float WalkSpeed => Style.walkSpeed * Mathf.Sqrt(Mathf.Max(0.1f, Body.SizeFactor));

        private bool gaitHolds;
        private float strayWait;

        // What the Fight Rhythm moved the root by itself (travel, circling) since the walk last
        // measured: not walking.
        private Vector3 rhythmMoved;

        /// <summary>The walk's step: after the root has moved (walked, knocked back), before the rhythm and the pose.</summary>
        private void UpdateGait(float dt)
        {
            if (Gait == null)
            {
                return;
            }

            Vector3 velocity = WalkVelocity - (dt > 0f ? rhythmMoved / dt : Vector3.zero);
            rhythmMoved = Vector3.zero;
            bool free = IsAlive && State == UnitState.Standing && CurrentMove == null && cutShort == null && HitStop <= 0f
                        && Knockback.magnitude < 0.05f * SizeSpeed();
            if (Walk != null)
            {
                // (Branch locomotion-lab: the new walk has the feet and the body while it walks, and
                // animates all the root's travel, the Fight Rhythm's too: its feet are planted.)
                UpdateWalk(dt, WalkVelocity, free);
                return;
            }

            Gait.Update(dt, velocity, free, WalkSpeed, Body.LegLength);

            if (free && Gait.Weight >= 0.3f && Gait.Walking)
            {
                gaitHolds = true;
                Stance.Hold = true;   // only the walk's steps
                foreach (int foot in Gait.DueSteps)
                {
                    TakeGaitStep(foot);
                }

                // A foot left too far behind, or going somewhere the walk no longer goes (it turned
                // back, or sharply), or crossed over: it steps (again) now, from where it is, rather
                // than wait for its turn round the cycle. The worse one first (a planted one before
                // one already on its way), and not more often than every tenth of a second.
                strayWait -= dt;
                if (strayWait <= 0f)
                {
                    int worst = -1;
                    float worstScore = 0f;
                    for (int f = 0; f < 2; f++)
                    {
                        // (Against where the hips will be when it lands; a low walk reaches only
                        // about half a leg out from under them.)
                        Vector3 rootThen = transform.position + Gait.Velocity * Stance.StepTimeLeft(f);
                        Vector3 at = Quaternion.Inverse(transform.rotation) * (Stance.Target(f) - rootThen) / Body.LegLength;
                        at.y = 0f;
                        float side = f == Stance.Left ? -1f : 1f;
                        float otherX = transform.InverseTransformPoint(Stance.Target(1 - f)).x / Body.LegLength;
                        float ownX = transform.InverseTransformPoint(Stance.Target(f)).x / Body.LegLength;
                        bool crossed = side * (ownX - otherX) < -0.02f;
                        float score = at.magnitude / (Stance.Stepping(f) ? 0.55f : 0.5f);
                        if (crossed)
                        {
                            score = Mathf.Max(score, 1.01f);
                        }

                        if (!Stance.Stepping(f))
                        {
                            score *= 1.2f;
                        }

                        if (score > 1f && score > worstScore)
                        {
                            worstScore = score;
                            worst = f;
                        }
                    }

                    if (worst >= 0)
                    {
                        TakeGaitStep(worst, 0.3f);
                        strayWait = 0.1f;
                    }
                }
            }
            else if (gaitHolds)
            {
                gaitHolds = false;
                if (CurrentMove == null && !postureHolds)
                {
                    Stance.Hold = false;   // the stance takes the feet home
                }
            }

            Pose.GaitWeight = Gait.Weight;
        }

        /// <param name="share">The share of the cycle it takes (0: the capture's own swing).</param>
        private void TakeGaitStep(int foot, float share = 0f)
        {
            var clip = Gait.Clip;
            Vector3 velocity = Gait.Velocity;
            float swing = (share > 0f ? share : clip.Swing(foot)) * Gait.CycleTime;
            float stance = Mathf.Max(0f, Gait.CycleTime - swing);
            // Where the hips will be when it lands, and half the time it then stands ahead of that.
            Vector3 hips = transform.position + velocity * swing;
            float side = foot == Stance.Left ? -1f : 1f;
            // (At most a third of a leg ahead: a slow cadence's long stance would otherwise put the
            // foot out where the leg can't follow it through.)
            float speed = velocity.magnitude;
            float ahead = speed > 1e-3f ? Mathf.Min(0.5f * stance * speed, 0.33f * Body.LegLength) / speed : 0f;
            Vector3 to = hips + transform.right * (side * 0.5f * clip.width * Body.LegLength) + velocity * ahead;
            // Never across the other foot (walking sideways: the trailing foot closes up, the
            // leading one steps out).
            Vector3 local = transform.InverseTransformPoint(to);
            float other = transform.InverseTransformPoint(Stance.Target(1 - foot)).x;
            float gap = Mathf.Max(0.08f, 0.6f * clip.width) * Body.LegLength;
            local.x = side < 0f ? Mathf.Min(local.x, other - gap) : Mathf.Max(local.x, other + gap);
            to = transform.TransformPoint(local);
            float yaw = facingYaw + side * 8f;
            Stance.StepTo(foot, to, yaw, swing, clip.stepHeight * Body.LegLength);
        }

        /// <summary>Lets go of the feet (a stumble's catch steps have them); the walk's body fades out by itself.</summary>
        private void ReleaseGait()
        {
            if (gaitHolds && CurrentMove == null && !postureHolds)
            {
                Stance.Hold = false;
            }

            gaitHolds = false;
        }

        /// <summary>Stops the walk at once (knocked down, moved by the sandbox).</summary>
        private void StopGait()
        {
            Gait?.Stop();
            if (gaitHolds && CurrentMove == null && !postureHolds)
            {
                Stance.Hold = false;
            }

            gaitHolds = false;
            rhythmMoved = Vector3.zero;
            Pose.GaitWeight = 0f;
        }
    }
}
