using System.Collections.Generic;

namespace MaskEffect
{
    public class DisplacementFieldAbility : IMaskAbility
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
        public void OnAttackLanded(MechController target, int damage) { }
        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }

        public void OnBattleStart()
        {
            // Passive: 25% miss chance applied as permanent effect (very long duration)
            owner.statusHandler.ApplyEffect(new StatusEffect(
                StatusEffectType.MissChance, 9999f, data.value1, owner, owner.mechId));
        }

        public void Cleanup() { }
    }
}
