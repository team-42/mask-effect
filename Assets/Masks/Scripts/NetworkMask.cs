using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MaskEffect
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkMask : NetworkBehaviour
    {
        [SyncVar] public MaskType maskType;
        [SyncVar] public string maskDataPath;
        [SyncVar] public uint ownerMechNetId;

        [System.NonSerialized] public MaskData maskData;
        [System.NonSerialized] public MechController ownerMech;
        [System.NonSerialized] public IMaskAbility activeAbility;

        private GameObject _ringVisual;
        private GameObject _iconVisual; // New field for the icon visual

        /// <summary>
        /// Called on the server after instantiation but BEFORE NetworkServer.Spawn().
        /// Sets SyncVars so clients receive correct initial values.
        /// </summary>
        public void InitializeOnServer(MaskData data, MechController mech, IBattleGrid grid, List<MechController> allMechs)
        {
            maskData = data;
            ownerMech = mech;

            // Set SyncVars before Spawn so clients get them on first sync
            maskType = data.maskType;
            maskDataPath = $"Data/Masks/{data.name}";
            ownerMechNetId = NetworkHelper.IsOffline ? 0 : mech.netId;

            // Parent to mech so it moves with it
            transform.SetParent(mech.transform, false);
            transform.localPosition = new Vector3(0f, 0.05f, 0f);

            // Create ability (server-only logic)
            if (mech.chassisData == null)
            {
                Debug.LogError($"[NetworkMask] InitializeOnServer: mech.chassisData is null for mech {mech.mechId} team={mech.team}");
                return;
            }
            MaskAbilityData abilityData = data.GetAbilityForChassis(mech.chassisData.chassisType);
            if (abilityData != null)
            {
                activeAbility = MaskAbilityFactory.Create(abilityData.abilityClassId);
                if (activeAbility != null && allMechs != null)
                {
                    activeAbility.Initialize(mech, abilityData, grid, allMechs);
                }
            }
        }

        private void Start()
        {
            // Offline fallback: OnStartClient() never fires without NetworkManager
            if (NetworkHelper.IsOffline)
            {
                SetupClientSide();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SetupClientSide();
        }

        /// <summary>
        /// Ensures client-side visuals are created. Safe to call multiple times
        /// (ring creation and mech notification are idempotent).
        /// Called from OnStartClient, Start (offline), and explicitly by
        /// BattleManager after spawn to handle deferred OnStartClient on host.
        /// </summary>
        public void SetupClientSide()
        {
            // Load mask data from Resources if not already set (server sets it directly)
            if (maskData == null && !string.IsNullOrEmpty(maskDataPath))
            {
                maskData = Resources.Load<MaskData>(maskDataPath);
                if (maskData == null)
                {
                    Debug.LogWarning($"[NetworkMask] Failed to load mask data at '{maskDataPath}'");
                    return;
                }
            }

            // Resolve owner mech
            if (ownerMech == null)
            {
                if (NetworkHelper.IsOffline)
                {
                    // In offline mode, we're parented to the mech already
                    ownerMech = GetComponentInParent<MechController>();
                }
                else if (ownerMechNetId != 0 && NetworkClient.spawned.TryGetValue(ownerMechNetId, out NetworkIdentity mechIdentity))
                {
                    ownerMech = mechIdentity.GetComponent<MechController>();
                }
            }

            if (ownerMech == null)
            {
                Debug.LogWarning("[NetworkMask] Could not resolve owner mech");
                return;
            }

            // Parent to mech (in multiplayer, transform parent is not synced)
            if (transform.parent != ownerMech.transform)
            {
                transform.SetParent(ownerMech.transform, false);
                transform.localPosition = new Vector3(0f, 0.05f, 0f);
            }

            // Create ring visual
            CreateRingVisual();
            // Create hovering icon visual
            CreateHoveringIconVisual();

            // Notify the mech about this mask (for stat recalc, tint, etc.)
            ownerMech.ApplyMaskFromNetwork(this);

            // Notify FogOfWarManager so mask visuals are hidden if fog is active
            if (FogOfWarManager.Instance != null)
                FogOfWarManager.Instance.OnMaskVisualsCreated(ownerMech, this);
        }

        private void CreateRingVisual()
        {
            if (_ringVisual != null) return;
            if (ownerMech == null || ownerMech.chassisData == null || maskData == null) return;

            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var col = indicator.GetComponent<Collider>();
            if (col != null) Destroy(col);

            indicator.name = MechSpawner.MASK_RING_NAME;
            indicator.transform.SetParent(transform, false);
            indicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            float ringSize = ownerMech.chassisData.indicatorRadius * 3.5f;
            indicator.transform.localScale = new Vector3(ringSize, ringSize, 1f);
            indicator.transform.localPosition = Vector3.zero;

            Material ringMat = Resources.Load<Material>("Materials/MaskRing");
            var renderer = indicator.GetComponent<Renderer>();
            if (renderer != null && ringMat != null)
            {
                renderer.material = new Material(ringMat);
                renderer.material.SetColor("_Color", maskData.maskTint);
            }

            _ringVisual = indicator;
        }

        private void CreateHoveringIconVisual()
        {
            if (_iconVisual != null) return;
            if (ownerMech == null || maskData == null || maskData.maskIcon == null) return;

            GameObject iconGO = new GameObject("MaskIcon");
            iconGO.transform.SetParent(transform, false);
            // Position slightly above the mech, higher than the ring
            iconGO.transform.localPosition = new Vector3(0f, 1.25f, 0f); // Hovering higher above the models
            iconGO.transform.localScale = new Vector3(0.175f, 0.175f, 0.175f); // Further decreased size by 50%

            SpriteRenderer spriteRenderer = iconGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = maskData.maskIcon;
            spriteRenderer.sortingOrder = 10; // Ensure it renders above other elements

            // Add HoverBob for animation
            HoverBob bob = iconGO.AddComponent<HoverBob>();
            bob.amplitude = 0.1f;
            bob.frequency = 1.5f;

            // Add Billboard component to face the camera
            iconGO.AddComponent<Billboard>();

            _iconVisual = iconGO;
        }

        private void OnDestroy()
        {
            if (activeAbility != null)
                activeAbility.Cleanup();
        }
    }
}
