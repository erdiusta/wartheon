public enum Orientation
{
    North,
    East,
    South,
    West,
    None
}

public enum GameState
{
    gameStarted,
    playingLevel,
    engagingEnemies,
    bossStage,
    engagingBoss,
    levelCompleted,
    gameWon,
    gameLost,
    gamePaused,
    dungeonOverviewMap,
    restartGame
}

public enum ActiveItemType
{
    Generic,
    Boomerang,
    Bomb,
    Shiruken,
    Trap
}

public enum BoomerangPhase
{
    Aim,
    Fire,
    Return
}

public enum ShirukenPhase
{
    Fire,
    Ricochet
}

public enum MoveStatus
{
    Idle,
    Stagger,
    Stun,
    Slow
}

public enum HealthStatus
{
    Normal,
    Poisoned,
    Bleeding
}

public enum ArmorStatus
{
    Normal,
    SilverArmor,
    GoldenArmor,
    Acid
}

public enum AimDirection
{
    Up,
    UpRight,
    UpLeft,
    Right,
    Left,
    Down
}

public enum WeaponClass
{
    Sword,
    Axe,
    Hammer,
    Shield,
    Spear,
    Staff,
    Bow,
    Dagger,
}

public enum WieldType
{
    OneHanded,
    TwoHanded
}

public enum DamageType
{
    Slashing,
    Piercing,
    Crushing,
    Fire,
    Water,
    Earth,
    Air,
    Dark,
    Light
}

public enum AttackType
{
    MeleeRight,
    MeleeLeft,
    Projectile,
    Contact,
    Gradual
}

public enum ChestSpawnEvent
{
    onRoomEntry,
    onEnemiesDefeated
}

public enum ChestSpawnPosition
{
    atSpawnerPosition,
    atPlayerPosition
}

public enum ChestState
{
    closed,
    weaponItem,
    empty
}

public enum EnemyBehaviour
{
    Pursuit,
    AimAndShoot,
    PrepareAndDash,
    InstantDash
}

public enum EnemyPhase
{
    Patrol,
    Chase,
    GetHit,
    Attack,
    Death
}
