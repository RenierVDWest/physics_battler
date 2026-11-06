using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Plays a combo (the combat plan's step 5): its first strike aimed, then each next one taken up where
    /// its link has it (<see cref="StrikeLink"/>), aimed afresh, when the last one reaches its link moment,
    /// or ends early (stopped by what it met: the combo goes on from where it backed out).
    ///
    /// With <see cref="Strikes"/> given, the next strike is chosen there and then, as the old move planner
    /// chose a move only if it could land: of the follow-ons in order of their cost, the first that lands on
    /// its target from where the link leaves the body (<see cref="LocoBody.MissAfterPull"/> within
    /// <see cref="LandWithin"/>), a follow-on that needs the body turned less onto its target before one that
    /// needs it turned more (<see cref="TurnWeight"/>: a strike swung with the body turned well off the way it
    /// was played looks it). None lands (the target driven back by the last blow), the combo presses: when the
    /// strike is over it steps in after the target in its guard (<see cref="Want"/>, for whoever moves the walk)
    /// until one would land, and goes on with it; for at most <see cref="PressMost"/>, then it ends.
    /// </summary>
    public sealed class ComboPlayer
    {
        private readonly Locomotor loco;
        private readonly LocoBody poser;

        /// <summary>Where each strike is aimed, as it starts.</summary>
        public Func<LocoClip, Vector3> TargetFor;

        /// <summary>Each strike as it starts (the first's link is null).</summary>
        public event Action<LocoClip, StrikeLink> Started;

        private List<StrikeLink> chain = new List<StrikeLink>();
        private int next;
        private LocoClip current;

        /// <summary>The strikes a combo may go on with (null: the chain given to <see cref="Start"/> as it is).</summary>
        public IList<LocoClip> Strikes;

        /// <summary>The most a follow-on out of a stopped blow may cost (its poses met less well, bridged by the switch's fade).</summary>
        public float ReboundMostCost = 3.2f;

        private bool rebounding;

        /// <summary>The most strikes in a combo, and how near a follow-on must reach its target to be chosen (metres).</summary>
        public int Most = 3;
        public float LandWithin = 0.15f, MostCost = 2.2f;

        /// <summary>What turning the body a full <see cref="LocoBody.MostAimTurn"/> onto the target adds to a follow-on's cost.</summary>
        public float TurnWeight = 1.0f;

        /// <summary>Pressing: the longest it steps in after the target (seconds), and how fast (m/s at a Medium's size).</summary>
        public float PressMost = 3f, PressSpeed = 2.2f;

        /// <summary>Pressing, a follow-on is taken up once the body needs turning at most this onto the target (degrees), for the first half of the pressing.</summary>
        public float PressTurn = 90f;

        /// <summary>Pressing, it stops stepping in once the nearest follow-on would miss by no more than this past landing (metres): the plan's lunge and pull take up the rest.</summary>
        public float StopWithin = 0.15f;

        /// <summary>Stepping in after the target (the strike over, no follow-on landing from where it is).</summary>
        public bool Pressing { get; private set; }

        /// <summary>How the walk should move while pressing (world, m/s; zero otherwise), and where it should face (degrees).</summary>
        public Vector3 Want { get; private set; }
        public float WantFacing { get; private set; }

        private float pressedFor, shotTime;

        /// <summary>The follow-ons weighed for the last strike chosen at a link (for the logs).</summary>
        public string LastChoice = "";

        private readonly System.Text.StringBuilder why = new System.Text.StringBuilder();

        private readonly HashSet<LocoClip> used = new HashSet<LocoClip>();
        private int played;

        /// <summary>Why the combo ended where it did ("" while it goes on).</summary>
        public string Ended = "";

        /// <summary>
        /// A combo of cores (the plan "Combo feel"): the chain given to <see cref="Start"/> played as it is
        /// (<see cref="StrikeCombos.CoreChain"/>), each blow cut at its core's exit (or where what it met stopped it)
        /// and the next taken up at once at its core's entry, its chamber quickened (<see cref="StrikeCombos.CoreChamberSpeed"/>),
        /// the root turned and moved so the hips meet, aimed where the target will be, with a longer step in where the
        /// last blow drove it back (<see cref="MostStepIn"/>). The opener is taken up <see cref="StrikeCombos.OpenLead"/>
        /// before its contact. No pressing, no choosing as it goes. Off: the earlier chooser.
        /// </summary>
        public bool Cores = true;

        /// <summary>A follow-on's longest step in after a target its last blow drove back (metres at a Medium's size), and how near the try steps are.</summary>
        public float MostStepIn = 2f;

        /// <summary>The longer step in the last follow-on took (metres).</summary>
        public float LastStepIn { get; private set; }

        /// <summary>The blow playing is the combo's last, its showstopper.</summary>
        public bool LastBlow => current != null && played >= Mathf.Min(Most, (Cores ? chain.Count : Most - 1) + 1);

        public ComboPlayer(Locomotor loco, LocoBody poser)
        {
            this.loco = loco;
            this.poser = poser;
        }

        /// <summary>The most a follow-on's lunge is stretched (the first strike's, <see cref="LocoBody.MostLunge"/> as it is): a combo goes after a body its blows drive back.</summary>
        public float FollowLunge = 1.7f;

        private float firstLunge = -1f;

        /// <summary>The follow-ons' lunge on (or the first strike's back).</summary>
        private void FollowOns(bool on)
        {
            if (firstLunge < 0f)
            {
                firstLunge = poser.MostLunge;
                firstTurn = poser.MostAimTurn;
            }

            poser.MostLunge = on ? Mathf.Max(firstLunge, FollowLunge) : firstLunge;
            poser.MostAimTurn = on ? Mathf.Max(firstTurn, FollowTurn) : firstTurn;
        }

        /// <summary>The most a follow-on turns the body onto its target in its wind-up (degrees): a combo pivots after its target rather than stopping to turn.</summary>
        public float FollowTurn = 90f;

        private float firstTurn = 60f;

        /// <summary>A combo is under way.</summary>
        public bool Playing => current != null;

        public IReadOnlyList<StrikeLink> Chain => chain;

        public void Start(LocoClip first, List<StrikeLink> links)
        {
            chain = links ?? new List<StrikeLink>();
            next = 0;
            current = first;
            shotTime = 0f;
            used.Clear();
            used.Add(first);
            played = 1;
            Ended = "";
            Pressing = false;
            Want = Vector3.zero;
            FollowOns(false);
            if (Cores && StrikeCores.TryGet(first, out var core))
            {
                // (From the guard straight into its blow: a leap at its takeoff, the rest as late as a beat allows, so
                // long as it lands facing its target (a spinning chop from before its spin).)
                float entry = StrikeCombos.OpenerEntry(first);
                var target = Target(first, entry);
                Vector3 stepIn = StepIn(first, target, entry, loco.Leg / 0.845f);
                LastStepIn = stepIn.magnitude;
                poser.PlayStrike(first, target, startFrom: entry, stepIn: stepIn);
                loco.Chamber(StrikeCombos.CoreChamberSpeed(core.Contact - entry), core.Contact, chain.Count > 0 ? FollowSpeed : 1f);
                magnetFor = first;
                shotTime = entry;
            }
            else
            {
                poser.PlayStrike(first, Target(first));
            }

            FollowOns(true);
            Started?.Invoke(first, null);
        }

        /// <summary>A combo of cores: the blow playing goes on to its core's exit (or ends, or is stopped), then the next is taken up.</summary>
        private void StepCores(bool still)
        {
            // (Past the blow's contact, met short of it (backing out of a block or parry) or over, with another to come:
            // whoever plays it may turn the next on someone else.)
            if (PastContact != null && current != null && contactPassed != current && next < chain.Count && played < Most
                && (!still || loco.Shot != current || loco.ShotSpeed < 0f || (StrikeCores.TryGet(current, out var playing) && loco.ShotTime >= playing.Contact)))
            {
                contactPassed = current;
                PastContact(played);
            }

            if (next >= chain.Count || played >= Most)
            {
                if (!still)
                {
                    current = null;
                }

                return;
            }

            if (still && StrikeCores.TryGet(current, out var core) && loco.ShotTime < core.Exit)
            {
                return;
            }

            float at = Mathf.Min(shotTime, current.Duration);
            var to = Reconsider(chain[next].To, at);
            if (to != chain[next].To)
            {
                // (The rest of the plan from the blow taken instead.)
                LastChoice = $"{chain[next].To.Name} would turn the body {plannedTurn:0} degrees onto its target from where the last blow left it; {to.Name} instead";
                chain[next] = StrikeCombos.CoreLink(current, to);
                if (next + 1 < chain.Count && chain[next + 1].To != to)
                {
                    chain[next + 1] = StrikeCombos.CoreLink(to, chain[next + 1].To);
                }
                else if (next + 1 < chain.Count)
                {
                    chain.RemoveAt(next + 1);
                }
            }

            GoCore(to, at);
            next++;
        }

        /// <summary>The strike picked for the combo: never swapped for another as it goes.</summary>
        public LocoClip Keep;

        /// <summary>
        /// Called once a blow with another to follow has passed its contact, with how many it has played: the moment to
        /// turn the next on another target (<see cref="TargetFor"/>; the plan "Fighting off several attackers", step 8).
        /// </summary>
        public Action<int> PastContact;

        private LocoClip contactPassed;

        /// <summary>
        /// The blow it takes up next, as the one it strikes at can see it coming (the weight plan's step 1). Taken up at
        /// its core's entry, a combo's next blow comes into view only 0.15 to 0.3 s before its contact, often on the other
        /// side, but a fighter reads it coming out of the last one's follow-through. Its clip, where its core is taken
        /// up, how long until its contact (seconds as they pass), and its plan as the strike will be played: the root (the
        /// link's, stepped in) and facing (turned onto its target) it plays from, the lunge's stretch, and the arm's pull
        /// (world) as far as it goes; and, when known, the grip and the blade's way at contact (world). False when none
        /// is planned (or it is not a combo of cores).
        /// </summary>
        public bool Upcoming(out LocoClip clip, out float entry, out float toContact, out Vector3 rootAt, out float facing, out float lunge, out Vector3 pull, out bool bladeKnown, out Vector3 grip, out Vector3 way)
        {
            clip = null;
            entry = toContact = facing = 0f;
            lunge = 1f;
            rootAt = pull = grip = way = Vector3.zero;
            bladeKnown = false;
            if (!Cores || current == null || next >= chain.Count || played >= Most || loco.Current != Locomotor.Mode.OneShot || loco.Shot != current
                || !StrikeCores.TryGet(current, out var core) || !StrikeCores.TryGet(chain[next].To, out var nextCore))
            {
                return false;
            }

            clip = chain[next].To;
            entry = nextCore.Entry;
            float pace = Mathf.Max(0.05f, loco.TimeScale);
            // (A blow met, held in its bind or backing out of it, takes its next one up at once, from where it is.)
            bool met = loco.ShotSpeed <= 0f;
            float from = met ? Mathf.Min(loco.ShotTime, core.Exit) : core.Exit;
            float untilExit = met ? 0f : Mathf.Max(0f, core.Exit - loco.ShotTime) / (Mathf.Max(0.3f, loco.ShotRate) * pace);
            // (Its chamber quickened from s down to its own pace: on average (2s + 1) / 3 times as fast.)
            float chamber = StrikeCombos.CoreChamberSpeed(nextCore.Lead);
            toContact = untilExit + nextCore.Lead / ((2f * chamber + 1f) / 3f * pace);
            if (upcomingFrom != current || upcomingTo != clip || Mathf.Abs(upcomingAt - from) > 0.02f)
            {
                upcomingFrom = current;
                upcomingTo = clip;
                upcomingAt = from;
                upcomingMeet = StrikeCombos.Meet(current, from, clip, entry);
            }

            float scale = loco.Leg / 0.845f;
            facing = loco.Facing + upcomingMeet.Turn;
            rootAt = loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * upcomingMeet.Shift * scale;
            Vector3 aim = Target(clip, entry);
            rootAt += StepIn(clip, aim, entry, scale, rootAt, facing);
            poser.PlanFrom(clip, aim, entry, rootAt, facing, out float by, out lunge, out pull, out Vector3 miss);
            // (Where its blade will be at contact, as the plan has it: what the one it strikes at reads coming.)
            bladeKnown = poser.PlannedBlade(clip, aim, entry, rootAt, facing, pull, out grip, out way);
            if (Magnetic && TargetFor != null)
            {
                // (Its wind-up's magnet will carry the body across what the swing misses by, as far as it goes: the pull
                // takes up only the rest.)
                Vector3 snap = Snap(miss, rootAt, MostSnap * scale);
                rootAt += snap;
                pull = Vector3.ClampMagnitude(pull - snap, poser.MostAimPull);
            }

            facing += by;
            return true;
        }

        /// <summary>The magnet's carry across <paramref name="miss"/> from a root there: flat, no nearer the target's body than the fighting distance, at most <paramref name="most"/>.</summary>
        private Vector3 Snap(Vector3 miss, Vector3 rootAt, float most)
        {
            Vector3 off = miss;
            off.y = 0f;
            if (poser.KeepFrom.HasValue && poser.KeepDistance > 0f)
            {
                Vector3 from = rootAt + off - poser.KeepFrom.Value;
                from.y = 0f;
                if (from.magnitude < poser.KeepDistance)
                {
                    off += (from.sqrMagnitude > 1e-6f ? from.normalized : -off.normalized) * poser.KeepDistance - from;
                }
            }

            return Vector3.ClampMagnitude(off, most);
        }

        private LocoClip upcomingFrom, upcomingTo;
        private float upcomingAt;
        private StrikeLink upcomingMeet;

        /// <summary>A follow-on turns the body at most this much onto its target in its wind-up (degrees): past it, another is taken.</summary>
        public float MostCoreTurn = StrikeCombos.MostAim;

        private float plannedTurn;

        /// <summary>
        /// The planned next blow, or, if it would turn the body more than <see cref="MostCoreTurn"/> onto its target from
        /// where the last one actually left it (a blow stopped early has turned less than its plan), the one of the same
        /// part (a showstopper for the last, a quick blow before) not yet used that turns it least and meets best.
        /// </summary>
        private LocoClip Reconsider(LocoClip planned, float at)
        {
            if (Strikes == null || planned == Keep)
            {
                return planned;
            }

            float Turn(LocoClip to, out float cost)
            {
                StrikeCores.TryGet(to, out var core);
                var meet = StrikeCombos.Meet(current, at, to, core.Entry);
                float scale = loco.Leg / 0.845f;
                float facing = loco.Facing + meet.Turn;
                Vector3 rootAt = loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * meet.Shift * scale;
                cost = meet.Cost;
                return Mathf.Abs(poser.AimTurnFor(to, Target(to, core.Entry), core.Entry, rootAt, facing));
            }

            plannedTurn = Turn(planned, out _);
            if (plannedTurn <= MostCoreTurn)
            {
                return planned;
            }

            bool last = next == chain.Count - 1;
            var finishers = StrikeCombos.Finishers(Strikes);
            LocoClip best = planned;
            float bestScore = plannedTurn / 90f * 3f;
            foreach (var c in Strikes)
            {
                // (Of the same part and the same sweep: the left, right, left kept.)
                if (used.Contains(c) || c == planned || finishers.Contains(c) != last || !StrikeCores.TryGet(c, out var core) || (core.Leap && !last)
                    || StrikeCombos.Side(c) != StrikeCombos.Side(planned))
                {
                    continue;
                }

                float turn = Turn(c, out float cost);
                if (turn > MostCoreTurn)
                {
                    continue;
                }

                float score = cost * 0.3f + turn / 90f * 3f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }

            return best;
        }

        private void GoCore(LocoClip to, float at)
        {
            StrikeCores.TryGet(to, out var core);
            float entry = core.Entry;
            var meet = StrikeCombos.Meet(current, at, to, entry);
            float scale = loco.Leg / 0.845f;
            loco.TurnRoot(meet.Turn, Quaternion.Euler(0f, loco.Facing, 0f) * meet.Shift * scale);
            var target = Target(to, entry);
            Vector3 stepIn = StepIn(to, target, entry, scale);
            LastStepIn = stepIn.magnitude;
            poser.PlayStrike(to, target, startFrom: entry, stepIn: stepIn);
            loco.Chamber(StrikeCombos.CoreChamberSpeed(core.Lead), core.Contact, next + 1 < chain.Count ? FollowSpeed : 1f);
            magnetFor = to;
            current = to;
            shotTime = entry;
            used.Add(to);
            played++;
            Started?.Invoke(to, meet);
        }

        /// <summary>The longer step in a follow-on needs to land on a target driven back (none if it lands from where the body is): straight toward it, no nearer the target's body than the fighting distance.</summary>
        private Vector3 StepIn(LocoClip to, Vector3 target, float entry, float scale) => StepIn(to, target, entry, scale, loco.Position, loco.Facing);

        private Vector3 StepIn(LocoClip to, Vector3 target, float entry, float scale, Vector3 rootAt, float facing)
        {
            float miss = poser.MissBeforePull(to, target, entry, rootAt, facing);
            if (miss <= StepWithin)
            {
                return Vector3.zero;
            }

            Vector3 way = target - rootAt;
            way.y = 0f;
            if (way.sqrMagnitude < 1e-6f)
            {
                return Vector3.zero;
            }

            way.Normalize();
            Vector3 best = Vector3.zero;
            for (float d = 0.05f; d <= MostStepIn * scale + 1e-4f; d += 0.05f)
            {
                Vector3 at = rootAt + way * d;
                if (poser.KeepFrom.HasValue && poser.KeepDistance > 0f)
                {
                    Vector3 off = at - poser.KeepFrom.Value;
                    off.y = 0f;
                    if (off.magnitude < poser.KeepDistance + 0.02f)
                    {
                        break;
                    }
                }

                float m = poser.MissBeforePull(to, target, entry, at, facing);
                if (m < miss - 0.01f)
                {
                    miss = m;
                    best = way * d;
                }

                if (miss <= StepWithin)
                {
                    break;
                }
            }

            return best;
        }

        /// <summary>
        /// A combo's magnet (the playtest: let the blows knock the target back, and pull the attacker in after it): through
        /// each blow's wind-up the body is carried after its target as it moves off from where the blow was aimed, the
        /// gap closed by contact, never nearer than the fighting distance. Fast as a combo is, the feet keeping up with
        /// it doesn't show.
        /// </summary>
        public bool Magnetic = true;

        /// <summary>A combo blow's follow-through (its slash swept on through contact to its core's exit) plays this much faster; the showstopper's at its own pace.</summary>
        public float FollowSpeed = 1.8f;

        private LocoClip magnetFor;

        /// <summary>How fast the magnet snaps the body in (seconds, the e-folding: most of the gap is closed in twice this).</summary>
        public float SnapTime = 0.03f;

        /// <summary>
        /// The most the magnet carries the body over one blow, and in one step (metres at a Medium's size): the playtest,
        /// a Large snapped 1.5 m in a frame after a dummy it had driven off, its physical body was left behind and it went
        /// down, sliding across the board. Beyond it, the arm's pull and the miss.
        /// </summary>
        public float MostSnap = 0.6f, MostSnapStep = 0.12f;

        private float snapped;
        private LocoClip snappedFor;

        private void Magnet(float dt)
        {
            if (!Cores || !Magnetic || TargetFor == null || current == null || magnetFor != current || loco.Shot != current
                || loco.Current != Locomotor.Mode.OneShot || !StrikeCores.TryGet(current, out var core) || loco.ShotTime >= core.Contact)
            {
                return;
            }

            // (Where the blade lands as the strike swings it, without the arm's pull, against where its target will be
            // then: the body snapped across the difference, so the swing itself lands it and the arm needn't reach.)
            float left = (core.Contact - loco.ShotTime) / Mathf.Max(0.3f, loco.ShotRate);
            Vector3 v = TargetVelocity != null ? TargetVelocity() : Vector3.zero;
            v.y = 0f;
            // (Led only by what the snap lags it: snapped every step, it follows as the target moves; led by the whole
            // wind-up, a body driven off and slowing was overshot by 0.3 m.)
            Vector3 future = TargetFor(current) + v * Mathf.Min(left, 2f * SnapTime);
            Vector3 off = future - poser.AimPredicted;
            off.y = 0f;
            Vector3 move = off * (1f - Mathf.Exp(-dt / Mathf.Max(1e-3f, SnapTime)));
            if (poser.KeepFrom.HasValue && poser.KeepDistance > 0f)
            {
                // (No nearer than the fighting distance, of where the target's body is now.)
                Vector3 from = loco.Position + move - poser.KeepFrom.Value;
                from.y = 0f;
                if (from.magnitude < poser.KeepDistance)
                {
                    Vector3 back = (from.sqrMagnitude > 1e-6f ? from.normalized : -off.normalized) * poser.KeepDistance - from;
                    move += back;
                }
            }

            if (snappedFor != current)
            {
                snappedFor = current;
                snapped = 0f;
            }

            float scale = loco.Leg / 0.845f;
            move = Vector3.ClampMagnitude(move, Mathf.Min(MostSnapStep * scale, Mathf.Max(0f, MostSnap * scale - snapped)));
            snapped += move.magnitude;
            loco.Position += move;
            poser.SnappedBy(move);
            Carried?.Invoke(move);
        }

        /// <summary>The magnet moved the body this far at once (world): a physical body goes with it (<see cref="ClipFighter.CarryBy"/>).</summary>
        public Action<Vector3> Carried;

        /// <summary>A follow-on steps in until its blade would reach within this of its target before the arm's pull (metres): the pull is for the last of it, not the distance.</summary>
        public float StepWithin = 0.03f;

        public void Stop()
        {
            FollowOns(false);
            current = null;
            Pressing = false;
            loco.Hurry = false;
            Want = Vector3.zero;
        }

        /// <summary>How the target is moving (world, m/s; null: it stands): a strike is aimed where it will be.</summary>
        public Func<Vector3> TargetVelocity;

        /// <summary>How much of the target's way it is led by, at most how far ahead (seconds): a driven body slows.</summary>
        public const float LeadShare = 0.9f, MostLead = 0.8f;

        private Vector3 Target(LocoClip clip, float startFrom = 0f)
        {
            Vector3 at = TargetFor != null ? TargetFor(clip) : poser.NominalTarget();
            if (TargetVelocity != null && clip != null && clip.ContactFrame >= 0)
            {
                Vector3 v = TargetVelocity();
                v.y = 0f;
                at += v * (LeadShare * Mathf.Clamp(clip.ContactFrame / clip.Rate - startFrom, 0f, MostLead));
            }

            return at;
        }

        /// <summary>The follow-ons of the strike playing (not yet used, cost at most <see cref="MostCost"/>; any cost, <paramref name="fromGuard"/>: taken up from the guard, not out of it).</summary>
        private List<StrikeLink> Options(bool fromGuard = false)
        {
            var options = new List<StrikeLink>();
            foreach (var to in Strikes)
            {
                if (!used.Contains(to))
                {
                    var link = StrikeCombos.Link(current, to);
                    if (fromGuard || link.Cost <= MostCost)
                    {
                        options.Add(link);
                    }
                }
            }

            return options;
        }

        private void Go(StrikeLink link)
        {
            loco.TurnRoot(link.Turn, Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * (loco.Leg / 0.845f));
            poser.PlayStrike(link.To, Target(link.To, link.Entry), startFrom: link.Entry);
            // (Its chamber quickened: the next blow's wind-up, not the blow.)
            loco.Chamber(StrikeCombos.ChamberSpeed, link.To.ContactFrame / link.To.Rate);
            current = link.To;
            shotTime = link.Entry;
            used.Add(link.To);
            played++;
            Started?.Invoke(link.To, link);
        }

        /// <summary>
        /// The follow-ons of the strike playing that would land from where its link leaves the body, best first;
        /// the time to go is the earliest of their links (or the strike's end).
        /// </summary>
        private void StepChoosing(bool still)
        {
            if (played >= Most)
            {
                if (!still)
                {
                    current = null;
                }

                return;
            }

            // (Each follow-on's link has its own moment: the strike goes on until the first of them.)
            var options = Options();
            if (!still && options.Count > 0)
            {
                // (Over before its links' moments, cut short by what it met: the follow-ons are taken up from where
                // it stopped, the body turned and placed as it is there, not as it would have been at its link mark.)
                for (int i = 0; i < options.Count; i++)
                {
                    if (shotTime + 1e-3f < options[i].At)
                    {
                        var cut = StrikeCombos.BestAt(current, shotTime, options[i].To);
                        options[i] = cut;
                    }
                }

                float cheapest = float.MaxValue;
                string cheapestName = "";
                foreach (var l in options)
                {
                    if (l.Cost < cheapest)
                    {
                        cheapest = l.Cost;
                        cheapestName = l.To.Name;
                    }
                }

                // (Out of a stopped blow, a poorer meeting of the poses is bridged: the switch's fade carries it.)
                float most = rebounding ? ReboundMostCost : MostCost;
                rebounding = false;
                options.RemoveAll(l => l.Cost > most);
                if (options.Count == 0)
                {
                    // (Stopped mid-blow, its pose meets no follow-on's wind-up: it backs out into its guard, and the
                    // combo goes on from there, as pressing does, facing the target first.)
                    Pressing = true;
                    pressedFor = 0f;
                    Ended = $"cut short at {shotTime:0.00} s: no follow-on meets it there (the nearest, {cheapestName}, costs {cheapest:0.00}); the next is taken up from the guard";
                    return;
                }
            }

            if (options.Count == 0)
            {
                // (None follows on out of this strike quickly enough: its recovery is cut short, and the next is taken
                // up from the guard, as pressing does.)
                CutRecovery(still);
                if (!still)
                {
                    Pressing = true;
                    pressedFor = 0f;
                    Ended = "no follow-on out of it; the next is taken up from the guard";
                }

                return;
            }

            float scale = loco.Leg / 0.845f;
            why.Clear();
            StrikeLink best = null, nearest = null;
            float nearestMiss = float.MaxValue, bestScore = float.MaxValue;
            foreach (var link in options)
            {
                // (From where the link leaves the root: turned and moved there.)
                Vector3 rootAt = loco.Position + Quaternion.Euler(0f, loco.Facing, 0f) * link.Shift * scale;
                float facing = loco.Facing + link.Turn;
                var target = Target(link.To, link.Entry);
                float miss = poser.MissAfterPull(link.To, target, link.Entry, rootAt, facing);
                if (miss < nearestMiss)
                {
                    nearestMiss = miss;
                    nearest = link;
                }

                float turnNeeded = poser.AimTurnFor(link.To, target, link.Entry, rootAt, facing);
                why.Append($"{link.To.Name.Replace("sns_", "")} cost {link.Cost:0.00} miss {miss:0.00} turn {turnNeeded:+0;-0}; ");
                if (miss <= LandWithin)
                {
                    float turn = Mathf.Abs(turnNeeded);
                    float score = link.Cost + TurnWeight * turn / Mathf.Max(1f, poser.MostAimTurn);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = link;
                    }
                }
            }

            if (best == null)
            {
                // (None lands from here: the strike plays on a little (the body moves); then the combo presses.)
                if (still)
                {
                    CutRecovery(true);
                    return;
                }

                Pressing = true;
                pressedFor = 0f;
                Ended = $"no follow-on would land from where the strike left it (the nearest, {nearest?.To.Name}, would miss by {nearestMiss:0.00} m)";
                return;
            }

            if (still && loco.ShotTime < best.At)
            {
                return;
            }

            LastChoice = why.ToString();

            Go(best);
        }

        /// <summary>How long after its contact a strike with no follow-on out of it plays on before it is ended (seconds of its clip): a combo doesn't wait out a long recovery.</summary>
        public const float RecoverFor = 0.3f;

        /// <summary>A pressed-in strike is taken up this far before its contact (seconds of its clip), its chamber quickened: from the guard, the wind-up's start is skipped.</summary>
        public const float PressEntryLead = 0.45f;

        private void CutRecovery(bool still)
        {
            if (still && current != null && current.ContactFrame >= 0 && loco.ShotTime > current.ContactFrame / current.Rate + RecoverFor)
            {
                loco.EndShotNow();
            }
        }

        private static float PressEntry(LocoClip clip) => clip.ContactFrame < 0 ? 0f : Mathf.Max(0f, clip.ContactFrame / clip.Rate - PressEntryLead);

        /// <summary>
        /// Pressing: stepping in after the target in the guard until a follow-on of the last strike would land from
        /// where the body is, played from its start (the last strike is over: there is no link into it now).
        /// </summary>
        private void StepPressing(float dt)
        {
            pressedFor += dt;
            Vector3 target = Target(current);
            Vector3 to = target - loco.Position;
            to.y = 0f;
            float toward = to.sqrMagnitude > 1e-4f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : loco.Facing;
            // (Turned to face the target only when it is well off to the side: less than that, the strike's own
            // turn in its wind-up brings the body round, which a turn on the spot first would only slow.)
            WantFacing = Mathf.Abs(Mathf.DeltaAngle(loco.Facing, toward)) > PressTurn ? toward : loco.Facing;
            // (Free to strike once out of the last one and standing in its guard: a strike from feet still where the
            // last one left them would have them jump to its own.)
            loco.Hurry = true;
            bool free = loco.StanceSettled;
            StrikeLink best = null;
            float bestScore = float.MaxValue, nearestMiss = float.MaxValue;
            // (Out of the last strike and in the guard, any strike not yet used may come next: its cost only orders them.)
            foreach (var link in Options(fromGuard: true))
            {
                float entry = PressEntry(link.To);
                var at = Target(link.To, entry);
                float miss = poser.MissAfterPull(link.To, at, entry, loco.Position, loco.Facing);
                nearestMiss = Mathf.Min(nearestMiss, miss);
                float turnNeeded = Mathf.Abs(poser.AimTurnFor(link.To, at, entry, loco.Position, loco.Facing));
                // (Facing the target first, for a while: a strike swung from a body turned away has to turn it in its wind-up.)
                // (Once it has stood within reach a moment (it stops stepping in there), as near as it got will do.)
                float within = pressedFor > 0.5f ? LandWithin + StopWithin : LandWithin;
                if (miss <= within && (turnNeeded <= PressTurn || pressedFor > PressMost * 0.6f))
                {
                    float turn = turnNeeded;
                    float score = link.Cost + TurnWeight * turn / Mathf.Max(1f, poser.MostAimTurn);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = link;
                    }
                }
            }

            if (best != null && free)
            {
                Pressing = false;
                loco.Hurry = false;
                Want = Vector3.zero;
                Ended = "";
                float from = PressEntry(best.To);
                poser.PlayStrike(best.To, Target(best.To, from), startFrom: from);
                loco.Chamber(StrikeCombos.ChamberSpeed, best.To.ContactFrame / best.To.Rate);
                current = best.To;
                shotTime = from;
                used.Add(best.To);
                played++;
                Started?.Invoke(best.To, null);
                return;
            }

            if (pressedFor > PressMost)
            {
                Ended = $"pressed {PressMost:0.0} s after the target and no follow-on would land (the nearest would miss by {nearestMiss:0.00} m)";
                Stop();
                return;
            }

            // (Toward the target, slowing as the nearest follow-on comes within reach of it; within reach, it stops
            // and stands into its guard to strike.)
            float speed = nearestMiss <= LandWithin + StopWithin ? 0f : PressSpeed * Mathf.Sqrt(loco.Leg / 0.845f) * Mathf.Clamp01(0.3f + nearestMiss / 0.3f);
            // (Never in nearer than the fighting distance: there it stands, and strikes from there or not at all.)
            if (poser.KeepFrom.HasValue && poser.KeepDistance > 0f)
            {
                Vector3 off = loco.Position - poser.KeepFrom.Value;
                off.y = 0f;
                speed *= Mathf.Clamp01((off.magnitude - poser.KeepDistance - 0.02f) / 0.15f);
            }

            Want = to.sqrMagnitude > 1e-4f ? to.normalized * speed : Vector3.zero;
        }

        /// <summary>One step, after the walk's (not through a hit stop).</summary>
        public void Step() => Step(1f / 60f);

        /// <summary>One step of <paramref name="dt"/> seconds, after the walk's (not through a hit stop).</summary>
        public void Step(float dt)
        {
            if (current == null)
            {
                if (firstLunge >= 0f)
                {
                    FollowOns(false);
                }

                return;
            }

            Magnet(dt);
            // (The strike playing follows its target, as far as its pull goes.)
            if (TargetFor != null && !Pressing)
            {
                poser.Retarget(TargetFor(current));
            }

            bool still = loco.Current == Locomotor.Mode.OneShot && loco.Shot == current;
            // (A blow stopped by what it met, backing out of it: the next blow is taken up from there at once, not
            // after it has backed out and stood in its guard. Bam, and bam again.)
            if (still && loco.ShotSpeed < 0f && (Strikes != null || Cores) && !Pressing)
            {
                shotTime = loco.ShotTime;
                still = false;
                rebounding = true;
            }

            if (still)
            {
                // (Where the strike is, for the moment it is over: a cut-short one stops short of its links.)
                shotTime = loco.ShotTime;
            }

            if (Cores)
            {
                StepCores(still);
                return;
            }

            if (Strikes != null)
            {
                if (Pressing)
                {
                    StepPressing(dt);
                    return;
                }

                StepChoosing(still);
                return;
            }

            if (next >= chain.Count)
            {
                if (!still)
                {
                    current = null;
                }

                return;
            }

            var link = chain[next];
            if (still && loco.ShotTime < link.At)
            {
                return;
            }

            Go(link);
            next++;
        }
    }
}
