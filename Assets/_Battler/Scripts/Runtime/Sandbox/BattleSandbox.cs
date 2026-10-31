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

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                Time.timeScale = 1f;
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

                var unit = Instantiate(prefab, spawn.transform.position, Quaternion.Euler(0f, BoardGrid.TeamYaw(spawn.team), 0f));
                string label = $"{(spawn.team == 0 ? "A" : "B")} {spawn.BodyName}";
                count[label] = count.TryGetValue(label, out int n) ? n + 1 : 1;
                unit.name = count[label] > 1 ? $"{label} {count[label]}" : label;
                unit.Team = spawn.team;
                if (spawn.profileOverride != null)
                {
                    unit.Profile = spawn.profileOverride;
                }

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
            brawl.Fight = fight;
            brawl.TurnedAway = turnedAway;
            brawl.Step(units, Time.fixedDeltaTime, Time.time);
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
            GUILayout.Label("R reset · F fight · P pause · . step · T turn · M mark");
            GUILayout.Label("Right-drag to orbit, middle-drag to pan, scroll to zoom.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset")) Spawn();
            if (GUILayout.Button(turnedAway ? "Face the enemy" : "Turn around")) TurnAround();
            if (GUILayout.Button(fight ? "Stop fighting" : "Fight")) ToggleFight();
            GUILayout.EndHorizontal();

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
            if (unit.CurrentMove != null)
            {
                label += $"\n{unit.CurrentMove.Move.name.Replace("MD_", "")} → {unit.CurrentMove.Part}";
            }

            GUI.Label(new Rect(screen.x - 70f, Screen.height - screen.y - 34f, 140f, 36f), label);
        }
    }
}
