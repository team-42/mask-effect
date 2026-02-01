using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class OverwhelmingForceAbility : IMaskAbility
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

            // Every 4th attack: stun both targets for 1s
            if (attackCount % (int)data.value1 == 0)
            {
                target.statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Stun, data.duration, 0f, owner, owner.mechId));
            }

            // +15% bonus damage to stunned targets
            if (target.statusHandler.IsStunned())
            {
                int bonusDmg = Mathf.RoundToInt(damage * 0.15f);
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
