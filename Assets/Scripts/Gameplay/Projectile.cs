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

        private MechController _attacker; // Resolved attacker reference
        private MechController _target;   // Resolved target reference

        private float currentLifetime;

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
            currentLifetime = projectileData.Lifetime;

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
            currentLifetime = projectileData.Lifetime;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Resolve attacker and target on clients
            if (NetworkClient.spawned.TryGetValue(attackerNetId, out NetworkIdentity attackerIdentity))
            {
                _attacker = attackerIdentity.GetComponent<MechController>();
            }
            if (NetworkClient.spawned.TryGetValue(targetNetId, out NetworkIdentity targetIdentity))
            {
                _target = targetIdentity.GetComponent<MechController>();
            }
        }

        private void Update()
        {
            if (!NetworkHelper.IsServerOrOffline) return;

            if (_target == null || !_target.isAlive)
            {
                NetworkHelper.SmartDestroy(gameObject);
                return;
            }

            Vector3 targetPos = _target.VisualCenter;
            Vector3 direction = (targetPos - transform.position).normalized;
            transform.position += direction * projectileData.Speed * Time.deltaTime;

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                NetworkHelper.SmartDestroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkHelper.IsServerOrOffline) return;

            MechController hitMech = other.GetComponent<MechController>();
            if (hitMech != null && hitMech == _target)
            {
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
