using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class KillShotAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
        }

        public void Tick(float dt) { }

        public void OnAttackLanded(MechController target, int damage)
        {
            // Targets below 30% HP take +75% damage
            float hpPercent = (float)target.currentHP / target.maxHP;
            if (hpPercent < data.value2) // value2 = 0.3 (30% threshold)
            {
                int bonusDmg = Mathf.RoundToInt(damage * data.value1); // value1 = 0.75 (75% bonus)
                if (bonusDmg > 0)
                    target.TakeDamage(bonusDmg, owner);
            }
        }

        public void OnTakeDamage(MechController attacker, int damage) { }

        public void OnKill(MechController killed)
        {
            // On kill: next shot has no charge-up time
            owner.ResetCharge();
        }

        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
