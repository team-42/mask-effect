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
        [SyncVar(hook = nameof(OnChassisDataPathChanged))] public string chassisDataPath;
        [SyncVar] public uint equippedMaskNetId;
        public ChassisData chassisData; // Loaded on client via hook
        public MaskData equippedMask;
        [System.NonSerialized] public NetworkMask networkMask;

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

        [Header("Sniper Charge")]
        [SyncVar] public bool isCharging;
        [SyncVar] public float chargeTimer;
        private bool chargeInterrupted;

        [Header("References")]
        public StatusEffectHandler statusHandler;
        public MechMovement movement;

        [Header("VFX")]
        public GameObject deathEffectPrefab;

        // SFX
        private AudioSource _sfxSource;
        private AudioClip _laserClip;

        // Ability (set when mask is equipped)
        [System.NonSerialized] public IMaskAbility activeAbility;

        // References set by BattleManager
        private IBattleGrid grid;
        public List<MechController> allMechs; // Made public for TilePathfinder

        private const float RETARGET_INTERVAL = 0.5f;

        /// <summary>
        /// World-space center of the mech's visual body (accounts for hover height on flying mechs).
        /// </summary>
        public Vector3 VisualCenter =>
            transform.position + Vector3.up * (chassisData != null && chassisData.canFly ? chassisData.hoverHeight : 0f);

        private void Start()
        {
            // In singleplayer (no NetworkManager), OnStartClient() never fires.
            // Run visual setup here instead.
            if (NetworkHelper.IsOffline)
            {
                SetupVisuals();
            }
        }

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
            // Ensure visuals are set up when the client spawns the mech
            SetupVisuals();
        }

        // Callback for chassisDataPath SyncVar
        public void OnChassisDataPathChanged(string oldPath, string newPath)
        {
            if (isClient && !string.IsNullOrEmpty(newPath))
            {
                chassisData = Resources.Load<ChassisData>(newPath);
                SetupVisuals(); // Update visuals when chassis data changes
            }
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

        // Method to set up or update mech visuals based on chassisData and equippedMask
        private void SetupVisuals()
        {
            if (chassisData == null) return;

            // Clear existing visual children to prevent duplicates
            foreach (Transform child in transform)
            {
                if (child.name == "Body" || child.name == "BottomHalf" || child.name == MechSpawner.TOP_HALF_NAME)
                {
                    Destroy(child.gameObject);
                }
            }

            Color teamColor = team == Team.Player ? MechSpawner.PlayerTeamColor : MechSpawner.EnemyTeamColor;
            Vector3 scale = chassisData.chassisScale;

            // Try to load 3D model from Resources/Models/ by chassis name
            GameObject modelPrefab = Resources.Load<GameObject>("Models/" + chassisData.chassisName);

            if (modelPrefab != null)
            {
                // --- 3D Model path ---
                GameObject body = Instantiate(modelPrefab, transform);
                body.name = "Body";
                body.transform.localScale = scale;
                body.transform.localEulerAngles = chassisData.modelRotationOffset;

                // Elevate body for flying mechs + add hover bob
                if (chassisData.canFly && chassisData.hoverHeight > 0f)
                {
                    body.transform.localPosition = new Vector3(0f, chassisData.hoverHeight, 0f);
                    if (body.GetComponent<HoverBob>() == null)
                    {
                        var bob = body.AddComponent<HoverBob>();
                        bob.amplitude = 0.15f;
                        bob.frequency = 1.2f;
                    }
                }
                else
                {
                    body.transform.localPosition = Vector3.zero;
                }

                Renderer[] bodyRenderers = body.GetComponentsInChildren<Renderer>();
                foreach (var rend in bodyRenderers)
                    rend.material.color = teamColor;

                // If a mask is equipped, apply its tint to the top half
                if (equippedMask != null)
                {
                    Transform topHalf = body.transform.Find(MechSpawner.TOP_HALF_NAME);
                    if (topHalf != null)
                    {
                        Renderer topRenderer = topHalf.GetComponent<Renderer>();
                        if (topRenderer != null)
                            topRenderer.material.color = equippedMask.maskTint;
                    }
                }
            }
            else
            {
                // --- Primitive fallback path ---
                float halfY = scale.y * 0.5f;

                GameObject bottom = GameObject.CreatePrimitive(chassisData.primitiveShape);
                bottom.name = "BottomHalf";
                bottom.transform.SetParent(transform, false);
                bottom.transform.localScale = new Vector3(scale.x, halfY, scale.z);
                bottom.transform.localPosition = new Vector3(0f, halfY * 0.5f, 0f);
                SetRendererColor(bottom, teamColor);
                RemoveCollider(bottom);

                GameObject top = GameObject.CreatePrimitive(chassisData.primitiveShape);
                top.name = MechSpawner.TOP_HALF_NAME;
                top.transform.SetParent(transform, false);
                top.transform.localScale = new Vector3(scale.x, halfY, scale.z);
                top.transform.localPosition = new Vector3(0f, halfY * 1.5f, 0f);
                SetRendererColor(top, teamColor);
                RemoveCollider(top);

                // Apply mask tint if equipped
                if (equippedMask != null)
                {
                    SetRendererColor(top, equippedMask.maskTint);
                }
            }

            SetupAudio();
        }

        // Helper methods for SetupVisuals (copied from MechSpawner)
        private void SetRendererColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = color;
        }

        private void RemoveCollider(GameObject obj)
        {
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        public void Initialize(ChassisData chassis, Team team, int id, IBattleGrid grid)
        {
            // Set team and identity BEFORE chassisDataPath, because the
            // SyncVar hook for chassisDataPath calls SetupVisuals() which
            // reads team to determine the color.
            this.chassisData = chassis;
            this.team = team;
            this.mechId = id;
            this.grid = grid;
            this.isAlive = true;
            this.equippedMask = null;
            this.activeAbility = null;

            // Set SyncVars after team is set so hooks see correct team
            if (NetworkHelper.IsServerOrOffline)
            {
                this.chassisDataPath = $"Data/Chassis/{chassis.name}";
                this.equippedMaskNetId = 0;
            }

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

            SetupAudio();
        }

        public void SetAllMechsList(List<MechController> mechs)
        {
            this.allMechs = mechs;
        }

        /// <summary>
        /// Server-side: equip a mask via its NetworkMask object.
        /// Called by BattleManager after spawning the NetworkMask.
        /// </summary>
        public void EquipMask(NetworkMask netMask)
        {
            networkMask = netMask;
            equippedMask = netMask.maskData;
            activeAbility = netMask.activeAbility;

            if (NetworkHelper.IsServerOrOffline)
            {
                equippedMaskNetId = NetworkHelper.IsOffline ? 0 : netMask.netId;
            }

            targetingMode = equippedMask.defaultTargetingMode;
            RecalculateStats();
            currentHP = maxHP;
        }

        /// <summary>
        /// Client-side: called by NetworkMask.OnStartClient to apply mask data.
        /// </summary>
        public void ApplyMaskFromNetwork(NetworkMask netMask)
        {
            networkMask = netMask;
            equippedMask = netMask.maskData;
            RecalculateStats();
            SetupVisuals();
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
            if (!NetworkHelper.IsServerOrOffline) return;
            if (!isAlive) return;

            float markMultiplier = statusHandler.GetMarkMultiplier();
            float totalEvasion = evasion + statusHandler.GetMissChance();
            bool evaded = CombatMath.RollEvasion(totalEvasion);

            if (evaded) return;

            // Interrupt Sniper charge on hit
            if (isCharging)
            {
                isCharging = false;
                chargeInterrupted = true;
                attackCooldown = attackInterval * 0.5f;
            }

            int hpBefore = currentHP;
            CombatMath.ApplyDamage(rawDamage, attacker.currentDamageType, armor, currentResistanceType, currentResistanceValue, markMultiplier, ref currentHP, statusHandler);
            int actualDamage = hpBefore - currentHP;

            // Notify ability of incoming damage
            if (activeAbility != null)
                activeAbility.OnTakeDamage(attacker, actualDamage);

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
            if (!NetworkHelper.IsServerOrOffline) return;
            if (!isAlive) return;
            isAlive = false;
            currentHP = 0;

            // Cleanup ability via NetworkMask (it owns the ability now)
            if (networkMask != null)
            {
                NetworkHelper.SmartDestroy(networkMask.gameObject);
                networkMask = null;
            }
            else if (activeAbility != null)
            {
                activeAbility.Cleanup();
            }

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

            NetworkHelper.SmartDestroy(gameObject);
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

        public void UpdateCombat(float dt)
        {
            if (!NetworkHelper.IsServerOrOffline) return;
            if (!isAlive) return;

            statusHandler.TickEffects(dt);

            // Stunned: skip all actions
            if (statusHandler.IsStunned())
            {
                isCharging = false;
                return;
            }

            // Retarget periodically
            retargetTimer -= dt;
            if (retargetTimer <= 0f || currentTarget == null || !currentTarget.isAlive)
            {
                currentTarget = TargetingSystem.GetTarget(this, allMechs, grid);
                if (!NetworkHelper.IsOffline)
                {
                    currentTargetNetId = currentTarget != null ? currentTarget.netId : 0;
                }
                retargetTimer = RETARGET_INTERVAL;
            }

            if (currentTarget == null) return;

            // Tick ability
            if (activeAbility != null)
                activeAbility.Tick(dt);

            // Attack cooldown
            attackCooldown -= dt;

            // Sniper: hold position while target in range, use charge-up before firing
            if (chassisData != null && chassisData.chassisType == ChassisType.Sniper)
            {
                bool targetInRange = movement.IsInRange(currentTarget);
                if (!targetInRange)
                {
                    movement.MoveToward(currentTarget, dt);
                    isCharging = false;
                }
                else if (attackCooldown <= 0f)
                {
                    if (!isCharging)
                    {
                        isCharging = true;
                        chargeTimer = 1.0f;
                        chargeInterrupted = false;
                    }

                    if (isCharging && !chargeInterrupted)
                    {
                        chargeTimer -= dt;
                        if (chargeTimer <= 0f)
                        {
                            isCharging = false;
                            TryAttack();
                        }
                    }
                    else if (chargeInterrupted)
                    {
                        // Wait for cooldown to reset before trying again
                        isCharging = false;
                        chargeInterrupted = false;
                    }
                }
            }
            else
            {
                // Standard combat for all other chassis
                bool inRange = movement.MoveToward(currentTarget, dt);
                if (inRange && attackCooldown <= 0f)
                {
                    TryAttack();
                }
            }
        }

        private void TryAttack()
        {
            if (currentTarget == null || !currentTarget.isAlive) return;
            // Prevent friendly fire – never deal damage to same-team targets
            if (currentTarget.team == team) return;

            attackCooldown = attackInterval;

            // Face target
            if (NetworkHelper.IsServerOrOffline)
            {
                Vector3 dir = (currentTarget.transform.position - transform.position).normalized;
                if (dir != Vector3.zero)
                    transform.forward = dir;
            }

            if (chassisData.isRanged && chassisData.projectilePrefab != null)
            {
                GameObject projectileGO = Instantiate(chassisData.projectilePrefab, VisualCenter, Quaternion.identity);
                Projectile projectile = projectileGO.GetComponent<Projectile>();
                if (projectile != null)
                {
                    if (NetworkHelper.IsOffline)
                    {
                        projectile.InitializeOffline(this, currentTarget, attackDamage, currentDamageType);
                    }
                    else
                    {
                        projectile.Initialize(this, currentTarget, attackDamage, currentDamageType);
                        NetworkServer.Spawn(projectileGO);
                    }
                }

                PlayLaserSound();
            }
            else
            {
                // Melee attack
                currentTarget.TakeDamage(attackDamage, this);

                // Colossus cleave: hit a second target within range
                if (chassisData != null && chassisData.chassisType == ChassisType.Colossus)
                {
                    MechController secondTarget = FindSecondCleaveTarget();
                    if (secondTarget != null)
                    {
                        secondTarget.TakeDamage(attackDamage, this);
                        if (activeAbility != null)
                            activeAbility.OnAttackLanded(secondTarget, attackDamage);
                    }
                }
            }

            if (activeAbility != null)
                activeAbility.OnAttackLanded(currentTarget, attackDamage);
        }

        private MechController FindSecondCleaveTarget()
        {
            if (allMechs == null) return null;
            MechController closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < allMechs.Count; i++)
            {
                if (!allMechs[i].isAlive) continue;
                if (allMechs[i].team == team) continue;
                if (allMechs[i] == currentTarget) continue;

                float dist = Vector3.Distance(transform.position, allMechs[i].transform.position);
                if (dist <= range && dist < closestDist)
                {
                    closestDist = dist;
                    closest = allMechs[i];
                }
            }
            return closest;
        }

        /// <summary>
        /// Resets the Sniper charge state, allowing the next shot to fire without charge-up.
        /// Used by KillShotAbility on kill.
        /// </summary>
        public void ResetCharge()
        {
            isCharging = false;
            chargeTimer = 0f;
            chargeInterrupted = false;
            attackCooldown = 0f;
        }

        public void OnBattleStart()
        {
            if (activeAbility != null)
                activeAbility.OnBattleStart();
        }

        private void SetupAudio()
        {
            if (_sfxSource == null)
            {
                _sfxSource = GetComponent<AudioSource>();
                if (_sfxSource == null)
                    _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.playOnAwake = false;
                _sfxSource.spatialBlend = 0f;
                _sfxSource.volume = 0.5f;
            }
            if (_laserClip == null && chassisData != null)
            {
                string clipName = chassisData.chassisType.ToString().ToLower() + "_laser";
                _laserClip = Resources.Load<AudioClip>("MechSounds/" + clipName);
            }
        }

        private void PlayLaserSound()
        {
            if (_laserClip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(_laserClip);
        }

        public float GetDPS()
        {
            if (attackInterval <= 0f) return 0f;
            return attackDamage / attackInterval;
        }
    }
}
