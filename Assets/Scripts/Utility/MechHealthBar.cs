using UnityEngine;

namespace MaskEffect
{
    /// <summary>
    /// Displays a health bar above a mech. Shows remaining HP (green) and missing HP (red).
    /// Only visible when the mech is damaged but alive.
    /// </summary>
    public class MechHealthBar : MonoBehaviour
    {
        private MechController _mech;
        private GameObject _quad;
        private Material _material;
        private bool _initialized;
        private bool _positionedForFlying;

        private const float BASE_HEIGHT = 1.6f;
        private static readonly int HealthPercentProperty = Shader.PropertyToID("_HealthPercent");

        /// <summary>
        /// Initialize the health bar for a given mech.
        /// </summary>
        public void Initialize(MechController mech)
        {
            _mech = mech;

            // Position above the mech body (will be updated for flying mechs when chassisData is available)
            UpdatePositionForChassis();

            // Create quad
            _quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quad.name = "HealthBarQuad";
            _quad.transform.SetParent(transform, false);
            _quad.transform.localPosition = Vector3.zero;
            _quad.transform.localScale = new Vector3(0.8f, 0.1f, 1f);

            // Remove collider from quad
            var collider = _quad.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            // Load and apply material (instanced for per-mech health value)
            Material baseMaterial = Resources.Load<Material>("Materials/HealthBar");
            if (baseMaterial != null)
            {
                _material = new Material(baseMaterial);
                // Force maximum render queue to render on top of all transparent effects including 2D sprites
                _material.renderQueue = 5000;
                _quad.GetComponent<Renderer>().material = _material;

                var renderer = _quad.GetComponent<Renderer>();
                renderer.allowOcclusionWhenDynamic = false;
            }
            else
            {
                Debug.LogWarning("[MechHealthBar] HealthBar material not found in Resources/Materials/");
            }

            // Add billboard component for camera-facing
            if (GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent<Billboard>();
            }

            _initialized = true;

            // Initial visibility check
            UpdateVisibility();
        }

        /// <summary>
        /// Update position based on chassis hover height. Call this when chassisData becomes available.
        /// </summary>
        public void UpdatePositionForChassis()
        {
            if (_mech == null) return;

            float hoverOffset = (_mech.chassisData != null && _mech.chassisData.canFly) ? _mech.chassisData.hoverHeight : 0f;
            transform.localPosition = new Vector3(0f, BASE_HEIGHT + hoverOffset, 0f);

            if (hoverOffset > 0f)
                _positionedForFlying = true;
        }

        private void LateUpdate()
        {
            // If chassisData wasn't available during Initialize, update position when it becomes available
            if (!_positionedForFlying && _mech != null && _mech.chassisData != null && _mech.chassisData.canFly)
            {
                UpdatePositionForChassis();
            }
        }

        /// <summary>
        /// Update the health bar fill percentage.
        /// </summary>
        public void UpdateHealth(float percent)
        {
            if (!_initialized || _material == null) return;

            _material.SetFloat(HealthPercentProperty, Mathf.Clamp01(percent));
            UpdateVisibility();
        }

        /// <summary>
        /// Update visibility: show only when damaged but alive.
        /// </summary>
        private void UpdateVisibility()
        {
            if (_quad == null || _mech == null) return;

            bool shouldShow = _mech.isAlive && _mech.currentHP < _mech.maxHP;
            _quad.SetActive(shouldShow);
        }

        /// <summary>
        /// Force hide the health bar (called on mech death).
        /// </summary>
        public void ForceHide()
        {
            if (_quad != null)
                _quad.SetActive(false);
        }

        private void OnDestroy()
        {
            // Clean up instanced material
            if (_material != null)
            {
                Object.Destroy(_material);
                _material = null;
            }
        }
    }
}
