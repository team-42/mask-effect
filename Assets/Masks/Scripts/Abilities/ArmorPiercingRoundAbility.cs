using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class ArmorPiercingRoundAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private int attackCount;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.attackCount = 0;
        }

        public void Tick(float dt) { }

        public void OnAttackLanded(MechController target, int damage)
        {
            attackCount++;

            // Every 3rd shot ignores 50% armor + distance bonus
            if (attackCount % 3 == 0)
            {
                float dist = Vector3.Distance(owner.transform.position, target.transform.position);
                float distBonus = dist * 0.1f; // +10% damage per tile of distance
                int armorIgnored = Mathf.RoundToInt(target.armor * data.value1); // 50% armor ignore
                int bonusDmg = Mathf.RoundToInt(armorIgnored + damage * distBonus);
                if (bonusDmg > 0)
                    target.TakeDamage(bonusDmg, owner);
            }
        }

        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
