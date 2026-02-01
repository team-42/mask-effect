using UnityEngine;
using UnityEngine.UIElements;

namespace MaskEffect
{
    public class HealthBarUIController : MonoBehaviour
    {
        public UIDocument uiDocument;
        private VisualElement healthBarFill;
        private VisualElement maskEffectPlaceholder;

        private void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                healthBarFill = uiDocument.rootVisualElement.Q<VisualElement>("health-bar-fill");
                maskEffectPlaceholder = uiDocument.rootVisualElement.Q<VisualElement>("mask-effect-placeholder");
            }
            else
            {
                Debug.LogError("UIDocument or its rootVisualElement is null in HealthBarUIController.");
            }
        }

        public void UpdateHealthBar(int currentHP, int maxHP)
        {
            if (healthBarFill == null) return;

            float fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
            healthBarFill.style.width = new Length(fillAmount * 100, LengthUnit.Percent);
        }

        public void UpdateMaskEffectPlaceholder(bool hasMaskEffect)
        {
            if (maskEffectPlaceholder == null) return;

            maskEffectPlaceholder.style.display = hasMaskEffect ? DisplayStyle.Flex : DisplayStyle.None;
            // TODO: In the future, update this to show specific mask effect icons
        }
    }
}
