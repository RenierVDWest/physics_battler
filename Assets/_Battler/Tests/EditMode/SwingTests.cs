using System.IO;
using System.Reflection;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// The big swings (the attack rework before milestone 4): every ready-made move, played by a
    /// medium unit at a medium target, is a real swing. A big arc, fast through contact (and fastest
    /// there, not in the wind-up), carried on past the aim point, the body turning behind it, the
    /// feet stepping into it.
    /// </summary>
    [TestFixture]
    public class SwingTests
    {
        private static UnitController Build(string body, Vector3 at, float yaw, int team)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
            var profile = UnitProfile.CreateDefault();
            profile.body = body;
            var root = UnitBuilder.Build(RigFile.Parse(File.ReadAllText(path)), profile, $"Swing {body} {team}", _ => null, 0);
            root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            var unit = root.GetComponent<UnitController>();
            unit.Profile = profile;
            unit.Team = team;
            typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
            unit.DealsHits = false;
            unit.RhythmOn = false;   // (the swing's own shape and speed, measured from the plain guard)
            return unit;
        }

        private sealed class Played
        {
            public MoveState State;
            public float ChestTurn, WindUpSpeed;
            public int Steps;
        }

        private static Played Play(MoveStyle style)
        {
            var move = MoveDefinition.Create(style);
            var weapon = WeaponDefinition.CreateDefault(style.ToString().StartsWith("Sword") ? WeaponKind.Sword : WeaponKind.Club);
            weapon.moves = new[] { move };
            var attacker = Build("Medium", Vector3.zero, 0f, 0);
            var target = Build("Medium", Vector3.forward * 5f, 180f, 1);
            try
            {
                float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                target.Place(Vector3.forward * distance, 180f);
                attacker.Equip(weapon, null);
                for (int i = 0; i < 20; i++)
                {
                    attacker.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                }

                MoveState state = null;
                foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Neck, AimPart.Head, AimPart.KneeL })
                {
                    if (state == null && move.targets.Contains(part))
                    {
                        var s = MovePlanner.Prepare(attacker, target, move, part);
                        state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                    }
                }

                Assert.IsNotNull(state, $"{style}: a medium unit can strike a medium one");
                var played = new Played { State = state };
                float minYaw = float.MaxValue, maxYaw = float.MinValue;
                int stepsBefore = attacker.Stance.StepsTaken;
                float released = (move.SwingTravel(0) + move.SwingTravel(1)) / move.SwingTotal;
                Vector3 last = Vector3.zero;
                bool have = false;
                Assert.IsTrue(attacker.StartMove(state));
                for (int i = 0; i < 600 && attacker.IsAttacking; i++)
                {
                    attacker.PreviewPose(1f / 60f);
                    target.PreviewPose(1f / 60f);
                    Vector3 strike = attacker.Pose.StrikePoint;
                    if (have && state.Progress < released && state.T > 0.1f)
                    {
                        played.WindUpSpeed = Mathf.Max(played.WindUpSpeed, Vector3.Distance(strike, last) * 60f);
                    }

                    last = strike;
                    have = true;
                    Vector3 chest = attacker.Anim.Part(PartId.Chest).rotation * Vector3.forward;
                    float yaw = Mathf.DeltaAngle(attacker.FacingYaw, Mathf.Atan2(chest.x, chest.z) * Mathf.Rad2Deg);
                    minYaw = Mathf.Min(minYaw, yaw);
                    maxYaw = Mathf.Max(maxYaw, yaw);
                    if (!state.ContactPassed)
                    {
                        played.Steps = attacker.Stance.StepsTaken - stepsBefore;
                    }
                }

                played.ChestTurn = maxYaw - minYaw;
                return played;
            }
            finally
            {
                Object.DestroyImmediate(attacker.gameObject);
                Object.DestroyImmediate(target.gameObject);
            }
        }

        /// <summary>
        /// Plays every ready-made blow (a medium unit at a medium target, the first part each can
        /// strike), calling <paramref name="each"/> after every step of it.
        /// </summary>
        private static void PlayEveryBlow(System.Action<UnitController, MoveStyle, MoveState> each)
        {
            foreach (MoveStyle style in System.Enum.GetValues(typeof(MoveStyle)))
            {
                var move = MoveDefinition.Create(style);
                if (move == null)
                {
                    continue;
                }

                var weapon = WeaponDefinition.CreateDefault(style.ToString().StartsWith("Sword") ? WeaponKind.Sword : WeaponKind.Club);
                weapon.moves = new[] { move };
                var attacker = Build("Medium", Vector3.zero, 0f, 0);
                var target = Build("Medium", Vector3.forward * 5f, 180f, 1);
                try
                {
                    float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                    target.Place(Vector3.forward * distance, 180f);
                    attacker.Equip(weapon, null);
                    for (int i = 0; i < 20; i++)
                    {
                        attacker.PreviewPose(1f / 60f);
                        target.PreviewPose(1f / 60f);
                    }

                    MoveState state = null;
                    foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Neck, AimPart.Head, AimPart.KneeL })
                    {
                        if (!move.targets.Contains(part))
                        {
                            continue;
                        }

                        var s = MovePlanner.Prepare(attacker, target, move, part);
                        if (s != null && MovePlanner.Acceptable(attacker, s))
                        {
                            state = s;
                            break;
                        }
                    }

                    if (state == null || !attacker.StartMove(state))
                    {
                        continue;
                    }

                    for (int step = 0; step < 600 && attacker.IsAttacking; step++)
                    {
                        attacker.PreviewPose(1f / 60f);
                        target.PreviewPose(1f / 60f);
                        each(attacker, style, state);
                    }
                }
                finally
                {
                    Object.DestroyImmediate(attacker.gameObject);
                    Object.DestroyImmediate(target.gameObject);
                }
            }
        }

        /// <summary>
        /// Milestone 9c: through every ready-made blow the weapon arm moves on smoothly. Its parts' turning
        /// speed changes little from one step to the next, and none turns far in a step while the blade
        /// itself moves little (the arm's shape carries on from the last step rather than jumping to
        /// another way of holding the weapon). Push A: 835 and 178 degrees; the first try at the
        /// straight wrists: 6521.
        /// </summary>
        [Test]
        public void TheWeaponArmNeverJumpsInABlow()
        {
            var parts = new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR };
            var changes = new System.Collections.Generic.List<float>();
            float worstTurn = 0f;
            string worstWhere = "";
            var was = new Quaternion[3];
            var rate = new float[3];
            Vector3 lastStrike = Vector3.zero;
            MoveState playing = null;
            int step = 0;
            PlayEveryBlow((attacker, style, state) =>
            {
                if (state != playing)
                {
                    playing = state;
                    step = 0;
                }

                for (int k = 0; k < 3; k++)
                {
                    var now = attacker.Anim.Part(parts[k]).rotation;
                    if (step > 0)
                    {
                        float turn = Quaternion.Angle(was[k], now);
                        float r = turn * 60f;
                        if (step > 1)
                        {
                            changes.Add(Mathf.Abs(r - rate[k]));
                        }

                        // (A fast blow turns the arm fast; a turn the blade's own travel doesn't explain is a jump.)
                        bool still = (attacker.Pose.StrikePoint - lastStrike).magnitude < 0.15f;
                        if (still && turn > worstTurn)
                        {
                            worstTurn = turn;
                            worstWhere = $"{style} {parts[k]} at {state.T:0.00}";
                        }

                        rate[k] = r;
                    }

                    was[k] = now;
                }

                lastStrike = attacker.Pose.StrikePoint;
                step++;
            });

            Assert.Greater(changes.Count, 300, "blows were played");
            changes.Sort();
            float p95 = changes[(int)(0.95f * (changes.Count - 1))];
            Assert.Less(p95, 1500f, $"the arm's turning speed changes smoothly (95th percentile {p95:0} degrees a second from one step to the next)");
            Assert.Less(worstTurn, 60f, $"no part of the arm turns far in a step the blade barely moves (worst {worstTurn:0} degrees: {worstWhere})");
        }

        /// <summary>
        /// Milestone 9c: through every ready-made blow the weapon arm keeps a person's shape: its
        /// shoulder, elbow and wrist inside their limits (the joint table) and the arm (the far half of
        /// the upper arm, the forearm, the hand) out of its own torso. The first try at the straight
        /// wrists: the shoulder more than 10 degrees past its limits in 27% of blow steps, up to 130.
        /// </summary>
        [Test]
        public void TheWeaponArmKeepsAPersonsShapeInABlow()
        {
            float worstPast = 0f, deepest = 0f;
            string pastWhere = "", deepWhere = "";
            int steps = 0;
            PlayEveryBlow((attacker, style, state) =>
            {
                steps++;
                var anim = attacker.Anim;
                foreach (var id in new[] { PartId.UpperArmR, PartId.ForearmR, PartId.HandR })
                {
                    var part = anim.Part(id);
                    // (The arm rebuild: against the arm rig's limits, the model its poses are made in.)
                    float past = PhysicsBattler.Motion.ArmRig.PastLimits(anim, id, anim.Part(PartInfo.Parent(id)).rotation, part.rotation);
                    if (past > worstPast)
                    {
                        worstPast = past;
                        pastWhere = $"{style} {id} at {state.T:0.00}";
                    }
                }

                float clear = BodyClearance.Measure(attacker.Rig, anim, state.Weapon.length * 0.98f, attacker.Loadout.Scale, out var closest, out bool blade);
                if (!blade && -clear > deepest)
                {
                    deepest = -clear;
                    deepWhere = $"{style} on the {closest} at {state.T:0.00}";
                }
            });

            Assert.Greater(steps, 300, "blows were played");
            Assert.LessOrEqual(worstPast, 1f, $"no joint of the weapon arm past its limits (worst {worstPast:0} degrees: {pastWhere})");
            Assert.LessOrEqual(deepest, 0.03f, $"the arm stays out of its own body (deepest {deepest * 100:0.0} cm: {deepWhere})");
        }

        [Test] public void SwordCutIsABigSwing() => BigSwing(MoveStyle.SwordCut);
        [Test] public void SwordBackhandIsABigSwing() => BigSwing(MoveStyle.SwordBackhand, 250f);
        [Test] public void SwordCleaveIsABigSwing() => BigSwing(MoveStyle.SwordCleave);
        [Test] public void ClubSwingIsABigSwing() => BigSwing(MoveStyle.ClubSwing);
        [Test] public void ClubSmashIsABigSwing() => BigSwing(MoveStyle.ClubSmash, turn: 50f);
        [Test] public void ClubRisingIsABigSwing() => BigSwing(MoveStyle.ClubRising);

        private static void BigSwing(MoveStyle style, float arc = 280f, float turn = 110f)
        {
            var p = Play(style);
            var s = p.State;
            Assert.Greater(s.Arc, arc, $"{style}: the weapon sweeps a big arc from the release to the end of the follow-through ({s.Arc:0}°)");
            Assert.Greater(s.ContactSpeed, 11f, $"{style}: fast through contact ({s.ContactSpeed:0.0} m/s)");
            Assert.Less(s.ContactSpeed, 24f, $"{style}: not absurdly fast ({s.ContactSpeed:0.0} m/s)");
            Assert.Greater(s.ContactSpeed, 1.2f * p.WindUpSpeed, $"{style}: much faster through contact than in the wind-up ({s.ContactSpeed:0.0} against {p.WindUpSpeed:0.0} m/s)");
            Assert.Greater(s.Depth, 0.12f, $"{style}: carried on past the aim point ({s.Depth * 100f:0} cm)");
            Assert.Greater(p.ChestTurn, turn, $"{style}: the body turns behind it ({p.ChestTurn:0}°)");
            Assert.GreaterOrEqual(p.Steps, 1, $"{style}: the feet step into it");
        }

        [Test]
        public void TheThrustRunsThemThrough()
        {
            var p = Play(MoveStyle.SwordThrust);
            var s = p.State;
            Assert.Greater(s.Depth, 0.45f, $"the point is driven deep past the aim point ({s.Depth * 100f:0} cm)");
            Assert.Greater(s.ContactSpeed, 8f, $"and fast ({s.ContactSpeed:0.0} m/s)");
            Assert.GreaterOrEqual(p.Steps, 1, "with a step");
        }
    }
}
