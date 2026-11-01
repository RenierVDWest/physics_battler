using System;
using System.Collections.Generic;
using PhysicsBattler.Combat;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// What a Fight Rhythm adds to the pose this step, in the unit's space and already in metres
    /// (scaled to the body): the base loop plus the posture under way.
    /// </summary>
    public struct RhythmSample
    {
        public Vector3 hips;      // metres
        public float drop;        // metres the hips go down (bounce + crouch)
        public float lean, roll, turn;
        public float hipTurn;     // degrees the pelvis turns on its own (the chest keeps its turn)
        public Vector2 head;
        public Vector3 main, mainTurn, off, offTurn;

        public static RhythmSample operator +(RhythmSample a, RhythmSample b) => new RhythmSample
        {
            hips = a.hips + b.hips, drop = a.drop + b.drop, lean = a.lean + b.lean, roll = a.roll + b.roll, turn = a.turn + b.turn,
            hipTurn = a.hipTurn + b.hipTurn, head = a.head + b.head, main = a.main + b.main, mainTurn = a.mainTurn + b.mainTurn,
            off = a.off + b.off, offTurn = a.offTurn + b.offTurn,
        };
    }

    /// <summary>
    /// Plays a unit's Fight Rhythm (the director): the base loop always, and postures mixed in over
    /// it. After each posture it waits a while (longer the more tired), then picks the next by its
    /// weight, the mood and the last posture's usual follow-up, never the same twice running. Before
    /// a blow the fight logic asks for a lead-in (<see cref="TryLeadIn"/>). Being swung at makes the
    /// defensive postures likelier for a moment. Mood: energy falls with postures, blows and hits
    /// and comes back slowly; aggression rises with hits landed and falls with hits taken. Pure
    /// maths and timing: the unit carries out the steps, the travel and the weapon feints.
    /// </summary>
    public sealed class RhythmPlayer
    {
        private readonly System.Random random;
        private readonly float[] phase = new float[6];
        private float time, nextAt, fade = 1f, threatUntil, circleSign = 1f, circleFlipAt;
        private PostureDefinition last;

        public RhythmDefinition Definition { get; }

        /// <summary>The posture under way (null: just the base), how far through it (0 to 1), and whether it leads into a blow.</summary>
        public PostureDefinition Current { get; private set; }
        public float Progress { get; private set; }
        public bool CurrentIsLeadIn { get; private set; }
        public bool Fading => Current != null && fade < 1f;

        /// <summary>Its length in seconds for this body (bigger bodies are a little slower).</summary>
        public float Length(PostureDefinition p) => Mathf.Max(0.1f, p.duration * pace);

        public float Energy { get; private set; } = 1f;
        public float Aggression { get; private set; }

        /// <summary>Energy pinned (gallery, tests; negative: free).</summary>
        public float EnergyOverride { get; set; } = -1f;

        /// <summary>
        /// Milestone 8: a tempo drives the mood. Energy comes back only as the tempo gives it
        /// (<see cref="Recharge"/>), and aggression settles back to the style's faster.
        /// </summary>
        public bool TempoDriven { get; set; }

        /// <summary>The tempo states the postures picked now must suit (all, without a tempo).</summary>
        public TempoUse Use { get; set; } = TempoUse.All;

        /// <summary>The unit is walking: only postures that leave the feet alone may play.</summary>
        public bool Walking { get; set; }

        /// <summary>How fast and how big the base loop plays (the tempo's stance: slow and deep gathering, quick pressing).</summary>
        public float LoopSpeed { get; set; } = 1f;
        public float LoopSize { get; set; } = 1f;

        /// <summary>Off, it plays only the base and the postures it is told to (the gallery).</summary>
        public bool Auto { get; set; } = true;

        /// <summary>Who it is fighting (feints and circling need someone to face).</summary>
        public bool HasOpponent { get; set; }

        /// <summary>The body parts open right now (a posture's window, or a guard sagging with fatigue).</summary>
        public AimParts Open { get; private set; }

        /// <summary>How far into the feint's wind-up the weapon is (0 to 1; 0 when no feint).</summary>
        public float Feint { get; private set; }

        /// <summary>The unit's sideways speed circling its opponent, m/s at medium size (+ to its right).</summary>
        public float Circle { get; private set; }

        public event Action<PostureDefinition, bool> Started;
        public event Action<PostureDefinition, bool> Ended;

        /// <summary>A posture already under way became the lead-in to a blow.</summary>
        public event Action<PostureDefinition> LedIn;

        /// <summary>Planned steps that came due this update, for the unit to take.</summary>
        public readonly List<PostureStep> DueSteps = new List<PostureStep>();

        /// <summary>Travel this update (leg lengths, unit space).</summary>
        public Vector3 TravelDelta { get; private set; }
        private Vector3 lastTravel;

        private float pace = 1f;

        // How far round the captured loop it is (0 to 1).
        private float clipPhase;

        public RhythmPlayer(RhythmDefinition definition, int seed)
        {
            Definition = definition;
            random = new System.Random(seed);
            for (int i = 0; i < phase.Length; i++)
            {
                phase[i] = (float)(random.NextDouble() * Math.PI * 2.0);
            }

            Aggression = definition != null ? definition.aggression : 0.5f;
            nextAt = Gap() * 0.5f;
            circleFlipAt = (definition != null ? definition.circleFlip : 3f) * (float)random.NextDouble();
        }

        /// <summary>The body's size: postures take a little longer for bigger bodies.</summary>
        public void SetSize(float size) => pace = Mathf.Sqrt(Mathf.Clamp(size, 0.3f, 3f));

        // ---- Mood -----------------------------------------------------------------------------

        private float EnergyNow => EnergyOverride >= 0f ? EnergyOverride : Energy;

        /// <summary>Energy back (the tempo's regeneration).</summary>
        public void Recharge(float amount) => Energy = Mathf.Min(1f, Energy + Mathf.Max(0f, amount));

        /// <summary>Energy spent (a blow's cost, a dodge).</summary>
        public void SpendEnergy(float amount) => Spend(amount);

        private void Spend(float amount)
        {
            if (Definition != null)
            {
                Energy = Mathf.Max(Definition.minEnergy, Energy - amount);
            }
        }

        /// <summary>The unit starts a blow.</summary>
        public void NoteBlow() => Spend(Definition != null ? Definition.blowCost : 0f);

        /// <summary>The unit's blow landed.</summary>
        public void NoteLanded() => Aggression = Mathf.Clamp01(Aggression + 0.08f);

        /// <summary>The unit was struck (share of its health lost).</summary>
        public void NoteHit(float share)
        {
            Spend((Definition != null ? Definition.hitCost : 0f) * Mathf.Clamp01(share));
            Aggression = Mathf.Clamp01(Aggression - 0.06f);
        }

        /// <summary>A blow is coming at the unit: defensive postures are likelier for a moment.</summary>
        public void NoteThreat() => threatUntil = time + 1.5f;

        // ---- Each step ------------------------------------------------------------------------

        /// <summary>
        /// Advances the rhythm. <paramref name="free"/>: the unit is on its feet with nothing else to
        /// do (no blow, block, stumble): postures may play; otherwise one under way fades out.
        /// </summary>
        public void Update(float dt, bool free)
        {
            DueSteps.Clear();
            TravelDelta = Vector3.zero;
            if (Definition == null || dt <= 0f)
            {
                return;
            }

            time += dt;
            if (Definition.clip != null && !Definition.clip.IsEmpty)
            {
                // Round the loop at the capture's pace (slower on a bigger body), a little slower when tired.
                clipPhase = Mathf.Repeat(clipPhase + dt * LoopSpeed * Mathf.Lerp(0.8f, 1f, EnergyNow) / (Definition.clip.length * pace), 1f);
            }

            if (EnergyOverride < 0f && !TempoDriven)
            {
                Energy = Mathf.Min(1f, Energy + Definition.energyRegen * dt);
            }

            Aggression = Mathf.MoveTowards(Aggression, Definition.aggression, (TempoDriven ? 0.03f : 0.01f) * dt);
            Circle = 0f;
            if (Definition.circling > 0f && HasOpponent && free)
            {
                if (time >= circleFlipAt)
                {
                    circleSign = random.NextDouble() < 0.6 ? -circleSign : circleSign;
                    circleFlipAt = time + Definition.circleFlip * (0.5f + (float)random.NextDouble());
                }

                Circle = circleSign * Definition.circling * Mathf.Lerp(0.4f, 1f, EnergyNow);
            }

            if (Current != null)
            {
                // Once it starts fading (busy for a moment), it fades all the way out: a half-faded
                // posture would play on at reduced size with its steps and openings skipped.
                // (Walking now, a posture that moves the feet gives way to the walk; one the tempo's new
                // state doesn't allow gives way too, unless it is leading into a blow.)
                if (!free || fade < 1f || (Walking && Current.MovesFeet) || ((Current.use & Use) == 0 && !CurrentIsLeadIn))
                {
                    fade = Mathf.Max(0f, fade - dt / 0.15f);
                }

                float length = Length(Current);
                float before = Progress;
                Progress = Mathf.Min(1f, Progress + dt / length);
                if (fade >= 1f)
                {
                    foreach (var s in Current.steps)
                    {
                        if (s.at >= before && s.at < Progress)
                        {
                            DueSteps.Add(s);
                        }
                    }

                    Vector3 travel = Current.Sample(Progress).travel;
                    TravelDelta = travel - lastTravel;
                    lastTravel = travel;
                }

                if (Progress >= 1f || fade <= 0f)
                {
                    Finish(fade < 1f);
                }
            }
            else if (free && Auto && time >= nextAt)
            {
                var next = Pick(false);
                if (next != null)
                {
                    Begin(next, false);
                }
                else
                {
                    nextAt = time + Gap();
                }
            }
            else if (!free)
            {
                // Busy (a blow, a block, off balance): the next posture waits a moment after.
                nextAt = Mathf.Max(nextAt, time + 0.4f);
            }

            Open = Current != null && fade >= 1f ? Current.OpenAt(Progress) : AimParts.None;
            if (EnergyNow < 0.35f)
            {
                Open |= AimParts.Head;   // the guard sags
            }

            Feint = Current != null && !string.IsNullOrEmpty(Current.feintMove) ? Current.FeintAt(Progress) * fade : 0f;
        }

        /// <summary>
        /// Before a blow: with the style's chance (more when aggressive), starts a lead-in posture
        /// and returns how long it takes (seconds); 0 if the blow should go now.
        /// </summary>
        public float TryLeadIn()
        {
            if (Definition == null)
            {
                return 0f;
            }

            // Already in a feint or another lead-in: that is the lead-in; the blow comes as it ends.
            if (Current != null && !Fading && Current.leadIn)
            {
                if (!CurrentIsLeadIn)
                {
                    CurrentIsLeadIn = true;
                    LedIn?.Invoke(Current);
                }

                return (1f - Progress) * Length(Current);
            }

            float chance = Definition.leadInChance * Mathf.Lerp(0.7f, 1.3f, Aggression) * Mathf.Lerp(0.7f, 1f, EnergyNow);
            if (random.NextDouble() >= chance)
            {
                return 0f;
            }

            var p = Pick(true);
            if (p == null)
            {
                return 0f;
            }

            if (Current != null)
            {
                Finish(true);
            }

            Begin(p, true);
            return Length(p);
        }

        /// <summary>Plays a posture now (the gallery, tests).</summary>
        public void Force(PostureDefinition posture)
        {
            if (Current != null)
            {
                Finish(true);
            }

            Begin(posture, false);
        }

        /// <summary>Stops the posture under way at once (knocked down).</summary>
        public void Stop()
        {
            if (Current != null)
            {
                Finish(true);
            }

            Feint = 0f;
            Open = AimParts.None;
        }

        private void Begin(PostureDefinition p, bool leadIn)
        {
            Current = p;
            CurrentIsLeadIn = leadIn;
            Progress = 0f;
            fade = 1f;
            lastTravel = Vector3.zero;
            Spend(Definition != null ? Definition.postureCost : 0f);
            Started?.Invoke(p, leadIn);
        }

        private void Finish(bool interrupted)
        {
            var p = Current;
            last = p;
            Current = null;
            Progress = 0f;
            CurrentIsLeadIn = false;
            fade = 1f;
            nextAt = time + Gap();
            Ended?.Invoke(p, interrupted);
        }

        /// <summary>Seconds until the next posture: the style's gap, longer when tired.</summary>
        private float Gap()
        {
            if (Definition == null)
            {
                return 1f;
            }

            float g = Mathf.Lerp(Definition.gap.x, Definition.gap.y, (float)random.NextDouble());
            return g / Mathf.Max(0.2f, EnergyNow);
        }

        /// <summary>The next posture: weight, times the mood's taste for its kind, times the last one's follow-up; never the last again.</summary>
        private PostureDefinition Pick(bool leadIn)
        {
            float total = 0f;
            var weights = new float[Definition.postures.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                var p = Definition.postures[i];
                if (p == null || p == last || (leadIn && !p.leadIn) || (!HasOpponent && !string.IsNullOrEmpty(p.feintMove))
                    || (p.use & Use) == 0 || (Walking && p.MovesFeet))
                {
                    continue;
                }

                float w = Mathf.Max(0f, p.weight);
                switch (p.kind)
                {
                    case PostureKind.Feint:
                    case PostureKind.Flourish:
                        w *= Mathf.Lerp(0.5f, 1.5f, Aggression);
                        break;
                    case PostureKind.Defence:
                        w *= Mathf.Lerp(1.6f, 0.5f, Aggression) * (time < threatUntil ? 3f : 1f);
                        break;
                }

                if (last != null && last.followedBy == p.Label)
                {
                    w *= Mathf.Max(0f, last.followBias);
                }

                weights[i] = w;
                total += w;
            }

            if (total <= 0f)
            {
                return null;
            }

            double r = random.NextDouble() * total;
            for (int i = 0; i < weights.Length; i++)
            {
                r -= weights[i];
                if (r <= 0.0 && weights[i] > 0f)
                {
                    return Definition.postures[i];
                }
            }

            return null;
        }

        // ---- The pose -------------------------------------------------------------------------

        /// <summary>The base loop plus the posture under way, scaled to a body (leg length, size).</summary>
        /// <param name="baseWeight">How much of the base loop's motion plays (the guards stay): less while the unit walks.</param>
        public RhythmSample Sample(float leg, float size, float baseWeight = 1f)
        {
            var r = new RhythmSample();
            var d = Definition;
            if (d == null)
            {
                return r;
            }

            float e = EnergyNow;
            float t = time;
            float sag = d.tiredGuardDrop * (1f - e) * size;
            var clip = d.clip;
            Vector3 hands = Vector3.zero, offHand = Vector3.zero;
            if (clip != null && !clip.IsEmpty)
            {
                // The captured loop: its rock, bounce, turns, head and hands (smaller when tired).
                var k = clip.Sample(clipPhase);
                float a = clip.strength * Mathf.Lerp(0.4f, 1f, e) * baseWeight * LoopSize;
                r.hips = k.hips * (leg * a);
                r.drop = k.drop * leg * a;
                r.turn = k.turn * a;
                r.hipTurn = k.hipTurn * a;
                r.lean = k.lean * a;
                r.roll = k.roll * a;
                r.head = k.head * a;
                hands = k.main * a;
                offHand = k.off * a;
            }
            else
            {
                // The bounce: down from the stance height and back, livelier when fresh.
                if (d.bounceRate > 0f)
                {
                    r.drop = d.bounceHeight * leg * Mathf.Lerp(0.3f, 1f, e) * Mathf.Abs(Mathf.Sin(Mathf.PI * d.bounceRate * t + phase[0])) * baseWeight;
                }

                float sway = Mathf.Sin(2f * Mathf.PI * d.swayRate * t + phase[1]) * baseWeight;
                r.hips = new Vector3(d.hipSway * leg * sway, 0f, 0f);
                r.head = new Vector2(0f, d.headSway * Mathf.Sin(2f * Mathf.PI * d.swayRate * t + phase[2]) * baseWeight);
                r.roll = 0.5f * d.headSway * sway;
            }

            // The weapon's own motion (with a captured loop too: the capture holds no weapon).
            float w = 2f * Mathf.PI * d.loopRate * t + phase[3];
            Vector3 loop = Vector3.zero, loopTurn = Vector3.zero;
            switch (d.weaponLoop)
            {
                case WeaponLoop.Pump:
                    loop = new Vector3(0f, Mathf.Sin(w), 0.3f * Mathf.Sin(w + 1f)) * d.loopSize;
                    loopTurn = new Vector3(d.loopTurn * Mathf.Sin(w), 0f, 0f);
                    break;
                case WeaponLoop.FigureEight:
                    // (An upright stick: it fans side to side by rolling, nods by pitching.)
                    loop = new Vector3(Mathf.Sin(w), 0.5f * Mathf.Sin(2f * w), 0f) * d.loopSize;
                    loopTurn = new Vector3(0.5f * d.loopTurn * Mathf.Sin(2f * w), 0f, -d.loopTurn * Mathf.Sin(w));
                    break;
                case WeaponLoop.TipCircle:
                    loop = new Vector3(Mathf.Sin(w), Mathf.Cos(w), 0f) * d.loopSize;
                    loopTurn = new Vector3(d.loopTurn * Mathf.Cos(w), d.loopTurn * Mathf.Sin(w), 0f);
                    break;
            }

            r.main = (d.mainGuard + hands + loop * baseWeight) * size + Vector3.down * sag;
            r.mainTurn = d.mainGuardTurn + loopTurn * baseWeight;
            r.off = (d.offGuard + offHand) * size + Vector3.down * (1.3f * sag);
            r.offTurn = d.offGuardTurn;

            if (Current != null)
            {
                var k = Current.Sample(Progress);
                float f = fade;
                r.hips += k.hips * (leg * f);
                r.drop += k.crouch * leg * f;
                r.lean += k.lean * f;
                r.roll += k.roll * f;
                r.turn += k.turn * f;
                r.head += k.head * f;
                r.main += k.main * (size * f);
                r.mainTurn += k.mainTurn * f;
                r.off += k.off * (size * f);
                r.offTurn += k.offTurn * f;
            }

            return r;
        }
    }
}
