using UnityEngine;

namespace MaskEffect
{
    public class Billboard : MonoBehaviour
    {
        private Transform mainCameraTransform;

        void Start()
        {
            // Find the main camera's transform. This assumes there is always a main camera.
            // If the camera might change or be inactive, this could be updated in Update()
            // or through an event system.
            if (Camera.main != null)
            {
                mainCameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("Billboard script could not find a main camera. Ensure your camera is tagged 'MainCamera'.");
            }
        }

        void LateUpdate()
        {
            if (mainCameraTransform == null)
            {
                // Try to find the camera again if it was null in Start()
                if (Camera.main != null)
                {
                    mainCameraTransform = Camera.main.transform;
                }
                else
                {
                    return; // Still no camera, do nothing
                }
            }

            // Make the UI element face the camera
            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                             mainCameraTransform.rotation * Vector3.up);
        }
    }
}
