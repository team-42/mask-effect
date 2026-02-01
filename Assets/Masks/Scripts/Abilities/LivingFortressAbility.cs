using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class LivingFortressAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private IBattleGrid grid;
        private List<MechController> allMechs;
        private float shieldCooldown;
        private float auraTick;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.grid = grid;
            this.allMechs = allMechs;
            this.shieldCooldown = data.cooldown;
            this.auraTick = 0f;
        }

        public void Tick(float dt)
        {
            // Passive aura: allies behind Colossus get periodic small shield (simulating -25% damage)
            auraTick -= dt;
            if (auraTick <= 0f)
            {
                auraTick = 2f;
                ApplyPositionalShield();
            }

            // Periodic AoE shield every cooldown seconds
            shieldCooldown -= dt;
            if (shieldCooldown <= 0f)
            {
                shieldCooldown = data.cooldown;
                ApplyAoEShield();
            }
        }

        private void ApplyPositionalShield()
        {
            int ownerTile = grid.GetNearestTile(owner.transform.position);
            int ownerCol = grid.GetColumn(ownerTile);
            int ownerRow = grid.GetRow(ownerTile);

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;
                int allyTile = grid.GetNearestTile(allMechs[i].transform.position);
                int allyCol = grid.GetColumn(allyTile);
                int allyRow = grid.GetRow(allyTile);

                // "Behind" Colossus = same column, further from enemy (lower row for Player, higher for Enemy)
                bool isBehind = allyCol == ownerCol &&
                    ((owner.team == Team.Player && allyRow < ownerRow) ||
                     (owner.team == Team.Enemy && allyRow > ownerRow));

                if (isBehind)
                {
                    // Small shield representing 25% damage reduction
                    float shieldAmount = allMechs[i].maxHP * 0.05f;
                    allMechs[i].statusHandler.ApplyEffect(new StatusEffect(
                        StatusEffectType.Shield, 2.5f, shieldAmount, owner, owner.mechId + 500));
                }
            }
        }

        private void ApplyAoEShield()
        {
            float radius = data.value2;
            float shieldAmount = owner.maxHP * data.value1;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team) continue;
                float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                if (dist <= radius)
                {
                    allMechs[i].statusHandler.ApplyEffect(new StatusEffect(
                        StatusEffectType.Shield, 5f, shieldAmount, owner, owner.mechId));
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
