using UnityEngine;
using Mirror;

namespace MaskEffect
{
    public class Projectile : NetworkBehaviour
    {
        private ProjectileData projectileData; // Removed [SerializeField]
        [SerializeField] private GameObject hitEffectPrefab; // Optional visual effect on hit

        [SyncVar] private uint attackerNetId;
        [SyncVar] private uint targetNetId;
        [SyncVar] private DamageType damageType;
        [SyncVar] private float projectileSpeed; // Synced for client-side movement
        [SyncVar] private float projectileLifetime; // Synced for client-side lifetime tracking

        private MechController _attacker; // Resolved attacker reference
        private MechController _target;   // Resolved target reference

        private float currentLifetime;
        private bool hasHit = false; // Prevent multiple collision triggers
        private bool needsTargetResolution = false; // Client needs deferred resolution

        public override void OnStartServer()
        {
            base.OnStartServer();
            // projectileData is expected to be set by Initialize() shortly after
        }

        /// <summary>
        /// Networked initialization: sets direct references immediately (needed on
        /// host where Update() may run before OnStartClient resolves netIds) and
        /// also stores netIds for pure-client resolution via OnStartClient.
        /// </summary>
        public void Initialize(MechController attacker, MechController target, ProjectileData data, DamageType dmgType)
        {
            _attacker = attacker;
            _target = target;
            projectileData = data;
            damageType = dmgType;

            // Set SyncVars for client-side movement
            projectileSpeed = data.Speed;
            projectileLifetime = data.Lifetime;
            currentLifetime = projectileLifetime;

            var attackerNI = attacker.GetComponent<NetworkIdentity>();
            var targetNI = target.GetComponent<NetworkIdentity>();
            if (attackerNI != null) attackerNetId = attackerNI.netId;
            if (targetNI != null) targetNetId = targetNI.netId;
        }

        /// <summary>
        /// Singleplayer initialization: direct MechController references
        /// instead of netIds that require NetworkManager.
        /// </summary>
        public void InitializeOffline(MechController attacker, MechController target, ProjectileData data, DamageType dmgType)
        {
            _attacker = attacker;
            _target = target;
            projectileData = data;
            damageType = dmgType;

            // Set movement data
            projectileSpeed = data.Speed;
            projectileLifetime = data.Lifetime;
            currentLifetime = projectileLifetime;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Try to resolve attacker and target on clients
            if (NetworkClient.spawned.TryGetValue(attackerNetId, out NetworkIdentity attackerIdentity))
            {
                _attacker = attackerIdentity.GetComponent<MechController>();
            }

            if (NetworkClient.spawned.TryGetValue(targetNetId, out NetworkIdentity targetIdentity))
            {
                _target = targetIdentity.GetComponent<MechController>();
            }

            // If target resolution failed, defer to Update() (spawn timing race condition)
            if (_target == null && targetNetId != 0)
            {
                needsTargetResolution = true;
                Debug.LogWarning($"[Projectile] Deferring target resolution to Update(). targetNetId={targetNetId}");
            }
        }

        private void Update()
        {
            // Client deferred resolution: handle spawn timing race condition
            if (needsTargetResolution && isClient && !isServer)
            {
                TryResolveTarget();
            }

            // Allow BOTH server and clients to run movement for smooth visuals
            if (NetworkHelper.IsOffline)
            {
                // Singleplayer mode - run full logic
                UpdateMovement();
            }
            else if (isServer)
            {
                // Server runs authoritative movement + handles destruction
                UpdateMovement();
            }
            else if (isClient)
            {
                // Clients run visual prediction only (no destruction)
                UpdateMovementClientOnly();
            }
        }

        private void TryResolveTarget()
        {
            if (NetworkClient.spawned.TryGetValue(targetNetId, out NetworkIdentity targetIdentity))
            {
                _target = targetIdentity.GetComponent<MechController>();
                if (_target != null)
                {
                    needsTargetResolution = false;
                    Debug.Log($"[Projectile] Successfully resolved target on client! targetNetId={targetNetId}");
                }
                else
                {
                    Debug.LogError($"[Projectile] Target NetworkIdentity found but MechController missing! targetNetId={targetNetId}");
                }
            }

            // Also try to resolve attacker if it failed
            if (_attacker == null && attackerNetId != 0)
            {
                if (NetworkClient.spawned.TryGetValue(attackerNetId, out NetworkIdentity attackerIdentity))
                {
                    _attacker = attackerIdentity.GetComponent<MechController>();
                }
            }
        }

        private void UpdateMovement()
        {
            if (hasHit) return; // Stop updating after hit

            if (_target == null || !_target.isAlive)
            {
                NetworkHelper.SmartDestroy(gameObject);
                return;
            }

            Vector3 targetPos = _target.VisualCenter;
            Vector3 direction = (targetPos - transform.position).normalized;
            transform.position += direction * projectileSpeed * Time.deltaTime;

            // Rotate projectile to face target
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                NetworkHelper.SmartDestroy(gameObject);
            }
        }

        private void UpdateMovementClientOnly()
        {
            if (hasHit) return; // Stop updating after hit

            // Client-side prediction: move toward target without destroying
            if (_target == null || !_target.isAlive)
            {
                // Target died - just stop moving, let server handle cleanup
                return;
            }

            Vector3 targetPos = _target.VisualCenter;
            Vector3 direction = (targetPos - transform.position).normalized;
            transform.position += direction * projectileSpeed * Time.deltaTime;

            // Rotate projectile to face target
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Don't decrement lifetime or destroy on client - server handles that
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkHelper.IsServerOrOffline) return;
            if (hasHit) return; // Prevent multiple hits

            MechController hitMech = other.GetComponent<MechController>();
            if (hitMech != null && hitMech == _target)
            {
                hasHit = true; // Mark as hit to stop Update and prevent re-entry

                // Disable renderer immediately to hide projectile
                var renderer = GetComponentInChildren<Renderer>();
                if (renderer != null) renderer.enabled = false;

                _target.TakeDamage(Mathf.FloorToInt(projectileData.Damage), _attacker);

                if (hitEffectPrefab != null)
                {
                    if (NetworkHelper.IsOffline)
                        Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                    else
                        RpcInstantiateHitEffect(transform.position);
                }
                NetworkHelper.SmartDestroy(gameObject);
            }
        }

        [ClientRpc]
        private void RpcInstantiateHitEffect(Vector3 position)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.identity);
            }
        }
    }
}
