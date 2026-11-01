using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>Whether a unit is on its feet.</summary>
    public enum UnitState
    {
        /// <summary>On its feet, fighting.</summary>
        Standing,

        /// <summary>Knocked off balance: carried along by the blow, catching itself with quick steps.</summary>
        Stumbling,

        /// <summary>Knocked down or launched: a ragdoll until it comes to rest.</summary>
        Down,

        /// <summary>Getting up from where it lay.</summary>
        GettingUp,
    }

    /// <summary>
    /// Milestone 4: what a hard blow does to a unit's balance. Every blow's push (the impulse its
    /// struck part didn't take, over the unit's mass) decides how far it goes, against the unit's
    /// size (a bigger body's speeds go with the square root of its size):
    ///
    ///  - below <see cref="StumbleAt"/>: the unit slides back and its feet keep up (milestone 3);
    ///  - a stumble (and always when its poise breaks): carried along in the blow's direction, the
    ///    upper body leaning with it and the arms thrown out, it catches itself with quick steps,
    ///    each one taking some of the speed, and ends up where the blow sent it;
    ///  - from <see cref="KnockdownAt"/>: knocked down, a ragdoll with the blow's speed, the upper
    ///    body most, so it topples over its feet;
    ///  - from <see cref="LaunchAt"/>: launched, the whole body thrown off its feet. A body thrown
    ///    into others hurts them (and itself), and they react the same way.
    ///
    /// A body down comes to rest, then gets up (<see cref="GetUp"/>) from how it lies, and is sent
    /// back onto the nearest free tile (<see cref="NeedsSpot"/>).
    /// </summary>
    public partial class UnitController
    {
        /// <summary>Something happened to a unit's balance (stumbled, knocked down, launched, got up), for the log.</summary>
        public static event Action<UnitController, string> BalanceChanged;

        public UnitState State { get; private set; } = UnitState.Standing;

        /// <summary>On its feet and in control: it may walk, turn and strike.</summary>
        public bool CanAct => IsAlive && State == UnitState.Standing;

        /// <summary>On the ground, or getting up.</summary>
        public bool IsDown => State == UnitState.Down || State == UnitState.GettingUp;

        /// <summary>Just got up: it goes back onto the nearest free tile before it fights again (the game loop clears this).</summary>
        public bool NeedsSpot { get; set; }

        /// <summary>A stumble's speed (m/s, world, horizontal).</summary>
        public Vector3 StumbleVelocity { get; private set; }

        /// <summary>How the last get-up started.</summary>
        public GetUp.Kind LastGetUp { get; private set; }

        /// <summary>Milestone 9c: the lowest point of what the hands hold over the ground in this step's pose (metres; negative: in it).</summary>
        public float WeaponGroundGap => State == UnitState.GettingUp ? getUp.WeaponGroundGap : Pose.WeaponGroundGap;

        /// <summary>Which way the shield faces getting up (world; zero without one, or not getting up).</summary>
        public Vector3 GetUpShieldFace => State == UnitState.GettingUp ? getUp.ShieldFace : Vector3.zero;

        /// <summary>When the standing pose starts to take over a get-up, and how long it lasts (seconds).</summary>
        public float GetUpStandFrom => getUp.StandFrom;
        public float GetUpBlendIn => getUp.BlendIn;
        public float GetUpDuration => getUp.Duration;

        /// <summary>Seconds in the current state.</summary>
        public float StateTime { get; private set; }

        public int TimesDown { get; private set; }
        public int TimesUp { get; private set; }

        /// <summary>Pushes (m/s at medium size) from which a blow makes a unit stumble, knocks it down, launches it.</summary>
        public const float StumbleAt = 0.8f, KnockdownAt = 2.2f, LaunchAt = 4f;

        /// <summary>The slowest a broken poise sends a unit stumbling (m/s, medium size).</summary>
        private const float OffBalanceSpeed = 1.1f;

        /// <summary>Share of a stumble's speed left after each catch step, and its slowing in between (seconds).</summary>
        private const float CatchStep = 0.55f, StumbleDrag = 1.4f;

        /// <summary>The fastest a unit is launched (m/s, medium size, horizontal), and how much of it goes up.</summary>
        private const float MaxLaunch = 6.5f, LaunchLift = 0.35f;

        /// <summary>Least time down (seconds, medium size), the most, and the pelvis's speed it must stay under for <see cref="SettleTime"/> to get up.</summary>
        private const float MinDown = 1.1f, MaxDown = 4f, SettleSpeed = 0.25f, SettleTime = 0.3f;

        /// <summary>Lean of a stumbling body (degrees per m/s at medium size, and the most).</summary>
        private const float LeanPerSpeed = 14f, MaxStumbleLean = 30f;

        /// <summary>A part of a body down hits another unit at this closing speed or more (m/s).</summary>
        private const float ImpactSpeed = 1.5f;

        /// <summary>Damage per N·s of a body impact; the thrown body takes half of it too.</summary>
        public const float ImpactDamage = 0.15f;

        /// <summary>Seconds before the same thrown body can hit the same unit again.</summary>
        private const float ImpactCooldown = 0.5f;

        private GetUp getUp;
        private readonly PoseBuffer lying = new PoseBuffer();
        private readonly PoseBuffer standing = new PoseBuffer();
        private readonly Dictionary<UnitController, float> impactsUntil = new Dictionary<UnitController, float>();
        private float downCalm;
        private UnitController knockedBy;
        private readonly Vector3[] preStep = new Vector3[PartInfo.Count];
        private bool launched;
        private int stumbleSteps;
        private Vector3 stumbleFrom;
        private Vector3 topple, toppleShift;
        private float flail;

        private void InitializeBalance()
        {
            getUp = new GetUp(transform, Anim, Body, Stance);
            Stance.Landed += OnFootLanded;
        }

        private void SetState(UnitState state)
        {
            State = state;
            StateTime = 0f;
        }

        /// <summary>Back on its feet at once (the sandbox moved it).</summary>
        private void ResetBalance()
        {
            SetState(UnitState.Standing);
            StumbleVelocity = Vector3.zero;
            topple = toppleShift = Vector3.zero;
            flail = 0f;
            Pose.Topple = Pose.ToppleShift = Vector3.zero;
            Pose.Flail = 0f;
            Stance.Hold = false;
            Stance.Drift = Vector3.zero;
            Stance.Urgency = 0f;
            NeedsSpot = false;
        }

        /// <summary>
        /// Knocks the unit by a speed (m/s, world; up is ignored): it slides, stumbles, falls or is
        /// launched as hard as that is for its size. <paramref name="offBalance"/>: its poise broke,
        /// so it stumbles at least. A blow on a shield never knocks it down.
        /// </summary>
        /// <param name="by">Who knocked it (a body down doesn't strike the one who put it there: it is carried along by their swing).</param>
        public void Knock(Vector3 push, bool offBalance = false, bool blocked = false, string cause = null, UnitController by = null)
        {
            if (!IsAlive || !initialized)
            {
                return;
            }

            push.y = 0f;
            float k = SizeSpeed();
            switch (State)
            {
                case UnitState.Down:
                    // Already on the ground: the blow moves the body.
                    PushBody(push * 0.6f, 0f, 0f, 0f);
                    downCalm = 0f;
                    return;
                case UnitState.GettingUp:
                    if (push.magnitude >= StumbleAt * k || offBalance)
                    {
                        knockedBy = by;
                        KnockDown(push, false, cause);
                    }

                    return;
            }

            Vector3 total = push + (State == UnitState.Stumbling ? StumbleVelocity : Knockback);
            float speed = total.magnitude / k;
            if (blocked && speed >= KnockdownAt)
            {
                total = total * ((KnockdownAt - 0.01f) / speed);
                speed = KnockdownAt - 0.01f;
            }

            if (speed >= KnockdownAt)
            {
                knockedBy = by;
                KnockDown(total, speed >= LaunchAt, cause);
            }
            else if (speed >= StumbleAt || offBalance || State == UnitState.Stumbling)
            {
                Stumble(total, offBalance, cause);
            }
            else
            {
                Knockback = Vector3.ClampMagnitude(Knockback + push, MaxKnockback * k);
            }
        }

        // ---- Stumbling ---------------------------------------------------------------------------

        private void Stumble(Vector3 velocity, bool offBalance, string cause)
        {
            float k = SizeSpeed();
            if (offBalance && velocity.magnitude < OffBalanceSpeed * k)
            {
                Vector3 dir = velocity.sqrMagnitude > 1e-6f ? velocity.normalized : -transform.forward;
                velocity = dir * (OffBalanceSpeed * k);
            }

            bool fresh = State != UnitState.Stumbling;
            StopRhythm();
            ReleaseGait();   // (the walk fades: the catch steps have the feet)
            StumbleVelocity = velocity;
            Knockback = Vector3.zero;
            if (CurrentMove != null)
            {
                CancelMove(CurrentMove, "knocked off balance");
            }

            DropBlock();
            if (fresh)
            {
                SetState(UnitState.Stumbling);
                stumbleSteps = 0;
                stumbleFrom = transform.position;
                BalanceChanged?.Invoke(this, $"stumble: knocked off balance at {velocity.magnitude:0.00} m/s toward {Heading(velocity)}"
                                             + (cause != null ? $" ({cause})" : ""));
            }
        }

        private void OnFootLanded(int foot, Vector3 at)
        {
            if (State == UnitState.Stumbling)
            {
                // A catch step: the foot stops some of the fall.
                StumbleVelocity *= CatchStep;
                stumbleSteps++;
            }
        }

        /// <summary>The stumble carries the unit along, slowing; it is over once it is slow and both feet are down.</summary>
        private void UpdateStumble(float dt)
        {
            transform.position += StumbleVelocity * dt;
            StumbleVelocity *= Mathf.Exp(-dt / StumbleDrag);
            float k = SizeSpeed();
            bool settled = StumbleVelocity.magnitude < 0.15f * k && !Stance.AnyStepping && StateTime > 0.3f;
            if (settled || StateTime > 3f)
            {
                Vector3 moved = transform.position - stumbleFrom;
                moved.y = 0f;
                StumbleVelocity = Vector3.zero;
                SetState(UnitState.Standing);
                BalanceChanged?.Invoke(this, $"regained balance after {stumbleSteps} catch step{(stumbleSteps == 1 ? "" : "s")}, {moved.magnitude:0.00} m toward {Heading(moved)}");
            }
        }

        /// <summary>The pose leans with a stumble and the arms go out; both ease in and out.</summary>
        private void UpdateTopple(float dt)
        {
            float k = SizeSpeed();
            Vector3 v = State == UnitState.Stumbling ? StumbleVelocity : Vector3.zero;
            Vector3 wantedLean = Vector3.zero, wantedShift = Vector3.zero;
            float wantedFlail = 0f;
            if (v.sqrMagnitude > 1e-6f)
            {
                float speed = v.magnitude / k;
                Vector3 axis = Vector3.Cross(Vector3.up, v.normalized);
                wantedLean = axis * (Mathf.Min(MaxStumbleLean, LeanPerSpeed * speed) * Mathf.Deg2Rad);
                wantedShift = v.normalized * Mathf.Min(0.15f * Body.LegLength, 0.12f * v.magnitude);
                wantedFlail = Mathf.Clamp01(speed / 1.2f);
            }

            float rate = dt / 0.08f;
            topple = Vector3.Lerp(topple, wantedLean, Mathf.Clamp01(rate));
            toppleShift = Vector3.Lerp(toppleShift, wantedShift, Mathf.Clamp01(rate));
            flail = Mathf.Lerp(flail, wantedFlail, Mathf.Clamp01(0.6f * rate));
            Stance.Urgency = Mathf.Clamp01(v.magnitude / k / 0.8f);
            Pose.Topple = topple;
            Pose.ToppleShift = toppleShift;
            Pose.Flail = flail;
            if (v.sqrMagnitude > 1e-6f)
            {
                Pose.FlailDirection = v.normalized;
            }
        }

        // ---- Down --------------------------------------------------------------------------------

        private void KnockDown(Vector3 velocity, bool launch, string cause)
        {
            float k = SizeSpeed();
            velocity.y = 0f;
            velocity = Vector3.ClampMagnitude(velocity, (launch ? MaxLaunch : LaunchAt) * k);
            // What the body was already doing (the legs were kinematic: they had no speed of their own).
            Vector3 carried = State == UnitState.Stumbling ? StumbleVelocity : Knockback;
            if (CurrentMove != null)
            {
                CancelMove(CurrentMove, launch ? "launched" : "knocked down");
            }

            DropCutShort();
            StopRhythm();
            StopGait();
            Pose.Move = null;
            DropBlock();
            Knockback = Vector3.zero;
            StumbleVelocity = Vector3.zero;
            Stance.Drift = Vector3.zero;
            Stance.Urgency = 0f;
            Stance.Hold = false;
            ClearRecoil();
            ClearTimers();
            topple = toppleShift = Vector3.zero;
            flail = 0f;
            Pose.Topple = Pose.ToppleShift = Vector3.zero;
            Pose.Flail = 0f;
            if (MainWeapon != null)
            {
                MainWeapon.ShowTrail(false);
            }
            launched = launch;
            downCalm = 0f;
            impactsUntil.Clear();
            bool was = State == UnitState.Down;
            SetState(UnitState.Down);
            TimesDown += was ? 0 : 1;

            // Every part to physics, carried along by the blow: knocked down, the upper body most,
            // so it topples over its feet; launched, the whole body, thrown up as well.
            SetAuthority(null);
            foreach (var part in Rig.Parts)
            {
                if (part.body != null && part.body.isKinematic && !Rig.IsSevered(part.id))
                {
                    part.body.isKinematic = false;
                    part.body.linearVelocity = carried;
                    part.body.angularVelocity = Vector3.zero;
                }
            }

            // (The parts already move with what it was doing: only the rest is added.)
            float lift = launch ? LaunchLift * velocity.magnitude : 0f;
            Vector3 added = velocity - carried;
            if (launch)
            {
                PushBody(added, 0.9f, 0.85f, lift);
            }
            else
            {
                PushBody(added, 0.6f, 0.3f, 0f);
            }

            BalanceChanged?.Invoke(this, $"{(launch ? "launched" : "knocked down")} at {velocity.magnitude:0.00} m/s toward {Heading(velocity)}"
                                         + (cause != null ? $" ({cause})" : ""));
        }

        /// <summary>Adds speed to the physical body: the upper body all of it, the pelvis and legs the shares given, and some lift.</summary>
        private void PushBody(Vector3 velocity, float pelvisShare, float legShare, float lift)
        {
            foreach (var part in Rig.Parts)
            {
                if (part.body == null || part.body.isKinematic || Rig.IsSevered(part.id))
                {
                    continue;
                }

                var region = AuthorityController.RegionOf(part.id);
                float share = part.id == PartId.Pelvis ? pelvisShare : region == BodyRegion.Legs ? legShare : 1f;
                part.body.AddForce(velocity * share + Vector3.up * lift, ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// A body on the ground: the root follows its pelvis; flung parts hit whoever they meet; once
        /// it has lain still long enough it gets up.
        /// </summary>
        private void StepDown(float dt, bool drive)
        {
            StateTime += dt;
            LocalTime += dt;
            var pelvis = Rig.Part(PartId.Pelvis).body;
            Vector3 p = pelvis.position;
            transform.position = new Vector3(p.x, Stance.GroundY, p.z);
            if (drive)
            {
                FindImpacts();
            }

            float k = SizeSpeed();
            downCalm = pelvis.linearVelocity.magnitude < SettleSpeed * k ? downCalm + dt : 0f;
            float least = MinDown * Mathf.Sqrt(Mathf.Max(0.3f, Body.SizeFactor));
            // (A unit that lost a leg doesn't get up again.)
            if (!LegLost && ((StateTime >= least && downCalm >= SettleTime) || StateTime >= MaxDown))
            {
                BeginGetUp();
                StepGetUp(0f, drive);
                return;
            }

            if (drive)
            {
                SetAuthority(null);
                Authority.Apply(dt);
                foreach (var part in Rig.Parts)
                {
                    preStep[(int)part.id] = part.body.linearVelocity;
                }
            }
        }

        /// <summary>
        /// Parts of a thrown body moving fast into another unit's parts strike it: an impulse of
        /// half the closing speed times the two units' reduced mass, which hurts and knocks it
        /// (it may stumble, fall or be launched in turn). The thrown body takes half the damage.
        /// PhysX does the bounce.
        /// </summary>
        private void FindImpacts()
        {
            if (StateTime > 2.5f || !IsAlive)
            {
                return;
            }

            float reach = 2.5f * Mathf.Max(1f, Body.SizeFactor);
            float dt = Time.fixedDeltaTime;
            foreach (var mine in Rig.Parts)
            {
                if (mine.body == null || mine.partCollider == null || Rig.IsSevered(mine.id))
                {
                    continue;
                }

                Vector3 v = mine.body.linearVelocity;
                if (v.sqrMagnitude < ImpactSpeed * ImpactSpeed)
                {
                    continue;
                }

                Vector3 c = mine.body.worldCenterOfMass;
                for (int u = 0; u < all.Count && IsAlive; u++)
                {
                    var other = all[u];
                    if (!CanStrike(other))
                    {
                        continue;
                    }

                    Vector3 apart = other.transform.position - transform.position;
                    apart.y = 0f;
                    if (apart.magnitude > reach + 2.5f * other.Body.SizeFactor)
                    {
                        continue;
                    }

                    foreach (var theirs in other.Rig.Parts)
                    {
                        if (theirs.partCollider == null || theirs.body == null || other.Rig.IsSevered(theirs.id))
                        {
                            continue;
                        }

                        // Touching, or about to within this step (a fast part moves several centimetres a step).
                        Vector3 q = theirs.partCollider.ClosestPoint(c);
                        Vector3 r = mine.partCollider.ClosestPoint(q);
                        float gap = 0.04f + (v - PartVelocity(other, theirs)).magnitude * dt;
                        if ((q - r).sqrMagnitude > gap * gap)
                        {
                            continue;
                        }

                        if (TryImpact(mine, v, other, theirs, q))
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// PhysX reported a part of this body touching something: another unit's part met at speed is
        /// a body impact (the speed it had before the step, since the contact has already slowed it).
        /// </summary>
        private void ImpactFromCollision(BodyPart mine, Collision collision)
        {
            if (State != UnitState.Down || !IsAlive || StateTime > 2.5f || collision.collider == null)
            {
                return;
            }

            var theirs = collision.collider.GetComponentInParent<BodyPart>();
            if (theirs == null || theirs.Owner == null || theirs.Owner == this || theirs.partCollider != collision.collider || !CanStrike(theirs.Owner))
            {
                return;
            }

            Vector3 v = preStep != null ? preStep[(int)mine.id] : mine.body.linearVelocity;
            TryImpact(mine, v, theirs.Owner, theirs, theirs.partCollider.ClosestPoint(mine.body.worldCenterOfMass));
        }

        private bool CanStrike(UnitController other) =>
            other != this && other != knockedBy && other.initialized && other.IsAlive
            && !(impactsUntil.TryGetValue(other, out float until) && LocalTime < until);

        private static Vector3 PartVelocity(UnitController unit, BodyPart part) =>
            part.body.isKinematic ? unit.Knockback + unit.StumbleVelocity : part.body.linearVelocity;

        private bool TryImpact(BodyPart mine, Vector3 v, UnitController other, BodyPart theirs, Vector3 point)
        {
            Vector3 relative = v - PartVelocity(other, theirs);
            Vector3 n = point - mine.body.worldCenterOfMass;
            n = n.sqrMagnitude > 1e-6f ? n.normalized : relative.normalized;
            float closing = Vector3.Dot(relative, n);
            if (closing < ImpactSpeed)
            {
                return false;
            }

            BodyImpact(other, theirs.id, mine.id, point, closing, relative.normalized);
            return true;
        }

        private void BodyImpact(UnitController other, PartId part, PartId mine, Vector3 point, float closing, Vector3 direction)
        {
            // Once per pair, whichever of two thrown bodies finds it first.
            impactsUntil[other] = LocalTime + ImpactCooldown;
            other.impactsUntil[this] = other.LocalTime + ImpactCooldown;
            float m1 = Body.Mass, m2 = other.Body.Mass;
            float j = closing * 0.5f * (m1 * m2 / Mathf.Max(1f, m1 + m2));
            float damage = ImpactDamage * j * DamageRules.PartMultiplier(part);
            var result = new HitResult
            {
                Damage = damage,
                PoiseDamage = DamageRules.PoiseScale * j,
                Impulse = direction * j,
                PartShare = DamageRules.PartShare(part),
                StopsSwing = true,
            };
            result.Reaction = other.Health - damage <= 0f ? Reaction.Killed
                : other.Poise - result.PoiseDamage <= 0f ? Reaction.Stagger
                : result.PoiseDamage >= DamageRules.FlinchShare * other.profile.maxPoise ? Reaction.Flinch
                : Reaction.Recoil;
            var report = new HitReport
            {
                Attacker = this, Struck = other, Part = part, Point = point, Speed = closing, Result = result,
                Kind = launched ? "a launched body thrown into it" : "a falling body", Impact = true, ImpactPart = mine,
            };
            other.TakeHit(report);
            Hit?.Invoke(report);

            // The thrown body is hurt too (its own fall: no push, PhysX bounces it).
            var back = new HitResult { Damage = 0.5f * damage, Reaction = Health - 0.5f * damage <= 0f ? Reaction.Killed : Reaction.Recoil, StopsSwing = true };
            var self = new HitReport
            {
                Attacker = other, Struck = this, Part = mine, Point = point, Speed = closing, Result = back,
                Kind = "thrown into " + other.name, Impact = true, ImpactPart = part,
            };
            TakeHit(self);
            Hit?.Invoke(self);
        }

        // ---- Getting up --------------------------------------------------------------------------

        /// <summary>Face up or down by which way the pelvis's front looks; the root goes under it, facing along the body.</summary>
        private void BeginGetUp()
        {
            var pelvis = Rig.Part(PartId.Pelvis).body;
            Vector3 front = pelvis.rotation * Vector3.forward;
            var kind = front.y > 0f ? GetUp.Kind.FaceUp : GetUp.Kind.FaceDown;
            Vector3 headward = Rig.Part(PartId.Head).body.position - pelvis.position;
            headward.y = 0f;
            if (headward.sqrMagnitude < 0.01f * Body.SizeFactor * Body.SizeFactor)
            {
                // (Sitting or curled up: along the pelvis instead.)
                headward = pelvis.rotation * Vector3.up;
                headward.y = 0f;
            }

            GetUp.Place(kind, pelvis.position, headward, Body.SizeFactor, out var position, out float yaw);
            position.y = Stance.GroundY;
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            facingYaw = yaw;
            Stance.Plant(position, yaw);
            hasLastRoot = false;
            Stance.Hold = true;
            Stance.Drift = Vector3.zero;
            ClearTimers();
            lying.Capture(Rig);
            getUp.HoldsMain = MainWeapon != null;
            getUp.HoldsOff = OffWeapon != null;
            getUp.Loadout = Loadout;
            getUp.Rig = Rig;
            getUp.Threat = GetUpThreat();
            getUp.Begin(kind, lying);
            LastGetUp = kind;
            float down = StateTime;
            SetState(UnitState.GettingUp);
            BalanceChanged?.Invoke(this, $"getting up {(kind == GetUp.Kind.FaceDown ? "face down" : "face up")} after {down:0.0} s down");
        }

        private void StepGetUp(float dt, bool drive)
        {
            StateTime += dt;
            LocalTime += dt;
            float t = StateTime;
            PoseBuffer stand = null;
            if (t > getUp.StandFrom)
            {
                // The standing pose, as it will be once up (the feet already on their spots, the
                // rhythm's base loop playing).
                PoseRhythmBase();
                Pose.Compose(LocalTime, dt, true, armPlays: true);
                standing.Capture(Anim);
                stand = standing;
            }

            getUp.Threat = GetUpThreat();
            getUp.Compose(t, stand);
            if (drive)
            {
                SetAuthority(null);
                Authority.Apply(dt);
            }

            if (t >= getUp.Duration)
            {
                SetState(UnitState.Standing);
                Stance.Hold = false;
                Poise = profile.maxPoise;
                NeedsSpot = true;
                TimesUp++;
                BalanceChanged?.Invoke(this, $"got up ({(LastGetUp == GetUp.Kind.FaceDown ? "face down" : "face up")})");
            }
        }

        /// <summary>
        /// Milestone 9c: where the enemy it minds is, as it knows it (chest height), for the shield
        /// getting up; none if it knows of none.
        /// </summary>
        private Vector3? GetUpThreat()
        {
            var foe = Foe();
            if (foe == null || !Known.Knows(foe))
            {
                return null;
            }

            Vector3 at = Known.Estimate(foe, LocalTime);
            return at + Vector3.up * (foe.Body.PelvisHeight + 0.35f * foe.Body.SizeFactor);
        }

        /// <summary>How firmly the body is held while getting up: the pins come back from weak, the legs kinematic once blended in.</summary>
        private void SetGetUpAuthority()
        {
            float t = StateTime;
            float blend = getUp.BlendIn;
            float ramp = Mathf.Clamp01(t / (blend + 0.3f));
            ramp = ramp * ramp * (3f - 2f * ramp);
            if (t < blend)
            {
                Authority.Set(BodyRegion.Legs, AuthorityLevel.Pinned, Mathf.Lerp(0.25f, 1f, t / blend));
            }
            else
            {
                Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
            }

            // While the pins are still weak the joints keep their anatomical limits (a head or arm
            // held this loosely could otherwise flop through impossible angles).
            bool weak = ramp < 0.6f;
            Authority.Set(BodyRegion.Torso, AuthorityLevel.Pinned, Mathf.Lerp(0.15f, profile.torsoPin, ramp), weak);
            Authority.Set(BodyRegion.Head, AuthorityLevel.Pinned, Mathf.Lerp(0.15f, profile.torsoPin, ramp), weak);
            Authority.Set(BodyRegion.ArmL, AuthorityLevel.Pinned, Mathf.Lerp(0.15f, profile.armPin, ramp), weak);
            Authority.Set(BodyRegion.ArmR, AuthorityLevel.Pinned, Mathf.Lerp(0.15f, profile.armPin, ramp), weak);
        }

        /// <summary>Hit recovery and hit stop don't run while down: cleared, so the unit isn't limp or frozen once up.</summary>
        private void ClearTimers()
        {
            for (int r = 0; r < recoverLeft.Length; r++)
            {
                recoverLeft[r] = 0f;
            }

            HitStop = 0f;
        }

        private void DropBlock()
        {
            incoming = null;
            BlockBlend = 0f;
            Pose.BlockBlend = 0f;
        }

        private void ClearRecoil()
        {
            reactShift = reactShiftV = reactLean = reactLeanV = reactHead = reactHeadV = Vector3.zero;
            Pose.ReactShift = Pose.ReactLean = Pose.ReactHead = Vector3.zero;
        }

        private static string Heading(Vector3 v)
        {
            v.y = 0f;
            if (v.sqrMagnitude < 1e-8f)
            {
                return "nowhere";
            }

            float angle = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
            string[] names = { "+Z", "+X+Z", "+X", "+X-Z", "-Z", "-X-Z", "-X", "-X+Z" };
            int i = Mathf.RoundToInt(Mathf.Repeat(angle, 360f) / 45f) % 8;
            return names[i];
        }
    }
}
