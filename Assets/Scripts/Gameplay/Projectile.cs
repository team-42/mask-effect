using UnityEngine;
using Mirror;

namespace MaskEffect
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private GameObject hitEffectPrefab; // Optional visual effect on hit

        [SyncVar] private uint attackerNetId;
        [SyncVar] private uint targetNetId;
        [SyncVar] private int damage;
        [SyncVar] private DamageType damageType;

        private MechController _attacker; // Resolved attacker reference
        private MechController _target;   // Resolved target reference

        private float currentLifetime;

        public override void OnStartServer()
        {
            base.OnStartServer();
            currentLifetime = lifetime;
        }

        public void Initialize(uint attackerId, uint targetId, int dmg, DamageType dmgType)
        {
            attackerNetId = attackerId;
            targetNetId = targetId;
            damage = dmg;
            damageType = dmgType;
            // currentLifetime is set in OnStartServer
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
            if (!isServer) return; // Only server updates projectile logic

            if (_target == null || !_target.isAlive)
            {
                NetworkServer.Destroy(gameObject);
                return;
            }

            Vector3 direction = (_target.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                if (isServer)
                {
                    NetworkServer.Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return; // Only server processes collisions

            MechController hitMech = other.GetComponent<MechController>();
            if (hitMech != null && hitMech == _target)
            {
                _target.TakeDamage(damage, _attacker);

                if (hitEffectPrefab != null)
                {
                    // Instantiate hit effect on all clients
                    RpcInstantiateHitEffect(transform.position);
                }
                NetworkServer.Destroy(gameObject);
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
