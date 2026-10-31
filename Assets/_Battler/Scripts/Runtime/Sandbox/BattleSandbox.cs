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

            recorder = FindAnyObjectByType<PlaytestRecorder>();
            if (recorder == null)
            {
                recorder = gameObject.AddComponent<PlaytestRecorder>();
            }

            JointConventions.EnsureProbed();
            Note("joint probe: " + JointConventions.Report);
            Spawn();
        }

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
            turnedAway = false;
            brawl = new Brawl(seed);
            fightLoop = new Battle.FightLoop(seed) { Grid = Grid };
            announced = false;
            var spawns = FindObjectsByType<UnitSpawn>();
            System.Array.Sort(spawns, (a, b) => a.team != b.team ? a.team.CompareTo(b.team) : a.body.CompareTo(b.body));
            var count = new Dictionary<string, int>();
            foreach (var spawn in spawns)
            {
                spawn.ClearPreview();
                var prefab = PrefabFor(spawn.body);
                if (prefab == null)
                {
                    message = "Missing unit prefabs. Run Physics Battler > Setup > Run Project Setup.";
                    continue;
                }

                // Made under an inactive holder, so its profile and team are set before it wakes and builds itself.
                var holder = new GameObject("Spawning");
                holder.SetActive(false);
                var unit = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(0f, BoardGrid.TeamYaw(spawn.team), 0f), holder.transform);
                string label = $"{(spawn.team == 0 ? "A" : "B")} {spawn.BodyName}";
                count[label] = count.TryGetValue(label, out int n) ? n + 1 : 1;
                unit.name = count[label] > 1 ? $"{label} {count[label]}" : label;
                unit.Team = spawn.team;
                if (spawn.profileOverride != null)
                {
                    unit.Profile = spawn.profileOverride;
                }

                unit.transform.SetParent(null, true);
                Destroy(holder);

                Tint(unit, TeamMaterial(spawn.team));
                units.Add(unit);
                pendingEquip.Add((unit, spawn.mainHand, spawn.offHand));
            }

            if (spawns.Length == 0)
            {
                message = "No unit markers in the scene. Add a UnitSpawn (or run Physics Battler > Setup > Run Project Setup).";
            }

            Note($"spawn: {units.Count} units on the {Grid.columns}x{Grid.rows} board");
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
            if (!Application.isPlaying)
            {
                return;
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
        }

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

            panelRect = GUILayout.Window(4711, panelRect, DrawPanel, "Physics Battler", GUILayout.Width(290));
            foreach (var unit in units)
            {
                DrawUnitLabel(unit);
            }
        }

        private void DrawPanel(int id)
        {
            GUILayout.Label("R reset · F fight · B bystander · L launch · G gallery · H rhythms · P pause · . step · T turn · M mark");
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
            if (GUILayout.Button("Sever test (K)")) SeverTest();
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

            GUI.Label(new Rect(screen.x - 70f, Screen.height - screen.y - 48f, 140f, 52f), label);
        }
    }
}
