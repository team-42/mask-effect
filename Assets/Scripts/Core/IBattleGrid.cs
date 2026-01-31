using UnityEngine;

namespace MaskEffect
{
    public interface IBattleGrid
    {
        Vector3 GetWorldPosition(int tileIndex);
        int GetNearestTile(Vector3 worldPos);
        bool IsTileOccupied(int tileIndex);
        void SetTileOccupant(int tileIndex, MechController mech);
        void ClearTile(int tileIndex);
        float GetDistance(int tileA, int tileB);
        int[] GetSpawnTiles(Team team);
        int[] GetAdjacentTiles(int tileIndex);
        bool IsBackline(int tileIndex, Team relativeTo);
        int GetTileCount();
    }
}