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
        private string headingText;
        private string bodyText;
        private Color tooltipTintColor;

        private const float Padding = 12f;
        private const float OffsetX = 14f;
        private const float OffsetY = -28f;
        private const float MinBodyWidth = 300f;

        /// <summary>
        /// Show the tooltip.  Safe to call every frame; internally a no-op when the
        /// displayed content has not changed.
        /// </summary>
        public void Show(MaskData mask, MechController mech)
        {
            string newHeading = $"{mask.maskName} \u2192 {mech.chassisData.chassisName}";

            MaskAbilityData ability = mask.GetAbilityForChassis(mech.chassisData.chassisType);
            string newBody = ability != null
                ? $"{ability.abilityName} \u2014 {ability.description}"
                : "";

            if (visible && headingText == newHeading && bodyText == newBody)
                return;

            headingText = newHeading;
            bodyText = newBody;
            tooltipTintColor = mask.maskTint;
            visible = true;
        }

        /// <summary>
        /// Show the chassis-info tooltip (idle hover, no mask being carried).
        /// Safe to call every frame; internally a no-op when displayed content
        /// has not changed.
        /// </summary>
        public void ShowForChassis(MechController mech)
        {
            string newHeading = mech.chassisData.chassisName;
            string newBody = mech.chassisData.description ?? "";

            if (visible && headingText == newHeading && bodyText == newBody)
                return;

            headingText = newHeading;
            bodyText = newBody;
            tooltipTintColor = Color.gray;
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

            GUIStyle headingStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            headingStyle.normal.textColor = Color.white;
            headingStyle.hover.textColor = Color.white;

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            bodyStyle.normal.textColor = Color.white;
            bodyStyle.hover.textColor = Color.white;

            Vector2 headingSize = headingStyle.CalcSize(new GUIContent(headingText));
            float boxW = Mathf.Max(headingSize.x, MinBodyWidth) + Padding * 2f;
            float bodyW = boxW - Padding * 2f;

            float bodyH = bodyText.Length > 0
                ? bodyStyle.CalcHeight(new GUIContent(bodyText), bodyW)
                : 0f;

            float separatorH = bodyH > 0 ? 1f : 0f;
            float gapH = bodyH > 0 ? Padding * 0.5f : 0f;
            float boxH = Padding + headingSize.y + gapH + separatorH + gapH + bodyH + Padding;

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

            // Heading
            Rect headingRect = new Rect(
                tooltipRect.x + Padding,
                tooltipRect.y + Padding,
                bodyW,
                headingSize.y);
            GUI.Label(headingRect, headingText, headingStyle);

            if (bodyH > 0)
            {
                // Thin separator line
                float sepY = headingRect.y + headingRect.height + gapH;
                Color prevColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.25f);
                GUI.DrawTexture(new Rect(tooltipRect.x + Padding, sepY, bodyW, separatorH), Texture2D.whiteTexture);
                GUI.color = prevColor;

                // Body
                Rect bodyRect = new Rect(
                    tooltipRect.x + Padding,
                    sepY + separatorH + gapH,
                    bodyW,
                    bodyH);
                GUI.Label(bodyRect, bodyText, bodyStyle);
            }
        }
    }
}
