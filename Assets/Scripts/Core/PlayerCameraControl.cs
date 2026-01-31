using UnityEngine;

public class PlayerCameraControl : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 3.0f;
    public float zoomSpeed = 5.0f; // New variable for zoom speed

    void Update()
    {
        // Camera Movement (WASD)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Camera Rotation (Mouse)
        if (Input.GetMouseButton(1)) // Right-click to rotate
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 rotation = transform.localEulerAngles;
            rotation.y += mouseX * rotationSpeed;
            rotation.x -= mouseY * rotationSpeed;
            transform.localEulerAngles = rotation;
        }

        // Camera Zoom (Mouse Wheel)
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            transform.position += transform.forward * scrollInput * zoomSpeed;
        }
    }
}
