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
            var weapon = attacker.Loadout?.Main;
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
            return attacker.TryOut(state) <= (state.Generated ? 0.66f : 1f) * Tolerance
                   && attacker.Pose.HandCleared.magnitude <= ContactPush * attacker.Body.SizeFactor
                   && attacker.Clearance(state, ClearanceSamples, out _, out _, out _) >= -OwnBodyClearance * attacker.Body.SizeFactor;
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
            var weapon = attacker.Loadout?.Main;
            if (weapon == null || weapon.moves == null)
            {
                return null;
            }

            var options = new List<(MoveDefinition move, AimPart part, float weight)>();
            foreach (var move in weapon.moves)
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

            while (options.Count > 0)
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
                var state = Prepare(attacker, target, option.move, option.part);
                if (state != null && Acceptable(attacker, state))
                {
                    return state;
                }
            }

            return null;
        }
    }
}
