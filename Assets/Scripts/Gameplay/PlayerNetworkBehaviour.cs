using Mirror;
using UnityEngine;
using UnityEngine.UI; // Required for CanvasScaler and GraphicRaycaster
using UnityEngine.UIElements; // Required for UI Toolkit elements
using System.Collections.Generic; // Potentially needed for UI management
using MaskEffect; // Required for MaskPanelUI

public class PlayerNetworkBehaviour : NetworkBehaviour
{
    [Header("Player Visuals & UI")]
    public GameObject mechPrefab; // Assign your mech prefab here in the Inspector
    public GameObject maskMenuUIPrefab; // Assign your mask menu UI prefab here in the Inspector

    private GameObject instantiatedMech;
    private GameObject instantiatedMaskMenuUI;
    private MaskPanelUI maskPanelUI; // Reference to the MaskPanelUI script if it exists

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("PlayerNetworkBehaviour OnStartClient called.");

        // Instantiate mech for all clients (including local client)
        if (mechPrefab != null)
        {
            instantiatedMech = Instantiate(mechPrefab, transform);
            instantiatedMech.transform.localPosition = Vector3.zero; // Position relative to player object
            Debug.Log($"Mech instantiated for player {connectionToClient.connectionId}");
        }
        else
        {
            Debug.LogWarning("Mech Prefab is not assigned in PlayerNetworkBehaviour.");
        }
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

        // Instantiate mask menu UI only for the local player
        if (maskMenuUIPrefab != null)
        {
            // Find the main Canvas in the scene or create one if necessary
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>(); // Use UnityEngine.UI.CanvasScaler
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>(); // Use UnityEngine.UI.GraphicRaycaster
            }

            instantiatedMaskMenuUI = Instantiate(maskMenuUIPrefab, canvas.transform);
            maskPanelUI = instantiatedMaskMenuUI.GetComponent<MaskPanelUI>();
            if (maskPanelUI != null)
            {
                maskPanelUI.gameObject.SetActive(true); // Ensure the UI is active
                Debug.Log("Mask Menu UI instantiated and activated for local player.");
            }
            else
            {
                Debug.LogWarning("Mask Menu UI Prefab does not have a MaskPanelUI component.");
            }
        }
        else
        {
            Debug.LogWarning("Mask Menu UI Prefab is not assigned in PlayerNetworkBehaviour.");
        }
    }

    public override void OnStopLocalPlayer()
    {
        base.OnStopLocalPlayer();
        Debug.Log("PlayerNetworkBehaviour OnStopLocalPlayer called. This is no longer the local player.");
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        Debug.Log("PlayerNetworkBehaviour OnStopClient called.");

        // Clean up instantiated objects
        if (instantiatedMech != null)
        {
            Destroy(instantiatedMech);
        }
        if (instantiatedMaskMenuUI != null)
        {
            Destroy(instantiatedMaskMenuUI);
        }
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
