using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaskEffect
{
    public class GameOverUI : MonoBehaviour
    {
        private bool visible;
        private Team winner;
        private bool subscribed;

        private Texture2D overlayTexture;

        private void Start()
        {
            overlayTexture = new Texture2D(1, 1);
            overlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.75f));
            overlayTexture.Apply();

            TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (subscribed || BattleManager.Instance == null) return;
            BattleManager.Instance.OnRoundEnded += OnRoundEnded;
            BattleManager.Instance.OnStateChanged += OnStateChanged;
            subscribed = true;
        }

        private void Update()
        {
            if (!subscribed) TrySubscribe();
        }

        private void OnDestroy()
        {
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnRoundEnded -= OnRoundEnded;
                BattleManager.Instance.OnStateChanged -= OnStateChanged;
            }

            if (overlayTexture != null)
                Destroy(overlayTexture);
        }

        private void OnRoundEnded(Team winner)
        {
            this.winner = winner;
            visible = true;
        }

        private void OnStateChanged(BattleState state)
        {
            if (state != BattleState.RoundEnd)
                visible = false;
        }

        private void OnGUI()
        {
            if (!visible) return;

            // Full-screen dark overlay
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

            float boxW = 400f;
            float boxH = 300f;
            float boxX = (Screen.width - boxW) * 0.5f;
            float boxY = (Screen.height - boxH) * 0.5f;

            GUILayout.BeginArea(new Rect(boxX, boxY, boxW, boxH));

            // Title
            bool playerWon = winner == Team.Player;
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 42,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = playerWon ? new Color(0.2f, 1f, 0.3f) : new Color(1f, 0.25f, 0.2f);

            GUILayout.Space(20f);
            GUILayout.Label(playerWon ? "SIEG!" : "NIEDERLAGE!", titleStyle);
            GUILayout.Space(15f);

            // Stats
            GUIStyle statsStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            statsStyle.normal.textColor = Color.white;

            int roundNum = BattleManager.Instance != null ? BattleManager.Instance.roundNumber : 0;
            GUILayout.Label($"Runde {roundNum}", statsStyle);
            GUILayout.Space(8f);

            if (BattleManager.Instance != null)
            {
                int playerAlive = 0, enemyAlive = 0;
                var allMechs = BattleManager.Instance.allMechs;
                for (int i = 0; i < allMechs.Count; i++)
                {
                    if (!allMechs[i].isAlive) continue;
                    if (allMechs[i].team == Team.Player) playerAlive++;
                    else enemyAlive++;
                }
                GUILayout.Label($"Eigene Mechs: {playerAlive}   Gegner: {enemyAlive}", statsStyle);
            }

            GUILayout.Space(30f);

            // Buttons
            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            btnStyle.normal.textColor = Color.white;
            btnStyle.hover.textColor = Color.white;
            btnStyle.padding = new RectOffset(20, 20, 12, 12);

            if (GUILayout.Button("Naechste Runde", btnStyle, GUILayout.Height(50f)))
            {
                visible = false;
                if (BattleManager.Instance != null)
                    BattleManager.Instance.StartNewRound();
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("Zurueck zur Lobby", btnStyle, GUILayout.Height(50f)))
            {
                visible = false;
                SceneManager.LoadScene("LobbyScene");
            }

            GUILayout.EndArea();
        }
    }
}
