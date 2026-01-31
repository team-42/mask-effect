using UnityEditor;
using UnityEngine;
using Mirror;

public class NetworkManagerCreator : EditorWindow
{
    [MenuItem("Tools/Mirror/Create NetworkManager")]
    public static void CreateNetworkManager()
    {
        GameObject networkManagerGameObject = new GameObject("NetworkManager");
        networkManagerGameObject.AddComponent<NetworkManager>();
        Selection.activeGameObject = networkManagerGameObject;
        Debug.Log("NetworkManager GameObject created and NetworkManager component added.");
    }
}
