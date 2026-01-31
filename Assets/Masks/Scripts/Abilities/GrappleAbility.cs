using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class GrappleAbility : IMaskAbility
    {
        private MechController owner;
        private MaskAbilityData data;
        private IBattleGrid grid;
        private List<MechController> allMechs;
        private float cooldownTimer;

        public void Initialize(MechController owner, MaskAbilityData data, IBattleGrid grid,
            List<MechController> allMechs)
        {
            this.owner = owner;
            this.data = data;
            this.grid = grid;
            this.allMechs = allMechs;
            this.cooldownTimer = data.cooldown;
        }

        public void Tick(float dt)
        {
            cooldownTimer -= dt;

            if (cooldownTimer <= 0f)
            {
                cooldownTimer = data.cooldown;
                ExecuteGrapple();
            }
        }

        private void ExecuteGrapple()
        {
            MechController farthest = null;
            float maxDist = -1f;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive || allMechs[i].team == owner.team) continue;
                float dist = Vector3.Distance(owner.transform.position, allMechs[i].transform.position);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = allMechs[i];
                }
            }

            if (farthest == null) return;

            int ownerTile = grid.GetNearestTile(owner.transform.position);
            int[] adjacent = grid.GetAdjacentTiles(ownerTile);

            for (int i = 0; i < adjacent.Length; i++)
            {
                if (!grid.IsTileOccupied(adjacent[i]))
                {
                    int oldTile = grid.GetNearestTile(farthest.transform.position);
                    grid.ClearTile(oldTile);
                    farthest.transform.position = grid.GetWorldPosition(adjacent[i]);
                    grid.SetTileOccupant(adjacent[i], farthest);

                    farthest.statusHandler.ApplyEffect(new StatusEffect(
                        StatusEffectType.Root, data.duration, 0f, owner, owner.mechId));
                    break;
                }
            }
        }

        public void OnAttackLanded(MechController target, int damage) { }
        public void OnKill(MechController killed) { }
        public void OnBattleStart() { }
        public void Cleanup() { }
    }
}