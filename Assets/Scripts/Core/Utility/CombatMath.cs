using UnityEngine;

namespace MaskEffect
{
    public static class CombatMath
    {
        public static int CalculateArmorReduction(int rawDamage, int armor)
        {
            if (armor <= 0) return rawDamage;
            return Mathf.FloorToInt(rawDamage * 100f / (100f + armor));
        }

        public static bool RollEvasion(float evasion)
        {
            return Random.value < evasion;
        }

        /// <summary>
        /// Apply damage to a mech. Shield absorbs first, then HP.
        /// Returns actual damage dealt to HP.
        /// </summary>
        public static int ApplyDamage(int rawDamage, int armor, float markMultiplier,
            ref int currentHP, StatusEffectHandler statusHandler)
        {
            int damage = CalculateArmorReduction(rawDamage, armor);
            damage = Mathf.FloorToInt(damage * markMultiplier);
            damage = Mathf.Max(damage, 1);

            float shield = statusHandler.GetShieldAmount();
            if (shield > 0f)
            {
                if (shield >= damage)
                {
                    statusHandler.DamageShield(damage);
                    return 0;
                }
                else
                {
                    int remaining = damage - Mathf.FloorToInt(shield);
                    statusHandler.DamageShield(shield);
                    currentHP -= remaining;
                    return remaining;
                }
            }

            currentHP -= damage;
            return damage;
        }
    }
}