using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MaskEffect
{
    public class BattleManager : NetworkBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private float roundTimeLimit = 45f;
        [SerializeField] private int masksPerSide = 2;
        [SerializeField] private bool autoStartCombat = false;

        [Header("References")]
        [SerializeField] private MechSpawner spawner;
        [SerializeField] private SimpleFlatGrid grid;
        [SerializeField] private MaskData[] availableMasks;
        [SerializeField] private GameObject maskPrefab;

        [Header("State")]
        [SyncVar(hook = nameof(OnCurrentStateChanged))]
        public BattleState currentState;
        [SyncVar] public int roundNumber;
        public float roundTimer;

        // Round-end stats synced to clients for GameOverUI
        [SyncVar] public Team lastRoundWinner;
        [SyncVar] public int lastPlayerAlive;
        [SyncVar] public int lastEnemyAlive;

        // Readiness tracking for multiplayer mask assignment
        [SyncVar] public bool playerSideReady;
        [SyncVar] public bool enemySideReady;

        public List<MechController> allMechs = new List<MechController>();
        public List<MechController> playerMechs = new List<MechController>();
        public List<MechController> enemyMechs = new List<MechController>();

        [SyncVar] private int playerMasksAssigned;
        [SyncVar] private int enemyMasksAssigned;

        // Public accessors
        public int PlayerMasksAssigned => playerMasksAssigned;
        public int EnemyMasksAssigned => enemyMasksAssigned;
        public int MasksPerSide => masksPerSide;
        public MaskData[] AvailableMasks => availableMasks;
        public SimpleFlatGrid Grid => grid;

        /// <summary>
        /// Set by LobbyUIController before scene transition.
        /// true = singleplayer (AI controls enemy side),
        /// false = multiplayer (human enemy player).
        /// </summary>
        public static bool AIControlsEnemySide = true;

        /// <summary>
        /// True when there are 2+ network connections (real multiplayer, not SP via localhost).
        /// </summary>
        public bool IsMultiplayerMatch =>
            NetworkServer.active && NetworkServer.connections.Count >= 2;

        // Events (fire locally; clients receive via SyncVar hooks and RPCs)
        public event Action<BattleState> OnStateChanged;
        public event Action<MechController> OnMechDied;
        public event Action<Team> OnRoundEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            AutoWireReferences();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            spawner.Initialize(grid);
            grid.GenerateVisualTiles();
            EnsureUIComponents();

            StartNewRound();

            if (autoStartCombat)
                ForceStartCombat();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // On pure clients (not host), ensure UI components exist
            if (!isServer)
                EnsureUIComponents();
        }

        /// <summary>
        /// Ensures all required UI components exist in the scene.
        /// Needed because the multiplayer scene may not have them pre-placed.
        /// </summary>
        private void EnsureUIComponents()
        {
            if (FindFirstObjectByType<MaskAssignmentManager>() == null)
                gameObject.AddComponent<MaskAssignmentManager>();
            if (FindFirstObjectByType<MaskPanelUI>() == null)
                gameObject.AddComponent<MaskPanelUI>();
            if (FindFirstObjectByType<GameOverUI>() == null)
                gameObject.AddComponent<GameOverUI>();
        }

        private void Start()
        {
            // In singleplayer (no NetworkManager), OnStartServer() never fires.
            // Run initialization here instead.
            if (NetworkHelper.IsOffline)
            {
                spawner.Initialize(grid);
                grid.GenerateVisualTiles();
                StartNewRound();

                if (autoStartCombat)
                    ForceStartCombat();
            }
        }

        private void AutoWireReferences()
        {
            if (spawner == null) spawner = GetComponent<MechSpawner>();
            if (grid == null) grid = GetComponent<SimpleFlatGrid>();
            if (availableMasks == null || availableMasks.Length == 0)
                availableMasks = Resources.LoadAll<MaskData>("Data/Masks");
        }

        // --- SyncVar Hook ---

        private void OnCurrentStateChanged(BattleState oldState, BattleState newState)
        {
            // Fires on clients when server changes currentState
            OnStateChanged?.Invoke(newState);
        }

        // --- State Management ---

        private void SetState(BattleState newState)
        {
            currentState = newState;
            // SyncVar hook fires on clients automatically.
            // On server, hook does NOT fire, so invoke event manually.
            OnStateChanged?.Invoke(newState);
        }

        // --- Round Lifecycle ---

        public void StartNewRound()
        {
            if (!NetworkHelper.IsServerOrOffline) return;

            roundNumber++;
            roundTimer = roundTimeLimit;
            playerMasksAssigned = 0;
            enemyMasksAssigned = 0;
            playerSideReady = false;
            enemySideReady = false;

            // Clear previous round
            if (allMechs.Count > 0)
            {
                // Destroy mask objects before mechs (they have separate NetworkIdentity)
                for (int i = 0; i < allMechs.Count; i++)
                {
                    if (allMechs[i].networkMask != null)
                    {
                        NetworkHelper.SmartDestroy(allMechs[i].networkMask.gameObject);
                        allMechs[i].networkMask = null;
                    }
                }

                spawner.ClearAllMechs(allMechs);
                allMechs.Clear();
                playerMechs.Clear();
                enemyMechs.Clear();
                grid.ClearAllOccupants();
            }

            // Spawn new mechs
            var (pMechs, eMechs) = spawner.SpawnRound();
            playerMechs = pMechs;
            enemyMechs = eMechs;

            allMechs.AddRange(playerMechs);
            allMechs.AddRange(enemyMechs);

            // Give all mechs the full list reference
            for (int i = 0; i < allMechs.Count; i++)
            {
                allMechs[i].SetAllMechsList(allMechs);
            }

            // Pre-assign enemy masks only in singleplayer (AI opponent).
            // autoCreatePlayer is the reliable MP indicator: false in SP, true in MP
            // (set by LobbyUIController before scene transition).
            bool expectsRemotePlayer = NetworkManager.singleton != null
                && NetworkManager.singleton.autoCreatePlayer;
            if (AIControlsEnemySide && !expectsRemotePlayer)
            {
                AIAssignMasks();
                enemySideReady = true;
            }

            SetState(BattleState.MaskAssignment);
        }

        // --- Mech Repositioning ---

        /// <summary>
        /// Mirror Command: client (or host) requests mech repositioning on the server.
        /// requiresAuthority=false because BattleManager is a scene object with no owner.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdRepositionMech(uint mechNetId, int fromTileIndex, int toTileIndex, NetworkConnectionToClient sender = null)
        {
            if (currentState != BattleState.MaskAssignment) return;

            // Resolve mech
            if (!NetworkServer.spawned.TryGetValue(mechNetId, out NetworkIdentity mechIdentity)) return;
            MechController mech = mechIdentity.GetComponent<MechController>();
            if (mech == null || !mech.isAlive) return;

            // Validate team: host (connectionId 0) = Team.Player, client = Team.Enemy
            bool isSenderHost = sender == null || sender.connectionId == 0;
            if (isSenderHost && mech.team != Team.Player) return;
            if (!isSenderHost && mech.team != Team.Enemy) return;

            // Validate target tile zone
            TileZone requiredZone = mech.team == Team.Player ? TileZone.Player : TileZone.Enemy;
            if (grid.GetTileZone(toTileIndex) != requiredZone) return;

            // Validate target tile is unoccupied (or same as origin)
            if (toTileIndex != fromTileIndex && grid.IsTileOccupied(toTileIndex)) return;

            // Server-side: update grid and position
            grid.ClearTile(fromTileIndex);
            grid.SetTileOccupant(toTileIndex, mech);
            mech.transform.position = grid.GetTileWorldPosition(toTileIndex);
        }

        // --- Mask Assignment ---

        public void AssignMaskToMech(MechController mech, MaskData mask)
        {
            if (mech == null || mask == null) return;
            if (mech.equippedMask != null) return;

            GameObject maskGO = Instantiate(maskPrefab);
            NetworkMask netMask = maskGO.GetComponent<NetworkMask>();
            netMask.InitializeOnServer(mask, mech, grid, allMechs);

            // Network spawn so all clients receive the mask object
            NetworkHelper.SpawnOrIgnore(maskGO);

            mech.EquipMask(netMask);

            // On the host, OnStartClient may be deferred until LateUpdate.
            // Trigger visual setup immediately so ring + tint appear in round 1.
            netMask.SetupClientSide();
        }

        /// <summary>
        /// Local entry point for mask assignment. In offline mode, assigns directly.
        /// In online mode, routes through CmdAssignMask.
        /// </summary>
        public void PlayerAssignMask(MechController mech, MaskData mask)
        {
            if (currentState != BattleState.MaskAssignment) return;
            if (mech == null || mask == null) return;
            if (mech.equippedMask != null) return;

            if (NetworkHelper.IsOffline)
            {
                // Singleplayer: direct assignment
                if (mech.team != Team.Player) return;
                AssignMaskToMech(mech, mask);
                playerMasksAssigned++;

                if (playerMasksAssigned >= masksPerSide)
                {
                    StartCombat();
                }
            }
            else
            {
                // Multiplayer: route through Command
                CmdAssignMask(mech.netId, $"Data/Masks/{mask.name}");
            }
        }

        /// <summary>
        /// Mirror Command: client (or host) requests mask assignment on the server.
        /// requiresAuthority=false because BattleManager is a scene object with no owner.
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdAssignMask(uint mechNetId, string maskDataPath, NetworkConnectionToClient sender = null)
        {
            if (currentState != BattleState.MaskAssignment) return;

            // Enforce per-side mask limit
            bool isSenderHost = sender == null || sender.connectionId == 0;
            if (isSenderHost && playerMasksAssigned >= masksPerSide) return;
            if (!isSenderHost && enemyMasksAssigned >= masksPerSide) return;

            // Resolve mech
            if (!NetworkServer.spawned.TryGetValue(mechNetId, out NetworkIdentity mechIdentity)) return;
            MechController mech = mechIdentity.GetComponent<MechController>();
            if (mech == null || !mech.isAlive || mech.equippedMask != null) return;

            // Resolve mask data
            MaskData mask = Resources.Load<MaskData>(maskDataPath);
            if (mask == null) return;

            // Validate team: host (connectionId 0) = Team.Player, client = Team.Enemy
            if (isSenderHost && mech.team != Team.Player) return;
            if (!isSenderHost && mech.team != Team.Enemy) return;

            AssignMaskToMech(mech, mask);

            if (mech.team == Team.Player)
            {
                playerMasksAssigned++;
                if (playerMasksAssigned >= masksPerSide)
                    playerSideReady = true;
            }
            else
            {
                enemyMasksAssigned++;
                if (enemyMasksAssigned >= masksPerSide)
                    enemySideReady = true;
            }

            // Start combat when both sides have finished assigning
            if (playerSideReady && enemySideReady)
            {
                StartCombat();
            }
        }

        public void OnMaskAssignmentComplete()
        {
            StartCombat();
        }

        // --- Combat ---

        public void StartCombat()
        {
            SetState(BattleState.Combat);

            // Trigger OnBattleStart for all mechs with abilities
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (allMechs[i].isAlive)
                    allMechs[i].OnBattleStart();
            }
        }

        private void Update()
        {
            if (currentState != BattleState.Combat) return;
            if (!NetworkHelper.IsServerOrOffline) return;

            float dt = Time.deltaTime;
            roundTimer -= dt;

            TickCombat(dt);
            CheckRoundEnd();
        }

        private void TickCombat(float dt)
        {
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (allMechs[i].isAlive)
                {
                    allMechs[i].UpdateCombat(dt);
                }
            }
        }

        private void CheckRoundEnd()
        {
            bool playerAlive = false;
            bool enemyAlive = false;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive) continue;
                if (allMechs[i].team == Team.Player) playerAlive = true;
                else enemyAlive = true;

                if (playerAlive && enemyAlive) break;
            }

            if (!playerAlive)
            {
                EndRound(Team.Enemy);
            }
            else if (!enemyAlive)
            {
                EndRound(Team.Player);
            }
            else if (roundTimer <= 0f)
            {
                EndRound(DetermineTimerWinner());
            }
        }

        private Team DetermineTimerWinner()
        {
            int playerHP = 0;
            int enemyHP = 0;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive) continue;
                if (allMechs[i].team == Team.Player)
                    playerHP += allMechs[i].currentHP;
                else
                    enemyHP += allMechs[i].currentHP;
            }

            return playerHP >= enemyHP ? Team.Player : Team.Enemy;
        }

        private void EndRound(Team winner)
        {
            // Compute stats while allMechs is available (server only)
            int pAlive = 0, eAlive = 0;
            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive) continue;
                if (allMechs[i].team == Team.Player) pAlive++;
                else eAlive++;
            }

            lastRoundWinner = winner;
            lastPlayerAlive = pAlive;
            lastEnemyAlive = eAlive;

            SetState(BattleState.RoundEnd);
            Debug.Log($"Round {roundNumber} ended. Winner: {winner}");
            OnRoundEnded?.Invoke(winner);

            // Notify clients about round end
            if (NetworkServer.active)
                RpcRoundEnded(winner);
        }

        [ClientRpc]
        private void RpcRoundEnded(Team winner)
        {
            // Host already fired event locally in EndRound()
            if (isServer) return;
            OnRoundEnded?.Invoke(winner);
        }

        // --- AI ---

        private void AIAssignMasks()
        {
            // Defensive: ensure mask data is loaded (scene serialization can lose references)
            if (availableMasks == null || availableMasks.Length == 0 || availableMasks[0] == null)
                availableMasks = Resources.LoadAll<MaskData>("Data/Masks");

            Debug.Log($"[BattleManager] AIAssignMasks: enemies={enemyMechs.Count}, masks={availableMasks?.Length ?? 0}, maskPrefab={(maskPrefab != null ? "OK" : "NULL")}");
            RandomAssignMasks(enemyMechs);
        }

        private void RandomAssignMasks(List<MechController> mechs)
        {
            if (availableMasks == null || availableMasks.Length == 0) return;

            List<MechController> unmasked = new List<MechController>();
            for (int i = 0; i < mechs.Count; i++)
            {
                if (mechs[i].isAlive && mechs[i].equippedMask == null)
                    unmasked.Add(mechs[i]);
            }

            // Shuffle
            for (int i = unmasked.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (unmasked[i], unmasked[j]) = (unmasked[j], unmasked[i]);
            }

            int assignCount = Mathf.Min(masksPerSide, unmasked.Count);
            for (int i = 0; i < assignCount; i++)
            {
                MaskData randomMask = availableMasks[UnityEngine.Random.Range(0, availableMasks.Length)];
                AssignMaskToMech(unmasked[i], randomMask);
            }
        }

        // --- Utility ---

        public void SkipToNextRound()
        {
            StartNewRound();
        }

        public void ForceStartCombat()
        {
            // Auto-assign random masks to player side for testing
            RandomAssignMasks(playerMechs);
            OnMaskAssignmentComplete();
        }
    }
}
