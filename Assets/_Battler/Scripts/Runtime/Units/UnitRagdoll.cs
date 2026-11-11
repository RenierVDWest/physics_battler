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

        private bool onGround;
        private float groundCalm, groundHeavy, groundStruckUntil;

        /// <summary>Damping and friction once a body has settled, its separation speed, and the solver work while it lies.</summary>
        public const float GroundLinearDamping = 1f, GroundAngularDamping = 4f, GroundFriction = 0.9f, GroundDepenetration = 1f;
        /// <summary>How long a blow on a body on the ground leaves it light (s).</summary>
        public const float GroundStruckFree = 0.5f;
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

        private float groundClock;

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
