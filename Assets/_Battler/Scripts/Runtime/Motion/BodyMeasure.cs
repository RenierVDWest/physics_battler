using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Lengths and rest positions of a body, measured once from its animation rig in the rest pose,
    /// in the unit's own space (x right, y up from the ground, z forward). Everything the pose
    /// pipeline does is in proportion to these, so one combat style fits any body size.
    /// </summary>
    public sealed class BodyMeasure
    {
        public float PelvisHeight;         // pelvis pivot above the ground, standing straight
        public Vector3 HipL, HipR;         // hip pivots relative to the pelvis pivot (rest)
        public float Thigh, Shin;          // hip-knee and knee-ankle lengths
        public float AnkleHeight;          // ankle pivot above the ground
        public Vector3 AnkleL, AnkleR;     // ankle pivots, unit space (rest)
        public float Mass;                 // total mass of the parts
        public readonly float[] PartMass = new float[PartInfo.Count];

        /// <summary>Hip to ankle when the leg is straight.</summary>
        public float LegLength => Thigh + Shin;

        /// <summary>Shoulder to shoulder (upper-arm pivots), metres. 0.41 at medium size; the large body is much broader than it is tall.</summary>
        public float ShoulderWidth = 0.41f;

        /// <summary>Breadth against the medium body: sideways distances (a swing's path, the guard) scale by this.</summary>
        public float WidthFactor => ShoulderWidth / 0.41f;

        /// <summary>How big the body is against the medium body (its leg length / 0.845 m).</summary>
        public float SizeFactor => LegLength / 0.845f;

        public static BodyMeasure Of(Transform unitRoot, AnimRig anim, UnitRig rig)
        {
            var m = new BodyMeasure();
            Vector3 Local(PartId id) => unitRoot.InverseTransformPoint(anim.Part(id).position);
            Vector3 pelvis = Local(PartId.Pelvis);
            m.PelvisHeight = pelvis.y;
            m.HipL = Local(PartId.ThighL) - pelvis;
            m.HipR = Local(PartId.ThighR) - pelvis;
            m.Thigh = Vector3.Distance(Local(PartId.ThighL), Local(PartId.ShinL));
            m.Shin = Vector3.Distance(Local(PartId.ShinL), Local(PartId.FootL));
            m.AnkleL = Local(PartId.FootL);
            m.AnkleR = Local(PartId.FootR);
            m.AnkleHeight = (m.AnkleL.y + m.AnkleR.y) * 0.5f;
            m.ShoulderWidth = Vector3.Distance(Local(PartId.UpperArmL), Local(PartId.UpperArmR));
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                float mass = part != null && part.body != null ? part.body.mass : 1f;
                m.PartMass[(int)id] = mass;
                m.Mass += mass;
            }

            return m;
        }

        /// <summary>For tests: a medium-like body without a rig.</summary>
        public static BodyMeasure Simple(float size = 1f)
        {
            var m = new BodyMeasure
            {
                PelvisHeight = 0.985f * size,
                HipL = new Vector3(-0.095f, -0.055f, 0f) * size,
                HipR = new Vector3(0.095f, -0.055f, 0f) * size,
                Thigh = 0.43f * size,
                Shin = 0.415f * size,
                AnkleHeight = 0.085f * size,
                ShoulderWidth = 0.41f * size,
            };
            m.AnkleL = new Vector3(-0.095f, 0.085f, 0f) * size;
            m.AnkleR = new Vector3(0.095f, 0.085f, 0f) * size;
            for (int i = 0; i < PartInfo.Count; i++)
            {
                m.PartMass[i] = 5f;
                m.Mass += 5f;
            }

            return m;
        }
    }
}
