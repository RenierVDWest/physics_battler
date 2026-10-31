using System.Collections;
using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    /// <summary>
    /// The prototype scene: two orcs, one with the big club, and a panel of scenario buttons to
    /// test the combat physics. Keys: Space attack, R reset, P pause, . step one physics frame,
    /// G ghost on/off, H hit the attacker from the side, M mark the playtest log. Left-click a body
    /// to push it. Every Play session is recorded by a <see cref="PlaytestRecorder"/> (added if the
    /// scene has none) into PlaytestLogs/.
    /// </summary>
    public class SandboxController : MonoBehaviour
    {
        [Header("Prefabs and data (wired by the project setup)")]
        public UnitController orcPrefab;
        public UnitController goblinPrefab;
        public Weapon clubPrefab;
        public AttackDefinition attack;
        public GameObject cratePrefab;
        public GameObject barrelPrefab;
        public OrbitCamera orbitCamera;

        [Header("Scenario")]
        [Range(1.2f, 2.8f)] public float distance = 2.0f;
        public float sideHitImpulse = 420f;
        public float pushImpulse = 120f;
        public bool autoAttack;
        public float autoAttackInterval = 4f;
        public bool showGhost = true;
        [Tooltip("Hits on allies do damage too (they are always hit physically).")]
        public bool friendlyFireDamage;

        private readonly List<GameObject> extras = new List<GameObject>();
        private readonly List<string> log = new List<string>();
        private UnitController attacker;
        private UnitController victim;
        private float autoTimer;
        private float timeScale = 1f;
        private bool paused;
        private string message = "";
        private Rect panelRect = new Rect(12, 12, 300, 10);

        public UnitController Attacker => attacker;
        public UnitController Victim => victim;

        private void OnEnable()
        {
            CombatEvents.HitLanded += OnHit;
        }

        private void OnDisable()
        {
            CombatEvents.HitLanded -= OnHit;
            Time.timeScale = 1f;
        }

        private PlaytestRecorder recorder;

        private void Start()
        {
            recorder = FindFirstObjectByType<PlaytestRecorder>();
            if (recorder == null)
            {
                recorder = gameObject.AddComponent<PlaytestRecorder>();
            }

            JointConventions.EnsureProbed();
            Note("joint probe: " + JointConventions.Report);
            Spawn();
        }

        private static void Note(string text)
        {
            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(null, "input", text);
            }
        }

        public void Spawn()
        {
            Note($"spawn: two orcs {distance:0.00} m apart");
            Clear();
            if (orcPrefab == null)
            {
                message = "No orc prefab. Run Physics Battler > Setup > Run Project Setup.";
                return;
            }

            Vector3 left = new Vector3(-distance * 0.5f, 0f, 0f);
            Vector3 right = new Vector3(distance * 0.5f, 0f, 0f);
            attacker = SpawnUnit(orcPrefab, "Orc A (club)", left, Vector3.right, 0);
            victim = SpawnUnit(orcPrefab, "Orc B", right, Vector3.left, 1);
            if (clubPrefab != null && attacker != null)
            {
                attacker.Equip(Instantiate(clubPrefab));
            }

            if (orbitCamera != null)
            {
                orbitCamera.target = new Vector3(0f, 1f, 0f);
                orbitCamera.Apply();
            }

            message = "";
        }

        private UnitController SpawnUnit(UnitController prefab, string unitName, Vector3 position, Vector3 facing, int team)
        {
            var unit = Instantiate(prefab, position, Quaternion.LookRotation(facing, Vector3.up));
            unit.name = unitName;
            unit.Team = team;
            unit.ShowGhost = showGhost;
            return unit;
        }

        private void Clear()
        {
            if (attacker != null)
            {
                Destroy(attacker.gameObject);
            }

            if (victim != null)
            {
                Destroy(victim.gameObject);
            }

            foreach (var extra in extras)
            {
                if (extra != null)
                {
                    Destroy(extra);
                }
            }

            extras.Clear();
            attacker = null;
            victim = null;
        }

        private void Update()
        {
            CombatRules.FriendlyFireDamage = friendlyFireDamage;
            if (SandboxInput.KeyPressed(KeyCode.Space)) Attack();
            if (SandboxInput.KeyPressed(KeyCode.R)) Spawn();
            if (SandboxInput.KeyPressed(KeyCode.P)) TogglePause();
            if (SandboxInput.KeyPressed(KeyCode.Period)) StepOnce();
            if (SandboxInput.KeyPressed(KeyCode.G)) SetGhost(!showGhost);
            if (SandboxInput.KeyPressed(KeyCode.H)) HitAttackerFromSide();
            if (SandboxInput.KeyPressed(KeyCode.M)) Mark();

            if (SandboxInput.LeftPressed && !OverPanel())
            {
                PushUnderMouse();
            }

            if (autoAttack && attacker != null && !paused)
            {
                autoTimer += Time.deltaTime;
                if (autoTimer >= autoAttackInterval)
                {
                    autoTimer = 0f;
                    Attack();
                }
            }
        }

        public void Attack()
        {
            if (attacker == null || victim == null)
            {
                return;
            }

            Note("attack ordered");
            bool started = attacker.TryAttack(victim, attack, out string reason);
            message = started ? "" : "Attack: " + reason;
            if (!started && PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(attacker, "attack", "attack refused: " + reason);
            }
        }

        public void Mark()
        {
            if (recorder != null && recorder.Recording)
            {
                recorder.Mark();
                message = $"Mark {recorder.MarkCount} written to the log.";
            }
        }

        public void HitAttackerFromSide()
        {
            if (attacker == null)
            {
                return;
            }

            var chest = attacker.Rig.Part(PartId.Chest);
            Note($"side hit on {attacker.name}: {sideHitImpulse:0} N·s from its left");
            Vector3 impulse = attacker.Root.right * sideHitImpulse + Vector3.up * (sideHitImpulse * 0.1f);
            attacker.Push(chest, impulse, chest.body.worldCenterOfMass - attacker.Root.right * 0.2f);
        }

        /// <summary>A point the current (or a fresh) swing passes through before reaching its target.</summary>
        private bool ArcPoint(float theta, out Vector3 point)
        {
            point = Vector3.zero;
            if (attacker == null || victim == null)
            {
                return false;
            }

            if (!attacker.Planner.Active)
            {
                Attack();
            }

            if (!attacker.Planner.Active)
            {
                return false;
            }

            var plan = attacker.Planner.Plan;
            var reach = attacker.Planner.Reach;
            point = plan.shoulder + SwingSolver.Direction(plan, theta) * (plan.contactReach + reach.sweetSpot * 0.8f);
            return true;
        }

        public void GoblinIntoArc()
        {
            if (goblinPrefab == null || !ArcPoint(-30f, out var point))
            {
                return;
            }

            Vector3 ground = new Vector3(point.x, 0f, point.z);
            Vector3 facing = attacker.Root.position - ground;
            facing.y = 0f;
            Note($"goblin placed into the arc at ({ground.x:0.00}, {ground.z:0.00})");
            var goblin = SpawnUnit(goblinPrefab, "Goblin", ground, facing.sqrMagnitude > 0.01f ? facing : Vector3.left, 1);
            extras.Add(goblin.gameObject);
        }

        public void BarrelIntoArc()
        {
            if (barrelPrefab == null || !ArcPoint(-35f, out var point))
            {
                return;
            }

            Note($"barrel placed into the arc at ({point.x:0.00}, {point.z:0.00})");
            extras.Add(Instantiate(barrelPrefab, new Vector3(point.x, 0.5f, point.z), Quaternion.identity));
        }

        public void DropCrateBetween()
        {
            if (cratePrefab == null || attacker == null || victim == null)
            {
                return;
            }

            Vector3 middle = (attacker.Root.position + victim.Root.position) * 0.5f + Vector3.up * 2.6f;
            Note("crate dropped between them");
            extras.Add(Instantiate(cratePrefab, middle, Quaternion.Euler(0f, 20f, 0f)));
        }

        public void KillVictim()
        {
            if (victim != null)
            {
                Note($"kill {victim.name}");
                victim.Kill();
            }
        }

        private void SetGhost(bool on)
        {
            showGhost = on;
            foreach (var unit in FindObjectsByType<UnitController>(FindObjectsSortMode.None))
            {
                unit.ShowGhost = on;
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

        private void PushUnderMouse()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(SandboxInput.MousePosition);
            int mask = LayerMask.GetMask(Layers.Body, Layers.Weapon, Layers.Prop);
            if (!UnityEngine.Physics.Raycast(ray, out var hit, 100f, mask == 0 ? ~0 : mask, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Vector3 impulse = (ray.direction + Vector3.up * 0.15f).normalized * pushImpulse;
            var part = hit.collider.GetComponentInParent<BodyPart>();
            if (part != null && part.Owner != null)
            {
                Note($"push {part.Owner.name} {part.id}: {pushImpulse:0} N·s");
                part.Owner.Push(part, impulse, hit.point);
            }
            else if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(impulse, hit.point, ForceMode.Impulse);
            }
        }

        private void OnHit(HitEvent hit)
        {
            string line = $"{hit.attacker.name} > {hit.victim.name} {hit.part}: {hit.speed:0.0} m/s, " +
                          $"{hit.damage:0} dmg, -{hit.stabilityLoss:0} stab{(hit.friendly ? " (ally)" : "")}";
            log.Insert(0, line);
            if (log.Count > 8)
            {
                log.RemoveAt(log.Count - 1);
            }
        }

        private bool OverPanel()
        {
            Vector2 mouse = SandboxInput.MousePosition;
            mouse.y = Screen.height - mouse.y;
            return panelRect.Contains(mouse);
        }

        private void OnGUI()
        {
            panelRect = GUILayout.Window(4711, panelRect, DrawPanel, "Physics Battler sandbox", GUILayout.Width(300));
            DrawUnitLabel(attacker);
            DrawUnitLabel(victim);
            foreach (var extra in extras)
            {
                if (extra != null && extra.TryGetComponent<UnitController>(out var unit))
                {
                    DrawUnitLabel(unit);
                }
            }
        }

        private void DrawPanel(int id)
        {
            GUILayout.Label("Space attack · R reset · P pause · . step · G ghost · H side hit · M mark");
            GUILayout.Label("Left-click a body to push it, right-drag to orbit, scroll to zoom.");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Attack")) Attack();
            if (GUILayout.Button("Reset")) Spawn();
            GUILayout.EndHorizontal();
            autoAttack = GUILayout.Toggle(autoAttack, $" Attack every {autoAttackInterval:0} s");

            GUILayout.Label($"Distance (applies on Reset): {distance:0.00} m");
            distance = GUILayout.HorizontalSlider(distance, 1.2f, 2.8f);

            GUILayout.Space(6);
            GUILayout.Label("Interference tests");
            if (GUILayout.Button("Goblin into the arc")) GoblinIntoArc();
            if (GUILayout.Button("Barrel into the arc")) BarrelIntoArc();
            if (GUILayout.Button("Drop a crate between them")) DropCrateBetween();
            if (GUILayout.Button($"Hit the attacker from the side ({sideHitImpulse:0} N s)")) HitAttackerFromSide();
            sideHitImpulse = GUILayout.HorizontalSlider(sideHitImpulse, 50f, 1200f);
            if (GUILayout.Button("Kill Orc B")) KillVictim();

            GUILayout.Space(6);
            GUILayout.Label($"Push strength: {pushImpulse:0} N s");
            pushImpulse = GUILayout.HorizontalSlider(pushImpulse, 20f, 600f);
            friendlyFireDamage = GUILayout.Toggle(friendlyFireDamage, " Allies take damage");
            bool ghost = GUILayout.Toggle(showGhost, " Show ghost (Gizmos on)");
            if (ghost != showGhost)
            {
                SetGhost(ghost);
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

            if (attacker != null && attacker.Planner != null && attacker.Planner.Active)
            {
                var plan = attacker.Planner.Plan;
                GUILayout.Label($"Swing: {attacker.Planner.Current}, θ {attacker.Planner.Theta:0}°, roll {plan.rollDegrees:0}°, " +
                                $"reach {plan.contactReach:0.00} m{(plan.crowdedFactor < 1f ? " (crowded)" : "")}");
            }

            if (!string.IsNullOrEmpty(message))
            {
                GUILayout.Label(message);
            }

            GUILayout.Label("Joint probe: " + JointConventions.Report);
            DrawRecorder();
            GUILayout.Space(6);
            GUILayout.Label("Hits");
            foreach (var line in log)
            {
                GUILayout.Label(line);
            }

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

            Vector3 head = unit.Rig.Part(PartId.Head).transform.position + Vector3.up * 0.45f * unit.Scale;
            Vector3 screen = Camera.main.WorldToScreenPoint(head);
            if (screen.z < 0f)
            {
                return;
            }

            var state = unit.State;
            float width = 130f;
            var rect = new Rect(screen.x - width * 0.5f, Screen.height - screen.y - 34f, width, 18f);
            GUI.Label(rect, $"{unit.name}: {state.Kind}");
            var bar = new Rect(rect.x, rect.y + 18f, width, 6f);
            GUI.DrawTexture(bar, Texture2D.grayTexture);
            GUI.DrawTexture(new Rect(bar.x, bar.y, width * state.Stability / unit.Profile.maxStability, bar.height), Texture2D.whiteTexture);
            var hp = new Rect(rect.x, rect.y + 26f, width, 4f);
            GUI.DrawTexture(new Rect(hp.x, hp.y, width * state.Health / unit.Profile.maxHealth, hp.height), Texture2D.redTexture);
        }
    }
}
