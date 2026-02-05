using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    public override void Start()
    {
        base.Start();
        // Ensure we return to lobby when networking stops
        if (string.IsNullOrEmpty(offlineScene))
            offlineScene = "MainMenu";
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log($"Player added for connection {conn.connectionId}. Total players: {numPlayers}");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"Player disconnected from connection {conn.connectionId}. Total players: {numPlayers - 1}");
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Debug.Log("Server stopped.");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("Client stopped.");
    }
}
