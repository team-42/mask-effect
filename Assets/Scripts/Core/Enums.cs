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
        Tank,
        Sniper,
        Colossus
    }

    public enum MaskType
    {
        None,
        Warrior,
        Rogue,
        Angel,
        Phantom,
        Commander
    }

    public enum TargetingMode
    {
        Nearest,
        LowestHP,
        HighestThreat,
        BacklinePriority,
        FarthestEnemy,
        LowestHPAlly,
        FurthestInRange
    }

    public enum StatusEffectType
    {
        Shield,
        Mark,
        Slow,
        Root,
        Taunt,
        Stun,
        Untargetable,
        Invisible,
        MissChance
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }

    public enum ResistanceType
    {
        Physical,
        Magical
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

    public enum SpawnPreference
    {
        Random,
        Backline,
        FrontlineCenter
    }
}
