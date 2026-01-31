using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class ExecuteChainAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;
        private bool hasDamageBonus;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.allMechs = allMechs;
            this.hasDamageBonus = false;
        }

        public void Tick(float dt) { }

        public void OnAttackLanded(MechController target, int damage)
        {
            if (hasDamageBonus)
            {
                int bonusDmg = Mathf.RoundToInt(damage * data.value1);
                target.TakeDamage(bonusDmg, owner);
                hasDamageBonus = false;
            }
        }

        public void OnKill(MechController killed)
        {
            hasDamageBonus = true;

            MechController nextTarget = null;
            int lowestHP = int.MaxValue;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team == owner.team || allMechs[i] == killed) continue;
                if (allMechs[i].currentHP < lowestHP)
                {
                    lowestHP = allMechs[i].currentHP;
                    nextTarget = allMechs[i];
                }
            }

            if (nextTarget != null)
            {
                Vector3 dashPos = nextTarget.transform.position +
                    (owner.transform.position - nextTarget.transform.position).normalized * 1f;
                owner.movement.TeleportTo(dashPos);
                owner.currentTarget = nextTarget;
            }
        }

        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}