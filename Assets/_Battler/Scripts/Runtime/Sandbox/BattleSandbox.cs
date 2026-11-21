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

        /// <summary>
        /// A floor past the floor's edge. The scene's floor is 40 m square, and a pair that is bashed back again and
        /// again walks to its edge in a long session: on 19 November (his log of 20:10) a target was thrown off it three
        /// times, fell 20 to 34 m, and was put back on its pose from there (parts at 55 to 82 m/s in that get-up). An
        /// unseen floor a millimetre lower carries on from the edge, with the floor's own layer and surface.
        /// </summary>
        private static void EnsureFloorPastTheEdge()
        {
            const string name = "Ground (past the edge)";
            if (GameObject.Find(name) != null)
            {
                return;
            }

            var ground = GameObject.Find("Ground");
            var floor = ground != null ? ground.GetComponent<Collider>() : null;
            var far = new GameObject(name);
            far.layer = ground != null ? ground.layer : Layers.Index(Layers.Ground);
            far.transform.position = new Vector3(0f, (floor != null ? floor.bounds.max.y : 0f) - 0.501f, 0f);
            var box = far.AddComponent<BoxCollider>();
            box.size = new Vector3(4000f, 1f, 4000f);
            if (floor != null)
            {
                box.sharedMaterial = floor.sharedMaterial;
            }
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            EnsureFloorPastTheEdge();

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

        /// <summary>Says whether a lab of this name is one he is asked to go to (the panel's "look here"): its button in the bar gets a frame.</summary>
        public static System.Func<string, bool> PlaceLooked;

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

                if (PlaceLooked != null && PlaceLooked(name))
                {
                    Frame(GUILayoutUtility.GetLastRect());
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
            PhysicsBattler.Locomotion.ClipFighter.GetUpBegan += OnGetUpBegan;
            HitFlashes.Ensure();
        }

        private void OnDisable()
        {
            UnitController.Hit -= OnHit;
            PhysicsBattler.Locomotion.ClipFighter.GetUpBegan -= OnGetUpBegan;
            SandboxInput.OverPanel = false;
            SandboxInput.Typing = false;
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
                BeginRow();
                Words(new GUIContent("Shield:", "How a fighter with a shield carries it (Combat Feel \"The Shield Is Held In Cover\"). Held in cover (new): the board is held before his chest, looking at the enemy, whatever he is doing: walking, a gesture of the sword arm, a blow of his own. It goes with the body and gives a little, but stays in place, and leaves only to meet a blow. The recording's (old): the shield arm does what each recording has it do: it swung open in every cut and every fake of one."), Styles().Text, GUILayout.Width(80f));
                if (Press(coverFeel.theShieldIsHeldInCover ? "[held in cover]" : "held in cover", "The board is held before his chest.")) coverFeel.theShieldIsHeldInCover = true;
                if (Press(!coverFeel.theShieldIsHeldInCover ? "[the recording's (old)]" : "the recording's (old)", "The shield arm does what each recording has it do.")) coverFeel.theShieldIsHeldInCover = false;
                EndRow();
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

            BeginRow();
            Words(new GUIContent("Actions:", "Where the actions are: the buttons that have a fighter do something (move, leap, posture, bash, strike, the tests, the dance), the pace, and each action's own settings. In a window (new): a window of their own at the right, the pace at its top, each action's settings folded under its buttons. In the panel (old): all of it here."), Styles().Text, GUILayout.Width(80f));
            if (Press(ActionsApart ? "[in a window]" : "in a window", "The buttons in a window of their own.")) ActionsApart = true;
            if (Press(!ActionsApart ? "[in the panel]" : "in the panel", "The buttons among the settings here, as they were.")) ActionsApart = false;
            EndRow();
            var mover = Duel.Mover;
            var style = mover != null && mover.Clip != null && mover.Clip.Loco != null ? mover.Clip.Loco.Style : null;
            Words(new GUIContent("Style: " + (style != null ? style.Name : "none") + (style != null ? $" (the pack \"{style.Prefix}\")" : ""),
                "The movement style the swordsman's guard moves in: a pack of recordings, each with a job in the walk (standing, walking forward and back, a step to each side, running, turning). The standard style is the sword and shield pack's guard, as the fight has moved until now."), Styles().Text);
            BeginRow();
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

            EndRow();
            if (duelCapoeira)
            {
                var gingaFeel = combatFeel != null ? combatFeel : (combatFeel = FindAnyObjectByType<PhysicsBattler.Locomotion.CombatFeel>());
                BeginRow();
                Words(new GUIContent("Hands:", "Empty: he has nothing in his hands, as the capoeira recordings were made, so the ginga can be set beside them like for like. Sword: a sword alone (how the sword is carried in the ginga is the next step, not done yet). Sets the lab out again."), Styles().Text, GUILayout.Width(60f));
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

                EndRow();
                if (gingaFeel != null)
                {
                    BeginRow();
                    Words(new GUIContent("Ginga:", "Made (new): the ginga made of the recorded takes' own motion, as an attack is made of a recorded strike: every part of the body moves as the recording has it; it is made smaller, he is held at its rests, he goes from one take to another, and his feet are held where they are put down. Looped (old): the first pass, one take played round and round."), Styles().Text, GUILayout.Width(60f));
                    if (Press(gingaFeel.gingaIsMadeOfItsTakes ? "[made]" : "made", "The ginga made of its takes' motion (Combat Feel \"Ginga Is Made Of Its Takes\").")) gingaFeel.gingaIsMadeOfItsTakes = true;
                    if (Press(!gingaFeel.gingaIsMadeOfItsTakes ? "[looped]" : "looped", "One recorded take, looped (the first pass).")) gingaFeel.gingaIsMadeOfItsTakes = false;
                    EndRow();
                }

                var madeOf = Duel.Mover != null && Duel.Mover.Clip != null ? Duel.Mover.Clip.Loco : null;
                if (gingaFeel != null && gingaFeel.gingaIsMadeOfItsTakes)
                {
                    gingaFeel.gingaRestlessness = Lever("Restless", gingaFeel.gingaRestlessness, 0.05f, 1f, "How long he stays at each rest. 1: not at all, as the recordings, which never stop. 0.4: a second or so. Near 0: many seconds.");
                    gingaFeel.gingaMadeSize = Lever("Size", gingaFeel.gingaMadeSize, 0.4f, 1f, "How big the ginga is against the recordings: the feet's places and the hips' travel. 1: as recorded.");
                    gingaFeel.gingaTempo = Lever("Tempo", gingaFeel.gingaTempo, 0.5f, 1.6f, "How fast it is played against the recordings. 1: as recorded.");
                    gingaFeel.gingaVariation = Lever("Varies", gingaFeel.gingaVariation, 0f, 1f, "How much it varies: how often he goes over to another of the four takes, how unevenly long his rests are, how far off their places his feet land. 0: one take, every time alike.");
                    BeginRow();
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

                    EndRow();
                    Words(new GUIContent(madeOf == null || madeOf.Guard == null || madeOf.Guard.Pattern == null
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
                    BeginRow();
                    Words(new GUIContent("Sword:", "How the blade is held as the sword hand goes with the ginga. By the wrist (new): carried on the forearm with the wrist straight, turned part of the way to where he faces, its point trailing the hand, kept out of his own body. As recorded (old): laid along the recorded empty hand's fingers, which sweeps it through his own head as the arm crosses his face."), Styles().Text, GUILayout.Width(60f));
                    if (Press(gingaFeel.gingaSwordFollowsTheArm ? "[by the wrist]" : "by the wrist", "The blade carried on the forearm, pointed part of the way ahead, trailing the hand, out of his body (Combat Feel \"Ginga Sword Follows The Arm\").")) gingaFeel.gingaSwordFollowsTheArm = true;
                    if (Press(!gingaFeel.gingaSwordFollowsTheArm ? "[as recorded]" : "as recorded", "The blade along the recorded hand's fingers.")) gingaFeel.gingaSwordFollowsTheArm = false;
                    EndRow();
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
                BeginRow();
                Words(new GUIContent("Movement:", "Made (new): put together from its parts at every moment. Recorded (old): the recordings' points played as they are. With the levers at rest the two should look the same; the levers only act on the made one."), Styles().Text, GUILayout.Width(80f));
                if (Press(moveFeel.standardMovementIsMadeOfItsParts ? "[made]" : "made", "Made of its parts (Combat Feel \"Standard Movement Is Made Of Its Parts\").")) moveFeel.standardMovementIsMadeOfItsParts = true;
                if (Press(!moveFeel.standardMovementIsMadeOfItsParts ? "[recorded]" : "recorded", "The recordings played as they are.")) moveFeel.standardMovementIsMadeOfItsParts = false;
                EndRow();
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
                    BeginRow();
                    if (Press("At rest", "Every lever back to rest: he moves as the recordings."))
                    {
                        moveFeel.movementBounce = moveFeel.movementSway = moveFeel.movementHipTurn = moveFeel.movementChestTurn = moveFeel.movementArmSwing = moveFeel.movementStepLift = 1f;
                        moveFeel.movementLean = moveFeel.movementCrouch = 0f;
                    }

                    Words(new GUIContent("Tries of the levers (Prowler, Strutter, Bull) are a fighter's own now: \"Each his own\" below.", "The Prowl and Strut tries that were here changed everyone's movement, and with \"Keep what I tune\" on they were saved into the scene as everyone's. They are given to one fighter at a time under \"Each his own\", which is not saved."), Styles().Text);
                    EndRow();
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
                Words(new GUIContent("The buttons, and each action's settings under its buttons, are in the Actions window.", "\"Actions: in the panel\" above puts them all back here."), Styles().Text);
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
            BeginRow();
            Words(new GUIContent("Levers:", "Each his own (new): a fighter's own levers are laid on everyone's. Everyone's (old): every fighter moves by the one shared set, as it was (Combat Feel \"Each Fighter Has His Own Levers\")."), Styles().Text, GUILayout.Width(80f));
            if (Press(feel.eachFighterHasHisOwnLevers ? "[each his own]" : "each his own", "A fighter's own levers are laid on everyone's.")) feel.eachFighterHasHisOwnLevers = true;
            if (Press(!feel.eachFighterHasHisOwnLevers ? "[everyone's]" : "everyone's", "Every fighter moves by everyone's levers alone, as it was.")) feel.eachFighterHasHisOwnLevers = false;
            EndRow();
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
                BeginRow();
                Words(new GUIContent(whose[k], k == 0 ? "The fighter you move." : "The one he moves about."), Styles().Text, GUILayout.Width(80f));
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

                EndRow();
            }

            var shown = both[ownShown].Clip != null ? both[ownShown].Clip.Loco : null;
            if (shown == null)
            {
                return;
            }

            var own = shown.Own ?? (shown.Own = PhysicsBattler.Locomotion.FighterLevers.Plain());
            Words(new GUIContent($"{(ownShown == 0 ? "His" : "The other's")} own ({own.Name}), each times everyone's:", "1: as everyone. Lean and crouch are added to everyone's."), Styles().Text);
            float was = own.Bounce + own.Sway + own.HipTurn + own.ChestTurn + own.ArmSwing + own.StepLift + own.Lean + own.Crouch + own.Feint + own.Lunge + own.Drop + own.Bluff + own.Guard + own.Restless + own.Aggressive + own.Leaps + own.Heavy + own.Dodges + own.Fakes + own.GuardBreaks;
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
            own.Fakes = Lever("Fakes", own.Fakes, 0f, 3f, "In the dance: how many fake attacks he throws for each real one. 0: none. 0.5: one fake for every two real attacks. 2: two fakes for each. A fake his pattern has that this does not let him throw is left out, and his real attack comes sooner.");
            own.GuardBreaks = Lever("Guard breaks", own.GuardBreaks, 0f, 1f, "In the dance: how many of his own blows are a hard bash to break the other's guard, instead of a cut (twice as often after a blow the other stopped). 0: never. With a shield only.");
            own.Dodges = Lever("Dodges", own.Dodges, 0f, 1f, "In the dance: how readily he ducks aside under a high blow he sees coming, where another would trust his shield (0: never; 1: every time).");
            if (was != own.Bounce + own.Sway + own.HipTurn + own.ChestTurn + own.ArmSwing + own.StepLift + own.Lean + own.Crouch + own.Feint + own.Lunge + own.Drop + own.Bluff + own.Guard + own.Restless + own.Aggressive + own.Leaps + own.Heavy + own.Dodges + own.Fakes + own.GuardBreaks && own.Name != "By hand")
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
            BeginRow();
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

            EndRow();
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

            if (filtering)
            {
                // (Searching: every action's settings are looked through.)
                return true;
            }

            string key = "Actions." + name;
            if (!sectionOpen.TryGetValue(key, out bool open))
            {
                open = PlayerPrefs.GetFloat("BattleSandbox.Section." + key, 0f) > 0.5f;
                sectionOpen[key] = open;
            }

            var tint = GUI.backgroundColor;
            GUI.backgroundColor = zone >= 0 ? ZoneTone(zone, 0.5f, 0.34f) : new Color(0.3f, 0.3f, 0.3f);
            bool pressed = GUILayout.Button(new GUIContent((open ? "−  " : "+  ") + what, "This action's settings, and what was measured of it. Press to fold them open or shut."), Styles().Fold);
            GUI.backgroundColor = tint;
            if (pressed)
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
            Readout(new GUIContent("The other's bash: " + (Duel.Other != null && Duel.Other.Clip != null && Duel.Other.Clip.LastBash.Length > 0 ? Duel.Other.Clip.LastBash + "." : "none yet."), "The other's own last bash: his counter to a hard bash (\"Hard bash met: countered\"), and whether it held."), 2);
            Readout(new GUIContent("Impact meter: " + (PhysicsBattler.Locomotion.ImpactMeter.Last != null ? PhysicsBattler.Locomotion.ImpactMeter.Last.Text + "." : "nothing measured yet (it reads 0.6 s after a bash or a blow on a shield)."), "The impact meter (the plan \"Impacts that carry weight\"): what came in along the line between the two, what is left as motion of the two bodies afterwards, and what each body showed in the 0.6 s after: how far its trunk's lean changed, its hips dropped, its head shifted over its hips and its shield hand moved against its chest. It measures; it changes nothing. Compare \"A bash lands: as an impact / as a shove (old)\" on this line."), 6);
            StumbleButtons(feel, inWindow);
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
            BladePathRow(feel, Duel.Mover != null ? Duel.Mover.Clip : null);
            if (feel != null)
            {
                feel.attacksAreThrownAt = Lever("Speed: single", feel.attacksAreThrownAt, 0.6f, 2f, "Attack speed of a blow on its own: a press on a part of the body map (Combat Feel \"Attacks Are Thrown At\"). 1: as it was. 1.25: a quarter faster throughout: wind-up, drive and the way back. The blade lands that much faster and harder too. Not the combos: those have the slider below.");
                feel.aCombosBlowsAreThrownAt = Lever("Speed: combos", feel.aCombosBlowsAreThrownAt, 0.6f, 2f, "Attack speed of a combo's blows: both blows of \"Throw two\" (Combat Feel \"A Combos Blows Are Thrown At\"). 1: as recorded. 1.25: a quarter faster throughout. Apart from a single blow's speed, the slider above.");
                BeginRow();
                Words(new GUIContent("The second blow:", "What he does when the first blow has driven his man out of the second's reach (Combat Feel \"For The Second Blow He Jumps In\"). He jumps in (new): he is carried toward him in a tenth of a second, as a fighting game's combo does, and the blow is made for where that puts him. He stays (old): the second blow often met nothing."));
                if (Press(feel.forTheSecondBlowHeJumpsIn ? "[he jumps in]" : "he jumps in", "When his man has been driven out of reach.")) feel.forTheSecondBlowHeJumpsIn = true;
                if (Press(!feel.forTheSecondBlowHeJumpsIn ? "[he stays (old)]" : "he stays (old)", "As before.")) feel.forTheSecondBlowHeJumpsIn = false;
                EndRow();
                feel.heJumpsInFrom = Lever("Jumps in from", feel.heJumpsInFrom, 0f, 1f, "He jumps in when the second blow would fall at least this far short (metres).");
                feel.heJumpsInAtMost = Lever("Jumps in at most", feel.heJumpsInAtMost, 0.2f, 3f, "By at most this far (metres).");
                feel.theJumpInTakes = Lever("The jump in takes", feel.theJumpInTakes, 0.02f, 0.4f, "How long the jump in takes (seconds). 0.1 is almost a teleport; longer is a slide.");
                BeginRow();
                Words(new GUIContent("In the jump, his body:", "What his physical body does in the jump in (Combat Feel \"In The Jump His Body Is Carried With Him\"). Carried (new): the jump begins and ends gently and his whole body is given the speed he is carried at; nothing of his physics is put out. Dragged (old): he was carried off at once and his body dragged after him by its hips; after a jump of about 90 cm his chest was left half a metre behind and he fell."));
                if (Press(feel.inTheJumpHisBodyIsCarriedWithHim ? "[carried]" : "carried", "His body goes with him.")) feel.inTheJumpHisBodyIsCarriedWithHim = true;
                if (Press(!feel.inTheJumpHisBodyIsCarriedWithHim ? "[dragged (old)]" : "dragged (old)", "As before: he could fall after a long jump.")) feel.inTheJumpHisBodyIsCarriedWithHim = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Combo:", "Step 3 of the combos plan: several blows as one swing, at the chest, the last the big one (Combat Feel \"A Combo Has This Many Blows\"). 2 blows: a forehand, then a rising backhand. 3 blows: and a last blow, chosen in the row below. Each blow flows out of the carry-through of the one before; the first is a quick light cut, the last lands hardest, freezes longest and throws him."));
                if (Press(feel.aComboHasThisManyBlows <= 2 ? "[2 blows]" : "2 blows", "A forehand, then a rising backhand as the big one.")) feel.aComboHasThisManyBlows = 2;
                if (Press(feel.aComboHasThisManyBlows >= 3 ? "[3 blows]" : "3 blows", "A light forehand, a rising backhand, and the big one.")) feel.aComboHasThisManyBlows = 3;
                if (Press("Throw combo", "He throws the combo at the chest now."))
                {
                    message = Duel.ThrowCombo(Aim, PhysicsBattler.Locomotion.AimSpot.Chest, Time.time) ? "Duel lab: a combo asked for at the chest." : "Duel lab: he can't throw a combo just now (he is busy or down, or the last combo is still being watched).";
                }

                EndRow();
                BeginRow();
                Words(new GUIContent("Last blow:", "The last blow of a combo of three (Combat Feel \"A Combos Last Blow Is\"). Forehand: brought round over his shoulder. Chop: he leaps, the blade comes over his head and down. Spin: he turns right round on the spot, the sword over his head through the turn, and cuts low across the hips (from the recording Renier gave on 21 November). Each begins early in its own wind-up, which is the loop that brings the blade round: da, da ... DUM."));
                for (int last = 0; last < PhysicsBattler.Locomotion.FlowCombos.LastNames.Length; last++)
                {
                    string lastName = PhysicsBattler.Locomotion.FlowCombos.LastNames[last];
                    if (Press(feel.aCombosLastBlowIs == last ? "[" + lastName + "]" : lastName, "The combo of three ends in this.")) feel.aCombosLastBlowIs = last;
                }

                EndRow();
                BeginRow();
                Words(new GUIContent("A combo's blows:", "Whether each blow has more behind it than the one before (Combat Feel \"A Combos Blows Grow\"). Light, plain, heavy (new): a quick light first cut, a plain linking blow, and a last blow that lands hardest, freezes longest and throws him. All plain (old): as step 2 had them."));
                if (Press(feel.aCombosBlowsGrow ? "[light, plain, heavy]" : "light, plain, heavy", "Each blow bigger than the last.")) feel.aCombosBlowsGrow = true;
                if (Press(!feel.aCombosBlowsGrow ? "[all plain (old)]" : "all plain (old)", "As step 2 had them.")) feel.aCombosBlowsGrow = false;
                EndRow();
                feel.aCombosBigBlowIsThrownAt = Lever("Speed: big one", feel.aCombosBigBlowIsThrownAt, 0.6f, 2f, "Attack speed of the combo's last blow alone (Combat Feel \"A Combos Big Blow Is Thrown At\"). 1: as recorded. The first blows go at \"Speed: combos\"; the big one keeps its own time, so that there is a wait before it and the sword can make its whole swing.");
                BeginRow();
                Words(new GUIContent("The big one's swing:", "How the last blow is swung (Combat Feel \"The Big One Swings Big\"). Big (new): wound up further and a little slower, the arm drawn further back and carried further through, the hips driving and sinking more, the cut itself quicker, and it drives on through him at nearly full pace. As recorded (old): the recording's own swing, as at 11:43."));
                if (Press(feel.theBigOneSwingsBig ? "[big]" : "big", "A slow gather and a fast cut.")) feel.theBigOneSwingsBig = true;
                if (Press(!feel.theBigOneSwingsBig ? "[as recorded (old)]" : "as recorded (old)", "As at 11:43.")) feel.theBigOneSwingsBig = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Before the big one, a blow:", "What the combo's earlier blows do to him (Combat Feel \"A Combos Earlier Blows Leave Him In Reach\"). Leaves him in reach (new): it drives him back but never sends him stumbling or down, so the big one finds him where it expects him. Throws him (old): as any blow; at high combo speeds the first blow often threw him out of reach or floored him."));
                if (Press(feel.aCombosEarlierBlowsLeaveHimInReach ? "[leaves him in reach]" : "leaves him in reach", "He is driven back, no more.")) feel.aCombosEarlierBlowsLeaveHimInReach = true;
                if (Press(!feel.aCombosEarlierBlowsLeaveHimInReach ? "[throws him (old)]" : "throws him (old)", "As any blow does.")) feel.aCombosEarlierBlowsLeaveHimInReach = false;
                EndRow();
                feel.aCombosLastBlowLandsWith = Lever("Last blow lands with", feel.aCombosLastBlowLandsWith, 1f, 3f, "How hard the last blow lands against a plain blow (1), before what it gains from the blows landed before it. It is also the showstopper: twice the knock.");
                feel.theLastBlowGrowsWithEachBlowLanded = Lever("Grows, a blow landed", feel.theLastBlowGrowsWithEachBlowLanded, 0f, 0.6f, "The last blow lands this much harder for each blow of the combo that landed before it.");
                feel.theLastBlowsFreezeIsAtMost = Lever("Last blow's freeze", feel.theLastBlowsFreezeIsAtMost, 0.1f, 0.3f, "The longest the last blow freezes both men on the hit (seconds).");
                feel.theLastBlowWindsUpAtLeast = Lever("Last blow winds up", feel.theLastBlowWindsUpAtLeast, 0.15f, 0.6f, "The last blow is begun at least this long before it lands (seconds): longer is a bigger, later blow.");
                feel.theLastBlowsJoinMakesUpAtMost = Lever("Last join makes up", feel.theLastBlowsJoinMakesUpAtMost, 0f, 0.4f, "How much there may be to make up where the last blow is joined on, as time (seconds). 0.2: the last blow comes sooner, the blade's course kinks a little more at the join. 0: the smoothest join, the last blow about 0.2 s later.");
                BeginRow();
                Words(new GUIContent("After two landed, the last:", "What the last blow does to a man two blows of the combo have already landed on (Combat Feel \"After Two Landed The Last Blow Floors Him\"). Floors him (new): he goes down, whatever the blow threw him at. Only if thrown: he goes down only where the blow throws him faster than his legs can catch (a chop onto the head then leaves him standing)."));
                if (Press(feel.afterTwoLandedTheLastBlowFloorsHim ? "[floors him]" : "floors him", "He goes down.")) feel.afterTwoLandedTheLastBlowFloorsHim = true;
                if (Press(!feel.afterTwoLandedTheLastBlowFloorsHim ? "[only if thrown]" : "only if thrown", "By the blow's own throw.")) feel.afterTwoLandedTheLastBlowFloorsHim = false;
                EndRow();
                Readout(new GUIContent("His last combo: " + Duel.ComboTold + ".", "Each blow of the last combo: how hard it was set to land (x a plain blow), when it landed after the first, what it met and how fast, how heavy it was on him, how long both froze, and what it did to him. Then the time from the first hit to the last, how much heavier the last landed than the first, and where his man was 1.5 s after the last hit."), 7);
                BeginRow();
                Words(new GUIContent("Two blows:", "Step 2 of the combos plan: a forehand down and a rising backhand back up the same line, at the chest (Combat Feel \"A Second Blow Flows Out Of The First\"). Flowing (new): one swing. The first blow's own carry-through is played as far as the corner where the blade turns, and there the second is begun in the middle of its own wind-up: the blade never goes back to the guard between the hits. One after the other (old): a whole attack, and when he is free again the other."));
                if (Press(feel.aSecondBlowFlowsOutOfTheFirst ? "[flowing]" : "flowing", "One swing with two hits.")) feel.aSecondBlowFlowsOutOfTheFirst = true;
                if (Press(!feel.aSecondBlowFlowsOutOfTheFirst ? "[one after the other (old)]" : "one after the other (old)", "Two whole attacks.")) feel.aSecondBlowFlowsOutOfTheFirst = false;
                if (Press("Throw two", "He throws the two blows at the chest now."))
                {
                    message = Duel.ThrowTwo(Aim, PhysicsBattler.Locomotion.AimSpot.Chest, Time.time) ? "Duel lab: two blows asked for at the chest." : "Duel lab: he can't throw two just now (he is busy, or down).";
                }

                EndRow();
                BeginRow();
                Words(new GUIContent("At the join, his body:", "What his body does between the two hits (Combat Feel \"At The Join His Body Carries On\"). Carries on (new): the second blow is laid onto where the first has him, so his hips, his feet and his chest go on from there. Jumps (old): the first build, in which he was turned, dropped and leaned to another pose in one step between the hits."));
                if (Press(feel.atTheJoinHisBodyCarriesOn ? "[carries on]" : "carries on", "From where the first blow has him.")) feel.atTheJoinHisBodyCarriesOn = true;
                if (Press(!feel.atTheJoinHisBodyCarriesOn ? "[jumps (old)]" : "jumps (old)", "The first build.")) feel.atTheJoinHisBodyCarriesOn = false;
                EndRow();
                feel.theJoinMakesUpAtMost = Lever("Join makes up at most", feel.theJoinMakesUpAtMost, 0.05f, 0.4f, "How much there may be to make up at the join, as time (seconds). 0.15: the first blow is left at the corner where its blade turns; the blade nearly stops there. 0.25: left sooner, the blade still moving and the second hit sooner, but the blade's course kinks more.");
                feel.theJoinBendsOver = Lever("Join bends over", feel.theJoinBendsOver, 0.04f, 0.3f, "Over how long the body is bent from the first blow onto the second (seconds).");
                Readout(new GUIContent("His last two blows: " + Duel.TwoTold + ".", "Where the first blow was left and the second entered; what each met; how long from the first landing to the second; how far the sword's point travelled between them and the slowest it went (the freeze on the hit apart); how far the posed point was thrown off its course at the join; and how far from the spot the second landed."), 4);
                BeginRow();
                Words(new GUIContent("A blade that cut him:", "What the sword does after a blow that landed on a body (Combat Feel \"A Blade Passes The Body It Cut\"). Passes on (new): the sword is let through the body its blow drove on through, so it sweeps on as its pose does; his shield and his sword still stop it. Sticks (old): the solid sword lies against the body for about a tenth of a second after the hit, then whips on."));
                if (Press(feel.aBladePassesTheBodyItCut ? "[passes on]" : "passes on", "It sweeps on through.")) feel.aBladePassesTheBodyItCut = true;
                if (Press(!feel.aBladePassesTheBodyItCut ? "[sticks (old)]" : "sticks (old)", "As before.")) feel.aBladePassesTheBodyItCut = false;
                EndRow();
            }
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
            BeginRow();
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

            EndRow();
            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("His walk:", "How the guard's four steps are carried (Combat Feel \"The Guard Walks In One Carriage\"). One carriage (new): he steps forward and back on an archer's legs, which keep his turned stance, under his own chest and arms; and each of his four steps is carried as his stand is: his hips at the stand's height, his feet's places toward the stand's. As recorded (old): the sword and shield pack's own walk forward and back, which is 9 cm taller than his stand, narrow and square, and each step as its recording has it."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.theGuardWalksInOneCarriage ? "[one carriage]" : "one carriage", "Every step carried as his stand is.")) feel.theGuardWalksInOneCarriage = true;
                if (Press(!feel.theGuardWalksInOneCarriage ? "[as recorded (old)]" : "as recorded (old)", "Each step as its recording has it, as it was.")) feel.theGuardWalksInOneCarriage = false;
                EndRow();
            }

            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("A leg left behind:", "What happens when his body goes away from a planted foot, as at the first step out of his wide stand (Combat Feel \"A Leg Left Behind Steps Before The Hips Sink\"). Steps first (new): the foot is let go to step after him as soon as its leg is all but straight. Hips sink first (old): his hips sank up to 10 cm before the foot was let go."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aLegLeftBehindStepsBeforeTheHipsSink ? "[steps first]" : "steps first", "The foot steps after him before his hips sink.")) feel.aLegLeftBehindStepsBeforeTheHipsSink = true;
                if (Press(!feel.aLegLeftBehindStepsBeforeTheHipsSink ? "[hips sink first (old)]" : "hips sink first (old)", "His hips sink all they can first, as it was.")) feel.aLegLeftBehindStepsBeforeTheHipsSink = false;
                EndRow();
            }

            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("His steps:", "How he walks when told to walk on in his guard (Combat Feel \"A Step Once Begun Is Finished\"). Whole (new): a step keeps its way and pace until its foot lands, he stops only as a foot lands, and asked to go slowly he takes quick short steps and stands between them. As asked (old): the walk follows what is asked at every moment, in slow motion when that is slow. To see either: the footwork course above, or the dance."), GUILayout.Width(80f));
                if (Press(feel.aStepOnceBegunIsFinished ? "[whole]" : "whole", "Whole steps, with stands between.")) feel.aStepOnceBegunIsFinished = true;
                if (Press(!feel.aStepOnceBegunIsFinished ? "[as asked (old)]" : "as asked (old)", "The walk follows what is asked at every moment, as it was.")) feel.aStepOnceBegunIsFinished = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("A diagonal:", "A step that goes between two of his four ways (Combat Feel \"A Diagonal Turns The Legs\"). Legs turned (new): the nearest way's recording alone, its legs and hips turned to the way he goes, his chest and shield left facing the other. Mixed (old): the two nearest recordings mixed. Only with whole steps."), GUILayout.Width(80f));
                if (Press(feel.aDiagonalTurnsTheLegs ? "[legs turned]" : "legs turned", "One recording, its legs turned.")) feel.aDiagonalTurnsTheLegs = true;
                if (Press(!feel.aDiagonalTurnsTheLegs ? "[mixed (old)]" : "mixed (old)", "Two recordings mixed, as it was.")) feel.aDiagonalTurnsTheLegs = false;
                EndRow();
                if (feel.aStepOnceBegunIsFinished)
                {
                    feel.wholeStepLeastPace = Lever("Step's pace, least", feel.wholeStepLeastPace, 0.3f, 1f, "A whole step's pace, at least, as a share of the recording's own. Higher: quicker, longer steps and longer stands between.");
                    feel.wholeStepLeastTempo = Lever("Step's tempo, least", feel.wholeStepLeastTempo, 0.5f, 1f, "A whole step's tempo, at least, against the recording's own. Lower lets the feet slow down with the pace.");
                    feel.wholeStepPausesFrom = Lever("Stands when ahead by", feel.wholeStepPausesFrom, 0.03f, 0.5f, "He stands once his steps have taken him this far (metres) ahead of what he was asked. Lower: a stand after nearly every step.");
                }
            }

            Readout(new GUIContent("Course: " + Duel.CourseText + ". " + (Duel.Meter.Seconds > 0.5f ? "His footwork " + FootworkText(Duel.Meter, 0) + "." : ""), "What the meter made of the course, so far or when it ended. The band his hips' height moved over while he stood or walked (the middle 90% of the time): a level walk is a few centimetres. How often his hips rose or sank 10 cm or more within 0.3 s with no gesture or blow playing: the jump into another pose. How many steps he took in a spell of walking. How much of his walking was two recordings mixed, each a quarter or more. And how fast his steps went against the recordings' own tempo."), 5);
        }

        /// <summary>The turns round on demand: the other is put at his side or behind him, and he turns to face him, quickly or as he did.</summary>
        private void TurnButtons(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            BeginRow();
            Words(new GUIContent("Put the other:", "To see a turn round on demand. The other is put, at once, this far round him from where he faces, as far off as he was: an enemy suddenly at his side or behind him. He turns to face him by himself, and that turn is timed below. Standing, or while he walks or circles."), Styles().Text, GUILayout.Width(110f));
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

            EndRow();
            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("His turn:", "How he turns to an enemy at his side or behind him (Combat Feel \"A Fighter Turns Round Quickly\"). Quick (new): the quick turn round from the pack \"Turns\", its steps on his own stand under his own chest and arms, the shorter way, aimed at where the other is. As it was (old): his guard's quarter turn stretched, or its turn round, and the rest eased round on his feet; walking, his facing eased round as he steps."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aFighterTurnsRoundQuickly ? "[quick]" : "quick", "The quick turn round.")) feel.aFighterTurnsRoundQuickly = true;
                if (Press(!feel.aFighterTurnsRoundQuickly ? "[as it was (old)]" : "as it was (old)", "His guard's own turns, as before.")) feel.aFighterTurnsRoundQuickly = false;
                EndRow();
            }

            BeginRow();
            Words(new GUIContent("As he turns:", "What he does as he turns round to the other, here in the lab. Just turns: the quick turn alone, every time. Cuts: every turn round is made with a cut in it, so it can be seen on demand: press one of \"Put the other\" above. As in a fight: he cuts as he turns when the other is striking at him, and one time in four otherwise (Combat Feel \"A Fighter Cuts As He Turns Round\"). Turning to his right, a backhand that goes round with him; to his left, his forehand."), Styles().Text, GUILayout.Width(110f));
            if (Press(Duel.TurnCuts == 1 ? "[just turns]" : "just turns", "The quick turn alone, every time: never with a cut.")) Duel.TurnCuts = 1;
            if (Press(Duel.TurnCuts == 2 ? "[cuts]" : "cuts", "Every turn round is made with a cut in it.")) Duel.TurnCuts = 2;
            if (Press(Duel.TurnCuts == 0 ? "[as in a fight]" : "as in a fight", "As the fight has it: he cuts as he turns when the other is striking at him, and one time in four otherwise.")) Duel.TurnCuts = 0;
            EndRow();
            BeginRow();
            Words(new GUIContent("His cut (right):", "The recorded cut whose chest, head and arms are taken for a cut made turning to his right (the pack \"Spin Around Slash\"; never its spin or its leap). To his left it is his own forehand."), Styles().Text, GUILayout.Width(110f));
            foreach (var (cut, name, what) in new[] { ("spin_backhand_high", "high backhand", "A flat backhand at the height of the head and shoulders."), ("spin_backhand_mid", "middle backhand", "A backhand down across the body."), ("spin_chop", "chop", "A chop from overhead.") })
            {
                if (Press(PhysicsBattler.Locomotion.TurnSlashes.RightCut == cut ? $"[{name}]" : name, what)) PhysicsBattler.Locomotion.TurnSlashes.RightCut = cut;
            }

            EndRow();
            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("After the cut:", "How a cut made turning to his right ends (Combat Feel \"A Turn Cut Ends Short\"). Ends short (new): the recording is followed for a tenth of a second after the blade lands (the rest of the sweep across his front), his arm is held out there a moment, then it eases back to his guard. As recorded (old): the spinning man's own carry-through, in which the sword hand goes behind his back and the wrist rolls over."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aTurnCutEndsShort ? "[ends short]" : "ends short", "His arm eases back to his guard soon after the blade lands.")) feel.aTurnCutEndsShort = true;
                if (Press(!feel.aTurnCutEndsShort ? "[as recorded (old)]" : "as recorded (old)", "The recording's own carry-through, as it was.")) feel.aTurnCutEndsShort = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("His aim:", "How far a cut made turning round is bent onto its target (Combat Feel \"A Turn Cut Sweeps Free\"). Sweeps free (new): the hand is moved 7 cm at most and the wrist steered 8 degrees at most; the blade meets the other as the turn carries it round. Bent onto him (old): as any attack, the hand moved up to 40 cm and the wrist turned over at the moment planned for the landing, which was the arm twisting as the swing ended."), Styles().Text, GUILayout.Width(110f));
                if (Press(feel.aTurnCutSweepsFree ? "[sweeps free]" : "sweeps free", "The blade sweeps round with the turn, hardly bent onto the target.")) feel.aTurnCutSweepsFree = true;
                if (Press(!feel.aTurnCutSweepsFree ? "[bent onto him (old)]" : "bent onto him (old)", "Aimed as any attack is, as it was.")) feel.aTurnCutSweepsFree = false;
                EndRow();
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
                BeginRow();
                Words(new GUIContent("Slow orders:", "What an order to walk up, back away or circle does at a pace under a walk's (about 0.9 m/s). Walked (new): he walks on at that pace on his walk's own cycles, slowed, and to the sides on a slow walk recorded for it. Stepped (old): he stands and takes single short steps, which is meant for a small change of where he stands, not for going on (Combat Feel \"A Walk Kept Up Is Walked Not Stepped\")."), Styles().Text, GUILayout.Width(90f));
                if (Press(feel.aWalkKeptUpIsWalkedNotStepped ? "[walked]" : "walked", "He walks on at any pace.")) feel.aWalkKeptUpIsWalkedNotStepped = true;
                if (Press(!feel.aWalkKeptUpIsWalkedNotStepped ? "[stepped (old)]" : "stepped (old)", "Under a walk's pace he stands and steps, as it was.")) feel.aWalkKeptUpIsWalkedNotStepped = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Slow circle:", "What he circles on under his side-step's own pace (about 1.1 m/s), when slow orders are walked. Each is a recording's legs under his guard's own chest and arms. Crouched: the pack's careful crouched step, low, each foot long on the ground. Upright: the pack's slow strafe, feet near each other. Side-step: his guard's own side-step, slowed (no new recording)."), Styles().Text, GUILayout.Width(90f));
                if (Press(feel.slowCircleOn == 1 ? "[crouched]" : "crouched", "The crouched careful step.")) feel.slowCircleOn = 1;
                if (Press(feel.slowCircleOn == 2 ? "[upright]" : "upright", "The upright slow strafe.")) feel.slowCircleOn = 2;
                if (Press(feel.slowCircleOn == 0 ? "[side-step]" : "side-step", "His guard's own side-step, slowed.")) feel.slowCircleOn = 0;
                EndRow();
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

        // ---- Stumbles on demand (the Fight Rhythm plan's step 9c; 19 November) ----

        private int stumbleBash = -1, stumbleWaits = -1;
        private float stumbleBashAt;

        /// <summary>How far off the other is put for each bash of "Stumble him" (metres at a Medium's size).</summary>
        private static readonly float[] StumbleBashFrom = { 1f, 1.8f, 1.9f, 1.9f, 1.4f, 1.4f, 1.4f, 1.4f };

        /// <summary>How fast the test's kick for a man knocked far throws him (m/s at a Medium's size): harder than any kick a fighter has, to land him metres off.</summary>
        private const float FarKickThrows = 8f;

        /// <summary>The place of "Kick him" in <see cref="StumbleBashFrom"/> (soft; medium and hard follow it): asked for and waited for as a bash is.</summary>
        private const int KickHim = 4;

        /// <summary>The other is stood before him and bashed or kicked: at once, or once both are on their feet.</summary>
        private void AskStumble(int kind, string name)
        {
            var basher = Duel.Mover != null ? Duel.Mover.Clip : null;
            var other = Duel.Other != null ? Duel.Other.Clip : null;
            if (basher == null)
            {
                message = name + ": not now (the lab is not set out).";
            }
            else if (kind < KickHim && basher.ShieldColliders.Count == 0)
            {
                message = name + ": he has no shield. \"Set out\" under the shield test gives both one.";
            }
            else if ((other != null && other.Fallen) || basher.Fallen)
            {
                stumbleWaits = kind;
                message = name + ": waiting until both are on their feet.";
            }
            else if (Duel.PutOtherAt(StumbleBashFrom[kind]))
            {
                stumbleBash = kind;
                stumbleBashAt = Time.time + 0.6f;
                message = $"{name}: the other is stood {StumbleBashFrom[kind]:0.0} m before him.";
            }
            else
            {
                message = name + ": not now (the lab is not set out).";
            }
        }

        /// <summary>A bash asked for by "Stumble him" is thrown once the other has stood a moment where he was put.</summary>
        private void StepStumbleBash()
        {
            if (stumbleWaits >= 0)
            {
                // (Asked for while one of them was off his feet: once both stand, the other is put and the bash follows.)
                var first = Duel.Mover != null ? Duel.Mover.Clip : null;
                var second = Duel.Other != null ? Duel.Other.Clip : null;
                if (first == null || second == null)
                {
                    stumbleWaits = -1;
                }
                else if (!first.Fallen && !second.Fallen && !first.Stumbling && !second.Stumbling)
                {
                    int waited = stumbleWaits;
                    stumbleWaits = -1;
                    if (Duel.PutOtherAt(StumbleBashFrom[waited]))
                    {
                        stumbleBash = waited;
                        stumbleBashAt = Time.time + 0.6f;
                        message = $"Stumble him: the other is stood {StumbleBashFrom[waited]:0.0} m before him.";
                    }
                }
            }

            if (stumbleBash < 0 || Time.time < stumbleBashAt)
            {
                return;
            }

            int kind = stumbleBash;
            stumbleBash = -1;
            var basher = Duel.Mover != null ? Duel.Mover.Clip : null;
            var other = Duel.Other != null ? Duel.Other.Clip : null;
            if (basher == null || other == null)
            {
                return;
            }

            if (kind >= KickHim)
            {
                bool far = kind - KickHim > 2;
                message = basher.KickAt(other, true, Mathf.Min(kind - KickHim, 2), far ? FarKickThrows : 0f) ? (far ? "Knock him down and strike him: a kick harder than any he has, for the test." : $"Kick him: a {PhysicsBattler.Locomotion.ClipFighter.KickNames[kind - KickHim]} front kick.") : "Kick him: he could not kick (in a blow, a bash or another move, stumbling, or down).";
                return;
            }

            string[] names = { "small", "medium", "hard", "extra hard" };
            message = basher.Bash(other, true, Mathf.Min(kind, 2), false, kind == 3) ? $"Stumble him: a {names[kind]} bash." : "Stumble him: he could not bash (in a blow, a leap or a bash, meeting a blow, or down).";
        }

        /// <summary>Stumbles and falls: a bash at a standing fighter on demand, small, medium and hard; old and new; the levers; what the last stumble was.</summary>
        private void StumbleButtons(PhysicsBattler.Locomotion.CombatFeel feel, bool inWindow)
        {
            Heading("Stumbles and falls", "What being bashed, kicked or struck hard does to a fighter. His body is a weight on legs: thrown by an impact it tips away from his feet, and only a foot put down ahead of it stops it. If his feet get there he keeps them, on one stumble step or several, and may be turned round. If his body outruns them he goes down. Both men of an impact can be thrown off balance: what goes in comes out. Nothing is drawn by lot.");
            BeginRow();
            Words(new GUIContent("Slow motion:", "How fast the game runs, to watch a stumble, a kick or a get-up closely. It is the same game, only slower: nothing in it changes. It stays until you put it back to full speed."));
            float[] speeds = { 1f, 0.5f, 0.25f, 0.1f };
            string[] speedNames = { "full speed", "half", "a quarter", "a tenth" };
            for (int k = 0; k < speeds.Length; k++)
            {
                bool chosen = Mathf.Abs(timeScale - speeds[k]) < 0.01f;
                if (Press(chosen ? "[" + speedNames[k] + "]" : speedNames[k], $"The game at {speeds[k]:0.##} of its speed."))
                {
                    timeScale = speeds[k];
                    Time.timeScale = paused ? 0f : timeScale;
                    message = speeds[k] >= 1f ? "Slow motion: off, full speed." : $"Slow motion: the game runs at {speedNames[k]} speed.";
                }
            }

            EndRow();
            var basher = Duel.Mover != null ? Duel.Mover.Clip : null;
            var other = Duel.Other != null ? Duel.Other.Clip : null;
            string[] names = { "Small", "Medium", "Hard", "Extra hard" };
            string[] tips =
            {
                "The other is stood 1 m before him and he bashes him from where he stands. A shove: a step back, or his stance takes it.",
                "The other is stood 1.8 m before him and he leaps in behind his board. The other is thrown back off balance and stumbles: two or three stumble steps.",
                "The other is stood 1.9 m before him and he leaps all the way in. With \"Hard bash met: countered\" the other braces and is carried back a step or two; with \"not countered\" he is thrown hard, stumbles and may go down.",
                "The hard bash's leap, driven through with everything he has (\"Extra hard bash pushes through\" in the settings below). Not countered, the other is thrown off his feet and lands metres back. For now only from this button: no fighter throws it by himself.",
            };
            BeginRow();
            Words(new GUIContent("Stumble him:", "A bash at a standing fighter on demand. The other is put straight before him at that bash's distance, stands a moment, and is bashed. Pressed while either of them is down, getting up or still stumbling, it waits until both stand. Watch the other's feet, and the basher's: either can be thrown off balance. Use the pace at the top of this window to see it slowly."), Styles().Text, GUILayout.Width(110f));
            for (int kind = 0; kind < names.Length; kind++)
            {
                if (Press(names[kind], tips[kind]))
                {
                    // (19 November: a man who was down or getting up was put on the spot at once, and bashed as he
                    // rose. Seen in Renier's log: his body thrown 70 to 80 cm back onto its pose, three times in one
                    // session. The button waits for both to stand.)
                    AskStumble(kind, "Stumble him");
                }
            }

            EndRow();
            BeginRow();
            Words(new GUIContent("Kick him:", "A kick at a standing fighter on demand (Combat Feel \"A Kick Throws A Body\"). The other is put 1.4 m straight before him, stands a moment, and is kicked: a front kick with the back foot, from the sword-and-shield recordings. Where the foot meets his body or his board he is thrown as by any impact. Pressed while either is down or stumbling, it waits until both stand. No fighter kicks by himself yet."));
            string[] kicks = { "Soft kick", "Medium kick", "Hard kick" };
            string[] kickTips =
            {
                "The other is stood 1.4 m before him and he kicks him chest high with his back foot. On a body: thrown back a stumble step or two. On a board held in cover: less.",
                "The same kick, harder (\"Medium kick power\"). He is thrown back stumbling hard: several stumble steps, and he may only just keep his feet.",
                "The same kick with everything in it (\"Hard kick power\"). It is meant to knock him over backwards, through his board too.",
            };
            for (int kick = 0; kick < kicks.Length; kick++)
            {
                if (Press(kicks[kick], kickTips[kick]))
                {
                    AskStumble(KickHim + kick, "Kick him");
                }
            }

            EndRow();
            if (feel != null)
            {
                feel.kickPower = Lever("Kick power", feel.kickPower, 0.5f, 6f, "How fast a kick throws a man of the kicker's own weight (m/s at a Medium's size). A medium bash throws at about 2.1, a hard one at about 4.3.");
                feel.mediumKickPower = Lever("Medium kick power", feel.mediumKickPower, 0.5f, 8f, "How fast a medium kick throws a man of the kicker's own weight (m/s at a Medium's size).");
                feel.hardKickPower = Lever("Hard kick power", feel.hardKickPower, 0.5f, 8f, "How fast a hard kick throws a man of the kicker's own weight (m/s at a Medium's size). Meant to knock him over backwards.");
                feel.kickGoesIntoHim = Lever("Kicks into him", feel.kickGoesIntoHim, 0f, 0.6f, "How far past the other's front the kicking foot is aimed (metres at a Medium's size).");
                BeginRow();
                Words(new GUIContent("Not in his guard:", "What an impact does to a man who is not standing in a guard: a bare-handed man, for one (Combat Feel \"A Man Not In His Guard Stumbles And Falls Too\"). He stumbles and falls (new): his legs decide, as any fighter's do: stumble steps, caught or down. He is only shoved (old): however hard he is hit he goes back on quick little steps and never falls. To see it: \"Set out\" the target with nothing in his hands, then kick him soft, medium and hard."));
                if (Press(feel.aManNotInHisGuardStumblesAndFallsToo ? "[he stumbles and falls]" : "he stumbles and falls", "His legs decide: stumble steps, caught or down.")) feel.aManNotInHisGuardStumblesAndFallsToo = true;
                if (Press(!feel.aManNotInHisGuardStumblesAndFallsToo ? "[he is only shoved (old)]" : "he is only shoved (old)", "As before.")) feel.aManNotInHisGuardStumblesAndFallsToo = false;
                EndRow();
            }

            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("An opening:", "What he does when the other stumbles, has his guard knocked open, or has his back turned (Combat Feel \"A Fighter Pounces On An Opening\"). Pounces (new): as soon as he is free he goes for it: he runs in and throws a light blow at the chest while he is still running, so that it lands as he arrives, in one motion; in reach he strikes at once. Once for each opening. Stands off (old): he keeps his guard's distance and lets the other find his feet."));
                if (Press(feel.aFighterPouncesOnAnOpening ? "[he pounces]" : "he pounces", "He goes for it.")) feel.aFighterPouncesOnAnOpening = true;
                if (Press(!feel.aFighterPouncesOnAnOpening ? "[he stands off (old)]" : "he stands off (old)", "As before.")) feel.aFighterPouncesOnAnOpening = false;
                EndRow();
                feel.pounceReadsIn = Lever("Pounce reads in", feel.pounceReadsIn, 0f, 0.6f, "How soon he reads an opening and goes for it (seconds).");
                feel.aStumbleIsAnOpeningFrom = Lever("Pounce on a stumble of", feel.aStumbleIsAnOpeningFrom, 0f, 2f, "A stumble is an opening he pounces on only once it has driven the other man this far (metres). 0: any stumble, as it was. Larger: he pounces less often.");
                BeginRow();
                Words(new GUIContent("After he goes for him:", "What the swordsman does once a pounce, or a blow at a man who is down, is over and you have given him no order to move (Combat Feel \"After Going For Him He Takes His Distance Again\"). He steps back (new): to the distance he stood at before he went, so your next blow starts from the lab's distance again. He stands there (old): where the going ended; the target, walking back to its own spot, then comes right up against him."));
                if (Press(feel.afterGoingForHimHeTakesHisDistanceAgain ? "[he steps back]" : "he steps back", "To the distance he stood at.")) feel.afterGoingForHimHeTakesHisDistanceAgain = true;
                if (Press(!feel.afterGoingForHimHeTakesHisDistanceAgain ? "[he stands there (old)]" : "he stands there (old)", "As before.")) feel.afterGoingForHimHeTakesHisDistanceAgain = false;
                EndRow();
                feel.heStepsBackThisLongAfterHisBlowLands = Lever("Steps back after", feel.heStepsBackThisLongAfterHisBlowLands, 0f, 1f, "How long after his blow lands he begins the step back (seconds). 0: he waits for the whole blow (as it was at 21:15), and the target walks back into him first.");
                Readout(new GUIContent($"Their distance: the two are {Duel.Gap:0.00} m apart; he keeps {Duel.StoodOff:0.00} m. After his last going: " + (Duel.TookDistance.Length > 0 ? Duel.TookDistance : "nothing yet") + ".", "How far apart the two are now, root to root; the distance he keeps (between the two spots, as last seen with both standing still on them); and what he did when his last pounce or blow at a man who was down was over."), 2);
            }

            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("Stumbling, his guard:", "Whether a man who is stumbling can meet a blow (Combat Feel \"A Stumbling Fighter Cannot Guard\"). He cannot guard (new): for as long as he is off balance, and until he has found his feet, his shield and sword do not go to a blow. By the clock (old): his guard was open for 0.8 s after a bash or a kick, and after that he blocked even while still stumbling backward. Here and in the dance only: in the fight of six it is still by the clock."));
                if (Press(feel.aStumblingFighterCannotGuard ? "[he cannot guard]" : "he cannot guard", "While he is off balance.")) feel.aStumblingFighterCannotGuard = true;
                if (Press(!feel.aStumblingFighterCannotGuard ? "[by the clock (old)]" : "by the clock (old)", "As before.")) feel.aStumblingFighterCannotGuard = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("An opening, his blow:", "Where the pounce's blow goes (Combat Feel \"A Blow On The Run Goes For What Is Open\"). What is open (new): as he throws it, the other's chest, head, neck, sword shoulder, sword arm, belly and thighs are looked at in turn from his sword shoulder, and the first with nothing before it is taken. His chest (old): always the chest, which a man with a shield has his board before."));
                if (Press(feel.aBlowOnTheRunGoesForWhatIsOpen ? "[what is open]" : "what is open", "The first spot with nothing before it.")) feel.aBlowOnTheRunGoesForWhatIsOpen = true;
                if (Press(!feel.aBlowOnTheRunGoesForWhatIsOpen ? "[his chest (old)]" : "his chest (old)", "As before.")) feel.aBlowOnTheRunGoesForWhatIsOpen = false;
                EndRow();
            }

            Readout(new GUIContent("His last pounce: " + Duel.Pounced + $" ({Duel.OpeningsSeen} openings seen, {Duel.PounceLeaps} gone for, {Duel.PounceBlows} struck at).", "What opening he saw, how far off, how far and fast he ran, where and how fast he was going as he threw the blow and as it landed, and what the blade met."), 3);
            BeginRow();
            Words(new GUIContent("A man who is down:", "What he does while the other lies off his feet (Combat Feel \"A Fighter Goes For A Man Who Is Down\"). He goes for him (the default, as you decided on 20 November): he runs to him, from however far he was thrown, and stabs down from beside his chest as he arrives, in one motion. He holds off (old): he lets him get up, which is the way to watch a get-up undisturbed. \"Strike him where he lies\" asks for it once whatever this says."));
            if (feel != null)
            {
                if (Press(feel.aFighterGoesForAManWhoIsDown ? "[he goes for him]" : "he goes for him", "Every time the other is down: he runs to him and stabs down. The default, as you decided on 20 November.")) feel.aFighterGoesForAManWhoIsDown = true;
                if (Press(!feel.aFighterGoesForAManWhoIsDown ? "[he holds off (old)]" : "he holds off (old)", "He lets him get up: the way to watch a get-up undisturbed.")) feel.aFighterGoesForAManWhoIsDown = false;
            }

            if (Press(Duel.StrikeDownedOnce ? "[Strike him where he lies]" : "Strike him where he lies", "Once: the next time the other is down (or now, if he is), he runs to him and strikes down at his chest as he arrives."))
            {
                Duel.StrikeDownedOnce = !Duel.StrikeDownedOnce;
                message = Duel.StrikeDownedOnce ? "A man who is down: he will be struck where he lies, once." : "A man who is down: not asked for any more.";
            }

            EndRow();
            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("A man who is down, his blow:", "The blow at a man who lies (Combat Feel \"A Man Who Is Down Is Struck From Beside Him\"). A stab from beside him (new): he runs to a place beside the man's chest, square to how he lies, bends to it and stabs down at the first of chest, belly, hips, head and neck that is open. A standing blow, straight at him (old): he runs straight at him and swings low; it mostly met a shin, an arm or a hand first."));
                if (Press(feel.aManWhoIsDownIsStruckFromBesideHim ? "[a stab from beside him]" : "a stab from beside him", "Beside his chest, bent to it, the point going down.")) feel.aManWhoIsDownIsStruckFromBesideHim = true;
                if (Press(!feel.aManWhoIsDownIsStruckFromBesideHim ? "[a standing blow, straight at him (old)]" : "a standing blow, straight at him (old)", "As before.")) feel.aManWhoIsDownIsStruckFromBesideHim = false;
                EndRow();
                feel.heBendsToAManWhoIsDownBy = Lever("He bends to it by", feel.heBendsToAManWhoIsDownBy, 0f, 0.35f, "How much lower he carries his hips through the stab at a man who is down (leg lengths). 0: as tall as in any thrust.");
            }

            BeginRow();
            Words(new GUIContent("Knock him down and strike him:", "The test for a man who is down, in one press. The other is stood before him and knocked off his feet; then he runs to him and strikes down at his chest as he arrives, in one motion: no stop between the run and the blow. Near: a hard kick, the other lands about three metres off. Far: a kick harder than any a fighter has, only for this test, so that he lands five metres and more off and has to be run after. Watch the runner's feet as the blow goes out: they should not stop."));
            if (Press("Near", "A hard kick knocks him over; then he is run at and struck where he lies."))
            {
                Duel.StrikeDownedOnce = true;
                AskStumble(KickHim + 2, "Knock him down and strike him");
            }

            if (Press("Far", "A kick harder than any he has throws him metres back; then he is run after and struck where he lies."))
            {
                Duel.StrikeDownedOnce = true;
                AskStumble(KickHim + 3, "Knock him down and strike him");
            }

            EndRow();
            if (feel != null)
            {
                feel.heGoesForHimAt = Lever("He goes for him at", feel.heGoesForHimAt, 1.5f, 6f, "How fast he runs at a man he has an opening on, or one who is down (m/s at a Medium's size). He throws the blow while still running, so that it lands as he comes into reach.");
            }

            Readout(new GUIContent("Going for him now: " + (Duel.Going ? Duel.GoingNow : "no") + ".", "Whether he is running at the other now, and where the blow is."), 1);
            Readout(new GUIContent("His last blow at a man who was down: " + Duel.DownedStruck + $" ({Duel.DownedBlows} thrown).", "How far off the other lay, how far and how fast he ran, where and how fast he was going as he threw the blow and as it landed, the slowest he went in between, and what the blade met."), 3);
            Readout(new GUIContent("His last kick: " + (basher != null && basher.LastKick.Length > 0 ? basher.LastKick + "." : "none yet."), "Where the foot landed, how fast it was going, and what it did to the other."), 2);
            if (feel == null)
            {
                return;
            }

            BeginRow();
            Words(new GUIContent("Off balance:", "What a fighter thrown off balance does (Combat Feel \"Off Balance He Stumbles On Real Steps\"). Real stumble steps (new): his feet go for the point that would stop him, each landing takes speed out of him and his knees give; caught, turned round, or down by whether his feet get there. The walk played fast (old): he is carried back on the guard's upright walk and falls only from a fixed speed."), GUILayout.Width(100f));
            if (Press(feel.offBalanceHeStumblesOnRealSteps ? "[real stumble steps]" : "real stumble steps", "His legs decide, on real steps.")) feel.offBalanceHeStumblesOnRealSteps = true;
            if (Press(!feel.offBalanceHeStumblesOnRealSteps ? "[the walk played fast (old)]" : "the walk played fast (old)", "As before.")) feel.offBalanceHeStumblesOnRealSteps = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("An impact, his knees and hips:", "What an impact does to a fighter's legs (Combat Feel \"A Body Takes An Impact In Its Knees And Hips\"). Give (new): a bash on him, his own bash meeting a board, or a heavy blow presses him down as it lands: his knees give, his hips drop by how hard it is, his trunk folds a little toward it, and he comes up again more slowly. Both men of a bash. Stand stiff (old): only a man thrown off balance sinks."));
            if (Press(feel.aBodyTakesAnImpactInItsKneesAndHips ? "[give]" : "give", "His knees and hips take it.")) feel.aBodyTakesAnImpactInItsKneesAndHips = true;
            if (Press(!feel.aBodyTakesAnImpactInItsKneesAndHips ? "[stand stiff (old)]" : "stand stiff (old)", "As before.")) feel.aBodyTakesAnImpactInItsKneesAndHips = false;
            EndRow();
            feel.impactContactTime = Lever("Contact time", feel.impactContactTime, 0.05f, 0.5f, "How long an impact takes to press him down (seconds). He comes up again over about twice and a half that.");
            feel.impactKneesGive = Lever("Knees give to it", feel.impactKneesGive, 0f, 0.25f, "How far his hips drop for a hard impact, one that changes his speed by 3 m/s (metres on a Medium).");
            BeginRow();
            Words(new GUIContent("A blow, his body:", "What a weapon's blow does to the body it lands on (Combat Feel \"A Body Takes A Blow In Its Knees And Trunk\"). Takes it (new): his knees give, his trunk folds the way the blow goes and his chest is turned by it, by how hard the blow is; he comes up again over about half a second. Stands stiff (old): only the struck part was knocked and a reaction clip played."));
            if (Press(feel.aBodyTakesABlowInItsKneesAndTrunk ? "[takes it]" : "takes it", "His whole body gives to the blow.")) feel.aBodyTakesABlowInItsKneesAndTrunk = true;
            if (Press(!feel.aBodyTakesABlowInItsKneesAndTrunk ? "[stands stiff (old)]" : "stands stiff (old)", "As before.")) feel.aBodyTakesABlowInItsKneesAndTrunk = false;
            EndRow();
            feel.blowKneesGive = Lever("A blow, knees give", feel.blowKneesGive, 0f, 0.2f, "For a hard blow: how far his hips drop (metres).");
            feel.blowTrunkFolds = Lever("A blow, trunk folds", feel.blowTrunkFolds, 0f, 30f, "For a hard blow: how far his trunk folds the way the blow goes (degrees).");
            feel.blowChestTurns = Lever("A blow, chest turns", feel.blowChestTurns, 0f, 40f, "For a hard blow well off his middle: how far his chest is turned by it (degrees).");
            {
                var took = Duel.Other != null ? Duel.Other.Clip : Aim.Target != null ? Aim.Target.Clip : null;
                Readout(new GUIContent("The last blow he took: " + (took == null || took.BlowsPressed == 0 ? "none yet (the duel lab's or the aim test's target)." : $"hips down {100f * took.BlowPressedMost:0} cm, trunk folded {took.BlowFoldedMost:0} degrees, chest turned {took.BlowTurnedMost:0} degrees ({took.BlowsPressed} blows taken so)."), "How far the target's body gave to the last blow that landed on it."), 1);
            }

            feel.impactTrunkFolds = Lever("Trunk folds to it", feel.impactTrunkFolds, 0f, 20f, "How far his trunk folds toward a hard impact as it presses him down (degrees).");
            BeginRow();
            Words(new GUIContent("Stumbling, his arms:", "What a stumbling fighter's arms do (Combat Feel \"A Stumbling Fighter's Arms Go Out For Balance\"). Go out for balance (new): his upper arms lift out from his sides by how fast he is going, the far arm from where he tips the further, and both are left behind a little by the way he is thrown. Hold his guard (old): they stay as his guard has them."));
            if (Press(feel.aStumblingFightersArmsGoOutForBalance ? "[go out for balance]" : "go out for balance", "His arms help him keep his feet.")) feel.aStumblingFightersArmsGoOutForBalance = true;
            if (Press(!feel.aStumblingFightersArmsGoOutForBalance ? "[hold his guard (old)]" : "hold his guard (old)", "As before.")) feel.aStumblingFightersArmsGoOutForBalance = false;
            EndRow();
            feel.stumblingArmsGoOutBy = Lever("Arms go out by", feel.stumblingArmsGoOutBy, 0f, 2f, "How much a stumbling fighter's arms go out: 1 as built, 0 not at all, 2 twice as far (never more than about 50 degrees).");
            BeginRow();
            Words(new GUIContent("A bash's speeds:", "Where the speeds the two leave a bash with come from (Combat Feel \"A Bash's Speeds Are Worked Out\"). Worked out (new): the momentum ledger. Both weights and both speeds go in; a man who has set himself passes some to the ground through his feet; what is left is shared out, equal and opposite. The basher is checked, stopped or thrown back by as much as he gave. Written in (old): the other gets the board's speed times the bash's weight, and a countered hard bash leaves both at two fixed speeds."), GUILayout.Width(100f));
            if (Press(feel.aBashsSpeedsAreWorkedOut ? "[worked out]" : "worked out", "The momentum ledger.")) feel.aBashsSpeedsAreWorkedOut = true;
            if (Press(!feel.aBashsSpeedsAreWorkedOut ? "[written in (old)]" : "written in (old)", "As before.")) feel.aBashsSpeedsAreWorkedOut = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("A bash lands:", "When a bash counts as having met him (Combat Feel \"A Bash Lands When The Board Touches Him\"). When the board touches (new): against his body, or his own board if it is between them. From 46 cm off (old): some 30 cm of air before the board; the other flew off untouched."), GUILayout.Width(100f));
            if (Press(feel.aBashLandsWhenTheBoardTouchesHim ? "[when the board touches]" : "when the board touches", "On touch.")) feel.aBashLandsWhenTheBoardTouchesHim = true;
            if (Press(!feel.aBashLandsWhenTheBoardTouchesHim ? "[from 46 cm off (old)]" : "from 46 cm off (old)", "As before.")) feel.aBashLandsWhenTheBoardTouchesHim = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("After a bash:", "What the basher does once his bash has landed on a man who keeps his feet (Combat Feel \"After A Bash He Holds His Ground\"). He holds his ground (new): his own legs take what is left of his going forward, and the two end apart. He goes on (old): at whatever speed was left him; the two can end board against board, shields stuck together."), GUILayout.Width(100f));
            if (Press(feel.afterABashHeHoldsHisGround ? "[he holds his ground]" : "he holds his ground", "He stops where the boards met.")) feel.afterABashHeHoldsHisGround = true;
            if (Press(!feel.afterABashHeHoldsHisGround ? "[he goes on (old)]" : "he goes on (old)", "As before.")) feel.afterABashHeHoldsHisGround = false;
            EndRow();
            Readout(new GUIContent("The other's last stumble: " + (other != null && other.LastStumble.Length > 0 ? other.LastStumble + "." : "none yet."), "What threw him, his stumble steps, how far it carried and turned him, and how it ended."), 3);
            Readout(new GUIContent("His own last stumble: " + (basher != null && basher.LastStumble.Length > 0 ? basher.LastStumble + "." : "none yet."), "The basher's own: what came back at him can throw him off balance too."), 2);
            if (ActionSettings(inWindow, "Stumbles", "Stumbles and falls: settings"))
            {
                if (feel.aBashsSpeedsAreWorkedOut)
                {
                    feel.impactBounce = Lever("Impact bounce", feel.impactBounce, 0f, 1f, "How much two bodies that ran into each other spring apart, of the speed they closed at. Higher throws the other further and stops the basher harder.");
                    feel.feetTakeOfAnImpact = Lever("Feet take", feel.feetTakeOfAnImpact, 0f, 1f, "How much of what his grip allows a fighter who has set himself (a counter) passes to the ground. 0: bracing does nothing.");
                }

                if (feel.aBashsSpeedsAreWorkedOut)
                {
                    feel.smallBashPushesThrough = Lever("Small bash pushes through", feel.smallBashPushesThrough, 0f, 10f, "How much speed his arm and legs add to his board while the two are pressed together (m/s). Higher: the other is thrown harder.");
                    feel.mediumBashPushesThrough = Lever("Medium bash pushes through", feel.mediumBashPushesThrough, 0f, 10f, "The same for the medium bash.");
                    feel.hardBashPushesThrough = Lever("Hard bash pushes through", feel.hardBashPushesThrough, 0f, 10f, "The same for the hard bash.");
                    feel.extraHardBashPushesThrough = Lever("Extra hard bash pushes through", feel.extraHardBashPushesThrough, 0f, 14f, "The same for the extra hard bash: at 6 an uncountered man is thrown off his feet and lands metres back.");
                }

                if (feel.offBalanceHeStumblesOnRealSteps)
                {
                    feel.stumbleKneesGiveAtOnce = Lever("Knees give at once", feel.stumbleKneesGiveAtOnce, 0f, 2f, "How much his knees give as he is thrown, before his first step. 0: his legs stand stiff while his upper body recoils.");
                    feel.stumbleFootSpeed = Lever("Stumble foot speed", feel.stumbleFootSpeed, 3f, 9f, "The fastest a foot goes in a stumble step (m/s). This decides how hard he can be thrown and keep his feet. Higher: he stays up from harder blows.");
                    feel.stumbleStepReach = Lever("Stumble step reach", feel.stumbleStepReach, 0.25f, 0.9f, "How far from under his hips a stumbling foot can be put down (leg lengths). Shorter: more, quicker steps, a scramble. Longer: fewer, longer steps.");
                    feel.stumbleStepTakes = Lever("Stumble step takes", feel.stumbleStepTakes, 0.08f, 0.5f, "The shortest a stumble step is in the air (seconds).");
                    feel.stumbleFirstFootLiftsIn = Lever("First foot lifts in", feel.stumbleFirstFootLiftsIn, 0f, 0.4f, "How long after he is thrown his first foot lifts (seconds).");
                    feel.stumbleLegsBrake = Lever("Legs brake", feel.stumbleLegsBrake, 0f, 1f, "How much of what a foot landing ahead of him can take out of his speed it does take. Lower: he stumbles further.");
                    feel.stumbleKneesGive = Lever("Knees give", feel.stumbleKneesGive, 0f, 0.15f, "How far his knees give as a stumbling foot takes his weight.");
                    feel.stumbleDownAt = Lever("Down at", feel.stumbleDownAt, 0.2f, 0.7f, "How far his hips may sink in a stumble before he is down (leg lengths).");
                    feel.stumbleTrunkTips = Lever("Trunk tips", feel.stumbleTrunkTips, 0f, 1f, "How much his upper body tips with a stumble: the way he goes while his weight is past his feet, back against a foot landed ahead of him. 0: upright over stumbling legs.");
                    feel.stumbleTurnedBy = Lever("Turned by", feel.stumbleTurnedBy, 0f, 1f, "How much of an impact's twist turns the whole of him round. 0: never turned.");
                }
            }
        }

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
            BeginRow();
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

            EndRow();
            BeginRow();
            Words(new GUIContent("He bashes:", "What a press of a bash goes at. The other: from where he stands, leaping as far as that bash does toward where the other is. The air: straight ahead with that bash's whole leap, whoever is there or not, to see the leap and the drive by themselves."), Styles().Text, GUILayout.Width(80f));
            if (Press(!bashAtAir ? "[the other]" : "the other", "He bashes the other.")) bashAtAir = false;
            if (Press(bashAtAir ? "[the air]" : "the air", "He bashes straight ahead, at nobody.")) bashAtAir = true;
            EndRow();
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
            BeginRow();
            Words(new GUIContent("After it:", "What he does once his board has met the other. Goes through: he carries on to where his leap ends, into the space the other is thrown out of. Stops: he comes down where he met him."), Styles().Text, GUILayout.Width(80f));
            if (Press(b.GoesThrough ? "[goes through]" : "goes through", "He carries on to where his leap ends.")) b.GoesThrough = true;
            if (Press(!b.GoesThrough ? "[stops]" : "stops", "He comes down where he met him.")) b.GoesThrough = false;
            EndRow();
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
            BeginRow();
            Words(new GUIContent("Too near:", "What a fighter with a shield does when an enemy comes too near him (Combat Feel \"A Shield Bashes What Comes Too Near\"). Bashes (new): he drives his shield and his body into him and shoves him back out of his space. Nothing (old): as it was."), Styles().Text, GUILayout.Width(80f));
            if (Press(feel.aShieldBashesWhatComesTooNear ? "[bashes]" : "bashes", "He bashes an enemy who comes too near.")) feel.aShieldBashesWhatComesTooNear = true;
            if (Press(!feel.aShieldBashesWhatComesTooNear ? "[nothing (old)]" : "nothing (old)", "No bash.")) feel.aShieldBashesWhatComesTooNear = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("A bash lands:", "How a bash lands (Combat Feel \"A Bash Lands As An Impact\"). As an impact (new): it hands the other its momentum at the point the board really met him, and he shows it as he shows a sword blow: trunk rocked, body bent where it landed, shield arm driven back and guard knocked open if it met his shield; the basher takes the same back at his own shield arm; both stand still a moment. As a shove (old): it only changes the other's speed over the ground, or knocks him over."), GUILayout.Width(100f));
            if (Press(feel.aBashLandsAsAnImpact ? "[as an impact]" : "as an impact", "Both bodies show it.")) feel.aBashLandsAsAnImpact = true;
            if (Press(!feel.aBashLandsAsAnImpact ? "[as a shove (old)]" : "as a shove (old)", "Only the other's speed over the ground changes.")) feel.aBashLandsAsAnImpact = false;
            EndRow();
            if (feel.aBashLandsAsAnImpact)
            {
                feel.bashRocksTheTrunk = Lever("Bash rocks", feel.bashRocksTheTrunk, 0f, 0.6f, "How much of a bash's momentum rocks a trunk, against a sword blow's 1. Higher: every bash rocks both trunks further, and a small one comes nearer a hard one.");
                feel.bashRocksUpTo = Mathf.Round(Slide("Rocks up to", $"{feel.bashRocksUpTo:0} deg", "The furthest a body's impact may rock an upper body (the spine and the chest together). A sword blow's most is 34 degrees.", feel.bashRocksUpTo, 34f, 90f));
                feel.bashForceOnTheBody = Lever("Bash force", feel.bashForceOnTheBody, 0f, 0.5f, "How much of a bash's momentum is put on the physical upper body as force, on top of what the pose shows.");
            }

            BeginRow();
            Words(new GUIContent("A block jars:", "What a blow stopped on a shield does to the one who struck it (Combat Feel \"A Block Jars The Striker By All Of It\"). By all of it (new): what the shield took comes back at his weapon hand, equal and opposite, and rocks his trunk as it rocks the blocker's. By a part (old): a part of it, by \"Jar Scale\"."), GUILayout.Width(100f));
            if (Press(feel.aBlockJarsTheStrikerByAllOfIt ? "[the striker by all of it]" : "the striker by all of it", "Equal and opposite.")) feel.aBlockJarsTheStrikerByAllOfIt = true;
            if (Press(!feel.aBlockJarsTheStrikerByAllOfIt ? "[by a part (old)]" : "by a part (old)", "As before.")) feel.aBlockJarsTheStrikerByAllOfIt = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("What he holds:", "How a sword and shield fighter's hands follow the pose (Combat Feel \"What He Holds Moves Smoothly\"). Moves smoothly (new): the shield hand, and the sword hand while it is not striking, speed up and settle in toward where the pose has them, so nothing shows as a jump. As posed (old): each hand put where the pose has it every step."));
            if (Press(feel.whatHeHoldsMovesSmoothly ? "[moves smoothly]" : "moves smoothly", "Shield and sword follow the pose, settling in.")) feel.whatHeHoldsMovesSmoothly = true;
            if (Press(!feel.whatHeHoldsMovesSmoothly ? "[as posed (old)]" : "as posed (old)", "As before.")) feel.whatHeHoldsMovesSmoothly = false;
            EndRow();
            if (feel.whatHeHoldsMovesSmoothly)
            {
                feel.heldThingsSettleAt = Lever("Held settles at", feel.heldThingsSettleAt, 8f, 60f, "How quickly a held shield or sword follows its pose while nothing presses (1/s; three over it is about the time it takes to arrive). Lower: smoother, heavier and later.");
                feel.heldThingsSettleAtWhenPressed = Lever("Held settles at, pressed", feel.heldThingsSettleAtWhenPressed, 20f, 120f, "The same while the shield is raised to a blow or driven in a bash. Lower: a smoother raise that may come too late.");
            }

            BeginRow();
            Words(new GUIContent("His shield:", "Whether a fighter's own shield is a solid thing against his own body while he is on his feet (Combat Feel \"A Shield Is Stopped By Its Bearer's Body\"). Solid (new): the physics engine stops the board at his trunk, head, thighs and shins, as it did only while he fell. Passes through (old): only the pose keeps them apart."));
            if (Press(feel.aShieldIsStoppedByItsBearersBody ? "[solid against his body]" : "solid against his body", "The board is stopped by his own body.")) feel.aShieldIsStoppedByItsBearersBody = true;
            if (Press(!feel.aShieldIsStoppedByItsBearersBody ? "[passes through (old)]" : "passes through (old)", "As before.")) feel.aShieldIsStoppedByItsBearersBody = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("His sword:", "Whether a fighter's own sword is a solid thing against his own body while he is on his feet (Combat Feel \"A Sword Is Stopped By Its Bearer's Body\"). Solid (new): the blade is stopped at his trunk, head, thighs and shins. Passes through (old): as before."));
            if (Press(feel.aSwordIsStoppedByItsBearersBody ? "[solid against his body]" : "solid against his body", "The blade is stopped by his own body.")) feel.aSwordIsStoppedByItsBearersBody = true;
            if (Press(!feel.aSwordIsStoppedByItsBearersBody ? "[passes through (old)]" : "passes through (old)", "As before.")) feel.aSwordIsStoppedByItsBearersBody = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("His shield and his sword:", "What happens where a fighter's own sword and shield want the same space (Combat Feel \"The Shield Gives Way To The Sword\"). The shield gives way (new): where his sword arm or blade passes through his own board, now or in the moment ahead, the board steps aside and comes back after. Pass through each other (old): each arm posed alone."));
            if (Press(feel.theShieldGivesWayToTheSword ? "[the shield gives way]" : "the shield gives way", "The board steps out of the sword's path.")) feel.theShieldGivesWayToTheSword = true;
            if (Press(!feel.theShieldGivesWayToTheSword ? "[pass through each other (old)]" : "pass through each other (old)", "As before.")) feel.theShieldGivesWayToTheSword = false;
            EndRow();
            if (feel.theShieldGivesWayToTheSword)
            {
                feel.swordLane = Lever("Sword's lane", feel.swordLane, 0f, 0.2f, "How far clear of the board's rim the sword's path is kept (metres at a Medium's size). More: the shield stands further off the sword, and moves more.");
                feel.swordSeenAhead = Lever("Sword seen ahead", feel.swordSeenAhead, 0f, 0.5f, "How far ahead the sword's path is looked for (seconds). More: the shield opens sooner. Less: later and quicker.");
            }

            BeginRow();
            Words(new GUIContent("Against his body:", "How a shield or sword that is stopped by its bearer's own body is held there (Combat Feel \"A Held Thing Meets His Body Softly\"). Met softly (new): that arm presses with a third of its strength. Driven on (old): pressed into him with all the arm has, which makes it tremble."));
            if (Press(feel.aHeldThingMeetsHisBodySoftly ? "[met softly]" : "met softly", "The arm rests it there, pressing lightly.")) feel.aHeldThingMeetsHisBodySoftly = true;
            if (Press(!feel.aHeldThingMeetsHisBodySoftly ? "[driven on (old)]" : "driven on (old)", "Pressed in with all the arm has.")) feel.aHeldThingMeetsHisBodySoftly = false;
            EndRow();
            BeginRow();
            Words(new GUIContent("Caught on his own body:", "What happens to a shield or sword that was stopped on the wrong side of his own body, far from where his pose has it (Combat Feel \"A Held Thing Caught On His Own Body Is Let Through\"). Let through (new): after 0.4 s it passes through him once, back to its pose, and is solid against him again from then. Stays caught (old): it stays where it was stopped, his arm pressing it against the part it cannot pass. To see it: fell a shield bearer onto his side a few times and watch where the board is when he stands again."), GUILayout.Width(170f));
            if (Press(feel.aHeldThingCaughtOnHisOwnBodyIsLetThrough ? "[let through]" : "let through", "Back to its pose, through him once.")) feel.aHeldThingCaughtOnHisOwnBodyIsLetThrough = true;
            if (Press(!feel.aHeldThingCaughtOnHisOwnBodyIsLetThrough ? "[stays caught (old)]" : "stays caught (old)", "As before.")) feel.aHeldThingCaughtOnHisOwnBodyIsLetThrough = false;
            EndRow();
            Readout(new GUIContent("His own crossings: " + PhysicsBattler.Locomotion.SelfCrossings.Short() + ".", "Counted on the body as it is seen while a playtest is recorded: lines for the blade and the sword forearm against the board's disc, the board's rim and face against his own body. Strict: the meshes are thicker."), 2);
            Readout(new GUIContent("What he holds trembles: " + PhysicsBattler.Locomotion.HeldShake.Short() + ".", "A tremble is what is left of a place when its own smooth motion is taken off (the average of five steps about it). Read it old against new with the rows above; a strike reads high by its own quickening."), 2);
            BeginRow();
            Words(new GUIContent("Impacts:", "How a struck body comes back from any impact, a blade's or a bash's (Combat Feel \"Impacts Are Absorbed\"; a first trial). Absorbed (new): trunk and head go as far as before and settle without swinging back past; trunk, head and legs come back slower than they went; a blade stopped by a shield or a parry stays on it. As before (old): a struck body comes back on a spring that swings past and back again, and a stopped blade is thrown back off what stopped it."));
            if (Press(feel.impactsAreAbsorbed ? "[absorbed]" : "absorbed", "No swing back; slower back than out; the blade stays on the shield.")) feel.impactsAreAbsorbed = true;
            if (Press(!feel.impactsAreAbsorbed ? "[as before (old)]" : "as before (old)", "Springs that swing past; the blade thrown back off.")) feel.impactsAreAbsorbed = false;
            EndRow();
            if (feel.impactsAreAbsorbed)
            {
                feel.absorbedHowMuch = Lever("Absorbed how much", feel.absorbedHowMuch, 0f, 1f, "0: as before. 1: the most: no swing back at all, trunk, head and legs come back at half the speed they went, a stopped blade keeps a quarter of its spring back off a shield.");
            }

            CounterBashSettings(feel);
            if (feel.aShieldBashesWhatComesTooNear)
            {
                feel.shieldBashWithin = Lever("Bash within", feel.shieldBashWithin, 0.6f, 1.6f, "How near an enemy must come to be bashed (metres, root to root). A blow reaches at about 1.5 m; a blow on the move steps in to about 1 m.");
            }
        }

        /// <summary>A hard bash countered by a medium bash: the switch and how often the counter holds. Shown with the bash's settings and with the dance's.</summary>
        private void CounterBashSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel == null)
            {
                return;
            }

            BeginRow();
            Words(new GUIContent("Hard bash met:", "What one with a shield does when he sees a hard bash coming (Combat Feel \"A Hard Bash Is Countered By A Medium Bash\"). Countered (new): he braces and meets it with a medium bash of his own; whether that keeps him on his feet is a draw, \"Counter holds\". Nothing (old): a hard bash that meets him knocks him over every time. To see it: \"Hard bash\" at the other, both with shields."), GUILayout.Width(100f));
            if (Press(feel.aHardBashIsCounteredByAMediumBash ? "[countered]" : "countered", "He meets it with a medium bash of his own.")) feel.aHardBashIsCounteredByAMediumBash = true;
            if (Press(!feel.aHardBashIsCounteredByAMediumBash ? "[nothing (old)]" : "nothing (old)", "A hard bash knocks him over every time.")) feel.aHardBashIsCounteredByAMediumBash = false;
            EndRow();
            if (feel.aHardBashIsCounteredByAMediumBash)
            {
                feel.counterBashHolds = Mathf.Round(Slide("Counter holds", $"{feel.counterBashHolds:0} %", "How often a counter-bash stops a hard bash knocking him over (per cent). 100: every time. 0: never: he counters and goes down all the same. To be tied to his defensive skill later.", feel.counterBashHolds, 0f, 100f));
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
                    BeginRow();
                    Words(new GUIContent("Walking:", "What a fake lunge does when it is asked for while he walks. On the move (new): his legs go on walking and his hips dart toward the other and sink, and come back. Stops (old): he stops and lunges on the attack's own legs."), Styles().Text, GUILayout.Width(90f));
                    if (Press(feel.aFakeLungeIsThrownOnTheMove ? "[on the move]" : "on the move", "Thrown over the walk (Combat Feel \"A Fake Lunge Is Thrown On The Move\").")) feel.aFakeLungeIsThrownOnTheMove = true;
                    if (Press(!feel.aFakeLungeIsThrownOnTheMove ? "[stops]" : "stops", "He stops for it, as it was.")) feel.aFakeLungeIsThrownOnTheMove = false;
                    EndRow();
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
                Words(new GUIContent("He strikes:", "How a blow asked for while he walks is thrown. On the move: the walk goes on under the blow, its legs answering it (they are checked as the blade comes round, and step in so it reaches). Planted: he plants himself and lunges, the way he was going carried into the blow and run down under its wind-up. Stopped: he stops in one step and strikes from his guard, as it was before. Standing still, or stepping slowly in his stance, a blow is always planted."), Styles().Text);
                BeginRow();
                if (Press(way == 0 ? "[on the move]" : "on the move", "The walk goes on under the blow (Combat Feel \"Attacks Are Thrown On The Move\"). He must be walking: a pace of about 1 m/s or more.")) way = 0;
                if (Press(way == 1 ? "[planted]" : "planted", "He plants himself and lunges, his speed carried into the blow (Combat Feel \"Attacks Start From Where He Is\").")) way = 1;
                if (Press(way == 2 ? "[stopped (old)]" : "stopped (old)", "He stops dead and strikes from his guard, as it was.")) way = 2;
                EndRow();
                feel.attacksAreThrownOnTheMove = way == 0;
                feel.attacksStartFromWhereHeIs = way != 2;
                if (way == 0)
                {
                    BeginRow();
                    Words(new GUIContent("Out of reach:", "What a blow asked for on the move does when the walk cannot bring it within reach. On the move (new): it is thrown on the move all the same, and misses. Plants (old): he stops, plants himself and lunges for it."), Styles().Text, GUILayout.Width(90f));
                    if (Press(feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach ? "[on the move]" : "on the move", "Thrown on the move all the same; it misses (Combat Feel \"A Blow On The Move Stays On The Move Out Of Reach\").")) feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach = true;
                    if (Press(!feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach ? "[plants]" : "plants", "He plants himself and lunges for it, as it was.")) feel.aBlowOnTheMoveStaysOnTheMoveOutOfReach = false;
                    EndRow();
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
            Words(new GUIContent("It uncovers its chest: it lowers its shield.", "The part the target uncovers and the swordsman is to hit: its chest, by lowering its shield. (Its belly and hips sit behind the middle of the board; no clean move of the shield takes it off them, so they are left to the real postures.)"), Styles().Text);
            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("Swordsman:", "How he takes the opening. Reads it (new): he recognises the uncovering as it starts and times his blow to land while the part is open. He has to have seen that uncovering once: the first time, he can only react. Only reacts: he starts his blow once he sees the part open."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.theSwordsmanReadsOpenings ? "[reads it]" : "reads it", "He reads the uncovering (Combat Feel \"The Swordsman Reads Openings\").")) feel.theSwordsmanReadsOpenings = true;
                if (Press(!feel.theSwordsmanReadsOpenings ? "[only reacts]" : "only reacts", "He starts his blow once he sees the part open.")) feel.theSwordsmanReadsOpenings = false;
                if (Press("Forget", "He forgets every uncovering he has seen, and the pattern of the target's fakes: the next is new to him.")) { Shield.Taker.Forget(); Fakes.Forget(); }
                EndRow();
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
            BeginRow();
            Words(new GUIContent("Its pattern:", "Steady: always the same number of short fakes before the long one, so it can be counted. Mixed up: the number is drawn afresh each time (1 to 3), so counting tells nothing, and a reader is wrong as often as right."), Styles().Text, GUILayout.Width(80f));
            if (Press(!Fakes.MixedUp ? "[steady]" : "steady", "Always the same number of short fakes before the long one.")) Fakes.MixedUp = false;
            if (Press(Fakes.MixedUp ? "[mixed up]" : "mixed up", "The number of short fakes before the long one is drawn afresh each time.")) Fakes.MixedUp = true;
            EndRow();
            Fakes.Shorts = Mathf.RoundToInt(Slide("Short ones", $"{Fakes.Shorts}", "How many short fakes come before each long one (the steady pattern).", Fakes.Shorts, 1f, 4f));
            Fakes.Beat = Mathf.Round(10f * Slide("Beat", $"{Fakes.Beat:0.0} s", "How long from the start of one fake to the start of the next.", Fakes.Beat, 1.3f, 3f)) / 10f;
            if (feel != null)
            {
                feel.longFakeHold = Lever("Long holds", feel.longFakeHold, 0f, 0.6f, "How long the long fake is held at its furthest (seconds). The longer, the longer its chest is open.");
                BeginRow();
                Words(new GUIContent("In a fake:", "What a fighter's shield does while he is in a fake of his own (Combat Feel \"A Sword Fake Takes The Shield\"). Free (as it is): a fake is the sword arm's alone; the shield stays in its cover and meets a blow as readily as ever. Committed: it does not go to meet a blow until the fake is over, so a fake costs him his cover."), Styles().Text, GUILayout.Width(80f));
                if (Press(!feel.aSwordFakeTakesTheShield ? "[shield free]" : "shield free", "A fake is the sword arm's alone.")) feel.aSwordFakeTakesTheShield = false;
                if (Press(feel.aSwordFakeTakesTheShield ? "[shield committed]" : "shield committed", "In a fake his shield does not go to meet a blow.")) feel.aSwordFakeTakesTheShield = true;
                EndRow();
            }

            FakeMakingSettings(feel);
            Readout(new GUIContent("The pattern: " + Fakes.Learned + ".", "What the swordsman has learned of the target's pattern by watching it: nothing of it is given him."), 2);
            Readout(new GUIContent("Last fake: " + (Fakes.Now != null ? Fakes.Now.Text(Fakes.Spot) : "none yet") + ".", "The target's last fake: when it was furthest and how long it took, and when it left its chest open, as measured on the bodies (seconds after it began)."), 3);
            Readout(new GUIContent("Last blow: " + (Fakes.LastBlow != null ? Fakes.LastBlow.Verdict : "none yet."), "The swordsman's last blow in this test: what he threw it by, what it met, and whether the chest was open as it met."), 4);
            Readout(Fakes.Fakes.Count > 0 ? Fakes.Tally() : "", 2);
        }

        /// <summary>
        /// How a fake is made and how the other answers it (18 November): a real attack cut short, where it is cut and
        /// by how much that varies, which attacks it is cut from; how often it is taken for a blow, and dodged. Shown
        /// with the fakes test and with the dance: the same settings in both.
        /// </summary>
        private void FakeMakingSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel == null)
            {
                return;
            }

            BeginRow();
            Words(new GUIContent("A fake is:", "How a sword fake is made (Combat Feel \"A Fake Is A Real Attack Cut Short\"). A real attack cut short (new): a light or heavy attack, made for its target as any blow is, checked on its way there and taken straight back to guard. It makes no swing sound and can hit nothing. The old feint: an attack's wind-up taken back before the blade comes forward."), GUILayout.Width(80f));
            if (Press(feel.aFakeIsARealAttackCutShort ? "[a real attack cut short]" : "a real attack cut short", "A real attack, checked on its way to the target.")) feel.aFakeIsARealAttackCutShort = true;
            if (Press(!feel.aFakeIsARealAttackCutShort ? "[the old feint]" : "the old feint", "The wind-up only, taken back.")) feel.aFakeIsARealAttackCutShort = false;
            EndRow();
            if (feel.aFakeIsARealAttackCutShort)
            {
                feel.fakeCutOff = Lever("Fake cut-off", feel.fakeCutOff, 0.05f, 0.95f, "Where a fake is cut off, of the attack's way from the moment its hips start in (0) to where it would land (1). Lower: checked early, hardly out of the wind-up. Higher: the blade is nearly on the other before it is checked.");
                feel.fakeCutOffVaries = Lever("Cut-off varies", feel.fakeCutOffVaries, 0f, 0.45f, "How far either side of the cut-off a fake may be cut, drawn afresh for each fake. 0: always the same spot. 0.2 with a cut-off of 0.5: anywhere from 0.3 to 0.7.");
                BeginRow();
                Words(new GUIContent("Cut from:", "Which attacks fakes are cut from (Combat Feel \"Fakes Cut From\"). Light (as it is): quick, with a short wind-up; the heavy ones did not look right as fakes."), GUILayout.Width(80f));
                if (Press(feel.fakesCutFrom == 0 ? "[light]" : "light", "Light attacks only.")) feel.fakesCutFrom = 0;
                if (Press(feel.fakesCutFrom == 1 ? "[heavy]" : "heavy", "Heavy attacks only.")) feel.fakesCutFrom = 1;
                if (Press(feel.fakesCutFrom == 2 ? "[both]" : "both", "Light and heavy attacks, drawn for each fake.")) feel.fakesCutFrom = 2;
                EndRow();
                BeginRow();
                Words(new GUIContent("Wind-up:", "Which attacks a fake may be cut from, by their wind-up (Combat Feel \"Fakes Come Only From Wind Ups Behind Him\"). Behind him (new): only attacks whose wind-up draws the sword arm back behind him; one that sets the sword before his chest first, and a thrust, are not faked. Any (old): from any attack."), GUILayout.Width(80f));
                if (Press(feel.fakesComeOnlyFromWindUpsBehindHim ? "[behind him only]" : "behind him only", "Only attacks that wind up behind him are faked.")) feel.fakesComeOnlyFromWindUpsBehindHim = true;
                if (Press(!feel.fakesComeOnlyFromWindUpsBehindHim ? "[any (old)]" : "any (old)", "Any attack may be faked.")) feel.fakesComeOnlyFromWindUpsBehindHim = false;
                EndRow();
                feel.aFakeIsTakenForABlow = Lever("Fake believed", feel.aFakeIsTakenForABlow, 0f, 1f, "How often the other takes a fake for a real blow and answers it: his shield or sword goes to meet it, or he dodges. For the look only so far: being fooled costs him nothing yet.");
                feel.aFakeTakenForABlowIsDodged = Lever("Fake dodged", feel.aFakeTakenForABlowIsDodged, 0f, 1f, "Of the fakes he takes for a blow, how many he dodges; the rest he blocks. In a gesture of his own he lets it go to dodge; in a blow of his own he cannot.");
                Readout(new GUIContent(PostureMaker.LastCutOf.Length > 0 ? $"Last fake: {PostureMaker.LastCutOf.Replace("made_", "").Replace("_", " ")}, cut at {PostureMaker.LastCut:0.00} of its way." : "Last fake: none yet.", "The attack the last fake was cut from, and where it was cut (0: as its hips start in; 1: where it would land)."), 1);
            }

            BeginRow();
            Words(new GUIContent("Dodges:", "To see the dodges. As set: a fake is dodged as often as the levers above say, and a real blow in the dance as often as each fighter's own \"Dodges\" lever says (high blows only). Every one: every fake is taken for a blow and dodged, and in the dance every real blow that is seen in time is dodged too. A switch for looking, not for play."), GUILayout.Width(80f));
            if (Press(!ClipFighter.DodgesEvery ? "[as set]" : "as set", "As the levers say.")) ClipFighter.DodgesEvery = false;
            if (Press(ClipFighter.DodgesEvery ? "[every one (to see them)]" : "every one (to see them)", "Every fake and every blow seen in time is dodged.")) ClipFighter.DodgesEvery = true;
            EndRow();
        }

        /// <summary>The dance's settings and what was measured: each fighter's pattern, whether he reads, what he has learned, how it goes.</summary>
        private void DanceSettings(PhysicsBattler.Locomotion.CombatFeel feel)
        {
            if (feel != null)
            {
                BeginRow();
                Words(new GUIContent("Reading:", "Whether fighters read each other's patterns at all (Combat Feel \"Fighters Read Each Other's Patterns\"). Off (old): nobody reads, whatever he is set to below."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.fightersReadEachOthersPatterns ? "[on]" : "on", "A fighter set to read learns the other's pattern and uses it.")) feel.fightersReadEachOthersPatterns = true;
                if (Press(!feel.fightersReadEachOthersPatterns ? "[off (old)]" : "off (old)", "Nobody reads.")) feel.fightersReadEachOthersPatterns = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Their feet:", "Whether the dance is paced by the gap between them (Combat Feel \"The Dance Is Paced By The Gap\"). Paced (new): each keeps the distance he likes, goes round the other in spells, comes in for his blow and out after it, and steps out or stands for a blow he expects; how restless and how aggressive he is are his own levers (\"Each his own\"). On their spots (old): they stand where they are."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.theDanceIsPacedByTheGap ? "[paced by the gap]" : "paced by the gap", "They move about each other.")) feel.theDanceIsPacedByTheGap = true;
                if (Press(!feel.theDanceIsPacedByTheGap ? "[on their spots (old)]" : "on their spots (old)", "They stand where they are.")) feel.theDanceIsPacedByTheGap = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Setting off:", "How often they set off (Combat Feel \"The Dance Asks For Steps\"). Calm (new): a dancer does not set off in or out for less than a step's worth, goes all the way there once he has, and stands a moment once he has stopped. Restless (old): he sets off for 12 cm and stops 12 cm short."), GUILayout.Width(80f));
                if (Press(feel.theDanceAsksForSteps ? "[calm]" : "calm", "Calmer feet.")) feel.theDanceAsksForSteps = true;
                if (Press(!feel.theDanceAsksForSteps ? "[restless (old)]" : "restless (old)", "As before.")) feel.theDanceAsksForSteps = false;
                EndRow();
                FakeMakingSettings(feel);
                BeginRow();
                Words(new GUIContent("Fakes:", "How many fakes a dancer throws (Combat Feel \"The Dance Keeps Fakes To A Ratio\"). To his ratio (new): so many fake attacks for each real one, his own lever \"Fakes\" under \"Each his own\" (0.5: one fake for every two real attacks); a fake over that is left out and his real attack comes sooner. All (old): every fake his pattern has."), GUILayout.Width(80f));
                if (Press(feel.theDanceKeepsFakesToARatio ? "[to his ratio]" : "to his ratio", "So many fakes for each real attack.")) feel.theDanceKeepsFakesToARatio = true;
                if (Press(!feel.theDanceKeepsFakesToARatio ? "[all his pattern has (old)]" : "all his pattern has (old)", "Every fake his pattern has.")) feel.theDanceKeepsFakesToARatio = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Too near:", "What happens when the two come too near each other in the dance (Combat Feel \"The Dance Bashes For Space\"). One bashes (new): once, to get the other back out. If one came in at the other, the one come in at bashes, small or medium; if they drifted together, one throws a small bash to push them apart. Both bash (old): each bashes whenever he can."), GUILayout.Width(80f));
                if (Press(feel.theDanceBashesForSpace ? "[one bashes him out]" : "one bashes him out", "The one come in at bashes; drifted together, one of them.")) feel.theDanceBashesForSpace = true;
                if (Press(!feel.theDanceBashesForSpace ? "[both bash (old)]" : "both bash (old)", "As before.")) feel.theDanceBashesForSpace = false;
                EndRow();
                if (feel.theDanceBashesForSpace)
                {
                    feel.spaceBashesThatAreMedium = Lever("Medium bashes", feel.spaceBashesThatAreMedium, 0f, 1f, "Of the bashes at one who came in at him, how many are a medium bash (throws him back stumbling); the rest are small (a shove).");
                }

                BeginRow();
                Words(new GUIContent("Hard bash:", "Whether a hard bash is thrown in the dance (Combat Feel \"A Hard Bash Is An Attack In The Dance\"). An attack (new): some of a dancer's own blows are a hard bash, to break the other's guard: his own lever \"Guard breaks\" under \"Each his own\", twice as often after a blow the other stopped. Never (old)."), GUILayout.Width(80f));
                if (Press(feel.aHardBashIsAnAttackInTheDance ? "[an attack]" : "an attack", "Some of his blows are a hard bash.")) feel.aHardBashIsAnAttackInTheDance = true;
                if (Press(!feel.aHardBashIsAnAttackInTheDance ? "[never (old)]" : "never (old)", "No hard bash in the dance.")) feel.aHardBashIsAnAttackInTheDance = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("An opening (dance):", "What a dancer does when the other stumbles, has his guard knocked open or turns his back (Combat Feel \"The Dance Pounces On An Opening\"). He pounces (new): he runs in and strikes in one motion, by himself. He keeps to his pattern (old): as before."));
                if (Press(feel.theDancePouncesOnAnOpening ? "[he pounces]" : "he pounces", "He runs in and strikes in one motion.")) feel.theDancePouncesOnAnOpening = true;
                if (Press(!feel.theDancePouncesOnAnOpening ? "[he keeps to his pattern (old)]" : "he keeps to his pattern (old)", "As before.")) feel.theDancePouncesOnAnOpening = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("A man down (dance):", "What a dancer does when the other is knocked off his feet (Combat Feel \"The Dance Strikes A Man Who Is Down\"). He goes for him (new): he runs to him and stabs down from beside him. He lets him up (old): he waits until the other stands. Only the dance: in the duel lab \"A man who is down:\" decides. You decided on 20 November that fighters go for him."));
                if (Press(feel.theDanceStrikesAManWhoIsDown ? "[he goes for him]" : "he goes for him", "He runs to him and stabs down.")) feel.theDanceStrikesAManWhoIsDown = true;
                if (Press(!feel.theDanceStrikesAManWhoIsDown ? "[he lets him up (old)]" : "he lets him up (old)", "As before.")) feel.theDanceStrikesAManWhoIsDown = false;
                EndRow();
                BeginRow();
                Words(new GUIContent("Kicks (dance):", "Whether dancers kick by themselves (Combat Feel \"The Dance Kicks\"). They kick (new): a fighter with no shield kicks a man who comes too near back out and throws a hard kick as his guard-breaker; one with a shield kicks instead of bashing some of the times. Nobody kicks (old): as before."));
                if (Press(feel.theDanceKicks ? "[they kick]" : "they kick", "Kicks for space and as an attack.")) feel.theDanceKicks = true;
                if (Press(!feel.theDanceKicks ? "[nobody kicks (old)]" : "nobody kicks (old)", "As before.")) feel.theDanceKicks = false;
                EndRow();
                if (feel.theDanceKicks)
                {
                    feel.danceKicksInsteadOfBashing = Lever("Kicks instead of bashing", feel.danceKicksInsteadOfBashing, 0f, 1f, "Of the bashes a fighter with a shield would throw in the dance, how many are a kick instead.");
                }

                CounterBashSettings(feel);
                if (feel.theDanceAsksForSteps)
                {
                    feel.danceStepsWorth = Lever("A step's worth", feel.danceStepsWorth, 0.12f, 0.8f, "The least ground a dancer sets off in or out for (metres).");
                    feel.danceStandsAtLeast = Lever("Stands at least", feel.danceStandsAtLeast, 0f, 2f, "Once stopped, the least he stands before he sets off again (seconds).");
                }
                BeginRow();
                Words(new GUIContent("Stopped:", "What happens when a blow is stopped by a shield or a sword. Answered (new): the one who stopped it strikes back at once, while the other is still in his blow; it is what a blow wasted on a fake costs (Combat Feel \"A Stopped Blow Is Answered\"). Nothing (old): a stopped blow costs nothing."), Styles().Text, GUILayout.Width(80f));
                if (Press(feel.aStoppedBlowIsAnswered ? "[answered]" : "answered", "The one who stopped it strikes back at once.")) feel.aStoppedBlowIsAnswered = true;
                if (Press(!feel.aStoppedBlowIsAnswered ? "[nothing (old)]" : "nothing (old)", "A stopped blow costs nothing.")) feel.aStoppedBlowIsAnswered = false;
                EndRow();
            }

            string[] whose = { "Him", "The other" };
            var both = new[] { Dancing.A, Dancing.B };
            for (int k = 0; k < 2; k++)
            {
                var d = both[k];
                BeginRow();
                Words(new GUIContent(whose[k] + ":", "His pattern of gestures. Each made-up fighter has one of his own; pressing Dance the first time gives each the one that goes with his own levers (\"Each his own\")."), Styles().Text, GUILayout.Width(80f));
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

                EndRow();
                BeginRow();
                Gap(84f);
                if (Press(!d.Pattern.MixedUp ? "[plain]" : "plain", "Always the same number of gestures before his tell, and his blow always follows it: a poorer swordsman's pattern, which can be counted.")) d.Pattern.MixedUp = false;
                if (Press(d.Pattern.MixedUp ? "[mixed up]" : "mixed up", "The number of gestures before his tell is drawn afresh each time (1 to 3), and one tell in three has no blow after it: counting tells nothing.")) d.Pattern.MixedUp = true;
                if (Press(d.Reads ? "[reads]" : "reads", "He learns the other's pattern by watching and uses it.")) d.Reads = true;
                if (Press(!d.Reads ? "[only reacts]" : "only reacts", "He gestures on regardless, and goes for a chest he sees open.")) d.Reads = false;
                EndRow();
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
                Readout(new GUIContent("By himself: " + Dancing.OwnMoves(d) + ". Last pounce: " + d.Run.Pounced + ". Last blow at a man down: " + d.Run.DownedStruck + ".", "What he did of his own accord in this dance: pounces on an opening, blows at the other while he was down, kicks."), 4);
                Readout(new GUIContent(Dancing.Gestures(d) > 0 ? $"His fakes: {d.FakesThrown} thrown, {d.FakesLeftOut} left out by his ratio. His bashes: " + Dancing.Bashes(d) + "." : "", "His fake attacks, and those his pattern had that his ratio of fakes to real attacks left out. His bashes: those to get the other out of his space (small, medium; how many at one who had come in at him), his hard bashes thrown as an attack and what came of them, and his own counter-bashes against a hard bash."), 3);
            }

            Readout(new GUIContent(Dancing.On && Dancing.GapSteps > 0 ? $"Now {Dancing.Gap:0.00} m apart; he is {(Dancing.A.Tempo.Length > 0 ? Dancing.A.Tempo : "standing")}, the other {(Dancing.B.Tempo.Length > 0 ? Dancing.B.Tempo : "standing")}. So far: {Dancing.Feet()}." : "", "The gap between them now, what each is about (coming in from far off; probing at the edge of reach; pressing, when the fight goes his way; shelled up, when it goes against him), and the gap and their walking over the whole dance."), 4);
            Readout(new GUIContent(Dancing.Ready ? $"How the fight is going: him {Dancing.A.Going(Time.time):+0;-0;0}, the other {Dancing.B.Going(Time.time):+0;-0;0}." : "", "Blows landed less blows taken, over the last 20 seconds. The first number the fight keeps of how it is going for each: what a fighter does next will come to hang on it."), 1);
        }

        /// <summary>The duel lab's orders: walk to, back away, stand, circle.</summary>
        private void DuelMoveButtons()
        {
            BeginRow();
            if (Press("Walk to", $"He walks up to the other and stands {Duel.Near:0.0} m from it.")) Duel.Give(DuelLab.Order.WalkTo);
            if (Press("Back away", $"He backs away from the other, facing it, until he is {Duel.Far:0.0} m from it.")) Duel.Give(DuelLab.Order.BackAway);
            if (Press("Stand", "He stands where he is.")) Duel.Give(DuelLab.Order.Stand);
            EndRow();
            BeginRow();
            if (Press("Circle left", "He goes round the other to his own left, facing it, at the distance he is at now, until told otherwise.")) Duel.Give(DuelLab.Order.CircleLeft);
            if (Press("Circle right", "He goes round the other to his own right, facing it, at the distance he is at now, until told otherwise.")) Duel.Give(DuelLab.Order.CircleRight);
            EndRow();
        }

        /// <summary>The duel lab's leaps and hops.</summary>
        private void DuelLeapButtons()
        {
            BeginRow();
            if (Press("Leap in", "He springs toward the other.")) Duel.Give(DuelLab.Order.LeapIn);
            if (Press("Leap out", "He springs back from the other, facing it.")) Duel.Give(DuelLab.Order.LeapOut);
            if (Press("Hop left", "He springs to his own left.")) Duel.Give(DuelLab.Order.HopLeft);
            if (Press("Hop right", "He springs to his own right.")) Duel.Give(DuelLab.Order.HopRight);
            EndRow();
        }

        /// <summary>The duel lab's drop.</summary>
        private void DuelDropButton()
        {
            BeginRow();
            if (Press("Drop", "He dips now.") && Duel.Mover != null && Duel.Mover.Clip != null)
            {
                bool dropped = Duel.Mover.Clip.Drop();
                message = dropped ? "Duel lab: he drops." : "Duel lab: he cannot drop just now (he is in a blow or a leap, or one is under way).";
                if (dropped)
                {
                    Duel.WatchOpenings("drop");
                }
            }

            EndRow();

            BeginRow();
            if (Press("Dodge left", "He ducks and leans to his left, his hips going that way, and comes back up; his feet stay where they are and his shield goes with his chest. Rebuilt from the recordings \"Dodging\".") && Duel.Mover != null && Duel.Mover.Clip != null)
            {
                message = Duel.Mover.Clip.Dodge(-1) ? "Dodge left." : "Dodge: he could not (in a blow, a drop under way, or down).";
            }

            if (Press("Dodge right", "He ducks and leans to his right, and comes back up.") && Duel.Mover != null && Duel.Mover.Clip != null)
            {
                message = Duel.Mover.Clip.Dodge(1) ? "Dodge right." : "Dodge: he could not (in a blow, a drop under way, or down).";
            }

            EndRow();
        }

        /// <summary>The duel lab's bluffs of blade and shield, and the guard's height.</summary>
        private void DuelBluffButtons()
        {
            var poser = Duel.Mover != null ? Duel.Mover.Clip : null;
            BeginRow();
            if (Press("Blade dip", "The point dips and comes back.") && poser != null && poser.Bluff(PhysicsBattler.Locomotion.Locomotor.Pulse.BladeDip)) Duel.WatchOpenings("blade dip");
            if (Press("Blade flick", "The point flicks aside and comes back.") && poser != null && poser.Bluff(PhysicsBattler.Locomotion.Locomotor.Pulse.BladeFlick)) Duel.WatchOpenings("blade flick");
            if (Press("Blade circle", "The point goes once round.") && poser != null && poser.Bluff(PhysicsBattler.Locomotion.Locomotor.Pulse.BladeCircle)) Duel.WatchOpenings("blade circle");
            // (No shield press: the shield keeps its cover and does not gesture. The playtest of 17 November.)
            EndRow();
            BeginRow();
            Words(new GUIContent("Guard:", "Where he carries his sword arm: as the recordings have it, higher, or lower. He stays in it until it is changed."), Styles().Text, GUILayout.Width(60f));
            if (Press(duelGuard == 1 ? "[high]" : "high", "The sword arm carried high.")) duelGuard = 1;
            if (Press(duelGuard == 0 ? "[middle]" : "middle", "As the recordings have it.")) duelGuard = 0;
            if (Press(duelGuard == -1 ? "[low]" : "low", "The sword arm carried low.")) duelGuard = -1;
            EndRow();
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
            Words(new GUIContent("A press is:", "What pressing a part of the body asks him for. A blow: the attack. The other three are postures, the short bursts a fighter throws to draw the other out; each is the beginning of the very attack he would have made at that part, taken back. A feint: his sword arm and chest go into the attack's wind-up and come back. A fake lunge: his legs and hips go half way into the attack's lunge and come back, his arms staying in his guard (from a walk he stops for it). Both: a feint on a fake lunge, which reads as a blow he means."), Styles().Text);
            BeginRow();
            if (Press(duelPosture == 0 ? "[a blow]" : "a blow", "The attack itself.")) duelPosture = 0;
            if (Press(duelPosture == 1 ? "[a feint]" : "a feint", "The attack's wind-up, taken back. Works standing and on the move.")) duelPosture = 1;
            if (Press(duelPosture == 2 ? "[a fake lunge]" : "a fake lunge", "The attack's lunge, half way and back, his arms in his guard.")) duelPosture = 2;
            if (Press(duelPosture == 3 ? "[both]" : "both", "A feint on a fake lunge.")) duelPosture = 3;
            EndRow();

            BeginRow();
            Words(new GUIContent("The blow is:", "The class of the blow a press throws (Combat Feel \"Blows Are Light Or Heavy\"). Plain: as every blow was. Light: fast and less powerful, little wind-up, back at once. Heavy: slow and powerful, wound far back and slowly, carried all the way through. The same recorded cut is made each way."), Styles().Text, GUILayout.Width(90f));
            if (Press(Aim.Kind == PhysicsBattler.Locomotion.AttackClass.Plain ? "[plain]" : "plain", "As every blow was.")) Aim.Kind = PhysicsBattler.Locomotion.AttackClass.Plain;
            if (Press(Aim.Kind == PhysicsBattler.Locomotion.AttackClass.Light ? "[light]" : "light", "Fast and less powerful.")) Aim.Kind = PhysicsBattler.Locomotion.AttackClass.Light;
            if (Press(Aim.Kind == PhysicsBattler.Locomotion.AttackClass.Heavy ? "[heavy]" : "heavy", "Slow and powerful, with a long wind-up.")) Aim.Kind = PhysicsBattler.Locomotion.AttackClass.Heavy;
            EndRow();
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
            BeginRow();
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
            EndRow();
        }

        /// <summary>The fakes test's buttons.</summary>
        private void FakeTestButtons()
        {
            BeginRow();
            if (Press("Short fake", "The target throws one short fake slash now. (Fakes being real attacks cut short: this one is cut at the near end of the range, the cut-off less what it varies by.) (With no sword and shield on the target, the lab is set out for it first.)") && !SetOutFirst(3) && !Fakes.Throw(false, Time.time)) message = "Fakes test: not now: " + (Fakes.Cannot.Length > 0 ? Fakes.Cannot : "the target cannot") + ".";
            if (Press("Long fake", "The target throws one long fake slash now. (Fakes being real attacks cut short: this one is cut at the far end of the range, the cut-off plus what it varies by, and held there.) By itself it has no pattern before it, so a reader has nothing to go by and lets it go; one who only reacts goes for it.") && !SetOutFirst(4) && !Fakes.Throw(true, Time.time)) message = "Fakes test: not now: " + (Fakes.Cannot.Length > 0 ? Fakes.Cannot : "the target cannot") + ".";
            if (Press(Fakes.Pattern ? "[Pattern]" : "Pattern", "The target plays its pattern round and round: so many short fakes, then a long one, one on each beat. Struck, or off its spot, it begins the pattern again once it is back.") && (Fakes.Pattern || !SetOutFirst(5)))
            {
                Fakes.Pattern = !Fakes.Pattern;
                Fakes.Restart();
            }

            EndRow();
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
            // (What was measured is no setting: left out while searching.)
            if (filtering)
            {
                return;
            }

            var style = Styles().Text;
            GUILayout.Label(what, style, GUILayout.Height(lines * Mathf.Max(14f, style.lineHeight) + 4f));
            // (A readout he is asked to watch, by what it is called: its words up to the colon.)
            int colon = what.text != null ? what.text.IndexOf(':') : -1;
            if (colon > 0 && Looked(what.text.Substring(0, colon)))
            {
                FrameLast();
            }
        }

        private void Readout(string what, int lines) => Readout(new GUIContent(what), lines);

        /// <summary>
        /// The actions window (the duel lab): everything that has one of the two fighters do something, in one place.
        /// The pace stays at its top; under it, in a view that scrolls when it is taller than the screen, each
        /// action's buttons and, folded under them, that action's own settings and what was measured of it.
        /// </summary>
        private void DrawActions(int id)
        {
            ResetDraw();
            DuelPace();
            Readout($"Order: {Duel.Now}. {Duel.Gap:0.00} m apart, going {Duel.Speed:0.00} m/s.", 1);
            float room = Screen.height / Mathf.Max(0.1f, uiScale) - actionsRect.y - actionsChrome - 8f;
            actionsView = actionsContent < 0f ? Mathf.Max(160f, room) : Mathf.Clamp(actionsContent + 4f, 60f, Mathf.Max(160f, room));
            actionsScroll = GUILayout.BeginScrollView(new Vector2(0f, actionsScroll.y), false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(actionsView));
            rowRoom = ActionsWidth - ViewEdges;
            GUILayout.BeginVertical(GUILayout.Width(rowRoom));
            BeginFiltered();
            if (filtering)
            {
                GUILayout.Label(new GUIContent($"Only what has \"{searchShown}\" in its name (the panel's search).", "The search box at the top of the panel decides what is shown here too. Clear it for all the actions."), Styles().Hint);
            }

            ActionGroups(true);
            Gap(6f);
            if (Press("Put these back in the panel", "The buttons and their settings go back into the panel, as they were, and this window closes. \"Actions: in a window\" at the top of the duel lab brings it back."))
            {
                ActionsApart = false;
            }

            EndFiltered();
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

            // (Not while a target is down or getting up: its spot goes with it then, and a tube set beside where it lay
            // would wait in the wrong place.)
            if (ready && !Tubes.Armed && !down)
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
                    StepStumbleBash();
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

            // (Each fighter's blade path through his last attack, the combos plan's step 1: over the game view, under the panel.)
            if (PhysicsBattler.Locomotion.BladePath.On && Camera.main != null)
            {
                var paths = PhysicsBattler.Locomotion.BladePath.Shown;
                for (int i = paths.Count - 1; i >= 0; i--)
                {
                    // (A fighter the lab has cleared away: his path goes with him.)
                    if (paths[i].Who == null || paths[i].Who.FightUnit == null)
                    {
                        paths.RemoveAt(i);
                        continue;
                    }

                    BladePathView.Draw(paths[i], Camera.main);
                }
            }

            // (The panel and the units' labels scaled together, text and buttons alike: at the screen's own size they
            // were too small to read.)
            var was = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
            if (searchFocused && Event.current != null && Event.current.type == EventType.MouseDown && !panelRect.Contains(Event.current.mousePosition))
            {
                // (A press outside the panel: the keys are the game's again.)
                GUIUtility.keyboardControl = 0;
                searchFocused = SandboxInput.Typing = false;
            }

            // (The duel lab's actions in a window of their own, at the right of the screen until it is dragged.)
            if (Event.current != null && Event.current.type == EventType.Layout)
            {
                LatchSearch();
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
            public GUIStyle Section, Fold, Heading, Text, Value, Toggle, Hint, Box, Cell, Button, Search, SearchHint;
            public Texture2D Flat, FlatOver, FlatDown;
        }

        // (What the panel shows, as it was when the panel was last laid out: a press that changes it (a test begun, a
        // recording stopped) shows from the next layout, so every event draws the controls that were laid out for it.)
        private bool shownTube, shownTarget, shownAim, shownDuel, shownRecording, shownFeel;
        private string shownMessage = "";

        private PanelStyles panel;
        private Vector2 panelScroll;
        private float panelContent = -1f, panelChrome = 150f, panelView;
        private readonly Dictionary<string, bool> sectionOpen = new Dictionary<string, bool>();

        private static Texture2D FlatTexture(float grey)
        {
            var made = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            made.SetPixel(0, 0, new Color(grey, grey, grey, 1f));
            made.Apply();
            return made;
        }

        private PanelStyles Styles()
        {
            if (panel != null && panel.Flat != null)
            {
                return panel;
            }

            // (Made once: a style made each time the panel is drawn is thrown away each time.)
            panel = new PanelStyles
            {
                // (Flat, so the colour of the part of the panel they are in shows on them: see Zone.)
                Flat = FlatTexture(0.8f),
                FlatOver = FlatTexture(1f),
                FlatDown = FlatTexture(0.6f),
                Section = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold },
                Fold = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft },
                Button = new GUIStyle(GUI.skin.button),
                // (A heading is found at a glance: larger, in its own colour, on a tinted bar across the panel. See Heading.)
                Heading = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = true, fontSize = Mathf.RoundToInt(1.15f * (GUI.skin.label.fontSize > 0 ? GUI.skin.label.fontSize : 13)), padding = new RectOffset(8, 4, 3, 3) },
                Cell = new GUIStyle(GUI.skin.label) { wordWrap = false, clipping = TextClipping.Clip },
                Text = new GUIStyle(GUI.skin.label) { wordWrap = true },
                Value = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight },
                Toggle = new GUIStyle(GUI.skin.toggle) { wordWrap = true },
                Hint = new GUIStyle(GUI.skin.label) { wordWrap = true, alignment = TextAnchor.UpperLeft, fontStyle = FontStyle.Italic },
                Box = new GUIStyle(GUI.skin.box),
                Search = new GUIStyle(GUI.skin.textField),
                SearchHint = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Italic, wordWrap = false, clipping = TextClipping.Clip, padding = new RectOffset(6, 4, 2, 2) },
            };
            panel.Heading.normal.textColor = Color.white;
            panel.SearchHint.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
            foreach (var flat in new[] { panel.Section, panel.Fold, panel.Button })
            {
                flat.border = new RectOffset(0, 0, 0, 0);
                flat.normal.background = flat.focused.background = flat.onNormal.background = flat.onFocused.background = panel.Flat;
                flat.hover.background = flat.onHover.background = panel.FlatOver;
                flat.active.background = flat.onActive.background = panel.FlatDown;
                flat.normal.textColor = flat.hover.textColor = flat.active.textColor = flat.focused.textColor = Color.white;
            }

            return panel;
        }

        // ---- A colour for each part of the panel (18 November) ----
        // Each heading, and each section's bar, has a colour of its own; the buttons under it, down to the next
        // heading, are a darker and greyer tone of the same colour. The colour comes from the heading's name, so a
        // part of the panel is the same colour every time; two that follow each other are never alike.

        /// <summary>The colours the parts of the panel are given, as hues (0 to 1), in order round the colour wheel.</summary>
        private static readonly float[] ZoneHues = { 0.115f, 0.21f, 0.33f, 0.44f, 0.52f, 0.59f, 0.68f, 0.78f, 0.9f, 0.02f };

        // (The part of the panel being drawn: the hue its buttons take; below 0, none yet: buttons as the skin has them.)
        private int zone = -1;
        private string zoneName = "";

        /// <summary>The colour of the part of the panel with this name: from its name, moved on if it is like the one before it.</summary>
        private int ZoneOf(string name)
        {
            uint hash = 2166136261u;
            foreach (char c in name)
            {
                hash = (hash ^ c) * 16777619u;
            }

            int n = ZoneHues.Length, at = (int)(hash % (uint)n);
            for (int tries = 0; tries < n && zone >= 0; tries++)
            {
                int apart = Mathf.Abs(at - zone);
                if (Mathf.Min(apart, n - apart) >= 2)
                {
                    break;
                }

                at = (at + 3) % n;
            }

            return at;
        }

        private void EnterZone(string name)
        {
            zone = ZoneOf(name);
            zoneName = name;
        }

        private static Color ZoneTone(int zone, float saturation, float value, float alpha = 1f)
        {
            var c = Color.HSVToRGB(ZoneHues[Mathf.Clamp(zone, 0, ZoneHues.Length - 1)], saturation, value);
            c.a = alpha;
            return c;
        }

        /// <summary>A button in the colour of the part of the panel it is in: a darker, greyer tone of its heading's colour; brighter if it is the one chosen (its name in square brackets).</summary>
        private bool ZoneButton(GUIContent content, GUIStyle plain, GUILayoutOption[] options)
        {
            if (zone < 0)
            {
                return plain != null ? GUILayout.Button(content, plain, options) : GUILayout.Button(content, options);
            }

            string text = content.text ?? "";
            bool chosen = text.Length > 2 && text[0] == '[' && text[text.Length - 1] == ']';
            var was = GUI.backgroundColor;
            GUI.backgroundColor = chosen ? ZoneTone(zone, 0.62f, 0.76f) : ZoneTone(zone, 0.5f, 0.5f);
            bool pressed = GUILayout.Button(content, Styles().Button, options);
            GUI.backgroundColor = was;
            return pressed;
        }

        // ---- Look here (19 November; Renier's standing rule) ----
        // "When you tell me how to test your changes, add highlights in the UI to actually show me the buttons and
        // areas of the UI I should be looking at." Every push that needs his eye sets the three lists below: the steps
        // in plain words (shown in a box at the top of the panel), the names of the things to press and watch (each
        // gets a bright frame wherever it is drawn: a lab's button in the bar, a section's bar, a heading, a row by
        // its name, a button, a tick, a slider), and a tag that names the push. A row is framed whole by its name.
        // Names are matched whole, without a chosen button's square brackets; capitals do not matter.

        /// <summary>Names this push's list. Change it with every push that needs his eye: his "highlights off" is kept by tag, so a new list is shown again.</summary>
        private const string LookTag = "21 November, 13:57: the big one hits harder; a new spin from your recording; the first blows keep him in reach";

        /// <summary>What to do, in order, in plain words.</summary>
        private static readonly string[] LookSteps =
        {
            "1. Go to \"Duel\", find \"Strike at\", and press \"Throw combo\" a good many times (\"Last blow: forehand\"). New: the big one has its own speed, \"Speed: big one\" (1). However fast \"Speed: combos\" is, the last blow takes its own time: a wait, then the cut. It is also swung bigger and goes right through him.",
            "2. Press \"spin\" in \"Last blow:\" and throw again. It is a new spin, made from the recording you sent: he turns right round on the spot, the sword over his head, and cuts across low. Tell me if he turns far enough now and if the cut hits hard enough. He ends very close to the dummy.",
            "3. New: the first two blows no longer knock the dummy away or down (row \"Before the big one, a blow:\"). They push him back a little, so the big one finds him where it expects him. To compare: \"throws him (old)\".",
            "4. To compare the big swing with before: \"The big one's swing: as recorded (old)\". And try \"Speed: big one\" at 1.3 or 1.5 if the wait before it is too long.",
            "5. What I would like to hear: does the big forehand look strong now? Is the wait before it right? Is the spin right? Then I go on with step 4 (the combo against a shield).",
        };

        /// <summary>The names of what he is to press and watch.</summary>
        private static readonly string[] LookHere =
        {
            "Duel", "Strike at", "Throw combo", "Last blow:", "Speed: combos", "Speed: big one", "The big one's swing:", "Before the big one, a blow:", "His last combo",
        };

        /// <summary>For the panel check (headless): the names of <see cref="LookHere"/> that were really drawn, and every section taken as open while it looks.</summary>
        public static readonly HashSet<string> LookFramed = new HashSet<string>();
        public static bool OpenAllSections;

        /// <summary>The names he is asked to press and watch (for the panel check).</summary>
        public static IReadOnlyList<string> LookNames => LookHere;

        private static readonly Color LookTone = new Color(1f, 0.82f, 0.1f);
        private bool lookOn = true, lookRead, rowLooked;

        /// <summary>The lab's stop frame: the game is stopped the moment a get-up begins.</summary>
        private bool holdAtGetUp;

        private void OnGetUpBegan(PhysicsBattler.Locomotion.ClipFighter who)
        {
            if (holdAtGetUp && !paused)
            {
                TogglePause();
                message = "Held at the start of a get-up. \"Step\" goes on one step; \"Let him go on\" runs the clock.";
            }
        }

        private static string LookKey => "BattleSandbox.LookOff." + LookTag;

        /// <summary>A thing of this name is one he is asked to press or watch.</summary>
        private bool Looked(string name)
        {
            if (!lookRead)
            {
                lookRead = true;
                lookOn = PlayerPrefs.GetFloat(LookKey, 0f) < 0.5f;
            }

            if (!lookOn || string.IsNullOrEmpty(name))
            {
                return false;
            }

            name = name.Trim();
            if (name.Length > 2 && name[0] == '[' && name[name.Length - 1] == ']')
            {
                name = name.Substring(1, name.Length - 2);
            }

            foreach (string one in LookHere)
            {
                // ("Heading>Name": only under that heading or in that section.)
                int under = one.IndexOf('>');
                if (under > 0 ? string.Equals(one.Substring(0, under), zoneName, System.StringComparison.OrdinalIgnoreCase) && string.Equals(one.Substring(under + 1), name, System.StringComparison.OrdinalIgnoreCase)
                    : string.Equals(one, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    LookFramed.Add(one);
                    return true;
                }
            }

            return false;
        }

        /// <summary>A bright frame round what was drawn last.</summary>
        private void FrameLast()
        {
            if (Event.current != null && Event.current.type == EventType.Repaint)
            {
                Frame(GUILayoutUtility.GetLastRect());
            }
        }

        /// <summary>A bright frame round a place on the panel: it breathes slowly so the eye finds it, by the real clock (it goes on while the game is paused).</summary>
        private static void Frame(Rect at)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || at.width < 2f || at.height < 2f)
            {
                return;
            }

            const float thick = 2f;
            var was = GUI.color;
            float breath = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 4f);
            GUI.color = new Color(LookTone.r, LookTone.g, LookTone.b, breath);
            GUI.DrawTexture(new Rect(at.x - thick, at.y - thick, at.width + 2f * thick, thick), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(at.x - thick, at.y + at.height, at.width + 2f * thick, thick), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(at.x - thick, at.y, thick, at.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(at.x + at.width, at.y, thick, at.height), Texture2D.whiteTexture);
            GUI.color = was;
        }

        /// <summary>The box at the top of the panel: what to test after this push, and the switch for the frames.</summary>
        private void DrawLookHere()
        {
            Looked("");
            if (LookSteps.Length == 0)
            {
                return;
            }

            var was = GUI.contentColor;
            GUI.contentColor = LookTone;
            GUILayout.Label(new GUIContent("Test this (" + LookTag + ")", "What Claude asks you to look at after its last push. The things named here have a yellow frame wherever they are on the panel."), Styles().Heading);
            GUI.contentColor = was;
            if (lookOn)
            {
                foreach (string step in LookSteps)
                {
                    GUILayout.Label(step, Styles().Text);
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(lookOn ? "Hide this and the yellow frames" : "Show what to test, and the yellow frames", "The steps above and the frames round the things they name. Kept until the next push brings a new list."), Styles().Button))
            {
                lookOn = !lookOn;
                PlayerPrefs.SetFloat(LookKey, lookOn ? 0f : 1f);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }

        // ---- The search (18 November) ----
        // A box at the top of the panel, as in an engine's details panel: with words in it, every section is opened
        // and only the settings with those words in their name are shown, each under its own heading. A row of the
        // panel (a name and its choices) is one setting: it is shown whole if the words are in its name or in any of
        // its choices. What is measured, and text that is no setting, is left out while searching.

        private string search = "", searchShown = "";
        private string[] searchWords = new string[0];
        private bool searchFocused, filtering;
        private int searchHits, rowDepth, rowsBegun;
        private bool rowMatches;
        private string rowKey, pendingSection, pendingSectionTip, pendingHeading, pendingHeadingTip;
        private int pendingSectionZone, pendingHeadingZone;
        private readonly System.Text.StringBuilder rowText = new System.Text.StringBuilder();
        // (Every row's words, by its first: read off as the panel is laid out, used from the next time it is laid out.)
        private Dictionary<string, string> rowTexts = new Dictionary<string, string>(), rowTextsNext = new Dictionary<string, string>();
        private const string SearchName = "PhysicsBattler.PanelSearch";

        private static bool LayingOut => Event.current != null && Event.current.type == EventType.Layout;

        /// <summary>What is searched for is taken as each layout begins (OnGUI), so every event draws what was laid out for it.</summary>
        private void LatchSearch()
        {
            searchShown = (search ?? "").Trim();
            searchWords = searchShown.Length > 0 ? searchShown.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries) : new string[0];
            if (rowTextsNext.Count > 0)
            {
                (rowTexts, rowTextsNext) = (rowTextsNext, rowTexts);
                rowTextsNext.Clear();
            }
        }

        private bool Matches(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            foreach (string word in searchWords)
            {
                if (text.IndexOf(word, System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>A window's scrolling part begins: from here the search decides what is drawn, and no part of the panel has a colour yet.</summary>
        private void BeginFiltered()
        {
            filtering = searchWords.Length > 0;
            searchHits = 0;
            zone = -1;
            zoneName = "";
            rowDepth = rowsBegun = 0;
            rowKey = pendingSection = pendingHeading = null;
            rowMatches = false;
        }

        private void EndFiltered()
        {
            if (filtering && searchHits == 0)
            {
                GUILayout.Label($"Nothing here has \"{searchShown}\" in its name.", Styles().Text);
            }

            filtering = false;
            zone = -1;
            zoneName = "";
            pendingSection = pendingHeading = null;
        }

        /// <summary>
        /// A thing with a name is about to be drawn (a button, a tick, a slider, a row's name): false, it is left out,
        /// the search being for something else. In a row, the row is judged whole by its first thing.
        /// </summary>
        private bool Shown(string name)
        {
            name = name ?? "";
            bool first = rowDepth > 0 && rowKey == null;
            if (rowDepth > 0)
            {
                if (first)
                {
                    rowKey = zoneName + "/" + name;
                }

                if (LayingOut)
                {
                    rowText.Append(name).Append(" | ");
                }
            }

            if (!filtering)
            {
                return true;
            }

            if (rowDepth > 0)
            {
                if (first)
                {
                    rowMatches = Matches(rowTexts.TryGetValue(rowKey, out string all) ? all : name);
                    searchHits += rowMatches ? 1 : 0;
                }

                if (!rowMatches)
                {
                    return false;
                }
            }
            else if (!Matches(name))
            {
                return false;
            }
            else
            {
                searchHits++;
            }

            ShowPending();
            return true;
        }

        /// <summary>While searching: the section's bar and the heading that what is about to be drawn stands under, drawn now if they have not been; and its row begun.</summary>
        private void ShowPending()
        {
            if (pendingSection != null)
            {
                GUILayout.Space(6f);
                var was = GUI.backgroundColor;
                GUI.backgroundColor = ZoneTone(pendingSectionZone, 0.6f, 0.62f);
                GUILayout.Button(new GUIContent("−  " + pendingSection, pendingSectionTip), Styles().Section);
                GUI.backgroundColor = was;
                pendingSection = null;
            }

            if (pendingHeading != null)
            {
                HeadingBar(pendingHeading, pendingHeadingTip, pendingHeadingZone);
                pendingHeading = null;
            }

            while (rowsBegun < rowDepth)
            {
                // (As BeginRow opens one: a column of lines.)
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                rowsBegun++;
                rowTaken = 0f;
            }
        }

        /// <summary>What a window's edges and its scroll bar take of its width (the window's own padding on both sides, the bar and its margins).</summary>
        public const float ViewEdges = 48f;

        /// <summary>How wide what is in the view being drawn may be, and how much of the row being drawn is taken.</summary>
        private float rowRoom = PanelWidth - ViewEdges, rowTaken;

        /// <summary>A row of the panel begins: a name and its choices, or buttons side by side. One setting, to the search.</summary>
        private void BeginRow()
        {
            rowDepth++;
            if (!filtering)
            {
                // (In a column of its own: a row too long for the window goes on on a line below.)
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                rowsBegun++;
                rowTaken = 0f;
            }
        }

        /// <summary>
        /// A thing is about to be drawn in a row: where the row has no room left for it, the row goes on on a new line
        /// first. By what the thing's own words need; a name is never counted for more than 0.45 of the row.
        /// </summary>
        private void RowTakes(GUIContent content, GUIStyle style, bool name)
        {
            if (rowDepth != 1 || rowsBegun < 1 || content == null || Event.current == null)
            {
                return;
            }

            float needs = (style ?? GUI.skin.label).CalcSize(content).x + 6f;
            if (name)
            {
                needs = Mathf.Min(needs, 0.45f * rowRoom);
            }

            if (rowTaken > 0f && rowTaken + needs > rowRoom - 8f)
            {
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                rowTaken = 0f;
            }

            rowTaken += needs;
        }

        private void EndRow()
        {
            if (rowDepth <= 0)
            {
                return;
            }

            if (rowsBegun >= rowDepth)
            {
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                rowsBegun--;
            }

            rowDepth--;
            if (rowDepth == 0)
            {
                if (rowLooked && rowsBegun == 0 && !filtering)
                {
                    // (The whole row: its name and its choices.)
                    FrameLast();
                }

                rowLooked = false;
                if (rowKey != null && LayingOut)
                {
                    rowTextsNext[rowKey] = rowTextsNext.TryGetValue(rowKey, out string before) ? before + rowText : rowText.ToString();
                }

                rowKey = null;
                rowMatches = false;
                rowText.Length = 0;
            }
        }

        /// <summary>Words on the panel. In a row they are part of it (its name, most often); by themselves they are no setting, and are left out while searching.</summary>
        private void Words(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (rowDepth > 0 ? Shown(content.text) : !filtering)
            {
                RowTakes(content, style, true);
                GUILayout.Label(content, style, options);
                LookedWords(content.text);
            }
        }

        private void Words(GUIContent content, params GUILayoutOption[] options)
        {
            if (rowDepth > 0 ? Shown(content.text) : !filtering)
            {
                RowTakes(content, null, true);
                GUILayout.Label(content, options);
                LookedWords(content.text);
            }
        }

        /// <summary>Words he is asked to look at: in a row the whole row is framed as it ends; alone, or while searching, the words themselves.</summary>
        private void LookedWords(string text)
        {
            if (!Looked(text))
            {
                return;
            }

            if (rowDepth > 0 && !filtering)
            {
                rowLooked = true;
            }
            else
            {
                FrameLast();
            }
        }

        private void Words(string text, GUIStyle style, params GUILayoutOption[] options) => Words(new GUIContent(text), style, options);

        private void Words(string text, params GUILayoutOption[] options) => Words(new GUIContent(text), options);

        /// <summary>Room between things; none while searching, where only the hits are drawn.</summary>
        private void Gap(float pixels)
        {
            if (!filtering)
            {
                GUILayout.Space(pixels);
            }
        }

        private void Flex()
        {
            if (!filtering || (rowDepth > 0 && rowsBegun >= rowDepth))
            {
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary>The search box, at the top of the panel: its words, and a button to clear them.</summary>
        private void SearchBox()
        {
            var e = Event.current;
            bool focused = GUI.GetNameOfFocusedControl() == SearchName;
            if (e != null && e.type == EventType.KeyDown && focused && (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                // (Escape: the whole panel again. Enter: the keys are the game's again, the search kept.)
                if (e.keyCode == KeyCode.Escape)
                {
                    search = "";
                }

                GUIUtility.keyboardControl = 0;
                e.Use();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Search", "Type part of a setting's name: every section is opened and only the settings with those words in their name are shown, under their headings. Several words: all of them must be in it. Escape, or the × button, shows the whole panel again. While you type here the game's keys (R, P, F and the rest) do nothing."), Styles().Text, GUILayout.Width(50f));
            GUI.SetNextControlName(SearchName);
            search = GUILayout.TextField(search ?? "", 60, Styles().Search, GUILayout.ExpandWidth(true));
            if (e != null && e.type == EventType.Repaint)
            {
                searchBoxAt = GUILayoutUtility.GetLastRect();
                if (string.IsNullOrEmpty(search) && !focused)
                {
                    GUI.Label(searchBoxAt, "a setting's name, or part of it", Styles().SearchHint);
                }
            }

            if (GUILayout.Button(new GUIContent("×", "Clear the search: the whole panel again."), GUILayout.Width(26f)))
            {
                search = "";
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.EndHorizontal();
            if (e != null && e.type == EventType.MouseDown && focused && !searchBoxAt.Contains(e.mousePosition))
            {
                // (A press anywhere else: the keys are the game's again.)
                GUIUtility.keyboardControl = 0;
            }

            if (e != null && e.type == EventType.Repaint)
            {
                searchFocused = GUI.GetNameOfFocusedControl() == SearchName;
                SandboxInput.Typing = searchFocused;
            }
        }

        private Rect searchBoxAt;

        /// <summary>A section's heading, pressed to fold it shut or open (kept between sessions). True: it is open.</summary>
        private bool Section(string name, bool openAtFirst, string tip)
        {
            if (!sectionOpen.TryGetValue(name, out bool open))
            {
                open = PlayerPrefs.GetFloat("BattleSandbox.Section." + name, openAtFirst ? 1f : 0f) > 0.5f;
                sectionOpen[name] = open;
            }

            open |= OpenAllSections;

            EnterZone(name);
            if (filtering)
            {
                // (Searching: every section is looked through, and its bar is drawn only over a hit.)
                pendingSection = name;
                pendingSectionTip = tip;
                pendingSectionZone = zone;
                pendingHeading = null;
                return true;
            }

            GUILayout.Space(6f);
            var was = GUI.backgroundColor;
            GUI.backgroundColor = ZoneTone(zone, 0.6f, 0.62f);
            bool pressed = GUILayout.Button(new GUIContent((open ? "−  " : "+  ") + name, tip), Styles().Section);
            GUI.backgroundColor = was;
            if (Looked(name))
            {
                FrameLast();
            }

            if (pressed)
            {
                // (Open or shut from the next time the panel is laid out: what is drawn in answer to this press has
                // to be what was laid out for it.)
                sectionOpen[name] = !open;
                PlayerPrefs.SetFloat("BattleSandbox.Section." + name, open ? 0f : 1f);
            }

            return open;
        }

        /// <summary>A heading inside a section, with room over it. What follows it, down to the next heading, takes its colour.</summary>
        private void Heading(string text, string tip = "")
        {
            EnterZone(text);
            if (filtering)
            {
                pendingHeading = text;
                pendingHeadingTip = tip;
                pendingHeadingZone = zone;
                return;
            }

            HeadingBar(text, tip, zone);
        }

        private void HeadingBar(string text, string tip, int hue)
        {
            GUILayout.Space(10f);
            var style = Styles().Heading;
            var content = new GUIContent(text, tip);
            Rect at = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                // (A tinted bar across the panel with a bright edge at its left: the eye finds it going down the panel.)
                var was = GUI.color;
                GUI.color = ZoneTone(hue, 0.7f, 1f, 0.2f);
                GUI.DrawTexture(at, Texture2D.whiteTexture);
                GUI.color = ZoneTone(hue, 0.7f, 1f, 0.95f);
                GUI.DrawTexture(new Rect(at.x, at.y, 3f, at.height), Texture2D.whiteTexture);
                GUI.color = was;
            }

            var ink = GUI.contentColor;
            GUI.contentColor = ZoneTone(hue, 0.45f, 1f);
            GUI.Label(at, content, style);
            GUI.contentColor = ink;
            if (Looked(text))
            {
                Frame(at);
            }

            GUILayout.Space(2f);
        }

        private bool Press(string text, string tip, params GUILayoutOption[] options)
        {
            if (!Shown(text))
            {
                return false;
            }

            RowTakes(new GUIContent(text), Styles().Button, false);
            bool pressed = ZoneButton(new GUIContent(text, tip), null, options);
            if (Looked(text))
            {
                FrameLast();
            }

            return pressed;
        }

        private bool Tick(bool value, string text, string tip)
        {
            if (!Shown(text))
            {
                return value;
            }

            RowTakes(new GUIContent(text), Styles().Toggle, false);
            value = GUILayout.Toggle(value, new GUIContent(text, tip), Styles().Toggle);
            if (Looked(text))
            {
                FrameLast();
            }

            return value;
        }

        /// <summary>A slider on one line: its name, the slider, its value. What it means is its tooltip.</summary>
        private float Slide(string name, string value, string tip, float now, float least, float most)
        {
            if (!Shown(name))
            {
                return now;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(name, tip), Styles().Text, GUILayout.Width(96f));
            GUILayout.BeginVertical();
            GUILayout.Space(7f);
            now = GUILayout.HorizontalSlider(now, least, most);
            GUILayout.EndVertical();
            GUILayout.Label(new GUIContent(value, tip), Styles().Value, GUILayout.Width(78f));
            GUILayout.EndHorizontal();
            if (Looked(name))
            {
                FrameLast();
            }

            return now;
        }

        /// <summary>A window begins to be drawn: nothing is searched or coloured until its scrolling part (also after a draw that was broken off part of the way).</summary>
        private void ResetDraw()
        {
            filtering = false;
            rowDepth = rowsBegun = 0;
            rowKey = pendingSection = pendingHeading = null;
            rowMatches = false;
            rowText.Length = 0;
            zone = -1;
            zoneName = "";
        }

        private void DrawPanel(int id)
        {
            ResetDraw();
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
            PlaceLooked = Looked;
            var goTo = PlaceBar(shownAim ? (shownDuel ? Place.Duel : Place.Aim) : shownTube ? Place.Tubes : shownTarget ? Place.Targets : Place.Battle);
            if (goTo.HasValue)
            {
                GoTo(goTo.Value);
            }

            Gap(4f);
            BeginRow();
            if (Press(paused ? "Resume" : "Pause", "Stop and start the game's clock (P).")) TogglePause();
            if (Press("Step", "One step of the game, then stopped again (the full stop key).")) StepOnce();
            if (Press("Reset", "The scene set out afresh: every fighter back on its place (R).")) Spawn();
            // (The panel's size, always in reach: smaller and larger.)
            Gap(8f);
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

            EndRow();
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
            SearchBox();

            // ---- The sections, in a view that scrolls when they are taller than the screen ----
            float room = Screen.height / Mathf.Max(0.1f, uiScale) - panelRect.y - panelChrome - 8f;
            panelView = panelContent < 0f ? Mathf.Max(160f, room) : Mathf.Clamp(panelContent + 4f, 60f, Mathf.Max(160f, room));
            // (No scrolling sideways, ever: Renier, 21 November, "much of the debug UI needs to be scrolled horizontally.
            // Can you make it so that it needs no scrolling horizontally." One row wider than the window made the
            // whole view that wide, and every line of text was then laid out to that width and cut off at the window's
            // edge. The view has no bar across, what is in it is held to the window's own width, and a row of buttons
            // too long for it goes on on a line below (BeginRow, RowTakes).)
            panelScroll = GUILayout.BeginScrollView(new Vector2(0f, panelScroll.y), false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(panelView));
            rowRoom = PanelWidth - ViewEdges;
            GUILayout.BeginVertical(GUILayout.Width(rowRoom));
            DrawLookHere();
            BeginFiltered();
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
            EndFiltered();
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
                Gap(4f);
                BeginRow();
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

                EndRow();
            }

            // ---- What the last thing pressed did, and what the thing under the mouse does ----
            // (Always two lines tall: the view above it does not change its height as what it says changes.)
            Gap(4f);
            Readout(shownMessage, 2);

            Gap(4f);
            GUILayout.BeginVertical(Styles().Box);
            string hint = !string.IsNullOrEmpty(GUI.tooltip) ? GUI.tooltip : !string.IsNullOrEmpty(actionsTip) ? actionsTip : "Point at anything to read here what it does.";
            Words(hint, Styles().Hint, GUILayout.Height(HintHeight));
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

            BeginRow();
            if (Press(fight ? "Stop fighting" : "Fight", "The two sides fight, or stand (F).")) ToggleFight();
            if (Press(turnedAway ? "Face the enemy" : "Turn around", "Every fighter turned about, or back (T).")) TurnAround();
            EndRow();

            Heading("Tests in the battle", "Each sets out its own scene from the battle's fighters.");
            BeginRow();
            if (Press("Bystander (B)", "A blow swung with a third fighter standing in its way.")) BystanderTest();
            if (Press("Launch (L)", "The heaviest armed fighter strikes the lightest enemy, with a third standing where the blow will throw it.")) LaunchTest();
            if (Press("Sever (K)", "A swordsman cuts at an enemy's elbow, and the arm should come off there. Again for another go.")) SeverTest();
            EndRow();
            BeginRow();
            if (Press("Ground (D)", "Two fighters go down in the middle, one knocked down and one killed, and the others walk straight across them for 20 s. Again stops it.")) GroundTest();
            if (Press("Shield (J)", "A sword and a club take turns striking a shield-holder from in front, two blows each. Again for another round.")) ShieldTest();
            EndRow();
            BeginRow();
            if (Press(galleryRun != null ? "Stop gallery (G)" : "Move gallery (G)", "Every move in turn against a dummy, first slowly and then at full speed. Again ends it.")) Gallery();
            if (Press(rhythmRun != null ? "Stop rhythms (H)" : "Rhythm gallery (H)", "Each fighting style in turn: its base, then every posture, slowed and at full speed. Again ends it.")) RhythmGallery();
            EndRow();

            if (shownTarget)
            {
                // (Each respawns the test with the other body: the attackers with their weapon, a Large's club or a
                // Medium's sword; the targets with what they stand with.)
                Heading("Target test");
                BeginRow();
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

                EndRow();
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
                    BeginRow();
                    if (Press("Line: " + (Aim.Line.HasValue ? BuiltBlow.LineName(Aim.Line.Value) : "chosen for the part"), "The line the blade comes in on, as the swordsman sees it. Chosen for the part: of the lines that part may be cut on, the one his arm makes best from where he stands. Press to go through the eight."))
                    {
                        Aim.Line = !Aim.Line.HasValue ? CutLine.Down : Aim.Line.Value == CutLine.Up ? (CutLine?)null : Aim.Line.Value + 1;
                    }

                    if (Press("Size: " + (!Aim.Size.HasValue ? "chosen" : Aim.Size.Value == CutSize.Wrist ? "wrist cut" : "elbow cut"), "A wrist cut is the quickest and lightest: it lands in about a quarter of a second. An elbow cut swings the forearm: about 0.4 s, and harder. Chosen: the wrist for hands and forearms, the elbow for the rest."))
                    {
                        Aim.Size = !Aim.Size.HasValue ? CutSize.Wrist : Aim.Size.Value == CutSize.Wrist ? CutSize.Elbow : (CutSize?)null;
                    }

                    EndRow();
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
            Gap(4f);
            BeginRow();
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

            EndRow();

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
            Words(aimLast.Length > 0 ? aimLast : "None yet.", Styles().Text);
            Words(Aim.Blows.Count > 0 ? "So far: " + Aim.Tally() + "." : "", Styles().Text);
            BladePathRow(feel, Aim.Attacker != null ? Aim.Attacker.Clip : null);
        }

        /// <summary>
        /// The blade's path (the combos plan's step 1): drawn or hidden, and the last attack's path in numbers. A view
        /// only: <see cref="PhysicsBattler.Locomotion.BladePath"/>, drawn by <see cref="BladePathView"/>.
        /// </summary>
        private void BladePathRow(PhysicsBattler.Locomotion.CombatFeel feel, PhysicsBattler.Locomotion.ClipFighter who)
        {
            bool drawn = feel != null && feel.theBladesPathIsDrawn;
            BeginRow();
            Words(new GUIContent("The blade's path:", "A view only: nobody fights differently (Combat Feel \"The Blade's Path Is Drawn\"; step 1 of the combos plan). Drawn: as he throws an attack, the path its maker planned for the sword's point is drawn in pale blue: thin over the wind-up, thick through the cut and its carry-through to where the blade turns, thin and broken on the way back to the guard. A white bar is the blade as it is planned to land; a yellow cross is the spot you asked for. As the attack goes, the sword's real path is drawn over it in orange, with an orange bar for the blade at the moment it was to land and a dark dot for every step of the game's clock: where the dots bunch up the blade is slow (right after the white bar they lie on one another: the freeze on a hit, which is meant). Both stay until his next attack: pause, or use \"Slow motion\" under \"Stumbles and falls\", and look from any side. Hidden (the default): nothing is worked out or drawn."));
            if (feel != null)
            {
                if (Press(drawn ? "[drawn]" : "drawn", "Every attack's planned path in pale blue and its real path in orange, kept until the next attack."))
                {
                    feel.theBladesPathIsDrawn = true;
                    message = "The blade's path: drawn. Have him strike (press a part of the body above).";
                }

                if (Press(!drawn ? "[hidden]" : "hidden", "Nothing is worked out or drawn, as before."))
                {
                    feel.theBladesPathIsDrawn = false;
                    message = "The blade's path: hidden.";
                }
            }

            EndRow();
            var path = drawn && who != null ? who.BladePath : null;
            // (Always drawn: a line that comes and goes between a frame's two passes upsets the panel. Only its height
            // goes by the switch, as it was when this pass began.)
            Readout(new GUIContent("His last blade path: " + (!drawn ? "hidden." : path == null ? "none yet: have him strike." : path.Tell() + "."),
                "The last attack he threw, as its maker planned the blade and as the blade then went. First the cut and its two corners: where its blade starts and where it ends, from its recording. Planned: when the blade lands (seconds into the attack's clip), the nearest its striking part passes to the spot asked for, and how far the point is carried on after the landing to where the blade turns (against where he stands, so his own walking is not counted). As it went, read off the sword itself each step: the fastest the point went as it landed, the nearest it passed to the spot, what it met in the blow's own words, how long it was frozen on the hit (that is meant) and how long after that its point still went slower than 5 m/s (that is the blade sticking at the body), and how far it was carried on over the same stretch of the attack as the plan's. Last, how far the sword's point was from the planned line, on average and at most through the cut, and how far behind its plan it ran: a physical sword follows its pose a step or two late. A join of two blows into one swing (the plan's step 2) will be judged by these numbers."), drawn ? 9 : 1);
        }

        private bool aimStyleOpen;

        /// <summary>The aim test's made attacks: which blow, on which legs, when it lands, and the style it is made in.</summary>
        private void DrawMadeAttacks(CombatFeel feel)
        {
            BeginRow();
            if (Press("Blow: " + AimAttacks[aimAttack].name, "Which recorded blow the attack is made from. The best fit: of the three plain ones (forehand cut, rising backhand, thrust), the one whose blade comes onto the part with the least change. The leaping and hopping ones are only made when named here."))
            {
                aimAttack = (aimAttack + 1) % AimAttacks.Length;
            }

            if (Press("Legs: " + AimLegs[aimLegs].name, "The legs the blow is put on. Its own: the legs it was recorded with. Another's: that recording's hips, legs, steps and travel under this blow's chest and arms (a thrust on the forehand's lunge is a lunging thrust; a forehand on the thrust's legs is a cut from where it stands). The best fit: whichever brings the blade to the part with the least change, its own first."))
            {
                aimLegs = (aimLegs + 1) % AimLegs.Length;
            }

            EndRow();
            Aim.Attack = AimAttacks[aimAttack].clip;
            Aim.Legs = AimLegs[aimLegs].clip;
            Aim.LandIn = Mathf.Round(20f * Slide("Lands in", Aim.LandIn > 0.01f ? $"{Aim.LandIn:0.00} s" : "its own time", "When the blade is to land, counted from the press. Its own time: as the recording has it (a forehand cut about 0.6 s, a rising backhand 0.8 s, a thrust 0.45 s). Set: the wind-up is stretched or shortened so the blade lands then (between a third and three times its own length); the drive itself is not changed.", Aim.LandIn, 0f, 1.6f)) / 20f;

            BeginRow();
            Words(new GUIContent("Style", "The same blow as different fighters would throw it. Each button sets all the levers below; then press a part."), Styles().Text, GUILayout.Width(40f));
            foreach (var preset in AttackStyle.Presets)
            {
                if (Press(preset.name, preset.name == "Plain" ? "The recording's own: every lever at 1." : preset.name == "Heavy" ? "Wound far back and slowly, thrown from the hips, deep in the lunge, carried all the way through." : preset.name == "Quick" ? "Little wind-up, a short fast blow from the guard, back at once." : preset.name == "Brawler" ? "Big arm swings on a quiet body: the arm does the work, the hips little." : preset.name == "Low" ? "Crouched, deep in the knees, leaning in." : "The hips well ahead and the arm late: the body cracks like a whip."))
                {
                    feel.SetAttackStyle(preset.style);
                    Note("aim test: attack style " + preset.name);
                }
            }

            EndRow();
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
            // (A map of what was measured, no setting: left out while searching.)
            if (filtering)
            {
                return;
            }

            BeginRow();
            Flex();
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
                // (One line, of one width and one height, whatever it says: the map and the panel under it do not
                // move as a part is covered or opens. His right and his left are R and L, so the longest still fits.)
                string shown = (open ? name : name + " (" + behind.Replace("its ", "") + ")").Replace("right ", "R ").Replace("left ", "L ");
                Words(new GUIContent(shown, open ? "Open: a straight line from the other's sword shoulder reaches it." : "Covered: " + behind + " is before it."), Styles().Cell, GUILayout.Width(AimButtonWidth + 26f), GUILayout.Height(Mathf.Max(14f, Styles().Cell.lineHeight) + 4f));
            }

            GUI.color = was;
            Flex();
            EndRow();
        }

        private void AimRow(AimSpot? left, AimSpot? middle = null, AimSpot? right = null)
        {
            BeginRow();
            // (One spot: in the middle. Two: side by side. Three: across.)
            if (!middle.HasValue && !right.HasValue)
            {
                Flex();
                AimButton(left);
                Flex();
            }
            else if (!right.HasValue)
            {
                Flex();
                AimButton(left);
                AimButton(middle);
                Flex();
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
                    Gap(AimButtonWidth + 4f);
                }

                AimButton(right);
            }

            EndRow();
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
            if (Press(spot.Value == aimSpot ? "[" + name + "]" : name, $"One blow at the target's {AimSpots.Name(spot.Value)}.", GUILayout.Width(AimButtonWidth)))
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
            BeginRow();
            if (Press("Back", "Struck on the front with 200 N·s: they fall backward.")) FallTest(0f, false);
            if (Press("Forward", "Struck on the back with 200 N·s: they fall forward.")) FallTest(180f, false);
            if (Press("Left", "Struck on the right side with 200 N·s: they fall to their left.")) FallTest(270f, false);
            if (Press("Right", "Struck on the left side with 200 N·s: they fall to their right.")) FallTest(90f, false);
            EndRow();
            if (Press("Thrown back (450)", "Struck on the front with 450 N·s, a Large's club at its hardest: thrown off their feet.")) FallTest(0f, true);

            Heading("A fall, then a turn round", "While the targets are down, what they stand facing is put behind them, so once up they turn round to it: the get-up into a turn, and the swordsman's sword through it.");
            BeginRow();
            if (Press("Back, +170", "They fall backward, and get up to turn round 170 degrees to their right.")) FallTest(0f, false, 170f);
            if (Press("Back, −170", "They fall backward, and get up to turn round 170 degrees to their left.")) FallTest(0f, false, -170f);
            if (Press("Thrown", "They are thrown back, and get up to turn round.")) FallTest(0f, true, 170f);
            EndRow();

            Heading("Blows on demand", "A tube taken as a blow of that size, from the side the targets are turned to. Their legs decide what comes of it: a recoil, a stumble, a fall.");
            BeginRow();
            if (Press("Cut (60)", "60 N·s: a little more than a Medium's sword cut.")) BlowTest(60f);
            if (Press("Heavy (200)", "200 N·s: a Large's club.")) BlowTest(200f);
            if (Press("Club (600)", "600 N·s: more than any fighter strikes with.")) BlowTest(600f);
            EndRow();
            if (Press("A fall to catch (165)", "165 N·s: only just more than a Medium's legs catch, so the fall is one that can be caught on a knee and a hand.")) BlowTest(165f);

            Heading("The tube", "What the Hit button and the 6 key send. The buttons above set these themselves.");
            BeginRow();
            if (Press("Hit (6)", "Sends the waiting tubes as they are set below.")) TubeHit();
            if (Press(largeTargets ? "Targets: Larges" : "Targets: Mediums", "Sets the test out again with targets of the other size."))
            {
                largeTargets = !largeTargets;
                TubeTestScene();
            }

            EndRow();
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

            // ---- The start of a get-up (the plan "From how he lies to how he gets up", steps 1 and 2; 19 November) ----
            Heading("The start of a get-up", "The first moment of a get-up: the blend from how he lies into the get-up. In Renier's log of 19 November nine tenths of a get-up's crossings were in its first 0.75 s. Here it can be held still and looked at, and the start can be chosen old or new.");
            BeginRow();
            Words(new GUIContent("Hold him at the start:", "On: the game stops the moment any fighter begins to get up, before he has moved: you see exactly how he lies. Then \"Step\" at the top of the panel goes on one step at a time, or set \"Speed\" low and press \"Let him go on\". With several on the ground it stops again for each of them. Off: nothing is stopped."), GUILayout.Width(150f));
            if (Press(holdAtGetUp ? "[on]" : "on", "Stop as a get-up begins.")) holdAtGetUp = true;
            if (Press(!holdAtGetUp ? "[off]" : "off", "Never stop.")) holdAtGetUp = false;
            if (Press("Let him go on", "The clock runs again (the same as Resume at the top).") && paused)
            {
                TogglePause();
            }

            EndRow();
            BeginRow();
            Words(new GUIContent("From how he lies:", "What a fallen fighter does in the moment before he gets up (Combat Feel \"He Gathers Himself Before He Gets Up\"). Gathers himself (new): he goes on as the physical body he fell in, and his muscles draw his arms and legs to where the get-up starts; the ground, his own body and his shield are solid to them, so they go round what is in the way. Then the get-up takes over. Blends (old): the get-up takes over where he lies, and every part turns straight to where the get-up has it. Fell him the same way with each and watch the moment before he rises."), GUILayout.Width(150f));
            bool gathers = feel != null ? feel.heGathersHimselfBeforeHeGetsUp : PhysicsBattler.Locomotion.ClipFighter.GathersBeforeGetUp;
            bool gathersNow = gathers;
            if (Press(gathers ? "[gathers himself]" : "gathers himself", "Drawn to the start as a physical body first.")) gathersNow = true;
            if (Press(!gathers ? "[blends (old)]" : "blends (old)", "As before.")) gathersNow = false;
            EndRow();
            if (gathersNow != gathers)
            {
                if (feel != null)
                {
                    feel.heGathersHimselfBeforeHeGetsUp = gathersNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.GathersBeforeGetUp = gathersNow;
            }

            if (feel != null && gathersNow)
            {
                feel.gatherTakesAtMost = Lever("Gather takes at most", feel.gatherTakesAtMost, 0.2f, 1.5f, "The most a gather may take (seconds). Most of it is time he lay still anyway; what is over makes him lie that much longer.");
                feel.gatherMuscle = Lever("Gather muscle", feel.gatherMuscle, 0.2f, 2f, "How hard his muscles draw him to the start, as a share of a person's strength. More: quicker and stiffer, and more likely to thrash.");
                feel.gatherTurnsHisTrunk = Lever("Gather turns his trunk", feel.gatherTurnsHisTrunk, 0f, 1f, "How hard his trunk is turned toward how the get-up's start has it (a share of his weight on a metre's lever): a man turning himself over by pushing on the ground. 0: his trunk lies as it lies (the first build). Too much looks like being turned by a hand.");
            }

            Readout(new GUIContent("His last gather: " + (PhysicsBattler.Locomotion.ClipFighter.LastGatherTold.Length > 0 ? PhysicsBattler.Locomotion.ClipFighter.LastGatherTold : "none yet") + ".", "The last gather of any fighter: how long it took, the fastest any part moved in it, and how far the furthest hand or foot still was from the start as the get-up took over."), 2);

            BeginRow();
            Words(new GUIContent("Taken up:", "How the get-up takes a fallen fighter up (Combat Feel \"A Get-Up Takes Him Up As He Moves\"). As he moves (new): the get-up goes on from the pose he has and the speed each part has, and what is left is eased out; his limbs go by where their joints are; a part that is close takes less time. As if he lay still (old): his body is stopped for a step, and every part turns from how it lay to the get-up's pose over 0.6 s. Fell him the same way with each and watch the moment he begins to rise: a stop, or a part that jumps."), GUILayout.Width(150f));
            bool takes = feel != null ? feel.aGetUpTakesHimUpAsHeMoves : PhysicsBattler.Motion.GetUp.TakesOverAsHeMoves;
            bool takesNow = takes;
            if (Press(takes ? "[as he moves]" : "as he moves", "From the pose and the speed he has.")) takesNow = true;
            if (Press(!takes ? "[as if he lay still (old)]" : "as if he lay still (old)", "As before.")) takesNow = false;
            EndRow();
            if (takesNow != takes)
            {
                if (feel != null)
                {
                    feel.aGetUpTakesHimUpAsHeMoves = takesNow;
                }

                PhysicsBattler.Motion.GetUp.TakesOverAsHeMoves = takesNow;
            }

            if (feel != null && takesNow)
            {
                feel.takeOverTakesAtMost = Lever("Take-over takes at most", feel.takeOverTakesAtMost, 0.2f, 1.2f, "The most any part takes to ease into the get-up's pose (seconds). A part that is close takes less by itself. Lower: on the get-up's own poses sooner, parts that were far move faster. Higher: slower and softer.");
            }

            BeginRow();
            Words(new GUIContent("The start:", "How a get-up's start is chosen (Combat Feel \"A Get-Up Starts Nearest To How He Lies\"). Nearest to how he lies (new): every start he could use is weighed (the one his hips say, from his side too, each posting on either hand) and the one his hands, elbows, knees and feet are nearest to is taken. By his hips only (old): the way his hips face decides, as before. Fell him the same way with each and watch his arms in the first moment."), GUILayout.Width(150f));
            bool nearest = feel != null ? feel.aGetUpStartsNearestToHowHeLies : PhysicsBattler.Locomotion.ClipFighter.GetUpStartNearest;
            bool nearestNow = nearest;
            if (Press(nearest ? "[nearest to how he lies]" : "nearest to how he lies", "Every start weighed; the nearest taken.")) nearestNow = true;
            if (Press(!nearest ? "[by his hips only (old)]" : "by his hips only (old)", "As before.")) nearestNow = false;
            EndRow();
            if (nearestNow != nearest)
            {
                if (feel != null)
                {
                    feel.aGetUpStartsNearestToHowHeLies = nearestNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.GetUpStartNearest = nearestNow;
            }

            Readout(new GUIContent("His last get-up: " + (PhysicsBattler.Locomotion.ClipFighter.LastGetUpStartTold.Length > 0 ? PhysicsBattler.Locomotion.ClipFighter.LastGetUpStartTold : "none yet") + ".", "The first 0.75 s of the last get-up of any fighter, as seen: how long his own shield, sword or sword arm crossed him (counted while a playtest is recorded), the fastest any part moved, how far each hand went; then the take-over (its first 0.2 s): how fast his fastest part was moving as he was taken up, and the biggest jump in any part's speed from one step to the next (a part stopped dead or thrown shows as a jump); and how the start was chosen. Written about half a second after he is up."), 4);

            Heading("Getting up");
            // (One choice of three, where there were two switches: with the older one off, "as recorded" changed
            // nothing and said nothing. Seen in Unity, 18 November.)
            bool natural = feel != null ? feel.getUpsAreNatural : PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps;
            bool recorded = feel != null ? feel.getUpsAreFromTheRecordings : PhysicsBattler.Motion.GetUp.FromRecordings;
            int way = !natural ? 2 : recorded ? 0 : 1, wayNow = way;
            BeginRow();
            Words(new GUIContent("The way up:", "How a fallen fighter gets up. As recorded (new): the way a recorded man does, read off the recording and fitted to this fighter (from his back he sits up and rocks forward over his feet; from his front he pushes up, lifts his hips and steps his feet in). Made by hand: the key poses written by hand, from how he really lies, as before 17 November. Oldest: as before 14 November. To see one: Falls on demand above (Back, Forward, Left, Right) in the target test."), Styles().Text, GUILayout.Width(80f));
            if (Press(way == 0 ? "[as recorded]" : "as recorded", "The get-ups read off the recordings.")) wayNow = 0;
            if (Press(way == 1 ? "[made by hand]" : "made by hand", "The get-ups made by hand, as before 17 November.")) wayNow = 1;
            if (Press(way == 2 ? "[oldest]" : "oldest", "The get-ups as before 14 November.")) wayNow = 2;
            EndRow();
            if (wayNow != way)
            {
                if (feel != null)
                {
                    feel.getUpsAreNatural = wayNow != 2;
                    feel.getUpsAreFromTheRecordings = wayNow == 0;
                }

                PhysicsBattler.Locomotion.ClipFighter.NaturalGetUps = wayNow != 2;
                PhysicsBattler.Motion.GetUp.FromRecordings = wayNow == 0;
                Note(wayNow == 0 ? "get-ups: as recorded" : wayNow == 1 ? "get-ups: made by hand" : "get-ups: the oldest, as before 14 November");
            }

            if (feel != null && way == 0)
            {
                BeginRow();
                Words(new GUIContent("A foot by the hips:", "As recorded, from his back, he tucks one foot back under himself. Beside them (new): while his hips are low, a foot that comes near them stays out at its own side, beside the hip. Under them (old): where the recording has it, which on this body is through the hips."), Styles().Text, GUILayout.Width(120f));
                if (Press(feel.recordedGetUpFeetClearTheHips ? "[beside them]" : "beside them", "The foot stays out at its own side of the hips.")) feel.recordedGetUpFeetClearTheHips = true;
                if (Press(!feel.recordedGetUpFeetClearTheHips ? "[under them (old)]" : "under them (old)", "Where the recording has it.")) feel.recordedGetUpFeetClearTheHips = false;
                EndRow();
                PhysicsBattler.Motion.GetUp.FeetClearHips = feel.recordedGetUpFeetClearTheHips;
                feel.recordedGetUpFootClears = Lever("Foot stays out by", feel.recordedGetUpFootClears, 0.1f, 0.5f, "How far out at its own side of the hips' middle a foot near low hips stays, as a share of the leg's length. Lower: nearer the hips (0.1 is in them). Higher: the knee splays out more.");
                PhysicsBattler.Motion.GetUp.FootClearsHips = feel.recordedGetUpFootClears;
            }

            if (feel != null)
            {
                feel.recordedGetUpTakes = Lever("Recorded get-up takes", feel.recordedGetUpTakes, 0.4f, 1f, "How long a recorded get-up is given, as a share of the recording's own time (3.7 s from his back, 4.5 s from his front). Lower: up sooner. 1: the recorded man's own pace.");
                PhysicsBattler.Motion.GetUp.RecordedTakes = feel.recordedGetUpTakes;
            }

            bool held = feel != null ? feel.getUpsKeepWhatTheHandsHoldOffTheGround : PhysicsBattler.Locomotion.ClipFighter.GetUpsKnowWhatIsHeld;
            bool heldNow = Tick(held, "A get-up knows what is held", "A shield is laid flat under the forearm while the hands push and held up toward what he fights as it comes off them, and what both hands hold is kept out of the ground. Off: as it was until 18 November: a shield or a sword through the floor.");
            if (heldNow != held)
            {
                if (feel != null)
                {
                    feel.getUpsKeepWhatTheHandsHoldOffTheGround = heldNow;
                }

                PhysicsBattler.Locomotion.ClipFighter.GetUpsKnowWhatIsHeld = heldNow;
                Note(heldNow ? "get-ups: what is held kept off the ground" : "get-ups: not knowing what is held (old)");
            }

            BeginRow();
            Words(new GUIContent("Getting up, his blade:", "What keeps a rising fighter's sword out of the ground when his forearm points it down (Combat Feel \"A Rising Fighter's Blade Is Turned Clear Of The Ground\"). Turned clear at the wrist (new): the wrist turns it only as far as lifts it out. The arm thrown up (old): his whole arm goes up to lift it out and comes back the next moment, which is the sword that shook as he got up. Fell him from his back with \"Falls on demand\" to see it: about a second into the get-up."), GUILayout.Width(150f));
            bool bladeClear = feel != null ? feel.aRisingFightersBladeIsTurnedClearOfTheGround : PhysicsBattler.Motion.GetUp.BladeTurnsClear;
            bool bladeClearNow = bladeClear;
            if (Press(bladeClear ? "[turned clear at the wrist]" : "turned clear at the wrist", "The wrist lifts the blade out.")) bladeClearNow = true;
            if (Press(!bladeClear ? "[the arm thrown up (old)]" : "the arm thrown up (old)", "As before.")) bladeClearNow = false;
            EndRow();
            if (bladeClearNow != bladeClear)
            {
                if (feel != null)
                {
                    feel.aRisingFightersBladeIsTurnedClearOfTheGround = bladeClearNow;
                }

                PhysicsBattler.Motion.GetUp.BladeTurnsClear = bladeClearNow;
            }

            BeginRow();
            Words(new GUIContent("Falling and lying, his shield:", "What a fighter's shield does against his own body while he falls and lies (Combat Feel \"A Fallen Fighter's Shield Stays Solid Against His Body\"). Stays solid (new): his trunk, head, thighs and shins stop it, so it does not sink into him and his legs do not fall into it. Passes through him (old): as before. Fell him with \"Falls on demand\", or bash him down with \"Stumble him: Extra hard\", and watch the board."), GUILayout.Width(170f));
            bool boardSolid = feel != null ? feel.aFallenFightersShieldStaysSolidAgainstHisBody : PhysicsBattler.Authority.UpperBodyArticulation.ShieldStoppedOffFeet;
            bool boardSolidNow = boardSolid;
            if (Press(boardSolid ? "[stays solid]" : "stays solid", "Stopped by his own body.")) boardSolidNow = true;
            if (Press(!boardSolid ? "[passes through him (old)]" : "passes through him (old)", "As before.")) boardSolidNow = false;
            EndRow();
            if (boardSolidNow != boardSolid)
            {
                if (feel != null)
                {
                    feel.aFallenFightersShieldStaysSolidAgainstHisBody = boardSolidNow;
                }

                PhysicsBattler.Authority.UpperBodyArticulation.ShieldStoppedOffFeet = boardSolidNow;
            }

            BeginRow();
            Words(new GUIContent("Off their feet, two fighters:", "Whether two fighters are kept apart while one of them is down or getting up (Combat Feel \"Fighters Keep Apart Off Their Feet Too\"). Keep apart (new): one on his feet is walked off a body lying or rising next to him, and one lying waits to get up until there is room. Nothing keeps them apart (old): as before; two who fell together got up inside each other and locked."), GUILayout.Width(170f));
            bool room = feel != null ? feel.fightersKeepApartOffTheirFeetToo : ClipFighter.KeepsApartOffHisFeet;
            bool roomNow = room;
            if (Press(room ? "[keep apart]" : "keep apart", "Walked off him; he waits for room.")) roomNow = true;
            if (Press(!room ? "[nothing keeps them apart (old)]" : "nothing keeps them apart (old)", "As before.")) roomNow = false;
            EndRow();
            if (roomNow != room)
            {
                if (feel != null)
                {
                    feel.fightersKeepApartOffTheirFeetToo = roomNow;
                }

                ClipFighter.KeepsApartOffHisFeet = roomNow;
            }

            BeginRow();
            Words(new GUIContent("Getting up, his sword hand:", "Where a rising fighter's sword hand is (Combat Feel \"A Rising Fighter's Sword Hand Stays Where It Lies\"). Stays where it lies (new): pinned to the ground where it lay, turned as it lay, until his arm cannot reach it any more; it may turn only a few degrees and never fast. Goes with the get-up's keys (old): the hand is put where each key has it and the sword turns about with it. Fell him with \"Falls on demand\" and watch the sword as he rises."), GUILayout.Width(170f));
            bool handStays = feel != null ? feel.aRisingFightersSwordHandStaysWhereItLies : PhysicsBattler.Motion.GetUp.SwordHandStays;
            bool handStaysNow = handStays;
            if (Press(handStays ? "[stays where it lies]" : "stays where it lies", "Pinned to the ground.")) handStaysNow = true;
            if (Press(!handStays ? "[goes with the keys (old)]" : "goes with the keys (old)", "As before.")) handStaysNow = false;
            EndRow();
            if (handStaysNow != handStays)
            {
                if (feel != null)
                {
                    feel.aRisingFightersSwordHandStaysWhereItLies = handStaysNow;
                }

                PhysicsBattler.Motion.GetUp.SwordHandStays = handStaysNow;
            }

            if (handStaysNow)
            {
                BeginRow();
                Words(new GUIContent("While it is pinned:", "What his pinned sword hand does while he pushes himself up on it (Combat Feel \"A Rising Fighter's Sword Hand Is Held Still\"). Held still (new): once it has come down to the ground it neither moves nor turns until the pin lets it go, and the physical hand is held to that place and turn: the sword should lie still. May turn a little (old): it may turn a few degrees to and fro and only follows its pose with its own muscles: the sword shakes. Fell him the same way with each and watch the sword while his hand is on the ground."), GUILayout.Width(150f));
                bool still = feel != null ? feel.aRisingFightersSwordHandIsHeldStill : PhysicsBattler.Motion.GetUp.SwordHandHeldStill;
                bool stillNow = still;
                if (Press(still ? "[held still]" : "held still", "It neither moves nor turns while pinned.")) stillNow = true;
                if (Press(!still ? "[may turn a little (old)]" : "may turn a little (old)", "As before.")) stillNow = false;
                EndRow();
                if (stillNow != still)
                {
                    if (feel != null)
                    {
                        feel.aRisingFightersSwordHandIsHeldStill = stillNow;
                    }

                    PhysicsBattler.Motion.GetUp.SwordHandHeldStill = stillNow;
                }
            }

            BeginRow();
            Words(new GUIContent("Getting up, his knees:", "Where a rising fighter's knees are (Combat Feel \"A Rising Fighter's Knees Stay Above The Ground\"). Stay above the ground (new): where the get-up's pose would put a knee under the floor, he kneels on it: the knee is put on the ground and the shin lies from it toward where the foot was asked. Where the pose puts them (old): under the floor or not; from his front a knee was 17 cm under it for most of a second. Fell him Forward with each and watch his knees as he pushes up."), GUILayout.Width(150f));
            bool knees = feel != null ? feel.aRisingFightersKneesStayAboveTheGround : PhysicsBattler.Motion.GetUp.KneesClearTheGround;
            bool kneesNow = knees;
            if (Press(knees ? "[stay above the ground]" : "stay above the ground", "No knee under the floor.")) kneesNow = true;
            if (Press(!knees ? "[where the pose puts them (old)]" : "where the pose puts them (old)", "As before.")) kneesNow = false;
            EndRow();
            if (kneesNow != knees)
            {
                if (feel != null)
                {
                    feel.aRisingFightersKneesStayAboveTheGround = kneesNow;
                }

                PhysicsBattler.Motion.GetUp.KneesClearTheGround = kneesNow;
            }

            BeginRow();
            Words(new GUIContent("Getting up, his shield:", "Where a rising fighter's shield is (Combat Feel \"A Rising Fighter's Shield Is Carried Round Him\"). Carried round him (new): from his front or a side the board lies flat on the ground out to his shield side, his forearm flat on it; he pushes up on that forearm, and the board swings out round his side as it comes up. As recorded (old): his shield hand goes flat on the ground under his shoulder, and the board stands in his chest and hips."));
            bool carried = feel != null ? feel.aRisingFightersShieldIsCarriedRoundHim : PhysicsBattler.Motion.GetUp.ShieldCarriedRound;
            bool carriedNow = carried;
            if (Press(carried ? "[carried round him]" : "carried round him", "The board flat out to his side, then round him.")) carriedNow = true;
            if (Press(!carried ? "[as recorded (old)]" : "as recorded (old)", "As before: the shield hand under his shoulder.")) carriedNow = false;
            EndRow();
            if (carriedNow != carried)
            {
                if (feel != null)
                {
                    feel.aRisingFightersShieldIsCarriedRoundHim = carriedNow;
                }

                PhysicsBattler.Motion.GetUp.ShieldCarriedRound = carriedNow;
            }

            BeginRow();
            Words(new GUIContent("The end of a get-up:", "How a get-up hands him over to his stand (Combat Feel \"A Get-Up Ends Into His Stand Over A Moment\"). Eased (new): the whole of him goes from where the get-up left him to where his stand has him over a fifth of a second. At once (old): only his arms were eased; his hips, trunk, head and legs jumped there in one step."));
            bool ends = feel != null ? feel.aGetUpEndsIntoHisStandOverAMoment : ClipFighter.WholeBodyEasesOutOfGetUp;
            bool endsNow = ends;
            if (Press(ends ? "[eased]" : "eased", "The whole of him eased into his stand.")) endsNow = true;
            if (Press(!ends ? "[at once (old)]" : "at once (old)", "As before: only his arms eased.")) endsNow = false;
            EndRow();
            if (endsNow != ends)
            {
                if (feel != null)
                {
                    feel.aGetUpEndsIntoHisStandOverAMoment = endsNow;
                }

                ClipFighter.WholeBodyEasesOutOfGetUp = endsNow;
            }

            if (feel != null && handStaysNow)
            {
                feel.risingSwordHandMayTurn = Lever("Pinned, it may turn", feel.risingSwordHandMayTurn, 0f, 45f, "How far the pinned sword hand may turn from how it lay (degrees).");
                feel.risingSwordHandTurnsAtMost = Lever("It turns at most", feel.risingSwordHandTurnsAtMost, 30f, 720f, "The fastest the sword hand ever turns in a get-up (degrees a second).");
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

            BeginRow();
            if (Press("Step back (2)", "Every fighter standing in its guard steps back from its opponent, as it does to a blow read at the last moment. Best in the target test, slowed down.")) StepBackTest();
            if (Press("Turn (3)", "Every fighter standing still turns a quarter turn and a bit to its right, and the fight turns it back. Best in the target test, slowed down.")) TurnTest();
            if (Press("Knock a leg (4)", "Every fighter's right shin knocked sideways: a small twitch. Needs physical legs.")) KnockLegTest();
            EndRow();
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

            Words($"Panel size {uiScale:0.0}x: the − and + at the top make it smaller and larger.", Styles().Text);
            Heading("Keys");
            Words("R reset · F fight · T turn around\nP pause · . step · M mark the log\n1 targets · 5 tubes · 6 hit · 7 aim (the walk lab: the bar at the top)\n2 step back · 3 turn · 4 knock a leg\nB bystander · L launch · K sever · D ground · J shield\nG move gallery · H rhythm gallery", Styles().Text);
            Heading("Camera");
            Words("Right-drag to orbit, middle-drag to pan, scroll to zoom.", Styles().Text);
            Heading("Joint probe");
            Words(JointConventions.Report, Styles().Text);
        }

        private void DrawRecorder()
        {
            if (recorder == null)
            {
                return;
            }

            BeginRow();
            if (shownRecording)
            {
                Words(new GUIContent($"Rec {recorder.EventCount}", $"Recording to PlaytestLogs/{recorder.SessionName}: {recorder.EventCount} events, {recorder.MarkCount} marks."), Styles().Text, GUILayout.Width(84f));
                if (Press("Mark (M)", "A mark put in the log at this moment, to find it by.")) Mark();
                if (Press("New log", "This log closed and a new one begun.")) recorder.NewSession();
                if (Press("Stop", "The recording stopped.")) recorder.StopSession();
            }
            else
            {
                Words(new GUIContent("Not recording", "No playtest log is being written."), Styles().Text, GUILayout.Width(96f));
                if (Press("Start recording", "A playtest log begun in PlaytestLogs.")) recorder.StartSession();
            }

            EndRow();
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
