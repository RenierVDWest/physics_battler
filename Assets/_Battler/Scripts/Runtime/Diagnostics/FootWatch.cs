using System;
using System.Collections.Generic;
using System.Text;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// The legs and feet watched (for the walking work): each step, for each foot, where it is and
    /// which way it points (as posed, and as the physics body has it), against where the stance has
    /// it planted and which way it means it to point; how high its sole is off the ground; and while
    /// it is planted, how fast it slides and twists. Each plant is judged when the foot lifts again
    /// (a "foot" event when it slid, twisted, floated or sank). While the unit walks, how far its feet
    /// travel against how far its body does (well under 1 is the body gliding over feet that don't
    /// keep up). Summed for session.md's Walking lines.
    /// </summary>
    internal sealed class FootWatch
    {
        /// <summary>A plant is noted as an event past these: slid (metres), twisted (degrees), off the ground (metres, up or down).</summary>
        public const float SlideNote = 0.03f, TwistNote = 10f, FloatNote = 0.025f;

        /// <summary>A planted sole further than this off the ground counts as floating (or sunk).</summary>
        public const float Contact = 0.02f;

        private sealed class Foot
        {
            public PartId part;
            public string side;
            /// <summary>The foot's own forward (the pose turns it by the stance's yaw about up: its +Z).</summary>
            public Vector3 localForward = Vector3.forward;
            public float soleOffset = float.NaN;

            // Last step's, for speeds.
            public Vector3 lastPhys, lastAnim;
            public float lastPhysYaw, lastAnimYaw;
            public bool hasLast, wasPlanted;

            // The plant under way.
            public Vector3 plantStartPhys, plantStartAnim;
            public float plantStartPhysYaw, plantStartAnimYaw, plantTime, plantHeightSum, plantHeightMost, plantHeightLeast;
            public int plantSamples;
            public float travel;
        }

        private readonly Foot[] feet = new Foot[2];
        private readonly UnitController unit;
        private readonly Action<string> note;

        // Summary figures (standing only).
        public float plantedTime, floatTime, sinkTime, walkTime;
        public readonly List<float> slideSpeeds = new List<float>(), animSlideSpeeds = new List<float>(), twistSpeeds = new List<float>();
        public readonly List<float> yawOff = new List<float>(), hipsOff = new List<float>(), moveOff = new List<float>(), soleHeights = new List<float>();
        public float walkForward, walkSideways, walkBackward;
        public int plants, plantsSlid, plantsTwisted, plantsFloated, plantsSunk;
        public float worstPlantSlide, worstPlantTwist, worstFloat, worstSink;
        public float rootTravelWalking, footTravelWalking;
        private Vector3 lastRoot;
        private bool hasRoot;

        public FootWatch(UnitController unit, Action<string> note)
        {
            this.unit = unit;
            this.note = note;
            feet[Stance.Left] = new Foot { part = PartId.FootL, side = "L" };
            feet[Stance.Right] = new Foot { part = PartId.FootR, side = "R" };
        }

        /// <summary>Which way a foot points, flat (degrees about up, as the root's yaw).</summary>
        private static float YawOf(Quaternion rotation, Vector3 localForward)
        {
            Vector3 f = rotation * localForward;
            f.y = 0f;
            return f.sqrMagnitude > 1e-8f ? Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg : 0f;
        }

        private static float Flat(Vector3 a, Vector3 b)
        {
            Vector3 d = a - b;
            d.y = 0f;
            return d.magnitude;
        }

        /// <summary>Each step: the columns, and the plants and walking judged.</summary>
        public void Sample(TableWriter frames, float dt)
        {
            var stance = unit.Stance;
            var anim = unit.Anim;
            bool upright = unit.IsAlive && unit.State == UnitState.Standing;
            bool walking = unit.Gait != null && unit.Gait.Weight >= 0.3f;
            float ground = stance.GroundY;
            Vector3 root = unit.Root.position;
            for (int f = 0; f < 2; f++)
            {
                var foot = feet[f];
                var bodyPart = unit.Rig.Part(foot.part);
                var animFoot = anim.Part(foot.part);
                Vector3 phys = bodyPart.body != null ? bodyPart.body.position : bodyPart.transform.position;
                Quaternion physRot = bodyPart.body != null ? bodyPart.body.rotation : bodyPart.transform.rotation;
                Vector3 animAt = animFoot.position;
                float physYaw = YawOf(physRot, foot.localForward), animYaw = YawOf(animFoot.rotation, foot.localForward);
                bool planted = upright && !stance.Stepping(f);

                // The sole off the ground: the collider's lowest point (the foot's centre less its height
                // over its sole when first planted, where there is no collider to measure).
                var bounds = bodyPart.partCollider != null ? bodyPart.partCollider.bounds : default;
                bool measured = bounds.size.sqrMagnitude > 1e-8f;
                if (float.IsNaN(foot.soleOffset) && planted && !walking)
                {
                    foot.soleOffset = measured ? phys.y - bounds.min.y : animAt.y - ground;
                }

                float offset = float.IsNaN(foot.soleOffset) ? 0f : foot.soleOffset;
                float physSole = measured ? bounds.min.y - ground : phys.y - offset - ground;
                float animSole = animAt.y - offset - ground;

                float physSpeed = 0f, animSpeed = 0f, twist = 0f;
                if (foot.hasLast && dt > 0f)
                {
                    physSpeed = Flat(phys, foot.lastPhys) / dt;
                    animSpeed = Flat(animAt, foot.lastAnim) / dt;
                    twist = Mathf.Abs(Mathf.DeltaAngle(foot.lastPhysYaw, physYaw)) / dt;
                }

                float plantYaw = stance.Yaw(f);
                float off = Mathf.DeltaAngle(plantYaw, animYaw);
                float hipsYaw = PhysicsBattler.Combat.ShieldCone.FacingOf(anim.Part(PartId.Pelvis).rotation, unit.FacingYaw);
                float offHips = Mathf.DeltaAngle(hipsYaw, animYaw);
                frames.Add("footH" + foot.side, physSole, 3);
                frames.Add("aFootH" + foot.side, animSole, 3);
                frames.Add("footYaw" + foot.side, physYaw, 1);
                frames.Add("aFootYaw" + foot.side, animYaw, 1);
                frames.Add("plantYaw" + foot.side, plantYaw, 1);
                frames.Add("footVsHips" + foot.side, offHips, 1);
                frames.Add("aSlide" + foot.side, planted ? Flat(animAt, stance.Position(f)) : 0f, 3);
                frames.Add("slip" + foot.side, planted ? physSpeed : 0f, 3);
                frames.Add("aSlip" + foot.side, planted ? animSpeed : 0f, 3);
                frames.Add("twist" + foot.side, planted ? twist : 0f, 1);

                if (planted)
                {
                    if (!foot.wasPlanted)
                    {
                        foot.plantStartPhys = phys;
                        foot.plantStartAnim = animAt;
                        foot.plantStartPhysYaw = physYaw;
                        foot.plantStartAnimYaw = animYaw;
                        foot.plantTime = 0f;
                        foot.plantSamples = 0;
                        foot.plantHeightSum = 0f;
                        foot.plantHeightMost = float.MinValue;
                        foot.plantHeightLeast = float.MaxValue;
                    }

                    foot.plantTime += dt;
                    foot.plantSamples++;
                    foot.plantHeightSum += physSole;
                    foot.plantHeightMost = Mathf.Max(foot.plantHeightMost, physSole);
                    foot.plantHeightLeast = Mathf.Min(foot.plantHeightLeast, physSole);
                    plantedTime += dt;
                    floatTime += physSole > Contact ? dt : 0f;
                    sinkTime += physSole < -Contact ? dt : 0f;
                    if (foot.hasLast)
                    {
                        slideSpeeds.Add(physSpeed);
                        animSlideSpeeds.Add(animSpeed);
                        twistSpeeds.Add(twist);
                    }

                    yawOff.Add(Mathf.Abs(off));
                    hipsOff.Add(Mathf.Abs(offHips));
                    soleHeights.Add(physSole);
                }
                else if (foot.wasPlanted)
                {
                    EndPlant(foot, phys, animAt, physYaw, animYaw);
                }

                if (upright && walking && unit.Gait.Velocity.sqrMagnitude > 0.01f)
                {
                    Vector3 v = unit.Gait.Velocity;
                    float moveYaw = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
                    if (f == 0)
                    {
                        // (Which way it walks against which way it faces: forward, sideways, backward.)
                        float rel = Mathf.Abs(Mathf.DeltaAngle(unit.FacingYaw, moveYaw));
                        walkForward += rel < 45f ? dt : 0f;
                        walkSideways += rel >= 45f && rel <= 135f ? dt : 0f;
                        walkBackward += rel > 135f ? dt : 0f;
                    }

                    // (Against the way it walks, folded so walking backward on forward-pointing feet counts as in line.)
                    float d = Mathf.Abs(Mathf.DeltaAngle(moveYaw, animYaw));
                    moveOff.Add(Mathf.Min(d, 180f - d));
                    if (foot.hasLast)
                    {
                        footTravelWalking += 0.5f * Flat(animAt, foot.lastAnim);
                    }
                }

                foot.wasPlanted = planted;
                foot.lastPhys = phys;
                foot.lastAnim = animAt;
                foot.lastPhysYaw = physYaw;
                foot.lastAnimYaw = animYaw;
                foot.hasLast = true;
            }

            // The body against its feet while it walks.
            if (upright && walking && hasRoot)
            {
                walkTime += dt;
                rootTravelWalking += Flat(root, lastRoot);
            }

            lastRoot = root;
            hasRoot = true;
        }

        /// <summary>The foot lifts: its plant judged (and noted if it slid, twisted, floated or sank).</summary>
        private void EndPlant(Foot foot, Vector3 phys, Vector3 animAt, float physYaw, float animYaw)
        {
            if (foot.plantSamples < 2)
            {
                return;
            }

            plants++;
            float slid = Flat(foot.lastPhys, foot.plantStartPhys);
            float animSlid = Flat(foot.lastAnim, foot.plantStartAnim);
            float twisted = Mathf.Abs(Mathf.DeltaAngle(foot.plantStartPhysYaw, foot.lastPhysYaw));
            float mean = foot.plantHeightSum / foot.plantSamples;
            worstPlantSlide = Mathf.Max(worstPlantSlide, slid);
            worstPlantTwist = Mathf.Max(worstPlantTwist, twisted);
            worstFloat = Mathf.Max(worstFloat, foot.plantHeightMost);
            worstSink = Mathf.Max(worstSink, -foot.plantHeightLeast);
            var what = new StringBuilder();
            if (slid > SlideNote || animSlid > SlideNote)
            {
                plantsSlid++;
                what.Append($"slid {slid * 100f:0} cm (posed {animSlid * 100f:0} cm)");
            }

            if (twisted > TwistNote)
            {
                plantsTwisted++;
                what.Append(what.Length > 0 ? ", " : "").Append($"twisted {twisted:0}°");
            }

            if (mean > FloatNote)
            {
                plantsFloated++;
                what.Append(what.Length > 0 ? ", " : "").Append($"floated {mean * 100f:0} cm off the ground");
            }
            else if (mean < -FloatNote)
            {
                plantsSunk++;
                what.Append(what.Length > 0 ? ", " : "").Append($"sank {-mean * 100f:0} cm into the ground");
            }

            if (what.Length > 0)
            {
                bool walking = unit.Gait != null && unit.Gait.Weight >= 0.3f;
                note?.Invoke($"{(foot.side == "L" ? "left" : "right")} foot, planted {foot.plantTime:0.00} s{(walking ? " while walking" : "")}: {what}");
            }
        }

        private static float Percentile(List<float> values, float p)
        {
            if (values.Count == 0)
            {
                return 0f;
            }

            var sorted = new List<float>(values);
            sorted.Sort();
            return sorted[Mathf.Clamp(Mathf.RoundToInt(p * (sorted.Count - 1)), 0, sorted.Count - 1)];
        }

        /// <summary>The Walking line for session.md.</summary>
        public void Write(StringBuilder md)
        {
            if (plantedTime <= 0f)
            {
                return;
            }

            float ratio = rootTravelWalking > 1e-3f ? footTravelWalking / rootTravelWalking : 0f;
            md.AppendLine($"Feet: {plants} plants, {plantsSlid} slid over {SlideNote * 100f:0} cm, {plantsTwisted} twisted over {TwistNote:0}°, "
                          + $"{plantsFloated} floated and {plantsSunk} sank over {FloatNote * 100f:0.0} cm (worst: slid {worstPlantSlide * 100f:0} cm, twisted {worstPlantTwist:0}°, "
                          + $"{worstFloat * 100f:0.0} cm up, {worstSink * 100f:0.0} cm down). Planted feet: off the ground {100f * floatTime / plantedTime:0}% of the time, "
                          + $"in it {100f * sinkTime / plantedTime:0}%; sole height median {Percentile(soleHeights, 0.5f) * 100f:0.0} cm; "
                          + $"sliding (physical) median {Percentile(slideSpeeds, 0.5f) * 100f:0} cm/s, 95% under {Percentile(slideSpeeds, 0.95f) * 100f:0} cm/s; "
                          + $"(posed) 95% under {Percentile(animSlideSpeeds, 0.95f) * 100f:0} cm/s; twisting 95% under {Percentile(twistSpeeds, 0.95f):0}°/s; "
                          + $"pointing off their planned way 95% under {Percentile(yawOff, 0.95f):0}°, off the hips' way median {Percentile(hipsOff, 0.5f):0}°, 95% under {Percentile(hipsOff, 0.95f):0}°.");
            if (walkTime > 0f)
            {
                float walked = Mathf.Max(1e-3f, walkForward + walkSideways + walkBackward);
                md.AppendLine($"Walking: {walkTime:0.0} s ({100f * walkForward / walked:0}% forward, {100f * walkSideways / walked:0}% sideways, {100f * walkBackward / walked:0}% backward), "
                              + $"the body went {rootTravelWalking:0.0} m and the feet {footTravelWalking:0.0} m each on average "
                              + $"(feet/body {ratio:0.00}; well under 1 is gliding); the feet point off the line it walks along (either way) median {Percentile(moveOff, 0.5f):0}°, "
                              + $"95% under {Percentile(moveOff, 0.95f):0}°.");
            }

            md.AppendLine();
        }
    }
}
