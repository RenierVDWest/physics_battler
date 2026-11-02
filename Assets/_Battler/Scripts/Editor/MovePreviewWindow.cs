using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Editor.Setup;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using PhysicsBattler.Sandbox;
using PhysicsBattler.Units;
using UnityEditor;
using UnityEngine;

namespace PhysicsBattler.Editor
{
    /// <summary>
    /// Physics Battler > Move Preview: plays a move against a dummy target at a chosen size,
    /// distance and body part, in the open scene (off to the side of the board, never saved).
    /// Scrub through the move; the Scene view shows the striking part's path (yellow to red), the
    /// locked aim point, and where the striking part is now. The window reports whether the
    /// planner would choose the blow, and how close it passes the aim point at contact. Change the
    /// move asset and the preview follows. It does for moves what Draw Attack Paths does in Hexamus.
    /// The arm rebuild's bench, under Arm keys: jump to a blow's wind-up, contact or follow-through
    /// and set its keys there, the hand (dragged in the Scene view), the wrist and the elbow null;
    /// the Scene view draws the arm, the elbow's circle and its null, the hand's way and its keys.
    /// </summary>
    public sealed class MovePreviewWindow : EditorWindow
    {
        private const string RootName = "[Move preview - not saved]";
        private const float Step = 1f / 60f;

        private BodySize attackerBody = BodySize.Medium;
        private WeaponDefinition weapon;
        private WeaponDefinition offHand;
        private MoveDefinition move;
        private BodySize targetBody = BodySize.Medium;
        private AimPart part = AimPart.Chest;
        private float distance = 1.6f;
        private float scrub = 0.5f;
        private bool showPath = true;

        private GameObject root;
        private UnitController attacker;
        private UnitController target;
        private readonly List<Vector3> path = new List<Vector3>();
        private Vector3 aim;
        private Vector3 strikeNow;
        private string verdict = "";
        private float plannedError = -1f;
        private float playedError = -1f;
        private float contactFraction;
        private bool dirty = true;

        // ---- The arm bench ------------------------------------------------------------------
        private enum ArmKey { None, WindUp, Contact, Follow }
        private ArmKey key = ArmKey.None;
        private MoveState state;
        private readonly List<Vector3> handPath = new List<Vector3>();
        private Vector3 shoulderAt, elbowAt, wristAt, gripAt;
        private ArmSolve armAt;
        private bool haveArm;
        private float windUpTime, followTime;

        [MenuItem("Physics Battler/Move Preview", priority = 20)]
        public static void Open()
        {
            GetWindow<MovePreviewWindow>("Move Preview").Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DrawScene;
            if (weapon == null)
            {
                weapon = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(BattlerPaths.Weapon("Sword"));
            }

            dirty = true;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DrawScene;
            Clear();
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("The move preview works outside Play mode. (In play, the playtest log's blow film strips show each blow.)", MessageType.Info);
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Attacker", EditorStyles.boldLabel);
            attackerBody = (BodySize)EditorGUILayout.EnumPopup("Body", attackerBody);
            weapon = (WeaponDefinition)EditorGUILayout.ObjectField("Weapon", weapon, typeof(WeaponDefinition), false);
            offHand = (WeaponDefinition)EditorGUILayout.ObjectField("Off hand", offHand, typeof(WeaponDefinition), false);
            var moves = weapon != null && weapon.moves != null ? weapon.moves : new MoveDefinition[0];
            var names = new string[moves.Length];
            int current = 0;
            for (int i = 0; i < moves.Length; i++)
            {
                names[i] = moves[i] != null ? moves[i].name : "(missing)";
                if (moves[i] == move)
                {
                    current = i;
                }
            }

            if (moves.Length > 0)
            {
                current = EditorGUILayout.Popup("Move", current, names);
                move = moves[current];
            }
            else
            {
                move = null;
                EditorGUILayout.HelpBox("Pick a weapon with moves.", MessageType.None);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);
            targetBody = (BodySize)EditorGUILayout.EnumPopup("Body", targetBody);
            part = (AimPart)EditorGUILayout.EnumPopup("Aim at", part);
            distance = EditorGUILayout.Slider("Distance (m)", distance, 0.3f, 3.2f);
            showPath = EditorGUILayout.Toggle("Draw the path", showPath);
            if (EditorGUI.EndChangeCheck())
            {
                dirty = true;
            }

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            scrub = EditorGUILayout.Slider("Time", scrub, 0f, 1f);
            bool scrubbed = EditorGUI.EndChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Start"))
                {
                    scrub = 0f;
                    scrubbed = true;
                }

                if (GUILayout.Button("Contact"))
                {
                    scrub = contactFraction;
                    scrubbed = true;
                }

                if (GUILayout.Button("End"))
                {
                    scrub = 1f;
                    scrubbed = true;
                }

                if (GUILayout.Button("Rebuild"))
                {
                    dirty = true;
                }
            }

