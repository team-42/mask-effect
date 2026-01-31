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

        [Header("Visual Tiles")]
        [SerializeField] private bool generateVisualTiles = false;
        [SerializeField] private Color playerTileColor = new Color(0.3f, 0.4f, 0.7f);
        [SerializeField] private Color enemyTileColor = new Color(0.7f, 0.35f, 0.3f);
        [SerializeField] private Color neutralTileColor = new Color(0.45f, 0.45f, 0.45f);

        private Dictionary<int, MechController> occupants = new Dictionary<int, MechController>();
        private GameObject[] tileVisuals;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float TileSize => tileSize;
        public Vector3 GridOrigin => gridOrigin;

        private void Awake()
        {
            if (generateVisualTiles)
                GenerateVisualTiles();
        }

        private void GenerateVisualTiles()
        {
            tileVisuals = new GameObject[gridWidth * gridHeight];
            GameObject tilesParent = new GameObject("Tiles");
            tilesParent.transform.SetParent(transform);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer == -1)
                Debug.LogWarning("Layer 'Ground' not defined. Add it in Edit > Project Settings > Tags and Layers.");

            int playerEndX = gridWidth / 4;
            int enemyStartX = gridWidth - gridWidth / 4;

            for (int z = 0; z < gridHeight; z++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    int idx = ToIndex(x, z);
                    Vector3 worldPos = GetWorldPosition(idx);

                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Tile_{x}_{z}";
                    tile.transform.SetParent(tilesParent.transform);
                    tile.transform.position = new Vector3(worldPos.x, -0.05f, worldPos.z);
                    tile.transform.localScale = new Vector3(tileSize * 0.95f, 0.1f, tileSize * 0.95f);

                    if (groundLayer >= 0)
                        tile.layer = groundLayer;

                    Color tileColor;
                    if (x < playerEndX)
                        tileColor = playerTileColor;
                    else if (x >= enemyStartX)
                        tileColor = enemyTileColor;
                    else
                        tileColor = neutralTileColor;

                    var renderer = tile.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = tileColor;

                    tileVisuals[idx] = tile;
                }
            }
        }

        public GameObject GetTileVisual(int tileIndex)
        {
            if (tileVisuals == null || tileIndex < 0 || tileIndex >= tileVisuals.Length)
                return null;
            return tileVisuals[tileIndex];
        }

        public TileZone GetTileZone(int tileIndex)
        {
            var (x, _) = FromIndex(tileIndex);
            if (x < gridWidth / 4)
                return TileZone.Player;
            if (x >= gridWidth - gridWidth / 4)
                return TileZone.Enemy;
            return TileZone.Neutral;
        }

        public int GetTileX(int tileIndex)
        {
            return FromIndex(tileIndex).x;
        }

        public int GetTileZ(int tileIndex)
        {
            return FromIndex(tileIndex).z;
        }

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

        public int GetMirroredTile(int tileIndex)
        {
            var (x, z) = FromIndex(tileIndex);
            int mirrorX = gridWidth - 1 - x;
            return ToIndex(mirrorX, z);
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
