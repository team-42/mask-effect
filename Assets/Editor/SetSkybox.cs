using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SetSkybox
{
    [MenuItem("MaskEffect/Apply Mars Environment")]
    public static void ApplyMarsEnvironment()
    {
        // Skybox
        var skyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpaceSkybox.mat");
        if (skyMat != null)
            RenderSettings.skybox = skyMat;

        // Mars ambient lighting - warm dusty tones
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.25f, 0.15f, 0.1f);
        RenderSettings.ambientEquatorColor = new Color(0.3f, 0.18f, 0.1f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.08f, 0.05f);
        RenderSettings.ambientIntensity = 1.0f;

        // Dusty Mars fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.45f, 0.25f, 0.15f);
        RenderSettings.fogDensity = 0.015f;

        // Place or update ground plane
        PlaceGroundPlane();

        DynamicGI.UpdateEnvironment();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Mars environment applied (skybox + lighting + fog + ground plane).");
    }

    static void PlaceGroundPlane()
    {
        var groundMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/MarsGround.mat");
        if (groundMat == null)
        {
            Debug.LogError("MarsGround.mat not found!");
            return;
        }

        // Find or create the ground plane
        const string groundName = "MarsGround";
        var existing = GameObject.Find(groundName);
        if (existing != null)
        {
            // Update material
            var r = existing.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = groundMat;
            Debug.Log("Updated existing MarsGround plane.");
            return;
        }

        // Create a large plane beneath the grid
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = groundName;
        ground.transform.position = new Vector3(0f, -0.15f, 0f);
        ground.transform.localScale = new Vector3(15f, 1f, 15f);
        ground.isStatic = true;

        var renderer = ground.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = groundMat;

        // Remove collider (not needed for visual ground)
        var col = ground.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        Debug.Log("Created MarsGround plane (150x150 units).");
    }
}
