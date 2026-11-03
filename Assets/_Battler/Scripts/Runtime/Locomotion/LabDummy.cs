using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The lab's training dummy (the combat plan's step 4): a second body standing in its guard, facing
    /// the walker, walked by its own <see cref="Locomotor"/> and <see cref="LocoBody"/>. Physical, as the
    /// walker is (the legs carried, the torso, head and arms held by pins), it takes blows: the part struck
    /// is knocked and its hold softened for a moment, the rest of the blow pushes the walk back a step.
    /// With its shield up (<see cref="Blocking"/>) it holds the shield across the blow.
    /// </summary>
    public sealed class LabDummy
    {
        public readonly GameObject Unit;
        public readonly UnitRig Rig;
        public readonly AnimRig Anim;
        public readonly Locomotor Loco;
        public readonly LocoBody Poser;
        public AuthorityController Authority { get; }

        /// <summary>The colliders of what the dummy holds as a shield (a blade that meets one is blocked).</summary>
        public readonly HashSet<Collider> ShieldColliders = new HashSet<Collider>();

        private readonly Dictionary<Collider, PartId> parts = new Dictionary<Collider, PartId>();
        private readonly UnitProfile profile;
        private bool snapped;

        /// <summary>Where it stands: knocked off it, it steps back onto it (after <see cref="HomeAfter"/> seconds unstruck).</summary>
        public Vector3 Home;

        /// <summary>How long after a blow it starts back, from how far off its spot (leg lengths... a Medium's metres), and how fast (m/s).</summary>
        public const float HomeAfter = 1f, HomeFrom = 0.15f, HomeSpeed = 0.7f;

        private float clock, lastHit = -10f;

        /// <summary>The body's mass (kg) and its size against a Medium's.</summary>
        public readonly float Mass, Size;

        /// <summary>Its poise: blows wear it down, it comes back over time (as a unit's does in the fight).</summary>
        public float Poise, MaxPoise;

        /// <summary>How the dummy stands (O in the lab): open (no guard), in its guard, or blocking.</summary>
        public enum Stances
        {
            Open,
            Guard,
            Block,
        }

        private Stances stance = Stances.Guard;

        public Stances Stance
        {
            get => stance;
            set
            {
                stance = value;
                Loco.GuardWanted = value != Stances.Open;
            }
        }

        /// <summary>The shield is held up across the blow coming.</summary>
        public bool Blocking => stance == Stances.Block;

        /// <summary>Where the blow coming is aimed (world): the shield goes between it and the striker.</summary>
        public Vector3? BlockAt;

        private float blockWeight;

        /// <summary>How fast the shield comes up and goes down again (seconds for all of it).</summary>
        public const float BlockRaise = 0.15f;

        /// <summary>Poise back a second (share of the most).</summary>
        public const float PoiseBack = 0.15f;

        public int Hits, Blocks;

        // Each region's hold, softened by a blow and coming back.
        private readonly float[] pinFrom = new float[5], pinTime = new float[5], pinLeft = new float[5];

        /// <param name="unit">The unit's body, just built (its own controller taken off), in its rest pose.</param>
        public LabDummy(GameObject unit, UnitRig rig, UnitProfile profile, Vector3 at, float facing, float groundY, string guardPrefix, bool physical)
        {
            Unit = unit;
            Rig = rig;
            this.profile = profile;
            if (physical)
            {
                // (As the lab's walker: the body set up as a unit sets up its own, in its rest pose.)
                JointConventions.EnsureProbed();
                foreach (var part in rig.Parts)
                {
                    UnitBuilder.ConfigureBody(part.body, profile);
                    part.limits = JointLimitTable.Physical(part.id);
                    if (part.joint != null)
                    {
                        part.jointFrame = JointFrames.For(rig, part.id);
                        JointFrames.Apply(part.joint, part.jointFrame);
                        JointMath.ApplyLimits(part.joint, part.limits);
                    }
                }
            }

            Anim = new AnimRig(unit.transform, rig);
            float mass = 0f;
            foreach (var part in rig.Parts)
            {
                if (part.body != null)
                {
                    part.body.isKinematic = true;
                    part.body.interpolation = RigidbodyInterpolation.Interpolate;
                    mass += part.body.mass;
                }

                if (part.partCollider != null)
                {
                    parts[part.partCollider] = part.id;
                }
            }

            Mass = Mathf.Max(20f, mass);
            foreach (var id in PartInfo.All)
            {
                rig.Part(id).transform.SetParent(unit.transform, true);
            }

            if (physical)
            {
                Physics.SyncTransforms();
                var all = rig.Parts;
                for (int i = 0; i < all.Length; i++)
                {
                    for (int j = i + 1; j < all.Length; j++)
                    {
                        if (all[i].partCollider != null && all[j].partCollider != null)
                        {
                            Physics.IgnoreCollision(all[i].partCollider, all[j].partCollider, true);
                        }
                    }
                }

                Authority = new AuthorityController(rig, Anim);
                for (int r = 0; r < 5; r++)
                {
                    Hold((BodyRegion)r, 1f);
                }
            }

            float leg = Anim.RestLocalPosition(PartId.ShinL).magnitude + Anim.RestLocalPosition(PartId.FootL).magnitude;
            Size = leg / 0.845f;
            Loco = new Locomotor(leg);
            Poser = new LocoBody(Anim, Loco) { GroundY = groundY };
            Poser.SoleFrom(rig);
            Loco.SetGuard(guardPrefix);
            Loco.GuardWanted = true;
            Loco.Place(new Vector3(at.x, groundY, at.z), facing);
            Home = new Vector3(at.x, groundY, at.z);
            MaxPoise = Poise = profile.maxPoise;
        }

        /// <summary>The body part a collider is, if it is one of the dummy's.</summary>
        public bool PartOf(Collider collider, out PartId id) => parts.TryGetValue(collider, out id);

        /// <summary>Where a part of it is now, as posed (world).</summary>
        public Vector3 PartAt(PartId id) => Anim.Part(id).position;

        /// <summary>Where to strike it, low, in the middle or high (0, 1, 2): its hips, the middle of its chest, its face.</summary>
        public Vector3 TargetAt(int height) =>
            height <= 0 ? PartAt(PartId.Pelvis)
            : height == 1 ? 0.5f * (PartAt(PartId.Spine) + PartAt(PartId.Chest))
            : PartAt(PartId.Head) + Vector3.up * (0.08f * Size);

        private float BasePin(BodyRegion region) =>
            region == BodyRegion.ArmL || region == BodyRegion.ArmR ? profile.armPin : profile.torsoPin;

        private void Hold(BodyRegion region, float share)
        {
            if (Authority == null)
            {
                return;
            }

            if (region == BodyRegion.Legs)
            {
                Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
                return;
            }

            Authority.Set(region, AuthorityLevel.Pinned, BasePin(region) * share);
        }

        /// <summary>
        /// One step: it stands in its guard facing <paramref name="faceToward"/> (held still through a hit
        /// stop, <paramref name="frozen"/>), the shield raised if it blocks, the physical body held to it.
        /// </summary>
        public void Step(float dt, Vector3 faceToward, bool frozen)
        {
            clock += dt;
            if (!frozen)
            {
                Vector3 to = faceToward - Loco.Position;
                float yaw = new Vector2(to.x, to.z).sqrMagnitude > 0.01f ? Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg : Loco.Facing;
                // (Knocked off its spot, it steps back onto it once it has stood a moment, as a sparring partner would.)
                Vector3 home = Home - Loco.Position;
                home.y = 0f;
                Vector3 want = Vector3.zero;
                if (Loco.Current != Locomotor.Mode.OneShot && clock - lastHit > HomeAfter && home.magnitude > HomeFrom * Size)
                {
                    want = Vector3.ClampMagnitude(home * 2f, HomeSpeed * Mathf.Sqrt(Size));
                }

                Loco.Step(dt, want, yaw);
                Poser.Pose(dt);
                RaiseShield(dt, faceToward);
                React(dt);
                Recover(dt);
                Poise = Mathf.Min(MaxPoise, Poise + PoiseBack * MaxPoise * dt);
            }

            if (Authority != null && snapped)
            {
                Authority.Apply(dt);
            }
            else
            {
                Carry();
                snapped = true;
            }
        }

        /// <summary>
        /// The shield held across the blow: the left hand put between where the blow is aimed and the
        /// striker, its face toward the striker.
        /// </summary>
        private void RaiseShield(float dt, Vector3 striker)
        {
            blockWeight = Mathf.MoveTowards(blockWeight, Blocking ? 1f : 0f, dt / BlockRaise);
            if (blockWeight <= 0f)
            {
                return;
            }

            var upper = Anim.Part(PartId.UpperArmL);
            var fore = Anim.Part(PartId.ForearmL);
            var hand = Anim.Part(PartId.HandL);
            Vector3 chest = Anim.Part(PartId.Chest).position;
            Vector3 aimed = BlockAt ?? chest;
            Vector3 toStriker = striker + Vector3.up * (aimed.y - striker.y) - aimed;
            toStriker = toStriker.sqrMagnitude > 1e-4f ? toStriker.normalized : Quaternion.Euler(0f, Loco.Facing, 0f) * Vector3.forward;
            Vector3 want = aimed + toStriker * (0.32f * Size);
            float reach = (Anim.RestLocalPosition(PartId.ForearmL).magnitude + Anim.RestLocalPosition(PartId.HandL).magnitude) * 0.9f;
            Vector3 fromShoulder = Vector3.ClampMagnitude(want - upper.position, reach);
            Vector3 target = Vector3.Lerp(hand.position, upper.position + fromShoulder, blockWeight);
            var turn = Quaternion.Euler(0f, Loco.Facing, 0f);
            TwoBoneIK.Solve(upper, fore, hand, target, turn * new Vector3(-1f, -0.6f, 0f));
            // (The shield's face is across the hand: it looks the way the hand points.)
            Vector3 along = hand.rotation * Anim.RestLocalPosition(PartId.HandL).normalized;
            hand.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(along, toStriker), blockWeight) * hand.rotation;
        }

        /// <summary>The softened holds coming back.</summary>
        private void Recover(float dt)
        {
            for (int r = 1; r < 5; r++)
            {
                if (pinLeft[r] <= 0f)
                {
                    continue;
                }

                pinLeft[r] = Mathf.Max(0f, pinLeft[r] - dt);
                float u = 1f - pinLeft[r] / Mathf.Max(0.01f, pinTime[r]);
                Hold((BodyRegion)r, Mathf.Lerp(pinFrom[r], 1f, u * u * (3f - 2f * u)));
            }
        }

        private void Soften(BodyRegion region, float share, float over)
        {
            int r = (int)region;
            if (region == BodyRegion.Legs || (pinLeft[r] > 0f && pinFrom[r] < share))
            {
                return;
            }

            pinFrom[r] = share;
            pinTime[r] = pinLeft[r] = over;
            Hold(region, share);
        }

        /// <summary>The most a blow sends a struck part on its own (m/s at a Medium's size), and a struck shield arm.</summary>
        public const float MostPartSpeed = 5f, MostShieldSpeed = 2f;

        /// <summary>The most a blow pushes the whole dummy back (m/s).</summary>
        public const float MostKnock = 1.6f;

        /// <summary>
        /// A blow lands: on <paramref name="part"/>, or on the shield. What the part takes knocks it (its
        /// hold softened, coming back), the rest pushes the walk back.
        /// </summary>
        public void TakeHit(PartId part, bool shield, Vector3 impulse, float partShare, float poiseDamage, Combat.Reaction reaction, Vector3 point)
        {
            lastHit = clock;
            // The pose shows it (the fight's recoil): the pinned body is pulled back to its pose each step,
            // so a kick on the physical part alone hardly shows.
            float react = (reaction == Combat.Reaction.Stagger ? 1.6f : reaction == Combat.Reaction.Flinch ? 1.25f : 1f) * ReactScale;
            Recoil(impulse, point, shield ? PartId.HandL : part, shield ? 0.6f * react : react);
            var armRegion = shield ? BodyRegion.ArmL : AuthorityController.RegionOf(part);
            if (armRegion == BodyRegion.ArmL || armRegion == BodyRegion.ArmR)
            {
                KnockArm(armRegion == BodyRegion.ArmR ? 0 : 1, impulse * (shield ? 1f : partShare), point);
            }

            if (shield)
            {
                Blocks++;
            }
            else
            {
                Hits++;
            }

            Poise = Mathf.Max(0f, Poise - poiseDamage);
            var region = shield ? BodyRegion.ArmL : AuthorityController.RegionOf(part);
            bool hard = reaction == Combat.Reaction.Stagger || reaction == Combat.Reaction.Flinch;
            Soften(region, shield ? 0.4f : hard ? 0.2f : 0.3f, shield ? 0.4f : hard ? 0.9f : 0.6f);
            if (region != BodyRegion.Torso && !shield)
            {
                Soften(BodyRegion.Torso, 0.6f, 0.5f);
            }

            var struck = Rig.Part(shield ? PartId.HandL : part).body;
            if (Authority != null && struck != null && !struck.isKinematic)
            {
                Vector3 kick = impulse * partShare / Mathf.Max(0.5f, struck.mass);
                struck.AddForce(Vector3.ClampMagnitude(kick, (shield ? MostShieldSpeed : MostPartSpeed) * Size), ForceMode.VelocityChange);
            }

            Vector3 knock = impulse * (1f - 0.5f * partShare) * KnockScale / Mass;
            knock.y = 0f;
            Loco.Push(Vector3.ClampMagnitude(knock, MostKnock));
        }

        // ---- The pose's reaction to a blow (the fight's recoil and arm knock, UnitCombat) -------------------

        private Vector3 lean, leanV, headSnap, headV;

        /// <summary>How far the blows throw the upper body now (radians), for the logs.</summary>
        public float Lean => lean.magnitude;
        private readonly Vector3[] knock = new Vector3[2], knockV = new Vector3[2], knockTurn = new Vector3[2], knockTurnV = new Vector3[2];

        /// <summary>
        /// How much harder the dummy reacts than the fight's rules have a unit react (the lab's first playtest:
        /// at the fight's own, a Medium's blow on a Medium leaned it 7 to 17 degrees and pushed it 0.2 m/s, and
        /// read as nothing). The fight's own are tuned when the walk joins it (step 8).
        /// </summary>
        public float ReactScale = 1.8f, KnockScale = 1.6f;

        /// <summary>The recoil's swing back (Hz), its damping and the head's; the most it throws the upper body and the head (radians).</summary>
        public const float ReactFrequency = 1.6f, ReactDamping = 0.55f, HeadFrequency = 2.4f, MostLean = 0.6f, MostHeadSnap = 0.7f, Kick = 1.8f;

        /// <summary>A struck arm's knock aside: its return (Hz, critically damped), the most it moves the hand (m at a Medium's size) and turns it (radians).</summary>
        public const float ArmKnockFrequency = 2.5f, MostArmKnock = 0.12f, MostArmTurn = 0.44f;

        private static float Tanh(float x)
        {
            float e = Mathf.Exp(-2f * Mathf.Abs(x));
            return Mathf.Sign(x) * (1f - e) / (1f + e);
        }

        /// <summary>The upper body turned about the hips by the blow's lever; the head snapped by one to it.</summary>
        private void Recoil(Vector3 impulse, Vector3 point, PartId part, float scale)
        {
            float j = impulse.magnitude * scale;
            if (j < 1e-4f)
            {
                return;
            }

            float leg = Loco.Leg;
            Vector3 dir = impulse / impulse.magnitude;
            float w = 2f * Mathf.PI * ReactFrequency;
            Vector3 lever = point - Anim.Part(PartId.Pelvis).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                float leverage = Mathf.Clamp(lever.magnitude / leg, 0.2f, 1.4f);
                float amount = MostLean * Tanh(1.5f * j * leverage / (Mass * leg));
                leanV += axis.normalized * (amount * Kick * w);
            }

            if (part == PartId.Head)
            {
                Vector3 headLever = point - Anim.Part(PartId.Head).position + Vector3.up * (0.1f * leg);
                Vector3 headAxis = Vector3.Cross(headLever, dir);
                if (headAxis.sqrMagnitude > 1e-8f)
                {
                    float snap = MostHeadSnap * Tanh(6f * j / (Mass * leg));
                    headV += headAxis.normalized * (snap * Kick * 2f * Mathf.PI * HeadFrequency);
                }
            }
        }

        /// <summary>A struck arm (or the shield it holds) knocked aside and turned, then back.</summary>
        private void KnockArm(int a, Vector3 impulse, Vector3 point)
        {
            float j = impulse.magnitude;
            if (j < 1e-4f)
            {
                return;
            }

            float armMass = 0f;
            foreach (var id in a == 0 ? new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR } : new[] { PartId.UpperArmL, PartId.ForearmL, PartId.HandL })
            {
                var body = Rig.Part(id).body;
                armMass += body != null ? body.mass : 0f;
            }

            float hard = Tanh(j / (Mathf.Max(1f, armMass + 3f) * 3f * Mathf.Sqrt(Size)));
            float kick = (float)System.Math.E * 2f * Mathf.PI * ArmKnockFrequency;
            Vector3 dir = impulse / j;
            knockV[a] += dir * (MostArmKnock * Size * hard * kick);
            Vector3 lever = point - Anim.Part(a == 0 ? PartId.HandR : PartId.HandL).position;
            Vector3 axis = Vector3.Cross(lever, dir);
            if (axis.sqrMagnitude > 1e-8f)
            {
                knockTurnV[a] += axis.normalized * (MostArmTurn * hard * kick);
            }
        }

        private static void Spring(ref Vector3 x, ref Vector3 v, float hz, float damping, float dt)
        {
            float w = 2f * Mathf.PI * hz;
            v += (-w * w * x - 2f * damping * w * v) * dt;
            x += v * dt;
        }

        private static void CriticalSpring(ref Vector3 x, ref Vector3 v, float w, float dt)
        {
            float decay = Mathf.Exp(-w * dt);
            Vector3 c = v + w * x;
            Vector3 nx = (x + c * dt) * decay;
            v = (v - c * (w * dt)) * decay;
            x = nx;
        }

        /// <summary>The reaction springs stepped and put on the pose: the upper body leans away, the head snaps, a struck arm is knocked aside.</summary>
        private void React(float dt)
        {
            Spring(ref lean, ref leanV, ReactFrequency, ReactDamping, dt);
            Spring(ref headSnap, ref headV, HeadFrequency, ReactDamping, dt);
            lean = Vector3.ClampMagnitude(lean, MostLean);
            headSnap = Vector3.ClampMagnitude(headSnap, MostHeadSnap);
            float wk = 2f * Mathf.PI * ArmKnockFrequency;
            for (int a = 0; a < 2; a++)
            {
                CriticalSpring(ref knock[a], ref knockV[a], wk, dt);
                CriticalSpring(ref knockTurn[a], ref knockTurnV[a], wk, dt);
            }

            float angle = lean.magnitude * Mathf.Rad2Deg;
            if (angle > 0.01f)
            {
                // (The spine takes some of the turn, the chest the rest; the hips and legs stay as they stand.)
                Vector3 axis = lean.normalized;
                var spine = Anim.Part(PartId.Spine);
                var chest = Anim.Part(PartId.Chest);
                spine.rotation = Quaternion.AngleAxis(0.45f * angle, axis) * spine.rotation;
                chest.rotation = Quaternion.AngleAxis(0.55f * angle, axis) * chest.rotation;
            }

            if (headSnap.sqrMagnitude > 1e-8f)
            {
                var head = Anim.Part(PartId.Head);
                head.rotation = Quaternion.AngleAxis(headSnap.magnitude * Mathf.Rad2Deg, headSnap.normalized) * head.rotation;
            }

            for (int a = 0; a < 2; a++)
            {
                if (knock[a].sqrMagnitude < 1e-8f && knockTurn[a].sqrMagnitude < 1e-8f)
                {
                    continue;
                }

                var upper = Anim.Part(a == 0 ? PartId.UpperArmR : PartId.UpperArmL);
                var fore = Anim.Part(a == 0 ? PartId.ForearmR : PartId.ForearmL);
                var hand = Anim.Part(a == 0 ? PartId.HandR : PartId.HandL);
                var turnHand = knockTurn[a].sqrMagnitude > 1e-8f ? Quaternion.AngleAxis(knockTurn[a].magnitude * Mathf.Rad2Deg, knockTurn[a].normalized) : Quaternion.identity;
                var keep = turnHand * hand.rotation;
                Vector3 pole = fore.position - 0.5f * (upper.position + hand.position);
                TwoBoneIK.Solve(upper, fore, hand, hand.position + knock[a], pole);
                hand.rotation = keep;
            }
        }

        /// <summary>The body parts where the animation rig has them (kinematic).</summary>
        private void Carry()
        {
            foreach (var id in PartInfo.All)
            {
                var part = Rig.Part(id);
                var target = Anim.Part(id);
                if (part.body != null)
                {
                    part.body.MovePosition(target.position);
                    part.body.MoveRotation(target.rotation);
                }
                else
                {
                    part.transform.SetPositionAndRotation(target.position, target.rotation);
                }
            }
        }

        public void Destroy()
        {
            if (Unit != null)
            {
                Object.Destroy(Unit);
            }
        }
    }
}
