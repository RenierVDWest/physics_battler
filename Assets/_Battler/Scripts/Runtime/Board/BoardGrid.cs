using UnityEngine;

namespace PhysicsBattler.Board
{
    /// <summary>A tile on the board: column (x, left to right) and row (z, near to far).</summary>
    [System.Serializable]
    public struct Tile
    {
        public int column;
        public int row;

        public Tile(int column, int row)
        {
            this.column = column;
            this.row = row;
        }

        public override string ToString() => $"{(char)('A' + column)}{row + 1}";
    }

    /// <summary>
    /// The board: an 8 by 8 grid of square tiles centred on its origin, on the ground. Team 0
    /// deploys on the near rows (low z) facing +Z, team 1 on the far rows facing -Z. Pure maths.
    /// </summary>
    [System.Serializable]
    public class BoardGrid
    {
        public int columns = 8;
        public int rows = 8;
        [Tooltip("Tile size in metres (the large unit is about 2.4 m tall).")]
        public float tileSize = 1.6f;
        public Vector3 origin = Vector3.zero;

        public float Width => columns * tileSize;
        public float Depth => rows * tileSize;

        public bool Contains(Tile tile) => tile.column >= 0 && tile.column < columns && tile.row >= 0 && tile.row < rows;

        public Vector3 Center(Tile tile)
        {
            return origin + new Vector3((tile.column - (columns - 1) * 0.5f) * tileSize, 0f,
                                        (tile.row - (rows - 1) * 0.5f) * tileSize);
        }

        public Tile TileAt(Vector3 point)
        {
            Vector3 local = point - origin;
            int c = Mathf.FloorToInt(local.x / tileSize + columns * 0.5f);
            int r = Mathf.FloorToInt(local.z / tileSize + rows * 0.5f);
            return new Tile(c, r);
        }

        /// <summary>The yaw a team faces at the start: team 0 toward +Z, team 1 toward -Z.</summary>
        public static float TeamYaw(int team) => team == 0 ? 0f : 180f;
    }
}
