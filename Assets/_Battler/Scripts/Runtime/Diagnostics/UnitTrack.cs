using System.Text;
using PhysicsBattler.Authority;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Follows one unit through a playtest: a frames table (one row of scalars per physics step) and
    /// a pose table (every part, physical and animated), plus totals for the summary.
    ///
    /// Frames columns: t, step; root (rootX, rootZ, rootYaw); hips (pelvisH physical, animPelvisH,
    /// pelvisDrop when a foot was out of reach, weightShift); balance (comOffset: the animated centre
    /// of mass from the support point, physCom: the physical one, both horizontal metres); feet
    /// (stepL/R 0 or 1, liftL/R, slideL/R: how far each physical foot is from its planted spot,
    /// steps, shuffles); idle (swayX, swayZ, bob); authority per region (lvl_*: 0 kinematic,
    /// 1 pinned, 2 loose, 3 ragdoll; pin_*); and every part's distance (err_*, metres) and angle
    /// (aerr_*, degrees) from its pose.
    /// </summary>
    internal sealed class UnitTrack
    {
        private readonly PlaytestRecorder recorder;
        private readonly TableWriter frames;
        private readonly TableWriter pose;
        private readonly float[] errSum = new float[PartInfo.Count];
        private readonly float[] errMax = new float[PartInfo.Count];
        private readonly float[] angSum = new float[PartInfo.Count];
        private readonly float[] angMax = new float[PartInfo.Count];

        public readonly UnitController unit;
        public readonly string label;
        public readonly string stem;
        public readonly int id;
        public float spawnTime;
        public float endTime = -1f;
        public int samples;
        public float maxSlide;
        public float maxComOffset;
        public float maxPelvisDrop;
        public int steps;
        public int shuffles;

        public UnitTrack(PlaytestRecorder recorder, UnitController unit, int id, string label, string stem,
            string framesPath, string posePath)
        {
            this.recorder = recorder;
            this.unit = unit;
            this.id = id;
            this.label = label;
            this.stem = stem;
            frames = new TableWriter(framesPath);
            pose = new TableWriter(posePath);
            spawnTime = PlaytestRecorder.Now;
            if (unit != null && unit.Stance != null)
            {
                unit.Stance.Landed += OnLanded;
            }
        }

        public bool Alive => unit != null;
        public int FrameRows => frames.Rows;

        public void Close(string why)
        {
            if (unit != null && unit.Stance != null)
            {
                unit.Stance.Landed -= OnLanded;
            }

            endTime = PlaytestRecorder.Now;
            frames.Dispose();
            pose.Dispose();
        }

        public void Flush()
        {
            frames.Flush();
            pose.Flush();
        }

        private void OnLanded(int foot, Vector3 where)
        {
            if (unit == null)
            {
                return;
            }

            float error = unit.Stance.Error(foot, unit.Root.position, unit.FacingYaw);
            recorder.Event(this, "step", $"{(foot == Stance.Left ? "left" : "right")} foot planted at ({where.x:0.00}, {where.z:0.00}), "
                                         + $"{error * 100f:0} cm from its spot");
        }

        public void OnTrace(string category, string text)
        {
        }

        /// <summary>Runs before the unit's own step: the body as PhysX left it, the pose as last built.</summary>
        public void Sample(int step, bool writePose)
        {
            if (unit == null || !unit.IsInitialized)
            {
                return;
            }

            float t = PlaytestRecorder.Now;
            var root = unit.Root;
            var anim = unit.Anim;
            var stance = unit.Stance;
            var p = unit.Pose;
            float ground = root.position.y;

            frames.Begin();
            frames.Add("t", t, 4);
            frames.Add("step", step);
            frames.Add("rootX", root.position.x, 3);
            frames.Add("rootZ", root.position.z, 3);
            frames.Add("rootYaw", unit.FacingYaw, 1);
            frames.Add("pelvisH", unit.Rig.Part(PartId.Pelvis).body.position.y - ground, 3);
            frames.Add("animPelvisH", anim.Part(PartId.Pelvis).position.y - ground, 3);
            frames.Add("pelvisDrop", p.PelvisDrop, 3);
            frames.Add("weightShift", p.WeightShift, 3);

            Vector3 support = p.SupportPoint;
            Vector3 com = p.CenterOfMass - support;
            com.y = 0f;
            Vector3 phys = unit.Rig.CenterOfMass() - support;
            phys.y = 0f;
            frames.Add("comOffset", com.magnitude, 3);
            frames.Add("physCom", phys.magnitude, 3);
            maxComOffset = Mathf.Max(maxComOffset, com.magnitude);
            maxPelvisDrop = Mathf.Max(maxPelvisDrop, p.PelvisDrop);

            for (int f = 0; f < 2; f++)
            {
                string side = f == Stance.Left ? "L" : "R";
                frames.Add("step" + side, stance.Stepping(f));
                frames.Add("lift" + side, stance.Lift(f), 3);
                var footPart = unit.Rig.Part(f == Stance.Left ? PartId.FootL : PartId.FootR).body.position;
                Vector3 planted = stance.Position(f);
                Vector3 d = footPart - planted;
                d.y = 0f;
                float slide = stance.Stepping(f) ? 0f : d.magnitude;
                frames.Add("slide" + side, slide, 3);
                maxSlide = Mathf.Max(maxSlide, slide);
            }

            steps = stance.StepsTaken;
            shuffles = stance.ShufflesTaken;
            frames.Add("steps", steps);
            frames.Add("shuffles", shuffles);
            var idle = p.LastIdle;
            frames.Add("swayX", idle.hipShift.x, 3);
            frames.Add("swayZ", idle.hipShift.z, 3);
            frames.Add("bob", idle.bob, 3);

            for (int r = 0; r < 5; r++)
            {
                var region = (BodyRegion)r;
                frames.Add("lvl_" + region, (int)unit.Authority.Level(region));
                frames.Add("pin_" + region, unit.Authority.Pin(region), 2);
            }

            foreach (var part in PartInfo.All)
            {
                unit.Authority.Error(part, out float distance, out float angle);
                frames.Add("err_" + part, distance, 3);
                frames.Add("aerr_" + part, angle, 1);
                errSum[(int)part] += distance;
                errMax[(int)part] = Mathf.Max(errMax[(int)part], distance);
                angSum[(int)part] += angle;
                angMax[(int)part] = Mathf.Max(angMax[(int)part], angle);
            }

            frames.End();
            samples++;

            if (writePose)
            {
                WritePose(t);
            }
        }

        private void WritePose(float t)
        {
            pose.Begin();
            pose.Add("t", t, 4);
            foreach (var part in PartInfo.All)
            {
                string n = part.ToString();
                var body = unit.Rig.Part(part).body;
                Vector3 p = body.position;
                Quaternion q = body.rotation;
                Vector3 g = unit.Anim.Part(part).position;
                pose.Add(n + ".x", p.x, 3);
                pose.Add(n + ".y", p.y, 3);
                pose.Add(n + ".z", p.z, 3);
                pose.Add(n + ".qx", q.x, 3);
                pose.Add(n + ".qy", q.y, 3);
                pose.Add(n + ".qz", q.z, 3);
                pose.Add(n + ".qw", q.w, 3);
                pose.Add(n + ".gx", g.x, 3);
                pose.Add(n + ".gy", g.y, 3);
                pose.Add(n + ".gz", g.z, 3);
            }

            for (int f = 0; f < 2; f++)
            {
                Vector3 planted = unit.Stance.Position(f);
                string side = f == Stance.Left ? "L" : "R";
                pose.Add("plant" + side + ".x", planted.x, 3);
                pose.Add("plant" + side + ".z", planted.z, 3);
            }

            pose.End();
        }

        /// <summary>One line for a mark: what this unit is doing right now.</summary>
        public string Snapshot()
        {
            if (unit == null || !unit.IsInitialized)
            {
                return "gone";
            }

            var p = unit.Pose;
            unit.Authority.Error(PartId.Chest, out float chest, out _);
            unit.Authority.Error(PartId.HandR, out float hand, out _);
            return $"facing {unit.FacingYaw:0}°, hips {unit.Rig.Part(PartId.Pelvis).body.position.y - unit.Root.position.y:0.00} m, "
                   + $"weight {Vector3.ProjectOnPlane(p.CenterOfMass - p.SupportPoint, Vector3.up).magnitude * 100f:0} cm off the support point, "
                   + $"{(unit.Stance.AnyStepping ? "stepping" : "planted")}, chest {chest * 100f:0} cm and right hand {hand * 100f:0} cm from the pose";
        }

        public void AppendSummary(StringBuilder md)
        {
            md.AppendLine();
            md.AppendLine($"## {label}");
            md.AppendLine();
            float until = endTime >= 0f ? endTime : PlaytestRecorder.Now;
            md.AppendLine($"Present {spawnTime:0.0}–{until:0.0} s. Steps {steps}, idle shuffles {shuffles}. "
                          + $"Feet slid at most {maxSlide * 100f:0.0} cm while planted; the weight was at most "
                          + $"{maxComOffset * 100f:0} cm from the support point; hips dropped for reach by up to {maxPelvisDrop * 100f:0} cm.");
            md.AppendLine();
            md.AppendLine("Physical parts against the pose (mean / max): distance in cm, angle in degrees.");
            md.AppendLine();
            md.AppendLine("| Part | Authority | Distance | Angle |");
            md.AppendLine("| --- | --- | --- | --- |");
            int n = Mathf.Max(1, samples);
            foreach (var part in PartInfo.All)
            {
                string level = unit != null && unit.Authority != null ? unit.Authority.Level(AuthorityController.RegionOf(part)).ToString() : "?";
                md.AppendLine($"| {part} | {level} | {errSum[(int)part] / n * 100f:0.0} / {errMax[(int)part] * 100f:0.0} | "
                              + $"{angSum[(int)part] / n:0} / {angMax[(int)part]:0} |");
            }

            md.AppendLine();
        }
    }
}
