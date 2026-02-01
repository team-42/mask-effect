using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class TitanPresenceAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.allMechs = allMechs;
        }

        public void Tick(float dt)
        {
            // Passive: allies within radius regen 2% max HP per second
            float radius = data.value2;
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;
                float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                if (dist <= radius)
                {
                    int healAmount = Mathf.RoundToInt(allMechs[i].maxHP * data.value1 * dt);
                    if (healAmount > 0)
                    {
                        allMechs[i].currentHP = Mathf.Min(allMechs[i].currentHP + healAmount, allMechs[i].maxHP);
                    }
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
