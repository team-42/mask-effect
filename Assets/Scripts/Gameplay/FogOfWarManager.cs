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

        // Track hidden renderers/colliders per mech for clean reveal
        private Dictionary<MechController, List<Renderer>> hiddenRenderers = new Dictionary<MechController, List<Renderer>>();
        private Dictionary<MechController, List<Collider>> hiddenColliders = new Dictionary<MechController, List<Collider>>();

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
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;

            if (BattleManager.Instance != null)
                BattleManager.Instance.OnStateChanged -= OnBattleStateChanged;
        }

        private void Start()
        {
            // Subscribe to battle state changes
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.OnStateChanged += OnBattleStateChanged;

                // If we start and state is already MaskAssignment (late join), activate fog
                if (BattleManager.Instance.currentState == BattleState.MaskAssignment)
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

            // Hide all enemy mechs
            if (BattleManager.Instance != null)
            {
                foreach (var mech in BattleManager.Instance.allMechs)
                {
                    if (mech != null && mech.team == EnemyTeam)
                        HideMech(mech);
                }
            }

            // Create fog wall visual
            CreateFogWall();

            fogActive = true;
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

        // ---- Fog Wall Visual ----

        private void CreateFogWall()
        {
            if (fogWallInstance != null) return;

            // Determine wall position based on perspective
            SimpleFlatGrid grid = BattleManager.Instance != null ? BattleManager.Instance.Grid : null;
            float wallX;
            if (grid != null)
            {
                int playerEndX = grid.GridWidth / 4;          // 7
                int enemyStartX = grid.GridWidth - grid.GridWidth / 4; // 21

                if (MyTeam == Team.Player)
                    wallX = grid.GridOrigin.x + enemyStartX * grid.TileSize; // 7.0
                else
                    wallX = grid.GridOrigin.x + playerEndX * grid.TileSize;  // -7.0

                float wallZ = grid.GridOrigin.z + (grid.GridHeight * 0.5f) * grid.TileSize; // 0.0
                float wallWidth = grid.GridHeight * grid.TileSize + 4f; // 16 units
                float wallHeight = 6f;

                // Create a quad
                fogWallInstance = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fogWallInstance.name = "FogWall";

                // Remove collider (we don't want it to block raycasts)
                Collider col = fogWallInstance.GetComponent<Collider>();
                if (col != null) Destroy(col);

                // Position and orient: face toward the player's side
                fogWallInstance.transform.position = new Vector3(wallX, wallHeight * 0.5f, wallZ);
                fogWallInstance.transform.localScale = new Vector3(wallWidth, wallHeight, 1f);

                // Rotate to face the correct direction
                if (MyTeam == Team.Player)
                    fogWallInstance.transform.rotation = Quaternion.Euler(0, -90, 0); // face left (toward player)
                else
                    fogWallInstance.transform.rotation = Quaternion.Euler(0, 90, 0); // face right (toward client's team)

                // Apply fog material
                Material fogMat = Resources.Load<Material>("Materials/FogWall");
                if (fogMat != null)
                {
                    fogWallMaterial = new Material(fogMat); // Instance so we can animate
                    fogWallMaterial.SetFloat("_Dissolve", 0f);
                }
                else
                {
                    // Fallback: create a semi-transparent material
                    fogWallMaterial = new Material(Shader.Find("MaskEffect/FogWall"));
                    if (fogWallMaterial.shader == null || fogWallMaterial.shader.name == "Hidden/InternalErrorShader")
                    {
                        fogWallMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        fogWallMaterial.SetFloat("_Surface", 1); // Transparent
                        fogWallMaterial.color = new Color(0.72f, 0.45f, 0.3f, 0.7f);
                    }
                }
                fogWallInstance.GetComponent<Renderer>().material = fogWallMaterial;
            }
        }

        // ---- Fog Deactivation (with reveal animation) ----

        private void DeactivateFog()
        {
            if (!fogActive) return;
            revealCoroutine = StartCoroutine(RevealSequence());
        }

        private IEnumerator RevealSequence()
        {
            // Phase 1: Dissolve the fog wall
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

            // Destroy fog wall
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

            // Destroy fog wall
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
