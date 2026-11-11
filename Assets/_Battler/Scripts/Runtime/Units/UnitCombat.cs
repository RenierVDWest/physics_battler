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
        private bool hasLastSweep;
        private System.Random combatRandom;
        private MoveState cutShort;
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

        /// <summary>How far the body takes up space around its hips (metres, horizontally).</summary>
        public float BodyRadius => 0.58f * Body.ShoulderWidth;

        // ---- Recoil ------------------------------------------------------------------------------

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

        private static PartId[] ArmChain(BodyRegion region) => region == BodyRegion.ArmR
            ? new[] { PartId.HandR, PartId.ForearmR, PartId.UpperArmR }
            : new[] { PartId.HandL, PartId.ForearmL, PartId.UpperArmL };

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
    }
}
