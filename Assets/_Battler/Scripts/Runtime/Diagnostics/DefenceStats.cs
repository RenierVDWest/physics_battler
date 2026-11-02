using System.Collections.Generic;
using System.Text;
using PhysicsBattler.Motion;
using PhysicsBattler.Units;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Milestone 11's gate, measured in play: while its opponent presses, how much of the time a
    /// unit spends in its shell; how many of the combo links thrown at it it blocks, parries or
    /// takes on its guarding arms; how often its guard breaks; and how many ended bursts it answers
    /// with a counter (a riposte between the links counts). Summed by body (style), for session.md
    /// and the `pressure` runner.
    /// </summary>
    public sealed class DefenceStats
    {
        public sealed class Side
        {
            public float time, pressed, shellPressed;
            public int links, linksDefended, blows, blowsDefended, blocks, parries, armGuards, guardBreaks;
            public int burstsEnded, burstsCountered, ripostes;
        }

        /// <summary>By body (Small, Medium, Large).</summary>
        public readonly Dictionary<string, Side> Sides = new Dictionary<string, Side>();

        private readonly Dictionary<UnitController, float> counteredAt = new Dictionary<UnitController, float>();
        private readonly List<(UnitController defender, float at)> bursts = new List<(UnitController, float)>();

        /// <summary>Seconds around a burst's end in which a counter from the one it pressed answers it.</summary>
        public const float CounterWindowBefore = 1.5f, CounterWindowAfter = 2.5f;

        private float now;

        private static string BodyOf(UnitController unit) => unit != null && unit.Profile != null ? unit.Profile.body : "?";

        private Side Of(UnitController unit)
        {
            string body = BodyOf(unit);
            if (!Sides.TryGetValue(body, out var side))
            {
                side = new Side();
                Sides[body] = side;
            }

            return side;
        }

        /// <summary>Each physics step, for each unit: its time, and its time pressed (in its shell or not).</summary>
        public void Sample(UnitController unit, IEnumerable<UnitController> everyone, float dt, float time)
        {
            now = time;
            if (unit == null || !unit.IsAlive || !unit.IsInitialized || unit.State != UnitState.Standing)
            {
                return;
            }

            var side = Of(unit);
            side.time += dt;
            bool pressed = false;
            foreach (var other in everyone)
            {
                if (other != null && other != unit && other.IsAlive && other.Team != unit.Team && other.TempoOn && other.Tempo != null
                    && other.Tempo.State == TempoState.Press && other.Opponent == unit)
                {
                    pressed = true;
                    break;
                }
            }

            if (pressed)
            {
                side.pressed += dt;
                if (unit.TempoOn && unit.Tempo != null && unit.Tempo.State == TempoState.Shell)
                {
                    side.shellPressed += dt;
                }
            }
        }

        /// <summary>A combo link was thrown (its first, or a later one).</summary>
        public void OnCombo(UnitController attacker, ComboRun combo, string what)
        {
            if (combo?.Target != null && (what == "started" || what.StartsWith("link")))
            {
                Of(combo.Target).links++;
            }
        }

        /// <summary>A blow met something.</summary>
        public void OnHit(HitReport hit)
        {
            if (hit == null || hit.Impact || hit.Move == null || hit.Struck == null || hit.Attacker == null)
            {
                return;
            }

            var side = Of(hit.Struck);
            bool defended = hit.Blocked || hit.ArmGuard;
            side.blows++;
            side.blowsDefended += defended ? 1 : 0;
            side.blocks += hit.Shield ? 1 : 0;
            side.parries += hit.Parried ? 1 : 0;
            side.armGuards += hit.ArmGuard ? 1 : 0;
            if (defended && hit.Attacker.Combo != null && ReferenceEquals(hit.Attacker.Combo.Target, hit.Struck))
            {
                side.linksDefended++;
            }
        }

        public void OnGuardBroken(UnitController unit, HitReport hit) => Of(unit).guardBreaks++;

        /// <summary>A unit's tempo changed: a press ended (a burst to answer), or a counter began.</summary>
        public void OnTempo(UnitController unit, TempoState from, TempoState to, string why)
        {
            if (from == TempoState.Press && to != TempoState.Press && unit.Opponent != null && unit.Opponent.IsAlive)
            {
                bursts.Add((unit.Opponent, now));
                Of(unit.Opponent).burstsEnded++;
            }

            if (to == TempoState.Counter)
            {
                counteredAt[unit] = now;
                if (why != null && why.StartsWith("a riposte"))
                {
                    Of(unit).ripostes++;
                }
            }

            Settle(false);
        }

        /// <summary>Bursts whose window has passed: answered or not.</summary>
        private void Settle(bool all)
        {
            for (int i = bursts.Count - 1; i >= 0; i--)
            {
                var (defender, at) = bursts[i];
                bool answered = counteredAt.TryGetValue(defender, out float when) && when >= at - CounterWindowBefore && when <= at + CounterWindowAfter;
                if (answered || all || now > at + CounterWindowAfter)
                {
                    if (answered)
                    {
                        Of(defender).burstsCountered++;
                    }

                    bursts.RemoveAt(i);
                }
            }
        }

        private static string Pct(float a, float b) => b > 0f ? $"{100f * a / b:0}%" : "n/a";

        /// <summary>The gate's numbers, by body, as markdown lines.</summary>
        public void Write(StringBuilder md)
        {
            Settle(true);
            foreach (var pair in Sides)
            {
                var s = pair.Value;
                md.AppendLine($"- {pair.Key}: pressed {s.pressed:0} s, {Pct(s.shellPressed, s.pressed)} of it in the shell (gate 70%); "
                              + $"{s.linksDefended} of {s.links} combo links defended, {Pct(s.linksDefended, s.links)} (gate 30–60%); "
                              + $"{s.blowsDefended} of {s.blows} blows that met it on its guard ({s.blocks} blocked, {s.parries} parried, {s.armGuards} on its guarding arms); "
                              + $"{s.guardBreaks} guard breaks, {(s.time > 0f ? 60f * s.guardBreaks / s.time : 0f):0.0} a minute on its feet (gate about 1 at most); "
                              + $"{s.burstsCountered} of {s.burstsEnded} bursts at it answered with a counter, {Pct(s.burstsCountered, s.burstsEnded)} (gate 30–50%), {s.ripostes} of them ripostes between links.");
            }
        }
    }
}
