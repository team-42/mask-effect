using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class SimpleFlatGrid : MonoBehaviour, IBattleGrid
    {
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Vector3 gridOrigin = new Vector3(-10f, 0f, -5f);

        private Dictionary<int, MechController> occupants = new Dictionary<int, MechController>();

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;

        private int ToIndex(int x, int z)
        {
            return z * gridWidth + x;
        }

        private (int x, int z) FromIndex(int index)
        {
            int z = index / gridWidth;
            int x = index % gridWidth;
            return (x, z);
        }

        public Vector3 GetWorldPosition(int tileIndex)
        {
            var (x, z) = FromIndex(tileIndex);
            return gridOrigin + new Vector3(x * tileSize + tileSize * 0.5f, 0f, z * tileSize + tileSize * 0.5f);
        }

        public int GetNearestTile(Vector3 worldPos)
        {
            Vector3 local = worldPos - gridOrigin;
            int x = Mathf.Clamp(Mathf.FloorToInt(local.x / tileSize), 0, gridWidth - 1);
            int z = Mathf.Clamp(Mathf.FloorToInt(local.z / tileSize), 0, gridHeight - 1);
            return ToIndex(x, z);
        }

        public bool IsTileOccupied(int tileIndex)
        {
            return occupants.ContainsKey(tileIndex) && occupants[tileIndex] != null && occupants[tileIndex].isAlive;
        }

        public void SetTileOccupant(int tileIndex, MechController mech)
        {
            occupants[tileIndex] = mech;
        }

        public void ClearTile(int tileIndex)
        {
            occupants.Remove(tileIndex);
        }

        public float GetDistance(int tileA, int tileB)
        {
            return Vector3.Distance(GetWorldPosition(tileA), GetWorldPosition(tileB));
        }

        public int[] GetSpawnTiles(Team team)
        {
            List<int> tiles = new List<int>();
            int startX, endX;

            if (team == Team.Player)
            {
                startX = 0;
                endX = gridWidth / 4; // left quarter
            }
            else
            {
                startX = gridWidth - gridWidth / 4; // right quarter
                endX = gridWidth;
            }

            for (int x = startX; x < endX; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    int idx = ToIndex(x, z);
                    if (!IsTileOccupied(idx))
                        tiles.Add(idx);
                }
            }

            return tiles.ToArray();
        }

        public int[] GetAdjacentTiles(int tileIndex)
        {
            var (x, z) = FromIndex(tileIndex);
            List<int> adj = new List<int>();

            if (x > 0) adj.Add(ToIndex(x - 1, z));
            if (x < gridWidth - 1) adj.Add(ToIndex(x + 1, z));
            if (z > 0) adj.Add(ToIndex(x, z - 1));
            if (z < gridHeight - 1) adj.Add(ToIndex(x, z + 1));

            return adj.ToArray();
        }

        public bool IsBackline(int tileIndex, Team relativeTo)
        {
            var (x, _) = FromIndex(tileIndex);
            if (relativeTo == Team.Player)
                return x < gridWidth / 6; // rear portion of player side
            else
                return x >= gridWidth - gridWidth / 6; // rear portion of enemy side
        }

        public int GetTileCount()
        {
            return gridWidth * gridHeight;
        }

        public void ClearAllOccupants()
        {
            occupants.Clear();
        }
    }
}