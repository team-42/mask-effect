using System.Collections.Generic;
using UnityEngine;
using Mirror; // Add Mirror namespace

namespace MaskEffect
{
    public class MechController : NetworkBehaviour // Change base class to NetworkBehaviour
    {
        [Header("Identity")]
        [SyncVar] public int mechId;
        [SyncVar] public Team team;
        // ChassisData and MaskData are complex ScriptableObjects,
        // they should be synchronized by reference (e.g., asset path or ID)
        // and loaded on clients, or their relevant properties can be SyncVar'd.
        // For now, assuming they are loaded independently on clients.
        public ChassisData chassisData;
        public MaskData equippedMask;

        [Header("Runtime Stats")]
        [SyncVar] public int maxHP;
        [SyncVar] public int currentHP;
        [SyncVar] public int armor;
        [SyncVar] public int attackDamage;
        [SyncVar] public float attackInterval;
        [SyncVar] public float range;
        [SyncVar] public float moveSpeed;
        [SyncVar] public float evasion;
        [SyncVar] public DamageType currentDamageType;
        [SyncVar] public ResistanceType currentResistanceType;
        [SyncVar] public float currentResistanceValue;

        [Header("Combat State")]
        [SyncVar] public bool isAlive = true;
        [SyncVar] public uint currentTargetNetId; // Synchronize target by netId
        public MechController currentTarget; // Resolved reference on client
        [SyncVar] public TargetingMode targetingMode = TargetingMode.Nearest;
        [SyncVar] public float attackCooldown;
        [SyncVar] public float retargetTimer;

        [Header("References")]
        public StatusEffectHandler statusHandler;
        public MechMovement movement;

        // Ability (set when mask is equipped)
        [System.NonSerialized] public IMaskAbility activeAbility;

        // Prefab for mask indicator disc (set by MechSpawner)
        [HideInInspector] public GameObject maskIndicatorPrefab;

        // References set by BattleManager
        private IBattleGrid grid;
        public List<MechController> allMechs; // Made public for TilePathfinder

        private const float RETARGET_INTERVAL = 0.5f;

        public override void OnStartServer()
        {
            base.OnStartServer();
            // Initialize SyncVars on the server
            // currentHP and isAlive are already SyncVar'd and initialized
            // Other SyncVars like mechId, team, etc., should be set before NetworkServer.Spawn
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Hook up SyncVar callbacks if needed, or resolve references
        }

        // Callback for currentTargetNetId SyncVar
        public void OnTargetNetIdChanged(uint oldNetId, uint newNetId)
        {
            if (isClient)
            {
                if (NetworkClient.spawned.TryGetValue(newNetId, out NetworkIdentity targetIdentity))
                {
                    currentTarget = targetIdentity.GetComponent<MechController>();
                }
                else
                {
                    currentTarget = null;
                }
            }
        }

        public void Initialize(ChassisData chassis, Team team, int id, IBattleGrid grid)
        {
            // These are SyncVars, set them directly
            this.chassisData = chassis; // This will need to be handled differently for networked objects
            this.team = team;
            this.mechId = id;
            this.grid = grid; // This is a local reference, not networked
            this.isAlive = true;
            this.equippedMask = null; // This will need to be handled differently for networked objects
            this.activeAbility = null;

            statusHandler = GetComponent<StatusEffectHandler>();
            if (statusHandler == null)
                statusHandler = gameObject.AddComponent<StatusEffectHandler>();

            movement = GetComponent<MechMovement>();
            if (movement == null)
                movement = gameObject.AddComponent<MechMovement>();
            movement.Initialize(this, grid);

            RecalculateStats();
            currentHP = maxHP;
            attackCooldown = 0f;
            retargetTimer = 0f;
        }

        public void SetAllMechsList(List<MechController> mechs)
        {
            this.allMechs = mechs;
        }

        public void EquipMask(MaskData mask)
        {
            equippedMask = mask;
            targetingMode = mask.defaultTargetingMode;
            RecalculateStats();
            currentHP = maxHP; // reset HP with new max

            MaskAbilityData abilityData = mask.GetAbilityForChassis(chassisData.chassisType);
            if (abilityData != null)
            {
                activeAbility = MaskAbilityFactory.Create(abilityData.abilityClassId);
                if (activeAbility != null && allMechs != null)
                {
                    activeAbility.Initialize(this, abilityData, grid, allMechs);
                }
            }

            // Create or update mask indicator disc
            Transform topHalf = transform.Find(MechSpawner.TOP_HALF_NAME);
            if (topHalf == null)
            {
                GameObject indicator;
                if (maskIndicatorPrefab != null)
                {
                    indicator = Instantiate(maskIndicatorPrefab);
                }
                else
                {
                    indicator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    var col = indicator.GetComponent<Collider>();
                    if (col != null) Destroy(col);
                }
                indicator.name = MechSpawner.TOP_HALF_NAME;
                indicator.transform.SetParent(transform, false);
                indicator.transform.localScale = new Vector3(
                    chassisData.indicatorRadius * 2f,
                    0.05f,
                    chassisData.indicatorRadius * 2f
                );
                indicator.transform.localPosition = new Vector3(0f, chassisData.indicatorHeight, 0f);
                topHalf = indicator.transform;
            }
            var topRenderer = topHalf.GetComponent<Renderer>();
            if (topRenderer != null)
                topRenderer.material.color = mask.maskTint;
        }

