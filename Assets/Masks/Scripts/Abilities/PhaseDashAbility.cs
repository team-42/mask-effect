using System.Collections.Generic;

namespace MaskEffect
{
    public class PhaseDashAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
        }

        public void Tick(float dt) { }

        public void OnAttackLanded(MechController target, int damage)
        {
            // 40% chance to become untargetable for 1s after each attack
            if (UnityEngine.Random.value < data.value1)
            {
                owner.statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Untargetable, data.duration, 0f, owner, owner.mechId));
            }
        }

        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
