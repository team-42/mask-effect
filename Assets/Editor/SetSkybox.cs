using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class SetSkybox
{
    [MenuItem("MaskEffect/Apply Space Skybox")]
    public static void ApplySpaceSkybox()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/SpaceSkybox.mat");
        if (mat == null)
        {
            Debug.LogError("SpaceSkybox.mat not found!");
            return;
        }

        RenderSettings.skybox = mat;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.04f, 0.04f, 0.08f);
        RenderSettings.ambientEquatorColor = new Color(0.06f, 0.03f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.02f, 0.02f, 0.03f);
        RenderSettings.ambientIntensity = 1.0f;

        // Enable fog for depth
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.02f, 0.01f, 0.04f);
        RenderSettings.fogDensity = 0.02f;

        DynamicGI.UpdateEnvironment();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Space Skybox applied + ambient lighting adjusted.");
    }
}
