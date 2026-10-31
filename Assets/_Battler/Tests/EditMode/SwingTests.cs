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
