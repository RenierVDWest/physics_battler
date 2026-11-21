using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>How a <see cref="BladeStreak"/> looks (shared by every streak made with it: changed live from the lab's panel).</summary>
    [Serializable]
    public sealed class StreakSettings
    {
        /// <summary>Seconds a point of the streak lasts before it has faded away.</summary>
        public float Lifetime = 0.16f;

        /// <summary>The tip's speed turning round the base (m/s) below which nothing streaks, and at which it is fully drawn: a sword carried through a step leaves nothing.</summary>
        public float MinSpeed = 3.5f, FullSpeed = 7f;

        /// <summary>The colour at the newest end (its alpha how strong) and at the oldest, where it fades out: a pale grey near the steel's own.</summary>
        public Color Head = new Color(0.86f, 0.87f, 0.88f, 0.55f), Tail = new Color(0.62f, 0.64f, 0.66f, 0f);

        /// <summary>Curve points added between frames, so a fast swing draws an arc and not straight pieces.</summary>
        public int Smoothing = 4;

        /// <summary>Drawn at all (the lab's switch).</summary>
        public bool On = true;
    }

    /// <summary>
    /// The streak a blade's edge sweeps (from Hexamus's MotionTrail): the band between two points along it, the base
    /// and the tip, over the last fraction of a second, only while the tip turns fast round the base, fading from the
    /// newest end to the oldest and from the tip down to the base. It lives in world space, so it finishes fading after
    /// a hit stop, a switch, or its blade being gone (then it removes itself).
    /// </summary>
    public sealed class BladeStreak : MonoBehaviour
    {
        private struct Sample
        {
            public Vector3 A, B;
            public float Time, Strength;
        }

        private static Material material;

        private readonly List<Sample> samples = new List<Sample>();
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color> colours = new List<Color>();
        private readonly List<int> triangles = new List<int>();
        private Transform source;
        private Vector3 localBase, localTip, lastTip, lastBase;
        private bool hasLast, sourceLost;
        private Mesh mesh;

        public StreakSettings Settings { get; set; }

        /// <summary>A blade's streak: the band between two points on <paramref name="blade"/>, in its own space.</summary>
        public static BladeStreak Sweep(Transform blade, Vector3 localBase, Vector3 localTip, StreakSettings settings, Transform parent)
        {
            var go = new GameObject("Blade streak");
            go.transform.SetParent(parent, false);
            var streak = go.AddComponent<BladeStreak>();
            streak.source = blade;
            streak.localBase = localBase;
            streak.localTip = localTip;
            streak.Settings = settings ?? new StreakSettings();
            streak.mesh = new Mesh { name = "Blade streak" };
            streak.mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh = streak.mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = SharedMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return streak;
        }

        /// <summary>See-through and lit by nothing: the streak's colour comes from its vertices.</summary>
        private static Material SharedMaterial()
        {
            if (material == null)
            {
                var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Particles/Unlit");
                material = shader != null ? new Material(shader) { name = "Blade streak", hideFlags = HideFlags.DontSave } : null;
            }

            return material;
        }

        private void LateUpdate()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            float now = Time.time;
            if (source != null && source.gameObject.activeInHierarchy)
            {
                Record(now);
            }
            else if (source == null)
            {
                sourceLost = true;
            }

            while (samples.Count > 0 && now - samples[0].Time > Settings.Lifetime)
            {
                samples.RemoveAt(0);
            }

            Build(now);
            if (sourceLost && samples.Count == 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
        }

        private void Record(float now)
        {
            Vector3 a = source.TransformPoint(localBase), b = source.TransformPoint(localTip);
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            // (Snapped somewhere in one frame, a rebuild or a teleport: not a movement.)
            bool jumped = hasLast && (b - lastTip).magnitude > 0.8f;
            // (The tip turning round the base: not whoever holds it walking or lunging.)
            float speed = hasLast ? ((b - lastTip) - (a - lastBase)).magnitude / dt : 0f;
            lastTip = b;
            lastBase = a;
            hasLast = true;
            if (jumped || !Settings.On)
            {
                samples.Clear();
                return;
            }

            float strength = Mathf.Clamp01((speed - Settings.MinSpeed) / Mathf.Max(0.01f, Settings.FullSpeed - Settings.MinSpeed));
            if (strength <= 0f && (samples.Count == 0 || samples[samples.Count - 1].Strength <= 0f))
            {
                // (Slow, and nothing to draw: only this point, so a streak starting next frame starts here.)
                samples.Clear();
            }

            samples.Add(new Sample { A = a, B = b, Time = now, Strength = strength });
        }

        private void Build(float now)
        {
            mesh.Clear();
            if (samples.Count < 2)
            {
                return;
            }

            vertices.Clear();
            colours.Clear();
            triangles.Clear();
            int steps = Mathf.Max(1, Settings.Smoothing + 1);
            bool any = false;
            for (int i = 0; i < samples.Count - 1; i++)
            {
                var p0 = samples[Mathf.Max(0, i - 1)];
                var p1 = samples[i];
                var p2 = samples[i + 1];
                var p3 = samples[Mathf.Min(samples.Count - 1, i + 2)];
                for (int s = 0; s < steps; s++)
                {
                    if (i > 0 && s == 0)
                    {
                        continue;
                    }

                    float t = s / (float)steps;
                    Add(CatmullRom(p0.A, p1.A, p2.A, p3.A, t), CatmullRom(p0.B, p1.B, p2.B, p3.B, t), Mathf.Lerp(p1.Time, p2.Time, t), Mathf.Lerp(p1.Strength, p2.Strength, t), now, ref any);
                }
            }

            var last = samples[samples.Count - 1];
            Add(last.A, last.B, last.Time, last.Strength, now, ref any);
            if (!any)
            {
                return;
            }

            for (int i = 0; i + 3 < vertices.Count; i += 2)
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 2);
                triangles.Add(i + 1);
                triangles.Add(i + 3);
            }

            mesh.SetVertices(vertices);
            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private void Add(Vector3 a, Vector3 b, float time, float strength, float now, ref bool any)
        {
            float age = Mathf.Clamp01((now - time) / Settings.Lifetime);
            var colour = Color.Lerp(Settings.Head, Settings.Tail, age);
            colour.a *= strength * (1f - age);
            any |= colour.a > 0.001f;
            // (Strongest at the tip, almost nothing at the base.)
            vertices.Add(a);
            vertices.Add(b);
            colours.Add(new Color(colour.r, colour.g, colour.b, colour.a * 0.1f));
            colours.Add(colour);
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (3f * p1 - p0 - 3f * p2 + p3) * t3);
        }
    }

    /// <summary>
    /// The show of a combo (the combos plan's step 6): the cues a combo gives as it goes, each with its own switch
    /// (the scene's Combat Feel). Streaks that grow with each blow; a swish that rises; the deep hit of the big one; a
    /// kick of the camera as the big one lands; a short beat of slow motion when a combo floors its man; a spark and a
    /// ring when a blow is stopped. Each cue is counted as it is given, seen or not (the headless runs count them:
    /// nothing headless can draw or play them).
    /// </summary>
    public static class ComboShow
    {
        public static bool Streaks = true, Swish = true, DeepHit = true, CameraKick = true, SlowBeat = true, Sparks = true;

        /// <summary>How much higher each blow's swish is than the one before (a share of the pitch).</summary>
        public static float SwishRises = 0.14f;

        /// <summary>How hard the camera is kicked as the big one lands (0 to 1), and less on a shield.</summary>
        public static float KickBy = 0.8f;

        /// <summary>The slow beat: how slow (a share of the game's pace) and how long (seconds of the real clock).</summary>
        public static float SlowTo = 0.3f, SlowFor = 0.35f;

        /// <summary>Cues given so far: streaks begun (by rank: light, link, big), swishes pitched, deep hits, camera kicks, slow beats, sparks, rings.</summary>
        public static readonly int[] StreaksBegun = new int[4];
        public static int Swishes, DeepHits, Kicks, SlowBeats, SparksThrown, Rings;

        /// <summary>The camera is to be kicked: along this way (the world's), this hard (0 to 1).</summary>
        public static event System.Action<Vector3, float> Kicked;

        /// <summary>The game is to go slow for a beat: to this share of its pace, for this long (seconds of the real clock).</summary>
        public static event System.Action<float, float> Slowed;

        public static string Counted => $"streaks begun {StreaksBegun[1]} light, {StreaksBegun[2]} link, {StreaksBegun[3]} big; swishes pitched {Swishes}; deep hits {DeepHits}; camera kicks {Kicks}; slow beats {SlowBeats}; sparks {SparksThrown}, rings {Rings}";

        public static void Reset()
        {
            System.Array.Clear(StreaksBegun, 0, StreaksBegun.Length);
            Swishes = DeepHits = Kicks = SlowBeats = SparksThrown = Rings = 0;
        }

        /// <summary>A blow of a combo has met something: the big one kicks the camera and lands deep; a stopped blow throws a spark and rings.</summary>
        public static void BlowMet(bool last, bool onBody, Vector3 point, Vector3 way)
        {
            if (last)
            {
                DeepHits += DeepHit ? 1 : 0;
                if (CameraKick)
                {
                    Kicks++;
                    Kicked?.Invoke(way, onBody ? KickBy : 0.5f * KickBy);
                }
            }

            if (!onBody && Sparks)
            {
                SparksThrown++;
                Rings++;
                if (Application.isPlaying)
                {
                    HitSpark.Throw(point, way, last ? 1.5f : 1f);
                }

                PhysicsBattler.Audio.Sfx.Play(PhysicsBattler.Audio.SfxIds.ParryClang, point, last ? 0.7f : 0.45f, last ? 1.25f : 1.45f);
            }
        }

        /// <summary>A combo has floored its man: a short beat of slow motion.</summary>
        public static void Floored()
        {
            if (SlowBeat)
            {
                SlowBeats++;
                Slowed?.Invoke(SlowTo, SlowFor);
            }
        }
    }

    /// <summary>
    /// A spark where a blow is stopped (the combos plan's step 6): a dozen short bright lines thrown out from the
    /// point, back along the way the blade came and to the sides, gone in a fifth of a second. Built blind: nothing
    /// headless draws it.
    /// </summary>
    public sealed class HitSpark : MonoBehaviour
    {
        private const int Lines = 12;
        private const float Lasts = 0.2f;
        private readonly Vector3[] goes = new Vector3[Lines];
        private Mesh mesh;
        private float born, size = 1f;
        private static Material material;
        private static readonly System.Random luck = new System.Random(7);

        public static void Throw(Vector3 at, Vector3 way, float size)
        {
            var go = new GameObject("Hit spark");
            go.transform.position = at;
            var spark = go.AddComponent<HitSpark>();
            spark.size = size;
            spark.born = Time.time;
            Vector3 back = way.sqrMagnitude > 1e-6f ? -way.normalized : Vector3.up;
            for (int i = 0; i < Lines; i++)
            {
                var any = new Vector3((float)luck.NextDouble() * 2f - 1f, (float)luck.NextDouble() * 2f - 1f, (float)luck.NextDouble() * 2f - 1f);
                spark.goes[i] = (back * 0.6f + any).normalized * (2.5f + 3.5f * (float)luck.NextDouble());
            }

            spark.mesh = new Mesh { name = "Hit spark" };
            spark.mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh = spark.mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            if (material == null)
            {
                var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Particles/Unlit");
                material = shader != null ? new Material(shader) { name = "Hit spark", hideFlags = HideFlags.DontSave } : null;
            }

            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void LateUpdate()
        {
            float age = Time.time - born;
            if (age >= Lasts || mesh == null)
            {
                Destroy(gameObject);
                return;
            }

            // Each line: a thin quad from where its head was a moment ago to where it is, facing the camera.
            float k = age / Lasts, fade = 1f - k;
            var cam = Camera.main;
            Vector3 toEye = cam != null ? (cam.transform.position - transform.position).normalized : Vector3.up;
            var vertices = new Vector3[Lines * 4];
            var colors = new Color[Lines * 4];
            var triangles = new int[Lines * 6];
            for (int i = 0; i < Lines; i++)
            {
                // (Slowing as it flies, and falling a little.)
                Vector3 head = goes[i] * (age * (1f - 0.5f * k)) * 0.5f * size + Vector3.down * (1.5f * age * age);
                Vector3 tail = head - goes[i].normalized * (0.07f * size * fade + 0.01f);
                Vector3 side = Vector3.Cross(goes[i], toEye).normalized * (0.006f * size);
                int v = i * 4;
                vertices[v] = tail - side;
                vertices[v + 1] = tail + side;
                vertices[v + 2] = head + side;
                vertices[v + 3] = head - side;
                var bright = new Color(1f, 0.92f, 0.6f, fade);
                colors[v] = colors[v + 1] = new Color(1f, 0.6f, 0.2f, 0f);
                colors[v + 2] = colors[v + 3] = bright;
                int t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }

            mesh.Clear();
            mesh.SetVertices(new List<Vector3>(vertices));
            mesh.SetColors(new List<Color>(colors));
            mesh.SetTriangles(new List<int>(triangles), 0);
            mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                Destroy(mesh);
            }
        }
    }
}
