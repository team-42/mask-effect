using System.Collections.Generic;

namespace MaskEffect
{
    public class MarkAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private HashSet<int> markedTargets = new HashSet<int>();

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.markedTargets.Clear();
        }

        public void Tick(float dt) { }

        public void OnAttackLanded(MechController target, int damage)
        {
            if (!markedTargets.Contains(target.mechId))
            {
                markedTargets.Add(target.mechId);
                target.statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Mark, data.duration, data.value1, owner, owner.mechId));
            }
        }

        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}