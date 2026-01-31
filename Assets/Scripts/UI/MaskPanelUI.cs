using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class MaskPanelUI : MonoBehaviour
    {
        [SerializeField] private MaskAssignmentManager assignmentManager;

        private List<MaskSlotEntry> slots = new List<MaskSlotEntry>();
        private int masksUsed;
        private int totalMasks;
        private bool visible;
        private Rect panelRect;

        private struct MaskSlotEntry
        {
            public MaskData mask;
            public bool used;
        }

        private void Start()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnStateChanged += OnBattleStateChanged;

            if (assignmentManager == null)
                assignmentManager = FindFirstObjectByType<MaskAssignmentManager>();

            if (BattleManager.Instance != null && BattleManager.Instance.currentState == BattleState.MaskAssignment)
                ShowPanel();
        }

        private void OnDestroy()
        {
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnStateChanged -= OnBattleStateChanged;
        }

        private void OnBattleStateChanged(BattleState state)
        {
            if (state == BattleState.MaskAssignment)
                ShowPanel();
            else
                HidePanel();
        }

        private void ShowPanel()
        {
            visible = true;
            slots.Clear();
            masksUsed = 0;
            totalMasks = BattleManager.Instance.MasksPerSide;
            MaskData[] available = BattleManager.Instance.AvailableMasks;

            if (available == null || available.Length == 0) return;

            for (int i = 0; i < totalMasks; i++)
            {
                MaskData mask = available[Random.Range(0, available.Length)];
                slots.Add(new MaskSlotEntry { mask = mask, used = false });
            }
        }

        private void HidePanel()
        {
            visible = false;
        }

        public void MarkSlotUsed(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            var entry = slots[slotIndex];
            entry.used = true;
            slots[slotIndex] = entry;
            masksUsed++;
        }

        public bool IsMouseOverPanel()
        {
            if (!visible) return false;
            Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            return panelRect.Contains(mousePos);
        }

        private void OnGUI()
        {
            if (!visible) return;

            float panelWidth = 210f;
            float slotHeight = 70f;
            float panelHeight = 70f + slots.Count * (slotHeight + 5f);
            float panelX = 10f;
            float panelY = (Screen.height - panelHeight) * 0.5f;

            panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Panel background
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(panelRect);
            GUILayout.Space(10f);

            // Header
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = Color.white;
            GUILayout.Label($"Masken ({masksUsed}/{totalMasks})", headerStyle);

            GUILayout.Space(8f);

            for (int i = 0; i < slots.Count; i++)
            {
                DrawSlot(i);
                GUILayout.Space(3f);
            }

            GUILayout.Space(5f);

            // Instructions
            GUIStyle instrStyle = new GUIStyle(GUI.skin.label);
            instrStyle.fontSize = 11;
            instrStyle.alignment = TextAnchor.MiddleCenter;
            instrStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            instrStyle.wordWrap = true;
            GUILayout.Label("Klicke Maske, dann Mech", instrStyle);

            GUILayout.EndArea();
        }

        private void DrawSlot(int index)
        {
            MaskSlotEntry slot = slots[index];

            bool wasEnabled = GUI.enabled;
            GUI.enabled = !slot.used;

            // Create a colored button with the mask name
            Color prevBg = GUI.backgroundColor;
            Color tint = slot.used ? Color.gray : slot.mask.maskTint;
            GUI.backgroundColor = tint;

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.normal.textColor = Color.white;
            btnStyle.hover.textColor = Color.white;
            btnStyle.active.textColor = Color.white;
            btnStyle.padding = new RectOffset(10, 10, 10, 10);

            string label = slot.used ? $"[{slot.mask.maskName}] (vergeben)" : slot.mask.maskName;

            if (GUILayout.Button(label, btnStyle, GUILayout.Height(55f)))
            {
                if (!slot.used && assignmentManager != null)
                    assignmentManager.StartCarryingMask(slot.mask, index);
            }

            GUI.backgroundColor = prevBg;
            GUI.enabled = wasEnabled;
        }
    }
}
