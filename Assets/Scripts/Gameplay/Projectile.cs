using UnityEngine;

namespace MaskEffect
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private GameObject hitEffectPrefab; // Optional visual effect on hit

        private MechController attacker;
        private MechController target;
        private int damage;
        private DamageType damageType;

        private float currentLifetime;

        public void Initialize(MechController attacker, MechController target, int damage, DamageType damageType)
        {
            this.attacker = attacker;
            this.target = target;
            this.damage = damage;
            this.damageType = damageType;
            this.currentLifetime = lifetime;
        }

        private void Update()
        {
            if (target == null || !target.isAlive)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            MechController hitMech = other.GetComponent<MechController>();
            if (hitMech != null && hitMech == target)
            {
                target.TakeDamage(damage, attacker);

                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
}
