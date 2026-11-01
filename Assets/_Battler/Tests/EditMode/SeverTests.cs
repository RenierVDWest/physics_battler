using System.Collections.Generic;
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
    /// Milestone 5: a sword's edge crossing a joint fast and hard cuts the limb off there; the unit
    /// loses what the limb did (its weapon, its footing, its life) and bleeds. Clubs never sever.
    /// </summary>
    [TestFixture]
    public class SeverTests
    {
        private static int built;

        private static UnitController Build(string body, Vector3 at, float yaw, int team)
        {
            string path = Path.Combine(Application.dataPath, $"_Battler/Data/Rigs/{body.ToLowerInvariant()}_rig.json");
            var rig = RigFile.Parse(File.ReadAllText(path));
            var profile = UnitProfile.CreateDefault();
            profile.body = body;
            var root = UnitBuilder.Build(rig, profile, $"Sever {body} {team} #{++built}", _ => null, 0);
            root.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            var unit = root.GetComponent<UnitController>();
            unit.Profile = profile;
            unit.Team = team;
            typeof(UnitController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(unit, null);
            unit.RhythmOn = false;   // (the aim is locked: a moving guard would change what the blade meets)
            return unit;
        }

        private static WeaponDefinition Weapon(WeaponKind kind)
        {
            var w = WeaponDefinition.CreateDefault(kind);
            w.moves = kind == WeaponKind.Sword
                ? new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand) }
                : new[] { MoveDefinition.Create(MoveStyle.ClubSwing) };
            return w;
        }

        private static void Step(IEnumerable<UnitController> units, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                foreach (var u in units)
                {
                    u.PreviewPose(1f / 60f);
                }
            }
        }

        private static bool Under(Transform t, Transform above)
        {
            for (; t != null; t = t.parent)
            {
                if (t == above)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Destroy(params UnitController[] units)
        {
            foreach (var u in units)
            {
                if (u != null)
                {
                    Object.DestroyImmediate(u.gameObject);
                }
            }
        }

        /// <summary>A medium attacker with the weapon facing a medium target (a club in its right hand) at engage distance.</summary>
        private static (UnitController attacker, UnitController target) Duel(WeaponKind kind)
        {
            var attacker = Build("Medium", Vector3.zero, 0f, 0);
            var target = Build("Medium", Vector3.forward * 5f, 180f, 1);
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            target.Place(Vector3.forward * distance, 180f);
            attacker.Equip(Weapon(kind), null);
            target.Equip(Weapon(WeaponKind.Club), null);
            Step(new[] { attacker, target }, 20);
            return (attacker, target);
        }

        /// <summary>Plays a blow aimed at a part; the hits it dealt.</summary>
        private static List<HitReport> Play(UnitController attacker, UnitController target, MoveDefinition move, AimPart part)
        {
            var hits = new List<HitReport>();
            void OnHit(HitReport r)
            {
                if (r.Attacker == attacker)
                {
                    hits.Add(r);
                }
            }

            var state = MovePlanner.Prepare(attacker, target, move, part);
            if (state == null)
            {
                return hits;
            }

            UnitController.Hit += OnHit;
            try
            {
                if (!attacker.StartMove(state))
                {
                    return hits;
                }

                for (int i = 0; i < 300 && attacker.IsAttacking; i++)
                {
                    Step(new[] { attacker, target });
                }
            }
            finally
            {
                UnitController.Hit -= OnHit;
            }

            return hits;
        }

        [Test]
        public void ACutAimedAtTheElbowTakesTheArmOffThere()
        {
            // The milestone's gate. Each elbow, by whichever of the sword's cuts reaches it.
            var report = new System.Text.StringBuilder();
            int severedAtTheElbow = 0, tried = 0;
            foreach (var part in new[] { AimPart.ElbowR, AimPart.ElbowL })
            {
                foreach (int m in new[] { 0, 1 })
                {
                    var (attacker, target) = Duel(WeaponKind.Sword);
                    try
                    {
                        var move = attacker.Loadout.Main.moves[m];
                        var hits = Play(attacker, target, move, part);
                        if (hits.Count == 0)
                        {
                            report.AppendLine($"{move.name} at {part}: no hit");
                            continue;
                        }

                        tried++;
                        var forearm = AimPartInfo.BodyPartOf(part);
                        var upper = PartInfo.Parent(forearm);
                        var first = hits[0];
                        bool there = target.Rig.IsSevered(forearm) && !target.Rig.IsSevered(upper);
                        severedAtTheElbow += there ? 1 : 0;
                        report.AppendLine($"{move.name} at {part}: struck the {first.Part} at {first.Speed:0.0} m/s for {first.Result.Damage:0.0}, "
                                          + $"severed {(first.Severed.HasValue ? PartInfo.JointName(first.Severed.Value) : "nothing")}{(first.SeverMiss != null ? " (" + first.SeverMiss + ")" : "")}");
                        if (there)
                        {
                            Assert.IsTrue(target.Rig.IsSevered(PartInfo.Parent(forearm) == PartId.UpperArmR ? PartId.HandR : PartId.HandL), "the hand goes with the forearm");
                            Assert.IsNull(target.Rig.Part(forearm).Owner, "the limb is no longer the unit's");
                            Assert.IsNull(target.Rig.Part(forearm).joint, "the elbow's joint is gone");
                            Assert.AreEqual(1, target.SeveredLimbs.Count);
                            Assert.Greater(target.Bleeding, 0f, "it bleeds");
                            if (part == AimPart.ElbowR)
                            {
                                Assert.IsNull(target.Loadout.Main, "the right hand's club went with it");
                                Assert.IsNull(target.MainWeapon);
                            }
                        }
                    }
                    finally
                    {
                        Destroy(attacker, target);
                    }
                }
            }

            System.Console.WriteLine(report.ToString());
            Assert.Greater(tried, 0, report.ToString());
            Assert.Greater(severedAtTheElbow, 0, "a cut aimed at an elbow took the arm off there\n" + report);
        }

        [Test]
        public void AClubNeverSevers()
        {
            foreach (var part in new[] { AimPart.ElbowR, AimPart.ElbowL, AimPart.Neck, AimPart.KneeL })
            {
                var (attacker, target) = Duel(WeaponKind.Club);
                try
                {
                    var hits = Play(attacker, target, attacker.Loadout.Main.moves[0], part);
                    foreach (var h in hits)
                    {
                        Assert.IsFalse(h.Severed.HasValue, $"a club at the {part} severed nothing");
                    }

                    Assert.IsFalse(target.Rig.AnySevered);
                }
                finally
                {
                    Destroy(attacker, target);
                }
            }
        }

        [Test]
        public void ASlowEdgeOnlyWounds()
        {
            // The same cut at the elbow with the sword's sever speed out of reach: a wound, no sever.
            var (attacker, target) = Duel(WeaponKind.Sword);
            try
            {
                attacker.Loadout.Main.severSpeed = 1000f;
                var hits = Play(attacker, target, attacker.Loadout.Main.moves[0], AimPart.ElbowR);
                Assert.IsFalse(target.Rig.AnySevered, "nothing came off");
                foreach (var h in hits)
                {
                    Assert.IsFalse(h.Severed.HasValue);
                }
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        /// <summary>Gives the held sword a blade collider as a prefab's would be (the stubs can't instantiate one): a trigger, on the Weapon layer.</summary>
        private static Weapon WithBlade(Weapon weapon)
        {
            var blade = new GameObject("Blade") { hideFlags = HideFlags.DontSave };
            blade.transform.SetParent(weapon.transform, false);
            blade.layer = Layers.Index(Layers.Weapon);
            var box = blade.AddComponent<BoxCollider>();
            box.size = new Vector3(0.02f, 0.05f, 0.9f);
            box.isTrigger = true;
            return weapon;
        }

        /// <summary>Milestone 9c: a held weapon is solid while its unit lies on the ground, and only reports what it touches standing.</summary>
        [Test]
        public void AHeldWeaponIsSolidOnlyWhileItsUnitLies()
        {
            var (attacker, target) = Duel(WeaponKind.Sword);
            try
            {
                var weapon = WithBlade(attacker.MainWeapon);
                Assert.Greater(weapon.Colliders.Length, 0, "the test sword has a collider");
                var ground = typeof(UnitController).GetMethod("UpdateGround", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(ground);
                ground.Invoke(attacker, new object[] { 1f / 60f });
                foreach (var collider in weapon.Colliders)
                {
                    Assert.IsTrue(collider.isTrigger, "standing: it reports what it touches");
                }

                attacker.DropForTest(false, Vector3.zero);
                Assert.IsTrue(attacker.Lying);
                ground.Invoke(attacker, new object[] { 1f / 60f });
                Assert.IsTrue(weapon.Lying, "down: it lies with it");
                foreach (var collider in weapon.Colliders)
                {
                    Assert.IsFalse(collider.isTrigger, "and is solid");
                    Assert.IsTrue(collider.hasModifiableContacts, "a standing unit only nudges it");
                }

                weapon.SetLying(false);
                foreach (var collider in weapon.Colliders)
                {
                    Assert.IsTrue(collider.isTrigger, "up again: it reports what it touches");
                }
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        [Test]
        public void LosingTheWeaponArmLosesTheWeapon()
        {
            var (attacker, target) = Duel(WeaponKind.Sword);
            try
            {
                var weapon = WithBlade(attacker.MainWeapon);
                Assert.Greater(weapon.Colliders.Length, 0);
                Assert.IsTrue(attacker.Sever(PartId.ForearmR, Vector3.right * 10f));
                Assert.IsNull(attacker.Loadout.Main);
                Assert.IsNull(attacker.MainWeapon);
                Assert.IsNull(weapon.Owner, "the sword is no one's");
                Assert.IsFalse(Under(weapon.transform, attacker.Rig.Part(PartId.HandR).transform), "it fell from the hand");
                Assert.IsFalse(Under(weapon.transform, attacker.transform), "out of the unit");
                // Milestone 9c: on its own, solid, on the Weapon layer, carried on by the cut.
                var body = weapon.GetComponent<Rigidbody>();
                Assert.IsNotNull(body, "with a body of its own");
                Assert.IsFalse(body.isKinematic);
                Assert.AreEqual(weapon.definition.massKg * Mathf.Pow(weapon.Scale, 3f), body.mass, 1e-3f, "as heavy as the weapon");
                Assert.Greater(body.linearVelocity.y, 0.5f, "thrown up with the limb");
                foreach (var collider in weapon.GetComponentsInChildren<Collider>(true))
                {
                    Assert.IsFalse(collider.isTrigger, "solid");
                    Assert.AreEqual(Layers.Index(Layers.Weapon), collider.gameObject.layer, "on the Weapon layer");
                }

                Assert.IsTrue(System.Linq.Enumerable.Contains(attacker.DroppedWeapons, weapon.gameObject), "kept, to be cleared away with the unit");
                Assert.IsNull(MovePlanner.Plan(attacker, target, new System.Random(1)), "no blows without it");
                Assert.IsTrue(attacker.IsAlive);
                Assert.IsFalse(attacker.Rig.IsSevered(PartId.UpperArmR), "the upper arm stays");
                // It can't be armed again in that hand.
                attacker.Equip(Weapon(WeaponKind.Sword), null);
                Assert.IsNull(attacker.MainWeapon);
                Step(new[] { attacker, target }, 30);   // (and it still poses and steps without it)
                Assert.IsTrue(attacker.IsAlive);
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        [Test]
        public void LosingALegPutsItDownForGoodAndItBleeds()
        {
            var (attacker, target) = Duel(WeaponKind.Sword);
            try
            {
                float health = target.Health;
                Assert.IsTrue(target.Sever(PartId.ShinL, Vector3.right * 10f));
                Assert.IsTrue(target.LegLost);
                Assert.AreEqual(UnitState.Down, target.State, "no footing: down");
                Assert.Less(target.Health, health, "the wound");
                float wounded = target.Health;
                Step(new[] { attacker, target }, 120);
                Assert.Less(target.Health, wounded - 0.9f * 2f * target.Bleeding, "it bleeds");
                Assert.AreNotEqual(UnitState.Standing, target.State, "it doesn't get up on one leg");
                Assert.IsFalse(target.CanAct);
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        [Test]
        public void TheNeckCutKills()
        {
            var (attacker, target) = Duel(WeaponKind.Sword);
            try
            {
                Assert.IsTrue(target.Sever(PartId.Head, Vector3.right * 10f));
                Assert.IsFalse(target.IsAlive);
                Assert.IsTrue(target.Rig.IsSevered(PartId.Head));
                Assert.IsFalse(target.Rig.IsSevered(PartId.Chest));
            }
            finally
            {
                Destroy(attacker, target);
            }
        }

        [Test]
        public void CutOffLimbsGoWithTheUnit()
        {
            var (attacker, target) = Duel(WeaponKind.Sword);
            target.Sever(PartId.HandL, Vector3.up);
            var limb = target.SeveredLimbs[0];
            Assert.IsNotNull(limb);
            // (Edit mode: the harness doesn't call OnDestroy itself.)
            typeof(UnitController).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(target, null);
            Assert.AreEqual(0, target.SeveredLimbs.Count, "removed with the unit");
            Destroy(attacker, target);
        }
    }
}
