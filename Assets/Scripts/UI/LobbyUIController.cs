using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LobbyUIController : MonoBehaviour
{
    public string battleArenaSceneName = "BattleArenaScene";

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
        {
            startGameButton.clicked += LoadBattleArenaScene;
        }
        else
        {
            Debug.LogError("Start Game Button not found in UXML.");
        }
    }

    void OnDisable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        var startGameButton = root.Q<Button>("startGameButton");

        if (startGameButton != null)
        {
            startGameButton.clicked -= LoadBattleArenaScene;
        }
    }

    void LoadBattleArenaScene()
    {
        SceneManager.LoadScene(battleArenaSceneName);
    }
}