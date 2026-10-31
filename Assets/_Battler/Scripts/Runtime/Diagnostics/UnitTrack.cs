using System.Collections.Generic;
using System.Text;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Diagnostics
{
    /// <summary>
    /// Follows one unit for the <see cref="PlaytestRecorder"/>: writes its frame table (one row of
    /// scalars per physics step) and pose table (every part, physical and ghost), turns what it sees
    /// into events (state changes, swing phases, contacts, get-up progress), and keeps the episodes
    /// (each swing, each fall) and the joint tracking statistics for the session summary.
    /// </summary>
    internal sealed class UnitTrack
    {
        // ---- Episodes, for the summary ----------------------------------------------------------

        public sealed class AttackEpisode
        {
            public int number;
            public float start;
            public float end = -1f;
            public string victim;
            public float planDistance;
            public float contactReach;
            public float roll;
            public float entryTheta;
            public string furthestPhase = "WindUp";
            public bool committed;
            public float maxTipSpeed;
            public float maxStrikeLag;
            public float minClubToTarget = float.MaxValue;
            public float thetaAtClosest;
            public float clubThetaAtClosest;
            public string phaseAtClosest = "";
            public float maxTrackingErrorArm;
            public readonly List<string> contacts = new List<string>();
            public string hit;
            public readonly List<string> rejected = new List<string>();
            public string abandoned;
            public float lastGhostTheta;
            public float lastClubTheta;

            public string Outcome
            {
                get
                {
                    if (hit != null) return "HIT " + hit;
                    if (abandoned != null) return "abandoned: " + abandoned;
                    if (rejected.Count > 0) return "touched but no hit: " + rejected[0];
                    if (end < 0f) return "unfinished";
                    return minClubToTarget < float.MaxValue
                        ? $"missed: club passed {minClubToTarget:0.00} m from the target point (in {phaseAtClosest})"
                        : "missed";
                }
            }
        }

        public sealed class FallEpisode
        {
            public int number;
            public float start;
            public string reason;
            public string firstGroundParts = "";
            public float landedAt = -1f;
            public float getUpStart = -1f;
            public string lying = "";
            public float getUpMaxPelvis;
            public float getUpEndPelvis = -1f;
            public float getUpEndTilt = -1f;
            public float standStart = -1f;
            public string result;   // null until known
        }

        // ---- State -----------------------------------------------------------------------------

        private readonly PlaytestRecorder recorder;
        private readonly TableWriter frames;
        private readonly TableWriter pose;
        private readonly float[] errSum = new float[PartInfo.Count * Contexts];
        private readonly float[] errMax = new float[PartInfo.Count * Contexts];
        private readonly int[] errCount = new int[Contexts];
        private readonly float[] stateTime = new float[6];
        private readonly float[] lastGroundTouch = new float[PartInfo.Count];

        public readonly UnitController unit;
        public readonly string label;
        public readonly string stem;
        public readonly int id;
        public readonly List<AttackEpisode> attacks = new List<AttackEpisode>();
        public readonly List<FallEpisode> falls = new List<FallEpisode>();
        public int hitsTaken;
        public float damageTaken;
        public float spawnTime;
        public float endTime = -1f;
        public int catchSteps;
        public int settleSteps;

        private AttackEpisode attack;
        private FallEpisode fall;
        private bool subscribed;
        private Vector3 tipInSlot;
        private bool hasTip;
        private int frameCount;
        private int getUpCheckpoint;
        private Weapon notedWeapon;

        public const int Contexts = 7;
        public static readonly string[] ContextNames = { "Idle", "Attacking", "Flinch", "Stagger", "Knockdown", "GettingUp", "Dead" };

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
            for (int i = 0; i < lastGroundTouch.Length; i++)
            {
                lastGroundTouch[i] = -10f;
            }

            Subscribe();
        }

        public bool Alive => unit != null;
        public int FrameRows => frames.Rows;

        private void Subscribe()
        {
            if (subscribed || unit == null || !unit.IsInitialized)
            {
                return;
            }

            unit.State.Changed += OnStateChanged;
            unit.Planner.PhaseChanged += OnPhaseChanged;
            unit.Stance.Landed += OnStepLanded;
            unit.ContactEntered += OnContact;
            subscribed = true;
        }

        public void Close(string why)
        {
            if (subscribed && unit != null)
            {
                unit.State.Changed -= OnStateChanged;
                unit.Planner.PhaseChanged -= OnPhaseChanged;
                unit.Stance.Landed -= OnStepLanded;
                unit.ContactEntered -= OnContact;
            }

            subscribed = false;
            endTime = PlaytestRecorder.Now;
            if (attack != null)
            {
                FinishAttack(why);
            }

            frames.Dispose();
            pose.Dispose();
        }

        public void Flush()
        {
            frames.Flush();
            pose.Flush();
        }

        private void Log(string category, string text) => recorder.Event(this, category, text);

        // ---- Events ----------------------------------------------------------------------------

        private void OnStateChanged(UnitStateKind previous, UnitStateKind next)
        {
            var sense = unit.LastSense;
            Log("state", $"{previous} → {next}: {unit.State.LastReason}  [stability {unit.State.Stability:0}, health {unit.State.Health:0}, "
                         + $"chest tilt {sense.tiltDegrees:0}°, pelvis {sense.pelvisHeight:0.00}/{unit.RestPelvisHeight:0.00} m, "
                         + $"balance error {unit.Balance.BalanceError:0.00} m{(unit.Grounded ? "" : ", feet off the ground")}]");

            if (next == UnitStateKind.Knockdown)
            {
                if (fall != null && fall.result == null)
                {
                    fall.result = previous == UnitStateKind.GettingUp
                        ? "FAILED: knocked down again while getting up"
                        : $"FAILED: fell again {PlaytestRecorder.Now - Mathf.Max(fall.standStart, fall.start):0.0} s after standing ({unit.State.LastReason})";
                    Log("getup", $"fall #{fall.number} result: {fall.result}");
                }

                fall = new FallEpisode { number = falls.Count + 1, start = PlaytestRecorder.Now, reason = unit.State.LastReason };
                falls.Add(fall);
            }
            else if (next == UnitStateKind.GettingUp && fall != null)
            {
                fall.getUpStart = PlaytestRecorder.Now;
                fall.getUpMaxPelvis = 0f;
                getUpCheckpoint = 0;
            }
            else if (next == UnitStateKind.Steady && previous == UnitStateKind.GettingUp && fall != null)
            {
                fall.standStart = PlaytestRecorder.Now;
                fall.getUpEndPelvis = sense.pelvisHeight;
                fall.getUpEndTilt = sense.tiltDegrees;
            }
            else if (next == UnitStateKind.Dead && fall != null && fall.result == null)
            {
                fall.result = "died";
            }
        }

        private void OnPhaseChanged(AttackPlanner.Phase phase)
        {
            var planner = unit.Planner;
            if (phase == AttackPlanner.Phase.WindUp)
            {
                if (attack != null)
                {
                    FinishAttack("replaced by a new swing");
                }

                var plan = planner.Plan;
                attack = new AttackEpisode
                {
                    number = attacks.Count + 1,
                    start = PlaytestRecorder.Now,
                    victim = planner.Target != null ? recorder.LabelOf(planner.Target) : "?",
                    planDistance = plan.distance,
                    contactReach = plan.contactReach,
                    roll = plan.rollDegrees,
                    entryTheta = planner.EntryTheta,
                };
                attacks.Add(attack);
                if (planner.Definition != null)
                {
                    recorder.NoteConfig(planner.Definition);
                }

                return;
            }

            if (attack == null)
            {
                return;
            }

            Club(out _, out _, out float clubTheta, out float tipSpeed, out float toTarget);
            string where = $"ghost θ {planner.Theta:0}°, club θ {clubTheta:0}° (lag {Mathf.DeltaAngle(clubTheta, planner.Theta):0}°), "
                           + $"tip {tipSpeed:0.0} m/s, club {toTarget:0.00} m from the target point";
            if (phase == AttackPlanner.Phase.Idle)
            {
                if (planner.AbandonedTimer > 0f && attack.abandoned == null)
                {
                    attack.abandoned = $"in {attack.furthestPhase}";
                }

                FinishAttack(null);
                return;
            }

            attack.furthestPhase = phase.ToString();
            attack.committed |= planner.Committed;
            Log("attack", $"swing #{attack.number} → {phase}{(phase == AttackPlanner.Phase.Follow && planner.LandedHit ? " (hit landed)" : "")}  [{where}]");
        }

        private void FinishAttack(string why)
        {
            if (attack == null)
            {
                return;
            }

            attack.end = PlaytestRecorder.Now;
            if (why != null && attack.hit == null)
            {
                attack.abandoned = why;
            }

            Log("attack", $"swing #{attack.number} result: {attack.Outcome}; fastest tip {attack.maxTipSpeed:0.0} m/s, "
                          + $"worst strike lag {attack.maxStrikeLag:0}°, worst arm tracking error {attack.maxTrackingErrorArm:0}°"
                          + (attack.contacts.Count > 0 ? $"; club touched: {string.Join(", ", attack.contacts)}" : "; club touched nothing"));
            attack = null;
        }

        private void OnStepLanded(PartId foot, Vector3 where)
        {
            var body = unit.Rig.Part(foot).body.position;
            Vector3 miss = body - where;
            miss.y = 0f;
            Log("step", $"{foot} landed{(miss.magnitude > 0.08f * unit.Scale ? $", {miss.magnitude:0.00} m from where it was placed" : "")}; "
                        + $"balance error now {unit.Balance.BalanceError:0.00} m");
        }

        private void OnContact(BodyPart part, Collision collision)
        {
            var other = collision.collider;
            if (other == null)
            {
                return;
            }

            float speed = collision.relativeVelocity.magnitude;
            var weapon = unit.Weapon;
            bool weaponContact = false;
            if (weapon != null && part == weapon.Hand)
            {
                for (int i = 0; i < collision.contactCount && !weaponContact; i++)
                {
                    weaponContact = weapon.Owns(collision.GetContact(i).thisCollider);
                }
            }

            string what = recorder.Describe(other);
            if (weaponContact)
            {
                var planner = unit.Planner;
                string phase = planner.Active ? $"{planner.Current} θ {planner.Theta:0}°" : "idle";
                Log("club", $"club touched {what} at {speed:0.0} m/s ({phase})");
                if (attack != null && attack.contacts.Count < 8)
                {
                    attack.contacts.Add($"{what} {speed:0.0} m/s in {phase}");
                }

                return;
            }

            bool ground = other.gameObject.layer == LayerMask.NameToLayer(Layers.Ground);
            int index = (int)part.id;
            if (ground)
            {
                if (PartInfo.IsFoot(part.id) && unit.State.IsStanding)
                {
                    return;   // steps are logged on their own
                }

                bool fresh = PlaytestRecorder.Now - lastGroundTouch[index] > 0.5f;
                lastGroundTouch[index] = PlaytestRecorder.Now;
                if (fresh)
                {
                    Log("contact", $"{part.id} hit the ground at {speed:0.0} m/s ({unit.State.Kind})");
                    if (fall != null && fall.landedAt < 0f && !PartInfo.IsFoot(part.id))
                    {
                        fall.landedAt = PlaytestRecorder.Now;
                        fall.firstGroundParts = part.id.ToString();
                    }
                }

                return;
            }

            if (speed > 0.5f)
            {
                Log("contact", $"{part.id} touched {what} at {speed:0.0} m/s");
            }
        }

        public void OnHitLanded(in HitEvent hit)
        {
            if (attack != null && hit.attacker == unit && attack.hit == null)
            {
                attack.hit = $"{recorder.LabelOf(hit.victim)} {hit.part} at {hit.speed:0.0} m/s ({unit.Planner.Current})";
            }

            if (hit.victim == unit)
            {
                hitsTaken++;
                damageTaken += hit.damage;
            }
        }

        public void OnTrace(string category, string text)
        {
            if (category == "step")
            {
                if (text.StartsWith("catch")) catchSteps++;
                else if (text.StartsWith("settle")) settleSteps++;
            }
            else if (category == "attack" && attack != null && text.StartsWith("swing abandoned "))
            {
                attack.abandoned = text.Substring("swing abandoned ".Length);
            }
            else if (category == "hit" && attack != null && attack.rejected.Count < 4)
            {
                int colon = text.IndexOf(": no hit, ");
                attack.rejected.Add(colon >= 0 ? text.Substring(colon + 10) : text);
            }
        }

        // ---- Sampling --------------------------------------------------------------------------

        /// <summary>Runs before the units' own step: the body as PhysX left it, the ghost as last posed.</summary>
        public void Sample(int step, bool writePose)
        {
            if (unit == null || !unit.IsInitialized)
            {
                return;
            }

            Subscribe();
            if (unit.Weapon != notedWeapon)
            {
                notedWeapon = unit.Weapon;
                if (notedWeapon != null)
                {
                    recorder.NoteConfig(notedWeapon.Definition);
                    Log("equip", $"holding {notedWeapon.name.Replace("(Clone)", "")} ({notedWeapon.Definition.name}): physics mass {notedWeapon.Definition.physicsMassKg:0.0} kg, "
                                 + $"tip {notedWeapon.TipDistance:0.00} m from the grip");
                    hasTip = false;
                }
            }

            var rig = unit.Rig;
            var ghost = unit.Ghost;
            var state = unit.State;
            var balance = unit.Balance;
            var planner = unit.Planner;
            var stance = unit.Stance;
            var muscles = unit.Muscles;
            var root = unit.Root;
            var pelvis = rig.Part(PartId.Pelvis).body;
            var chest = rig.Part(PartId.Chest).body;
            float t = PlaytestRecorder.Now;
            float dt = Time.fixedDeltaTime;
            float ground = unit.GroundY;
            stateTime[(int)state.Kind] += dt;

            int context = planner.Active ? 1 : ContextIndex(state.Kind);
            float pelvisHeight = pelvis.position.y - ground;
            float tilt = Vector3.Angle(chest.rotation * Vector3.up, Vector3.up);

            frames.Begin();
            frames.Add("t", t, 4);
            frames.Add("step", step);
            frames.Add("state", state.Kind.ToString());
            frames.Add("stateTime", state.TimeInState, 3);
            frames.Add("stability", state.Stability, 1);
            frames.Add("health", state.Health, 1);
            frames.Add("assist", state.Balance, 3);
            frames.Add("tone", state.MuscleTone, 3);
            frames.Add("getUp", state.GetUpProgress, 3);
            frames.Add("grounded", unit.Grounded);
            int groundMask = 0, otherMask = 0;
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                if (t - part.LastGroundContactTime <= dt * 1.5f) groundMask |= 1 << (int)id;
                if (t - part.LastOtherContactTime <= dt * 1.5f) otherMask |= 1 << (int)id;
            }

            frames.Add("groundMask", groundMask);
            frames.Add("otherMask", otherMask);
            frames.Add("rootX", root.position.x, 3);
            frames.Add("rootZ", root.position.z, 3);
            frames.Add("rootYaw", Yaw(root.forward), 1);
            frames.Add("groundY", ground, 3);
            frames.Add("pelvisH", pelvisHeight, 3);
            frames.Add("ghostPelvisH", ghost.Part(PartId.Pelvis).position.y - ground, 3);
            frames.Add("pelvisSpeed", pelvis.linearVelocity.magnitude, 2);
            frames.Add("pelvisVy", pelvis.linearVelocity.y, 2);
            frames.Add("chestTilt", tilt, 1);
            frames.Add("pelvisTilt", Vector3.Angle(pelvis.rotation * Vector3.up, Vector3.up), 1);
            frames.Add("chestFrontUp", Vector3.Dot(chest.rotation * Vector3.forward, Vector3.up), 2);
            frames.Add("chestYaw", Yaw(Flat(chest.rotation * Vector3.forward, root.forward)), 1);
            frames.Add("comX", balance.CenterOfMass.x, 3);
            frames.Add("comY", balance.CenterOfMass.y - ground, 3);
            frames.Add("comZ", balance.CenterOfMass.z, 3);
            frames.Add("captureX", balance.CapturePoint.x, 3);
            frames.Add("captureZ", balance.CapturePoint.z, 3);
            frames.Add("supportX", balance.SupportCenter.x, 3);
            frames.Add("supportZ", balance.SupportCenter.z, 3);
            frames.Add("balanceError", balance.BalanceError, 3);
            frames.Add("caught", unit.CaughtBalance);
            frames.Add("stepping", stance.Stepping);
            frames.Add("stepFoot", stance.Stepping ? stance.SteppingFoot.ToString() : "");
            frames.Add("stepsTaken", stance.StepsTaken);
            frames.Add("forceVertical", balance.LastVerticalForce, 0);
            frames.Add("forceHorizontal", balance.LastHorizontalForce, 0);
            frames.Add("uprightPelvis", balance.LastUprightPelvis, 1);
            frames.Add("uprightChest", balance.LastUprightChest, 1);

            bool hasClub = Club(out Vector3 grip, out Vector3 tip, out float clubTheta, out float tipSpeed, out float toTarget);
            frames.Add("phase", planner.Current.ToString());
            frames.Add("phaseTime", planner.Active ? planner.PhaseTime : 0f, 3);
            frames.Add("committed", planner.Committed);
            if (planner.Active)
            {
                frames.Add("theta", planner.Theta, 1);
                frames.Add("clubTheta", clubTheta, 1);
                frames.Add("lag", Mathf.DeltaAngle(clubTheta, planner.Theta), 1);
            }
            else
            {
                frames.Blank("theta");
                frames.Blank("clubTheta");
                frames.Blank("lag");
            }

            frames.Add("tipSpeed", hasClub ? tipSpeed : float.NaN, 2);
            frames.Add("tipH", hasClub ? tip.y - ground : float.NaN, 3);
            frames.Add("gripError", hasClub && ghost.SlotWeaponR != null ? Vector3.Distance(grip, ghost.SlotWeaponR.position) : float.NaN, 3);
            frames.Add("clubToTarget", planner.Active ? toTarget : float.NaN, 3);
            var target = planner.Target;
            frames.Add("clubToVictim", hasClub && target != null ? SegmentDistance(grip, tip, target.AimPoint) : float.NaN, 3);

            float armError = 0f;
            foreach (var id in PartInfo.All)
            {
                float error = TrackingError(id);
                frames.Add("err_" + id, error, 1);
                int k = context * PartInfo.Count + (int)id;
                errSum[k] += error;
                errMax[k] = Mathf.Max(errMax[k], error);
                if (id == PartId.UpperArmR || id == PartId.ForearmR || id == PartId.HandR)
                {
                    armError = Mathf.Max(armError, error);
                }
            }

            errCount[context]++;
            foreach (var id in PartInfo.All)
            {
                frames.Add("str_" + id, muscles.Strength(id), 2);
            }

            frames.End();
            frameCount++;

            if (attack != null && planner.Active)
            {
                UpdateAttack(planner, clubTheta, tipSpeed, toTarget, armError);
            }

            if (fall != null)
            {
                UpdateFall(state, pelvisHeight, tilt, groundMask);
            }

            if (writePose)
            {
                WritePose(t, hasClub, grip, tip, planner);
            }
        }

        private void UpdateAttack(AttackPlanner planner, float clubTheta, float tipSpeed, float toTarget, float armError)
        {
            attack.lastGhostTheta = planner.Theta;
            attack.lastClubTheta = clubTheta;
            attack.maxTipSpeed = Mathf.Max(attack.maxTipSpeed, tipSpeed);
            attack.maxTrackingErrorArm = Mathf.Max(attack.maxTrackingErrorArm, armError);
            if (planner.Current == AttackPlanner.Phase.Strike)
            {
                attack.maxStrikeLag = Mathf.Max(attack.maxStrikeLag, Mathf.Abs(Mathf.DeltaAngle(clubTheta, planner.Theta)));
            }

            if (planner.Current >= AttackPlanner.Phase.Load && toTarget < attack.minClubToTarget)
            {
                attack.minClubToTarget = toTarget;
                attack.thetaAtClosest = planner.Theta;
                attack.clubThetaAtClosest = clubTheta;
                attack.phaseAtClosest = planner.Current.ToString();
            }
        }

        private void UpdateFall(UnitState state, float pelvisHeight, float tilt, int groundMask)
        {
            if (state.Kind == UnitStateKind.GettingUp)
            {
                fall.getUpMaxPelvis = Mathf.Max(fall.getUpMaxPelvis, pelvisHeight);
                if (fall.lying.Length == 0)
                {
                    fall.lying = UnitController.DescribeLying(unit.Rig.Part(PartId.Chest).body.rotation);
                }

                int checkpoint = Mathf.FloorToInt(state.GetUpProgress * 4f);   // 25 %, 50 %, 75 %
                if (checkpoint > getUpCheckpoint && checkpoint < 4)
                {
                    getUpCheckpoint = checkpoint;
                    Log("getup", $"{checkpoint * 25}%: pelvis {pelvisHeight:0.00} m (ghost wants {unit.Ghost.Part(PartId.Pelvis).position.y - unit.GroundY:0.00}), "
                                 + $"chest tilt {tilt:0}°, assist {state.Balance:0.00}, tone {state.MuscleTone:0.00}, "
                                 + $"lift {unit.Balance.LastVerticalForce:0} N (body weight {unit.Rig.TotalMass() * 9.81f:0} N), "
                                 + $"touching the ground: {MaskNames(groundMask)}");
                }
            }
            else if (fall.standStart >= 0f && fall.result == null && state.IsStanding && PlaytestRecorder.Now - fall.standStart > 2f)
            {
                fall.result = $"stood up (pelvis {fall.getUpEndPelvis:0.00} m, tilt {fall.getUpEndTilt:0}° when the get-up ended)";
                Log("getup", $"fall #{fall.number} result: {fall.result}");
            }
        }

        private void WritePose(float t, bool hasClub, Vector3 grip, Vector3 tip, AttackPlanner planner)
        {
            var rig = unit.Rig;
            var ghost = unit.Ghost;
            pose.Begin();
            pose.Add("t", t, 4);
            foreach (var id in PartInfo.All)
            {
                var body = rig.Part(id).body;
                Vector3 p = body.position;
                Quaternion q = body.rotation;
                Vector3 g = ghost.Part(id).position;
                string n = id.ToString();
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

            Vector3 ghostGrip = ghost.SlotWeaponR != null ? ghost.SlotWeaponR.position : Vector3.zero;
            Vector3 ghostTip = ghost.SlotWeaponR != null ? ghostGrip + ghost.SlotWeaponR.rotation * tipInSlot : Vector3.zero;
            AddPoint(pose, "grip", grip, hasClub);
            AddPoint(pose, "tip", tip, hasClub);
            AddPoint(pose, "ghostGrip", ghostGrip, hasClub && ghost.SlotWeaponR != null);
            AddPoint(pose, "ghostTip", ghostTip, hasClub && ghost.SlotWeaponR != null);
            AddPoint(pose, "target", planner.Active ? planner.Plan.target : Vector3.zero, planner.Active);
            AddPoint(pose, "shoulder", planner.Active ? planner.Plan.shoulder : Vector3.zero, planner.Active);
            AddPoint(pose, "capture", unit.Balance.CapturePoint, true);
            pose.End();
        }

        private static void AddPoint(TableWriter table, string name, Vector3 p, bool valid)
        {
            table.Add(name + ".x", valid ? p.x : float.NaN, 3);
            table.Add(name + ".y", valid ? p.y : float.NaN, 3);
            table.Add(name + ".z", valid ? p.z : float.NaN, 3);
        }

        /// <summary>The held weapon in the physics pose: grip and tip points, its angle in the current swing plane, tip speed.</summary>
        private bool Club(out Vector3 grip, out Vector3 tip, out float clubTheta, out float tipSpeed, out float toTarget)
        {
            grip = tip = Vector3.zero;
            clubTheta = tipSpeed = 0f;
            toTarget = float.NaN;
            var weapon = unit.Weapon;
            var slot = unit.Rig.SlotWeaponR;
            if (weapon == null || weapon.Hand == null || slot == null)
            {
                return false;
            }

            var hand = weapon.Hand;
            if (!hasTip)
            {
                Transform tipMarker = weapon.tip != null ? weapon.tip : weapon.sweetSpot;
                tipInSlot = tipMarker != null
                    ? Quaternion.Inverse(slot.rotation) * (tipMarker.position - slot.position)
                    : Vector3.forward * weapon.TipDistance;
                hasTip = true;
            }

            grip = hand.PhysicalPoint(slot.position);
            Quaternion slotRotation = hand.body.rotation * (Quaternion.Inverse(hand.transform.rotation) * slot.rotation);
            tip = grip + slotRotation * tipInSlot;
            tipSpeed = hand.body.GetPointVelocity(tip).magnitude;
            var planner = unit.Planner;
            if (planner.Active || planner.AbandonedTimer > 0f)
            {
                var plan = planner.Plan;
                clubTheta = SwingSolver.AngleOf(plan, tip - grip);
                toTarget = SegmentDistance(grip, tip, plan.target);
            }

            return true;
        }

        private float TrackingError(PartId id)
        {
            var rig = unit.Rig;
            var ghost = unit.Ghost.Part(id);
            var body = rig.Part(id).body;
            if (!PartInfo.HasParent(id))
            {
                return Quaternion.Angle(ghost.rotation, body.rotation);
            }

            var parent = rig.Part(PartInfo.Parent(id)).body;
            return Quaternion.Angle(ghost.localRotation, Quaternion.Inverse(parent.rotation) * body.rotation);
        }

        public static int ContextIndex(UnitStateKind kind)
        {
            switch (kind)
            {
                case UnitStateKind.Steady: return 0;
                case UnitStateKind.Flinch: return 2;
                case UnitStateKind.Stagger: return 3;
                case UnitStateKind.Knockdown: return 4;
                case UnitStateKind.GettingUp: return 5;
                default: return 6;
            }
        }

        public static string MaskNames(int mask)
        {
            if (mask == 0)
            {
                return "nothing";
            }

            var names = new StringBuilder();
            foreach (var id in PartInfo.All)
            {
                if ((mask & (1 << (int)id)) != 0)
                {
                    if (names.Length > 0) names.Append(' ');
                    names.Append(id);
                }
            }

            return names.ToString();
        }

        /// <summary>Compass heading of a direction in degrees (0 = +Z, 90 = +X).</summary>
        private static float Yaw(Vector3 v) => Mathf.Repeat(Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, 360f);

        private static Vector3 Flat(Vector3 v, Vector3 fallback)
        {
            v.y = 0f;
            return v.sqrMagnitude > 1e-6f ? v : fallback;
        }

        public static float SegmentDistance(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 ab = b - a;
            float t = ab.sqrMagnitude > 1e-8f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector3.Distance(a + ab * t, p);
        }

        // ---- Summary ---------------------------------------------------------------------------

        public string StateTimes()
        {
            var text = new StringBuilder();
            for (int i = 0; i < stateTime.Length; i++)
            {
                if (stateTime[i] > 0f)
                {
                    if (text.Length > 0) text.Append(", ");
                    text.Append($"{(UnitStateKind)i} {stateTime[i]:0.0} s");
                }
            }

            return text.ToString();
        }

        /// <summary>Mean and max tracking error (degrees) of a part in a context, or false if never seen.</summary>
        public bool Tracking(int context, PartId id, out float mean, out float max)
        {
            int n = errCount[context];
            int k = context * PartInfo.Count + (int)id;
            mean = n > 0 ? errSum[k] / n : 0f;
            max = errMax[k];
            return n > 0;
        }

        public float ContextSeconds(int context) => errCount[context] * Time.fixedDeltaTime;

        public string Snapshot()
        {
            if (unit == null || !unit.IsInitialized)
            {
                return "gone";
            }

            var sense = unit.LastSense;
            var planner = unit.Planner;
            var worst = new List<KeyValuePair<float, PartId>>();
            foreach (var id in PartInfo.All)
            {
                worst.Add(new KeyValuePair<float, PartId>(TrackingError(id), id));
            }

            worst.Sort((a, b) => b.Key.CompareTo(a.Key));
            return $"{unit.State.Kind} {unit.State.TimeInState:0.0} s"
                   + (planner.Active ? $", swing {planner.Current} θ {planner.Theta:0}°" : "")
                   + $", pelvis {sense.pelvisHeight:0.00} m, chest tilt {sense.tiltDegrees:0}°, balance error {unit.Balance.BalanceError:0.00} m, "
                   + $"stability {unit.State.Stability:0}; worst tracking {worst[0].Value} {worst[0].Key:0}°, {worst[1].Value} {worst[1].Key:0}°, {worst[2].Value} {worst[2].Key:0}°";
        }
    }
}
