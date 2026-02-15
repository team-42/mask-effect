using UnityEngine;

namespace MaskEffect
{
    /// <summary>
    /// Displays a pulsing Fresnel sphere and a blue shield bar above the mech
    /// when the mech has an active shield status effect.
    /// Mirrors the MechHealthBar pattern.
    /// </summary>
    public class ShieldVisual : MonoBehaviour
    {
        private MechController _mech;
        private GameObject _sphere;
        private Material _sphereMat;
        private GameObject _barQuad;
        private Material _barMat;
        private bool _initialized;
        private float _maxShield;

        private const float SHIELD_BAR_OFFSET = 0.2f;
        private static readonly int ShieldPercentProp = Shader.PropertyToID("_ShieldPercent");

        public void Initialize(MechController mech)
        {
            _mech = mech;

            float hoverOffset = (mech.chassisData != null && mech.chassisData.canFly)
                ? mech.chassisData.hoverHeight
                : 0f;

            // --- Shield sphere ---
            _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _sphere.name = "ShieldSphere";
            _sphere.transform.SetParent(mech.transform, false);
            _sphere.transform.localPosition = new Vector3(0f, hoverOffset + 0.4f, 0f);
            _sphere.transform.localScale = Vector3.one * 1.4f;

            var sphereCollider = _sphere.GetComponent<Collider>();
            if (sphereCollider != null)
                Object.Destroy(sphereCollider);

            Material sphereBase = Resources.Load<Material>("Materials/ShieldBubble");
            if (sphereBase != null)
            {
                _sphereMat = new Material(sphereBase);
                _sphere.GetComponent<Renderer>().material = _sphereMat;
            }
            else
            {
                Debug.LogWarning("[ShieldVisual] ShieldBubble material not found in Resources/Materials/");
            }

            _sphere.SetActive(false);

            // --- Shield bar ---
            GameObject barPivot = new GameObject("ShieldBarPivot");
            barPivot.transform.SetParent(mech.transform, false);
            barPivot.transform.localPosition = new Vector3(0f, MechHealthBar.BASE_HEIGHT + SHIELD_BAR_OFFSET + hoverOffset, 0f);

            if (barPivot.GetComponent<Billboard>() == null)
                barPivot.AddComponent<Billboard>();

            _barQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _barQuad.name = "ShieldBarQuad";
            _barQuad.transform.SetParent(barPivot.transform, false);
            _barQuad.transform.localPosition = Vector3.zero;
            _barQuad.transform.localScale = new Vector3(0.8f, 0.1f, 1f);

            var barCollider = _barQuad.GetComponent<Collider>();
            if (barCollider != null)
                Object.Destroy(barCollider);

            Material barBase = Resources.Load<Material>("Materials/ShieldBar");
            if (barBase != null)
            {
                _barMat = new Material(barBase);
                _barMat.renderQueue = 4999;
                var rend = _barQuad.GetComponent<Renderer>();
                rend.material = _barMat;
                rend.allowOcclusionWhenDynamic = false;
            }
            else
            {
                Debug.LogWarning("[ShieldVisual] ShieldBar material not found in Resources/Materials/");
            }

            barPivot.SetActive(false);
            _initialized = true;
        }

        public void UpdateShield(float shieldAmount, int maxHP)
        {
            if (!_initialized) return;

            bool active = _mech != null && _mech.isAlive && shieldAmount > 0f;

            if (_sphere != null)
                _sphere.SetActive(active);

            if (_barQuad != null)
                _barQuad.transform.parent.gameObject.SetActive(active);

            if (active && _barMat != null)
            {
                // Record max shield when it first appears (or grows)
                if (shieldAmount > _maxShield)
                    _maxShield = shieldAmount;
                _barMat.SetFloat(ShieldPercentProp, Mathf.Clamp01(shieldAmount / _maxShield));
            }
            else if (!active)
            {
                _maxShield = 0f;
            }
        }

        public void ForceHide()
        {
            if (_sphere != null)
                _sphere.SetActive(false);

            if (_barQuad != null)
                _barQuad.transform.parent.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_sphereMat != null)
                Object.Destroy(_sphereMat);
            if (_barMat != null)
                Object.Destroy(_barMat);
        }
    }
}
