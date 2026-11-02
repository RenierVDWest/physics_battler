using System.Collections.Generic;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Gameplay's half of a blow: which move, at which body part. A blow starts by locking the aim
    /// point where the part is now and working out the aim frame and the lunge (<see cref="Prepare"/>).
    /// A move is only chosen if it can land: the planner poses the attacker at the moment of contact
    /// (<see cref="UnitController.TryOut"/>, everything put back after) and keeps the move only if
    /// the striking part reaches the aim point within <see cref="Tolerance"/>, and only if the weapon and
    /// weapon arm keep clear of the attacker's own body throughout (<see cref="BodyClearance"/>).
    /// </summary>
    public static class MovePlanner
    {
        /// <summary>How close the striking part must come to the aim point at contact (metres).</summary>
        public const float Tolerance = 0.03f;

        /// <summary>How far the aim frame tilts down toward a low target, or up toward a high one (degrees).</summary>
        public const float MaxTiltDown = 60f, MaxTiltUp = 40f;

        /// <summary>How clear of its own body (head, chest, spine, pelvis) a blow's weapon and weapon arm must stay (metres at medium size).</summary>
        public const float OwnBodyClearance = 0.01f;

        /// <summary>How many poses through a blow the planner checks for clearance of the attacker's own body.</summary>
        public const int ClearanceSamples = 32;

        /// <summary>How far the torso's clearance may move the hand at a blow's striking moment, in its dry run (metres, a medium body's).</summary>
        public const float ContactPush = 0.005f;

        /// <summary>How much more often the body is aimed at than the elbows and knees.</summary>
        private const float BodyWeight = 3f;

        /// <summary>
        /// Locks a blow's aim on a body part of the target, or returns null if the lunge needed is
        /// longer than the move allows.
        /// </summary>
        public static MoveState Prepare(UnitController attacker, UnitController target, MoveDefinition move, AimPart part)
        {
            using (PhysicsBattler.Diagnostics.Perf.Begin(PhysicsBattler.Diagnostics.PerfSection.Prepare))
            {
                return PrepareCore(attacker, target, move, part);
            }
        }

        private static MoveState PrepareCore(UnitController attacker, UnitController target, MoveDefinition move, AimPart part)
        {
            // (Milestone 10: an off-hand blow is thrown with what the off hand has: its fist, its shield.)
            var weapon = attacker.Loadout?.For(move);
            if (move == null || weapon == null || target == null || !target.IsInitialized)
            {
                return null;
            }

            Vector3 aim = AimPartInfo.PhysicalPoint(target.Rig, part);
            return Prepare(attacker, move, weapon, aim, part, target);
        }

        /// <summary>
        /// A point in the way of a planned blow (the bystander test and its scene): on the blade as
        /// the arm actually poses it, two thirds of the way from the grip to the striking point, at the moment
        /// the striking point passes <paramref name="before"/> metres short of the aim on its way in.
        /// A body centred there is struck before the target.
        /// </summary>
        public static Vector3 InTheWay(UnitController attacker, MoveState state, float before)
        {
            float contact = state.ContactTime > 0f ? state.ContactTime : 0.5f;
            attacker.WeaponAt(state, contact, out var grip, out var strike);
            for (float t = contact; t > 0f; t -= 0.01f)
            {
                attacker.WeaponAt(state, t, out grip, out strike);
                if (Vector3.Distance(strike, state.Aim) >= before)
                {
                    break;
                }
            }

            // (The arm rebuild: two thirds of the way out to the striking part. A hand-keyed blade
            // turns about the hand as it comes in, its inner half trailing behind the striking part's
            // way; nearer the striking part is where it sweeps.)
            return Vector3.Lerp(grip, strike, 0.67f);
        }

        /// <summary>Locks a blow on a point (tests and the move preview aim at plain points too).</summary>
        public static MoveState Prepare(UnitController attacker, MoveDefinition move, WeaponDefinition weapon, Vector3 aim, AimPart part, object target)
        {
            if (move != null && move.Close)
            {
                return OffHandPath.Prepare(attacker, move, weapon, aim, part, target);
            }

            if (move != null && move.shape != MoveShape.Path)
            {
                return SwingPath.Prepare(attacker, move, weapon, aim, part, target);
            }

            Vector3 root = attacker.Root.position;
            Vector3 flat = aim - root;
            flat.y = 0f;
            float distance = flat.magnitude;
            Vector3 forward = distance > 1e-3f ? flat / distance : attacker.Root.forward;
            float size = attacker.Body.SizeFactor;

            // Stand where the chest is the move's reach from the aim point: closer for a target far
            // above or below.
            float chestHeight = attacker.Anim.Part(Rig.PartId.Chest).position.y;
            float rise = aim.y - chestHeight;
            float reach = move.reach * size;
            float across = Mathf.Sqrt(Mathf.Max(reach * reach - rise * rise, (0.3f * size) * (0.3f * size)));
            float lunge = distance - across;
            if (lunge > move.maxLunge * size || lunge < -0.4f * size)
            {
                return null;
            }

            // The aim frame looks from the chest (where it will be at contact) to the aim point, so a
            // blow at something low winds up above and behind the shoulder and comes down onto it,
            // and the path keeps its distance from the attacker's own body at any height.
            Vector3 chestAtContact = new Vector3(root.x, chestHeight, root.z) + forward * lunge;
            Vector3 line = aim - chestAtContact;
            float pitch = Mathf.Atan2(line.y, Mathf.Max(1e-3f, across)) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, -MaxTiltDown, MaxTiltUp);
            Quaternion frame = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(-pitch, 0f, 0f);

            float t = move.ContactTime();
            return new MoveState
            {
                Move = move,
                Weapon = weapon,
                Target = target,
                Part = part,
                Aim = aim,
                Frame = frame,
                Lunge = forward * lunge,
                Scale = size,
                WidthScale = attacker.Body.WidthFactor,
                Duration = move.Duration * Mathf.Sqrt(size),
                ContactTime = t,
                BodyPeak = move.BodyAt(t),
                DriveStart = DriveStart(move, t),
            };
        }

        /// <summary>
        /// When the body stops rocking back and drives forward (normalized time): the first moment after
        /// the body curve's lowest point before contact where it is back above zero.
        /// </summary>
        public static float DriveStart(MoveDefinition move, float contact)
        {
            float lowest = float.MaxValue, lowestAt = 0f;
            const int samples = 60;
            for (int i = 0; i <= samples; i++)
            {
                float t = contact * i / samples;
                float b = move.BodyAt(t);
                if (b < lowest)
                {
                    lowest = b;
                    lowestAt = t;
                }
            }

            for (int i = 0; i <= samples; i++)
            {
                float t = Mathf.Lerp(lowestAt, contact, i / (float)samples);
                if (move.BodyAt(t) >= 0f)
                {
                    return Mathf.Min(t, 0.75f * contact);
                }
            }

            return 0.5f * contact;
        }

        /// <summary>
        /// A blow is chosen only if it lands (the striking part within <see cref="Tolerance"/> of the
        /// aim point at contact) and the weapon and weapon arm stay clear of the attacker's own head
        /// and torso the whole way.
        /// </summary>
        public static bool Acceptable(UnitController attacker, MoveState state)
        {
            // (A swing's dry run leaves a margin: played step by step, its steps land a little differently.)
            // (The arm rebuild: and one that lands without the arm being pushed off its own body at the
            // striking moment, which, played, moves it off the aim.)
            if (attacker.TryOut(state) > (state.Generated ? 0.66f : 1f) * Tolerance
                || attacker.Pose.HandCleared.magnitude > ContactPush * attacker.Body.SizeFactor)
            {
                return false;
            }

            // (The performance pass: the poses stop at the first that fails.)
            float limit = -OwnBodyClearance * attacker.Body.SizeFactor;
            return attacker.Clearance(state, ClearanceSamples, limit, out _, out _, out _) >= limit;
        }

        /// <summary>
        /// Picks a move and a body part of the target that the attacker can strike from where it
        /// stands, at random (the body more often than the limbs). Null if nothing reaches.
        /// </summary>
        public static MoveState Plan(UnitController attacker, UnitController target, System.Random random)
            => Plan(attacker, target, random, AimParts.None, 1f);

        /// <param name="prefer">Body parts to favour (milestone 6: parts the target has open right now, as the attacker reads them).</param>
        /// <param name="preferWeight">How many times likelier a favoured part is.</param>
        public static MoveState Plan(UnitController attacker, UnitController target, System.Random random, AimParts prefer, float preferWeight)
        {
            using (PhysicsBattler.Diagnostics.Perf.Begin(PhysicsBattler.Diagnostics.PerfSection.Plan))
            {
                return PlanCore(attacker, target, random, prefer, preferWeight);
            }
        }

        private static MoveState PlanCore(UnitController attacker, UnitController target, System.Random random, AimParts prefer, float preferWeight)
        {
            var options = Options(attacker, target, prefer, preferWeight);
            while (options.Count > 0)
            {
                var option = Pick(options, random);
                var state = Prepare(attacker, target, option.move, option.part);
                if (state != null && Acceptable(attacker, state))
                {
                    return state;
                }
            }

            return null;
        }

        /// <summary>Every move and body part the attacker could throw at the target, weighted (the body more than the limbs, the parts preferred more).</summary>
        public static List<(MoveDefinition move, AimPart part, float weight)> Options(UnitController attacker, UnitController target, AimParts prefer, float preferWeight)
        {
            var options = new List<(MoveDefinition move, AimPart part, float weight)>();
            var weapon = attacker.Loadout?.Main;
            if (weapon == null || weapon.moves == null || target == null)
            {
                return options;
            }

            // (Milestone 10: and the off hand's blows, the jab with a bare lead hand.)
            var moves = new List<MoveDefinition>(weapon.moves);
            moves.AddRange(WeaponDefinition.ExtraMoves(weapon));
            if (!attacker.Rig.IsSevered(PartId.HandL))
            {
                foreach (var m in attacker.Loadout.OffHandMoves)
                {
                    if (m != null && m.hand == MoveHand.Off)
                    {
                        moves.Add(m);
                    }
                }
            }

            foreach (var move in moves)
            {
                if (move == null || !move.UsableBy(attacker.Body.SizeFactor))
                {
                    continue;
                }

                foreach (var part in AimPartInfo.All)
                {
                    // (Not at a limb that is gone.)
                    if (move.targets.Contains(part) && !target.Rig.IsSevered(AimPartInfo.BodyPartOf(part)))
                    {
                        bool body = part <= AimPart.Belly;
                        options.Add((move, part, (body ? BodyWeight : 1f) * (prefer.Contains(part) ? preferWeight : 1f)));
                    }
                }
            }

            return options;
        }

        /// <summary>Takes one option out at random by its weight.</summary>
        public static (MoveDefinition move, AimPart part, float weight) Pick(List<(MoveDefinition move, AimPart part, float weight)> options, System.Random random)
        {
            float total = 0f;
            foreach (var o in options)
            {
                total += o.weight;
            }

            float pick = (float)random.NextDouble() * total;
            int i = 0;
            for (; i < options.Count - 1; i++)
            {
                pick -= options[i].weight;
                if (pick <= 0f)
                {
                    break;
                }
            }

            var option = options[i];
            options.RemoveAt(i);
            return option;
        }

        // ---- Planning a few poses a step (the performance pass) -----------------------------------

        /// <summary>
        /// How long the planning may take each physics step, all units together (milliseconds): a
        /// blow's planning poses the body dry fifty-odd times, which in one step was a 40 to 120 ms
        /// stutter; spread over a few steps, while the attacker gets ready, it is never seen.
        /// </summary>
        public const float StepBudgetMs = 4f;

        private static long budgetStart;
        private static int budgetStep = -1;

        /// <summary>Starts this physics step's planning budget (once a step; the fight loop calls it).</summary>
        public static void BeginStep(int step)
        {
            if (budgetStep != step)
            {
                budgetStep = step;
                budgetStart = System.Diagnostics.Stopwatch.GetTimestamp();
            }
        }

        private static bool BudgetLeft() =>
            (System.Diagnostics.Stopwatch.GetTimestamp() - budgetStart) * 1000.0 / System.Diagnostics.Stopwatch.Frequency < StepBudgetMs;

        /// <summary>
        /// A blow being planned a few dry poses a step: the planner's choice (<see cref="Plan"/>) at
        /// each target in turn, with each candidate's checks (<see cref="Acceptable"/>: its contact,
        /// then its clearance pose by pose) and, for the one chosen, its posed path, so the blow starts
        /// with no posing left to do. The same choices as planning in one go.
        /// </summary>
        public sealed class Job
        {
            private readonly IEnumerator<bool> work;

            /// <summary>The blow chosen (once <see cref="Done"/>), or null: nothing lands.</summary>
            public MoveState Result { get; private set; }

            /// <summary>The target the blow was chosen at.</summary>
            public UnitController Target { get; private set; }

            public bool Done { get; private set; }

            /// <summary>The steps it has taken so far.</summary>
            public int Steps { get; private set; }

            public Job(UnitController attacker, IList<(UnitController target, AimParts prefer)> targets, System.Random random, float preferWeight)
            {
                work = Run(attacker, targets, random, preferWeight);
            }

            /// <summary>Does this step's share of the work (at least one pose; then as long as the step's budget lasts). True once done.</summary>
            public bool Step()
            {
                if (Done)
                {
                    return true;
                }

                Steps++;
                using (PhysicsBattler.Diagnostics.Perf.Begin(PhysicsBattler.Diagnostics.PerfSection.Plan))
                {
                    do
                    {
                        if (!work.MoveNext())
                        {
                            Done = true;
                            return true;
                        }
                    }
                    while (BudgetLeft());
                }

                return false;
            }

            private IEnumerator<bool> Run(UnitController attacker, IList<(UnitController target, AimParts prefer)> targets, System.Random random, float preferWeight)
            {
                foreach (var (target, prefer) in targets)
                {
                    if (target == null || !target.IsInitialized || !target.IsAlive)
                    {
                        continue;
                    }

                    var options = Options(attacker, target, prefer, preferWeight);
                    while (options.Count > 0)
                    {
                        var option = Pick(options, random);
                        var state = Prepare(attacker, target, option.move, option.part);
                        yield return false;
                        if (state == null)
                        {
                            continue;
                        }

                        // Its contact.
                        bool lands;
                        using (PhysicsBattler.Diagnostics.Perf.Begin(PhysicsBattler.Diagnostics.PerfSection.TryOut))
                        {
                            lands = attacker.TryOut(state) <= (state.Generated ? 0.66f : 1f) * Tolerance
                                    && attacker.Pose.HandCleared.magnitude <= ContactPush * attacker.Body.SizeFactor;
                        }

                        yield return false;
                        if (!lands)
                        {
                            continue;
                        }

                        // Its clearance, pose by pose.
                        float limit = -OwnBodyClearance * attacker.Body.SizeFactor;
                        bool clear = true;
                        for (int i = 0; i < ClearanceSamples && clear; i++)
                        {
                            using (PhysicsBattler.Diagnostics.Perf.Begin(PhysicsBattler.Diagnostics.PerfSection.Clearance))
                            {
                                clear = attacker.ClearanceAt(state, i, ClearanceSamples, out _, out _) >= limit;
                            }

                            yield return false;
                        }

                        if (!clear)
                        {
                            continue;
                        }

                        // Chosen: its posed path, pose by pose.
                        if (state.Generated && state.HasContactHand)
                        {
                            var samples = SwingPath.PathSamples(state);
                            for (int k = 0; k < samples.Count; k++)
                            {
                                if (!samples[k].known)
                                {
                                    samples[k] = SwingPath.PoseSample(attacker, state, samples[k].at);
                                    yield return false;
                                }
                            }

                            state.PosedPath = new List<(float, Vector3)>();
                            foreach (var sample in samples)
                            {
                                state.PosedPath.Add((sample.at, sample.strike));
                            }
                        }

                        Result = state;
                        Target = target;
                        yield break;
                    }
                }
            }
        }
    }
}
