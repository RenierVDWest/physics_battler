using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Branch locomotion-lab, the walk joined to the fight (the plan's "Joining the arms and combat"):
    /// while the unit is free to walk (no blow, not knocked back, standing), the new locomotion
    /// (<see cref="Locomotor"/>, <see cref="LocoBody"/>) animates the root the fight moves: the feet
    /// locked where they land, the pelvis, spine, chest and head as the reference walk has them. The
    /// pose pipeline blends its own lower body and torso into the walk's by <see cref="WalkWeight"/>, and
    /// the Fight Rhythm, the guard, the look and the arms go on top as before. A blow, a knock-back or
    /// a stumble hands the feet back to the stance where the walk left them; free again, the walk takes
    /// them over where the stance has them.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>The new walk in the fight (off: the old gait and stance). Off until the join is taken step by step in the lab.</summary>
        public static bool NewWalk = false;

        /// <summary>How fast the walk takes the body over, and gives it back (seconds for all of it).</summary>
        public const float WalkTakeOver = 0.25f, WalkHandBack = 0.12f;

        public Locomotor Walk { get; private set; }
        public LocoBody WalkBody { get; private set; }

        /// <summary>How much of the lower body and torso is the walk's (0: the stance's).</summary>
        public float WalkWeight { get; private set; }

        private bool walkOwnsFeet;

        /// <summary>The walk has the feet now (its locks say which are planted).</summary>
        public bool WalkOwnsFeet => walkOwnsFeet;

        /// <summary>The walk's step, instead of the old gait's (<see cref="UpdateGait"/>).</summary>
        private void UpdateWalk(float dt, Vector3 velocity, bool free)
        {
            Pose.GaitWeight = 0f;
            WalkBody.GroundY = Stance.GroundY;
            if (free && !walkOwnsFeet)
            {
                // Taking over: standing on the feet where the last pose had them (a stance's step half
                // done, a get-up's last pose: wherever they are).
                Walk.PlaceStanding(transform.position, transform.eulerAngles.y,
                    Anim.Part(PartId.FootL).position, Anim.Part(PartId.FootR).position);
                WalkBody.ReleaseFeet();
                walkOwnsFeet = true;
            }
            else if (!free && walkOwnsFeet)
            {
                // Handing back: the stance stands where the walk's feet are (a foot in the air is put
                // down under itself), and steps from there.
                for (int f = 0; f < 2; f++)
                {
                    Vector3 at = WalkBody.Feet[f].Target;
                    float yaw = WalkBody.FootRotation(f).eulerAngles.y;
                    Stance.SetFoot(f, new Vector3(at.x, Stance.GroundY, at.z), yaw);
                }

                walkOwnsFeet = false;
                if (CurrentMove == null && !postureHolds)
                {
                    Stance.Hold = false;
                }
            }

            WalkWeight = Mathf.MoveTowards(WalkWeight, free ? 1f : 0f, dt / (free ? WalkTakeOver : WalkHandBack));
            if (!walkOwnsFeet)
            {
                Pose.WalkWeight = WalkWeight;
                if (WalkWeight <= 0f)
                {
                    Pose.Walk = null;
                }

                return;
            }

            Walk.Follow(dt, transform.position, transform.eulerAngles.y, velocity);
            WalkBody.UpdateFeet(dt);

            // The stance keeps the walk's feet (on the ground under them), so anything that asks where
            // the feet are (balance, the planner, the logs) is told, and a hand-back starts from them.
            Stance.Hold = true;
            for (int f = 0; f < 2; f++)
            {
                Vector3 at = WalkBody.Feet[f].Target;
                Stance.SetFoot(f, new Vector3(at.x, Stance.GroundY, at.z), WalkBody.FootRotation(f).eulerAngles.y);
            }

            Pose.Walk = WalkBody;
            Pose.WalkWeight = WalkWeight;
        }
    }
}
