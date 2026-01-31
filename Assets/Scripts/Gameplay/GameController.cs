using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class GameController : NetworkBehaviour
{
    public static GameController Instance { get; private set; }

    private Dictionary<NetworkConnectionToClient, string> playerPlans = new Dictionary<NetworkConnectionToClient, string>();
    private int playersReady = 0;
    private const int RequiredPlayers = 2; // Assuming a 2-player game

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("GameController OnStartServer called.");
        playerPlans.Clear();
        playersReady = 0;
    }

    // Called by PlayerNetworkBehaviour when a client sends a plan
    [Server]
    public void ReceivePlayerPlan(NetworkConnectionToClient sender, string planData)
    {
        if (!playerPlans.ContainsKey(sender))
        {
            playerPlans.Add(sender, planData);
            playersReady++;
            Debug.Log($"Server received plan from connection {sender.connectionId}. Total players ready: {playersReady}");

            if (playersReady == RequiredPlayers)
            {
                Debug.Log("All players have submitted plans. Simulating outcome...");
                SimulateOutcome();
            }
        }
        else
        {
            Debug.LogWarning($"Player {sender.connectionId} already submitted a plan for this round.");
        }
    }

    [Server]
    private void SimulateOutcome()
    {
        // This is where the game logic for simulating the outcome would go.
        // For now, we'll just log the received plans and send a generic result.
        string outcomeMessage = "Battle Result: ";
        foreach (var entry in playerPlans)
        {
            outcomeMessage += $"Player {entry.Key.connectionId} planned '{entry.Value}'; ";
        }

        Debug.Log($"Simulation complete. Outcome: {outcomeMessage}");

        // Send the result to all clients
        RpcBattleResult(outcomeMessage);

        // Reset for next round (if applicable)
        playerPlans.Clear();
        playersReady = 0;
    }

    [ClientRpc]
    private void RpcBattleResult(string resultData)
    {
        Debug.Log($"Client received battle result from GameController: {resultData}");
        // Clients would update their UI/game state here
    }
}
