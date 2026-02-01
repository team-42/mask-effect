using UnityEngine;

namespace MaskEffect
{
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("[Billboard] Main camera not found. Ensure your camera is tagged 'MainCamera'.");
                enabled = false; // Disable script if no camera is found
            }
        }

        void LateUpdate()
        {
            if (mainCamera == null) return;

            // Make the object face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                             mainCamera.transform.rotation * Vector3.up);
        }
    }
}
