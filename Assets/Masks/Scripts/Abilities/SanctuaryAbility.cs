using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class SanctuaryAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private IBattleGrid grid;
        private List<MechController> allMechs;
        private float healCooldown;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.grid = grid;
            this.allMechs = allMechs;
            this.healCooldown = data.cooldown;
        }

        public void Tick(float dt)
        {
            healCooldown -= dt;

            if (healCooldown <= 0f)
            {
                healCooldown = data.cooldown;
                HealAdjacentAllies();
            }
        }

        private void HealAdjacentAllies()
        {
            int ownerTile = grid.GetNearestTile(owner.transform.position);
            int[] adjacent = grid.GetNeighbors(ownerTile);

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;

                int allyTile = grid.GetNearestTile(allMechs[i].transform.position);
                bool isAdjacent = false;
                for (int j = 0; j < adjacent.Length; j++)
                {
                    if (adjacent[j] == allyTile) { isAdjacent = true; break; }
                }

                if (isAdjacent)
                {
                    int healAmount = Mathf.RoundToInt(owner.maxHP * data.value2);
                    allMechs[i].currentHP = Mathf.Min(allMechs[i].currentHP + healAmount, allMechs[i].maxHP);
                }
            }
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
