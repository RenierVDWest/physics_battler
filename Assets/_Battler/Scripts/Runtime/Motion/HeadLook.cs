using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>Something the head could look at: an id (the same thing keeps its id), where it is, how much it matters, and how long it has been there.</summary>
    public struct LookPoint
    {
        public int id;
        public Vector3 point;
        public float score;

        /// <summary>Seconds since it appeared: a new stimulus is answered only after the reaction latency.</summary>
        public float age;

        /// <summary>What it is, for the log ("opponent", "blow", "glance").</summary>
        public string kind;
    }

    /// <summary>
    /// Where the head looks (milestone 9). Each step an attention score picks one point: a new one
    /// must beat the current by a margin, and the current is held a moment so the head doesn't
    /// twitch (a much stronger one, like a blow, cuts in). Then the look turns toward it as a person's
    /// does: eased in and out, a big look in a third to half a second, never faster than a neck
    /// turns, and only as far as a head can (with the chest's help).
    /// </summary>
    public sealed class HeadLook
    {
        /// <summary>How far the head turns from the chest (degrees each way), how much more the chest adds, and how far it looks down and up.</summary>
        public const float Turn = 80f, ChestTurn = 30f, Down = 60f, Up = 50f;

        /// <summary>The shortest time a point is held (seconds), how much better a new one must score, and the fastest the look turns (degrees per second).</summary>
        public const float Hold = 0.4f, Margin = 0.15f, MaxSpeed = 350f;

        /// <summary>How quickly the look settles (rad/s, critically damped): a 90 degree look is 90% there in about 0.35 s.</summary>
        public const float Stiffness = 11f;

        /// <summary>Seconds before a new stimulus is answered (0.1 to 0.2: people differ).</summary>
        public float Latency = 0.15f;

        /// <summary>The look now: degrees right of the unit's facing, and up.</summary>
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        /// <summary>What it is looking at: the point's id (-1: nothing), where, its score and what kind of thing.</summary>
        public int Current { get; private set; } = -1;
        public Vector3 Point { get; private set; }
        public float Score { get; private set; }
        public string Kind { get; private set; } = "";

        /// <summary>Where the look is headed (degrees from the facing, and up), within the limits.</summary>
        public float TargetYaw { get; private set; }
        public float TargetPitch { get; private set; }

        /// <summary>Is what it looks at within a head's reach (with the chest's help) from where the unit faces?</summary>
        public bool Reachable { get; private set; } = true;

        private float chosenAt = float.NegativeInfinity;
        private float yawSpeed, pitchSpeed;

        public void Reset()
        {
            Yaw = Pitch = yawSpeed = pitchSpeed = 0f;
            Current = -1;
            Kind = "";
            chosenAt = float.NegativeInfinity;
        }

        /// <summary>Picks what to look at among <paramref name="points"/>.</summary>
        public void Choose(IList<LookPoint> points, float now)
        {
            int best = -1, current = -1;
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                if (p.id == Current)
                {
                    current = i;
                }

                if (p.age < Latency && p.id != Current)
                {
                    continue;   // (not noticed yet)
                }

                if (best < 0 || p.score > points[best].score)
                {
                    best = i;
                }
            }

            if (current < 0)
            {
                // What it looked at is gone: the best there is, at once.
                if (best >= 0)
                {
                    Take(points[best], now);
                }
                else
                {
                    Current = -1;
                    Kind = "";
                }

                return;
            }

            var held = points[current];
            Point = held.point;
            Score = held.score;
            if (best >= 0 && best != current)
            {
                var b = points[best];
                bool better = b.score > held.score + Margin;
                bool urgent = b.score >= 2f * held.score;
                if (better && (now - chosenAt >= Hold || urgent))
                {
                    Take(b, now);
                }
            }
        }

        private void Take(LookPoint p, float now)
        {
            Current = p.id;
            Point = p.point;
            Score = p.score;
            Kind = p.kind ?? "";
            chosenAt = now;
        }

        /// <summary>Turns the look toward the point chosen, seen from <paramref name="eye"/>, for a unit facing <paramref name="facingYaw"/>.</summary>
        public void Follow(Vector3 eye, float facingYaw, float dt)
        {
            if (Current >= 0)
            {
                Vector3 to = Point - eye;
                Vector3 flat = new Vector3(to.x, 0f, to.z);
                if (flat.sqrMagnitude > 1e-6f)
                {
                    TargetYaw = Mathf.DeltaAngle(facingYaw, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg);
                }

                TargetPitch = Mathf.Atan2(to.y, Mathf.Max(1e-3f, flat.magnitude)) * Mathf.Rad2Deg;
            }
            else
            {
                TargetYaw = 0f;
                TargetPitch = 0f;
            }

            Reachable = Mathf.Abs(TargetYaw) <= Turn + ChestTurn && TargetPitch >= -Down && TargetPitch <= Up;
            TargetYaw = Mathf.Clamp(TargetYaw, -(Turn + ChestTurn), Turn + ChestTurn);
            TargetPitch = Mathf.Clamp(TargetPitch, -Down, Up);
            if (dt <= 0f)
            {
                return;
            }

            float yaw = Yaw, pitch = Pitch;
            Ease(ref yaw, ref yawSpeed, TargetYaw, dt);
            Ease(ref pitch, ref pitchSpeed, TargetPitch, dt);
            Yaw = Mathf.Clamp(yaw, -(Turn + ChestTurn), Turn + ChestTurn);
            Pitch = Mathf.Clamp(pitch, -Down, Up);
        }

        /// <summary>A critically damped approach, never faster than a neck turns.</summary>
        private static void Ease(ref float x, ref float v, float target, float dt)
        {
            float w = Stiffness;
            v += (w * w * (target - x) - 2f * w * v) * dt;
            v = Mathf.Clamp(v, -MaxSpeed, MaxSpeed);
            x += v * dt;
        }

        /// <summary>
        /// How the look is shared out: the chest's turn (a fifth of the look, and whatever is past
        /// the neck's own reach, at most <see cref="ChestTurn"/>; nothing while a blow owns the chest).
        /// </summary>
        /// <param name="chestNow">How far the chest is already turned from the facing (a side-on stance's), degrees.</param>
        public static float ChestShare(float yaw, float chestNow, float blowBlend)
        {
            float need = Mathf.DeltaAngle(chestNow, yaw);
            float beyond = Mathf.Sign(need) * Mathf.Max(0f, Mathf.Abs(need) - Turn);
            return Mathf.Clamp(0.2f * yaw + beyond, -ChestTurn, ChestTurn) * Mathf.Clamp01(1f - blowBlend);
        }

        /// <summary>The head doesn't point dead on: the eyes take the last few degrees (a quarter of the look, at most 10 across and 8 up or down).</summary>
        public static float EyesAcross(float yaw) => Mathf.Clamp(0.25f * yaw, -10f, 10f);
        public static float EyesUpDown(float pitch) => Mathf.Clamp(0.2f * pitch, -8f, 8f);
    }
}
