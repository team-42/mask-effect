using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log($"Player added for connection {conn.connectionId}. Total players: {numPlayers}");

        // You can add custom logic here, e.g., spawning specific player prefabs
        // or assigning player-specific data.
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        Debug.Log($"Player disconnected from connection {conn.connectionId}. Total players: {numPlayers - 1}");
        base.OnServerDisconnect(conn);
    }
}
