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
        [SerializeField] private bool autoStartCombat = true;

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

        private int playerMasksAssigned;

        // Public accessors
        public int PlayerMasksAssigned => playerMasksAssigned;
        public int MasksPerSide => masksPerSide;
        public MaskData[] AvailableMasks => availableMasks;
        public SimpleFlatGrid Grid => grid;

        // Events
        public event Action<BattleState> OnStateChanged;
        public event Action<MechController> OnMechDied;
        public event Action<Team> OnRoundEnded;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            spawner.Initialize(grid);
            StartNewRound();

            if (autoStartCombat)
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
            playerMasksAssigned = 0;

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

            // Pre-assign enemy masks randomly
            AIAssignMasks();

            SetState(BattleState.MaskAssignment);
        }

        public void AssignMaskToMech(MechController mech, MaskData mask)
        {
            if (mech == null || mask == null) return;
            if (mech.equippedMask != null) return; // already has a mask

            mech.EquipMask(mask);
        }

        public void PlayerAssignMask(MechController mech, MaskData mask)
        {
            if (currentState != BattleState.MaskAssignment) return;
            if (mech == null || mask == null) return;
            if (mech.team != Team.Player) return;
            if (mech.equippedMask != null) return;

            AssignMaskToMech(mech, mask);
            playerMasksAssigned++;

            if (playerMasksAssigned >= masksPerSide)
            {
                StartCombat();
            }
        }

        public void OnMaskAssignmentComplete()
        {
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
            // Auto-assign random masks to player side for testing
            RandomAssignMasks(playerMechs);
            OnMaskAssignmentComplete();
        }
    }
}
