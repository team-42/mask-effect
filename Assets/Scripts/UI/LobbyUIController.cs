using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Mirror;

public class LobbyUIController : MonoBehaviour
{
    public string battleArenaSceneName = "BattleArenaScene";
    public string multiplayerSceneName = "BattleArenaMultiplayer";

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component not found on this GameObject.");
            return;
        }

        var root = uiDocument.rootVisualElement;

        var startGameButton = root.Q<Button>("startGameButton");
        if (startGameButton != null)
            startGameButton.clicked += LoadBattleArenaScene;

        var multiplayerButton = root.Q<Button>("multiplayerButton");
        if (multiplayerButton != null)
            multiplayerButton.clicked += LoadMultiplayerScene;
    }

    void OnDisable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        if (root == null) return;

        var startGameButton = root.Q<Button>("startGameButton");
        if (startGameButton != null)
            startGameButton.clicked -= LoadBattleArenaScene;

        var multiplayerButton = root.Q<Button>("multiplayerButton");
        if (multiplayerButton != null)
            multiplayerButton.clicked -= LoadMultiplayerScene;
    }

    void LoadBattleArenaScene()
    {
        var nm = NetworkManager.singleton;
        if (nm != null)
        {
            nm.onlineScene = battleArenaSceneName;
            nm.autoCreatePlayer = false; // No player prefab needed in singleplayer
            nm.StartHost(); // Local host - Mirror handles scene transition
        }
        else
        {
            SceneManager.LoadScene(battleArenaSceneName);
        }
    }

    void LoadMultiplayerScene()
    {
        var nm = NetworkManager.singleton;
        if (nm != null)
        {
            nm.onlineScene = multiplayerSceneName;
            nm.autoCreatePlayer = true; // Multiplayer needs player prefabs
            nm.StartHost(); // Host starts, clients join separately
        }
        else
        {
            SceneManager.LoadScene(multiplayerSceneName);
        }
    }
}