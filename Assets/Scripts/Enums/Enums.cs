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

public enum NpcType
{
    Vendor,
    BlackMarketSeller,
    Gambler
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

public enum CinematicPhase
{
    openingScene,
    riftOpening,
    mobSpilledFromRift,
    finalSpeech,
    closingScene
}

public enum MoldranSpeechOrder
{
    firstSpeech,
    secondSpeech
}

public enum ActiveItemType
{
    Generic,
    Boomerang,
    Bomb,
    Shiruken,
    Pentagram,
    Dummy,
    Hourglass,
    Compass,
    Potion,
    Summoner,
    Incendiary,
    BobbyPin
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
    OminousGripOfThunder,
    HaloOfBlindingRadiance,
    ChestplateOfTheLastLight,
    RingOfTempestStrikes,
    HelmOfTheEternalVigil,
    InfernoSash,
    GirdleOfFirmament,
    BloodforgedGirdle,
    SandweaversSash,
    RubyPendant,
    SapphirePendant,
    TopazPendant,
    EmeraldPendant,
    GildedGuardian,
    WhisperingHood,
    EnchantersSpire,
    RecantersCloak,
    MantleOfStars,
    CloakOfWindwalker,
    GoldenCloak,
    EmbercladBracers,
    VenomTouchedGloves,
    RingOfMight,
    BootsOfInfernalMarch,
    RingOfVitality,
    RingOfSagacity,
    BlazingHeartplate,
    FrostboundChainmail,
    VenomweaveVest
}

public enum ProjectileType
{
    Normal,
    Laser
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
    Frozen
}

public enum HealthStatus
{
    Normal,
    Poisoned,
    Burned
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
    Right,
    DownRight,
    Left,
    DownLeft,
    Down,
    UpLeft
}

public enum AttackDirection
{
    Up,
    UpRight,
    Right,
    DownRight,
    Down,
    DownLeft,
    Left,
    UpLeft
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
    CrudeBow,
    Crossbow,
    OldStaff,
    SolarFlare,
    HeavensGale,
    Shield,
    ApolloShield,
    BronzeHuntingBow,
    ChordOfTheSerpent,
    EbonLongbow,
    Netherstrand,
    BlackTalon,
    HailstormSculptor,
    Trident,
    Blazefury,
    Scarlet,
    Doombringer,
    Stormblade,
    Ravager,
    ArcaneConduit,
    StaffOfTheWild,
    TwilightStaff,
    VenomAxe,
    PhoenixBolt,
    HammerOfTheThunderlord,
    VipersBite,
    BloodfangClaw,
    SilverwingBow
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
    Claw,
    Crossbow
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
    Thrust
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

public enum EnemyCategory
{
    None,
    Bat,
    CrimsonWing,
    Croclodyte,
    FireElemental,
    EmberclawImp,
    StormsparkImp,
    Nexarion,
    NexarionLord,
    Pixie,
    Sprite,
    Skeleton,
    Viper,
    RedViper,
    Snapthorn,
    Ravagevine,
    Spider,
    BlackWidow,
    Wight,
    Wraith,
    Centaur,
    Treant,
    Galvanus,
    Sepharoth,
    FrostWrym,
    Venomancer,
    FireWrym,
    Moldran,
    Slime,
    Beholder,
    Blightpump,
    AirElemental,
    EarthElemental,
    WaterElemental,
    IceNexarion
}

public enum EnemyBehaviour
{
    Pursuit,
    AimAndShoot,
    PrepareAndDash,
    Centaur,
    Treant,
    Galvanus,
    Sepharoth,
    FrostWrym,
    Venomancer,
    FireWrym,
    Moldran
}

public enum EnemyType
{
    Normal,
    Boss,
    Minion
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

public enum TreantPhase
{
    None,
    Wait,
    StraightAttack,
    RazorLeaf,
    Summon,
    Heal
}

public enum GalvanusPhase
{
    None,
    Wait,
    LightningBolt,
    DashAttack,
    Lightning
}

public enum SepharothPhase
{
    None,
    Wait,
    InvisibleAndMine,
    SmearAttack,
    LaserBeam
}

public enum FrostWrymPhase
{
    None,
    Wait,
    IceProjectile,
    Icicle,
    TailAttack,
    FrostBreath
}

public enum VenomancerPhase
{
    None,
    Wait,
    StoneRain,
    SlamGround,
    ToxicPool,
    SludgeThrow
}

public enum FireWrymPhase
{
    None,
    Wait,
    FireProjectile,
    FirePillar,
    TailAttack,
    FireBreath
}

public enum MoldranPhase
{
    None,
    Wait,
    Projectile,
    Spike,
    SwingAttack,
    Heal
}

public enum BookPage
{
    Stats,
    Weapons,
    Passives,
    Actives,
    Beastiary,
    Bosses,
    Build
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
    YourHandsFull,
    DontMeetRequiredPrimaryStats
}

public enum DropType
{
    PassiveItem,
    ActiveItem,
    Weapon
}
