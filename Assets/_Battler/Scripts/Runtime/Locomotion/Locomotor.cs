using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The new locomotion's core (the locomotion lab): where the body is going and how it moves, from
    /// the reference clips. Each step it takes what the steering wants (a velocity and, apart from it,
    /// a facing), eases toward them with springs fitted to the clips' pace, and makes the body's pose
    /// for it: the cycles that go the way it travels (forward, sideways, backward; walking and running)
    /// blended at one shared gait phase, the phase advanced by the ground covered, the strides warped a
    /// little for speeds between the clips. Switching between whole motions (standing, walking, the
    /// transition clips) never averages two motions: the difference is faded out (inertialization).
    /// The output is the pose against the root (metres; x right, y up, z forward), the root itself,
    /// and the feet's contacts; <see cref="LocoBody"/> puts it on the rig and holds the feet.
    /// </summary>
    public sealed class Locomotor
    {
        public enum Mode
        {
            Idle,
            Cycle,
            OneShot,
        }

        // ---- What it is -------------------------------------------------------------------------

        /// <summary>The body's leg length (metres): everything in the clips is in leg lengths.</summary>
        public readonly float Leg;

        /// <summary>The speed it changes from walking to running (m/s): a Froude number of 0.5.</summary>
        public readonly float RunSpeed;

        // ---- Its state --------------------------------------------------------------------------

        public Vector3 Position;
        public float Facing;
        public Vector3 Velocity;
        public Vector3 Acceleration;
        public float FacingSpeed;
        public Mode Current { get; private set; } = Mode.Idle;

        /// <summary>The gait phase (0 to 1: the left foot lands at 0, the right at 0.5).</summary>
        public float Phase { get; private set; }

        /// <summary>How much it runs (0 walking, 1 running), and whether it is in its running gait.</summary>
        public float RunWeight { get; private set; }
        public bool Running { get; private set; }

        /// <summary>The cycles playing now and their weights (for the log).</summary>
        public readonly List<(LocoClip clip, float weight)> Blend = new List<(LocoClip, float)>();

        /// <summary>The one-shot clip playing (a start, a stop, a turn), and how far into it.</summary>
        public LocoClip Shot { get; private set; }
        public float ShotTime { get; private set; }

        /// <summary>The pose against the root (metres), the feet's contacts (0 to 1: down, heel, toe), as of the last step.</summary>
        public readonly Vector3[] Points = new Vector3[LocoClip.BoneCount];
        public readonly float[] Down = new float[2], Heel = new float[2], Toe = new float[2];

        /// <summary>The stride warp applied (1: as captured) and the cadence against the clips'.</summary>
        public float StrideScale { get; private set; } = 1f;
        public float Cadence { get; private set; } = 1f;

        /// <summary>How fast the gait phase goes on (cycles a second).</summary>
        public float PhaseRate { get; private set; } = 1f;

        /// <summary>How long until a foot that is up lands again (seconds; a guess when standing).</summary>
        public float SwingLeft(int foot)
        {
            if (Current == Mode.Cycle)
            {
                float land = foot == 0 ? 0f : 0.5f;
                return Mathf.Repeat(land - Phase, 1f) / Mathf.Max(0.3f, PhaseRate);
            }

            if (Current == Mode.OneShot && Shot != null)
            {
                float step = 1f / Shot.Rate;
                for (float t = ShotTime + step; t < Mathf.Min(shotEnd, ShotTime + 1.2f); t += step)
                {
                    if ((Shot.Contact(foot, t) & LocoContact.Down) != 0)
                    {
                        return (t - ShotTime) / TimeScale;
                    }
                }
            }

            return 0.3f;
        }

        // ---- Tuning -----------------------------------------------------------------------------

        /// <summary>Half-lives (seconds) of the velocity spring walking and running, and of the facing spring.</summary>
        public float VelocityHalfLife = 0.35f, RunVelocityHalfLife = 0.45f, FacingHalfLife = 0.18f;

        /// <summary>The fastest it turns (degrees a second) walking and running.</summary>
        public float MostTurn = 200f, MostRunTurn = 140f;

        /// <summary>How far a stride may be stretched or shortened against the clips' (the rest is cadence).</summary>
        public float StrideMost = 1.3f, StrideLeast = 0.3f;

        /// <summary>Below this speed (m/s) it stands; above <see cref="WalkFrom"/> it walks.</summary>
        public float StandBelow = 0.12f, WalkFrom = 0.25f;

        /// <summary>Seconds a switch between whole motions takes to fade out.</summary>
        public float SwitchHalfLife = 0.1f;

        /// <summary>A switch between the relaxed walk and a combat clip changes the whole body: it fades over this (seconds, half-life).</summary>
        public float GuardSwitchHalfLife = 0.2f;

        private float switchHalfLifeNow = 0.1f;

        /// <summary>How many switches between motions there have been (each one faded); a pose built on the walk fades its own parts at each.</summary>
        public int Switches { get; private set; }

        /// <summary>The fade of the switch going on now (seconds, half-life).</summary>
        public float SwitchHalfLifeNow => switchHalfLifeNow;

        /// <summary>The current motion's own pose this step, before the switch's fade (leg lengths... scaled, against the root, as <see cref="Points"/>).</summary>
        public Vector3[] RawPoints => raw;
        private bool lastWasGuard;

        // ---- The clips ----------------------------------------------------------------------------

        private readonly LocoClip idle, walkF, walkB, walkLc, walkRc, walkLs, walkRs, runF, runL, runR;
        private float idleTime;

        // Inertialization: what is left of the last motion's pose against the new one, fading out.
        private readonly Vector3[] offset = new Vector3[LocoClip.BoneCount];
        private readonly Vector3[] offsetVelocity = new Vector3[LocoClip.BoneCount];
        private readonly Vector3[] raw = new Vector3[LocoClip.BoneCount];
        private readonly Vector3[] lastRaw = new Vector3[LocoClip.BoneCount];
        private readonly Vector3[] lastOut = new Vector3[LocoClip.BoneCount];
        private readonly Vector3[] scratch = new Vector3[LocoClip.BoneCount];
        private bool hasLast;

        /// <summary>The clips' actor's leg (metres), and how much faster than the clips this body's gait runs.</summary>
        public const float ClipLeg = 0.889f;
        public readonly float TimeScale = 1f;

        public Locomotor(float leg)
        {
            Leg = leg;
            RunSpeed = Mathf.Sqrt(0.5f * 9.81f * leg);
            // (Bodies of other sizes move as the clips would at their size: a gait's speed goes with the
            // square root of the leg (the Froude number), its stride with the leg, so a shorter leg steps
            // quicker. The clips' actor had legs of 0.89 m.)
            TimeScale = Mathf.Clamp(Mathf.Sqrt(ClipLeg / Mathf.Max(0.1f, leg)), 0.7f, 1.6f);
            idle = LocoLibrary.Get("idle");
            walkF = LocoLibrary.Get("walk_fwd");
            walkB = LocoLibrary.Get("walk_back");
            walkLc = LocoLibrary.Get("walk_left_cross");
            walkRc = LocoLibrary.Get("walk_right_cross");
            walkLs = LocoLibrary.Get("walk_left_step");
            walkRs = LocoLibrary.Get("walk_right_step");
            runF = LocoLibrary.Get("run_fwd");
            runL = LocoLibrary.Get("run_left");
            runR = LocoLibrary.Get("run_right");
        }

        public bool Ready => idle != null && walkF != null && walkB != null && walkLc != null && walkRc != null;

        /// <summary>Places it (standing, nothing to fade).</summary>
        public void Place(Vector3 position, float facing)
        {
            Position = position;
            Facing = facing;
            Velocity = Vector3.zero;
            Acceleration = Vector3.zero;
            FacingSpeed = 0f;
            Current = Mode.Idle;
            Phase = 0f;
            idleTime = 0f;
            hasLast = false;
            stanceHeld = false;
            Shot = null;
            for (int i = 0; i < offset.Length; i++)
            {
                offset[i] = offsetVelocity[i] = Vector3.zero;
            }
        }

        /// <summary>
        /// Placed standing with its ankles where they are (world: handed the body by another pose, its
        /// feet stay put): the standing clip's legs, each moved onto its ankle.
        /// </summary>
        public void PlaceStanding(Vector3 position, float facing, Vector3 leftAnkle, Vector3 rightAnkle)
        {
            Place(new Vector3(position.x, 0f, position.z), facing);
            if (idle == null)
            {
                return;
            }

            idle.Pose(0f, raw, Leg);
            var turn = Quaternion.Euler(0f, Facing, 0f);
            for (int f = 0; f < 2; f++)
            {
                Vector3 clipAnkle = Position + turn * raw[(int)(f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot)];
                Vector3 want = f == 0 ? leftAnkle : rightAnkle;
                Vector3 shift = new Vector3(want.x - clipAnkle.x, 0f, want.z - clipAnkle.z);
                for (int i = 0; i < 4; i++)
                {
                    stance[f * 4 + i] = Position + turn * raw[(int)LegBones[f * 4 + i]] + shift;
                }
            }

            stanceHeld = true;
            footHeld[0] = footHeld[1] = true;
            lastSettle = 0f;
        }

        // ---- Each step ------------------------------------------------------------------------------

        /// <param name="wantVelocity">Where the steering wants it to go (world, m/s, flat).</param>
        /// <param name="wantFacing">Which way it wants to face (degrees).</param>
        public void Step(float dt, Vector3 wantVelocity, float wantFacing)
        {
            if (!Ready || dt <= 0f)
            {
                return;
            }

            wantVelocity.y = 0f;
            wantNow = wantVelocity;
            float want = wantVelocity.magnitude;
            if (Current == Mode.OneShot)
            {
                ShotStep(dt);
                Inertialize(dt);
                return;
            }

            // The springs: the velocity and the facing ease toward what is wanted, at the clips' pace.
            // (Standing, the body turns on its feet only a little: a bigger turn is a turning clip.)
            float halfLife = Mathf.Lerp(VelocityHalfLife, RunVelocityHalfLife, RunWeight);
            Vector3 before = Velocity;
            Springs.Track(ref Velocity, ref Acceleration, wantVelocity, halfLife, dt);
            Position += 0.5f * (before + Velocity) * dt;
            float turnWanted = Mathf.DeltaAngle(Facing, wantFacing);
            float facingGoal = Current == Mode.Idle ? Facing + Mathf.Clamp(turnWanted, -StandTwist, StandTwist) : Facing + turnWanted;
            float facing = Facing;
            Springs.Track(ref facing, ref FacingSpeed, facingGoal, Current == Mode.Idle ? 2f * FacingHalfLife : FacingHalfLife, dt);
            float most = Mathf.Lerp(MostTurn, MostRunTurn, RunWeight) * dt;
            Facing += Mathf.Clamp(facing - Facing, -most, most);
            if (Mathf.Abs(facing - Facing) > 1e-3f)
            {
                FacingSpeed = Mathf.Clamp(FacingSpeed, -most / dt, most / dt);
            }

            float speed = new Vector2(Velocity.x, Velocity.z).magnitude;

            // What it does next: a transition clip, or a change between standing and the cycles.
            if (Current == Mode.Idle)
            {
                Vector3 localWant = Quaternion.Euler(0f, -Facing, 0f) * wantVelocity;
                float wantAngle = want > 1e-3f ? Mathf.Atan2(localWant.x, localWant.z) * Mathf.Rad2Deg : 0f;
                if (Mathf.Abs(turnWanted) > TurnFrom && (want < WalkFrom || Mathf.Abs(wantAngle) > 60f && Mathf.Abs(turnWanted) > 60f))
                {
                    StartTurn(turnWanted);
                }
                else if (want > WalkFrom && Mathf.Abs(wantAngle) < 35f && want < RunSpeed && StartClip() != null)
                {
                    StartShot(StartClip(), ShotEnds.Cycle, startFrom: StartFirstMove);
                }
                else if (speed > WalkFrom || want > WalkFrom)
                {
                    Switch(Mode.Cycle);
                }
            }
            else if (Current == Mode.Cycle)
            {
                Vector3 local = Quaternion.Euler(0f, -Facing, 0f) * Velocity;
                float angle = speed > 0.05f ? Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg : 0f;
                float reverse = want > 0.3f && speed > 0.3f ? Vector3.Angle(Velocity, wantVelocity) : 0f;
                if (reverse > 135f && speed > 0.6f && Mathf.Abs(angle) < 40f && Mathf.Abs(turnWanted) > 100f)
                {
                    StartPivot(Vector3.Cross(Velocity, wantVelocity).y);
                }
                else if (speed > 0.45f && !Running && Mathf.Abs(angle) < 35f && TryStop(want, speed))
                {
                }
                else if (speed < StandBelow && want < StandBelow && BothFeetDown() && (StandsWide() || finishing > 1.2f))
                {
                    Switch(Mode.Idle);
                }
            }

            if (Current == Mode.OneShot)
            {
                ShotStep(dt);
            }
            else if (Current == Mode.Idle)
            {
                IdlePose(dt);
            }
            else
            {
                CyclePose(dt, speed);
            }

            Inertialize(dt);
            Facing = Mathf.DeltaAngle(0f, Facing);
        }

        /// <summary>
        /// Plays a clip as it is, its root moving the body, then stands (the films of the combat clips: a
        /// loop plays for <paramref name="loops"/> rounds).
        /// </summary>
        public void Play(LocoClip clip, int loops = 2)
        {
            if (clip == null)
            {
                return;
            }

            StartShot(clip, ShotEnds.Idle, endAt: clip.Loop ? clip.Duration * Mathf.Max(1, loops) : clip.Duration);
        }

        /// <summary>
        /// The walk animating a root that something else moves (the fight's steering, knock-backs, a
        /// lunge): its place, facing and velocity are taken as they are, and only the body is posed:
        /// the cycles and the standing feet, no clips that move the root themselves.
        /// </summary>
        public void Follow(float dt, Vector3 position, float facing, Vector3 velocity)
        {
            if (!Ready || dt <= 0f)
            {
                return;
            }

            if (Current == Mode.OneShot)
            {
                Shot = null;
                Current = Mode.Cycle;
                pendingSwitch = hasLast;
            }

            velocity.y = 0f;
            wantNow = velocity;
            // (A little smoothing: the measured velocity jitters step to step.)
            Vector3 was = Velocity;
            Velocity = Vector3.Lerp(Velocity, velocity, 1f - Mathf.Exp(-dt / 0.06f));
            Acceleration = (Velocity - was) / dt;
            FacingSpeed = Mathf.DeltaAngle(Facing, facing) / dt;
            Facing = Mathf.DeltaAngle(0f, facing);
            Position = new Vector3(position.x, 0f, position.z);
            float speed = new Vector2(Velocity.x, Velocity.z).magnitude;
            if (Current == Mode.Idle && speed > WalkFrom)
            {
                Switch(Mode.Cycle);
            }
            else if (Current == Mode.Cycle && speed < StandBelow && velocity.magnitude < StandBelow && BothFeetDown() && (StandsWide() || finishing > 1.2f))
            {
                Switch(Mode.Idle);
            }

            if (Current == Mode.Idle)
            {
                IdlePose(dt);
            }
            else
            {
                CyclePose(dt, speed);
            }

            Inertialize(dt);
        }

        /// <summary>
        /// A shove: the body's velocity jumps by <paramref name="change"/> (m/s, flat). Whatever it was doing
        /// it steps now, its feet catching it; the steering's velocity spring brings it back to what it
        /// wants, step by step (the plan's stage 6: a capture step from the clips' own stepping).
        /// </summary>
        public void Push(Vector3 change)
        {
            change.y = 0f;
            if (change.sqrMagnitude < 1e-6f || !Ready)
            {
                return;
            }

            Velocity += change;
            Acceleration = Vector3.zero;
            if (Current == Mode.OneShot)
            {
                Shot = null;
                Current = Mode.Cycle;
                Phase = Down[0] >= Down[1] ? 0.1f : 0.6f;
                pendingSwitch = hasLast;
            }
            else if (Current == Mode.Idle)
            {
                Switch(Mode.Cycle);
                AimTravel(Velocity);
            }
            else
            {
                // (Already stepping: the blend turns to the new way over a few steps' time, faded.)
                pendingSwitch = hasLast;
            }

            Pushes++;
        }

        public int Pushes { get; private set; }

        /// <summary>
        /// A small, steady push (m/s added this step, flat): something leaning on the body, or the
        /// upper body knocked off its pose. Only the velocity changes; the walk follows it as it would
        /// any change of speed (a standing body sets off once it is moving). Ignored during a clip.
        /// </summary>
        public void Nudge(Vector3 change)
        {
            change.y = 0f;
            if (Current != Mode.OneShot)
            {
                Velocity += change;
            }
        }

        /// <summary>Standing, how far the body turns on its feet (degrees) before it steps round (a turning clip), and when it turns at all.</summary>
        public float StandTwist = 30f, TurnFrom = 40f;

        /// <summary>Walking, a wanted speed under this (m/s) is a stop: the stopping clip plays.</summary>
        public float StopWhen = 0.35f;

        private bool BothFeetDown() => Down[0] >= 0.5f && Down[1] >= 0.5f;

        /// <summary>The feet apart enough to stand on (across the body: at least 60% of the standing clip's stance); a stop with them together steps once more.</summary>
        private bool StandsWide()
        {
            if (idleWidth <= 0f && idle != null)
            {
                idleWidth = Mathf.Abs(idle.Point(LocoBone.LeftFoot, 0f).x - idle.Point(LocoBone.RightFoot, 0f).x);
            }

            return Mathf.Abs(lastOut[(int)LocoBone.LeftFoot].x - lastOut[(int)LocoBone.RightFoot].x) >= 0.6f * idleWidth * Leg;
        }

        private float idleWidth, finishing;

        /// <summary>A switch between whole motions: the new one starts, what differs is faded out.</summary>
        private void Switch(Mode to)
        {
            var from = Current;
            Current = to;
            if (to == Mode.Cycle && from == Mode.Idle)
            {
                AimTravel(wantNow);
                Phase = SetOffPhase();
            }
            else if (to == Mode.Idle)
            {
                idleTime = 0f;
                HoldStance();
            }

            // (What the old motion had and the new one doesn't is kept, then faded out: worked out
            // once the new motion's first pose is known.)
            pendingSwitch = hasLast;
        }

        /// <summary>
        /// The phase to set off at from a stand: the foot that steps first lifts (phase 0.1: the right;
        /// 0.6: the left). Going forward or back, the foot behind; going sideways, the foot on that side
        /// (a side-step leads with it); a foot that is up already, that one.
        /// </summary>
        private float SetOffPhase()
        {
            if (Down[0] < 0.5f || Down[1] < 0.5f)
            {
                return Down[0] >= Down[1] ? 0.1f : 0.6f;
            }

            Vector3 way = Quaternion.Euler(0f, travelAngle, 0f) * Vector3.forward;
            float left = Vector3.Dot(lastOut[(int)LocoBone.LeftFoot], way);
            float right = Vector3.Dot(lastOut[(int)LocoBone.RightFoot], way);
            bool sideways = Mathf.Abs(travelAngle) > 45f && Mathf.Abs(travelAngle) < 135f;
            bool rightFirst = sideways ? right > left : right < left;
            return rightFirst ? 0.1f : 0.6f;
        }

        // ---- Standing ---------------------------------------------------------------------------------

        // The feet where it stopped (world), held while it stands: the idle's own feet would slide.
        private readonly Vector3[] stance = new Vector3[8];
        private static readonly LocoBone[] LegBones =
        {
            LocoBone.LeftLeg, LocoBone.LeftFoot, LocoBone.LeftToeBase, LocoBone.LeftToeEnd,
            LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.RightToeBase, LocoBone.RightToeEnd,
        };

        private bool stanceHeld;

        private readonly Vector3[] shown = new Vector3[LocoClip.BoneCount];

        private void HoldStance() => HoldStance(lastOut);

        private void HoldStance(Vector3[] pose)
        {
            var turn = Quaternion.Euler(0f, Facing, 0f);
            for (int i = 0; i < LegBones.Length; i++)
            {
                stance[i] = Position + turn * (hasLast ? pose[(int)LegBones[i]] : Vector3.zero);
            }

            stanceHeld = hasLast;
            for (int f = 0; f < 2; f++)
            {
                // (A foot in the air when it stops is not held there: the standing clip puts it down.)
                footHeld[f] = stanceHeld && Down[f] >= 0.5f;
            }

            lastSettle = 0f;
        }

        /// <summary>Standing: whether a foot is held where it stood (or given back to the standing clip).</summary>
        public bool FootHeld(int f) => stanceHeld && footHeld[f];

        // Each foot held where it stood, or given back to the standing clip.
        private readonly bool[] footHeld = new bool[2];
        private float lastSettle;

        /// <summary>
        /// Standing, a held foot further than this (leg lengths) from where the standing clip has it is given
        /// back to the clip, one foot at a time (the body's feet then step there): after a stop, a strike or
        /// a guard clip it settles into its own stance rather than staying where it ended.
        /// </summary>
        public float SettleStep = 0.1f, SettleFrom = 0.35f, SettleBetween = 0.45f;

        private void IdlePose(float dt)
        {
            idleTime += dt * TimeScale;
            idle.Pose(idleTime, raw, Leg);
            Contacts(idle, idleTime, 1f, true);
            Blend.Clear();
            Blend.Add((idle, 1f));
            if (stanceHeld)
            {
                // (The legs where it stopped; the knees a little the idle's way. A held foot far from
                // the standing clip's is given back to it, one at a time.)
                var back = Quaternion.Euler(0f, -Facing, 0f);
                if (idleTime > SettleFrom && idleTime - lastSettle > SettleBetween)
                {
                    int worst = -1;
                    float most = SettleStep * Leg;
                    for (int f = 0; f < 2; f++)
                    {
                        Vector3 held = back * (stance[f * 4 + 1] - Position) - raw[(int)(f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot)];
                        held.y = 0f;
                        if (footHeld[f] && held.magnitude > most)
                        {
                            most = held.magnitude;
                            worst = f;
                        }
                    }

                    if (worst >= 0)
                    {
                        footHeld[worst] = false;
                        lastSettle = idleTime;
                        pendingSwitch = hasLast;
                    }
                }

                for (int i = 0; i < LegBones.Length; i++)
                {
                    if (footHeld[i / 4])
                    {
                        raw[(int)LegBones[i]] = back * (stance[i] - Position);
                    }
                }
            }
        }

        // ---- Transition clips ---------------------------------------------------------------------------

        private enum ShotEnds
        {
            Idle,
            Cycle,
        }

        private ShotEnds shotThen;
        private float shotEnd, shotTurnScale = 1f, shotMoveScale = 1f;
        private Quaternion shotBend = Quaternion.identity;

        /// <summary>
        /// Where the steering means to stop (world), if it does: the stopping clip is started where its
        /// own travel left matches the distance there, and bent and stretched a little to end on it.
        /// </summary>
        public Vector3? StopAt;

        /// <summary>How far a stopping clip may be stretched or bent to end on the stopping point.</summary>
        public float StopStretchLeast = 0.7f, StopStretchMost = 1.3f, StopBendMost = 20f;

        private bool TryStop(float want, float speed)
        {
            if (!StopAt.HasValue && want >= StopWhen)
            {
                return false;
            }

            if (!StopClip(speed, out var stop, out float from))
            {
                return false;
            }

            stop.RootMotion(from, stop.Duration, out var left, out _);
            var turn = Quaternion.Euler(0f, Facing, 0f);
            Vector3 clipLeft = turn * new Vector3(left.x, 0f, left.y) * Leg;
            float scale = 1f;
            var bend = Quaternion.identity;
            if (StopAt.HasValue)
            {
                Vector3 to = StopAt.Value - Position;
                to.y = 0f;
                float d = to.magnitude;
                Vector3 heading = Velocity.sqrMagnitude > 1e-4f ? Velocity.normalized : turn * Vector3.forward;
                // (It stops when the clip's travel left reaches the point; a point behind or well to the side is left to the steering.)
                if (d > clipLeft.magnitude * StopStretchMost * 0.8f + 0.02f || Vector3.Dot(to, heading) < 0.7f * d)
                {
                    return want < StopWhen && StartStop(stop, from, 1f, Quaternion.identity);
                }

                scale = Mathf.Clamp(d / Mathf.Max(0.05f, clipLeft.magnitude), StopStretchLeast, StopStretchMost);
                float angle = Mathf.Clamp(Vector3.SignedAngle(clipLeft, to, Vector3.up), -StopBendMost, StopBendMost);
                bend = Quaternion.Euler(0f, angle, 0f);
            }

            return StartStop(stop, from, scale, bend);
        }

        private bool StartStop(LocoClip stop, float from, float scale, Quaternion bend)
        {
            StartShot(stop, ShotEnds.Idle, startFrom: from);
            shotMoveScale = scale;
            shotBend = bend;
            return true;
        }
        private int startCount;

        /// <summary>The start clip to play (the two mirror images in turn), or null.</summary>
        private LocoClip StartClip() => LocoLibrary.Get(startCount % 2 == 0 ? "start_walk" : "start_walk_m");

        private float StartFirstMove => FirstMove(StartClip());

        /// <summary>Where a clip's root starts to move (seconds): many start clips stand a while first.</summary>
        private static float FirstMove(LocoClip clip)
        {
            if (clip == null)
            {
                return 0f;
            }

            for (float t = 0f; t < clip.Duration; t += 1f / clip.Rate)
            {
                clip.RootMotion(t, t + 0.1f, out var m, out _);
                if (m.magnitude / 0.1f > 0.08f)
                {
                    return Mathf.Max(0f, t - 0.15f);
                }
            }

            return 0f;
        }

        private void StartShot(LocoClip clip, ShotEnds then, float startFrom = 0f, float turnScale = 1f, float endAt = -1f)
        {
            if (clip == clipOf(StartClip()))
            {
                startCount++;
            }

            Shot = clip;
            ShotTime = startFrom;
            shotThen = then;
            shotTurnScale = turnScale;
            shotMoveScale = 1f;
            shotBend = Quaternion.identity;
            shotEnd = endAt > 0f ? endAt : then == ShotEnds.Cycle ? HandOver(clip, startFrom) : clip.Duration;
            Current = Mode.OneShot;
            Blend.Clear();
            Blend.Add((clip, 1f));
            pendingSwitch = hasLast;
        }

        private static LocoClip clipOf(LocoClip c) => c;

        /// <summary>
        /// Where a clip that goes on into walking hands over to the cycles: at a landing, once its root
        /// has reached most of its speed (or its end).
        /// </summary>
        private float HandOver(LocoClip clip, float from)
        {
            float top = 0f;
            for (float t = from; t < clip.Duration - 0.1f; t += 1f / clip.Rate)
            {
                clip.RootMotion(t, t + 0.1f, out var m, out _);
                top = Mathf.Max(top, m.magnitude / 0.1f);
            }

            for (float t = from + 0.3f; t < clip.Duration - 0.05f; t += 1f / clip.Rate)
            {
                clip.RootMotion(t, t + 0.1f, out var m, out _);
                bool lands = false;
                for (int f = 0; f < 2; f++)
                {
                    lands |= (clip.Contact(f, t) & LocoContact.Down) != 0 && (clip.Contact(f, t - 1f / clip.Rate) & LocoContact.Down) == 0;
                }

                if (lands && m.magnitude / 0.1f >= 0.85f * top)
                {
                    return t;
                }
            }

            return clip.Duration;
        }

        private void StartTurn(float degrees)
        {
            bool big = Mathf.Abs(degrees) >= 135f;
            string name = (big ? "turn180_" : "turn90_") + (degrees > 0f ? "right" : "left");
            var clip = LocoLibrary.Get(name);
            if (clip == null || Mathf.Abs(clip.Turn) < 1f)
            {
                Facing += degrees;   // (no clip: it just turns)
                return;
            }

            StartShot(clip, ShotEnds.Idle, turnScale: Mathf.Clamp(degrees / clip.Turn, 0.4f, 1.6f));
        }

        private void StartPivot(float side)
        {
            string name = Running ? "turn180_run" : "turn180_walk";
            var clip = LocoLibrary.Get(name);
            var mirrored = LocoLibrary.Get(name + "_m");
            if (clip == null)
            {
                return;
            }

            // (The clip that turns the way it is going round.)
            var pick = mirrored != null && Mathf.Sign(clip.Turn) != Mathf.Sign(side) ? mirrored : clip;
            StartShot(pick, ShotEnds.Cycle, endAt: pick.Duration);
        }

        /// <summary>
        /// The stopping clip to play and where in it to start: at the walking speed it has now, with the
        /// same foot the last to land (the clip and its mirror image both tried).
        /// </summary>
        private bool StopClip(float speed, out LocoClip best, out float from)
        {
            best = null;
            from = 0f;
            float bestCost = float.MaxValue;
            int lastFoot = Phase < 0.5f ? 0 : 1;
            float sinceLanding = (Phase < 0.5f ? Phase : Phase - 0.5f);
            foreach (var name in new[] { "stop_walk", "stop_walk_m" })
            {
                var clip = LocoLibrary.Get(name);
                if (clip == null)
                {
                    continue;
                }

                float lastLanding = -1f;
                int landedFoot = -1;
                for (float t = 1f / clip.Rate; t < clip.Duration * 0.5f; t += 1f / clip.Rate)
                {
                    for (int f = 0; f < 2; f++)
                    {
                        if ((clip.Contact(f, t) & LocoContact.Down) != 0 && (clip.Contact(f, t - 1f / clip.Rate) & LocoContact.Down) == 0)
                        {
                            lastLanding = t;
                            landedFoot = f;
                        }
                    }

                    if (landedFoot != lastFoot)
                    {
                        continue;
                    }

                    clip.RootMotion(t - 0.05f, t + 0.05f, out var m, out _);
                    float clipSpeed = m.magnitude * Leg * TimeScale / 0.1f;
                    float cost = Mathf.Abs(clipSpeed - speed) + 0.5f * Mathf.Abs((t - lastLanding) - sinceLanding * 1.0f);
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        best = clip;
                        from = t;
                    }
                }
            }

            return best != null;
        }

        private void ShotStep(float dt)
        {
            var clip = Shot;
            float from = ShotTime;
            ShotTime = Mathf.Min(shotEnd, ShotTime + dt * TimeScale);
            clip.RootMotion(from, ShotTime, out var moved, out float turned);
            var turn = Quaternion.Euler(0f, Facing, 0f);
            Vector3 step = shotBend * (turn * new Vector3(moved.x, 0f, moved.y)) * (Leg * shotMoveScale);
            Position += step;
            Facing += turned * shotTurnScale;
            Velocity = step / dt;
            Acceleration = Vector3.zero;
            FacingSpeed = turned * shotTurnScale / dt;
            clip.Pose(ShotTime, raw, Leg);
            Contacts(clip, ShotTime, 1f, true);
            if (ShotTime >= shotEnd - 1e-4f)
            {
                // (The new motion's pose is worked out now, in this same step, so the switch fades from
                // the last pose shown into it.)
                if (shotThen == ShotEnds.Cycle)
                {
                    // On into the cycles: the phase from the foot that last landed, and how long ago.
                    Phase = PhaseAfter(clip, ShotTime, Velocity.magnitude);
                    AimTravel(Velocity);
                    Current = Mode.Cycle;
                    CyclePose(0f, new Vector2(Velocity.x, Velocity.z).magnitude);
                }
                else
                {
                    // To a stand: the feet held where the clip left them.
                    for (int i = 0; i < raw.Length; i++)
                    {
                        shown[i] = raw[i] + offset[i];
                    }

                    Current = Mode.Idle;
                    idleTime = 0f;
                    HoldStance(shown);
                    IdlePose(0f);
                }

                pendingSwitch = hasLast;
                Shot = null;
            }
        }

        private float PhaseAfter(LocoClip clip, float time, float speed)
        {
            for (float t = time; t > 0f; t -= 1f / clip.Rate)
            {
                for (int f = 0; f < 2; f++)
                {
                    if ((clip.Contact(f, t) & LocoContact.Down) != 0 && (clip.Contact(f, t - 1f / clip.Rate) & LocoContact.Down) == 0)
                    {
                        float stride = walkF.Stride * Leg;
                        return Mathf.Repeat((f == 0 ? 0f : 0.5f) + (time - t) * Mathf.Max(0.3f, speed) / Mathf.Max(0.1f, stride), 1f);
                    }
                }
            }

            return 0.1f;
        }

        private bool pendingSwitch;

        // ---- The cycles -------------------------------------------------------------------------------

        private Vector3 wantNow;
        private float travelAngle;

        private void AimTravel(Vector3 way)
        {
            Vector3 local = Quaternion.Euler(0f, -Facing, 0f) * way;
            if (new Vector2(local.x, local.z).sqrMagnitude > 1e-4f)
            {
                travelAngle = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
            }
        }

        private void CyclePose(float dt, float speed)
        {
            // The way it travels against its facing (degrees, + right), and the gait.
            // (Slow, the way it travels is unsure: the way it wants to go, or the way it was going.)
            Vector3 local = Quaternion.Euler(0f, -Facing, 0f) * (speed > 0.2f ? Velocity : wantNow);
            if (speed > 0.2f || wantNow.magnitude > 0.2f)
            {
                float measured = Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg;
                // (Kept within ±180: past it, the blend below would find no pair of directions.)
                travelAngle = Mathf.DeltaAngle(0f, travelAngle + Mathf.DeltaAngle(travelAngle, measured) * (1f - Mathf.Pow(0.5f, dt / 0.08f)));
            }

            float angle = travelAngle;
            float up = RunSpeed * 1.08f, down = RunSpeed * 0.92f;
            bool backward = Mathf.Abs(angle) > 120f;
            if (!Running && speed > up && !backward && runF != null)
            {
                Running = true;
            }
            else if (Running && (speed < down || backward))
            {
                Running = false;
            }

            RunWeight = Mathf.MoveTowards(RunWeight, Running ? 1f : 0f, dt / 0.4f);

            Blend.Clear();
            AddDirections(Blend, angle, speed, 1f - RunWeight, false);
            if (RunWeight > 0f)
            {
                AddDirections(Blend, angle, speed, RunWeight, true);
            }

            // The blend's own speed and stride (m/s, metres a cycle): the phase goes on by the ground covered.
            float naturalSpeed = 0f, stride = 0f, total = 0f;
            foreach (var (clip, w) in Blend)
            {
                naturalSpeed += w * clip.Speed * Leg * TimeScale;
                stride += w * clip.Stride * Leg;
                total += w;
            }

            naturalSpeed /= Mathf.Max(1e-4f, total);
            stride /= Mathf.Max(1e-4f, total);
            float k = speed / Mathf.Max(0.05f, naturalSpeed);
            StrideScale = Mathf.Clamp(Mathf.Sqrt(Mathf.Max(0.01f, k)), StrideLeast, StrideMost);
            Cadence = k / StrideScale;
            PhaseRate = speed / Mathf.Max(0.05f, stride * StrideScale);
            finishing = wantNow.magnitude < StandBelow ? finishing + dt : 0f;
            if (wantNow.magnitude < StandBelow && (!BothFeetDown() || !StandsWide()))
            {
                // (Come to a stand with a foot up: the step is finished, not frozen.)
                PhaseRate = Mathf.Max(PhaseRate, 0.7f);
            }

            Phase = Mathf.Repeat(Phase + PhaseRate * dt, 1f);

            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Vector3.zero;
            }

            // The body: every cycle at the shared phase.
            float[] duty = { 0f, 0f };
            foreach (var (clip, w) in Blend)
            {
                float t = clip.TimeAtPhase(Phase);
                clip.Pose(t, scratch, Leg);
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] += scratch[i] * (w / total);
                }

                duty[0] += clip.Duty[0] * w / total;
                duty[1] += clip.Duty[1] * w / total;
            }

            // The legs, each foot on its own: every cycle's stance stretched onto the blend's, so all
            // the cycles have the foot down together (Johansen's semi-procedural locomotion). A foot
            // that is down in one cycle and swinging in another would slide.
            Vector3 d = speed > 0.05f ? new Vector3(local.x, 0f, local.z).normalized : Vector3.forward;
            Vector3 hips = 0.5f * (raw[(int)LocoBone.LeftUpLeg] + raw[(int)LocoBone.RightUpLeg]);
            for (int f = 0; f < 2; f++)
            {
                var bones = f == 0 ? LeftLegBones : RightLegBones;
                foreach (var b in bones)
                {
                    raw[(int)b] = Vector3.zero;
                }

                float land = f == 0 ? 0f : 0.5f;
                float u = Mathf.Repeat(Phase - land, 1f);
                float sBlend = duty[f];
                float stanceTravel = 0f;
                Heel[f] = Toe[f] = 0f;
                foreach (var (clip, w) in Blend)
                {
                    float si = clip.Duty[f];
                    float ui = u < sBlend ? u * si / sBlend : si + (u - sBlend) * (1f - si) / (1f - sBlend);
                    float t = clip.TimeAtPhase(land + ui);
                    clip.Pose(t, scratch, Leg, bones);
                    foreach (var b in bones)
                    {
                        raw[(int)b] += scratch[(int)b] * (w / total);
                    }

                    var c = clip.Contact(f, t);
                    Heel[f] += (c & LocoContact.Heel) != 0 ? w / total : 0f;
                    Toe[f] += (c & LocoContact.Toe) != 0 ? w / total : 0f;
                    stanceTravel += w / total * clip.Stride * Leg * si;
                }

                Down[f] = u < sBlend ? 1f : 0f;

                // Stride warping: the foot (and half as much the knee) nearer or further along the way
                // it travels, about its own hip (where it stands at rest: about the hips' middle, a
                // short side-step would draw the feet together); on top, what makes the stance foot's
                // travel the root's.
                Vector3 hip = raw[(int)(f == 0 ? LocoBone.LeftUpLeg : LocoBone.RightUpLeg)];
                float fit = stanceTravel > 1e-4f ? stride * sBlend / stanceTravel : 1f;
                float warp = StrideScale * Mathf.Clamp(fit, 0.8f, 1.25f);
                if (Mathf.Abs(warp - 1f) > 1e-3f)
                {
                    foreach (var b in bones)
                    {
                        float share = b == LocoBone.LeftLeg || b == LocoBone.RightLeg ? 0.5f : 1f;
                        Vector3 p = raw[(int)b];
                        float along = Vector3.Dot(p - hip, d);
                        raw[(int)b] = p + d * (along * (warp - 1f) * share);
                    }
                }
            }
        }

        private static readonly LocoBone[] LeftLegBones = { LocoBone.LeftLeg, LocoBone.LeftFoot, LocoBone.LeftToeBase, LocoBone.LeftToeEnd };
        private static readonly LocoBone[] RightLegBones = { LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.RightToeBase, LocoBone.RightToeEnd };


        /// <summary>The cycles for a way of travelling: the two nearest of forward, right, backward and left, by angle.</summary>
        private void AddDirections(List<(LocoClip, float)> into, float angle, float speed, float weight, bool run)
        {
            if (weight <= 0f)
            {
                return;
            }

            // (Sideways: slow side-steps blend into the crossover walk as it speeds up.)
            angle = Mathf.DeltaAngle(0f, angle);
            float cross = Mathf.Clamp01((speed - 0.8f) / 0.6f);
            var forward = run ? runF : walkF;
            var back = walkB;
            float sideAt = 85f;
            var dirs = new[] { -180f, -sideAt, 0f, sideAt, 180f };
            int seg = 0;
            for (int i = 0; i < 4; i++)
            {
                if (angle >= dirs[i] && angle <= dirs[i + 1])
                {
                    seg = i;
                }
            }

            float t = Mathf.InverseLerp(dirs[seg], dirs[seg + 1], angle);
            void Side(bool right, float w)
            {
                if (w <= 0f)
                {
                    return;
                }

                if (run)
                {
                    into.Add((right ? runR : runL, w));
                    return;
                }

                var slow = right ? walkRs : walkLs;
                var fast = right ? walkRc : walkLc;
                if (slow != null && cross < 1f)
                {
                    into.Add((slow, w * (1f - cross)));
                }

                if (cross > 0f || slow == null)
                {
                    into.Add((fast, w * (slow == null ? 1f : cross)));
                }
            }

            switch (seg)
            {
                case 0: into.Add((back, weight * (1f - t))); Side(false, weight * t); break;
                case 1: Side(false, weight * (1f - t)); into.Add((forward, weight * t)); break;
                case 2: into.Add((forward, weight * (1f - t))); Side(true, weight * t); break;
                default: Side(true, weight * (1f - t)); into.Add((back, weight * t)); break;
            }

            into.RemoveAll(e => e.Item2 <= 1e-4f);
        }

        private void Contacts(LocoClip clip, float time, float weight, bool reset)
        {
            for (int f = 0; f < 2; f++)
            {
                if (reset)
                {
                    Down[f] = Heel[f] = Toe[f] = 0f;
                }

                var c = clip.Contact(f, time);
                Down[f] += (c & LocoContact.Down) != 0 ? weight : 0f;
                Heel[f] += (c & LocoContact.Heel) != 0 ? weight : 0f;
                Toe[f] += (c & LocoContact.Toe) != 0 ? weight : 0f;
            }
        }

        // ---- Inertialization ------------------------------------------------------------------------

        private void Inertialize(float dt)
        {
            if (pendingSwitch)
            {
                // (Into or out of a combat clip: a longer fade; between walk clips, the usual.)
                bool guardNow = Current == Mode.OneShot && Shot != null && Shot.IsGuard;
                switchHalfLifeNow = guardNow != lastWasGuard ? GuardSwitchHalfLife : SwitchHalfLife;
                lastWasGuard = guardNow;
                for (int i = 0; i < offset.Length; i++)
                {
                    offset[i] = lastOut[i] - raw[i];
                    offsetVelocity[i] = Vector3.zero;
                }

                pendingSwitch = false;
                Switches++;
            }

            for (int i = 0; i < raw.Length; i++)
            {
                if (offset[i].sqrMagnitude > 1e-10f || offsetVelocity[i].sqrMagnitude > 1e-10f)
                {
                    Springs.Decay(ref offset[i], ref offsetVelocity[i], switchHalfLifeNow, dt);
                }

                Points[i] = raw[i] + offset[i];
                lastRaw[i] = raw[i];
                lastOut[i] = Points[i];
            }

            hasLast = true;
        }
    }

    /// <summary>Critically damped springs, solved exactly (Holden, "Spring-It-On").</summary>
    public static class Springs
    {
        private const float Ln2 = 0.69314718f;

        public static float Damping(float halfLife) => 4f * Ln2 / (halfLife + 1e-5f);

        /// <summary>A value (and its rate) toward a goal.</summary>
        public static void Track(ref Vector3 x, ref Vector3 v, Vector3 goal, float halfLife, float dt)
        {
            float y = Damping(halfLife) / 2f;
            Vector3 j0 = x - goal;
            Vector3 j1 = v + j0 * y;
            float e = Mathf.Exp(-y * dt);
            x = e * (j0 + j1 * dt) + goal;
            v = e * (v - j1 * y * dt);
        }

        public static void Track(ref float x, ref float v, float goal, float halfLife, float dt)
        {
            float y = Damping(halfLife) / 2f;
            float j0 = x - goal;
            float j1 = v + j0 * y;
            float e = Mathf.Exp(-y * dt);
            x = e * (j0 + j1 * dt) + goal;
            v = e * (v - j1 * y * dt);
        }

        /// <summary>An offset (and its rate) fading to nothing.</summary>
        public static void Decay(ref Vector3 x, ref Vector3 v, float halfLife, float dt)
        {
            Track(ref x, ref v, Vector3.zero, halfLife, dt);
        }
    }
}
