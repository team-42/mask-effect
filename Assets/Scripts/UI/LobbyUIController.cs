using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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
        SceneManager.LoadScene(battleArenaSceneName);
    }

    void LoadMultiplayerScene()
    {
        SceneManager.LoadScene(multiplayerSceneName);
    }
}