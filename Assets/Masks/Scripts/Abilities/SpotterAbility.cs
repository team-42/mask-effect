using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class SpotterAbility : IMaskAbility
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
                // Mark current target for 4s with 20% damage bonus
                if (owner.currentTarget != null && owner.currentTarget.isAlive)
                {
                    owner.currentTarget.statusHandler.ApplyEffect(new StatusEffect(
                        StatusEffectType.Mark, data.duration, data.value1, owner, owner.mechId));
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
