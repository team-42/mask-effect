using System.Collections.Generic;

namespace MaskEffect
{
    public class CloakAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private float cooldownTimer;
        private bool hasDamageBonus;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.cooldownTimer = data.cooldown;
        }

        public void Tick(float dt)
        {
            cooldownTimer -= dt;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = data.cooldown;
                // Become invisible for duration, next attack gets bonus damage
                owner.statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Invisible, data.duration, 0f, owner, owner.mechId));
                hasDamageBonus = true;
            }
        }

        public void OnAttackLanded(MechController target, int damage)
        {
            // Breaking cloak: remove invisible and deal bonus damage
            if (hasDamageBonus)
            {
                hasDamageBonus = false;
                owner.statusHandler.RemoveEffect(StatusEffectType.Invisible, owner);
                int bonusDmg = UnityEngine.Mathf.RoundToInt(damage * data.value1);
                target.TakeDamage(bonusDmg, owner);
            }
        }

        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
