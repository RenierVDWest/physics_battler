using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Where the ghost's feet are planted (world ankle positions) and any step in progress. Planted
    /// feet stay put in the world while the unit's root drifts; a step carries one foot to a new
    /// spot along a small arc.
    /// </summary>
    public sealed class Stance
    {
        private Vector3 footL;
        private Vector3 footR;
        private Vector3 stepFrom;
        private Vector3 stepTo;
        private float stepTime;
        private float stepDuration;
        private float stepHeight;

        public bool Stepping { get; private set; }
        public PartId SteppingFoot { get; private set; }
        public int StepsTaken { get; set; }

        /// <summary>Where the step in progress will land.</summary>
        public Vector3 StepTarget => stepTo;

        /// <summary>A step finished: the foot and where it was planted.</summary>
        public event System.Action<PartId, Vector3> Landed;

        /// <summary>Ankle positions relative to the unit root in the rest pose.</summary>
        public Vector3 RestLocalL { get; set; }
        public Vector3 RestLocalR { get; set; }

        /// <summary>How much each foot is turned out from the root's facing (degrees about up; + turns right).</summary>
        public float FootYawL { get; set; }
        public float FootYawR { get; set; }

        /// <summary>The planted rotation of a foot under a root facing <paramref name="rootRotation"/>.</summary>
        public Quaternion FootRotation(PartId foot, Quaternion rootRotation)
        {
            return rootRotation * Quaternion.Euler(0f, foot == PartId.FootL ? FootYawL : FootYawR, 0f);
        }

        /// <summary>Plants both feet in their rest spots under <paramref name="root"/>.</summary>
        public void Plant(Transform root, float groundY)
        {
            footL = OnGround(root.TransformPoint(RestLocalL), RestLocalL.y, groundY);
            footR = OnGround(root.TransformPoint(RestLocalR), RestLocalR.y, groundY);
            Stepping = false;
            StepsTaken = 0;
        }

        private static Vector3 OnGround(Vector3 point, float ankleHeight, float groundY)
        {
            point.y = groundY + ankleHeight;
            return point;
        }

        public Vector3 Planted(PartId foot) => foot == PartId.FootL ? footL : footR;

        /// <summary>The foot's current target, including the arc of a step in progress.</summary>
        public Vector3 Current(PartId foot)
        {
            if (!Stepping || foot != SteppingFoot)
            {
                return Planted(foot);
            }

            float t = Ease.Smooth(stepTime / stepDuration);
            Vector3 position = Vector3.Lerp(stepFrom, stepTo, t);
            position.y += Mathf.Sin(t * Mathf.PI) * stepHeight;
            return position;
        }

        public void BeginStep(PartId foot, Vector3 target, float duration, float height)
        {
            SteppingFoot = foot;
            stepFrom = Planted(foot);
            stepTo = target;
            stepTime = 0f;
            stepDuration = Mathf.Max(0.05f, duration);
            stepHeight = height;
            Stepping = true;
            StepsTaken++;
        }

        public void Update(float dt)
        {
            if (!Stepping)
            {
                return;
            }

            stepTime += dt;
            if (stepTime >= stepDuration)
            {
                if (SteppingFoot == PartId.FootL)
                {
                    footL = stepTo;
                }
                else
                {
                    footR = stepTo;
                }

                Stepping = false;
                Landed?.Invoke(SteppingFoot, stepTo);
            }
        }
    }
}
