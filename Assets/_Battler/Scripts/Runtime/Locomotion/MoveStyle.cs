using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A movement style as a pack (the plan "Fight Rhythm from Recordings", step 1): a named set of recordings, each
    /// with a job in the walk. The walk itself (planted feet, real steps, stride fitting, stand and step) is the same
    /// for every style; what a style changes is which recordings it steps with. Give the walk another pack and the
    /// fighter moves another way.
    ///
    /// <para>The jobs: standing; walking forward and back; a step to each side; running forward and back; turning on
    /// the spot to each side and about. A job with no clip named takes the clip its prefix has for it
    /// (<see cref="UsualNames"/>: "sns_" + "walk_back"); a job the pack has nothing for is filled from the relaxed walk,
    /// as it always was.</para>
    /// </summary>
    public sealed class MoveStyle
    {
        /// <summary>What the style is called (the panels show it).</summary>
        public string Name;

        /// <summary>The clip library's prefix for this pack's recordings ("sns_": the sword and shield's guard).</summary>
        public string Prefix;

        /// <summary>
        /// The style's own profile in the profile file (<see cref="StyleProfile"/>: "guard", "sword"; null: it has none):
        /// what its carriage is counted from when another style's manner is laid over it (<see cref="StyleManner"/>).
        /// </summary>
        public string Profile;

        /// <summary>The jobs, in the order of <see cref="UsualNames"/>.</summary>
        public enum Job
        {
            Stand,
            WalkForward,
            WalkBack,
            StepLeft,
            StepRight,
            RunForward,
            RunBack,
            TurnLeft,
            TurnRight,
            TurnAbout,
        }

        /// <summary>What each job's clip is usually called, after the prefix. (Standing: the steadier take first.)</summary>
        public static readonly string[][] UsualNames =
        {
            new[] { "idle_b", "idle" }, new[] { "walk" }, new[] { "walk_back" }, new[] { "step_left" }, new[] { "step_right" },
            new[] { "run" }, new[] { "run_back" }, new[] { "turn_left" }, new[] { "turn_right" }, new[] { "turn180" },
        };

        /// <summary>A job's clip by its full name in the library, where it is not the prefix's usual one.</summary>
        public readonly Dictionary<Job, string> Named = new Dictionary<Job, string>();

        /// <summary>The clip a job is done with (null: the pack has none, and the relaxed walk's is used).</summary>
        public LocoClip Clip(Job job)
        {
            if (Named.TryGetValue(job, out string name))
            {
                return LocoLibrary.Get(name);
            }

            foreach (string usual in UsualNames[(int)job])
            {
                var clip = LocoLibrary.Get(Prefix + usual);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        /// <summary>
        /// The style's brisk pace (the plan's step 2; the scene's Combat Feel, "Guard Jogs"): above a walk the guard
        /// jogs, forward, back and to each side, where it had only its run forward and back (4 m/s, its stride
        /// shortened to the pace) and nothing of its own to the sides. The jogs were recorded with empty hands
        /// (2.4 to 3.0 m/s): their hips and legs are put under the guard's own chest, head and arms
        /// (<see cref="LegsUnder"/>), so the shield and the sword are carried as the guard carries them.
        /// </summary>
        public static bool Jogs = true;

        /// <summary>
        /// The guard walks in one carriage (the plan "Convincing walking in the dance", step 2; Combat Feel "The Guard
        /// Walks In One Carriage"). On: the standard style steps forward and back on an archer's legs, which keep the
        /// guard's turned stance (the sword and shield pack's own walk is tall, narrow and square), under the guard's
        /// own side-step's chest and arms; and its four steps (forward, back, left, right) are each carried as its
        /// stand is (<see cref="MoveScore.CarryAs"/>): the hips at the stand's height, the feet's places toward the
        /// stand's. Off: each as recorded, as it was. Its jogs and runs, for covering ground, are as recorded either way.
        /// </summary>
        public static bool OneCarriage
        {
            get => oneCarriage;
            set
            {
                if (value != oneCarriage)
                {
                    oneCarriage = value;
                    Stamp++;
                }
            }
        }

        private static bool oneCarriage = true;

        /// <summary>Changes whenever what a style's pack holds changes: a walker makes its guard's clips again (<see cref="Locomotor.RefreshStyle"/>).</summary>
        public static int Stamp { get; private set; }

        /// <summary>The library's prefix for the legs a style jogs on ("jog_"), or null: it has no jog.</summary>
        public string JogPrefix;

        /// <summary>The pack as the walk takes it (null when it has nothing to stand or walk with).</summary>
        public Locomotor.ClipSet Clips()
        {
            var set = new Locomotor.ClipSet
            {
                Name = Prefix,
                Idle = Clip(Job.Stand),
                WalkF = Clip(Job.WalkForward),
                WalkB = Clip(Job.WalkBack),
                StepL = Clip(Job.StepLeft),
                StepR = Clip(Job.StepRight),
                RunF = Clip(Job.RunForward),
                RunB = Clip(Job.RunBack),
                TurnL = Clip(Job.TurnLeft),
                TurnR = Clip(Job.TurnRight),
                Turn180 = Clip(Job.TurnAbout),
            };
            if (set.Idle == null || set.WalkF == null)
            {
                return null;
            }

            if (StandSteps)
            {
                // (The ginga made of its takes' own motion; or, with that off, one recorded take on a loop.)
                StandPattern pattern = null;
                if (StandPattern.Generated)
                {
                    var takes = new List<LocoClip>();
                    // (The quieter take first: he begins in it.)
                    foreach (string name in new[] { "cap_ginga_b", "cap_ginga", "cap_ginga_c", "cap_ginga_d" })
                    {
                        var take = LocoLibrary.Get(name);
                        if (take != null)
                        {
                            takes.Add(take);
                        }
                    }

                    pattern = StandPattern.Ginga(takes);
                }

                if (pattern != null)
                {
                    set.Pattern = pattern;
                    set.IdleSteps = true;
                }
                else
                {
                    set.IdleSteps = set.Idle.Source != null;
                    set.Idle = Sized(set.Idle, StandSize, StandArms);
                }

                set.Name = Name;
            }

            if (Jogs && !string.IsNullOrEmpty(JogPrefix))
            {
                // (Each jog's legs under the guard's chest and arms as they are in the nearest thing it has: its run
                // forward and back, its walk for the sides.)
                set.RunF = LegsUnder(LocoLibrary.Get(JogPrefix + "fwd"), set.RunF ?? set.WalkF, Prefix + "jog") ?? set.RunF;
                set.RunB = LegsUnder(LocoLibrary.Get(JogPrefix + "back"), set.RunB ?? set.WalkB ?? set.WalkF, Prefix + "jog_back") ?? set.RunB;
                set.RunL = LegsUnder(LocoLibrary.Get(JogPrefix + "left"), set.StepL ?? set.WalkF, Prefix + "jog_left");
                set.RunR = LegsUnder(LocoLibrary.Get(JogPrefix + "right"), set.StepR ?? set.WalkF, Prefix + "jog_right");
            }

            {
                // (The slow walks to the sides, recorded with empty hands: their legs under the guard's side-steps' chest and arms.)
                set.SlowL = LegsUnder(LocoLibrary.Get("slow_left"), set.StepL ?? set.WalkF, Prefix + "slow_left");
                set.SlowR = LegsUnder(LocoLibrary.Get("slow_right"), set.StepR ?? set.WalkF, Prefix + "slow_right");
                set.SneakL = LegsUnder(LocoLibrary.Get("sneak_left"), set.StepL ?? set.WalkF, Prefix + "sneak_left");
                set.SneakR = LegsUnder(LocoLibrary.Get("sneak_right"), set.StepR ?? set.WalkF, Prefix + "sneak_right");
            }

            if (OneCarriage && this == Standard)
            {
                // (The guard's walk forward and back on legs that keep his stance: an archer's, whose hips stay turned
                // 70 degrees from where he aims as the guard's own side-steps' do; under the guard's own side-step's
                // chest and arms. Then each of the four carried as the stand is.)
                set.WalkF = LegsUnder(LocoLibrary.Get("bow_walk_fwd"), set.StepL ?? set.WalkF, Prefix + "advance") ?? set.WalkF;
                set.WalkB = LegsUnder(LocoLibrary.Get("bow_walk_back"), set.StepL ?? set.WalkB ?? set.WalkF, Prefix + "retreat") ?? set.WalkB;
                var stand = Carriage.Of(set.Idle).Mid;
                foreach (var step in new[] { set.WalkF, set.WalkB, set.StepL, set.StepR })
                {
                    if (step != null)
                    {
                        MoveScore.Of(step).CarryAs(stand);
                    }
                }
            }

            if (GuardWalkRebuilt && this == Standard)
            {
                Rebuild(set);
            }

            if (this == Standard)
            {
                // (Its turns from the pack "Turns", recorded with empty hands: each one's steps on the guard's own stand,
                // under the guard's own chest and arms. The quick turn round, each way; and the turn a third of the
                // way round, each way.)
                set.QuickR = OnTheStand(LocoLibrary.Get("quick_turn180"), set.Idle, Prefix + "quick_turn_right");
                set.QuickL = OnTheStand(LocoLibrary.Get("quick_turn180_m"), set.Idle, Prefix + "quick_turn_left");
                set.WideR = OnTheStand(LocoLibrary.Get("wide_turn_right"), set.Idle, Prefix + "wide_turn_right");
                set.WideL = OnTheStand(LocoLibrary.Get("wide_turn_left"), set.Idle, Prefix + "wide_turn_left");
            }

            return set;
        }

        /// <summary>
        /// The round trip (the plan "Movement Styles as Offshoots of the Standard Set", step S1, its test T0; the
        /// scene's Combat Feel, "Guard Walk Is Rebuilt From The Plain Walk"): the standard guard walks forward and
        /// back on the relaxed walk's own recordings, empty-handed and upright, with the guard's manner laid on them
        /// (<see cref="StyleManner"/>): its lean, its chest's turn, its arms held up and still, as its profile has
        /// them in numbers. Nothing of the guard's own walk is played. Off (as it is): the guard's recordings. The
        /// rest of the guard (its stand, its steps to the sides, its turns, its jog) is its own either way.
        /// </summary>
        public static bool GuardWalkRebuilt = false;

        /// <summary>How much of the guard's carriage and of its motion the rebuilt walk is given (1: all of each, as measured).</summary>
        public static float RebuiltCarriage = 1f, RebuiltMotion = 1f;

        private static void Rebuild(Locomotor.ClipSet set)
        {
            StyleProfile plain = StyleProfile.Get("plain"), guard = StyleProfile.Get("guard");
            if (plain == null || guard == null)
            {
                return;
            }

            foreach (var (job, clip) in new[] { (Job.WalkForward, "walk_fwd"), (Job.WalkBack, "walk_back") })
            {
                if (plain.Has(job) && guard.Has(job) && plain.For(job, out var from) && guard.For(job, out var to))
                {
                    var rebuilt = StyleManner.Over(LocoLibrary.Get(clip), from, to, "rebuilt_" + clip, RebuiltCarriage, RebuiltMotion);
                    if (rebuilt != null && job == Job.WalkForward) set.WalkF = rebuilt;
                    if (rebuilt != null && job == Job.WalkBack) set.WalkB = rebuilt;
                }
            }
        }

        private static readonly Dictionary<string, LocoClip> made = new Dictionary<string, LocoClip>();

        /// <summary>
        /// One step cycle's hips and legs under another's chest, head and arms, frame by frame at the same moment of
        /// the stride (the left foot lands at the same time in both): a clip the walk plays like any cycle. Its travel,
        /// its feet's contacts and its stride are the legs'. Null when either is missing or is not a step cycle. (Made
        /// once for each name.)
        /// </summary>
        public static LocoClip LegsUnder(LocoClip legs, LocoClip upper, string name)
        {
            if (legs == null || upper == null || legs.Source == null || !legs.HasPhase || !upper.HasPhase)
            {
                return null;
            }

            if (made.TryGetValue(name, out var clip))
            {
                return clip;
            }

            var m = BodyMeasures.OfClips;
            var d = legs.Source;
            var data = new LocoClipData
            {
                name = name, role = d.role, loop = d.loop, rate = d.rate, frames = d.frames, legLength = d.legLength, duration = d.duration,
                speed = d.speed, direction = d.direction, turn = d.turn, stride = d.stride, markers = d.markers, root = d.root,
                contacts = d.contacts, phase = d.phase, kind = d.kind, set = "guard",
                about = legs.Name + "'s hips and legs under " + upper.Name + "'s chest, head and arms",
                points = new float[d.frames * LocoClip.BoneCount * 3],
            };
            Vector3[] under = new Vector3[LocoClip.BoneCount], over = new Vector3[LocoClip.BoneCount], p = new Vector3[LocoClip.BoneCount];
            for (int f = 0; f < d.frames; f++)
            {
                float time = f / d.rate;
                legs.Pose(time, under, 1f);
                upper.Pose(upper.TimeAtPhase(legs.PhaseAt(time)), over, 1f);
                var lower = m.Decompose(under);
                var t = m.Decompose(over);
                // (The chest's twist on the hips, the head and the arms are the upper clip's; where these hips turn and
                // lean, the chest goes with them.)
                t.Pelvis = lower.Pelvis; t.PelvisTurn = lower.PelvisTurn;
                t.AnkleL = lower.AnkleL; t.AnkleR = lower.AnkleR; t.KneeL = lower.KneeL; t.KneeR = lower.KneeR;
                t.FootL = lower.FootL; t.FootR = lower.FootR; t.ToeL = lower.ToeL; t.ToeR = lower.ToeR;
                m.Compose(t, p);
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int at = (f * LocoClip.BoneCount + i) * 3;
                    data.points[at] = p[i].x;
                    data.points[at + 1] = p[i].y;
                    data.points[at + 2] = p[i].z;
                }
            }

            return made[name] = new LocoClip(data);
        }

        /// <summary>How far the feet are drawn in from the stand's wide places at the middle of a turn made on the stand (0: not at all; 1: all the way to the recording's own, narrower, places).</summary>
        public static float TurnDrawsIn
        {
            get => turnDrawsIn;
            set
            {
                if (Mathf.Abs(value - turnDrawsIn) > 0.004f)
                {
                    turnDrawsIn = value;
                    Stamp++;
                }
            }
        }

        private static float turnDrawsIn = 0.5f;

        /// <summary>
        /// How much of a recorded turn's sinking a turn made on the stand takes (1: all of it). The pack's quick turn
        /// sinks 11 cm at its middle, from a stance that much taller than the guard's stand: to about the height the
        /// guard stands at already.
        /// </summary>
        public static float TurnSinks
        {
            get => turnSinks;
            set
            {
                if (Mathf.Abs(value - turnSinks) > 0.004f)
                {
                    turnSinks = value;
                    Stamp++;
                }
            }
        }

        private static float turnSinks = 0.4f;

        /// <summary>
        /// A turn on the spot made on a stand (the quick turns round, 17 November): a recording of a turn, made with
        /// empty hands from an upright stance of its own, gives its steps; the stance it starts from and ends in, and
        /// everything above the hips, are the stand's. Frame by frame, what the take's hips and feet do is taken as it
        /// differs from the take's own first and last frames, and laid on the stand: the hips' place, turn and lean;
        /// each foot's place, kept where it is in the world while the take has it on the ground (the difference
        /// between the two stances is given while the foot is in the air, so a planted foot does not slide), and
        /// which way it points; the knees turned with the feet. The chest's twist on the hips, the head, the arms and
        /// the hands are the stand's own throughout. The first and the last frame are the stand itself. The clip's
        /// travel, turn and contacts are the take's. Null when either is missing. (Made once for each name.)
        /// </summary>
        public static LocoClip OnTheStand(LocoClip take, LocoClip standClip, string name)
        {
            if (take == null || standClip == null || take.Source == null || take.Frames < 3)
            {
                return null;
            }

            string key = name + "/" + Mathf.RoundToInt(100f * TurnDrawsIn) + "/" + Mathf.RoundToInt(100f * TurnSinks);
            if (made.TryGetValue(key, out var clip))
            {
                return clip;
            }

            var m = BodyMeasures.OfClips;
            var d = take.Source;
            int n = d.frames;
            var stand = Carriage.Of(standClip).Mid;
            var p = new Vector3[LocoClip.BoneCount];
            var t = new BodyTracks[n];
            var place = new Vector3[n];
            var yaw = new float[n];
            for (int f = 0; f < n; f++)
            {
                take.Pose(f / d.rate, p, 1f);
                t[f] = f == 0 ? m.Decompose(p) : m.Decompose(p, t[f - 1]);
                take.Root(f / d.rate, out var at, out float facing);
                place[f] = new Vector3(at.x, 0f, at.y);
                yaw[f] = f == 0 ? facing : yaw[f - 1] + Mathf.DeltaAngle(yaw[f - 1], facing);
            }

            // (How far through its turn each frame is.)
            float all = yaw[n - 1] - yaw[0];
            var through = new float[n];
            for (int f = 0; f < n; f++)
            {
                through[f] = Mathf.Abs(all) > 1f ? Mathf.Clamp01((yaw[f] - yaw[0]) / all) : f / (n - 1f);
            }

            BodyTracks first = t[0], last = t[n - 1];
            Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
            float YawOf(Vector3 way) => Mathf.Atan2(way.x, way.z) * Mathf.Rad2Deg;

            // Each foot: what it is given (a shift on the ground, a turn about the upright) at each frame.
            var shift = new Vector3[2][];
            var swivel = new float[2][];
            for (int s = 0; s < 2; s++)
            {
                Vector3 standAnkle = s == 0 ? stand.AnkleL : stand.AnkleR, standFoot = s == 0 ? stand.FootL : stand.FootR;
                Vector3 Ankle(in BodyTracks b) => s == 0 ? b.AnkleL : b.AnkleR;
                Vector3 Foot(in BodyTracks b) => s == 0 ? b.FootL : b.FootR;
                Vector3 off0 = Flat(standAnkle - Ankle(first)), off1 = Flat(standAnkle - Ankle(last));
                float turn0 = Mathf.DeltaAngle(YawOf(Foot(first)), YawOf(standFoot)), turn1 = Mathf.DeltaAngle(YawOf(Foot(last)), YawOf(standFoot));

                // (The spells it stands through, the first frame and the last among them.)
                var spells = new List<(int from, int to)>();
                int began = -1;
                for (int f = 0; f <= n; f++)
                {
                    bool down = f < n && (f == 0 || f == n - 1 || (d.contacts[f * 2 + s] & (int)LocoContact.Down) != 0);
                    if (down && began < 0)
                    {
                        began = f;
                    }
                    else if (!down && began >= 0)
                    {
                        spells.Add((began, f - 1));
                        began = -1;
                    }
                }

                var given = new Vector3[spells.Count];
                var turned = new float[spells.Count];
                for (int i = 0; i < spells.Count; i++)
                {
                    int at = spells[i].from == 0 ? 0 : spells[i].to == n - 1 ? n - 1 : (spells[i].from + spells[i].to) / 2;
                    float u = through[at];
                    float keep = 1f - TurnDrawsIn * Mathf.Sin(Mathf.PI * u);
                    given[i] = Quaternion.Euler(0f, yaw[at], 0f) * (Vector3.Lerp(off0, off1, u) * keep);
                    turned[i] = Mathf.LerpAngle(turn0, turn1, u);
                }

                shift[s] = new Vector3[n];
                swivel[s] = new float[n];
                for (int i = 0; i < spells.Count; i++)
                {
                    for (int f = spells[i].from; f <= spells[i].to; f++)
                    {
                        shift[s][f] = given[i];
                        swivel[s][f] = turned[i];
                    }

                    if (i + 1 < spells.Count)
                    {
                        int a = spells[i].to, b = spells[i + 1].from;
                        for (int f = a + 1; f < b; f++)
                        {
                            float w = Mathf.SmoothStep(0f, 1f, (f - a) / (float)(b - a));
                            shift[s][f] = Vector3.Lerp(given[i], given[i + 1], w);
                            swivel[s][f] = Mathf.LerpAngle(turned[i], turned[i + 1], w);
                        }
                    }
                }
            }

            var data = new LocoClipData
            {
                name = name, role = d.role, loop = false, rate = d.rate, frames = n, legLength = d.legLength, duration = d.duration,
                speed = d.speed, direction = d.direction, turn = d.turn, stride = d.stride, markers = d.markers, root = d.root,
                contacts = d.contacts, phase = d.phase, kind = d.kind, set = "guard",
                about = take.Name + "'s steps on " + standClip.Name + "'s stand, under its chest, head and arms",
                points = new float[n * LocoClip.BoneCount * 3],
            };
            Vector3 hips0 = stand.Pelvis - first.Pelvis, hips1 = stand.Pelvis - last.Pelvis;
            Vector3 lean0 = stand.PelvisTurn - first.PelvisTurn, lean1 = stand.PelvisTurn - last.PelvisTurn;
            for (int f = 0; f < n; f++)
            {
                float u = Mathf.SmoothStep(0f, 1f, through[f]);
                var body = stand;
                Vector3 hipsTurned = Vector3.Lerp(lean0, lean1, u);
                body.Pelvis = t[f].Pelvis + Vector3.Lerp(hips0, hips1, u);
                // (Its sinking, as it differs from the height the take begins and ends at, taken in part.)
                float rest = stand.Pelvis.y;
                body.Pelvis.y = rest + (body.Pelvis.y - rest) * TurnSinks;
                body.PelvisTurn = t[f].PelvisTurn + hipsTurned;
                var toRoot = Quaternion.Euler(0f, -yaw[f], 0f);
                for (int s = 0; s < 2; s++)
                {
                    bool left = s == 0;
                    Vector3 ankle = left ? t[f].AnkleL : t[f].AnkleR;
                    ankle += toRoot * shift[s][f];
                    var about = Quaternion.Euler(0f, swivel[s][f], 0f);
                    Vector3 foot = about * (left ? t[f].FootL : t[f].FootR), toe = about * (left ? t[f].ToeL : t[f].ToeR);
                    // (The knee turned as the foot is, against the hips: it stays over the toes.)
                    Vector3 knee = Quaternion.Euler(0f, swivel[s][f] - hipsTurned.y, 0f) * (left ? t[f].KneeL : t[f].KneeR);
                    Vector3 kneeFirst = Quaternion.Euler(0f, swivel[s][0] - lean0.y, 0f) * (left ? first.KneeL : first.KneeR);
                    Vector3 kneeLast = Quaternion.Euler(0f, swivel[s][n - 1] - lean1.y, 0f) * (left ? last.KneeL : last.KneeR);
                    Vector3 standKnee = left ? stand.KneeL : stand.KneeR;
                    knee = Quaternion.Slerp(Quaternion.FromToRotation(kneeFirst, standKnee), Quaternion.FromToRotation(kneeLast, standKnee), u) * knee;
                    if (left)
                    {
                        body.AnkleL = ankle; body.FootL = foot; body.ToeL = toe; body.KneeL = knee.normalized;
                    }
                    else
                    {
                        body.AnkleR = ankle; body.FootR = foot; body.ToeR = toe; body.KneeR = knee.normalized;
                    }
                }

                m.Compose(body, p);
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int at = (f * LocoClip.BoneCount + i) * 3;
                    data.points[at] = p[i].x;
                    data.points[at + 1] = p[i].y;
                    data.points[at + 2] = p[i].z;
                }
            }

            return made[key] = new LocoClip(data);
        }

        /// <summary>
        /// The style's stand steps (the plan's step 4: a base that is never still, capoeira's ginga): its standing clip
        /// is a loop in which the feet step on the spot, and the walk stands in it with the feet going as the
        /// recording has them, where it holds a guard's feet where they stopped.
        /// </summary>
        public bool StandSteps;

        /// <summary>
        /// How big a stepping stand is made against its recording (1: as recorded; the scene's Combat Feel, "Ginga
        /// Size"): its steps, its hips' sway and sinking, its body's turns and its arms' swings, each counted from
        /// the middle of the loop and multiplied (<see cref="Sized"/>).
        /// </summary>
        public static float StandSize = 0.67f;

        /// <summary>How much of a stepping stand's arm swings is kept on top of <see cref="StandSize"/> (1: all; the scene's Combat Feel, "Ginga Arms").</summary>
        public static float StandArms = 1f;

        /// <summary>The takes of the ginga the capoeira style can stand in (the clip library's names), and what each is.</summary>
        public static readonly (string clip, string about)[] GingaTakes =
        {
            ("cap_ginga", "the plain ginga: a step back to each side, 3.4 s"),
            ("cap_ginga_b", "a quieter take, 2.9 s"),
            ("cap_ginga_c", "a lower take, the body turning, 3.5 s"),
            ("cap_ginga_d", "a lower take, 3.8 s"),
        };

        /// <summary>
        /// A stepping stand made bigger or smaller than its recording, as a clip the walk stands in: each part's track
        /// (<see cref="BodyTracks"/>) is taken, what it does is counted from the middle of the loop (the hips' height
        /// from where they are highest; a foot's from the ground) and multiplied, and the body put together again. A
        /// foot that stands still in the recording stands still in it. (Made once for each size.)
        /// </summary>
        public static LocoClip Sized(LocoClip clip, float size, float arms)
        {
            if (clip == null || clip.Source == null || (Mathf.Abs(size - 1f) < 0.005f && Mathf.Abs(arms - 1f) < 0.005f))
            {
                return clip;
            }

            string name = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_x{1:0.00}_{2:0.00}", clip.Name, size, arms);
            if (made.TryGetValue(name, out var sized))
            {
                return sized;
            }

            var m = BodyMeasures.OfClips;
            var d = clip.Source;
            var p = new Vector3[LocoClip.BoneCount];
            var frames = new BodyTracks[d.frames];
            for (int f = 0; f < d.frames; f++)
            {
                clip.Pose(f / d.rate, p, 1f);
                frames[f] = m.Decompose(p, f > 0 ? frames[f - 1] : (BodyTracks?)null);
            }

            // The middle of the loop: each track's mean (a way's: the mean way; a hand's turn: the mean turn).
            var mid = new BodyTracks();
            var handL = new float[4];
            var handR = new float[4];
            float top = float.MinValue, groundL = float.MaxValue, groundR = float.MaxValue;
            foreach (var t in frames)
            {
                mid.Pelvis += t.Pelvis; mid.PelvisTurn += t.PelvisTurn; mid.ChestTurn += t.ChestTurn; mid.HeadTurn += t.HeadTurn;
                mid.ClavL += t.ClavL; mid.ClavR += t.ClavR; mid.UpperL += t.UpperL; mid.UpperR += t.UpperR; mid.ForeL += t.ForeL; mid.ForeR += t.ForeR;
                mid.AnkleL += t.AnkleL; mid.AnkleR += t.AnkleR; mid.KneeL += t.KneeL; mid.KneeR += t.KneeR;
                mid.FootL += t.FootL; mid.FootR += t.FootR; mid.ToeL += t.ToeL; mid.ToeR += t.ToeR;
                handL[0] += t.HandL.x; handL[1] += t.HandL.y; handL[2] += t.HandL.z; handL[3] += t.HandL.w;
                handR[0] += t.HandR.x; handR[1] += t.HandR.y; handR[2] += t.HandR.z; handR[3] += t.HandR.w;
                top = Mathf.Max(top, t.Pelvis.y);
                groundL = Mathf.Min(groundL, t.AnkleL.y);
                groundR = Mathf.Min(groundR, t.AnkleR.y);
            }

            float k = 1f / Mathf.Max(1, frames.Length);
            mid.Pelvis *= k; mid.PelvisTurn *= k; mid.ChestTurn *= k; mid.HeadTurn *= k; mid.AnkleL *= k; mid.AnkleR *= k;
            mid.ClavL.Normalize(); mid.ClavR.Normalize(); mid.UpperL.Normalize(); mid.UpperR.Normalize(); mid.ForeL.Normalize(); mid.ForeR.Normalize();
            mid.KneeL.Normalize(); mid.KneeR.Normalize(); mid.FootL.Normalize(); mid.FootR.Normalize(); mid.ToeL.Normalize(); mid.ToeR.Normalize();
            mid.HandL = MeanTurn(handL);
            mid.HandR = MeanTurn(handR);
            float swing = size * arms;

            var data = new LocoClipData
            {
                name = name, role = d.role, loop = d.loop, rate = d.rate, frames = d.frames, legLength = d.legLength, duration = d.duration,
                speed = d.speed, direction = d.direction, turn = d.turn, stride = d.stride, markers = d.markers, root = d.root,
                contacts = d.contacts, phase = d.phase, kind = d.kind, set = d.set,
                about = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} at {1:0.00} of its size, its arms' swings at {2:0.00}", clip.Name, size, swing),
                points = new float[d.frames * LocoClip.BoneCount * 3],
            };
            for (int f = 0; f < d.frames; f++)
            {
                var t = frames[f];
                var r = t;
                r.Pelvis = new Vector3(mid.Pelvis.x + size * (t.Pelvis.x - mid.Pelvis.x), top - size * (top - t.Pelvis.y), mid.Pelvis.z + size * (t.Pelvis.z - mid.Pelvis.z));
                r.PelvisTurn = mid.PelvisTurn + size * (t.PelvisTurn - mid.PelvisTurn);
                r.ChestTurn = mid.ChestTurn + size * (t.ChestTurn - mid.ChestTurn);
                r.HeadTurn = mid.HeadTurn + size * (t.HeadTurn - mid.HeadTurn);
                r.AnkleL = new Vector3(mid.AnkleL.x + size * (t.AnkleL.x - mid.AnkleL.x), groundL + size * (t.AnkleL.y - groundL), mid.AnkleL.z + size * (t.AnkleL.z - mid.AnkleL.z));
                r.AnkleR = new Vector3(mid.AnkleR.x + size * (t.AnkleR.x - mid.AnkleR.x), groundR + size * (t.AnkleR.y - groundR), mid.AnkleR.z + size * (t.AnkleR.z - mid.AnkleR.z));
                r.KneeL = BodyTracks.Way(mid.KneeL, t.KneeL, size); r.KneeR = BodyTracks.Way(mid.KneeR, t.KneeR, size);
                r.FootL = BodyTracks.Way(mid.FootL, t.FootL, size); r.FootR = BodyTracks.Way(mid.FootR, t.FootR, size);
                r.ToeL = BodyTracks.Way(mid.ToeL, t.ToeL, size); r.ToeR = BodyTracks.Way(mid.ToeR, t.ToeR, size);
                r.ClavL = BodyTracks.Way(mid.ClavL, t.ClavL, swing); r.ClavR = BodyTracks.Way(mid.ClavR, t.ClavR, swing);
                r.UpperL = BodyTracks.Way(mid.UpperL, t.UpperL, swing); r.UpperR = BodyTracks.Way(mid.UpperR, t.UpperR, swing);
                r.ForeL = BodyTracks.Way(mid.ForeL, t.ForeL, swing); r.ForeR = BodyTracks.Way(mid.ForeR, t.ForeR, swing);
                r.HandL = Quaternion.SlerpUnclamped(mid.HandL, Quaternion.Dot(mid.HandL, t.HandL) < 0f ? new Quaternion(-t.HandL.x, -t.HandL.y, -t.HandL.z, -t.HandL.w) : t.HandL, swing);
                r.HandR = Quaternion.SlerpUnclamped(mid.HandR, Quaternion.Dot(mid.HandR, t.HandR) < 0f ? new Quaternion(-t.HandR.x, -t.HandR.y, -t.HandR.z, -t.HandR.w) : t.HandR, swing);
                m.Compose(r, p);
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int at = (f * LocoClip.BoneCount + i) * 3;
                    data.points[at] = p[i].x;
                    data.points[at + 1] = p[i].y;
                    data.points[at + 2] = p[i].z;
                }
            }

            return made[name] = new LocoClip(data);
        }

        private static Quaternion MeanTurn(float[] sum)
        {
            float length = Mathf.Sqrt(sum[0] * sum[0] + sum[1] * sum[1] + sum[2] * sum[2] + sum[3] * sum[3]);
            return length > 1e-6f ? new Quaternion(sum[0] / length, sum[1] / length, sum[2] / length, sum[3] / length) : Quaternion.identity;
        }

        /// <summary>The standard style: the sword and shield pack's guard, as the fight has moved until now.</summary>
        public static readonly MoveStyle Standard = new MoveStyle { Name = "Standard", Prefix = "sns_", JogPrefix = "jog_", Profile = "guard" };

        /// <summary>
        /// The capoeira style (the plan's step 4): it stands in the ginga, never still, one foot stepping back and then
        /// the other, the hips swaying under it; to go anywhere it walks and jogs as the standard style does.
        /// </summary>
        public static readonly MoveStyle Capoeira = MakeCapoeira();

        private static MoveStyle MakeCapoeira()
        {
            var style = new MoveStyle { Name = "Capoeira", Prefix = "sns_", JogPrefix = "jog_", StandSteps = true, Profile = "guard" };
            style.Named[Job.Stand] = GingaTakes[0].clip;
            return style;
        }

        /// <summary>The take of the ginga the capoeira style stands in (<see cref="GingaTakes"/>).</summary>
        public static int GingaTake
        {
            get
            {
                for (int i = 0; i < GingaTakes.Length; i++)
                {
                    if (Capoeira.Named[Job.Stand] == GingaTakes[i].clip)
                    {
                        return i;
                    }
                }

                return 0;
            }
            set => Capoeira.Named[Job.Stand] = GingaTakes[Mathf.Clamp(value, 0, GingaTakes.Length - 1)].clip;
        }

        private static readonly Dictionary<string, MoveStyle> byPrefix = new Dictionary<string, MoveStyle> { { "sns_", Standard } };

        /// <summary>The style a guard's clips make (by their prefix: "sns_" is <see cref="Standard"/>; "lg_", the Large's two-handed guard).</summary>
        public static MoveStyle Of(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return null;
            }

            if (!byPrefix.TryGetValue(prefix, out var style))
            {
                byPrefix[prefix] = style = new MoveStyle { Name = prefix.TrimEnd('_'), Prefix = prefix, Profile = prefix == "sw_" ? "sword" : null };
            }

            return style;
        }
    }
}
