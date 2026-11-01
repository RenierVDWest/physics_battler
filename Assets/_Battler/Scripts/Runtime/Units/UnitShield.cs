using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>What the shield is doing (milestone 9).</summary>
    public enum ShieldState
    {
        None,

        /// <summary>Held toward the enemy it is minding (its chest).</summary>
        Guard,

        /// <summary>Following a seen wind-up: the enemy's weapon.</summary>
        Track,

        /// <summary>Set on a seen blow's path, its face against the weapon's travel.</summary>
        Block,

        /// <summary>Raised to the eyes (shelled up).</summary>
        High,
    }

    /// <summary>
    /// The shield under control (milestone 9). The shield always points at the threat the unit
    /// knows of: the chest of the enemy it is minding, that enemy's weapon while it winds up, a seen
    /// blow's path at contact. It moves there at a person's pace (a block quickly, back to the guard
    /// more slowly), and only inside the cone a shield arm can cover from the chest (the pose clamps
    /// it there and keeps it out of the body). A seen blow outside that cone turns the unit to face
    /// it; it can block once the blow is inside, if there is time. What it hasn't seen moves nothing.
    /// </summary>
    public partial class UnitController
    {
        public ShieldState ShieldMode { get; private set; }

        /// <summary>The point the shield faces (world): the threat, or in a block where the weapon comes in from.</summary>
        public Vector3 ShieldThreat { get; private set; }

        /// <summary>The point the shield is moved toward (world): in a block, the blow's path at contact.</summary>
        public Vector3 ShieldAimPoint { get; private set; }

        /// <summary>How far the shield's face is turned from the way to its threat from the chest (degrees), as posed.</summary>
        public float ShieldOffThreat
        {
            get
            {
                // (Seen from the chest: a big enemy up close can stand nearly where the shield is.)
                Vector3 to = ShieldThreat - Anim.Part(PartId.Chest).position;
                return to.sqrMagnitude > 1e-6f ? Vector3.Angle(Pose.ShieldFace, to) : 0f;
            }
        }

        /// <summary>The fastest the shield turns toward a blow, toward a wind-up, and back to the guard (degrees per second), and how stiffly (rad/s; critically damped).</summary>
        public const float ShieldFast = 300f, ShieldTrack = 250f, ShieldSlow = 150f, ShieldStiffFast = 16f, ShieldStiffSlow = 9f;

        /// <summary>
        /// Milestone 9c: the shield's aim holds still until the threat has moved this far from it
        /// (degrees), and then for at least <see cref="ShieldAimHold"/> (seconds) after each move; its
        /// state (guard, track, raised) holds at least <see cref="ShieldModeHold"/>. A block goes at once.
        /// </summary>
        public const float ShieldDeadband = 12f, ShieldAimHold = 0.3f, ShieldModeHold = 0.25f;

        private float heldYaw, heldPitch, heldSince = float.NegativeInfinity, modeSince = float.NegativeInfinity;
        private Vector3 heldThreat;

        // The shield's way from the chest (world yaw and pitch), eased.
        private float shieldYaw, shieldPitch, shieldYawSpeed, shieldPitchSpeed;
        private bool shieldPlaced;
        private Vector3 shieldFace = Vector3.forward;

        /// <summary>In a block, where the weapon comes in from (further out along its path): what the shield faces.</summary>
        private Vector3 blockFrom3;

        /// <summary>A seen blow outside the shield's reach: where to turn to face it (none otherwise).</summary>
        private Vector3? turnToThreat;

        /// <summary>The enemy it is minding: its opponent if it knows it is there, else the nearest enemy it knows of.</summary>
        public UnitController Foe()
        {
            if (Opponent != null && Opponent.IsAlive && Knows(Opponent))
            {
                return Opponent;
            }

            UnitController best = null;
            float bestDistance = float.MaxValue;
            foreach (var pair in Known.All)
            {
                var other = pair.Key;
                if (other == null || !other.IsAlive || other.Team == Team)
                {
                    continue;
                }

                float d = (pair.Value.position - transform.position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = other;
                }
            }

            return best;
        }

        /// <summary>Each step, before the pose: where the shield goes, and whether it is set against a blow.</summary>
        private void UpdateShield(float dt)
        {
            turnToThreat = null;
            if (Loadout == null || !Loadout.HasShield || !IsAlive)
            {
                ShieldMode = ShieldState.None;
                Pose.Shield = default;
                LowerBlock(dt, false);
                return;
            }

            var chestPart = Anim.Part(PartId.Chest);
            Vector3 chest = chestPart.position;
            float chestYaw = ShieldCone.FacingOf(chestPart.rotation, facingYaw);
            float size = Body.SizeFactor;
            var mode = ShieldState.Guard;
            Vector3 threat = chest + Quaternion.Euler(0f, chestYaw, 0f) * Vector3.forward;
            bool blocking = false;

            if (incoming != null && incomingAttacker != null && incomingAttacker.CurrentMove == incoming && !incoming.ContactPassed)
            {
                // A blow it has seen: onto its path at contact, once its reaction time is up and the
                // path is where the shield can go; until then it follows the weapon.
                float contact = incoming.Move.ProgressAt(incoming.ContactTime);
                Vector3 at = ComesFrom(incoming, contact, chest, 0.4f * size);
                bool inReach = ShieldCone.Contains(at - chest, chestYaw, true, 5f);
                bool up = LocalTime >= blockFrom && LocalTime <= blockUntil;
                if (inReach && up)
                {
                    mode = ShieldState.Block;
                    threat = at;
                    blockFrom3 = ComesFrom(incoming, contact, chest, 0.9f * size);
                    blocking = true;
                    Vector3 travel = incoming.PathPoint(Mathf.Min(1f, contact + 0.02f)) - incoming.PathPoint(Mathf.Max(0f, contact - 0.02f));
                    Vector3 toAttacker = incomingAttacker.Anim.Part(PartId.Chest).position - chest;
                    Vector3 face = (travel.sqrMagnitude > 1e-8f ? -0.6f * travel.normalized : Vector3.zero)
                        + (toAttacker.sqrMagnitude > 1e-6f ? 0.4f * toAttacker.normalized : Vector3.zero);
                    if (face.sqrMagnitude > 1e-6f)
                    {
                        shieldFace = face.normalized;
                    }
                }
                else
                {
                    // (Toward where the blow will come in from, once: it doesn't chase the weapon round.)
                    mode = ShieldState.Track;
                    threat = ComesFrom(incoming, contact, chest, 0.9f * size);
                    if (!inReach)
                    {
                        turnToThreat = incomingAttacker.transform.position;
                    }
                }
            }
            else
            {
                var foe = Foe();
                if (foe != null)
                {
                    var blow = foe.CurrentMove;
                    if (blow != null && !blow.ContactPassed && ReferenceEquals(blow.Target, this) && blow.HasLastStrike && Sees(foe) >= Sight.Clear
                        && (foe.transform.position - transform.position).magnitude <= TrackRange * Mathf.Max(size, foe.Body.SizeFactor))
                    {
                        mode = ShieldState.Track;
                        threat = ComesFrom(blow, blow.Move.ProgressAt(blow.ContactTime), chest, 0.9f * size);
                    }
                    else
                    {
                        threat = ChestOf(foe);
                    }
                }

                if (mode == ShieldState.Guard && Tempo != null && TempoOn && Tempo.State == TempoState.Shell)
                {
                    mode = ShieldState.High;
                    threat += Vector3.up * (0.45f * size);   // (raised to the eyes)
                }
            }

            // Milestone 9c: its state holds a moment (a block goes at once).
            if (mode != ShieldMode && mode != ShieldState.Block && ShieldMode != ShieldState.None && shieldPlaced
                && LocalTime - modeSince < ShieldModeHold)
            {
                mode = ShieldMode;
                threat = heldThreat;
            }
            else if (mode != ShieldMode)
            {
                modeSince = LocalTime;
            }

            heldThreat = threat;

            // Toward it at a person's pace, from inside the cone (a threat behind it doesn't drag the
            // shield round the back).
            Vector3 to = threat - chest;
            ShieldCone.Angles(to, chestYaw, out float yaw, out float pitch);
            ShieldCone.Clamp(ref yaw, ref pitch, true);
            float targetYaw = chestYaw + yaw;

            // Milestone 9c: the aim holds still until the threat has moved clearly (or a block comes),
            // and then not again for a moment: the shield doesn't follow every sway and step.
            bool moved = Mathf.Abs(Mathf.DeltaAngle(heldYaw, targetYaw)) > ShieldDeadband || Mathf.Abs(pitch - heldPitch) > ShieldDeadband;
            bool modeChanged = mode != ShieldMode;
            if (!shieldPlaced || mode == ShieldState.Block || modeChanged || (moved && LocalTime - heldSince >= ShieldAimHold))
            {
                heldYaw = targetYaw;
                heldPitch = pitch;
                heldSince = LocalTime;
            }

            if (!shieldPlaced)
            {
                shieldYaw = heldYaw;
                shieldPitch = heldPitch;
                shieldYawSpeed = shieldPitchSpeed = 0f;
                shieldPlaced = true;
            }
            else if (dt > 0f)
            {
                bool quick = mode == ShieldState.Block || mode == ShieldState.Track;
                float stiff = quick ? ShieldStiffFast : ShieldStiffSlow;
                float most = mode == ShieldState.Block ? ShieldFast : mode == ShieldState.Track ? ShieldTrack : ShieldSlow;
                shieldYaw = Ease(shieldYaw, ref shieldYawSpeed, shieldYaw + Mathf.DeltaAngle(shieldYaw, heldYaw), stiff, most, dt);
                shieldPitch = Ease(shieldPitch, ref shieldPitchSpeed, heldPitch, stiff, most, dt);
            }

            ShieldMode = mode;
            ShieldAimPoint = threat;
            ShieldThreat = mode == ShieldState.Block ? blockFrom3 : threat;
            LowerBlock(dt, blocking);
            float b = BlockBlend * BlockBlend * (3f - 2f * BlockBlend);
            Pose.Shield = new ShieldAim
            {
                weight = 1f,
                direction = ShieldCone.Direction(0f, shieldYaw, shieldPitch),
                face = shieldFace,
                faceBlend = b,
                reach = 1f + 0.1f * b,
            };
        }

        /// <summary>What the shield follows of a wind-up: between the attacker's shoulders and its weapon (it doesn't chase the weapon round its head).</summary>
        private static Vector3 WindUp(UnitController attacker)
        {
            Vector3 chest = attacker.Anim.Part(PartId.Chest).position;
            return attacker.CurrentMove != null && attacker.CurrentMove.HasLastStrike ? Vector3.Lerp(chest, attacker.Pose.StrikePoint, 0.6f) : chest;
        }

        /// <summary>
        /// Where a blow comes in from: the last point of its path before contact that is still
        /// <paramref name="clear"/> out from the chest (the contact point itself is on the body, so
        /// the way to it from the chest says little about where the weapon is).
        /// </summary>
        private static Vector3 ComesFrom(MoveState blow, float contact, Vector3 chest, float clear)
        {
            Vector3 at = blow.PathPoint(contact);
            for (int i = 1; i <= 12; i++)
            {
                Vector3 p = blow.PathPoint(Mathf.Max(0f, contact - 0.04f * i));
                at = p;
                if ((p - chest).magnitude >= clear)
                {
                    break;
                }
            }

            return at;
        }

        /// <summary>The shield goes up (toward a block) or down; a blow that is past lets go of it.</summary>
        private void LowerBlock(float dt, bool blocking)
        {
            BlockBlend = Mathf.MoveTowards(BlockBlend, blocking ? 1f : 0f, dt / 0.15f);
            if (incoming != null && LocalTime > blockUntil && BlockBlend <= 0f)
            {
                incoming = null;
                incomingAttacker = null;
            }

            Pose.BlockBlend = BlockBlend;
        }

        /// <summary>
        /// A critically damped approach (solved exactly: it arrives without overshooting, at any
        /// step), at most <paramref name="most"/> per second.
        /// </summary>
        public static float Ease(float x, ref float v, float target, float stiffness, float most, float dt)
        {
            float e = x - target;
            float decay = Mathf.Exp(-stiffness * dt);
            float c = v + stiffness * e;
            float next = target + (e + c * dt) * decay;
            v = (v - c * stiffness * dt) * decay;
            float step = Mathf.Clamp(next - x, -most * dt, most * dt);
            v = Mathf.Clamp(v, -most, most);
            return x + step;
        }

        /// <summary>A seen wind-up is tracked only by a unit this close to the attacker (metres at medium size): further off the shield stays in its guard.</summary>
        public const float TrackRange = 2.6f;
    }
}
