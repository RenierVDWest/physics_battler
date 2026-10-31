using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Tests
{
    /// <summary>
    /// Milestone 3's gate: a bystander in the way takes the blow. Every weapon contact is a hit,
    /// whoever it lands on; and the damage rules: weight matters, a heavy body or a shield stops
    /// the swing, hit stop freezes both units.
    /// </summary>
    [TestFixture]
    public class HitTests
    {
        // ---- The damage rules -------------------------------------------------------------------

        private static HitResult Blow(float attackerMass, float struckMass, PartId part, bool shield = false, float speed = 6f)
        {
            var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
            return DamageRules.Resolve(sword, StrikeWith.Edge, 1f, speed, Vector3.forward, attackerMass, struckMass, part, shield, 1f, 100f, 45f, 45f);
        }

        [Test]
        public void TheSameBlowMovesALightUnitFurther()
        {
            var hit = Blow(80f, 40f, PartId.Chest);
            Assert.Greater(DamageRules.KnockbackSpeed(hit, 40f), 1.9f * DamageRules.KnockbackSpeed(hit, 80f));
        }

        [Test]
        public void ASwingCleavesThroughALighterBodyAndStopsOnAnEqualOne()
        {
            var light = Blow(80f, 38f, PartId.Chest);
            Assert.IsFalse(light.StopsSwing, "a medium swing cleaves through a small body");
            Assert.Greater(light.MomentumLeft, DamageRules.CleaveShare - 1e-4f);
            Assert.Less(light.MomentumLeft, 1f, "and carries on slowed");
            foreach (float mass in new[] { 80f, 280f })
            {
                var hit = Blow(80f, mass, PartId.Chest);
                Assert.IsTrue(hit.StopsSwing, $"a {mass} kg body stops it");
                Assert.AreEqual(0f, hit.MomentumLeft);
            }

            Assert.IsFalse(Blow(280f, 80f, PartId.Chest).StopsSwing, "a large unit cleaves through a medium one");
        }

        [Test]
        public void AShieldTakesNoDamageAndStopsTheSwing()
        {
            var hit = Blow(80f, 80f, PartId.HandL, shield: true);
            Assert.IsTrue(hit.Blocked);
            Assert.AreEqual(0f, hit.Damage);
            Assert.Greater(hit.PoiseDamage, 0f, "a block still costs poise");
            Assert.IsTrue(hit.StopsSwing);
        }

        [Test]
        public void TheHeadHurtsMostAndFasterBlowsHurtMore()
        {
            Assert.Greater(Blow(80f, 80f, PartId.Head).Damage, Blow(80f, 80f, PartId.Chest).Damage);
            Assert.Greater(Blow(80f, 80f, PartId.Chest).Damage, Blow(80f, 80f, PartId.HandR).Damage);
            Assert.Greater(Blow(80f, 80f, PartId.Chest, speed: 9f).Damage, Blow(80f, 80f, PartId.Chest).Damage);
        }

        [Test]
        public void ReactionsGoFromRecoilToFlinchToStaggerToKilled()
        {
            var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
            Reaction At(float speed, float health, float poise) =>
                DamageRules.Resolve(sword, StrikeWith.Edge, 1f, speed, Vector3.forward, 80f, 80f, PartId.Chest, false, 1f, health, poise, 45f).Reaction;

            Assert.AreEqual(Reaction.Recoil, At(3f, 100f, 45f));
            Assert.AreEqual(Reaction.Flinch, At(20f, 100f, 45f));
            Assert.AreEqual(Reaction.Stagger, At(6f, 100f, 5f));
            Assert.AreEqual(Reaction.Killed, At(6f, 5f, 45f));
        }

        // ---- Blows between units ----------------------------------------------------------------

        private static int built;

        private static WeaponDefinition Sword()
        {
            var sword = WeaponDefinition.CreateDefault(WeaponKind.Sword);
            sword.moves = new[] { MoveDefinition.Create(MoveStyle.SwordCut), MoveDefinition.Create(MoveStyle.SwordBackhand), MoveDefinition.Create(MoveStyle.SwordThrust) };
            return sword;
        }

        private static UnitController Build(string body, Vector3 at, float yaw, int team)
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

        private static void Destroy(params UnitController[] units)
        {
            foreach (var unit in units)
            {
                if (unit != null)
                {
                    Object.DestroyImmediate(unit.gameObject);
                }
            }
        }

        private static void Step(IEnumerable<UnitController> units, int steps = 1)
        {
            for (int i = 0; i < steps; i++)
            {
                foreach (var unit in units)
                {
                    unit.PreviewPose(1f / 60f);
                }
            }
        }

        /// <summary>A medium swordsman (team 0) facing +Z, its target where it would engage.</summary>
        private static (UnitController attacker, UnitController target, float distance) Duel(string targetBody = "Medium")
        {
            var attacker = Build("Medium", Vector3.zero, 0f, 0);
            var target = Build(targetBody, Vector3.forward * 5f, 180f, 1);
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            target.Place(Vector3.forward * distance, 180f);
            attacker.Equip(Sword(), null);
            return (attacker, target, distance);
        }

        /// <summary>Plays a blow to its end (or its cancelling); the hits it dealt, in order.</summary>
        private static List<HitReport> Play(UnitController attacker, MoveState state, UnitController[] everyone, System.Action<HitReport> onHit = null)
        {
            var hits = new List<HitReport>();
            void OnHit(HitReport report)
            {
                if (report.Attacker == attacker)
                {
                    hits.Add(report);
                    onHit?.Invoke(report);
                }
            }

            UnitController.Hit += OnHit;
            try
            {
                Assert.IsTrue(attacker.StartMove(state), "the blow starts");
                for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                {
                    Step(everyone);
                }
            }
            finally
            {
                UnitController.Hit -= OnHit;
            }

            return hits;
        }

        /// <summary>
        /// Plans a blow at the target and stands a third unit in its way, centred on the blade
        /// (<see cref="MovePlanner.InTheWay"/>). Returns the hits the blow dealt.
        /// </summary>
        private static List<HitReport> BlowThrough(UnitController attacker, UnitController target, UnitController bystander, AimPart part, float before, out Vector3 at)
        {
            var state = BlowThroughState(attacker, target, bystander, part, before, out at);
            return Play(attacker, state, new[] { attacker, target, bystander });
        }

        private static MoveState BlowThroughState(UnitController attacker, UnitController target, UnitController bystander, AimPart part, float before, out Vector3 at)
        {
            var everyone = new[] { attacker, target, bystander };
            Step(everyone, 20);
            var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], part);
            Assert.IsNotNull(state, $"a cut at the target's {part}");
            var root = attacker.Root;
            at = MovePlanner.InTheWay(attacker, state, before);
            var torso = bystander.Rig.Part(PartId.Chest).body.position - bystander.Root.position;
            bystander.Teleport(new Vector3(at.x - torso.x, 0f, at.z - torso.z), root.eulerAngles.y + 180f);
            Step(new[] { bystander }, 2);
            return state;
        }

        [Test]
        public void ABystanderInTheWayTakesTheBlow() => BystanderTakesTheBlow(1, "someone in the way");

        [Test]
        public void AnAllyInTheWayTakesTheBlowInFull() => BystanderTakesTheBlow(0, "an ally (friendly fire)");

        /// <summary>
        /// The gate: a third unit (medium, as heavy as the attacker) stands between the attacker and
        /// the target, across a cut at the target's belly. It takes the blow, the swing stops on it so the
        /// target comes away untouched, and friendly fire does full damage.
        /// </summary>
        private static void BystanderTakesTheBlow(int bystanderTeam, string kind)
        {
            var (attacker, target, _) = Duel();
            var bystander = Build("Medium", new Vector3(3f, 0f, 3f), 180f, bystanderTeam);
            var everyone = new[] { attacker, target, bystander };
            try
            {
                float targetHealth = target.Health, bystanderHealth = bystander.Health;
                var hits = BlowThrough(attacker, target, bystander, AimPart.Belly, 0.3f, out var at);

                Assert.Greater(hits.Count, 0, "the blow strikes something");
                Assert.IsTrue(hits[0].Struck == bystander, $"the one in the way is struck first (struck {(hits[0].Struck == target ? "the target" : hits[0].Struck.name)}'s {hits[0].Part} at {hits[0].Point}; the one in the way stood at {at})");
                Assert.AreEqual(kind, hits[0].Kind);
                Assert.Greater(hits[0].Result.Damage, 0f, "full damage");
                Assert.Less(bystander.Health, bystanderHealth);
                Assert.IsTrue(hits[0].Result.StopsSwing, "as heavy as the attacker: the swing stops");
                Assert.AreEqual(targetHealth, target.Health, "the target is untouched");
            }
            finally
            {
                Destroy(everyone);
            }
        }

        /// <summary>
        /// A light body in the way is cleaved through: the swing carries on, slowed, and the body is
        /// thrown aside out of the arc, much harder than a body heavy enough to stop the swing.
        /// </summary>
        [Test]
        public void ALightBystanderIsCleavedThroughAndThrownAside()
        {
            (List<HitReport> hits, float knock, bool targetHurt) Run(string body)
            {
                var (attacker, target, _) = Duel("Small");
                var bystander = Build(body, new Vector3(3f, 0f, 3f), 180f, 1);
                var everyone = new[] { attacker, target, bystander };
                try
                {
                    float knock = 0f, targetHealth = target.Health;
                    var state = BlowThroughState(attacker, target, bystander, AimPart.Belly, 0.3f, out var at);
                    var hits = new List<HitReport>();
                    void OnHit(HitReport h)
                    {
                        if (h.Attacker == attacker)
                        {
                            hits.Add(h);
                        }
                    }

                    UnitController.Hit += OnHit;
                    try
                    {
                        Assert.IsTrue(attacker.StartMove(state));
                        for (int i = 0; i < 400 && attacker.IsAttacking; i++)
                        {
                            Step(everyone);
                            knock = Mathf.Max(knock, bystander.Knockback.magnitude);
                        }
                    }
                    finally
                    {
                        UnitController.Hit -= OnHit;
                    }

                    Assert.Greater(hits.Count, 0, "the blow strikes something");
                    Assert.IsTrue(hits[0].Struck == bystander, $"the {body} unit in the way is struck first (stood at {at}; struck {hits[0].Struck.name}'s {hits[0].Part})");
                    return (hits, knock, target.Health < targetHealth);
                }
                finally
                {
                    Destroy(everyone);
                }
            }

            var small = Run("Small");
            Assert.IsFalse(small.hits[0].Result.StopsSwing, "a medium swing cleaves through a small body");
            Assert.Greater(small.knock, 1.5f, $"and throws it aside ({small.knock:0.00} m/s)");

            var medium = Run("Medium");
            Assert.IsTrue(medium.hits[0].Result.StopsSwing, "a medium body stops a medium swing");
            Assert.AreEqual(1, medium.hits.Count, "nothing behind it is struck");
            Assert.IsFalse(medium.targetHurt, "the target is untouched");
            Assert.Greater(small.knock, 1.5f * medium.knock, $"the cleaved body is thrown much harder ({small.knock:0.00} against {medium.knock:0.00} m/s)");
        }

        /// <summary>The weapon stops where it meets the body and bounces back: it doesn't carry on through.</summary>
        [Test]
        public void TheWeaponStopsAtTheBody()
        {
            var (attacker, target, _) = Duel();
            var everyone = new[] { attacker, target };
            try
            {
                Step(everyone, 20);
                var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], AimPart.Chest);
                Assert.IsNotNull(state);
                bool hit = false;
                void OnHit(HitReport report)
                {
                    hit |= report.Attacker == attacker;
                }

                UnitController.Hit += OnHit;
                try
                {
                    Assert.IsTrue(attacker.StartMove(state));
                    for (int i = 0; i < 400 && !hit && attacker.IsAttacking; i++)
                    {
                        Step(everyone);
                    }
                }
                finally
                {
                    UnitController.Hit -= OnHit;
                }

                Assert.IsTrue(hit, "the blow lands");
                var weapon = attacker.MainWeapon;
                for (int i = 0; i < 40; i++)
                {
                    Step(everyone);
                    Vector3 grip = weapon.WorldPoint(Vector3.zero), tip = weapon.WorldPoint(weapon.definition.tip);
                    foreach (var collider in Physics.OverlapCapsule(grip, tip, 0.005f, ~0, QueryTriggerInteraction.Collide))
                    {
                        var part = collider.GetComponentInParent<BodyPart>();
                        // (A step or two to bounce out of the few centimetres it went in.)
                        if (part != null && part.Owner == target && part.partCollider == collider && i >= 3)
                        {
                            Assert.Fail($"{i} steps after the hit the blade is still in {target.name}'s {part.id}");
                        }
                    }
                }

                Assert.IsFalse(attacker.IsAttacking, "the blow is over");
            }
            finally
            {
                Destroy(everyone);
            }
        }

        /// <summary>A hard hit throws the struck body back: hips and chest go with the blow, the upper body bends away.</summary>
        [Test]
        public void AHardHitThrowsTheBodyBack()
        {
            var attacker = Build("Large", Vector3.zero, 0f, 0);
            var target = Build("Medium", Vector3.forward * 5f, 180f, 1);
            var club = WeaponDefinition.CreateDefault(WeaponKind.Club);
            club.moves = new[] { MoveDefinition.Create(MoveStyle.ClubSwing) };
            attacker.Equip(club, null);
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            target.Place(Vector3.forward * distance, 180f);
            var everyone = new[] { attacker, target };
            try
            {
                Step(everyone, 20);
                var state = MovePlanner.Prepare(attacker, target, club.moves[0], AimPart.Chest);
                Assert.IsNotNull(state);
                HitReport first = null;
                Vector3 chestBefore = Vector3.zero;
                void OnHit(HitReport h)
                {
                    if (first == null && h.Attacker == attacker)
                    {
                        first = h;
                        chestBefore = target.Anim.Part(PartId.Chest).position;
                    }
                }

                // Over the 0.3 s after the hit the chest is carried with the blow and the upper body bends.
                float moved = 0f, bend = 0f;
                int after = 0;
                UnitController.Hit += OnHit;
                try
                {
                    Assert.IsTrue(attacker.StartMove(state));
                    for (int i = 0; i < 400 && after < 18; i++)
                    {
                        Step(everyone);
                        if (first != null)
                        {
                            after++;
                            Vector3 dir = first.Result.Impulse;
                            dir.y = 0f;
                            dir.Normalize();
                            moved = Mathf.Max(moved, Vector3.Dot(target.Anim.Part(PartId.Chest).position - chestBefore, dir));
                            bend = Mathf.Max(bend, target.Pose.ReactLean.magnitude * Mathf.Rad2Deg);
                        }
                    }
                }
                finally
                {
                    UnitController.Hit -= OnHit;
                }

                Assert.IsNotNull(first, "the blow lands");
                Assert.IsTrue(first.Struck == target);
                Assert.Greater(moved, 0.12f, $"the chest is thrown with the blow ({moved * 100f:0} cm)");
                Assert.Greater(bend, 12f, $"the upper body bends away ({bend:0}°)");
                Assert.Greater(target.Knockback.magnitude + moved, 0f);
            }
            finally
            {
                Destroy(everyone);
            }
        }

        /// <summary>Two units can't stand in the same place: placed together, they push apart.</summary>
        [Test]
        public void BodiesDontShareSpace()
        {
            var a = Build("Medium", Vector3.zero, 0f, 0);
            var b = Build("Large", new Vector3(0.05f, 0f, 0.1f), 180f, 1);
            var everyone = new[] { a, b };
            try
            {
                Step(everyone, 60);
                Vector3 between = a.Anim.Part(PartId.Pelvis).position - b.Anim.Part(PartId.Pelvis).position;
                between.y = 0f;
                float apart = a.BodyRadius + b.BodyRadius;
                Assert.Greater(between.magnitude, 0.95f * apart, $"the hips are {between.magnitude * 100f:0} cm apart (bodies need {apart * 100f:0} cm)");
                Vector3 moved = a.Root.position - Vector3.zero, movedB = b.Root.position - new Vector3(0.05f, 0f, 0.1f);
                Assert.Greater(moved.magnitude, movedB.magnitude, "the lighter one gives way more");
            }
            finally
            {
                Destroy(everyone);
            }
        }

        /// <summary>At a hit, both the attacker and the struck stand still for the hit stop (60–100 ms).</summary>
        [Test]
        public void HitStopFreezesBothUnits()
        {
            var (attacker, target, _) = Duel("Small");
            var everyone = new[] { attacker, target };
            try
            {
                Step(everyone, 20);
                var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], AimPart.Chest);
                Assert.IsNotNull(state);
                bool hit = false;
                float attackerStop = 0f, targetStop = 0f;
                void OnHit(HitReport report)
                {
                    if (report.Attacker == attacker && !hit)
                    {
                        hit = true;
                        attackerStop = attacker.HitStop;
                        targetStop = report.Struck.HitStop;
                    }
                }

                UnitController.Hit += OnHit;
                try
                {
                    Assert.IsTrue(attacker.StartMove(state));
                    for (int i = 0; i < 400 && !hit && attacker.IsAttacking; i++)
                    {
                        Step(everyone);
                    }
                }
                finally
                {
                    UnitController.Hit -= OnHit;
                }

                Assert.IsTrue(hit, "the blow lands");
                Assert.GreaterOrEqual(attackerStop, 0.04f, "the attacker stops");
                Assert.LessOrEqual(attackerStop, 0.1f, "for 40-100 ms");
                Assert.GreaterOrEqual(targetStop, 0.04f, "and so does the struck");
                Assert.LessOrEqual(targetStop, 0.1f, "for 40-100 ms");

                // One more step (shorter than the hit stop): neither unit's time moves.
                float attackerTime = attacker.LocalTime, targetTime = target.LocalTime;
                Step(everyone);
                Assert.AreEqual(attackerTime, attacker.LocalTime, 1e-6f, "the attacker's time stands still");
                Assert.AreEqual(targetTime, target.LocalTime, 1e-6f, "the struck unit's time stands still");
            }
            finally
            {
                Destroy(everyone);
            }
        }

        /// <summary>A shield raised across the blow takes it: no damage, and the swing stops.</summary>
        [Test]
        public void AShieldBlocks()
        {
            var (attacker, target, _) = Duel();
            var shield = WeaponDefinition.CreateDefault(WeaponKind.Shield);
            target.Equip(Sword(), shield);
            // The default shield has no prefab: give it a face to strike.
            var face = target.OffWeapon.gameObject.AddComponent<BoxCollider>();
            face.center = shield.faceCenter;
            face.size = new Vector3(2f * shield.radius, 2f * shield.radius, 0.04f);
            target.Style.blockChance = 1f;
            var everyone = new[] { attacker, target };
            try
            {
                Step(everyone, 20);
                var state = MovePlanner.Prepare(attacker, target, attacker.Loadout.Main.moves[0], AimPart.Chest);
                Assert.IsNotNull(state);
                float health = target.Health;
                string cut = null;
                void OnEnded(UnitController unit, MoveState move)
                {
                    if (unit == attacker)
                    {
                        cut = move.Cancelled;
                    }
                }

                UnitController.MoveEnded += OnEnded;
                List<HitReport> hits;
                try
                {
                    hits = Play(attacker, state, everyone);
                }
                finally
                {
                    UnitController.MoveEnded -= OnEnded;
                }

                Assert.Greater(hits.Count, 0, "the blow strikes something");
                Assert.IsTrue(hits[0].Shield, $"the shield takes it (struck the {hits[0].Part})");
                Assert.IsTrue(hits[0].Result.Blocked);
                Assert.AreEqual(health, target.Health, "no damage");
                Assert.IsTrue((cut ?? "").Contains("shield"), $"the swing is stopped by the shield (ended: {cut ?? "not cut short"})");
            }
            finally
            {
                Destroy(everyone);
            }
        }
    }
}
