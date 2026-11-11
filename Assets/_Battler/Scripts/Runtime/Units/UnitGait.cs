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
        // What the Fight Rhythm moved the root by itself (travel, circling) since the walk last
        // measured: not walking.
        private Vector3 rhythmMoved;

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
