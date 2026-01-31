namespace MaskEffect
{
    public enum Team
    {
        Player,
        Enemy
    }

    public enum ChassisType
    {
        Scout,
        Jet,
        Tank
    }

    public enum MaskType
    {
        None,
        Warrior,
        Rogue,
        Angel
    }

    public enum TargetingMode
    {
        Nearest,
        LowestHP,
        HighestThreat,
        BacklinePriority,
        FarthestEnemy,
        LowestHPAlly
    }

    public enum StatusEffectType
    {
        Shield,
        Mark,
        Slow,
        Root,
        Taunt
    }

    public enum BattleState
    {
        Setup,
        MaskAssignment,
        Combat,
        RoundEnd
    }

    public enum TileZone
    {
        Player,
        Enemy,
        Neutral
    }
}