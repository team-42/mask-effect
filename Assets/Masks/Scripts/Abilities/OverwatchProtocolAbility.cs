using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class OverwatchProtocolAbility : IMaskAbility
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
                ShieldLowestAlly();
            }
        }

        private void ShieldLowestAlly()
        {
            MechController lowestAlly = null;
            int lowestHP = int.MaxValue;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;
                if (allMechs[i].currentHP < lowestHP)
                {
                    lowestHP = allMechs[i].currentHP;
                    lowestAlly = allMechs[i];
                }
            }

            if (lowestAlly == null) return;

            // Shield ally for 15% of Sniper's attack damage
            float shieldAmount = owner.attackDamage * data.value1;
            lowestAlly.statusHandler.ApplyEffect(new StatusEffect(
                StatusEffectType.Shield, data.duration, shieldAmount, owner, owner.mechId));
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
