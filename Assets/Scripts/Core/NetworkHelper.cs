using UnityEngine;
using Mirror;

namespace MaskEffect
{
    /// <summary>
    /// Provides dual-mode helpers so gameplay code works both with and without
    /// an active NetworkManager (singleplayer vs multiplayer).
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// True when no active server or client connection (singleplayer mode).
        /// </summary>
        public static bool IsOffline =>
            !NetworkServer.active && !NetworkClient.active;

        /// <summary>
        /// True when we should run authoritative / server-side logic.
        /// Multiplayer: only on the server. Singleplayer: always true.
        /// </summary>
        public static bool IsServerOrOffline =>
            NetworkServer.active || IsOffline;

        /// <summary>
        /// Spawn a GameObject on the network if networking is active.
        /// In singleplayer, does nothing (object is already in the scene).
        /// </summary>
        public static void SpawnOrIgnore(GameObject go)
        {
            if (NetworkServer.active)
                NetworkServer.Spawn(go);
        }

        /// <summary>
        /// Destroy a GameObject. Uses NetworkServer.Destroy in multiplayer,
        /// plain Object.Destroy in singleplayer.
        /// </summary>
        public static void SmartDestroy(GameObject go)
        {
            if (NetworkServer.active)
                NetworkServer.Destroy(go);
            else
                Object.Destroy(go);
        }
    }
}
