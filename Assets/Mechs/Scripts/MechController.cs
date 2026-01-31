using System.Collections.Generic;
using UnityEngine;

namespace MaskEffect
{
    public class MechController : MonoBehaviour
    {
        [Header("Identity")]
        public int mechId;
        public Team team;
        public ChassisData chassisData;

        [Header("Mask")]
        public MaskData equippedMask;

        [Header("Runtime Stats")]
        public int maxHP;
        public int currentHP;
        public int armor;
        public int attackDamage;
        public float attackInterval;
        public float range;
        public float moveSpeed;
        public float evasion;
        public DamageType currentDamageType;
        public ResistanceType currentResistanceType;
        public float currentResistanceValue;

        [Header("Combat State")]
        public bool isAlive = true;
        public MechController currentTarget;
        public TargetingMode targetingMode = TargetingMode.Nearest;
        public float attackCooldown;
        public float retargetTimer;

        [Header("References")]
        public StatusEffectHandler statusHandler;
        public MechMovement movement;

        [Header("VFX")]
        public GameObject deathEffectPrefab;

        // Ability (set when mask is equipped)
        [System.NonSerialized] public IMaskAbility activeAbility;

        // Prefab for mask indicator disc (set by MechSpawner)
        [HideInInspector] public GameObject maskIndicatorPrefab;

        // Material for ground ring indicator (set by MechSpawner)
        [HideInInspector] public Material maskRingMaterial;

        // References set by BattleManager
        private IBattleGrid grid;
        public List<MechController> allMechs; // Made public for TilePathfinder

        private const float RETARGET_INTERVAL = 0.5f;

        /// <summary>
        /// World-space center of the mech's visual body (accounts for hover height on flying mechs).
        /// </summary>
        public Vector3 VisualCenter =>
            transform.position + Vector3.up * (chassisData != null && chassisData.canFly ? chassisData.hoverHeight : 0f);

        public void Initialize(ChassisData chassis, Team team, int id, IBattleGrid grid)
        {
            this.chassisData = chassis;
            this.team = team;
            this.mechId = id;
            this.grid = grid;
            this.isAlive = true;
            this.equippedMask = null;
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

            // Create or update mask ground ring indicator
            Transform ring = transform.Find(MechSpawner.MASK_RING_NAME);
            if (ring == null)
            {
                GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var col = indicator.GetComponent<Collider>();
                if (col != null) Destroy(col);
                indicator.name = MechSpawner.MASK_RING_NAME;
                indicator.transform.SetParent(transform, false);
                // Rotate quad to lie flat on the ground
                indicator.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                float ringSize = chassisData.indicatorRadius * 7f;
                indicator.transform.localScale = new Vector3(ringSize, ringSize, 1f);
                // Position just above ground to avoid z-fighting
                indicator.transform.localPosition = new Vector3(0f, 0.05f, 0f);
                ring = indicator.transform;

                // Apply ring material (load from Resources if not assigned)
                if (maskRingMaterial == null)
                    maskRingMaterial = Resources.Load<Material>("Materials/MaskRing");
                var renderer = indicator.GetComponent<Renderer>();
                if (renderer != null && maskRingMaterial != null)
                {
                    renderer.material = new Material(maskRingMaterial);
                }
            }
            var ringRenderer = ring.GetComponent<Renderer>();
            if (ringRenderer != null)
                ringRenderer.material.SetColor("_Color", mask.maskTint);
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

            // Instantiate death effect
            if (deathEffectPrefab != null)
            {
                GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    Destroy(effect, ps.main.duration);
                }
                else
                {
                    Destroy(effect, 3f); // Default destroy time if no ParticleSystem found
                }
            }

            gameObject.SetActive(false);
        }

        public void UpdateCombat(float dt)
        {
            if (!isAlive) return;

            statusHandler.TickEffects(dt);

            // Retarget periodically
            retargetTimer -= dt;
            if (retargetTimer <= 0f || currentTarget == null || !currentTarget.isAlive)
            {
                currentTarget = TargetingSystem.GetTarget(this, allMechs, grid);
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
            Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.forward = dir;

            if (chassisData.isRanged && chassisData.projectilePrefab != null)
            {
                GameObject projectileGO = Instantiate(chassisData.projectilePrefab, VisualCenter, Quaternion.identity);
                Projectile projectile = projectileGO.GetComponent<Projectile>();
                if (projectile != null)
                {
                    projectile.Initialize(this, currentTarget, attackDamage, currentDamageType);
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
