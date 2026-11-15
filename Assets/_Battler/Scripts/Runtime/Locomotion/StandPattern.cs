using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a long step is taken (the plan "Movement Styles as Offshoots of the Standard Set", step S3): a foot swept
    /// a long way over the ground while the other stays planted. Read off the capoeira pack's gingas (fifteen steps
    /// of 1.4 to 2.2 m): a step takes 0.38 s for each metre it goes; the ankle rises 6 to 7 cm for each metre, and is
    /// highest near the end of the step that is behind him (late in a step back, early in a step forward); its path
    /// bows a little off the straight line, away from the foot that stands.
    /// </summary>
    public struct LongStepStyle
    {
        /// <summary>How high the ankle rises, as a share of how far the step goes; and the least it rises (leg lengths).</summary>
        public float Lift, LeastLift;

        /// <summary>Where along a step back the foot is highest (0 to 1 of the swing); a step forward is highest as far from its end.</summary>
        public float HighAt;

        /// <summary>How far the path bows off the straight line, away from the standing foot, as a share of how far the step goes.</summary>
        public float Bow;

        /// <summary>How long a step takes for each leg length it goes, and the least it takes (seconds).</summary>
        public float TimePerLength, LeastTime;

        public static LongStepStyle Plain => new LongStepStyle { Lift = 0.065f, LeastLift = 0.04f, HighAt = 0.7f, Bow = 0.08f, TimePerLength = 0.34f, LeastTime = 0.24f };
    }

    /// <summary>
    /// The long step (the plan's step S3), a part of the standard set: where a swinging foot's ankle is at each
    /// moment of a step from one place to another, and how long the step takes. The foot that stands is not moved.
    /// </summary>
    public static class LongStep
    {
        public static LongStepStyle Style = LongStepStyle.Plain;

        /// <summary>How long a step from one place to another takes (seconds; the places in leg lengths).</summary>
        public static float Time(Vector3 from, Vector3 to, in LongStepStyle style) =>
            Mathf.Max(style.LeastTime, style.TimePerLength * new Vector2(to.x - from.x, to.z - from.z).magnitude);

        /// <summary>Where the swinging foot's ankle is, a share <paramref name="u"/> of the way through its step (the places' own frame and units).</summary>
        /// <param name="planted">Where the other foot stands: the path bows away from it.</param>
        public static Vector3 Swing(Vector3 from, Vector3 to, float u, Vector3 planted, in LongStepStyle style)
        {
            u = Mathf.Clamp01(u);
            Vector3 line = new Vector3(to.x - from.x, 0f, to.z - from.z);
            float far = line.magnitude;
            if (far < 1e-4f)
            {
                return Vector3.Lerp(from, to, u);
            }

            Vector3 along = line / far, across = new Vector3(along.z, 0f, -along.x);
            float side = Vector3.Dot(0.5f * (from + to) - planted, across) >= 0f ? 1f : -1f;
            float highAt = Mathf.Clamp(to.z < from.z ? style.HighAt : 1f - style.HighAt, 0.15f, 0.85f);
            // (The swing's own share bent so its middle falls where the foot is highest.)
            float bent = Mathf.Pow(u, Mathf.Log(0.5f) / Mathf.Log(highAt));
            float lift = Mathf.Max(style.LeastLift, style.Lift * far) * Mathf.Sin(Mathf.PI * bent);
            float gone = u * u * (3f - 2f * u);
            return Vector3.Lerp(from, to, gone) + across * (side * style.Bow * far * Mathf.Sin(Mathf.PI * u)) + Vector3.up * lift;
        }
    }

    /// <summary>
    /// A stand that steps, made of its recordings' own motion (the ginga; the plan "Movement Styles as Offshoots of
    /// the Standard Set", the second way of step S4): as an attack is made of a recorded strike, the ginga is made of
    /// the recorded takes, frame by frame, and only what has to change is changed.
    ///
    /// <para><b>What is the recording's.</b> Every part of the body at every moment: how the hips shuttle and rise,
    /// how the trunk turns and bends, how the arms sweep, how each foot swings and how it lies on the ground. A take
    /// is taken apart into its parts' tracks once, and read at whatever moment of it he is in.</para>
    /// <para><b>What is changed.</b> Its size (the feet's places and the hips' travel, about the take's middle). Its
    /// pace. Its rests: each take comes twice to a stand with one foot back, and there he is held as long as his
    /// restlessness lets him: he slows into it and rocks about it, the take's own motion there gone through slowly, on
    /// and back (the recorded man all but stops there too, and goes straight on). Which take: at a rest he may go over to
    /// another of the four, on the same side, so no stretch of it comes round again the same. And the feet: a foot
    /// on the ground is held exactly where it was put down, and a foot in the air goes the recording's own way,
    /// bent so that it leaves from where the foot stood and lands where the take he is now in has it, a little off
    /// each time.</para>
    /// <para>As he comes to stand the take is laid out about him with his hips where they are, at the rest his feet
    /// are nearest, and each foot that is off its place steps onto it (<see cref="LongStep"/>).</para>
    /// </summary>
    public sealed class StandPattern
    {
        /// <summary>The capoeira style stands in the ginga made of its takes' motion (the scene's Combat Feel, "Ginga Is Made Of Its Takes"); off: one recorded take, looped, as the first pass had it.</summary>
        public static bool Generated = true;

        /// <summary>How big it is made against the recordings (the feet's places and the hips' travel; 1: as recorded).</summary>
        public static float Size = 0.8f;

        /// <summary>How fast it is played against the recordings (1: as recorded).</summary>
        public static float Tempo = 1f;

        /// <summary>How restless he is: how long he stays at each rest (1: not at all, as the recordings; 0.4: a second or so; near 0: many seconds).</summary>
        public static float Restless = 0.4f;

        /// <summary>How much it varies (0: one take, every rest as long, every foot on its place; 1: he goes over to another take at every other rest, the rests up to six tenths longer or shorter, the feet up to 6 cm off their places).</summary>
        public static float Vary = 0.5f;

        /// <summary>He goes on from his rest now (the duel lab's button): counted up; each pattern takes it once.</summary>
        public static int ChangesAsked;

        /// <summary>How long going over from one take to another, or from the pose he came to stand in, takes (seconds).</summary>
        public const float Fade = 0.4f;

        /// <summary>A foot nearer its place than this when he comes to stand is left where it is (leg lengths).</summary>
        public const float NearEnough = 0.08f;

        /// <summary>One foot is "back" at a rest when it is this far behind the other (leg lengths).</summary>
        public const float BackBy = 0.45f;

        private sealed class Take
        {
            public string Name;
            public BodyTracks[] Frames;
            public bool[] DownL, DownR;
            // (How much of him is moving at each frame: his hips', head's and hands' speeds together, leg lengths a second.)
            public float[] Stir;
            public float Rate, Duration;
            // (Its rests: the moment in the middle of each, and whether the right foot is the back one there.)
            public readonly List<(float at, bool rightBack)> Rests = new List<(float at, bool rightBack)>();

            public int Count => Frames.Length;

            public BodyTracks At(float time)
            {
                float f = Mathf.Repeat(time, Duration) * Rate;
                int a = Mathf.FloorToInt(f) % Count, b = (a + 1) % Count;
                return BodyTracks.Lerp(Frames[a], Frames[b], f - Mathf.Floor(f));
            }

            public bool Down(int foot, float time)
            {
                int f = Mathf.RoundToInt(Mathf.Repeat(time, Duration) * Rate) % Count;
                return foot == 0 ? DownL[f] : DownR[f];
            }

            /// <summary>When the foot, in the air at a time, next comes down (a time at or after it, on past the loop's end if need be).</summary>
            public float Lands(int foot, float time)
            {
                float step = 1f / Rate;
                for (float t = time; t < time + Duration; t += step)
                {
                    if (Down(foot, t))
                    {
                        return t;
                    }
                }

                return time + step;
            }
        }

        private static readonly Dictionary<string, Take> taken = new Dictionary<string, Take>();

        private static Take Of(LocoClip clip)
        {
            if (taken.TryGetValue(clip.Name, out var take))
            {
                return take;
            }

            var m = BodyMeasures.OfClips;
            var p = new Vector3[LocoClip.BoneCount];
            // (A loop's last frame is its first again: left out.)
            int n = Mathf.Max(2, clip.Frames - 1);
            take = new Take { Name = clip.Name, Frames = new BodyTracks[n], DownL = new bool[n], DownR = new bool[n], Stir = new float[n], Rate = clip.Rate, Duration = n / clip.Rate };
            var before = new Vector3[LocoClip.BoneCount];
            int[] stirred = { (int)LocoBone.Hips, (int)LocoBone.Head, (int)LocoBone.LeftHand, (int)LocoBone.RightHand };
            for (int f = 0; f <= n; f++)
            {
                clip.Pose((f % n) / clip.Rate, p, 1f);
                if (f > 0)
                {
                    foreach (int b in stirred)
                    {
                        take.Stir[f - 1] += (p[b] - before[b]).magnitude * clip.Rate;
                    }
                }

                System.Array.Copy(p, before, p.Length);
                if (f == n)
                {
                    break;
                }

                take.Frames[f] = f > 0 ? m.Decompose(p, take.Frames[f - 1]) : m.Decompose(p);
                take.DownL[f] = (clip.Contact(0, f / clip.Rate) & LocoContact.Down) != 0;
                take.DownR[f] = (clip.Contact(1, f / clip.Rate) & LocoContact.Down) != 0;
            }

            // The rests: each stretch with both feet down and one well behind the other; its middle.
            bool Rests(int f, out bool rightBack)
            {
                float behind = take.Frames[f].AnkleL.z - take.Frames[f].AnkleR.z;
                rightBack = behind > 0f;
                return take.DownL[f] && take.DownR[f] && Mathf.Abs(behind) > BackBy;
            }

            int first = 0;
            while (first < n && Rests(first, out _))
            {
                first++;
            }

            for (int k = 0; k < n; k++)
            {
                int f = (first + k) % n;
                if (!Rests(f, out bool rightBack))
                {
                    continue;
                }

                int length = 1;
                while (length < n && Rests((f + length) % n, out _))
                {
                    length++;
                }

                // (The moment of it he is stillest at: the recorded man all but stops there.)
                int still = f;
                for (int g = f; g < f + length; g++)
                {
                    if (take.Stir[((g % n) + n) % n] < take.Stir[((still % n) + n) % n])
                    {
                        still = g;
                    }
                }

                take.Rests.Add((Mathf.Repeat((still + 0.5f) / take.Rate, take.Duration), rightBack));
                k += length - 1;
            }

            return taken[clip.Name] = take;
        }

        private readonly List<Take> takes = new List<Take>();
        private readonly System.Random random;
        private static int made;

        private Take take;
        private float at;                       // the moment of the take he is in (seconds of it)
        private Vector3 origin;                 // where the take's own middle is, against the walk's root
        private readonly Vector3[] foot = new Vector3[2];       // where each foot stands, or left the ground
        private readonly bool[] air = new bool[2];
        private readonly float[] lifted = new float[2], lands = new float[2];
        private readonly Vector3[] goesTo = new Vector3[2];
        private int restAt = -1;                // the rest he is held in, or -1
        private float held, holdFor;
        private float pace = 1f;                // how fast the take is gone through now (1: as recorded)
        private float restMoment, pastRest;     // the moment he is held about; no rest is come to, and no foot lifts, before he is past this
        private int restsSince;
        private BodyTracks fadeFrom;
        private float fade = 1f;
        private int asked;
        private float breath;
        // (Coming to stand: each foot that is off its place steps onto it, one after the other.)
        private readonly List<(int foot, Vector3 from, Vector3 to, float time)> entry = new List<(int foot, Vector3 from, Vector3 to, float time)>();
        private float entryAt;
        private bool begun;

        /// <summary>How many times he has gone on from a rest, and the take he is in and the moment of it (for the checks against the recordings).</summary>
        public int Changes { get; private set; }
        public string TakeName => take != null ? take.Name : "";
        public float TakeTime => take != null ? Mathf.Repeat(at, take.Duration) : 0f;

        /// <summary>Where the pattern has his hips now (the walk's root frame, leg lengths; for the checks).</summary>
        public Vector3 Hips => lastOut.Pelvis;
        public Vector3 AnkleL => lastOut.AnkleL;
        public Vector3 AnkleR => lastOut.AnkleR;

        /// <summary>He is held at a rest now.</summary>
        public bool Resting => restAt >= 0 || entry.Count > 0;

        private StandPattern(IEnumerable<LocoClip> clips)
        {
            foreach (var clip in clips)
            {
                var t = Of(clip);
                if (t.Rests.Count >= 2)
                {
                    takes.Add(t);
                }
            }

            random = new System.Random(7919 * ++made);
            asked = ChangesAsked;
        }

        /// <summary>The ginga of some recorded takes (their clips; each a loop on the spot that comes to rest on both sides); null when none will do.</summary>
        public static StandPattern Ginga(IEnumerable<LocoClip> clips)
        {
            var pattern = new StandPattern(clips);
            return pattern.takes.Count > 0 ? pattern : null;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private static float Smooth(float u)
        {
            u = Mathf.Clamp01(u);
            return u * u * (3f - 2f * u);
        }

        private float Varies() => Mathf.Clamp01(Vary) * (float)(random.NextDouble() * 2.0 - 1.0);

        /// <summary>A place of the take's (its own frame, leg lengths) as it is against the walk's root: made as big as the pattern is, about the take's middle.</summary>
        private Vector3 Placed(Vector3 inTake) => origin + new Vector3(inTake.x * Size, inTake.y, inTake.z * Size);

        private static Vector3 Ankle(in BodyTracks t, int which) => which == 0 ? t.AnkleL : t.AnkleR;

        /// <summary>
        /// He has come to stand, his feet where they are (the walk's root frame, leg lengths): the take is laid out
        /// about him at the rest his feet are nearest, his hips where they are, and each foot that is off its place
        /// is set to step onto it.
        /// </summary>
        public void Begin(Vector3 ankleL, Vector3 ankleR, in BodyTracks standing)
        {
            bool rightBack = ankleR.z <= ankleL.z;
            take = takes[0];
            int rest = 0;
            for (int i = 0; i < take.Rests.Count; i++)
            {
                if (take.Rests[i].rightBack == rightBack)
                {
                    rest = i;
                }
            }

            at = take.Rests[rest].at;
            var pose = take.At(at);
            // (Laid out so the rest's hips are over the root, where his are.)
            origin = Vector3.zero;
            origin = -Flat(Placed(pose.Pelvis));
            foot[0] = Flat(ankleL);
            foot[1] = Flat(ankleR);
            air[0] = air[1] = false;
            originFoot = -1;
            entry.Clear();
            entryAt = 0f;
            int back = rightBack ? 1 : 0;
            foreach (int which in new[] { back, 1 - back })
            {
                Vector3 to = Flat(Placed(Ankle(pose, which)));
                if ((to - foot[which]).magnitude > NearEnough)
                {
                    entry.Add((which, foot[which], to, LongStep.Time(foot[which], to, LongStep.Style)));
                }
            }

            restAt = rest;
            restMoment = at;
            pastRest = at;
            pace = 0f;
            held = 0f;
            holdFor = Mathf.Max(0.3f, HoldTime());
            fadeFrom = standing;
            fade = 0f;
            asked = ChangesAsked;
            begun = true;
        }

        public bool Begun => begun;

        /// <summary>The walk has left the stand: the next time he stands the pattern is laid out again.</summary>
        public void End() => begun = false;

        /// <summary>How long he stays at a rest (seconds): none when he is as restless as the recordings; a second or so at 0.4; a little different each time.</summary>
        private float HoldTime()
        {
            float restless = Mathf.Clamp(Restless, 0.03f, 1f);
            return restless >= 0.995f ? 0f : Mathf.Min(10f, 0.35f * (1f / Mathf.Pow(restless, 1.5f) - 1f) * (1f + 0.6f * Varies()));
        }

        /// <summary>
        /// The next moment of it: the body's points (the walk's root frame, leg lengths) and whether each foot is on
        /// the ground. <paramref name="dt"/> is the time gone by (seconds, at the body's own pace).
        /// </summary>
        public void Step(float dt, Vector3[] points, out bool downL, out bool downR)
        {
            var m = BodyMeasures.OfClips;
            breath += dt;
            fade = Mathf.Min(1f, fade + dt / Fade);
            if (ChangesAsked != asked)
            {
                // (Asked to go on now: what is left of his rest is skipped.)
                asked = ChangesAsked;
                holdFor = 0f;
            }

            if (entry.Count > 0)
            {
                // Coming to stand: the feet step onto their places, one after the other; the body waits at the rest.
                entryAt += dt;
                var (which, from, to, time) = entry[0];
                float u = entryAt / Mathf.Max(0.05f, time);
                if (u >= 1f)
                {
                    foot[which] = to;
                    entry.RemoveAt(0);
                    entryAt = 0f;
                }
            }
            else if (restAt >= 0)
            {
                // Held at a rest: he rocks about it, the take's own motion there gone through slowly, on and back.
                held += dt;
                float turns = Mathf.Max(1f, Mathf.Round(holdFor / RockEvery));
                float w = 2f * Mathf.PI * turns / Mathf.Max(0.2f, holdFor);
                Pace(RockPace * Mathf.Cos(w * held), dt);
                at = Mathf.Clamp(at + pace * dt, restMoment - MostRock, restMoment + MostRock);
                if (held >= holdFor)
                {
                    restAt = -1;
                    pastRest = Mathf.Max(at, restMoment) + 0.03f;
                    holdFor = HoldTime();
                    Changes++;
                }
            }
            else
            {
                float before = at;
                // (How far the next rest is: he slows as he comes to one he will be held at.)
                float next = float.MaxValue;
                int which = -1;
                for (int i = 0; i < take.Rests.Count; i++)
                {
                    float gone = Mathf.Repeat(take.Rests[i].at - Mathf.Repeat(before, take.Duration), take.Duration);
                    if (gone > 1e-4f && gone < next)
                    {
                        next = gone;
                        which = i;
                    }
                }

                bool comes = before >= pastRest && holdFor > 0f && !air[0] && !air[1];
                Pace(Mathf.Max(0.2f, Tempo) * (comes ? Mathf.Lerp(RockPace, 1f, Mathf.Clamp01(next / SlowsFrom)) : 1f), dt);
                at += dt * pace;
                if (which >= 0 && before >= pastRest && next <= at - before + 1e-5f && !air[0] && !air[1])
                {
                    // (Come to a rest: he is held there, and may go over to another take.)
                    at = before + next;
                    restMoment = at;
                    held = 0f;
                    restsSince++;
                    if (holdFor > 0f)
                    {
                        restAt = which;
                    }
                    else
                    {
                        holdFor = HoldTime();
                    }

                    MaybeChangeTake(take.Rests[which].rightBack);
                }
            }

            var t = take.At(at);
            Vector3 ankleL = Foot(0, t), ankleR = Foot(1, t);
            // The body: the take's, as big as the pattern, where the pattern is laid out.
            Vector3 hips = Placed(t.Pelvis);
            t.Pelvis = new Vector3(hips.x, t.Pelvis.y, hips.z);

            if (fade < 1f)
            {
                // (Going over from the pose he came to stand in, or from another take: everything but the feet's places.)
                t = BodyTracks.Lerp(fadeFrom, t, Smooth(fade));
            }

            t.AnkleL = ankleL;
            t.AnkleR = ankleR;
            m.Compose(t, points);
            downL = !air[0] && !(entry.Count > 0 && entry[0].foot == 0);
            downR = !air[1] && !(entry.Count > 0 && entry[0].foot == 1);
            lastOut = t;
        }

        /// <summary>The take is gone through at a pace that changes no faster than this (a second: from still to full pace in a quarter of one).</summary>
        public const float PaceChanges = 4f;

        /// <summary>Held at a rest he rocks about it at this pace at most (of the recording's), once every so many seconds, this far on and back at most (seconds of the take); he slows to it over the last so many seconds of the take before the rest.</summary>
        public const float RockPace = 0.25f, RockEvery = 2.4f, MostRock = 0.12f, SlowsFrom = 0.2f;

        private void Pace(float wanted, float dt) => pace = Mathf.MoveTowards(pace, wanted, PaceChanges * dt);

        private BodyTracks lastOut;
        private Vector3 originFrom, originTo;
        private int originFoot = -1;

        /// <summary>
        /// Where a foot's ankle is now. On the ground it is where it was put down (its height the take's: a heel
        /// lifts and sets). In the air it goes the take's own way, bent so that it leaves from where the foot stood
        /// and lands where the take has it now.
        /// </summary>
        private Vector3 Foot(int which, in BodyTracks t)
        {
            Vector3 inTake = Ankle(t, which);
            if (entry.Count > 0)
            {
                if (entry[0].foot == which)
                {
                    var (_, from, to, time) = entry[0];
                    Vector3 swung = LongStep.Swing(from, to, entryAt / Mathf.Max(0.05f, time), foot[1 - which], LongStep.Style);
                    return new Vector3(swung.x, inTake.y + swung.y, swung.z);
                }

                return new Vector3(foot[which].x, inTake.y, foot[which].z);
            }

            bool down = take.Down(which, at);
            if (!air[which] && !down && restAt < 0 && at >= pastRest)
            {
                // It leaves the ground: where it will land is settled now, a little off the take's own place.
                air[which] = true;
                lifted[which] = at;
                lands[which] = take.Lands(which, at);
                // (The pattern's middle comes back under him as he steps: half of what it is off, each step. It goes
                // off when he comes to stand at a rest, which is to one side of it, and when he goes over to another take.)
                originFrom = origin;
                originTo = origin + Vector3.ClampMagnitude(-origin * 0.5f, 0.15f);
                originFoot = which;
                Vector3 inTakeLanding = Ankle(take.At(lands[which]), which);
                Vector3 landing = originTo + new Vector3(inTakeLanding.x * Size, 0f, inTakeLanding.z * Size);
                // (A little off its place each time, a long step more than a short one.)
                float far = Mathf.Clamp01((landing - foot[which]).magnitude / 0.6f);
                goesTo[which] = landing + new Vector3(Varies(), 0f, Varies()) * (0.06f / Locomotor.ClipLeg) * Size * far;
            }
            else if (air[which] && (down || at >= lands[which]))
            {
                air[which] = false;
                foot[which] = goesTo[which];
                if (originFoot == which)
                {
                    origin = originTo;
                    originFoot = -1;
                }
            }

            if (!air[which])
            {
                return new Vector3(foot[which].x, inTake.y, foot[which].z);
            }

            // (The take's own path, carried at its start to where the foot stood and at its end to where it will land.)
            float u = Smooth((at - lifted[which]) / Mathf.Max(0.03f, lands[which] - lifted[which]));
            if (originFoot == which)
            {
                origin = Vector3.Lerp(originFrom, originTo, u);
            }

            Vector3 Sized(Vector3 v) => new Vector3(v.x * Size, 0f, v.z * Size);
            Vector3 path = Sized(inTake), left = Sized(Ankle(take.At(lifted[which]), which)), arrives = Sized(Ankle(take.At(lands[which]), which));
            // (Against the take's own start and end of this swing, so where the pattern is laid out does not come into it.)
            Vector3 place = foot[which] * (1f - u) + goesTo[which] * u + (path - Vector3.Lerp(left, arrives, u));
            return new Vector3(place.x, inTake.y, place.z);
        }

        /// <summary>At a rest he may go over to another take, at its rest on the same side: laid out so that the foot he stands on in front is where it is.</summary>
        private void MaybeChangeTake(bool rightBack)
        {
            if (takes.Count < 2 || Vary <= 0.01f || restsSince < Mathf.RoundToInt(Mathf.Lerp(8f, 2f, Mathf.Clamp01(Vary))))
            {
                return;
            }

            var next = takes[random.Next(takes.Count)];
            if (next == take)
            {
                return;
            }

            int rest = -1;
            for (int i = 0; i < next.Rests.Count; i++)
            {
                if (next.Rests[i].rightBack == rightBack)
                {
                    rest = i;
                }
            }

            if (rest < 0)
            {
                return;
            }

            fadeFrom = lastOut;
            fade = 0f;
            take = next;
            at = next.Rests[rest].at;
            restMoment = at;
            pastRest = at;
            // (Laid out so that its two feet are as near as can be to where his are: the middle between them the same.)
            var pose = next.At(at);
            origin = Vector3.zero;
            origin = 0.5f * (foot[0] + foot[1]) - 0.5f * (Flat(Placed(pose.AnkleL)) + Flat(Placed(pose.AnkleR)));
            originFoot = -1;
            restsSince = 0;
            if (restAt >= 0)
            {
                restAt = rest;
            }
        }
    }
}
