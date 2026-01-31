using UnityEngine;

public class DragAndDropSnapper : MonoBehaviour
{
    private TileGridManager gridManager;
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 originalScale; // Store the original scale

    void Start()
    {
        gridManager = FindFirstObjectByType<TileGridManager>();
        mainCamera = Camera.main;
        originalScale = transform.localScale; // Store the initial scale

        if (gridManager == null)
        {
            Debug.LogError("TileGridManager not found in scene. DragAndDropSnapper disabled.");
            enabled = false;
            return;
        }
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found in scene. DragAndDropSnapper disabled.");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform) // Check if we clicked on this object
                {
                    isDragging = true;
                    dragOffset = transform.position - hit.point;
                    transform.localScale = originalScale * 0.95f; // Apply a slight shrink effect (reduced from 0.9f)
                }
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground"))) // Assuming tiles are on a "Ground" layer
            {
                // Calculate the target position based on mouse hit point and drag offset
                Vector3 targetWorldPosition = hit.point + dragOffset;
                // Snap to tile position during drag
                Vector3 currentSnappedPosition = gridManager.GetNearestTilePosition(targetWorldPosition);
                float tileTopY = 0.05f; // Half of the tile's scale.y (0.1f / 2)
                float draggableHalfHeight = transform.localScale.y / 2;
                transform.position = new Vector3(currentSnappedPosition.x, tileTopY + draggableHalfHeight, currentSnappedPosition.z);
            }
            // If not hitting ground, the object should stay at its last valid snapped position
            // No 'else' block needed here, as movement is restricted to the grid.
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            transform.localScale = originalScale; // Restore original scale
            // The object is already snapped during drag, so no additional snapping needed on release.
            // However, we can re-evaluate the position one last time to ensure it's perfectly on a tile.
            if (gridManager != null)
            {
                Vector3 finalSnappedPosition = gridManager.GetNearestTilePosition(transform.position);
                float tileTopY = 0.05f;
                float draggableHalfHeight = transform.localScale.y / 2;
                transform.position = new Vector3(finalSnappedPosition.x, tileTopY + draggableHalfHeight, finalSnappedPosition.z);
            }
        }
    }
}
