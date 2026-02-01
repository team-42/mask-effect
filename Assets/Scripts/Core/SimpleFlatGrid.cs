using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class SimpleFlatGrid : MonoBehaviour, IBattleGrid
    {
        [SerializeField] private int gridWidth = 28;
        [SerializeField] private int gridHeight = 12;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Vector3 gridOrigin = new Vector3(-14f, 0f, -6f);

        [Header("Visual Tiles")]
        [SerializeField] private bool generateVisualTiles = false;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private Color playerTileColor = new Color(0.75f, 0.82f, 1f);
        [SerializeField] private Color enemyTileColor = new Color(1f, 0.75f, 0.7f);
        [SerializeField] private Color neutralTileColor = new Color(0.9f, 0.9f, 0.9f);

        private Dictionary<int, MechController> occupants = new Dictionary<int, MechController>();
        private GameObject[] tileVisuals;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float TileSize => tileSize;
        public Vector3 GridOrigin => gridOrigin;

        private void Awake()
        {
            // Tile generation is now triggered by BattleManager after networking is ready.
        }

        /// <summary>
        /// Creates visual tiles and network-spawns them so clients receive the grid.
        /// Called by BattleManager on server/offline after networking is initialized.
        /// </summary>
        public void GenerateVisualTiles()
        {
            if (tileVisuals != null) return; // Already generated

            tileVisuals = new GameObject[gridWidth * gridHeight];

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
                    Vector3 worldPos = GetTileWorldPosition(idx);

                    GameObject tile = tilePrefab != null
                        ? Instantiate(tilePrefab)
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = $"Tile_{x}_{z}";
                    tile.transform.position = new Vector3(worldPos.x, -0.05f, worldPos.z);
                    tile.transform.localScale = new Vector3(tileSize * 0.95f, 0.1f, tileSize * 0.95f);

                    if (groundLayer >= 0)
                        tile.layer = groundLayer;

                    // Determine zone and color
                    TileZone zone;
                    Color color;
                    if (x < playerEndX)
                    {
                        zone = TileZone.Player;
                        color = playerTileColor;
                    }
                    else if (x >= enemyStartX)
                    {
                        zone = TileZone.Enemy;
                        color = enemyTileColor;
                    }
                    else
                    {
                        zone = TileZone.Neutral;
                        color = neutralTileColor;
                    }

                    // Set SyncVars on TileController before spawning
                    var tileCtrl = tile.GetComponent<TileController>();
                    if (tileCtrl != null)
                    {
                        tileCtrl.tileIndex = idx;
                        tileCtrl.zone = zone;
                        tileCtrl.tileColor = color;
                    }

                    // Apply color immediately (server/offline sees it right away)
                    var renderer = tile.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = color;

                    // Network-spawn so clients receive the tile
                    NetworkHelper.SpawnOrIgnore(tile);

                    tileVisuals[idx] = tile;
                }
            }
        }

        /// <summary>
        /// Called by TileController on clients to register their tile visual
        /// after receiving the networked spawn.
        /// </summary>
        public void RegisterTileVisual(int tileIndex, GameObject tileGO)
        {
            if (tileVisuals == null)
                tileVisuals = new GameObject[gridWidth * gridHeight];

            if (tileIndex >= 0 && tileIndex < tileVisuals.Length)
                tileVisuals[tileIndex] = tileGO;
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

        public Vector3 GetTileWorldPosition(int tileIndex)
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

        public float GetDistanceBetweenTiles(int tileA, int tileB)
        {
            return Vector3.Distance(GetTileWorldPosition(tileA), GetTileWorldPosition(tileB));
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

        public int[] GetNeighbors(int tileIndex)
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

        public int GetColumn(int tileIndex)
        {
            return FromIndex(tileIndex).x;
        }

        public int GetRow(int tileIndex)
        {
            return FromIndex(tileIndex).z;
        }

        public void ClearAllOccupants()
        {
            occupants.Clear();
        }
    }
}
