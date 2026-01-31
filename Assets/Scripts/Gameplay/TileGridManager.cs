using UnityEngine;

public class TileGridManager : MonoBehaviour
{
    public int gridWidth = 5;
    public int gridHeight = 8;
    private GameObject[,] tiles;

    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        tiles = new GameObject[gridWidth, gridHeight];
        GameObject tilesParent = new GameObject("Tiles");
        tilesParent.transform.SetParent(transform);

        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 tilePosition = new Vector3(x, 0, z);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.position = tilePosition;
                tile.transform.localScale = new Vector3(1, 0.1f, 1); // Make tiles flat
                tile.name = $"Tile_{x}_{z}";
                tile.transform.SetParent(tilesParent.transform);
                // Ensure a collider is present for raycasting
                if (tile.GetComponent<Collider>() == null)
                {
                    tile.AddComponent<BoxCollider>();
                }
                // Check if the "Ground" layer exists before assigning
                int groundLayer = LayerMask.NameToLayer("Ground");
                if (groundLayer == -1)
                {
                    Debug.LogError("Layer 'Ground' is not defined. Please add it in Edit -> Project Settings -> Tags and Layers.");
                }
                else
                {
                    // Set the layer for each individual tile
                    tile.layer = groundLayer;
                }
                tiles[x, z] = tile;
            }
        }
        // The parent GameObject itself doesn't need to be on the "Ground" layer,
        // as raycasting targets individual tiles.
    }

    public Vector3 GetNearestTilePosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x);
        int z = Mathf.RoundToInt(worldPosition.z);

        x = Mathf.Clamp(x, 0, gridWidth - 1);
        z = Mathf.Clamp(z, 0, gridHeight - 1);

        return new Vector3(x, 0, z);
    }
}
