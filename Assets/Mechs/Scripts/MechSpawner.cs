using System.Collections.Generic;
using UnityEngine;
using Mirror; // Add Mirror namespace

namespace MaskEffect
{
    public class MechSpawner : MonoBehaviour
    {
        [SerializeField] private ChassisData[] chassisOptions;
        public static Color PlayerTeamColor { get; private set; } = new Color(0.2f, 0.4f, 1f);
        public static Color EnemyTeamColor { get; private set; } = new Color(1f, 0.95f, 0.0f);

        [SerializeField] private Color _playerTeamColor = new Color(0.2f, 0.4f, 1f); // For Inspector assignment
        [SerializeField] private Color _enemyTeamColor = new Color(1f, 0.5f, 0.1f); // For Inspector assignment

        [Header("Spawn Config")]
        [SerializeField] private int minMechCount = 5;
        [SerializeField] private int maxMechCount = 10;

        [Header("Prefabs")]
        [SerializeField] private GameObject mechPrefab;
        [SerializeField] private GameObject deathEffectPrefab;

        private IBattleGrid grid;

        // Tag for finding top-half child renderers (legacy)
        public const string TOP_HALF_NAME = "TopHalf";
        // Tag for finding mask ground ring
        public const string MASK_RING_NAME = "MaskRing";

        public void Initialize(IBattleGrid grid)
        {
            this.grid = grid;
            if (chassisOptions == null || chassisOptions.Length == 0)
                chassisOptions = Resources.LoadAll<ChassisData>("Data/Chassis");

            // Set static colors from inspector fields
            PlayerTeamColor = _playerTeamColor;
            EnemyTeamColor = _enemyTeamColor;
        }

