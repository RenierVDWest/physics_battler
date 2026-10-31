using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Board
{
    /// <summary>
    /// Draws the board: a checkerboard of tiles just above the ground, with a border. Only for show;
    /// units stand on the ground collider underneath.
    /// </summary>
    [DisallowMultipleComponent]
    public class BoardView : MonoBehaviour
    {
        public BoardGrid grid = new BoardGrid();
        public Material lightTiles;
        public Material darkTiles;
        public Material border;

        private GameObject built;

        private void Awake()
        {
            Build();
        }

        public void Build()
        {
            if (built != null)
            {
                Destroy(built);
            }

            built = new GameObject("Board Tiles");
            built.transform.SetParent(transform, false);
            AddMesh("Light", Tiles(0), lightTiles);
            AddMesh("Dark", Tiles(1), darkTiles);
            AddMesh("Border", Border(), border != null ? border : darkTiles);
        }

        private void AddMesh(string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
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
            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
