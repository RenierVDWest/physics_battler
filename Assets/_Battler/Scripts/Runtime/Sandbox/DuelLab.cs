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
            LeapIn,
            LeapOut,
            HopLeft,
            HopRight,
        }

        /// <summary>How far a leap goes (metres at a Medium's size) and how long it takes (seconds).</summary>
        public float LeapFar = 0.5f, LeapTime = 0.25f;

        /// <summary>How the last leap went: asked, and whether both feet left the ground together (a hop) or stepped after the body.</summary>
        public string LastLeap = "none yet";

        /// <summary>The order it is carrying out.</summary>
        public Order Now { get; private set; }

        /// <summary>How near it walks to the other, and how far it backs away (metres, root to root).</summary>
        public float Near = 1.5f, Far = 4f;

        /// <summary>
        /// The pace he is asked to go at (m/s at a Medium's size; 0: his guard's own walking pace, as the fight asked
        /// until now). Under a walk's speed he stands and steps; at a walk's he walks; above it his guard's run
        /// carries him, its stride shortened to the pace.
        /// </summary>
        public float Pace;

        /// <summary>The pace he goes at now (m/s): the one asked, or his walk's own.</summary>
        public float PaceNow => Mover == null ? 0f : Pace > 0.01f ? Pace * Mathf.Sqrt(Mathf.Max(0.1f, Mover.Body.SizeFactor)) : Mover.WalkSpeed;

        /// <summary>The fastest he went under the order he has (m/s, over the ground).</summary>
        public float Fastest { get; private set; }

        /// <summary>
        /// Setting off: over the first 0.35 s of the order he has, the fastest either foot went and the fastest his
        /// hips went (m/s, the body as seen), and how far he had got by then (metres). Feet whipped to their places
        /// while the body has hardly moved read as a jump.
        /// </summary>
        public float SetOffFoot { get; private set; }
        public float SetOffHips { get; private set; }
        public float SetOffWent { get; private set; }

        /// <summary>Set (the headless run): each step of a setting off is written to it.</summary>
        public System.Collections.Generic.List<string> Trace;

        private bool leaping;
        private float since;
        private Vector3 lastFootL, lastFootR, lastHips;

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
            Fastest = 0f;
            since = 0f;
            SetOffFoot = SetOffHips = SetOffWent = 0f;
            lastFootL = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootL);
            lastFootR = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootR);
            lastHips = Mover.PartPosition(PhysicsBattler.Rig.PartId.Pelvis);
            radius = Flat(Other.Root.position - Mover.Root.position).magnitude;
            if (order == Order.Stand)
            {
                Stand();
            }
            else if (order >= Order.LeapIn)
            {
                // (A leap is done at once, from where he stands; then he stands where it takes him.)
                Vector3 toward = Flat(Other.Root.position - Mover.Root.position);
                toward = toward.sqrMagnitude > 1e-6f ? toward.normalized : Mover.transform.forward;
                Vector3 left = Vector3.Cross(toward, Vector3.up);
                Vector3 way = order == Order.LeapIn ? toward : order == Order.LeapOut ? -toward : order == Order.HopLeft ? left : -left;
                Mover.DummyGo = null;
                bool made = PhysicsBattler.Locomotion.LeapMaker.On;
                bool leapt = Mover.Clip != null && (made ? Mover.Clip.LeapMade(way, LeapFar, LeapTime) : Mover.Clip.LeapNow(way, LeapFar, LeapTime));
                Mover.DummySpot = Mover.Root.position + (leapt ? way * (LeapFar * Mover.Body.SizeFactor) : Vector3.zero);
                LastLeap = !leapt ? $"{order}: he could not (in a blow, or down)" : $"{order}: {LeapFar:0.00} m, {LeapTime:0.00} s in the air, " + (made ? "made of the recorded jump" : Mover.Clip.LeapHopped ? "the stance's hop: both feet off the ground together" : "stepped after the body (he was not standing in his stance)");
                leaping = true;
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
            Fastest = Mathf.Max(Fastest, Speed);
            if (Now != Order.Stand && since < 0.35f && dt > 1e-5f)
            {
                since += dt;
                Vector3 l = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootL), r = Mover.PartPosition(PhysicsBattler.Rig.PartId.FootR), h = Mover.PartPosition(PhysicsBattler.Rig.PartId.Pelvis);
                SetOffFoot = Mathf.Max(SetOffFoot, Mathf.Max((l - lastFootL).magnitude, (r - lastFootR).magnitude) / dt);
                SetOffHips = Mathf.Max(SetOffHips, (h - lastHips).magnitude / dt);
                SetOffWent = Gone;
                if (Trace != null)
                {
                    var loco = Mover.Clip.Loco;
                    Trace.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00} s: left foot {1:0.0} m/s, right {2:0.0}, hips {3:0.0}; going {4:0.00} m/s; stride phase {5:0.00} at {6:0.00} a second, stride x{7:0.00}, {8}{9}",
                        since, (l - lastFootL).magnitude / dt, (r - lastFootR).magnitude / dt, (h - lastHips).magnitude / dt, Speed, loco.Phase, loco.PhaseRate, loco.StrideScale, loco.Current, loco.Running ? " running" : ""));
                }

                lastFootL = l;
                lastFootR = r;
                lastHips = h;
            }
            if (leaping)
            {
                // (Until the leap's ground is covered; then he stands.)
                if (Mover.Clip == null || (!Mover.Clip.GivingGround && Speed < 0.15f && Mover.Clip.Loco.Current != PhysicsBattler.Locomotion.Locomotor.Mode.OneShot))
                {
                    // (Where the leap took him is his spot: he does not creep on to where it was meant to.)
                    leaping = false;
                    Stand();
                }

                return;
            }

            if (Now == Order.Stand || Gap < 1e-3f)
            {
                return;
            }

            Vector3 toward = to / Gap, left = Vector3.Cross(toward, Vector3.up);
            float pace = PaceNow;
            // (How soon he starts to slow for where he is to stop: sooner the faster he comes, as far as his walk coasts.)
            float slows = Mathf.Max(0.5f, 2f / Mathf.Max(1f, pace));
            switch (Now)
            {
                case Order.WalkTo:
                    if (Gap <= Near)
                    {
                        Stand();
                        return;
                    }

                    // (Slowing over the last of it, as the fight's walk in does.)
                    Mover.DummyGo = toward * Mathf.Min(pace, slows * 2f * (Gap - Near) + 0.2f);
                    break;
                case Order.BackAway:
                    if (Gap >= Far)
                    {
                        Stand();
                        return;
                    }

                    Mover.DummyGo = -toward * Mathf.Min(pace, slows * 2f * (Far - Gap) + 0.2f);
                    break;
                default:
                    // (Round the other at the distance it had. Going straight along the circle's edge it would leave the
                    // circle, the faster the further: it is steered inward by as much as the circle bends over the time
                    // its walk takes to answer, and in or out as it drifts.)
                    Vector3 round = (Now == Order.CircleLeft ? left : -left) * pace
                        + toward * Mathf.Clamp(pace * pace / Mathf.Max(0.5f, radius) * 0.45f + 2.5f * (Gap - radius), -0.6f * pace, pace);
                    Mover.DummyGo = round.normalized * pace;
                    break;
            }
        }
    }
}
