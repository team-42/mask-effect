using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MaskEffect
{
    /// <summary>
    /// Manages Fog of War: hides enemy mechs during MaskAssignment phase,
    /// reveals them with animation when Combat starts.
    /// Purely client-side — no networking needed.
    /// </summary>
    public class FogOfWarManager : MonoBehaviour
    {
        public static FogOfWarManager Instance { get; private set; }

        private bool fogActive;
        private GameObject fogWallInstance;
        private Material fogWallMaterial;
        private Coroutine revealCoroutine;
        private Coroutine scanCoroutine;

        // Track hidden renderers/colliders per mech for clean reveal
        private Dictionary<MechController, List<Renderer>> hiddenRenderers = new Dictionary<MechController, List<Renderer>>();
        private Dictionary<MechController, List<Collider>> hiddenColliders = new Dictionary<MechController, List<Collider>>();
        // Track which mechs we've already processed (even if they had no renderers yet)
        private HashSet<MechController> trackedEnemyMechs = new HashSet<MechController>();

        // Perspective: which team is "mine" vs "enemy"
        private Team MyTeam
        {
            get
            {
                if (NetworkHelper.IsOffline) return Team.Player;
                if (NetworkServer.active) return Team.Player;
                return Team.Enemy;
            }
        }

        private Team EnemyTeam => MyTeam == Team.Player ? Team.Enemy : Team.Player;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            // Subscribe immediately — BattleManager.Instance is already set
            // (BattleManager.Awake sets it, and we're created after that in EnsureUIComponents)
            if (BattleManager.Instance != null)
                BattleManager.Instance.OnStateChanged += OnBattleStateChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (BattleManager.Instance != null)
                BattleManager.Instance.OnStateChanged -= OnBattleStateChanged;
        }

        private void Start()
        {
            // Catch case where state was already set before we subscribed
            if (BattleManager.Instance != null &&
                BattleManager.Instance.currentState == BattleState.MaskAssignment)
            {
                ActivateFog();
            }
        }

        private void OnBattleStateChanged(BattleState newState)
        {
            switch (newState)
            {
                case BattleState.MaskAssignment:
                    ActivateFog();
                    break;
                case BattleState.Combat:
                    DeactivateFog();
                    break;
                case BattleState.Setup:
                case BattleState.RoundEnd:
                    // Safety cleanup — instant reveal, no animation
                    if (fogActive)
                        ForceRevealAll();
                    break;
            }
        }

        // ---- Fog Activation ----

        private void ActivateFog()
        {
            if (fogActive) return;

            // Stop any ongoing reveal
            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
                revealCoroutine = null;
            }

            hiddenRenderers.Clear();
            hiddenColliders.Clear();
            trackedEnemyMechs.Clear();

            // Find ALL enemy mechs in the scene (not just from allMechs list,
            // which is only populated on the server)
            HideAllEnemyMechs();

            // Create fog area visual
            CreateFogWall();

            fogActive = true;

            // Start periodic scan to catch mechs that spawn or get visuals after fog activation
            if (scanCoroutine != null)
                StopCoroutine(scanCoroutine);
            scanCoroutine = StartCoroutine(ScanForNewEnemyMechs());
        }

        /// <summary>
        /// Finds and hides all enemy mechs currently in the scene.
        /// Uses FindObjectsByType to work on both server and client
        /// (BattleManager.allMechs is only populated on the server).
        /// </summary>
        private void HideAllEnemyMechs()
        {
            MechController[] allMechs = FindObjectsByType<MechController>(FindObjectsSortMode.None);
            foreach (var mech in allMechs)
            {
                if (mech != null && mech.team == EnemyTeam && !trackedEnemyMechs.Contains(mech))
                {
                    trackedEnemyMechs.Add(mech);
                    HideMech(mech);
                }
            }
        }

        /// <summary>
        /// Periodically scans for new enemy mechs or newly created renderers.
        /// Handles: late-spawned mechs on client, SetupVisuals() running after fog activation.
        /// Runs every 0.2s while fog is active.
        /// </summary>
        private IEnumerator ScanForNewEnemyMechs()
        {
            while (fogActive)
            {
                yield return new WaitForSeconds(0.2f);

                if (!fogActive) break;

                // Find any new enemy mechs
                MechController[] allMechs = FindObjectsByType<MechController>(FindObjectsSortMode.None);
                foreach (var mech in allMechs)
                {
                    if (mech == null || mech.team != EnemyTeam) continue;

                    if (!trackedEnemyMechs.Contains(mech))
                    {
                        // Brand new mech — hide everything
                        trackedEnemyMechs.Add(mech);
                        HideMech(mech);
                    }
                    else
                    {
                        // Already tracked — check for new renderers (SetupVisuals ran late)
                        HideNewRenderers(mech);
                    }
                }
            }
            scanCoroutine = null;
        }

        private void HideMech(MechController mech)
        {
            // Hide all renderers in the mech hierarchy
            Renderer[] renderers = mech.GetComponentsInChildren<Renderer>(true);
            var rendList = new List<Renderer>(renderers.Length);
            foreach (var r in renderers)
            {
                if (r.enabled)
                {
                    r.enabled = false;
                    rendList.Add(r);
                }
            }
            hiddenRenderers[mech] = rendList;

            // Disable colliders to prevent raycast interaction
            Collider[] colliders = mech.GetComponentsInChildren<Collider>(true);
            var colList = new List<Collider>(colliders.Length);
            foreach (var c in colliders)
            {
                if (c.enabled)
                {
                    c.enabled = false;
                    colList.Add(c);
                }
            }
            hiddenColliders[mech] = colList;
        }

        /// <summary>
        /// For an already-tracked mech, find any renderers/colliders that were
        /// created after the initial HideMech call (e.g. SetupVisuals ran late).
        /// </summary>
        private void HideNewRenderers(MechController mech)
        {
            if (!hiddenRenderers.TryGetValue(mech, out var knownRenderers))
                knownRenderers = new List<Renderer>();

            Renderer[] currentRenderers = mech.GetComponentsInChildren<Renderer>(true);
            foreach (var r in currentRenderers)
            {
                if (r.enabled && !knownRenderers.Contains(r))
                {
                    r.enabled = false;
                    knownRenderers.Add(r);
                }
            }
            hiddenRenderers[mech] = knownRenderers;

            if (!hiddenColliders.TryGetValue(mech, out var knownColliders))
                knownColliders = new List<Collider>();

            Collider[] currentColliders = mech.GetComponentsInChildren<Collider>(true);
            foreach (var c in currentColliders)
            {
                if (c.enabled && !knownColliders.Contains(c))
                {
                    c.enabled = false;
                    knownColliders.Add(c);
                }
            }
            hiddenColliders[mech] = knownColliders;
        }

        /// <summary>
        /// Called by NetworkMask.SetupClientSide() when mask visuals are created.
        /// If fog is active and the mech belongs to the enemy team, hide the new visuals.
        /// </summary>
        public void OnMaskVisualsCreated(MechController mech, NetworkMask mask)
        {
            if (!fogActive) return;
            if (mech == null || mask == null) return;
            if (mech.team != EnemyTeam) return;

            // Hide the newly created mask renderers
            Renderer[] maskRenderers = mask.GetComponentsInChildren<Renderer>(true);
            foreach (var r in maskRenderers)
                r.enabled = false;

            // Track them for reveal
            if (!hiddenRenderers.ContainsKey(mech))
                hiddenRenderers[mech] = new List<Renderer>();
            hiddenRenderers[mech].AddRange(maskRenderers);
        }

        // ---- Fog Area Visual ----

        private void CreateFogWall()
        {
            if (fogWallInstance != null) return;

            SimpleFlatGrid grid = BattleManager.Instance != null ? BattleManager.Instance.Grid : null;
            if (grid == null) return;

            // Calculate enemy zone bounds in world space
            int playerEndX = grid.GridWidth / 4;               // 7
            int enemyStartX = grid.GridWidth - grid.GridWidth / 4; // 21

            float zoneStartX, zoneEndX;
            if (MyTeam == Team.Player)
            {
                // Hide enemy zone (right side): tile columns 21..28
                zoneStartX = grid.GridOrigin.x + enemyStartX * grid.TileSize; // 7.0
                zoneEndX = grid.GridOrigin.x + grid.GridWidth * grid.TileSize; // 14.0
            }
            else
            {
                // Client hides player zone (left side): tile columns 0..7
                zoneStartX = grid.GridOrigin.x;                                // -14.0
                zoneEndX = grid.GridOrigin.x + playerEndX * grid.TileSize;     // -7.0
            }

            float zoneStartZ = grid.GridOrigin.z;                              // -6.0
            float zoneEndZ = grid.GridOrigin.z + grid.GridHeight * grid.TileSize; // 6.0

            float zoneSizeX = zoneEndX - zoneStartX;   // 7
            float zoneSizeZ = zoneEndZ - zoneStartZ;   // 12
            float centerX = (zoneStartX + zoneEndX) * 0.5f;
            float centerZ = (zoneStartZ + zoneEndZ) * 0.5f;

            // Add padding so the fog extends slightly beyond the zone edges
            float padding = 1.5f;

            // Create a horizontal quad facing upward
            fogWallInstance = GameObject.CreatePrimitive(PrimitiveType.Quad);
            fogWallInstance.name = "FogArea";

            // Remove collider
            Collider col = fogWallInstance.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // Orient: face upward, positioned just above ground level
            fogWallInstance.transform.rotation = Quaternion.Euler(90, 0, 0);
            fogWallInstance.transform.position = new Vector3(centerX, 1.5f, centerZ);
            fogWallInstance.transform.localScale = new Vector3(zoneSizeX + padding, zoneSizeZ + padding, 1f);

            // Apply fog material
            Material fogMat = Resources.Load<Material>("Materials/FogWall");
            if (fogMat != null)
            {
                fogWallMaterial = new Material(fogMat);
                fogWallMaterial.SetFloat("_Dissolve", 0f);
            }
            else
            {
                fogWallMaterial = new Material(Shader.Find("MaskEffect/FogWall"));
                if (fogWallMaterial.shader == null || fogWallMaterial.shader.name == "Hidden/InternalErrorShader")
                {
                    fogWallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    fogWallMaterial.SetFloat("_Surface", 1);
                    fogWallMaterial.color = new Color(0.02f, 0.02f, 0.05f, 0.9f);
                }
            }
            fogWallInstance.GetComponent<Renderer>().material = fogWallMaterial;
        }

        // ---- Fog Deactivation (with reveal animation) ----

        private void DeactivateFog()
        {
            if (!fogActive) return;

            // Stop scanning
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
            }

            revealCoroutine = StartCoroutine(RevealSequence());
        }

        private IEnumerator RevealSequence()
        {
            // Phase 1: Dissolve the fog area
            if (fogWallInstance != null && fogWallMaterial != null)
            {
                float duration = 0.8f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    fogWallMaterial.SetFloat("_Dissolve", t);
                    yield return null;
                }
            }

            // Destroy fog area
            if (fogWallInstance != null)
            {
                Destroy(fogWallInstance);
                fogWallInstance = null;
            }
            if (fogWallMaterial != null)
            {
                Destroy(fogWallMaterial);
                fogWallMaterial = null;
            }

            // Phase 2: Staggered mech reveal
            var mechsToReveal = new List<MechController>(hiddenRenderers.Keys);
            float perMechDelay = 0.08f;

            foreach (var mech in mechsToReveal)
            {
                if (mech == null) continue;

                // Re-enable colliders
                if (hiddenColliders.TryGetValue(mech, out var colliders))
                {
                    foreach (var c in colliders)
                        if (c != null) c.enabled = true;
                }

                // Re-enable renderers
                if (hiddenRenderers.TryGetValue(mech, out var renderers))
                {
                    foreach (var r in renderers)
                        if (r != null) r.enabled = true;
                }

                yield return new WaitForSeconds(perMechDelay);
            }

            // Cleanup
            hiddenRenderers.Clear();
            hiddenColliders.Clear();
            trackedEnemyMechs.Clear();
            fogActive = false;
            revealCoroutine = null;
        }

        /// <summary>
        /// Instant reveal without animation (used for safety cleanup on unexpected state changes).
        /// </summary>
        private void ForceRevealAll()
        {
            if (revealCoroutine != null)
            {
                StopCoroutine(revealCoroutine);
                revealCoroutine = null;
            }
            if (scanCoroutine != null)
            {
                StopCoroutine(scanCoroutine);
                scanCoroutine = null;
            }

            // Re-enable all hidden renderers
            foreach (var kvp in hiddenRenderers)
            {
                if (kvp.Key == null) continue;
                foreach (var r in kvp.Value)
                    if (r != null) r.enabled = true;
            }

            // Re-enable all hidden colliders
            foreach (var kvp in hiddenColliders)
            {
                if (kvp.Key == null) continue;
                foreach (var c in kvp.Value)
                    if (c != null) c.enabled = true;
            }

            hiddenRenderers.Clear();
            hiddenColliders.Clear();
            trackedEnemyMechs.Clear();

            // Destroy fog area
            if (fogWallInstance != null)
            {
                Destroy(fogWallInstance);
                fogWallInstance = null;
            }
            if (fogWallMaterial != null)
            {
                Destroy(fogWallMaterial);
                fogWallMaterial = null;
            }

            fogActive = false;
        }
    }
}
