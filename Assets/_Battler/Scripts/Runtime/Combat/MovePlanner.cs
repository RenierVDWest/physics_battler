using System.Collections.Generic;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Gameplay's half of a blow: which move, at which body part. A blow starts by locking the aim
    /// point where the part is now and working out the aim frame and the lunge (<see cref="Prepare"/>).
    /// A move is only chosen if it can land: the planner poses the attacker at the moment of contact
    /// (<see cref="UnitController.TryOut"/>, everything put back after) and keeps the move only if
    /// the striking part reaches the aim point within <see cref="Tolerance"/>.
    /// </summary>
    public static class MovePlanner
    {
        /// <summary>How close the striking part must come to the aim point at contact (metres).</summary>
        public const float Tolerance = 0.03f;

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

            Vector3 aim = AimPartInfo.Physical(target.Rig, part).position;
            return Prepare(attacker, move, weapon, aim, part, target);
        }

        /// <summary>Locks a blow on a point (tests and the move preview aim at plain points too).</summary>
        public static MoveState Prepare(UnitController attacker, MoveDefinition move, WeaponDefinition weapon, Vector3 aim, AimPart part, object target)
        {
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

            float t = move.ContactTime();
            return new MoveState
            {
                Move = move,
                Weapon = weapon,
                Target = target,
                Part = part,
                Aim = aim,
                Frame = Quaternion.LookRotation(forward, Vector3.up),
                Lunge = forward * lunge,
                Scale = size,
                Duration = move.duration * Mathf.Sqrt(size),
                ContactTime = t,
                BodyPeak = move.BodyAt(t),
            };
        }

        /// <summary>
        /// Picks a move and a body part of the target that the attacker can strike from where it
        /// stands, at random (the body more often than the limbs). Null if nothing reaches.
        /// </summary>
        public static MoveState Plan(UnitController attacker, UnitController target, System.Random random)
        {
            var weapon = attacker.Loadout?.Main;
            if (weapon == null || weapon.moves == null)
            {
                return null;
            }

            var options = new List<(MoveDefinition move, AimPart part, float weight)>();
            foreach (var move in weapon.moves)
            {
                if (move == null)
                {
                    continue;
                }

                foreach (var part in AimPartInfo.All)
                {
                    if (move.targets.Contains(part))
                    {
                        bool body = part <= AimPart.Belly;
                        options.Add((move, part, body ? BodyWeight : 1f));
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
                if (state != null && attacker.TryOut(state) <= Tolerance)
                {
                    return state;
                }
            }

            return null;
        }
    }
}
