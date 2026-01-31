using UnityEngine;

namespace MaskEffect
{
    /// <summary>
    /// Adds a gentle hovering bob to the child body of a flying mech.
    /// Attach to the Body child object, not the root mech.
    /// </summary>
    public class HoverBob : MonoBehaviour
    {
        public float amplitude = 0.15f;
        public float frequency = 1.2f;

        private float baseY;
        private float timeOffset;

        private void Start()
        {
            baseY = transform.localPosition.y;
            // Random offset so all jets don't bob in sync
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            Vector3 pos = transform.localPosition;
            pos.y = baseY + Mathf.Sin((Time.time + timeOffset) * frequency * Mathf.PI * 2f) * amplitude;
            transform.localPosition = pos;
        }
    }
}