            if (scrubbed)
            {
                key = ArmKey.None;
            }

            ArmKeysGui(ref scrubbed);
            if (dirty || scrubbed)
            {
                Simulate(dirty);
                dirty = false;
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(verdict, plannedError >= 0f && plannedError <= MovePlanner.Tolerance ? MessageType.Info : MessageType.Warning);
            if (GUILayout.Button("Frame in the Scene view") && attacker != null)
            {
                var view = SceneView.lastActiveSceneView;
                if (view != null)
                {
                    view.LookAt(0.5f * (attacker.transform.position + target.transform.position) + Vector3.up, Quaternion.Euler(15f, 90f, 0f), 3.5f);
                }
            }
        }

        private void Clear()
        {
            if (root != null)
            {
                DestroyImmediate(root);
            }

            var stray = GameObject.Find(RootName);
            if (stray != null)
            {
                DestroyImmediate(stray);
            }

            root = null;
            attacker = target = null;
        }

        private UnitController Make(BodySize body, Vector3 at, float yaw, int team)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BattlerPaths.UnitPrefab(body.ToString()));
            if (prefab == null)
            {
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.transform.SetParent(root.transform, false);
            go.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            go.name = $"{body} ({(team == 0 ? "attacker" : "target")})";
            var unit = go.GetComponent<UnitController>();
            unit.Team = team;
            if (!unit.InitializePreview())
            {
                return null;
            }

            var sandbox = FindAnyObjectByType<BattleSandbox>();
            if (sandbox != null)
            {
                BattleSandbox.Tint(unit, sandbox.TeamMaterial(team));
            }

            return unit;
        }

        /// <summary>Builds attacker and target (if asked), then plays the move from its start to the scrubbed time.</summary>
        private void Simulate(bool rebuild)
        {
            Clear();
            path.Clear();
            handPath.Clear();
            haveArm = false;
            this.state = null;
            plannedError = playedError = -1f;
            if (move == null || weapon == null)
            {
                verdict = "Pick a weapon and a move.";
                return;
            }

            // Off to the side of the board, so the preview never sits on the lineup.
            Vector3 origin = new Vector3(-9f, 0f, 0f);
            root = new GameObject(RootName) { hideFlags = HideFlags.DontSave };
            attacker = Make(attackerBody, origin, 0f, 0);
            target = Make(targetBody, origin + Vector3.forward * distance, 180f, 1);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags |= HideFlags.DontSave;
            }

            if (attacker == null || target == null)
            {
                verdict = "Unit prefabs are missing: run Physics Battler > Setup > Run Project Setup.";
                return;
            }

            attacker.Equip(weapon, offHand);
            for (int i = 0; i < 20; i++)
            {
                attacker.PreviewPose(Step);
                target.PreviewPose(Step);
            }

            var state = MovePlanner.Prepare(attacker, target, move, part);
            if (state == null)
            {
                verdict = $"Out of reach: a {attackerBody} can't lunge far enough with {move.name} to strike a {targetBody}'s {part} from {distance:0.00} m"
                          + " (or the target is too close).";
                return;
            }

            this.state = state;
            aim = state.Aim;
            contactFraction = state.ContactTime;
            windUpTime = move.WindUpEnd;
            followTime = move.SwingFollowEnd;
            plannedError = attacker.TryOut(state);
            if (!move.targets.Contains(part))
            {
                verdict = $"{move.name} isn't set to aim at the {part} (its Targets), so the planner won't choose it. Shown anyway.\n";
            }
            else
            {
                verdict = "";
            }

            void OnContact(UnitController unit, MoveState s, ContactReport report)
            {
                if (unit == attacker)
                {
                    playedError = report.AnimError;
                }
            }

