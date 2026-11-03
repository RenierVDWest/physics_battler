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

            Vector3 support = p.WeightTarget;   // (the rhythm's deliberate shifts are meant)
            Vector3 com = p.CenterOfMass - support;
            com.y = 0f;
            Vector3 phys = unit.Rig.CenterOfMass() - support;
            phys.y = 0f;
            frames.Add("comOffset", com.magnitude, 3);
            frames.Add("physCom", phys.magnitude, 3);
            // The stance figures only count while it stands (stumbling, down and getting up are meant to break them).
            bool upright = unit.IsAlive && unit.State == UnitState.Standing;
            if (upright)
            {
                maxComOffset = Mathf.Max(maxComOffset, com.magnitude);
                maxPelvisDrop = Mathf.Max(maxPelvisDrop, p.PelvisDrop);
                uprightSamples++;
            }

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
                if (upright)
                {
                    maxSlide = Mathf.Max(maxSlide, slide);
                }
            }

            footWatch?.Sample(frames, Time.fixedDeltaTime);
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
                // Milestone 10: the arms' joints against their poses' (joint space; the arm rebuild's step 6).
                if (AuthorityController.RegionOf(part) == BodyRegion.ArmL || AuthorityController.RegionOf(part) == BodyRegion.ArmR)
                {
                    frames.Add("jerr_" + part, unit.Authority.JointError(part), 1);
                }
                if (upright)
                {
                    errSum[(int)part] += distance;
                    errMax[(int)part] = Mathf.Max(errMax[(int)part], distance);
                    angSum[(int)part] += angle;
                    angMax[(int)part] = Mathf.Max(angMax[(int)part], angle);
                }
            }

            var move = unit.CurrentMove;
            var weapon = unit.MainWeapon;
            frames.Add("move", move != null ? move.Move.name.Replace("MD_", "") : "");
            frames.Add("moveT", move != null ? move.T : 0f, 3);
            frames.Add("blend", move != null ? move.Blend : 0f, 2);
            frames.Add("aimPart", move != null ? move.Part.ToString() : "");
            Vector3 aim = move != null ? move.Aim : Vector3.zero;
            Vector3 strike = move != null ? p.StrikePoint : Vector3.zero;
            Vector3 physical = move != null && weapon != null && move.Weapon != null ? weapon.PhysicalPoint(move.Weapon.StrikePoint(move.Move.strikeWith)) : Vector3.zero;
            frames.Add("aimX", aim.x, 3);
            frames.Add("aimY", aim.y, 3);
            frames.Add("aimZ", aim.z, 3);
            frames.Add("strikeX", strike.x, 3);
            frames.Add("strikeY", strike.y, 3);
            frames.Add("strikeZ", strike.z, 3);
            frames.Add("pstrikeX", physical.x, 3);
            frames.Add("pstrikeY", physical.y, 3);
            frames.Add("pstrikeZ", physical.z, 3);
            frames.Add("aimErr", move != null ? p.AimError : 0f, 3);
            frames.Add("health", unit.Health, 1);
            frames.Add("poise", unit.Poise, 1);
            frames.Add("alive", unit.IsAlive);
            frames.Add("knock", unit.Knockback.magnitude, 3);
            frames.Add("state", unit.IsAlive ? unit.State.ToString() : "Dead");
            frames.Add("stumble", unit.StumbleVelocity.magnitude, 3);
            var rhythm = unit.Rhythm;
            frames.Add("posture", rhythm?.Current != null ? rhythm.Current.Label : "");
            frames.Add("energy", rhythm != null ? rhythm.Energy : 1f, 3);
            // The walk: how much it plays, how fast it is being walked, round the cycle.
            frames.Add("walk", unit.Gait != null ? unit.Gait.Weight : 0f, 2);
            frames.Add("walkSpeed", unit.Gait != null ? unit.Gait.Velocity.magnitude : 0f, 3);
            frames.Add("walkPhase", unit.Gait != null ? unit.Gait.Phase : 0f, 3);
            frames.Add("aggression", rhythm != null ? rhythm.Aggression : 0f, 3);
            frames.Add("open", unit.Openings == Combat.AimParts.None ? "" : unit.Openings.ToString().Replace(", ", " "));
            frames.Add("hitStop", unit.HitStop > 0f);
            frames.Add("block", unit.BlockBlend, 2);
            // Milestone 8: the tempo (gathering, probing, pressing, shelled up, countering) and the pressure on it.
            var tempo = unit.TempoOn ? unit.Tempo : null;
            frames.Add("tempo", tempo != null ? tempo.State.ToString() : "");
            frames.Add("pressure", tempo != null ? tempo.Pressure : 0f, 2);
            // Milestone 11: the shell's guard, a parry, a broken guard.
            frames.Add("shellGuard", unit.ShellWeight, 2);
            frames.Add("parry", unit.ParryBlend, 2);
            frames.Add("guardOpen", unit.LocalTime < unit.GuardOpenUntil);
            // Milestone 9: what the head attends to, how far it is turned against the chest (and up),
            // how far from what it attends to it points; how many units it knows of; the shield's
            // state, how far its face is from its threat, and how far its rim stays from its own body.
            var look = unit.Look;
            float lookOff = unit.LookOff;
            frames.Add("look", look.Current >= 0 ? look.Kind : "");
            frames.Add("headTurn", p.HeadTurn, 1);
            frames.Add("headPitch", p.HeadPitch, 1);
            frames.Add("lookOff", lookOff, 1);
            frames.Add("known", unit.Known.Count);
            bool hasShield = unit.Loadout != null && unit.Loadout.HasShield;
            frames.Add("shield", hasShield ? unit.ShieldMode.ToString() : "");
            frames.Add("shieldOff", hasShield ? unit.ShieldOffThreat : 0f, 1);
            frames.Add("shieldClear", hasShield && p.ShieldClearance < 10f ? p.ShieldClearance : 0f, 3);
            // Milestone 9b: the joints against their limits (the physical body, and the pose), and a body
            // on the ground: its fastest part and how close the nearest standing unit's legs are.
            float jointPast = unit.WorstJoint(true, out var joint);
            float posePast = unit.WorstJoint(false, out var poseJoint);
            bool lying = unit.Lying;
            float fastest = lying ? unit.FastestPart() : 0f;
            float nearLeg = lying ? unit.NearestStandingLeg() : 99f;
            frames.Add("jointPast", jointPast, 1);
            frames.Add("joint", jointPast > 0f ? joint.ToString() : "");
            frames.Add("posePast", posePast, 1);
            frames.Add("poseJoint", posePast > 0f ? poseJoint.ToString() : "");
            frames.Add("lyingSpeed", fastest, 2);
            frames.Add("nearLeg", lying ? Mathf.Min(nearLeg, 9f) : 0f, 2);
            if (lying)
            {
                lyingSamples++;
                lyingPast += jointPast > 10f ? 1 : 0;
                if (jointPast > lyingWorst)
                {
                    lyingWorst = jointPast;
                    lyingWorstJoint = joint.ToString();
                }

                (nearLeg < 0.5f ? speedsNear : speedsAlone).Add(fastest);
            }

            if (upright)
            {
                posePastSamples += posePast > 10f ? 1 : 0;
                if (posePast > poseWorst)
                {
                    poseWorst = posePast;
                    poseWorstJoint = poseJoint.ToString();
                }
            }

            if (upright)
            {
                lookSamples++;
                lookOnPoint += look.Current >= 0 && lookOff <= 25f ? 1 : 0;
                if (look.Reachable)
                {
                    lookReachable++;
                    lookReachableOn += look.Current >= 0 && lookOff <= 25f ? 1 : 0;
                }
                headOutside += Mathf.Abs(p.HeadTurn) > 80.5f || p.HeadPitch < -60.5f || p.HeadPitch > 50.5f ? 1 : 0;
                if (hasShield)
                {
                    shieldSamples++;
                    shieldFaced += unit.ShieldOffThreat <= 45f ? 1 : 0;
                    shieldWorstClear = Mathf.Min(shieldWorstClear, p.ShieldClearance);
                }
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
                Vector3 planted = unit.Stance.Position(f);
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
