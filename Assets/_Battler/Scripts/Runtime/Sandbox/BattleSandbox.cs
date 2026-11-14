using System.Collections;
using System.Collections.Generic;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The battle sandbox: the 8 by 8 board and the units laid out on it as <see cref="UnitSpawn"/>
    /// markers in the scene (each shows its unit while you edit). In play, the markers' previews go
    /// and the real units are spawned on them.
    ///
    /// Milestone 2: units close in on the nearest enemy, face it, and strike at a body part their
    /// moves can reach (<see cref="MovePlanner"/>): the blow is aimed at where that part was when
    /// it started. Weapons don't hurt yet (milestone 3); what they touch is logged.
    ///
    /// Keys: R reset, P pause, . step one physics frame, T turn everyone around, F fighting on/off,
    /// M mark the playtest log. Right-drag orbits, middle-drag pans, scroll zooms. Every Play session
    /// is recorded by a <see cref="PlaytestRecorder"/> into PlaytestLogs/.
    /// </summary>
    [DefaultExecutionOrder(-150)]   // gameplay first, then the units' own step (-100)
    public class BattleSandbox : MonoBehaviour
    {
        [Header("Wired by the project setup")]
        public UnitController smallPrefab;
        public UnitController mediumPrefab;
        public UnitController largePrefab;
        public BoardView board;
        public OrbitCamera orbitCamera;
        public Material teamA;
        public Material teamB;

        [Header("Locomotion lab (branch locomotion-lab)")]
        [Tooltip("Instead of the fight: one Medium walks the new locomotion round the lab's course (C: drive it with W A S D).")]
        public bool locomotionLab = true;

        [Header("Fighting")]
        [Tooltip("Units close in and strike. Off: they only stand and face each other (F).")]
        public bool fight = true;
        [Tooltip("Seed for the choice of blows and timings (0: different every time).")]
        public int seed;

        private readonly List<UnitController> units = new List<UnitController>();
        private Brawl brawl = new Brawl(0);
        // Milestone 6: the fight itself (the Brawl is kept only to turn everyone away, T).
        private Battle.FightLoop fightLoop = new Battle.FightLoop(0);
        private bool announced;
        private PlaytestRecorder recorder;
        private bool turnedAway;
        private float timeScale = 1f;
        private bool paused;
        private string message = "";
        private Rect panelRect = new Rect(12, 12, 300, 10);

        public IReadOnlyList<UnitController> Units => units;

        public UnitController PrefabFor(BodySize body)
        {
            switch (body)
            {
                case BodySize.Small: return smallPrefab;
                case BodySize.Large: return largePrefab;
                default: return mediumPrefab;
            }
        }

        public UnitProfile ProfileFor(BodySize body)
        {
            var prefab = PrefabFor(body);
            return prefab != null ? prefab.Profile : null;
        }

        public Material TeamMaterial(int team) => team == 0 ? teamA : teamB;

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (locomotionLab)
            {
                // The lab runs on its own: the sandbox's fight, recorder and panel stay off.
                var lab = gameObject.GetComponent<Locomotion.LocomotionLab>();
                if (lab == null)
                {
                    lab = gameObject.AddComponent<Locomotion.LocomotionLab>();
                }

                foreach (var spawn in FindObjectsByType<UnitSpawn>())
                {
                    spawn.ClearPreview();
                }

                lab.Begin(smallPrefab, mediumPrefab, largePrefab, orbitCamera, teamA, board != null ? board.grid.origin : Vector3.zero);
                enabled = false;
                return;
            }

            BeginFight();
        }

        /// <summary>From the locomotion lab to the fight (the lab's F): the units spawn and fight, walking with the new walk.</summary>
        public void WatchFight()
        {
            watchFrom = Time.frameCount;
            locomotionLab = false;
            enabled = true;
            BeginFight();
        }

        /// <summary>From the locomotion lab to the target test (the lab's 1), as <see cref="WatchFight"/> goes to the battle.</summary>
        public void WatchTargets()
        {
            targetTest = true;
            fight = true;
            WatchFight();
            message = "Target test: left to right, a target that does not block, one with sword and shield, one with a sword. R resets.";
        }

        private int watchFrom = -1;

        private void BeginFight()
        {
            recorder = FindAnyObjectByType<PlaytestRecorder>();
            if (recorder == null)
            {
                recorder = gameObject.AddComponent<PlaytestRecorder>();
            }

            JointConventions.EnsureProbed();
            Note("joint probe: " + JointConventions.Report);
            // (Performance: after a long frame Unity runs as many physics steps as it fell behind, up to
            // a third of a second's worth (20), and that frame is long in turn: the 20:16 playtest had
            // hitches followed by frames of 6 and 7 steps. At most this much physics in a frame: past
            // it, the game slows for a moment instead of freezing longer.)
            Time.maximumDeltaTime = MaxPhysicsPerFrame;
            Note($"at most {MaxPhysicsPerFrame * 1000f:0} ms of physics a frame");
            Spawn();
        }

        /// <summary>The most physics time a rendered frame catches up on (seconds): three steps.</summary>
        public const float MaxPhysicsPerFrame = 0.05f;

        private void OnEnable()
        {
            UnitController.Hit += OnHit;
        }

        private void OnDisable()
        {
            UnitController.Hit -= OnHit;
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>Heavy hits jolt the camera a little (the panel turns it off).</summary>
        private void OnHit(HitReport hit)
        {
            float impulse = hit.Result.Impulse.magnitude;
            if (orbitCamera != null && impulse > 70f)
            {
                orbitCamera.Shake((impulse - 50f) / 150f);
            }
        }

        private static void Note(string text)
        {
            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(null, "input", text);
            }
        }

        private BoardGrid Grid => board != null ? board.grid : new BoardGrid();

        /// <summary>Spawns a unit on every <see cref="UnitSpawn"/> marker in the scene (the markers' previews are hidden in play).</summary>
        public void Spawn()
        {
            Clear();
            // (With physical upper bodies the game steps the engine itself, several physics steps to each of its own.)
            var stepper = GetComponent<PhysicsBattler.Authority.PhysicsStepper>();
            if (PhysicsBattler.Locomotion.ClipFighter.PhysicalUpper && stepper == null)
            {
                stepper = gameObject.AddComponent<PhysicsBattler.Authority.PhysicsStepper>();
            }

            if (stepper != null)
            {
                stepper.enabled = PhysicsBattler.Locomotion.ClipFighter.PhysicalUpper;
            }

            turnedAway = false;
            brawl = new Brawl(seed);
            fightLoop = new Battle.FightLoop(seed) { Grid = Grid };
            announced = false;
            fightLoop.Endless = targetTest;
            var spawns = FindObjectsByType<UnitSpawn>();
            System.Array.Sort(spawns, (a, b) => a.team != b.team ? a.team.CompareTo(b.team) : a.body.CompareTo(b.body));
            var count = new Dictionary<string, int>();
            foreach (var spawn in spawns)
            {
                spawn.ClearPreview();
                if (targetTest)
                {
                    continue;   // (the target test lays its own units out)
                }

                string label = $"{(spawn.team == 0 ? "A" : "B")} {spawn.BodyName}";
                count[label] = count.TryGetValue(label, out int n) ? n + 1 : 1;
                SpawnUnit(spawn.body, spawn.team, spawn.transform.position, count[label] > 1 ? $"{label} {count[label]}" : label, spawn.mainHand, spawn.offHand, spawn.profileOverride);
            }

            if (targetTest)
            {
                SpawnTargets();
            }
            else if (spawns.Length == 0)
            {
                message = "No unit markers in the scene. Add a UnitSpawn (or run Physics Battler > Setup > Run Project Setup).";
            }

            Note($"spawn: {units.Count} units on the {Grid.columns}x{Grid.rows} board{(targetTest ? $" (the target test: {(largeAttackers ? "Large" : "Medium")} attackers on {(largeTargets ? "Large" : "Medium")} targets)" : "")}");
            if (PhysicsBattler.Locomotion.ClipFighter.PhysicalUpper)
            {
                ArticulationConventions.EnsureProbed();
                Note($"physical upper bodies: on, the game stepping the engine {PhysicsBattler.Authority.PhysicsSteps.Substeps} times a step; a blade at most {PhysicsBattler.Locomotion.CombatFeel.MostBladeSpeed:0} m/s; articulation probe: {ArticulationConventions.Report}");
            }
        }

        /// <summary>One unit of a body and a team at a place, facing as its team does, with what it is to hold; null without its prefab.</summary>
        private UnitController SpawnUnit(BodySize body, int team, Vector3 at, string label, WeaponDefinition main, WeaponDefinition off, UnitProfile profile = null)
        {
            var prefab = PrefabFor(body);
            if (prefab == null)
            {
                message = "Missing unit prefabs. Run Physics Battler > Setup > Run Project Setup.";
                return null;
            }

            // Made under an inactive holder, so its profile and team are set before it wakes and builds itself.
            var holder = new GameObject("Spawning");
            holder.SetActive(false);
            var unit = Instantiate(prefab, at, Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f), holder.transform);
            unit.name = label;
            unit.Team = team;
            if (profile != null)
            {
                unit.Profile = profile;
            }

            // (Tinted before it wakes: waking moves its body parts out from under it.)
            Tint(unit, TeamMaterial(team));
            unit.transform.SetParent(null, true);
            Destroy(holder);

            units.Add(unit);
            pendingEquip.Add((unit, main, off));
            return unit;
        }

        // ---- The target test: three attackers, each on a target that stands there ---------------------------------

        private bool targetTest, largeAttackers, largeTargets, targetsHarmed;
        private PhysicsBattler.Locomotion.CombatFeel combatFeel;

        /// <summary>How far apart the target test's three pairs stand, and each attacker from its target (metres).</summary>
        public const float TargetPairsApart = 4f, TargetFromAttacker = 2.6f;

        /// <summary>
        /// The target test (1): instead of the battle, three attackers each on a target that only stands there, side by
        /// side: one with nothing in its hands (it does not block), one with sword and shield (it blocks), one with a
        /// sword alone (it parries). The targets never strike and walk back to their spots when a blow has driven them
        /// off; unless the panel says otherwise they take no harm, so they can be struck again and again. The panel
        /// turns the attackers, or the targets, into Larges.
        /// </summary>
        public void TargetTest()
        {
            targetTest = true;
            tubeTest = false;
            fight = true;
            Spawn();
            message = "Target test: left to right, a target that does not block, one with sword and shield, one with a sword. R resets.";
        }

        // ---- The tube test: the targets alone, each struck by a tube of a set weight and speed -------------------------

        private bool tubeTest;
        private float tubesSpawnedAt;
        private Material tubeLook;

        /// <summary>The tube test's tubes and their settings (the panel's sliders).</summary>
        public readonly TubeTest Tubes = new TubeTest();

        /// <summary>
        /// The tube test (5): the target test's three targets with no attackers (one with nothing in its hands, one with
        /// sword and shield, one with a sword), each with a tube waiting at its side. Hit (6) sends every tube across its
        /// target with the weight and speed the panel's sliders give it; the sliders also set how high it goes, how far
        /// its end reaches into the target, and how far the targets are turned, so the tube meets their front, a side or
        /// their back. Reset (R) stands them up fresh. New tubes are set waiting when the last have gone.
        /// </summary>
        public void TubeTestScene()
        {
            targetTest = true;
            tubeTest = true;
            fight = true;
            Spawn();
            tubesSpawnedAt = Time.time;
            message = "Tube test: three targets and a tube beside each. Hit (6) sends the tubes across them; the sliders set how hard, how fast, how high, how deep and from which side. R resets.";
        }

        /// <summary>The tube test's hit: every waiting tube is sent across its target.</summary>
        public void TubeHit()
        {
            if (!tubeTest)
            {
                TubeTestScene();
                return;
            }

            int sent = Tubes.Launch();
            message = sent > 0
                ? $"Tubes: {sent} sent, {Tubes.Mass:0.0} kg at {Tubes.Speed:0.0} m/s ({Tubes.Impulse:0} N·s), {Tubes.Height * 100f:0} cm up, at {Tubes.Side}{(Tubes.AsBlow ? ", taken as blows" : ", the physics only")}."
                : "Tubes: none waiting yet (they are set a second after the targets are, and again when the last have gone).";
            if (sent > 0)
            {
                Note($"tube test: {sent} tubes, {Tubes.Mass:0.0} kg at {Tubes.Speed:0.0} m/s ({Tubes.Impulse:0} N·s), height {Tubes.Height:0.00} m, reach {Tubes.Reach:0.00} m, turned {Tubes.Turned:0}, {(Tubes.Fells ? "the fall test (they go down)" : Tubes.AsBlow ? "as blows" : "physics only")}, physical legs {(PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs ? "on" : "off")}");
            }
        }

        /// <summary>The fall test's blow (N·s): well over what a Medium's legs catch; and a thrown one's, a Large's club at its hardest.</summary>
        public const float FallTestImpulse = 200f, ThrownTestImpulse = 450f;

        /// <summary>How long the targets are given to turn before a fall test's tubes are sent (seconds).</summary>
        public const float FallTestTurnsFor = 0.8f;

        private float fallHitAt = -1f;

        /// <summary>
        /// The fall test (the plan "Controlled falls", step 1): a fall on demand. The targets are turned so the tube
        /// meets the side that sends them the way asked (<paramref name="turned"/>: 0 struck on the front, they go
        /// backward; 180 on the back, forward; 270 on the right side, to their left; 90 on the left side, to their
        /// right), the tube is taken as a blow, and they go down whatever their legs say. Each fall is measured in
        /// the playtest log (<c>fall</c>: what reached the ground when, the head, the hips).
        /// </summary>
        public void FallTest(float turned, bool thrown)
        {
            if (!tubeTest)
            {
                TubeTestScene();
            }

            bool turns = Mathf.Abs(Mathf.DeltaAngle(Tubes.Turned, turned)) > 1f;
            Tubes.Turned = turned;
            Tubes.Impulse = thrown ? ThrownTestImpulse : FallTestImpulse;
            Tubes.Speed = 8f;
            Tubes.Height = 1.3f;
            Tubes.Reach = 0.15f;
            Tubes.AsBlow = true;
            Tubes.Fells = true;
            // (Sent once the targets have turned and a tube is waiting.)
            fallHitAt = Time.time + (turns ? FallTestTurnsFor : 0f);
            message = $"Fall test: the targets struck on {Tubes.Side} with {Tubes.Impulse:0} N·s, to go down. The tubes are sent as soon as the targets have turned and a tube is waiting.";
        }

        /// <summary>The tube test, each game step: the targets face as the panel turns them, the tubes wait where it puts them, and what they meet is noted.</summary>
        private void StepTubes()
        {
            if (!tubeTest)
            {
                return;
            }

            Vector3 toTarget = Quaternion.Euler(0f, BoardGrid.TeamYaw(0), 0f) * Vector3.forward;
            Vector3 faces = Tubes.Facing(toTarget);
            bool ready = Time.time - tubesSpawnedAt > 1f;
            foreach (var u in units)
            {
                if (u == null || !u.TargetDummy)
                {
                    continue;
                }

                u.DummyFace = u.DummySpot + faces * 5f;
                ready &= u.IsInitialized && u.Clip != null;
            }

            if (ready && !Tubes.Armed)
            {
                if (tubeLook == null)
                {
                    tubeLook = TeamMaterial(0);
                }

                var targets = new List<UnitController>();
                foreach (var u in units)
                {
                    if (u != null && u.TargetDummy && u.IsAlive)
                    {
                        targets.Add(u);
                    }
                }

                Tubes.Arm(targets, toTarget, tubeLook);
            }

            Tubes.Place();
            if (fallHitAt >= 0f && Time.time >= fallHitAt && Tubes.Waiting)
            {
                fallHitAt = -1f;
                TubeHit();
            }

            Tubes.Step(Time.fixedDeltaTime);
            if (Tubes.Report.Length > 0 && !message.Contains(Tubes.Report))
            {
                message = "Tubes met: " + Tubes.Report + ".";
            }
        }

        /// <summary>Back from the target test to the battle laid out by the scene's markers.</summary>
        public void BattleLayout()
        {
            targetTest = false;
            tubeTest = false;
            Spawn();
            message = "";
        }

        private void SpawnTargets()
        {
            // (The weapons, from the scene's markers, whatever they stand for: a sword, a shield, a club.)
            WeaponDefinition sword = null, shield = null, club = null;
            foreach (var spawn in FindObjectsByType<UnitSpawn>(FindObjectsInactive.Include))
            {
                foreach (var w in new[] { spawn.mainHand, spawn.offHand })
                {
                    if (w == null)
                    {
                        continue;
                    }

                    if (w.IsShield) shield = shield != null ? shield : w;
                    else if (w.kind == WeaponKind.Sword) sword = sword != null ? sword : w;
                    else if (w.kind == WeaponKind.Club) club = club != null ? club : w;
                }
            }

            if (sword == null || shield == null || (largeAttackers && club == null))
            {
                message = "The target test takes its sword, shield and club from the scene's unit markers: one of them is missing.";
            }

            var attackerBody = largeAttackers ? BodySize.Large : BodySize.Medium;
            var targetBody = largeTargets ? BodySize.Large : BodySize.Medium;
            string[] kinds = { "open", "shield", "sword" };
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * TargetPairsApart;
                Vector3 spot = Grid.origin + new Vector3(x, 0f, 0.5f * TargetFromAttacker);
                var target = SpawnUnit(targetBody, 1, spot, $"Target {kinds[i]}", i == 0 ? null : sword, i == 1 ? shield : null);
                if (target != null)
                {
                    target.TargetDummy = true;
                    target.DummySpot = spot;
                    target.Unharmed = !targetsHarmed;
                }

                if (tubeTest)
                {
                    // (The tube test: the targets alone. What strikes them is a tube.)
                    continue;
                }

                // (A Large's weapon is its club; a Medium's its sword.)
                var attacker = SpawnUnit(attackerBody, 0, Grid.origin + new Vector3(x, 0f, -0.5f * TargetFromAttacker), $"Attacker {i + 1}", largeAttackers ? club : sword, null);
                if (target == null || attacker == null)
                {
                    continue;
                }

                attacker.OnlyTarget = target;
            }
        }

        // Weapons go in the hands once the unit has built its rig (its Awake runs on Instantiate).
        private readonly List<(UnitController unit, WeaponDefinition main, WeaponDefinition off)> pendingEquip =
            new List<(UnitController, WeaponDefinition, WeaponDefinition)>();

        /// <summary>The body material takes the team colour; the joints keep theirs.</summary>
        public static void Tint(UnitController unit, Material material)
        {
            if (material == null)
            {
                return;
            }

            foreach (var renderer in unit.GetComponentsInChildren<MeshRenderer>(true))
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].name.StartsWith("M_Body_Main"))
                    {
                        mats[i] = material;
                    }
                }

                renderer.sharedMaterials = mats;
            }
        }

        private void Clear()
        {
            foreach (var unit in units)
            {
                if (unit != null)
                {
                    Destroy(unit.gameObject);
                }
            }

            units.Clear();
            pendingEquip.Clear();
            Tubes.Clear();
            tubesSpawnedAt = Application.isPlaying ? Time.time : 0f;
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            foreach (var (unit, main, off) in pendingEquip)
            {
                if (unit != null && unit.IsInitialized)
                {
                    unit.Equip(main, off);
                    if (PlaytestTrace.Enabled)
                    {
                        PlaytestTrace.Note(unit, "equip", $"{(main != null ? main.name : "bare hands")}{(off != null ? " and " + off.name : "")}, scaled {unit.Loadout.Scale:0.00}");
                    }
                }
            }

            pendingEquip.Clear();
            using (Perf.Begin(PerfSection.Sandbox))
            {
                StepFight();
                StepTubes();
            }
        }

        private void StepFight()
        {
            if (turnedAway)
            {
                brawl.Fight = false;
                brawl.TurnedAway = true;
                brawl.Grid = Grid;
                brawl.Step(units, Time.fixedDeltaTime, Time.time);
            }
            else
            {
                fightLoop.Fight = fight;
                fightLoop.Grid = Grid;
                fightLoop.Step(units, Time.fixedDeltaTime, Time.time);
                if (fightLoop.Over && !announced)
                {
                    announced = true;
                    message = $"Fight over: {fightLoop.Result}. R for another.";
                    Note("fight over: " + fightLoop.Result);
                }
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || Time.frameCount <= watchFrom)
            {
                return;   // (the key that left the lab is not the fight's too)
            }

            if (SandboxInput.KeyPressed(KeyCode.R)) Spawn();
            if (SandboxInput.KeyPressed(KeyCode.P)) TogglePause();
            if (SandboxInput.KeyPressed(KeyCode.Period)) StepOnce();
            if (SandboxInput.KeyPressed(KeyCode.T)) TurnAround();
            if (SandboxInput.KeyPressed(KeyCode.M)) Mark();
            if (SandboxInput.KeyPressed(KeyCode.F)) ToggleFight();
            if (SandboxInput.KeyPressed(KeyCode.B)) BystanderTest();
            if (SandboxInput.KeyPressed(KeyCode.G)) Gallery();
            if (SandboxInput.KeyPressed(KeyCode.L)) LaunchTest();
            if (SandboxInput.KeyPressed(KeyCode.H)) RhythmGallery();
            if (SandboxInput.KeyPressed(KeyCode.K)) SeverTest();
            if (SandboxInput.KeyPressed(KeyCode.D)) GroundTest();
            if (SandboxInput.KeyPressed(KeyCode.J)) ShieldTest();
            if (SandboxInput.KeyPressed(KeyCode.Alpha1)) TargetTest();
            if (SandboxInput.KeyPressed(KeyCode.Alpha2)) StepBackTest();
            if (SandboxInput.KeyPressed(KeyCode.Alpha3)) TurnTest();
            if (SandboxInput.KeyPressed(KeyCode.Alpha4)) KnockLegTest();
            if (SandboxInput.KeyPressed(KeyCode.Alpha5)) TubeTestScene();
            if (SandboxInput.KeyPressed(KeyCode.Alpha6)) TubeHit();
        }

        /// <summary>
        /// To see a physical leg take a knock on demand: every fighter on its feet has its right shin knocked sideways
        /// (<see cref="KnockLegSpeed"/> m/s), as a blow to the leg knocks it; the leg swings aside and its muscles bring
        /// it back. Only with physical legs on: without, a leg goes where the animation puts it and nothing moves it.
        /// </summary>
        public void KnockLegTest()
        {
            int knocked = 0, all = 0;
            foreach (var u in units)
            {
                if (u == null || !u.IsInitialized || !u.IsAlive || u.Clip == null)
                {
                    continue;
                }

                all++;
                if (u.Clip.KnockLegNow(KnockLegSpeed))
                {
                    knocked++;
                }
            }

            message = knocked > 0
                ? $"Leg knock: the right shin of {knocked} of {all} fighters knocked to its left at {KnockLegSpeed:0} m/s. Expect a small twitch (a few centimetres, a tenth of a second), not a swing: see the README. 4 or the button again to repeat."
                : "Leg knock: nothing to knock. Switch 'Physical legs' on first: without it a leg goes where the animation puts it and nothing moves it.";
            Note($"leg knock test: {knocked} of {all} knocked");
        }

        /// <summary>The leg knock test's knock (m/s at a Medium's size).</summary>
        public const float KnockLegSpeed = 6f;

        /// <summary>
        /// To see a turn on demand: every fighter standing still turns now, a quarter turn and a bit to its right
        /// (<see cref="TurnTestDegrees"/>) with its turn's animation, as it does to face an enemy at its side; the fight
        /// then turns it back to its opponent, which is a second turn the other way. Those in a strike, a reaction or a
        /// step do not. Best in the target test, slowed down or stepped.
        /// </summary>
        public void TurnTest()
        {
            int turned = 0, all = 0;
            foreach (var u in units)
            {
                if (u == null || !u.IsInitialized || !u.IsAlive || u.Clip == null)
                {
                    continue;
                }

                all++;
                if (u.Clip.TurnNow(TurnTestDegrees))
                {
                    turned++;
                }
            }

            bool ball = PhysicsBattler.Locomotion.LocoBody.PivotsOnBall;
            message = $"Turn: {turned} of {all} fighters turned {TurnTestDegrees:0} degrees to their right, and will turn back to their opponents "
                + $"({(ball ? "planted feet pivot on the ball of the foot" : "as before: planted feet go round about the ankle")}). Watch the foot that stays on the ground. "
                + "3 or the button again to repeat; the speed buttons or Pause and Step to see it slowly.";
            Note($"turn test: {turned} of {all} turned, {(ball ? "pivoting on the ball" : "about the ankle")}");
        }

        /// <summary>The turn test's turn (degrees, to the right).</summary>
        public const float TurnTestDegrees = 110f;

        /// <summary>
        /// To see a step back on demand: every fighter standing in its guard steps back now from its opponent (or the
        /// nearest enemy), as it does to a blow it reads at the last moment: a third of a metre, a tenth of a second's
        /// warning. One in a strike or a reaction, or down, does not. In a fight or in the target test; best seen in the
        /// target test, slowed down or stepped.
        /// </summary>
        public void StepBackTest()
        {
            int stepped = 0, standing = 0;
            foreach (var u in units)
            {
                if (u == null || !u.IsInitialized || !u.IsAlive || u.Clip == null)
                {
                    continue;
                }

                standing++;
                UnitController from = u.Opponent != null && u.Opponent.IsAlive && u.Opponent.Clip != null ? u.Opponent : null;
                if (from == null)
                {
                    float nearest = float.MaxValue;
                    foreach (var o in units)
                    {
                        if (o == null || o == u || !o.IsInitialized || !o.IsAlive || o.Clip == null || o.Team == u.Team)
                        {
                            continue;
                        }

                        float d = (o.Clip.Loco.Position - u.Clip.Loco.Position).sqrMagnitude;
                        if (d < nearest)
                        {
                            nearest = d;
                            from = o;
                        }
                    }
                }

                // (With no enemy left: straight back the way it faces.)
                Vector3 away = from != null ? from.Clip.Loco.Position : u.Clip.Loco.Position;
                if (u.Clip.StepBackNow(away, StepBackTestMetres, StepBackTestWarning))
                {
                    stepped++;
                }
            }

            bool hop = PhysicsBattler.Locomotion.ClipFighter.StepBackHops;
            message = $"Step back: {stepped} of {standing} fighters stepped back {StepBackTestMetres * 100f:0} cm ({(hop ? "a hop: both feet off and down together" : "as before: each foot let go and brought after")}). "
                + "Those in a strike or a reaction did not. 2 or the button again to repeat; the speed buttons or Pause and Step to see it slowly.";
            Note($"step back test: {stepped} of {standing} stepped back, {(hop ? "hopping" : "not hopping")}");
        }

        /// <summary>The step back test's step: how far (metres at a Medium's size) and how long before the blow it would be to (seconds). A late sword arm's: the common one.</summary>
        public const float StepBackTestMetres = 0.35f, StepBackTestWarning = 0.1f;

        /// <summary>
        /// Milestone 3's gate, on demand: a sword unit strikes at an enemy with a third unit standing
        /// in the way of the blow. The one in the way should take it. Fighting stops for the test (F
        /// to resume); the rest of the units wait at the side.
        /// </summary>
        private Coroutine bystanderRun, galleryRun;

        /// <summary>
        /// Every move in turn, against a dummy: one unit of each weapon strikes at an enemy in the
        /// middle of the board, first slowly (a third of the speed), then at full speed. Its blows
        /// don't hurt during the gallery. Fighting stops for it; G again (or F) ends it.
        /// </summary>
        public void Gallery()
        {
            if (galleryRun != null)
            {
                StopCoroutine(galleryRun);
                galleryRun = null;
                Time.timeScale = paused ? 0f : timeScale;
                message = "Gallery stopped.";
                return;
            }

            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive);
            var attackers = new List<UnitController>();
            foreach (WeaponKind kind in new[] { WeaponKind.Sword, WeaponKind.Club })
            {
                var unit = alive.Find(u => u.Loadout?.Main != null && u.Loadout.Main.kind == kind && !attackers.Contains(u));
                if (unit != null)
                {
                    attackers.Add(unit);
                }
            }

            if (attackers.Count == 0 || alive.Count < 2)
            {
                message = "The gallery needs an armed unit and one other. Press R to reset.";
                return;
            }

            fight = false;
            galleryRun = StartCoroutine(GalleryRun(attackers, alive));
        }

        private System.Collections.IEnumerator GalleryRun(List<UnitController> attackers, List<UnitController> alive)
        {
            foreach (var attacker in attackers)
            {
                var target = alive.Find(u => u.Team != attacker.Team) ?? alive.Find(u => u != attacker);
                bool hurts = attacker.DealsHits;
                attacker.DealsHits = false;
                try
                {
                    foreach (var move in attacker.Loadout.Main.moves)
                    {
                        if (move == null)
                        {
                            continue;
                        }

                        foreach (float speed in new[] { 0.33f, 1f })
                        {
                            // The pair in the middle, the rest off to the side.
                            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
                            attacker.Teleport(new Vector3(-0.5f * distance, 0f, 0f), 90f);
                            target.Teleport(new Vector3(0.5f * distance, 0f, 0f), 270f);
                            int k = 0;
                            foreach (var other in alive)
                            {
                                if (other != attacker && other != target && other.IsAlive)
                                {
                                    other.Teleport(new Vector3(-4f + 1.6f * k++, 0f, 5.2f), 180f);
                                }
                            }

                            Time.timeScale = paused ? 0f : timeScale;
                            yield return new WaitForSeconds(0.7f);
                            MoveState state = null;
                            foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Head, AimPart.Neck, AimPart.KneeL, AimPart.ElbowL })
                            {
                                if (state == null && move.targets.Contains(part))
                                {
                                    var s = MovePlanner.Prepare(attacker, target, move, part);
                                    state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                                }
                            }

                            string name = move.name.Replace("MD_", "");
                            if (state == null || !attacker.StartMove(state))
                            {
                                message = $"Gallery: {attacker.name}'s {name} can't reach {target.name} from there.";
                                yield return new WaitForSeconds(1f);
                                continue;
                            }

                            message = $"Gallery: {attacker.name}'s {name} at {target.name}'s {state.Part}{(speed < 1f ? ", slowed to a third" : "")}. G stops.";
                            Note($"gallery: {attacker.name} {name} at {state.Part}, speed {speed:0.00}");
                            Time.timeScale = paused ? 0f : timeScale * speed;
                            while (attacker.IsAttacking)
                            {
                                yield return null;
                            }

                            Time.timeScale = paused ? 0f : timeScale;
                        }
                    }
                }
                finally
                {
                    attacker.DealsHits = hurts;
                    Time.timeScale = paused ? 0f : timeScale;
                }
            }

            message = "Gallery done. F to fight again.";
            galleryRun = null;
        }

        public void BystanderTest()
        {
            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive);
            var attacker = alive.Find(u => u.Loadout?.Main != null && u.Loadout.Main.kind == WeaponKind.Sword)
                           ?? alive.Find(u => u.Loadout?.Main != null);
            var target = attacker != null ? alive.Find(u => u.Team != attacker.Team) : null;
            var bystander = target != null ? alive.Find(u => u != attacker && u != target) : null;
            if (bystander == null)
            {
                message = "The bystander test needs three living units, one armed. Press R to reset.";
                return;
            }

            fight = false;
            if (bystanderRun != null)
            {
                StopCoroutine(bystanderRun);
            }

            bystanderRun = StartCoroutine(Bystander(attacker, target, bystander, alive));
        }

        private System.Collections.IEnumerator Bystander(UnitController attacker, UnitController target, UnitController bystander, List<UnitController> alive)
        {
            // Along the board's middle: the attacker facing +X, the target where it would engage, the
            // bystander off to the side until the blow is planned, then stood in its way.
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            Vector3 start = new Vector3(-distance * 0.5f, 0f, 0f);
            attacker.Teleport(start, 90f);
            target.Teleport(start + Vector3.right * distance, 270f);
            bystander.Teleport(new Vector3(0f, 0f, -3f), 0f);
            int k = 0;
            foreach (var other in alive)
            {
                if (other != attacker && other != target && other != bystander)
                {
                    other.Teleport(new Vector3(-4f + 1.6f * k++, 0f, 5.2f), 180f);
                }
            }

            Note($"bystander test: {attacker.name} strikes at {target.name} with {bystander.name} in the way");
            message = $"Bystander test: {attacker.name} → {target.name}, {bystander.name} in the way. F to fight again.";
            yield return new WaitForSeconds(0.8f);
            // A cut (the weapon's first move) at the belly, else whatever the weapon can land.
            MoveState state = null;
            foreach (var move in attacker.Loadout.Main.moves)
            {
                foreach (var part in new[] { AimPart.Belly, AimPart.Chest, AimPart.Head })
                {
                    if (state == null && move != null && move.targets.Contains(part))
                    {
                        var s = MovePlanner.Prepare(attacker, target, move, part);
                        state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                    }
                }
            }

            if (state == null)
            {
                message = "Bystander test: no blow reaches the target from there.";
                yield break;
            }

            Vector3 at = MovePlanner.InTheWay(attacker, state, 0.3f);
            var chest = bystander.Rig.Part(PartId.Chest);
            Vector3 torso = (chest.body != null ? chest.body.position : chest.transform.position) - bystander.Root.position;
            bystander.Teleport(new Vector3(at.x - torso.x, 0f, at.z - torso.z), 270f);
            yield return new WaitForSeconds(0.25f);
            if (!attacker.StartMove(state))
            {
                message = "Bystander test: the blow didn't start (press B again).";
            }
        }

        /// <summary>
        /// Milestone 4's gate, on demand: the heaviest armed unit strikes the lightest enemy, with a
        /// third unit standing where the blow will throw it. The struck body should be launched (or
        /// at least knocked down) into the third, hurting it, and both should get up again.
        /// </summary>
        public void LaunchTest()
        {
            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive);
            UnitController attacker = null, target = null, bystander = null;
            foreach (var u in alive)
            {
                if (u.Loadout?.Main != null && (attacker == null || u.Body.Mass > attacker.Body.Mass))
                {
                    attacker = u;
                }
            }

            foreach (var u in alive)
            {
                if (attacker != null && u.Team != attacker.Team && (target == null || u.Body.Mass < target.Body.Mass))
                {
                    target = u;
                }
            }

            bystander = target != null ? alive.Find(u => u != attacker && u != target && u.Team == target.Team)
                                         ?? alive.Find(u => u != attacker && u != target) : null;
            if (bystander == null)
            {
                message = "The launch test needs three living units, one armed. Press R to reset.";
                return;
            }

            fight = false;
            if (bystanderRun != null)
            {
                StopCoroutine(bystanderRun);
            }

            bystanderRun = StartCoroutine(Launch(attacker, target, bystander, alive));
        }

        private System.Collections.IEnumerator Launch(UnitController attacker, UnitController target, UnitController bystander, List<UnitController> alive)
        {
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            Vector3 start = new Vector3(-distance * 0.5f, 0f, 0f);
            attacker.Teleport(start, 90f);
            target.Teleport(start + Vector3.right * distance, 270f);
            bystander.Teleport(new Vector3(0f, 0f, -4f), 0f);
            int k = 0;
            foreach (var other in alive)
            {
                if (other != attacker && other != target && other != bystander)
                {
                    other.Teleport(new Vector3(-4f + 1.6f * k++, 0f, 5.2f), 180f);
                }
            }

            yield return new WaitForSeconds(0.8f);
            // The weapon's heaviest swing at the chest, else whatever lands.
            MoveState state = null;
            foreach (var move in attacker.Loadout.Main.moves)
            {
                foreach (var part in new[] { AimPart.Chest, AimPart.Belly, AimPart.Head })
                {
                    if (state == null && move != null && move.shape == MoveShape.Swing && move.targets.Contains(part))
                    {
                        var s = MovePlanner.Prepare(attacker, target, move, part);
                        state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                    }
                }
            }

            if (state == null)
            {
                message = "Launch test: no swing reaches the target from there.";
                yield break;
            }

            // Where the blow will send it: along the weapon's path at contact.
            float c = state.Move.ContactProgress;
            Vector3 along = state.PathPoint(Mathf.Min(1f, c + 0.01f)) - state.PathPoint(Mathf.Max(0f, c - 0.01f));
            along.y = 0f;
            along = along.sqrMagnitude > 1e-8f ? along.normalized : Vector3.right;
            Vector3 lands = target.Root.position + along * (0.9f * target.Body.SizeFactor + 0.7f * bystander.Body.SizeFactor);
            bystander.Teleport(lands, Mathf.Atan2(-along.x, -along.z) * Mathf.Rad2Deg);
            Note($"launch test: {attacker.name} strikes {target.name} toward {bystander.name}");
            message = $"Launch test: {attacker.name} → {target.name}, thrown toward {bystander.name}. F to fight again.";
            yield return new WaitForSeconds(0.3f);
            if (!attacker.StartMove(state))
            {
                message = "Launch test: the blow didn't start (press L again).";
            }
        }

        /// <summary>
        /// Milestone 9b's test, on demand: two units go down in the middle of the board, one knocked
        /// down and one killed, and the others walk back and forth straight across them for 20 s (no
        /// steering round them: the hard case). The bodies should be nudged and rolled, never flung,
        /// and their joints should stay within their limits. D again stops it.
        /// </summary>
        public void GroundTest()
        {
            if (bystanderRun != null)
            {
                StopCoroutine(bystanderRun);
                bystanderRun = null;
                if (groundTesting)
                {
                    groundTesting = false;
                    message = "Ground test stopped. F to fight again.";
                    return;
                }
            }

            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive);
            if (alive.Count < 3)
            {
                message = "The ground test needs three living units. Press R to reset.";
                return;
            }

            fight = false;
            bystanderRun = StartCoroutine(GroundRun(alive));
        }

        private bool groundTesting;

        private System.Collections.IEnumerator GroundRun(List<UnitController> alive)
        {
            groundTesting = true;
            // The two lightest go down, side by side across the middle; the rest stand off to either side.
            alive.Sort((a, b) => a.Body.Mass.CompareTo(b.Body.Mass));
            var downed = alive[0];
            var dead = alive[1];
            downed.Teleport(new Vector3(-0.5f, 0f, 0f), 0f);
            dead.Teleport(new Vector3(0.5f, 0f, 0f), 0f);
            var walkers = alive.GetRange(2, alive.Count - 2);
            for (int i = 0; i < walkers.Count; i++)
            {
                walkers[i].Teleport(new Vector3(-1.2f + 1.2f * (i % 3), 0f, i % 2 == 0 ? -2.5f : 2.5f), i % 2 == 0 ? 0f : 180f);
            }

            yield return new WaitForSeconds(0.5f);
            downed.DropForTest(false, new Vector3(0f, 0f, -1.2f));
            dead.DropForTest(true, new Vector3(0f, 0f, 1.2f));
            Note("ground test: " + downed.name + " knocked down, " + dead.name + " killed; the others walk across them for 20 s");
            message = $"Ground test: {downed.name} knocked down, {dead.name} killed; the others walk across them. D to stop.";
            yield return new WaitForSeconds(1.5f);

            // Back and forth across the bodies, each on its own line, for 20 s.
            float end = Time.time + 20f;
            var goals = new Dictionary<UnitController, float>();
            foreach (var w in walkers)
            {
                goals[w] = -Mathf.Sign(w.Root.position.z) * 2.5f;
            }

            while (Time.time < end)
            {
                yield return new WaitForFixedUpdate();
                foreach (var w in walkers)
                {
                    if (w == null || !w.IsAlive || !w.CanAct)
                    {
                        continue;
                    }

                    Vector3 p = w.Root.position;
                    Vector3 goal = new Vector3(p.x, p.y, goals[w]);
                    if (Mathf.Abs(goal.z - p.z) < 0.15f)
                    {
                        goals[w] = -goals[w];
                        continue;
                    }

                    Vector3 step = Vector3.ClampMagnitude(goal - p, 0.8f * w.WalkSpeed * Time.fixedDeltaTime);
                    w.Root.position = p + step;
                    w.FacePoint(goal);
                }
            }

            groundTesting = false;
            bystanderRun = null;
            Note("ground test: over");
            message = "Ground test over. M marks the log; F to fight again.";
        }

        /// <summary>
        /// Milestone 5's gate, on demand: a sword unit cuts at an enemy's elbow (whichever of its cuts
        /// reaches one), and the arm should come off there. K again for another go.
        /// </summary>
        public void SeverTest()
        {
            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive && u.CanAct);
            var attacker = alive.Find(u => u.Loadout?.Main != null && u.Loadout.Main.kind == WeaponKind.Sword);
            UnitController target = null;
            foreach (var u in alive)
            {
                if (attacker != null && u.Team != attacker.Team && (target == null || Mathf.Abs(u.Body.Mass - attacker.Body.Mass) < Mathf.Abs(target.Body.Mass - attacker.Body.Mass)))
                {
                    target = u;
                }
            }

            if (target == null)
            {
                message = "The sever test needs a sword unit and a standing enemy. Press R to reset.";
                return;
            }

            fight = false;
            if (bystanderRun != null)
            {
                StopCoroutine(bystanderRun);
            }

            bystanderRun = StartCoroutine(SeverRun(attacker, target, alive));
        }

        private System.Collections.IEnumerator SeverRun(UnitController attacker, UnitController target, List<UnitController> alive)
        {
            float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * target.Body.SizeFactor;
            Vector3 start = new Vector3(-distance * 0.5f, 0f, 0f);
            attacker.Teleport(start, 90f);
            target.Teleport(start + Vector3.right * distance, 270f);
            int k = 0;
            foreach (var other in alive)
            {
                if (other != attacker && other != target)
                {
                    other.Teleport(new Vector3(-4f + 1.6f * k++, 0f, 5.2f), 180f);
                }
            }

            yield return new WaitForSeconds(0.8f);
            MoveState state = null;
            foreach (var part in new[] { AimPart.ElbowR, AimPart.ElbowL })
            {
                foreach (var move in attacker.Loadout.Main.moves)
                {
                    if (state == null && move != null && move.strikeWith == StrikeWith.Edge && move.targets.Contains(part) && !target.Rig.IsSevered(AimPartInfo.BodyPartOf(part)))
                    {
                        var s = MovePlanner.Prepare(attacker, target, move, part);
                        state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                    }
                }
            }

            if (state == null)
            {
                message = "Sever test: no cut reaches an elbow from there (press K again).";
                yield break;
            }

            Note($"sever test: {attacker.name} cuts at {target.name}'s {state.Part}");
            message = $"Sever test: {attacker.name} cuts at {target.name}'s {state.Part} with {state.Move.name.Replace("MD_", "")}. F to fight again.";
            if (!attacker.StartMove(state))
            {
                message = "Sever test: the blow didn't start (press K again).";
            }
        }

        /// <summary>
        /// Milestone 9c: a shield-holder squared up in the middle of the board; a sword unit and a club
        /// unit take turns striking at it from in front (two blows each), so the shield's block, the
        /// knock and the return can be judged by eye. J again for another round.
        /// </summary>
        public void ShieldTest()
        {
            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive && u.CanAct);
            var defender = alive.Find(u => u.Loadout != null && u.Loadout.HasShield && !u.Rig.IsSevered(PartId.HandL));
            var attackers = new List<UnitController>();
            if (defender != null)
            {
                foreach (var kind in new[] { WeaponKind.Sword, WeaponKind.Club })
                {
                    var a = alive.Find(u => u != defender && u.Team != defender.Team && u.Loadout?.Main != null && u.Loadout.Main.kind == kind)
                            ?? alive.Find(u => u != defender && u.Loadout?.Main != null && u.Loadout.Main.kind == kind);
                    if (a != null)
                    {
                        attackers.Add(a);
                    }
                }
            }

            if (defender == null || attackers.Count == 0)
            {
                message = "The shield test needs a unit with a shield and an armed unit. Press R to reset.";
                return;
            }

            fight = false;
            if (bystanderRun != null)
            {
                StopCoroutine(bystanderRun);
            }

            bystanderRun = StartCoroutine(ShieldRun(defender, attackers, alive));
        }

        private System.Collections.IEnumerator ShieldRun(UnitController defender, List<UnitController> attackers, List<UnitController> alive)
        {
            float keep = defender.Style.blockChance;
            defender.Style.blockChance = 1f;
            try
            {
                int k = 0;
                foreach (var other in alive)
                {
                    if (other != defender && !attackers.Contains(other))
                    {
                        other.Teleport(new Vector3(-4f + 1.6f * k++, 0f, 5.2f), 180f);
                    }
                }

                for (int round = 0; round < 2 * attackers.Count; round++)
                {
                    var attacker = attackers[round % attackers.Count];
                    if (attacker == null || !attacker.IsAlive || defender == null || !defender.IsAlive)
                    {
                        break;
                    }

                    // (The one not striking stands aside.)
                    foreach (var a in attackers)
                    {
                        if (a != attacker)
                        {
                            a.Teleport(new Vector3(3.5f, 0f, -3f), 270f);
                        }
                    }

                    float distance = attacker.Style.engageDistance * attacker.Body.SizeFactor + 0.25f * defender.Body.SizeFactor;
                    defender.Teleport(Vector3.zero, 90f);
                    attacker.Teleport(Vector3.right * distance, 270f);
                    defender.Opponent = attacker;
                    attacker.Opponent = defender;
                    yield return new WaitForSeconds(1f);
                    MoveState state = null;
                    foreach (var part in new[] { AimPart.Chest, AimPart.Head, AimPart.Belly })
                    {
                        foreach (var move in attacker.Loadout.Main.moves)
                        {
                            if (state == null && move != null && move.targets.Contains(part))
                            {
                                var s = MovePlanner.Prepare(attacker, defender, move, part);
                                state = s != null && MovePlanner.Acceptable(attacker, s) ? s : null;
                            }
                        }
                    }

                    if (state == null || !attacker.StartMove(state))
                    {
                        Note($"shield test: {attacker.name} found no blow from there");
                        continue;
                    }

                    Note($"shield test: {attacker.name} strikes at {defender.name}'s {state.Part} with {state.Move.name.Replace("MD_", "")}");
                    message = $"Shield test: {attacker.name} strikes at {defender.name} ({state.Move.name.Replace("MD_", "")}). Watch the shield's knock and return. J for another round.";
                    yield return new WaitForSeconds(2.5f);
                }
            }
            finally
            {
                if (defender != null)
                {
                    defender.Style.blockChance = keep;
                }
            }

            bystanderRun = null;
            Note("shield test: over");
            message = "Shield test over. M marks the log; J for another round; F to fight again.";
        }

        private Coroutine rhythmRun;
        private bool tiredAll;

        /// <summary>
        /// The Fight Rhythms on show: each style in turn (one unit of each) squared up to an opponent
        /// in the middle of the board: its base for a few seconds, then every posture slowed to 40%
        /// and at full speed. Fighting stops for it; H again ends it.
        /// </summary>
        public void RhythmGallery()
        {
            if (rhythmRun != null)
            {
                StopCoroutine(rhythmRun);
                rhythmRun = null;
                Time.timeScale = paused ? 0f : timeScale;
                foreach (var u in units)
                {
                    if (u != null && u.Rhythm != null)
                    {
                        u.Rhythm.Auto = true;
                    }
                }

                message = "Rhythm gallery stopped.";
                return;
            }

            var alive = units.FindAll(u => u != null && u.IsInitialized && u.IsAlive && u.Rhythm != null);
            var shown = new List<UnitController>();
            foreach (var u in alive)
            {
                if (shown.Find(s => s.Rhythm.Definition == u.Rhythm.Definition) == null)
                {
                    shown.Add(u);
                }
            }

            if (shown.Count == 0 || alive.Count < 2)
            {
                message = "The rhythm gallery needs two living units. Press R to reset.";
                return;
            }

            fight = false;
            rhythmRun = StartCoroutine(RhythmRun(shown, alive));
        }

        private System.Collections.IEnumerator RhythmRun(List<UnitController> shown, List<UnitController> alive)
        {
            foreach (var unit in shown)
            {
                var opponent = alive.Find(u => u != unit && u.Team != unit.Team) ?? alive.Find(u => u != unit);
                float distance = unit.Style.engageDistance * unit.Body.SizeFactor + 0.25f * opponent.Body.SizeFactor;
                unit.Teleport(new Vector3(-0.5f * distance, 0f, 0f), 90f);
                opponent.Teleport(new Vector3(0.5f * distance, 0f, 0f), 270f);
                int k = 0;
                foreach (var other in alive)
                {
                    if (other != unit && other != opponent)
                    {
                        other.Teleport(new Vector3(-4f + 1.6f * k++, 0f, 5.2f), 180f);
                    }
                }

                unit.Rhythm.Auto = false;
                unit.Opponent = opponent;
                string style = unit.Rhythm.Definition.Label;
                message = $"Rhythm gallery: {unit.name}, {style}: the base. H stops.";
                Note($"rhythm gallery: {unit.name} {style} base");
                Time.timeScale = paused ? 0f : timeScale;
                yield return new WaitForSeconds(3f);
                foreach (var posture in unit.Rhythm.Definition.postures)
                {
                    if (posture == null)
                    {
                        continue;
                    }

                    foreach (float speed in new[] { 0.4f, 1f })
                    {
                        message = $"Rhythm gallery: {unit.name}, {style}: {posture.Label}{(speed < 1f ? ", slowed to 40%" : "")}. H stops.";
                        Note($"rhythm gallery: {unit.name} {posture.Label} at speed {speed:0.0}");
                        Time.timeScale = paused ? 0f : timeScale * speed;
                        unit.Rhythm.Force(posture);
                        while (unit.Rhythm.Current == posture)
                        {
                            yield return null;
                        }

                        Time.timeScale = paused ? 0f : timeScale;
                        yield return new WaitForSeconds(0.8f);
                    }
                }

                unit.Rhythm.Auto = true;
            }

            Time.timeScale = paused ? 0f : timeScale;
            message = "Rhythm gallery done. F to fight again.";
            rhythmRun = null;
        }

        public void ToggleFight()
        {
            fight = !fight;
            Note(fight ? "fighting on" : "fighting off");
        }

        public void TurnAround()
        {
            turnedAway = !turnedAway;
            Note(turnedAway ? "turn away from the enemy" : "turn back to the enemy");
        }

        public void Mark()
        {
            if (recorder != null && recorder.Recording)
            {
                recorder.Mark();
                message = $"Mark {recorder.MarkCount} written to the log.";
            }
        }

        private void TogglePause()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : timeScale;
            Note(paused ? "pause" : "resume");
        }

        private void StepOnce()
        {
            if (!paused)
            {
                TogglePause();
            }

            StartCoroutine(StepRoutine());
        }

        private IEnumerator StepRoutine()
        {
            Time.timeScale = 1f;
            yield return new WaitForFixedUpdate();
            Time.timeScale = 0f;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (uiScale < 0f)
            {
                uiScale = PlayerPrefs.GetFloat(UiScaleKey, DefaultUiScale);
            }

            // (The legs' opinion under each fighter, at the screen's own size, under the panel and the labels.)
            if (PhysicsBattler.Locomotion.CombatFeel.ShowLegs && Camera.main != null)
            {
                foreach (var unit in units)
                {
                    if (unit != null && unit.IsInitialized && unit.IsAlive && unit.Clip != null && !unit.Clip.Fallen)
                    {
                        GroundingView.Draw(unit.Clip.Ground, Camera.main, unit.Clip.Size);
                    }
                }
            }

            // (The panel and the units' labels scaled together, text and buttons alike: at the screen's own size they
            // were too small to read.)
            var was = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
            panelRect = GUILayout.Window(4711, panelRect, DrawPanel, "Physics Battler", GUILayout.Width(290));
            // (Never dragged off the screen: its title bar stays where it can be grabbed.)
            panelRect.x = Mathf.Clamp(panelRect.x, 60f - panelRect.width, Mathf.Max(0f, Screen.width / uiScale - 60f));
            panelRect.y = Mathf.Clamp(panelRect.y, 0f, Mathf.Max(0f, Screen.height / uiScale - 30f));
            foreach (var unit in units)
            {
                DrawUnitLabel(unit);
            }

            GUI.matrix = was;
        }

        /// <summary>How large the panel and the labels are drawn (times the screen's own size; kept between sessions; below 0: not read yet).</summary>
        private static float uiScale = -1f;

        private const string UiScaleKey = "BattleSandbox.UiScale";

        /// <summary>The size they are drawn at until it is changed with the panel's own buttons.</summary>
        public const float DefaultUiScale = 1.8f;

        private void DrawPanel(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Panel size {uiScale:0.0}x");
            if (GUILayout.Button("\u2212", GUILayout.Width(36f)))
            {
                uiScale = Mathf.Max(0.8f, uiScale - 0.1f);
                PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            }

            if (GUILayout.Button("+", GUILayout.Width(36f)))
            {
                uiScale = Mathf.Min(3f, uiScale + 0.1f);
                PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            }

            GUILayout.EndHorizontal();
            GUILayout.Label("R reset · F fight · 1 target test · B bystander · L launch · K sever · D ground · G gallery · H rhythms · P pause · . step · T turn · M mark");
            GUILayout.Label("Right-drag to orbit, middle-drag to pan, scroll to zoom.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset")) Spawn();
            if (GUILayout.Button(turnedAway ? "Face the enemy" : "Turn around")) TurnAround();
            if (GUILayout.Button(fight ? "Stop fighting" : "Fight")) ToggleFight();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Bystander test (B)")) BystanderTest();
            if (GUILayout.Button(galleryRun != null ? "Stop gallery (G)" : "Move gallery (G)")) Gallery();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Launch test (L)")) LaunchTest();
            if (GUILayout.Button(rhythmRun != null ? "Stop rhythms (H)" : "Rhythm gallery (H)")) RhythmGallery();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sever test (K)")) SeverTest();
            if (GUILayout.Button("Ground test (D)")) GroundTest();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Shield test (J)")) ShieldTest();
            if (GUILayout.Button(targetTest ? "Back to the battle" : "Target test (1)"))
            {
                if (targetTest) BattleLayout(); else TargetTest();
            }
            GUILayout.EndHorizontal();
            // (The tube test: the targets alone, struck by tubes of a set weight and speed.)
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(tubeTest ? "Tube test: reset (R)" : "Tube test (5)")) TubeTestScene();
            if (tubeTest && GUILayout.Button("Hit (6)")) TubeHit();
            GUILayout.EndHorizontal();
            if (tubeTest)
            {
                GUILayout.Label($"How hard: {Tubes.Impulse:0} N·s (a tube of {Tubes.Mass:0.0} kg). A Medium's sword cut is about 45, a Large's club 100 to 300.");
                Tubes.Impulse = Mathf.Round(GUILayout.HorizontalSlider(Tubes.Impulse, 5f, 600f));
                GUILayout.Label($"How fast: {Tubes.Speed:0.0} m/s (slower is heavier for the same hit: more of a shove)");
                Tubes.Speed = Mathf.Round(2f * GUILayout.HorizontalSlider(Tubes.Speed, 2f, 25f)) / 2f;
                GUILayout.Label($"How high: {Tubes.Height * 100f:0} cm over the ground");
                Tubes.Height = Mathf.Round(100f * GUILayout.HorizontalSlider(Tubes.Height, 0.1f, 2.4f)) / 100f;
                GUILayout.Label($"Nearer or further: its end {(Tubes.Reach >= 0f ? $"{Tubes.Reach * 100f:0} cm past the target's middle (it meets more of the body)" : $"{-Tubes.Reach * 100f:0} cm short of the target's middle (it clips the near edge, or misses)")}");
                Tubes.Reach = Mathf.Round(100f * GUILayout.HorizontalSlider(Tubes.Reach, -0.6f, 0.6f)) / 100f;
                GUILayout.Label($"Targets turned: {Tubes.Turned:0} degrees: struck on {Tubes.Side} (0 the front, 90 the left side, 180 the back, 270 the right side)");
                Tubes.Turned = Mathf.Round(GUILayout.HorizontalSlider(Tubes.Turned, 0f, 355f) / 5f) * 5f;
                Tubes.AsBlow = GUILayout.Toggle(Tubes.AsBlow, "Also taken as a blow of the fight's (recoil, stumble, fall); off: the physics only");
                Tubes.Fells = GUILayout.Toggle(Tubes.Fells, "The target goes down, whatever its legs say (the fall test)");
                GUILayout.Label("Falls on demand: which way the targets go down. Each fall is measured in the log (what landed first, the head, the hips).");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Back")) FallTest(0f, false);
                if (GUILayout.Button("Forward")) FallTest(180f, false);
                if (GUILayout.Button("Left")) FallTest(270f, false);
                if (GUILayout.Button("Right")) FallTest(90f, false);
                GUILayout.EndHorizontal();
                if (GUILayout.Button("Thrown back (a Large's club at its hardest)")) FallTest(0f, true);
                if (GUILayout.Button(largeTargets ? "Targets: Larges" : "Targets: Mediums"))
                {
                    largeTargets = !largeTargets;
                    TubeTestScene();
                }
            }

            if (targetTest && !tubeTest)
            {
                // (Each respawns the test with the other body: the attackers with their weapon, a Large's club or a
                // Medium's sword; the targets with what they stand with.)
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(largeAttackers ? "Attackers: Larges" : "Attackers: Mediums"))
                {
                    largeAttackers = !largeAttackers;
                    TargetTest();
                }

                if (GUILayout.Button(largeTargets ? "Targets: Larges" : "Targets: Mediums"))
                {
                    largeTargets = !largeTargets;
                    TargetTest();
                }
                GUILayout.EndHorizontal();
                bool harmed = GUILayout.Toggle(targetsHarmed, "Targets take harm (off: they can't be hurt or cut)");
                if (harmed != targetsHarmed)
                {
                    targetsHarmed = harmed;
                    foreach (var u in units)
                    {
                        if (u != null && u.TargetDummy)
                        {
                            u.Unharmed = !targetsHarmed;
                        }
                    }
                }
            }
            // (The step back: on demand, and as a hop or as it was. The scene's Combat Feel sets the second every
            // frame, so the toggle changes it there.)
            if (GUILayout.Button("Step back now (2): every standing fighter")) StepBackTest();
            var feel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
            bool hops = feel != null ? feel.stepBackHops : PhysicsBattler.Locomotion.ClipFighter.StepBackHops;
            bool hopsNow = GUILayout.Toggle(hops, "A step back is a hop (off: as before)");
            if (hopsNow != hops)
            {
                if (feel != null)
                {
                    feel.stepBackHops = hopsNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.StepBackHops = hopsNow;
            }

            if (GUILayout.Button("Turn now (3): every standing fighter")) TurnTest();
            bool ball = feel != null ? feel.feetPivotOnTheBall : PhysicsBattler.Locomotion.LocoBody.PivotsOnBall;
            bool ballNow = GUILayout.Toggle(ball, "Planted feet pivot on the ball (off: as before)");
            if (ballNow != ball)
            {
                if (feel != null)
                {
                    feel.feetPivotOnTheBall = ballNow;
                }

                PhysicsBattler.Locomotion.LocoBody.PivotsOnBall = ballNow;
            }

            // (Physical legs: off until looked at. The scene's Combat Feel sets it every frame, so the toggle changes it there.)
            bool legs = feel != null ? feel.legsArePhysical : PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs;
            bool legsNow = GUILayout.Toggle(legs, "Physical legs (off: as before 14 November)");
            if (legsNow != legs)
            {
                if (feel != null)
                {
                    feel.legsArePhysical = legsNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs = legsNow;
                message = legsNow
                    ? "Physical legs on: the legs are part of the physical body now. Look at standing, stepping (2), a turn (3), a blow to a leg, a knock-down."
                    : "Physical legs off: the legs go where the animation puts them, as before.";
                Note(legsNow ? "physical legs on" : "physical legs off");
            }

            if (GUILayout.Button("Knock a leg (4): a small twitch; needs physical legs")) KnockLegTest();

            bool tired = GUILayout.Toggle(tiredAll, "Everyone tired (Fight Rhythm energy 25%)");
            if (tired != tiredAll)
            {
                tiredAll = tired;
                foreach (var u in units)
                {
                    if (u != null && u.Rhythm != null)
                    {
                        u.Rhythm.EnergyOverride = tiredAll ? 0.25f : -1f;
                    }
                }
            }
            OrbitCamera.ShakeOnHits = GUILayout.Toggle(OrbitCamera.ShakeOnHits, "Camera shake on heavy hits");
            // Milestone 10 (the arm rebuild's step 6): the weapon arm on joint motors, or on the world servo as before.
            bool muscles = GUILayout.Toggle(PhysicsBattler.Authority.AuthorityController.ArmMuscles, "Weapon arm on joint motors (off: pulled to its pose)");
            if (muscles != PhysicsBattler.Authority.AuthorityController.ArmMuscles)
            {
                PhysicsBattler.Authority.AuthorityController.ArmMuscles = muscles;
                Note("weapon arm: " + (muscles ? "joint motors" : "pulled to its pose"));
            }


            GUILayout.Label($"Time scale: {timeScale:0.00}{(paused ? " (paused)" : "")}");
            float newScale = GUILayout.HorizontalSlider(timeScale, 0.05f, 1f);
            if (!Mathf.Approximately(newScale, timeScale))
            {
                timeScale = newScale;
                if (!paused)
                {
                    Time.timeScale = timeScale;
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(paused ? "Resume" : "Pause")) TogglePause();
            if (GUILayout.Button("Step")) StepOnce();
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(message))
            {
                GUILayout.Label(message);
            }

            GUILayout.Label("Joint probe: " + JointConventions.Report);
            DrawRecorder();
            GUI.DragWindow();
        }

        private void DrawRecorder()
        {
            GUILayout.Space(6);
            if (recorder == null)
            {
                return;
            }

            if (recorder.Recording)
            {
                GUILayout.Label($"Recording PlaytestLogs/{recorder.SessionName} ({recorder.EventCount} events, {recorder.MarkCount} marks)");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Mark (M)")) Mark();
                if (GUILayout.Button("New log")) recorder.NewSession();
                if (GUILayout.Button("Stop")) recorder.StopSession();
                GUILayout.EndHorizontal();
            }
            else if (GUILayout.Button("Start recording"))
            {
                recorder.StartSession();
            }
        }

        private static void DrawUnitLabel(UnitController unit)
        {
            if (unit == null || !unit.IsInitialized || Camera.main == null)
            {
                return;
            }

            Vector3 head = unit.Rig.Part(PartId.Head).transform.position + Vector3.up * 0.3f * unit.Body.SizeFactor;
            Vector3 screen = Camera.main.WorldToScreenPoint(head);
            if (screen.z < 0f)
            {
                return;
            }

            string label = unit.name;
            if (!unit.IsAlive)
            {
                label += " (dead)";
            }
            else
            {
                label += $"\n{unit.Health:0} hp · poise {Mathf.Max(0f, unit.Poise):0}";
                if (unit.CurrentMove != null)
                {
                    label += $"\n{unit.CurrentMove.Move.name.Replace("MD_", "")} → {unit.CurrentMove.Part}";
                }
                else if (unit.BlockBlend > 0.5f)
                {
                    label += "\nblocking";
                }
                else if (unit.State == UnitState.Stumbling)
                {
                    label += "\nstumbling";
                }
                else if (unit.Rhythm?.Current != null)
                {
                    label += "\n" + unit.Rhythm.Current.Label + (unit.Rhythm.CurrentIsLeadIn ? " (lead-in)" : "");
                }

                if (unit.Rig.AnySevered || unit.Bleeding > 0f)
                {
                    var lost = new System.Text.StringBuilder();
                    foreach (var id in PartInfo.All)
                    {
                        if (unit.Rig.IsSevered(id) && PartInfo.HasParent(id) && !unit.Rig.IsSevered(PartInfo.Parent(id)))
                        {
                            lost.Append(lost.Length > 0 ? ", " : "").Append(PartInfo.JointName(id));
                        }
                    }

                    label += $"\ncut at {lost} · bleeding {unit.Bleeding:0.0}/s";
                }

                if (unit.Openings != AimParts.None && unit.State == UnitState.Standing)
                {
                    label += "\nopen: " + unit.Openings.ToString().Replace(", ", " ");
                }
                else if (unit.State == UnitState.Down)
                {
                    label += "\ndown";
                }
                else if (unit.State == UnitState.GettingUp)
                {
                    label += "\ngetting up";
                }
            }

            // (Where the unit is on the screen, in the scaled panel's own measure.)
            float scale = uiScale > 0f ? uiScale : 1f;
            GUI.Label(new Rect(screen.x / scale - 70f, (Screen.height - screen.y) / scale - 48f, 140f, 52f), label);
        }
    }
}
