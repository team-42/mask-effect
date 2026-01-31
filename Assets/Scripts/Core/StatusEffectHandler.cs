using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class StatusEffectHandler : MonoBehaviour
    {
        private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();

        public void ApplyEffect(StatusEffect effect)
        {
            // Prevent duplicate stacking from same source
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == effect.type && activeEffects[i].stackId == effect.stackId
                    && activeEffects[i].source == effect.source && effect.stackId != 0)
                {
                    // Refresh duration
                    activeEffects[i].duration = effect.duration;
                    activeEffects[i].value = effect.value;
                    return;
                }
            }
            activeEffects.Add(effect);
        }

        public void RemoveEffect(StatusEffectType type, MechController source = null)
        {
            activeEffects.RemoveAll(e => e.type == type && (source == null || e.source == source));
        }

        public void RemoveAllEffects()
        {
            activeEffects.Clear();
        }

        public bool HasEffect(StatusEffectType type)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == type) return true;
            }
            return false;
        }

        public StatusEffect GetEffect(StatusEffectType type)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == type) return activeEffects[i];
            }
            return null;
        }

        public float GetShieldAmount()
        {
            float total = 0f;
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == StatusEffectType.Shield)
                    total += activeEffects[i].value;
            }
            return total;
        }

        public void DamageShield(float amount)
        {
            float remaining = amount;
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].type != StatusEffectType.Shield) continue;
                if (remaining <= 0f) break;

                if (activeEffects[i].value <= remaining)
                {
                    remaining -= activeEffects[i].value;
                    activeEffects.RemoveAt(i);
                }
                else
                {
                    activeEffects[i].value -= remaining;
                    remaining = 0f;
                }
            }
        }

        public bool IsRooted()
        {
            return HasEffect(StatusEffectType.Root);
        }

        public bool IsSlowed()
        {
            return HasEffect(StatusEffectType.Slow);
        }

        public bool IsTaunted(out MechController taunter)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].type == StatusEffectType.Taunt && activeEffects[i].source != null
                    && activeEffects[i].source.isAlive)
                {
                    taunter = activeEffects[i].source;
                    return true;
                }
            }
            taunter = null;
            return false;
        }

        public float GetMarkMultiplier()
        {
            if (HasEffect(StatusEffectType.Mark))
                return 1.15f;
            return 1f;
        }

        public float GetSlowMultiplier()
        {
            var slow = GetEffect(StatusEffectType.Slow);
            if (slow != null)
                return 1f - slow.value; // value is the slow percentage (0.3 = 30% slow)
            return 1f;
        }

        public void TickEffects(float dt)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].Tick(dt);
                if (activeEffects[i].IsExpired)
                {
                    activeEffects.RemoveAt(i);
                }
            }
        }
    }
}