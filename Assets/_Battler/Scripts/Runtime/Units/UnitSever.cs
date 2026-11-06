using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Milestone 5: severing. A sword's edge that crosses a limb close to a joint (the neck, a
    /// shoulder, elbow, wrist, hip, knee or ankle), moving across the bone, fast enough and hard
    /// enough, cuts it through there: the limb leaves the unit as a loose physics object with what
    /// it held. The unit loses what the limb did (its weapon, its shield, its footing; its head is
    /// its life), takes the wound's damage and bleeds. Clubs and thrusts never sever (for now).
    /// </summary>
    public partial class UnitController
    {
        /// <summary>A limb came off (the unit, the part below the joint cut, the blow; null for a test's cut).</summary>
        public static event Action<UnitController, PartId, HitReport> Severed;

        /// <summary>A leg is gone: it can't stand (it stays down, bleeding).</summary>
        public bool LegLost { get; private set; }

        /// <summary>Health lost a second to its wounds.</summary>
        public float Bleeding { get; private set; }

        /// <summary>The cut-off limbs lying about (removed with the unit).</summary>
        public IReadOnlyList<GameObject> SeveredLimbs => severedLimbs;

        private readonly List<GameObject> severedLimbs = new List<GameObject>();
        private readonly List<(PartId at, float age)> severedAt = new List<(PartId, float)>();
        private UnitController lastWounder;

        /// <summary>How close to a joint (metres at medium size) the edge must cross the limb to cut through it there.</summary>
        public const float SeverReach = 0.05f;

        /// <summary>How square to the bone the edge must move: at most this much of its motion along the bone (cos of the angle).</summary>
        public const float SeverAlong = 0.75f;

        /// <summary>
        /// Would this cut sever a limb of this unit? The edge (a to b, the blade this step) struck
        /// <paramref name="struck"/>; the joints it could cut are that part's own (above it) and
        /// those of the parts hanging from it. The nearest one the edge crosses within reach, across
        /// the bone, fast and hard enough, or null (with why not, for the log).
        /// </summary>
        public PartId? SeverCut(BodyPart struck, WeaponDefinition weapon, StrikeWith with, float weaponScale, float speed,
            Vector3 direction, Vector3 a, Vector3 b, Vector3 contact, float damage, out string why)
        {
            why = null;
            if (struck == null || weapon == null || weapon.kind != WeaponKind.Sword || with != StrikeWith.Edge)
            {
                return null;   // (clubs and thrusts don't sever)
            }

            // Only the stretch of the edge where it met the body cuts (a blade lying across the chest
            // doesn't take the head off with its far end).
            {
                Vector3 ab = b - a;
                float length = ab.magnitude;
                if (length > 1e-4f)
                {
                    float at = Mathf.Clamp(Vector3.Dot(contact - a, ab / length), 0f, length);
                    float half = 0.12f * Body.SizeFactor;
                    Vector3 dir = ab / length;
                    Vector3 from = a + dir * Mathf.Max(0f, at - half), to = a + dir * Mathf.Min(length, at + half);
                    a = from;
                    b = to;
                }
            }

            PartId? best = null;
            float bestDistance = float.MaxValue;
            foreach (var id in PartInfo.All)
            {
                bool candidate = id == struck.id || (PartInfo.HasParent(id) && PartInfo.Parent(id) == struck.id);
                // (The neck only by a blade that met the head or neck itself: a cut into the top of
                // the chest is a chest wound.)
                if (!candidate || !PartInfo.IsSeverable(id) || Rig.IsSevered(id) || (id == PartId.Head && struck.id != PartId.Head))
                {
                    continue;
                }

                // Through the limb within a few centimetres of the joint, along the bone: the edge
                // passes the bone's stretch either side of the joint closer than the limb is thick.
                var below = Rig.Part(id);
                Vector3 joint = Pos(id);
                Vector3 bone = BoneAxis(id);
                float along = SeverReach * Body.SizeFactor;
                float distance = SegmentDistance(a, b, joint - bone * along, joint + bone * along);
                float reach = LimbRadius(below, Body.SizeFactor) + weapon.sweepRadius * weaponScale;
                // (And it met the body there: a cut into the chest just under the neck is a chest wound.)
                float met = DistanceToSegment(contact, joint - bone * along, joint + bone * along);
                if (met > reach + 0.02f * Body.SizeFactor)
                {
                    if (distance <= reach && why == null)
                    {
                        why = $"the edge met the body {met * 100f:0} cm from the bone at the {PartInfo.JointName(id)}";
                    }

                    continue;
                }

                if (distance > reach || distance >= bestDistance)
                {
                    if (distance <= reach + 0.1f && why == null)
                    {
                        why = $"the edge passed {distance * 100f:0} cm from the bone at the {PartInfo.JointName(id)} (the limb is {reach * 100f:0} thick with the edge)";
                    }

                    continue;
                }

                Vector3 moving = direction.sqrMagnitude > 1e-8f ? direction.normalized : Vector3.zero;
                if (bone.sqrMagnitude > 1e-8f && Mathf.Abs(Vector3.Dot(moving, bone)) > SeverAlong)
                {
                    why = $"the edge ran along the bone at the {PartInfo.JointName(id)}";
                    continue;
                }

                if (speed < weapon.severSpeed)
                {
                    why = $"too slow to cut through the {PartInfo.JointName(id)} ({speed:0.0} of {weapon.severSpeed:0.0} m/s)";
                    continue;
                }

                float needed = DamageRules.SeverDamage(id) * Body.SizeFactor;
                if (damage < needed)
                {
                    why = $"not hard enough to cut through the {PartInfo.JointName(id)} ({damage:0.0} of {needed:0.0} damage)";
                    continue;
                }

                best = id;
                bestDistance = distance;
            }

            if (best.HasValue)
            {
                why = null;
            }

            return best;
        }

        /// <summary>
        /// Cuts the limb off at the joint above <paramref name="at"/> (with everything hanging from
        /// it) and throws it along <paramref name="impulse"/>. The unit loses what the limb did,
        /// takes the wound and bleeds. Works on the dead too (a killing cut still takes the limb).
        /// </summary>
        public bool Sever(PartId at, Vector3 impulse, HitReport hit = null)
        {
            if (!PartInfo.IsSeverable(at) || Rig.IsSevered(at))
            {
                return false;
            }

            bool weaponArm = at == PartId.HandR || PartInfo.IsBelow(PartId.HandR, at);
            bool offArm = at == PartId.HandL || PartInfo.IsBelow(PartId.HandL, at);
            bool leg = AuthorityController.RegionOf(at) == BodyRegion.Legs && at != PartId.Pelvis;
            // Milestone 9c: what the hand held falls from it on its own, to land on the floor.
            Rigidbody droppedMain = null, droppedOff = null;
            if (weaponArm && MainWeapon != null)
            {
                droppedMain = MainWeapon.Release();
                KeepDropped(MainWeapon, droppedMain);
                MainWeapon = null;
                Loadout.Main = null;
            }

            if (offArm && OffWeapon != null)
            {
                droppedOff = OffWeapon.Release();
                KeepDropped(OffWeapon, droppedOff);
                if (droppedOff != null)
                {
                    // (Its weight leaves the hand with it.)
                    var shieldHand = Rig.Part(PartId.HandL).body;
                    if (shieldHand != null)
                    {
                        shieldHand.mass = Mathf.Max(0.05f, shieldHand.mass - shieldMassOnHand);
                    }

                    shieldMassOnHand = 0f;
                }

                OffWeapon = null;
                Loadout.Off = null;
                DropBlock();
            }

            var limb = Rig.Sever(at);
            if (limb == null)
            {
                return false;
            }

            severedLimbs.Add(limb);
            severedAt.Add((at, 0f));
            // Milestone 10: a cut-off limb is a body on the ground: a standing unit's foot nudges it,
            // never pins it to the floor (a get-up's foot came down on a severed shin and the downed
            // body beside it was thrown 7.8 m up).
            GroundContacts.Set(SeveredColliders(at), true);
            if (!physical)
            {
                foreach (var t in limb.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.hideFlags = HideFlags.DontSave;
                }
            }

            // The limb flies off with some of the blow and a turn.
            if (physical)
            {
                float mass = 0f;
                foreach (var id in PartInfo.All)
                {
                    if (Rig.CutAt(id) == at)
                    {
                        mass += Rig.Part(id).body.mass;
                    }
                }

                Vector3 v = Vector3.ClampMagnitude(impulse * 0.5f / Mathf.Max(0.5f, mass), 6f * SizeSpeed()) + Vector3.up * 1.5f * SizeSpeed();
                foreach (var id in PartInfo.All)
                {
                    if (Rig.CutAt(id) == at)
                    {
                        Rig.Part(id).body.AddForce(v, ForceMode.VelocityChange);
                    }
                }

                // (What the hand held flies with it.)
                foreach (var dropped in new[] { droppedMain, droppedOff })
                {
                    if (dropped != null)
                    {
                        dropped.AddForce(v, ForceMode.VelocityChange);
                    }
                }

                if (impulse.sqrMagnitude > 1e-8f)
                {
                    Rig.Part(at).body.AddTorque(Vector3.Cross(Vector3.up, impulse.normalized) * 8f, ForceMode.VelocityChange);
                }
            }

            lastWounder = hit != null ? hit.Attacker : lastWounder;
            Wounded(at, impulse, weaponArm, leg, offArm);
            Severed?.Invoke(this, at, hit);   // (after the wound: the log shows the unit as it is now)
            return true;
        }

        /// <summary>What losing the limb does to the unit (nothing, if it was dead already).</summary>
        private void Wounded(PartId at, Vector3 impulse, bool weaponArm, bool leg, bool offArm = false)
        {
            if (!IsAlive)
            {
                return;
            }

            if (Clip != null)
            {
                ClipWounded(at, impulse, weaponArm, offArm, leg);
                return;
            }

            if (at == PartId.Head)
            {
                Die(Wound("beheaded", at));
                return;
            }

            if (CurrentMove != null && (weaponArm || leg))
            {
                CancelMove(CurrentMove, "lost a limb");
            }

            // The wound, and the bleeding.
            Health = Mathf.Max(0f, Health - DamageRules.SeverLoss(at) * profile.maxHealth);
            Bleeding += DamageRules.BleedRate(at) * profile.maxHealth;
            if (Health <= 0f)
            {
                Die(Wound("bled out", at));
                return;
            }

            if (leg)
            {
                // No footing: down, and it stays down.
                LegLost = true;
                if (State == UnitState.Standing || State == UnitState.Stumbling || State == UnitState.GettingUp)
                {
                    KnockDown(impulse * 0.3f / Mathf.Max(1f, Body.Mass), false, "lost a leg");
                }
            }
        }

        /// <summary>
        /// A cut-off limb ignored its old body's parts (a unit's parts never collide); half a second
        /// after the cut (clear of the stump) it collides with them again, so it can't fall through.
        /// </summary>
        private void UpdateSeveredCollisions(float dt)
        {
            for (int i = severedAt.Count - 1; i >= 0; i--)
            {
                var (at, age) = severedAt[i];
                age += dt;
                if (age < 0.5f)
                {
                    severedAt[i] = (at, age);
                    continue;
                }

                severedAt.RemoveAt(i);
                foreach (var cut in PartInfo.All)
                {
                    if (Rig.CutAt(cut) != at)
                    {
                        continue;
                    }

                    foreach (var id in PartInfo.All)
                    {
                        var a = Rig.Part(cut).partCollider;
                        var b = Rig.Part(id).partCollider;
                        if (a != null && b != null && !Rig.IsSevered(id))
                        {
                            Physics.IgnoreCollision(a, b, false);
                        }
                    }
                }
            }
        }

        /// <summary>The wounds bleed (every step, standing or down).</summary>
        private void UpdateBleeding(float dt)
        {
            if (!IsAlive || Bleeding <= 0f || dt <= 0f)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - Bleeding * dt);
            if (Clip != null)
            {
                Clip.Health = Health;
            }

            if (Health <= 0f)
            {
                if (Clip != null)
                {
                    ClipDie(Wound("bled out"));
                }
                else
                {
                    Die(Wound("bled out"));
                }
            }
        }

        private HitReport Wound(string kind, PartId part = PartId.Chest)
        {
            return new HitReport { Attacker = lastWounder, Struck = this, Part = part, Kind = kind, Result = new HitResult { Reaction = Reaction.Killed } };
        }

        private readonly List<GameObject> droppedWeapons = new List<GameObject>();

        /// <summary>Weapons fallen from a cut-off hand (lying where they fell, cleared away with the unit).</summary>
        public IReadOnlyList<GameObject> DroppedWeapons => droppedWeapons;

        private void KeepDropped(Weapon weapon, Rigidbody body)
        {
            if (body == null)
            {
                return;
            }

            droppedWeapons.Add(weapon.gameObject);
            if (!physical)
            {
                weapon.gameObject.hideFlags = HideFlags.DontSave;
            }
        }

        /// <summary>Removes the limbs cut off this unit (with the unit), and the weapons they dropped.</summary>
        private void DestroySeveredLimbs()
        {
            // (Its cut-off limbs no longer count as bodies on the ground.)
            if (Rig != null)
            {
                foreach (var id in PartInfo.All)
                {
                    var cut = Rig.CutAt(id);
                    if (cut.HasValue && cut.Value == id)
                    {
                        GroundContacts.Set(SeveredColliders(id), null);
                    }
                }
            }

            foreach (var weapon in droppedWeapons)
            {
                if (weapon == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(weapon);
                }
                else
                {
                    DestroyImmediate(weapon);
                }
            }

            droppedWeapons.Clear();
            foreach (var limb in severedLimbs)
            {
                if (limb == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(limb);
                }
                else
                {
                    DestroyImmediate(limb);
                }
            }

            severedLimbs.Clear();
        }

        /// <summary>The colliders of the limb cut off at <paramref name="at"/>, each with its part's mass.</summary>
        private System.Collections.Generic.List<(Collider, float)> SeveredColliders(PartId at)
        {
            var list = new System.Collections.Generic.List<(Collider, float)>();
            foreach (var id in PartInfo.All)
            {
                var part = Rig.Part(id);
                if (Rig.CutAt(id) == at && part.partCollider != null && part.body != null)
                {
                    list.Add((part.partCollider, part.body.mass));
                }
            }

            return list;
        }

        /// <summary>
        /// The bone below a joint: toward the next joint down; for a hand, foot or head (no joint
        /// below), the bone above carried on (a wrist cut crosses the forearm's line, an ankle the shin's).
        /// </summary>
        private Vector3 BoneAxis(PartId below)
        {
            Vector3 from = Pos(below);
            foreach (var id in PartInfo.All)
            {
                if (PartInfo.HasParent(id) && PartInfo.Parent(id) == below)
                {
                    return (Pos(id) - from).normalized;
                }
            }

            return PartInfo.HasParent(below) ? (from - Pos(PartInfo.Parent(below))).normalized : Vector3.zero;
        }

        /// <summary>Where a part's joint is in the physics (the transform lags it inside the physics step).</summary>
        private Vector3 Pos(PartId id)
        {
            // (A clip fighter's blows meet its pose: the cut is judged where they met it.)
            if (Clip != null && Anim != null && Anim.Part(id) != null)
            {
                return Anim.Part(id).position;
            }

            var part = Rig.Part(id);
            return part.body != null ? part.body.position : part.transform.position;
        }

        /// <summary>How thick the limb is at its joint: the neck, a wrist and an ankle by their own size, the rest by the limb's capsule.</summary>
        private static float LimbRadius(BodyPart part, float size)
        {
            switch (part.id)
            {
                case PartId.Head: return 0.05f * size;
                case PartId.HandL:
                case PartId.HandR:
                case PartId.FootL:
                case PartId.FootR: return 0.035f * size;
            }

            var collider = part.partCollider;
            float scale = collider != null ? Mathf.Max(collider.transform.lossyScale.x, collider.transform.lossyScale.z) : 1f;
            switch (collider)
            {
                case CapsuleCollider c: return c.radius * scale;
                case SphereCollider s: return s.radius * scale;
                case BoxCollider b: return 0.5f * Mathf.Min(b.size.x, b.size.z) * scale;
                default: return 0.04f;
            }
        }

        /// <summary>The closest two segments come (p1-q1, p2-q2).</summary>
        private static float SegmentDistance(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
        {
            Vector3 d1 = q1 - p1, d2 = q2 - p2, r = p1 - p2;
            float a = Vector3.Dot(d1, d1), e = Vector3.Dot(d2, d2), f = Vector3.Dot(d2, r);
            float s, t;
            if (a <= 1e-10f && e <= 1e-10f)
            {
                return Vector3.Distance(p1, p2);
            }

            if (a <= 1e-10f)
            {
                s = 0f;
                t = Mathf.Clamp01(f / e);
            }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= 1e-10f)
                {
                    t = 0f;
                    s = Mathf.Clamp01(-c / a);
                }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;
                    s = denom > 1e-10f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                    t = (b * s + f) / e;
                    if (t < 0f)
                    {
                        t = 0f;
                        s = Mathf.Clamp01(-c / a);
                    }
                    else if (t > 1f)
                    {
                        t = 1f;
                        s = Mathf.Clamp01((b - c) / a);
                    }
                }
            }

            return Vector3.Distance(p1 + d1 * s, p2 + d2 * t);
        }

        private static float DistanceToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = ab.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector3.Distance(p, a + ab * t);
        }
    }
}
