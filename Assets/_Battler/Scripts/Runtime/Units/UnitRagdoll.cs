using System.Collections.Generic;
using PhysicsBattler.Muscles;
using PhysicsBattler.Authority;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// The unit as a body on the ground (milestone 9b): how far its joints are past their limits,
    /// how fast its parts move, and how close the nearest standing unit's legs are. The fixes that
    /// keep a lying body within its limits and heavy are in the rest of this partial class.
    /// </summary>
    public partial class UnitController
    {
        private static readonly PartId[] LegParts = { PartId.ThighL, PartId.ShinL, PartId.FootL, PartId.ThighR, PartId.ShinR, PartId.FootR };

        /// <summary>A body on the ground: down (alive) or dead.</summary>
        public bool Lying => initialized && (!IsAlive || State == UnitState.Down);

        /// <summary>
        /// How far a joint (the part and the one it hangs from) is past its limits, in degrees (0
        /// inside): on the physical body, or on the pose.
        /// </summary>
        public float JointPast(PartId id, bool physical)
        {
            if (!PartInfo.HasParent(id) || Rig.IsSevered(id))
            {
                return 0f;
            }

            var part = Rig.Part(id);
            var parentId = PartInfo.Parent(id);
            Quaternion child = physical ? part.transform.rotation : Anim.Part(id).rotation;
            Quaternion parent = physical ? Rig.Part(parentId).transform.rotation : Anim.Part(parentId).rotation;
            // (The arms against the arm rig's limits, the model their poses are made in.)
            if (AuthorityController.RegionOf(id) == BodyRegion.ArmL || AuthorityController.RegionOf(id) == BodyRegion.ArmR)
            {
                return PhysicsBattler.Motion.ArmRig.PastLimits(Anim, id, parent, child);
            }

            return JointMath.PastLimits(part.JointTurn(parent, child), part.limits, out _, out _, out _);
        }

        /// <summary>The joint furthest past its limits now, and by how much (degrees).</summary>
        public float WorstJoint(bool physical, out PartId which)
        {
            which = PartId.Pelvis;
            float worst = 0f;
            foreach (var id in PartInfo.All)
            {
                float past = JointPast(id, physical);
                if (past > worst)
                {
                    worst = past;
                    which = id;
                }
            }

            return worst;
        }

        /// <summary>The fastest any of its physical parts is moving (m/s).</summary>
        public float FastestPart()
        {
            float fastest = 0f;
            foreach (var part in Rig.Parts)
            {
                if (part.body != null && !Rig.IsSevered(part.id) && !part.body.isKinematic)
                {
                    fastest = Mathf.Max(fastest, part.body.linearVelocity.magnitude);
                }
            }

            return fastest;
        }

        /// <summary>How close the nearest standing unit's legs come to any of its parts (metres; large if none near).</summary>
        public float NearestStandingLeg()
        {
            float best = 99f;
            foreach (var other in all)
            {
                if (other == this || !other.initialized || other.Lying || other.State == UnitState.GettingUp)
                {
                    continue;
                }

                Vector3 apart = other.transform.position - transform.position;
                apart.y = 0f;
                if (apart.magnitude > 4f * Mathf.Max(1f, Body.SizeFactor) + 2f * other.Body.SizeFactor)
                {
                    continue;
                }

                foreach (var leg in LegParts)
                {
                    Vector3 l = other.Rig.Part(leg).transform.position;
                    foreach (var part in Rig.Parts)
                    {
                        if (!Rig.IsSevered(part.id))
                        {
                            best = Mathf.Min(best, Vector3.Distance(l, part.transform.position));
                        }
                    }
                }
            }

            return best;
        }

        // ---- On the ground (milestone 9b) -------------------------------------------------------

        /// <summary>Pairs of its own parts that don't share a joint and don't touch at rest: they collide while it lies on the ground.</summary>
        private readonly List<(Collider a, Collider b)> groundPairs = new List<(Collider, Collider)>();

        private bool onGround;
        private float groundCalm, groundHeavy, groundStruckUntil;

        /// <summary>How settled a body on the ground is (0: just fell or was struck; 1: heavy, damped, still).</summary>
        public float GroundSettled => groundHeavy;

        /// <summary>Damping and friction once a body has settled, its separation speed, and the solver work while it lies.</summary>
        public const float GroundLinearDamping = 1f, GroundAngularDamping = 4f, GroundFriction = 0.9f, GroundDepenetration = 1f;
        public const int GroundSolverIterations = 20, GroundSolverVelocityIterations = 8;

        /// <summary>The joints' muscle tone on the ground (damper per square-root kilogram): a downed unit more than a dead one.</summary>
        public const float DownTone = 0.6f, DeadTone = 0.35f;

        /// <summary>
        /// Milestone 10: a body on the ground never flies off by itself. For <see cref="LyingFree"/>
        /// seconds after it falls or is struck it moves as it was sent; after that no part of it
        /// goes faster than <see cref="LyingMostSpeed"/> (m/s at medium size) or rises faster than
        /// <see cref="LyingMostRise"/>, and a part already faster (still rolling, thrown) only slows
        /// (by <see cref="LyingSlowing"/> m/s², falling aside). The playtests had a downed large one
        /// thrown 7.8 m up and a medium one shaking for a second against a standing unit's legs: a
        /// limb trapped (under a foot, past a joint's limit) and the solver pushing the rest away.
        /// </summary>
        public const float LyingFree = 1f, LyingMostSpeed = 4f, LyingMostRise = 2f, LyingSlowing = 8f;

        private float lyingSince;
        private readonly float[] lyingCeiling = new float[PartInfo.Count];

        /// <summary>
        /// Called once at start (rest pose): finds the pairs of its own parts that may collide on the
        /// ground, weights its joints for the light limbs, and registers its parts as standing.
        /// </summary>
        private void InitializeGround()
        {
            groundPairs.Clear();
            var parts = Rig.Parts;
            for (int i = 0; i < parts.Length; i++)
            {
                for (int j = i + 1; j < parts.Length; j++)
                {
                    var a = parts[i];
                    var b = parts[j];
                    if (a.partCollider == null || b.partCollider == null || Neighbours(a.id, b.id))
                    {
                        continue;
                    }

                    if (Physics.ComputePenetration(a.partCollider, a.partCollider.transform.position, a.partCollider.transform.rotation,
                            b.partCollider, b.partCollider.transform.position, b.partCollider.transform.rotation, out _, out _))
                    {
                        continue;   // (touching at rest: they would push apart at once)
                    }

                    groundPairs.Add((a.partCollider, b.partCollider));
                }
            }

            // A light limb on a heavy body (the upper arm on the chest: 6.7 to 1) is weighted closer
            // to it in the joint solve, at most 3 to 1, so a push on the arm doesn't tear the joint.
            foreach (var part in parts)
            {
                if (part.joint != null && part.joint.connectedBody != null && part.body != null)
                {
                    float ratio = part.joint.connectedBody.mass / Mathf.Max(0.01f, part.body.mass);
                    part.joint.connectedMassScale = ratio > 3f ? ratio / 3f : 1f;
                }
            }

            onGround = false;
            GroundContacts.Set(PartColliders(), false);
        }

        private static bool Neighbours(PartId a, PartId b)
        {
            bool aHasParent = PartInfo.HasParent(a), bHasParent = PartInfo.HasParent(b);
            return (aHasParent && PartInfo.Parent(a) == b) || (bHasParent && PartInfo.Parent(b) == a)
                   || (aHasParent && bHasParent && PartInfo.Parent(a) == PartInfo.Parent(b));
        }

        private IEnumerable<(Collider, float)> PartColliders()
        {
            foreach (var part in Rig.Parts)
            {
                if (part.partCollider != null && part.body != null && !Rig.IsSevered(part.id))
                {
                    yield return (part.partCollider, part.body.mass);
                }
            }
        }

        /// <summary>Each step: on the ground or not, and how settled. Down, it is weighted down and its own limbs collide with it; up, as before.</summary>
        private void UpdateGround(float dt)
        {
            if (!physical)
            {
                return;
            }

            groundClock += dt;
            bool lying = Lying;
            if (lying != onGround)
            {
                onGround = lying;
                foreach (var (a, b) in groundPairs)
                {
                    var pa = a.GetComponentInParent<BodyPart>();
                    var pb = b.GetComponentInParent<BodyPart>();
                    if ((pa != null && Rig.IsSevered(pa.id)) || (pb != null && Rig.IsSevered(pb.id)))
                    {
                        continue;   // (a cut-off limb has its own rules)
                    }

                    Physics.IgnoreCollision(a, b, !lying);
                }

                foreach (var part in Rig.Parts)
                {
                    if (part.body == null || Rig.IsSevered(part.id))
                    {
                        continue;
                    }

                    part.body.solverIterations = lying ? GroundSolverIterations : profile.solverIterations;
                    part.body.solverVelocityIterations = lying ? GroundSolverVelocityIterations : profile.solverVelocityIterations;
                    // Milestone 10: on the ground the joints weigh both sides as they are. (A light limb
                    // weighted toward the heavy body keeps a push on the arm from tearing it standing,
                    // but a joint held past its limit then makes speed out of nothing: the body flies.)
                    if (part.joint != null && part.joint.connectedBody != null)
                    {
                        float ratio = part.joint.connectedBody.mass / Mathf.Max(0.01f, part.body.mass);
                        part.joint.connectedMassScale = !lying && ratio > 3f ? ratio / 3f : 1f;
                    }
                }

                lyingSince = groundClock;

                groundCalm = 0f;
                groundHeavy = 0f;
                ApplyGroundWeight();
                GroundContacts.Set(PartColliders(), lying);
                // Milestone 9c: what it holds is solid on the ground with it.
                MainWeapon?.SetLying(lying);
            }

            Authority.GroundTone = lying ? (IsAlive ? DownTone : DeadTone) : 0f;
            GroundContacts.Publish();
            if (!lying)
            {
                return;
            }

            HoldLyingSpeed(dt);

            // Settled: the hips slow under 0.5 m/s (by size) for 0.3 s; then it grows heavy over 0.5 s,
            // and stays so (a walking leg nudging it doesn't make it light again; a blow does, or being
            // thrown along faster than 1.5 m/s).
            var pelvis = Rig.Part(PartId.Pelvis).body;
            float speed = pelvis != null ? pelvis.linearVelocity.magnitude : 0f;
            bool calm = speed < 0.5f * SizeSpeed() && groundClock >= groundStruckUntil;
            groundCalm = calm ? groundCalm + dt : 0f;
            float was = groundHeavy;
            if (speed > 1.5f * SizeSpeed())
            {
                groundHeavy = 0f;
            }
            else if (groundCalm >= 0.3f || groundHeavy > 0f)
            {
                groundHeavy = Mathf.MoveTowards(groundHeavy, 1f, dt / 0.5f);
            }

            if (Mathf.Abs(groundHeavy - was) > 1e-4f)
            {
                ApplyGroundWeight();
            }
        }

        private float groundClock;

        /// <summary>Milestone 10: the speed limit on a body on the ground (<see cref="LyingFree"/>).</summary>
        private void HoldLyingSpeed(float dt)
        {
            bool free = groundClock < lyingSince + LyingFree || groundClock < groundStruckUntil;
            float k = SizeSpeed();
            float most = LyingMostSpeed * k, rise = LyingMostRise * k;
            float fall = -UnityEngine.Physics.gravity.y * dt;
            foreach (var part in Rig.Parts)
            {
                var body = part.body;
                if (body == null || body.isKinematic || Rig.IsSevered(part.id))
                {
                    continue;
                }

                int i = (int)part.id;
                Vector3 v = body.linearVelocity;
                float speed = v.magnitude;
                if (free)
                {
                    lyingCeiling[i] = Mathf.Max(most, speed);
                    continue;
                }

                // Faster than the limit: it only slows (falling, gravity may add to it).
                float ceiling = Mathf.Max(most, lyingCeiling[i] - LyingSlowing * dt + (v.y < 0f ? fall : 0f));
                lyingCeiling[i] = ceiling;
                if (speed > ceiling)
                {
                    v *= ceiling / speed;
                }

                if (v.y > rise)
                {
                    v.y = rise;
                }

                if (v != body.linearVelocity)
                {
                    body.linearVelocity = v;
                }
            }
        }

        /// <summary>Struck while on the ground (a blow, a thrown body): light again for a moment, so it is still thrown.</summary>
        private void NoteGroundStruck()
        {
            if (!onGround)
            {
                return;
            }

            groundStruckUntil = groundClock + 0.5f;
            groundCalm = 0f;
            if (groundHeavy > 0f)
            {
                groundHeavy = 0f;
                ApplyGroundWeight();
            }
        }

        private void ApplyGroundWeight()
        {
            float h = onGround ? groundHeavy : 0f;
            var bodyMaterial = PhysicsMaterials.Get("Body", h >= 0.5f ? GroundFriction : profile.bodyFriction);
            foreach (var part in Rig.Parts)
            {
                if (part.body == null || Rig.IsSevered(part.id))
                {
                    continue;
                }

                part.body.linearDamping = Mathf.Lerp(profile.linearDamping, GroundLinearDamping, h);
                part.body.angularDamping = Mathf.Lerp(profile.angularDamping, GroundAngularDamping, h);
                part.body.maxDepenetrationVelocity = Mathf.Lerp(profile.maxDepenetrationVelocity, GroundDepenetration, h);
                if (part.partCollider != null && !PartInfo.IsFoot(part.id))
                {
                    part.partCollider.sharedMaterial = bodyMaterial;
                }
            }
        }

        /// <summary>Gone: its parts no longer count as lying or standing.</summary>
        private void ForgetGround()
        {
            if (Rig != null)
            {
                GroundContacts.Set(PartColliders(), null);
                MainWeapon?.SetLying(false);
            }
        }

        /// <summary>The sandbox's ground test: knocked straight down (as if by a blow), or killed outright.</summary>
        public void DropForTest(bool dead, Vector3 push)
        {
            if (!IsAlive || !initialized)
            {
                return;
            }

            if (dead)
            {
                Die(Wound("killed by the sandbox"));
                foreach (var part in Rig.Parts)
                {
                    if (part.body != null && !Rig.IsSevered(part.id))
                    {
                        part.body.AddForce(push, ForceMode.VelocityChange);
                    }
                }
            }
            else
            {
                KnockDown(push, false, "the sandbox's ground test");
            }
        }
    }
}
