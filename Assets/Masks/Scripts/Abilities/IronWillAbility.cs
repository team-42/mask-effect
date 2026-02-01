using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class IronWillAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private IBattleGrid grid;
        private List<MechController> allMechs;
        private float tickTimer;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.grid = grid;
            this.allMechs = allMechs;
            this.tickTimer = 0f;
        }

        public void Tick(float dt)
        {
            tickTimer -= dt;
            if (tickTimer > 0f) return;
            tickTimer = 0.5f; // Check adjacency every 0.5s

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
                    // Remove Slow and Root from adjacent allies
                    allMechs[i].statusHandler.RemoveEffect(StatusEffectType.Slow);
                    allMechs[i].statusHandler.RemoveEffect(StatusEffectType.Root);
                }
            }
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
