using System.Collections;
using System.Collections.Generic;
using PhysicsBattler.Board;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The battle sandbox: the 8 by 8 board with three units a side (small, medium, large), each
    /// facing the nearest enemy. Milestone 1: they stand in their stances and move as their combat
    /// styles say. Keys: R reset, P pause, . step one physics frame, T turn everyone around, M mark
    /// the playtest log. Right-drag orbits, middle-drag pans, scroll zooms. Every Play session is
    /// recorded by a <see cref="PlaytestRecorder"/> into PlaytestLogs/.
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

        [Header("Line-up (columns A to H, rows 1 to 8)")]
        public Tile[] teamASmallMediumLarge = { new Tile(2, 2), new Tile(4, 2), new Tile(6, 2) };
        public Tile[] teamBSmallMediumLarge = { new Tile(5, 5), new Tile(3, 5), new Tile(1, 5) };

        private readonly List<UnitController> units = new List<UnitController>();
        private PlaytestRecorder recorder;
        private bool turnedAway;
        private float timeScale = 1f;
        private bool paused;
        private string message = "";
        private Rect panelRect = new Rect(12, 12, 290, 10);

        public IReadOnlyList<UnitController> Units => units;

        private void Start()
        {
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
            Time.timeScale = 1f;
        }

        private static void Note(string text)
        {
            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(null, "input", text);
            }
        }

        private BoardGrid Grid => board != null ? board.grid : new BoardGrid();

        public void Spawn()
        {
            Clear();
            turnedAway = false;
            var prefabs = new[] { smallPrefab, mediumPrefab, largePrefab };
            var sizes = new[] { "Small", "Medium", "Large" };
            for (int team = 0; team < 2; team++)
            {
                var tiles = team == 0 ? teamASmallMediumLarge : teamBSmallMediumLarge;
                for (int k = 0; k < 3; k++)
                {
                    if (prefabs[k] == null)
                    {
                        message = "Missing unit prefabs. Run Physics Battler > Setup > Run Project Setup.";
                        continue;
                    }

                    var tile = tiles[k];
                    var position = Grid.Center(tile);
                    var unit = Instantiate(prefabs[k], position, Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f));
                    unit.name = $"{(team == 0 ? "A" : "B")} {sizes[k]}";
                    unit.Team = team;
                    Tint(unit, team == 0 ? teamA : teamB);
                    units.Add(unit);
                }
            }

            Note($"spawn: {units.Count} units on the {Grid.columns}x{Grid.rows} board");
        }

        /// <summary>The body material takes the team colour; the joints keep theirs.</summary>
        private static void Tint(UnitController unit, Material material)
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
        }

        private void FixedUpdate()
        {
            // Everyone faces the nearest enemy (or away from it, to test turning and stepping).
            foreach (var unit in units)
            {
                if (unit == null || !unit.IsInitialized)
                {
                    continue;
                }

                var enemy = Nearest(unit);
                if (enemy != null)
                {
                    Vector3 point = enemy.Root.position;
                    if (turnedAway)
                    {
                        point = unit.Root.position * 2f - point;
                    }

                    unit.FacePoint(point);
                }
            }
        }

        private UnitController Nearest(UnitController unit)
        {
            UnitController best = null;
            float bestDistance = float.MaxValue;
            foreach (var other in units)
            {
                if (other == null || other == unit || other.Team == unit.Team)
                {
                    continue;
                }

                float d = (other.Root.position - unit.Root.position).sqrMagnitude;
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = other;
                }
            }

            return best;
        }

        private void Update()
        {
            if (SandboxInput.KeyPressed(KeyCode.R)) Spawn();
            if (SandboxInput.KeyPressed(KeyCode.P)) TogglePause();
            if (SandboxInput.KeyPressed(KeyCode.Period)) StepOnce();
            if (SandboxInput.KeyPressed(KeyCode.T)) TurnAround();
            if (SandboxInput.KeyPressed(KeyCode.M)) Mark();
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
            panelRect = GUILayout.Window(4711, panelRect, DrawPanel, "Physics Battler", GUILayout.Width(290));
            foreach (var unit in units)
            {
                DrawUnitLabel(unit);
            }
        }

        private void DrawPanel(int id)
        {
            GUILayout.Label("R reset · P pause · . step · T turn around · M mark");
            GUILayout.Label("Right-drag to orbit, middle-drag to pan, scroll to zoom.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset")) Spawn();
            if (GUILayout.Button(turnedAway ? "Face the enemy" : "Turn around")) TurnAround();
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

            GUI.Label(new Rect(screen.x - 50f, Screen.height - screen.y - 20f, 100f, 18f), unit.name);
        }
    }
}
