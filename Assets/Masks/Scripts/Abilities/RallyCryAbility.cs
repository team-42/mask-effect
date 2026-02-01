using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class RallyCryAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;
        private float cooldownTimer;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.allMechs = allMechs;
            this.cooldownTimer = data.cooldown;
        }

        public void Tick(float dt)
        {
            cooldownTimer -= dt;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = data.cooldown;
                // Allies within radius get speed buff (negative Slow = speed boost)
                float radius = data.value2;
                for (int i = 0; i < allMechs.Count; i++)
                {
                    if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;
                    float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                    if (dist <= radius)
                    {
                        // Negative slow value = speed boost (GetSlowMultiplier returns 1 - value)
                        allMechs[i].statusHandler.ApplyEffect(new StatusEffect(
                            StatusEffectType.Slow, data.duration, -data.value1, owner, owner.mechId));
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
