using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Board
{
    /// <summary>
    /// Draws the board: a checkerboard of tiles just above the ground, with a border. Only for show;
    /// units stand on the ground collider underneath. The tiles are built in edit mode too, so the
    /// board is there while you lay out the scene; they are never saved into the scene.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class BoardView : MonoBehaviour
    {
        public BoardGrid grid = new BoardGrid();
        public Material lightTiles;
        public Material darkTiles;
        public Material border;

        private GameObject built;

        private const string TilesName = "[Board tiles - not saved]";

        private void OnEnable()
        {
            Build();
        }

        // Nothing is destroyed here (Unity refuses while objects are being deactivated); if only this
        // component was switched off, the tiles go right after.
        private void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.delayCall += ClearIfDisabled;
            }
#endif
        }

        private void ClearIfDisabled()
        {
            if (this != null && !isActiveAndEnabled)
            {
                Clear();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Objects can't be made inside OnValidate: rebuild right after.
            UnityEditor.EditorApplication.delayCall -= Rebuild;
            UnityEditor.EditorApplication.delayCall += Rebuild;
        }

        private void Rebuild()
        {
            if (this != null && isActiveAndEnabled)
            {
                Build();
            }
        }
#endif

        private void Clear()
        {
            // Also any left over from before a script reload.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (child.name == TilesName)
                {
                    // The meshes are made here and never saved: free them with the tiles.
                    foreach (var filter in child.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if (filter.sharedMesh != null)
                        {
                            if (Application.isPlaying)
                            {
                                Destroy(filter.sharedMesh);
                            }
                            else
                            {
                                DestroyImmediate(filter.sharedMesh);
                            }
                        }
                    }

                    if (Application.isPlaying)
                    {
                        Destroy(child);
                    }
                    else
                    {
                        DestroyImmediate(child);
                    }
                }
            }

            built = null;
        }

        public void Build()
        {
            Clear();
            built = new GameObject(TilesName) { hideFlags = HideFlags.DontSave };
            built.transform.SetParent(transform, false);
            AddMesh("Light", Tiles(0), lightTiles);
            AddMesh("Dark", Tiles(1), darkTiles);
            AddMesh("Border", Border(), border != null ? border : darkTiles);
        }

        private void AddMesh(string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.DontSave };
            go.transform.SetParent(built.transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private Mesh Tiles(int parity)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            float s = grid.tileSize * 0.5f * 0.97f;   // a hairline gap between tiles
            for (int c = 0; c < grid.columns; c++)
            {
                for (int r = 0; r < grid.rows; r++)
                {
                    if ((c + r) % 2 != parity)
                    {
                        continue;
                    }

                    Vector3 centre = grid.Center(new Tile(c, r)) - transform.position + Vector3.up * 0.004f;
                    Quad(verts, tris, centre, s, s);
                }
            }

            return Make(verts, tris);
        }

        private Mesh Border()
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            float w = grid.Width * 0.5f, d = grid.Depth * 0.5f, b = grid.tileSize * 0.12f;
            Vector3 c = grid.origin - transform.position + Vector3.up * 0.003f;
            Quad(verts, tris, c + new Vector3(0f, 0f, d + b * 0.5f), w + b, b * 0.5f);
            Quad(verts, tris, c + new Vector3(0f, 0f, -d - b * 0.5f), w + b, b * 0.5f);
            Quad(verts, tris, c + new Vector3(w + b * 0.5f, 0f, 0f), b * 0.5f, d);
            Quad(verts, tris, c + new Vector3(-w - b * 0.5f, 0f, 0f), b * 0.5f, d);
            return Make(verts, tris);
        }

        private static void Quad(List<Vector3> verts, List<int> tris, Vector3 c, float hx, float hz)
        {
            int i = verts.Count;
            verts.Add(c + new Vector3(-hx, 0f, -hz));
            verts.Add(c + new Vector3(-hx, 0f, hz));
            verts.Add(c + new Vector3(hx, 0f, hz));
            verts.Add(c + new Vector3(hx, 0f, -hz));
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(i + 2);
            tris.Add(i);
            tris.Add(i + 2);
            tris.Add(i + 3);
        }

        private static Mesh Make(List<Vector3> verts, List<int> tris)
        {
            var mesh = new Mesh { hideFlags = HideFlags.DontSave };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
