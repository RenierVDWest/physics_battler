using System.Collections.Generic;
using System.Text;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The locomotion lab's measures, step by step (the plan's Measures table): how far a planted foot
    /// moves during its plant (posed, and physical when a body is given), how far its sole is off the
    /// ground, time with both feet up while walking, the feet's travel against the body's, steps that
    /// set off before the other foot has landed, the biggest jump any part makes in a step, how hard
    /// the locks pull against the clips (the slide they hide), and how often a lock had to let go.
    /// </summary>
    public sealed class LocoMeasures
    {
        private readonly AnimRig anim;
        private readonly UnitRig body;
        private readonly Locomotor loco;
        private readonly LocoBody poser;

        private sealed class Plant
        {
            public bool on;
            public Vector3 start, physStart;
            public float worst, physWorst;
            public bool onToe;
        }

        private readonly Plant[] plants = { new Plant(), new Plant() };
        private readonly Vector3[] lastPart = new Vector3[PartInfo.Count];
        private bool hasLast;
        private Vector3 lastRoot;
        private readonly Vector3[] lastFoot = new Vector3[2];

        public int Steps, PlantsDone;
        public float PlantDriftWorst, PlantDriftSum, PhysDriftWorst, PhysDriftSum;
        public float SoleWorst;
        public float WalkTime, BothUpWalking, RunTime;
        public float BodyTravel, FeetTravel;
        public int EarlyLifts;
        public float PartJumpWorst;
        public string PartJumpWhat = "";
        public float ReachMissWorst;
        public readonly List<float> Pulls = new List<float>();
        public readonly List<float> LandingMisses = new List<float>();
        public float SwingFootFastest;
        public readonly List<string> Whips = new List<string>();
        private float lastWhip = -1f;
        public string SwingFootWhat = "";
        public int ForcedReleases;
        public readonly List<string> Notes = new List<string>();

        /// <summary>A plant counts as a slide past this (metres).</summary>
        public const float SlideGate = 0.01f;

        public LocoMeasures(AnimRig anim, Locomotor loco, LocoBody poser, UnitRig body = null)
        {
            this.anim = anim;
            this.loco = loco;
            this.poser = poser;
            this.body = body;
            restAnkle = anim.RestLocalPosition(PartId.Pelvis).y + anim.RestLocalPosition(PartId.ThighL).y
                        + anim.RestLocalPosition(PartId.ShinL).y + anim.RestLocalPosition(PartId.FootL).y;
        }

        private readonly float restAnkle;

        private static readonly PartId[] FootParts = { PartId.FootL, PartId.FootR };

        public void Sample(float dt, float time, string where)
        {
            Steps++;
            bool cycling = loco.Current == Locomotor.Mode.Cycle;
            bool walking = cycling && loco.RunWeight < 0.5f;
            if (walking)
            {
                WalkTime += dt;
            }
            else if (cycling)
            {
                RunTime += dt;
            }

            bool up0 = loco.Down[0] < 0.5f, up1 = loco.Down[1] < 0.5f;
            if (walking && up0 && up1)
            {
                BothUpWalking += dt;
            }

            for (int f = 0; f < 2; f++)
            {
                var lk = poser.Feet[f];
                Vector3 p = poser.ContactPoint(f);
                Vector3 phys = body != null ? body.Part(FootParts[f]).body.position : p;
                ReachMissWorst = Mathf.Max(ReachMissWorst, Vector3.Distance(anim.Part(FootParts[f]).position, lk.Target));
                var plant = plants[f];
                if (lk.Locked && !plant.on)
                {
                    LandingMisses.Add(lk.LandedOff);
                    plant.on = true;
                    plant.onToe = lk.OnToe;
                    plant.start = p;
                    plant.physStart = phys;
                    plant.worst = plant.physWorst = 0f;
                    // (A step set off while the other foot was still in the air.)
                    if (walking && !poser.Feet[1 - f].Locked)
                    {
                        // fine: it lands; the other foot will land too
                    }
                }
                else if (!lk.Locked && plant.on)
                {
                    plant.on = false;
                    PlantsDone++;
                    PlantDriftSum += plant.worst;
                    PhysDriftSum += plant.physWorst;
                    PlantDriftWorst = Mathf.Max(PlantDriftWorst, plant.worst);
                    PhysDriftWorst = Mathf.Max(PhysDriftWorst, plant.physWorst);
                    Pulls.Add(lk.Released);
                    if (plant.worst > SlideGate && Notes.Count < 40)
                    {
                        Notes.Add($"{time:0.00} s, {where}: the {(f == 0 ? "left" : "right")} foot moved {plant.worst * 100f:0.0} cm while planted");
                    }

                    if (walking && !poser.Feet[1 - f].Locked)
                    {
                        EarlyLifts++;
                    }
                }

                if (plant.on && lk.Locked && lk.OnToe != plant.onToe)
                {
                    // (Rolled onto the toe, or down onto the heel: the point on the ground changes.)
                    plant.onToe = lk.OnToe;
                    plant.start = p;
                    plant.physStart = phys;
                }

                if (plant.on)
                {
                    Vector3 d = p - plant.start;
                    d.y = 0f;
                    plant.worst = Mathf.Max(plant.worst, d.magnitude);
                    Vector3 dp = phys - plant.physStart;
                    dp.y = 0f;
                    plant.physWorst = Mathf.Max(plant.physWorst, dp.magnitude);
                    // (The sole: the ankle's height over its resting height.)
                    float sole = anim.Part(FootParts[f]).position.y - poser.GroundY - restAnkle;
                    if (lk.Locked && !lk.OnToe)
                    {
                        SoleWorst = Mathf.Max(SoleWorst, Mathf.Abs(sole));
                    }
                }

                // (Travel and swing speed by the ankle: the contact point jumps from heel to toe as the foot rolls.)
                Vector3 ankle = anim.Part(FootParts[f]).position;
                if (cycling && hasLast)
                {
                    Vector3 m = ankle - lastFoot[f];
                    m.y = 0f;
                    FeetTravel += 0.5f * m.magnitude;
                }

                if (hasLast && dt > 0f && !lk.Locked)
                {
                    // (A swinging foot's speed against the body's: a foot that whips about shows here.)
                    Vector3 rel = (ankle - lastFoot[f]) / dt - loco.Velocity;
                    rel.y = 0f;
                    if (rel.magnitude > 5f && time - lastWhip > 0.3f)
                    {
                        lastWhip = time;
                        Whips.Add($"{time:0.00} s, {where}: the {(f == 0 ? "left" : "right")} foot swung at {rel.magnitude:0.0} m/s ({loco.Current}{(loco.Shot != null ? " " + loco.Shot.Name : "")})");
                    }

                    if (rel.magnitude > SwingFootFastest)
                    {
                        SwingFootFastest = rel.magnitude;
                        SwingFootWhat = $"{(f == 0 ? "left" : "right")} at {time:0.00} s ({where})";
                    }
                }

                lastFoot[f] = ankle;
            }

            ForcedReleases = poser.Feet[0].ForcedReleases + poser.Feet[1].ForcedReleases;
            Vector3 root = loco.Position;
            if (cycling && hasLast)
            {
                Vector3 m = root - lastRoot;
                m.y = 0f;
                BodyTravel += m.magnitude;
            }

            lastRoot = root;
            foreach (var id in PartInfo.All)
            {
                Vector3 p = anim.Part(id).position;
                if (hasLast && dt > 0f)
                {
                    float jump = (p - lastPart[(int)id]).magnitude / dt;
                    if (jump > PartJumpWorst)
                    {
                        PartJumpWorst = jump;
                        PartJumpWhat = $"{id} at {time:0.00} s ({where})";
                    }
                }

                lastPart[(int)id] = p;
            }

            hasLast = true;
        }

        private static float Percentile(List<float> values, float p)
        {
            if (values.Count == 0)
            {
                return 0f;
            }

            var s = new List<float>(values);
            s.Sort();
            return s[Mathf.Clamp(Mathf.RoundToInt(p * (s.Count - 1)), 0, s.Count - 1)];
        }

        public string Report()
        {
            var sb = new StringBuilder();
            float ratio = BodyTravel > 1e-3f ? FeetTravel / BodyTravel : 0f;
            sb.AppendLine($"Plants: {PlantsDone}. Planted feet moved (posed) at most {PlantDriftWorst * 100f:0.0} cm, {PlantDriftSum / Mathf.Max(1, PlantsDone) * 100f:0.00} cm on average (gate under 1 cm)"
                          + (body != null ? $"; physical at most {PhysDriftWorst * 100f:0.0} cm, {PhysDriftSum / Mathf.Max(1, PlantsDone) * 100f:0.00} cm on average" : "") + ".");
            sb.AppendLine($"The locks pulled the feet from the clips' by {Percentile(Pulls, 0.5f) * 100f:0.0} cm (median), {Percentile(Pulls, 0.95f) * 100f:0.0} cm (95%), {(Pulls.Count > 0 ? Percentile(Pulls, 1f) * 100f : 0f):0.0} cm at most; {ForcedReleases} locks let go early (over-stretched).");
            sb.AppendLine($"Feet landed off the clips' by {Percentile(LandingMisses, 0.5f) * 100f:0.0} cm (median), {Percentile(LandingMisses, 0.95f) * 100f:0.0} cm (95%); the fastest swinging foot went {SwingFootFastest:0.0} m/s against the body ({SwingFootWhat}).");
            sb.AppendLine($"Walking {WalkTime:0.0} s, running {RunTime:0.0} s; both feet up while walking {BothUpWalking:0.00} s ({100f * BothUpWalking / Mathf.Max(1e-3f, WalkTime):0.0}%, gate 0%); steps lifted before the other foot was down: {EarlyLifts}.");
            sb.AppendLine($"Feet travelled {FeetTravel:0.0} m against the body's {BodyTravel:0.0} m: {ratio:0.00} (gate 0.95 to 1.05).");
            sb.AppendLine($"Planted soles off the ground at most {SoleWorst * 100f:0.0} cm (gate 1 cm); the ankles missed their targets by at most {ReachMissWorst * 100f:0.0} cm; fastest part {PartJumpWorst:0.0} m/s ({PartJumpWhat}).");
            foreach (var n in Whips)
            {
                sb.AppendLine("  " + n);
            }

            foreach (var n in Notes)
            {
                sb.AppendLine("  " + n);
            }

            return sb.ToString();
        }
    }
}
