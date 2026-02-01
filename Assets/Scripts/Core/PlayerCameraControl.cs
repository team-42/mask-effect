using UnityEngine;

public class PlayerCameraControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1200.0f; // Drastically increased move speed for interpolation responsiveness
    public Vector2 arenaBoundsX = new Vector2(-5f, 5f);
    public Vector2 arenaBoundsZ = new Vector2(-4f, 1f);

    [Header("Rotation Settings")]
    public float rotationSpeed = 3.0f;
    public float minPitch = 10.0f; // Minimum camera pitch (looking up)
    public float maxPitch = 80.0f; // Maximum camera pitch (looking down)

    [Header("Zoom Settings")]
    public float zoomSpeed = 6400.0f; // Drastically increased zoom speed for direct height change
    public float minHeight = 5.0f; // Minimum camera height (closer to ground)
    public float maxHeight = 50.0f; // Maximum camera height (further from ground)

    private Vector3 cameraTargetPosition;
    private Quaternion cameraTargetRotation;
    private Vector3 lastMousePosition; // For middle-mouse drag panning

    void Start()
    {
        cameraTargetPosition = transform.position;
        cameraTargetRotation = transform.rotation;
    }

    void Update()
    {
        HandleMovement();
        HandleDragPanning();
        HandleRotation();
        HandleZoom();

        // Smoothly interpolate camera position and rotation
        transform.position = Vector3.Lerp(transform.position, cameraTargetPosition, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraTargetRotation, Time.deltaTime * rotationSpeed);
    }

    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Project forward and right vectors onto the XZ plane to ensure horizontal movement
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * verticalInput + right * horizontalInput;
        cameraTargetPosition += moveDirection * moveSpeed * Time.deltaTime;

        // Clamp camera movement to arena bounds
        ClampCameraPosition();
    }

    void HandleDragPanning()
    {
        if (Input.GetMouseButtonDown(2)) // Middle-mouse press
        {
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(2)) // Middle-mouse held
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Convert screen-space drag to world-space movement
            // The multiplier (0.01f) might need adjustment based on desired sensitivity
            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * (moveSpeed * 0.01f);
            cameraTargetPosition += move;

            ClampCameraPosition();
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // Right-click to rotate
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Apply yaw (Y-axis rotation)
            cameraTargetRotation *= Quaternion.Euler(0, mouseX * rotationSpeed, 0);

            // Apply pitch (X-axis rotation) and clamp it
            float currentPitch = cameraTargetRotation.eulerAngles.x;
            if (currentPitch > 180) currentPitch -= 360; // Normalize angle to -180 to 180

            float newPitch = currentPitch - mouseY * rotationSpeed; // Inverted mouseY for natural feel
            newPitch = Mathf.Clamp(newPitch, minPitch, maxPitch);

            // Convert back to Quaternion, keeping Y rotation
            cameraTargetRotation = Quaternion.Euler(newPitch, cameraTargetRotation.eulerAngles.y, 0);
        }
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            // Get ground point camera is looking at
            Ray ray = new Ray(transform.position, transform.forward);
            Plane groundPlane = new Plane(Vector3.up, 0f); // Assuming ground is at y=0

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 focusPoint = ray.GetPoint(enter);
                float currentHeight = transform.position.y;
                float targetHeight = Mathf.Clamp(
                    currentHeight - 3.0f * scrollInput * zoomSpeed,
                    minHeight,
                    maxHeight
                );

                // Preserve current pitch angle during zoom
                float pitchAngle = cameraTargetRotation.eulerAngles.x;
                if (pitchAngle > 180) pitchAngle -= 360; // Normalize angle

                // Reposition camera to maintain focus on ground point
                // Calculate the horizontal distance from the focus point
                float horizontalDistance = targetHeight / Mathf.Tan(pitchAngle * Mathf.Deg2Rad);

                // Get the horizontal direction from the focus point to the camera
                Vector3 horizontalDirection = (transform.position - focusPoint);
                horizontalDirection.y = 0;
                horizontalDirection.Normalize();

                // Calculate the new camera target position
                cameraTargetPosition = focusPoint + horizontalDirection * horizontalDistance;
                cameraTargetPosition.y = targetHeight; // Set the height

                cameraTargetRotation = Quaternion.Euler(pitchAngle, cameraTargetRotation.eulerAngles.y, 0);
                ClampCameraPosition();
            }
        }
    }

    void ClampCameraPosition()
    {
        cameraTargetPosition.x = Mathf.Clamp(cameraTargetPosition.x, arenaBoundsX.x, arenaBoundsX.y);
        cameraTargetPosition.z = Mathf.Clamp(cameraTargetPosition.z, arenaBoundsZ.x, arenaBoundsZ.y);
    }
}
