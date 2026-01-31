using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class MechSpawner : MonoBehaviour
    {
        [SerializeField] private ChassisData[] chassisOptions;
        [SerializeField] private Color playerTeamTint = new Color(0.3f, 0.5f, 1f);
        [SerializeField] private Color enemyTeamTint = new Color(1f, 0.3f, 0.3f);

        private IBattleGrid grid;

        public void Initialize(IBattleGrid grid)
        {
            this.grid = grid;
            if (chassisOptions == null || chassisOptions.Length == 0)
                chassisOptions = Resources.LoadAll<ChassisData>("Data/Chassis");
        }

        public (List<MechController> playerMechs, List<MechController> enemyMechs) SpawnRound()
        {
            MechIdProvider.Reset();

            int mechCount = Random.Range(5, 11);

            // Generate random chassis lineup
            ChassisData[] lineup = new ChassisData[mechCount];
            for (int i = 0; i < mechCount; i++)
            {
                lineup[i] = chassisOptions[Random.Range(0, chassisOptions.Length)];
            }

            // Spawn player side
            int[] playerTiles = grid.GetSpawnTiles(Team.Player);
            List<MechController> playerMechs = SpawnTeam(lineup, Team.Player, playerTiles);

            // Mirror: spawn enemy side with same lineup
            int[] enemyTiles = grid.GetSpawnTiles(Team.Enemy);
            List<MechController> enemyMechs = SpawnTeam(lineup, Team.Enemy, enemyTiles);

            return (playerMechs, enemyMechs);
        }

        private List<MechController> SpawnTeam(ChassisData[] lineup, Team team, int[] availableTiles)
        {
            List<MechController> mechs = new List<MechController>();

            // Shuffle available tiles
            ShuffleTiles(availableTiles);

            int count = Mathf.Min(lineup.Length, availableTiles.Length);
            for (int i = 0; i < count; i++)
            {
                int tileIndex = availableTiles[i];
                Vector3 pos = grid.GetWorldPosition(tileIndex);
                MechController mech = CreateMech(lineup[i], team, pos, MechIdProvider.GetNextId());
                grid.SetTileOccupant(tileIndex, mech);
                mechs.Add(mech);
            }

            return mechs;
        }

        private MechController CreateMech(ChassisData chassis, Team team, Vector3 position, int id)
        {
            GameObject go = GameObject.CreatePrimitive(chassis.primitiveShape);
            go.name = $"{team}_{chassis.chassisName}_{id}";
            go.transform.position = position;
            go.transform.localScale = chassis.chassisScale;

            // Apply team tint to chassis color
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color tint = team == Team.Player ? playerTeamTint : enemyTeamTint;
                renderer.material.color = Color.Lerp(chassis.chassisColor, tint, 0.5f);
            }

            // Remove default collider (we handle collision via grid)
            var collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);

            // Add components
            var statusHandler = go.AddComponent<StatusEffectHandler>();
            var movement = go.AddComponent<MechMovement>();
            var controller = go.AddComponent<MechController>();

            controller.Initialize(chassis, team, id, grid);

            return controller;
        }

        private void ShuffleTiles(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public void ClearAllMechs(List<MechController> allMechs)
        {
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (allMechs[i] != null)
                    Destroy(allMechs[i].gameObject);
            }
        }
    }
}