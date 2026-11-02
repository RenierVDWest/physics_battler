using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>One weapon contact: who struck whom, where, how hard, and what it did.</summary>
    public sealed class HitReport
    {
        public UnitController Attacker;
        public UnitController Struck;
        public PartId Part;
        public bool Shield;

        /// <summary>Milestone 11: met by the struck unit's weapon set across it (a parry): stopped, no damage.</summary>
        public bool Parried;

        /// <summary>Stopped by a shield or a parrying weapon.</summary>
        public bool Blocked => Shield || Parried;

        /// <summary>Milestone 11: landed on an arm the struck unit's shell guards with (its poise as a block's).</summary>
        public bool ArmGuard;

        /// <summary>Milestone 11: it broke the struck unit's guard.</summary>
        public bool GuardBreak;
        public MoveState Move;
        public Vector3 Point;
        public float Speed;
        public HitResult Result;

        /// <summary>The part the blow was aimed at; the target but another part; an ally of the attacker; or someone else in the way.</summary>
        public string Kind;

        /// <summary>Milestone 5: the joint the blow cut through (the part below it), or null; and why a near cut didn't (for the log).</summary>
        public PartId? Severed;
        public string SeverMiss;

        /// <summary>Not a weapon: a body thrown into another (<see cref="Move"/> is null); the thrower's part that struck.</summary>
        public bool Impact;
        public PartId ImpactPart;

        /// <summary>What the blow is called in the log: the move's name, or a body impact.</summary>
        public string What => Move != null ? Move.Move.name.Replace("MD_", "") : Impact ? "body impact" : "blow";
    }

    /// <summary>
    /// The combat half of a unit (milestone 3): its health and poise, the weapon sweep that finds
    /// what a blow strikes, the reactions to being struck, and keeping bodies apart. There are no
    /// hit animations: recoil springs throw the pose back from the blow, the struck part takes
    /// some of the impulse physically while held loosely, and the rest knocks the whole unit back
    /// (the feet step after it).
    /// </summary>
    public partial class UnitController
    {
        public static event Action<HitReport> Hit;
        public static event Action<UnitController, HitReport> Died;

        /// <summary>A blow is coming at a unit, and what it does about it: (defender, attacker, the blow, what it did).</summary>
        public static event Action<UnitController, UnitController, MoveState, string> Defending;

        public float Health { get; private set; }
        public float Poise { get; private set; }
        public bool IsAlive { get; private set; } = true;

        /// <summary>Off, the unit's blows pass through bodies (the aim tests measure the whole blow).</summary>
        public bool DealsHits { get; set; } = true;

        /// <summary>The unit's own sliding from a knock-back (m/s, world, horizontal).</summary>
        public Vector3 Knockback { get; private set; }

        /// <summary>Seconds of hit stop left: the pose and the blow stand still.</summary>
        public float HitStop { get; private set; }

        /// <summary>How far the shield is raised across an incoming blow (0 to 1).</summary>
        public float BlockBlend { get; private set; }

        public Reaction LastReaction { get; private set; } = Reaction.Recoil;

        // Recovering regions: physics owns them after a hit, the pin ramps back up.
        private readonly float[] recoverFrom = new float[5];
        private readonly float[] recoverTime = new float[5];
        private readonly float[] recoverLeft = new float[5];

        private readonly HashSet<UnitController> struckThisBlow = new HashSet<UnitController>();
        private float momentumLeft = 1f;
        private Vector3 lastGrip, lastTip, lastStrike;
        private bool hasLastSweep;
        private System.Random combatRandom;
        private MoveState cutShort;

        /// <summary>Seconds a cut-short blow takes to leave the pose.</summary>
        private const float CutShortFade = 0.35f;

        /// <summary>How fast a stopped blow plays backwards as it bounces off (share of its own speed).</summary>
        private const float ReboundSpeed = 0.5f;

        /// <summary>Move time advanced at the last step (a stopped blow is wound back to where it met the body).</summary>
        private float lastAdvance;

        /// <summary>Where the root was after the last step (a walk is measured from it); cleared when the unit is moved at once.</summary>
        private Vector3 lastRoot;

        /// <summary>How fast the game walked the root over the last step (world, m/s).</summary>
        public Vector3 WalkVelocity { get; private set; }
        private bool hasLastRoot;

        // The recoil springs: hips pushed, upper body and head thrown back by a hit (Pose.React*).
        private Vector3 reactShift, reactShiftV, reactLean, reactLeanV, reactHead, reactHeadV;

        /// <summary>The recoil's swing back (Hz) and damping (1 = no overshoot): a little overshoot reads as weight.</summary>
        private const float ReactFrequency = 1.6f, ReactDamping = 0.55f, HeadFrequency = 2.4f;

        /// <summary>The most a hit throws the upper body (radians), the head, and the hips (share of the leg).</summary>
        private const float MaxLean = 0.6f, MaxHeadSnap = 0.7f, MaxShift = 0.22f;

        /// <summary>The kick's speed against the spring's own (a damped spring kicked at Kick·ω·x peaks near x).</summary>
        private const float Kick = 1.8f;

        /// <summary>The fastest a struck part is sent on its own (m/s, medium size).</summary>
        private const float MaxPartSpeed = 5f;

        // Milestone 9c: a hit on an arm (or on the shield it holds) knocks that arm's pose aside and
        // lets it come back, critically damped: once out of the way, one smooth return.
        private readonly Vector3[] armKnock = new Vector3[2], armKnockV = new Vector3[2], armTurn = new Vector3[2], armTurnV = new Vector3[2];

        /// <summary>The arm knock's return (Hz; critically damped, so it peaks about a sixth of a second after the hit and is back in about half a second).</summary>
        public const float ArmKnockFrequency = 2.5f;

        /// <summary>The most a hit knocks an arm's hand aside (metres at medium size) and turns it (radians, about 25 degrees).</summary>
        public const float MaxArmKnock = 0.12f, MaxArmTurn = 0.44f;

        /// <summary>A struck arm's hold drops to this share of its pin (about 2 Hz for the arms' 0.7) and comes back over this long (s).</summary>
        public const float ArmRecoverShare = 0.4f, ArmRecoverTime = 0.4f;

        /// <summary>The most a hit on an arm sends the arm's physical parts (m/s, medium size): the pose carries the visible knock.</summary>
        public const float MaxArmPartSpeed = 2f;

        /// <summary>The fastest a knock-back slides the unit (m/s, medium size).</summary>
        private const float MaxKnockback = 3.5f;

        /// <summary>The fastest a cleave throws a body aside (m/s, medium size).</summary>
        private const float MaxCleaveSpeed = 3f;

        /// <summary>Speeds go with the square root of size (a bigger body's parts swing slower for the same look).</summary>
        private float SizeSpeed() => Mathf.Sqrt(Mathf.Max(0.3f, Body.SizeFactor));

        // An incoming blow the shield is raised against.
        private MoveState incoming;
        private float blockFrom, blockUntil;

        private void InitializeCombat(int seed)
        {
            Health = profile.maxHealth;
            Poise = profile.maxPoise;
            IsAlive = true;
            combatRandom = new System.Random(seed + 17);
        }

        // ---- Each step --------------------------------------------------------------------------

        /// <summary>Hit stop: the time the pose runs on (0 while stopped).</summary>
        private float CombatTime(float dt)
        {
            if (HitStop > 0f)
            {
                HitStop = Mathf.Max(0f, HitStop - dt);
                return 0f;
            }

            return dt;
        }

        /// <summary>Knock-back slides the unit (the feet step after it); poise comes back.</summary>
        private void CombatBeforePose(float dt)
        {
            // How fast the game walked the root since the last step (the sandbox moves it between
            // steps): the feet keep up with a walk as with a knock-back.
            Vector3 walked = Vector3.zero;
            if (hasLastRoot && dt > 0f && State == UnitState.Standing)
            {
                walked = transform.position - lastRoot;
                walked.y = 0f;
                walked = Vector3.ClampMagnitude(walked / dt, 3f * SizeSpeed());
            }

            if (State == UnitState.Stumbling)
            {
                UpdateStumble(dt);
            }
            else if (Knockback.sqrMagnitude > 1e-6f)
            {
                transform.position += Knockback * dt;
                Knockback *= Mathf.Exp(-dt / KnockbackDecay);
            }
            else
            {
                Knockback = Vector3.zero;
            }

            Vector3 pushed = Separate(dt);
            // The feet hurry after a knock-back or a stumble (catch steps), or after being pushed aside.
            Stance.Drift = (State == UnitState.Stumbling ? StumbleVelocity : Knockback) + walked + (dt > 0f ? pushed / dt : Vector3.zero);
            WalkVelocity = walked;
            lastRoot = transform.position;
            hasLastRoot = true;
            UpdateTopple(dt);
            UpdateRecoil(dt);
            Poise = Mathf.Min(profile.maxPoise, Poise + profile.poiseRegen * dt);
            for (int r = 0; r < 5; r++)
            {
                recoverLeft[r] = Mathf.Max(0f, recoverLeft[r] - dt);
            }

            // (The shield goes up or down in UpdateShield, milestone 9.)
        }

        /// <summary>Seconds a knock-back takes to die away (it slows exponentially).</summary>
        private const float KnockbackDecay = 0.3f;

        /// <summary>How firmly each region is held this step: the profile's pins, a hit's ramp, the blow's weapon arm.</summary>
        private void SetAuthority(MoveState move)
        {
            if (!IsAlive || State == UnitState.Down)
            {
                for (int r = 0; r < 5; r++)
                {
                    Authority.Set((BodyRegion)r, AuthorityLevel.Ragdoll, 0f);
                }

                return;
            }

            if (State == UnitState.GettingUp)
            {
                SetGetUpAuthority();
                return;
            }

            Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
            // (Milestone 10: an off-hand blow is the off arm's; the weapon arm keeps its guard.)
            var offMove = move != null && move.Move.hand == MoveHand.Off ? move : null;
            if (offMove != null)
            {
                move = null;
            }

            float blend = move != null ? move.Blend : 0f;
            // Stumbling, the arms are held loosely (they fly out with the body) and the torso a little less.
            float loose = Mathf.Lerp(1f, 0.4f, flail);
            float armR = Mathf.Lerp(profile.armPin, move != null ? move.Move.weaponArmPin : profile.armPin, blend) * loose;
            float armL = (BlockBlend > 0f ? Mathf.Lerp(profile.armPin, 0.95f, BlockBlend) : profile.armPin) * loose;
            float torso = profile.torsoPin * Mathf.Lerp(1f, 0.8f, flail);
            SetRegion(BodyRegion.Torso, torso, false);
            SetRegion(BodyRegion.Head, torso, false);
            // The off arm in its own blow is posed exactly, as the weapon arm is in one, and for a moment after.
            bool kinematicOff = offMove != null && profile.weaponArmKinematicInBlows && offMove.Blend > 0.01f;
            if (kinematicOff)
            {
                offArmHeldUntil = LocalTime + WeaponArmHeldAfter;
            }
            else if (profile.weaponArmKinematicInBlows && State == UnitState.Standing && LocalTime < offArmHeldUntil)
            {
                kinematicOff = true;
            }

            SetRegion(BodyRegion.ArmL, armL, kinematicOff);
            // (In a blow, and while a stopped one fades, the weapon arm is posed exactly: the physical
            // arm can't follow a fast swing's turns, and chasing them shakes. Milestone 9c tried holding
            // it by the servo instead; the playtest showed the blade trailing and flailing.)
            bool kinematicArm = (move != null && profile.weaponArmKinematicInBlows && blend > 0.01f)
                || (ParryBlend > 0f && profile.weaponArmKinematicInBlows);   // (milestone 11: and while it parries)
            // Milestone 10: and for a moment after, while the arm comes back to its guard (held by the
            // servo straight away, the arm trailed its fast return by 30 to 80 degrees and twisted).
            if (kinematicArm)
            {
                weaponArmHeldUntil = LocalTime + WeaponArmHeldAfter;
            }
            else if (profile.weaponArmKinematicInBlows && State == UnitState.Standing && LocalTime < weaponArmHeldUntil)
            {
                kinematicArm = true;
            }

            SetRegion(BodyRegion.ArmR, armR, kinematicArm);
        }

        /// <summary>Seconds the weapon arm stays posed exactly after a blow, while it comes back to its guard.</summary>
        public const float WeaponArmHeldAfter = 0.4f;

        private float weaponArmHeldUntil = -1f, offArmHeldUntil = -1f;

        private void SetRegion(BodyRegion region, float pin, bool kinematic)
        {
            int r = (int)region;
            // A held arm's joints swing free: the servo shapes it. (Milestone 9c gave held arms their
            // anatomical limits, against sword wrists twisted 100 to 150 degrees past them after
            // blows; milestone 10 found the limits trapped the arm instead. Each part is turned
            // straight toward its pose, and that way ran the wrist into the ends of its narrow
            // side-to-side range (55 degrees, beside a roll of up to 127): stopped there, the guard
            // arm sat 20 to 30 degrees off its pose for seconds at a time, every playtest from the
            // night they went in, against 2 degrees before. Free bodies in the stub track it within
            // a degree. The twists after blows are kept off by holding the weapon arm on its pose
            // for a moment after each blow instead: WeaponArmHeldAfter.)
            //
            // Milestone 10 (the arm rebuild's step 6): the weapon arm is held by its joints' motors
            // instead, its limits on: the arm rig poses it inside the very limits PhysX holds (step 1),
            // and a motor turns its own joint only, so it can't drag the arm into them.
            bool muscled = region == BodyRegion.ArmR && AuthorityController.ArmMuscles;
            bool limits = muscled;
            var held = muscled ? AuthorityLevel.Muscled : AuthorityLevel.Pinned;
            if (recoverLeft[r] > 0f)
            {
                // Recovering from a hit: physics has it, the pin eases back up.
                float t = 1f - recoverLeft[r] / Mathf.Max(1e-3f, recoverTime[r]);
                float ease = t * t * (3f - 2f * t);
                Authority.Set(region, held, Mathf.Lerp(recoverFrom[r] * pin, pin, ease), limits);
                return;
            }

            Authority.Set(region, kinematic ? AuthorityLevel.Kinematic : held, pin, limits);
        }

        // ---- Bodies keep apart -------------------------------------------------------------------

        /// <summary>
        /// Two bodies can't stand in the same place: where this unit's hips come closer to another
        /// living unit's than their two body radii, it is pushed out by its share (the lighter one
        /// moves more; the other unit does its share on its own step). The feet step after.
        /// </summary>
        private Vector3 Separate(float dt)
        {
            Vector3 total = Vector3.zero;
            if (!initialized || !IsAlive || IsDown)
            {
                return total;
            }

            float most = MaxSeparationSpeed * Mathf.Max(dt, 1e-3f);

            Vector3 me = Anim.Part(PartId.Pelvis).position;
            float myRadius = BodyRadius;
            foreach (var other in all)
            {
                // (A body on the ground is only pushed about by PhysX; one getting up holds its place.)
                if (other == this || !other.initialized || !other.IsAlive || other.State == UnitState.Down)
                {
                    continue;
                }

                // A unit lunging at its target isn't pushed back by it (the lunge is the push: its
                // planted feet hold its hips, so moving its root would only pile up); the target gives way.
                bool myTarget = CurrentMove != null && ReferenceEquals(CurrentMove.Target, other);
                bool theirTarget = other.CurrentMove != null && ReferenceEquals(other.CurrentMove.Target, this);
                if (myTarget)
                {
                    continue;
                }

                // The hips, and the roots (a knock-back slides the root while the planted feet hold
                // the hips): whichever overlaps more.
                Vector3 away = me - other.Anim.Part(PartId.Pelvis).position;
                away.y = 0f;
                Vector3 awayRoots = transform.position - other.transform.position;
                awayRoots.y = 0f;
                if (awayRoots.sqrMagnitude < away.sqrMagnitude)
                {
                    away = awayRoots;
                }

                float apart = myRadius + other.BodyRadius;
                float d = away.magnitude;
                if (d >= apart)
                {
                    continue;
                }

                Vector3 n = d > 1e-4f ? away / d : transform.rotation * Vector3.back;
                float share = theirTarget || other.State == UnitState.GettingUp ? 1f : other.Body.Mass / Mathf.Max(1f, other.Body.Mass + Body.Mass);
                Vector3 push = n * Mathf.Min((apart - d) * share, most);
                transform.position += push;
                me += push;
                total += push;
                // Walking or being knocked into someone stops there; stumbling into someone knocks
                // them by what they take of it (by weight).
                float into = Vector3.Dot(Knockback, -n);
                if (into > 0f)
                {
                    Knockback += n * into;
                }

                into = Vector3.Dot(StumbleVelocity, -n);
                if (into > 0f && State == UnitState.Stumbling)
                {
                    StumbleVelocity += n * into;
                    float given = into * Body.Mass / Mathf.Max(1f, Body.Mass + other.Body.Mass);
                    if (other.State == UnitState.Standing || other.State == UnitState.Stumbling)
                    {
                        other.Knock(-n * given, false, false, "stumbled into by " + name);
                    }
                }
            }

            return total;
        }

        /// <summary>The fastest bodies are pushed apart (m/s): quick enough to stay out of each other, never a pop.</summary>
        private const float MaxSeparationSpeed = 2.5f;

        /// <summary>How far the body takes up space around its hips (metres, horizontally).</summary>
        public float BodyRadius => 0.58f * Body.ShoulderWidth;

        // ---- Recoil ------------------------------------------------------------------------------

        private void UpdateRecoil(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            float leg = Body.LegLength;
            Spring(ref reactShift, ref reactShiftV, ReactFrequency, ReactDamping, dt);
            Spring(ref reactLean, ref reactLeanV, ReactFrequency, ReactDamping, dt);
            Spring(ref reactHead, ref reactHeadV, HeadFrequency, ReactDamping, dt);
            reactShift = Vector3.ClampMagnitude(reactShift, MaxShift * leg);
            reactLean = Vector3.ClampMagnitude(reactLean, MaxLean);
            reactHead = Vector3.ClampMagnitude(reactHead, MaxHeadSnap);
            Pose.ReactShift = reactShift;
            Pose.ReactLean = reactLean;
            Pose.ReactHead = reactHead;

            float wk = 2f * Mathf.PI * ArmKnockFrequency;
            for (int a = 0; a < 2; a++)
            {
                CriticalSpring(ref armKnock[a], ref armKnockV[a], wk, dt);
                CriticalSpring(ref armTurn[a], ref armTurnV[a], wk, dt);
            }

            Pose.KnockR = armKnock[0];
            Pose.KnockTurnR = armTurn[0];
            Pose.KnockL = armKnock[1];
            Pose.KnockTurnL = armTurn[1];
        }

        /// <summary>One step of a critically damped spring toward zero, solved exactly (no overshoot, stable at any step).</summary>
        public static void CriticalSpring(ref Vector3 x, ref Vector3 v, float w, float dt)
        {
            float decay = Mathf.Exp(-w * dt);
            Vector3 c = v + w * x;
            Vector3 nx = (x + c * dt) * decay;
            v = (v - c * (w * dt)) * decay;
            x = nx;
        }

        /// <summary>
        /// A hit on an arm, or on the shield it holds: its pose is knocked back along the blow and
        /// turned by the blow's lever about the hand, up to <see cref="MaxArmKnock"/> and
        /// <see cref="MaxArmTurn"/>, saturating with the blow; the spring brings it back.
        /// </summary>
        private void KnockArm(BodyRegion region, Vector3 impulse, Vector3 point, float carried)
        {
            if (region != BodyRegion.ArmL && region != BodyRegion.ArmR)
            {
                return;
            }

            int a = region == BodyRegion.ArmR ? 0 : 1;
            float j = impulse.magnitude;
            if (j < 1e-4f)
            {
                return;
            }

            // (How hard, for what it moves: the arm, and what the hand carries.)
            float mass = carried;
            foreach (var id in ArmChain(region))
            {
                var body = Rig.Part(id).body;
                mass += body != null ? body.mass : 0f;
            }

            float hard = Tanh(j / (Mathf.Max(1f, mass) * 3f * SizeSpeed()));
            float w = 2f * Mathf.PI * ArmKnockFrequency;
            // (A critically damped spring kicked at e·ω·x from rest peaks at x.)
            float kick = (float)System.Math.E * w;
            Vector3 dir = impulse / j;
            armKnockV[a] += dir * (MaxArmKnock * Body.SizeFactor * hard * kick);
            var hand = Anim.Part(region == BodyRegion.ArmR ? PartId.HandR : PartId.HandL);
            Vector3 lever = point - hand.position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                armTurnV[a] += axis.normalized * (MaxArmTurn * hard * kick);
            }
        }

        /// <summary>How far back along its path a stopped blow plays at most (share of the blow).</summary>
        public const float RewindMost = 0.12f;

        /// <summary>How far a stopped blow's weapon springs back (metres at medium size): a light stop, a hard one.</summary>
        public const float ReboundLight = 0.15f, ReboundHard = 0.25f;

        /// <summary>A blow stopped by a body or a shield: the weapon arm's pose springs back along the push it met and comes back, as the blow fades into the guard.</summary>
        private void Rebound(Vector3 push, Vector3 point, bool off = false)
        {
            // (Milestone 10: an off-hand blow's arm springs back, not the weapon arm.)
            int k = off ? 1 : 0;
            float j = push.magnitude;
            float mass = 0f;
            foreach (var id in ArmChain(off ? BodyRegion.ArmL : BodyRegion.ArmR))
            {
                var body = Rig.Part(id).body;
                mass += body != null ? body.mass : 0f;
            }

            var held = off ? Loadout?.Off : Loadout?.Main;
            if (held != null)
            {
                mass += held.massKg * Loadout.Scale * Loadout.Scale * Loadout.Scale;
            }

            float hard = Tanh(j / (Mathf.Max(1f, mass) * 3f * SizeSpeed()));
            float w = 2f * Mathf.PI * ArmKnockFrequency;
            float kick = (float)System.Math.E * w;
            Vector3 dir = push / j;
            armKnockV[k] += dir * (Mathf.Lerp(ReboundLight, ReboundHard, hard) * Body.SizeFactor * kick);
            Vector3 lever = point - Anim.Part(off ? PartId.HandL : PartId.HandR).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                armTurnV[k] += axis.normalized * (MaxArmTurn * hard * kick);
            }
        }

        private static PartId[] ArmChain(BodyRegion region) => region == BodyRegion.ArmR
            ? new[] { PartId.HandR, PartId.ForearmR, PartId.UpperArmR }
            : new[] { PartId.HandL, PartId.ForearmL, PartId.UpperArmL };

        private static void Spring(ref Vector3 x, ref Vector3 v, float hz, float damping, float dt)
        {
            float w = 2f * Mathf.PI * hz;
            v += (-w * w * x - 2f * damping * w * v) * dt;
            x += v * dt;
        }

        /// <summary>
        /// A blow's push on the pose: the hips go with it, the upper body turns about the hips the
        /// way the blow levers it (bent back, twisted if it lands off centre), the head snaps if it
        /// was struck. Heavier hits saturate rather than throw the body about.
        /// </summary>
        private void Recoil(Vector3 impulse, Vector3 point, PartId part, float scale)
        {
            float j = impulse.magnitude * scale;
            if (j < 1e-4f)
            {
                return;
            }

            float leg = Body.LegLength;
            float mass = Mathf.Max(1f, Body.Mass);
            Vector3 dir = impulse / impulse.magnitude;
            Vector3 flat = new Vector3(dir.x, 0f, dir.z);
            float w = 2f * Mathf.PI * ReactFrequency;

            // The hips: pushed with the blow, the knees giving a little.
            float push = MaxShift * leg * Tanh(1.2f * j / (mass * Mathf.Sqrt(leg)));
            reactShiftV += (flat.normalized * push + Vector3.down * (0.35f * push)) * (Kick * w);

            // The upper body: turned about the hips by the blow's lever (bent away, twisted).
            Vector3 lever = point - Anim.Part(PartId.Pelvis).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                float leverage = Mathf.Clamp(lever.magnitude / leg, 0.2f, 1.4f);
                float lean = MaxLean * Tanh(1.5f * j * leverage / (mass * leg));
                reactLeanV += axis.normalized * (lean * Kick * w);
            }

            // The head: a blow to it snaps it back and round.
            if (part == PartId.Head)
            {
                Vector3 headLever = point - Anim.Part(PartId.Head).position + Vector3.up * (0.1f * leg);
                Vector3 headAxis = Vector3.Cross(headLever, dir);
                if (headAxis.sqrMagnitude > 1e-8f)
                {
                    float snap = MaxHeadSnap * Tanh(6f * j / (mass * leg));
                    reactHeadV += headAxis.normalized * (snap * Kick * 2f * Mathf.PI * HeadFrequency);
                }
            }
        }

        private static float Tanh(float x)
        {
            float e = Mathf.Exp(-2f * Mathf.Abs(x));
            return Mathf.Sign(x) * (1f - e) / (1f + e);
        }

        // ---- The sweep ---------------------------------------------------------------------------

        private void BeginSweep()
        {
            struckThisBlow.Clear();
            momentumLeft = 1f;
            hasLastSweep = false;
        }

        /// <summary>
        /// What strikes in a blow, as PhysX has it now: the segment swept (from near the hand out to the
        /// tip) and the striking point. The weapon in the hand; milestone 10: a jab's fist, from the
        /// wrist to the knuckles. False if there is nothing to strike with.
        /// </summary>
        public bool StrikingPart(MoveState move, out Vector3 grip, out Vector3 tip, out Vector3 strike)
        {
            grip = tip = strike = Vector3.zero;
            var weapon = move?.Weapon;
            if (weapon == null)
            {
                return false;
            }

            if (move.Move.hand == MoveHand.Off && weapon.IsShield)
            {
                // (Milestone 10: a bash: the shield from its grip out to the middle of its face.)
                if (OffWeapon == null)
                {
                    return false;
                }

                grip = OffWeapon.PhysicalPoint(Vector3.zero);
                tip = strike = OffWeapon.PhysicalPoint(weapon.faceCenter);
                return true;
            }

            if (move.Move.hand == MoveHand.Off && weapon.kind == WeaponKind.Fist)
            {
                if (Rig.IsSevered(PartId.HandL))
                {
                    return false;
                }

                var hand = Rig.Part(PartId.HandL);
                grip = hand.PhysicalPoint(hand.transform.position);
                tip = strike = hand.PhysicalPoint(hand.transform.TransformPoint(OffHandPath.KnuckleLocal(Anim, Body.SizeFactor)));
                return true;
            }

            if (MainWeapon == null)
            {
                return false;
            }

            if (move.Move.strikeWith == StrikeWith.Butt)
            {
                // (Milestone 10: a butt strike: from the grip down to the butt end below the fist.)
                grip = MainWeapon.PhysicalPoint(Vector3.zero);
                tip = strike = MainWeapon.PhysicalPoint(weapon.Butt);
                return true;
            }

            float from = weapon.kind == WeaponKind.Club ? 0.35f : 0.1f;
            grip = MainWeapon.PhysicalPoint(weapon.tip * from);
            tip = MainWeapon.PhysicalPoint(weapon.tip);
            strike = MainWeapon.PhysicalPoint(weapon.StrikePoint(move.Move.strikeWith));
            return true;
        }

        /// <summary>
        /// The weapon as PhysX has it now, swept from where it was last step, from the start of the
        /// active window to the end of the follow-through: the first body part or shield it meets
        /// is struck (any unit: friendly fire counts) and stops the blow there.
        /// </summary>
        private void Sweep(MoveState move, float dt)
        {
            var weapon = move.Weapon;
            if (weapon == null || dt <= 0f || !StrikingPart(move, out Vector3 grip, out Vector3 tip, out Vector3 strike))
            {
                return;
            }

            bool had = hasLastSweep;
            Vector3 lastG = lastGrip, lastT = lastTip, lastS = lastStrike;
            lastGrip = grip;
            lastTip = tip;
            lastStrike = strike;
            hasLastSweep = true;
            // From the start of the active window to the end of the follow-through: the weapon can't
            // pass through a body on the way out either.
            bool live = move.Progress >= move.Move.activeWindow.x && move.Blend > 0.3f;
            if (!had || !DealsHits || !live || momentumLeft <= 0f)
            {
                return;
            }

            // (Capped: a weapon arm switching from kinematic can jump in one step.)
            float speed = Mathf.Min(Vector3.Distance(strike, lastS) / dt, 16f);
            if (speed < weapon.minHitSpeed)
            {
                return;
            }

            Vector3 direction = strike - lastS;
            // (A shield's bash sweeps most of its face.)
            float radius = Mathf.Max(0.01f, (move.Move.strikeWith == StrikeWith.Face ? 0.7f * weapon.radius : weapon.sweepRadius) * Loadout.Scale);
            float travel = Mathf.Max(Vector3.Distance(grip, lastG), Vector3.Distance(tip, lastT));
            int samples = Mathf.Clamp(Mathf.CeilToInt(travel / (1.5f * radius)), 1, 10);
            var found = new List<(UnitController unit, BodyPart part, Weapon shield, Collider collider, float order)>();
            // A swing's weapon turns about its pivot: between steps it is swept round the arc, not
            // along the straight chord (at 20 m/s that would cut the corner by several centimetres).
            bool arc = move.Generated && move.Move.shape == MoveShape.Swing;
            Vector3 pivot = arc ? move.PivotAt(move.Progress) : Vector3.zero;
            for (int i = 1; i <= samples; i++)
            {
                float s = i / (float)samples;
                Vector3 a = arc ? pivot + Vector3.Slerp(lastG - pivot, grip - pivot, s) : Vector3.Lerp(lastG, grip, s);
                Vector3 b = arc ? pivot + Vector3.Slerp(lastT - pivot, tip - pivot, s) : Vector3.Lerp(lastT, tip, s);
                Vector3 mid = 0.5f * (a + b);
                found.Clear();
                foreach (var collider in Physics.OverlapCapsule(a, b, radius, ~0, QueryTriggerInteraction.Collide))
                {
                    var shield = collider.GetComponentInParent<Weapon>();
                    if (shield != null && shield.definition != null && shield.definition.IsShield && shield.Owner != null && shield.Owner != this)
                    {
                        found.Add((shield.Owner, null, shield, collider, AlongBlade(collider, a, b) - 0.05f));
                        continue;
                    }

                    if (shield != null && shield.Owner != null && shield.Owner != this && shield.Owner.Parrying && shield.InMainHand)
                    {
                        // (Milestone 11: a weapon set across the blow: a parry, met like a shield.)
                        found.Add((shield.Owner, null, shield, collider, AlongBlade(collider, a, b) - 0.05f));
                        continue;
                    }

                    if (shield != null)
                    {
                        continue;   // a weapon not set against the blow: they pass (clashes in passing are for later)
                    }

                    var part = collider.GetComponentInParent<BodyPart>();
                    if (part == null || part.Owner == null || part.Owner == this || part.partCollider != collider)
                    {
                        continue;
                    }

                    found.Add((part.Owner, part, null, collider, AlongBlade(collider, a, b)));
                }

                // In this stretch, nearest the hand first: the blade turns about the hand, so a body
                // nearer the hand is in the way of everything further out (a shield before the arm behind it).
                found.Sort((x, y) => x.order.CompareTo(y.order));
                foreach (var f in found)
                {
                    if (!f.unit.IsAlive || struckThisBlow.Contains(f.unit))
                    {
                        continue;
                    }

                    struckThisBlow.Add(f.unit);
                    var report = Strike(move, f.unit, f.part, f.shield, f.collider.ClosestPoint(mid), speed, direction, a, b);
                    if (report.Result.StopsSwing)
                    {
                        momentumLeft = 0f;
                        // Back to where the weapon met the body: the start of this stretch of the sweep.
                        move.Time = Mathf.Max(0f, move.Time - lastAdvance * (1f - (i - 1) / (float)samples));
                        StopSwing(move, report);
                        return;
                    }

                    // Cleaved through: the swing goes on, slowed by what it gave up, and the body it
                    // went through is shoved out of its way.
                    momentumLeft = report.Result.MomentumLeft;
                    move.TimeScale = Mathf.Min(move.TimeScale, Mathf.Max(0.3f, report.Result.MomentumLeft));
                    if (report.Struck.IsAlive)
                    {
                        Vector3 along = direction;
                        along.y = 0f;
                        report.Struck.Shove(along.sqrMagnitude > 1e-8f ? along.normalized : transform.forward,
                            CleaveShove * speed * report.Result.MomentumLeft * DamageRules.Transfer(move.Move.strikeWith), this);
                    }
                }
            }
        }

        /// <summary>How far out along the blade (0 at a, the hand's end, to 1 at b, the tip) a collider touches it.</summary>
        private static float AlongBlade(Collider collider, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float length = Mathf.Max(1e-6f, ab.sqrMagnitude);
            float T(Vector3 p) => Mathf.Clamp01(Vector3.Dot(p - a, ab) / length);
            Vector3 near = collider.ClosestPoint(0.5f * (a + b));
            near = collider.ClosestPoint(a + ab * T(near));
            return T(near);
        }

        /// <summary>Share of the weapon's speed (times what it keeps, times how much of its weight lands) a cleaved body is shoved along at.</summary>
        private const float CleaveShove = 0.15f;

        /// <summary>
        /// A swing went through this unit: it is thrown along with the weapon, out of the arc (off
        /// balance at least; a heavy swing knocks it down or launches it).
        /// </summary>
        public void Shove(Vector3 direction, float speed, UnitController by = null)
        {
            if (!IsAlive)
            {
                return;
            }

            if (State == UnitState.Standing || State == UnitState.Stumbling)
            {
                Recoil(direction * (speed * Body.Mass * 0.25f), Anim.Part(PartId.Chest).position, PartId.Chest, 1.6f);
                StartRecovery(BodyRegion.Torso, 0.2f, 0.9f);
                StartRecovery(BodyRegion.Head, 0.3f, 0.9f);
                if (CurrentMove != null)
                {
                    CancelMove(CurrentMove, "knocked aside");
                }
            }

            Knock(direction * speed, true, false, "cleaved through", by);
        }

        private HitReport Strike(MoveState move, UnitController struck, BodyPart part, Weapon shield, Vector3 point, float speed, Vector3 direction,
            Vector3 bladeFrom, Vector3 bladeTo)
        {
            bool parried = shield != null && (shield.definition == null || !shield.definition.IsShield);
            var partId = parried ? PartId.HandR : shield != null ? PartId.HandL : part.id;
            bool armGuard = shield == null && struck.ArmGuarding(partId);
            var result = DamageRules.Resolve(move.Weapon, move.Move.strikeWith, Loadout.Scale, speed, direction,
                Body.Mass, struck.Body.Mass, partId, shield != null, momentumLeft, struck.Health, struck.Poise, struck.profile.maxPoise, parried, armGuard);
            string kind = armGuard ? "its guarding arm"
                : parried ? "parried by the " + (shield.definition != null ? shield.definition.kind.ToString().ToLowerInvariant() : "weapon")
                : shield != null ? "blocked by the shield"
                : ReferenceEquals(struck, move.Target) && partId == AimPartInfo.BodyPartOf(move.Part) ? "the part aimed at"
                : ReferenceEquals(struck, move.Target) ? "the target, another part"
                : struck.Team == Team ? "an ally (friendly fire)" : "someone in the way";
            var report = new HitReport
            {
                Attacker = this, Struck = struck, Part = partId, Shield = shield != null && !parried, Parried = parried, ArmGuard = armGuard, Move = move, Point = point, Speed = speed,
                Result = result, Kind = kind,
            };
            // Milestone 5: a sword's edge crossing a joint, fast and hard enough, cuts the limb off
            // there (judged before the blow moves anything); the blade goes on through.
            PartId? cut = shield == null
                ? struck.SeverCut(part, move.Weapon, move.Move.strikeWith, Loadout.Scale, speed, direction, bladeFrom, bladeTo, point, result.Damage, out report.SeverMiss)
                : null;
            struck.TakeHit(report);
            if (cut.HasValue && struck.Sever(cut.Value, report.Result.Impulse, report))
            {
                report.Severed = cut;
                var r = report.Result;
                r.StopsSwing = false;
                r.MomentumLeft = Mathf.Max(r.MomentumLeft, 0.7f * Mathf.Clamp01(momentumLeft));
                report.Result = r;
            }

            if (!report.Blocked)
            {
                Rhythm?.NoteLanded();
            }

            // Hit stop: the attacker and the struck stand still a moment, longer for a heavier blow.
            float stop = Mathf.Clamp(move.Move.hitStop * (0.6f + result.Impulse.magnitude / 40f), 0.04f, 0.1f);
            HitStop = Mathf.Max(HitStop, stop);
            struck.HitStop = Mathf.Max(struck.HitStop, stop);
            // The attacker feels it too: the blow's push comes back up the weapon arm.
            Recoil(-result.Impulse, MainWeapon != null ? MainWeapon.transform.position : point, PartId.HandR, shield != null ? (parried ? 0.45f : 0.35f) : 0.2f);
            Hit?.Invoke(report);
            return report;
        }

        /// <summary>The swing met something it can't go through: the blow ends and the weapon arm recoils.</summary>
        private void StopSwing(MoveState move, HitReport report)
        {
            // Milestone 9c: the blow backs out of what it met along its path, but only a little (it
            // could otherwise carry the arm back into its wind-up), and fades into the guard; the
            // weapon springs back along the push it met, critically damped.
            move.Rewind = true;
            move.RewindFloor = Mathf.Max(0f, move.Time - RewindMost * move.Duration);
            if (report.Result.Impulse.sqrMagnitude > 1e-8f)
            {
                Rebound(-report.Result.Impulse, report.Point, move.Move.hand == MoveHand.Off);
            }

            if (report.Blocked)
            {
                Tempo?.NoteBlocked();
            }

            CancelMove(move, "stopped by " + report.Struck.name + (report.Shield ? "'s shield" : report.Parried ? "'s parry" : ""));
        }

        // ---- Being struck ------------------------------------------------------------------------

        private void TakeHit(HitReport hit)
        {
            // (A body on the ground, dead or alive, is let fly by a blow: milestone 10's speed limit.)
            NoteGroundStruck();
            if (!IsAlive)
            {
                return;
            }

            NoteStruck(hit);
            var r = hit.Result;
            Health = Mathf.Max(0f, Health - r.Damage);
            Rhythm?.NoteHit(r.Damage / Mathf.Max(1f, profile.maxHealth));
            Tempo?.NoteHit(r.PoiseDamage / Mathf.Max(1f, profile.maxPoise));
            Poise -= r.PoiseDamage;
            if (hit.Blocked || hit.ArmGuard)
            {
                NoteGuarded(hit, ref r);   // (milestone 11: its energy, and a guard broken under a long barrage)
            }

            LastReaction = r.Reaction;

            // The struck part gives (held loosely for a moment, the pins ramp back), the regions
            // joined to it less. The pose carries the visible reaction (Recoil below), so nothing
            // goes fully limp: a loose arm would swing its weapon about.
            var region = AuthorityController.RegionOf(hit.Part);
            bool hard = r.Reaction == Reaction.Stagger || r.Reaction == Reaction.Flinch;
            float hold = r.Reaction == Reaction.Stagger ? 0.9f : r.Reaction == Reaction.Flinch ? 0.6f : 0.4f;
            bool armStruck = (region == BodyRegion.ArmL || region == BodyRegion.ArmR) && (State == UnitState.Standing || State == UnitState.Stumbling);
            // (A struck arm softens to about 2 Hz and firms up over 0.4 s: its pose carries the knock.)
            StartRecovery(region, armStruck ? ArmRecoverShare : hard ? 0.2f : 0.3f, armStruck ? ArmRecoverTime : hold);
            foreach (var neighbour in Neighbours(region))
            {
                StartRecovery(neighbour, hard ? 0.45f : 0.65f, 0.7f * hold);
            }

            // The impulse goes in at the part and the part it hangs from, each taking what it can
            // without flying off on its own (the pins pull them back quickly); the rest moves the
            // whole unit: that is what reads as weight.
            Vector3 left = r.Impulse * r.PartShare;
            float taken = 0f;
            bool armHeld = (region == BodyRegion.ArmL || region == BodyRegion.ArmR) && (State == UnitState.Standing || State == UnitState.Stumbling);
            float carried = hit.Shield && region == BodyRegion.ArmL && Loadout.Off != null ? Loadout.Off.massKg * Loadout.Scale * Loadout.Scale * Loadout.Scale
                : hit.Parried && region == BodyRegion.ArmR && Loadout.Main != null ? Loadout.Main.massKg * Loadout.Scale * Loadout.Scale * Loadout.Scale : 0f;
            if (armHeld && r.Impulse.sqrMagnitude > 1e-8f)
            {
                // Milestone 9c: a standing unit's arm (and the shield it holds) takes the blow as one:
                // the hand, forearm and upper arm all move by the same small amount (by their mass and
                // what the hand carries), at most MaxArmPartSpeed; the pose shows the knock.
                float chain = carried;
                foreach (var id in ArmChain(region))
                {
                    var body = Rig.Part(id).body;
                    chain += body != null ? body.mass : 0f;
                }

                Vector3 change = Vector3.ClampMagnitude(left / Mathf.Max(0.1f, chain), MaxArmPartSpeed * SizeSpeed());
                foreach (var id in ArmChain(region))
                {
                    var body = Rig.Part(id).body;
                    if (body != null && !body.isKinematic)
                    {
                        body.AddForce(change * body.mass, ForceMode.Impulse);
                        taken += change.magnitude * body.mass;
                    }
                }

                taken += change.magnitude * carried;
                KnockArm(region, r.Impulse, hit.Point, carried);
            }
            else if ((region != BodyRegion.Legs || State == UnitState.Down) && r.Impulse.sqrMagnitude > 1e-8f)
            {
                var id = hit.Part;
                float cap = MaxPartSpeed * SizeSpeed();
                for (int link = 0; link < 2 && left.sqrMagnitude > 1e-8f; link++)
                {
                    var body = Rig.Part(id).body;
                    if (body != null)
                    {
                        Vector3 here = Vector3.ClampMagnitude(left, body.mass * cap);
                        if (body.isKinematic)
                        {
                            body.isKinematic = false;   // (the authority layer sets it again next step)
                        }

                        body.AddForceAtPosition(here, link == 0 ? hit.Point : body.worldCenterOfMass, ForceMode.Impulse);
                        taken += here.magnitude;
                        left -= here;
                    }

                    if (id == PartId.Pelvis || !PartInfo.HasParent(id))
                    {
                        break;
                    }

                    id = PartInfo.Parent(id);
                    cap *= 0.5f;   // the part it hangs from, less on its own
                }
            }

            float impulse = r.Impulse.magnitude;
            r.PartShare = impulse > 1e-4f ? taken / impulse : 0f;
            hit.Result = r;

            // The pose throws the body back from the blow; harder reactions more.
            float react = r.Reaction == Reaction.Stagger ? 1.6f : r.Reaction == Reaction.Flinch ? 1.25f : 1f;
            if (State == UnitState.Standing || State == UnitState.Stumbling)
            {
                Recoil(r.Impulse, hit.Point, hit.Part, react);
            }

            Vector3 push = (r.Impulse - r.Impulse.normalized * taken) / Mathf.Max(1f, Body.Mass);
            push.y = 0f;
            if (r.Reaction == Reaction.Stagger)
            {
                push *= 1.5f;
            }

            if (r.Reaction == Reaction.Killed)
            {
                Die(hit);
                return;
            }

            if (r.Reaction == Reaction.Stagger)
            {
                Poise = profile.maxPoise;   // poise restored once broken
            }

            if ((r.Reaction == Reaction.Stagger || r.Reaction == Reaction.Flinch) && CurrentMove != null)
            {
                CancelMove(CurrentMove, r.Reaction == Reaction.Stagger ? "staggered" : "flinched");
            }

            // Milestone 4: the push slides it, knocks it off balance, down, or off its feet. A broken
            // poise always sends it stumbling.
            Knock(push, r.Reaction == Reaction.Stagger, r.Blocked, hit.Impact ? "a body thrown into it" : null, hit.Attacker);
        }

        private void StartRecovery(BodyRegion region, float fromShare, float seconds)
        {
            if (region == BodyRegion.Legs)
            {
                return;
            }

            int r = (int)region;
            // A new hit while still recovering keeps the looser of the two.
            if (recoverLeft[r] > 0f)
            {
                float t = 1f - recoverLeft[r] / Mathf.Max(1e-3f, recoverTime[r]);
                fromShare = Mathf.Min(fromShare, Mathf.Lerp(recoverFrom[r], 1f, t));
            }

            recoverFrom[r] = fromShare;
            recoverTime[r] = seconds;
            recoverLeft[r] = seconds;
        }

        private static IEnumerable<BodyRegion> Neighbours(BodyRegion region)
        {
            switch (region)
            {
                case BodyRegion.Torso:
                    yield return BodyRegion.Head;
                    yield return BodyRegion.ArmL;
                    yield return BodyRegion.ArmR;
                    break;
                case BodyRegion.Head:
                case BodyRegion.ArmL:
                case BodyRegion.ArmR:
                case BodyRegion.Legs:
                    yield return BodyRegion.Torso;
                    break;
            }
        }

        private void Die(HitReport hit)
        {
            IsAlive = false;
            Health = 0f;
            StopGait();
            if (CurrentMove != null)
            {
                CancelMove(CurrentMove, "killed");
            }

            StopRhythm();

            incoming = null;
            incomingAttacker = null;
            DropParry();
            threats.Clear();
            BlockBlend = 0f;
            Pose.BlockBlend = 0f;
            Knockback = Vector3.zero;
            StumbleVelocity = Vector3.zero;
            // Every part to physics, with the blow's whole impulse in the part struck.
            SetAuthority(null);
            foreach (var part in Rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.isKinematic = false;
                }
            }

            // The struck part already took its share (TakeHit); what it didn't take moves the whole
            // body, spread by mass, so it falls away from the blow rather than flying apart.
            Vector3 rest = hit.Result.Impulse * (1f - hit.Result.PartShare);
            float mass = Mathf.Max(1f, Body.Mass);
            Vector3 velocity = Vector3.ClampMagnitude(rest / mass, 1.5f * MaxKnockback * SizeSpeed());
            foreach (var part in Rig.Parts)
            {
                if (part.body != null && !Rig.IsSevered(part.id))
                {
                    part.body.AddForce(velocity, ForceMode.VelocityChange);
                }
            }

            Died?.Invoke(this, hit);
        }

        /// <summary>Ends a blow early (stopped, flinched, staggered, killed): back to the guard, the feet settle by themselves.</summary>
        private void CancelMove(MoveState move, string why)
        {
            if (CurrentMove != move)
            {
                return;
            }

            move.Cancelled = why;
            CurrentMove = null;
            Stance.Hold = false;
            // The pose lets go of the blow over a moment from where it stopped, rather than snapping
            // back to guard; feet out in the lunge step back home.
            cutShort = move;
            if (move.SteppedOut && !move.SteppedBack)
            {
                move.SteppedBack = true;
                int lead = Stance.Lead;
                Stance.StepTo(lead, Stance.HomeWorld(lead, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(lead), 0.28f);
            }

            if ((move.RearOut || move.Move.passingStep && move.SteppedOut) && !move.RearBack)
            {
                move.RearBack = true;
                int rear = 1 - Stance.Lead;
                Stance.StepTo(rear, Stance.HomeWorld(rear, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(rear), 0.32f);
            }

            Tempo?.NoteBlowEnded();
            MoveEnded?.Invoke(this, move);
        }

        /// <summary>Drops a fading blow at once (the unit was moved).</summary>
        private void DropCutShort()
        {
            if (cutShort != null && Pose.Move == cutShort)
            {
                Pose.Move = null;
            }

            cutShort = null;
        }

        /// <summary>A cut-short blow fading out of the pose (gone once a new blow starts).</summary>
        private void FadeCutShort(float dt)
        {
            if (cutShort == null)
            {
                return;
            }

            if (Pose.Move != cutShort)
            {
                cutShort = null;
                return;
            }

            if (cutShort.Rewind)
            {
                cutShort.Time = Mathf.Max(cutShort.RewindFloor, cutShort.Time - dt * ReboundSpeed);
            }

            cutShort.Fade -= dt / CutShortFade;
            if (cutShort.Fade <= 0f)
            {
                Pose.Move = null;
                cutShort = null;
            }
        }
    }
}
