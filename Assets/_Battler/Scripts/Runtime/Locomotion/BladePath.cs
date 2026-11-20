using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The path of a blade through one made attack (the plan "Combos that flow and end in a bang" of 20 November 2026,
    /// step 1): where the maker planned the sword to go as the attack was thrown, and where the sword then went.
    ///
    /// <para><b>The plan</b> is worked out once, as the attack is thrown, from the made clip and what the maker knew:
    /// a planted attack's blade from its clip and its own root's travel (<see cref="MadeAttack.Blade"/>); a blow on
    /// the move's from its clip laid on the walk as the walk was then, carried where the plan has the walk take him
    /// (<see cref="AttackOnTheMove.Layer"/>, <see cref="AttackOnTheMove.Walked"/>: what the walk does each step,
    /// done ahead of time). <b>As it went</b> is the fighter's own weapon, read each step while the clip plays
    /// (<see cref="StrikeHits.Blade"/>).</para>
    ///
    /// <para>It changes nothing a fighter does, and nothing is kept unless <see cref="On"/> is set (the scene's
    /// Combat Feel, "The Blade's Path Is Drawn"; the duel lab's "The blade's path"). The lab draws it
    /// (<c>BladePathView</c>); the measures tell how far the blade is carried through and how near the plan it went:
    /// what a join of two blows (the plan's step 2) will be checked by.</para>
    /// </summary>
    public sealed class BladePath
    {
        /// <summary>Paths are worked out and kept (off: nothing is, and a fighter's step costs nothing more).</summary>
        public static bool On;

        /// <summary>The last path of each fighter, for the lab to draw (cleared when <see cref="On"/> goes off).</summary>
        public static readonly List<BladePath> Shown = new List<BladePath>();

        /// <summary>Set (the headless run, the playtest log): told of each path once its attack is over.</summary>
        public static System.Action<BladePath> Told;

        /// <summary>The plan is worked out every so often along the clip (seconds).</summary>
        public const float PlanStep = 1f / 60f;

        public readonly ClipFighter Who;
        public readonly MadeAttack Made;
        public readonly LocoClip Clip;
        public readonly string Name;
        public readonly bool OnTheMove;

        /// <summary>When the attack was thrown (the fighter's own clock, seconds).</summary>
        public readonly float Since;

        /// <summary>When the hips start in and when the blade lands (seconds of the clip), and how long the clip is.</summary>
        public readonly float DriveAt, HitAt, Duration;

        /// <summary>The point the attack was asked for (world).</summary>
        public readonly Vector3 Target;

        /// <summary>The plan, every <see cref="PlanStep"/> of the clip (world): the grip, the point, the striking part, and where he stands.</summary>
        public Vector3[] PlanGrip, PlanTip, PlanStrike, PlanRoot;

        /// <summary>As it went, each step the clip played (world), with the clip's time then, and whether the blow was frozen on a hit.</summary>
        public readonly List<float> WentAt = new List<float>();
        public readonly List<Vector3> WentGrip = new List<Vector3>(), WentTip = new List<Vector3>(), WentStrike = new List<Vector3>(), WentRoot = new List<Vector3>();

        /// <summary>The point as the pose had it at each of those steps (world): what the sword as seen was being driven to. A sword that sticks while its pose sweeps on is held by something; one whose pose crawls too was told to.</summary>
        public readonly List<Vector3> PosedTip = new List<Vector3>();
        public readonly List<bool> WentFrozen = new List<bool>();

        /// <summary>What the blade met on its way, in the blow's own words (<see cref="StrikeHits.Last"/>, its middle), in order.</summary>
        public readonly List<string> Met = new List<string>();

        /// <summary>Set (the headless run's step dump): each step after the landing, what solid thing of another body the sword as seen lies against is written down (<see cref="Touched"/>). For finding what holds a blade that sticks.</summary>
        public static bool TracesTouch;

        /// <summary>With <see cref="TracesTouch"/>: for each step from the landing on, the solid colliders not his own that the sword as seen overlaps ("" for none), by name.</summary>
        public readonly List<string> Touched = new List<string>();

        /// <summary>The attack is over (its clip no longer plays): the measures are whole.</summary>
        public bool Done { get; private set; }

        /// <summary>The cut's corners, from its recording (<see cref="BladeFlow"/>).</summary>
        public readonly BladeCorner From, To;

        /// <summary>How long after its landing the planned blade turns (seconds of the clip): where the plan's carry-through ends.</summary>
        public float PlanTurnAfter { get; private set; }

        private float stepLength = 1f / 50f;

        private BladePath(ClipFighter who, MadeAttack made, Vector3 target, float since)
        {
            Who = who;
            Made = made;
            Clip = made.Clip;
            Name = made.Score != null ? made.Score.Name : made.Clip.Name;
            OnTheMove = made.OnTheMove;
            DriveAt = made.DriveAt;
            HitAt = made.ContactAt;
            Duration = made.Clip.Duration;
            Target = target;
            Since = since;
            var flow = made.Score != null && made.Score.Clip != null ? BladeFlow.Of(made.Score.Clip) : null;
            From = flow != null ? flow.From : BladeCorner.Guard;
            To = flow != null ? flow.To : BladeCorner.Guard;
        }

        /// <summary>
        /// The path of an attack as it is thrown: its plan worked out, nothing gone yet. The fighter stands at
        /// <paramref name="from"/> (world, on the ground) facing <paramref name="facing"/> (degrees); his leg is
        /// <paramref name="leg"/> metres, his blade <paramref name="length"/> leg lengths; the attack was asked for
        /// <paramref name="target"/> (world).
        /// </summary>
        public static BladePath Plan(ClipFighter who, MadeAttack made, Vector3 from, float facing, float leg, float length, Vector3 target, float since)
        {
            if (made == null || made.Clip == null)
            {
                return null;
            }

            var path = new BladePath(who, made, target, since);
            var clip = made.Clip;
            int n = Mathf.Max(2, Mathf.CeilToInt(clip.Duration / PlanStep) + 1);
            path.PlanGrip = new Vector3[n];
            path.PlanTip = new Vector3[n];
            path.PlanStrike = new Vector3[n];
            path.PlanRoot = new Vector3[n];
            var turn = Quaternion.Euler(0f, facing, 0f);
            Vector3 World(Vector3 own) => from + turn * new Vector3(own.x, 0f, own.z) * leg + Vector3.up * (own.y * leg);
            bool thrust = made.Score != null && made.Score.Thrust;
            // (What the blade strikes with: a thrust's point; a cut's striking part, so far along the blade.)
            float share = thrust ? 1f : made.Hold.Length > 1e-4f ? Mathf.Clamp01(made.Hold.Strike / made.Hold.Length) : 0.7f;
            if (!made.OnTheMove)
            {
                for (int k = 0; k < n; k++)
                {
                    float u = Mathf.Min(clip.Duration, k * PlanStep);
                    made.Blade(u, length, out Vector3 grip, out Vector3 tip);
                    clip.RootMotion(0f, u, out Vector2 moved, out _);
                    path.PlanGrip[k] = World(grip);
                    path.PlanTip[k] = World(tip);
                    path.PlanStrike[k] = World(Vector3.Lerp(grip, tip, share));
                    path.PlanRoot[k] = World(new Vector3(moved.x, 0f, moved.y));
                }
            }
            else
            {
                // On the move: the clip's chest, head and arms on the walk's legs as the walk was as the blow began,
                // the hips driven by the blow, the whole carried where the plan has the walk take him. What
                // Locomotor.OverStep does each step, done ahead of time.
                var m = BodyMeasures.OfClips;
                var asked = made.Asked;
                var pose = new Vector3[LocoClip.BoneCount];
                clip.Pose(0f, pose, 1f);
                var guard = m.Decompose(pose);
                var was = guard;
                for (int k = 0; k < n; k++)
                {
                    float u = Mathf.Min(clip.Duration, k * PlanStep);
                    clip.Pose(u, pose, 1f);
                    var blow = m.Decompose(pose, was);
                    was = blow;
                    clip.RootMotion(0f, u, out _, out float clipTurned);
                    float weight = Mathf.Clamp01((clip.Duration - u) / AttackOnTheMove.HandsBack);
                    weight = weight * weight * (3f - 2f * weight);
                    var layered = AttackOnTheMove.Layer(asked.WalkNow, blow, guard, clipTurned, weight, asked.Into * AttackOnTheMove.Hold(u, made.DriveAt, made.ContactAt), asked.Move);
                    AttackScore.SwordOf(layered, m, made.Hold, false, out _, out _, out Vector3 grip, out Vector3 way);
                    AttackOnTheMove.Walked(asked.Walks, asked.Turns, asked.Faces ? asked.FacesAt : (Vector3?)null, made.StepIn, u, made.DriveAt, made.ContactAt, asked.Move.Check, out Vector3 at, out float walkTurned);
                    var walked = Quaternion.Euler(0f, walkTurned, 0f);
                    Vector3 flat = at + walked * new Vector3(grip.x, 0f, grip.z);
                    grip = new Vector3(flat.x, grip.y, flat.z);
                    way = walked * way;
                    if (asked.Aimed)
                    {
                        // (The wrist's steer onto the target about the landing, as MadeAttack.Blade has a planted attack's.)
                        float c = made.ContactAt, begins = thrust ? 0.4f * c : Mathf.Max(0f, c - 0.15f);
                        float v = u <= c ? Mathf.Clamp01((u - begins) / Mathf.Max(0.05f, c - begins)) : 1f - Mathf.Clamp01((u - c) / 0.25f);
                        way = AttackMaker.Steered(way, asked.Target - grip, (thrust ? made.Hold.MostThrustSteer : made.Hold.MostSteer) * v * v * (3f - 2f * v), out _);
                    }

                    Vector3 tip = grip + way * length;
                    path.PlanGrip[k] = World(grip);
                    path.PlanTip[k] = World(tip);
                    path.PlanStrike[k] = World(Vector3.Lerp(grip, tip, share));
                    path.PlanRoot[k] = World(at);
                }
            }

            path.PlanTurnAfter = path.Measure().PlanTurnAfter;
            Shown.RemoveAll(p => p.Who == who);
            Shown.Add(path);
            return path;
        }

        /// <summary>One step of the attack as it goes: the clip's time, the weapon as it is (world), and where he stands.</summary>
        public void Went(float clipTime, Vector3 grip, Vector3 tip, Vector3 strike, Vector3 root, bool frozen, float dt, Vector3? posedTip = null)
        {
            if (Done)
            {
                return;
            }

            stepLength = dt > 1e-5f ? dt : stepLength;
            WentAt.Add(clipTime);
            WentGrip.Add(grip);
            WentTip.Add(tip);
            WentStrike.Add(strike);
            WentRoot.Add(new Vector3(root.x, 0f, root.z));
            WentFrozen.Add(frozen);
            PosedTip.Add(posedTip ?? tip);
        }

        /// <summary>The blade has met something: what the blow says of it (<see cref="StrikeHits.Last"/>: "clip at speed: what it did (reaction, impulse ...)"; the middle is kept).</summary>
        public void Meets(string told)
        {
            if (Done || string.IsNullOrEmpty(told) || Met.Count >= 6)
            {
                return;
            }

            int from = told.IndexOf(": ", System.StringComparison.Ordinal), tail = told.IndexOf(", impulse ", System.StringComparison.Ordinal);
            int to = tail > 0 ? told.LastIndexOf(" (", tail, System.StringComparison.Ordinal) : -1;
            from = from >= 0 ? from + 2 : 0;
            Met.Add(to > from ? told.Substring(from, to - from) : told.Substring(from));
        }

        /// <summary>The attack is over: nothing more is added, and whoever listens is told.</summary>
        public void End()
        {
            if (Done)
            {
                return;
            }

            Done = true;
            Told?.Invoke(this);
        }

        /// <summary>The plan at a time of the clip (world): the point, the striking part, the grip, where he stands.</summary>
        public void PlanAt(float clipTime, out Vector3 grip, out Vector3 tip, out Vector3 strike, out Vector3 root)
        {
            float f = Mathf.Clamp(clipTime / PlanStep, 0f, PlanTip.Length - 1);
            int a = Mathf.Min(Mathf.FloorToInt(f), PlanTip.Length - 2);
            float t = f - a;
            grip = Vector3.Lerp(PlanGrip[a], PlanGrip[a + 1], t);
            tip = Vector3.Lerp(PlanTip[a], PlanTip[a + 1], t);
            strike = Vector3.Lerp(PlanStrike[a], PlanStrike[a + 1], t);
            root = Vector3.Lerp(PlanRoot[a], PlanRoot[a + 1], t);
        }

        /// <summary>What a path measures: the plan's own figures, and the blade as it went against them.</summary>
        public struct Measures
        {
            /// <summary>
            /// The plan. The fastest the point goes about its landing and how slow it is where the blade turns
            /// (m/s, against where he stands, <b>at the clip's own pace</b>: the walk plays a strike's clip slower
            /// where its blade would outrun a sword, <see cref="LocoBody.BladePaceShare"/>, so these are not speeds
            /// that will be seen); how far it is carried from the landing to the turn (metres along its path) and
            /// how long after the landing that is (seconds of the clip); the nearest the striking part passes to
            /// the target through the cut (metres).
            /// </summary>
            public float PlanSpeed, PlanCarry, PlanTurnAfter, PlanSlowest, PlanMiss;

            /// <summary>As it went: any of it was seen, and it was seen past its landing.</summary>
            public bool Seen, SeenLanding;

            /// <summary>
            /// As it went. The fastest the point went about its landing (m/s, against where he stands, by the game's
            /// clock); how far it went on from the landing over the stretch of the attack the plan's carry is
            /// measured over (metres: to compare with <see cref="PlanCarry"/>); and the nearest the striking part
            /// passed to the target (metres).
            /// </summary>
            public float WentSpeed, WentCarry, WentMiss;

            /// <summary>
            /// How long the blade crawled after it landed (seconds): the steps in the first <see cref="CrawlWithin"/>
            /// seconds after the landing, the freeze on the hit not counted, in which its point went slower than
            /// <see cref="CrawlsUnder"/>. A blade is frozen on a hit on purpose; one that then starts again slowly
            /// is seen to stick at the body.
            /// </summary>
            public float Crawl;

            /// <summary>
            /// How far the point as it went was from the planned path (metres: from the nearest place on the plan's
            /// line, whenever the plan has the point there), on average and at most from the start of the drive to
            /// 0.2 s after the landing; and how far behind its plan it ran (seconds; under 0: ahead): the time of the
            /// clip against the time the plan has the point at that place. A physical sword follows its pose a step
            /// or two late, which at a blade's speed is half a metre: late is not off the path.
            /// </summary>
            public float OffPath, OffPathMost, Behind;

            /// <summary>How long it was frozen on hits (seconds).</summary>
            public float Frozen;
        }

        /// <summary>A blade crawls while its point goes slower than this (m/s), looked for this long after it lands (seconds of the game's clock, a freeze not counted).</summary>
        public const float CrawlsUnder = 5f, CrawlWithin = 0.15f;

        /// <summary>A crawl is looked for only over this share of the planned carry-through, from the landing: nearer its turn the blade is slow by itself (a thrust's point stops and comes back).</summary>
        public const float CrawlShare = 0.6f;

        /// <summary>The planned point nearest a place, looked for between two times of the clip: how far it is, and when the plan has it there.</summary>
        private float NearestOnPlan(Vector3 place, float from, float to, out float when)
        {
            int n = PlanTip.Length, a = Mathf.Clamp(Mathf.FloorToInt(from / PlanStep), 0, n - 2), b = Mathf.Clamp(Mathf.CeilToInt(to / PlanStep), a + 1, n - 1);
            float best = float.PositiveInfinity;
            when = a * PlanStep;
            for (int k = a; k < b; k++)
            {
                Vector3 p = PlanTip[k], q = PlanTip[k + 1], pq = q - p;
                float t = pq.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(place - p, pq) / pq.sqrMagnitude) : 0f;
                float far = Vector3.Distance(place, p + pq * t);
                if (far < best)
                {
                    best = far;
                    when = (k + t) * PlanStep;
                }
            }

            return best;
        }

        /// <summary>The path's measures as far as it has gone.</summary>
        public Measures Measure()
        {
            var r = new Measures();
            int n = PlanTip.Length, hit = Mathf.Clamp(Mathf.RoundToInt(HitAt / PlanStep), 1, n - 2);
            // The plan: the point against where he stands.
            var speed = new float[n];
            for (int k = 0; k < n; k++)
            {
                int a = Mathf.Max(0, k - 1), b = Mathf.Min(n - 1, k + 1);
                speed[k] = ((PlanTip[b] - PlanRoot[b]) - (PlanTip[a] - PlanRoot[a])).magnitude / ((b - a) * PlanStep);
            }

            float peak = 0f;
            for (int k = Mathf.Max(0, hit - 8); k <= Mathf.Min(n - 1, hit + 8); k++)
            {
                peak = Mathf.Max(peak, speed[k]);
            }

            for (int k = Mathf.Max(0, hit - 4); k <= Mathf.Min(n - 1, hit + 4); k++)
            {
                r.PlanSpeed = Mathf.Max(r.PlanSpeed, speed[k]);
            }

            int turn = TurnAfter(speed, hit, peak, PlanStep);
            r.PlanTurnAfter = (turn - hit) * PlanStep;
            r.PlanSlowest = speed[turn];
            for (int k = hit; k < turn; k++)
            {
                r.PlanCarry += ((PlanTip[k + 1] - PlanRoot[k + 1]) - (PlanTip[k] - PlanRoot[k])).magnitude;
            }

            // (The nearest its striking part passes to the spot through the cut: a thrust's point goes on through it.)
            r.PlanMiss = float.PositiveInfinity;
            for (int k = Mathf.Clamp(Mathf.FloorToInt(DriveAt / PlanStep), 0, n - 1); k <= Mathf.Min(n - 1, Mathf.CeilToInt((HitAt + 0.2f) / PlanStep)); k++)
            {
                r.PlanMiss = Mathf.Min(r.PlanMiss, Vector3.Distance(PlanStrike[k], Target));
            }

            r.PlanMiss = float.IsInfinity(r.PlanMiss) ? Vector3.Distance(PlanStrike[hit], Target) : r.PlanMiss;

            // As it went.
            int m = WentAt.Count;
            r.Seen = m > 1;
            if (!r.Seen)
            {
                return r;
            }

            float sum = 0f, late = 0f, miss = float.PositiveInfinity;
            int counted = 0, landed = -1;
            for (int i = 0; i < m; i++)
            {
                r.Frozen += WentFrozen[i] ? stepLength : 0f;
                if (WentAt[i] >= DriveAt && WentAt[i] <= HitAt + 0.2f)
                {
                    float off = NearestOnPlan(WentTip[i], WentAt[i] - 0.3f, WentAt[i] + 0.15f, out float when);
                    r.OffPathMost = Mathf.Max(r.OffPathMost, off);
                    sum += off;
                    late += WentAt[i] - when;
                    counted++;
                    miss = Mathf.Min(miss, Vector3.Distance(WentStrike[i], Target));
                }

                if (landed < 0 && WentAt[i] >= HitAt)
                {
                    landed = i;
                }
            }

            r.OffPath = counted > 0 ? sum / counted : 0f;
            r.Behind = counted > 0 ? late / counted : 0f;
            r.WentMiss = float.IsInfinity(miss) ? 0f : miss;
            r.SeenLanding = landed > 0 && landed < m - 1;
            if (!r.SeenLanding)
            {
                return r;
            }

            // (Its speed by the game's clock, each step it was not frozen on a hit: a freeze is not the blade moving slowly.)
            float until = HitAt + r.PlanTurnAfter, since = 0f;
            bool any = false;
            for (int i = Mathf.Max(1, landed - 3); i < m; i++)
            {
                if (WentFrozen[i] || WentAt[i] <= WentAt[i - 1] + 1e-6f)
                {
                    continue;
                }

                float gone = ((WentTip[i] - WentRoot[i]) - (WentTip[i - 1] - WentRoot[i - 1])).magnitude, pace = gone / stepLength;
                if (i <= landed + 3)
                {
                    r.WentSpeed = Mathf.Max(r.WentSpeed, pace);
                }

                if (i <= landed)
                {
                    continue;
                }

                any = true;
                since += stepLength;
                // (Only while the plan still has the blade well short of its turn: near its turn a blade is slow by itself.)
                if (since <= CrawlWithin + 1e-4f && pace < CrawlsUnder && WentAt[i] <= HitAt + CrawlShare * r.PlanTurnAfter)
                {
                    r.Crawl += stepLength;
                }

                if (WentAt[i] <= until + 1e-4f)
                {
                    r.WentCarry += gone;
                }
            }

            r.SeenLanding = any;
            return r;
        }

        /// <summary>Where a blade turns after it lands, as <see cref="BladeFlow"/> reads a recording's: the first moment it is slowest within a tenth of a second either side and under a third of its fastest; none: where it is slowest in the 0.8 s after.</summary>
        private static int TurnAfter(float[] speed, int hit, float peak, float step)
        {
            int n = speed.Length, lo = Mathf.Min(n - 1, hit + Mathf.Max(1, Mathf.RoundToInt(BladeFlow.NotWithin / step))), hi = Mathf.Min(n - 1, hit + Mathf.RoundToInt(BladeFlow.LooksAfter / step));
            int w = Mathf.Max(1, Mathf.RoundToInt(BladeFlow.TurnOver / step)), best = lo;
            for (int k = lo; k <= hi; k++)
            {
                best = speed[k] < speed[best] ? k : best;
                if (speed[k] >= BladeFlow.TurnsUnder * peak)
                {
                    continue;
                }

                bool least = true;
                for (int g = Mathf.Max(0, k - w); g <= Mathf.Min(n - 1, k + w) && least; g++)
                {
                    least = speed[g] >= speed[k];
                }

                if (least)
                {
                    return k;
                }
            }

            return best;
        }

        /// <summary>The path in words, for the lab's readout, the reports and the playtest log (no full stop at its end).</summary>
        public string Tell()
        {
            var r = Measure();
            var ci = CultureInfo.InvariantCulture;
            string plan = string.Format(ci, "{0}, {1}, {2} to {3}. Planned: lands {4:0.00} s in, {5:0} cm from the spot, carried on {6:0.00} m to its turn",
                Name, OnTheMove ? "on the move" : "planted", BladeFlow.Say(From), BladeFlow.Say(To), HitAt, 100f * r.PlanMiss, r.PlanCarry);
            if (!r.Seen)
            {
                return plan + ". Not seen yet";
            }

            string met = Met.Count == 0 ? "it met nothing" : "it " + string.Join(", then ", Met);
            // (A freeze on a hit is meant; what the blade crawls after it is not.)
            string stuck = r.Frozen <= 0f && r.Crawl <= 0f ? "" : string.Format(ci, "; frozen {0:0.00} s{1}", r.Frozen, r.Crawl > 0f ? string.Format(ci, ", then under {0:0} m/s for {1:0.00} s more", CrawlsUnder, r.Crawl) : "");
            string off = string.Format(ci, "The point {0:0} cm off its planned path on average, {1:0} cm at most, {2:0.00} s {3} it", 100f * r.OffPath, 100f * r.OffPathMost, Mathf.Abs(r.Behind), r.Behind >= 0f ? "behind" : "ahead of");
            if (!r.SeenLanding)
            {
                return plan + string.Format(ci, ". As it went: {0:0.00} s of its {1:0.00} s, and it did not get past its landing; {2}. {3}", WentAt[WentAt.Count - 1], Duration, met, off);
            }

            return plan + string.Format(ci, ". As it went: up to {0:0} m/s as it landed, {1:0} cm from the spot; {2}{3}; carried on {4:0.00} m. {5}",
                r.WentSpeed, 100f * r.WentMiss, met, stuck, r.WentCarry, off);
        }
    }
}
