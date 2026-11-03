using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>One clip as baked (Tools/Mocap/Locomotion/loco_bake.py), read from Resources/Locomotion/LocoClips.json.</summary>
    [Serializable]
    public class LocoClipData
    {
        public string name;
        public string mirrorOf;
        public string role;
        public bool loop;
        public float rate;
        public int frames;
        public float legLength;
        public float duration;
        public float speed;
        public float direction;
        public float turn;
        public float stride;
        public int[] markers;
        public float[] root;
        public float[] points;
        public int[] contacts;
        public float[] phase;
        public string about;
        public string kind;
        public string set;
    }

    [Serializable]
    public class LocoClipFile
    {
        public string about;
        public string[] bones;
        public LocoClipData[] clips;
    }

    /// <summary>The bones a clip carries, in the baked order.</summary>
    public enum LocoBone
    {
        Hips, Spine, Spine1, Spine2, Neck, Head, HeadTop, LeftEye, RightEye,
        LeftShoulder, LeftArm, LeftForeArm, LeftHand, LeftHandIndex, LeftHandMiddle, LeftHandPinky,
        RightShoulder, RightArm, RightForeArm, RightHand, RightHandIndex, RightHandMiddle, RightHandPinky,
        LeftUpLeg, LeftLeg, LeftFoot, LeftToeBase, LeftToeEnd,
        RightUpLeg, RightLeg, RightFoot, RightToeBase, RightToeEnd,
    }

    /// <summary>A foot's contact with the ground in a clip frame (bits): its heel, its toe, either.</summary>
    [Flags]
    public enum LocoContact
    {
        None = 0,
        Heel = 1,
        Toe = 2,
        Down = 4,
    }

    /// <summary>
    /// A locomotion clip, ready to sample: every bone's place against the clip's root (x right, y up,
    /// z forward, in leg lengths), the root's own track (on the ground, in leg lengths, and its facing),
    /// the feet's contacts, and for a loop its gait phase (the left foot lands at 0, the right at 0.5).
    /// </summary>
    public sealed class LocoClip
    {
        public const int BoneCount = 33;

        public readonly string Name;
        public readonly string Role;

        /// <summary>A strike's kind and height ("forehand high"); empty for the rest.</summary>
        public readonly string Kind;

        /// <summary>A combat clip (a guard, a guard's footwork, a strike): its hands hold the weapons themselves.</summary>
        public readonly bool IsGuard;

        /// <summary>A strike's marks (frames in the clip): its contact, and from when a next strike may follow; empty for the rest.</summary>
        public readonly int ContactFrame = -1, LinkFrom = -1;
        public readonly bool Loop;
        public readonly float Rate;
        public readonly int Frames;
        public readonly float Duration;

        /// <summary>Speed (leg lengths a second), the way it travels against its facing (degrees, + right), its turn (degrees), and a loop's stride (leg lengths a cycle).</summary>
        public readonly float Speed, Direction, Turn, Stride;

        /// <summary>A loop's frames where the left and the right foot land (none: no phase).</summary>
        public readonly int LeftLands = -1, RightLands = -1;

        private readonly Vector3[] points;
        private readonly Vector2[] root;
        private readonly float[] yaw;
        private readonly LocoContact[] contacts;
        private readonly float[] phase;
        private readonly Vector2 loopDrift;

        public bool HasPhase => LeftLands >= 0 && RightLands >= 0;

        /// <summary>A loop's share of its cycle each foot is down for, from its landing (left, right).</summary>
        public readonly float[] Duty = new float[2];

        public LocoClip(LocoClipData d)
        {
            Name = d.name;
            Role = d.role;
            Kind = d.kind ?? "";
            IsGuard = d.set == "guard";
            if (Role == "strike" && d.markers != null && d.markers.Length == 2)
            {
                ContactFrame = d.markers[0];
                LinkFrom = d.markers[1];
            }
            Loop = d.loop;
            Rate = d.rate;
            Frames = d.frames;
            Duration = d.duration;
            Speed = d.speed;
            Direction = d.direction;
            Turn = d.turn;
            Stride = d.stride;
            points = new Vector3[Frames * BoneCount];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = new Vector3(d.points[3 * i], d.points[3 * i + 1], d.points[3 * i + 2]);
            }

            root = new Vector2[Frames];
            yaw = new float[Frames];
            for (int i = 0; i < Frames; i++)
            {
                root[i] = new Vector2(d.root[3 * i], d.root[3 * i + 1]);
                yaw[i] = d.root[3 * i + 2];
            }

            contacts = new LocoContact[Frames * 2];
            for (int i = 0; i < contacts.Length; i++)
            {
                contacts[i] = (LocoContact)d.contacts[i];
            }

            phase = d.phase != null && d.phase.Length == Frames ? d.phase : null;
            if (d.markers != null && d.markers.Length == 2 && phase != null && Role != "strike")
            {
                LeftLands = d.markers[0];
                RightLands = d.markers[1];
            }

            // Each foot's stance in the gait phase: it lands at 0 (left) or 0.5 (right) and lifts at
            // LiftPhase (the share of the cycle it is down for, on from its landing, is its duty).
            for (int f = 0; f < 2; f++)
            {
                Duty[f] = 0.6f;
                if (LeftLands < 0)
                {
                    continue;
                }

                int land = f == 0 ? LeftLands : RightLands;
                int down = 0;
                while (down < Frames && (contacts[((land + down) % Frames) * 2 + f] & LocoContact.Down) != 0)
                {
                    down++;
                }

                int lift = (land + down) % Frames;
                float landPhase = f == 0 ? 0f : 0.5f;
                Duty[f] = Mathf.Clamp(Mathf.Repeat(phase[lift] - landPhase, 1f), 0.05f, 0.95f);
            }

            // (A loop's root goes on by the same drift each time round.)
            loopDrift = Loop && Frames > 1 ? (root[Frames - 1] - root[0]) * Frames / (Frames - 1) : Vector2.zero;
        }

        /// <summary>A loop's time wrapped into its length; a one-shot's held at its ends.</summary>
        public float Wrap(float time) => Loop ? Mathf.Repeat(time, Duration) : Mathf.Clamp(time, 0f, Duration);

        private void Frame(float time, out int a, out int b, out float t)
        {
            float f = Wrap(time) * Rate;
            if (Loop)
            {
                a = Mathf.FloorToInt(f) % Frames;
                b = (a + 1) % Frames;
            }
            else
            {
                a = Mathf.Clamp(Mathf.FloorToInt(f), 0, Frames - 1);
                b = Mathf.Min(a + 1, Frames - 1);
            }

            t = Mathf.Clamp01(f - Mathf.Floor(f));
        }

        /// <summary>A bone's place against the root at a time (leg lengths).</summary>
        public Vector3 Point(LocoBone bone, float time)
        {
            Frame(time, out int a, out int b, out float t);
            return Vector3.Lerp(points[a * BoneCount + (int)bone], points[b * BoneCount + (int)bone], t);
        }

        /// <summary>Some bones' places at a time, into <paramref name="into"/> (leg lengths), scaled.</summary>
        public void Pose(float time, Vector3[] into, float scale, LocoBone[] bones)
        {
            Frame(time, out int a, out int b, out float t);
            foreach (var bone in bones)
            {
                int i = (int)bone;
                into[i] = Vector3.Lerp(points[a * BoneCount + i], points[b * BoneCount + i], t) * scale;
            }
        }

        /// <summary>Every bone's place at a time, into <paramref name="into"/> (leg lengths), scaled.</summary>
        public void Pose(float time, Vector3[] into, float scale)
        {
            Frame(time, out int a, out int b, out float t);
            for (int i = 0; i < BoneCount; i++)
            {
                into[i] = Vector3.Lerp(points[a * BoneCount + i], points[b * BoneCount + i], t) * scale;
            }
        }

        /// <summary>A foot's contact at a time (the nearer frame's).</summary>
        public LocoContact Contact(int foot, float time)
        {
            Frame(time, out int a, out int b, out float t);
            return contacts[(t < 0.5f ? a : b) * 2 + foot];
        }

        /// <summary>The root at a time: its place on the ground (leg lengths, the clip's own axes) and facing (degrees); a loop's goes on round after round.</summary>
        public void Root(float time, out Vector2 place, out float facing)
        {
            Frame(time, out int a, out int b, out float t);
            place = Vector2.Lerp(root[a], root[b], t);
            if (Loop && b < a)
            {
                place = Vector2.Lerp(root[a], root[b] + loopDrift, t);
            }

            if (Loop)
            {
                place += loopDrift * Mathf.Floor(time / Duration);
            }

            facing = Mathf.LerpAngle(yaw[a], yaw[b], t);
        }

        /// <summary>
        /// How the root moves from one time to a later one, in the root's own frame at the first time
        /// (leg lengths; x right, y forward), and how far it turns (degrees).
        /// </summary>
        public void RootMotion(float from, float to, out Vector2 moved, out float turned)
        {
            Root(from, out var a, out float ya);
            Root(to, out var b, out float yb);
            Vector2 d = b - a;
            float r = -ya * Mathf.Deg2Rad;
            float c = Mathf.Cos(r), s = Mathf.Sin(r);
            moved = new Vector2(c * d.x - s * d.y, s * d.x + c * d.y);
            turned = Mathf.DeltaAngle(ya, yb);
        }

        /// <summary>A loop's gait phase at a time (0 to 1).</summary>
        public float PhaseAt(float time)
        {
            if (phase == null)
            {
                return 0f;
            }

            Frame(time, out int a, out int b, out float t);
            float pa = phase[a], pb = phase[b];
            if (pb < pa)
            {
                pb += 1f;
            }

            return Mathf.Repeat(Mathf.Lerp(pa, pb, t), 1f);
        }

        /// <summary>The time in a loop at a gait phase (the left foot lands at 0, the right at 0.5; evened out between).</summary>
        public float TimeAtPhase(float gaitPhase)
        {
            if (!HasPhase)
            {
                return gaitPhase * Duration;
            }

            float p = Mathf.Repeat(gaitPhase, 1f);
            int spanLR = (RightLands - LeftLands + Frames) % Frames;
            int spanRL = (LeftLands - RightLands + Frames) % Frames;
            if (spanLR == 0) spanLR = Frames;
            if (spanRL == 0) spanRL = Frames;
            float frame = p < 0.5f ? LeftLands + p / 0.5f * spanLR : RightLands + (p - 0.5f) / 0.5f * spanRL;
            return Mathf.Repeat(frame, Frames) / Rate;
        }
    }

    /// <summary>
    /// The locomotion clips (loaded once): from Resources in the game, or straight from the file
    /// under Assets in the headless runner.
    /// </summary>
    public static class LocoLibrary
    {
        /// <summary>
        /// The Large's own two-handed clips (lg_) for its guard and strikes; off (the playtest's choice for
        /// now): it uses the Medium's sword and shield clips, its club in the sword hand, the shield hand empty.
        /// </summary>
        public static bool LargeOwnClips = false;

        /// <summary>The guard and strike clips' prefix for a body ("Small", "Medium", "Large").</summary>
        public static string CombatPrefix(string body) => body == "Large" && LargeOwnClips ? "lg_" : "sns_";

        /// <summary>The Medium's strikes the Large uses (the playtest's picks: the rest look wrong on it).</summary>
        public static readonly string[] LargeStrikes = { "sns_backhand_lunge", "sns_combo_forehand", "sns_forehand" };

        /// <summary>A strike clip is one of this body's.</summary>
        public static bool StrikeFor(string body, string clip) =>
            body != "Large" || LargeOwnClips || System.Array.IndexOf(LargeStrikes, clip) >= 0;

        /// <summary>The body holds its weapon in both hands (the Large with its own two-handed clips).</summary>
        public static bool TwoHanded(string body) => body == "Large" && LargeOwnClips;

        public const string ResourcePath = "Locomotion/LocoClips";
        public const string AssetPath = "_Battler/Resources/Locomotion/LocoClips.json";

        private static Dictionary<string, LocoClip> clips;

        public static IReadOnlyDictionary<string, LocoClip> Clips
        {
            get
            {
                if (clips == null)
                {
                    Load();
                }

                return clips;
            }
        }

        public static LocoClip Get(string name) => Clips.TryGetValue(name, out var clip) ? clip : null;

        public static void Load()
        {
            clips = new Dictionary<string, LocoClip>();
            // (The locomotion's clips, and the combat's guards and strikes beside them.)
            if (!LoadFile(ResourcePath, AssetPath))
            {
                Debug.LogWarning("Locomotion: no clip library at Resources/" + ResourcePath);
            }

            LoadFile(CombatResourcePath, CombatAssetPath);
        }

        public const string CombatResourcePath = "Locomotion/CombatClips";
        public const string CombatAssetPath = "_Battler/Resources/Locomotion/CombatClips.json";

        private static bool LoadFile(string resource, string asset)
        {
            string text = null;
            var loaded = Resources.Load<TextAsset>(resource);
            if (loaded != null)
            {
                text = loaded.text;
            }
            else
            {
                string path = Path.Combine(Application.dataPath, asset);
                if (!File.Exists(path))
                {
                    // (Headless runs may point the data path elsewhere: the project's Assets, looked
                    // for up from where it runs.)
                    for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
                    {
                        string tryPath = Path.Combine(dir.FullName, "Assets", asset);
                        if (File.Exists(tryPath))
                        {
                            path = tryPath;
                            break;
                        }
                    }
                }

                if (File.Exists(path))
                {
                    text = File.ReadAllText(path);
                }
            }

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var file = JsonUtility.FromJson<LocoClipFile>(text);
            foreach (var d in file.clips)
            {
                clips[d.name] = new LocoClip(d);
            }

            return true;
        }
    }
}