            UnitController.MoveContact += OnContact;
            try
            {
                attacker.StartMove(state);
                int steps = Mathf.RoundToInt(scrub * state.Duration / Step);
                int all = Mathf.CeilToInt(state.Duration / Step);
                for (int i = 0; i < all && attacker.IsAttacking; i++)
                {
                    if (i == steps)
                    {
                        strikeNow = attacker.Pose.StrikePoint;
                        RecordArm();
                        SnapshotPose();
                    }

                    attacker.PreviewPose(Step);
                    target.PreviewPose(Step);
                    path.Add(attacker.Pose.StrikePoint);
                    handPath.Add(attacker.Anim.Part(PartId.HandR).position);
                }

                if (steps >= all)
                {
                    strikeNow = path.Count > 0 ? path[path.Count - 1] : aim;
                    RecordArm();
                    SnapshotPose();
                }

                RestorePose();
            }
            finally
            {
                UnitController.MoveContact -= OnContact;
            }

            verdict += $"{move.name}: {state.Duration:0.00} s, contact at {state.ContactTime * state.Duration:0.00} s, lunge {state.Lunge.magnitude:0.00} m, "
                       + $"the striking part {state.Arc:0}° round from the release to the follow-through's end.\n"
                       + $"Planner: {(plannedError <= MovePlanner.Tolerance ? "lands" : "misses")} ({plannedError * 100f:0.0} cm at contact). "
                       + $"Played: passes {(playedError >= 0f ? (playedError * 100f).ToString("0.0") + " cm" : "-")} from the aim point.";
        }

        // The units are left posed at the scrubbed time: the simulation runs on to the end for the
        // path, so the pose at the scrubbed step is kept and put back.
        private readonly List<(Transform t, Vector3 p, Quaternion r)> snapshot = new List<(Transform, Vector3, Quaternion)>();

        private void SnapshotPose()
        {
            snapshot.Clear();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                snapshot.Add((t, t.position, t.rotation));
            }
        }

        private void RestorePose()
        {
            foreach (var (t, p, r) in snapshot)
            {
                if (t != null)
                {
                    t.SetPositionAndRotation(p, r);
                }
            }
        }

        private void DrawScene(SceneView view)
        {
            if (attacker == null || path.Count == 0 || !showPath)
            {
                return;
            }

            for (int i = 1; i < path.Count; i++)
            {
                Handles.color = Color.Lerp(Color.yellow, Color.red, (float)i / path.Count);
                Handles.DrawLine(path[i - 1], path[i], 2f);
            }

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, aim, Quaternion.identity, 0.05f, EventType.Repaint);
            Handles.Label(aim + Vector3.up * 0.08f, $"aim: {part}");
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, strikeNow, Quaternion.identity, 0.035f, EventType.Repaint);
            DrawArm();
        }

        // ---- The arm bench ------------------------------------------------------------------

        /// <summary>The arm keys: jump to a key's moment and set the hand, the wrist and the elbow null there.</summary>
        private void ArmKeysGui(ref bool scrubbed)
        {
            if (move == null || move.shape == MoveShape.Path)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Arm keys", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Wind-up"))
                {
                    key = ArmKey.WindUp;
                    scrub = move.WindUpEnd;
                    scrubbed = true;
                }

                if (GUILayout.Button("Contact"))
                {
                    key = ArmKey.Contact;
                    scrub = move.ContactTime();
                    scrubbed = true;
                }

                if (GUILayout.Button("Follow-through"))
                {
                    key = ArmKey.Follow;
                    scrub = move.SwingFollowEnd;
                    scrubbed = true;
                }
            }

            if (key == ArmKey.None)
            {
                EditorGUILayout.HelpBox("Jump to a key to set it. The Scene view draws the arm, the elbow's circle and its null (orange), the hand's way (green) and its keys.", MessageType.None);
                return;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 hand = key == ArmKey.WindUp ? move.handWindUp : key == ArmKey.Follow ? move.handFollow : Vector3.zero;
            if (key != ArmKey.Contact)
            {
                hand = EditorGUILayout.Vector3Field("Hand (from the shoulder, m)", hand);
            }
            else
            {
                EditorGUILayout.LabelField("Hand: solved when the blow is planned, to land on the aim.");
            }

            Vector3 wrist = move.WristKey(key == ArmKey.WindUp ? 1 : key == ArmKey.Contact ? 2 : 3);
            wrist.x = EditorGUILayout.Slider("Wrist turn (palm down +)", wrist.x, -ArmRig.TwistLimit, ArmRig.TwistLimit);
            wrist.y = EditorGUILayout.Slider("Wrist flexion (to the palm +)", wrist.y, -ArmRig.FlexLimit, ArmRig.FlexLimit);
            wrist.z = EditorGUILayout.Slider("Wrist deviation (to the thumb +)", wrist.z, -ArmRig.DeviationLimit, ArmRig.RadialLimit);
            float elbow = key == ArmKey.WindUp ? move.elbowWindUp : key == ArmKey.Contact ? move.elbowContact : move.elbowFollow;
            elbow = EditorGUILayout.Slider("Elbow null (0 hanging, 90 out, 180 up, -90 across)", elbow, -180f, 180f);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(move, "Set an arm key");
                SetKey(hand, wrist, elbow);
                dirty = true;
            }

            if (haveArm)
            {
                EditorGUILayout.HelpBox($"Wrist {armAt.twist:0}/{armAt.flexion:0}/{armAt.deviation:0}" + (armAt.wristExcess > 0.5f ? $", {armAt.wristExcess:0}° past its limits" : ", inside its limits")
                                        + $"; elbow bent {armAt.elbowBend:0}°" + (armAt.shoulderPast > 0.5f ? $"; the shoulder {armAt.shoulderPast:0}° past its reach" : "")
                                        + (armAt.clearanceShort > 0.001f ? $"; the arm {armAt.clearanceShort * 100f:0.0} cm into its clearance" : ""), MessageType.None);
            }
        }

        private void SetKey(Vector3 hand, Vector3 wrist, float elbow)
        {
            switch (key)
            {
                case ArmKey.WindUp:
                    move.handWindUp = hand;
                    move.wristWindUp = wrist;
                    move.elbowWindUp = elbow;
                    break;
                case ArmKey.Contact:
                    move.wristContact = wrist;
                    move.elbowContact = elbow;
                    break;
                case ArmKey.Follow:
                    move.handFollow = hand;
                    move.wristFollow = wrist;
                    move.elbowFollow = elbow;
                    break;
            }

            EditorUtility.SetDirty(move);
        }

        private void RecordArm()
        {
            var anim = attacker.Anim;
            shoulderAt = anim.Part(PartId.UpperArmR).position;
            elbowAt = anim.Part(PartId.ForearmR).position;
            wristAt = anim.Part(PartId.HandR).position;
            gripAt = anim.SlotWeaponR != null ? anim.SlotWeaponR.position : wristAt;
            armAt = attacker.Pose.LastWeaponArm;
            haveArm = true;
        }

        private void DrawArm()
        {
            if (!haveArm || state == null)
            {
                return;
            }

            // The arm, the elbow's circle round the shoulder-to-wrist line and its null.
            Handles.color = Color.white;
            Handles.DrawLine(shoulderAt, elbowAt, 3f);
            Handles.DrawLine(elbowAt, wristAt, 3f);
            Handles.DrawLine(wristAt, gripAt, 2f);
            if (armAt.circleRadius > 1e-3f)
            {
                Handles.color = new Color(1f, 0.55f, 0.1f, 0.8f);
                Handles.DrawWireDisc(armAt.circleCentre, (wristAt - shoulderAt).normalized, armAt.circleRadius);
                Handles.DrawLine(armAt.circleCentre, armAt.circleCentre + armAt.nullWay * armAt.circleRadius, 3f);
                Handles.Label(armAt.circleCentre + armAt.nullWay * (armAt.circleRadius + 0.03f), "elbow null");
            }

            // The hand's way, and its keys (the contact's solved when planned).
            Handles.color = new Color(0.3f, 0.9f, 0.4f);
            for (int i = 1; i < handPath.Count; i++)
            {
                Handles.DrawLine(handPath[i - 1], handPath[i], 1.5f);
            }

            if (state.HasContactHand)
            {
                Vector3 contactHand = state.Aim + state.Frame * state.ContactHand;
                Handles.SphereHandleCap(0, contactHand, Quaternion.identity, 0.04f, EventType.Repaint);
                Handles.Label(contactHand + Vector3.up * 0.06f, "contact hand");
            }

            if (key == ArmKey.WindUp || key == ArmKey.Follow)
            {
                // (The key is where the hand goes from the shoulder at the key's moment, in the blow's
                // aim frame, at a medium body's size.)
                Vector3 local = key == ArmKey.WindUp ? move.handWindUp : move.handFollow;
                Vector3 at = shoulderAt + state.Frame * (local * state.Scale);
                Handles.DrawDottedLine(shoulderAt, at, 4f);
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.PositionHandle(at, state.Frame);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(move, "Move a hand key");
                    Vector3 hand = Quaternion.Inverse(state.Frame) * (moved - shoulderAt) / Mathf.Max(1e-3f, state.Scale);
                    SetKey(hand, move.WristKey(key == ArmKey.WindUp ? 1 : 3), key == ArmKey.WindUp ? move.elbowWindUp : move.elbowFollow);
                    dirty = true;
                    Repaint();
                }
            }
        }
    }
}
