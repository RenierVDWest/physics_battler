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
}
