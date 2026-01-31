using UnityEditor;
using UnityEngine;
using Mirror;

public class NetworkManagerHUDAdder : EditorWindow
{
    [MenuItem("Tools/Mirror/Add NetworkManagerHUD")]
    public static void AddNetworkManagerHUD()
    {
        GameObject networkManagerGameObject = GameObject.Find("NetworkManager");

        if (networkManagerGameObject != null)
        {
            if (networkManagerGameObject.GetComponent<NetworkManagerHUD>() == null)
            {
                networkManagerGameObject.AddComponent<NetworkManagerHUD>();
                Debug.Log("NetworkManagerHUD component added to NetworkManager.");
            }
            else
            {
                Debug.Log("NetworkManagerHUD already present on NetworkManager.");
            }
            Selection.activeGameObject = networkManagerGameObject;
        }
        else
        {
            Debug.LogError("NetworkManager GameObject not found in the scene. Please create it first using 'Tools/Mirror/Create NetworkManager'.");
        }
    }
}