        public (List<MechController> playerMechs, List<MechController> enemyMechs) SpawnRound()
        {
            MechIdProvider.Reset();

            int mechCount = Random.Range(minMechCount, maxMechCount + 1);

            // Generate random chassis lineup (shared by both sides)
            ChassisData[] lineup = new ChassisData[mechCount];
            for (int i = 0; i < mechCount; i++)
            {
                lineup[i] = chassisOptions[Random.Range(0, chassisOptions.Length)];
            }

            // Pick spawn positions on the player side, sorted for preference matching
            int[] playerTiles = grid.GetSpawnTiles(Team.Player);
            int count = Mathf.Min(lineup.Length, playerTiles.Length);

            // Assign tiles based on spawn preference:
            // Backline chassis get the lowest X tiles (farthest from enemy for Player)
            // FrontlineCenter chassis get the highest X tiles (closest to enemy)
            // Random chassis get shuffled remaining tiles
            AssignTilesByPreference(lineup, playerTiles, count);

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
            // Instantiate from prefab (has MechController, MechMovement, StatusEffectHandler, BoxCollider)
            GameObject go = Instantiate(mechPrefab, position, Quaternion.identity);
            go.name = $"{team}_{chassis.chassisName}_{id}";

            Color teamColor = team == Team.Player ? MechSpawner.PlayerTeamColor : MechSpawner.EnemyTeamColor;
            Vector3 scale = chassis.chassisScale;

            // Get or add BoxCollider (pre-attached on prefab)
            BoxCollider interactionCollider = go.GetComponent<BoxCollider>();
            if (interactionCollider == null)
                interactionCollider = go.AddComponent<BoxCollider>();

            // Try to load 3D model from Resources/Models/ by chassis name
            GameObject modelPrefab = Resources.Load<GameObject>("Models/" + chassis.chassisName);

            if (modelPrefab != null)
            {
                // --- 3D Model path ---
                GameObject body = Instantiate(modelPrefab, go.transform);
                body.name = "Body";
                body.transform.localScale = scale;
                body.transform.localEulerAngles = chassis.modelRotationOffset;

                // Elevate body for flying mechs
                if (chassis.canFly && chassis.hoverHeight > 0f)
                {
                    body.transform.localPosition = new Vector3(0f, chassis.hoverHeight, 0f);
                    var bob = body.AddComponent<HoverBob>();
                    bob.amplitude = 0.15f;
                    bob.frequency = 1.2f;
                }
                else
                {
                    body.transform.localPosition = Vector3.zero;
                }

                Renderer[] bodyRenderers = body.GetComponentsInChildren<Renderer>();
                foreach (var rend in bodyRenderers)
                    rend.material.color = teamColor;

                Collider[] modelColliders = body.GetComponentsInChildren<Collider>();
                foreach (var col in modelColliders)
                    Destroy(col);

                // Auto-compute collider from rendered model bounds
                if (bodyRenderers.Length > 0)
                {
                    Bounds bounds = bodyRenderers[0].bounds;
                    for (int r = 1; r < bodyRenderers.Length; r++)
                        bounds.Encapsulate(bodyRenderers[r].bounds);
                    interactionCollider.center = go.transform.InverseTransformPoint(bounds.center);
                    interactionCollider.size = bounds.size;
                }
                else
                {
                    interactionCollider.center = new Vector3(0f, 0.4f, 0f);
                    interactionCollider.size = new Vector3(0.6f, 0.8f, 0.6f);
                }
            }
            else
            {
                // --- Primitive fallback path ---
                float halfY = scale.y * 0.5f;

                GameObject bottom = GameObject.CreatePrimitive(chassis.primitiveShape);
                bottom.name = "BottomHalf";
                bottom.transform.SetParent(go.transform, false);
                bottom.transform.localScale = new Vector3(scale.x, halfY, scale.z);
                bottom.transform.localPosition = new Vector3(0f, halfY * 0.5f, 0f);
                SetRendererColor(bottom, teamColor);
                RemoveCollider(bottom);

                GameObject top = GameObject.CreatePrimitive(chassis.primitiveShape);
                top.name = TOP_HALF_NAME;
                top.transform.SetParent(go.transform, false);
                top.transform.localScale = new Vector3(scale.x, halfY, scale.z);
                top.transform.localPosition = new Vector3(0f, halfY * 1.5f, 0f);
                SetRendererColor(top, teamColor);
                RemoveCollider(top);

                interactionCollider.center = new Vector3(0f, scale.y * 0.5f, 0f);
                interactionCollider.size = scale;
            }

            // Enforce mech layer
            int mechLayer = LayerMask.NameToLayer("Mech");
            if (mechLayer >= 0)
                go.layer = mechLayer;

            // Get pre-attached components from prefab (or add if not using prefab)
            var controller = go.GetComponent<MechController>();
            if (controller == null)
            {
                go.AddComponent<StatusEffectHandler>();
                go.AddComponent<MechMovement>();
                controller = go.AddComponent<MechController>();
            }

            controller.Initialize(chassis, team, id, grid);

            // Ensure death VFX is assigned (fallback if prefab reference was lost)
            if (controller.deathEffectPrefab == null && deathEffectPrefab != null)
                controller.deathEffectPrefab = deathEffectPrefab;

            // Set the chassisDataPath SyncVar on the server (or locally in singleplayer)
            if (NetworkHelper.IsServerOrOffline)
            {
                controller.chassisDataPath = $"Data/Chassis/{chassis.name}";
            }

            // Network spawn AFTER Initialize so clients receive correct initial SyncVars
            NetworkHelper.SpawnOrIgnore(go);

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

        private void AssignTilesByPreference(ChassisData[] lineup, int[] tiles, int count)
        {
            // Sort tiles by X ascending (low X = backline for Player, high X = frontline)
            System.Array.Sort(tiles, (a, b) =>
                grid.GetTileWorldPosition(a).x.CompareTo(grid.GetTileWorldPosition(b).x));

            // Separate lineup indices by preference
            List<int> backlineIndices = new List<int>();
            List<int> frontlineIndices = new List<int>();
            List<int> randomIndices = new List<int>();

            for (int i = 0; i < count; i++)
            {
                switch (lineup[i].spawnPreference)
                {
                    case SpawnPreference.Backline:
                        backlineIndices.Add(i);
                        break;
                    case SpawnPreference.FrontlineCenter:
                        frontlineIndices.Add(i);
                        break;
                    default:
                        randomIndices.Add(i);
                        break;
                }
            }

            // Assign backline chassis to lowest X tiles (start of sorted array)
            // Assign frontline chassis to highest X tiles (end of sorted array)
            // Shuffle remaining tiles for random chassis
            int[] assignedTileIndex = new int[count];
            bool[] tileUsed = new bool[tiles.Length];

            // Backline: assign from start (lowest X)
            int tilePtr = 0;
            foreach (int li in backlineIndices)
            {
                while (tilePtr < tiles.Length && tileUsed[tilePtr]) tilePtr++;
                if (tilePtr < tiles.Length)
                {
                    assignedTileIndex[li] = tilePtr;
                    tileUsed[tilePtr] = true;
                    tilePtr++;
                }
            }

            // Frontline: assign from end (highest X)
            tilePtr = tiles.Length - 1;
            foreach (int li in frontlineIndices)
            {
                while (tilePtr >= 0 && tileUsed[tilePtr]) tilePtr--;
                if (tilePtr >= 0)
                {
                    assignedTileIndex[li] = tilePtr;
                    tileUsed[tilePtr] = true;
                    tilePtr--;
                }
            }

            // Random: collect remaining unused tiles, shuffle, assign
            List<int> remainingTileIndices = new List<int>();
            for (int i = 0; i < tiles.Length; i++)
            {
                if (!tileUsed[i]) remainingTileIndices.Add(i);
            }
            ShuffleList(remainingTileIndices);

            int rPtr = 0;
            foreach (int li in randomIndices)
            {
                if (rPtr < remainingTileIndices.Count)
                {
                    assignedTileIndex[li] = remainingTileIndices[rPtr];
                    rPtr++;
                }
            }

            // Reorder tiles array to match lineup order
            int[] orderedTiles = new int[count];
            for (int i = 0; i < count; i++)
            {
                orderedTiles[i] = tiles[assignedTileIndex[i]];
            }
            System.Array.Copy(orderedTiles, tiles, count);
        }

        private void ShuffleTiles(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        private void ShuffleList(List<int> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public void ClearAllMechs(List<MechController> allMechs)
        {
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (allMechs[i] != null && NetworkHelper.IsServerOrOffline)
                {
                    NetworkHelper.SmartDestroy(allMechs[i].gameObject);
                }
            }
        }
    }
}
