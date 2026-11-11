using System;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>What a unit is doing with its energy right now (milestone 8): its tempo.</summary>
    public enum TempoState
    {
        /// <summary>Holding back out of reach, circling, settling its guard: energy comes back fast.</summary>
        Gather,
        /// <summary>Single blows and feints now and then, as before the tempo.</summary>
        Probe,
        /// <summary>Closing in and throwing blow after blow (combos from milestone 9) until spent.</summary>
        Press,
        /// <summary>Under a barrage: guard high, crouched, giving ground (milestone 10 makes it defend harder).</summary>
        Shell,
        /// <summary>The barrage has stopped: one quick answer at once.</summary>
        Counter,
    }

    /// <summary>The tempo states a posture may play in.</summary>
    [Flags]
    public enum TempoUse
    {
        None = 0,
        Gather = 1,
        Probe = 2,
        Press = 4,
        Shell = 8,
        Counter = 16,
        All = Gather | Probe | Press | Shell | Counter,
    }

    /// <summary>
    /// A held stance layered on top of the Fight Rhythm while a unit is in one tempo state: how low
    /// it sits, how far it leans and puts its weight forward, where its hands go, and how quickly
    /// and how big its base loop plays. Held for seconds, it carries the mood (a posture lasts half a
    /// second). Lengths for the medium body: hips and crouch in leg lengths, hands in metres.
    /// </summary>
    [Serializable]
    public struct MoodStance
    {
        [Tooltip("How much lower the hips sit, leg lengths.")]
        public float crouch;
        [Tooltip("Forward lean, degrees (+ forward).")]
        public float lean;
        [Tooltip("The hips (and the weight) forward (+) or back (-), leg lengths.")]
        public float forward;
        [Tooltip("The head nodding down (+: chin down), degrees.")]
        public float chin;
        [Tooltip("The weapon hand moved from its guard, metres (x right, y up, z forward).")]
        public Vector3 main;
        [Tooltip("The off hand (or the shield) moved from its guard, metres.")]
        public Vector3 off;
        [Tooltip("How fast the base loop plays (1: as captured).")]
        public float loopSpeed;
        [Tooltip("How big the base loop plays (1: as captured).")]
        public float loopSize;

        public static MoodStance Neutral => new MoodStance { loopSpeed = 1f, loopSize = 1f };
    }

    /// <summary>Milestone 11: how a style's arms cover it in the shell.</summary>
    public enum ShellGuard
    {
        /// <summary>The stance's own hands (a shield-bearer: the shield raised to the eyes).</summary>
        None = 0,

        /// <summary>The boxer's high shell: both forearms up beside the head, the weapon upright in front, elbows tight over the ribs.</summary>
        High = 1,

        /// <summary>The stick held across the body, low and light.</summary>
        Across = 2,
    }

    /// <summary>
    /// A fighting style's temperament (milestone 8): when it holds back, when it presses and for how
    /// long, how much pressure it takes before it shells up, and what its energy costs and earns.
    /// </summary>
    [Serializable]
    public class Temperament
    {
        [Header("Energy")]
        [Tooltip("Energy back a second while gathering (standing or circling at range).")]
        public float gatherRegen = 0.25f;
        [Tooltip("Energy back a second while probing (between its blows).")]
        public float probeRegen = 0.1f;
        [Tooltip("Energy back a second while walking somewhere (not gathering).")]
        public float walkRegen = 0.05f;
        [Tooltip("Energy back a second in the shell, under pressure.")]
        public float shellRegen = 0.02f;
        [Tooltip("What a probing blow costs.")]
        public float probeCost = 0.06f;
        [Tooltip("What a pressing blow costs.")]
        public float pressCost = 0.15f;

        [Header("When it presses")]
        [Tooltip("Energy that ends a gather in a probe (when it isn't fired up enough to press).")]
        [Range(0f, 1f)] public float probeAt = 0.45f;
        [Tooltip("Energy it presses at (with the aggression below).")]
        [Range(0f, 1f)] public float pressAt = 0.6f;
        [Tooltip("Aggression it needs to press.")]
        [Range(0f, 1f)] public float pressAggression = 0.45f;
        [Tooltip("Energy that ends a press (it goes back to gathering).")]
        [Range(0f, 1f)] public float pressEnd = 0.25f;
        [Tooltip("The most blows in one press.")]
        public int burstMax = 4;
        [Tooltip("Chance a gather holds on until it is fully charged and goes straight into a press (otherwise it probes once rested).")]
        [Range(0f, 1f)] public float holdChance = 0.4f;
        [Tooltip("Energy a long gather holds on to before it explodes into a press.")]
        [Range(0f, 1f)] public float fullCharge = 0.9f;
        [Tooltip("Seconds between probing blows (after the last one ends).")]
        public Vector2 probeInterval = new Vector2(0.9f, 1.8f);
        [Tooltip("Seconds between pressing blows (after the last one ends).")]
        public Vector2 pressGap = new Vector2(0.15f, 0.4f);
        [Tooltip("Gathering this long, it probes anyway once it has the energy (seconds).")]
        public float maxGather = 5f;

        [Header("Under pressure")]
        [Tooltip("Pressure it shells up at (each blow at it adds 0.15, 0.4 from a unit pressing; it fades 0.5 a second after 1.5 s of quiet). Pressing itself, it takes half as much again.")]
        public float composure = 0.5f;
        [Tooltip("The longest it stays shelled up once the blows have stopped coming for a second (seconds).")]
        public float maxShell = 4f;
        [Tooltip("Energy it needs to counter once the barrage stops.")]
        [Range(0f, 1f)] public float counterEnergy = 0.25f;
        [Tooltip("Milestone 11: how its arms cover it in the shell (a shield is raised to the eyes whatever this says).")]
        public ShellGuard shellGuard = ShellGuard.None;

        [Header("Stances (on top of the Fight Rhythm)")]
        public MoodStance gather = MoodStance.Neutral;
        public MoodStance press = MoodStance.Neutral;
        public MoodStance shell = MoodStance.Neutral;
    }

    /// <summary>What the tempo needs to know about the unit and its fight each step.</summary>
    public struct TempoInput
    {
        public float energy, aggression;
        /// <summary>On its feet and able to act.</summary>
        public bool canAct;
        /// <summary>In a blow of its own.</summary>
        public bool attacking;
        public bool walking;
        /// <summary>Close to the one it is fighting (in or near reach).</summary>
        public bool engaged;
        /// <summary>The one it is fighting is reeling, down to its last health or poise: worth pressing.</summary>
        public bool targetVulnerable;
    }

    /// <summary>
    /// A unit's tempo (milestone 8): pure state and timing. Energy builds while it gathers and is
    /// spent in a press; aggression decides when; pressure from the other side's blows pushes it into
    /// its shell, and when the barrage stops it counters. The fight loop asks it whether a blow is
    /// due; the unit poses its stance and pays for its blows.
    /// </summary>
    public sealed class TempoDirector
    {
        public Temperament Temperament { get; }

        public TempoState State { get; private set; } = TempoState.Gather;

        /// <summary>Seconds in the current state.</summary>
        public float StateTime { get; private set; }

        /// <summary>How hard it is being pressed (0 and up: each blow at it adds about 0.25).</summary>
        public float Pressure { get; private set; }

        /// <summary>Blows thrown in the current press.</summary>
        public int BurstBlows { get; private set; }

        /// <summary>Energy a second it gets back right now (the unit adds it to its energy).</summary>
        public float Regen { get; private set; }

        /// <summary>The state changed: (from, to, why).</summary>
        public event Action<TempoState, TempoState, string> Changed;

        private float time, lastIncoming = -999f, lastBlowStart = -999f, untouched;
        private bool countered, broken;

        /// <summary>This gather holds on to a full charge (decided as it starts).</summary>
        private bool holding;
        private readonly System.Random random;
        private int blocked;

        public TempoDirector(Temperament temperament, int seed = 0)
        {
            Temperament = temperament ?? new Temperament();
            random = new System.Random(seed);
            holding = true;   // (the first gather, walking in fresh: it arrives charged)
        }

        /// <summary>Seconds without a blow at it before the pressure starts to fade.</summary>
        public const float QuietTime = 1.5f;

        /// <summary>What its next blow costs.</summary>
        public float BlowCost => State == TempoState.Press || State == TempoState.Counter ? Temperament.pressCost : Temperament.probeCost;

        /// <summary>Whether it throws blows in this state at all (a gatherer only snaps at an opening).</summary>
        public bool Strikes => State == TempoState.Probe || State == TempoState.Press || State == TempoState.Counter;

        /// <summary>Seconds after its last blow ends before the next one (a random unit number picks in the range).</summary>
        public float BlowDelay(float random01)
        {
            var t = Temperament;
            switch (State)
            {
                case TempoState.Press: return Mathf.Lerp(t.pressGap.x, t.pressGap.y, random01);
                case TempoState.Counter: return 0f;
                case TempoState.Probe: return Mathf.Lerp(t.probeInterval.x, t.probeInterval.y, random01);
                default: return float.PositiveInfinity;
            }
        }

        // ---- What happens to it ---------------------------------------------------------------

        /// <summary>A blow is coming at it (one from a unit pressing counts for more).</summary>
        public void NoteIncoming(bool fromPress)
        {
            Pressure = Mathf.Min(MostPressure, Pressure + (fromPress ? 0.4f : 0.15f));
            lastIncoming = time;
        }

        /// <summary>Milestone 11: a press alone takes the pressure to this share of its composure at most; and nothing takes it past <see cref="MostPressure"/>.</summary>
        public const float PressedCap = 1.1f, MostPressure = 3f;

        /// <summary>It was struck (the share of its poise the blow took).</summary>
        public void NoteHit(float poiseShare)
        {
            Pressure = Mathf.Min(MostPressure, Pressure + 0.5f * Mathf.Clamp01(poiseShare));
            lastIncoming = time;
            if (State == TempoState.Press && poiseShare > 0.35f)
            {
                broken = true;   // (a heavy hit breaks a press)
            }
        }

        public void NoteBlowStarted()
        {
            lastBlowStart = time;
            if (State == TempoState.Press)
            {
                BurstBlows++;
            }

            if (State == TempoState.Counter)
            {
                countered = true;
            }
        }

        /// <summary>
        /// Milestone 11: it took a blow on its guard (a block, a parry, its guarding arms). Shelled up,
        /// with the energy to answer and its composure about it, it may riposte at once, between the
        /// barrage's links, instead of waiting for the barrage to end.
        /// </summary>
        public void NoteDefended()
        {
            if (State != TempoState.Shell || energyNow < RiposteEnergy * Temperament.counterEnergy)
            {
                return;
            }

            // (The steadier for a calm temperament, the less for the weight of the barrage.)
            float composed = Mathf.Clamp01(1f - Pressure / MostPressure) * Mathf.Clamp(Temperament.composure / 0.6f, 0.6f, 1.4f);
            if (random.NextDouble() < RiposteChance * composed)
            {
                Go(TempoState.Counter, $"a riposte between links (pressure {Pressure:0.00}, energy {energyNow:0.00})");
            }
        }

        /// <summary>Milestone 11: the chance a fully composed unit ripostes off a blow it took on its guard.</summary>
        public const float RiposteChance = 0.5f;

        /// <summary>Milestone 11: the share of the energy a counter needs that a riposte needs (it is one quick blow).</summary>
        public const float RiposteEnergy = 0.5f;

        private float energyNow;

        /// <summary>When its last blow ended (its own clock).</summary>
        public float LastBlowEnd { get; private set; } = -999f;

        public void NoteBlowEnded() => LastBlowEnd = time;

        // ---- Each step --------------------------------------------------------------------------

        public void Update(float dt, TempoInput input)
        {
            time += dt;
            StateTime += dt;
            // (It fades only once the blows have stopped coming for a moment: a press's next blow
            // follows the last within about two seconds, and the shell waits for the barrage to end.)
            if (time - lastIncoming > QuietTime)
            {
                Pressure = Mathf.Max(0f, Pressure - 0.5f * dt);
            }

            var t = Temperament;
            float e = input.energy;
            energyNow = e;
            untouched = input.engaged ? 0f : untouched + dt;
            bool fired = input.aggression >= t.pressAggression;
            if (input.canAct && !input.attacking)
            {
                switch (State)
                {
                    case TempoState.Gather:
                        // (Walking in from afar it gathers too, but only goes on once it is close.)
                        if (Shelled(input) || !input.engaged || StateTime < 0.8f) break;
                        if (holding && e >= Mathf.Max(t.pressAt, t.fullCharge) && fired)
                        {
                            Go(TempoState.Press, $"fully charged (energy {e:0.00}, aggression {input.aggression:0.00})");
                        }
                        else if (input.targetVulnerable && e >= t.pressEnd + 0.2f && fired)
                        {
                            Go(TempoState.Press, "smells blood");
                        }
                        else if (e >= t.probeAt && (!holding || !fired || StateTime >= t.maxGather))
                        {
                            Go(TempoState.Probe, $"rested (energy {e:0.00})");
                        }

                        break;

                    case TempoState.Probe:
                        if (Shelled(input)) break;
                        if (e >= t.pressAt && fired && input.engaged)
                        {
                            Go(TempoState.Press, $"fired up (energy {e:0.00}, aggression {input.aggression:0.00})");
                        }
                        else if (input.targetVulnerable && e >= t.pressEnd + 0.2f && fired && input.engaged)
                        {
                            Go(TempoState.Press, "smells blood");
                        }
                        else if (e < t.pressEnd + 0.05f)
                        {
                            Go(TempoState.Gather, $"tired (energy {e:0.00})");
                        }

                        break;

                    case TempoState.Press:
                        if (Shelled(input)) break;
                        if (broken)
                        {
                            Go(TempoState.Gather, "the press was broken");
                        }
                        else if (e < t.pressEnd)
                        {
                            Go(TempoState.Gather, $"spent (energy {e:0.00}, {BurstBlows} blows)");
                        }
                        else if (BurstBlows >= Mathf.Max(1, t.burstMax))
                        {
                            Go(TempoState.Gather, $"burst done ({BurstBlows} blows)");
                        }
                        else if (untouched > 1.5f)
                        {
                            Go(TempoState.Probe, "lost touch");
                        }
                        else if (time - Mathf.Max(lastBlowStart, time - StateTime) > 3f)
                        {
                            Go(TempoState.Gather, "nothing to press");
                        }

                        break;

                    case TempoState.Shell:
                        if (Pressure < 0.5f * t.composure || (StateTime > t.maxShell && time - lastIncoming > 1f))
                        {
                            if (e >= t.counterEnergy && input.engaged)
                            {
                                Go(TempoState.Counter, $"the barrage stopped (energy {e:0.00})");
                            }
                            else
                            {
                                Go(TempoState.Gather, $"the barrage stopped, too tired to answer (energy {e:0.00})");
                            }
                        }

                        break;

                    case TempoState.Counter:
                        if (countered)   // (its answer has been thrown and is over: this runs only between blows)
                        {
                            Go(e >= t.pressAt && fired ? TempoState.Press : TempoState.Probe, "answered");
                        }
                        else if (!countered && StateTime > 1.2f)
                        {
                            Go(TempoState.Probe, "no answer came");
                        }

                        break;
                }
            }
            else if (!input.canAct && State == TempoState.Press)
            {
                Go(TempoState.Gather, "knocked out of its press");
            }

            // Energy back: fast while gathering, some while probing, little under pressure, none pressing.
            switch (State)
            {
                case TempoState.Gather:
                    Regen = input.walking ? 0.5f * (t.gatherRegen + t.walkRegen) : t.gatherRegen;
                    break;
                case TempoState.Probe:
                    Regen = input.attacking ? 0f : input.walking ? t.walkRegen : t.probeRegen;
                    break;
                case TempoState.Shell:
                    Regen = t.shellRegen;
                    break;
                default:
                    Regen = 0f;
                    break;
            }
        }

        private bool Shelled(TempoInput input)
        {
            // (Pressing, it is committed: it takes more before it shells up.)
            if (Pressure < Temperament.composure * (State == TempoState.Press ? 1.5f : 1f))
            {
                return false;
            }

            Go(TempoState.Shell, $"pressed (pressure {Pressure:0.00})");
            return true;
        }

        private void Go(TempoState to, string why)
        {
            var from = State;
            State = to;
            StateTime = 0f;
            if (to == TempoState.Press)
            {
                BurstBlows = 0;
                blocked = 0;
                broken = false;
            }

            if (to == TempoState.Counter)
            {
                countered = false;
            }

            if (to == TempoState.Gather)
            {
                holding = random.NextDouble() < Temperament.holdChance;
            }

            Changed?.Invoke(from, to, why);
        }
    }
}
