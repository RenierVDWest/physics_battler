using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Fight Rhythms: each style is a base and four postures; every posture stays balanced over the
    /// feet and clear of the unit's own body, without pops; the boxer feints before most of his
    /// swings; tired fighters posture less and drop their guard; postures publish their openings.
    /// </summary>
    [TestFixture]
    public class RhythmTests
    {
        private const float Dt = 1f / 60f;
        private static int built;

        internal static WeaponDefinition Sword()
        {
            var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
            sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordCleave), MoveDefinition.Create(MoveStyle.SwordThrust) };
            return sword;
        }

        internal static WeaponDefinition Club()
        {
            var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
            club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing), MoveDefinition.Create(MoveStyle.ClubSmash), MoveDefinition.Create(MoveStyle.ClubRising) };
            return club;
        }

        /// <summary>A unit armed as in the game: clubs for the small and large, sword and shield for the medium.</summary>
        internal static UnitController Build(string body, Vector3 at, float yaw, int team)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
            var rig = RigFile.Parse(File.ReadAllText(path));
            var profile = UnitProfile.CreateDefault();
            profile.body = body;
            profile.style = CombatStyle.CreateFor(body);   // (each body stands as its captured idle expects)
            var root = UnitBuilder.Build(rig, profile, $"Rhythm {body} {team} #{++built}", _ => null, 0);
            root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            var unit = root.GetComponent<UnitController>();
            unit.Profile = profile;
            unit.Team = team;
            typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
            unit.TempoOn = false;   // (the Fight Rhythm on its own: the tempo's stances have their own test, in TempoTests)
            if (body == "Medium")
            {
                unit.Equip(Sword(), WeaponDefinition.CreateDefault(WeaponKind.Shield));
            }
            else
            {
                unit.Equip(Club(), null);
            }

            return unit;
        }

        /// <summary>A unit and an opponent squared up at striking distance (the opponent stands in its plain guard).</summary>
        internal static (UnitController unit, UnitController opponent) Pair(string body, string opponentBody = "Medium")
        {
            var unit = Build(body, Vector3.zero, 0f, 0);
            var opponent = Build(opponentBody, Vector3.forward * 5f, 180f, 1);
            float distance = unit.Style.engageDistance * unit.Body.SizeFactor + 0.25f * opponent.Body.SizeFactor;
            opponent.Place(Vector3.forward * distance, 180f);
            opponent.RhythmOn = false;
            unit.Opponent = opponent;
            unit.FacePoint(opponent.Root.position);
            return (unit, opponent);
        }

        internal static void Step(IEnumerable<UnitController> units, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                foreach (var u in units)
                {
                    u.PreviewPose(Dt);
                }
            }
        }

        internal static void Destroy(params UnitController[] units)
        {
            foreach (var u in units)
            {
                if (u != null)
                {
                    Object.DestroyImmediate(u.gameObject);
                }
            }
        }

        private static readonly string[] Bodies = { "Large", "Medium", "Small" };

        [Test]
        public void EveryStyleIsABaseAndFourPostures()
        {
            foreach (RhythmStyle style in System.Enum.GetValues(typeof(RhythmStyle)))
            {
                var r = RhythmDefinition.Create(style);
                Assert.AreEqual(6, r.postures.Length, $"{style}: four postures, and one to gather and one to press (milestone 8)");
                Assert.IsTrue(r.bounceRate > 0f || r.weaponLoop != WeaponLoop.None || r.circling > 0f, $"{style}: a base that moves");
                int leadIns = 0;
                foreach (var p in r.postures)
                {
                    leadIns += p.leadIn ? 1 : 0;
                    Assert.Greater(p.duration, 0.2f, p.name);
                    Assert.Less(p.duration, 1.3f, p.name);
                    float last = 0f;
                    foreach (var k in p.keys)
                    {
                        Assert.GreaterOrEqual(k.at, last, $"{p.name}: keys in order");
                        last = k.at;
                    }

                    Assert.Greater(p.openings.Length, 0, $"{p.name}: an opening");
                    foreach (var o in p.openings)
                    {
                        Assert.IsTrue(o.from >= 0f && o.to <= 1f && o.from < o.to && o.parts != AimParts.None, $"{p.name}: opening window");
                    }

                    Assert.IsNotNull(r.Find(p.followedBy), $"{p.name}: followed by a posture of its own style ({p.followedBy})");
                    var end = p.Sample(1f);
                    Assert.Less(end.main.magnitude + end.off.magnitude + Mathf.Abs(end.lean) + Mathf.Abs(end.turn) + end.hips.magnitude, 1e-4f, $"{p.name}: back to the base at the end");
                }

                Assert.GreaterOrEqual(leadIns, 2, $"{style}: lead-ins to choose from");
            }
        }

        /// <summary>
        /// Every posture and the base, on the body it belongs to: the weight stays over the feet, the
        /// weapon and arms stay out of the unit's own body (as a played blow must), and nothing pops.
        /// </summary>
        [Test]
        public void PosturesStayBalancedAndClearOfTheBody()
        {
            var report = new StringBuilder();
            bool failed = false;
            foreach (string body in Bodies)
            {
                var (unit, opponent) = Pair(body);
                var both = new[] { unit, opponent };
                try
                {
                    Step(both, 30);
                    var postures = new List<PostureDefinition> { null };
                    postures.AddRange(unit.Rhythm.Definition.postures);
                    float leg = unit.Body.LegLength, size = unit.Body.SizeFactor;
                    float weaponLength = unit.Loadout.Main.length * 0.98f;
                    foreach (var posture in postures)
                    {
                        float com = 0f, clear = float.MaxValue, pop = 0f, feint = 0f, clearAt = 0f;
                        string clearWhat = "";
                        var last = new Vector3[PartInfo.Count];
                        var lastStep = new float[PartInfo.Count];
                        int frames = 0;
                        if (posture != null)
                        {
                            unit.Rhythm.Force(posture);
                        }

                        unit.Rhythm.EnergyOverride = 1f;
                        int limit = posture != null ? 200 : 120;
                        for (int i = 0; i < limit; i++)
                        {
                            Step(both);
                            if (posture != null && unit.Rhythm.Current != posture)
                            {
                                break;
                            }

                            // Over the feet: how far the weight is from the line between them (a long
                            // stance may rock well forward and back along it on purpose).
                            com = Mathf.Max(com, OffTheFeet(unit));
                            float c = BodyClearance.Measure(unit.Rig, unit.Anim, weaponLength, unit.Loadout.Scale, out var part, out bool blade);
                            if (c < clear)
                            {
                                clear = c;
                                clearWhat = $"{(blade ? "weapon" : "arm")} on the {part}";
                                clearAt = posture != null ? unit.Rhythm.Progress : 0f;
                            }

                            feint = Mathf.Max(feint, unit.Rhythm.Feint);
                            foreach (var id in PartInfo.All)
                            {
                                Vector3 p = unit.Anim.Part(id).position;
                                if (frames > 0)
                                {
                                    float d = Vector3.Distance(p, last[(int)id]);
                                    if (frames > 1)
                                    {
                                        if (d - 2f * lastStep[(int)id] > pop && System.Environment.GetEnvironmentVariable("RHYTHM_DEBUG") != null)
                                        {
                                            System.Console.WriteLine($"  pop {body} {(posture != null ? posture.Label : "base")} frame {frames} {id}: {(d - 2f * lastStep[(int)id]) * 100f:0.0} cm (walk {unit.Gait?.Weight:0.00}, posture {unit.Rhythm.Current?.Label} {unit.Rhythm.Progress:0.00}, stepping {unit.Stance.AnyStepping}, move {unit.Pose.Move != null})");
                                        }

                                        pop = Mathf.Max(pop, d - 2f * lastStep[(int)id]);
                                    }

                                    lastStep[(int)id] = d;
                                }

                                last[(int)id] = p;
                            }

                            frames++;
                        }

                        string name = posture != null ? posture.Label : "base";
                        bool bad = com > 0.17f * leg ||   // (0.17: since 9b the knees point where the hips do, which puts the Large Shift at 16.5%)
                                   clear < -(MovePlanner.OwnBodyClearance + 0.03f) * size || pop > 0.04f * size
                                   || (posture != null && !string.IsNullOrEmpty(posture.feintMove) && feint < 0.5f);
                        failed |= bad;
                        report.AppendLine($"{body} {name}: weight off the feet {com / leg * 100f:0}% of the leg, clearance {clear * 100f:0.0} cm ({clearWhat} at {clearAt:0.00}), pop {pop * 100f:0.0} cm, feint {feint:0.00}{(bad ? "  <-- out of bounds" : "")}");
                        Step(both, 20);
                    }
                }
                finally
                {
                    Destroy(both);
                }
            }

            System.Console.WriteLine(report.ToString());
            Assert.IsFalse(failed, report.ToString());
        }

        /// <summary>Your pattern: the boxer tends to feint right before he really swings.</summary>
        [Test]
        public void TheBoxerFeintsBeforeMostOfHisSwings()
        {
            var (unit, opponent) = Pair("Large");
            unit.DealsHits = false;
            opponent.DealsHits = false;
            var both = new List<UnitController> { unit, opponent };
            int blows = 0, ledIn = 0;
            float leadAt = -10f, now = 0f;
            void OnPosture(UnitController u, PostureDefinition p, bool lead)
            {
                if (u == unit && lead)
                {
                    leadAt = now;
                }
            }

            void OnMove(UnitController u, MoveState m)
            {
                if (u == unit)
                {
                    blows++;
                    ledIn += now - leadAt < 1.2f ? 1 : 0;
                }
            }

            UnitController.PostureStarted += OnPosture;
            UnitController.MoveStarted += OnMove;
            try
            {
                var brawl = new Brawl(3);
                for (int i = 0; i < 60 * 40; i++)
                {
                    now = i * Dt;
                    brawl.Step(both, Dt, now);
                    Step(both);
                }
            }
            finally
            {
                UnitController.PostureStarted -= OnPosture;
                UnitController.MoveStarted -= OnMove;
                Destroy(both.ToArray());
            }

            Assert.GreaterOrEqual(blows, 6, "it fights");
            Assert.GreaterOrEqual(ledIn, 0.55f * blows, $"most blows come right after a lead-in ({ledIn} of {blows})");
        }

        [Test]
        public void TheBoxerRocksAsTheCaptureDoes()
        {
            // The large unit's base is the captured boxing idle: the hips rock forward and down onto
            // the lead leg and back up, round the loop at the capture's pace (a little slower on the
            // bigger body), the bare lead hand up in front of the shoulder.
            var (unit, opponent) = Pair("Large");
            var both = new[] { unit, opponent };
            try
            {
                Assert.IsFalse(unit.Rhythm.Definition.clip.IsEmpty, "the boxer has a captured loop");
                unit.Rhythm.Auto = false;
                unit.Rhythm.EnergyOverride = 1f;
                Step(both, 60);
                float leg = unit.Body.LegLength;
                var fore = new List<float>();
                var drop = new List<float>();
                float handHigh = float.MaxValue;
                for (int i = 0; i < 300; i++)
                {
                    Step(both);
                    Vector3 feet = 0.5f * (unit.Anim.Part(PartId.FootL).position + unit.Anim.Part(PartId.FootR).position);
                    Vector3 hips = unit.Anim.Part(PartId.Pelvis).position;
                    fore.Add(Vector3.Dot(hips - feet, unit.Root.forward) / leg);
                    drop.Add(hips.y / leg);
                    handHigh = Mathf.Min(handHigh, unit.Anim.Part(PartId.HandL).position.y - unit.Anim.Part(PartId.Chest).position.y);
                }

                float rock = Max(fore) - Min(fore), bob = Max(drop) - Min(drop);
                System.Console.WriteLine($"rock {rock:0.000} legs, bob {bob:0.000} legs, lead hand at least {handHigh:0.00} m above the chest");
                Assert.Greater(rock, 0.12f, "the hips rock fore and aft as the capture's (0.18 legs)");
                Assert.Greater(bob, 0.04f, "and bob (0.07 legs)");
                Assert.Greater(handHigh, 0.1f, "the lead hand stays up");

                // The loop's length: the rock's period, against the capture's on this body.
                float expected = unit.Rhythm.Definition.clip.length * Mathf.Sqrt(unit.Body.SizeFactor);
                int bestLag = 0;
                float best = float.MinValue;
                for (int lag = 60; lag < 200; lag++)
                {
                    float c = 0f;
                    for (int i = 0; i + lag < fore.Count; i++)
                    {
                        c += (fore[i] - 0.5f * (Max(fore) + Min(fore))) * (fore[i + lag] - 0.5f * (Max(fore) + Min(fore)));
                    }

                    c /= fore.Count - lag;
                    if (c > best)
                    {
                        best = c;
                        bestLag = lag;
                    }
                }

                Assert.AreEqual(expected, bestLag / 60f, 0.2f, "round the loop at the capture's pace");
            }
            finally
            {
                Destroy(both);
            }
        }

        /// <summary>How far the pose's weight is from the line between the feet (horizontal metres).</summary>
        internal static float OffTheFeet(UnitController unit)
        {
            Vector3 a = unit.Stance.Position(Stance.Left), b = unit.Stance.Position(Stance.Right), c = unit.Pose.CenterOfMass;
            a.y = b.y = c.y = 0f;
            Vector3 ab = b - a;
            float u = ab.sqrMagnitude > 1e-8f ? Mathf.Clamp01(Vector3.Dot(c - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector3.Distance(c, a + ab * u);
        }

        private static float Max(List<float> values)
        {
            float m = float.MinValue;
            foreach (float v in values) m = Mathf.Max(m, v);
            return m;
        }

        private static float Min(List<float> values)
        {
            float m = float.MaxValue;
            foreach (float v in values) m = Mathf.Min(m, v);
            return m;
        }

        [Test]
        public void TiredFightersPostureLessAndDropTheirGuard()
        {
            var (unit, opponent) = Pair("Large");
            var both = new[] { unit, opponent };
            int count = 0;
            void OnPosture(UnitController u, PostureDefinition p, bool lead) => count += u == unit ? 1 : 0;
            UnitController.PostureStarted += OnPosture;
            try
            {
                unit.Rhythm.EnergyOverride = 1f;
                float freshGuard = unit.Rhythm.Sample(unit.Body.LegLength, unit.Body.SizeFactor).off.y;
                Step(both, 60 * 60);
                int fresh = count;
                count = 0;
                unit.Rhythm.EnergyOverride = 0.3f;
                float tiredGuard = unit.Rhythm.Sample(unit.Body.LegLength, unit.Body.SizeFactor).off.y;
                Step(both, 60 * 60);
                int tired = count;
                Assert.Greater(fresh, 20, $"a fresh boxer postures often ({fresh} a minute)");
                Assert.LessOrEqual(tired, 0.5f * fresh, $"tired, at most half as often ({tired} against {fresh} a minute)");
                Assert.Greater(freshGuard - tiredGuard, 0.08f, $"and the guard sags ({(freshGuard - tiredGuard) * 100f:0} cm)");
                Assert.IsTrue((unit.Openings & AimParts.Head) != 0, "leaving the head open");
            }
            finally
            {
                UnitController.PostureStarted -= OnPosture;
                Destroy(both);
            }
        }

        [Test]
        public void PosturesPublishTheirOpenings()
        {
            var (unit, opponent) = Pair("Medium");
            var both = new[] { unit, opponent };
            try
            {
                Step(both, 20);
                unit.Rhythm.EnergyOverride = 1f;
                var peek = unit.Rhythm.Definition.Find("Peek");
                unit.Rhythm.Force(peek);
                bool open = false;
                for (int i = 0; i < 60 && unit.Rhythm.Current == peek; i++)
                {
                    Step(both);
                    open |= (unit.Openings & AimParts.Head) != 0;
                }

                Assert.IsTrue(open, "the peek leaves the head open");
                Step(both, 2);
                Assert.AreEqual(AimParts.None, unit.Openings & AimParts.Head, "and closes it after");
            }
            finally
            {
                Destroy(both);
            }
        }

        /// <summary>A feint plays the blow's wind-up but never swings: no blow starts, nothing is struck.</summary>
        [Test]
        public void AFeintNeverStrikes()
        {
            var (unit, opponent) = Pair("Large");
            var both = new[] { unit, opponent };
            int hits = 0;
            void OnHit(HitReport h) => hits += h.Attacker == unit ? 1 : 0;
            UnitController.Hit += OnHit;
            try
            {
                Step(both, 20);
                unit.Rhythm.Force(unit.Rhythm.Definition.Find("ClubFeint"));
                bool feinting = false;
                for (int i = 0; i < 80; i++)
                {
                    Step(both);
                    feinting |= unit.Pose.Move != null && unit.CurrentMove == null;
                    Assert.IsNull(unit.CurrentMove);
                }

                Assert.IsTrue(feinting, "the club's wind-up is posed");
                Assert.IsNull(unit.Pose.Move, "and gone after");
                Assert.AreEqual(0, hits);
            }
            finally
            {
                UnitController.Hit -= OnHit;
                Destroy(both);
            }
        }
    }
}
