using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class RevengeBlinkAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private IBattleGrid grid;
        private List<MechController> allMechs;
        private float cooldownTimer;
        private bool hasDamageBonus;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.grid = grid;
            this.allMechs = allMechs;
            this.cooldownTimer = 0f;
        }

        public void Tick(float dt)
        {
            if (cooldownTimer > 0f)
                cooldownTimer -= dt;
        }

        public void OnTakeDamage(MechController attacker, int damage)
        {
            if (cooldownTimer > 0f) return;
            cooldownTimer = data.cooldown;

            // Teleport to tile at maximum range from any enemy
            Vector3 bestPos = FindSafestTile();
            if (bestPos != Vector3.zero)
            {
                int oldTile = grid.GetNearestTile(owner.transform.position);
                grid.ClearTile(oldTile);
                owner.transform.position = bestPos;
                int newTile = grid.GetNearestTile(bestPos);
                grid.SetTileOccupant(newTile, owner);
            }

            hasDamageBonus = true;
        }

        public void OnAttackLanded(MechController target, int damage)
        {
            if (hasDamageBonus)
            {
                hasDamageBonus = false;
                int bonusDmg = Mathf.RoundToInt(damage * data.value1);
                target.TakeDamage(bonusDmg, owner);
            }
        }

        private Vector3 FindSafestTile()
        {
            int[] teamTiles = grid.GetSpawnTiles(owner.team);
            Vector3 bestPos = Vector3.zero;
            float bestMinDist = -1f;

            for (int i = 0; i < teamTiles.Length; i++)
            {
                if (grid.IsTileOccupied(teamTiles[i])) continue;

                Vector3 tilePos = grid.GetTileWorldPosition(teamTiles[i]);
                float minEnemyDist = float.MaxValue;

                for (int j = 0; j < allMechs.Count; j++)
                {
                    if (!allMechs[j].isAlive || allMechs[j].team == owner.team) continue;
                    float dist = Vector3.Distance(tilePos, allMechs[j].transform.position);
                    if (dist < minEnemyDist) minEnemyDist = dist;
                }

                if (minEnemyDist > bestMinDist)
                {
                    bestMinDist = minEnemyDist;
                    bestPos = tilePos;
                }
            }

            return bestPos;
        }

        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
