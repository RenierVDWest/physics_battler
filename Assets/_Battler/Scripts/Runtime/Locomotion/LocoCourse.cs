using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The locomotion lab's course: a list of legs the body walks in turn, each saying where to go,
    /// how fast, and which way to face meanwhile (the way it goes, or a fixed way: side-steps and
    /// walking backward). It steers like the game will: a wanted velocity toward the next point,
    /// slowing to arrive where a leg stops, and a wanted facing. Board metres, the board's middle at 0.
    /// </summary>
    public sealed class LocoCourse
    {
        public enum Face
        {
            /// <summary>The way it goes.</summary>
            Travel,
            /// <summary>A fixed way (degrees).</summary>
            Fixed,
        }

        public sealed class Leg
        {
            public string Name;
            public Vector3 To;
            public float Speed;
            public Face Facing;
            public float Yaw;
            public bool Stop;
            public float Hold;          // a pause: seconds standing (no point)
            public Vector3 ArcCenter;   // an arc: round this, by Arc degrees (+ clockwise from above)
            public float Arc;
        }

        public readonly List<Leg> Legs = new List<Leg>();
        public int Index { get; private set; }
        public Leg Current => Index < Legs.Count ? Legs[Index] : null;
        public Vector3 Start;
        public float StartYaw;
        public bool Repeat = true;

        /// <summary>How fast it slows to arrive (m/s²), and how close to a point counts as there.</summary>
        public float Braking = 1.6f, There = 0.25f, PassWithin = 0.6f;

        private float held;
        private float arcDone;
        private Vector3 arcFrom;
        private bool arcStarted;

        /// <summary>A leg changed: (its index, its name).</summary>
        public event System.Action<int, string> LegStarted;

        public LocoCourse Hold(float seconds, string name = "stand")
        {
            Legs.Add(new Leg { Name = name, Hold = seconds });
            return this;
        }

        public LocoCourse To(float x, float z, float speed, string name, bool stop = true, Face face = Face.Travel, float yaw = 0f)
        {
            Legs.Add(new Leg { Name = name, To = new Vector3(x, 0f, z), Speed = speed, Stop = stop, Facing = face, Yaw = yaw });
            return this;
        }

        public LocoCourse Around(float cx, float cz, float degrees, float speed, string name, bool stop = false)
        {
            Legs.Add(new Leg { Name = name, ArcCenter = new Vector3(cx, 0f, cz), Arc = degrees, Speed = speed, Stop = stop });
            return this;
        }

        /// <summary>The lab's course, walked in a loop: everything the plan asks the walk to show.</summary>
        public static LocoCourse Lab()
        {
            var c = new LocoCourse { Start = new Vector3(-4.5f, 0f, -4.5f), StartYaw = 0f };
            c.Hold(1.5f)
                .To(-4.5f, 3.5f, 1.4f, "walk straight, then stop")
                .Hold(0.8f)
                .To(-2f, 3.5f, 1.0f, "side-step right", true, Face.Fixed, 0f)
                .Hold(0.5f)
                .To(-3.5f, 3.5f, 0.6f, "side-step left, slowly", true, Face.Fixed, 0f)
                .Hold(0.6f)
                .To(1f, 3.5f, 1.4f, "turn right and walk", false)
                .To(1f, 0.5f, 1.4f, "turn right", false)
                .To(4.5f, 0.5f, 1.4f, "turn left", false)
                .Around(4.5f, -1.5f, 180f, 1.3f, "curve right")
                .Around(4.5f, -4.5f, -180f, 1.3f, "curve left", true)
                .Hold(0.6f)
                .To(4.5f, -1.5f, 1.0f, "walk backward", true, Face.Fixed, 180f)
                .Hold(0.6f)
                .To(1.5f, -1.5f, 1.4f, "walk on", false)
                .To(4f, -1.5f, 1.4f, "turn back", true)
                .Hold(0.6f)
                .To(-4.5f, -1.5f, 3.6f, "run", false)
                .Around(-4.5f, -3.5f, -180f, 3.2f, "running curve", false)
                .To(-1f, -5.5f, 1.4f, "slow to a walk", false)
                .To(-4.5f, -4.5f, 1.2f, "back to the start", true)
                .Hold(0.5f);
            return c;
        }

        public void Reset()
        {
            Index = 0;
            held = 0f;
            arcStarted = false;
            LegStarted?.Invoke(0, Current?.Name);
        }

        private void Next()
        {
            Index++;
            held = 0f;
            arcStarted = false;
            if (Index >= Legs.Count && Repeat)
            {
                Index = 0;
            }

            LegStarted?.Invoke(Index, Current?.Name);
        }

        /// <summary>Where the current leg means to stop (world), if it stops: the walk plans its stopping step to end there.</summary>
        public Vector3? StopPoint
        {
            get
            {
                var leg = Current;
                if (leg == null || !leg.Stop || leg.Hold > 0f)
                {
                    return null;
                }

                if (leg.Arc != 0f)
                {
                    return arcStarted ? leg.ArcCenter + Quaternion.Euler(0f, leg.Arc, 0f) * arcFrom : (Vector3?)null;
                }

                return leg.To;
            }
        }

        /// <summary>What it wants this step: a velocity (world, m/s) and a facing (degrees).</summary>
        public void Steer(float dt, Vector3 position, float facing, Vector3 velocity, out Vector3 want, out float wantFacing)
        {
            want = Vector3.zero;
            wantFacing = facing;
            var leg = Current;
            if (leg == null)
            {
                return;
            }

            position.y = 0f;
            if (leg.Hold > 0f)
            {
                held += dt;
                if (held >= leg.Hold)
                {
                    Next();
                }

                return;
            }

            if (leg.Arc != 0f)
            {
                if (!arcStarted)
                {
                    arcStarted = true;
                    arcFrom = position - leg.ArcCenter;
                    arcDone = 0f;
                }

                float radius = arcFrom.magnitude;
                float sign = Mathf.Sign(leg.Arc);
                // (Along the circle at its speed, a little ahead of where it is; pulled back onto it.)
                Vector3 rel = position - leg.ArcCenter;
                float angleNow = Vector3.SignedAngle(arcFrom, rel, Vector3.up);
                arcDone = Mathf.Max(arcDone, angleNow * sign);
                float left = Mathf.Abs(leg.Arc) - arcDone;
                Vector3 tangent = Vector3.Cross(Vector3.up, rel.normalized) * sign;   // (clockwise from above, for +)
                Vector3 back = (radius - rel.magnitude) * rel.normalized;
                float speed = leg.Speed;
                if (leg.Stop)
                {
                    speed = Mathf.Min(speed, Mathf.Sqrt(2f * Braking * Mathf.Max(0f, left * Mathf.Deg2Rad * radius)));
                }

                want = tangent * speed + back * 2f;
                wantFacing = Mathf.Atan2(want.x, want.z) * Mathf.Rad2Deg;
                if (left <= 2f)
                {
                    Next();
                }

                return;
            }

            Vector3 to = leg.To - position;
            float distance = to.magnitude;
            float s = leg.Speed;
            if (leg.Stop)
            {
                s = Mathf.Min(s, Mathf.Sqrt(2f * Braking * Mathf.Max(0f, distance - 0.5f * There)));
            }

            want = distance > 1e-4f ? to / distance * s : Vector3.zero;
            wantFacing = leg.Facing == Face.Fixed ? leg.Yaw : (distance > 0.3f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : facing);
            bool there = leg.Stop ? distance < There && velocity.magnitude < 0.2f : distance < PassWithin;
            if (there)
            {
                Next();
            }
        }
    }
}
