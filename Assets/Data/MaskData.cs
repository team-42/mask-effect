using UnityEngine;

namespace MaskEffect
{
    [CreateAssetMenu(fileName = "NewMask", menuName = "MaskEffect/Mask Data")]
    public class MaskData : ScriptableObject
    {
        public string maskName;
        public MaskType maskType;

        [Header("Targeting Override")]
        public TargetingMode defaultTargetingMode = TargetingMode.Nearest;

        [Header("Stat Modifiers (additive)")]
        public int bonusHP;
        public int bonusArmor;
        public int bonusAttackDamage;
        public float bonusAttackInterval;
        public float bonusEvasion;

        [Header("Abilities (one per chassis type)")]
        public MaskAbilityData scoutAbility;
        public MaskAbilityData jetAbility;
        public MaskAbilityData tankAbility;

        [Header("Visuals")]
        public Color maskTint = Color.white;

        public MaskAbilityData GetAbilityForChassis(ChassisType chassis)
        {
            return chassis switch
            {
                ChassisType.Scout => scoutAbility,
                ChassisType.Jet => jetAbility,
                ChassisType.Tank => tankAbility,
                _ => null
            };
        }
    }
}