using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>A combo under way: its string, how many links it will throw, the one playing.</summary>
    public sealed class ComboRun
    {
        public ComboDefinition Definition;
        public UnitController Target;

        /// <summary>The links it means to throw (the string's, or fewer: the energy it has).</summary>
        public int Length;

        /// <summary>The link playing (0 the first).</summary>
        public int Link;

        /// <summary>Why it came out: press, counter, punish, full gather.</summary>
        public string Why;

        /// <summary>When each link started (unit time), and when each link's contact came.</summary>
        public readonly List<float> LinkStarts = new List<float>();
        public readonly List<float> Contacts = new List<float>();

        /// <summary>The link before was stopped and the string keeps hammering: the next comes once the rebound has played a while.</summary>
        public bool AfterStop;
    }

    /// <summary>
    /// Milestone 10: combos. One combo owns the move slot and hands it from link to link. At a link's
    /// link-out (just after contact) the next link is planned from where the body now is: the lunge
    /// is banked (the root moves to where the hips went, the feet have already stepped there), the
    /// next blow is aimed, and it starts part way into its wind-up (its link-in). For a tenth of a
    /// second the pose blends from the outgoing link's into the incoming one's, so nothing jumps.
    /// Only the last link plays its recovery. A link that doesn't reach (the target gave ground)
    /// ends the combo there, with that link's recovery; a link stopped by a shield or a body ends it,
    /// or (a string that keeps hammering) the next comes as it rebounds; a hit taken ends it.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A combo started, linked, or ended: (unit, combo, what: "started", "link 2", "ended: why").</summary>
        public static event Action<UnitController, ComboRun, string> ComboChanged;

        /// <summary>The combo under way, or null.</summary>
        public ComboRun Combo { get; private set; }

        /// <summary>Seconds the pose takes to ease from one link (playing on) into the next.</summary>
        public const float LinkBlendTime = 0.2f;

        /// <summary>The room the banked root keeps from the target, per unit of each body's size (metres; a body's width and a little).</summary>
        public const float BankRoom = 0.36f;

        /// <summary>The pace a chained link starts at (it picks up to full over about a third of a second).</summary>
        public const float LinkStartPace = 0.5f;

        /// <summary>How far the target may have given ground beyond what a link reaches before the combo ends (metres at medium size; the planner's lunge covers the rest).</summary>
        public const float LinkGiveGround = 0.5f;

        private readonly System.Random comboRandom = new System.Random(17);

        /// <summary>
        /// Plans one link of a combo at its target: its preferred parts first (in a random order),
        /// then the move's other targets. Null if nothing reaches cleanly.
        /// </summary>
        public MoveState PlanLink(ComboDefinition combo, int index, UnitController target, System.Random random)
        {
            if (combo == null || index < 0 || index >= combo.links.Length || target == null || !target.IsInitialized || !target.IsAlive)
            {
                return null;
            }

            var link = combo.links[index];
            var move = ComboDefinition.MoveFor(Loadout?.Main, link.move);
            if (move == null)
            {
                return null;
            }

            random ??= comboRandom;
            var planned = PlanLinkMove(move, link.prefer, target, random);
            if (planned != null || index == 0)
            {
                return planned;
            }

            // (From where the last link left it the string's move doesn't land: another of the
            // weapon's blows does, at the parts the link goes for. A fighter improvises.)
            var weapon = Loadout?.Main;
            if (weapon?.moves == null)
            {
                return null;
            }

            var before = ComboDefinition.MoveFor(weapon, combo.links[index - 1].move);
            var others = new List<MoveDefinition>();
            foreach (var other in weapon.moves)
            {
                if (other != null && other != move && other != before && other.UsableBy(Body.SizeFactor))
                {
                    others.Insert(random.Next(others.Count + 1), other);
                }
            }

            foreach (var other in others)
            {
                planned = PlanLinkMove(other, link.prefer, target, random);
                if (planned != null)
                {
                    return planned;
                }
            }

            return null;
        }

        private MoveState PlanLinkMove(MoveDefinition move, AimParts prefer, UnitController target, System.Random random)
        {
            var link = new ComboLink(MoveStyle.SwordCut, prefer);
            var preferred = new List<AimPart>();
            var others = new List<AimPart>();
            foreach (var part in AimPartInfo.All)
            {
                if (!move.targets.Contains(part) || target.Rig.IsSevered(AimPartInfo.BodyPartOf(part)))
                {
                    continue;
                }

                (link.prefer.Contains(part) ? preferred : others).Insert(random.Next(1 + (link.prefer.Contains(part) ? preferred.Count : others.Count)), part);
            }

            preferred.AddRange(others);
            foreach (var part in preferred)
            {
                var state = MovePlanner.Prepare(this, target, move, part);
                if (state != null && MovePlanner.Acceptable(this, state))
                {
                    return state;
                }
            }

            return null;
        }

        /// <summary>Starts a combo with its first link, planned already (<see cref="PlanLink"/>).</summary>
        public bool StartCombo(ComboDefinition combo, int length, UnitController target, MoveState first, string why = null)
        {
            if (combo == null || first == null || Combo != null || !StartMove(first))
            {
                return false;
            }

            Combo = new ComboRun
            {
                Definition = combo,
                Target = target,
                Length = Mathf.Clamp(length, 1, combo.links.Length),
                Link = 0,
                Why = why ?? "",
            };
            Combo.LinkStarts.Add(LocalTime);
            ComboChanged?.Invoke(this, Combo, "started");
            return true;
        }

        /// <summary>Ends the combo (the link playing plays out as a single blow would).</summary>
        public void EndCombo(string why)
        {
            var combo = Combo;
            if (combo == null)
            {
                return;
            }

            Combo = null;
            ComboChanged?.Invoke(this, combo, "ended: " + why);
        }

        /// <summary>
        /// The end of a step: a link at its link-out hands on to the next; a stopped link ends the
        /// combo or, hammering, the next comes as it rebounds; a finished one ends it.
        /// </summary>
        private void StepCombo()
        {
            var combo = Combo;
            if (combo == null)
            {
                return;
            }

            if (!CanAct || combo.Target == null || !combo.Target.IsAlive)
            {
                EndCombo(!CanAct ? "off its feet" : "the target is gone");
                return;
            }

            var move = CurrentMove;
            if (move == null)
            {
                // The link playing is over: finished, or cut short.
                var stopped = cutShort;
                // (Stopped in a body, the blow landed: the string goes on. Stopped on a shield, it
                // was blocked: the string's rule.)
                string why = stopped != null ? stopped.Cancelled : null;
                bool landed = why != null && why.StartsWith("stopped by") && !why.EndsWith("'s shield");
                bool blocked = why != null && why.StartsWith("stopped by") && why.EndsWith("'s shield");
                bool hammer = combo.Link < combo.Length - 1 && (landed || (blocked && combo.Definition.ifBlocked == ComboBlocked.Continue));
                if (!hammer)
                {
                    EndCombo(stopped != null && stopped.Cancelled != null ? stopped.Cancelled : "finished");
                    return;
                }

                // (Keeps hammering: the next link once the rebound has played a little.)
                combo.AfterStop = true;
                if (stopped.Fade > 0.6f)
                {
                    lastPose.Capture(Anim);
                    lastPoseKnown = true;
                    return;
                }

                NextLink(null);
                lastPoseKnown = false;
                return;
            }

            if (combo.Contacts.Count <= combo.Link && move.ContactPassed)
            {
                combo.Contacts.Add(LocalTime);
            }

            if (combo.Link >= combo.Length - 1)
            {
                lastPoseKnown = false;
                return;   // (the last link plays out, recovery and all)
            }

            if (!move.ContactPassed || move.T < ComboDefinition.LinkOut(move.Move, move.ContactTime))
            {
                // (The pose a step before a hand-over, so the next link carries on its motion.)
                lastPose.Capture(Anim);
                lastPoseKnown = true;
                return;
            }

            NextLink(move);
            lastPoseKnown = false;
        }

        /// <summary>Plans the next link from where the body is now and, if it reaches, hands the move slot on to it.</summary>
        private void NextLink(MoveState outgoing)
        {
            var combo = Combo;
            if (Tempo != null && TempoOn && Rhythm != null && Rhythm.Energy < 0.5f * Tempo.BlowCost)
            {
                EndCombo("out of energy");
                return;
            }

            // The lunge banked: the root goes to where the hips have gone (the lunge's steps have put
            // the feet there), so the next link is planned, and later steps home, from there.
            Vector3 rootWas = transform.position;
            Vector3 bank = Vector3.zero;
            if (outgoing != null)
            {
                bank = outgoing.Lunge * Mathf.Max(0f, outgoing.Body) * outgoing.Blend;
                bank.y = 0f;
                // (Never into the target's room: the root keeps a body's width from it, as the
                // steering keeps bodies apart; the next link's lunge makes up the rest.)
                if (combo.Target != null && bank.sqrMagnitude > 1e-8f)
                {
                    Vector3 to = combo.Target.Root.position - rootWas;
                    to.y = 0f;
                    float keep = BankRoom * (Body.SizeFactor + combo.Target.Body.SizeFactor);
                    float along = Vector3.Dot(bank, to.normalized);
                    float most = Mathf.Max(0f, to.magnitude - keep);
                    if (along > most)
                    {
                        bank *= most / along;
                    }
                }

                transform.position += bank;
            }

            var next = PlanLink(combo.Definition, combo.Link + 1, combo.Target, comboRandom);
            if (next == null)
            {
                transform.position = rootWas;
                EndCombo("the next link doesn't reach");
                return;
            }

            // (The pose as it stands is kept in the world: it hangs off the root, which just moved.)
            Anim.Part(PartId.Pelvis).position -= bank;

            // The pose eases from this link (playing on through its follow-through) into the next.
            var handing = outgoing ?? cutShort;
            if (outgoing != null)
            {
                CurrentMove = null;
                Pose.Move = null;
                Tempo?.NoteBlowEnded();
                MoveEnded?.Invoke(this, outgoing);
            }
            else if (cutShort != null)
            {
                Pose.Move = null;
                cutShort = null;
            }

            linkOut = handing;
            linkOutAge = 0f;
            handOverFrom.Capture(Anim);
            handOverPrevKnown = lastPoseKnown;
            if (lastPoseKnown)
            {
                handOverPrev.CopyFrom(lastPose);
            }

            handOverPending = true;
            Pose.LinkFrom = handing;
            Pose.LinkWeight = 1f;
            // (The arm rebuild: with no blow to hand over from (a link after a stopped one), the weapon
            // arm eases out of where it was held instead.)
            Pose.HoldLinkArm(handing == null);
            Pose.LinkBank = bank;

            combo.Link++;
            combo.AfterStop = false;
            if (!StartMove(next, ComboDefinition.LinkIn(next.Move)))
            {
                EndCombo("couldn't start the next link");
                return;
            }

            // (It starts slowly and picks up, as a blow growing out of the last one does: the arm
            // doesn't leap from the last link's pace into this one's.)
            next.TimeScale = LinkStartPace;
            linkInAt = next.T;
            combo.LinkStarts.Add(LocalTime);
            ComboChanged?.Invoke(this, combo, $"link {combo.Link + 1}: {next.Move.name.Replace("MD_", "")} at the {next.Part}" + (outgoing == null ? ", after a stop" : ""));
        }

        private MoveState linkOut;
        private float linkOutAge, linkInAt;

        // The hand-over's leftover: what the pose differs by between the last step of one link and the
        // first of the next, decaying to nothing over LinkBlendTime (so no part jumps, whatever the
        // two links' poses make of the arm's solve, the lean or the sink).
        private readonly PoseBuffer handOverFrom = new PoseBuffer(), handOverTo = new PoseBuffer(), handOverPrev = new PoseBuffer(), lastPose = new PoseBuffer();
        private bool handOverPrevKnown, lastPoseKnown;
        private readonly Quaternion[] handOverTurn = new Quaternion[PartInfo.Count];
        private Vector3 handOverShift;
        private float handOverAge = -1f;
        private bool handOverPending;

        /// <summary>Right after the pose is composed: what is left of the hand-over's difference, decaying.</summary>
        private void SmoothHandOver(float dt)
        {
            if (handOverPending)
            {
                handOverPending = false;
                handOverTo.Capture(Anim);
                // (Matched to where the last link would have been now, carrying on as it moved: the
                // pose neither stops for a step nor jumps.)
                foreach (var id in PartInfo.All)
                {
                    int i = (int)id;
                    Quaternion from = handOverFrom.Rotations[i];
                    if (handOverPrevKnown)
                    {
                        Quaternion step = from * Quaternion.Inverse(handOverPrev.Rotations[i]);
                        from = step * from;
                    }

                    handOverTurn[i] = from * Quaternion.Inverse(handOverTo.Rotations[i]);
                }

                Vector3 fromPelvis = handOverFrom.PelvisPosition + (handOverPrevKnown ? handOverFrom.PelvisPosition - handOverPrev.PelvisPosition : Vector3.zero);
                handOverShift = fromPelvis - handOverTo.PelvisPosition;
                handOverAge = 0f;
            }

            if (handOverAge < 0f)
            {
                return;
            }

            float k = handOverAge / LinkBlendTime;
            if (k >= 1f)
            {
                handOverAge = -1f;
                return;
            }

            float w = 1f - k * k * (3f - 2f * k);
            // (The feet stay where the stance has them: the legs are solved again to them after.)
            for (int f = 0; f < 2; f++)
            {
                var thigh = Anim.Part(f == 0 ? PartId.ThighL : PartId.ThighR);
                var foot = Anim.Part(f == 0 ? PartId.FootL : PartId.FootR);
                var shin = Anim.Part(f == 0 ? PartId.ShinL : PartId.ShinR);
                feetAt[f] = foot.position;
                feetTurn[f] = foot.rotation;
                knees[f] = shin.position - 0.5f * (thigh.position + foot.position);
            }

            foreach (var id in PartInfo.All)
            {
                if (IsLeg(id))
                {
                    continue;   // (the feet stay where the stance has them)
                }

                // (The arm rebuild: the weapon arm's joints are its rig's, never turned after its solve;
                // its hand, wrist and elbow null are blended from link to link instead, and it rides on
                // the chest as it eases over. The off arm, not on the rig yet, eases over as before.)
                if (AuthorityController.RegionOf(id) == BodyRegion.ArmR)
                {
                    continue;
                }

                var part = Anim.Part(id);
                var turn = Quaternion.Slerp(Quaternion.identity, handOverTurn[(int)id], w);
                if (id == PartId.Pelvis)
                {
                    part.rotation = turn * part.rotation;
                    part.position += handOverShift * w;
                }
                else
                {
                    part.localRotation = turn * part.localRotation;
                }
            }

            for (int f = 0; f < 2; f++)
            {
                var foot = Anim.Part(f == 0 ? PartId.FootL : PartId.FootR);
                TwoBoneIK.Solve(Anim.Part(f == 0 ? PartId.ThighL : PartId.ThighR), Anim.Part(f == 0 ? PartId.ShinL : PartId.ShinR), foot, feetAt[f], knees[f]);
                foot.rotation = feetTurn[f];
            }

            handOverAge += dt;
        }

        private readonly Vector3[] feetAt = new Vector3[2], knees = new Vector3[2];
        private readonly Quaternion[] feetTurn = new Quaternion[2];

        private static bool IsLeg(PartId id) => id == PartId.ThighL || id == PartId.ThighR || id == PartId.ShinL || id == PartId.ShinR || id == PartId.FootL || id == PartId.FootR;

        /// <summary>Before the pose is composed: the last link plays on and gives way to the new one.</summary>
        private void BlendLink(float dt)
        {
            if (linkOut == null)
            {
                if (Pose.LinkArmHeld)
                {
                    // (A link after a stopped one: the held arm let go over the same span as a hand-over.)
                    var playing = CurrentMove;
                    float g = playing != null ? Mathf.Clamp01((playing.T - linkInAt) / Mathf.Max(0.05f, 0.9f * playing.ContactTime - linkInAt)) : 1f;
                    Pose.LinkWeight = 1f - g * g * (3f - 2f * g);
                    if (g >= 1f)
                    {
                        Pose.HoldLinkArm(false);
                        Pose.LinkWeight = 0f;
                    }
                }

                return;
            }

            linkOutAge += dt;
            // (Over the rest of the new link's wind-up: it grows out of the last one, its turns and
            // its weapon's way all its own by the striking moment.)
            var current = CurrentMove;
            float k = current != null ? Mathf.Clamp01((current.T - linkInAt) / Mathf.Max(0.05f, 0.9f * current.ContactTime - linkInAt)) : 1f;
            if (k >= 1f || current == null)
            {
                linkOut = null;
                Pose.LinkFrom = null;
                Pose.LinkWeight = 0f;
                return;
            }

            if (!linkOut.Rewind)
            {
                linkOut.Time = Mathf.Min(linkOut.Duration, linkOut.Time + dt * linkOut.TimeScale);
            }

            Pose.LinkWeight = 1f - k * k * (3f - 2f * k);
        }

        /// <summary>Drops a combo (the unit was reset or knocked out of it).</summary>
        private void ClearCombo()
        {
            Combo = null;
            handOverAge = -1f;
            handOverPending = false;
            linkOut = null;
            Pose.LinkFrom = null;
            Pose.LinkWeight = 0f;
            Pose.HoldLinkArm(false);
        }
    }
}
