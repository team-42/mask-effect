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
                tiles[x, z] = tile;
            }
        }
        // Set the layer for the parent GameObject after all children are parented
        tilesParent.layer = LayerMask.NameToLayer("Ground");
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
