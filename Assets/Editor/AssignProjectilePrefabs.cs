using UnityEditor;
using UnityEngine;
using MaskEffect;

public class AssignProjectilePrefabs : EditorWindow
{
    [MenuItem("MaskEffect/Assign Projectile Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<AssignProjectilePrefabs>("Assign Projectile Prefabs");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Assign Prefabs to ProjectileData"))
        {
            AssignPrefabs();
        }
    }

    private static void AssignPrefabs()
    {
        Debug.Log("Attempting to assign projectile prefabs to ProjectileData assets...");

        // Scout
        AssignSinglePrefab("ScoutProjectileData", "Missile1Prefab");
        // Jet
        AssignSinglePrefab("JetProjectileData", "Missile2Prefab");
        // Tank
        AssignSinglePrefab("TankProjectileData", "Missile3Prefab");
        // Sniper
        AssignSinglePrefab("SniperProjectileData", "Missile4Prefab");
        // Colossus
        AssignSinglePrefab("ColossusProjectileData", "Missile5Prefab");
        // Missile6
        AssignSinglePrefab("Missile6ProjectileData", "Missile6Prefab");
        // Missile7
        AssignSinglePrefab("Missile7ProjectileData", "Missile7Prefab");
        // Missile8
        AssignSinglePrefab("Missile8ProjectileData", "Missile8Prefab");
        // Missile9
        AssignSinglePrefab("Missile9ProjectileData", "Missile9Prefab");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Finished assigning projectile prefabs.");
    }

    private static void AssignSinglePrefab(string projectileDataName, string prefabName)
    {
        ProjectileData projectileData = AssetDatabase.LoadAssetAtPath<ProjectileData>($"Assets/Resources/Data/Projectiles/{projectileDataName}.asset");
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{prefabName}.prefab");

        if (projectileData != null && prefab != null)
        {
            projectileData.ProjectilePrefab = prefab;
            EditorUtility.SetDirty(projectileData);
            Debug.Log($"Assigned {prefabName}.prefab to {projectileDataName}.asset");
        }
        else
        {
            Debug.LogError($"Failed to assign: ProjectileData '{projectileDataName}.asset' or Prefab '{prefabName}.prefab' not found.");
        }
    }
}
