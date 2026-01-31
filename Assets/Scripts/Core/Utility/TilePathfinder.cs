using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public static class TilePathfinder
    {
        public static List<int> FindPath(IBattleGrid grid, int startTile, int targetTile, List<MechController> allMechs)
        {
            if (grid == null || startTile == -1 || targetTile == -1) return null;

            // A* algorithm implementation
            // Open list: tiles to evaluate
            // Closed list: tiles already evaluated
            // gScore: cost from start to current
            // hScore: heuristic (estimated cost from current to target)
            // fScore: gScore + hScore
            // cameFrom: reconstruct path

            Dictionary<int, int> cameFrom = new Dictionary<int, int>();
            Dictionary<int, float> gScore = new Dictionary<int, float>();
            Dictionary<int, float> fScore = new Dictionary<int, float>();
            PriorityQueue<int> openSet = new PriorityQueue<int>();

            gScore[startTile] = 0;
            fScore[startTile] = Heuristic(grid, startTile, targetTile);
            openSet.Enqueue(startTile, fScore[startTile]);

            while (openSet.Count > 0)
            {
                int current = openSet.Dequeue();

                if (current == targetTile)
                {
                    return ReconstructPath(cameFrom, current);
                }

                foreach (int neighbor in grid.GetNeighbors(current))
                {
                    // Check if neighbor is walkable (not occupied by another mech)
                    if (grid.IsTileOccupied(neighbor) && neighbor != targetTile)
                    {
                        continue; // Cannot path through occupied tile
                    }

                    float tentative_gScore = gScore.GetValueOrDefault(current, float.MaxValue) + grid.GetDistanceBetweenTiles(current, neighbor);

                    if (tentative_gScore < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentative_gScore;
                        fScore[neighbor] = tentative_gScore + Heuristic(grid, neighbor, targetTile);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return null; // No path found
        }

        private static float Heuristic(IBattleGrid grid, int fromTile, int toTile)
        {
            // Manhattan distance or Euclidean distance
            Vector3 fromPos = grid.GetTileWorldPosition(fromTile);
            Vector3 toPos = grid.GetTileWorldPosition(toTile);
            return Vector3.Distance(fromPos, toPos); // Euclidean distance
        }

        private static List<int> ReconstructPath(Dictionary<int, int> cameFrom, int current)
        {
            List<int> path = new List<int>();
            while (cameFrom.ContainsKey(current))
            {
                path.Insert(0, current);
                current = cameFrom[current];
            }
            path.Insert(0, current); // Add the start node
            return path;
        }
    }

    // Simple Priority Queue for A*
    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new List<(T, float)>();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
            elements.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public T Dequeue()
        {
            T bestItem = elements[0].item;
            elements.RemoveAt(0);
            return bestItem;
        }
    }
}
