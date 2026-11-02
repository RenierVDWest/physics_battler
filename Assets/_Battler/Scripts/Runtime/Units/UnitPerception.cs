using System;
using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// What the unit perceives and knows (milestone 9). It sees from its head: clearly within 30
    /// degrees of where it points, movement out to about 100 degrees each way, and it feels what is
    /// right beside it. It remembers each unit it has perceived (where, when, how it moved) for a few
    /// seconds after losing sight of it. A blow coming at it counts only once it sees it: then it
    /// reacts (later if it caught it only at the edge of its view); one it never sees lands
    /// unopposed, and its attacker is known from then on.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A unit came into another's knowledge: (who, whom, how it was first perceived).</summary>
        public static event Action<UnitController, UnitController, Sight> Noticed;

        /// <summary>A unit it no longer perceived was forgotten: (who, whom).</summary>
        public static event Action<UnitController, UnitController> Forgot;

        /// <summary>How it sees (from its profile; unset values default).</summary>
        public PerceptionSkill Senses { get; private set; } = PerceptionSkill.Default;

        /// <summary>Every unit it knows about: last perceived where, when and how.</summary>
        public Knowledge<UnitController> Known { get; } = new Knowledge<UnitController>();

        /// <summary>A number of its own (ids for what the head looks at).</summary>
        private int unitNumber;

        /// <summary>A blow coming at the unit: whether it has perceived it, how, and when.</summary>
        private sealed class Threat
        {
            public UnitController attacker;
            public MoveState move;
            public bool perceived;
            public Sight sight;
            public float perceivedAt;
            public bool landed;
        }

        private readonly List<Threat> threats = new List<Threat>();

        /// <summary>The blow its shield is raised against: who swung it.</summary>
        private UnitController incomingAttacker;

        private void InitializePerception(int seed)
        {
            Senses = profile.perception.Valid();
            Known.Clear();
            threats.Clear();
            InitializeLook(seed);
        }

        /// <summary>Where it sees from: its head (as posed; the body's own while it lies on the ground).</summary>
        private Transform SightHead => State == UnitState.Down || !IsAlive ? Rig.Part(PartId.Head).transform : Anim.Part(PartId.Head);

        /// <summary>Between the eyes.</summary>
        public Vector3 EyePosition
        {
            get
            {
                var head = SightHead;
                return head.position + head.rotation * (new Vector3(0f, 0.07f, 0.08f) * Body.SizeFactor);
            }
        }

        public Quaternion EyeRotation => SightHead.rotation;

        /// <summary>How it perceives another unit now: the best of its chest, its head and (in a blow) its weapon.</summary>
        public Sight SightOf(UnitController other)
        {
            if (other == null || !other.initialized)
            {
                return Sight.Unseen;
            }

            var head = SightHead;
            Vector3 eye = head.position + head.rotation * (new Vector3(0f, 0.07f, 0.08f) * Body.SizeFactor);
            Quaternion look = head.rotation;
            var skill = Senses;
            float felt = skill.feltRadius * Body.SizeFactor + other.BodyRadius;
            var sight = Perception.Classify(eye, look, other.Anim.Part(PartId.Chest).position, felt, skill);
            if (sight == Sight.Clear)
            {
                return sight;
            }

            var s = Perception.Classify(eye, look, other.Anim.Part(PartId.Head).position, felt, skill);
            if (s > sight)
            {
                sight = s;
            }

            if (other.CurrentMove != null && other.CurrentMove.HasLastStrike)
            {
                s = Perception.Classify(eye, look, other.Pose.StrikePoint, felt, skill);
                if (s > sight)
                {
                    sight = s;
                }
            }

            return sight;
        }

        /// <summary>How it perceives <paramref name="other"/> this moment (unseen if not at all).</summary>
        public Sight Sees(UnitController other) => Known.Current(other);

        /// <summary>Does it know <paramref name="other"/> is there (seen, felt or remembered)?</summary>
        public bool Knows(UnitController other) => other != null && Known.Knows(other);

        /// <summary>Where it thinks <paramref name="other"/>'s chest is: where it is, if it sees it; else remembered.</summary>
        public Vector3 ChestOf(UnitController other)
        {
            if (Sees(other) != Sight.Unseen || !Known.Knows(other))
            {
                return other.Anim.Part(PartId.Chest).position;
            }

            Vector3 at = Known.Estimate(other, LocalTime);
            return at + (other.Anim.Part(PartId.Chest).position - other.transform.position);
        }

        /// <summary>Each step: what it perceives of every other unit, what it forgets, and the blows coming at it it now sees.</summary>
        private void UpdatePerception()
        {
            float now = LocalTime;
            Known.BeginLook(now);
            foreach (var other in all)
            {
                if (other == this || !other.initialized || !other.IsAlive)
                {
                    continue;
                }

                var sight = SightOf(other);
                if (Known.Observe(other, other.transform.position, sight, now))
                {
                    Noticed?.Invoke(this, other, sight);
                }
            }

            foreach (var gone in Known.Forget(now, Senses.memory, u => u == null || !u.IsAlive || !u.initialized))
            {
                if (gone != null)
                {
                    Forgot?.Invoke(this, gone);
                }
            }

            UpdateThreats();
        }

        // ---- Blows coming at it ------------------------------------------------------------------

        /// <summary>A blow was started at this unit: noted; it reacts once (if) it perceives it.</summary>
        private void OnIncoming(UnitController attacker, MoveState move)
        {
            var threat = new Threat { attacker = attacker, move = move };
            threats.Add(threat);
            if (attacker != null)
            {
                CheckThreat(threat);   // (in view already: it reacts from now)
            }
        }

        private void UpdateThreats()
        {
            for (int i = threats.Count - 1; i >= 0; i--)
            {
                var t = threats[i];
                bool coming = t.attacker != null && t.attacker.IsAlive && t.attacker.CurrentMove == t.move && !t.move.ContactPassed;
                if (!coming)
                {
                    if (!t.perceived && !t.landed && t.attacker != null)
                    {
                        Defending?.Invoke(this, t.attacker, t.move, "never saw it (it missed)");
                    }

                    threats.RemoveAt(i);
                    continue;
                }

                if (!t.perceived)
                {
                    CheckThreat(t);
                }
            }
        }

        /// <summary>Does it see the blow now? If so it reacts: it feels the threat, and with a shield may raise it.</summary>
        private void CheckThreat(Threat t)
        {
            var sight = SightOf(t.attacker);
            if (!Perception.CanReact(sight))
            {
                return;
            }

            t.perceived = true;
            t.sight = sight;
            t.perceivedAt = LocalTime;
            Rhythm?.NoteThreat();
            Tempo?.NoteIncoming(FromPress(t.attacker));
            GiveGroundUnder(t.attacker);
            DecideDefence(t.attacker, t.move, sight);
        }

        /// <summary>Milestone 11: the chance it blocks or parries a seen blow (its style's, by its skill; more in the shell, a shield's most there).</summary>
        public float DefenceChance(float styleChance, bool shelled)
        {
            float chance = styleChance * Mathf.Lerp(0.6f, 1.4f, profile.defenceSkill);
            if (shelled)
            {
                chance += ShellDefence;
            }

            return Mathf.Clamp(chance, 0f, styleChance >= 1f ? 1f : 0.92f);   // (never sure, unless told to be)
        }

        /// <summary>Milestone 11: how much sooner (below 1) or later it reacts, by its skill.</summary>
        public float ReactionSkill => Mathf.Lerp(1.3f, 0.75f, profile.defenceSkill);

        /// <summary>Milestone 11: what the shell adds to the chance to block or parry.</summary>
        public const float ShellDefence = 0.25f;

        /// <summary>Milestone 11: a combo's later link, soaked in the shell, is met this much sooner.</summary>
        public const float ExpectedLinkReaction = 0.7f;

        private static bool FromPress(UnitController attacker) => attacker != null && attacker.Tempo != null && attacker.TempoOn && attacker.Tempo.State == TempoState.Press;

        private static string Seen(Sight sight) => sight == Sight.Clear ? "seen clearly" : "seen at the edge of its view";

        /// <summary>
        /// A blow it has just seen: with a shield (and on its feet, and not in a blow of its own) it
        /// may raise it, after its reaction time; without one (milestone 11) it may set its weapon
        /// across the blow, a parry. Shelled up it does either more readily; in the shell, a combo's
        /// later links are expected and met sooner.
        /// </summary>
        private void DecideDefence(UnitController attacker, MoveState move, Sight sight)
        {
            string seen = Seen(sight);
            bool shield = Loadout != null && Loadout.HasShield;
            bool parry = !shield && CanParry && move != null && !move.Move.Close;
            if (!CanAct || (!shield && !parry) || IsAttacking)
            {
                Defending?.Invoke(this, attacker, move, (!CanAct ? "can't defend: off its feet"
                    : IsAttacking ? "can't defend: in a blow of its own"
                    : "nothing to defend with") + $" ({seen})");
                return;
            }

            if (LocalTime < GuardOpenUntil)
            {
                Defending?.Invoke(this, attacker, move, $"can't defend: its guard is broken ({seen})");
                return;
            }

            bool shelled = Tempo != null && TempoOn && Tempo.State == TempoState.Shell;
            float chance = DefenceChance(shield ? Style.blockChance : Style.parryChance, shelled);
            if (combatRandom.NextDouble() >= chance)
            {
                Defending?.Invoke(this, attacker, move, $"{(shield ? "doesn't block" : "doesn't parry")} ({seen})");
                return;
            }

            float reaction = Style.reactionTime * Perception.ReactionScale(sight, Senses) * ReactionSkill;
            bool expected = shelled && attacker != null && attacker.Combo != null && attacker.Combo.Link > 0;
            if (expected)
            {
                reaction *= ExpectedLinkReaction;   // (the next link of a combo it is soaking: it knows it is coming)
            }

            float contactIn = Mathf.Max(0f, move.ContactTime * move.Duration - move.Time);
            string late = reaction > contactIn ? ", too late" : "";
            if (parry)
            {
                Defending?.Invoke(this, attacker, move, $"parries after {reaction:0.00} s ({seen}{(expected ? ", expected" : "")}{late})");
                StartParry(attacker, move, reaction);
                return;
            }

            Defending?.Invoke(this, attacker, move, $"raises the shield after {reaction:0.00} s ({seen}{(expected ? ", expected" : "")}{late})");
            incoming = move;
            incomingAttacker = attacker;
            blockFrom = LocalTime + reaction;
            blockUntil = LocalTime + Mathf.Max(0f, move.Duration * Mathf.Min(1f, move.Move.activeWindow.y + 0.1f) - move.Time);
        }

        /// <summary>It was struck: it feels who did it (known from now on); a blow it never saw lands unopposed and only now puts it under pressure.</summary>
        private void NoteStruck(HitReport hit)
        {
            var attacker = hit.Attacker;
            if (attacker == null || attacker == this)
            {
                return;
            }

            if (Sees(attacker) == Sight.Unseen && Known.Observe(attacker, attacker.transform.position, Sight.Felt, LocalTime))
            {
                Noticed?.Invoke(this, attacker, Sight.Felt);
            }

            foreach (var t in threats)
            {
                if (t.move == hit.Move && !t.perceived && !t.landed)
                {
                    t.landed = true;
                    Tempo?.NoteIncoming(FromPress(attacker));
                    Defending?.Invoke(this, attacker, hit.Move, "never saw it: struck");
                }
            }
        }
    }
}
