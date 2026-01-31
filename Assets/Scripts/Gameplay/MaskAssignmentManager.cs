using UnityEngine;

namespace MaskEffect
{
    public class MaskAssignmentManager : MonoBehaviour
    {
        [SerializeField] private SimpleFlatGrid grid;
        [SerializeField] private MaskPanelUI maskPanel;
        [SerializeField] private GameObject maskDragProxyPrefab;

        private Camera mainCamera;
        private LayerMask mechLayerMask;
        private LayerMask groundLayerMask;

        // Drag mode
        private enum DragMode { None, DraggingMech, CarryingMask }
        private DragMode currentMode = DragMode.None;

        // Mech drag state
        private MechController draggedMech;
        private Vector3 mechOriginalPosition;
        private int mechOriginalTile;

        // Mask carry state
        private MaskData carriedMask;
        private int carriedSlotIndex;
        private GameObject maskDragProxy;

        // Tile highlight state
        private int highlightedTile = -1;
        private Color highlightedOriginalColor;

        // Mech highlight state
        private MechController highlightedMech;
        private Color highlightedMechOriginalColor;

        private void Start()
        {
            mainCamera = Camera.main;
            if (grid == null)
                grid = FindFirstObjectByType<SimpleFlatGrid>();
            if (maskPanel == null)
                maskPanel = FindFirstObjectByType<MaskPanelUI>();

            // Auto-detect layers
            int mechLayer = LayerMask.NameToLayer("Mech");
            mechLayerMask = mechLayer >= 0 ? (1 << mechLayer) : ~0;
            int groundLayer = LayerMask.NameToLayer("Ground");
            groundLayerMask = groundLayer >= 0 ? (1 << groundLayer) : ~0;
        }

        private void Update()
        {
            if (BattleManager.Instance == null) return;
            if (BattleManager.Instance.currentState != BattleState.MaskAssignment) return;

            switch (currentMode)
            {
                case DragMode.None:
                    HandleIdleInput();
                    break;
                case DragMode.DraggingMech:
                    UpdateMechDrag();
                    break;
                case DragMode.CarryingMask:
                    UpdateMaskCarry();
                    break;
            }
        }

        // Called by MaskPanelUI when player clicks a mask slot
        public void StartCarryingMask(MaskData mask, int slotIndex)
        {
            if (currentMode != DragMode.None) return;

            carriedMask = mask;
            carriedSlotIndex = slotIndex;
            currentMode = DragMode.CarryingMask;

            // Create drag proxy from prefab
            if (maskDragProxyPrefab != null)
            {
                maskDragProxy = Instantiate(maskDragProxyPrefab);
            }
            else
            {
                maskDragProxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
                maskDragProxy.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                var col = maskDragProxy.GetComponent<Collider>();
                if (col != null) col.enabled = false;
            }
            maskDragProxy.name = "MaskDragProxy";
            var proxyRenderer = maskDragProxy.GetComponent<Renderer>();
            if (proxyRenderer != null)
                proxyRenderer.material.color = mask.maskTint;
        }

        private void HandleIdleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Don't start drag if clicking on the IMGUI panel
                if (maskPanel != null && maskPanel.IsMouseOverPanel())
                    return;

