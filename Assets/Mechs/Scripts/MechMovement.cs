using UnityEngine;

namespace MaskEffect
{
    public class MechMovement : MonoBehaviour
    {
        private MechController mech;
        private IBattleGrid grid;

        public void Initialize(MechController mech, IBattleGrid grid)
        {
            this.mech = mech;
            this.grid = grid;
        }

        /// <summary>
        /// Move towards the target. Returns true if within attack range.
        /// </summary>
        public bool MoveToward(MechController target, float dt)
        {
            if (target == null || !target.isAlive) return false;
            if (grid == null || mech == null) return false;
            if (mech.statusHandler.IsRooted()) return IsInRange(target);

            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist <= mech.range) return true;

            float speed = GetEffectiveMoveSpeed();
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 nextPos = transform.position + direction * speed * dt;

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
            else
            {
                // Blocked — try perpendicular movement
                Vector3 perp = Vector3.Cross(direction, Vector3.up).normalized;
                TryAlternativeMove(currentTile, transform.position + perp * speed * dt, dt);
            }

            return IsInRange(target);
        }

        private void TryAlternativeMove(int currentTile, Vector3 altPos, float dt)
        {
            int altTile = grid.GetNearestTile(altPos);
            if (altTile != currentTile && !grid.IsTileOccupied(altTile))
            {
                grid.ClearTile(currentTile);
                grid.SetTileOccupant(altTile, mech);
                transform.position = altPos;
            }
            // else: completely blocked, wait
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