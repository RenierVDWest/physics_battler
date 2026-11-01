using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Where the head looks (milestone 9, <see cref="HeadLook"/>): at what holds its attention. In
    /// order of weight: its own blow's aim, a blow it has seen coming at it, the weapon of the enemy
    /// it is minding as it winds up, someone new at the edge of its view, a glance now and then at
    /// whoever is nearest (enemy or ally), and otherwise that enemy's face. With nothing known, it
    /// looks about. Since it sees from its head, where it looks decides what it knows.
    /// </summary>
    public partial class UnitController
    {
        public HeadLook Look { get; } = new HeadLook();

        /// <summary>Off, the head doesn't look about (as before milestone 9): tests that measure something else, the chest left square.</summary>
        public bool LookOn { get; set; } = true;

        private readonly List<LookPoint> lookPoints = new List<LookPoint>();

        /// <summary>How far its gaze is from what it attends to (degrees): the head's pointing, less the few degrees its eyes take.</summary>
        public float LookOff
        {
            get
            {
                if (Look.Current < 0)
                {
                    return 0f;
                }

                Vector3 to = Look.Point - EyePosition;
                if (to.sqrMagnitude < 1e-6f)
                {
                    return 0f;
                }

                float eyes = new Vector2(HeadLook.EyesAcross(Look.Yaw), HeadLook.EyesUpDown(Look.Pitch)).magnitude;
                return Mathf.Max(0f, Vector3.Angle(EyeRotation * Vector3.forward, to) - eyes);
            }
        }
        private float lookWeight, nextGlanceAt, glanceUntil;
        private UnitController glanceAt;
        private System.Random lookRandom;

        /// <summary>What each kind of thing is worth to the head's attention.</summary>
        public const float OwnBlowAttention = 5f, BlowAttention = 3f, WindUpAttention = 2f, GlanceAttention = 1.6f, NewAttention = 1.4f,
            FoeAttention = 1f, SearchAttention = 0.5f;

        private void InitializeLook(int seed)
        {
            lookRandom = new System.Random(seed + 71);
            Look.Reset();
            Look.Latency = 0.1f + 0.1f * (float)lookRandom.NextDouble();
            lookWeight = 0f;
            nextGlanceAt = 2f + 3f * (float)lookRandom.NextDouble();
            glanceUntil = 0f;
            glanceAt = null;
        }

        private void AddLook(int id, Vector3 point, float score, float age, string kind)
        {
            lookPoints.Add(new LookPoint { id = id, point = point, score = score, age = age, kind = kind });
        }

        private static Vector3 FaceOf(UnitController unit) => Vector3.Lerp(unit.Anim.Part(PartId.Chest).position, unit.Anim.Part(PartId.Head).position, 0.6f);

        /// <summary>Where it watches another unit: its face, or lower on a much taller one close by (a head looks up only so far).</summary>
        private Vector3 WatchPoint(UnitController unit)
        {
            Vector3 face = FaceOf(unit), chest = unit.Anim.Part(PartId.Chest).position;
            Vector3 eye = EyePosition;
            float Up(Vector3 p)
            {
                Vector3 to = p - eye;
                return Mathf.Atan2(to.y, Mathf.Max(1e-3f, new Vector2(to.x, to.z).magnitude)) * Mathf.Rad2Deg;
            }

            float most = HeadLook.Up - 15f;
            if (Up(face) <= most)
            {
                return face;
            }

            float atChest = Up(chest);
            return atChest >= most ? chest : Vector3.Lerp(chest, face, (most - atChest) / Mathf.Max(1e-3f, Up(face) - atChest));
        }

        /// <summary>Each step, before the pose: what the head attends to and where it turns.</summary>
        private void UpdateLook(float dt)
        {
            float now = LocalTime;
            bool standing = LookOn && IsAlive && (State == UnitState.Standing || State == UnitState.Stumbling);
            lookWeight = Mathf.MoveTowards(lookWeight, standing ? 1f : 0f, dt / 0.3f);
            lookPoints.Clear();

            if (CurrentMove != null)
            {
                AddLook(1, CurrentMove.Aim, OwnBlowAttention, 10f, "its blow");
            }

            foreach (var t in threats)
            {
                if (t.perceived && t.attacker != null)
                {
                    Vector3 at = t.attacker.CurrentMove == t.move ? WindUp(t.attacker) : WatchPoint(t.attacker);
                    AddLook(1000 + t.attacker.unitNumber, at, BlowAttention, now - t.perceivedAt, "a blow");
                }
            }

            var foe = Foe();
            if (foe != null)
            {
                Vector3 face = Sees(foe) != Sight.Unseen ? WatchPoint(foe) : ChestOf(foe) + Vector3.up * (0.15f * foe.Body.SizeFactor);
                AddLook(2000 + foe.unitNumber, face, FoeAttention, 10f, "its enemy");
                var blow = foe.CurrentMove;
                if (blow != null && !blow.ContactPassed && blow.HasLastStrike && Sees(foe) >= Sight.Edge)
                {
                    AddLook(3000 + foe.unitNumber, WindUp(foe), WindUpAttention, blow.Time, "a wind-up");
                }
            }

            UnitController nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (var pair in Known.All)
            {
                var other = pair.Key;
                if (other == null || other == foe || !other.IsAlive)
                {
                    continue;
                }

                // (Only what the head can turn to without the feet: a glance doesn't turn the body.)
                Vector3 to = pair.Value.position - transform.position;
                if (Mathf.Abs(Mathf.DeltaAngle(facingYaw, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg)) > HeadLook.Turn + HeadLook.ChestTurn - 10f)
                {
                    continue;
                }

                float age = now - pair.Value.firstSeenAt;
                if (age < 1.5f && pair.Value.sight >= Sight.Edge)
                {
                    AddLook(4000 + other.unitNumber, WatchPoint(other), NewAttention * (1f - age / 1.5f) + 0.2f, age, "someone new");
                }

                float d = to.sqrMagnitude;
                if (d < nearestDistance)
                {
                    nearestDistance = d;
                    nearest = other;
                }
            }

            // Now and then a glance at whoever else is nearest.
            if (now >= nextGlanceAt)
            {
                glanceAt = nearest;
                glanceUntil = now + 0.8f;
                nextGlanceAt = now + 2.5f + 2.5f * (float)lookRandom.NextDouble();
            }

            if (now < glanceUntil && glanceAt != null && glanceAt.IsAlive && Knows(glanceAt) && CurrentMove == null)
            {
                AddLook(5000 + glanceAt.unitNumber, ChestOf(glanceAt) + Vector3.up * (0.15f * glanceAt.Body.SizeFactor), GlanceAttention, 1f, "a glance");
            }

            if (lookPoints.Count == 0)
            {
                // Nothing known: it looks about.
                Vector3 ahead = Quaternion.Euler(0f, facingYaw + 55f * Mathf.Sin(0.9f * now), 0f) * Vector3.forward;
                AddLook(6000, EyePosition + ahead * 4f, SearchAttention, 10f, "looking about");
            }

            Look.Choose(lookPoints, now);
            Look.Follow(EyePosition, facingYaw, dt);
            Pose.LookYaw = Look.Yaw;
            Pose.LookPitch = Look.Pitch;
            Pose.LookWeight = lookWeight;
        }

        /// <summary>Off its feet the head goes with the body (the look comes back in once it stands).</summary>
        private void DropLook()
        {
            lookWeight = 0f;
            Pose.LookWeight = 0f;
        }
    }
}
