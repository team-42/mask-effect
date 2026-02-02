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
        /// Returns the new HP value after damage is applied.
        /// </summary>
        public static int ApplyDamage(int rawDamage, DamageType damageType, int armor, ResistanceType resistanceType, float resistanceValue, float markMultiplier,
            int currentHP, StatusEffectHandler statusHandler)
        {
            int damage = rawDamage;

            // Apply damage type vs resistance
            if (damageType != DamageType.True && resistanceType.ToString() == damageType.ToString())
            {
                damage = Mathf.FloorToInt(damage * (1f - resistanceValue));
            }

            damage = CalculateArmorReduction(damage, armor);
            damage = Mathf.FloorToInt(damage * markMultiplier);
            damage = Mathf.Max(damage, 1);

            int newHP = currentHP;

            float shield = statusHandler.GetShieldAmount();
            if (shield > 0f)
            {
                if (shield >= damage)
                {
                    statusHandler.DamageShield(damage);
                    return newHP;  // No HP damage, return unchanged
                }
                else
                {
                    int remaining = damage - Mathf.FloorToInt(shield);
                    statusHandler.DamageShield(shield);
                    newHP -= remaining;
                    return newHP;
                }
            }

            newHP -= damage;
            return newHP;
        }
    }
}
