using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class ChallengeAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;
        private float shieldCooldown;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.allMechs = allMechs;
            this.shieldCooldown = 0f;
        }

        public void Tick(float dt)
        {
            shieldCooldown -= dt;
        }

        public void OnAttackLanded(MechController target, int damage)
        {
            if (shieldCooldown <= 0f)
            {
                shieldCooldown = data.cooldown;
                float shieldAmount = owner.maxHP * data.value1;
                owner.statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Shield, 5f, shieldAmount, owner, owner.mechId));
            }
        }

        public void OnKill(MechController killed) { }

        public void OnBattleStart()
        {
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team == owner.team) continue;
                float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                if (dist <= owner.range + 2f)
                {
                    allMechs[i].statusHandler.ApplyEffect(new StatusEffect(
                        StatusEffectType.Taunt, data.duration, 0f, owner, owner.mechId));
                }
            }
        }

        public void Cleanup() { }
    }
}