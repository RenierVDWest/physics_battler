using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The duel lab's movement orders (the plan "Fight Rhythm from Recordings", step 1): one fighter moved about
    /// another on demand, so a way of moving can be looked at by itself. Walk to it, back away from it, go round it
    /// to the left or the right, stand. The fighter is steered as the fight steers one (a velocity and a facing, to
    /// its walk), so what is seen is the walk the fight will have: planted feet, real steps.
    /// </summary>
    public sealed class DuelLab
    {
        public enum Order
        {
            Stand,
            WalkTo,
            BackAway,
            CircleLeft,
            CircleRight,
        }

        /// <summary>The order it is carrying out.</summary>
        public Order Now { get; private set; }

        /// <summary>How near it walks to the other, and how far it backs away (metres, root to root).</summary>
        public float Near = 1.5f, Far = 4f;

        /// <summary>The fighter moved, and the one it moves about.</summary>
        public UnitController Mover { get; private set; }
        public UnitController Other { get; private set; }

        /// <summary>How far apart the two are now (metres, root to root), how fast the mover is going over the ground (m/s) and how far it has gone since its order.</summary>
        public float Gap { get; private set; }
        public float Speed { get; private set; }
        public float Gone { get; private set; }

        private float radius;
        private Vector3 last;

        public bool Running => Mover != null && Other != null && Mover.IsInitialized && Other.IsInitialized;

        public void Begin(UnitController mover, UnitController other)
        {
            Mover = mover;
            Other = other;
            Now = Order.Stand;
            Gone = 0f;
            if (mover != null)
            {
                last = mover.Root.position;
            }
        }

        public void Stop()
        {
            if (Mover != null)
            {
                Mover.DummyGo = null;
            }

            Mover = Other = null;
            Now = Order.Stand;
        }

        /// <summary>Gives the mover an order. Going round, it keeps the distance it has now.</summary>
        public void Give(Order order)
        {
            if (!Running)
            {
                return;
            }

            Now = order;
            Gone = 0f;
            radius = Flat(Other.Root.position - Mover.Root.position).magnitude;
            if (order == Order.Stand)
            {
                Stand();
            }
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private void Stand()
        {
            // (Where it is now is its spot: it does not walk back to where it began.)
            Now = Order.Stand;
            Mover.DummyGo = null;
            Mover.DummySpot = Mover.Root.position;
        }

        /// <summary>One step of the lab (the physics step): the order turned into the velocity the mover is steered by.</summary>
        public void Step(float dt)
        {
            if (!Running)
            {
                return;
            }

            Vector3 to = Flat(Other.Root.position - Mover.Root.position);
            Gap = to.magnitude;
            Vector3 moved = Flat(Mover.Root.position - last);
            last = Mover.Root.position;
            Speed = Mathf.Lerp(Speed, moved.magnitude / Mathf.Max(1e-4f, dt), Mathf.Clamp01(dt / 0.15f));
            Gone += Now == Order.Stand ? 0f : moved.magnitude;
            if (Now == Order.Stand || Gap < 1e-3f)
            {
                return;
            }

            Vector3 toward = to / Gap, left = Vector3.Cross(toward, Vector3.up);
            float pace = Mover.WalkSpeed;
            switch (Now)
            {
                case Order.WalkTo:
                    if (Gap <= Near)
                    {
                        Stand();
                        return;
                    }

                    // (Slowing over the last of it, as the fight's walk in does.)
                    Mover.DummyGo = toward * Mathf.Min(pace, 2f * (Gap - Near) + 0.2f);
                    break;
                case Order.BackAway:
                    if (Gap >= Far)
                    {
                        Stand();
                        return;
                    }

                    Mover.DummyGo = -toward * Mathf.Min(pace, 2f * (Far - Gap) + 0.2f);
                    break;
                default:
                    // (Round the other at the distance it had, a little in or out as it drifts.)
                    Mover.DummyGo = (Now == Order.CircleLeft ? left : -left) * pace + toward * Mathf.Clamp(1.5f * (Gap - radius), -0.5f * pace, 0.5f * pace);
                    break;
            }
        }
    }
}