                // Try to pick up a player mech
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100f, mechLayerMask))
                {
                    MechController mech = hit.collider.GetComponent<MechController>();
                    if (mech != null && mech.team == Team.Player && mech.isAlive)
                    {
                        StartMechDrag(mech);
                    }
                }
            }
        }

        private void StartMechDrag(MechController mech)
        {
            draggedMech = mech;
            mechOriginalPosition = mech.transform.position;
            mechOriginalTile = grid.GetNearestTile(mech.transform.position);
            currentMode = DragMode.DraggingMech;
        }

        private void UpdateMechDrag()
        {
            // Cancel on right-click
            if (Input.GetMouseButtonDown(1))
            {
                CancelMechDrag();
                return;
            }

            // Move mech with mouse
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float dist;
            if (groundPlane.Raycast(ray, out dist))
            {
                Vector3 worldPoint = ray.GetPoint(dist);
                int tileIndex = grid.GetNearestTile(worldPoint);
                Vector3 tilePos = grid.GetTileWorldPosition(tileIndex);
                draggedMech.transform.position = tilePos;

                UpdateTileHighlight(tileIndex);
            }

            // Drop on mouse up
            if (Input.GetMouseButtonUp(0))
            {
                EndMechDrag();
            }
        }

        private void EndMechDrag()
        {
            int targetTile = grid.GetNearestTile(draggedMech.transform.position);

            bool validDrop = grid.GetTileZone(targetTile) == TileZone.Player
                && (!grid.IsTileOccupied(targetTile) || targetTile == mechOriginalTile);

            if (validDrop)
            {
                grid.ClearTile(mechOriginalTile);
                grid.SetTileOccupant(targetTile, draggedMech);
                draggedMech.transform.position = grid.GetTileWorldPosition(targetTile);
            }
            else
            {
                draggedMech.transform.position = mechOriginalPosition;
            }

            ClearTileHighlight();
            draggedMech = null;
            currentMode = DragMode.None;
        }

        private void CancelMechDrag()
        {
            draggedMech.transform.position = mechOriginalPosition;
            ClearTileHighlight();
            draggedMech = null;
            currentMode = DragMode.None;
        }

        private void UpdateMaskCarry()
        {
            // Move proxy with mouse
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float dist;
            if (groundPlane.Raycast(ray, out dist))
            {
                Vector3 worldPoint = ray.GetPoint(dist);
                if (maskDragProxy != null)
                    maskDragProxy.transform.position = new Vector3(worldPoint.x, 1f, worldPoint.z);
            }

            // Highlight mech under cursor
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, mechLayerMask))
            {
                MechController mech = hit.collider.GetComponent<MechController>();
                if (mech != null && mech.team == Team.Player && mech.isAlive && mech.equippedMask == null)
                {
                    SetMechHighlight(mech);
                }
                else
                {
                    ClearMechHighlight();
                }
            }
            else
            {
                ClearMechHighlight();
            }

            // Cancel on right-click
            if (Input.GetMouseButtonDown(1))
            {
                CancelMaskCarry();
                return;
            }

            // Drop mask on left-click
            if (Input.GetMouseButtonDown(0))
            {
                // Don't process if clicking on the panel
                if (maskPanel != null && maskPanel.IsMouseOverPanel())
                    return;

                if (highlightedMech != null)
                {
                    BattleManager.Instance.PlayerAssignMask(highlightedMech, carriedMask);
                    if (maskPanel != null)
                        maskPanel.MarkSlotUsed(carriedSlotIndex);
                    FinishMaskCarry();
                }
                else
                {
                    CancelMaskCarry();
                }
            }
        }

        private void CancelMaskCarry()
        {
            ClearMechHighlight();
            if (maskDragProxy != null)
                Destroy(maskDragProxy);
            carriedMask = null;
            currentMode = DragMode.None;
        }

        private void FinishMaskCarry()
        {
            // Don't restore old highlight color — EquipMask already set the mask tint
            highlightedMech = null;
            if (maskDragProxy != null)
                Destroy(maskDragProxy);
            carriedMask = null;
            currentMode = DragMode.None;
        }

        private void UpdateTileHighlight(int tileIndex)
        {
            if (tileIndex == highlightedTile) return;

            ClearTileHighlight();

            GameObject tileVisual = grid.GetTileVisual(tileIndex);
            if (tileVisual == null) return;

            var renderer = tileVisual.GetComponent<Renderer>();
            if (renderer == null) return;

            highlightedTile = tileIndex;
            highlightedOriginalColor = renderer.material.color;

            bool valid = grid.GetTileZone(tileIndex) == TileZone.Player
                && (!grid.IsTileOccupied(tileIndex) || tileIndex == mechOriginalTile);

            renderer.material.color = valid
                ? new Color(0.3f, 0.8f, 0.3f)
                : new Color(0.8f, 0.3f, 0.3f);
        }

        private void ClearTileHighlight()
        {
            if (highlightedTile < 0) return;

            GameObject tileVisual = grid.GetTileVisual(highlightedTile);
            if (tileVisual != null)
            {
                var renderer = tileVisual.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = highlightedOriginalColor;
            }

            highlightedTile = -1;
        }

        private void SetMechHighlight(MechController mech)
        {
            if (highlightedMech == mech) return;

            ClearMechHighlight();

            highlightedMech = mech;
            Transform topHalf = mech.transform.Find(MechSpawner.TOP_HALF_NAME);
            if (topHalf != null)
            {
                var renderer = topHalf.GetComponent<Renderer>();
                if (renderer != null)
                {
                    highlightedMechOriginalColor = renderer.material.color;
                    renderer.material.color = Color.white;
                }
            }
        }

        private void ClearMechHighlight()
        {
            if (highlightedMech == null) return;

            Transform topHalf = highlightedMech.transform.Find(MechSpawner.TOP_HALF_NAME);
            if (topHalf != null)
            {
                var renderer = topHalf.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = highlightedMechOriginalColor;
            }

            highlightedMech = null;
        }
    }
}
