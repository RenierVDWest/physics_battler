using UnityEngine;

namespace PhysicsBattler.Rig
{
    /// <summary>
    /// The unit's physical rig: every body part by <see cref="PartId"/>, and the marker nulls
    /// (weapon slots and aim points). Filled in by <see cref="UnitBuilder"/> when the prefab is built.
    /// </summary>
    [DisallowMultipleComponent]
    public class UnitRig : MonoBehaviour
    {
        [SerializeField] private BodyPart[] parts = new BodyPart[PartInfo.Count];
        [SerializeField] private Transform slotWeaponR;
        [SerializeField] private Transform slotWeaponL;
        [SerializeField] private Transform aimChest;
        [SerializeField] private Transform aimHead;
        [SerializeField] private Transform aimNeck;
        [SerializeField] private Transform aimBelly;
        [SerializeField] private float scale = 1f;

        public Transform SlotWeaponR => slotWeaponR;
        public Transform SlotWeaponL => slotWeaponL;
        public Transform AimChest => aimChest;
        public Transform AimHead => aimHead;
        public Transform AimNeck => aimNeck;
        public Transform AimBelly => aimBelly;

        /// <summary>Size relative to the rig file (the goblin is a 0.62 orc).</summary>
        public float Scale => scale;

        public BodyPart Part(PartId id) => parts[(int)id];

        // Milestone 5: parts cut off (and everything hanging from them). They have left the unit:
        // loose physics objects of their own, not posed, not counted, not struck as the unit.
        private readonly bool[] severed = new bool[PartInfo.Count];
        private readonly int[] cutAt = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

        /// <summary>The joint (the part below it) a cut-off part came off at, or null.</summary>
        public PartId? CutAt(PartId id) => cutAt[(int)id] >= 0 ? (PartId)cutAt[(int)id] : (PartId?)null;

        /// <summary>The part has been cut off the unit (at its own joint or one above it).</summary>
        public bool IsSevered(PartId id) => severed[(int)id];

        public bool AnySevered
        {
            get
            {
                foreach (bool s in severed)
                {
                    if (s)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Cuts a part off at its joint, with every part hanging from it: the joint goes, the limb
        /// leaves the unit's hierarchy as a loose physics object (its meshes, colliders and anything
        /// held in its hand go with it). Returns the limb's root object, or null if it was already off.
        /// </summary>
        public GameObject Sever(PartId at)
        {
            if (!PartInfo.IsSeverable(at) || severed[(int)at])
            {
                return null;
            }

            // The limb's own object: every part of it under one (in play the parts lie side by side
            // under the unit's body holder, not in a tree), so it goes as one thing.
            var limb = new GameObject(name + " " + PartInfo.NullName(at) + " (cut off)");
            var root = parts[(int)at];
            limb.transform.SetPositionAndRotation(root.transform.position, root.transform.rotation);
            foreach (var id in PartInfo.All)
            {
                // (A part already cut off lower down stays where it lies.)
                if ((id != at && !PartInfo.IsBelow(id, at)) || severed[(int)id])
                {
                    continue;
                }

                severed[(int)id] = true;
                cutAt[(int)id] = (int)at;
                var part = parts[(int)id];
                part.Owner = null;
                if (part.body != null)
                {
                    part.body.isKinematic = false;
                }

                // Its joints within the limb hold their anatomical limits again (the pose had freed
                // them): an elbow of a cut-off arm still bends like an elbow.
                if (id != at && part.joint != null)
                {
                    PhysicsBattler.Muscles.JointMath.ApplyLimits(part.joint, part.limits);
                    part.joint.angularXMotion = ConfigurableJointMotion.Limited;
                    part.joint.angularYMotion = ConfigurableJointMotion.Limited;
                    part.joint.angularZMotion = ConfigurableJointMotion.Limited;
                }
            }

            foreach (var id in PartInfo.All)
            {
                if (cutAt[(int)id] == (int)at)
                {
                    var t = parts[(int)id].transform;
                    var above = t.parent != null ? t.parent.GetComponent<BodyPart>() : null;
                    if (above == null || cutAt[(int)above.id] != (int)at)
                    {
                        t.SetParent(limb.transform, true);   // (one hanging from another cut part goes with it)
                    }
                }
            }

            if (root.joint != null)
            {
                // (Freed at once: a destroyed joint only goes at the end of the frame, and would hold
                // the limb on for this physics step.)
                var joint = root.joint;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Free;
                joint.angularXMotion = joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Free;
                if (Application.isPlaying)
                {
                    Destroy(joint);
                }
                else
                {
                    DestroyImmediate(joint);
                }

                root.joint = null;
            }

            return limb;
        }

        public BodyPart[] Parts => parts;

        public void Initialize(BodyPart[] builtParts, Transform slotR, Transform slotL, Transform chestAim,
            Transform headAim, Transform neckAim, Transform bellyAim, float rigScale)
        {
            aimNeck = neckAim;
            aimBelly = bellyAim;
            parts = builtParts;
            slotWeaponR = slotR;
            slotWeaponL = slotL;
            aimChest = chestAim;
            aimHead = headAim;
            scale = rigScale;
        }

        public bool IsComplete()
        {
            if (parts == null || parts.Length != PartInfo.Count)
            {
                return false;
            }

            foreach (var part in parts)
            {
                if (part == null || part.body == null)
                {
                    return false;
                }
            }

            return slotWeaponR != null && aimChest != null;
        }

        /// <summary>Total mass of all parts (and anything their rigidbodies carry, like a weapon).</summary>
        public float TotalMass()
        {
            float mass = 0f;
            foreach (var part in parts)
            {
                if (!severed[(int)part.id])
                {
                    mass += part.body.mass;
                }
            }

            return mass;
        }

        /// <summary>Mass-weighted centre of all the parts.</summary>
        public Vector3 CenterOfMass()
        {
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (var part in parts)
            {
                if (severed[(int)part.id])
                {
                    continue;
                }

                sum += part.body.worldCenterOfMass * part.body.mass;
                mass += part.body.mass;
            }

            return mass > 0f ? sum / mass : parts[0].transform.position;
        }
    }
}
