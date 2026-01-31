using UnityEditor;
using UnityEngine;

public class CreateTileTexture : EditorWindow
{
    [MenuItem("Tools/Create/Tile Texture")]
    public static void ShowWindow()
    {
        GetWindow<CreateTileTexture>("Create Tile Texture");
    }

    private int textureWidth = 32;
    private int textureHeight = 32;
    private Color fillColor = Color.gray;
    private string assetPath = "Assets/Materials/TileTexture.png";

    void OnGUI()
    {
        GUILayout.Label("Create Tile Texture", EditorStyles.boldLabel);

        textureWidth = EditorGUILayout.IntField("Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Height", textureHeight);
        fillColor = EditorGUILayout.ColorField("Fill Color", fillColor);
        assetPath = EditorGUILayout.TextField("Asset Path", assetPath);

        if (GUILayout.Button("Create Texture"))
        {
            CreateAndSaveTexture();
        }
    }

    void CreateAndSaveTexture()
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = fillColor;
        }
        texture.SetPixels(pixels);
        texture.Apply();

        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(assetPath, bytes);
        AssetDatabase.Refresh();
        Debug.Log("Texture saved to: " + assetPath);
    }
}