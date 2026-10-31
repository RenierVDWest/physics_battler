using PhysicsBattler.Board;
using PhysicsBattler.Combat;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Sandbox
{
    public enum BodySize
    {
        Small,
        Medium,
        Large,
    }

    /// <summary>
    /// Where a unit starts: its tile, team, body and weapons. Lives in the scene, so the lineup is
    /// laid out (and saved) in the editor. In edit mode it shows the unit standing on its tile in its
    /// stance, holding its weapons, in its team's colour; that preview is never saved. In play mode
    /// the preview goes and the <see cref="BattleSandbox"/> spawns the real unit here.
    ///
    /// Move it on the board and it snaps to the nearest tile; it faces the other side of the board.
    /// </summary>
    [ExecuteAlways]
    [SelectionBase]
    [DisallowMultipleComponent]
    public class UnitSpawn : MonoBehaviour
    {
        private const string PreviewName = "[Unit preview - not saved]";

        public BodySize body = BodySize.Medium;
        [Range(0, 1)] public int team;
        [Tooltip("Main hand (right). Empty: bare hands.")]
        public WeaponDefinition mainHand;
        [Tooltip("Off hand (left): a shield, or empty.")]
        public WeaponDefinition offHand;
        [Tooltip("Optional: a profile other than the body's own (Data/Units/UP_<Body>).")]
        public UnitProfile profileOverride;
        [Tooltip("Snap to the nearest tile's centre and face the other team.")]
        public bool snapToTile = true;
        [Tooltip("Show the unit in the scene while editing.")]
        public bool showPreview = true;

        private GameObject preview;

        public string BodyName => body.ToString();

        public Tile Tile(BoardGrid grid) => grid.TileAt(transform.position);

        private void OnEnable()
        {
            if (!Application.isPlaying && !WillPlay)
            {
                Snap();
                BuildPreview();
            }
        }

        // Nothing is destroyed here (Unity refuses while objects are being deactivated, and a destroyed
        // marker takes its preview with it). If only this component was switched off, the preview
        // goes right after.
        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += ClearIfDisabled;
#endif
        }

        private void ClearIfDisabled()
        {
            if (this != null && !isActiveAndEnabled)
            {
                ClearPreview();
            }
        }

        private static bool WillPlay
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
                return false;
#endif
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= Refresh;
            UnityEditor.EditorApplication.delayCall += Refresh;
        }

        private void Refresh()
        {
            if (this == null || !isActiveAndEnabled || Application.isPlaying || WillPlay)
            {
                return;
            }

            Snap();
            BuildPreview();
        }

        private void Update()
        {
            // Dragged in the scene: snap to the tile under it (the preview, a child, comes along).
            if (!Application.isPlaying && transform.hasChanged)
            {
                transform.hasChanged = false;
                Snap();
                if (preview != null)
                {
                    preview.transform.SetPositionAndRotation(transform.position, transform.rotation);
                }
            }
        }
#endif

        /// <summary>Onto the nearest tile's centre, facing the other team.</summary>
        public void Snap()
        {
            var grid = Grid();
            if (snapToTile && grid != null)
            {
                var tile = grid.TileAt(transform.position);
                tile.column = Mathf.Clamp(tile.column, 0, grid.columns - 1);
                tile.row = Mathf.Clamp(tile.row, 0, grid.rows - 1);
                transform.position = grid.Center(tile);
            }

            transform.rotation = Quaternion.Euler(0f, BoardGrid.TeamYaw(team), 0f);
            transform.hasChanged = false;
        }

        private static BoardGrid Grid()
        {
            var view = FindAnyObjectByType<BoardView>();
            return view != null ? view.grid : null;
        }

        public void ClearPreview()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (child.name == PreviewName)
                {
                    DestroyImmediate(child);
                }
            }

            preview = null;
        }

        /// <summary>The unit in its stance, armed and coloured: built from the sandbox's prefabs, never saved.</summary>
        public void BuildPreview()
        {
            ClearPreview();
            var sandbox = FindAnyObjectByType<BattleSandbox>();
            if (!showPreview || Application.isPlaying || sandbox == null)
            {
                return;
            }

            var prefab = sandbox.PrefabFor(body);
            if (prefab == null)
            {
                return;
            }

            var unit = Instantiate(prefab, transform.position, transform.rotation, transform);
            preview = unit.gameObject;
            preview.name = PreviewName;
            foreach (var t in preview.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags |= HideFlags.DontSave | HideFlags.NotEditable;
            }

            unit.Team = team;
            unit.Profile = profileOverride != null ? profileOverride : sandbox.ProfileFor(body);
            if (!unit.InitializePreview())
            {
                return;
            }

            unit.Equip(mainHand, offHand);
            // A moment of idle, so it stands in its stance rather than the rest pose.
            for (int i = 0; i < 30; i++)
            {
                unit.PreviewPose(1f / 60f);
            }

            BattleSandbox.Tint(unit, sandbox.TeamMaterial(team));
            foreach (var t in preview.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.hideFlags |= HideFlags.DontSave | HideFlags.NotEditable;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = team == 0 ? new Color(0.9f, 0.6f, 0.3f, 0.8f) : new Color(0.35f, 0.55f, 0.9f, 0.8f);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.01f, new Vector3(1.2f, 0.02f, 1.2f));
        }
    }
}