        public void RecalculateStats()
        {
            maxHP = chassisData.maxHP;
            armor = chassisData.armor;
            attackDamage = chassisData.attackDamage;
            attackInterval = chassisData.attackInterval;
            range = chassisData.range;
            moveSpeed = chassisData.moveSpeed;
            evasion = chassisData.evasion;
            currentDamageType = chassisData.baseDamageType;
            currentResistanceType = chassisData.resistanceType;
            currentResistanceValue = chassisData.resistanceValue;

            if (equippedMask != null)
            {
                maxHP += equippedMask.bonusHP;
                armor += equippedMask.bonusArmor;
                attackDamage += equippedMask.bonusAttackDamage;
                attackInterval += equippedMask.bonusAttackInterval;
                evasion += equippedMask.bonusEvasion;

                currentDamageType = equippedMask.damageTypeOverride;
                currentResistanceType = chassisData.resistanceType; // Mask doesn't change resistance type, only chassis
                currentResistanceValue = chassisData.resistanceValue;
                attackDamage = Mathf.FloorToInt(attackDamage * equippedMask.damageMultiplier);
            }

            attackInterval = Mathf.Max(attackInterval, 0.1f);
            evasion = Mathf.Clamp01(evasion);
        }

        // Only allow server to take damage
        [Server]
        public void TakeDamage(int rawDamage, MechController attacker)
        {
            if (!isAlive) return;

            float markMultiplier = statusHandler.GetMarkMultiplier();
            bool evaded = CombatMath.RollEvasion(evasion);

            if (evaded) return;

            CombatMath.ApplyDamage(rawDamage, attacker.currentDamageType, armor, currentResistanceType, currentResistanceValue, markMultiplier, ref currentHP, statusHandler);

            if (currentHP <= 0)
            {
                Die();
                if (attacker != null && attacker.activeAbility != null)
                {
                    attacker.activeAbility.OnKill(this);
                }
            }
        }

        [Server] // Only allow server to trigger Die
        public void Die()
        {
            if (!isAlive) return;
            isAlive = false;
            currentHP = 0;

            if (activeAbility != null)
                activeAbility.Cleanup();

            statusHandler.RemoveAllEffects();

            // Clear tile occupancy
            if (grid != null)
            {
                int tile = grid.GetNearestTile(transform.position);
                grid.ClearTile(tile);
            }

            // For networked objects, use NetworkServer.Destroy
            NetworkServer.Destroy(gameObject);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            // Enable input or other local player specific logic here
        }

        public override void OnStopLocalPlayer()
        {
            base.OnStopLocalPlayer();
            // Disable input or other local player specific logic here
        }

        [ServerCallback] // Only run on server
        public void UpdateCombat(float dt)
        {
            if (!isAlive) return;

            statusHandler.TickEffects(dt);

            // Retarget periodically
            retargetTimer -= dt;
            if (retargetTimer <= 0f || currentTarget == null || !currentTarget.isAlive)
            {
                currentTarget = TargetingSystem.GetTarget(this, allMechs, grid);
                if (currentTarget != null)
                {
                    currentTargetNetId = currentTarget.netId; // Update SyncVar
                }
                else
                {
                    currentTargetNetId = 0; // No target
                }
                retargetTimer = RETARGET_INTERVAL;
            }

            if (currentTarget == null) return;

            // Tick ability
            if (activeAbility != null)
                activeAbility.Tick(dt);

            // Attack cooldown
            attackCooldown -= dt;

            bool inRange = movement.MoveToward(currentTarget, dt);

            if (inRange && attackCooldown <= 0f)
            {
                TryAttack();
            }
        }

        private void TryAttack()
        {
            if (currentTarget == null || !currentTarget.isAlive) return;
            // Prevent friendly fire – never deal damage to same-team targets
            if (currentTarget.team == team) return;

            attackCooldown = attackInterval;

            // Face target
            // Only update rotation on server, NetworkTransform will synchronize
            if (isServer)
            {
                Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
                if (dir != Vector3.zero)
                    transform.forward = dir;
            }

            if (chassisData.isRanged && chassisData.projectilePrefab != null)
            {
                // Ensure the MechController itself has a NetworkIdentity to get its netId
                if (!TryGetComponent<NetworkIdentity>(out var attackerNetworkIdentity))
                {
                    Debug.LogError($"MechController {name} does not have a NetworkIdentity. Cannot spawn networked projectile.");
                    return;
                }
                if (!currentTarget.TryGetComponent<NetworkIdentity>(out var targetNetworkIdentity))
                {
                    Debug.LogError($"Target MechController {currentTarget.name} does not have a NetworkIdentity. Cannot spawn networked projectile.");
                    return;
                }

                GameObject projectileGO = Instantiate(chassisData.projectilePrefab, transform.position, Quaternion.identity);
                Projectile projectile = projectileGO.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(attackerNetworkIdentity.netId, targetNetworkIdentity.netId, attackDamage, currentDamageType);
                    NetworkServer.Spawn(projectileGO);
                }
            }
            else
            {
                currentTarget.TakeDamage(attackDamage, this);
            }

            if (activeAbility != null)
                activeAbility.OnAttackLanded(currentTarget, attackDamage);
        }

        public void OnBattleStart()
        {
            if (activeAbility != null)
                activeAbility.OnBattleStart();
        }

        public float GetDPS()
        {
            if (attackInterval <= 0f) return 0f;
            return attackDamage / attackInterval;
        }
    }
}
