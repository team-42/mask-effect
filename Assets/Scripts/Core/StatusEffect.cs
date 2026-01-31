namespace MaskEffect
{
    [System.Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public float duration;
        public float value;
        public MechController source;
        public int stackId;

        public bool IsExpired => duration <= 0f;

        public void Tick(float dt)
        {
            duration -= dt;
        }

        public StatusEffect(StatusEffectType type, float duration, float value,
            MechController source = null, int stackId = 0)
        {
            this.type = type;
            this.duration = duration;
            this.value = value;
            this.source = source;
            this.stackId = stackId;
        }
    }
}