using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A clip fighter's footwork against one opponent (the plan "Bringing the lab into the fight", step 3): the fight
    /// loop's own movement choices (<c>FightLoop.Approach</c>, the Fight Rhythm's circling), given as the velocity and
    /// facing its walk is steered by, so the guard's clips do the stepping. It walks in to where its strikes land,
    /// slowing over the last of it; holds that distance; backs off only when it is crowded or holding back; once near,
    /// goes round the other sideways, the two going round the same way; and always faces it. Its tempo moves the
    /// distance as the fight's does: gathering, it holds just out of the other's reach; pressing, it closes in;
    /// shelled up, it gives ground.
    /// </summary>
    public sealed class Footwork
    {
        /// <summary>The fight's tempo states, as they move the distance (the fight's <c>TempoState</c>; the counter is a probe here).</summary>
        public enum Tempo
        {
            Probe,
            Gather,
            Press,
            Shell,
        }

        public Tempo Mode = Tempo.Probe;

        /// <summary>Its walking pace in the guard, and its circling pace (m/s at a Medium's size).</summary>
        public float WalkSpeed = 1.1f, CircleSpeed = 0.35f;

        /// <summary>It goes round in spells this long (seconds, least and most), and stands between them this long: a fighter circles, settles, and circles again, now one way, now the other.</summary>
        public Vector2 CircleFor = new Vector2(1.5f, 3.5f), StandFor = new Vector2(0.8f, 2.2f);

        /// <summary>Which way it goes round now (+1 to its right, -1 to its left, 0 not circling).</summary>
        public float Circling { get; private set; }

        /// <summary>What it is doing, for the logs ("walking in", "holding", "circling left", "backing off").</summary>
        public string Doing { get; private set; } = "";

        /// <summary>The distance it wants now, and how far off it is (metres, root to root).</summary>
        public float Want { get; private set; }

        public float Off { get; private set; }

        private readonly ClipFighter self;
        private readonly int order;
        private readonly System.Random luck;
        private float side = 1f, sideFor;
        private bool standing;

        /// <param name="order">Which of two goes first: when both circle, the later follows the earlier's way round.</param>
        public Footwork(ClipFighter self, int order)
        {
            this.self = self;
            this.order = order;
            luck = new System.Random(1234 + 77 * order);
            side = order % 2 == 0 ? 1f : -1f;
            sideFor = Next(CircleFor);
        }

        private float Next(Vector2 range) => Mathf.Lerp(range.x, range.y, (float)luck.NextDouble());

        /// <summary>
        /// Where a body stands to strike (metres, root to root): the middle of its strikes' reaches (each strike's sweet
        /// spot at contact, flat, from where the strike starts), never inside the fighting distance.
        /// </summary>
        public static float StrikingDistance(ClipFighter f, ClipFighter target)
        {
            var reaches = new System.Collections.Generic.List<float>();
            foreach (var clip in f.StrikeClips)
            {
                if (StrikeReach.TryGet(f.Poser.BodyKey, clip.Name, out var grip, out var way))
                {
                    Vector3 sweet = grip + way * (f.Poser.WeaponLength * f.Poser.SweetSpot);
                    reaches.Add(new Vector2(sweet.x, sweet.z).magnitude);
                }
            }

            float reach = 1.4f * f.Size;
            if (reaches.Count > 0)
            {
                reaches.Sort();
                reach = reaches[reaches.Count / 2];
            }

            return Mathf.Max(reach, ClipFighter.FightingDistance(f.Size, target.Size) + 0.1f * f.Size);
        }

        /// <summary>
        /// One step's steering toward <paramref name="target"/> (its footwork, <paramref name="theirs"/>, if it has one,
        /// so two circle the same way round): the walk's velocity (m/s, world) and the facing (degrees).
        /// </summary>
        public void Steer(ClipFighter target, Footwork theirs, float dt, out Vector3 want, out float yaw)
        {
            want = Vector3.zero;
            yaw = self.Loco.Facing;
            Circling = 0f;
            if (target == null)
            {
                Doing = "no one to face";
                return;
            }

            Vector3 to = target.Loco.Position - self.Loco.Position;
            to.y = 0f;
            float distance = to.magnitude;
            Vector3 toward = distance > 1e-4f ? to / distance : Quaternion.Euler(0f, self.Loco.Facing, 0f) * Vector3.forward;
            float size = self.Size;
            if (self.Busy)
            {
                // (A strike, a reaction: the clip moves the body; its steering waits.)
                Doing = "busy";
                return;
            }

            yaw = Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg;
            float closest = ClipFighter.FightingDistance(self.Size, target.Size);
            float engage = StrikingDistance(self, target);
            float theirReach = StrikingDistance(target, self);
            switch (Mode)
            {
                case Tempo.Gather:
                    engage = Mathf.Max(engage, theirReach) + 0.35f * size;
                    break;
                case Tempo.Shell:
                    engage += 0.2f * size;
                    break;
                case Tempo.Press:
                    engage = Mathf.Max(closest + 0.05f * size, engage - 0.1f * size);
                    break;
            }

            Want = engage;
            float off = distance - engage;
            Off = off;
            float speed = WalkSpeed * Mathf.Sqrt(size);
            bool holdingBack = Mode == Tempo.Gather || Mode == Tempo.Shell;
            if (off > 0.05f * size)
            {
                want = toward * speed * Mathf.Clamp01(off / (0.4f * size));
                Doing = "walking in";
            }
            else if (off < -0.05f * size && (off < -0.4f * size || holdingBack || distance < closest))
            {
                want = -toward * 0.6f * speed * Mathf.Clamp01(-off / (0.3f * size));
                Doing = "backing off";
            }
            else
            {
                Doing = "holding";
            }

            // Near: round the other, sideways. Two squaring up go round the same way (each to its own right with the
            // same sign orbits; opposite signs would slide off together): the one made first leads.
            if (off <= 0.3f * size)
            {
                sideFor -= dt;
                if (sideFor <= 0f)
                {
                    // (A spell of circling over, it stands a moment; a stand over, it goes round again, either way.)
                    standing = !standing;
                    if (!standing)
                    {
                        side = luck.NextDouble() < 0.5 ? -side : side;
                    }

                    sideFor = Next(standing ? StandFor : CircleFor);
                }

                float circle = standing ? 0f : side;
                if (theirs != null && theirs.order < order && Mathf.Abs(theirs.Circling) > 1e-4f)
                {
                    // (The other circling: it goes round the same way, as the one who leads does.)
                    circle = Mathf.Sign(theirs.Circling);
                    side = circle;
                }

                Circling = circle;
                if (Mathf.Abs(circle) > 0f)
                {
                    Vector3 right = Vector3.Cross(Vector3.up, toward);
                    want += right * (circle * CircleSpeed * Mathf.Sqrt(size));
                    Doing = (Doing == "holding" ? "" : Doing + ", ") + (circle > 0f ? "circling right" : "circling left");
                }
            }
        }
    }
}
