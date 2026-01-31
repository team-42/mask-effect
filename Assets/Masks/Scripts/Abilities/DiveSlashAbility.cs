using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class DiveSlashAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private float cooldownTimer;
        private Vector3 originalPosition;

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

            if (cooldownTimer <= 0f && owner.currentTarget != null && owner.currentTarget.isAlive)
            {
                cooldownTimer = data.cooldown;
                ExecuteDive();
            }
        }

        private void ExecuteDive()
        {
            originalPosition = owner.transform.position;

            Vector3 targetPos = owner.currentTarget.transform.position;
            Vector3 offset = (owner.transform.position - targetPos).normalized * 0.5f;
            owner.movement.TeleportTo(targetPos + offset);

            int burstDamage = Mathf.RoundToInt(owner.attackDamage * data.value1);
            owner.currentTarget.TakeDamage(burstDamage, owner);

            owner.movement.TeleportTo(originalPosition);
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}