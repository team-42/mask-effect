using Mirror;
using UnityEngine;

public class PlayerNetworkBehaviour : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("PlayerNetworkBehaviour OnStartClient called.");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("PlayerNetworkBehaviour OnStartServer called.");
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Debug.Log("PlayerNetworkBehaviour OnStartLocalPlayer called. This is the local player.");
        // Example: Enable input for the local player
        // GetComponent<PlayerInput>().enabled = true;
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        Debug.Log("PlayerNetworkBehaviour OnStopLocalPlayer called. This is no longer the local player.");
        // Example: Disable input for the local player
        // GetComponent<PlayerInput>().enabled = false;
    }

    // Example Command: Client sends an action to the server
    [Command]
    public void CmdSendPlan(string planData)
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.ReceivePlayerPlan(connectionToClient, planData);
        }
        else
        {
            Debug.LogError("GameController instance not found on server.");
        }
    }

    // Example ClientRpc: Server sends battle result to all clients
    [ClientRpc]
    public void RpcBattleResult(string resultData)
    {
        Debug.Log($"Client received battle result: {resultData}");
        // In a real game, clients would update their UI/game state based on this result
    }
}
