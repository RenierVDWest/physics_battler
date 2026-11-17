using System.Collections;
using System.Collections.Generic;
using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Locomotion;
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

            if (BattleRetired)
            {
                // (Where the scene would have begun with the battle, it begins in the duel lab.)
                targetTest = true;
                aimTest = true;
                duelLab = true;
                fight = true;
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

        /// <summary>From the locomotion lab straight to the tube test (the lab's 5), as <see cref="WatchFight"/> goes to the battle.</summary>
        public void WatchTubes()
        {
            aimTest = false;
            targetTest = true;
            tubeTest = true;
            fight = true;
            if (!tubeTestBegun)
            {
                tubeTestBegun = true;
                RecallTubes();
            }

            WatchFight();
            message = "Tube test: three targets and a tube beside each. The falls and blows on demand are in the Tube test section; R resets.";
        }

        /// <summary>
        /// Back from the battle, the target test or the tube test to the locomotion lab: the fighters and tubes go,
        /// whatever test was running stops, and the lab's walker is set on its course again.
        /// </summary>
        public void BackToLab()
        {
            StopAllCoroutines();
            bystanderRun = galleryRun = rhythmRun = null;
            Clear();
            targetTest = false;
            tubeTest = false;
            aimTest = false;
            paused = false;
            Time.timeScale = 1f;
            message = "";
            // (The lab steps the engine as it always did, and writes its own log.)
            var stepper = GetComponent<PhysicsBattler.Authority.PhysicsStepper>();
            if (stepper != null)
            {
                stepper.enabled = false;
            }

            if (recorder != null && recorder.Recording)
            {
                recorder.StopSession();
                recordingStoppedForLab = true;
            }

            Note("back to the locomotion lab");
            locomotionLab = true;
            var lab = gameObject.GetComponent<Locomotion.LocomotionLab>();
            if (lab == null)
            {
                lab = gameObject.AddComponent<Locomotion.LocomotionLab>();
            }

            lab.Begin(smallPrefab, mediumPrefab, largePrefab, orbitCamera, teamA, board != null ? board.grid.origin : Vector3.zero);
            enabled = false;
        }

        private bool recordingStoppedForLab;

        /// <summary>The places the two panels go between, in the order of the bar at the top of each.</summary>
        public enum Place
        {
            Lab,
            Battle,
            Targets,
            Tubes,
            Aim,
            Duel,
        }

        /// <summary>
        /// The battle is retired (the scene's Combat Feel, "The Battle Is Retired"; the plan "Fight Rhythm from
        /// Recordings"): it is not among the places, and where the scene would have begun with it, it begins in the
        /// duel lab. The fight is being built again from the duel up, on the made attacks.
        /// </summary>
        public static bool BattleRetired = true;

        private static readonly (Place place, string name, string tip)[] Places =
        {
            (Place.Lab, "Walk lab", "The locomotion lab: one fighter walking its course, to drive, strike and spar with."),
            (Place.Battle, "Battle", "The battle laid out by the scene's markers: the two sides fight."),
            (Place.Targets, "Targets", "The target test (1): three attackers, each on a target that only stands there."),
            (Place.Tubes, "Tubes", "The tube test (5): the three targets alone, struck by tubes: falls and blows on demand."),
            (Place.Aim, "Aim", "The aim test (7): one swordsman and one target that stands there. A blow is asked for one part of the body at a time, and the part it lands on turns red."),
            (Place.Duel, "Duel", "The duel lab (8): one fighter moved about another on demand: walk to it, back away, go round it. Where the fight's movement is built, a step at a time."),
        };

        private static GUIStyle placeStyle, placeNowStyle;

        /// <summary>
        /// The bar at the top of both panels (the lab's and this one's): where one is, and a button to each of the
        /// others, so any of them is one press from any other. The place pressed, or none.
        /// </summary>
        public static Place? PlaceBar(Place now)
        {
            placeStyle ??= new GUIStyle(GUI.skin.button);
            placeNowStyle ??= new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            Place? pressed = null;
            GUILayout.BeginHorizontal();
            foreach (var (place, name, tip) in Places)
            {
                bool here = place == now;
                if (place == Place.Battle && BattleRetired && !here)
                {
                    continue;
                }

                if (GUILayout.Button(new GUIContent(here ? "> " + name + " <" : name, here ? "Where you are now. " + tip : tip), here ? placeNowStyle : placeStyle) && !here)
                {
                    pressed = place;
                }
            }

            GUILayout.EndHorizontal();
            return pressed;
        }

        /// <summary>Where this panel is now.</summary>
        private Place PlaceNow => aimTest ? (duelLab ? Place.Duel : Place.Aim) : tubeTest ? Place.Tubes : targetTest ? Place.Targets : Place.Battle;

        private void GoTo(Place place)
        {
            switch (place)
            {
                case Place.Lab:
                    BackToLab();
                    break;
                case Place.Battle:
                    BattleLayout();
                    break;
                case Place.Targets:
                    TargetTest();
                    break;
                case Place.Tubes:
                    TubeTestScene();
                    break;
                case Place.Aim:
                    AimTestScene();
                    break;
                case Place.Duel:
                    DuelLabScene();
                    break;
            }
        }

        /// <summary>From the locomotion lab to the target test (the lab's 1), as <see cref="WatchFight"/> goes to the battle.</summary>
        public void WatchTargets()
        {
            aimTest = false;
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

            if (recordingStoppedForLab)
            {
                // (The recording that was stopped to go to the lab goes on, as a new log.)
                recordingStoppedForLab = false;
                if (!recorder.Recording)
                {
                    recorder.StartSession();
                }
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
            HitFlashes.Ensure();
        }

        private void OnDisable()
        {
            UnitController.Hit -= OnHit;
            SandboxInput.OverPanel = false;
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
            }
        }

        /// <summary>Heavy hits jolt the camera a little (the panel turns it off).</summary>
        private void OnHit(HitReport hit)
        {
            if (aimTest)
            {
                Aim.OnHit(hit, Time.time);
                if (duelLab)
                {
                    Dancing.OnHit(hit, Time.time);
                }
            }

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

            Note($"spawn: {units.Count} units on the {Grid.columns}x{Grid.rows} board{(aimTest ? $" (the aim test: a swordsman {aimDistance:0.00} m from a target with {AimKits[aimTargetKit]})" : targetTest ? $" (the target test: {(largeAttackers ? "Large" : "Medium")} attackers on {(largeTargets ? "Large" : "Medium")} targets)" : "")}");
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
            aimTest = false;
            fight = true;
            Spawn();
            message = "Target test: left to right, a target that does not block, one with sword and shield, one with a sword. R resets.";
        }

        // ---- The aim test: one swordsman, one target, a blow asked for one part at a time --------------------------------

        private bool aimTest;
        private int aimTargetKit;
        private float aimDistance = AttackMaker.On ? AimDistanceMade : BuiltBlow.On ? AimDistanceBuilt : AimDistanceAtFirst, aimTurned;
        private AimSpot aimSpot = AimSpot.Chest;
        private string aimLast = "";

        /// <summary>The duel lab's swordsman moves in the capoeira style (the plan's step 4): a sword alone, standing in the ginga.</summary>
        private bool duelCapoeira;

        /// <summary>The duel lab: what a press on a part of the body asks for. 0: a blow. 1: a feint. 2: a fake lunge. 3: both.</summary>
        private int duelPosture;

        /// <summary>The duel lab: where he carries his sword arm. 1: high. 0: as the recordings. -1: low.</summary>
        private int duelGuard;

        /// <summary>The capoeira style is shelved: the duel lab does not offer it (Combat Feel "The Capoeira Style Is Shelved").</summary>
        public static bool CapoeiraShelved = true;

        /// <summary>In the capoeira style he has nothing in his hands, as the recordings were made (the ginga is judged against them like for like); off: a sword alone.</summary>
        private bool duelEmptyHands = true;

        /// <summary>The aim test's blows: asked, followed and counted.</summary>
        public readonly AimTest Aim = new AimTest();

        /// <summary>How far the aim test's swordsman stands from its target at first (metres at a Medium's size, root to root): about where today's strikes land.</summary>
        public const float AimDistanceAtFirst = 1.4f;

        /// <summary>The same with built blows, while their feet do not move: about as near as the fight lets two enemies come, where the arm and the trunk reach a head and a body.</summary>
        public const float AimDistanceBuilt = 1.05f;

        /// <summary>The same with made attacks: where the recorded cuts land with their own lunge.</summary>
        public const float AimDistanceMade = 1.5f;

        private static readonly string[] AimKits = { "open hands", "sword and shield", "a sword" };

        // (The made attacks the panel can ask for by name, and the legs they can be put on: the strike clips' names.)
        private static readonly (string clip, string name)[] AimAttacks =
        {
            (null, "the best fit"), ("sns_forehand", "forehand cut"), ("sns_rising", "rising backhand"), ("sns_thrust", "thrust"),
            ("sns_chop_lunge", "leaping chop"), ("sns_combo_forehand", "hopping forehand"),
            // (Taken out on 15 November, for now: the spinning chop "sns_chop_side" and the spinning backhand
            // "sns_backhand_lunge". Made from their recordings they did not work: he turns right round to strike.)
        };

        private static readonly (string clip, string name)[] AimLegs =
        {
            (null, "the best fit"), ("own", "its own"), ("sns_forehand", "the forehand's lunge"), ("sns_rising", "the rising cut's passing step"), ("sns_thrust", "the thrust's standing legs"),
        };

        private int aimAttack, aimLegs;

        /// <summary>
        /// The aim test (the plan "Aimed attacks"): instead of the battle, one swordsman facing one target that only
        /// stands there (with nothing in its hands, with sword and shield, or with a sword; it takes no harm). Neither
        /// strikes by itself. The panel's body map asks the swordsman for one blow at one part of the target; the part
        /// the blade lands on turns red, and each blow is measured: what it met, how near the spot it passed, how true
        /// its edge was, whether its feet held it.
        /// </summary>
        public void AimTestScene()
        {
            targetTest = true;
            tubeTest = false;
            aimTest = true;
            duelLab = false;
            fight = true;
            Aim.Stop();
            Duel.Stop();
            Spawn();
            message = "Aim test: press a part of the body and the swordsman strikes at it. The part his blade lands on turns red. R resets.";
        }

        // ---- The duel lab (the plan "Fight Rhythm from Recordings") ----

        /// <summary>The duel lab is on: the aim test's two (a swordsman and one that stands there), the swordsman moved about by orders.</summary>
        private bool duelLab;

        /// <summary>The duel lab's movement orders.</summary>
        public readonly DuelLab Duel = new DuelLab();

        /// <summary>The duel lab's shield test (the plan's step 7): the target uncovers a part for a moment and the swordsman takes it.</summary>
        public readonly ShieldTest Shield = new ShieldTest();

        /// <summary>The duel lab's fakes test (the plan's step 7, second part): the target throws fake slashes in a pattern and the swordsman takes the opening the long one leaves.</summary>
        public readonly FakeTest Fakes = new FakeTest();

        /// <summary>How far apart the duel lab's two begin (metres).</summary>
        public const float DuelApartAtFirst = 3f;

        /// <summary>
        /// The duel lab (8): a swordsman and one that stands there, three metres apart, and the swordsman moved about it
        /// on demand. The scene is the aim test's; what is asked of the swordsman is movement.
        /// </summary>
        public void DuelLabScene()
        {
            shieldSetOut = false;
            SetOutDuelLab();
        }

        /// <summary>The duel lab set out for the shield test: the target with sword and shield, the two at striking distance from the first moment.</summary>
        private bool shieldSetOut;

        /// <summary>When the lab was last set out for the shield test, and what was asked for that has to wait until the two are standing ready (0: nothing; 1: an uncovering; 2: the uncovering's loop; 3: a short fake; 4: a long fake; 5: the fakes' pattern).</summary>
        private float shieldSetOutAt = -10f;
        private int shieldPending;

        /// <summary>The duel lab's target stands with sword and shield, as the shield test and the fakes test need.</summary>
        private bool TargetHasShield => duelLab && Aim.Target != null && Aim.Target.Clip != null && Aim.Target.Clip.ShieldColliders.Count > 0 && Aim.Target.Clip.Strikes != null;

        /// <summary>
        /// A press in the shield test or the fakes test with the target not holding sword and shield: the lab is set
        /// out for it first, and what was asked for is done as soon as the two stand ready. True: it has to wait.
        /// </summary>
        private bool SetOutFirst(int asked)
        {
            if (TargetHasShield)
            {
                return false;
            }

            ShieldTestScene();
            shieldPending = asked;
            message = "The target had no sword and shield: the lab is set out for the test, and what you pressed follows in a moment.";
            return true;
        }

        /// <summary>What was pressed before the lab was set out is done once the two stand ready.</summary>
        private void StepShieldPending()
        {
            if (shieldPending == 0 || Time.time - shieldSetOutAt < 1.5f || !TargetHasShield)
            {
                return;
            }

            if (Time.time - shieldSetOutAt > 8f)
            {
                shieldPending = 0;
                return;
            }

            switch (shieldPending)
            {
                case 1:
                    if (Shield.Uncover(Time.time)) shieldPending = 0;
                    break;
                case 2:
                    Shield.Loop = true;
                    shieldPending = 0;
                    break;
                case 3:
                case 4:
                    if (Fakes.Throw(shieldPending == 4, Time.time)) shieldPending = 0;
                    break;
                case 6:
                    StartDance();
                    shieldPending = 0;
                    break;
                default:
                    Fakes.Pattern = true;
                    Fakes.Restart();
                    shieldPending = 0;
                    break;
            }
        }

        /// <summary>The duel lab set out for its shield test (the plan's step 7).</summary>
        public void ShieldTestScene()
        {
            shieldSetOutAt = Time.time;
            shieldSetOut = true;
            aimTargetKit = 1;
            SetOutDuelLab();
            message = "Shield test: set out. Press Uncover: the target takes its shield off the part chosen, and the swordsman goes for it.";
        }

        private void SetOutDuelLab()
        {
            targetTest = true;
            tubeTest = false;
            aimTest = true;
            duelLab = true;
            fight = true;
            Aim.Stop();
            Duel.Stop();
            Spawn();
            message = "Duel lab: the buttons move the swordsman about the other: walk to it, back away, go round it. R resets.";
        }

        /// <summary>From the locomotion lab straight to the duel lab, as <see cref="WatchFight"/> goes to the battle.</summary>
        public void WatchDuel()
        {
            targetTest = true;
            tubeTest = false;
            aimTest = true;
            duelLab = true;
            fight = true;
            Aim.Stop();
            Duel.Stop();
            WatchFight();
            message = "Duel lab: the buttons move the swordsman about the other: walk to it, back away, go round it. R resets.";
        }

        private void DrawDuelLab()
        {
            if (!Section("Duel lab", true, "One fighter moved about another on demand, so a way of moving can be looked at by itself. He is steered as the fight steers a fighter, so what is seen is the walk the fight will have."))
            {
                return;
            }

            var coverFeel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
            if (coverFeel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Shield:", "How a fighter with a shield carries it (Combat Feel \"The Shield Is Held In Cover\"). Held in cover (new): the board is held before his chest, looking at the enemy, whatever he is doing: walking, a gesture of the sword arm, a blow of his own. It goes with the body and gives a little, but stays in place, and leaves only to meet a blow. The recording's (old): the shield arm does what each recording has it do: it swung open in every cut and every fake of one."), Styles().Text, GUILayout.Width(80f));
                if (Press(coverFeel.theShieldIsHeldInCover ? "[held in cover]" : "held in cover", "The board is held before his chest.")) coverFeel.theShieldIsHeldInCover = true;
                if (Press(!coverFeel.theShieldIsHeldInCover ? "[the recording's (old)]" : "the recording's (old)", "The shield arm does what each recording has it do.")) coverFeel.theShieldIsHeldInCover = false;
                GUILayout.EndHorizontal();
                if (coverFeel.theShieldIsHeldInCover)
                {
                    coverFeel.shieldCoverHold = Lever("Cover holds", coverFeel.shieldCoverHold, 0f, 1f, "How much of the shield arm is the cover's while he stands, walks and gestures (1: quite still; less: more of the recording's own arm).");
                    coverFeel.shieldCoverInBlow = Lever("In a blow", coverFeel.shieldCoverInBlow, 0f, 1f, "How much of the shield arm is the cover's in a real blow of his own (0: it swings as the recording has it).");
                    var at = coverFeel.shieldCoverAt;
                    at.x = Lever("Cover: across", at.x, -25f, 15f, "Where the board's middle is held from the chest, to his right (below 0: toward the shield's side), cm.");
                    at.y = Lever("Cover: up", at.y, -30f, 15f, "Where the board's middle is held from the chest, up (below 0: lower), cm.");
                    at.z = Lever("Cover: ahead", at.z, 20f, 45f, "How far ahead of the chest the board's middle is held, cm.");
                    coverFeel.shieldCoverAt = at;
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Actions:", "Where the actions are: the buttons that have a fighter do something (move, leap, posture, bash, strike, the tests, the dance), the pace, and each action's own settings. In a window (new): a window of their own at the right, the pace at its top, each action's settings folded under its buttons. In the panel (old): all of it here."), Styles().Text, GUILayout.Width(80f));
            if (Press(ActionsApart ? "[in a window]" : "in a window", "The buttons in a window of their own.")) ActionsApart = true;
            if (Press(!ActionsApart ? "[in the panel]" : "in the panel", "The buttons among the settings here, as they were.")) ActionsApart = false;
            GUILayout.EndHorizontal();
            var mover = Duel.Mover;
            var style = mover != null && mover.Clip != null && mover.Clip.Loco != null ? mover.Clip.Loco.Style : null;
            GUILayout.Label(new GUIContent("Style: " + (style != null ? style.Name : "none") + (style != null ? $" (the pack \"{style.Prefix}\")" : ""),
                "The movement style the swordsman's guard moves in: a pack of recordings, each with a job in the walk (standing, walking forward and back, a step to each side, running, turning). The standard style is the sword and shield pack's guard, as the fight has moved until now."), Styles().Text);
            GUILayout.BeginHorizontal();
            if (Press(!duelCapoeira ? "[Standard]" : "Standard", "The standard style: sword and shield, standing in his guard. Sets the lab out again."))
            {
                if (duelCapoeira)
                {
                    duelCapoeira = false;
                    DuelLabScene();
                }
            }

            if (CapoeiraShelved && duelCapoeira)
            {
                duelCapoeira = false;
                DuelLabScene();
            }

            if (!CapoeiraShelved && Press(duelCapoeira ? "[Capoeira]" : "Capoeira", "The capoeira style: a sword alone, and he stands in the ginga: never still, one foot stepping back and then the other, the hips swaying, the arms and the sword going with it. To go anywhere he walks and jogs as the standard style does, and goes back into the ginga where he stops. Sets the lab out again."))
            {
                if (!duelCapoeira)
                {
                    duelCapoeira = true;
                    DuelLabScene();
                }
            }

            GUILayout.EndHorizontal();
            if (duelCapoeira)
            {
                var gingaFeel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Hands:", "Empty: he has nothing in his hands, as the capoeira recordings were made, so the ginga can be set beside them like for like. Sword: a sword alone (how the sword is carried in the ginga is the next step, not done yet). Sets the lab out again."), Styles().Text, GUILayout.Width(60f));
                if (Press(duelEmptyHands ? "[empty]" : "empty", "Nothing in his hands, as the recordings.") && !duelEmptyHands)
                {
                    duelEmptyHands = true;
                    DuelLabScene();
                }

                if (Press(!duelEmptyHands ? "[sword]" : "sword", "A sword alone.") && duelEmptyHands)
                {
                    duelEmptyHands = false;
                    DuelLabScene();
                }

                GUILayout.EndHorizontal();
                if (gingaFeel != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Ginga:", "Made (new): the ginga made of the recorded takes' own motion, as an attack is made of a recorded strike: every part of the body moves as the recording has it; it is made smaller, he is held at its rests, he goes from one take to another, and his feet are held where they are put down. Looped (old): the first pass, one take played round and round."), Styles().Text, GUILayout.Width(60f));
                    if (Press(gingaFeel.gingaIsMadeOfItsTakes ? "[made]" : "made", "The ginga made of its takes' motion (Combat Feel \"Ginga Is Made Of Its Takes\").")) gingaFeel.gingaIsMadeOfItsTakes = true;
                    if (Press(!gingaFeel.gingaIsMadeOfItsTakes ? "[looped]" : "looped", "One recorded take, looped (the first pass).")) gingaFeel.gingaIsMadeOfItsTakes = false;
                    GUILayout.EndHorizontal();
                }

                var madeOf = Duel.Mover != null && Duel.Mover.Clip != null ? Duel.Mover.Clip.Loco : null;
                if (gingaFeel != null && gingaFeel.gingaIsMadeOfItsTakes)
                {
                    gingaFeel.gingaRestlessness = Lever("Restless", gingaFeel.gingaRestlessness, 0.05f, 1f, "How long he stays at each rest. 1: not at all, as the recordings, which never stop. 0.4: a second or so. Near 0: many seconds.");
                    gingaFeel.gingaMadeSize = Lever("Size", gingaFeel.gingaMadeSize, 0.4f, 1f, "How big the ginga is against the recordings: the feet's places and the hips' travel. 1: as recorded.");
                    gingaFeel.gingaTempo = Lever("Tempo", gingaFeel.gingaTempo, 0.5f, 1.6f, "How fast it is played against the recordings. 1: as recorded.");
                    gingaFeel.gingaVariation = Lever("Varies", gingaFeel.gingaVariation, 0f, 1f, "How much it varies: how often he goes over to another of the four takes, how unevenly long his rests are, how far off their places his feet land. 0: one take, every time alike.");
                    GUILayout.BeginHorizontal();
                    if (Press("Go on now", "He goes on from his rest at once. Turn Restless right down and press this to look at one change of sides at a time."))
                    {
                        PhysicsBattler.Locomotion.StandPattern.ChangesAsked++;
                    }

                    if (Press("As recorded", "Sets the four levers so that he does the recordings themselves: full size, never resting, one take. This is what the checks set beside the recordings; look at it beside the clips."))
                    {
                        gingaFeel.gingaRestlessness = 1f;
                        gingaFeel.gingaMadeSize = 1f;
                        gingaFeel.gingaTempo = 1f;
                        gingaFeel.gingaVariation = 0f;
                    }

                    if (Press("As set", "Sets the four levers back to what they are set to at first (restless 0.4, size 0.8, tempo 1, varies 0.5)."))
                    {
                        gingaFeel.gingaRestlessness = 0.4f;
                        gingaFeel.gingaMadeSize = 0.8f;
                        gingaFeel.gingaTempo = 1f;
                        gingaFeel.gingaVariation = 0.5f;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Label(new GUIContent(madeOf == null || madeOf.Guard == null || madeOf.Guard.Pattern == null
                        ? "The made ginga is not on him (the lab was set out before the switch): press Capoeira again."
                        : string.Format(System.Globalization.CultureInfo.InvariantCulture, "In the take {0} at {1:0.00} s{2}; gone on from a rest {3} times.", madeOf.Guard.Pattern.TakeName, madeOf.Guard.Pattern.TakeTime, madeOf.Guard.Pattern.Resting ? ", resting" : "", madeOf.Guard.Pattern.Changes),
                        "Which recorded take he is in and the moment of it: the same moment of that recording is what he should look like, made smaller."), Styles().Text);
                    gingaFeel.capoeiraStyleMotion = Lever("Livelier", gingaFeel.capoeiraStyleMotion, 0f, 1f, "On the move (not in the ginga): how much further his hips, chest and head turn and sway than the standard's.");
                    gingaFeel.capoeiraStyleSquaresUp = Lever("Squares up", gingaFeel.capoeiraStyleSquaresUp, 0f, 1f, "On the move: how much his chest and head turn square to the other, as capoeira's do.");
                    gingaFeel.capoeiraStyleEyesUp = Lever("Eyes up", gingaFeel.capoeiraStyleEyesUp, 0f, 1f, "On the move: how much his eyes stay on his man. 1: his head faces as the standard's does, however far he leans. 0: tipped down with the lean.");
                }

                if (gingaFeel != null && !gingaFeel.gingaIsMadeOfItsTakes && Press("Ginga: " + PhysicsBattler.Locomotion.MoveStyle.GingaTakes[PhysicsBattler.Locomotion.MoveStyle.GingaTake].about, "Which recording of the ginga he stands in. Press for the next of the four."))
                {
                    PhysicsBattler.Locomotion.MoveStyle.GingaTake = (PhysicsBattler.Locomotion.MoveStyle.GingaTake + 1) % PhysicsBattler.Locomotion.MoveStyle.GingaTakes.Length;
                }

                if (gingaFeel != null && !gingaFeel.gingaIsMadeOfItsTakes)
                {
                    gingaFeel.gingaSize = Lever("Ginga size", gingaFeel.gingaSize, 0.3f, 1.2f, "How big the ginga is made against its recording: its steps, the hips' sway and sinking, the body's turns. 1: as recorded. 0.67: two thirds.");
                    gingaFeel.gingaArms = Lever("Ginga arms", gingaFeel.gingaArms, 0f, 1.5f, "How much of the arms' swings is kept, on top of the size. 0: the arms held where they are in the middle of the ginga.");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Sword:", "How the blade is held as the sword hand goes with the ginga. By the wrist (new): carried on the forearm with the wrist straight, turned part of the way to where he faces, its point trailing the hand, kept out of his own body. As recorded (old): laid along the recorded empty hand's fingers, which sweeps it through his own head as the arm crosses his face."), Styles().Text, GUILayout.Width(60f));
                    if (Press(gingaFeel.gingaSwordFollowsTheArm ? "[by the wrist]" : "by the wrist", "The blade carried on the forearm, pointed part of the way ahead, trailing the hand, out of his body (Combat Feel \"Ginga Sword Follows The Arm\").")) gingaFeel.gingaSwordFollowsTheArm = true;
                    if (Press(!gingaFeel.gingaSwordFollowsTheArm ? "[as recorded]" : "as recorded", "The blade along the recorded hand's fingers.")) gingaFeel.gingaSwordFollowsTheArm = false;
                    GUILayout.EndHorizontal();
                    if (gingaFeel.gingaSwordFollowsTheArm)
                    {
                        gingaFeel.gingaSwordPointsAhead = Lever("Points ahead", gingaFeel.gingaSwordPointsAhead, 0f, 1f, "How much the blade is turned from the forearm's line toward where he faces. 0: it swings as far as the arm does. 1: it always points ahead.");
                        gingaFeel.gingaSwordTrails = Lever("Trails", gingaFeel.gingaSwordTrails, 0.02f, 0.4f, "How long the blade's point takes to come round after the hand (seconds). More reads as a heavier blade, swung looser.");
                    }
                }
            }

            var moveFeel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
            if (moveFeel != null)
            {
                Heading("How he moves", "His standing, walking, jogging and stepping are made of their parts, as his attacks are: each recording is taken apart into what every part of the body does against the part it sits on, and he is put together again from those. The levers change one part's doing each; with all of them at rest he moves as the recordings.");
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Movement:", "Made (new): put together from its parts at every moment. Recorded (old): the recordings' points played as they are. With the levers at rest the two should look the same; the levers only act on the made one."), Styles().Text, GUILayout.Width(80f));
                if (Press(moveFeel.standardMovementIsMadeOfItsParts ? "[made]" : "made", "Made of its parts (Combat Feel \"Standard Movement Is Made Of Its Parts\").")) moveFeel.standardMovementIsMadeOfItsParts = true;
                if (Press(!moveFeel.standardMovementIsMadeOfItsParts ? "[recorded]" : "recorded", "The recordings played as they are.")) moveFeel.standardMovementIsMadeOfItsParts = false;
                GUILayout.EndHorizontal();
                if (moveFeel.standardMovementIsMadeOfItsParts)
                {
                    moveFeel.movementBounce = Lever("Bounce", moveFeel.movementBounce, 0f, 2.5f, "How much his hips rise and fall with each step. 1: as recorded. 0: level.");
                    moveFeel.movementSway = Lever("Sway", moveFeel.movementSway, 0f, 2.5f, "How much his hips sway from side to side and forward and back over his feet. 1: as recorded.");
                    moveFeel.movementHipTurn = Lever("Hip turn", moveFeel.movementHipTurn, 0f, 2.5f, "How much his hips turn with each step. 1: as recorded.");
                    moveFeel.movementChestTurn = Lever("Chest turn", moveFeel.movementChestTurn, 0f, 2.5f, "How much his chest turns against his hips with each step. 1: as recorded.");
                    moveFeel.movementArmSwing = Lever("Arm swing", moveFeel.movementArmSwing, 0f, 2.5f, "How much his arms swing. 1: as recorded. Shows only on an arm that is the walk's own: with the other fighter near, his sword and shield hands are placed by his guard.");
                    moveFeel.movementStepLift = Lever("Step lift", moveFeel.movementStepLift, 0.5f, 2.5f, "How high his feet are lifted in their swing. 1: as recorded.");
                    moveFeel.movementLean = Lever("Lean", moveFeel.movementLean, -10f, 25f, "How far he leans forward from the hips, added to the recording's own lean (degrees). His head stays up.");
                    moveFeel.movementCrouch = Lever("Crouch", moveFeel.movementCrouch, 0f, 15f, "How much lower he carries his hips, his feet where they were (cm).");
                    GUILayout.BeginHorizontal();
                    if (Press("At rest", "Every lever back to rest: he moves as the recordings."))
                    {
                        moveFeel.movementBounce = moveFeel.movementSway = moveFeel.movementHipTurn = moveFeel.movementChestTurn = moveFeel.movementArmSwing = moveFeel.movementStepLift = 1f;
                        moveFeel.movementLean = moveFeel.movementCrouch = 0f;
                    }

                    GUILayout.Label(new GUIContent("Tries of the levers (Prowler, Strutter, Bull) are a fighter's own now: \"Each his own\" below.", "The Prowl and Strut tries that were here changed everyone's movement, and with \"Keep what I tune\" on they were saved into the scene as everyone's. They are given to one fighter at a time under \"Each his own\", which is not saved."), Styles().Text);
                    GUILayout.EndHorizontal();
                }
            }

            if (moveFeel != null)
            {
                DrawOwnLevers(moveFeel);
            }

            Heading("What he leaves open", "Each part of his body as the other sees it from its sword shoulder: open (a straight line reaches it), or covered, and by what (his shield, or his own arm before it). Looked at on the bodies as they are, ten times a second, so whatever he does shows in it. Under it: what the last posture opened that was covered, and covered that was open, and when.");
            OpenRow(PhysicsBattler.Locomotion.AimSpot.Head);
            OpenRow(PhysicsBattler.Locomotion.AimSpot.ShoulderR, PhysicsBattler.Locomotion.AimSpot.Chest, PhysicsBattler.Locomotion.AimSpot.ShoulderL);
            OpenRow(PhysicsBattler.Locomotion.AimSpot.UpperArmR, PhysicsBattler.Locomotion.AimSpot.Belly, PhysicsBattler.Locomotion.AimSpot.UpperArmL);
            OpenRow(PhysicsBattler.Locomotion.AimSpot.ForearmR, PhysicsBattler.Locomotion.AimSpot.Hips, PhysicsBattler.Locomotion.AimSpot.ForearmL);
            OpenRow(PhysicsBattler.Locomotion.AimSpot.ThighR, PhysicsBattler.Locomotion.AimSpot.ThighL);
            OpenRow(PhysicsBattler.Locomotion.AimSpot.ShinR, PhysicsBattler.Locomotion.AimSpot.ShinL);
            Readout("Last posture: " + Duel.Opened, 3);
            if (ActionsApart)
            {
                Heading("His actions", "Everything that has one of the two do something (move, leap, posture, bash, strike, the tests, the dance) is in the Actions window: the buttons, and under each action's buttons its own settings and what was measured of it, folded shut until its \"settings\" bar is pressed.");
                GUILayout.Label(new GUIContent("The buttons, and each action's settings under its buttons, are in the Actions window.", "\"Actions: in the panel\" above puts them all back here."), Styles().Text);
                return;
            }

            ActionGroups(false);
        }

        /// <summary>Whose own levers the panel shows (0: his, the one moved; 1: the other's).</summary>
        private int ownShown;
        private readonly PhysicsBattler.Locomotion.FighterLevers[] ownKept = new PhysicsBattler.Locomotion.FighterLevers[2];

        /// <summary>
        /// "Each his own" (the dance's first part): the two fighters' own levers (<see cref="PhysicsBattler.Locomotion.FighterLevers"/>),
        /// laid on everyone's. A made-up fighter for each at a press, and every lever of the one shown.
        /// </summary>
        private void DrawOwnLevers(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            Heading("Each his own", "Each fighter has levers of his own, laid on everyone's (\"How he moves\" above, and the postures' sizes in Combat Feel): how he moves and how big his gestures are. Give the two different ones and they are told apart by how each carries himself. The one you move shows all of it as he walks, circles and gestures; the other shows his lean and crouch as he stands, and his feints in the fakes test.");
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Levers:", "Each his own (new): a fighter's own levers are laid on everyone's. Everyone's (old): every fighter moves by the one shared set, as it was (Combat Feel \"Each Fighter Has His Own Levers\")."), Styles().Text, GUILayout.Width(80f));
            if (Press(feel.eachFighterHasHisOwnLevers ? "[each his own]" : "each his own", "A fighter's own levers are laid on everyone's.")) feel.eachFighterHasHisOwnLevers = true;
            if (Press(!feel.eachFighterHasHisOwnLevers ? "[everyone's]" : "everyone's", "Every fighter moves by everyone's levers alone, as it was.")) feel.eachFighterHasHisOwnLevers = false;
            GUILayout.EndHorizontal();
            if (!feel.eachFighterHasHisOwnLevers || !Duel.Running)
            {
                return;
            }

            var both = new[] { Duel.Mover, Duel.Other };
            string[] whose = { "Him:", "The other:" };
            for (int k = 0; k < 2; k++)
            {
                var loco = both[k].Clip != null ? both[k].Clip.Loco : null;
                if (loco == null)
                {
                    continue;
                }

                // (His own are kept over a new setting out of the lab: new fighters are given the ones the last had.)
                if (loco.Own == null && ownKept[k] != null)
                {
                    loco.Own = ownKept[k];
                }

                ownKept[k] = loco.Own;
                string now = loco.Own != null ? loco.Own.Name : "Plain";
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(whose[k], k == 0 ? "The fighter you move." : "The one he moves about."), Styles().Text, GUILayout.Width(80f));
                foreach (string name in PhysicsBattler.Locomotion.FighterLevers.Names)
                {
                    string tip = name == "Plain" ? "As everyone: none of his own."
                        : name == "Prowler" ? "Low and leaning in, level, hips swaying and turning, feet kept near the ground; small quick gestures, a deep drop."
                        : name == "Strutter" ? "Upright and bouncing, chest turning and arms swinging wide, feet lifted high; big showy gestures."
                        : "Heavy and square: leaning in a little, hips and chest turning little, feet low; few frills, a long fake lunge.";
                    if (Press(now == name ? $"[{name}]" : name, tip))
                    {
                        loco.Own = PhysicsBattler.Locomotion.FighterLevers.Named(name);
                        ownShown = k;
                    }
                }

                if (Press(ownShown == k ? "[levers]" : "levers", "Show this one's own levers below, to set by hand."))
                {
                    ownShown = k;
                }

                GUILayout.EndHorizontal();
            }

            var shown = both[ownShown].Clip != null ? both[ownShown].Clip.Loco : null;
            if (shown == null)
            {
                return;
            }

            var own = shown.Own ?? (shown.Own = PhysicsBattler.Locomotion.FighterLevers.Plain());
            GUILayout.Label(new GUIContent($"{(ownShown == 0 ? "His" : "The other's")} own ({own.Name}), each times everyone's:", "1: as everyone. Lean and crouch are added to everyone's."), Styles().Text);
            float was = own.Bounce + own.Sway + own.HipTurn + own.ChestTurn + own.ArmSwing + own.StepLift + own.Lean + own.Crouch + own.Feint + own.Lunge + own.Drop + own.Bluff + own.Guard + own.Restless + own.Aggressive + own.Leaps + own.Heavy + own.Dodges;
            own.Bounce = Lever("Bounce", own.Bounce, 0f, 2.5f, "His hips' rise and fall with each step, times everyone's.");
            own.Sway = Lever("Sway", own.Sway, 0f, 2.5f, "His hips' sway over his feet, times everyone's.");
            own.HipTurn = Lever("Hip turn", own.HipTurn, 0f, 2.5f, "His hips' turning with each step, times everyone's.");
            own.ChestTurn = Lever("Chest turn", own.ChestTurn, 0f, 2.5f, "His chest's turning against his hips, times everyone's.");
            own.ArmSwing = Lever("Arm swing", own.ArmSwing, 0f, 2.5f, "His arms' swing, times everyone's. Shows only on an arm that is the walk's own.");
            own.StepLift = Lever("Step lift", own.StepLift, 0.5f, 2.5f, "How high his feet are lifted, times everyone's.");
            own.Lean = Lever("Lean", own.Lean, -10f, 25f, "His lean forward from the hips, added to everyone's (degrees).");
            own.Crouch = Lever("Crouch", own.Crouch, 0f, 15f, "How much lower he carries his hips, added to everyone's (cm).");
            own.Feint = Lever("Feints", own.Feint, 0.3f, 1.2f, "How deep his feints go into the wind-up, times everyone's (never past the whole wind-up).");
            own.Lunge = Lever("Fake lunges", own.Lunge, 0.3f, 2f, "How far his fake lunges carry him, times everyone's.");
            own.Drop = Lever("Drops", own.Drop, 0.3f, 2f, "How far he sinks in a drop, times everyone's.");
            own.Bluff = Lever("Bluffs", own.Bluff, 0.3f, 2f, "How big his blade's bluffs and his shield's press are, times everyone's.");
            own.Guard = Lever("Guard changes", own.Guard, 0.3f, 2f, "How far a high or low guard carries his sword arm, times everyone's. Set the guard again to see it.");
            own.Restless = Lever("Restless", own.Restless, 0f, 1f, "In the dance: how much he is on the move. 1: he goes round the other in long spells with short stands between. 0: he stands long and moves little.");
            own.Aggressive = Lever("Aggressive", own.Aggressive, 0f, 1f, "In the dance: the distance he likes (1: inside reach, 0: just outside it), how soon he is back in after a blow, and whether he stands and covers for a blow he expects (0.6 and more) or steps out of reach of it.");
            own.Leaps = Lever("Leaps", own.Leaps, 0f, 1f, "In the dance: how readily he leaps where a step would do (0: never, 1: every time). In, to close the last of the gap for his blow when he is still out of reach as it is due; and out, away from a blow he expects.");
            own.Heavy = Lever("Heavy blows", own.Heavy, 0f, 1f, "In the dance: how many of his own blows (after his tell) are heavy ones: slow and powerful, with a long wind-up. The rest are light: fast and less powerful. 0: all light; 1: all heavy.");
            own.Dodges = Lever("Dodges", own.Dodges, 0f, 1f, "In the dance: how readily he ducks aside under a high blow he sees coming, where another would trust his shield (0: never; 1: every time).");
            if (was != own.Bounce + own.Sway + own.HipTurn + own.ChestTurn + own.ArmSwing + own.StepLift + own.Lean + own.Crouch + own.Feint + own.Lunge + own.Drop + own.Bluff + own.Guard + own.Restless + own.Aggressive + own.Leaps + own.Heavy + own.Dodges && own.Name != "By hand")
            {
                own.Name = own.AtRest ? "Plain" : "By hand";
            }
        }

        /// <summary>The dance (the plan's step 8, second part): the two gesturing at each other, each reading the other.</summary>
        public readonly Dance Dancing = new Dance();
        private bool dancePatterned;

        /// <summary>The dance is begun: the stand-and-strike tests are stopped, and each fighter given the pattern that goes with his own levers unless one was chosen.</summary>
        private void StartDance()
        {
            Shield.Loop = false;
            Fakes.Pattern = false;
            Aim.Stop();
            Duel.Give(DuelLab.Order.Stand);
            if (!dancePatterned)
            {
                foreach (var d in new[] { Dancing.A, Dancing.B })
                {
                    var own = d.Unit != null && d.Unit.Clip != null ? d.Unit.Clip.Loco.Own : null;
                    bool mixed = d.Pattern.MixedUp;
                    d.Pattern = GesturePattern.Named(own != null ? own.Name : "Plain");
                    d.Pattern.MixedUp = mixed;
                }

                dancePatterned = true;
            }

            string cannot = Dancing.Cannot;
            Dancing.On = cannot.Length == 0;
            Duel.EndCourse();
            danceMeters[0].Reset();
            danceMeters[1].Reset();
            message = Dancing.On ? "The dance: both gesture in their own patterns and strike after their tells; each watches the other." : "The dance: not now: " + cannot + ".";
        }

        /// <summary>The dance's buttons.</summary>
        private void DanceButtons()
        {
            GUILayout.BeginHorizontal();
            if (Press(Dancing.On ? "[Dance]" : "Dance", "The two begin: each throws his gestures on his own beat, then his tell, then his real blow, round and round, and each watches the other. Pressed again, they stop. (With no sword and shield on the target, the lab is set out for it first.)"))
            {
                if (Dancing.On)
                {
                    Dancing.On = false;
                    message = "The dance: stopped.";
                }
                else if (!SetOutFirst(6))
                {
                    StartDance();
                }
            }

            if (Press("Forget", "Both forget what they have learned of each other's patterns: the next gestures are new to them."))
            {
                Dancing.Forget();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// The settings of an action. In the actions window they are folded under the action's buttons, shut at first,
        /// opened and shut by a press of their bar (kept between sessions); in the panel they are always shown.
        /// </summary>
        private bool ActionSettings(bool inWindow, string name, string what)
        {
            if (!inWindow)
            {
                return true;
            }

            string key = "Actions." + name;
            if (!sectionOpen.TryGetValue(key, out bool open))
            {
                open = PlayerPrefs.GetFloat("BattleSandbox.Section." + key, 0f) > 0.5f;
                sectionOpen[key] = open;
            }

            if (GUILayout.Button(new GUIContent((open ? "−  " : "+  ") + what, "This action's settings, and what was measured of it. Press to fold them open or shut."), Styles().Fold))
            {
                // (From the next time the window is laid out, as a section of the panel.)
                sectionOpen[key] = !open;
                PlayerPrefs.SetFloat("BattleSandbox.Section." + key, open ? 0f : 1f);
            }

            return open;
        }

        /// <summary>What the tests go by is kept as Combat Feel has it, whether their settings are shown or folded shut.</summary>
        private void SyncTests(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            Shield.Spot = AimSpot.Chest;
            if (feel == null)
            {
                return;
            }

            Shield.Taker.Reads = feel.theSwordsmanReadsOpenings;
            Shield.Taker.Reaction = feel.openingReactionTime;
            Fakes.Taker.Reads = feel.theSwordsmanReadsOpenings;
            Fakes.Taker.Reaction = feel.openingReactionTime;
            Dancing.A.Taker.Reaction = feel.openingReactionTime;
            Dancing.B.Taker.Reaction = feel.openingReactionTime;
        }

        /// <summary>
        /// Every action of the duel lab, each with its buttons and under them its own settings and what was measured of
        /// it: in the actions window (the settings folded) or in the panel (shown).
        /// </summary>
        private void ActionGroups(bool inWindow)
        {
            var feel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
            SyncTests(feel);
            Heading("Move him", "Each button is an order he carries out until it is done or another is given.");
            if (!inWindow)
            {
                DuelPace();
            }

            DuelMoveButtons();
            FootworkButtons(feel);
            TurnButtons(feel);
            if (ActionSettings(inWindow, "Move", "Moving: settings"))
            {
                PaceSettings(feel);
            }

            Heading("Leaps", "From a stand he springs toward the other, away from it or to a side: both feet leave the ground together and come down together.");
            DuelLeapButtons();
            if (ActionSettings(inWindow, "Leaps", "Leaps: settings"))
            {
                LeapSettings(feel);
            }

            Heading("Postures", "The short bursts a fighter throws between blows, over whatever he is doing. A drop: a sudden dip of the hips, as if loading to spring, and back up. A dodge: he ducks and leans aside. The blade's bluffs: its point dips, flicks aside, or goes once round. The guard: where he carries his sword arm until it is changed again.");
            DuelDropButton();
            DuelBluffButtons();
            if (ActionSettings(inWindow, "Postures", "Postures: settings"))
            {
                PostureSettings(feel);
            }

            Heading("Shield bash", "He drives his board straight ahead into the other, his body behind it. Three bashes, each with its own settings. Small: from where he stands; he sinks behind the board and goes in a little with it. Medium: he leaps in behind his board and stops short of the other. Hard: he crouches deep and leaps fast all the way into the other's space, and goes on through as the other is thrown out of it. What a bash does is not set: the speed the board arrives with, times the bash's weight, is the shove; under 1.5 m/s the other is shoved back a step, from there thrown back stumbling, from 4.5 m/s knocked down.");
            BashButtons();
            var basher = Duel.Mover != null ? Duel.Mover.Clip : null;
            Readout(new GUIContent("Last bash: " + (basher != null && basher.LastBash.Length > 0 ? basher.LastBash + "." : "none yet."), "How the last bash was thrown (the leap, the drive) and what it did: the speed the board closed on the other at, the shove, and whether he was shoved back, thrown back stumbling or knocked down."), 4);
            string[] bashes = { "Small", "Medium", "Hard" };
            for (int kind = 0; kind < bashes.Length; kind++)
            {
                if (ActionSettings(inWindow, "Bash." + bashes[kind], bashes[kind] + " bash: settings"))
                {
                    BashSliders(feel, kind);
                }
            }

            if (ActionSettings(inWindow, "Bash.Does", "What a bash does, and bashing by himself: settings"))
            {
                BashDoes(feel);
            }

            Heading("Strike at", "Press a part while he is moving (give him an order above first): he strikes at it from where he is, in the middle of his step, and then goes on with his order. Pressed while he stands it is the aim test's blow.");
            DuelPressIs();
            DuelBodyMap();
            if (ActionSettings(inWindow, "Strike", "Strikes: settings"))
            {
                StrikeSettings(feel);
            }

            Heading("Shield test", "The target holds a shield and takes it off one part of itself for a moment; the swordsman must hit that part while it is open. A blow takes time to land, so seeing the part open is too late: a swordsman who reads the uncovering starts as it begins (he goes by its tell, the dip of its blade, and by when it opened the last time he saw it) and lands while it is open; one who only reacts starts once he sees it open, and meets the shield coming back. Under the buttons: the window as it was measured, when his blow started and when it landed.");
            ShieldTestButtons();
            if (ActionSettings(inWindow, "ShieldTest", "Shield test: settings"))
            {
                ShieldTestSettings(feel);
            }

            Heading("Fakes test", "The shield test on real gestures. The target throws fake slashes at the swordsman: short ones, which leave its chest covered, and after so many of them a long one, which leaves its chest open for nearly half a second. They come on a steady beat. The swordsman set to read (above) watches the pattern through once without striking; after that he lets the short ones go, counts them, and starts his blow before the long one begins, to land as it opens. Set to only react he waits until he sees the chest open, and arrives after it is covered again. The target needs its sword and shield: if it has none, the first press of a button here sets the lab out for it, and what was pressed follows in a moment.");
            FakeTestButtons();
            if (ActionSettings(inWindow, "FakesTest", "Fakes test: settings"))
            {
                FakeTestSettings(feel);
            }

            Heading("The dance", "The shield test and the fakes test had one who gestured and one who struck; here both do both. Each fighter gestures in a pattern of his own: so many gestures on a beat, then his tell, then his real blow. Each watches the other. A fake of his own commits a fighter's shield, so a blow that comes while he is in one lands; out of one his shield meets it. One who reads learns the other's tell and count by watching, stops gesturing when the tell comes, and strikes into a gesture he knows leaves the chest open. One who does not read gestures on regardless and goes for a chest he sees open.");
            DanceButtons();
            if (ActionSettings(inWindow, "Dance", "The dance: settings"))
            {
                DanceSettings(feel);
            }
        }

        /// <summary>
        /// The footwork course (the plan "Convincing walking in the dance", step 1): the same footwork on demand, old
        /// walk against new, and what the meter made of it.
        /// </summary>
        private void FootworkButtons(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            GUILayout.BeginHorizontal();
            if (Press(Duel.CourseOn ? "[Footwork course]" : "Footwork course", $"He is taken through the same footwork every time, {DuelLab.CourseSeconds:0} seconds of it, at the dance's pace ({DuelLab.CoursePace:0.0} m/s) between {DuelLab.CourseNear:0.0} and {DuelLab.CourseFar:0.0} m: he retreats, advances, circles left, circles right, goes in and out twice, and stops and goes four times. His footwork is measured from its first step. Pressed again while it runs, it stops. Run it with the walk old and new and set the two readings side by side."))
            {
                if (Duel.CourseOn)
                {
                    Duel.EndCourse();
                    message = "Footwork course: stopped.";
                }
                else
                {
                    Shield.Loop = false;
                    Fakes.Pattern = false;
                    Dancing.On = false;
                    message = Duel.BeginCourse() ? "Footwork course: begun." : "Footwork course: not now (the lab is not set out).";
                }
            }

            GUILayout.EndHorizontal();
            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("His walk:", "How the guard's four steps are carried (Combat Feel \"The Guard Walks In One Carriage\"). One carriage (new): he steps forward and back on an archer's legs, which keep his turned stance, under his own chest and arms; and each of his four steps is carried as his stand is: his hips at the stand's height, his feet's places toward the stand's. As recorded (old): the sword and shield pack's own walk forward and back, which is 9 cm taller than his stand, narrow and square, and each step as its recording has it."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.theGuardWalksInOneCarriage ? "[one carriage]" : "one carriage", "Every step carried as his stand is.")) feel.theGuardWalksInOneCarriage = true;
                if (Press(!feel.theGuardWalksInOneCarriage ? "[as recorded (old)]" : "as recorded (old)", "Each step as its recording has it, as it was.")) feel.theGuardWalksInOneCarriage = false;
                GUILayout.EndHorizontal();
            }

            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("A leg left behind:", "What happens when his body goes away from a planted foot, as at the first step out of his wide stand (Combat Feel \"A Leg Left Behind Steps Before The Hips Sink\"). Steps first (new): the foot is let go to step after him as soon as its leg is all but straight. Hips sink first (old): his hips sank up to 10 cm before the foot was let go."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aLegLeftBehindStepsBeforeTheHipsSink ? "[steps first]" : "steps first", "The foot steps after him before his hips sink.")) feel.aLegLeftBehindStepsBeforeTheHipsSink = true;
                if (Press(!feel.aLegLeftBehindStepsBeforeTheHipsSink ? "[hips sink first (old)]" : "hips sink first (old)", "His hips sink all they can first, as it was.")) feel.aLegLeftBehindStepsBeforeTheHipsSink = false;
                GUILayout.EndHorizontal();
            }

            Readout(new GUIContent("Course: " + Duel.CourseText + ". " + (Duel.Meter.Seconds > 0.5f ? "His footwork " + FootworkText(Duel.Meter, 0) + "." : ""), "What the meter made of the course, so far or when it ended. The band his hips' height moved over while he stood or walked (the middle 90% of the time): a level walk is a few centimetres. How often his hips rose or sank 10 cm or more within 0.3 s with no gesture or blow playing: the jump into another pose. How many steps he took in a spell of walking. How much of his walking was two recordings mixed, each a quarter or more. And how fast his steps went against the recordings' own tempo."), 5);
        }

        /// <summary>The turns round on demand: the other is put at his side or behind him, and he turns to face him, quickly or as he did.</summary>
        private void TurnButtons(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Put the other:", "To see a turn round on demand. The other is put, at once, this far round him from where he faces, as far off as he was: an enemy suddenly at his side or behind him. He turns to face him by himself, and that turn is timed below. Standing, or while he walks or circles."), Styles().Text, GUILayout.Width(110f));
            foreach (var (by, name) in new[] { (-90, "left"), (-135, "back left"), (180, "behind"), (135, "back right"), (90, "right") })
            {
                if (Press(name, $"The other is put {(by == 180 ? "straight behind him" : $"{Mathf.Abs(by)} degrees round to his {(by > 0 ? "right" : "left")}")}; he turns to face him."))
                {
                    Shield.Loop = false;
                    Fakes.Pattern = false;
                    Dancing.On = false;
                    message = Duel.PutOther(by) ? $"Turn round: the other is {(by == 180 ? "straight behind him" : $"{Mathf.Abs(by)} degrees round to his {(by > 0 ? "right" : "left")}")}. Watch him turn." : "Turn round: not now (the lab is not set out).";
                }
            }

            GUILayout.EndHorizontal();
            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("His turn:", "How he turns to an enemy at his side or behind him (Combat Feel \"A Fighter Turns Round Quickly\"). Quick (new): the quick turn round from the pack \"Turns\", its steps on his own stand under his own chest and arms, the shorter way, aimed at where the other is. As it was (old): his guard's quarter turn stretched, or its turn round, and the rest eased round on his feet; walking, his facing eased round as he steps."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aFighterTurnsRoundQuickly ? "[quick]" : "quick", "The quick turn round.")) feel.aFighterTurnsRoundQuickly = true;
                if (Press(!feel.aFighterTurnsRoundQuickly ? "[as it was (old)]" : "as it was (old)", "His guard's own turns, as before.")) feel.aFighterTurnsRoundQuickly = false;
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("As he turns:", "What he does as he turns round to the other, here in the lab. Just turns: the quick turn alone, every time. Cuts: every turn round is made with a cut in it, so it can be seen on demand: press one of \"Put the other\" above. As in a fight: he cuts as he turns when the other is striking at him, and one time in four otherwise (Combat Feel \"A Fighter Cuts As He Turns Round\"). Turning to his right, a backhand that goes round with him; to his left, his forehand."), Styles().Text, GUILayout.Width(110f));
            if (Press(Duel.TurnCuts == 1 ? "[just turns]" : "just turns", "The quick turn alone, every time: never with a cut.")) Duel.TurnCuts = 1;
            if (Press(Duel.TurnCuts == 2 ? "[cuts]" : "cuts", "Every turn round is made with a cut in it.")) Duel.TurnCuts = 2;
            if (Press(Duel.TurnCuts == 0 ? "[as in a fight]" : "as in a fight", "As the fight has it: he cuts as he turns when the other is striking at him, and one time in four otherwise.")) Duel.TurnCuts = 0;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("His cut (right):", "The recorded cut whose chest, head and arms are taken for a cut made turning to his right (the pack \"Spin Around Slash\"; never its spin or its leap). To his left it is his own forehand."), Styles().Text, GUILayout.Width(110f));
            foreach (var (cut, name, what) in new[] { ("spin_backhand_high", "high backhand", "A flat backhand at the height of the head and shoulders."), ("spin_backhand_mid", "middle backhand", "A backhand down across the body."), ("spin_chop", "chop", "A chop from overhead.") })
            {
                if (Press(PhysicsBattler.Locomotion.TurnSlashes.RightCut == cut ? $"[{name}]" : name, what)) PhysicsBattler.Locomotion.TurnSlashes.RightCut = cut;
            }

            GUILayout.EndHorizontal();
            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("After the cut:", "How a cut made turning to his right ends (Combat Feel \"A Turn Cut Ends Short\"). Ends short (new): the recording is followed for a tenth of a second after the blade lands (the rest of the sweep across his front), his arm is held out there a moment, then it eases back to his guard. As recorded (old): the spinning man's own carry-through, in which the sword hand goes behind his back and the wrist rolls over."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aTurnCutEndsShort ? "[ends short]" : "ends short", "His arm eases back to his guard soon after the blade lands.")) feel.aTurnCutEndsShort = true;
                if (Press(!feel.aTurnCutEndsShort ? "[as recorded (old)]" : "as recorded (old)", "The recording's own carry-through, as it was.")) feel.aTurnCutEndsShort = false;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("His aim:", "How far a cut made turning round is bent onto its target (Combat Feel \"A Turn Cut Sweeps Free\"). Sweeps free (new): the hand is moved 7 cm at most and the wrist steered 8 degrees at most; the blade meets the other as the turn carries it round. Bent onto him (old): as any attack, the hand moved up to 40 cm and the wrist turned over at the moment planned for the landing, which was the arm twisting as the swing ended."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aTurnCutSweepsFree ? "[sweeps free]" : "sweeps free", "The blade sweeps round with the turn, hardly bent onto the target.")) feel.aTurnCutSweepsFree = true;
                if (Press(!feel.aTurnCutSweepsFree ? "[bent onto him (old)]" : "bent onto him (old)", "Aimed as any attack is, as it was.")) feel.aTurnCutSweepsFree = false;
                GUILayout.EndHorizontal();
                feel.turnCutSweepsOn = Lever("Cut sweeps on", feel.turnCutSweepsOn, 0.03f, 0.3f, "A turn cut ends short: how long after the blade lands the recording's sweep is still followed (seconds, times \"Blade goes\"). 0.1: across his front, out to his side; more, on round behind him.");
                feel.turnCutCarriesOn = Lever("Cut carries on", feel.turnCutCarriesOn, 0.1f, 0.5f, "A turn cut ends short: how long after the blade lands his arm stays out at the end of its sweep before it eases back to his guard (seconds).");
                feel.turnCutBladeGoes = Lever("Blade goes", feel.turnCutBladeGoes, 0.5f, 1.6f, "A cut made turning round: how far the blade goes. How far the sword arm is drawn back before the cut and carried on after it, against the recording (1).");
                feel.turnCutBladeLeads = Lever("Blade leads", feel.turnCutBladeLeads, -1f, 1f, "A cut made turning round: the blade against his body's turn. 0: the cut comes as his body comes round. Toward 1 the blade leads, ahead of his body through the turn. Toward -1 it lags: his body is round first and the blade is whipped through after it.");
            }

            Readout(new GUIContent("Last turn: " + Duel.TurnBackText + ".", "How far round the other was put, and how long he then took to face him again (to within 15 degrees)."), 2);
        }

        /// <summary>A footwork meter in words, worked out afresh no more than twice a second (it sorts what it has measured).</summary>
        private string FootworkText(FootworkMeter meter, int slot)
        {
            if (Time.unscaledTime >= footworkTextAt[slot] + 0.5f || Time.unscaledTime < footworkTextAt[slot])
            {
                footworkTextAt[slot] = Time.unscaledTime;
                footworkText[slot] = meter.Text;
            }

            return footworkText[slot];
        }

        private readonly float[] footworkTextAt = { -1f, -1f, -1f };
        private readonly string[] footworkText = { "", "", "" };

        /// <summary>The dance's two fighters' footwork, measured while they dance.</summary>
        private readonly FootworkMeter[] danceMeters = { new FootworkMeter(), new FootworkMeter() };

        /// <summary>Moving's settings: how his steps are carried, what a slow order does, how near he walks and how far he backs, and how he set off.</summary>
        private void PaceSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel != null && feel.theGuardWalksInOneCarriage)
            {
                feel.guardKeepsItsStance = Lever("Keeps stance", feel.guardKeepsItsStance, 0f, 1f, "One carriage: how much of his stand's own stance his feet keep while he steps. 0: each recording's own places (his feet nearer each other than he stands). 1: the stand's: as wide, and his left foot as far ahead. More keeps his stance but asks his legs to reach further.");
            }

            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Slow orders:", "What an order to walk up, back away or circle does at a pace under a walk's (about 0.9 m/s). Walked (new): he walks on at that pace on his walk's own cycles, slowed, and to the sides on a slow walk recorded for it. Stepped (old): he stands and takes single short steps, which is meant for a small change of where he stands, not for going on (Combat Feel \"A Walk Kept Up Is Walked Not Stepped\")."), Styles().Text, GUILayout.Width(90f));
                if (Press(feel.aWalkKeptUpIsWalkedNotStepped ? "[walked]" : "walked", "He walks on at any pace.")) feel.aWalkKeptUpIsWalkedNotStepped = true;
                if (Press(!feel.aWalkKeptUpIsWalkedNotStepped ? "[stepped (old)]" : "stepped (old)", "Under a walk's pace he stands and steps, as it was.")) feel.aWalkKeptUpIsWalkedNotStepped = false;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Slow circle:", "What he circles on under his side-step's own pace (about 1.1 m/s), when slow orders are walked. Each is a recording's legs under his guard's own chest and arms. Crouched: the pack's careful crouched step, low, each foot long on the ground. Upright: the pack's slow strafe, feet near each other. Side-step: his guard's own side-step, slowed (no new recording)."), Styles().Text, GUILayout.Width(90f));
                if (Press(feel.slowCircleOn == 1 ? "[crouched]" : "crouched", "The crouched careful step.")) feel.slowCircleOn = 1;
                if (Press(feel.slowCircleOn == 2 ? "[upright]" : "upright", "The upright slow strafe.")) feel.slowCircleOn = 2;
                if (Press(feel.slowCircleOn == 0 ? "[side-step]" : "side-step", "His guard's own side-step, slowed.")) feel.slowCircleOn = 0;
                GUILayout.EndHorizontal();
            }

            if (feel != null)
            {
                feel.quickTurnFrom = Lever("Quick turn from", feel.quickTurnFrom, 45f, 150f, "How far off (degrees) what he is to face must be for the quick turn round. Under it he turns as he did.");
                feel.quickTurnPace = Lever("Quick turn pace", feel.quickTurnPace, 0.8f, 2f, "How fast the quick turn is played against its recording (1: as recorded, the turn itself in half a second).");
                feel.quickTurnWithin = Lever("Quick turn within", feel.quickTurnWithin, 1f, 10f, "An enemy within this many metres makes a turn a hurried one; further off he turns as he did.");
                feel.quickTurnSinks = Lever("Quick turn sinks", feel.quickTurnSinks, 0f, 1f, "How much of the recording's sinking at the middle of the turn he takes (1: all 11 cm of it; 0: his hips stay level).");
                feel.quickTurnDrawsFeetIn = Lever("Turn draws feet in", feel.quickTurnDrawsFeetIn, 0f, 1f, "How far his feet are drawn in from his stand's wide places at the middle of the quick turn (0: as wide as he stands; 1: where the recording has them).");
            }

            Duel.Near = Mathf.Round(10f * Slide("Walks to", $"{Duel.Near:0.0} m", "How near the other he walks (metres, root to root).", Duel.Near, 1f, 3f)) / 10f;
            Duel.Far = Mathf.Round(10f * Slide("Backs to", $"{Duel.Far:0.0} m", "How far from the other he backs away (metres, root to root).", Duel.Far, 2f, 7f)) / 10f;
            // (Always the same lines, whatever the order: the panel is laid out and drawn from the same state.)
            Readout($"Order: {Duel.Now}. {Duel.Gap:0.00} m apart, going {Duel.Speed:0.00} m/s, {Duel.Gone:0.0} m since the order.", 2);
            Readout(new GUIContent($"Setting off: a foot at most {Duel.SetOffFoot:0.0} m/s and the hips {Duel.SetOffHips:0.0} m/s in the first 0.35 s, {Duel.SetOffWent:0.00} m gone by then.",
                "How he set off under the last order: the fastest either foot and the hips went in its first 0.35 s, and how far he had got. Feet whipped to their places while the body has hardly moved read as a jump (Combat Feel \"Sets Off At A Walk's Rhythm\")."), 3);
        }

        /// <summary>The leaps' settings: how far and how long, the made leap's levers, and the last leap.</summary>
        private void LeapSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            Duel.LeapFar = Mathf.Round(20f * Slide("Leap", $"{Duel.LeapFar:0.00} m", "How far a leap goes. He is carried no faster than 4 m/s and his feet are in the air 0.3 s at most, so about 1.2 m is the most.", Duel.LeapFar, 0.2f, 1.2f)) / 20f;
            Duel.LeapTime = Mathf.Round(100f * Slide("In the air", $"{Duel.LeapTime:0.00} s", "How long his feet are off the ground in a leap. Longer is a higher, floatier leap; shorter a low, quick one.", Duel.LeapTime, 0.15f, 0.5f)) / 100f;
            if (feel != null && feel.leapsAreMade)
            {
                feel.leapCrouch = Lever("Crouch", feel.leapCrouch, 0f, 2.5f, "How deep the hips sink before the push and as the legs take the landing.");
                feel.leapHeight = Lever("Height", feel.leapHeight, 0.3f, 3f, "How high the hips rise in the flight, and the legs tuck.");
                feel.leapLean = Lever("Lean", feel.leapLean, 0f, 35f, "How far the chest leans the way of the leap as it pushes off (degrees).");
                feel.leapArms = Lever("Arms", feel.leapArms, 0f, 1f, "How much of the recorded jump's arm swing the arms take (0: they stay in the guard).");
                feel.leapBody = Lever("Body", feel.leapBody, 0f, 1.5f, "How much of the recorded jump's fold forward in the crouch and stretch in the flight the trunk takes.");
                feel.leapPrepTime = Lever("Crouch time", feel.leapPrepTime, 0.08f, 0.5f, "How long the crouch takes before the push (seconds).");
            }

            Readout("Last leap: " + Duel.LastLeap, 2);
        }

        /// <summary>The postures' settings: the drop's and the blade's bluffs'.</summary>
        private void PostureSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel == null)
            {
                return;
            }

            feel.dropSinks = Lever("Sinks", feel.dropSinks, 3f, 25f, "How far his hips sink (cm). His feet stay where they are and his knees bend for it.");
            feel.dropLeans = Lever("Leans", feel.dropLeans, 0f, 25f, "How far he leans forward with it (degrees).");
            feel.dropDown = Lever("Down in", feel.dropDown, 0.05f, 0.4f, "How long it takes to get down (seconds).");
            feel.dropUp = Lever("Up in", feel.dropUp, 0.1f, 0.8f, "How long he takes to come back up (seconds).");
            feel.bladeBluffSize = Lever("Bluff size", feel.bladeBluffSize, 0.3f, 2f, "How big the blade's bluffs are (1: a dip of 38 degrees, a flick of 32, a round of 22).");
        }

        /// <summary>The bash at the other (off) or straight ahead at the air, to see the leap and the drive by themselves (on).</summary>
        private bool bashAtAir;

        /// <summary>The three bashes' buttons (the plan's step 9a, breaking the guard).</summary>
        private void BashButtons()
        {
            var basher = Duel.Mover != null ? Duel.Mover.Clip : null;
            string[] names = { "Small bash", "Medium bash", "Hard bash" };
            string[] tips =
            {
                "A small bash, from where he stands: he sinks behind his board, drives it straight ahead and his body goes in a little with it. It shoves. Stand him within about a metre of the other.",
                "A medium bash: he leaps in behind his board, stopping short of the other, and drives the board as he arrives. It throws the other back stumbling. From 1.5 to 2 m off.",
                "A hard bash: he crouches deep and leaps fast all the way into the other's space, leaning well in, his board driven as it reaches him, and goes on through as the other is thrown out of it. It knocks him down or far back. From 1.5 to 2.5 m off.",
            };
            GUILayout.BeginHorizontal();
            for (int kind = 0; kind < names.Length; kind++)
            {
                if (Press(names[kind], tips[kind]) && basher != null)
                {
                    if (basher.ShieldColliders.Count == 0)
                    {
                        message = "Shield bash: he has no shield. \"Set out\" under the shield test gives both one.";
                    }
                    else
                    {
                        bool bashed = basher.Bash(bashAtAir || Duel.Other == null ? null : Duel.Other.Clip, true, kind);
                        message = bashed ? names[kind] + (bashAtAir ? ", at the air." : ".") : "Shield bash: he could not (in a blow, a leap or a bash, meeting a blow, or down).";
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("He bashes:", "What a press of a bash goes at. The other: from where he stands, leaping as far as that bash does toward where the other is. The air: straight ahead with that bash's whole leap, whoever is there or not, to see the leap and the drive by themselves."), Styles().Text, GUILayout.Width(80f));
            if (Press(!bashAtAir ? "[the other]" : "the other", "He bashes the other.")) bashAtAir = false;
            if (Press(bashAtAir ? "[the air]" : "the air", "He bashes straight ahead, at nobody.")) bashAtAir = true;
            GUILayout.EndHorizontal();
        }

        private readonly PhysicsBattler.Locomotion.BashLevers bashShown = new PhysicsBattler.Locomotion.BashLevers();

        /// <summary>One bash's own settings (0: small, 1: medium, 2: hard), kept on Combat Feel.</summary>
        private void BashSliders(PhysicsBattler.Locomotion.CombatFeel feel, int kind)
        {
            if (feel == null)
            {
                return;
            }

            var b = bashShown;
            feel.BashInto(kind, b);
            b.MostLeap = Lever("Leaps in", b.MostLeap, 0f, 2.5f, "The most he leaps in (metres). 0: he does not leap: he stands, sinks behind his board and his body goes in a little with it.");
            b.EndsShort = Lever("Ends short", b.EndsShort, 0f, 1.2f, "Where his leap ends: this far short of where the other stands (metres). 0: on the other's own spot, all the way into his space. So from 2 m off, with 0.7 here, he leaps 1.3 m (if \"Leaps in\" allows that much).");
            b.LeapSpeed = Lever("Leap speed", b.LeapSpeed, 1.5f, 8f, "How hard he leaps in: the speed of the leap (m/s).");
            b.Reach = Lever("Arm reach", b.Reach, 0f, 1f, "How far his shield arm goes. At 0 a hand's breadth out from its cover. At 1 the board is drawn in to his chest first and then driven out to the arm's whole length, his shield shoulder thrown in behind it (his chest turned up to 40 degrees), so the board travels about three times as far.");
            b.Arm = Lever("Arm drive", b.Arm, 0f, 1f, "How hard his shield arm drives: how quickly the board goes out (0.22 to 0.08 s). Straight ahead, the board looking at the other.");
            b.Weight = Lever("Weight", b.Weight, 0.2f, 2f, "How much of the speed the board arrives with (his body's and his arm's together) the other is shoved back at.");
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("After it:", "What he does once his board has met the other. Goes through: he carries on to where his leap ends, into the space the other is thrown out of. Stops: he comes down where he met him."), Styles().Text, GUILayout.Width(80f));
            if (Press(b.GoesThrough ? "[goes through]" : "goes through", "He carries on to where his leap ends.")) b.GoesThrough = true;
            if (Press(!b.GoesThrough ? "[stops]" : "stops", "He comes down where he met him.")) b.GoesThrough = false;
            GUILayout.EndHorizontal();
            b.Crouch = Lever("Crouch", b.Crouch, 0.5f, 2.5f, "His leap: how deep he crouches before it and as his legs take the landing.");
            b.Lean = Lever("Lean", b.Lean, 0f, 45f, "How far he leans in behind his board (degrees): as he pushes off in a leap; a standing bash leans 0.4 of it.");
            b.Height = Lever("Height", b.Height, 0.2f, 1.5f, "His leap: how high his hips rise in the flight. Low is flat and driving.");
            b.Prep = Lever("Wind-up", b.Prep, 0.08f, 0.5f, "His leap: how long the crouch before it takes (seconds): the bash's wind-up, which the other sees.");
            b.Sinks = Lever("Sinks", b.Sinks, 0f, 0.3f, "Thrown standing (no leap, or the other already against him): how far his hips sink behind the board (leg lengths).");
            b.Surge = Lever("Surge", b.Surge, 0f, 3f, "Thrown standing: how fast his body goes in with the board (m/s).");
            feel.BashFrom(kind, b);
            feel.BashInto(kind, PhysicsBattler.Locomotion.ClipFighter.BashKinds[kind]);
        }

        /// <summary>What a bash does to the one it meets, and whether a fighter bashes by himself.</summary>
        private void BashDoes(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel == null)
            {
                return;
            }

            feel.bashStumblesFrom = Lever("Stumbles from", feel.bashStumblesFrom, 0.5f, 4f, "The shove (m/s) from which the other is thrown back stumbling, his guard open and a blow of his own dropped. Under it he is shoved back a step.");
            feel.bashFellsFrom = Lever("Down from", feel.bashFellsFrom, 2f, 9f, "The shove (m/s) from which the other is knocked down.");
            feel.bashThrows = Lever("Thrown", feel.bashThrows, 0.5f, 3f, "How hard one who is knocked down is thrown, times the shove. The shove times this is what his fall is thrown at: under 9 m/s he goes over about a metre back from where he stood; from 9 he is thrown off his feet and lands some metres back. So at 1.6 a shove of 5.6 m/s and more throws him.");
            feel.bashArmCounts = Lever("Arm counts", feel.bashArmCounts, 0f, 1f, "How much of the arm's own speed counts in what a bash does, against the speed his body comes in with. 1: as much. 0: his body's speed alone. An arm alone has little behind it; a body arriving has all of him.");
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Too near:", "What a fighter with a shield does when an enemy comes too near him (Combat Feel \"A Shield Bashes What Comes Too Near\"). Bashes (new): he drives his shield and his body into him and shoves him back out of his space. Nothing (old): as it was."), Styles().Text, GUILayout.Width(80f));
            if (Press(feel.aShieldBashesWhatComesTooNear ? "[bashes]" : "bashes", "He bashes an enemy who comes too near.")) feel.aShieldBashesWhatComesTooNear = true;
            if (Press(!feel.aShieldBashesWhatComesTooNear ? "[nothing (old)]" : "nothing (old)", "No bash.")) feel.aShieldBashesWhatComesTooNear = false;
            GUILayout.EndHorizontal();
            if (feel.aShieldBashesWhatComesTooNear)
            {
                feel.shieldBashWithin = Lever("Bash within", feel.shieldBashWithin, 0.6f, 1.6f, "How near an enemy must come to be bashed (metres, root to root). A blow reaches at about 1.5 m; a blow on the move steps in to about 1 m.");
            }
        }

        /// <summary>The strikes' settings: the feint's and the fake lunge's, how a blow out of a walk is thrown, and the last blow.</summary>
        private void StrikeSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel != null && duelPosture != 0)
            {
                if (duelPosture != 2)
                {
                    feel.feintDepth = Lever("Feint goes", feel.feintDepth, 0.3f, 1f, "How far into the attack's wind-up the feint goes before it is taken back. Nearer 1 it is told apart from the real blow later.");
                    feel.feintHold = Lever("Feint holds", feel.feintHold, 0f, 0.4f, "How long the feint is held at its furthest (seconds).");
                    feel.feintBack = Lever("Feint back", feel.feintBack, 0.25f, 1f, "How long the feint takes to come back (seconds).");
                    feel.feintBladeClear = Lever("Blade clear", feel.feintBladeClear, 0f, 0.4f, "A feint stops short of where its blade would be nearer his own head, neck or chest than this (metres from their middles). 0: it is held wherever the wind-up has it, as it was (on the forehand: against his head).");
                }

                if (duelPosture >= 2)
                {
                    feel.fakeLungeFar = Lever("Lunge goes", feel.fakeLungeFar, 0.1f, 0.6f, "How far the fake lunge carries his hips toward the other (metres).");
                    feel.fakeLungeOut = Lever("Lunge takes", feel.fakeLungeOut, 0.15f, 0.6f, "How long the fake lunge takes to get there (seconds). With a feint on it the legs take as long as the sword does.");
                    feel.fakeLungeBack = Lever("Lunge back", feel.fakeLungeBack, 0.25f, 1f, "How long the legs take to come back (seconds).");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Walking:", "What a fake lunge does when it is asked for while he walks. On the move (new): his legs go on walking and his hips dart toward the other and sink, and come back. Stops (old): he stops and lunges on the attack's own legs."), Styles().Text, GUILayout.Width(90f));
                    if (Press(feel.aFakeLungeIsThrownOnTheMove ? "[on the move]" : "on the move", "Thrown over the walk (Combat Feel \"A Fake Lunge Is Thrown On The Move\").")) feel.aFakeLungeIsThrownOnTheMove = true;
                    if (Press(!feel.aFakeLungeIsThrownOnTheMove ? "[stops]" : "stops", "He stops for it, as it was.")) feel.aFakeLungeIsThrownOnTheMove = false;
                    GUILayout.EndHorizontal();
                    if (feel.aFakeLungeIsThrownOnTheMove)
                    {
                        feel.fakeLungeOnTheMove = Lever("Darts in", feel.fakeLungeOnTheMove, 0.2f, 1.2f, "On the move: how much of the lunge's distance his hips dart toward the other over the walk's legs.");
                    }
                }
            }

            if (feel != null)
            {
                // (Three ways to strike out of a walk: over it; planted, his speed carried in; stopped dead, as it was.)
                int way = feel.attacksAreThrownOnTheMove ? 0 : feel.attacksStartFromWhereHeIs ? 1 : 2;
                GUILayout.Label(new GUIContent("He strikes:", "How a blow asked for while he walks is thrown. On the move: the walk goes on under the blow, its legs answering it (they are checked as the blade comes round, and step in so it reaches). Planted: he plants himself and lunges, the way he was going carried into the blow and run down under its wind-up. Stopped: he stops in one step and strikes from his guard, as it was before. Standing still, or stepping slowly in his stance, a blow is always planted."), Styles().Text);
                GUILayout.BeginHorizontal();
                if (Press(way == 0 ? "[on the move]" : "on the move", "The walk goes on under the blow (Combat Feel \"Attacks Are Thrown On The Move\"). He must be walking: a pace of about 1 m/s or more.")) way = 0;
                if (Press(way == 1 ? "[planted]" : "planted", "He plants himself and lunges, his speed carried into the blow (Combat Feel \"Attacks Start From Where He Is\").")) way = 1;
                if (Press(way == 2 ? "[stopped (old)]" : "stopped (old)", "He stops dead and strikes from his guard, as it was.")) way = 2;
                GUILayout.EndHorizontal();
                feel.attacksAreThrownOnTheMove = way == 0;
                feel.attacksStartFromWhereHeIs = way != 2;
                if (way == 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Out of reach:", "What a blow asked for on the move does when the walk cannot bring it within reach. On the move (new): it is thrown on the move all the same, and misses. Plants (old): he stops, plants himself and lunges for it."), Styles().Text, GUILayout.Width(90f));
                    if (Press(feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach ? "[on the move]" : "on the move", "Thrown on the move all the same; it misses (Combat Feel \"A Blow On The Move Stays On The Move Out Of Reach\").")) feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach = true;
                    if (Press(!feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach ? "[plants]" : "plants", "He plants himself and lunges for it, as it was.")) feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach = false;
                    GUILayout.EndHorizontal();
                    feel.movingBlowChecksTheWalk = Lever("Checks walk", feel.movingBlowChecksTheWalk, 0f, 0.85f, "On the move: how much the walk is checked through the blow. 0: his legs go on as if nothing happened. 0.45: as the blade lands he goes at 55% of his pace, his steps shortening and slowing with it, and picks up again after: a catch in his stride.");
                    feel.movingBlowStepsIn = Lever("Steps in", feel.movingBlowStepsIn, 0f, 1f, "On the move: how far the walk may carry him toward the target under the blow so the blade reaches (metres). 0: he strikes from wherever the walk has him, and what is out of reach is thrown planted.");
                    feel.movingBlowHipsTurn = Lever("Hips turn", feel.movingBlowHipsTurn, 0f, 1f, "On the move: how far the hips go from the walk's to the blow's own; the spine twists for the rest. 0: the hips are the walk's alone. 1: the blow's alone, turned 40 to 70 degrees over legs that go on walking.");
                    feel.movingBlowHipsGoIn = Lever("Hips go in", feel.movingBlowHipsGoIn, 0f, 0.2f, "On the move: how far the hips go toward the target as the blade lands, his weight into the blow (metres).");
                }
                else if (way == 1)
                {
                    feel.attackCarriesFor = Lever("Carries for", feel.attackCarriesFor, 0.1f, 0.8f, "Planted: how long the way he was going is carried on into the attack (seconds): his speed runs down to nothing over it. Longer is more momentum; shorter a sharp check before the blow.");
                }
            }

            Readout(new GUIContent("Last blow: " + (Duel.BlowBegan.Length > 0 ? "asked while " + Duel.BlowBegan + "; " + Duel.BlowThrown + "." : "none yet."), "What he was doing as the blow was asked for, and how it was thrown: on the move (which blow, when it was to land, the step in the walk took for it) or planted (and why, if it was asked for on the move)."), 3);
            Readout(new GUIContent("Its start: " + (Duel.BlowStart.Length > 0 ? Duel.BlowStart + "." : "none yet."),
                "How smoothly the blow began, over its first 0.4 s: the biggest change of speed from one step to the next of his root and, as posed, of his hips, each foot and his sword hand. A body going 1.4 m/s that stops in one step shows as 1.4: that is the snap. And how far his held feet slid over the ground."), 5);
            Readout(aimLast.Length > 0 ? aimLast : "", 3);
        }

        /// <summary>The shield test's settings and what was measured.</summary>
        private void ShieldTestSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            Shield.Spot = AimSpot.Chest;
            GUILayout.Label(new GUIContent("It uncovers its chest: it lowers its shield.", "The part the target uncovers and the swordsman is to hit: its chest, by lowering its shield. (Its belly and hips sit behind the middle of the board; no clean move of the shield takes it off them, so they are left to the real postures.)"), Styles().Text);
            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Swordsman:", "How he takes the opening. Reads it (new): he recognises the uncovering as it starts and times his blow to land while the part is open. He has to have seen that uncovering once: the first time, he can only react. Only reacts: he starts his blow once he sees the part open."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.theSwordsmanReadsOpenings ? "[reads it]" : "reads it", "He reads the uncovering (Combat Feel \"The Swordsman Reads Openings\").")) feel.theSwordsmanReadsOpenings = true;
                if (Press(!feel.theSwordsmanReadsOpenings ? "[only reacts]" : "only reacts", "He starts his blow once he sees the part open.")) feel.theSwordsmanReadsOpenings = false;
                if (Press("Forget", "He forgets every uncovering he has seen, and the pattern of the target's fakes: the next is new to him.")) { Shield.Taker.Forget(); Fakes.Forget(); }
                GUILayout.EndHorizontal();
                feel.openingReactionTime = Lever("Reaction", feel.openingReactionTime, 0.05f, 0.6f, "How long after he recognises the uncovering (reading) or sees the part open (reacting) he starts his blow (seconds).");
                feel.quickestOpeningBlow = Lever("Quickest blow", feel.quickestOpeningBlow, 0.3f, 0.8f, "The least time his blow may take to land (seconds). In its own time a blow takes about 0.6 s; under about 0.4 s it lands off its mark.");
                feel.uncoverTell = Lever("Tell lasts", feel.uncoverTell, 0f, 0.8f, "How long the target's tell lasts before its shield starts away (seconds): its blade's point is held down that long. It is what a reader goes by: with none, reading gains him nothing.");
                feel.uncoverOpenFor = Lever("Open for", feel.uncoverOpenFor, 0.15f, 1.2f, "How long the target leaves the part open (seconds).");
                Shield.Taker.Reads = feel.theSwordsmanReadsOpenings;
                Shield.Taker.Reaction = feel.openingReactionTime;
                Fakes.Taker.Reads = feel.theSwordsmanReadsOpenings;
                Fakes.Taker.Reaction = feel.openingReactionTime;
            }

            Readout(new GUIContent("Now: " + Shield.NowText, "The part chosen as the swordsman sees it this moment, and whether he has seen this uncovering before (what he goes by when he reads it)."), 2);
            Readout(new GUIContent("Last go: " + (Shield.Now != null ? (Shield.Now.Done ? "" : "(under way) ") + Shield.Now.Verdict : "none yet."), "The last go: when the part was open, as measured on the bodies (seconds after the uncovering began, its tell included); when the swordsman's blow started; what it met and when; and what he went by."), 4);
            Readout(Shield.Trials.Count > 0 ? Shield.Tally() : "", 2);
        }

        /// <summary>The fakes test's settings and what was measured.</summary>
        private void FakeTestSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            Readout(new GUIContent(TargetHasShield ? "The target has its sword and shield." : "The target has no sword and shield yet: any button below sets the lab out first.", "The fakes test and the shield test need the target to stand with sword and shield, 1.5 m from the swordsman. Set out (in the shield test) does that; so does the first press of any button of either test."), 2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Its pattern:", "Steady: always the same number of short fakes before the long one, so it can be counted. Mixed up: the number is drawn afresh each time (1 to 3), so counting tells nothing, and a reader is wrong as often as right."), Styles().Text, GUILayout.Width(80f));
            if (Press(!Fakes.MixedUp ? "[steady]" : "steady", "Always the same number of short fakes before the long one.")) Fakes.MixedUp = false;
            if (Press(Fakes.MixedUp ? "[mixed up]" : "mixed up", "The number of short fakes before the long one is drawn afresh each time.")) Fakes.MixedUp = true;
            GUILayout.EndHorizontal();
            Fakes.Shorts = Mathf.RoundToInt(Slide("Short ones", $"{Fakes.Shorts}", "How many short fakes come before each long one (the steady pattern).", Fakes.Shorts, 1f, 4f));
            Fakes.Beat = Mathf.Round(10f * Slide("Beat", $"{Fakes.Beat:0.0} s", "How long from the start of one fake to the start of the next.", Fakes.Beat, 1.3f, 3f)) / 10f;
            if (feel != null)
            {
                feel.longFakeHold = Lever("Long holds", feel.longFakeHold, 0f, 0.6f, "How long the long fake is held at its furthest (seconds). The longer, the longer its chest is open.");
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("In a fake:", "What a fighter's shield does while he is in a fake of his own (Combat Feel \"A Sword Fake Takes The Shield\"). Free (as it is): a fake is the sword arm's alone; the shield stays in its cover and meets a blow as readily as ever. Committed: it does not go to meet a blow until the fake is over, so a fake costs him his cover."), Styles().Text, GUILayout.Width(80f));
                if (Press(!feel.aSwordFakeTakesTheShield ? "[shield free]" : "shield free", "A fake is the sword arm's alone.")) feel.aSwordFakeTakesTheShield = false;
                if (Press(feel.aSwordFakeTakesTheShield ? "[shield committed]" : "shield committed", "In a fake his shield does not go to meet a blow.")) feel.aSwordFakeTakesTheShield = true;
                GUILayout.EndHorizontal();
            }

            Readout(new GUIContent("The pattern: " + Fakes.Learned + ".", "What the swordsman has learned of the target's pattern by watching it: nothing of it is given him."), 2);
            Readout(new GUIContent("Last fake: " + (Fakes.Now != null ? Fakes.Now.Text(Fakes.Spot) : "none yet") + ".", "The target's last fake: when it was furthest and how long it took, and when it left its chest open, as measured on the bodies (seconds after it began)."), 3);
            Readout(new GUIContent("Last blow: " + (Fakes.LastBlow != null ? Fakes.LastBlow.Verdict : "none yet."), "The swordsman's last blow in this test: what he threw it by, what it met, and whether the chest was open as it met."), 4);
            Readout(Fakes.Fakes.Count > 0 ? Fakes.Tally() : "", 2);
        }

        /// <summary>The dance's settings and what was measured: each fighter's pattern, whether he reads, what he has learned, how it goes.</summary>
        private void DanceSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Reading:", "Whether fighters read each other's patterns at all (Combat Feel \"Fighters Read Each Other's Patterns\"). Off (old): nobody reads, whatever he is set to below."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.fightersReadEachOthersPatterns ? "[on]" : "on", "A fighter set to read learns the other's pattern and uses it.")) feel.fightersReadEachOthersPatterns = true;
                if (Press(!feel.fightersReadEachOthersPatterns ? "[off (old)]" : "off (old)", "Nobody reads.")) feel.fightersReadEachOthersPatterns = false;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Their feet:", "Whether the dance is paced by the gap between them (Combat Feel \"The Dance Is Paced By The Gap\"). Paced (new): each keeps the distance he likes, goes round the other in spells, comes in for his blow and out after it, and steps out or stands for a blow he expects; how restless and how aggressive he is are his own levers (\"Each his own\"). On their spots (old): they stand where they are."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.theDanceIsPacedByTheGap ? "[paced by the gap]" : "paced by the gap", "They move about each other.")) feel.theDanceIsPacedByTheGap = true;
                if (Press(!feel.theDanceIsPacedByTheGap ? "[on their spots (old)]" : "on their spots (old)", "They stand where they are.")) feel.theDanceIsPacedByTheGap = false;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Stopped:", "What happens when a blow is stopped by a shield or a sword. Answered (new): the one who stopped it strikes back at once, while the other is still in his blow; it is what a blow wasted on a fake costs (Combat Feel \"A Stopped Blow Is Answered\"). Nothing (old): a stopped blow costs nothing."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.aStoppedBlowIsAnswered ? "[answered]" : "answered", "The one who stopped it strikes back at once.")) feel.aStoppedBlowIsAnswered = true;
                if (Press(!feel.aStoppedBlowIsAnswered ? "[nothing (old)]" : "nothing (old)", "A stopped blow costs nothing.")) feel.aStoppedBlowIsAnswered = false;
                GUILayout.EndHorizontal();
            }

            string[] whose = { "Him", "The other" };
            var both = new[] { Dancing.A, Dancing.B };
            for (int k = 0; k < 2; k++)
            {
                var d = both[k];
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(whose[k] + ":", "His pattern of gestures. Each made-up fighter has one of his own; pressing Dance the first time gives each the one that goes with his own levers (\"Each his own\")."), Styles().Text, GUILayout.Width(80f));
                foreach (string name in PhysicsBattler.Locomotion.FighterLevers.Names)
                {
                    string tip = name == "Plain" ? "A blade dip and a short fake, then a long fake, then his blow."
                        : name == "Prowler" ? "Three quick feelers (blade flick, blade dip, short fake), then he sinks (a drop), and springs: low, at the legs."
                        : name == "Strutter" ? "A blade circle, a blade dip and a short fake, then a long fake slash, then his blow: high, at the head and shoulders."
                        : "Two blade flicks, then his point dips, and he cuts: at the body and the thighs.";
                    if (Press(d.Pattern.Name == name ? $"[{name}]" : name, tip))
                    {
                        bool mixed = d.Pattern.MixedUp;
                        d.Pattern = GesturePattern.Named(name);
                        d.Pattern.MixedUp = mixed;
                        dancePatterned = true;
                        d.Foe.Forget();
                    }
                }

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(84f);
                if (Press(!d.Pattern.MixedUp ? "[plain]" : "plain", "Always the same number of gestures before his tell, and his blow always follows it: a poorer swordsman's pattern, which can be counted.")) d.Pattern.MixedUp = false;
                if (Press(d.Pattern.MixedUp ? "[mixed up]" : "mixed up", "The number of gestures before his tell is drawn afresh each time (1 to 3), and one tell in three has no blow after it: counting tells nothing.")) d.Pattern.MixedUp = true;
                if (Press(d.Reads ? "[reads]" : "reads", "He learns the other's pattern by watching and uses it.")) d.Reads = true;
                if (Press(!d.Reads ? "[only reacts]" : "only reacts", "He gestures on regardless, and goes for a chest he sees open.")) d.Reads = false;
                GUILayout.EndHorizontal();
                if (feel != null)
                {
                    d.Taker.Reaction = feel.openingReactionTime;
                }

                Readout(new GUIContent("His pattern: " + d.Pattern.Text + ".", "What he throws, in order."), 2);
                Readout(new GUIContent("He has learned: " + d.Learned + ".", "What he has learned of the other's pattern by watching it: nothing of it is given him."), 3);
                var last = d.Blows.Count > 0 ? d.Blows[d.Blows.Count - 1] : null;
                Readout(new GUIContent("His last blow: " + (last != null ? (last.Done ? "" : "(under way) ") + last.Verdict : "none yet."), "His last real blow: why he threw it, what it met, and what the other was doing as it met."), 4);
                Readout(Dancing.Gestures(d) > 0 ? Dancing.Tally(d) : "", 4);
                Readout(new GUIContent(danceMeters[k].Seconds > 0.5f ? "His footwork " + FootworkText(danceMeters[k], 1 + k) + "." : "", "His footwork in this dance, by the footwork meter (under \"Move him\"): the band his hips' height moved over while he stood or walked, how often they rose or sank 10 cm or more within 0.3 s, his steps a spell of walking, how much of his walking was two recordings mixed, and his steps' tempo against the recordings'."), 5);
                Readout(new GUIContent(Dancing.Gestures(d) > 0 ? "His blows: " + Dancing.Classes(d) + "." : "", "His blows by class: how many light and heavy, how long each took to land, how many landed and what those did; how often he struck into the other's wind-up; how often he dodged."), 3);
            }

            Readout(new GUIContent(Dancing.On && Dancing.GapSteps > 0 ? $"Now {Dancing.Gap:0.00} m apart; he is {(Dancing.A.Tempo.Length > 0 ? Dancing.A.Tempo : "standing")}, the other {(Dancing.B.Tempo.Length > 0 ? Dancing.B.Tempo : "standing")}. So far: {Dancing.Feet()}." : "", "The gap between them now, what each is about (coming in from far off; probing at the edge of reach; pressing, when the fight goes his way; shelled up, when it goes against him), and the gap and their walking over the whole dance."), 4);
            Readout(new GUIContent(Dancing.Ready ? $"How the fight is going: him {Dancing.A.Going(Time.time):+0;-0;0}, the other {Dancing.B.Going(Time.time):+0;-0;0}." : "", "Blows landed less blows taken, over the last 20 seconds. The first number the fight keeps of how it is going for each: what a fighter does next will come to hang on it."), 1);
        }

        /// <summary>The duel lab's orders: walk to, back away, stand, circle.</summary>
        private void DuelMoveButtons()
        {
            GUILayout.BeginHorizontal();
            if (Press("Walk to", $"He walks up to the other and stands {Duel.Near:0.0} m from it.")) Duel.Give(DuelLab.Order.WalkTo);
            if (Press("Back away", $"He backs away from the other, facing it, until he is {Duel.Far:0.0} m from it.")) Duel.Give(DuelLab.Order.BackAway);
            if (Press("Stand", "He stands where he is.")) Duel.Give(DuelLab.Order.Stand);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Press("Circle left", "He goes round the other to his own left, facing it, at the distance he is at now, until told otherwise.")) Duel.Give(DuelLab.Order.CircleLeft);
            if (Press("Circle right", "He goes round the other to his own right, facing it, at the distance he is at now, until told otherwise.")) Duel.Give(DuelLab.Order.CircleRight);
            GUILayout.EndHorizontal();
        }

        /// <summary>The duel lab's leaps and hops.</summary>
        private void DuelLeapButtons()
        {
            GUILayout.BeginHorizontal();
            if (Press("Leap in", "He springs toward the other.")) Duel.Give(DuelLab.Order.LeapIn);
            if (Press("Leap out", "He springs back from the other, facing it.")) Duel.Give(DuelLab.Order.LeapOut);
            if (Press("Hop left", "He springs to his own left.")) Duel.Give(DuelLab.Order.HopLeft);
            if (Press("Hop right", "He springs to his own right.")) Duel.Give(DuelLab.Order.HopRight);
            GUILayout.EndHorizontal();
        }

        /// <summary>The duel lab's drop.</summary>
        private void DuelDropButton()
        {
            GUILayout.BeginHorizontal();
            if (Press("Drop", "He dips now.") && Duel.Mover != null && Duel.Mover.Clip != null)
            {
                bool dropped = Duel.Mover.Clip.Drop();
                message = dropped ? "Duel lab: he drops." : "Duel lab: he cannot drop just now (he is in a blow or a leap, or one is under way).";
                if (dropped)
                {
                    Duel.WatchOpenings("drop");
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (Press("Dodge left", "He ducks and leans to his left, his hips going that way, and comes back up; his feet stay where they are and his shield goes with his chest. Rebuilt from the recordings \"Dodging\".") && Duel.Mover != null && Duel.Mover.Clip != null)
            {
                message = Duel.Mover.Clip.Dodge(-1) ? "Dodge left." : "Dodge: he could not (in a blow, a drop under way, or down).";
            }

            if (Press("Dodge right", "He ducks and leans to his right, and comes back up.") && Duel.Mover != null && Duel.Mover.Clip != null)
            {
                message = Duel.Mover.Clip.Dodge(1) ? "Dodge right." : "Dodge: he could not (in a blow, a drop under way, or down).";
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>The duel lab's bluffs of blade and shield, and the guard's height.</summary>
        private void DuelBluffButtons()
        {
            var poser = Duel.Mover != null ? Duel.Mover.Clip : null;
            GUILayout.BeginHorizontal();
            if (Press("Blade dip", "The point dips and comes back.") && poser != null && poser.Bluff(PhysicsBattler.Locomotion.Locomotor.Pulse.BladeDip)) Duel.WatchOpenings("blade dip");
            if (Press("Blade flick", "The point flicks aside and comes back.") && poser != null && poser.Bluff(PhysicsBattler.Locomotion.Locomotor.Pulse.BladeFlick)) Duel.WatchOpenings("blade flick");
            if (Press("Blade circle", "The point goes once round.") && poser != null && poser.Bluff(PhysicsBattler.Locomotion.Locomotor.Pulse.BladeCircle)) Duel.WatchOpenings("blade circle");
            // (No shield press: the shield keeps its cover and does not gesture. The playtest of 17 November.)
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Guard:", "Where he carries his sword arm: as the recordings have it, higher, or lower. He stays in it until it is changed."), Styles().Text, GUILayout.Width(60f));
            if (Press(duelGuard == 1 ? "[high]" : "high", "The sword arm carried high.")) duelGuard = 1;
            if (Press(duelGuard == 0 ? "[middle]" : "middle", "As the recordings have it.")) duelGuard = 0;
            if (Press(duelGuard == -1 ? "[low]" : "low", "The sword arm carried low.")) duelGuard = -1;
            GUILayout.EndHorizontal();
            if (poser != null)
            {
                poser.GuardAt(duelGuard);
            }
        }

        /// <summary>The pace he is asked to go at, in every order.</summary>
        private void DuelPace()
        {
            bool walked = PhysicsBattler.Locomotion.Locomotor.WalksOn;
            string slow = walked ? "slow walk" : "stand and step";
            Duel.Pace = Mathf.Round(10f * Slide("Pace", Duel.Pace > 0.01f ? $"{Duel.Pace:0.0} m/s ({(Duel.Pace < 0.9f ? slow : Duel.Pace < 1.9f ? "walk" : Duel.Pace < 3.3f ? "brisk" : "run")})" : $"{Duel.PaceNow:0.0} m/s ({slow})", "How fast he is asked to go, in every order. All the way down: his guard's own pace, as the fight asks. Under about 1 m/s it is a slow walk (or, with \"Under a walk's pace\" set to stepped, single steps from a stand); to about 2 m/s he walks; above it he goes at a jog with his guard up (Combat Feel \"Guard Jogs\"), and faster still the jog's stride is stretched.", Duel.Pace, 0f, 3.6f)) / 10f;
        }

        /// <summary>What a press of a part asks him for: a blow, or a posture.</summary>
        private void DuelPressIs()
        {
            GUILayout.Label(new GUIContent("A press is:", "What pressing a part of the body asks him for. A blow: the attack. The other three are postures, the short bursts a fighter throws to draw the other out; each is the beginning of the very attack he would have made at that part, taken back. A feint: his sword arm and chest go into the attack's wind-up and come back. A fake lunge: his legs and hips go half way into the attack's lunge and come back, his arms staying in his guard (from a walk he stops for it). Both: a feint on a fake lunge, which reads as a blow he means."), Styles().Text);
            GUILayout.BeginHorizontal();
            if (Press(duelPosture == 0 ? "[a blow]" : "a blow", "The attack itself.")) duelPosture = 0;
            if (Press(duelPosture == 1 ? "[a feint]" : "a feint", "The attack's wind-up, taken back. Works standing and on the move.")) duelPosture = 1;
            if (Press(duelPosture == 2 ? "[a fake lunge]" : "a fake lunge", "The attack's lunge, half way and back, his arms in his guard.")) duelPosture = 2;
            if (Press(duelPosture == 3 ? "[both]" : "both", "A feint on a fake lunge.")) duelPosture = 3;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("The blow is:", "The class of the blow a press throws (Combat Feel \"Blows Are Light Or Heavy\"). Plain: as every blow was. Light: fast and less powerful, little wind-up, back at once. Heavy: slow and powerful, wound far back and slowly, carried all the way through. The same recorded cut is made each way."), Styles().Text, GUILayout.Width(90f));
            if (Press(Aim.Kind == PhysicsBattler.Locomotion.AttackClass.Plain ? "[plain]" : "plain", "As every blow was.")) Aim.Kind = PhysicsBattler.Locomotion.AttackClass.Plain;
            if (Press(Aim.Kind == PhysicsBattler.Locomotion.AttackClass.Light ? "[light]" : "light", "Fast and less powerful.")) Aim.Kind = PhysicsBattler.Locomotion.AttackClass.Light;
            if (Press(Aim.Kind == PhysicsBattler.Locomotion.AttackClass.Heavy ? "[heavy]" : "heavy", "Slow and powerful, with a long wind-up.")) Aim.Kind = PhysicsBattler.Locomotion.AttackClass.Heavy;
            GUILayout.EndHorizontal();
        }

        /// <summary>The body map: a press strikes at the part.</summary>
        private void DuelBodyMap()
        {
            AimRow(AimSpot.Head);
            AimRow(AimSpot.ShoulderR, AimSpot.Chest, AimSpot.ShoulderL);
            AimRow(AimSpot.UpperArmR, AimSpot.Belly, AimSpot.UpperArmL);
            AimRow(AimSpot.ThighR, AimSpot.Hips, AimSpot.ThighL);
            AimRow(AimSpot.ShinR, AimSpot.ShinL);
        }

        /// <summary>The shield test's buttons.</summary>
        private void ShieldTestButtons()
        {
            bool hasShield = Aim.Target != null && Aim.Target.Clip != null && Aim.Target.Clip.ShieldColliders.Count > 0;
            GUILayout.BeginHorizontal();
            if (Press(hasShield && shieldSetOut ? "Set out again" : "Set out", "Sets the lab out for the test: the target with sword and shield, the two standing 1.5 m apart. The swordsman forgets what he has seen."))
            {
                ShieldTestScene();
            }

            if (Press("Uncover", "The target uncovers the part now, once. First its tell: the point of its blade dips and is held down (its sword, not its shield). Then its shield is carried straight down, still facing the swordsman, until a straight line from the swordsman's sword shoulder reaches its chest with room to spare; it stays away for as long as set and comes back. What is happening is written over the two as it happens.") && !SetOutFirst(1))
            {
                string cannot = Shield.Cannot;
                message = Shield.Uncover(Time.time) ? $"Shield test: the target uncovers its {AimSpots.Name(Shield.Spot)}." : "Shield test: not now: " + (cannot.Length > 0 ? cannot : "the target cannot (it is in a blow, or down)") + ".";
            }

            if (Press(Shield.Loop ? "[Loop]" : "Loop", "It uncovers again and again, a moment after each go is over and both are back on their spots.") && (Shield.Loop || !SetOutFirst(2))) Shield.Loop = !Shield.Loop;
            GUILayout.EndHorizontal();
        }

        /// <summary>The fakes test's buttons.</summary>
        private void FakeTestButtons()
        {
            GUILayout.BeginHorizontal();
            if (Press("Short fake", "The target throws one short fake slash now. (With no sword and shield on the target, the lab is set out for it first.)") && !SetOutFirst(3) && !Fakes.Throw(false, Time.time)) message = "Fakes test: not now: " + (Fakes.Cannot.Length > 0 ? Fakes.Cannot : "the target cannot") + ".";
            if (Press("Long fake", "The target throws one long fake slash now. By itself it has no pattern before it, so a reader has nothing to go by and lets it go; one who only reacts goes for it.") && !SetOutFirst(4) && !Fakes.Throw(true, Time.time)) message = "Fakes test: not now: " + (Fakes.Cannot.Length > 0 ? Fakes.Cannot : "the target cannot") + ".";
            if (Press(Fakes.Pattern ? "[Pattern]" : "Pattern", "The target plays its pattern round and round: so many short fakes, then a long one, one on each beat. Struck, or off its spot, it begins the pattern again once it is back.") && (Fakes.Pattern || !SetOutFirst(5)))
            {
                Fakes.Pattern = !Fakes.Pattern;
                Fakes.Restart();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// The duel lab's buttons that have a fighter do something stand in a window of their own (on), the panel left
        /// the settings and what was measured; or in the panel among them, as they were (off). Kept between plays.
        /// </summary>
        private bool ActionsApart
        {
            get => actionsApartShown;
            set
            {
                actionsApart = value ? 1 : 0;
                PlayerPrefs.SetInt("PhysicsBattler.ActionsApart", actionsApart);
            }
        }

        // (What is shown changes only as a layout begins: the panel is laid out and drawn from the same state.)
        private bool actionsApartShown, actionsWindowShown;
        private int actionsApart = -1;
        private Rect actionsRect = new Rect(-1f, 12f, ActionsWidth, 10f);
        private string actionsTip = "";
        public const float ActionsWidth = 430f;

        /// <summary>A line of what was measured, kept so many lines tall whatever it says: the panel under it does not move as its words change.</summary>
        private void Readout(GUIContent what, int lines)
        {
            var style = Styles().Text;
            GUILayout.Label(what, style, GUILayout.Height(lines * Mathf.Max(14f, style.lineHeight) + 4f));
        }

        private void Readout(string what, int lines) => Readout(new GUIContent(what), lines);

        /// <summary>
        /// The actions window (the duel lab): everything that has one of the two fighters do something, in one place.
        /// The pace stays at its top; under it, in a view that scrolls when it is taller than the screen, each
        /// action's buttons and, folded under them, that action's own settings and what was measured of it.
        /// </summary>
        private void DrawActions(int id)
        {
            DuelPace();
            Readout($"Order: {Duel.Now}. {Duel.Gap:0.00} m apart, going {Duel.Speed:0.00} m/s.", 1);
            float room = Screen.height / Mathf.Max(0.1f, uiScale) - actionsRect.y - actionsChrome - 8f;
            actionsView = actionsContent < 0f ? Mathf.Max(160f, room) : Mathf.Clamp(actionsContent + 4f, 60f, Mathf.Max(160f, room));
            actionsScroll = GUILayout.BeginScrollView(actionsScroll, false, true, GUILayout.Height(actionsView));
            GUILayout.BeginVertical();
            ActionGroups(true);
            GUILayout.Space(6f);
            if (Press("Put these back in the panel", "The buttons and their settings go back into the panel, as they were, and this window closes. \"Actions: in a window\" at the top of the duel lab brings it back."))
            {
                ActionsApart = false;
            }

            GUILayout.Space(4f);
            GUILayout.EndVertical();
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                actionsContent = GUILayoutUtility.GetLastRect().height;
            }

            GUILayout.EndScrollView();
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                actionsTip = GUI.tooltip ?? "";
            }

            GUI.DragWindow();
        }

        private Vector2 actionsScroll;
        private float actionsContent = -1f, actionsChrome = 70f, actionsView;

        /// <summary>From the locomotion lab straight to the aim test, as <see cref="WatchFight"/> goes to the battle.</summary>
        public void WatchAim()
        {
            targetTest = true;
            tubeTest = false;
            aimTest = true;
            fight = true;
            Aim.Stop();
            WatchFight();
            message = "Aim test: press a part of the body and the swordsman strikes at it. The part his blade lands on turns red. R resets.";
        }

        private void SpawnAim(WeaponDefinition sword, WeaponDefinition shield)
        {
            float half = 0.5f * (duelLab ? (shieldSetOut ? AimDistanceMade : DuelApartAtFirst) : aimDistance);
            Vector3 targetSpot = Grid.origin + new Vector3(0f, 0f, half), attackerSpot = Grid.origin + new Vector3(0f, 0f, -half);
            var target = SpawnUnit(BodySize.Medium, 1, targetSpot, "Target", aimTargetKit == 0 ? null : sword, aimTargetKit == 1 ? shield : null);
            // (With made attacks the swordsman has a shield as well: the strikes were recorded with sword and shield, and
            // their guard and their footwork are that kit's.)
            // (In the capoeira style he has a sword alone: the ginga's other arm is its own.)
            bool capoeira = duelLab && duelCapoeira;
            var attacker = SpawnUnit(BodySize.Medium, 0, attackerSpot, "Swordsman", capoeira && duelEmptyHands ? null : sword, (AttackMaker.On || duelLab) && !capoeira ? shield : null);
            Aim.Begin(attacker, target);
            LabelNotes.Clear();
            Shield.Begin(Aim);
            Fakes.Begin(Aim);
            Dancing.Begin(attacker, target);
            dancePatterned = false;
            if (duelLab)
            {
                Duel.Style = capoeira ? PhysicsBattler.Locomotion.MoveStyle.Capoeira : PhysicsBattler.Locomotion.MoveStyle.Standard;
                Duel.Begin(attacker, target);
            }
            else
            {
                Duel.Stop();
            }

            aimLast = "";
            if (target == null || attacker == null)
            {
                return;
            }

            // (Both stand on their spots and neither strikes by itself: the test asks for each blow.)
            target.TargetDummy = attacker.TargetDummy = true;
            target.Unharmed = attacker.Unharmed = true;
            target.DummySpot = targetSpot;
            attacker.DummySpot = attackerSpot;
            target.DummyHolds = attacker.DummyHolds = AimTest.HeldWithin;
            // (Built blows may be tried from nearer than the fight lets enemies come; the clips from where they are.)
            target.StandsAsNearAs = attacker.StandsAsNearAs = AttackMaker.On || BuiltBlow.On ? AimTest.NearestApart : 0f;
            attacker.OnlyTarget = target;
            PlaceAim();
        }

        /// <summary>The aim test's two put as far apart and the target turned as the panel has them: each walks to its spot.</summary>
        private void PlaceAim()
        {
            if (Aim.Attacker == null || Aim.Target == null)
            {
                return;
            }

            float half = 0.5f * (duelLab && shieldSetOut ? AimDistanceMade : aimDistance);
            Vector3 targetSpot = Grid.origin + new Vector3(0f, 0f, half), attackerSpot = Grid.origin + new Vector3(0f, 0f, -half);
            Aim.Target.DummySpot = targetSpot;
            Aim.Attacker.DummySpot = attackerSpot;
            // (Turned 0 the target faces the swordsman; turned 90 it shows him its left side.)
            Vector3 faces = Quaternion.Euler(0f, aimTurned, 0f) * (attackerSpot - targetSpot).normalized;
            Aim.Target.DummyFace = Mathf.Abs(aimTurned) > 0.5f ? targetSpot + faces * 5f : (Vector3?)null;
        }

        private void AimAt(AimSpot spot)
        {
            aimSpot = spot;
            Aim.Stop();
            if (duelLab && duelPosture != 0)
            {
                // (The press is for a posture: the attack is made for the part as a blow is, and only its beginning thrown.)
                PhysicsBattler.Locomotion.PostureMaker.Ask(duelPosture == 1 || duelPosture == 3 ? 1f : 0f, duelPosture >= 2 ? 1f : 0f);
            }

            // (In the duel lab the blow's start is watched, whatever he is doing as it is asked for.)
            if (duelLab && Duel.Running ? Duel.Strike(Aim, spot, Time.time) : Aim.Order(spot, Time.time))
            {
                if (duelLab && Duel.Running)
                {
                    Duel.WatchOpenings(duelPosture == 0 ? "a blow" : duelPosture == 1 ? "feint" : duelPosture == 2 ? "fake lunge" : "feint on a fake lunge");
                }

                message = $"Aim test: a blow asked for the {AimSpots.Name(spot)} ({Aim.Now.Strike}).";
            }
            else
            {
                message = Aim.Following ? "Aim test: the last blow is still under way." : "Aim test: the swordsman can't strike just now (he is busy, or down).";
            }
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
            aimTest = false;
            fight = true;
            Spawn();
            tubesSpawnedAt = Time.time;
            // (A tube is taken as a blow unless the panel says otherwise: the physics alone barely moves a body, and
            // a first hit that did nothing looked like a fault.)
            if (!tubeTestBegun)
            {
                tubeTestBegun = true;
                RecallTubes();
            }

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
                Note($"tube test: {sent} tubes, {Tubes.Mass:0.0} kg at {Tubes.Speed:0.0} m/s ({Tubes.Impulse:0} N·s), height {Tubes.Height:0.00} m, reach {Tubes.Reach:0.00} m, turned {Tubes.Turned:0}, {(Tubes.Fells ? "the fall test (they go down)" : Tubes.AsBlow ? "as blows" : "physics only")}, hit force {PhysicsBattler.Locomotion.CombatFeel.HitForce:0.0}, hips {(PhysicsBattler.Locomotion.ClipFighter.HeldHips ? $"held ({PhysicsBattler.Authority.UpperBodyArticulation.HipQuick:0.0} Hz, {PhysicsBattler.Authority.UpperBodyArticulation.HipMost:0.0} weights)" : "locked")}, physical legs {(PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs ? "on" : "off")}");
            }
        }

        /// <summary>The fall test's blow (N·s): well over what a Medium's legs catch; and a thrown one's, a Large's club at its hardest.</summary>
        public const float FallTestImpulse = 200f, ThrownTestImpulse = 450f;

        /// <summary>How long the targets are given to turn before a fall test's tubes are sent (seconds).</summary>
        public const float FallTestTurnsFor = 0.8f;

        /// <summary>How long targets left turned round are given to turn back to their line before a tube is sent (seconds).</summary>
        public const float TurnsRoundFor = 2.2f;

        private float fallHitAt = -1f;

        // (The fall test that ends in a turn: how far round what the targets face has been put, and how far it is to be put once they are down.)
        private float facesTurnedBy, turnRoundAfterFall;
        private bool tubeTestBegun;

        private float tunedCountedAt = -1f;
        private int tunedCount;
        private bool keepTuned = true;

        private const string TubeKey = "BattleSandbox.Tube.";

        /// <summary>The tube's own settings as the panel's sliders last had them, kept between sessions.</summary>
        private void RememberTubes()
        {
            PlayerPrefs.SetFloat(TubeKey + "Impulse", Tubes.Impulse);
            PlayerPrefs.SetFloat(TubeKey + "Speed", Tubes.Speed);
            PlayerPrefs.SetFloat(TubeKey + "Height", Tubes.Height);
            PlayerPrefs.SetFloat(TubeKey + "Reach", Tubes.Reach);
            PlayerPrefs.SetFloat(TubeKey + "Turned", Tubes.Turned);
            PlayerPrefs.SetFloat(TubeKey + "AsBlow", Tubes.AsBlow ? 1f : 0f);
            PlayerPrefs.SetFloat(TubeKey + "Fells", Tubes.Fells ? 1f : 0f);
        }

        /// <summary>The tube's settings as they were last set by hand (the first time the tube test is begun in a session).</summary>
        private void RecallTubes()
        {
            Tubes.Impulse = PlayerPrefs.GetFloat(TubeKey + "Impulse", Tubes.Impulse);
            Tubes.Speed = PlayerPrefs.GetFloat(TubeKey + "Speed", Tubes.Speed);
            Tubes.Height = PlayerPrefs.GetFloat(TubeKey + "Height", Tubes.Height);
            Tubes.Reach = PlayerPrefs.GetFloat(TubeKey + "Reach", Tubes.Reach);
            Tubes.Turned = PlayerPrefs.GetFloat(TubeKey + "Turned", Tubes.Turned);
            // (A tube is taken as a blow unless the panel has said otherwise: the physics alone barely moves a body.)
            Tubes.AsBlow = PlayerPrefs.GetFloat(TubeKey + "AsBlow", 1f) > 0.5f;
            Tubes.Fells = PlayerPrefs.GetFloat(TubeKey + "Fells", 0f) > 0.5f;
        }

        /// <summary>
        /// The fall test (the plan "Controlled falls", step 1): a fall on demand. The targets are turned so the tube
        /// meets the side that sends them the way asked (<paramref name="turned"/>: 0 struck on the front, they go
        /// backward; 180 on the back, forward; 270 on the right side, to their left; 90 on the left side, to their
        /// right), the tube is taken as a blow, and they go down whatever their legs say. Each fall is measured in
        /// the playtest log (<c>fall</c>: what reached the ground when, the head, the hips).
        /// </summary>
        /// <param name="thenTurn">Not 0: while the targets are down, what they stand facing is put this many degrees round them (+ to their right), so that once up they have to turn round to it: the get-up into a turn.</param>
        public void FallTest(float turned, bool thrown, float thenTurn = 0f)
        {
            if (!tubeTest)
            {
                TubeTestScene();
            }

            bool turns = Mathf.Abs(Mathf.DeltaAngle(Tubes.Turned, turned)) > 1f;
            // (Targets the last fall test left turned round turn back to their line first, which takes a turning round's time.)
            float turnsFor = Mathf.Abs(facesTurnedBy) > 1f ? TurnsRoundFor : turns ? FallTestTurnsFor : 0f;
            facesTurnedBy = 0f;
            turnRoundAfterFall = thenTurn;
            Tubes.Turned = turned;
            Tubes.Impulse = thrown ? ThrownTestImpulse : FallTestImpulse;
            Tubes.Speed = 8f;
            Tubes.Height = 1.3f;
            Tubes.Reach = 0.15f;
            Tubes.AsBlow = true;
            Tubes.Fells = true;
            // (Sent once the targets have turned and a tube is waiting.)
            fallHitAt = Time.time + turnsFor;
            message = $"Fall test: the targets struck on {Tubes.Side} with {Tubes.Impulse:0} N·s, to go down. The tubes are sent as soon as the targets have turned and a tube is waiting.";
        }

        /// <summary>
        /// A blow on demand: the waiting tubes sent as blows of <paramref name="impulse"/> (N·s) at chest height, from
        /// the side the targets are turned to; the legs decide what comes of it (the scene's hit force and the hold on
        /// the hips are what there is to see).
        /// </summary>
        public void BlowTest(float impulse)
        {
            if (!tubeTest)
            {
                TubeTestScene();
            }

            Tubes.Impulse = impulse;
            Tubes.Speed = 8f;
            Tubes.Height = 1.3f;
            Tubes.Reach = 0.15f;
            Tubes.AsBlow = true;
            Tubes.Fells = false;
            // (Targets a fall test left turned round stand to their line again first.)
            fallHitAt = Time.time + (Mathf.Abs(facesTurnedBy) > 1f ? TurnsRoundFor : 0f);
            facesTurnedBy = 0f;
            turnRoundAfterFall = 0f;
            message = $"Blow test: {impulse:0} N·s on {Tubes.Side}, taken as a blow, with a hit force of {PhysicsBattler.Locomotion.CombatFeel.HitForce:0.0}. The tubes are sent as soon as one is waiting.";
        }

        /// <summary>The tube test, each game step: the targets face as the panel turns them, the tubes wait where it puts them, and what they meet is noted.</summary>
        private void StepTubes()
        {
            if (!tubeTest)
            {
                return;
            }

            Vector3 toTarget = Quaternion.Euler(0f, BoardGrid.TeamYaw(0), 0f) * Vector3.forward;
            Vector3 faces = Quaternion.Euler(0f, facesTurnedBy, 0f) * Tubes.Facing(toTarget);
            bool ready = Time.time - tubesSpawnedAt > 1f;
            bool down = false;
            foreach (var u in units)
            {
                if (u == null || !u.TargetDummy)
                {
                    continue;
                }

                down |= u.IsAlive && u.Clip != null && u.Clip.Fallen;
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

            if (down && fallHitAt < 0f && Mathf.Abs(turnRoundAfterFall) > 1f)
            {
                // (A fall test that ends in a turn: what the targets face is put round behind them while they are down.)
                facesTurnedBy = turnRoundAfterFall;
                turnRoundAfterFall = 0f;
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
            aimTest = false;
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

            if (aimTest)
            {
                SpawnAim(sword, shield);
                return;
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
                if (aimTest && duelLab)
                {
                    Duel.Step(Time.fixedDeltaTime);
                }

                if (aimTest)
                {
                    Aim.Step(Time.fixedDeltaTime, Time.time);
                    if (Aim.Now != null && Aim.Now.Done)
                    {
                        aimLast = Aim.Now.Lines;
                    }

                    if (duelLab)
                    {
                        StepShieldPending();
                        Shield.Step(Time.fixedDeltaTime, Time.time);
                        Fakes.Step(Time.fixedDeltaTime, Time.time);
                        Dancing.Step(Time.fixedDeltaTime, Time.time);
                        if (Dancing.On && Dancing.Ready)
                        {
                            danceMeters[0].Step(Dancing.A.Unit, Time.fixedDeltaTime);
                            danceMeters[1].Step(Dancing.B.Unit, Time.fixedDeltaTime);
                        }

                        if (Aim.Target != null && Aim.Attacker != null)
                        {
                            LabelNotes[Aim.Target] = Dancing.On ? (Dancing.B.Note.Length > 0 ? Dancing.B.Note : Dancing.B.Tempo) : Shield.TargetNote.Length > 0 ? Shield.TargetNote : Fakes.TargetNote;
                            LabelNotes[Aim.Attacker] = Dancing.On ? (Dancing.A.Note.Length > 0 ? Dancing.A.Note : Dancing.A.Tempo) : Shield.AttackerNote.Length > 0 ? Shield.AttackerNote : Fakes.AttackerNote;
                        }
                    }
                }
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
            if (SandboxInput.KeyPressed(KeyCode.Alpha7)) AimTestScene();
            if (SandboxInput.KeyPressed(KeyCode.Alpha8)) DuelLabScene();
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
            // (The duel lab's actions in a window of their own, at the right of the screen until it is dragged.)
            if (Event.current != null && Event.current.type == EventType.Layout)
            {
                if (actionsApart < 0)
                {
                    actionsApart = PlayerPrefs.GetInt("PhysicsBattler.ActionsApart", 1);
                }

                actionsApartShown = actionsApart > 0;
                actionsWindowShown = aimTest && duelLab && Duel.Running && actionsApartShown;
            }

            bool actionsShown = actionsWindowShown;
            if (actionsShown)
            {
                if (actionsRect.x < 0f)
                {
                    actionsRect.x = Mathf.Max(panelRect.x + panelRect.width + 12f, Screen.width / uiScale - ActionsWidth - 12f);
                }

                if (Event.current != null && Event.current.type == EventType.Layout)
                {
                    actionsRect.height = 10f;
                }

                actionsRect = GUILayout.Window(4712, actionsRect, DrawActions, "Actions", GUILayout.Width(ActionsWidth));
                if (Event.current != null && Event.current.type == EventType.Repaint && actionsRect.height > actionsView + 20f)
                {
                    // (All of the window that is not the scrolling view: what the view's height leaves room for.)
                    actionsChrome = actionsRect.height - actionsView;
                }

                actionsRect.x = Mathf.Clamp(actionsRect.x, 60f - actionsRect.width, Mathf.Max(0f, Screen.width / uiScale - 60f));
                actionsRect.y = Mathf.Clamp(actionsRect.y, 0f, Mathf.Max(0f, Screen.height / uiScale - 30f));
            }
            else
            {
                actionsTip = "";
            }

            // (Fitted to what is in it afresh each time it is laid out: a window keeps the height it has grown to, so
            // one that had shown a long section stayed that tall when the section was folded shut.)
            if (Event.current != null && Event.current.type == EventType.Layout)
            {
                panelRect.height = 10f;
            }

            panelRect = GUILayout.Window(4711, panelRect, DrawPanel, "Physics Battler", GUILayout.Width(PanelWidth));
            if (Event.current != null && Event.current.type == EventType.Repaint && panelRect.height > panelView + 20f)
            {
                // (All of the panel that is not the scrolling view: what the view's height leaves room for.)
                panelChrome = panelRect.height - panelView;
            }

            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                // (Is the mouse over the panel: on the screen the panel is its size times the panel's scale, from the top.)
                Vector2 mouse = SandboxInput.MousePosition;
                float across = mouse.x / uiScale, down = (Screen.height - mouse.y) / uiScale;
                SandboxInput.OverPanel = across >= panelRect.x && across <= panelRect.x + panelRect.width && down >= panelRect.y && down <= panelRect.y + panelRect.height
                    || (actionsShown && across >= actionsRect.x && across <= actionsRect.x + actionsRect.width && down >= actionsRect.y && down <= actionsRect.y + actionsRect.height);
            }

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

        // ---- The panel -----------------------------------------------------------------------------
        //
        // What is always there: the clock (pause, step, speed) and the recording, at the top; what the last thing
        // pressed did and what the thing under the mouse does, at the bottom. Between them, sections that fold shut,
        // in a view that scrolls when they are taller than the screen. A control shows its name and its value; what
        // it means is its tooltip, shown in the box at the bottom.

        /// <summary>How wide the panel is (at a panel size of 1).</summary>
        public const float PanelWidth = 440f;

        private sealed class PanelStyles
        {
            public GUIStyle Section, Fold, Heading, Text, Value, Toggle, Hint, Box;
        }

        // (What the panel shows, as it was when the panel was last laid out: a press that changes it (a test begun, a
        // recording stopped) shows from the next layout, so every event draws the controls that were laid out for it.)
        private bool shownTube, shownTarget, shownAim, shownDuel, shownRecording, shownFeel;
        private string shownMessage = "";

        private PanelStyles panel;
        private Vector2 panelScroll;
        private float panelContent = -1f, panelChrome = 150f, panelView;
        private readonly Dictionary<string, bool> sectionOpen = new Dictionary<string, bool>();

        private PanelStyles Styles()
        {
            if (panel != null)
            {
                return panel;
            }

            // (Made once: a style made each time the panel is drawn is thrown away each time.)
            panel = new PanelStyles
            {
                Section = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold },
                Fold = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft },
                Heading = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = true },
                Text = new GUIStyle(GUI.skin.label) { wordWrap = true },
                Value = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight },
                Toggle = new GUIStyle(GUI.skin.toggle) { wordWrap = true },
                Hint = new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.UpperLeft, fontStyle = FontStyle.Italic },
                Box = new GUIStyle(GUI.skin.box),
            };
            return panel;
        }

        /// <summary>A section's heading, pressed to fold it shut or open (kept between sessions). True: it is open.</summary>
        private bool Section(string name, bool openAtFirst, string tip)
        {
            if (!sectionOpen.TryGetValue(name, out bool open))
            {
                open = PlayerPrefs.GetFloat("BattleSandbox.Section." + name, openAtFirst ? 1f : 0f) > 0.5f;
                sectionOpen[name] = open;
            }

            GUILayout.Space(6f);
            if (GUILayout.Button(new GUIContent((open ? "−  " : "+  ") + name, tip), Styles().Section))
            {
                // (Open or shut from the next time the panel is laid out: what is drawn in answer to this press has
                // to be what was laid out for it.)
                sectionOpen[name] = !open;
                PlayerPrefs.SetFloat("BattleSandbox.Section." + name, open ? 0f : 1f);
            }

            return open;
        }

        /// <summary>A heading inside a section, with room over it.</summary>
        private void Heading(string text, string tip = "")
        {
            GUILayout.Space(8f);
            GUILayout.Label(new GUIContent(text, tip), Styles().Heading);
        }

        private bool Press(string text, string tip, params GUILayoutOption[] options) => GUILayout.Button(new GUIContent(text, tip), options);

        private bool Tick(bool value, string text, string tip) => GUILayout.Toggle(value, new GUIContent(text, tip), Styles().Toggle);

        /// <summary>A slider on one line: its name, the slider, its value. What it means is its tooltip.</summary>
        private float Slide(string name, string value, string tip, float now, float least, float most)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(name, tip), Styles().Text, GUILayout.Width(96f));
            GUILayout.BeginVertical();
            GUILayout.Space(7f);
            now = GUILayout.HorizontalSlider(now, least, most);
            GUILayout.EndVertical();
            GUILayout.Label(new GUIContent(value, tip), Styles().Value, GUILayout.Width(78f));
            GUILayout.EndHorizontal();
            return now;
        }

        private void DrawPanel(int id)
        {
            var feel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
            if (Event.current == null || Event.current.type == EventType.Layout)
            {
                shownTube = tubeTest;
                shownAim = aimTest;
                shownDuel = aimTest && duelLab;
                shownTarget = targetTest && !tubeTest && !aimTest;
                shownRecording = recorder != null && recorder.Recording;
                shownFeel = feel != null;
                shownMessage = message ?? "";
            }

            if (!shownFeel)
            {
                feel = null;
            }

            // ---- Always there: where one is, the clock and the recording ----
            var goTo = PlaceBar(shownAim ? (shownDuel ? Place.Duel : Place.Aim) : shownTube ? Place.Tubes : shownTarget ? Place.Targets : Place.Battle);
            if (goTo.HasValue)
            {
                GoTo(goTo.Value);
            }

            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            if (Press(paused ? "Resume" : "Pause", "Stop and start the game's clock (P).")) TogglePause();
            if (Press("Step", "One step of the game, then stopped again (the full stop key).")) StepOnce();
            if (Press("Reset", "The scene set out afresh: every fighter back on its place (R).")) Spawn();
            // (The panel's size, always in reach: smaller and larger.)
            GUILayout.Space(8f);
            if (Press("−", $"The panel and the fighters' labels drawn smaller. Now {uiScale:0.0} times the screen's own size.", GUILayout.Width(30f)))
            {
                uiScale = Mathf.Max(0.8f, uiScale - 0.1f);
                PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            }

            if (Press("+", $"The panel and the fighters' labels drawn larger. Now {uiScale:0.0} times the screen's own size.", GUILayout.Width(30f)))
            {
                uiScale = Mathf.Min(3f, uiScale + 0.1f);
                PlayerPrefs.SetFloat(UiScaleKey, uiScale);
            }

            GUILayout.EndHorizontal();
            float newScale = Slide("Speed", $"{timeScale:0.00}x", "How fast the game's clock runs: 1 is real time, lower is slow motion.", timeScale, 0.05f, 1f);
            if (!Mathf.Approximately(newScale, timeScale))
            {
                timeScale = newScale;
                if (!paused)
                {
                    Time.timeScale = timeScale;
                }
            }

            DrawRecorder();

            // ---- The sections, in a view that scrolls when they are taller than the screen ----
            float room = Screen.height / Mathf.Max(0.1f, uiScale) - panelRect.y - panelChrome - 8f;
            panelView = panelContent < 0f ? Mathf.Max(160f, room) : Mathf.Clamp(panelContent + 4f, 60f, Mathf.Max(160f, room));
            panelScroll = GUILayout.BeginScrollView(panelScroll, false, true, GUILayout.Height(panelView));
            GUILayout.BeginVertical();
            DrawScenes();
            if (shownTube)
            {
                DrawTubeTest();
            }

            if (shownAim && shownDuel)
            {
                DrawDuelLab();
            }
            else if (shownAim)
            {
                DrawAimTest();
            }

            DrawHitsAndHips(feel);
            DrawFallsAndGetUps(feel);
            DrawMoves();
            DrawLegs(feel);
            DrawOther();
            DrawHelp();
            GUILayout.Space(4f);
            GUILayout.EndVertical();
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                panelContent = GUILayoutUtility.GetLastRect().height;
            }

            GUILayout.EndScrollView();

            // ---- Always there: what is tuned kept or not; what the last thing pressed did; what the thing under the mouse does ----
            if (feel != null)
            {
                GUILayout.Space(4f);
                GUILayout.BeginHorizontal();
                // (Counted twice a second, not at every event the panel is drawn for.)
                if (Time.unscaledTime >= tunedCountedAt + 0.5f || Time.unscaledTime < tunedCountedAt)
                {
                    tunedCountedAt = Time.unscaledTime;
                    tunedCount = feel.TunedSinceSaved();
                    keepTuned = PhysicsBattler.Locomotion.CombatFeel.KeepTuned;
                }

                int tuned = tunedCount;
                bool keep = keepTuned;
                bool keepNow = Tick(keep, tuned > 0 ? $"Keep what I tune ({tuned} changed)" : "Keep what I tune", "On: when play stops, every Combat Feel setting changed here is put on the scene's Combat Feel object, so it is there next time (the scene is saved for it, unless it had other unsaved changes). Off: what is tuned is dropped when play stops, as Unity does.");
                if (keepNow != keep)
                {
                    PhysicsBattler.Locomotion.CombatFeel.KeepTuned = keepNow;
                    keepTuned = keepNow;
                    Note(keepNow ? "what is tuned is kept when play stops" : "what is tuned is dropped when play stops");
                }

                if (Press("Back to saved", "Every Combat Feel setting back to how the scene has it saved: as it was when play began.", GUILayout.Width(104f)))
                {
                    int back = feel.BackToSaved();
                    tunedCountedAt = -1f;
                    message = back > 0 ? $"Combat Feel: {back} settings put back to how the scene has them saved." : "Combat Feel: nothing was changed from how the scene has it saved.";
                    Note($"combat feel back to saved: {back} settings");
                }

                GUILayout.EndHorizontal();
            }

            // ---- What the last thing pressed did, and what the thing under the mouse does ----
            // (Always two lines tall: the view above it does not change its height as what it says changes.)
            GUILayout.Space(4f);
            Readout(shownMessage, 2);

            GUILayout.Space(4f);
            GUILayout.BeginVertical(Styles().Box);
            string hint = !string.IsNullOrEmpty(GUI.tooltip) ? GUI.tooltip : !string.IsNullOrEmpty(actionsTip) ? actionsTip : "Point at anything to read here what it does.";
            GUILayout.Label(hint, Styles().Hint, GUILayout.Height(HintHeight));
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        /// <summary>How tall the box of what the thing under the mouse does is: room for four lines.</summary>
        public const float HintHeight = 66f;

        private void DrawScenes()
        {
            if (!Section("Fight and tests", true, "The fight on or off, and the tests that set out their own scene. The walk lab, the target test and the tube test are in the bar at the top."))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            if (Press(fight ? "Stop fighting" : "Fight", "The two sides fight, or stand (F).")) ToggleFight();
            if (Press(turnedAway ? "Face the enemy" : "Turn around", "Every fighter turned about, or back (T).")) TurnAround();
            GUILayout.EndHorizontal();

            Heading("Tests in the battle", "Each sets out its own scene from the battle's fighters.");
            GUILayout.BeginHorizontal();
            if (Press("Bystander (B)", "A blow swung with a third fighter standing in its way.")) BystanderTest();
            if (Press("Launch (L)", "The heaviest armed fighter strikes the lightest enemy, with a third standing where the blow will throw it.")) LaunchTest();
            if (Press("Sever (K)", "A swordsman cuts at an enemy's elbow, and the arm should come off there. Again for another go.")) SeverTest();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Press("Ground (D)", "Two fighters go down in the middle, one knocked down and one killed, and the others walk straight across them for 20 s. Again stops it.")) GroundTest();
            if (Press("Shield (J)", "A sword and a club take turns striking a shield-holder from in front, two blows each. Again for another round.")) ShieldTest();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (Press(galleryRun != null ? "Stop gallery (G)" : "Move gallery (G)", "Every move in turn against a dummy, first slowly and then at full speed. Again ends it.")) Gallery();
            if (Press(rhythmRun != null ? "Stop rhythms (H)" : "Rhythm gallery (H)", "Each fighting style in turn: its base, then every posture, slowed and at full speed. Again ends it.")) RhythmGallery();
            GUILayout.EndHorizontal();

            if (shownTarget)
            {
                // (Each respawns the test with the other body: the attackers with their weapon, a Large's club or a
                // Medium's sword; the targets with what they stand with.)
                Heading("Target test");
                GUILayout.BeginHorizontal();
                if (Press(largeAttackers ? "Attackers: Larges" : "Attackers: Mediums", "Sets the test out again with attackers of the other size: a Large with its club, a Medium with its sword."))
                {
                    largeAttackers = !largeAttackers;
                    TargetTest();
                }

                if (Press(largeTargets ? "Targets: Larges" : "Targets: Mediums", "Sets the test out again with targets of the other size."))
                {
                    largeTargets = !largeTargets;
                    TargetTest();
                }

                GUILayout.EndHorizontal();
                bool harmed = Tick(targetsHarmed, "Targets take harm", "Off: the targets can't be hurt or cut, so a test can go on as long as it is wanted.");
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
        }

        private void DrawAimTest()
        {
            if (!Section("Aim test", true, "One blow asked for one part of the target. The part the blade lands on turns red; each blow is measured below and in the log."))
            {
                return;
            }

            var feel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<CombatFeel>());
            if (feel != null)
            {
                Heading("The blow");
                // (Three ways to make the blow: made from the recordings, the first built blows, the strike clips as they are.)
                int way = feel.aimTestAttacksAreMade ? 0 : feel.aimTestBlowsAreBuilt ? 1 : 2;
                if (Press("Blows: " + (way == 0 ? "made from the recordings" : way == 1 ? "built (the first try: arm and trunk)" : "the strike clips as they are"),
                    "How the swordsman's blows are made. Made from the recordings: a recorded strike taken apart into each body part's own track, changed by the style and the aim, and put together again, so the whole body moves as the recording's does and the blade lands on the part asked for. Built: the first try, the sword arm and the trunk worked out in code, the feet still. The strike clips: the recordings as they are, the nearest picked. Sets the test out again."))
                {
                    way = (way + 1) % 3;
                    feel.aimTestAttacksAreMade = way == 0;
                    feel.aimTestBlowsAreBuilt = way != 2;
                    AttackMaker.On = way == 0;
                    BuiltBlow.On = way != 2;
                    aimDistance = way == 0 ? AimDistanceMade : way == 1 ? AimDistanceBuilt : AimDistanceAtFirst;
                    Note(way == 0 ? "aim test: attacks made from the recordings" : way == 1 ? "aim test: built blows" : "aim test: strike clips");
                    AimTestScene();
                }

                if (feel.aimTestAttacksAreMade)
                {
                    DrawMadeAttacks(feel);
                }

                if (feel.aimTestBlowsAreBuilt && !feel.aimTestAttacksAreMade)
                {
                    GUILayout.BeginHorizontal();
                    if (Press("Line: " + (Aim.Line.HasValue ? BuiltBlow.LineName(Aim.Line.Value) : "chosen for the part"), "The line the blade comes in on, as the swordsman sees it. Chosen for the part: of the lines that part may be cut on, the one his arm makes best from where he stands. Press to go through the eight."))
                    {
                        Aim.Line = !Aim.Line.HasValue ? CutLine.Down : Aim.Line.Value == CutLine.Up ? (CutLine?)null : Aim.Line.Value + 1;
                    }

                    if (Press("Size: " + (!Aim.Size.HasValue ? "chosen" : Aim.Size.Value == CutSize.Wrist ? "wrist cut" : "elbow cut"), "A wrist cut is the quickest and lightest: it lands in about a quarter of a second. An elbow cut swings the forearm: about 0.4 s, and harder. Chosen: the wrist for hands and forearms, the elbow for the rest."))
                    {
                        Aim.Size = !Aim.Size.HasValue ? CutSize.Wrist : Aim.Size.Value == CutSize.Wrist ? CutSize.Elbow : (CutSize?)null;
                    }

                    GUILayout.EndHorizontal();
                    feel.blowsUseTheTrunk = Tick(feel.blowsUseTheTrunk, "The trunk in the blow", "The trunk turns away as the blow is wound up and into it before the arm goes, and leans in for reach. Off: the arm alone, the body as it stood; a blow then reaches only an arm and a blade's length, so bring the two to about 0.75 m to see it land.");
                    feel.blowsArePushed = Mathf.Round(10f * Slide("Pushed", $"{feel.blowsArePushed:0.0}", "How far a blow is pushed past the fencing book: wound further back, a stillness at the top, carried further through. 0 is the book's blow. Where and when it lands does not change.", feel.blowsArePushed, 0f, 1f)) / 10f;
                }
            }

            Heading("Strike at", "Press a part and the swordsman strikes at it. Left and right are the target's own, so its right side is on your left as you face it.");
            AimRow(AimSpot.Head);
            AimRow(AimSpot.Neck);
            AimRow(AimSpot.ShoulderR, AimSpot.Chest, AimSpot.ShoulderL);
            AimRow(AimSpot.UpperArmR, AimSpot.Belly, AimSpot.UpperArmL);
            AimRow(AimSpot.ForearmR, AimSpot.Hips, AimSpot.ForearmL);
            AimRow(AimSpot.HandR, null, AimSpot.HandL);
            AimRow(AimSpot.ThighR, AimSpot.ThighL);
            AimRow(AimSpot.ShinR, AimSpot.ShinL);
            AimRow(AimSpot.FootR, AimSpot.FootL);
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            if (Press("Again", $"Another blow at the {AimSpots.Name(aimSpot)}.")) AimAt(aimSpot);
            if (Press(Aim.Waiting > 0 ? $"Stop ({Aim.Waiting} to go)" : "Strike them all", "Every part in turn, one blow each, a second apart. Again stops it."))
            {
                if (Aim.Waiting > 0)
                {
                    Aim.Stop();
                }
                else
                {
                    Aim.OrderAll();
                    message = "Aim test: every part in turn.";
                }
            }

            GUILayout.EndHorizontal();

            Heading("The target");
            if (Press("Holds: " + AimKits[aimTargetKit], "What the target stands with: nothing (it does not block), sword and shield (it blocks), a sword (it parries). Sets the test out again."))
            {
                aimTargetKit = (aimTargetKit + 1) % AimKits.Length;
                AimTestScene();
            }

            float distance = Mathf.Round(20f * Slide("Distance", $"{aimDistance:0.00} m", "How far the swordsman stands from the target, middle to middle. 1.4 m is about where today's strikes land. Both walk to their new places.", aimDistance, 0.6f, 2.6f)) / 20f;
            float turned = Mathf.Round(Slide("Turned", $"{aimTurned:0}°", "How the target stands turned: 0 it faces the swordsman, 90 it shows him its left side, 180 its back, -90 its right side.", aimTurned, -180f, 180f) / 15f) * 15f;
            if (!Mathf.Approximately(distance, aimDistance) || !Mathf.Approximately(turned, aimTurned))
            {
                aimDistance = distance;
                aimTurned = turned;
                PlaceAim();
            }

            Heading("The last blow", "What the blade met first, how near its striking part passed to the spot asked for, how far its edge was turned off the way it was going, how far the blade as seen was from where its pose had it, and whether the swordsman's weight stayed over his feet. Written to the log as an aim line.");
            // (Both lines always drawn: a line that comes and goes between a frame's two passes upsets the panel.)
            GUILayout.Label(aimLast.Length > 0 ? aimLast : "None yet.", Styles().Text);
            GUILayout.Label(Aim.Blows.Count > 0 ? "So far: " + Aim.Tally() + "." : "", Styles().Text);
        }

        private bool aimStyleOpen;

        /// <summary>The aim test's made attacks: which blow, on which legs, when it lands, and the style it is made in.</summary>
        private void DrawMadeAttacks(CombatFeel feel)
        {
            GUILayout.BeginHorizontal();
            if (Press("Blow: " + AimAttacks[aimAttack].name, "Which recorded blow the attack is made from. The best fit: of the three plain ones (forehand cut, rising backhand, thrust), the one whose blade comes onto the part with the least change. The leaping and hopping ones are only made when named here."))
            {
                aimAttack = (aimAttack + 1) % AimAttacks.Length;
            }

            if (Press("Legs: " + AimLegs[aimLegs].name, "The legs the blow is put on. Its own: the legs it was recorded with. Another's: that recording's hips, legs, steps and travel under this blow's chest and arms (a thrust on the forehand's lunge is a lunging thrust; a forehand on the thrust's legs is a cut from where it stands). The best fit: whichever brings the blade to the part with the least change, its own first."))
            {
                aimLegs = (aimLegs + 1) % AimLegs.Length;
            }

            GUILayout.EndHorizontal();
            Aim.Attack = AimAttacks[aimAttack].clip;
            Aim.Legs = AimLegs[aimLegs].clip;
            Aim.LandIn = Mathf.Round(20f * Slide("Lands in", Aim.LandIn > 0.01f ? $"{Aim.LandIn:0.00} s" : "its own time", "When the blade is to land, counted from the press. Its own time: as the recording has it (a forehand cut about 0.6 s, a rising backhand 0.8 s, a thrust 0.45 s). Set: the wind-up is stretched or shortened so the blade lands then (between a third and three times its own length); the drive itself is not changed.", Aim.LandIn, 0f, 1.6f)) / 20f;

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Style", "The same blow as different fighters would throw it. Each button sets all the levers below; then press a part."), Styles().Text, GUILayout.Width(40f));
            foreach (var preset in AttackStyle.Presets)
            {
                if (Press(preset.name, preset.name == "Plain" ? "The recording's own: every lever at 1." : preset.name == "Heavy" ? "Wound far back and slowly, thrown from the hips, deep in the lunge, carried all the way through." : preset.name == "Quick" ? "Little wind-up, a short fast blow from the guard, back at once." : preset.name == "Brawler" ? "Big arm swings on a quiet body: the arm does the work, the hips little." : preset.name == "Low" ? "Crouched, deep in the knees, leaning in." : "The hips well ahead and the arm late: the body cracks like a whip."))
                {
                    feel.SetAttackStyle(preset.style);
                    Note("aim test: attack style " + preset.name);
                }
            }

            GUILayout.EndHorizontal();
            if (Press((aimStyleOpen ? "−  " : "+  ") + "Style levers", "Each lever is one part of the body in one phase of the blow. 1 is the recording's own. They are the scene's Combat Feel fields (Attack ...), so they can be set there as well."))
            {
                aimStyleOpen = !aimStyleOpen;
            }

            if (aimStyleOpen)
            {
                feel.attackWindUp = Lever("Wind-up", feel.attackWindUp, 0f, 2f, "How far the body is wound up before the drive: the hips' turn and the chest's twist away from the blow.");
                feel.attackArmSwing = Lever("Arm swing", feel.attackArmSwing, 0.4f, 1.4f, "How far the sword arm travels from the guard as it is drawn back and as the blow is carried through. At the landing it is the recording's. Past about 1.3 it asks the arm further round than a shoulder goes.");
                feel.attackHipDrive = Lever("Hip drive", feel.attackHipDrive, 0.4f, 1.6f, "How far the hips turn through the blow.");
                feel.attackTwist = Lever("Twist", feel.attackTwist, 0.4f, 1.6f, "How far the chest twists on the hips.");
                feel.attackLean = Lever("Lean", feel.attackLean, 0f, 2f, "How far the chest leans through the blow.");
                feel.attackLunge = Lever("Lunge", feel.attackLunge, 0.4f, 1.8f, "How far the body travels, its steps with it. An aimed attack sets its own travel to reach, so here this shows only where the reach runs out.");
                feel.attackDrop = Lever("Drop", feel.attackDrop, 0f, 2f, "How far the hips sink through the blow, as the front knee takes the weight.");
                feel.attackCrouch = Lever("Crouch", feel.attackCrouch, 0f, 0.2f, "How much lower the hips are carried through the whole attack (leg lengths).");
                feel.attackFollowThrough = Lever("Carry-through", feel.attackFollowThrough, 0.3f, 1.6f, "How far the hips and chest carry on round after the blade has landed.");
                feel.attackWindUpTime = Lever("Wind-up time", feel.attackWindUpTime, 0.5f, 2f, "How long the wind-up takes.");
                feel.attackDriveTime = Lever("Drive time", feel.attackDriveTime, 0.6f, 2f, "How long the drive and the carry-through take: more is a heavier, slower blade.");
                feel.attackRecoveryTime = Lever("Back time", feel.attackRecoveryTime, 0.5f, 2f, "How long the way back to the guard takes.");
                feel.attackHipLead = Lever("Hips lead", feel.attackHipLead, -0.08f, 0.08f, "How much sooner the legs and hips go in the drive (seconds).");
                feel.attackArmLag = Lever("Arm lags", feel.attackArmLag, -0.08f, 0.08f, "How much later the sword arm goes in the drive (seconds).");
                AttackMaker.Style = feel.AttackStyleNow;
            }
        }

        private float Lever(string name, float now, float least, float most, string tip) => Mathf.Round(100f * Slide(name, $"{now:0.00}", tip, now, least, most)) / 100f;

        /// <summary>A row of the map of what the duel lab's mover leaves open: each spot's name, green where it is open, grey with what covers it where it is not.</summary>
        private void OpenRow(params AimSpot[] spots)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var was = GUI.color;
            foreach (var spot in spots)
            {
                string name = AimSpots.Name(spot), behind = "";
                bool open = true, known = false;
                foreach (var seen in Duel.Open)
                {
                    if (seen.Spot == spot)
                    {
                        open = seen.Open;
                        behind = seen.Behind;
                        known = true;
                    }
                }

                GUI.color = !known ? Color.gray : open ? new Color(0.45f, 1f, 0.45f) : new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label(new GUIContent(open ? name : name + " (" + behind.Replace("its ", "") + ")", open ? "Open: a straight line from the other's sword shoulder reaches it." : "Covered: " + behind + " is before it."), Styles().Text, GUILayout.Width(AimButtonWidth + 26f));
            }

            GUI.color = was;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void AimRow(AimSpot? left, AimSpot? middle = null, AimSpot? right = null)
        {
            GUILayout.BeginHorizontal();
            // (One spot: in the middle. Two: side by side. Three: across.)
            if (!middle.HasValue && !right.HasValue)
            {
                GUILayout.FlexibleSpace();
                AimButton(left);
                GUILayout.FlexibleSpace();
            }
            else if (!right.HasValue)
            {
                GUILayout.FlexibleSpace();
                AimButton(left);
                AimButton(middle);
                GUILayout.FlexibleSpace();
            }
            else
            {
                AimButton(left);
                if (middle.HasValue)
                {
                    AimButton(middle);
                }
                else
                {
                    GUILayout.Space(AimButtonWidth + 4f);
                }

                AimButton(right);
            }

            GUILayout.EndHorizontal();
        }

        private const float AimButtonWidth = 96f;

        private void AimButton(AimSpot? spot)
        {
            if (!spot.HasValue)
            {
                return;
            }

            string name = AimSpots.Name(spot.Value);
            name = char.ToUpper(name[0]) + name.Substring(1);
            if (GUILayout.Button(new GUIContent(spot.Value == aimSpot ? "> " + name : name, $"One blow at the target's {AimSpots.Name(spot.Value)}."), GUILayout.Width(AimButtonWidth)))
            {
                AimAt(spot.Value);
            }
        }

        private void DrawTubeTest()
        {
            if (!Section("Tube test", true, "The targets struck by tubes: falls and blows on demand, and the tube's own settings."))
            {
                return;
            }

            Heading("Falls on demand", "The targets are turned so the tube sends them the way asked, and they go down whatever their legs say. Each fall is measured in the log: what landed first, the head, the hips.");
            GUILayout.BeginHorizontal();
            if (Press("Back", "Struck on the front with 200 N·s: they fall backward.")) FallTest(0f, false);
            if (Press("Forward", "Struck on the back with 200 N·s: they fall forward.")) FallTest(180f, false);
            if (Press("Left", "Struck on the right side with 200 N·s: they fall to their left.")) FallTest(270f, false);
            if (Press("Right", "Struck on the left side with 200 N·s: they fall to their right.")) FallTest(90f, false);
            GUILayout.EndHorizontal();
            if (Press("Thrown back (450)", "Struck on the front with 450 N·s, a Large's club at its hardest: thrown off their feet.")) FallTest(0f, true);

            Heading("A fall, then a turn round", "While the targets are down, what they stand facing is put behind them, so once up they turn round to it: the get-up into a turn, and the swordsman's sword through it.");
            GUILayout.BeginHorizontal();
            if (Press("Back, +170", "They fall backward, and get up to turn round 170 degrees to their right.")) FallTest(0f, false, 170f);
            if (Press("Back, −170", "They fall backward, and get up to turn round 170 degrees to their left.")) FallTest(0f, false, -170f);
            if (Press("Thrown", "They are thrown back, and get up to turn round.")) FallTest(0f, true, 170f);
            GUILayout.EndHorizontal();

            Heading("Blows on demand", "A tube taken as a blow of that size, from the side the targets are turned to. Their legs decide what comes of it: a recoil, a stumble, a fall.");
            GUILayout.BeginHorizontal();
            if (Press("Cut (60)", "60 N·s: a little more than a Medium's sword cut.")) BlowTest(60f);
            if (Press("Heavy (200)", "200 N·s: a Large's club.")) BlowTest(200f);
            if (Press("Club (600)", "600 N·s: more than any fighter strikes with.")) BlowTest(600f);
            GUILayout.EndHorizontal();
            if (Press("A fall to catch (165)", "165 N·s: only just more than a Medium's legs catch, so the fall is one that can be caught on a knee and a hand.")) BlowTest(165f);

            Heading("The tube", "What the Hit button and the 6 key send. The buttons above set these themselves.");
            GUILayout.BeginHorizontal();
            if (Press("Hit (6)", "Sends the waiting tubes as they are set below.")) TubeHit();
            if (Press(largeTargets ? "Targets: Larges" : "Targets: Mediums", "Sets the test out again with targets of the other size."))
            {
                largeTargets = !largeTargets;
                TubeTestScene();
            }

            GUILayout.EndHorizontal();
            float tubeWas = Tubes.Impulse + 31f * Tubes.Speed + 977f * Tubes.Height + 7919f * Tubes.Reach + 104729f * Tubes.Turned + (Tubes.AsBlow ? 0.25f : 0f) + (Tubes.Fells ? 0.5f : 0f);
            Tubes.Impulse = Mathf.Round(Slide("How hard", $"{Tubes.Impulse:0} N·s", $"The tube's momentum: now a tube of {Tubes.Mass:0.0} kg. A Medium's sword cut is about 45 N·s, a Large's club 100 to 300.", Tubes.Impulse, 5f, 600f));
            Tubes.Speed = Mathf.Round(2f * Slide("How fast", $"{Tubes.Speed:0.0} m/s", "Slower is heavier for the same hit: more of a shove, less of a crack.", Tubes.Speed, 2f, 25f)) / 2f;
            Tubes.Height = Mathf.Round(100f * Slide("How high", $"{Tubes.Height * 100f:0} cm", "How far over the ground the tube flies: 130 cm is a Medium's chest.", Tubes.Height, 0.1f, 2.4f)) / 100f;
            Tubes.Reach = Mathf.Round(100f * Slide("Reach", $"{Tubes.Reach * 100f:0} cm", "Where the tube's end is against the target's middle. Past it (more than 0) the tube meets more of the body; short of it, it clips the near edge or misses.", Tubes.Reach, -0.6f, 0.6f)) / 100f;
            Tubes.Turned = Mathf.Round(Slide("Turned", $"{Tubes.Turned:0}°", $"How the targets stand turned to the tube: 0 struck on the front, 90 on the left side, 180 on the back, 270 on the right side. Now: struck on {Tubes.Side}.", Tubes.Turned, 0f, 355f) / 5f) * 5f;
            Tubes.AsBlow = Tick(Tubes.AsBlow, "Taken as a blow", "The tube is also a blow of the fight's: the target recoils, stumbles or falls as its legs decide. Off: the physics only.");
            Tubes.Fells = Tick(Tubes.Fells, "The target goes down", "The target falls whatever its legs say: the fall test.");
            if (!Mathf.Approximately(tubeWas, Tubes.Impulse + 31f * Tubes.Speed + 977f * Tubes.Height + 7919f * Tubes.Reach + 104729f * Tubes.Turned + (Tubes.AsBlow ? 0.25f : 0f) + (Tubes.Fells ? 0.5f : 0f)))
            {
                // (Set here by hand: remembered for the next session.)
                RememberTubes();
            }
        }

        private void DrawHitsAndHips(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel == null || !Section("Hits and hips", false, "How hard a blow throws the body about, and how the hips are held. Kept on the scene's Combat Feel."))
            {
                return;
            }

            // (The scene's Combat Feel sets these every frame, so the panel changes them there.)
            feel.struckPartsFlashRed = Tick(feel.struckPartsFlashRed, "A struck part turns red", "The part of a body a blow lands on turns red and fades back over a second: the part really struck, not the part aimed at. A blow on a shield or a sword tints nothing. Off: no tint, as before.");
            feel.hitForce = Mathf.Round(4f * Slide("Hit force", $"{feel.hitForce:0.00}x", "An extra push with every blow that lands: the blow's own momentum times this, on the part struck and a share on the parts joined to it. 0: only the weapon's touch.", feel.hitForce, 0f, 6f)) / 4f;
            bool held = Tick(feel.hipsAreHeldNotLocked, "Hips held by a spring", "The hips are held to the pose by a spring, so a blow can move them. Off: locked to the pose, as before 14 November.");
            if (held != feel.hipsAreHeldNotLocked)
            {
                feel.hipsAreHeldNotLocked = held;
                Note(held ? "hips held, not locked" : "hips locked, as before");
            }

            feel.hipHoldQuickness = Mathf.Round(2f * Slide("Hold: quick", $"{feel.hipHoldQuickness:0.0} /s", "How quickly the hips come back to the pose, in swings a second. Lower is looser.", feel.hipHoldQuickness, 2f, 12f)) / 2f;
            feel.hipHoldStrength = Mathf.Round(4f * Slide("Hold: strong", $"{feel.hipHoldStrength:0.00}x", "The most the hold pushes with, in times the body's weight.", feel.hipHoldStrength, 0.5f, 8f)) / 4f;
        }

        private void DrawFallsAndGetUps(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (!Section("Falls and getting up", false, "The new falls and get-ups, each with a switch back to how it was."))
            {
                return;
            }

            if (feel != null)
            {
                // (The scene's Combat Feel sets these every frame, so the panel changes them there.)
                Heading("Falls");
                bool oneBody = Tick(feel.fallsKeepTheBody, "Falls in the one body", "A fighter falls in the body it stands in, its muscles still working. Off: a ragdoll, as before 14 November.");
                if (oneBody != feel.fallsKeepTheBody)
                {
                    feel.fallsKeepTheBody = oneBody;
                    Note(oneBody ? "falls: in the one body" : "falls: the ragdoll, as before");
                }

                feel.fallMuscle = Mathf.Round(20f * Slide("Muscle", $"{feel.fallMuscle:0.00}", "How strong a falling body's muscles are, as a share of a person's strength. 0 is limp.", feel.fallMuscle, 0f, 2f)) / 20f;
                feel.fallsProtectTheHead = Tick(feel.fallsProtectTheHead, "The head kept off the ground", "Chin to the chest going backward, the head lifted going forward, tipped away going to a side.");
                feel.fallsReachForTheGround = Tick(feel.fallsReachForTheGround, "The arms reach for the ground", "The arms go out toward the ground the body is falling to. Off: held a little out from the sides.");
                feel.fallsHandsStayPlanted = Tick(feel.fallsHandsStayPlanted, "A hand that lands stays put", "A hand that reaches the ground is held where it landed while the body comes down over it, and the arm bends under it. Off: it skates over the ground, as before.");
                feel.fallHandHold = Mathf.Round(20f * Slide("Hand grip", $"{feel.fallHandHold:0.00}x", "How hard a landed hand grips the ground, in times the body's weight. Pulled harder it is dragged and brakes the body. Higher stops the body shorter; lower lets it slide on; 0 does not hold.", feel.fallHandHold, 0f, 3f)) / 20f;
                feel.fallsArmsStayOutOfTheBody = Tick(feel.fallsArmsStayOutOfTheBody, "Arms and shield stay out of the body", "Falling and lying, its forearms, hands and shield are stopped by its own trunk, and its hands are posed beside or before it, never through it. Off: as before, an arm it came down on lay inside it. Best seen with the Larges falling forward or to a side.");
                feel.fallsShieldKeptBeforeTheChest = Tick(feel.fallsShieldKeptBeforeTheChest, "The shield kept before the chest", "Falling, the shield arm stays across the front of the body, so it comes down on its shield or under it. Off: as before, the shield arm reaches for the ground, the board's edge lands first and the body is held up on it and rolls off. Look at the middle target (sword and shield).");
                feel.ragdollsHoldSolidThings = Tick(feel.ragdollsHoldSolidThings, "A ragdoll's sword and shield lie on the ground", "A fighter with a limb cut off falls as a ragdoll; what it holds is then stopped by the ground and lies there with it. Off: as before, its sword hangs through the ground. Seen in the fight, on the maimed and the dead.");
                feel.fallenBodiesSettle = Tick(feel.fallenBodiesSettle, "A body that is down settles", "Once its hips are down its muscles stop reaching and only hold the pose it lies in, so it comes to rest. Off: it goes on reaching, and on its side rocks back and forth, as before.");
                feel.thrownBodiesTuck = Tick(feel.thrownBodiesTuck, "A thrown body tucks", "Thrown off its feet (the Thrown back button, the Club blow) it curls up as it flies and tumbles, and stays down longer. Off: it falls as one knocked over does.");
                feel.fallsCanBeCaught = Tick(feel.fallsCanBeCaught, "A fall can be caught", "A fall only just more than the legs catch ends on a knee and a hand, or sitting, and the fighter rises from there.");
            }

            Heading("Getting up");
            bool natural = feel != null ? feel.getUpsAreNatural : PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps;
            bool naturalNow = Tick(natural, "The new get-ups", "From how it really lies, by one of three routes, turned toward its enemy. Off: as before 14 November.");
            if (naturalNow != natural)
            {
                if (feel != null)
                {
                    feel.getUpsAreNatural = naturalNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps = naturalNow;
                Note(naturalNow ? "get-ups: the new ones" : "get-ups: as before");
            }

            bool recorded = feel != null ? feel.getUpsAreFromTheRecordings : PhysicsBattler.Motion.GetUp.FromRecordings;
            bool recordedNow = recorded;
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("The way up:", "As recorded (new): the way a recorded man gets up, read off the recording and fitted to this fighter (from his back he sits up and rocks forward over his feet; from his front he pushes up, lifts his hips and steps his feet in). Made by hand (old): the get-ups as before 17 November. To see either: Falls on demand above (Back, Forward, Left, Right) in the target test."), GUILayout.Width(80f));
            if (Press(recorded ? "[as recorded]" : "as recorded", "The get-ups read off the recordings.")) recordedNow = true;
            if (Press(!recorded ? "[made by hand (old)]" : "made by hand (old)", "The get-ups made by hand, as before 17 November.")) recordedNow = false;
            GUILayout.EndHorizontal();
            if (recordedNow != recorded)
            {
                if (feel != null)
                {
                    feel.getUpsAreFromTheRecordings = recordedNow;
                }

                PhysicsBattler.Motion.GetUp.FromRecordings = recordedNow;
                Note(recordedNow ? "get-ups: as recorded" : "get-ups: made by hand");
            }

            if (feel != null)
            {
                feel.recordedGetUpTakes = Lever("Recorded get-up takes", feel.recordedGetUpTakes, 0.4f, 1f, "How long a recorded get-up is given, as a share of the recording's own time (3.7 s from his back, 4.5 s from his front). Lower: up sooner. 1: the recorded man's own pace.");
                PhysicsBattler.Motion.GetUp.RecordedTakes = feel.recordedGetUpTakes;
            }

            bool clear = feel != null ? feel.getUpLegsClearTheHips : PhysicsBattler.Motion.GetUp.LegsClearTheHips;
            bool clearNow = Tick(clear, "The legs keep clear of the hips", "The hips are lifted before the leg goes back under them. Off: as before, the knees swinging round through the trunk.");
            if (clearNow != clear)
            {
                if (feel != null)
                {
                    feel.getUpLegsClearTheHips = clearNow;
                }

                PhysicsBattler.Motion.GetUp.LegsClearTheHips = clearNow;
                Note(clearNow ? "get-ups: the legs clear of the hips" : "get-ups: the legs as before");
            }

            bool swordUp = Tick(PhysicsBattler.Locomotion.ClipFighter.SwordTakenUpWhereItIs, "The sword taken up where it is", "Once up, the sword goes to its guard from where the arm is, and a guard turns round the way asked. Off: as before, the arm reaching back for where it stood before the fall.");
            PhysicsBattler.Locomotion.ClipFighter.SwordTakenUpWhereItIs = swordUp;
            PhysicsBattler.Locomotion.Locomotor.GuardTurnsTheWayAsked = swordUp;
        }

        private void DrawMoves()
        {
            if (!Section("Moves on demand", false, "Something every standing fighter does now, to look at it."))
            {
                return;
            }

            GUILayout.BeginHorizontal();
            if (Press("Step back (2)", "Every fighter standing in its guard steps back from its opponent, as it does to a blow read at the last moment. Best in the target test, slowed down.")) StepBackTest();
            if (Press("Turn (3)", "Every fighter standing still turns a quarter turn and a bit to its right, and the fight turns it back. Best in the target test, slowed down.")) TurnTest();
            if (Press("Knock a leg (4)", "Every fighter's right shin knocked sideways: a small twitch. Needs physical legs.")) KnockLegTest();
            GUILayout.EndHorizontal();
        }

        private void DrawLegs(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (!Section("Legs and feet", false, "The physical legs and how the feet step, each with a switch back to how it was."))
            {
                return;
            }

            // (The scene's Combat Feel sets these every frame, so the toggles change them there.)
            bool legs = feel != null ? feel.legsArePhysical : PhysicsBattler.Locomotion.ClipFighter.PhysicalLegs;
            bool legsNow = Tick(legs, "Physical legs", "The legs are part of the physical body. Off: they go where the animation puts them, as before 14 November.");
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

            bool ball = feel != null ? feel.feetPivotOnTheBall : PhysicsBattler.Locomotion.LocoBody.PivotsOnBall;
            bool ballNow = Tick(ball, "Planted feet pivot on the ball", "A planted foot turns on its ball as the body turns over it. Off: as before.");
            if (ballNow != ball)
            {
                if (feel != null)
                {
                    feel.feetPivotOnTheBall = ballNow;
                }

                PhysicsBattler.Locomotion.LocoBody.PivotsOnBall = ballNow;
            }

            bool hops = feel != null ? feel.stepBackHops : PhysicsBattler.Locomotion.ClipFighter.StepBackHops;
            bool hopsNow = Tick(hops, "A step back is a hop", "Both feet leave the ground in a step back. Off: as before.");
            if (hopsNow != hops)
            {
                if (feel != null)
                {
                    feel.stepBackHops = hopsNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.StepBackHops = hopsNow;
            }
        }

        private void DrawOther()
        {
            if (!Section("Other switches", false, "Tiredness, the camera's shake, the weapon arm."))
            {
                return;
            }

            bool tired = Tick(tiredAll, "Everyone tired", "Every fighter's Fight Rhythm energy held at 25%.");
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

            OrbitCamera.ShakeOnHits = Tick(OrbitCamera.ShakeOnHits, "Camera shake on heavy hits", "The camera is shaken by a heavy blow that lands.");
            // Milestone 10 (the arm rebuild's step 6): the weapon arm on joint motors, or on the world servo as before.
            bool muscles = Tick(PhysicsBattler.Authority.AuthorityController.ArmMuscles, "Weapon arm on joint motors", "Off: the weapon arm is pulled to its pose, as before the arm was rebuilt.");
            if (muscles != PhysicsBattler.Authority.AuthorityController.ArmMuscles)
            {
                PhysicsBattler.Authority.AuthorityController.ArmMuscles = muscles;
                Note("weapon arm: " + (muscles ? "joint motors" : "pulled to its pose"));
            }
        }

        private void DrawHelp()
        {
            if (!Section("Keys and camera", false, "The keys, the camera, and what the engine's joints were found to do."))
            {
                return;
            }

            GUILayout.Label($"Panel size {uiScale:0.0}x: the − and + at the top make it smaller and larger.", Styles().Text);
            Heading("Keys");
            GUILayout.Label("R reset · F fight · T turn around\nP pause · . step · M mark the log\n1 targets · 5 tubes · 6 hit · 7 aim (the walk lab: the bar at the top)\n2 step back · 3 turn · 4 knock a leg\nB bystander · L launch · K sever · D ground · J shield\nG move gallery · H rhythm gallery", Styles().Text);
            Heading("Camera");
            GUILayout.Label("Right-drag to orbit, middle-drag to pan, scroll to zoom.", Styles().Text);
            Heading("Joint probe");
            GUILayout.Label(JointConventions.Report, Styles().Text);
        }

        private void DrawRecorder()
        {
            if (recorder == null)
            {
                return;
            }

            GUILayout.BeginHorizontal();
            if (shownRecording)
            {
                GUILayout.Label(new GUIContent($"Rec {recorder.EventCount}", $"Recording to PlaytestLogs/{recorder.SessionName}: {recorder.EventCount} events, {recorder.MarkCount} marks."), Styles().Text, GUILayout.Width(84f));
                if (Press("Mark (M)", "A mark put in the log at this moment, to find it by.")) Mark();
                if (Press("New log", "This log closed and a new one begun.")) recorder.NewSession();
                if (Press("Stop", "The recording stopped.")) recorder.StopSession();
            }
            else
            {
                GUILayout.Label(new GUIContent("Not recording", "No playtest log is being written."), Styles().Text, GUILayout.Width(96f));
                if (Press("Start recording", "A playtest log begun in PlaytestLogs.")) recorder.StartSession();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>A line more under a unit's label, for a test that wants what is going on shown over the fighters as it happens (the shield test).</summary>
        public static readonly Dictionary<UnitController, string> LabelNotes = new Dictionary<UnitController, string>();

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
                if (LabelNotes.TryGetValue(unit, out string note) && note.Length > 0)
                {
                    label += "\n" + note;
                }

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
            // (Tall enough for its lines, growing upward so it stays over the head; wide enough for a test's note.)
            float tall = Mathf.Max(52f, 17f * label.Split('\n').Length + 4f);
            GUI.Label(new Rect(screen.x / scale - 70f, (Screen.height - screen.y) / scale + 4f - tall, 280f, tall), label);
        }
    }
}
