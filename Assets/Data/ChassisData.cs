using UnityEngine;

namespace MaskEffect
{
    [CreateAssetMenu(fileName = "NewChassis", menuName = "MaskEffect/Chassis Data")]
    public class ChassisData : ScriptableObject
    {
        public string chassisName;
        public ChassisType chassisType;

        [Header("Base Stats")]
        public int maxHP = 100;
        public int armor = 10;
        public int attackDamage = 10;
        public float attackInterval = 1f;
        public float range = 1.5f;
        public float moveSpeed = 2f;
        [Range(0f, 1f)]
        public float evasion = 0.1f;

        [Header("Combat Type")]
        public bool isRanged = false;
        public GameObject projectilePrefab;

        [Header("Damage & Resistance")]
        public DamageType baseDamageType = DamageType.Physical;
        public ResistanceType resistanceType = ResistanceType.Physical;
        public float resistanceValue = 0f; // e.g., 0.1 for 10% resistance

        [Header("Visuals (Placeholder)")]
        public PrimitiveType primitiveShape = PrimitiveType.Cube;
        public Color chassisColor = Color.gray;
        public Vector3 chassisScale = Vector3.one;
    }
}
