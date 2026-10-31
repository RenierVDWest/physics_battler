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
            club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising) };
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

        /// <summary>Steps a fight (the loop, each unit's own step, then the stand-in physics); how many steps two standing units shared a tile.</summary>
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

                var on = new HashSet<long>();
                foreach (var u in units)
                {
                    if (u.IsAlive && u.State == UnitState.Standing && loop.TileOf(u, out var t) && !on.Add(((long)t.column << 16) | (uint)t.row))
                    {
                        shared++;
                    }
                }
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
                System.Console.WriteLine($"{loop.Result}; {blows} blows; {shared} steps with two standing units on one tile");
                Assert.IsTrue(loop.Over, "the fight ended by itself");
                Assert.AreNotEqual(-1, loop.Winner, "one team won: " + loop.Result);
                Assert.IsFalse(loop.Result.Contains("time"), "before the time limit: " + loop.Result);
                Assert.Greater(blows, 12, "they fought");
                Assert.Less(shared, 0.02f * (loop.EndedAt - loop.StartedAt) * 60f, "never two on one tile (but in passing)");
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
        public void UnitsWalkTileToTileAndStopNextToTheirTarget()
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
                // Until one strikes: they should have closed in to neighbouring tiles.
                Run(loop, units, 40f, () => units[0].IsAttacking || units[1].IsAttacking);
                Assert.IsTrue(units[0].IsAttacking || units[1].IsAttacking, "they met and one struck");
                // (The one striking stands on its tile; the other may still be stepping onto its own.)
                var striker = units[0].IsAttacking ? units[0] : units[1];
                var other = striker == units[0] ? units[1] : units[0];
                Vector3 apart = other.Root.position - striker.Root.position;
                apart.y = 0f;
                Assert.LessOrEqual(apart.magnitude, 2f * grid.tileSize, "they met: within a tile and a lunge");
                Assert.IsTrue(loop.TileOf(striker, out var t));
                Vector3 off = striker.Root.position - grid.Center(t);
                off.y = 0f;
                Assert.LessOrEqual(off.magnitude, 0.4f * grid.tileSize, $"{striker.name} on its tile");
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
