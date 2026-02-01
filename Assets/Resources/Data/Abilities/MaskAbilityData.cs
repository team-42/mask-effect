using UnityEngine;

namespace MaskEffect
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "MaskEffect/Mask Ability Data")]
    public class MaskAbilityData : ScriptableObject
    {
        public string abilityName;
        [TextArea]
        public string description;
        public ChassisType requiredChassis;
        public MaskType requiredMask;
        public float cooldown;
        public float duration;
        public float value1;
        public float value2;
        public string abilityClassId;
    }
}