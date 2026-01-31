using UnityEngine;
using UnityEditor;
using Mirror;

namespace MaskEffect
{
    public static class PrefabGenerator
    {
        private const string PrefabFolder = "Assets/Prefabs";

        [MenuItem("MaskEffect/Generate All Prefabs")]
        public static void GenerateAllPrefabs()
        {
            EnsureFolder(PrefabFolder);

            CreateMechPrefab();
            CreateTilePrefab();
            CreateMaskDragProxyPrefab();
            CreateMaskIndicatorPrefab();
            AddNetworkIdentityToProjectile();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PrefabGenerator] All prefabs generated/updated.");
        }

        [MenuItem("MaskEffect/Wire Prefab References In Scene")]
        public static void WirePrefabReferencesInScene()
        {
            var mechPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/MechPrefab.prefab");
            var tilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/TilePrefab.prefab");
            var maskDragProxy = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/MaskDragProxy.prefab");
            var maskIndicator = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/MaskIndicator.prefab");

            // Wire MechSpawner
            var spawner = Object.FindFirstObjectByType<MechSpawner>();
            if (spawner != null)
            {
                var so = new SerializedObject(spawner);
                so.FindProperty("mechPrefab").objectReferenceValue = mechPrefab;
                so.FindProperty("maskIndicatorPrefab").objectReferenceValue = maskIndicator;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(spawner);
                Debug.Log("[PrefabGenerator] Wired MechSpawner prefab references.");
            }
            else
            {
                Debug.LogWarning("[PrefabGenerator] MechSpawner not found in scene.");
            }

            // Wire SimpleFlatGrid
            var grid = Object.FindFirstObjectByType<SimpleFlatGrid>();
            if (grid != null)
            {
                var so = new SerializedObject(grid);
                so.FindProperty("tilePrefab").objectReferenceValue = tilePrefab;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(grid);
                Debug.Log("[PrefabGenerator] Wired SimpleFlatGrid prefab references.");
            }
            else
            {
                Debug.LogWarning("[PrefabGenerator] SimpleFlatGrid not found in scene.");
            }

            // Wire MaskAssignmentManager
            var maskMgr = Object.FindFirstObjectByType<MaskAssignmentManager>();
            if (maskMgr != null)
            {
                var so = new SerializedObject(maskMgr);
                so.FindProperty("maskDragProxyPrefab").objectReferenceValue = maskDragProxy;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(maskMgr);
                Debug.Log("[PrefabGenerator] Wired MaskAssignmentManager prefab references.");
            }
            else
            {
                Debug.LogWarning("[PrefabGenerator] MaskAssignmentManager not found in scene.");
            }

            Debug.Log("[PrefabGenerator] All prefab references wired in current scene. Save the scene to persist.");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static void CreateMechPrefab()
        {
            string path = $"{PrefabFolder}/MechPrefab.prefab";

            GameObject go = new GameObject("MechPrefab");

            // Core gameplay components (pre-attached so we don't need AddComponent at runtime)
            go.AddComponent<StatusEffectHandler>();
            go.AddComponent<MechMovement>();
            go.AddComponent<MechController>();
            go.AddComponent<BoxCollider>();

            // Network identity for Mirror spawning
            go.AddComponent<NetworkIdentity>();

            // Layer
            int mechLayer = LayerMask.NameToLayer("Mech");
            if (mechLayer >= 0) go.layer = mechLayer;

            SavePrefab(go, path);
        }

        private static void CreateTilePrefab()
        {
            string path = $"{PrefabFolder}/TilePrefab.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "TilePrefab";
            // Default scale; overridden at runtime based on tileSize
            go.transform.localScale = new Vector3(0.95f, 0.1f, 0.95f);

            SavePrefab(go, path);
        }

        private static void CreateMaskDragProxyPrefab()
        {
            string path = $"{PrefabFolder}/MaskDragProxy.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "MaskDragProxy";
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Disable collider so it doesn't interfere with raycasts
            var col = go.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            SavePrefab(go, path);
        }

        private static void CreateMaskIndicatorPrefab()
        {
            string path = $"{PrefabFolder}/MaskIndicator.prefab";

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "MaskIndicator";
            // Default scale; overridden at runtime based on chassisData
            go.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);

            // Remove collider - indicator is visual only
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            SavePrefab(go, path);
        }

        private static void AddNetworkIdentityToProjectile()
        {
            string path = $"{PrefabFolder}/ProjectilePrefab.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing == null)
            {
                Debug.LogWarning($"[PrefabGenerator] ProjectilePrefab not found at {path}");
                return;
            }

            if (existing.GetComponent<NetworkIdentity>() != null)
            {
                Debug.Log("[PrefabGenerator] ProjectilePrefab already has NetworkIdentity.");
                return;
            }

            // Instantiate, modify, save back
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(existing);
            instance.AddComponent<NetworkIdentity>();
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            Debug.Log($"[PrefabGenerator] Added NetworkIdentity to {path}");
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            Debug.Log($"[PrefabGenerator] Created/Updated {path}");
        }
    }
}
