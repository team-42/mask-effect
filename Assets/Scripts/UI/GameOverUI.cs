using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

namespace MaskEffect
{
    public class GameOverUI : MonoBehaviour
    {
        private bool visible;
        private Team winner;
        private bool subscribed;

        /// <summary>
        /// The team this local player controls. Host/offline = Player, Client = Enemy.
        /// </summary>
        private Team MyTeam =>
            (NetworkHelper.IsOffline || NetworkServer.active) ? Team.Player : Team.Enemy;

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

            // Title (perspective-correct: client controls Enemy team)
            bool playerWon = winner == MyTeam;
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
                bool isHost = MyTeam == Team.Player;
                int myAlive = isHost ? BattleManager.Instance.lastPlayerAlive : BattleManager.Instance.lastEnemyAlive;
                int theirAlive = isHost ? BattleManager.Instance.lastEnemyAlive : BattleManager.Instance.lastPlayerAlive;
                GUILayout.Label($"Eigene Mechs: {myAlive}   Gegner: {theirAlive}", statsStyle);
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

            // "Naechste Runde" only for server/host or offline
            if (NetworkHelper.IsServerOrOffline)
            {
                if (GUILayout.Button("Naechste Runde", btnStyle, GUILayout.Height(50f)))
                {
                    visible = false;
                    if (BattleManager.Instance != null)
                        BattleManager.Instance.StartNewRound();
                }

                GUILayout.Space(10f);
            }
            else
            {
                // Client: show waiting text
                GUIStyle waitStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
                waitStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
                GUILayout.Label("Warte auf Host...", waitStyle);
                GUILayout.Space(10f);
            }

            if (GUILayout.Button("Zurueck zur Lobby", btnStyle, GUILayout.Height(50f)))
            {
                visible = false;
                if (NetworkManager.singleton != null)
                {
                    if (NetworkServer.active && NetworkClient.isConnected)
                        NetworkManager.singleton.StopHost();
                    else if (NetworkClient.isConnected)
                        NetworkManager.singleton.StopClient();
                    else
                        SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }

            GUILayout.EndArea();
        }
    }
}
