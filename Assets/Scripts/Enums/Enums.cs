public enum Orientation
{
    North,
    East,
    South,
    West,
    None
}

public enum Character
{
    Astraeus,
    Erebus,
    Orion,
    Lyrisa
}

public enum GameState
{
    gameStarted,
    playingLevel,
    engagingEnemies,
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
    Trap,
    Dummy,
    Hourglass,
    Compass,
    Potion,
    Summoner
}

public enum PassiveItemCategory
{
    Primary,
    Secondary
}

public enum PassiveItemType
{
    Generic,
    RingOfFortune,
    ShadowCloak,
    WardenOfForest,
    BeltOfSorcery,
    WingedSandals,
    OminousGripOfThunder
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
}

public enum HealthStatus
{
    Normal,
    Poisoned,
}

public enum ArmorStatus
{
    Normal,
    SilverArmor,
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

public enum WeaponLevel
{
    Basic,
    Enchanted,
    Mythic,
    Legendary
}

public enum WeaponHitSpeed
{
    VerySlow,
    Slow,
    Medium,
    Fast,
    VeryFast
}

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
    ApolloShield,
    BronzeHuntingBow,
    ChordOfTheSerpent,
    EbonLongbow,
    Netherstrand,
    BlackTalon,
    HailstormSculptor
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
    Claw
}

public enum WieldType
{
    OneHanded,
    TwoHanded
}

public enum ElementalBias
{
    None,
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
    Centaur
}

public enum EnemyPhase
{
    Patrol,
    Chase,
    GetHit,
    Attack,
    Death
}

public enum CentaurPhase
{
    None,
    Wait,
    StraightArrowShot,
    ChargeAndRetreat,
    SpreadArrowShot,
}

public enum EnemyRace
{
    Critter,
    Beast,
    Eldritch,
    Elemental,
    Demon,
    Undead,
    Vermin,
    Reptile
}

public enum BookPage
{
    Stats,
    Weapons,
    Items,
    Beastiary,
    Bosses
}

public enum PassiveItemSlotName
{
    None,
    Head,
    Chest,
    Neck,
    Finger,
    Back,
    Waist,
    Arm,
    Leg
}

public enum SlotType
{
    Passive,
    Active,
    WeaponMainHand,
    WeaponOffHand
}

public enum ItemSwapPos
{
    None,
    DragMainSlotMain,
    DragMainSlotOff,
    DragOffSlotMain,
    DragOffSlotOff
}

public enum PopUpReason
{
    None,
    LessThanOneMainHandWeapon,
    DontHaveWeaponOnSelectedSet,
    OffHandFull,
    ShieldCantBePutOnMainHand,
    OffHandCantBeAddedToTwoHanded,
    TwoHandCantBeEquippedToOffHand,
    OffHandCatBeAddedToEmptyMainHand,
    EmptyOffHandFirst,
    EquipMainHandFirst,
    CantMoveYourMainHandWithEmptyOffHand,
    YourHandsFull
}

public enum DropType
{
    PassiveItem,
    ActiveItem,
    Weapon
}
