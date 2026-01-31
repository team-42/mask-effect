using System.Collections.Generic;

namespace MaskEffect
{
    public interface IMaskAbility
    {
        void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs);
        void Tick(float dt);
        void OnAttackLanded(MechController target, int damage);
        void OnKill(MechController killed);
        void OnBattleStart();
        void Cleanup();
    }
}