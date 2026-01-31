using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class HitAndRunAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private int attackCount;
        private float speedBuffTimer;
        private float originalAttackInterval;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.attackCount = 0;
            this.speedBuffTimer = 0f;
            this.originalAttackInterval = owner.attackInterval;
        }

        public void Tick(float dt)
        {
            if (speedBuffTimer > 0f)
            {
                speedBuffTimer -= dt;
                if (speedBuffTimer <= 0f)
                {
                    owner.attackInterval = originalAttackInterval;
                }
            }
        }

        public void OnAttackLanded(MechController target, int damage)
        {
            attackCount++;
            owner.movement.StepBack();

            if (attackCount % 3 == 0)
            {
                speedBuffTimer = data.duration;
                owner.attackInterval = originalAttackInterval * (1f - data.value1);
            }
        }

        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }

        public void Cleanup()
        {
            owner.attackInterval = originalAttackInterval;
        }
    }
}