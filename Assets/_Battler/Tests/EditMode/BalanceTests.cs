using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 4's gate: a launched body hurts who it hits; every unit gets up. And the stumble:
    /// a hard hit knocks a unit off balance, it catches itself with quick steps and ends up further
    /// along the blow's direction.
    ///
    /// These run the units' physics steps (<see cref="Simulate"/>): in Unity, PhysX moves the
    /// bodies; here a plain integrator does (gravity, the ground, friction), so a ragdoll's parts
    /// fall each on its own rather than held by their joints. Enough for what is tested: where the
    /// body goes, when it comes to rest, how it gets up.
    /// </summary>
    [TestFixture]
    public class BalanceTests
    {
        private static int built;
        private const float Dt = 1f / 60f;

        internal static UnitController Build(string body, Vector3 at, float yaw, int team)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
            var rig = RigFile.Parse(File.ReadAllText(path));
            var profile = UnitProfile.CreateDefault();
            profile.body = body;
            var root = UnitBuilder.Build(rig, profile, $"{body} {team} #{++built}", _ => null, 0);
            root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            var unit = root.GetComponent<UnitController>();
            unit.Profile = profile;
            unit.Team = team;
            typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
            return unit;
        }

        internal static void Destroy(params UnitController[] units)
        {
            foreach (var unit in units)
            {
                if (unit != null)
                {
                    Object.DestroyImmediate(unit.gameObject);
                }
            }
        }

        private static readonly MethodInfo FixedUpdate = typeof(UnitController).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>Physics steps: each unit's own step, then the bodies move.</summary>
        internal static void Simulate(IEnumerable<UnitController> units, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                foreach (var unit in units)
                {
                    FixedUpdate.Invoke(unit, null);
                }

                foreach (var unit in units)
                {
                    Integrate(unit, Dt);
                }
            }
        }

        /// <summary>Gravity, the ground (each part's centre stays above it), friction on it.</summary>
        internal static void Integrate(UnitController unit, float dt)
        {
            float floor = 0.06f * unit.Body.SizeFactor;
            foreach (var id in PartInfo.All)
            {
                var body = unit.Rig.Part(id).body;
                if (body.isKinematic)
                {
                    continue;
                }

                Vector3 v = body.linearVelocity + Physics.gravity * dt;
                Vector3 p = body.position + v * dt;
                if (p.y < floor)
                {
                    p.y = floor;
                    v.y = Mathf.Max(0f, v.y);
                    float keep = Mathf.Exp(-dt * 8f);
                    v.x *= keep;
                    v.z *= keep;
                    body.angularVelocity *= keep;
                }

                body.linearVelocity = v;
                body.position = p;
                Vector3 w = body.angularVelocity;
                float angle = w.magnitude * dt * Mathf.Rad2Deg;
                if (angle > 1e-5f)
                {
                    body.rotation = Quaternion.AngleAxis(angle, w.normalized) * body.rotation;
                }
            }
        }

        /// <summary>Lays the physical body flat on the ground about its pelvis: tipped forward (face down) or back (face up).</summary>
        internal static void Lay(UnitController unit, bool faceDown)
        {
            var pelvis = unit.Rig.Part(PartId.Pelvis).body;
            Vector3 pivot = pelvis.position;
            var tip = Quaternion.AngleAxis(faceDown ? 90f : -90f, unit.Root.right);
            float lowest = float.MaxValue;
            foreach (var part in unit.Rig.Parts)
            {
                lowest = Mathf.Min(lowest, (pivot + tip * (part.body.position - pivot)).y);
            }

            float up = 0.06f * unit.Body.SizeFactor - lowest;
            foreach (var part in unit.Rig.Parts)
            {
                part.body.position = pivot + tip * (part.body.position - pivot) + Vector3.up * up;
                part.body.rotation = tip * part.body.rotation;
                part.body.linearVelocity = Vector3.zero;
                part.body.angularVelocity = Vector3.zero;
            }
        }

        // ---- Stumbling ---------------------------------------------------------------------------

        /// <summary>The user's stagger: knocked off balance, the unit stumbles back with catch steps, ends up further back, and stands.</summary>
        [Test]
        public void AHardHitMakesAUnitStumbleAndCatchItself()
        {
            foreach (string body in new[] { "Small", "Medium", "Large" })
            {
                var unit = Build(body, Vector3.zero, 0f, 0);
                try
                {
                    Simulate(new[] { unit }, 30);
                    float k = Mathf.Sqrt(unit.Body.SizeFactor);
                    Vector3 start = unit.Root.position;
                    int stepsBefore = unit.Stance.StepsTaken;
                    unit.Knock(Vector3.back * (1.6f * k));
                    Assert.AreEqual(UnitState.Stumbling, unit.State, $"{body}: a 1.6 m/s push sends it stumbling");
                    float lean = 0f;
                    int t = 0;
                    for (; t < 300 && unit.State == UnitState.Stumbling; t++)
                    {
                        Simulate(new[] { unit });
                        lean = Mathf.Max(lean, unit.Pose.Topple.magnitude * Mathf.Rad2Deg);
                    }

                    int steps = unit.Stance.StepsTaken - stepsBefore;
                    float back = Vector3.Dot(unit.Root.position - start, Vector3.back);
                    Assert.AreEqual(UnitState.Standing, unit.State, $"{body}: it regains its balance ({t * Dt:0.0} s)");
                    Assert.GreaterOrEqual(steps, 2, $"{body}: with catch steps");
                    Assert.Greater(back, 0.3f * unit.Body.SizeFactor, $"{body}: and ends up further back ({back:0.00} m)");
                    Assert.Less(back, 1.5f * unit.Body.SizeFactor, $"{body}: but not far ({back:0.00} m)");
                    Assert.Greater(lean, 10f, $"{body}: leaning with the fall ({lean:0}°)");
                    Simulate(new[] { unit }, 60);
                    for (int f = 0; f < 2; f++)
                    {
                        Assert.Less(unit.Stance.Error(f, unit.Root.position, unit.FacingYaw), 0.2f * unit.Body.LegLength, $"{body}: feet back under it");
                    }
                }
                finally
                {
                    Destroy(unit);
                }
            }
        }

        [Test]
        public void ABrokenPoiseAlwaysStumblesAndASlightPushOnlySlides()
        {
            var unit = Build("Medium", Vector3.zero, 0f, 0);
            try
            {
                Simulate(new[] { unit }, 10);
                unit.Knock(Vector3.back * 0.3f);
                Assert.AreEqual(UnitState.Standing, unit.State, "a slight push only slides it");
                Assert.Greater(unit.Knockback.magnitude, 0.2f);
                Simulate(new[] { unit }, 60);
                unit.Knock(Vector3.back * 0.3f, offBalance: true);
                Assert.AreEqual(UnitState.Stumbling, unit.State, "off balance, it stumbles");
                Assert.GreaterOrEqual(unit.StumbleVelocity.magnitude, 1f);
            }
            finally
            {
                Destroy(unit);
            }
        }

        [Test]
        public void HarderPushesKnockDownThenLaunch()
        {
            var unit = Build("Medium", Vector3.zero, 0f, 0);
            try
            {
                Simulate(new[] { unit }, 10);
                unit.Knock(Vector3.back * 2.6f);
                Assert.AreEqual(UnitState.Down, unit.State, "2.6 m/s knocks it down");
                Vector3 chest = unit.Rig.Part(PartId.Chest).body.linearVelocity, foot = unit.Rig.Part(PartId.FootL).body.linearVelocity;
                Assert.Greater(Vector3.Dot(chest, Vector3.back), Vector3.Dot(foot, Vector3.back) + 1f, "the upper body goes more than the feet: it topples");

                var other = Build("Medium", new Vector3(5f, 0f, 0f), 0f, 0);
                try
                {
                    Simulate(new[] { other }, 10);
                    float stood = other.Rig.Part(PartId.Pelvis).body.position.y;
                    other.Knock(Vector3.back * 5f);
                    Assert.AreEqual(UnitState.Down, other.State, "5 m/s launches it");
                    float top = 0f;
                    for (int i = 0; i < 30; i++)
                    {
                        Simulate(new[] { other });
                        top = Mathf.Max(top, other.Rig.Part(PartId.Pelvis).body.position.y);
                    }

                    Assert.Greater(top, stood + 0.05f, "off its feet, into the air");
                }
                finally
                {
                    Destroy(other);
                }
            }
            finally
            {
                Destroy(unit);
            }
        }

        [Test]
        public void AShieldBlockNeverKnocksDown()
        {
            var unit = Build("Medium", Vector3.zero, 0f, 0);
            try
            {
                Simulate(new[] { unit }, 10);
                unit.Knock(Vector3.back * 3.5f, false, blocked: true);
                Assert.AreEqual(UnitState.Stumbling, unit.State);
            }
            finally
            {
                Destroy(unit);
            }
        }

        // ---- The gate ----------------------------------------------------------------------------

        /// <summary>Milestone 4's gate, first half: a launched body hurts who it hits, and they react.</summary>
        [Test]
        public void ALaunchedBodyHurtsWhoItHits()
        {
            var flier = Build("Small", Vector3.zero, 90f, 1);
            var struck = Build("Medium", new Vector3(1.3f, 0f, 0f), 270f, 1);
            var everyone = new[] { flier, struck };
            var impacts = new List<HitReport>();
            void OnHit(HitReport h)
            {
                if (h.Impact)
                {
                    impacts.Add(h);
                }
            }

            UnitController.Hit += OnHit;
            try
            {
                Simulate(everyone, 20);
                float health = struck.Health, own = flier.Health;
                flier.Knock(Vector3.right * 8f);
                Assert.AreEqual(UnitState.Down, flier.State, "launched");
                bool reacted = false;
                for (int i = 0; i < 90; i++)
                {
                    Simulate(everyone);
                    reacted |= struck.State != UnitState.Standing || struck.Knockback.magnitude > 0.1f;
                }

                var hit = impacts.Find(h => h.Struck == struck);
                Assert.IsNotNull(hit, "the thrown body strikes the unit in its way");
                Assert.IsTrue(hit.Attacker == flier);
                Assert.Less(struck.Health, health, "and hurts it");
                Assert.Greater(hit.Result.Impulse.magnitude, 20f, $"with weight ({hit.Result.Impulse.magnitude:0} N·s)");
                Assert.IsTrue(reacted, "which reacts: knocked back, off balance or down");
                Assert.Less(flier.Health, own, "the thrown body is hurt too");
                Assert.AreEqual(1, impacts.FindAll(h => h.Struck == struck).Count, "once, not every step it touches");
            }
            finally
            {
                UnitController.Hit -= OnHit;
                Destroy(everyone);
            }
        }

        /// <summary>Two thrown bodies meeting count as one impact, not one each way.</summary>
        [Test]
        public void TwoThrownBodiesMeetOnce()
        {
            var a = Build("Small", Vector3.zero, 90f, 0);
            var b = Build("Small", new Vector3(1.4f, 0f, 0f), 270f, 1);
            var everyone = new[] { a, b };
            var impacts = new List<HitReport>();
            void OnHit(HitReport h)
            {
                if (h.Impact && h.Result.Impulse.sqrMagnitude > 0f)
                {
                    impacts.Add(h);
                }
            }

            UnitController.Hit += OnHit;
            try
            {
                Simulate(everyone, 10);
                a.Knock(Vector3.right * 8f);
                b.Knock(Vector3.left * 8f);
                Simulate(everyone, 25);
                Assert.AreEqual(1, impacts.Count, $"one impact ({impacts.Count})");
            }
            finally
            {
                UnitController.Hit -= OnHit;
                Destroy(everyone);
            }
        }

        /// <summary>
        /// The gate as the sandbox's launch test (L) plays it: a large unit's club swing launches a
        /// small one into the unit standing where the blow throws it, which is hurt; both get up.
        /// </summary>
        [Test]
        public void AClubSwingLaunchesASmallUnitIntoTheNext()
        {
            var attacker = Build("Large", Vector3.zero, 90f, 0);
            var target = Build("Small", new Vector3(5f, 0f, 0f), 270f, 1);
            var next = Build("Medium", new Vector3(5f, 0f, 5f), 270f, 1);
            var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
            club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing) };
            attacker.Equip(club, null);
            var everyone = new[] { attacker, target, next };
            var hits = new List<HitReport>();
            void OnHit(HitReport h) => hits.Add(h);
            UnitController.Hit += OnHit;
            try
            {
                float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                target.Place(Vector3.right * distance, 270f);
                Simulate(everyone, 20);
                MoveState state = null;
                foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Head })
                {
                    if (state == null)
                    {
                        var s = MovePlanner.Prepare(attacker, target, club.moves[0], part);
                        state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                    }
                }

                Assert.IsNotNull(state, "the large unit can swing at the small one");
                float c = state.Move.ContactProgress;
                Vector3 along = state.PathPoint(Mathf.Min(1f, c + 0.01f)) - state.PathPoint(Mathf.Max(0f, c - 0.01f));
                along.y = 0f;
                along.Normalize();
                next.Place(target.Root.position + along * (0.9f * target.Body.SizeFactor + 0.7f * next.Body.SizeFactor), 0f);
                Simulate(everyone, 10);
                float health = next.Health;
                Assert.IsTrue(attacker.StartMove(state));
                bool launched = false, nextHurt = false;
                int t = 0;
                for (; t < 60 * 10; t++)
                {
                    Simulate(everyone);
                    launched |= target.State == UnitState.Down;
                    nextHurt |= next.Health < health;
                    if (launched && nextHurt && target.State == UnitState.Standing && next.State == UnitState.Standing && !attacker.IsAttacking)
                    {
                        break;
                    }
                }

                var blow = hits.Find(h => h.Attacker == attacker && h.Struck == target);
                Assert.IsNotNull(blow, $"the swing lands (hits: {string.Join(", ", hits.ConvertAll(h => $"{h.Attacker.name}>{h.Struck.name}"))})");
                Assert.IsTrue(launched, $"and throws the small unit off its feet (state {target.State})");
                Assert.IsNotNull(hits.Find(h => h.Impact && h.Attacker == target && h.Struck == next), "into the unit beyond");
                Assert.IsTrue(nextHurt, "which is hurt");
                Assert.AreEqual(UnitState.Standing, target.State, $"the small unit gets up ({t / 60f:0.0} s)");
                Assert.AreEqual(UnitState.Standing, next.State, "and so does the other, if it fell");
            }
            finally
            {
                UnitController.Hit -= OnHit;
                Destroy(everyone);
            }
        }

        /// <summary>Milestone 4's gate, second half: every unit gets up, face down or face up, back where it lay, without a pop.</summary>
        [Test]
        public void EveryUnitGetsUp()
        {
            foreach (string body in new[] { "Small", "Medium", "Large" })
            {
                foreach (bool faceDown in new[] { true, false })
                {
                    var unit = Build(body, Vector3.zero, 30f, 0);
                    var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
                    unit.Equip(sword, null);
                    string what = $"{body} {(faceDown ? "face down" : "face up")}";
                    try
                    {
                        Simulate(new[] { unit }, 10);
                        unit.Knock(unit.Root.forward * (faceDown ? 2.6f : -2.6f) * Mathf.Sqrt(unit.Body.SizeFactor));
                        Assert.AreEqual(UnitState.Down, unit.State, what);
                        Lay(unit, faceDown);
                        Vector3 lay = unit.Rig.Part(PartId.Pelvis).body.position;
                        // A pop: a part moving much further in one step than in the one before
                        // (a fast but smooth motion is fine).
                        float pop = 0f, fastest = 0f;
                        var last = new Vector3[PartInfo.Count];
                        var lastStep = new float[PartInfo.Count];
                        int seen = 0;
                        int t = 0;
                        for (; t < 60 * 8 && unit.State != UnitState.Standing; t++)
                        {
                            Simulate(new[] { unit });
                            if (unit.State == UnitState.GettingUp)
                            {
                                foreach (var id in PartInfo.All)
                                {
                                    Vector3 p = unit.Anim.Part(id).position;
                                    if (seen > 0)
                                    {
                                        float step = Vector3.Distance(p, last[(int)id]);
                                        fastest = Mathf.Max(fastest, step);
                                        if (seen > 1)
                                        {
                                            pop = Mathf.Max(pop, step - 2f * lastStep[(int)id]);
                                        }

                                        lastStep[(int)id] = step;
                                    }

                                    last[(int)id] = p;
                                }

                                seen++;
                            }
                        }

                        Assert.AreEqual(UnitState.Standing, unit.State, $"{what}: it gets up ({t * Dt:0.0} s)");
                        Assert.AreEqual(faceDown ? GetUp.Kind.FaceDown : GetUp.Kind.FaceUp, unit.LastGetUp, what);
                        Assert.Less(pop, 0.04f * Mathf.Max(1f, unit.Body.SizeFactor), $"{what}: no part of the pose pops ({pop * 100f:0} cm more in a step than in the one before)");
                        Assert.Less(fastest, 0.15f * Mathf.Max(1f, unit.Body.SizeFactor), $"{what}: or flies ({fastest * 100f:0} cm in a step)");
                        Vector3 moved = unit.Root.position - lay;
                        moved.y = 0f;
                        Assert.Less(moved.magnitude, 0.8f * unit.Body.SizeFactor, $"{what}: it stands up where it lay ({moved.magnitude:0.00} m away)");
                        Assert.IsTrue(unit.NeedsSpot, $"{what}: and is sent back onto a tile");

                        // Standing as before: the hips at their height, the step after the get-up seamless.
                        Vector3 pelvisBefore = unit.Anim.Part(PartId.Pelvis).position;
                        Simulate(new[] { unit });
                        Assert.Less(Vector3.Distance(unit.Anim.Part(PartId.Pelvis).position, pelvisBefore), 0.03f * unit.Body.SizeFactor, $"{what}: the standing pose takes over smoothly");
                        Simulate(new[] { unit }, 60);
                        float h = unit.Anim.Part(PartId.Pelvis).position.y - unit.Root.position.y;
                        Assert.Greater(h, 0.8f * unit.Body.PelvisHeight, $"{what}: standing");
                        Assert.AreEqual(UnitState.Standing, unit.State);
                    }
                    finally
                    {
                        Destroy(unit);
                    }
                }
            }
        }

        /// <summary>Once up, the brawl walks it to the middle of the nearest free tile before it fights on.</summary>
        [Test]
        public void BackOntoTheNearestFreeTile()
        {
            var unit = Build("Medium", new Vector3(0.5f, 0f, 0.3f), 0f, 0);
            var enemy = Build("Medium", new Vector3(0.8f, 0f, 1.6f + 0.8f), 180f, 1);
            var everyone = new List<UnitController> { unit, enemy };
            var grid = new BoardGrid();
            var brawl = new Brawl(1) { Grid = grid, Fight = false };
            try
            {
                Simulate(everyone, 5);
                unit.NeedsSpot = true;
                for (int i = 0; i < 60 * 4 && unit.NeedsSpot; i++)
                {
                    brawl.Step(everyone, Dt, i * Dt);
                    Simulate(everyone);
                }

                Assert.IsFalse(unit.NeedsSpot, "it gets there");
                var tile = grid.TileAt(unit.Root.position);
                Vector3 off = unit.Root.position - grid.Center(tile);
                off.y = 0f;
                Assert.Less(off.magnitude, 0.08f, $"in the middle of tile {tile}");
                Assert.AreNotEqual(grid.TileAt(enemy.Root.position).ToString(), tile.ToString(), "not the enemy's");
                Assert.AreEqual("E5", tile.ToString(), "the nearest free one");
            }
            finally
            {
                Destroy(everyone.ToArray());
            }
        }
    }
}
