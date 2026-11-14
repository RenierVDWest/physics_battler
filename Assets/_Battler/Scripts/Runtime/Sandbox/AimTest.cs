using System.Collections.Generic;
using PhysicsBattler.Locomotion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>One blow asked for a spot on a target, and what came of it (<see cref="AimTest"/>).</summary>
    public sealed class AimBlow
    {
        /// <summary>The spot asked for, the strike that was thrown at it, and how far it expected to miss when it started (metres; 0: it reaches).</summary>
        public AimSpot Spot;
        public string Strike = "";
        public float ExpectedMiss;

        /// <summary>When it was asked (the game's clock, seconds), and that it is over.</summary>
        public float Started;
        public bool Done;

        /// <summary>The first thing its blade met: a part of the target, its shield, or its sword (a parry); when (seconds after it was asked) and how fast the blade was going.</summary>
        public bool Met, Shield, Parried;
        public PartId Part;
        public float MetAfter, MetSpeed;

        /// <summary>How far from the spot the blade touched what it met (metres), and the attacker's size (1: a Medium).</summary>
        public float LandedFrom, Size = 1f;

        /// <summary>
        /// It met the target's body on the spot asked for: the part itself; a neck or a shoulder, which has no solid of its
        /// own, when it landed on a part round it and within <see cref="AimSpots.JointSpotWithin"/> of the spot.
        /// </summary>
        public bool OnSpot => Met && !Shield && !Parried && AimSpots.Counts(Spot, Part) && (!AimSpots.OverAJoint(Spot) || LandedFrom <= AimSpots.JointSpotWithin * Size);

        /// <summary>The nearest its blade's striking part came to the spot before it met anything (metres), and when (seconds after it was asked): where the blow was going, not where it was knocked to.</summary>
        public float Nearest = float.MaxValue, NearestAfter;

        /// <summary>There: how far the edge was turned off the way the blade was going (degrees; 0: edge on). And the furthest the blade as seen was from where its pose had it before it met anything (metres): how well the body carries out the blow it is given.</summary>
        public float EdgeOff, BladeOffPose;

        /// <summary>The furthest the blade as seen was from the path its pose took, before it met anything (metres): off its pose it may only be behind it; off its path it is going somewhere else.</summary>
        public float BladeOffPath;

        /// <summary>The furthest its weight was outside its feet through the blow (metres; 0: over them throughout), and how far outside as the blade met something or came nearest (a blow struck off balance has this well over 0).</summary>
        public float WeightOut, WeightOutThen;

        /// <summary>What it met, in words.</summary>
        public string MetText => !Met ? "nothing" : Shield ? "the shield" : Parried ? "the sword (parried)" : "the " + Part;

        /// <summary>The blow in four short lines, for the panel.</summary>
        public string Lines =>
            $"Asked: the {AimSpots.Name(Spot)} ({Strike}{(ExpectedMiss > 0.005f ? $", expected to fall {100f * ExpectedMiss:0} cm short" : "")})\n"
            + $"Met: {MetText}{(Met ? $", {MetAfter:0.00} s after, at {MetSpeed:0.0} m/s, {100f * LandedFrom:0} cm from the spot" : "")}"
            + $"{(OnSpot ? ". ON THE SPOT" : Met && !Shield && !Parried ? ". Not the spot" : "")}\n"
            + (Nearest < float.MaxValue
                ? $"Blade: came within {100f * Nearest:0} cm of the spot, edge {EdgeOff:0}° off its way, at most {100f * BladeOffPose:0} cm behind its pose and {100f * BladeOffPath:0} cm off its path\n"
                : "Blade: not followed\n")
            + $"Feet: weight {100f * WeightOutThen:0} cm outside them as it landed, {100f * WeightOut:0} cm at most";

        /// <summary>The blow in a line, for the logs.</summary>
        public string Text =>
            $"asked for the {AimSpots.Name(Spot)} with {Strike}: met {MetText}{(Met ? $" {MetAfter:0.00} s after, at {MetSpeed:0.0} m/s, {100f * LandedFrom:0} cm from the spot" : "")}"
            + $"{(OnSpot ? ": the spot asked for" : Met && !Shield && !Parried ? ": not the spot asked for" : "")}; "
            + (Nearest < float.MaxValue
                ? $"its striking part came within {100f * Nearest:0} cm of the spot ({NearestAfter:0.00} s after), the edge {EdgeOff:0}° off its way, the blade at most {100f * BladeOffPose:0} cm from where its pose had it and {100f * BladeOffPath:0} cm off the path its pose took; "
                : "its blade was not followed; ")
            + $"its weight {100f * WeightOutThen:0} cm outside its feet then, {100f * WeightOut:0} cm at most"
            + (ExpectedMiss > 0.005f ? $"; it expected to fall {100f * ExpectedMiss:0} cm short of the spot" : "");
    }

    /// <summary>
    /// The aim test (the plan "Aimed attacks"): one attacker, one target that stands there, and blows asked for one
    /// spot at a time. Each blow is followed from the moment it is asked to its end: what the blade met first, how
    /// near its striking part came to the spot, how true its edge was, how far the blade as seen was from the pose,
    /// and whether the attacker's weight stayed over its feet. Nothing here changes a blow; it asks for one and reads
    /// what happens. The sandbox's Aim test place and the headless <c>FIGHT_LINEUP=aim</c> both run it.
    /// </summary>
    public sealed class AimTest
    {
        public UnitController Attacker, Target;

        /// <summary>The blow under way (or the last one), and every blow asked so far.</summary>
        public AimBlow Now { get; private set; }
        public readonly List<AimBlow> Blows = new List<AimBlow>();

        /// <summary>A blow is followed for at least this long after it is asked, and this long between two of a run (seconds).</summary>
        public const float LeastFollowed = 0.3f, BetweenBlows = 1f;

        /// <summary>The test's two walk back to their spots once they are this far off them (metres at a Medium's size): closer than a target dummy's own 0.3 m, so the distance asked for is the distance struck from.</summary>
        public const float HeldWithin = 0.06f;

        private readonly Queue<AimSpot> waiting = new Queue<AimSpot>();
        private readonly List<Vector3> posedPath = new List<Vector3>();
        private Vector3 lastStrike;
        private bool lastKnown, readSinceMet;
        private float doneAt = -10f;

        /// <summary>Spots still waiting their turn (a run of them: <see cref="OrderAll"/>).</summary>
        public int Waiting => waiting.Count;

        /// <summary>A blow is being followed.</summary>
        public bool Following => Now != null && !Now.Done;

        /// <summary>Each blow as it ends.</summary>
        public event System.Action<AimBlow> Ended;

        /// <summary>
        /// Asks the attacker for one blow at <paramref name="spot"/> now. False when it can't start one (it is busy, down,
        /// or has no strikes) or one is still being followed: nothing is asked.
        /// </summary>
        public bool Order(AimSpot spot, float now)
        {
            if (Attacker == null || Target == null || Following || Attacker.Clip == null || Target.Clip == null)
            {
                return false;
            }

            if (!Attacker.AimStrike(Target, spot, out string strike, out float miss))
            {
                return false;
            }

            Now = new AimBlow { Spot = spot, Strike = strike, ExpectedMiss = miss, Started = now, Size = Attacker.Clip.Size };
            Blows.Add(Now);
            lastKnown = false;
            readSinceMet = false;
            posedPath.Clear();
            return true;
        }

        /// <summary>Every spot in turn, one blow each, a moment apart: asked as the attacker is free for it.</summary>
        public void OrderAll()
        {
            waiting.Clear();
            foreach (var spot in AimSpots.All)
            {
                waiting.Enqueue(spot);
            }
        }

        /// <summary>Begun again with these two: the blows so far are forgotten.</summary>
        public void Begin(UnitController attacker, UnitController target)
        {
            Attacker = attacker;
            Target = target;
            Now = null;
            Blows.Clear();
            waiting.Clear();
            doneAt = -10f;
        }

        /// <summary>The run of blows still waiting is dropped.</summary>
        public void Stop() => waiting.Clear();

        /// <summary>A hit of the fight's: the first by the attacker in the blow being followed is what that blow met.</summary>
        public void OnHit(HitReport hit, float now)
        {
            if (hit == null || !Following || hit.Attacker != Attacker || Now.Met)
            {
                return;
            }

            Now.Met = true;
            Now.Shield = hit.Shield;
            Now.Parried = hit.Parried;
            Now.Part = hit.Part;
            Now.MetAfter = now - Now.Started;
            Now.MetSpeed = hit.Speed;
            Now.LandedFrom = Vector3.Distance(hit.Point, AimSpots.Point(Target.Clip, Now.Spot));
            var ground = Attacker.Clip.Ground;
            if (ground.Known && !float.IsNaN(ground.WeightOut))
            {
                Now.WeightOutThen = ground.WeightOut;
            }
        }

        /// <summary>One step of the game, after the fighters have moved: the blow under way is read, ended when its strike is over, and the next of a run asked.</summary>
        public void Step(float dt, float now)
        {
            if (Attacker == null || Target == null || Attacker.Clip == null || Target.Clip == null)
            {
                return;
            }

            if (Following)
            {
                Read(now);
                if (!Attacker.Clip.Striking && now - Now.Started >= LeastFollowed)
                {
                    Now.Done = true;
                    doneAt = now;
                    if (Diagnostics.PlaytestTrace.Enabled)
                    {
                        Diagnostics.PlaytestTrace.Note(Attacker, "aim", Now.Text);
                    }

                    Ended?.Invoke(Now);
                }

                return;
            }

            if (waiting.Count > 0 && now - doneAt >= BetweenBlows && !Attacker.Clip.Busy && !Target.Clip.Fallen && Attacker.CanAct && OnItsSpot(Attacker) && OnItsSpot(Target))
            {
                var spot = waiting.Peek();
                if (Order(spot, now))
                {
                    waiting.Dequeue();
                }
            }
        }

        /// <summary>A run's next blow waits until both are back where they were put, so each is struck from the same distance.</summary>
        private static bool OnItsSpot(UnitController unit)
        {
            Vector3 off = unit.Root.position - unit.DummySpot;
            off.y = 0f;
            return !unit.TargetDummy || off.magnitude <= 2f * HeldWithin * unit.Body.SizeFactor;
        }

        private void Read(float now)
        {
            var clip = Attacker.Clip;
            var seen = clip.Rig.SlotWeaponR;
            var posed = clip.Anim.SlotWeaponR;
            if (seen == null)
            {
                return;
            }

            float along = clip.Poser.WeaponLength * clip.Poser.SweetSpot;
            Vector3 blade = seen.rotation * Vector3.forward;
            Vector3 strike = seen.position + blade * along;
            Vector3 spot = AimSpots.Point(Target.Clip, Now.Spot);
            if (clip.Ground.Known && !float.IsNaN(clip.Ground.WeightOut))
            {
                Now.WeightOut = Mathf.Max(Now.WeightOut, clip.Ground.WeightOut);
            }

            // (The nearest the striking part came between the last step and this one: a blade at 15 m/s goes 25 cm in a step.)
            Vector3 from = lastKnown ? lastStrike : strike;
            Vector3 way = strike - from;
            float t = way.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(spot - from, way) / way.sqrMagnitude) : 1f;
            float near = Vector3.Distance(spot, from + way * t);
            // (Once it has met something the blade goes where it is knocked: only the way up to there is the blow's own.)
            if (near < Now.Nearest && !readSinceMet)
            {
                Now.Nearest = near;
                Now.NearestAfter = now - Now.Started;
                // (Its edge against the way it is going, across the blade: a sword's two edges lie up and down its slot.)
                Vector3 across = Vector3.ProjectOnPlane(way, blade);
                Vector3 edge = seen.rotation * Vector3.up;
                float off = across.sqrMagnitude > 1e-10f ? Vector3.Angle(edge, across) : 0f;
                Now.EdgeOff = Mathf.Min(off, 180f - off);
                if (!Now.Met && clip.Ground.Known && !float.IsNaN(clip.Ground.WeightOut))
                {
                    Now.WeightOutThen = clip.Ground.WeightOut;
                }
            }

            if (!Now.Met && posed != null)
            {
                Vector3 posedStrike = posed.position + posed.rotation * Vector3.forward * along;
                Now.BladeOffPose = Mathf.Max(Now.BladeOffPose, Vector3.Distance(strike, posedStrike));
                // (Off the path the pose has taken so far: a blade that only trails its pose is on it.)
                posedPath.Add(posedStrike);
                float offPath = Vector3.Distance(strike, posedStrike);
                for (int i = 1; i < posedPath.Count; i++)
                {
                    Vector3 a = posedPath[i - 1], ab = posedPath[i] - a;
                    float u = ab.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(strike - a, ab) / ab.sqrMagnitude) : 0f;
                    offPath = Mathf.Min(offPath, Vector3.Distance(strike, a + ab * u));
                }

                Now.BladeOffPath = Mathf.Max(Now.BladeOffPath, offPath);
            }

            readSinceMet = Now.Met;

            lastStrike = strike;
            lastKnown = true;
        }

        /// <summary>The blows so far, counted: asked, met a body, met the spot asked for, met a shield or a sword, met nothing; and the middle of how near they passed (cm).</summary>
        public string Tally()
        {
            int done = 0, body = 0, spot = 0, stopped = 0, nothing = 0;
            var near = new List<float>();
            var after = new List<float>();
            var edge = new List<float>();
            foreach (var b in Blows)
            {
                if (!b.Done)
                {
                    continue;
                }

                done++;
                if (!b.Met) nothing++;
                else if (b.Shield || b.Parried) stopped++;
                else body++;
                spot += b.OnSpot ? 1 : 0;
                if (b.Nearest < float.MaxValue)
                {
                    near.Add(b.Nearest);
                    edge.Add(b.EdgeOff);
                }

                if (b.Met)
                {
                    after.Add(b.MetAfter);
                }
            }

            near.Sort();
            after.Sort();
            edge.Sort();
            return $"{done} blows: {spot} on the spot asked for, {body - spot} on another part, {stopped} on a shield or a sword, {nothing} on nothing"
                + (near.Count > 0 ? $"; the striking part came within {100f * near[near.Count / 2]:0} cm of the spot at the middle, {100f * near[near.Count - 1]:0} cm at the worst, the edge {edge[edge.Count / 2]:0}° off its way at the middle" : "")
                + (after.Count > 0 ? $"; they met something {after[0]:0.00} to {after[after.Count - 1]:0.00} s after they were asked ({after[after.Count / 2]:0.00} s at the middle)" : "");
        }
    }
}
