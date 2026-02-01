using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class CrushingGripAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;
        private float cooldownTimer;
        private MechController grippedTarget;
        private float gripTimer;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.allMechs = allMechs;
            this.cooldownTimer = data.cooldown;
        }

        public void Tick(float dt)
        {
            // Handle active grip
            if (grippedTarget != null)
            {
                gripTimer -= dt;
                if (gripTimer <= 0f || !grippedTarget.isAlive)
                {
                    // End grip
                    if (grippedTarget != null && grippedTarget.isAlive)
                    {
                        grippedTarget.statusHandler.RemoveEffect(StatusEffectType.Root, owner);
                    }
                    grippedTarget = null;
                }
                return;
            }

            cooldownTimer -= dt;
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = data.cooldown;
                ExecuteGrip();
            }
        }

        private void ExecuteGrip()
        {
            // Grab nearest enemy
            MechController nearest = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team == owner.team) continue;
                float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                if (dist <= owner.range + 1f && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = allMechs[i];
                }
            }

            if (nearest == null) return;

            grippedTarget = nearest;
            gripTimer = data.duration;

            // Root the gripped target
            grippedTarget.statusHandler.ApplyEffect(new StatusEffect(
                StatusEffectType.Root, data.duration, 0f, owner, owner.mechId));

            // Apply mark for bonus damage from Colossus
            grippedTarget.statusHandler.ApplyEffect(new StatusEffect(
                StatusEffectType.Mark, data.duration, 0.5f, owner, owner.mechId + 1000));
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
