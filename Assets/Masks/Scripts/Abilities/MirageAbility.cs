using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class MirageAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private List<MechController> allMechs;
        private float cooldownTimer;

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
            cooldownTimer -= dt;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = data.cooldown;
                // Simplified Mirage: Taunt 2 nearest enemies for duration (draws aggro like an illusion)
                ApplyTauntToNearestEnemies();
            }
        }

        private void ApplyTauntToNearestEnemies()
        {
            List<(MechController mech, float dist)> enemies = new List<(MechController, float)>();

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team == owner.team) continue;
                float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                enemies.Add((allMechs[i], dist));
            }

            enemies.Sort((a, b) => a.dist.CompareTo(b.dist));

            int tauntCount = Mathf.Min(2, enemies.Count);
            for (int i = 0; i < tauntCount; i++)
            {
                enemies[i].mech.statusHandler.ApplyEffect(new StatusEffect(
                    StatusEffectType.Taunt, data.duration, 0f, owner, owner.mechId));
            }
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnTakeDamage(MechController attacker, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}
