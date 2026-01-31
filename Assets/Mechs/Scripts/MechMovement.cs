using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class MechMovement : MonoBehaviour
    {
        private MechController mech;
        private IBattleGrid grid;
        private List<int> currentPath;
        private int pathIndex;

        public void Initialize(MechController mech, IBattleGrid grid)
        {
            this.mech = mech;
            this.grid = grid;
            currentPath = null;
            pathIndex = 0;
        }

        /// <summary>
        /// Move towards the target using pathfinding. Returns true if within attack range.
        /// </summary>
        public bool MoveToward(MechController target, float dt)
        {
            if (target == null || !target.isAlive) return false;
            if (grid == null || mech == null) return false;
            if (mech.statusHandler.IsRooted()) return IsInRange(target);

            // If target is in range, stop moving and return true
            if (IsInRange(target))
            {
                currentPath = null; // Clear path once in range
                return true;
            }

            // If no path or current path is invalid, find a new one
            int startTile = grid.GetNearestTile(transform.position);
            int targetTile = grid.GetNearestTile(target.transform.position);

            if (currentPath == null || pathIndex >= currentPath.Count || currentPath[pathIndex] != startTile)
            {
                currentPath = TilePathfinder.FindPath(grid, startTile, targetTile, mech.allMechs);
                pathIndex = 0;
                if (currentPath != null && currentPath.Count > 0 && currentPath[0] == startTile)
                {
                    pathIndex = 1; // Start from the next tile in the path
                }
            }

            // Follow the path
            if (currentPath != null && pathIndex < currentPath.Count)
            {
                int nextTile = currentPath[pathIndex];
                Vector3 targetPos = grid.GetTileWorldPosition(nextTile);
                Vector3 currentPos = transform.position;

                float step = GetEffectiveMoveSpeed() * dt;
                Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, step);

                int oldTile = grid.GetNearestTile(currentPos);
                int newTile = grid.GetNearestTile(newPos);

                if (oldTile != newTile)
                {
                    if (grid.IsTileOccupied(newTile) && newTile != targetTile)
                    {
                        // Path blocked, recalculate
                        currentPath = null;
                        return false;
                    }
                    grid.ClearTile(oldTile);
                    grid.SetTileOccupant(newTile, mech);
                }
                transform.position = newPos;

                // If we reached the next tile in the path, advance to the next one
                if (Vector3.Distance(transform.position, targetPos) < 0.01f)
                {
                    pathIndex++;
                }
            }
            else
            {
                // If no path found or path completed, fall back to direct movement (or wait)
                // This can happen if target is unreachable or pathfinding failed
                Vector3 direction = (target.transform.position - transform.position).normalized;
                Vector3 nextPos = transform.position + direction * GetEffectiveMoveSpeed() * dt;

                int currentTile = grid.GetNearestTile(transform.position);
                int nextTile = grid.GetNearestTile(nextPos);

                if (nextTile == currentTile || !grid.IsTileOccupied(nextTile))
                {
                    if (nextTile != currentTile)
                    {
                        grid.ClearTile(currentTile);
                        grid.SetTileOccupant(nextTile, mech);
                    }
                    transform.position = nextPos;
                }
            }

            return IsInRange(target);
        }

        public bool IsInRange(MechController target)
        {
            if (target == null) return false;
            return Vector3.Distance(transform.position, target.transform.position) <= mech.range;
        }

        public void TeleportTo(Vector3 worldPos)
        {
            if (grid == null) return;
            int currentTile = grid.GetNearestTile(transform.position);
            int newTile = grid.GetNearestTile(worldPos);
            grid.ClearTile(currentTile);
            transform.position = worldPos;
            grid.SetTileOccupant(newTile, mech);
        }

        public void StepBack()
        {
            if (grid == null || mech == null) return;
            if (mech.currentTarget == null) return;

            Vector3 awayDir = (transform.position - mech.currentTarget.transform.position).normalized;
            Vector3 stepPos = transform.position + awayDir * 1f;

            int currentTile = grid.GetNearestTile(transform.position);
            int stepTile = grid.GetNearestTile(stepPos);

            if (!grid.IsTileOccupied(stepTile) || stepTile == currentTile)
            {
                grid.ClearTile(currentTile);
                transform.position = stepPos;
                grid.SetTileOccupant(stepTile, mech);
            }
        }

        private float GetEffectiveMoveSpeed()
        {
            float speed = mech.moveSpeed;
            speed *= mech.statusHandler.GetSlowMultiplier();
            return Mathf.Max(speed, 0.1f);
        }
    }
}
