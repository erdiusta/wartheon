using Mirror;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyNetwork : NetworkBehaviour, IEnemyCombatData, IEnemyMovementData
{
    public InstantiatedRoom InstantiatedRoom { get; private set; }
    public Vector2Int[] SpawnPositions { get; private set; }

    #region INTERFACES
    public bool Isboss => isBoss;
    public float ElementalForgeRate => elementalForgeRate;
    public int ExperiencePoints => experincePoints;
    public EnemyBehaviour EnemyBehaviour => enemyBehaviour;
    public EnemyCategory EnemyCategory => enemyCategory;
    public bool IsImmuneAfterHit => isImmuneAfterHit;
    public float HitImmunityTime => hitImmunityTime;

    // Combat
    public float PhysicalResistance => physicalResistance;
    public float MagicResistance => magicResistance;
    public bool HasShield => hasShield;
    public float DeflectChance => deflectChance;
    public float DeflectionValue => deflectionValue;
    public bool IsImmuneToBleeding => isImmuneToBleeding;
    public bool IsImmuneToStun => isImmuneToStun;
    public bool IsImmuneToSlow => isImmuneToSlow;
    public bool IsImmuneToBurn => isImmuneToBurn;
    public bool IsImmuneToPoison => isImmuneToPoison;
    public bool IsImmuneToRoot => isImmuneToRoot;
    public bool IsImmuneToFrost => isImmuneToFrost;
    public bool IsImmuneToParalyze => isImmuneToParalyze;
    public bool IsImmuneToBlind => isImmuneToBlind;
    public bool IsImmuneToCurse => isImmuneToCurse;
    public bool IsImmuneToFear => isImmuneToFear;
    public int DealtMeleeDamageMin => dealtMeleeDamageMin;
    public int DealtMeleeDamageMax => dealtMeleeDamageMax;
    public bool CanWarm => canWarm;
    public float WarmChance => warmChance;
    public bool CanBurn => canBurn;
    public float BurnChance => burnChance;
    public bool HasBleedingDamage => hasBleedingDamage;
    public float BleedingChance => bleedingChance;
    public bool HasSlowDamage => hasSlowDamage;
    public float SlowChance => slowChance;
    public bool IsPoisonous => isPoisonous;
    public float PoisonChance => poisonChance;
    public bool HasAcid => hasAcid;
    public float AcidEfficiency => acidEfficiency;
    public bool HasStunDamage => hasStunDamage;
    public float StunChance => stunChance;
    public bool HasRootDamage => hasRootDamage;
    public float RootChance => rootChance;
    public bool HasChillDamage => hasChillDamage;
    public float ChillChance => chillChance;
    public bool HasFrostDamage => hasFrostDamage;
    public float FrostChance => frostChance;
    public bool HasStaticDamage => hasStaticDamage;
    public float StaticChance => staticChance;
    public bool HasParalyzeDamage => hasParalyzeDamage;
    public float ParalyzeChance => paralyzeChance;
    public bool HasCurseDamage => hasCurseDamage;
    public float CurseChance => curseChance;
    public bool HasFearDamage => hasFearDamage;
    public float FearChance => fearChance;
    public bool CanDrainHealth => canDrainHealth;
    public float HealthDrainChance => healthDrainChance;
    public bool HasBlindDamage => hasBlindDamage;
    public float BlindChance => blindChance;

    // MOVEMENT
    public float MaxBaseMoveSpeed => maxBaseMoveSpeed;

    #endregion

    Enemy enemy;

    // INITIALIZATION SYNC
    [SyncVar(hook = nameof(OnAnimSpeedChanged))]
    public float animSpeed;

    // DATA
    [SyncVar] public bool isBoss;
    [SyncVar] public float elementalForgeRate;
    [SyncVar] public int experincePoints;
    [SyncVar] public EnemyBehaviour enemyBehaviour;
    [SyncVar] public EnemyCategory enemyCategory;
    [SyncVar] public float maxBaseMoveSpeed;

    [Header("RESISTANCE DETAILS")]
    [SyncVar] public float physicalResistance;
    [SyncVar] public float magicResistance;
    [SyncVar] public bool hasShield;
    [SyncVar] public float deflectChance;
    [SyncVar] public float deflectionValue;

    [Header("IMMUNITY DETAILS")]
    [SyncVar] public bool isImmuneAfterHit;
    [SyncVar] public float hitImmunityTime;
    [SyncVar] public bool isImmuneToBleeding;
    [SyncVar] public bool isImmuneToStun;
    [SyncVar] public bool isImmuneToSlow;
    [SyncVar] public bool isImmuneToBurn;
    [SyncVar] public bool isImmuneToPoison;
    [SyncVar] public bool isImmuneToRoot;
    [SyncVar] public bool isImmuneToFrost;
    [SyncVar] public bool isImmuneToParalyze;
    [SyncVar] public bool isImmuneToBlind;
    [SyncVar] public bool isImmuneToCurse;
    [SyncVar] public bool isImmuneToFear;

    [Header("ATTACK DETAILS")]
    [SyncVar] public int dealtMeleeDamageMin;
    [SyncVar] public int dealtMeleeDamageMax;
    [SyncVar] public bool canWarm;
    [SyncVar] public float warmChance;
    [SyncVar] public bool canBurn;
    [SyncVar] public float burnChance;
    [SyncVar] public bool hasBleedingDamage;
    [SyncVar] public float bleedingChance;
    [SyncVar] public bool hasSlowDamage;
    [SyncVar] public float slowChance;
    [SyncVar] public bool isPoisonous;
    [SyncVar] public float poisonChance;
    [SyncVar] public bool hasAcid;
    [SyncVar] public float acidEfficiency;
    [SyncVar] public bool hasStunDamage;
    [SyncVar] public float stunChance;
    [SyncVar] public bool hasRootDamage;
    [SyncVar] public float rootChance;
    [SyncVar] public bool hasChillDamage;
    [SyncVar] public float chillChance;
    [SyncVar] public bool hasFrostDamage;
    [SyncVar] public float frostChance;
    [SyncVar] public bool hasStaticDamage;
    [SyncVar] public float staticChance;
    [SyncVar] public bool hasParalyzeDamage;
    [SyncVar] public float paralyzeChance;
    [SyncVar] public bool hasCurseDamage;
    [SyncVar] public float curseChance;
    [SyncVar] public bool hasFearDamage;
    [SyncVar] public float fearChance;
    [SyncVar] public bool canDrainHealth;
    [SyncVar] public float healthDrainChance;
    [SyncVar] public bool hasBlindDamage;
    [SyncVar] public float blindChance;

    EnemyVisualNetwork enemyVisualNetwork;

    private void OnAnimSpeedChanged(float oldVal, float newVal)
    {
        enemy.animator.speed = newVal;
    }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyVisualNetwork = GetComponent<EnemyVisualNetwork>();
    }

    [Server]
    public void ServerInitialize(EnemyDetailsSO details, int spawnIndex, DungeonLevelSO dungeonLevel)
    {
        PopulateEnemyDataIntoSyncVars(details);
        animSpeed = enemy.currentMoveSpeed / Settings.baseSpeedForEnemyAnimations;
        enemy.EnemyInitialization(details, spawnIndex, dungeonLevel, isMultiplayer: true);
        enemyVisualNetwork.ServerInitMaterialize(details);
    }

    [Server]
    public void InitializeRoom(InstantiatedRoom room, Vector2Int[] spawnPositions)
    {
        InstantiatedRoom = room;
        SpawnPositions = spawnPositions;
    }

    [Server]
    public void PopulateEnemyDataIntoSyncVars(EnemyDetailsSO details)
    {
        enemyCategory = details.enemyCategory;
        isBoss = enemy.Isboss;
        experincePoints = details.experiencePoint;
        enemyBehaviour = details.enemyBehaviour;
        maxBaseMoveSpeed = details.movementDetails.GetBaseMaxMoveSpeed();

        physicalResistance = details.physicalResistance;
        magicResistance = details.magicResistance;
        hasShield = details.hasShield;
        deflectChance = details.deflectChance;
        deflectionValue = details.deflectionValue;

        isImmuneToBleeding = details.isImmuneToBleeding;
        isImmuneToStun = details.isImmuneToStun;
        isImmuneToSlow = details.isImmuneToSlow;
        isImmuneToBurn = details.isImmuneToBurn;
        isImmuneToPoison = details.isImmuneToPoison;
        isImmuneToRoot = details.isImmuneToRoot;
        isImmuneToFrost = details.isImmuneToFrost;
        isImmuneToParalyze = details.isImmuneToParalyze;
        isImmuneToBlind = details.isImmuneToBlind;
        isImmuneToCurse = details.isImmuneToCurse;
        isImmuneToFear = details.isImmuneToFear;

        dealtMeleeDamageMin = details.dealtMeleeDamageMin;
        dealtMeleeDamageMax = details.dealtMeleeDamageMax;
        canWarm = details.canWarm;
        warmChance = details.warmChance;
        canBurn = details.canBurn;
        burnChance = details.burnChance;
        hasBleedingDamage = details.hasBleedingDamage;
        bleedingChance = details.bleedingChance;
        hasSlowDamage = details.hasSlowDamage;
        slowChance = details.slowChance;
        isPoisonous = details.isPoisonous;
        poisonChance = details.poisonChance;
        hasAcid = details.hasAcid;
        acidEfficiency = details.acidEfficiency;
        hasStunDamage = details.hasStunDamage;
        stunChance = details.stunChance;
        hasRootDamage = details.hasRootDamage;
        rootChance = details.rootChance;
        hasChillDamage = details.hasChillDamage;
        chillChance = details.chillChance;
        hasFrostDamage = details.hasFrostDamage;
        frostChance = details.frostChance;
        hasStaticDamage = details.hasStaticDamage;
        staticChance = details.staticChance;
        hasParalyzeDamage = details.hasParalyzeDamage;
        paralyzeChance = details.paralyzeChance;
        hasCurseDamage = details.hasCurseDamage;
        curseChance = details.curseChance;
        hasFearDamage = details.hasFearDamage;
        fearChance = details.fearChance;
        canDrainHealth = details.canDrainHealth;
        healthDrainChance = details.healthDrainChance;
        hasBlindDamage = details.hasBlindDamage;
        blindChance = details.blindChance;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public void ResetEnemySpeed()
    {
        enemy.currentMoveSpeed = MaxBaseMoveSpeed + enemy.additionalSpeedModifier - enemy.speedReducer;
        enemy.aiRigidbody2D.canMove = true;
        enemy.aiRigidbody2D.speed = enemy.currentMoveSpeed;
    }
}
