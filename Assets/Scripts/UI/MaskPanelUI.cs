using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MaskEffect
{
    public class MaskPanelUI : MonoBehaviour
    {
        [SerializeField] private MaskAssignmentManager assignmentManager;

        private List<MaskSlotEntry> slots = new List<MaskSlotEntry>();
        private int totalMasks;
        private bool visible;
        private Rect panelRect;

        // Singleton guard to prevent duplicate rendering
        public static MaskPanelUI Instance { get; private set; }

        private struct MaskSlotEntry
        {
            public MaskData mask;
            public bool used;
        }

        /// <summary>
        /// Returns the server-authoritative mask count for this player's side.
        /// Uses SyncVar-backed fields so the client always sees the real count.
        /// </summary>
        private int ServerMasksUsed
        {
            get
            {
                if (BattleManager.Instance == null) return 0;
                bool isClient = !NetworkHelper.IsOffline && !NetworkServer.active;
                return isClient
                    ? BattleManager.Instance.EnemyMasksAssigned
                    : BattleManager.Instance.PlayerMasksAssigned;
            }
        }

        /// <summary>
        /// True when the server says the mask limit is reached for this side.
        /// </summary>
        private bool AllMasksAssigned => ServerMasksUsed >= totalMasks;

        private void Start()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[MaskPanelUI] Duplicate instance found, destroying this one.");
                Destroy(this);
                return;
            }
            Instance = this;

            if (assignmentManager == null)
                assignmentManager = FindFirstObjectByType<MaskAssignmentManager>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (BattleManager.Instance == null) return;

            bool shouldShow = BattleManager.Instance.currentState == BattleState.MaskAssignment;

            if (shouldShow && !visible)
                ShowPanel();
            else if (!shouldShow && visible)
                HidePanel();
        }

        private void ShowPanel()
        {
            visible = true;
            slots.Clear();
            totalMasks = BattleManager.Instance.MasksPerSide;

            MaskData[] available = BattleManager.Instance.AvailableMasks;
            if (available == null || available.Length == 0)
                available = Resources.LoadAll<MaskData>("Data/Masks");

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
        }

        public void UnmarkSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            var entry = slots[slotIndex];
            entry.used = false;
            slots[slotIndex] = entry;
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

            // Host/offline: left side. Client: right side.
            bool isClient = !NetworkHelper.IsOffline && !NetworkServer.active;
            float panelX = isClient ? (Screen.width - panelWidth - 10f) : 10f;
            float panelY = (Screen.height - panelHeight) * 0.5f;

            panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Panel background
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(panelRect);
            GUILayout.Space(10f);

            // Header — use server-synced count for accuracy
            int displayCount = ServerMasksUsed;
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.normal.textColor = Color.white;
            GUILayout.Label($"Masken ({displayCount}/{totalMasks})", headerStyle);

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

            // Slot is blocked if locally marked OR server says all masks assigned
            bool blocked = slot.used || AllMasksAssigned;

            bool wasEnabled = GUI.enabled;
            GUI.enabled = !blocked;

            // Create a colored button with the mask name
            Color prevBg = GUI.backgroundColor;
            Color tint = blocked ? Color.gray : slot.mask.maskTint;
            GUI.backgroundColor = tint;

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
            btnStyle.fontSize = 14;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            btnStyle.normal.textColor = Color.white;
            btnStyle.hover.textColor = Color.white;
            btnStyle.active.textColor = Color.white;
            btnStyle.padding = new RectOffset(10, 10, 10, 10);

            string label = slot.used
                ? $"[{slot.mask.maskName}] (vergeben)"
                : slot.mask.maskName;

            if (GUILayout.Button(label, btnStyle, GUILayout.Height(55f)))
            {
                if (!blocked)
                {
                    // Mark slot used immediately so it can't be clicked again
                    MarkSlotUsed(index);

                    if (assignmentManager == null)
                        assignmentManager = FindFirstObjectByType<MaskAssignmentManager>();
                    if (assignmentManager != null)
                        assignmentManager.StartCarryingMask(slot.mask, index);
                }
            }

            GUI.backgroundColor = prevBg;
            GUI.enabled = wasEnabled;
        }
    }
}
