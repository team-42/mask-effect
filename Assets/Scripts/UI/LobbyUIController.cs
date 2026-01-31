using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Mirror;

public class LobbyUIController : MonoBehaviour
{
    public string battleArenaSceneName = "BattleArenaScene";
    public string multiplayerSceneName = "BattleArenaMultiplayer";

    private TextField ipField;

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

        var hostButton = root.Q<Button>("multiplayerHostButton");
        if (hostButton != null)
            hostButton.clicked += HostMultiplayerGame;

        var joinButton = root.Q<Button>("multiplayerJoinButton");
        if (joinButton != null)
            joinButton.clicked += JoinMultiplayerGame;

        ipField = root.Q<TextField>("ipAddressField");

        // Legacy: support old single "multiplayerButton" as host
        var multiplayerButton = root.Q<Button>("multiplayerButton");
        if (multiplayerButton != null)
            multiplayerButton.clicked += HostMultiplayerGame;
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

        var hostButton = root.Q<Button>("multiplayerHostButton");
        if (hostButton != null)
            hostButton.clicked -= HostMultiplayerGame;

        var joinButton = root.Q<Button>("multiplayerJoinButton");
        if (joinButton != null)
            joinButton.clicked -= JoinMultiplayerGame;

        var multiplayerButton = root.Q<Button>("multiplayerButton");
        if (multiplayerButton != null)
            multiplayerButton.clicked -= HostMultiplayerGame;
    }

    void LoadBattleArenaScene()
    {
        var nm = NetworkManager.singleton;
        if (nm != null)
        {
            nm.onlineScene = battleArenaSceneName;
            nm.autoCreatePlayer = false; // No player prefab needed in singleplayer
            nm.networkAddress = "localhost"; // Restrict to localhost for SP
            nm.StartHost();
        }
        else
        {
            SceneManager.LoadScene(battleArenaSceneName);
        }
    }

    void HostMultiplayerGame()
    {
        var nm = NetworkManager.singleton;
        if (nm != null)
        {
            nm.onlineScene = multiplayerSceneName;
            nm.autoCreatePlayer = true;
            nm.StartHost();
        }
        else
        {
            SceneManager.LoadScene(multiplayerSceneName);
        }
    }

    void JoinMultiplayerGame()
    {
        var nm = NetworkManager.singleton;
        if (nm == null) return;

        string ip = ipField != null ? ipField.value : "localhost";
        if (string.IsNullOrWhiteSpace(ip))
            ip = "localhost";

        nm.onlineScene = multiplayerSceneName;
        nm.autoCreatePlayer = true;
        nm.networkAddress = ip;
        nm.StartClient();
    }
}
