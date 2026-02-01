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

        [Header("Damage Modifiers")]
        public DamageType damageTypeOverride = DamageType.Physical; // Mask can change damage type
        public float damageMultiplier = 1f; // e.g., 1.2 for 20% more damage

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
