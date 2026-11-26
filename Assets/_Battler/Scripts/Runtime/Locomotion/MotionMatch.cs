using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The footage a matched walk is chosen from (the plan "Walking that feels like a real person", step 2): every
    /// moment of a set of recordings, each written down as a short list of numbers, so that the moment that best fits
    /// "the pose he is in now, and where he should be in the next second" can be searched for.
    ///
    /// What is written down for a moment, all in the root's own frame there (x right, y up, z forward; leg lengths):
    /// where each foot is, how fast each foot and the hips are going, and where the root will be and which way it
    /// will face a third of a second, two thirds and a whole second on.
    /// </summary>
    public sealed class MatchFootage
    {
        /// <summary>How many numbers describe a moment: 15 of his pose, 12 of where he goes.</summary>
        public const int Size = 27, PoseSize = 15;

        /// <summary>The moments are taken this often (a second).</summary>
        public const float Rate = 30f;

        /// <summary>How far ahead a moment's path is written down (seconds).</summary>
        public static readonly float[] Ahead = { 1f / 3f, 2f / 3f, 1f };

        /// <summary>What each kind of number counts for in a comparison: the feet's places, the feet's and hips' speeds, the path, the facing.</summary>
        public static float FeetWeigh = 0.75f, SpeedsWeigh = 1f, PathWeighs = 1f, FacingWeighs = 1.25f;

        public readonly LocoClip[] Clips;
        public readonly int Count;
        private readonly int[] clipOf;
        private readonly float[] timeOf;
        private readonly bool[] enter;
        private readonly float[] numbers;   // Count * Size, each number less its mean, over its spread, times its weight
        private readonly float[] mean = new float[Size], spread = new float[Size], weigh = new float[Size];
        private readonly Dictionary<LocoClip, int> firstOf = new Dictionary<LocoClip, int>();
        private readonly Dictionary<LocoClip, int> countOf = new Dictionary<LocoClip, int>();

        public MatchFootage(IEnumerable<LocoClip> clips)
        {
            var list = new List<LocoClip>();
            foreach (var c in clips)
            {
                if (c != null && c.Frames > 2)
                {
                    list.Add(c);
                }
            }

            Clips = list.ToArray();
            var cl = new List<int>();
            var tm = new List<float>();
            var en = new List<bool>();
            for (int k = 0; k < Clips.Length; k++)
            {
                var c = Clips[k];
                int n = c.Loop ? Mathf.Max(1, Mathf.RoundToInt(c.Duration * Rate)) : Mathf.FloorToInt(c.Duration * Rate + 0.001f) + 1;
                firstOf[c] = cl.Count;
                countOf[c] = n;
                for (int i = 0; i < n; i++)
                {
                    float t = Mathf.Min(i / Rate, c.Duration);
                    cl.Add(k);
                    tm.Add(t);
                    // (A recording that ends is not gone into in its last moments: there is nothing left of it to play.)
                    en.Add(c.Loop || t <= c.Duration - EnterBefore);
                }
            }

            Count = cl.Count;
            clipOf = cl.ToArray();
            timeOf = tm.ToArray();
            enter = en.ToArray();
            numbers = new float[Count * Size];
            var raw = new float[Size];
            var sum = new double[Size];
            var sq = new double[Size];
            for (int i = 0; i < Count; i++)
            {
                Raw(Clips[clipOf[i]], timeOf[i], raw);
                for (int f = 0; f < Size; f++)
                {
                    numbers[i * Size + f] = raw[f];
                    sum[f] += raw[f];
                    sq[f] += raw[f] * raw[f];
                }
            }

            for (int f = 0; f < Size; f++)
            {
                mean[f] = (float)(sum[f] / Mathf.Max(1, Count));
                spread[f] = Mathf.Max(0.01f, Mathf.Sqrt(Mathf.Max(0f, (float)(sq[f] / Mathf.Max(1, Count)) - mean[f] * mean[f])));
                weigh[f] = f < 6 ? FeetWeigh : f < PoseSize ? SpeedsWeigh : (f - PoseSize) % 4 < 2 ? PathWeighs : FacingWeighs;
            }

            // (The kinds of number are spread alike: each kind's numbers share one spread, so that a foot's way up
            // and down, which varies little, does not count as much as its way forward.)
            Share(0, 6);
            Share(6, PoseSize);
            for (int a = 0; a < Ahead.Length; a++)
            {
                Share(PoseSize + 4 * a, PoseSize + 4 * a + 2);
                Share(PoseSize + 4 * a + 2, PoseSize + 4 * a + 4);
            }

            for (int i = 0; i < Count; i++)
            {
                for (int f = 0; f < Size; f++)
                {
                    numbers[i * Size + f] = (numbers[i * Size + f] - mean[f]) / spread[f] * weigh[f];
                }
            }
        }

        /// <summary>A recording that ends is not gone into within this long of its end (seconds).</summary>
        public const float EnterBefore = 0.3f;

        private void Share(int from, int to)
        {
            float s = 0f;
            for (int f = from; f < to; f++)
            {
                s += spread[f] * spread[f];
            }

            s = Mathf.Sqrt(s / (to - from));
            for (int f = from; f < to; f++)
            {
                spread[f] = s;
            }
        }

        /// <summary>A place of a later moment's frame, as it lies in an earlier moment's frame, given how the root moved and turned between them.</summary>
        private static Vector3 Into(Vector3 p, Vector2 moved, float turned)
        {
            float r = turned * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector3(moved.x + p.x * c + p.z * s, p.y, moved.y - p.x * s + p.z * c);
        }

        /// <summary>How the root goes from a time to a later one; past a recording's end it goes on as it was going over its last fifth of a second.</summary>
        public static void Travel(LocoClip c, float from, float to, out Vector2 moved, out float turned)
        {
            if (c.Loop || to <= c.Duration)
            {
                c.RootMotion(from, to, out moved, out turned);
                return;
            }

            c.RootMotion(Mathf.Min(from, c.Duration), c.Duration, out moved, out turned);
            float back = Mathf.Min(0.2f, c.Duration);
            c.RootMotion(c.Duration - back, c.Duration, out var last, out float lastTurn);
            float over = to - Mathf.Max(from, c.Duration);
            // (Its last going, turned into the frame of "from".)
            float r = turned * Mathf.Deg2Rad, cs = Mathf.Cos(r), sn = Mathf.Sin(r);
            Vector2 on = last / back * over;
            moved += new Vector2(on.x * cs + on.y * sn, -on.x * sn + on.y * cs);
            turned += lastTurn / back * over;
        }

        /// <summary>A moment's numbers as they are (leg lengths, and leg lengths a second).</summary>
        public static void Raw(LocoClip c, float t, float[] into)
        {
            const float h = 1f / Rate;
            float t1 = c.Loop ? t + h : Mathf.Min(t + h, c.Duration);
            float t0 = c.Loop || t1 > t + 0.5f * h ? t : Mathf.Max(0f, t - h);
            if (t1 <= t0)
            {
                t1 = t0 + h;
            }

            c.RootMotion(t0, t1, out var moved, out float turned);
            float dt = t1 - t0;
            Vector3 fl0 = c.Point(LocoBone.LeftFoot, t0), fr0 = c.Point(LocoBone.RightFoot, t0), hp0 = c.Point(LocoBone.Hips, t0);
            Vector3 fl1 = Into(c.Point(LocoBone.LeftFoot, t1), moved, turned), fr1 = Into(c.Point(LocoBone.RightFoot, t1), moved, turned), hp1 = Into(c.Point(LocoBone.Hips, t1), moved, turned);
            Vector3 fl = c.Point(LocoBone.LeftFoot, t), fr = c.Point(LocoBone.RightFoot, t);
            Vector3 vl = (fl1 - fl0) / dt, vr = (fr1 - fr0) / dt, vh = (hp1 - hp0) / dt;
            into[0] = fl.x; into[1] = fl.y; into[2] = fl.z;
            into[3] = fr.x; into[4] = fr.y; into[5] = fr.z;
            into[6] = vl.x; into[7] = vl.y; into[8] = vl.z;
            into[9] = vr.x; into[10] = vr.y; into[11] = vr.z;
            into[12] = vh.x; into[13] = vh.y; into[14] = vh.z;
            for (int a = 0; a < Ahead.Length; a++)
            {
                Travel(c, t, t + Ahead[a], out var m, out float tu);
                float r = tu * Mathf.Deg2Rad;
                into[PoseSize + 4 * a] = m.x;
                into[PoseSize + 4 * a + 1] = m.y;
                into[PoseSize + 4 * a + 2] = Mathf.Sin(r);
                into[PoseSize + 4 * a + 3] = Mathf.Cos(r);
            }
        }

        /// <summary>Raw numbers made ready to compare (less the mean, over the spread, weighed).</summary>
        public void Ready(float[] raw, float[] into)
        {
            for (int f = 0; f < Size; f++)
            {
                into[f] = (raw[f] - mean[f]) / spread[f] * weigh[f];
            }
        }

        /// <summary>The moment of a recording nearest a time, as its number in the footage (-1: the recording is not in it).</summary>
        public int MomentOf(LocoClip c, float time)
        {
            if (c == null || !firstOf.TryGetValue(c, out int first))
            {
                return -1;
            }

            int n = countOf[c];
            int i = Mathf.RoundToInt(c.Wrap(time) * Rate);
            return first + (c.Loop ? i % n : Mathf.Min(i, n - 1));
        }

        public LocoClip ClipOf(int moment) => Clips[clipOf[moment]];

        public float TimeOf(int moment) => timeOf[moment];

        /// <summary>How far a moment is from what is asked (ready numbers; the smaller the nearer).</summary>
        public float Cost(int moment, float[] ask)
        {
            float cost = 0f;
            int at = moment * Size;
            for (int f = 0; f < Size; f++)
            {
                float d = numbers[at + f] - ask[f];
                cost += d * d;
            }

            return cost;
        }

        /// <summary>The moment that best fits what is asked, of those that may be gone into; a moment of the recording playing, within <paramref name="near"/> seconds of where it is, is left out.</summary>
        public int Best(float[] ask, LocoClip playing, float playingTime, float near, out float cost)
        {
            int best = -1;
            cost = float.MaxValue;
            int playingClip = playing == null ? -1 : Array.IndexOf(Clips, playing);
            for (int i = 0; i < Count; i++)
            {
                if (!enter[i])
                {
                    continue;
                }

                if (clipOf[i] == playingClip)
                {
                    float d = Mathf.Abs(timeOf[i] - playingTime);
                    if (playing.Loop)
                    {
                        d = Mathf.Min(d, playing.Duration - d);
                    }

                    if (d < near)
                    {
                        continue;
                    }
                }

                float c = 0f;
                int at = i * Size;
                for (int f = 0; f < Size && c < cost; f++)
                {
                    float d = numbers[at + f] - ask[f];
                    c += d * d;
                }

                if (c < cost)
                {
                    cost = c;
                    best = i;
                }
            }

            return best;
        }
    }

    /// <summary>
    /// A walk made by motion matching (the plan "Walking that feels like a real person", step 2): several times a
    /// second the footage is searched for the recorded moment that best fits the pose he is in and the path he is
    /// asked to take, and the recording is played on from there at its own pace. The recording moves his body: his
    /// root goes as the performer's went, pulled only a little toward what was asked. A jump from one recording to
    /// another is hidden by carrying the difference on and letting it die away (the pose's, and the root's speed's).
    ///
    /// It gives the pose as the library's points against the root, as a clip does: what rebuilds him from his parts
    /// comes after it and is not changed by it.
    /// </summary>
    public sealed class MotionMatch
    {
        /// <summary>The footage is searched this often (seconds).</summary>
        public static float SearchEvery = 0.1f;

        /// <summary>A found moment has to fit this much better than playing on, or he plays on (a share of playing on's own cost).</summary>
        public static float JumpsIfBetterBy = 0.15f;

        /// <summary>The recording is played at most this much faster or slower than it was performed, to go at the speed asked (0.1: a tenth; the plan's mark).</summary>
        public static float TempoAtMost = 0.1f;

        /// <summary>A moment of the recording that is playing, this near where it is, is not jumped to (seconds).</summary>
        public static float NotNearer = 0.25f;

        /// <summary>A jump's difference in pose dies away with this half-life (seconds); the root's speed's with the second.</summary>
        public static float PoseHalfLife = 0.07f, SpeedHalfLife = 0.12f;

        /// <summary>How fast the path asked for comes round to a new wish (the half-life of the speed and of the facing, seconds).</summary>
        public static float WishHalfLife = 0.22f, FacingHalfLife = 0.18f;

        /// <summary>The recording's own travel is pulled toward the speed asked by at most this share of its own speed, and its facing by at most this many degrees a second.</summary>
        public static float PullsAtMost = 0.05f, TurnsAtMost = 30f;

        public readonly MatchFootage Footage;
        public readonly float Leg;

        /// <summary>
        /// How a recorded moment is made into a pose (every library point against the root, metres). Left unset it
        /// is the recording's own points; the walk sets it to the rebuild from parts (<see cref="MoveScore.Pose(LocoClip, float, Vector3[], float, LocoBone[])"/>), so his levers work on it.
        /// </summary>
        public Action<LocoClip, float, Vector3[]> Sample;

        private void PoseOf(LocoClip clip, float time, Vector3[] into)
        {
            if (Sample != null)
            {
                Sample(clip, time, into);
            }
            else
            {
                clip.Pose(time, into, Leg);
            }
        }

        /// <summary>The recording playing and where in it he is.</summary>
        public LocoClip Clip { get; private set; }
        public float Time { get; private set; }

        /// <summary>His root in the world: its place on the ground (metres) and its facing (degrees, a turn to the right).</summary>
        public Vector2 Place;
        public float Facing;

        /// <summary>How fast his root went over the last step (m/s, the world's axes) and how fast the recording itself went.</summary>
        public Vector2 Velocity { get; private set; }
        public Vector2 ClipVelocity { get; private set; }

        /// <summary>Jumps made, searches made, and the last search's best cost against playing on's.</summary>
        public int Jumps { get; private set; }

        /// <summary>How fast the recording is played now against its own pace (1: as performed).</summary>
        public float Tempo { get; private set; } = 1f;
        public int Searches { get; private set; }
        public float LastBest { get; private set; }
        public float LastOn { get; private set; }

        private readonly Vector3[] off = new Vector3[LocoClip.BoneCount], offSpeed = new Vector3[LocoClip.BoneCount];
        private readonly Vector3[] shown = new Vector3[LocoClip.BoneCount], before = new Vector3[LocoClip.BoneCount], source = new Vector3[LocoClip.BoneCount], sourceWas = new Vector3[LocoClip.BoneCount];
        private readonly float[] raw = new float[MatchFootage.Size], ask = new float[MatchFootage.Size];
        private Vector2 speedOff;
        private float turnOff;
        private float sinceSearch = 1f;
        private bool posed;

        public MotionMatch(MatchFootage footage, float leg, LocoClip startIn, Vector2 place, float facing)
        {
            Footage = footage;
            Leg = leg;
            Clip = startIn ?? footage.Clips[0];
            Time = 0f;
            Place = place;
            Facing = facing;
        }

        private static Vector2 Turned(Vector2 v, float degrees)
        {
            // (Out of a frame facing "degrees" into the world: x right, y forward.)
            float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c + v.y * s, -v.x * s + v.y * c);
        }

        private static Vector2 IntoFrame(Vector2 world, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(world.x * c - world.y * s, world.x * s + world.y * c);
        }

        private static void Decay(ref Vector3 x, ref Vector3 v, float halfLife, float dt)
        {
            float y = 2f * 0.6931472f / Mathf.Max(1e-4f, halfLife);
            Vector3 j = v + x * y;
            float e = Mathf.Exp(-y * dt);
            x = e * (x + j * dt);
            v = e * (v - j * y * dt);
        }

        /// <summary>
        /// One step. <paramref name="wantVelocity"/>: the speed asked over the ground (m/s, the world's axes: x and
        /// z); <paramref name="wantFacing"/>: the facing asked (degrees). His root is moved and his pose made ready
        /// (<see cref="Pose"/>).
        /// </summary>
        public void Step(float dt, Vector2 wantVelocity, float wantFacing)
        {
            if (dt <= 0f)
            {
                return;
            }

            // (What was shown last step, to carry on from if he jumps.)
            if (posed)
            {
                Array.Copy(shown, before, shown.Length);
            }

            sinceSearch += dt;
            bool ending = !Clip.Loop && Time >= Clip.Duration - 0.5f * dt;
            if (sinceSearch >= SearchEvery || ending)
            {
                Search(wantVelocity, wantFacing, ending);
                sinceSearch = 0f;
            }

            // The recording plays on, and its root's going is his. Its pace is its own to within a tenth: asked to go a
            // little faster or slower than it goes, it is played that much faster or slower (feet and ground stay
            // together, where a pull on the root alone slides a planted foot).
            float wantTempo = 1f;
            if (TempoAtMost > 0f && wantVelocity.magnitude > 0.2f)
            {
                float ahead = Clip.Loop ? Time + 0.1f : Mathf.Min(Time + 0.1f, Clip.Duration);
                if (ahead > Time)
                {
                    Clip.RootMotion(Time, ahead, out var goes, out _);
                    Vector2 own = Turned(goes * Leg, Facing) / (ahead - Time);
                    if (own.magnitude > 0.25f)
                    {
                        wantTempo = Mathf.Clamp(Vector2.Dot(wantVelocity, own) / own.sqrMagnitude, 1f - TempoAtMost, 1f + TempoAtMost);
                    }
                }
            }

            Tempo = Mathf.Lerp(wantTempo, Tempo, Mathf.Exp(-0.6931472f * dt / 0.2f));
            float from = Time, to = Clip.Loop ? Time + dt * Tempo : Mathf.Min(Time + dt * Tempo, Clip.Duration);
            Vector2 moved = Vector2.zero;
            float turned = 0f;
            if (to > from)
            {
                Clip.RootMotion(from, to, out moved, out turned);
            }

            Time = Clip.Loop ? Mathf.Repeat(to, Clip.Duration) : to;
            Vector2 went = Turned(moved * Leg, Facing) / dt;
            ClipVelocity = went;

            // Pulled a little toward what was asked: never by more than a share of how fast the recording itself goes,
            // so that a man standing is not slid.
            Vector2 pull = wantVelocity - went;
            float most = PullsAtMost * went.magnitude;
            if (pull.magnitude > most)
            {
                pull *= most / pull.magnitude;
            }

            float turnPull = Mathf.Clamp(Mathf.DeltaAngle(Facing + turned, wantFacing) * 3f, -TurnsAtMost, TurnsAtMost) * Mathf.Clamp01(went.magnitude / 0.5f);

            // A jump's difference in the root's speed dies away rather than being dropped.
            float k = Mathf.Exp(-0.6931472f * dt / Mathf.Max(1e-4f, SpeedHalfLife));
            speedOff *= k;
            turnOff *= k;

            Vector2 velocity = went + pull + speedOff;
            Place += velocity * dt;
            Facing += turned + (turnPull + turnOff) * dt;
            Velocity = velocity;

            // The pose: the recording's, and what is left of the last jump's difference.
            Array.Copy(source, sourceWas, source.Length);
            PoseOf(Clip, Time, source);
            for (int i = 0; i < off.Length; i++)
            {
                Decay(ref off[i], ref offSpeed[i], PoseHalfLife, dt);
            }

            // A planted foot stays where it is planted: what a jump left between where it stands and where the new
            // recording has it is kept while it is down (the leg reaches for it), and dies away once it is lifted.
            for (int f = 0; f < 2; f++)
            {
                if (!planted[f])
                {
                    continue;
                }

                if (!Down(f) || !PlantedFeetStay)
                {
                    planted[f] = false;
                    for (int q = 0; q < 3; q++)
                    {
                        int b = (int)FootBones[f][q];
                        off[b] = new Vector3(plantedOff[3 * f + q].x, off[b].y, plantedOff[3 * f + q].z);
                        offSpeed[b] = new Vector3(0f, offSpeed[b].y, 0f);
                    }

                    continue;
                }

                // (Not past a short step's reach: more than that is let go slowly, as a foot shuffles.)
                float far = new Vector2(plantedOff[3 * f].x, plantedOff[3 * f].z).magnitude, reach = PlantedStaysWithin * Leg;
                float keep = far > reach ? Mathf.Max(reach / far, Mathf.Exp(-0.6931472f * dt / 0.15f)) : 1f;
                for (int q = 0; q < 3; q++)
                {
                    int b = (int)FootBones[f][q];
                    plantedOff[3 * f + q] *= keep;
                    off[b] = new Vector3(plantedOff[3 * f + q].x, off[b].y, plantedOff[3 * f + q].z);
                }
            }

            for (int i = 0; i < off.Length; i++)
            {
                shown[i] = source[i] + off[i];
            }

            posed = true;
            lastDt = dt;
            lastTurnRate = turned / dt;
        }

        private void Search(Vector2 wantVelocity, float wantFacing, bool mustLeave)
        {
            Searches++;
            // His pose's numbers are the playing moment's own; the path's are what is asked of him, in his own frame.
            MatchFootage.Raw(Clip, Time, raw);
            Vector2 goal = IntoFrame(wantVelocity, Facing) / Leg;
            Vector2 now = IntoFrame(Velocity, Facing) / Leg;
            float lv = 0.6931472f / Mathf.Max(1e-3f, WishHalfLife), lf = 0.6931472f / Mathf.Max(1e-3f, FacingHalfLife);
            float turn = Mathf.DeltaAngle(Facing, wantFacing);
            for (int a = 0; a < MatchFootage.Ahead.Length; a++)
            {
                float t = MatchFootage.Ahead[a];
                Vector2 at = goal * t + (now - goal) * ((1f - Mathf.Exp(-lv * t)) / lv);
                float r = turn * (1f - Mathf.Exp(-lf * t)) * Mathf.Deg2Rad;
                raw[MatchFootage.PoseSize + 4 * a] = at.x;
                raw[MatchFootage.PoseSize + 4 * a + 1] = at.y;
                raw[MatchFootage.PoseSize + 4 * a + 2] = Mathf.Sin(r);
                raw[MatchFootage.PoseSize + 4 * a + 3] = Mathf.Cos(r);
            }

            Footage.Ready(raw, ask);
            int here = Footage.MomentOf(Clip, Time);
            float on = mustLeave || here < 0 ? float.MaxValue : Footage.Cost(here, ask);
            int best = Footage.Best(ask, Clip, Time, NotNearer, out float cost);
            LastBest = cost;
            LastOn = on;
            if (best < 0 || !(cost < on * (1f - JumpsIfBetterBy)))
            {
                return;
            }

            JumpTo(Footage.ClipOf(best), Footage.TimeOf(best));
        }

        /// <summary>He goes on from another recorded moment; what he was doing is carried over and dies away.</summary>
        public void JumpTo(LocoClip clip, float time)
        {
            Jumps++;
            const float h = 1f / 60f;
            // (How the new recording's pose and root are going, against how the old one's were.)
            var next = new Vector3[LocoClip.BoneCount];
            var nextOn = new Vector3[LocoClip.BoneCount];
            PoseOf(clip, time, next);
            PoseOf(clip, clip.Loop ? time + h : Mathf.Min(time + h, clip.Duration), nextOn);
            MatchFootage.Travel(clip, time, time + h, out var moved, out float turned);
            Vector2 will = Turned(moved * Leg, Facing) / h;
            if (posed)
            {
                for (int i = 0; i < off.Length; i++)
                {
                    Vector3 was = (source[i] - sourceWas[i]) / Mathf.Max(1e-4f, lastDt) + offSpeed[i];
                    off[i] = shown[i] - next[i];
                    offSpeed[i] = was - (nextOn[i] - next[i]) / h;
                }

                speedOff += ClipVelocity - will;
                turnOff += lastTurnRate - turned / h;

                // (A foot down before the jump and after it: it stays where it stands.)
                for (int f = 0; f < 2; f++)
                {
                    bool stays = PlantedFeetStay && Down(f) && clip.Contact(f, time) != LocoContact.None;
                    if (stays)
                    {
                        planted[f] = true;
                        for (int k = 0; k < 3; k++)
                        {
                            plantedOff[3 * f + k] = off[(int)FootBones[f][k]];
                            plantedOff[3 * f + k].y = 0f;
                        }
                    }
                    else if (planted[f])
                    {
                        planted[f] = false;
                    }
                }
            }

            Clip = clip;
            Time = time;
        }

        private float lastDt = 1f / 60f, lastTurnRate;

        /// <summary>A foot that is down through a jump between recordings stays where it stands (the leg reaches for it), up to this far from where the new recording has it (leg lengths). Off: it slides there with the rest of the pose.</summary>
        public static bool PlantedFeetStay = true;
        public static float PlantedStaysWithin = 0.1f;

        private static readonly LocoBone[][] FootBones =
        {
            new[] { LocoBone.LeftFoot, LocoBone.LeftToeBase, LocoBone.LeftToeEnd },
            new[] { LocoBone.RightFoot, LocoBone.RightToeBase, LocoBone.RightToeEnd },
        };

        private readonly bool[] planted = new bool[2];
        private readonly Vector3[] plantedOff = new Vector3[6];

        /// <summary>His pose after the last step: every library point against his root (metres; x right, y up, z forward).</summary>
        public Vector3[] Pose => shown;

        /// <summary>Whether the recording has a foot down now (0 left, 1 right).</summary>
        public bool Down(int foot) => Clip.Contact(foot, Time) != LocoContact.None;

        /// <summary>A library point in the world (metres).</summary>
        public Vector3 World(LocoBone bone)
        {
            Vector3 p = shown[(int)bone];
            Vector2 flat = Place + Turned(new Vector2(p.x, p.z), Facing);
            return new Vector3(flat.x, p.y, flat.y);
        }
    }
}
