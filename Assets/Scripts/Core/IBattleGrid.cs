using UnityEngine;

namespace MaskEffect
{
    public interface IBattleGrid
    {
        int GetNearestTile(Vector3 worldPos);
        bool IsTileOccupied(int tileIndex);
        void SetTileOccupant(int tileIndex, MechController mech);
        void ClearTile(int tileIndex);
        float GetDistanceBetweenTiles(int tileA, int tileB);
        Vector3 GetTileWorldPosition(int tileIndex); // Added for pathfinding
        int[] GetNeighbors(int tileIndex); // Added for pathfinding
        int[] GetSpawnTiles(Team team);
        bool IsBackline(int tileIndex, Team relativeTo);
        int GetMirroredTile(int tileIndex);
        int GetTileCount();
    }
}
