using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private float roundTimeLimit = 45f;
        [SerializeField] private int masksPerSide = 2;

        [Header("References")]
        [SerializeField] private MechSpawner spawner;
        [SerializeField] private SimpleFlatGrid grid;
        [SerializeField] private MaskData[] availableMasks;

        [Header("State")]
        public BattleState currentState;
        public int roundNumber;
        public float roundTimer;

        public List<MechController> allMechs = new List<MechController>();
        public List<MechController> playerMechs = new List<MechController>();
        public List<MechController> enemyMechs = new List<MechController>();

        // Events
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

        private void Start()
        {
            spawner.Initialize(grid);
            StartNewRound();
            // Auto-start combat for testing (skip mask assignment UI)
            ForceStartCombat();
        }

        private void AutoWireReferences()
        {
            if (spawner == null) spawner = GetComponent<MechSpawner>();
            if (grid == null) grid = GetComponent<SimpleFlatGrid>();
            if (availableMasks == null || availableMasks.Length == 0)
                availableMasks = Resources.LoadAll<MaskData>("Data/Masks");
        }

        public void StartNewRound()
        {
            roundNumber++;
            roundTimer = roundTimeLimit;

            // Clear previous round
            if (allMechs.Count > 0)
            {
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

            SetState(BattleState.MaskAssignment);
        }

        public void AssignMaskToMech(MechController mech, MaskData mask)
        {
            if (mech == null || mask == null) return;
            if (mech.equippedMask != null) return; // already has a mask

            mech.EquipMask(mask);
        }

        public void OnMaskAssignmentComplete()
        {
            // AI assigns masks to enemy team
            AIAssignMasks();

            StartCombat();
        }

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
            SetState(BattleState.RoundEnd);
            Debug.Log($"Round {roundNumber} ended. Winner: {winner}");
            OnRoundEnded?.Invoke(winner);
        }

        private void AIAssignMasks()
        {
            if (availableMasks == null || availableMasks.Length == 0) return;

            // Simple random AI: assign masks to random enemy mechs
            List<MechController> unmasked = new List<MechController>();
            for (int i = 0; i < enemyMechs.Count; i++)
            {
                if (enemyMechs[i].isAlive && enemyMechs[i].equippedMask == null)
                    unmasked.Add(enemyMechs[i]);
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

        private void SetState(BattleState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        // Public utility for UI/testing
        public void SkipToNextRound()
        {
            StartNewRound();
        }

        public void ForceStartCombat()
        {
            OnMaskAssignmentComplete();
        }
    }
}