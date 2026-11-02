using System.Collections.Generic;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Lays out a Swing or Thrust move for one blow, from the move's numbers and where the attacker
    /// and the aim point are.
    ///
    /// A swing turns the weapon about a pivot in the body (the chest, a little toward the weapon side
    /// and up) in a plane through the aim point, tilted as the move says. The arm is at full stretch
    /// just after contact, and the attacker steps in until the aim point is that much closer than
    /// full reach. So the striking part arrives at the aim point still reaching out, and carries on
    /// <see cref="MoveDefinition.through"/> metres into the target before the arc takes it round and
    /// out. The pivot moves with the body (the rock-back, the lunge, the lean, the turn), so the
    /// wind-up is wound up behind a body that is itself turned away and rocked back.
    ///
    /// A thrust draws the point back toward the shoulder and the hip, then drives it along the line
    /// from the pivot through the aim point, past it, the body driving on behind it.
    ///
    /// The path is written into the blow's <see cref="MoveState"/> (world), evenly in the move's
    /// travel, with the pivot for each point. At contact it passes through the aim point exactly.
    /// </summary>
    public static class SwingPath
    {
        /// <summary>Points on the path (the curve through them is smooth to well under a millimetre).</summary>
        public const int Samples = 160;

        public static MoveState Prepare(UnitController attacker, MoveDefinition move, WeaponDefinition weapon, Vector3 aim, AimPart part, object target)
        {
            if (attacker == null || weapon == null || attacker.Anim == null)
            {
                return null;
            }

            var body = attacker.Body;
            float size = body.SizeFactor, width = body.WidthFactor, leg = body.LegLength;
            float scale = attacker.Loadout != null ? attacker.Loadout.Scale : size;
            Vector3 root = attacker.Root.position;
            Vector3 flat = aim - root;
            flat.y = 0f;
            float distance = flat.magnitude;
            Vector3 forward = distance > 1e-3f ? flat / distance : attacker.Root.forward;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            bool thrust = move.shape == MoveShape.Thrust;

            // Full reach from the pivot: to the shoulder, the arm nearly straight, the hand to the
            // grip, the grip to the striking part.
            var anim = attacker.Anim;
            Vector3 chestNow = anim.Part(PartId.Chest).position;
            var facingNow = Quaternion.Euler(0f, attacker.FacingYaw, 0f);
            Vector3 shoulderLocal = Quaternion.Inverse(facingNow) * (anim.Part(PartId.UpperArmR).position - chestNow);
            Vector3 pivotLocal = new Vector3(move.pivotSide * width, move.pivotUp * size, move.pivotForward * size);
            float arm = anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude;
            float handToGrip = anim.SlotWeaponR != null ? anim.SlotWeaponR.localPosition.magnitude : 0f;
            float strikeLocal = (weapon.StrikePoint(move.strikeWith) * scale).magnitude;
            // (The hand's grip sits off the line of the arm, and the arm is never quite straight.)
            // (The arm rebuild: no further than the arm can carry the striking part with the contact's
            // wrist, a weapon held across the arm reaching less far than one in line with it.)
            float armReach = 0.9f * arm + strikeLocal;
            var hand = anim.Part(PartId.HandR);
            if (anim.SlotWeaponR != null)
            {
                Vector3 inHand = Quaternion.Inverse(hand.rotation) * (anim.SlotWeaponR.position - hand.position)
                                 + anim.SlotWeaponR.localRotation * (weapon.StrikePoint(move.strikeWith) * scale);
                armReach = Mathf.Min(armReach, 0.96f * ArmRig.MaxReach(anim, true, ArmRig.Wrist(anim, true, move.wristContact), inHand));
            }

            float reach = (shoulderLocal - pivotLocal).magnitude + armReach;

            // How far from the pivot the aim point is at contact: full reach less what the blow
            // carries on into the target (a thrust's body drive carries some of it).
            float depth = thrust ? Mathf.Max(0.05f, move.through - move.drive) * size : move.through * size;
            float wanted = Mathf.Clamp(reach - depth, 0.5f * reach, reach - 0.05f * size);

            // Step in until the pivot is that far from the aim point: the pivot at contact is the
            // chest moved by the lunge and leant forward, raised and set to the side.
            float spineToChest = Vector3.Distance(anim.Part(PartId.Pelvis).position, chestNow);
            float leanShift = spineToChest * Mathf.Sin(move.leanThrough * Mathf.Deg2Rad);
            float pivotY = chestNow.y - move.crouchStrike * leg - spineToChest * (1f - Mathf.Cos(move.leanThrough * Mathf.Deg2Rad))
                           + move.pivotUp * size;
            float rise = aim.y - pivotY;
            float sideways = move.pivotSide * width;
            float across = Mathf.Sqrt(Mathf.Max(wanted * wanted - rise * rise - sideways * sideways, 0.09f * size * size));
            float lunge = distance - across - leanShift - move.pivotForward * size;
            if (lunge > move.maxLunge * size || lunge < -0.4f * size)
            {
                return null;
            }

            // The aim frame (the Path shape's, kept for the planner's other uses): from the chest at
            // contact to the aim point.
            Vector3 chestAtContact = new Vector3(root.x, chestNow.y, root.z) + forward * lunge;
            Vector3 line = aim - chestAtContact;
            float pitch = Mathf.Clamp(Mathf.Atan2(line.y, Mathf.Max(1e-3f, across)) * Mathf.Rad2Deg, -MovePlanner.MaxTiltDown, MovePlanner.MaxTiltUp);
            Quaternion frame = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(-pitch, 0f, 0f);

            float tc = move.ContactTime();
            var state = new MoveState
            {
                Move = move,
                Weapon = weapon,
                Target = target,
                Part = part,
                Aim = aim,
                Frame = frame,
                Lunge = forward * lunge,
                Scale = size,
                WidthScale = width,
                Duration = move.Duration * Mathf.Sqrt(size),
                ContactTime = tc,
                BodyPeak = move.BodyAt(tc),
                DriveStart = MovePlanner.DriveStart(move, tc),
                FullReach = reach,
            };

            // Where the pivot really is at contact: the attacker posed there (the body, before the arms).
            attacker.TryOut(state);
            Vector3 pivotAtContact = attacker.Pose.ChestBeforeArms + Side(move, state, tc, right, width, size) + Vector3.up * (move.pivotUp * size);
            Vector3 toAim = aim - pivotAtContact;
            float d = toAim.magnitude;
            if (d > 0.99f * reach || d < 0.2f * reach)
            {
                return null;
            }

            Vector3 e1 = toAim / d;
            Vector3 atContact = BodyOffset(move, state, tc, forward, right, leg, spineToChest, width);
            var path = new Vector3[Samples];
            var pivots = new Vector3[Samples];
            if (thrust)
            {
                state.ThrustLine = e1;
                float guard = 0.7f * reach, back = move.drawBack * size;
                float a0 = move.SwingTravel(0), a1 = move.SwingTravel(1), a2 = move.SwingTravel(2), a3 = move.SwingTravel(3);
                for (int i = 0; i < Samples; i++)
                {
                    float u = i / (float)(Samples - 1);
                    float t = TimeAt(move, u);
                    Vector3 pivot = pivotAtContact + BodyOffset(move, state, t, forward, right, leg, spineToChest, width) - atContact;
                    float a = u * move.SwingTotal;
                    float along, drawn;
                    if (a <= a0)
                    {
                        along = Mathf.Lerp(guard, back, a / a0);
                        drawn = a / a0;
                    }
                    else if (a <= a0 + a1)
                    {
                        along = back - 0.03f * size * (a - a0) / a1;
                        drawn = 1f;
                    }
                    else if (a <= a0 + a1 + a2)
                    {
                        float x = (a - a0 - a1) / a2;
                        along = Mathf.Lerp(back - 0.03f * size, d, x);
                        drawn = 1f - x;
                    }
                    else
                    {
                        along = d + depth * (a - a0 - a1 - a2) / a3;
                        drawn = 0f;
                    }

                    // Drawn back, the point comes in by the hip: down and out to the side.
                    Vector3 hip = (right * (0.1f * width) + Vector3.down * (0.22f * size)) * drawn;
                    pivots[i] = pivot;
                    path[i] = pivot + e1 * along + hip;
                }
            }
            else
            {
                // The plane: through the pivot and the aim point, the weapon moving at contact across
                // (to the attacker's left, or right for a backhand) and down as the tilt says.
                float tilt = move.planeTilt * Mathf.Deg2Rad;
                Vector3 across2 = (move.backhand ? right : -right) * Mathf.Cos(tilt) + Vector3.down * Mathf.Sin(tilt);
                Vector3 e2 = across2 - e1 * Vector3.Dot(across2, e1);
                if (e2.sqrMagnitude < 1e-6f)
                {
                    e2 = Vector3.Cross(e1, move.backhand ? Vector3.up : Vector3.down);
                }

                e2.Normalize();
                state.PlaneNormal = Vector3.Cross(e1, e2);
                float wound = -(move.windUpAngle + 4f);
                for (int i = 0; i < Samples; i++)
                {
                    float u = i / (float)(Samples - 1);
                    float t = TimeAt(move, u);
                    Vector3 pivot = pivotAtContact + BodyOffset(move, state, t, forward, right, leg, spineToChest, width) - atContact;
                    float theta = move.SwingAngle(u);
                    float r = Radius(move, theta, wound, d, reach);
                    float rad = theta * Mathf.Deg2Rad;
                    pivots[i] = pivot;
                    path[i] = pivot + r * (Mathf.Cos(rad) * e1 + Mathf.Sin(rad) * e2);
                }
            }

            state.Path = path;
            state.Pivots = pivots;
            // (The arm rebuild: where the hand must be at the striking moment for the blow to land.)
            attacker.PlanContactHand(state);
            return state;
        }

        /// <summary>
        /// How far out the striking part is at an angle on the arc: cocked in close in the wind-up
        /// (the elbow bent), reaching out through the contact (at the aim point's distance) to full
        /// stretch a little after it, easing in a little through the follow-through. One smooth,
        /// rising curve through the contact (no kink where the arm turns from bending to reaching).
        /// </summary>
        private static float Radius(MoveDefinition move, float theta, float wound, float contact, float reach)
        {
            float windUp = Mathf.Min(move.windUpRadius * reach, 0.92f * contact);
            float stretch = Mathf.Max(5f, move.stretchAfter);
            if (theta <= wound)
            {
                return windUp;
            }

            if (theta <= stretch)
            {
                // Monotone cubic Hermite through (wound, windUp), (0, contact), (stretch, reach),
                // flat at both ends.
                float s0 = (contact - windUp) / -wound, s1 = (reach - contact) / stretch;
                float m = s0 <= 0f || s1 <= 0f ? 0f : Mathf.Min(0.5f * (s0 + s1), 3f * Mathf.Min(s0, s1));
                return theta < 0f
                    ? Hermite(windUp, contact, 0f, m, -wound, (theta - wound) / -wound)
                    : Hermite(contact, reach, m, 0f, stretch, theta / stretch);
            }

            return Mathf.Lerp(reach, 0.9f * reach, Smooth((theta - stretch) / Mathf.Max(1f, move.followAngle - stretch)));
        }

        private static float Hermite(float p0, float p1, float m0, float m1, float span, float x)
        {
            x = Mathf.Clamp01(x);
            float x2 = x * x, x3 = x2 * x;
            return (2f * x3 - 3f * x2 + 1f) * p0 + (x3 - 2f * x2 + x) * span * m0 + (-2f * x3 + 3f * x2) * p1 + (x3 - x2) * span * m1;
        }

        /// <summary>The pivot's offset from the chest toward the weapon side, turned with the body at a moment.</summary>
        private static Vector3 Side(MoveDefinition move, MoveState state, float t, Vector3 right, float width, float size)
        {
            var at = state.CopyAt(t);
            float turn = (at.HipTurn + at.ChestTurn) * at.Blend;
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            return Quaternion.AngleAxis(turn, Vector3.up) * (right * (move.pivotSide * width) + forward * (move.pivotForward * size));
        }

        /// <summary>
        /// Where the body has moved the pivot at a moment (relative, world): the lunge or the
        /// rock-back, the drive, the crouch and the lean, as the pose applies them, and the turn.
        /// </summary>
        private static Vector3 BodyOffset(MoveDefinition move, MoveState state, float t, Vector3 forward, Vector3 right, float leg, float spineToChest, float width)
        {
            var at = state.CopyAt(t);
            float blend = at.Blend;
            float b = at.Body;
            Vector3 hips = b >= 0f ? state.Lunge * b : forward * (b * Motion.PosePipeline.RockBack * leg);
            hips += forward * at.Drive;
            hips += Vector3.down * (at.Crouch * leg);
            float lean = at.Lean * blend * Mathf.Deg2Rad;
            Vector3 chest = forward * (spineToChest * Mathf.Sin(lean)) + Vector3.down * (spineToChest * (1f - Mathf.Cos(lean)));
            return hips * blend + chest + Side(move, state, t, right, width, state.Scale);
        }

        /// <summary>The move's time (normalized) at which it has got to a progress along its path.</summary>
        internal static float TimeAt(MoveDefinition move, float progress)
        {
            float lo = 0f, hi = move.SwingFollowEnd;   // (the recovery comes back along the path: not searched)
            for (int i = 0; i < 32; i++)
            {
                float mid = 0.5f * (lo + hi);
                if (move.ProgressAt(mid) < progress)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return hi;
        }

        /// <summary>
        /// The arm rebuild: a hand-first blow's path made the way its striking part will really go, the
        /// blow posed dry (as if it started now) at <see cref="PosedSamples"/> places along it and the
        /// path's samples laid between them. What reads the path (the shield reading the blow coming, the
        /// sweeps) then reads the blow as played, not the path it was planned on.
        /// </summary>
        public static void PathFromPose(UnitController attacker, MoveState state)
        {
            if (state?.Path == null || !state.HasContactHand || !state.Generated)
            {
                return;
            }

            // (The performance pass: posed already, a few a step, by the planner.)
            if (state.PosedPath == null)
            {
                var samples = PathSamples(state);
                for (int k = 0; k < samples.Count; k++)
                {
                    if (!samples[k].known)
                    {
                        samples[k] = PoseSample(attacker, state, samples[k].at);
                    }
                }

                state.PosedPath = new List<(float, Vector3)>();
                foreach (var sample in samples)
                {
                    state.PosedPath.Add((sample.at, sample.strike));
                }
            }

            BuildPath(attacker, state);
        }

        /// <summary>
        /// Where along a blow its posed path is sampled (progress, sorted), and those the planner's
        /// clearance poses have already (their striking part from where the body stood).
        /// </summary>
        public static List<(float at, Vector3 strike, bool known)> PathSamples(MoveState state)
        {
            float cp = state.Move.ContactProgress;
            var samples = new List<(float at, Vector3 strike, bool known)>();
            // (The performance pass: the planner's clearance poses, taken as they are: the whole blow's
            // samples, posed already.)
            bool reuse = state.PosedTimes != null && state.PosedCount == state.PosedTimes.Length && state.PosedCount >= 8;
            if (reuse)
            {
                samples.Add((0f, Vector3.zero, false));
                for (int k = 0; k < state.PosedCount; k++)
                {
                    samples.Add((state.Move.ProgressAt(state.PosedTimes[k]), state.PosedStrikes[k], true));
                }

                samples.Add((1f, Vector3.zero, false));
            }
            else
            {
                for (int k = 0; k < PosedSamples; k++)
                {
                    samples.Add((k / (float)(PosedSamples - 1), Vector3.zero, false));
                }
            }

            for (int k = 0; k < PosedNearContact; k++)
            {
                samples.Add((Mathf.Clamp01(cp + NearContact * (2f * k / (PosedNearContact - 1) - 1f)), Vector3.zero, false));
            }

            samples.Sort((a, b) => a.at.CompareTo(b.at));
            return samples;
        }

        /// <summary>One posed path sample: the blow posed dry at that progress, its striking part from where the body stands.</summary>
        public static (float at, Vector3 strike, bool known) PoseSample(UnitController attacker, MoveState state, float at)
        {
            attacker.WeaponAt(state, TimeAt(state.Move, at), out _, out var strike);
            return (at, strike - attacker.Root.position, true);
        }

        /// <summary>The blow's path from its posed samples, laid from where the body stands now.</summary>
        public static void BuildPath(UnitController attacker, MoveState state)
        {
            var samples = state.PosedPath;
            if (samples == null || samples.Count < 2 || state.Path == null)
            {
                return;
            }

            Vector3 root = attacker.Root.position;
            var path = new Vector3[state.Path.Length];
            int j = 0;
            for (int i = 0; i < path.Length; i++)
            {
                float u = i / (float)(path.Length - 1);
                while (j < samples.Count - 2 && samples[j + 1].at < u)
                {
                    j++;
                }

                float span = samples[j + 1].at - samples[j].at;
                path[i] = root + (span > 1e-6f ? Vector3.Lerp(samples[j].strike, samples[j + 1].strike, Mathf.Clamp01((u - samples[j].at) / span)) : samples[j].strike);
            }

            state.Path = path;
        }

        /// <summary>How many dry poses <see cref="PathFromPose"/> takes along a blow, and how many more within <see cref="NearContact"/> of the striking moment's progress either side.</summary>
        public const int PosedSamples = 25, PosedNearContact = 17;

        public const float NearContact = 0.04f;

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }
    }
}
