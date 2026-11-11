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
        /// <summary>A body on the ground: down (alive) or dead.</summary>
        public bool Lying => initialized && (!IsAlive || State == UnitState.Down);

        // ---- On the ground (milestone 9b) -------------------------------------------------------

        /// <summary>Pairs of its own parts that don't share a joint and don't touch at rest: they collide while it lies on the ground.</summary>
        private readonly List<(Collider a, Collider b)> groundPairs = new List<(Collider, Collider)>();

        private bool onGround;
        private float groundCalm, groundHeavy, groundStruckUntil;

        /// <summary>Damping and friction once a body has settled, its separation speed, and the solver work while it lies.</summary>
        public const float GroundLinearDamping = 1f, GroundAngularDamping = 4f, GroundFriction = 0.9f, GroundDepenetration = 1f;
        public const int GroundSolverIterations = 20, GroundSolverVelocityIterations = 8;

        /// <summary>The joints' muscle tone on the ground (damper per square-root kilogram): a downed unit more than a dead one.</summary>
        public const float DownTone = 0.6f, DeadTone = 0.35f;

        /// <summary>
        /// Milestone 10: a body on the ground never flies off by itself. No part rises faster than it
        /// was sent up (gravity slowing that) or than <see cref="LyingMostRise"/>. For
        /// <see cref="LyingFree"/> seconds after it falls or is struck it otherwise moves as it was sent; after that no part of it
        /// goes faster than <see cref="LyingMostSpeed"/> (m/s at medium size) or rises faster than
        /// <see cref="LyingMostRise"/>, and a part already faster (still rolling, thrown) only slows
        /// (by <see cref="LyingSlowing"/> m/s², falling aside). The playtests had a downed large one
        /// thrown 7.8 m up and a medium one shaking for a second against a standing unit's legs: a
        /// limb trapped (under a foot, past a joint's limit) and the solver pushing the rest away.
        /// </summary>
        public const float LyingFree = 1f, LyingMostSpeed = 4f, LyingMostRise = 2.5f, LyingSlowing = 8f;

        /// <summary>How long what a body is sent with (a fall, a blow) is taken in before it may only slow (s).</summary>
        public const float LyingCapture = 0.15f;

        /// <summary>How long a blow on a body on the ground leaves it light (s).</summary>
        public const float GroundStruckFree = 0.5f;

        private float lyingSince, lyingSent = -1f;
        private readonly float[] lyingCeiling = new float[PartInfo.Count];
        private readonly float[] lyingRise = new float[PartInfo.Count];

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
            // Sent (fell, or struck on the ground): what it is sent with is taken in for a moment
            // (LyingCapture: the blow's push lands a step or two later); after that it only slows.
            float sent = Mathf.Max(lyingSince, groundStruckUntil - GroundStruckFree);
            if (sent != lyingSent)
            {
                lyingSent = sent;
                System.Array.Clear(lyingCeiling, 0, lyingCeiling.Length);
                System.Array.Clear(lyingRise, 0, lyingRise.Length);
            }

            bool capture = groundClock < sent + LyingCapture;
            bool free = groundClock < sent + LyingFree;
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
                if (capture)
                {
                    lyingCeiling[i] = Mathf.Max(lyingCeiling[i], most, speed);
                    lyingRise[i] = Mathf.Max(lyingRise[i], rise, v.y);
                    continue;
                }

                // Rising: no faster than it was sent up, gravity slowing that (a launch flies its arc;
                // nothing after it adds lift: the playtests had a body thrown 2.7 m up by its own joints
                // a fifth of a second after a blow launched it at 4.8 m/s).
                lyingRise[i] = Mathf.Max(rise, lyingRise[i] - fall);
                if (v.y > lyingRise[i])
                {
                    v.y = lyingRise[i];
                }

                // Past the free moment, no faster than it was going: it only slows (falling, gravity may add to it).
                if (!free)
                {
                    float ceiling = Mathf.Max(most, lyingCeiling[i] - LyingSlowing * dt + (v.y < 0f ? fall : 0f));
                    lyingCeiling[i] = ceiling;
                    float now = v.magnitude;
                    if (now > ceiling)
                    {
                        v *= ceiling / now;
                    }
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

            groundStruckUntil = groundClock + GroundStruckFree;
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
