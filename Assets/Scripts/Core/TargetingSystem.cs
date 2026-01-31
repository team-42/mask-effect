using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public static class TargetingSystem
    {
        public static MechController GetTarget(MechController seeker,
            List<MechController> allMechs, IBattleGrid grid)
        {
            if (allMechs == null) return null;

            // Taunt override
            if (seeker.statusHandler != null && seeker.statusHandler.IsTaunted(out MechController taunter))
            {
                if (taunter.isAlive) return taunter;
            }

            // Get enemies
            List<MechController> enemies = new List<MechController>();
            List<MechController> allies = new List<MechController>();
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive) continue;
                if (allMechs[i] == seeker) continue;
                if (allMechs[i].team != seeker.team)
                    enemies.Add(allMechs[i]);
                else
                    allies.Add(allMechs[i]);
            }

            if (enemies.Count == 0 && seeker.targetingMode != TargetingMode.LowestHPAlly)
                return null;

            return seeker.targetingMode switch
            {
                TargetingMode.Nearest => FindNearest(seeker, enemies),
                TargetingMode.LowestHP => FindLowestHP(enemies),
                TargetingMode.HighestThreat => FindHighestThreat(enemies),
                TargetingMode.BacklinePriority => FindBacklinePriority(seeker, enemies, grid),
                TargetingMode.FarthestEnemy => FindFarthestEnemy(seeker, enemies),
                TargetingMode.LowestHPAlly => FindLowestHPAlly(allies),
                _ => FindNearest(seeker, enemies)
            };
        }

        private static MechController FindNearest(MechController seeker, List<MechController> candidates)
        {
            MechController best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                float dist = Vector3.Distance(seeker.transform.position, candidates[i].transform.position);
                if (dist < bestDist || (Mathf.Approximately(dist, bestDist) && IsBetterTieBreak(candidates[i], best)))
                {
                    bestDist = dist;
                    best = candidates[i];
                }
            }
            return best;
        }

        private static MechController FindLowestHP(List<MechController> candidates)
        {
            MechController best = null;
            int bestHP = int.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].currentHP < bestHP ||
                    (candidates[i].currentHP == bestHP && IsBetterTieBreak(candidates[i], best)))
                {
                    bestHP = candidates[i].currentHP;
                    best = candidates[i];
                }
            }
            return best;
        }

        private static MechController FindHighestThreat(List<MechController> candidates)
        {
            MechController best = null;
            float bestDPS = -1f;

            for (int i = 0; i < candidates.Count; i++)
            {
                float dps = candidates[i].GetDPS();
                if (dps > bestDPS || (Mathf.Approximately(dps, bestDPS) && IsBetterTieBreak(candidates[i], best)))
                {
                    bestDPS = dps;
                    best = candidates[i];
                }
            }
            return best;
        }

        private static MechController FindBacklinePriority(MechController seeker,
            List<MechController> candidates, IBattleGrid grid)
        {
            // Prefer backline targets, fall back to nearest
            MechController backlineTarget = null;
            float backlineDist = float.MaxValue;
            MechController nearestTarget = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < candidates.Count; i++)
            {
                float dist = Vector3.Distance(seeker.transform.position, candidates[i].transform.position);
                int tile = grid.GetNearestTile(candidates[i].transform.position);

                if (grid.IsBackline(tile, candidates[i].team))
                {
                    if (dist < backlineDist || (Mathf.Approximately(dist, backlineDist)
                        && IsBetterTieBreak(candidates[i], backlineTarget)))
                    {
                        backlineDist = dist;
                        backlineTarget = candidates[i];
                    }
                }

                if (dist < nearestDist || (Mathf.Approximately(dist, nearestDist)
                    && IsBetterTieBreak(candidates[i], nearestTarget)))
                {
                    nearestDist = dist;
                    nearestTarget = candidates[i];
                }
            }

            return backlineTarget != null ? backlineTarget : nearestTarget;
        }

        private static MechController FindFarthestEnemy(MechController seeker,
            List<MechController> candidates)
        {
            MechController best = null;
            float bestDist = -1f;

            for (int i = 0; i < candidates.Count; i++)
            {
                float dist = Vector3.Distance(seeker.transform.position, candidates[i].transform.position);
                if (dist > bestDist || (Mathf.Approximately(dist, bestDist) && IsBetterTieBreak(candidates[i], best)))
                {
                    bestDist = dist;
                    best = candidates[i];
                }
            }
            return best;
        }

        private static MechController FindLowestHPAlly(List<MechController> allies)
        {
            MechController best = null;
            int bestHP = int.MaxValue;

            for (int i = 0; i < allies.Count; i++)
            {
                if (allies[i].currentHP < bestHP ||
                    (allies[i].currentHP == bestHP && IsBetterTieBreak(allies[i], best)))
                {
                    bestHP = allies[i].currentHP;
                    best = allies[i];
                }
            }
            return best;
        }

        private static bool IsBetterTieBreak(MechController candidate, MechController current)
        {
            if (current == null) return true;
            // Lower HP first, then lower mechId
            if (candidate.currentHP != current.currentHP)
                return candidate.currentHP < current.currentHP;
            return candidate.mechId < current.mechId;
        }
    }
}