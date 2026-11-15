using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a body is carried through one job (standing, walking forward, a step to the left): the middle of each
    /// part's track over the job (<see cref="Mid"/>: the hips' height, the lean, where the arms are held) and how far
    /// each part goes about its middle (the hips' sway and bob, the turns, the arms' swing). A style's manner is its
    /// carriages (the plan "Movement Styles as Offshoots of the Standard Set": the manner). The units are the
    /// tracks' (<see cref="BodyTracks"/>): leg lengths against the root, degrees.
    /// </summary>
    public struct Carriage
    {
        /// <summary>The middle of each part's track. (The legs' are kept for the record: a manner does not move the feet.)</summary>
        public BodyTracks Mid;

        /// <summary>How far the hips go about their middle (the root mean square: right, up, forward; leg lengths).</summary>
        public Vector3 HipsMove;

        /// <summary>How far the hips, the chest on the hips and the head on the chest turn about their middles (pitch, yaw, roll; degrees).</summary>
        public Vector3 HipsTurnMove, ChestTurnMove, HeadTurnMove;

        /// <summary>How far each arm swings about its middle (the upper arm's and the forearm's ways; degrees) and each hand turns.</summary>
        public float ArmL, ArmR, HandL, HandR;

        /// <summary>How the spine is bent, on average (what the tracks leave out of the carriage: the back's and the neck's own curve).</summary>
        public SpineShape Spine;

        /// <summary>The carriage of some clips taken together: every frame taken apart into its tracks, each track's middle and its going about it.</summary>
        public static Carriage Of(params LocoClip[] clips) => Of(clips, null);

        /// <summary>The same over only the frames taken (a clip and its frame: true to take it): a pose the clips come back to, as the middle of the moments they are in it.</summary>
        public static Carriage Of(IReadOnlyList<LocoClip> clips, Func<LocoClip, int, bool> take)
        {
            var m = BodyMeasures.OfClips;
            var p = new Vector3[LocoClip.BoneCount];
            var frames = new List<BodyTracks>();
            var spine = new SpineShape();
            foreach (var clip in clips)
            {
                BodyTracks? last = null;
                for (int f = 0; f < clip.Frames; f++)
                {
                    clip.Pose(f / clip.Rate, p, 1f);
                    var t = m.Decompose(p, last);
                    last = t;
                    if (take != null && !take(clip, f))
                    {
                        continue;
                    }

                    frames.Add(t);
                    var s = BodyMeasures.SpineOf(p);
                    spine.Spine += s.Spine; spine.Spine1 += s.Spine1; spine.Spine2 += s.Spine2; spine.Neck += s.Neck; spine.Head += s.Head;
                }
            }

            var c = Of(frames);
            float k = 1f / Mathf.Max(1, frames.Count);
            c.Spine = new SpineShape { Spine = spine.Spine * k, Spine1 = spine.Spine1 * k, Spine2 = spine.Spine2 * k, Neck = spine.Neck * k, Head = spine.Head * k };
            return c;
        }

        /// <summary>The carriage of some moments (in order: the yaws carried on from one to the next). The spine's own bend is the body's (<see cref="BodyMeasures.OwnSpine"/>).</summary>
        public static Carriage Of(IReadOnlyList<BodyTracks> frames)
        {
            var c = new Carriage { Spine = BodyMeasures.OfClips.OwnSpine };
            var mid = new BodyTracks();
            float[] handL = new float[4], handR = new float[4];
            int n = Mathf.Max(1, frames.Count);
            foreach (var t in frames)
            {
                mid.Pelvis += t.Pelvis; mid.PelvisTurn += t.PelvisTurn; mid.ChestTurn += t.ChestTurn; mid.HeadTurn += t.HeadTurn;
                mid.ClavL += t.ClavL; mid.ClavR += t.ClavR; mid.UpperL += t.UpperL; mid.UpperR += t.UpperR; mid.ForeL += t.ForeL; mid.ForeR += t.ForeR;
                mid.AnkleL += t.AnkleL; mid.AnkleR += t.AnkleR; mid.KneeL += t.KneeL; mid.KneeR += t.KneeR;
                mid.FootL += t.FootL; mid.FootR += t.FootR; mid.ToeL += t.ToeL; mid.ToeR += t.ToeR;
                // (A hand's turn summed on the first's side: q and -q are the same turn.)
                var l = Quaternion.Dot(t.HandL, frames[0].HandL) < 0f ? new Quaternion(-t.HandL.x, -t.HandL.y, -t.HandL.z, -t.HandL.w) : t.HandL;
                var r = Quaternion.Dot(t.HandR, frames[0].HandR) < 0f ? new Quaternion(-t.HandR.x, -t.HandR.y, -t.HandR.z, -t.HandR.w) : t.HandR;
                handL[0] += l.x; handL[1] += l.y; handL[2] += l.z; handL[3] += l.w;
                handR[0] += r.x; handR[1] += r.y; handR[2] += r.z; handR[3] += r.w;
            }

            float k = 1f / n;
            mid.Pelvis *= k; mid.PelvisTurn *= k; mid.ChestTurn *= k; mid.HeadTurn *= k; mid.AnkleL *= k; mid.AnkleR *= k;
            mid.ClavL = Unit(mid.ClavL, Vector3.right); mid.ClavR = Unit(mid.ClavR, Vector3.right);
            mid.UpperL = Unit(mid.UpperL, Vector3.down); mid.UpperR = Unit(mid.UpperR, Vector3.down);
            mid.ForeL = Unit(mid.ForeL, Vector3.down); mid.ForeR = Unit(mid.ForeR, Vector3.down);
            mid.KneeL = Unit(mid.KneeL, Vector3.forward); mid.KneeR = Unit(mid.KneeR, Vector3.forward);
            mid.FootL = Unit(mid.FootL, Vector3.forward); mid.FootR = Unit(mid.FootR, Vector3.forward);
            mid.ToeL = Unit(mid.ToeL, Vector3.forward); mid.ToeR = Unit(mid.ToeR, Vector3.forward);
            mid.HandL = MeanTurn(handL);
            mid.HandR = MeanTurn(handR);
            c.Mid = mid;

            Vector3 hips = Vector3.zero, hipsTurn = Vector3.zero, chestTurn = Vector3.zero, headTurn = Vector3.zero;
            float armL = 0f, armR = 0f, hL = 0f, hR = 0f;
            foreach (var t in frames)
            {
                hips += Squared(t.Pelvis - mid.Pelvis);
                hipsTurn += Squared(t.PelvisTurn - mid.PelvisTurn);
                chestTurn += Squared(t.ChestTurn - mid.ChestTurn);
                headTurn += Squared(t.HeadTurn - mid.HeadTurn);
                float ul = Vector3.Angle(t.UpperL, mid.UpperL), fl = Vector3.Angle(t.ForeL, mid.ForeL), ur = Vector3.Angle(t.UpperR, mid.UpperR), fr = Vector3.Angle(t.ForeR, mid.ForeR);
                armL += 0.5f * (ul * ul + fl * fl);
                armR += 0.5f * (ur * ur + fr * fr);
                float al = Quaternion.Angle(t.HandL, mid.HandL), ar = Quaternion.Angle(t.HandR, mid.HandR);
                hL += al * al;
                hR += ar * ar;
            }

            c.HipsMove = Root(hips * k);
            c.HipsTurnMove = Root(hipsTurn * k);
            c.ChestTurnMove = Root(chestTurn * k);
            c.HeadTurnMove = Root(headTurn * k);
            c.ArmL = Mathf.Sqrt(armL * k); c.ArmR = Mathf.Sqrt(armR * k); c.HandL = Mathf.Sqrt(hL * k); c.HandR = Mathf.Sqrt(hR * k);
            return c;
        }

        /// <summary>Some carriages taken together, each by its share (the shares add up to one): what a body doing a bit of each job is carried as.</summary>
        public static Carriage Mix(Carriage[] carriages, float[] shares)
        {
            var c = new Carriage();
            float[] handL = new float[4], handR = new float[4];
            Quaternion firstL = Quaternion.identity, firstR = Quaternion.identity;
            bool any = false;
            for (int i = 0; i < carriages.Length; i++)
            {
                float w = shares[i];
                if (w <= 0f)
                {
                    continue;
                }

                var o = carriages[i];
                var t = o.Mid;
                if (!any)
                {
                    firstL = t.HandL;
                    firstR = t.HandR;
                    any = true;
                }

                c.Mid.Pelvis += w * t.Pelvis; c.Mid.PelvisTurn += w * t.PelvisTurn; c.Mid.ChestTurn += w * t.ChestTurn; c.Mid.HeadTurn += w * t.HeadTurn;
                c.Mid.ClavL += w * t.ClavL; c.Mid.ClavR += w * t.ClavR; c.Mid.UpperL += w * t.UpperL; c.Mid.UpperR += w * t.UpperR; c.Mid.ForeL += w * t.ForeL; c.Mid.ForeR += w * t.ForeR;
                c.Mid.AnkleL += w * t.AnkleL; c.Mid.AnkleR += w * t.AnkleR;
                float sl = Quaternion.Dot(t.HandL, firstL) < 0f ? -w : w, sr = Quaternion.Dot(t.HandR, firstR) < 0f ? -w : w;
                handL[0] += sl * t.HandL.x; handL[1] += sl * t.HandL.y; handL[2] += sl * t.HandL.z; handL[3] += sl * t.HandL.w;
                handR[0] += sr * t.HandR.x; handR[1] += sr * t.HandR.y; handR[2] += sr * t.HandR.z; handR[3] += sr * t.HandR.w;
                c.Spine.Spine += w * o.Spine.Spine; c.Spine.Spine1 += w * o.Spine.Spine1; c.Spine.Spine2 += w * o.Spine.Spine2; c.Spine.Neck += w * o.Spine.Neck; c.Spine.Head += w * o.Spine.Head;
                c.HipsMove += w * o.HipsMove; c.HipsTurnMove += w * o.HipsTurnMove; c.ChestTurnMove += w * o.ChestTurnMove; c.HeadTurnMove += w * o.HeadTurnMove;
                c.ArmL += w * o.ArmL; c.ArmR += w * o.ArmR; c.HandL += w * o.HandL; c.HandR += w * o.HandR;
            }

            c.Mid.ClavL = Unit(c.Mid.ClavL, Vector3.left); c.Mid.ClavR = Unit(c.Mid.ClavR, Vector3.right);
            c.Mid.UpperL = Unit(c.Mid.UpperL, Vector3.down); c.Mid.UpperR = Unit(c.Mid.UpperR, Vector3.down);
            c.Mid.ForeL = Unit(c.Mid.ForeL, Vector3.down); c.Mid.ForeR = Unit(c.Mid.ForeR, Vector3.down);
            c.Mid.KneeL = c.Mid.KneeR = c.Mid.FootL = c.Mid.FootR = c.Mid.ToeL = c.Mid.ToeR = Vector3.forward;
            c.Mid.HandL = MeanTurn(handL);
            c.Mid.HandR = MeanTurn(handR);
            return c;
        }

        private static Vector3 Unit(Vector3 v, Vector3 otherwise) => v.sqrMagnitude > 1e-10f ? v.normalized : otherwise;
        private static Vector3 Squared(Vector3 v) => new Vector3(v.x * v.x, v.y * v.y, v.z * v.z);
        private static Vector3 Root(Vector3 v) => new Vector3(Mathf.Sqrt(v.x), Mathf.Sqrt(v.y), Mathf.Sqrt(v.z));

        internal static Quaternion MeanTurn(float[] sum)
        {
            float length = Mathf.Sqrt(sum[0] * sum[0] + sum[1] * sum[1] + sum[2] * sum[2] + sum[3] * sum[3]);
            return length > 1e-6f ? new Quaternion(sum[0] / length, sum[1] / length, sum[2] / length, sum[3] / length) : Quaternion.identity;
        }
    }

    /// <summary>One job's carriage as the profile file has it: every number named, so it can be read and set by hand.</summary>
    [Serializable]
    public class StyleCarriageData
    {
        /// <summary>The job (<see cref="MoveStyle.Job"/>'s name) and the recording it was measured on.</summary>
        public string job, from;

        /// <summary>Where the hips are against the root (right, up, forward; leg lengths: a leg is the thigh and the shin, so standing straight they are about 1.16 up).</summary>
        public float[] hips;

        /// <summary>The hips' turn, the chest's on the hips, the head's on the chest (pitch + forward, yaw + to the right, roll; degrees).</summary>
        public float[] hipsTurn, chestTurn, headTurn;

        /// <summary>Which way each arm bone points on the chest (right, up, forward; a unit vector).</summary>
        public float[] collarL, collarR, upperArmL, upperArmR, forearmL, forearmR;

        /// <summary>Each hand's turn on the chest (a quaternion: x, y, z, w).</summary>
        public float[] handL, handR;

        /// <summary>How the spine is bent: where each joint sits on the one below (the first joint on the hips, the next two, the neck on the chest, the head on the neck; right, up, forward, five times).</summary>
        public float[] spine;

        /// <summary>Where the ankles are on average (for the record: a manner does not move the feet).</summary>
        public float[] ankleL, ankleR;

        /// <summary>How far each goes about its middle (root mean square): the hips (right, up, forward), the turns (pitch, yaw, roll), the arms and the hands (left, right; degrees).</summary>
        public float[] hipsMove, hipsTurnMove, chestTurnMove, headTurnMove, armsMove, handsMove;
    }

    [Serializable]
    public class StyleProfileData
    {
        public string name, about;
        public StyleCarriageData[] carriages;

        /// <summary>Poses the style comes back to, by name (its "job" is the pose's name): the places of a step pattern (the ginga's two sides), measured as carriages are.</summary>
        public StyleCarriageData[] poses;
    }

    [Serializable]
    public class StyleProfileFile
    {
        public string about;
        public StyleProfileData[] styles;
    }

    /// <summary>
    /// A movement style's profile (the plan's stage 5): its carriage for each job it was measured in, as numbers.
    /// The recordings it was measured on are its ruler; nothing of them is played. Read from the profile file
    /// (<see cref="Get"/>), which the measuring tool writes (the headless "style" mode).
    /// </summary>
    public sealed class StyleProfile
    {
        public const string ResourcePath = "Locomotion/StyleProfiles";
        public const string AssetPath = "_Battler/Resources/Locomotion/StyleProfiles.json";

        public readonly string Name, About;
        private readonly Dictionary<MoveStyle.Job, Carriage> carriages = new Dictionary<MoveStyle.Job, Carriage>();
        private readonly Dictionary<MoveStyle.Job, string> measuredOn = new Dictionary<MoveStyle.Job, string>();

        public StyleProfile(string name, string about)
        {
            Name = name;
            About = about;
        }

        private readonly Dictionary<string, Carriage> poses = new Dictionary<string, Carriage>();
        private readonly Dictionary<string, string> posesOn = new Dictionary<string, string>();

        /// <summary>A pose the style comes back to, by its name (false: it has none so called).</summary>
        public bool Pose(string name, out Carriage pose) => poses.TryGetValue(name, out pose);

        /// <summary>A named pose, measured on what is named.</summary>
        public void SetPose(string name, in Carriage pose, string from)
        {
            poses[name] = pose;
            posesOn[name] = from;
        }

        /// <summary>A job's carriage, measured on what is named.</summary>
        public void Set(MoveStyle.Job job, in Carriage carriage, string from)
        {
            carriages[job] = carriage;
            measuredOn[job] = from;
        }

        /// <summary>The profile has this job's own carriage.</summary>
        public bool Has(MoveStyle.Job job) => carriages.ContainsKey(job);

        /// <summary>The jobs it has, in the jobs' order.</summary>
        public IEnumerable<MoveStyle.Job> Jobs
        {
            get
            {
                foreach (MoveStyle.Job job in Enum.GetValues(typeof(MoveStyle.Job)))
                {
                    if (carriages.ContainsKey(job))
                    {
                        yield return job;
                    }
                }
            }
        }

        public string MeasuredOn(MoveStyle.Job job) => measuredOn.TryGetValue(job, out string name) ? name : "";

        /// <summary>A job's carriage: its own, or the nearest it has (a way of going falls back on walking forward, and that on standing).</summary>
        public bool For(MoveStyle.Job job, out Carriage carriage)
        {
            if (carriages.TryGetValue(job, out carriage))
            {
                return true;
            }

            if (job != MoveStyle.Job.Stand && job != MoveStyle.Job.WalkForward && carriages.TryGetValue(MoveStyle.Job.WalkForward, out carriage))
            {
                return true;
            }

            return carriages.TryGetValue(MoveStyle.Job.Stand, out carriage);
        }

        private static Dictionary<string, StyleProfile> profiles;

        /// <summary>A style's profile by its name in the file ("plain": the relaxed walk; "guard": the sword and shield's; null: not there).</summary>
        public static StyleProfile Get(string name)
        {
            if (profiles == null)
            {
                Load();
            }

            return profiles.TryGetValue(name, out var profile) ? profile : null;
        }

        /// <summary>Every profile in the file.</summary>
        public static IEnumerable<StyleProfile> All
        {
            get
            {
                if (profiles == null)
                {
                    Load();
                }

                return profiles.Values;
            }
        }

        /// <summary>Reads the profile file again.</summary>
        public static void Load()
        {
            profiles = new Dictionary<string, StyleProfile>();
            string text = LocoLibrary.ReadText(ResourcePath, AssetPath);
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("Movement styles: no profile file at Resources/" + ResourcePath + " (a style laid over the standard set does nothing without it).");
                return;
            }

            var file = JsonUtility.FromJson<StyleProfileFile>(text);
            if (file?.styles == null)
            {
                return;
            }

            foreach (var data in file.styles)
            {
                var profile = new StyleProfile(data.name, data.about);
                foreach (var c in data.carriages ?? Array.Empty<StyleCarriageData>())
                {
                    if (Enum.TryParse(c.job, out MoveStyle.Job job))
                    {
                        profile.carriages[job] = FromData(c);
                        profile.measuredOn[job] = c.from ?? "";
                    }
                }

                foreach (var c in data.poses ?? Array.Empty<StyleCarriageData>())
                {
                    profile.poses[c.job] = FromData(c);
                    profile.posesOn[c.job] = c.from ?? "";
                }

                profiles[data.name] = profile;
            }
        }

        /// <summary>The profile as the file has it.</summary>
        public StyleProfileData ToData()
        {
            var list = new List<StyleCarriageData>();
            foreach (var job in Jobs)
            {
                list.Add(ToData(carriages[job], job.ToString(), MeasuredOn(job)));
            }

            var named = new List<StyleCarriageData>();
            foreach (var pair in poses)
            {
                named.Add(ToData(pair.Value, pair.Key, posesOn[pair.Key]));
            }

            return new StyleProfileData { name = Name, about = About, carriages = list.ToArray(), poses = named.ToArray() };
        }

        private static StyleCarriageData ToData(in Carriage c, string name, string from)
        {
            {
                var t = c.Mid;
                return (new StyleCarriageData
                {
                    job = name, from = from,
                    hips = A(t.Pelvis), hipsTurn = A(t.PelvisTurn), chestTurn = A(t.ChestTurn), headTurn = A(t.HeadTurn),
                    collarL = A(t.ClavL), collarR = A(t.ClavR), upperArmL = A(t.UpperL), upperArmR = A(t.UpperR), forearmL = A(t.ForeL), forearmR = A(t.ForeR),
                    handL = new[] { t.HandL.x, t.HandL.y, t.HandL.z, t.HandL.w }, handR = new[] { t.HandR.x, t.HandR.y, t.HandR.z, t.HandR.w },
                    ankleL = A(t.AnkleL), ankleR = A(t.AnkleR),
                    spine = new[]
                    {
                        c.Spine.Spine.x, c.Spine.Spine.y, c.Spine.Spine.z, c.Spine.Spine1.x, c.Spine.Spine1.y, c.Spine.Spine1.z, c.Spine.Spine2.x, c.Spine.Spine2.y, c.Spine.Spine2.z,
                        c.Spine.Neck.x, c.Spine.Neck.y, c.Spine.Neck.z, c.Spine.Head.x, c.Spine.Head.y, c.Spine.Head.z,
                    },
                    hipsMove = A(c.HipsMove), hipsTurnMove = A(c.HipsTurnMove), chestTurnMove = A(c.ChestTurnMove), headTurnMove = A(c.HeadTurnMove),
                    armsMove = new[] { c.ArmL, c.ArmR }, handsMove = new[] { c.HandL, c.HandR },
                });
            }
        }

        private static Carriage FromData(StyleCarriageData d)
        {
            var c = new Carriage();
            c.Mid.Pelvis = V(d.hips); c.Mid.PelvisTurn = V(d.hipsTurn); c.Mid.ChestTurn = V(d.chestTurn); c.Mid.HeadTurn = V(d.headTurn);
            c.Mid.ClavL = W(d.collarL, Vector3.left); c.Mid.ClavR = W(d.collarR, Vector3.right);
            c.Mid.UpperL = W(d.upperArmL, Vector3.down); c.Mid.UpperR = W(d.upperArmR, Vector3.down);
            c.Mid.ForeL = W(d.forearmL, Vector3.down); c.Mid.ForeR = W(d.forearmR, Vector3.down);
            c.Mid.HandL = Q(d.handL); c.Mid.HandR = Q(d.handR);
            c.Mid.AnkleL = V(d.ankleL); c.Mid.AnkleR = V(d.ankleR);
            c.Spine = d.spine != null && d.spine.Length >= 15
                ? new SpineShape
                {
                    Spine = new Vector3(d.spine[0], d.spine[1], d.spine[2]), Spine1 = new Vector3(d.spine[3], d.spine[4], d.spine[5]), Spine2 = new Vector3(d.spine[6], d.spine[7], d.spine[8]),
                    Neck = new Vector3(d.spine[9], d.spine[10], d.spine[11]), Head = new Vector3(d.spine[12], d.spine[13], d.spine[14]),
                }
                : BodyMeasures.OfClips.OwnSpine;
            c.Mid.KneeL = c.Mid.KneeR = c.Mid.FootL = c.Mid.FootR = c.Mid.ToeL = c.Mid.ToeR = Vector3.forward;
            c.HipsMove = V(d.hipsMove); c.HipsTurnMove = V(d.hipsTurnMove); c.ChestTurnMove = V(d.chestTurnMove); c.HeadTurnMove = V(d.headTurnMove);
            if (d.armsMove != null && d.armsMove.Length >= 2) { c.ArmL = d.armsMove[0]; c.ArmR = d.armsMove[1]; }
            if (d.handsMove != null && d.handsMove.Length >= 2) { c.HandL = d.handsMove[0]; c.HandR = d.handsMove[1]; }
            return c;
        }

        private static float[] A(Vector3 v) => new[] { v.x, v.y, v.z };
        private static Vector3 V(float[] a) => a != null && a.Length >= 3 ? new Vector3(a[0], a[1], a[2]) : Vector3.zero;
        private static Vector3 W(float[] a, Vector3 otherwise) => V(a).sqrMagnitude > 1e-8f ? V(a).normalized : otherwise;
        private static Quaternion Q(float[] a) => a != null && a.Length >= 4 ? Carriage.MeanTurn(a) : Quaternion.identity;
    }

    /// <summary>How much of a manner is laid, part by part (<see cref="StyleManner.Lay(in BodyTracks, in Carriage, in Carriage, in MannerDials)"/>).</summary>
    public struct MannerDials
    {
        /// <summary>How much of the change of carriage (0: none, 1: all): the lean, the hips' height, the spine's bend, where the arms are held.</summary>
        public float Carriage;

        /// <summary>How much of the change in how far each part goes (0: it goes as the base does, 1: as far as the laid style's).</summary>
        public float Motion;

        /// <summary>How much the chest and the head are turned to face the world as the laid style has them (0: they face as the base's; 1: as the style's). The hips are never turned on the feet: that is the stance's.</summary>
        public float Turn;

        /// <summary>How much of the arms' own carriage and motion (0: the arms are held on the chest as the base holds them; 1: as the laid style does).</summary>
        public float Arms;

        /// <summary>How much of the carriage is kept through a blow thrown from a stand (1: he strikes as leaned and sunk as he stands; 0: he comes up to the base's own carriage to strike, and goes back down after).</summary>
        public float InBlows;

        /// <summary>The dials for a blow thrown from a stand: the carriage as much as is kept through it; the arms and the hands the blow's own, hung from the chest as it made them.</summary>
        public MannerDials ForBlow() => new MannerDials { Carriage = Carriage * InBlows, Motion = Motion * InBlows, Turn = Turn, Arms = 0f, HandsKeep = 0f, HeadKeeps = HeadKeeps, InBlows = InBlows };

        /// <summary>How much the head keeps facing the world as the base has it (0: it tips with the style's own carriage of the head; 1: his eyes stay where the base has them, on his man, however the chest leans).</summary>
        public float HeadKeeps;

        /// <summary>How much the hands keep their turn in the world (0: they turn with the chest, as the laid style carries them; 1: a hand that holds a weapon goes on pointing it where the base pointed it, however the chest leans).</summary>
        public float HandsKeep;

        /// <summary>The whole of a manner, as measured: the round trip's.</summary>
        public static MannerDials Whole => new MannerDials { Carriage = 1f, Motion = 1f, Turn = 1f, Arms = 1f, HandsKeep = 0f };

        /// <summary>The same dials with the carriage, the motion and the turn each this much of what they were (a style part of the way on).</summary>
        public MannerDials Times(float amount) => new MannerDials { Carriage = Carriage * amount, Motion = Motion * amount, Turn = Turn, Arms = Arms, HandsKeep = HandsKeep, HeadKeeps = HeadKeeps, InBlows = InBlows };
    }

    /// <summary>
    /// The manner layer (the plan's steps S1 and S2): one style's carriage laid over a body that is moving as
    /// another's. The body's parts are taken as their tracks; each upper part is moved from where the base style
    /// carries it to where the laid style does, and what it does about that middle is made as big as the laid style
    /// has it. The feet stay exactly where they were, and the hips are not turned on them: the knees bend to the
    /// hips' new place.
    /// </summary>
    public static class StyleManner
    {
        /// <summary>The least and the most a part's going is multiplied by (a style that hardly moves a part against one that swings it, and the other way).</summary>
        public const float LeastGain = 0.1f, MostGain = 2.5f;

        /// <summary>The most the hips' own going (their sway, bob) is multiplied by: the feet stay, so more of it would pull the legs straight.</summary>
        public const float MostHipsGain = 1.5f;

        /// <summary>
        /// The style laid over the standard set in the fight (the plan's step S2; the scene's Combat Feel, "Capoeira
        /// Style Amount"): its profile's name, how much of it (0: none, the standard fighter exactly as he is; 1:
        /// all), and how much of each part of it.
        /// </summary>
        /// <summary>Set (the headless run): what the walk lays each step is written to it.</summary>
        public static System.Action<string> Trace;

        public static string Laid = "capoeira";
        public static float Amount = 0f;
        public static MannerDials Dials = new MannerDials { Carriage = 1f, Motion = 0.5f, Turn = 0.5f, Arms = 1f, HandsKeep = 1f, HeadKeeps = 1f, InBlows = 0.5f };

        /// <summary>A moment's tracks with a manner laid on: from the base's carriage to the laid style's, as much of each part as the dials say.</summary>
        public static BodyTracks Lay(in BodyTracks t, in Carriage from, in Carriage to, in MannerDials dials)
        {
            var r = t;
            float carriage = dials.Carriage, motion = dials.Motion;
            Vector3 one = Vector3.one;
            // (What a part does about its middle is made bigger or smaller only as far as the job usually takes it
            // (MostCounted times its usual going): a body far from the job's middle is on its way between two jobs,
            // and that way is not the style's to stretch.)
            Vector3 gh = Gains(to.HipsMove, from.HipsMove, motion, 0.002f, MostHipsGain);
            r.Pelvis = t.Pelvis + carriage * (to.Mid.Pelvis - from.Mid.Pelvis) + Vector3.Scale(gh - one, Within(t.Pelvis - from.Mid.Pelvis, from.HipsMove));
            // The turns. First what each does about its middle, made bigger or smaller.
            Vector3 hipsTurn = t.PelvisTurn + Vector3.Scale(Gains(to.HipsTurnMove, from.HipsTurnMove, motion, 0.3f, MostGain) - one, Within(t.PelvisTurn - from.Mid.PelvisTurn, from.HipsTurnMove));
            Vector3 chestTurn = t.ChestTurn + Vector3.Scale(Gains(to.ChestTurnMove, from.ChestTurnMove, motion, 0.3f, MostGain) - one, Within(t.ChestTurn - from.Mid.ChestTurn, from.ChestTurnMove));
            Vector3 headTurn = t.HeadTurn + Vector3.Scale(Gains(to.HeadTurnMove, from.HeadTurnMove, motion, 0.3f, MostGain) - one, Within(t.HeadTurn - from.Mid.HeadTurn, from.HeadTurnMove));
            // Then the carriage, against the root: where he faces, not where his hips happen to point. Each of the
            // hips, the chest and the head is tipped from how the base tips it to how the style does (its own up,
            // carried from the one to the other): a style that leans toward its man leans him toward his, however
            // his stance has turned his hips. The hips are not turned on the feet: that is the stance's. The chest
            // and the head are turned to face as the style's do, as much as is asked.
            Quaternion hipsNow = Quaternion.Euler(hipsTurn), chestNow = hipsNow * Quaternion.Euler(chestTurn), headNow = chestNow * Quaternion.Euler(headTurn);
            Quaternion hipsFrom = Quaternion.Euler(from.Mid.PelvisTurn), chestFrom = hipsFrom * Quaternion.Euler(from.Mid.ChestTurn), headFrom = chestFrom * Quaternion.Euler(from.Mid.HeadTurn);
            Quaternion hipsTo = Quaternion.Euler(to.Mid.PelvisTurn), chestTo = hipsTo * Quaternion.Euler(to.Mid.ChestTurn), headTo = chestTo * Quaternion.Euler(to.Mid.HeadTurn);
            Quaternion hipsLaid = Tipped(hipsFrom, hipsTo, carriage) * hipsNow;
            Quaternion chestLaid = Quaternion.AngleAxis(carriage * dials.Turn * Mathf.DeltaAngle(Heading(chestFrom), Heading(chestTo)), Vector3.up) * Tipped(chestFrom, chestTo, carriage) * chestNow;
            Quaternion headAsStyle = Quaternion.AngleAxis(carriage * dials.Turn * Mathf.DeltaAngle(Heading(headFrom), Heading(headTo)), Vector3.up) * Tipped(headFrom, headTo, carriage) * headNow;
            // (His eyes on his man: the head kept facing the world as the base has it, however the chest under it leans.)
            Quaternion headLaid = Quaternion.SlerpUnclamped(headAsStyle, Near(headNow, headAsStyle), dials.HeadKeeps);
            r.PelvisTurn = BodyMeasures.EulerOf(hipsLaid, hipsTurn.y);
            r.ChestTurn = BodyMeasures.EulerOf(Quaternion.Inverse(hipsLaid) * chestLaid, chestTurn.y);
            r.HeadTurn = BodyMeasures.EulerOf(Quaternion.Inverse(chestLaid) * headLaid, headTurn.y);
            float armsCarriage = carriage * dials.Arms, armsMotion = motion * dials.Arms;
            float gl = Gain(to.ArmL, from.ArmL, armsMotion, 0.5f, MostGain), gr = Gain(to.ArmR, from.ArmR, armsMotion, 0.5f, MostGain);
            float al = MostCounted * from.ArmL, ar = MostCounted * from.ArmR;
            r.ClavL = Carry(t.ClavL, from.Mid.ClavL, to.Mid.ClavL, armsCarriage, gl, al); r.ClavR = Carry(t.ClavR, from.Mid.ClavR, to.Mid.ClavR, armsCarriage, gr, ar);
            r.UpperL = Carry(t.UpperL, from.Mid.UpperL, to.Mid.UpperL, armsCarriage, gl, al); r.UpperR = Carry(t.UpperR, from.Mid.UpperR, to.Mid.UpperR, armsCarriage, gr, ar);
            r.ForeL = Carry(t.ForeL, from.Mid.ForeL, to.Mid.ForeL, armsCarriage, gl, al); r.ForeR = Carry(t.ForeR, from.Mid.ForeR, to.Mid.ForeR, armsCarriage, gr, ar);
            r.HandL = Carry(t.HandL, from.Mid.HandL, to.Mid.HandL, armsCarriage, Gain(to.HandL, from.HandL, armsMotion, 0.5f, MostGain), MostCounted * from.HandL);
            r.HandR = Carry(t.HandR, from.Mid.HandR, to.Mid.HandR, armsCarriage, Gain(to.HandR, from.HandR, armsMotion, 0.5f, MostGain), MostCounted * from.HandR);
            Quaternion hipsWere = Quaternion.Euler(t.PelvisTurn), hipsAre = Quaternion.Euler(r.PelvisTurn);
            if (dials.HandsKeep > 0f)
            {
                // (A hand that holds a weapon goes on pointing it where it pointed: its turn is kept on the chest, and the chest has leaned.)
                Quaternion keepHand = Quaternion.Inverse(hipsAre * Quaternion.Euler(r.ChestTurn)) * (hipsWere * Quaternion.Euler(t.ChestTurn));
                r.HandL = Quaternion.SlerpUnclamped(r.HandL, Near(keepHand * t.HandL, r.HandL), dials.HandsKeep);
                r.HandR = Quaternion.SlerpUnclamped(r.HandR, Near(keepHand * t.HandR, r.HandR), dials.HandsKeep);
            }

            // (The knees go on pointing where they pointed: their way is kept on the hips, and the hips have turned.)
            Quaternion keep = Quaternion.Inverse(hipsAre) * hipsWere;
            r.KneeL = keep * t.KneeL;
            r.KneeR = keep * t.KneeR;
            return r;
        }

        /// <summary>The turn that tips a part from how one carriage tips it to how another does (its own up, carried from the one to the other), as much of it as asked. It turns about a level line: nothing is turned to face another way.</summary>
        private static Quaternion Tipped(Quaternion from, Quaternion to, float share) =>
            Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.FromToRotation(from * Vector3.up, to * Vector3.up), share);

        /// <summary>Which way a part faces over the ground (degrees, + to the right).</summary>
        private static float Heading(Quaternion q)
        {
            Vector3 f = q * Vector3.forward;
            return Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg;
        }

        private static Quaternion Near(Quaternion q, Quaternion to) => Quaternion.Dot(q, to) < 0f ? new Quaternion(-q.x, -q.y, -q.z, -q.w) : q;

        /// <summary>How much a part's going is multiplied by: the laid style's against the base's, as much of that change as is asked. (A part the base holds still is left as it is: there is nothing to make bigger.)</summary>
        private static float Gain(float to, float from, float motion, float still, float most) =>
            from < still ? 1f : 1f + motion * (Mathf.Clamp(to / from, LeastGain, most) - 1f);

        private static Vector3 Gains(Vector3 to, Vector3 from, float motion, float still, float most) =>
            new Vector3(Gain(to.x, from.x, motion, still, most), Gain(to.y, from.y, motion, still, most), Gain(to.z, from.z, motion, still, most));

        /// <summary>How far from its middle a part's going is counted, in its usual goings (root mean square): past that it is carried as it is.</summary>
        public const float MostCounted = 2f;

        private static Vector3 Within(Vector3 off, Vector3 usual) => new Vector3(
            Mathf.Clamp(off.x, -MostCounted * usual.x, MostCounted * usual.x), Mathf.Clamp(off.y, -MostCounted * usual.y, MostCounted * usual.y), Mathf.Clamp(off.z, -MostCounted * usual.z, MostCounted * usual.z));

        /// <summary>A turn, no further than so many degrees.</summary>
        private static Quaternion Within(Quaternion does, float most)
        {
            float angle = Quaternion.Angle(Quaternion.identity, does);
            return angle > most && angle > 1e-3f ? Quaternion.SlerpUnclamped(Quaternion.identity, does, most / angle) : does;
        }

        /// <summary>A way carried from one middle to another: what it does about the base's middle (as far as <paramref name="counted"/> degrees of it), made bigger or smaller, about the laid style's.</summary>
        private static Vector3 Carry(Vector3 way, Vector3 fromMid, Vector3 toMid, float carriage, float gain, float counted)
        {
            Quaternion does = Quaternion.FromToRotation(fromMid, way);
            Vector3 mid = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.FromToRotation(fromMid, toMid), carriage) * fromMid;
            Vector3 v = Quaternion.SlerpUnclamped(Quaternion.identity, Within(does, counted), gain - 1f) * (does * mid);
            return v.sqrMagnitude > 1e-10f ? v.normalized : way;
        }

        private static Quaternion Carry(Quaternion turn, Quaternion fromMid, Quaternion toMid, float carriage, float gain, float counted)
        {
            if (Quaternion.Dot(fromMid, toMid) < 0f)
            {
                toMid = new Quaternion(-toMid.x, -toMid.y, -toMid.z, -toMid.w);
            }

            Quaternion does = turn * Quaternion.Inverse(fromMid);
            if (does.w < 0f)
            {
                does = new Quaternion(-does.x, -does.y, -does.z, -does.w);
            }

            return Quaternion.SlerpUnclamped(Quaternion.identity, Within(does, counted), gain - 1f) * does * Quaternion.SlerpUnclamped(fromMid, toMid, carriage);
        }

        private static readonly LocoBone[] FootPoints =
        {
            LocoBone.LeftFoot, LocoBone.RightFoot, LocoBone.LeftToeBase, LocoBone.RightToeBase, LocoBone.LeftToeEnd, LocoBone.RightToeEnd,
        };

        /// <summary>
        /// A body's points with a manner laid on (the same frame and units as the tracks; <paramref name="scratch"/>
        /// and <paramref name="scratch2"/> are two more bodies' room). Only what the manner changes is changed:
        /// each point is moved by what the manner moves it, and the feet's points are not moved at all.
        /// </summary>
        public static void Lay(Vector3[] points, in Carriage from, in Carriage to, in MannerDials dials, BodyTracks? last, out BodyTracks tracks, Vector3[] scratch, Vector3[] scratch2)
        {
            var m = BodyMeasures.OfClips;
            tracks = m.Decompose(points, last);
            var laid = Lay(tracks, from, to, dials);
            // (The spine's own bend goes from the base's to the laid style's with the carriage.)
            m.Compose(tracks, scratch, from.Spine);
            m.Compose(laid, scratch2, SpineShape.Lerp(from.Spine, to.Spine, dials.Carriage));
            for (int i = 0; i < points.Length; i++)
            {
                points[i] += scratch2[i] - scratch[i];
            }

            foreach (var foot in FootPoints)
            {
                points[(int)foot] -= scratch2[(int)foot] - scratch[(int)foot];
            }
        }

        private static readonly Dictionary<string, LocoClip> made = new Dictionary<string, LocoClip>();

        /// <summary>
        /// A clip with a manner laid on every frame, as a clip the walk plays like the one it was made of: its travel,
        /// its feet's contacts and its stride are that clip's. (Made once for each name.)
        /// </summary>
        public static LocoClip Over(LocoClip clip, in Carriage from, in Carriage to, string name, float carriage = 1f, float motion = 1f)
        {
            if (clip == null || clip.Source == null)
            {
                return null;
            }

            name = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_{1:0.00}_{2:0.00}", name, carriage, motion);
            if (made.TryGetValue(name, out var done))
            {
                return done;
            }

            var d = clip.Source;
            var data = new LocoClipData
            {
                name = name, role = d.role, loop = d.loop, rate = d.rate, frames = d.frames, legLength = d.legLength, duration = d.duration,
                speed = d.speed, direction = d.direction, turn = d.turn, stride = d.stride, markers = d.markers, root = d.root,
                contacts = d.contacts, phase = d.phase, kind = d.kind, set = "guard",
                about = clip.Name + " with another style's manner laid on it",
                points = new float[d.frames * LocoClip.BoneCount * 3],
            };
            Vector3[] p = new Vector3[LocoClip.BoneCount], a = new Vector3[LocoClip.BoneCount], b = new Vector3[LocoClip.BoneCount];
            BodyTracks? last = null;
            for (int f = 0; f < d.frames; f++)
            {
                clip.Pose(f / d.rate, p, 1f);
                var dials = MannerDials.Whole;
                dials.Carriage = carriage;
                dials.Motion = motion;
                Lay(p, from, to, dials, last, out var tracks, a, b);
                last = tracks;
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
    }
}
