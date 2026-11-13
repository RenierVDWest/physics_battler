using System.Collections.Generic;
using System.Text;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
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
    /// of mass from where the weight is meant to be (the support point, moved by the Fight Rhythm's
    /// deliberate shifts), physCom: the physical one, both horizontal metres); the walk (walk: how
    /// much the captured walk plays, walkSpeed, walkPhase); feet
    /// (stepL/R 0 or 1, liftL/R, slideL/R: how far each physical foot is from its planted spot,
    /// steps, shuffles); idle (swayX, swayZ, bob); authority per region (lvl_*: 0 kinematic,
    /// 1 pinned, 2 loose, 3 ragdoll, 4 muscled; pin_*); every part's distance (err_*, metres) and angle
    /// (aerr_*, degrees) from its pose; and the blow under way (move: its name or empty, moveT 0 to 1,
    /// blend, part aimed at, the locked aim point aimX/Y/Z, the striking part on the pose strikeX/Y/Z
    /// and on the physical weapon pstrikeX/Y/Z, aimErr: posed striking part to aim point).
    ///
    /// Pose columns: per part x/y/z/qx/qy/qz/qw (physical) and gx/gy/gz (animated); the planted
    /// foot spots plantL/R.x/z; the main weapon's grip and tip, physical (grip.*, tip.*) and posed
    /// (ghostGrip.*, ghostTip.*); and the locked aim point (target.*) during a blow.
    /// </summary>
    internal sealed class UnitTrack
    {
        /// <summary>The unit's description in meta.json, kept from while it lived.</summary>
        public string metaFields;

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
        public int samples, uprightSamples;
        public float maxSlide;

        /// <summary>The legs and feet watched: plants, sliding, twisting, floating, walking.</summary>
        private readonly FootWatch footWatch;
        public float maxComOffset;
        public float maxPelvisDrop;
        public int steps;
        public int shuffles;

        // Blows, for the summary.
        public readonly List<string> blowLines = new List<string>();
        public int blows, blowsReached, blowsStruckAimed;
        public float aimErrSum, aimErrMax, physErrSum, physErrMax;

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
                footWatch = new FootWatch(unit, text => recorder.Event(this, "foot", text));
            }
        }

        public bool Alive => unit != null;
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
            // (A clip fighter (the plan "Bringing the lab into the fight", step 4): its walk, not the old pose.)
            var loco = unit.Clip.Loco;
            frames.Add("clipMode", (int)loco.Current);
            frames.Add("clipSpeed", new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude, 3);
            frames.Add("clipWant", new Vector2(unit.ClipWant.x, unit.ClipWant.z).magnitude, 3);
            frames.Add("heldL", loco.FootHeld(0) ? 1 : 0);
            frames.Add("heldR", loco.FootHeld(1) ? 1 : 0);
            frames.Add("concentration", unit.Concentration, 2);
            // (The legs' opinion, the plan "Grounded legs", step 0: what each foot is doing (0 planted, 1 sliding, 2 landing,
            // 3 low in the air, 4 stepping), the body's weight and where a foot is needed, and how far each is outside the
            // support (cm; -1: both feet up).)
            var legs = unit.Clip.Ground;
            frames.Add("footL", (int)legs.Feet[0].State);
            frames.Add("footR", (int)legs.Feet[1].State);
            frames.Add("weightX", legs.Weight.x, 3);
            frames.Add("weightZ", legs.Weight.z, 3);
            frames.Add("weightH", legs.Weight.y - legs.GroundY, 3);
            frames.Add("neededX", legs.Needed.x, 3);
            frames.Add("neededZ", legs.Needed.z, 3);
            frames.Add("weightOut", float.IsNaN(legs.WeightOut) ? -1f : 100f * legs.WeightOut, 1);
            frames.Add("neededOut", float.IsNaN(legs.NeededOut) ? -1f : 100f * legs.NeededOut, 1);
            // (Stand and step, the plan's step 1: 0 standing still, 1 stepping in its stance, 2 a step walked with the
            // guard's walk, 3 walking as fast as it is asked, 4 walking off a shove, 5 in a clip, 6 down; and the ground the
            // steering has asked for and it has not yet stepped, cm.)
            frames.Add("stepping", unit.Clip.StepState);
            frames.Add("owed", 100f * unit.Clip.Pacer.Owed.magnitude, 1);
            // (The weight over the feet, the plan's step 3: how far the hips are shifted toward the carrying foot, cm.)
            frames.Add("hipShift", 100f * unit.Clip.Poser.HipShift.magnitude, 1);
            // (Giving ground to a blow now: its root moved back, its feet to step after it.)
            frames.Add("givingGround", unit.Clip.GivingGround ? 1f : 0f, 0);
            frames.End();
            samples++;
            if (writePose && unit.Authority != null)
            {
                WritePose(t);
            }

            return;
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
                if (part == PartId.ThighL || part == PartId.ThighR || part == PartId.ShinL || part == PartId.ShinR || part == PartId.FootL || part == PartId.FootR)
                {
                    // (The legs: the posed rotation too, to see which way they point.)
                    Quaternion gq = unit.Anim.Part(part).rotation;
                    pose.Add(n + ".gqx", gq.x, 3);
                    pose.Add(n + ".gqy", gq.y, 3);
                    pose.Add(n + ".gqz", gq.z, 3);
                    pose.Add(n + ".gqw", gq.w, 3);
                }
            }

            for (int f = 0; f < 2; f++)
            {
                Vector3 planted = unit.Clip.Poser.Feet[f].Target;
                string side = f == Stance.Left ? "L" : "R";
                pose.Add("plant" + side + ".x", planted.x, 3);
                pose.Add("plant" + side + ".z", planted.z, 3);
            }

            // The main weapon: physical grip and tip, and where the pose has them.
            var weapon = unit.MainWeapon;
            var slot = unit.Anim.SlotWeaponR;
            bool armed = weapon != null && weapon.definition != null && slot != null;
            Vector3 grip = armed ? weapon.PhysicalPoint(Vector3.zero) : Vector3.zero;
            Vector3 tip = armed ? weapon.PhysicalPoint(weapon.definition.tip) : Vector3.zero;
            Vector3 ghostGrip = armed ? slot.position : Vector3.zero;
            Vector3 ghostTip = armed ? slot.position + slot.rotation * (weapon.definition.tip * weapon.Scale) : Vector3.zero;
            AddPoint("grip", grip, armed);
            AddPoint("tip", tip, armed);
            AddPoint("ghostGrip", ghostGrip, armed);
            AddPoint("ghostTip", ghostTip, armed);
            var move = unit.CurrentMove;
            AddPoint("target", move != null ? move.Aim : Vector3.zero, move != null);
            pose.End();
        }

        private void AddPoint(string name, Vector3 p, bool present)
        {
            pose.Add(name + ".x", present ? p.x.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) : "");
            pose.Add(name + ".y", present ? p.y.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) : "");
            pose.Add(name + ".z", present ? p.z.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) : "");
        }

        /// <summary>A blow reached its contact moment: what it did, for the summary.</summary>
        public void NoteContact(MoveState move, ContactReport report, string line)
        {
            blows++;
            aimErrSum += report.AnimError;
            aimErrMax = Mathf.Max(aimErrMax, report.AnimError);

            if (report.AnimError <= MovePlanner.Tolerance)
            {
                blowsReached++;
            }

            if (report.StruckUnit != null && ReferenceEquals(report.StruckUnit, move.Target)
                && report.StruckPart == AimPartInfo.BodyPartOf(move.Part) && report.StruckDistance < 0.05f)
            {
                blowsStruckAimed++;
            }

            if (blowLines.Count < 60)
            {
                blowLines.Add(line);
            }
        }

        public int physicalBlows;
        public int hitsDealt, hitsTaken, blocked, strayHits, allyHits, aimedHits;
        public float damageDealt, damageTaken;

        public void NoteHitDealt(HitReport hit)
        {
            if (hit.Impact)
            {
                impactsDealt++;
                return;
            }

            hitsDealt++;
            damageDealt += hit.Result.Damage;
            if (hit.Shield) blocked++;
            else if (hit.Parried) parried++;
            else if (hit.Kind == "the part aimed at") aimedHits++;
            else if (hit.Kind.StartsWith("an ally")) allyHits++;
            else if (hit.Kind.StartsWith("someone")) strayHits++;
        }

        public int stumbles, knockdowns, launches, getUps, impactsDealt;
        public int postures, leadIns;
        private readonly System.Collections.Generic.Dictionary<string, int> postureCounts = new System.Collections.Generic.Dictionary<string, int>();

        public void NotePosture(Motion.PostureDefinition posture, bool leadIn)
        {
            postures++;
            leadIns += leadIn ? 1 : 0;
            postureCounts.TryGetValue(posture.Label, out int n);
            postureCounts[posture.Label] = n + 1;
        }

        public int incoming, shieldsRaised, defenceless, seenClearly, seenAtEdge, neverSeen;

        // Milestone 11: parries (its own blows parried; its weapon set across blows).
        public int parried, parriesTried;

        // Milestone 9: the look and the shield while standing.
        // Milestone 9b: joints against their limits, and bodies on the ground.
        public int lyingSamples, lyingPast, posePastSamples;
        public float lyingWorst, poseWorst;
        public string lyingWorstJoint = "", poseWorstJoint = "";
        public readonly List<float> speedsNear = new List<float>(), speedsAlone = new List<float>();

        public int lookSamples, lookOnPoint, lookReachable, lookReachableOn, headOutside, shieldSamples, shieldFaced;
        public float shieldWorstClear = float.MaxValue;

        /// <summary>Milestone 7: a blow came at it, and what it did about it (milestone 9: whether and how it saw it).</summary>
        public void NoteDefence(string what)
        {
            if (what == "never saw it: struck")
            {
                return;   // (counted when the blow ends: "never saw it")
            }

            incoming++;
            shieldsRaised += what.StartsWith("raises the shield") ? 1 : 0;
            parriesTried += what.StartsWith("parries") ? 1 : 0;
            defenceless += what.StartsWith("nothing to defend with") ? 1 : 0;
            seenClearly += what.Contains("(seen clearly") ? 1 : 0;
            seenAtEdge += what.Contains("(seen at the edge") ? 1 : 0;
            neverSeen += what.StartsWith("never saw it") ? 1 : 0;
        }

        public int severs;

        /// <summary>Milestone 5: a limb came off it.</summary>
        public void NoteSevered()
        {
            severs++;
        }

        /// <summary>A posture already under way became the lead-in to a blow.</summary>
        public void NoteLedIn()
        {
            leadIns++;
        }

        /// <summary>Something happened to the unit's balance (the log line's start says what).</summary>
        public void NoteBalance(string text)
        {
            if (text.StartsWith("stumble")) stumbles++;
            else if (text.StartsWith("knocked down")) knockdowns++;
            else if (text.StartsWith("launched")) launches++;
            else if (text.StartsWith("got up")) getUps++;
        }

        public void NoteHitTaken(HitReport hit)
        {
            hitsTaken++;
            damageTaken += hit.Result.Damage;
        }

        /// <summary>A blow ended: how close the physical weapon came.</summary>
        public void NotePhysical(float closest)
        {
            if (closest < 0f)
            {
                return;
            }

            physicalBlows++;
            physErrSum += closest;
            physErrMax = Mathf.Max(physErrMax, closest);
        }

        /// <summary>One line for a mark: what this unit is doing right now.</summary>
        public string Snapshot()
        {
            if (unit == null || !unit.IsInitialized)
            {
                return "gone";
            }

            var loco = unit.Clip.Loco;
            return $"facing {unit.FacingYaw:0}°, {loco.Current}{(loco.Shot != null && loco.Current == PhysicsBattler.Locomotion.Locomotor.Mode.OneShot ? " " + loco.Shot.Name : "")}, "
                   + $"walking {new Vector2(loco.Velocity.x, loco.Velocity.z).magnitude:0.00} m/s (asked {new Vector2(unit.ClipWant.x, unit.ClipWant.z).magnitude:0.00}), feet held {(loco.FootHeld(0) ? "L" : "-")}{(loco.FootHeld(1) ? "R" : "-")}, concentration {unit.Concentration:0.00}";
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
            footWatch?.Write(md);
            md.AppendLine("Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.");
            md.AppendLine();
            md.AppendLine("| Part | Authority | Distance | Angle |");
            md.AppendLine("| --- | --- | --- | --- |");
            int n = Mathf.Max(1, uprightSamples);
            foreach (var part in PartInfo.All)
            {
                string level = unit != null && unit.Authority != null ? unit.Authority.Level(AuthorityController.RegionOf(part)).ToString() : "?";
                md.AppendLine($"| {part} | {level} | {errSum[(int)part] / n * 100f:0.0} / {errMax[(int)part] * 100f:0.0} | "
                              + $"{angSum[(int)part] / n:0} / {angMax[(int)part]:0} |");
            }

            md.AppendLine();
            if (hitsDealt > 0 || hitsTaken > 0)
            {
                md.AppendLine($"Hits dealt: {hitsDealt} ({aimedHits} on the part aimed at, {blocked} blocked, {parried} parried, {strayHits} on someone in the way, "
                              + $"{allyHits} on allies), {damageDealt:0} damage. Hits taken: {hitsTaken}, {damageTaken:0} damage. "
                              + $"{(unit != null && !unit.IsAlive ? "Killed." : $"Health at the end {(unit != null ? unit.Health : 0f):0}.")}");
                md.AppendLine();
            }

            if (incoming > 0)
            {
                md.AppendLine($"Defence: {incoming} blows came at it ({seenClearly} seen clearly, {seenAtEdge} at the edge of its view, {neverSeen} never seen); "
                              + $"it raised the shield against {shieldsRaised}, set its weapon across {parriesTried}"
                              + (defenceless > 0 ? $", and had nothing to defend with against {defenceless}." : "."));
                md.AppendLine();
            }

            if (uprightSamples > 0)
            {
                md.AppendLine($"Pose against the joint limits (standing): a joint more than 10° past its limit {100f * posePastSamples / uprightSamples:0}% of the time; "
                              + $"the worst {poseWorst:0}° ({(poseWorstJoint == "" ? "none" : poseWorstJoint)}).");
            }

            if (lyingSamples > 0)
            {
                md.AppendLine($"Ragdoll (on the ground {lyingSamples / 60f:0.0} s): a joint more than 10° past its limit {100f * lyingPast / lyingSamples:0}% of the time; "
                              + $"the worst {lyingWorst:0}° ({(lyingWorstJoint == "" ? "none" : lyingWorstJoint)}). Fastest part with a standing leg within 0.5 m: "
                              + $"{Percentile(speedsNear, 0.5f)} median, {Percentile(speedsNear, 0.95f)} at the 95th percentile ({speedsNear.Count} steps); "
                              + $"with none: {Percentile(speedsAlone, 0.5f)} median ({speedsAlone.Count} steps).");
            }

            if (uprightSamples > 0 || lyingSamples > 0)
            {
                md.AppendLine();
            }

            if (lookSamples > 0)
            {
                md.AppendLine($"Look: within 25° of what it attended to {100f * lookOnPoint / lookSamples:0}% of the time standing "
                              + $"({100f * lookReachableOn / Mathf.Max(1, lookReachable):0}% of the time it was within the head's reach, {100f * lookReachable / lookSamples:0}% of the time); "
                              + $"head outside its limits {headOutside} steps.");
                if (shieldSamples > 0)
                {
                    md.AppendLine($"Shield: faced within 45° of its threat {100f * shieldFaced / shieldSamples:0}% of the time standing; "
                                  + $"its rim came within {shieldWorstClear * 100f:0.0} cm of its own body (negative: inside).");
                }

                md.AppendLine();
            }

            if (postures > 0)
            {
                var counts = new System.Collections.Generic.List<string>();
                foreach (var pair in postureCounts)
                {
                    counts.Add($"{pair.Key} {pair.Value}");
                }

                md.AppendLine($"Fight Rhythm ({(unit != null && unit.Rhythm != null ? unit.Rhythm.Definition.Label : "?")}): {postures} postures ({string.Join(", ", counts)}), "
                              + $"{leadIns} of them lead-ins to blows; energy at the end {(unit != null && unit.Rhythm != null ? unit.Rhythm.Energy : 0f):0.00}.");
                md.AppendLine();
            }

            if (stumbles + knockdowns + launches + getUps + impactsDealt > 0)
            {
                bool stillDown = unit != null && unit.IsAlive && unit.IsDown;
                md.AppendLine($"Balance: stumbled {stumbles} time{(stumbles == 1 ? "" : "s")}, knocked down {knockdowns}, launched {launches}, "
                              + $"got up {getUps}; thrown into others {impactsDealt} time{(impactsDealt == 1 ? "" : "s")}."
                              + (stillDown ? " Still down at the end." : ""));
                md.AppendLine();
            }

            if (blows > 0)
            {
                md.AppendLine($"Blows: {blows}. The posed striking part passed within 3 cm of the aim point in {blowsReached}; "
                              + $"aim error mean {aimErrSum / blows * 100f:0.0} cm, max {aimErrMax * 100f:0.0} cm (posed); "
                              + $"physical weapon's closest pass mean {physErrSum / Mathf.Max(1, physicalBlows) * 100f:0.0} cm, max {physErrMax * 100f:0.0} cm. "
                              + $"{blowsStruckAimed} reached the part they were aimed at (nearest part within 5 cm at contact).");
                md.AppendLine();
                foreach (var line in blowLines)
                {
                    md.AppendLine("- " + line);
                }

                md.AppendLine();
            }
        }

        private static string Percentile(List<float> values, float p)
        {
            if (values.Count == 0)
            {
                return "-";
            }

            var sorted = new List<float>(values);
            sorted.Sort();
            return $"{sorted[Mathf.Clamp(Mathf.RoundToInt(p * (sorted.Count - 1)), 0, sorted.Count - 1)]:0.0} m/s";
        }
    }
}
