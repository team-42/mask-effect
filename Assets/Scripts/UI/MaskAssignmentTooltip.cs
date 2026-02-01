using UnityEngine;

namespace MaskEffect
{
    /// <summary>
    /// IMGUI tooltip shown near the cursor while the player carries a mask over a
    /// valid (unmasked, friendly, alive) mech.  Lifecycle is driven entirely by
    /// MaskAssignmentManager via Show / Hide.
    /// </summary>
    public class MaskAssignmentTooltip : MonoBehaviour
    {
        private bool visible;
        private string tooltipText;
        private Color tooltipTintColor;

        private const float Padding = 8f;
        private const float OffsetX = 14f;
        private const float OffsetY = -28f;

        /// <summary>
        /// Show the tooltip.  Safe to call every frame; internally a no-op when the
        /// displayed content has not changed.
        /// </summary>
        public void Show(MaskData mask, MechController mech)
        {
            string newText = $"{mask.maskName} \u2192 {mech.chassisData.chassisName}";
            if (visible && tooltipText == newText)
                return;

            tooltipText = newText;
            tooltipTintColor = mask.maskTint;
            visible = true;
        }

        /// <summary>Hide the tooltip immediately.</summary>
        public void Hide()
        {
            visible = false;
        }

        private void OnGUI()
        {
            if (!visible) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;

            Vector2 textSize = style.CalcSize(new GUIContent(tooltipText));
            float boxW = textSize.x + Padding * 2f;
            float boxH = textSize.y + Padding * 2f;

            // GUI-space origin is top-left; Input.mousePosition origin is bottom-left.
            float rawX = Input.mousePosition.x + OffsetX;
            float rawY = (Screen.height - Input.mousePosition.y) + OffsetY;

            float clampedX = Mathf.Clamp(rawX, 0f, Screen.width - boxW);
            float clampedY = Mathf.Clamp(rawY, 0f, Screen.height - boxH);

            Rect tooltipRect = new Rect(clampedX, clampedY, boxW, boxH);

            // Semi-transparent background tinted by the mask color.
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(
                tooltipTintColor.r * 0.35f,
                tooltipTintColor.g * 0.35f,
                tooltipTintColor.b * 0.35f,
                0.85f);
            GUI.Box(tooltipRect, GUIContent.none);
            GUI.backgroundColor = prevBg;

            Rect labelRect = new Rect(
                tooltipRect.x + Padding,
                tooltipRect.y + Padding,
                textSize.x,
                textSize.y);
            GUI.Label(labelRect, tooltipText, style);
        }
    }
}
