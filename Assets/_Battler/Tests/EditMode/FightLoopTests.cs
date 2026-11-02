using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PhysicsBattler.Battle;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 6: the fight loop. Two teams of three start at opposite ends of the board and fight
    /// it out with no help: they find targets, walk tile to tile (never two on one tile), strike, and
    /// the fight ends with one team unable to fight.
    /// </summary>
    [TestFixture]
    public class FightLoopTests
    {
        private static readonly MethodInfo FixedUpdate = typeof(UnitController).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static List<UnitController> Teams(BoardGrid grid, (string body, Tile tile)[][] lineup)
        {
            var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
            sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
            var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
            club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising), MoveDefinition.Create(MoveStyle.ClubBackhand), MoveDefinition.Create(MoveStyle.ClubFan), MoveDefinition.Create(MoveStyle.ClubFanBackhand) };
            var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
            var units = new List<UnitController>();
            for (int team = 0; team < lineup.Length; team++)
            {
                foreach (var (body, tile) in lineup[team])
                {
                    string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
                    var profile = UnitProfile.CreateDefault();
                    profile.body = body;
                    profile.style = CombatStyle.CreateFor(body);
                    if (body == "Small") { profile.maxHealth = 60f; profile.maxPoise = 25f; }
                    if (body == "Large") { profile.maxHealth = 220f; profile.maxPoise = 120f; }
                    var root = UnitBuilder.Build(RigFile.Parse(File.ReadAllText(path)), profile, $"{(team == 0 ? "A" : "B")} {body}", _ => null, 0);
                    root.transform.SetPositionAndRotation(grid.Center(tile), Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f));
                    var unit = root.GetComponent<UnitController>();
                    unit.Profile = profile;
                    unit.Team = team;
                    typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
                    if (body == "Medium") unit.Equip(sword, shield); else unit.Equip(club, null);
                    units.Add(unit);
                }
            }

            return units;
        }

        internal static void Destroy(List<UnitController> units)
        {
            foreach (var u in units)
            {
                if (u != null)
                {
                    typeof(UnitController).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(u, null);
                    Object.DestroyImmediate(u.gameObject);
                }
            }
        }

        /// <summary>Steps a fight (the loop, each unit's own step, then the stand-in physics); in how many steps two standing bodies overlapped.</summary>
        internal static int Run(FightLoop loop, List<UnitController> units, float seconds, System.Func<bool> stop = null)
        {
            int shared = 0;
            float now = 0f;
            for (int step = 0; step < seconds * 60f && !loop.Over && (stop == null || !stop()); step++)
            {
                now += 1f / 60f;
                loop.Step(units, 1f / 60f, now);
                foreach (var u in units)
                {
                    FixedUpdate.Invoke(u, null);
                }

                foreach (var u in units)
                {
                    BalanceTests.Integrate(u, 1f / 60f);
                }

                shared += FightLoop.Overlapping(units) > 0 ? 1 : 0;
            }

            return shared;
        }

        [Test]
        public void AThreeAgainstThreeFightPlaysOutWithNoHelp()
        {
            // The milestone's gate.
            var grid = new BoardGrid();
            var units = Teams(grid, new[]
            {
                new[] { ("Small", new Tile(2, 1)), ("Medium", new Tile(3, 1)), ("Large", new Tile(4, 1)) },
                new[] { ("Medium", new Tile(4, 6)), ("Large", new Tile(2, 6)), ("Small", new Tile(3, 6)) },
            });
            var loop = new FightLoop(7) { Grid = grid, TimeLimit = 240f };
            int blows = 0;
            void OnMove(UnitController u, MoveState s) => blows++;
            UnitController.MoveStarted += OnMove;
            try
            {
                int shared = Run(loop, units, 260f);
                System.Console.WriteLine($"{loop.Result}; {blows} blows; {shared} steps with bodies overlapping; {loop.Stalls} stalls");
                Assert.IsTrue(loop.Over, "the fight ended by itself");
                Assert.AreNotEqual(-1, loop.Winner, "one team won: " + loop.Result);
                Assert.IsFalse(loop.Result.Contains("time"), "before the time limit: " + loop.Result);
                Assert.Greater(blows, 12, "they fought");
                Assert.Less(shared, 0.02f * (loop.EndedAt - loop.StartedAt) * 60f, "bodies never overlap (but for a moment in a scrum)");
                int left = 0;
                foreach (var u in units)
                {
                    left += u.Team == loop.Winner && FightLoop.CanFight(u) ? 1 : 0;
                    Assert.IsFalse(u.Team != loop.Winner && FightLoop.CanFight(u), $"{u.name} on the losing side can't fight any more");
                }

                Assert.Greater(left, 0);
            }
            finally
            {
                UnitController.MoveStarted -= OnMove;
                Destroy(units);
            }
        }

        [Test]
        public void UnitsWalkFreelyAndStopInReachOfTheirTarget()
        {
            var grid = new BoardGrid();
            var units = Teams(grid, new[]
            {
                new[] { ("Medium", new Tile(1, 1)) },
                new[] { ("Medium", new Tile(6, 6)) },
            });
            var loop = new FightLoop(3) { Grid = grid };
            try
            {
                // Until one strikes: they should have closed in, straight across the board (no tile path).
                Run(loop, units, 40f, () => units[0].IsAttacking || units[1].IsAttacking);
                Assert.IsTrue(units[0].IsAttacking || units[1].IsAttacking, "they met and one struck");
                var striker = units[0].IsAttacking ? units[0] : units[1];
                var other = striker == units[0] ? units[1] : units[0];
                Vector3 apart = other.Root.position - striker.Root.position;
                apart.y = 0f;
                float engage = FightLoop.EngageDistance(striker, other);
                Assert.LessOrEqual(apart.magnitude, engage + 0.9f * striker.Body.SizeFactor, "it struck from its striking distance (a lunge covers the rest)");
                Assert.Greater(apart.magnitude, FightLoop.BodyRadius(striker) + FightLoop.BodyRadius(other), "and not from inside the other's body");
                // Off the tile centres: they met on the diagonal, where no tile path would have put them.
                Vector3 mid = 0.5f * (units[0].Root.position + units[1].Root.position);
                Assert.Less(Mathf.Abs(mid.x - mid.z), 1.2f * grid.tileSize, "they met near the diagonal between their corners");
            }
            finally
            {
                Destroy(units);
            }
        }

        [Test]
        public void ThreeOnOneSpreadRoundTheTargetAndStayOnTheBoard()
        {
            // Three attackers on one target in a corner: they spread round it (no two bodies inside
            // each other), and the board's edge holds everyone.
            var grid = new BoardGrid();
            var units = Teams(grid, new[]
            {
                new[] { ("Medium", new Tile(2, 2)), ("Small", new Tile(3, 2)), ("Large", new Tile(1, 2)) },
                new[] { ("Large", new Tile(0, 7)) },
            });
            foreach (var u in units)
            {
                u.DealsHits = false;   // (no one gets hurt: just the movement)
            }

            var loop = new FightLoop(11) { Grid = grid, TimeLimit = 999f };
            try
            {
                int overlapped = Run(loop, units, 12f);
                Assert.Less(overlapped, 0.05f * 12f * 60f, $"bodies hardly ever overlap ({overlapped} steps)");
                var target = units[3];
                foreach (var u in units)
                {
                    Vector3 local = FightLoop.BodyAt(u) - grid.origin;
                    Assert.LessOrEqual(Mathf.Abs(local.x), 0.5f * grid.Width + 1e-3f, $"{u.name} on the board");
                    Assert.LessOrEqual(Mathf.Abs(local.z), 0.5f * grid.Depth + 1e-3f, $"{u.name} on the board");
                    if (u != target)
                    {
                        float d = Vector3.Distance(FightLoop.BodyAt(u), FightLoop.BodyAt(target));
                        Assert.Less(d, FightLoop.EngageDistance(u, target) + 1.2f, $"{u.name} got to its target ({d:0.00} m)");
                    }
                }

                // Round it: the attackers' bearings from the target spread over more than a narrow wedge.
                float min = float.MaxValue, max = float.MinValue;
                for (int i = 0; i < 3; i++)
                {
                    Vector3 d = FightLoop.BodyAt(units[i]) - FightLoop.BodyAt(target);
                    float a = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                    min = Mathf.Min(min, a);
                    max = Mathf.Max(max, a);
                }

                Assert.Greater(max - min, 40f, $"spread round the target ({max - min:0}° between the outermost)");
            }
            finally
            {
                Destroy(units);
            }
        }

        [Test]
        public void AUnitThatCantFightIsOutOfIt()
        {
            var grid = new BoardGrid();
            var units = Teams(grid, new[]
            {
                new[] { ("Medium", new Tile(3, 3)) },
                new[] { ("Medium", new Tile(3, 5)) },
            });
            var loop = new FightLoop(5) { Grid = grid };
            try
            {
                Run(loop, units, 1f);
                Assert.IsFalse(loop.Over);
                units[1].Sever(PartId.ForearmR, Vector3.right);   // (its sword arm: it can't fight on)
                Run(loop, units, 1f);
                Assert.IsTrue(loop.Over, "over: the other side can't fight");
                Assert.AreEqual(0, loop.Winner);
            }
            finally
            {
                Destroy(units);
            }
        }

        [Test]
        public void AttackersAimAtOpeningsTheyRead()
        {
            // Every planned blow at a target whose head is open aims at the head when it can.
            var grid = new BoardGrid();
            var units = Teams(grid, new[] { new[] { ("Medium", new Tile(3, 3)) }, new[] { ("Medium", new Tile(3, 4)) } });
            try
            {
                var attacker = units[0];
                var target = units[1];
                RhythmTests.Step(units, 20);
                var random = new System.Random(9);
                int head = 0, plain = 0, tries = 40;
                for (int i = 0; i < tries; i++)
                {
                    var s = MovePlanner.Plan(attacker, target, random, AimParts.Head, 50f);
                    head += s != null && s.Part == AimPart.Head ? 1 : 0;
                    var p = MovePlanner.Plan(attacker, target, random);
                    plain += p != null && p.Part == AimPart.Head ? 1 : 0;
                }

                System.Console.WriteLine($"head chosen {head} of {tries} reading the opening, {plain} without");
                Assert.Greater(head, plain + tries / 4, "an open head is aimed at far more often");
            }
            finally
            {
                Destroy(units);
            }
        }
    }
}
