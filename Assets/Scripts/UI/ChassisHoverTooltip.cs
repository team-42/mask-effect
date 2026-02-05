using UnityEngine;
using Mirror;

namespace MaskEffect
{
    /// <summary>
    /// Handles always-on chassis tooltip display during MaskAssignment and Combat states.
    /// Separate from mask assignment drag/drop logic for clean separation of concerns.
    /// </summary>
    public class ChassisHoverTooltip : MonoBehaviour
    {
        private Camera mainCamera;
        private LayerMask mechLayerMask;
        private MaskAssignmentTooltip tooltip;
        private MaskAssignmentManager maskManager;

        /// <summary>
        /// Determines which team the local player controls.
        /// In singleplayer or as host: Player team
        /// As client: Enemy team
        /// </summary>
        private Team MyTeam
        {
            get
            {
                if (NetworkHelper.IsOffline)
                    return Team.Player;
                if (NetworkServer.active)
                    return Team.Player;
                return Team.Enemy;
            }
        }

        void Start()
        {
            mainCamera = Camera.main;
            mechLayerMask = LayerMask.GetMask("Mech");
            maskManager = FindFirstObjectByType<MaskAssignmentManager>();
        }

        void Update()
        {
            // Only show tooltips during MaskAssignment and Combat states
            BattleState currentState = BattleManager.Instance?.currentState ?? BattleState.Setup;
            if (currentState != BattleState.MaskAssignment && currentState != BattleState.Combat)
            {
                GetTooltip().Hide();
                return;
            }

            // Skip if mask manager is actively dragging or carrying a mask
            // (don't interfere with its own tooltip logic)
            if (maskManager != null && maskManager.IsBusy)
            {
                return;
            }

            // Raycast for hover detection
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, mechLayerMask))
            {
                MechController mech = hit.collider.GetComponent<MechController>();
                if (IsValidHoverTarget(mech))
                {
                    GetTooltip().ShowForChassis(mech);
                }
                else
                {
                    GetTooltip().Hide();
                }
            }
            else
            {
                GetTooltip().Hide();
            }
        }

        /// <summary>
        /// Validates if a mech is a valid hover target for tooltips.
        /// Must be: alive, on local player's team, and have chassis data loaded.
        /// </summary>
        bool IsValidHoverTarget(MechController mech)
        {
            return mech != null
                && mech.team == MyTeam
                && mech.isAlive
                && mech.chassisData != null;
        }

        /// <summary>
        /// Lazy initialization of tooltip singleton.
        /// Reuses the same pattern as MaskAssignmentManager.
        /// </summary>
        MaskAssignmentTooltip GetTooltip()
        {
            if (tooltip == null)
            {
                tooltip = FindFirstObjectByType<MaskAssignmentTooltip>();
                if (tooltip == null)
                    tooltip = gameObject.AddComponent<MaskAssignmentTooltip>();
            }
            return tooltip;
        }
    }
}
