using System;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Milestone 11: the defensive posture's guard. In the shell the style's arms cover it (the
    /// boxer's forearms up beside its head, the eskrimador's stick across its body; a shield-bearer
    /// raises its shield). A blow that lands on a guarding forearm hurts as a forearm does and costs
    /// poise as a block does. Every block and parry costs a little energy. The shell is not a wall: a
    /// blow taken on the guard while the pressure is high and the poise nearly gone breaks it; the
    /// unit staggers with its arms flung open and can't defend for a moment.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A unit's guard was broken: (unit, the blow that broke it).</summary>
        public static event Action<UnitController, HitReport> GuardBroken;

        /// <summary>Until when its guard is open after a guard break (unit time): it can't block or parry.</summary>
        public float GuardOpenUntil { get; private set; } = float.NegativeInfinity;

        /// <summary>Milestone 11: how much ground the shell has given (metres): the fight loop keeps it this much further off.</summary>
        public float GiveGround { get; private set; }

        /// <summary>The ground the shell gives for each blow at it (share of the leg, at random in this range); the small body gives half as much again.</summary>
        public static readonly Vector2 GiveGroundPerLink = new Vector2(0.2f, 0.4f);

        /// <summary>The most ground one shell gives (share of the leg).</summary>
        public const float GiveGroundMost = 1.2f;

        /// <summary>Seconds the guard stays open after it breaks.</summary>
        public const float GuardOpenTime = 0.6f;

        /// <summary>A guard breaks under a blow on it while the pressure is at least this…</summary>
        public const float GuardBreakPressure = 1.6f;

        /// <summary>…and what is left of its poise is under this share.</summary>
        public const float GuardBreakPoise = 0.25f;

        /// <summary>Seconds after a guard break before another can happen (it takes a long barrage to break a guard twice).</summary>
        public const float GuardBreakRest = 40f;

        /// <summary>What a block or a parry costs in energy.</summary>
        public const float BlockEnergy = 0.04f;

        /// <summary>A blow it has seen coming: shelled up, it gives ground under it.</summary>
        private void GiveGroundUnder(UnitController attacker)
        {
            if (Tempo == null || !TempoOn || Tempo.State != TempoState.Shell || attacker == null)
            {
                return;
            }

            float share = Mathf.Lerp(GiveGroundPerLink.x, GiveGroundPerLink.y, (float)combatRandom.NextDouble());
            if (Tempo.Temperament.shellGuard == ShellGuard.Across)
            {
                share *= 1.5f;   // (the light one backs off further)
            }

            GiveGround = Mathf.Min(GiveGround + share * Body.LegLength, GiveGroundMost * Body.LegLength);
        }

        /// <summary>
        /// A blow it took on its guard (a shield, a parry, a guarding forearm): the energy it costs,
        /// and whether it breaks the guard (the reaction then a stagger). Called as the blow's
        /// poise comes off.
        /// </summary>
        private void NoteGuarded(HitReport hit, ref HitResult r)
        {
            if (hit.Blocked)
            {
                Rhythm?.SpendEnergy(BlockEnergy);
            }

            if (Tempo == null || !TempoOn || r.Reaction == Reaction.Killed || LocalTime < GuardOpenUntil)
            {
                return;
            }

            // (A long, well-fed barrage breaks it: the blow from one pressing it or a combo's link.)
            bool barrage = hit.Attacker != null && (hit.Attacker.Combo != null
                || (hit.Attacker.TempoOn && hit.Attacker.Tempo != null && hit.Attacker.Tempo.State == TempoState.Press));
            if (barrage && Tempo.State == TempoState.Shell && Tempo.Pressure >= GuardBreakPressure && Poise < GuardBreakPoise * profile.maxPoise
                && LocalTime >= GuardOpenUntil + GuardBreakRest)
            {
                r.Reaction = Reaction.Stagger;
                GuardOpenUntil = LocalTime + GuardOpenTime;
                DropBlock();
                hit.GuardBreak = true;
                GuardBroken?.Invoke(this, hit);
                return;
            }

            Tempo.NoteDefended();
        }
    }
}
