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

public enum AimDirection
{
    Up,
    UpRight,
    UpLeft,
    Right,
    Left,
    Down
}

<<<<<<< Updated upstream
=======
public enum WeaponTitle
{
    None,
    Gladius,
    Scimitar,
    SizzlingSword,
    Hatchet,
    PhalanxSpear,
    ClobberingTime,
    HolySword,
    AncientKatana,
    Carnage,
    Crusher,
    Dirk,
    Gambit,
    Bow,
    Crossbow,
    Staff,
    SolarFlare,
    HeavensGale,
    Shield,
    ApolloShield
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

public enum MeleeAttackType
{
    None,
    Swing,
    Sweep,
    Thrust,
}

public enum AttackType
{
    MeleeRight,
    MeleeLeft,
    Projectile,
    Contact,
    Gradual
}

>>>>>>> Stashed changes
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
    healthItem,
    ammoItem,
    weaponItem,
    empty
}
