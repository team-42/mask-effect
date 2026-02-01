using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class AirSuperiorityAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;
        private IBattleGrid grid;
        private float tickTimer;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.allMechs = allMechs;
            this.grid = grid;
            this.tickTimer = 0f;
        }

        public void Tick(float dt)
        {
            tickTimer -= dt;
            if (tickTimer > 0f) return;
            tickTimer = 2f; // Apply small shield every 2s to allies in same column

            int ownerTile = grid.GetNearestTile(owner.transform.position);
            int ownerCol = grid.GetColumn(ownerTile);

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;
                int allyTile = grid.GetNearestTile(allMechs[i].transform.position);
                int allyCol = grid.GetColumn(allyTile);

                if (allyCol == ownerCol)
                {
                    // Grant a small shield representing 10% damage reduction
                    float shieldAmount = allMechs[i].maxHP * data.value1;
                    allMechs[i].statusHandler.ApplyEffect(new StatusEffect(
                        StatusEffectType.Shield, 2.5f, shieldAmount, owner, owner.mechId));
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
