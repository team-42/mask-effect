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

        [Header("Visuals (Placeholder)")]
        public PrimitiveType primitiveShape = PrimitiveType.Cube;
        public Color chassisColor = Color.gray;
        public Vector3 chassisScale = Vector3.one;
    }
}