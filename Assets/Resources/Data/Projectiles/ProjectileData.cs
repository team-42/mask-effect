using UnityEngine;

[CreateAssetMenu(fileName = "NewProjectileData", menuName = "Mask Effect/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    public GameObject ProjectilePrefab;
    public float Speed = 20f;
    public float Damage = 10f;
    public float Lifetime = 3f; // Added Lifetime property
    // Add other projectile-specific properties here as needed
}
