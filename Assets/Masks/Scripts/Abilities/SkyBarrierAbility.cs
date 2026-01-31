using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class SkyBarrierAbility : IMaskAbility
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
                ExecuteBarrier();
            }
        }

        private void ExecuteBarrier()
        {
            List<MechController> allies = new List<MechController>();
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team != owner.team || allMechs[i] == owner) continue;
                allies.Add(allMechs[i]);
            }

            allies.Sort((a, b) => a.currentHP.CompareTo(b.currentHP));

            int shieldCount = Mathf.Min((int)data.value2, allies.Count);
            float shieldAmount = owner.maxHP * data.value1;

            for (int i = 0; i < shieldCount; i++)
            {
                allies[i].statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Shield, data.duration, shieldAmount, owner, owner.mechId));
            }
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}