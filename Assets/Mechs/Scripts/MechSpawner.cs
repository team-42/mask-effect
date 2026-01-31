using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class MechSpawner : MonoBehaviour
    {
        [SerializeField] private ChassisData[] chassisOptions;
        [SerializeField] private Color playerTeamColor = new Color(0.2f, 0.4f, 1f);
        [SerializeField] private Color enemyTeamColor = new Color(1f, 0.5f, 0.1f);

        private IBattleGrid grid;

        // Tag for finding top-half child renderers
        public const string TOP_HALF_NAME = "TopHalf";

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

            // Generate random chassis lineup (shared by both sides)
            ChassisData[] lineup = new ChassisData[mechCount];
            for (int i = 0; i < mechCount; i++)
            {
                lineup[i] = chassisOptions[Random.Range(0, chassisOptions.Length)];
            }

            // Pick random positions on the player side
            int[] playerTiles = grid.GetSpawnTiles(Team.Player);
            ShuffleTiles(playerTiles);
            int count = Mathf.Min(lineup.Length, playerTiles.Length);

            List<MechController> playerMechs = new List<MechController>();
            List<MechController> enemyMechs = new List<MechController>();

            for (int i = 0; i < count; i++)
            {
                int playerTile = playerTiles[i];
                int enemyTile = grid.GetMirroredTile(playerTile);

                // Spawn player mech
                Vector3 playerPos = grid.GetTileWorldPosition(playerTile);
                MechController playerMech = CreateMech(lineup[i], Team.Player, playerPos, MechIdProvider.GetNextId());
                playerMech.transform.rotation = Quaternion.LookRotation(Vector3.right);
                grid.SetTileOccupant(playerTile, playerMech);
                playerMechs.Add(playerMech);

                // Spawn mirrored enemy mech (same chassis, mirrored tile)
                Vector3 enemyPos = grid.GetTileWorldPosition(enemyTile);
                MechController enemyMech = CreateMech(lineup[i], Team.Enemy, enemyPos, MechIdProvider.GetNextId());
                enemyMech.transform.rotation = Quaternion.LookRotation(Vector3.left);
                grid.SetTileOccupant(enemyTile, enemyMech);
                enemyMechs.Add(enemyMech);
            }

            return (playerMechs, enemyMechs);
        }

        private MechController CreateMech(ChassisData chassis, Team team, Vector3 position, int id)
        {
            // Root object (empty parent)
            GameObject go = new GameObject($"{team}_{chassis.chassisName}_{id}");
            go.transform.position = position;

            Color teamColor = team == Team.Player ? playerTeamColor : enemyTeamColor;
            Vector3 scale = chassis.chassisScale;
            float halfY = scale.y * 0.5f;

            // Bottom half — team color
            GameObject bottom = GameObject.CreatePrimitive(chassis.primitiveShape);
            bottom.name = "BottomHalf";
            bottom.transform.SetParent(go.transform, false);
            bottom.transform.localScale = new Vector3(scale.x, halfY, scale.z);
            bottom.transform.localPosition = new Vector3(0f, halfY * 0.5f, 0f);
            SetRendererColor(bottom, teamColor);
            RemoveCollider(bottom);

            // Top half — team color (unmasked), changed by EquipMask
            GameObject top = GameObject.CreatePrimitive(chassis.primitiveShape);
            top.name = TOP_HALF_NAME;
            top.transform.SetParent(go.transform, false);
            top.transform.localScale = new Vector3(scale.x, halfY, scale.z);
            top.transform.localPosition = new Vector3(0f, halfY * 1.5f, 0f);
            SetRendererColor(top, teamColor);
            RemoveCollider(top);

            // Add interaction collider to root for raycasting
            BoxCollider interactionCollider = go.AddComponent<BoxCollider>();
            interactionCollider.center = new Vector3(0f, scale.y * 0.5f, 0f);
            interactionCollider.size = scale;

            // Set mech layer for raycast filtering
            int mechLayer = LayerMask.NameToLayer("Mech");
            if (mechLayer >= 0)
                go.layer = mechLayer;

            // Add components to root
            var statusHandler = go.AddComponent<StatusEffectHandler>();
            var movement = go.AddComponent<MechMovement>();
            var controller = go.AddComponent<MechController>();

            controller.Initialize(chassis, team, id, grid);

            return controller;
        }

        private void SetRendererColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }

        private void RemoveCollider(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
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
