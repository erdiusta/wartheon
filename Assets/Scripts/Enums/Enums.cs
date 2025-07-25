
public enum TutorialPhase
{
    TutorialIntro,
    Move,
    PickUpWeapon,
    AimAndFire,
    PickUpActiveItem,
    UseActiveItem,
    OpenGlossaryBook,
    MinimapCheck,
    OverviewMapCheck,
    SecondRoom,
    Combat,
    PickUpPrimaryPassiveHealth,
    PickUpPrimaryPassiveCoin,
    Parry,
    DodgeRoll,
    SpecialSkill,
    KillEmAll,
    PickUpSecondaryPassive,
    BuildsPage,
    WeaponSetSwitch,
    OtherCollectionsPage,
    FinishTutorial
}

public enum TutorialProcess
{
    Starting,
    QuestDisplayed,
    QuestPassed
}

public enum TooltipSource
{
    None,
    Pointer,
    Proximity
}


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
    Caelion,
    Morven,
    Nyveran,
    Mycara,
    Karnag,
    Kynara,
    Nymara,
    Nyxa
}

public enum NpcType
{
    None,
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

public enum PrimaryStatName
{
    Strength,
    Constitution,
    Dexterity,
    Intelligence,
    Willpower,
    Agility,
    Resolve,
    Ferocity
}

public enum InnerPathName
{
    KillersEdge,
    FocusedAggression,
    PunishersWill,
    ViciousMomentum,
    SurgingElements,
    CounterRiposte,
    Armorbane,
    TriadExecution,
    UnyieldingGuard,
    DieHard,
    StoneSkin,
    BattleScars,
    FortifiedResolve,
    ArcaneFortitude,
    SecondBreath,
    BlockThemAll,
    QuickReflexes,
    WeaversTempo,
    EfficientMind,
    ShiftingStance,
    CombatFocus,
    Resourceful,
    BattleReady,
    SurgeTapGain
}

public enum ActiveItemType
{
    Generic,
    Boomerang,
    Bomb,
    Shiruken,
    Pentagram,
    Decoy,
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

public enum PrimaryPassiveItemName
{
    None,
    Health,
    SilverCoin,
    GoldCoin,
    Key,
    Cure,
    HolyWater,
    Mana
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

[System.Flags]
public enum MoveStatus
{
    None = 0,
    Idle = 1 << 0, // 1
    KnockedBack = 1 << 1, // 2
    Stun = 1 << 2, // 4
    Root = 1 << 3, // 8
    Frozen = 1 << 4, // 16
    Paralyze = 1 << 5  // 32
}

[System.Flags]
public enum HealthStatus
{
    Normal = 0,
    Poisoned = 1 << 0, // 1 (binary 0001)
    Burned = 1 << 1, // 2 (binary 0010)
    Bleeding = 1 << 2  // 4 (binary 0100)
}

public enum ArmorStatus
{
    Normal,
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

public enum AttackShape
{
    None,
    Swing,
    Thrust,
    Cone
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
    Sylvarok,
    Galvanus,
    Sepharoth,
    Cryothar,
    Venomancer,
    Pyrothar,
    Moldran,
    MainSlime,
    Beholder,
    Blightpump,
    AirElemental,
    EarthElemental,
    WaterElemental,
    IceNexarion,
    MinionSlime,
    Moravelle
}

public enum EnemyBehaviour
{
    Pursuit,
    AimAndShoot,
    PrepareAndDash,
    Centaur,
    Sylvarok,
    Galvanus,
    Sepharoth,
    Cryothar,
    Venomancer,
    Pyrothar,
    Moldran,
    Roaming,
    Moravelle
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

public enum MoravellePhase
{
    None,
    Wait,
    StraightArrowShot,
    ChargeAndRetreat,
    SpreadArrowShot,
}

public enum SylvarokPhase
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
    DashAttack,
    LightningBolt,
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

public enum ActiveSkill
{
    SeismicSlam,
    Valor,
    ShieldBash,
    BreakTheLine,
    GuardedOath,
    UmbralMist,
    Stealth,
    BloodDrain,
    ShadowStep,
    CullTheMeek,
    Penetrate,
    TripleThreat,
    BindingArrow,
    ArrowsOfTheSevenPlagues,
    HuntersReach,
    Blizzard,
    MycarasSeal,
    SheerCold,
    Icebreaker,
    AbsoluteZero,
    FireBlast,
    MoltenRift,
    FlameLotus,
    KynarasEmbrace,
    BlazingCyclone,
    Rage,
    Shattercry,
    AxeThrow,
    Whirlrend,
    FeastOfWar,
    MistOfDisruption,
    NymarasWindveil,
    ChainLightning,
    EyeOfTheStorm,
    IonicRejuvenation,
    DontBlink,
    Whisperslice,
    Shiruken,
    VenomousIvy,
    FadeAndFeed
}
    

public enum ItemSlotStatus
{
    None,
    MainHand,
    OffHand,
    Active,
    Passive,
    Inventory
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
    WeaponOffHand,
    None,
    Drop
}

public enum ItemSwapPos
{
    None,
    DragPassiveInventorySlotPassive,
    DragPassiveSlotPassiveInventory,
    DragMainSlotMain,
    DragMainSlotOff,
    DragOffSlotMain,
    DragOffSlotOff,
    DragMainSlotInventory,
    DragOffSlotInventory,
    DragInventorySlotMain,
    DragInventorySlotOff
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
    DontMeetRequiredPrimaryStats,
    BobbyPinFailed,
    SummonerFailed
}

public enum MeleeHand
{
    None,
    MainHand,
    OffHand
}

public enum DropType
{
    PassiveItem,
    ActiveItem,
    Weapon
}
