using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The shock wave through the body on a hit (the plan "Shock waves through the body", 21 November): a wave starts at
    /// the part that was struck and travels out along the body, part by part, and each part shudders as it passes, then
    /// settles. It is drawn only: it moves the body's meshes (and what the hands hold) and nothing else, so no collider,
    /// no hit, no check and no physics sees it.
    ///
    /// Each part's swing, from its own arrival time t_p = d_p / c (its distance along the body over the wave's speed):
    /// u_p(t) = A · k^(j_p) · m_p · exp(−(t − t_p)·ln2 / h) · sin(2π f (t − t_p)): the strength, the pass-on once for each
    /// joint crossed, the part's looseness, a fade with a half-life, a swing at f a second. Along the blow's way.
    ///
    /// Drawn on the joints so the body stays joined: each part is turned so its own joint and the next go where the wave
    /// puts them. The ends (head, hands) are turned by the tilt, and the hands take what they hold with them: the whip.
    /// </summary>
    public sealed class ShockWave
    {
        // ---- The sliders (Combat Feel pushes them) -----------------------------------------------------------------

        /// <summary>On: every hit and impact sends a wave. Off (old): none.</summary>
        public static bool On = true;

        public const float DefaultFrequency = 7f, DefaultHalfLife = 0.15f, DefaultStrength = 0.09f, DefaultMost = 0.15f,
            DefaultSpeed = 5f, DefaultPassOn = 0.75f, DefaultLoose = 1.3f, DefaultTilt = 1.5f, DefaultOverTheTop = 1f,
            DefaultAttackerShare = 0.35f, DefaultHeavyBlow = 60f;

        /// <summary>Swings a second as the wave passes.</summary>
        public static float Frequency = DefaultFrequency;

        /// <summary>The time for a part's swing to halve (seconds).</summary>
        public static float HalfLife = DefaultHalfLife;

        /// <summary>How far a heavy blow (<see cref="HeavyBlow"/>) moves the part it struck (metres, at a Medium's size).</summary>
        public static float Strength = DefaultStrength;

        /// <summary>Never more than this, a part, all its waves together (metres, at a Medium's size), before the dial.</summary>
        public static float Most = DefaultMost;

        /// <summary>How fast the wave travels along the body (m/s).</summary>
        public static float Speed = DefaultSpeed;

        /// <summary>The share of the wave that gets through each joint.</summary>
        public static float PassOn = DefaultPassOn;

        /// <summary>How much further the head and the hands (and what they hold) swing.</summary>
        public static float LooseEnds = DefaultLoose;

        /// <summary>How much the head and hands turn as the wave runs through them (degrees for each cm of their swing).</summary>
        public static float Tilt = DefaultTilt;

        /// <summary>The one dial on the strength and the dying away together. 1: the first values (about three times a real body). 0: off.</summary>
        public static float OverTheTop = DefaultOverTheTop;

        /// <summary>On: the wave runs through the freeze of a hit. Off: it waits for the freeze to end.</summary>
        public static bool InTheFreeze = true;

        /// <summary>On: the attacker's own sword arm shudders when his blow is stopped by a shield or a blade.</summary>
        public static bool AttackerShudders = true;

        /// <summary>The attacker's shudder against the wave the blow sends into the man who stopped it.</summary>
        public static float AttackerShare = DefaultAttackerShare;

        /// <summary>On: the wave runs on through the hips into the legs, fading through the thighs and stopping at the shins (the feet stay planted). Off: it stops at the hips.</summary>
        public static bool Legs = true;

        /// <summary>A blow this heavy (N·s on a Medium) moves the struck part by <see cref="Strength"/>; a lighter one less.</summary>
        public static float HeavyBlow = DefaultHeavyBlow;

        /// <summary>Waves started, everywhere, since the scene began (for the report and the panel).</summary>
        public static int Started, Rung, Shudders;

        /// <summary>The struck part's swing of the waves begun (metres): the sum and the largest, and how many by size.</summary>
        public static float SwingSum, SwingMost;

        public static readonly int[] BySize = new int[4];

        /// <summary>The waves begun so far, told: how many, how far they moved the struck part, the attackers' shudders.</summary>
        public static string Counted() => Started == 0 ? (On ? "none begun" : "off") : string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} begun ({1} rung from the lab, {2} attackers' shudders); the struck part swung {3:0.0} cm on average, {4:0.0} at most; under 2 cm {5}, 2 to 5 cm {6}, 5 to 9 cm {7}, over 9 cm {8}",
            Started, Rung, Shudders, 100f * SwingSum / Started, 100f * SwingMost, BySize[0], BySize[1], BySize[2], BySize[3]);

        // ---- The body as a chain -----------------------------------------------------------------------------------

        private const int N = PartInfo.Count;

        // (The part each part's far joint is: the next part down the chain, or -1 for an end.)
        private static readonly int[] Next =
        {
            (int)PartId.Spine, (int)PartId.Chest, (int)PartId.Head, -1,
            (int)PartId.ForearmL, (int)PartId.HandL, -1,
            (int)PartId.ForearmR, (int)PartId.HandR, -1,
            (int)PartId.ShinL, (int)PartId.FootL, -1,
            (int)PartId.ShinR, (int)PartId.FootR, -1,
        };

        private static bool IsLeg(int p) => p >= (int)PartId.ThighL;

        private static bool IsLooseEnd(int p) => p == (int)PartId.Head || p == (int)PartId.HandL || p == (int)PartId.HandR;

        // (How much of the wave a leg's joint takes: the hip full, the knee half, the ankle none: the feet stay planted.)
        private static float LegFade(int p) => p == (int)PartId.ThighL || p == (int)PartId.ThighR ? 0.85f : p == (int)PartId.ShinL || p == (int)PartId.ShinR ? 0.5f : p == (int)PartId.FootL || p == (int)PartId.FootR ? 0f : 1f;

        private struct Wave
        {
            public int From;
            public Vector3 Way;
            public float Amp, Age, Wait;
            public float[] D;
            public int[] J;
        }

        private readonly List<Wave> waves = new List<Wave>(8);
        private readonly float[] link = new float[N];
        private bool measured;
        private readonly Vector3[] u = new Vector3[N];
        private readonly Vector3[] uEnd = new Vector3[N];

        /// <summary>The fighter's size against a Medium's.</summary>
        public float Size = 1f;

        /// <summary>Waves this body has taken.</summary>
        public int Taken { get; private set; }

        /// <summary>Its waves now going.</summary>
        public int Going => waves.Count;

        /// <summary>How far each part is swung now (metres, world), drawn.</summary>
        public Vector3 Offset(PartId p) => u[(int)p];

        /// <summary>The largest swing of any part now (metres).</summary>
        public float Largest
        {
            get
            {
                float m = 0f;
                for (int p = 0; p < N; p++)
                {
                    m = Mathf.Max(m, u[p].magnitude, uEnd[p].magnitude);
                }

                return m;
            }
        }

        /// <summary>The joints' lengths, from where the body's parts are (once, standing).</summary>
        public void Measure(System.Func<PartId, Vector3> at)
        {
            for (int p = 0; p < N; p++)
            {
                var id = (PartId)p;
                link[p] = PartInfo.HasParent(id) ? Vector3.Distance(at(id), at(PartInfo.Parent(id))) : 0f;
            }

            measured = true;
        }

        /// <summary>A hit: a wave from the part it landed on, along the blow, as strong as the blow is heavy (N·s).</summary>
        public void Hit(PartId part, Vector3 impulse, float scale = 1f, float wait = 0f)
        {
            float j = impulse.magnitude;
            // (In the labs and the dance only, as every new feel thing: the fight of six when Renier says.)
            if (!On || !AttackMaker.InALab || j < 1e-3f || scale <= 0f)
            {
                return;
            }

            float amp = Strength * Size * Mathf.Min(j / Mathf.Max(1f, HeavyBlow * Size * Size * Size), Most / Mathf.Max(1e-3f, Strength)) * scale;
            Start(part, impulse / j, amp, InTheFreeze ? 0f : wait);
        }

        /// <summary>The lab's ring: a wave from a part, along a way, with no blow (a heavy blow's strength).</summary>
        public void Ring(PartId part, Vector3 way)
        {
            Rung++;
            Start(part, way, Strength * Size, 0f);
        }

        private void Start(PartId part, Vector3 way, float amp, float wait)
        {
            amp *= OverTheTop;
            if (amp < 0.002f || way.sqrMagnitude < 1e-8f)
            {
                return;
            }

            way.y *= 0.3f;
            way = way.sqrMagnitude > 1e-8f ? way.normalized : Vector3.forward;
            if (waves.Count >= 8)
            {
                waves.RemoveAt(0);
            }

            // (How far along the body each part is from the one struck, and how many joints between: along the bones.)
            var d = new float[N];
            var jj = new int[N];
            for (int p = 0; p < N; p++)
            {
                d[p] = -1f;
            }

            d[(int)part] = 0f;
            var open = new Queue<int>();
            open.Enqueue((int)part);
            while (open.Count > 0)
            {
                int p = open.Dequeue();
                for (int q = 0; q < N; q++)
                {
                    if (d[q] >= 0f)
                    {
                        continue;
                    }

                    bool joined = PartInfo.HasParent((PartId)q) && (int)PartInfo.Parent((PartId)q) == p || PartInfo.HasParent((PartId)p) && (int)PartInfo.Parent((PartId)p) == q;
                    if (!joined)
                    {
                        continue;
                    }

                    float len = (int)PartInfo.Parent((PartId)q) == p ? link[q] : link[p];
                    d[q] = d[p] + (measured ? len : 0.25f * Size);
                    jj[q] = jj[p] + 1;
                    open.Enqueue(q);
                }
            }

            waves.Add(new Wave { From = (int)part, Way = way, Amp = amp, Age = 0f, Wait = wait, D = d, J = jj });
            Started++;
            SwingSum += amp;
            SwingMost = Mathf.Max(SwingMost, amp);
            BySize[amp < 0.02f ? 0 : amp < 0.05f ? 1 : amp < 0.09f ? 2 : 3]++;
            Taken++;
            Track(part, way);
        }

        /// <summary>The last wave begun waits this long before it starts, if the wave is not to run through the freeze.</summary>
        public void HoldLast(float wait)
        {
            if (InTheFreeze || waves.Count == 0 || wait <= 0f)
            {
                return;
            }

            var w = waves[waves.Count - 1];
            if (w.Age <= 0f)
            {
                w.Wait = Mathf.Max(w.Wait, wait);
                waves[waves.Count - 1] = w;
            }
        }

        private float SwingOf(in Wave w, float d, int j, float looseness)
        {
            float t = w.Age - w.Wait - d / Mathf.Max(0.1f, Speed);
            if (t <= 0f)
            {
                return 0f;
            }

            float h = Mathf.Max(0.005f, HalfLife * (0.5f + 0.5f * OverTheTop));
            float fade = Mathf.Exp(-t * 0.6931472f / h);
            if (fade < 0.01f)
            {
                return 0f;
            }

            return w.Amp * Mathf.Pow(Mathf.Clamp01(PassOn), j) * looseness * fade * Mathf.Sin(2f * Mathf.PI * Frequency * t);
        }

        /// <summary>The waves stepped on (game time) and each part's swing worked out.</summary>
        public void Step(float dt)
        {
            for (int k = waves.Count - 1; k >= 0; k--)
            {
                var w = waves[k];
                w.Age += dt;
                float h = Mathf.Max(0.005f, HalfLife * (0.5f + 0.5f * OverTheTop));
                // (Gone once even the furthest part has faded to a hundredth.)
                float furthest = 0f;
                foreach (float x in w.D)
                {
                    furthest = Mathf.Max(furthest, x);
                }

                if (w.Age > w.Wait + furthest / Mathf.Max(0.1f, Speed) + 7f * h + 0.05f)
                {
                    waves.RemoveAt(k);
                    continue;
                }

                waves[k] = w;
            }

            float cap = Most * Size * Mathf.Max(1f, OverTheTop);
            for (int p = 0; p < N; p++)
            {
                Vector3 s = Vector3.zero, e = Vector3.zero;
                float fade = IsLeg(p) ? (Legs ? LegFade(p) : 0f) : 1f;
                if (p == (int)PartId.Pelvis && !Legs)
                {
                    fade = 1f;
                }

                if (fade > 0f)
                {
                    foreach (var w in waves)
                    {
                        if (w.D[p] < 0f)
                        {
                            continue;
                        }

                        s += w.Way * SwingOf(w, w.D[p], w.J[p], fade);
                        if (IsLooseEnd(p))
                        {
                            // (The end of a loose part: a little further on and a little later, and loose.)
                            float end = (p == (int)PartId.Head ? 0.2f : 0.12f) * Size;
                            e += w.Way * SwingOf(w, w.D[p] + end, w.J[p], LooseEnds);
                        }
                    }
                }

                u[p] = Vector3.ClampMagnitude(s, cap);
                uEnd[p] = Vector3.ClampMagnitude(e, cap);
            }

            Sample();
        }

        // ---- Drawn ---------------------------------------------------------------------------------------------------

        private struct Drawn
        {
            public Transform T;
            public Vector3 Pos;
            public Quaternion Rot;
        }

        private readonly List<Drawn>[] drawn = new List<Drawn>[N];
        private readonly Dictionary<Transform, (Vector3, Quaternion)> rest = new Dictionary<Transform, (Vector3, Quaternion)>();
        private float lookAgain;
        private readonly Quaternion[] turns = new Quaternion[N];
        private readonly Vector3[] joint = new Vector3[N], drawnAt = new Vector3[N];
        private bool moved;

        /// <summary>How far the drawn body's joints were from where the wave puts them, at most, the last step (metres): drawn as a skeleton, the body stays joined and follows the wave this closely.</summary>
        public float Gap { get; private set; }

        /// <summary>
        /// Every mesh of the body (and of what each hand holds) found: the objects with a renderer and no collider under each
        /// part. Looked for again every half second, or when one is gone (a weapon changed hands).
        /// </summary>
        private void Find(UnitRig rig, float dt)
        {
            lookAgain -= dt;
            bool lost = false;
            for (int p = 0; p < N && !lost; p++)
            {
                if (drawn[p] == null)
                {
                    lost = true;
                    break;
                }

                foreach (var x in drawn[p])
                {
                    if (x.T == null)
                    {
                        lost = true;
                        break;
                    }
                }
            }

            if (!lost && lookAgain > 0f)
            {
                return;
            }

            lookAgain = 0.5f;
            for (int p = 0; p < N; p++)
            {
                drawn[p] = drawn[p] ?? new List<Drawn>(2);
                // (Each put back at rest first: one left out from now on, a part cut off, is not left swung.)
                foreach (var x in drawn[p])
                {
                    if (x.T != null)
                    {
                        x.T.localPosition = x.Pos;
                        x.T.localRotation = x.Rot;
                    }
                }

                drawn[p].Clear();
                var part = rig.Part((PartId)p);
                if (part == null || rig.IsSevered((PartId)p))
                {
                    continue;
                }

                foreach (var r in part.GetComponentsInChildren<Renderer>(true))
                {
                    var t = r.transform;
                    if (t == part.transform || t.GetComponent<Collider>() != null)
                    {
                        continue;
                    }

                    if (!rest.TryGetValue(t, out var was))
                    {
                        was = (t.localPosition, t.localRotation);
                        rest[t] = was;
                    }

                    drawn[p].Add(new Drawn { T = t, Pos = was.Item1, Rot = was.Item2 });
                }
            }
        }

        /// <summary>
        /// The swing put on the drawn body: each part's meshes turned and moved so its near joint goes where the wave puts
        /// it and its far joint where the wave puts that; the head and hands turned by the tilt. Every step from the meshes'
        /// own rest, so nothing builds up; once still, left at rest.
        /// </summary>
        public void Draw(UnitRig rig, float dt)
        {
            if (rig == null)
            {
                return;
            }

            if (!measured)
            {
                Measure(id => rig.Part(id) != null ? rig.Part(id).transform.position : Vector3.zero);
            }

            Find(rig, dt);
            bool still = Largest < 1e-4f;
            if (still && !moved)
            {
                return;
            }

            // (Drawn as a skeleton, from the hips out: each part starts where the part before it, drawn, has its joint, and
            // is turned so its far joint goes where the wave puts that one. So the body stays joined; the ends are turned
            // by the tilt, and the feet stay where they are turned from.)
            float off = 0f;
            for (int p = 0; p < N; p++)
            {
                turns[p] = Quaternion.identity;
                var part = rig.Part((PartId)p);
                Vector3 at = part != null ? part.transform.position : Vector3.zero;
                joint[p] = at;
                drawnAt[p] = at + u[p];
                if (still || part == null || rig.IsSevered((PartId)p))
                {
                    continue;
                }

                var id = (PartId)p;
                Quaternion before = Quaternion.identity;
                if (PartInfo.HasParent(id))
                {
                    int q = (int)PartInfo.Parent(id);
                    drawnAt[p] = turns[q] * (at - joint[q]) + drawnAt[q];
                    before = turns[q];
                }

                int next = Next[p];
                if (next >= 0 && rig.Part((PartId)next) != null)
                {
                    // (The turn that best brings every joint it carries where the wave puts them: the chest carries the
                    // neck and both shoulders, the hips the spine and both legs. Least squares, small turns.)
                    turns[p] = BestTurn(rig, p, at);
                }
                else if (IsLooseEnd(p))
                {
                    // (The head and the hands: turned with the part before them, and on by the tilt the way their end
                    // swings, at most 30 degrees: the whip.)
                    turns[p] = before;
                    Vector3 bone = p == (int)PartId.Head ? part.transform.up : at - joint[(int)PartInfo.Parent(id)];
                    Vector3 axis = Vector3.Cross(bone, uEnd[p]);
                    if (axis.sqrMagnitude > 1e-10f)
                    {
                        float angle = Mathf.Min(Tilt * 100f * uEnd[p].magnitude, 30f);
                        turns[p] = Quaternion.AngleAxis(angle, axis.normalized) * before;
                    }
                }

                if (!PartInfo.IsFoot(id))
                {
                    float e = (drawnAt[p] - at - u[p]).magnitude;
                    off = Mathf.Max(off, e);
                    if (Last != null && waves.Count == 1)
                    {
                        Last.Off[p] = Mathf.Max(Last.Off[p], e);
                    }
                }
            }

            Gap = off;
            // (The whip, measured: how far the sword's point (0.9 m out along its slot) and the shield's rim (0.3 m out) are
            // drawn from where they are, against the hand's own swing.)
            if (Last != null && waves.Count == 1 && !still)
            {
                for (int h = 0; h < 2; h++)
                {
                    var slot = h == 0 ? rig.SlotWeaponR : rig.SlotWeaponL;
                    int hp = h == 0 ? (int)PartId.HandR : (int)PartId.HandL;
                    if (slot == null)
                    {
                        continue;
                    }

                    Vector3 end = slot.position + slot.forward * ((h == 0 ? 0.9f : 0.3f) * Size);
                    float moved = (turns[hp] * (end - joint[hp]) + drawnAt[hp] - end).magnitude;
                    if (h == 0)
                    {
                        Last.Point = Mathf.Max(Last.Point, moved);
                    }
                    else
                    {
                        Last.Rim = Mathf.Max(Last.Rim, moved);
                    }
                }
            }
            for (int p = 0; p < N; p++)
            {
                var part = rig.Part((PartId)p);
                if (part == null || drawn[p] == null)
                {
                    continue;
                }

                foreach (var x in drawn[p])
                {
                    if (x.T != null)
                    {
                        x.T.localPosition = x.Pos;
                        x.T.localRotation = x.Rot;
                    }
                }

                if (still || rig.IsSevered((PartId)p))
                {
                    continue;
                }

                Vector3 at = joint[p];
                foreach (var x in drawn[p])
                {
                    if (x.T == null)
                    {
                        continue;
                    }

                    Vector3 pos = x.T.position;
                    Quaternion rot = x.T.rotation;
                    x.T.SetPositionAndRotation(turns[p] * (pos - at) + drawnAt[p], turns[p] * rot);
                }
            }

            moved = !still;
        }

        /// <summary>
        /// The turn of part p about its near joint (drawn at <see cref="drawnAt"/>) that best brings the joints of the parts
        /// hanging from it to where the wave puts them: the turning vector w minimising the sum of |w x r - e|^2 over
        /// them (r from the joint to each, e what is left to make up), with a little held back so a line of joints does
        /// not twist about itself.
        /// </summary>
        private Quaternion BestTurn(UnitRig rig, int p, Vector3 at)
        {
            float a00 = 0f, a01 = 0f, a02 = 0f, a11 = 0f, a12 = 0f, a22 = 0f;
            Vector3 b = Vector3.zero;
            float most = 0f;
            for (int q = 0; q < N; q++)
            {
                var id = (PartId)q;
                if (!PartInfo.HasParent(id) || (int)PartInfo.Parent(id) != p || rig.Part(id) == null || rig.IsSevered(id))
                {
                    continue;
                }

                Vector3 r = rig.Part(id).transform.position - at;
                Vector3 e = rig.Part(id).transform.position + u[q] - drawnAt[p] - r;
                float rr = r.sqrMagnitude;
                a00 += rr - r.x * r.x; a01 -= r.x * r.y; a02 -= r.x * r.z;
                a11 += rr - r.y * r.y; a12 -= r.y * r.z; a22 += rr - r.z * r.z;
                b += Vector3.Cross(r, e);
                most = Mathf.Max(most, rr);
            }

            if (most < 1e-6f)
            {
                return Quaternion.identity;
            }

            float hold = 0.02f * most;
            a00 += hold; a11 += hold; a22 += hold;
            float det = a00 * (a11 * a22 - a12 * a12) - a01 * (a01 * a22 - a12 * a02) + a02 * (a01 * a12 - a11 * a02);
            if (Mathf.Abs(det) < 1e-12f)
            {
                return Quaternion.identity;
            }

            float i00 = (a11 * a22 - a12 * a12) / det, i01 = (a02 * a12 - a01 * a22) / det, i02 = (a01 * a12 - a02 * a11) / det;
            float i11 = (a00 * a22 - a02 * a02) / det, i12 = (a01 * a02 - a00 * a12) / det, i22 = (a00 * a11 - a01 * a01) / det;
            var w = new Vector3(i00 * b.x + i01 * b.y + i02 * b.z, i01 * b.x + i11 * b.y + i12 * b.z, i02 * b.x + i12 * b.y + i22 * b.z);
            float angle = Mathf.Min(w.magnitude * Mathf.Rad2Deg, 35f);
            return angle > 1e-3f ? Quaternion.AngleAxis(angle, w.normalized) : Quaternion.identity;
        }

        /// <summary>Where a point that moves with part p is drawn now (the body as the wave draws it): for the clothes, which hang on the drawn body.</summary>
        public Vector3 DrawnPoint(PartId p, Vector3 x) => moved ? turns[(int)p] * (x - joint[(int)p]) + drawnAt[(int)p] : x;

        /// <summary>How part p is turned as drawn now.</summary>
        public Quaternion DrawnTurn(PartId p) => moved ? turns[(int)p] : Quaternion.identity;

        // ---- Measured: the last ring or hit, part by part (for the report and the panel) ---------------------------------

        public sealed class Trace
        {
            public PartId From;
            public Vector3 Way;
            public float Age;
            public readonly float[] FirstMoved = new float[N], FirstSwing = new float[N], Still = new float[N], LastSign = new float[N];
            public readonly int[] Swings = new int[N];
            public readonly float[] Off = new float[N];
            public float Largest, Gap, Point, Rim;
        }

        /// <summary>The last wave started on a body standing still in all others: what each part did.</summary>
        public Trace Last { get; private set; }

        private void Track(PartId from, Vector3 way)
        {
            var t = new Trace { From = from, Way = way };
            for (int p = 0; p < N; p++)
            {
                t.FirstMoved[p] = -1f;
                t.Still[p] = -1f;
            }

            Last = t;
        }

        private void Sample()
        {
            var t = Last;
            if (t == null || waves.Count != 1)
            {
                return;
            }

            t.Age = waves[0].Age;
            for (int p = 0; p < N; p++)
            {
                float along = Vector3.Dot(u[p], t.Way);
                float m = Mathf.Abs(along);
                if (m > 0.002f)
                {
                    if (t.FirstMoved[p] < 0f)
                    {
                        t.FirstMoved[p] = t.Age;
                    }

                    if (t.Swings[p] <= 1)
                    {
                        t.FirstSwing[p] = Mathf.Max(t.FirstSwing[p], m);
                    }

                    float sign = Mathf.Sign(along);
                    if (sign != t.LastSign[p])
                    {
                        t.Swings[p]++;
                        t.LastSign[p] = sign;
                    }

                    t.Still[p] = t.Age;
                }

                t.Largest = Mathf.Max(t.Largest, m);
            }

            t.Gap = Mathf.Max(t.Gap, Gap);
        }

        /// <summary>The last wave, told: where it started, and for each part when it first moved, its first swing, its swings, when it was still.</summary>
        public string Told()
        {
            var t = Last;
            if (t == null)
            {
                return "none yet";
            }

            var s = new System.Text.StringBuilder();
            s.Append($"from the {t.From}: ");
            foreach (var p in new[] { PartId.Chest, PartId.Head, PartId.UpperArmR, PartId.ForearmR, PartId.HandR, PartId.HandL, PartId.Pelvis, PartId.ThighR, PartId.ShinR, PartId.FootR })
            {
                int i = (int)p;
                s.Append(t.FirstMoved[i] < 0f ? $"{p} still; " : $"{p} at {t.FirstMoved[i]:0.00} s, {100f * t.FirstSwing[i]:0.0} cm, {(t.Swings[i] + 1) / 2} swings, still at {t.Still[i]:0.00} s; ");
            }

            int worst = 0;
            for (int p = 1; p < N; p++)
            {
                worst = t.Off[p] > t.Off[worst] ? p : worst;
            }

            s.Append($"the drawn joints within {1000f * t.Gap:0} mm of the wave (the furthest: the {(PartId)worst}); the sword's point swung {100f * t.Point:0.0} cm, the shield's rim {100f * t.Rim:0.0} cm");
            return s.ToString();
        }
    }
}
