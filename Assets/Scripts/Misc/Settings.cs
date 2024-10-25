using UnityEngine;

public static class Settings
{
    #region UNITS
    public const float pixelsPerUnit = 16f;
    public const float tileSizePixels = 16f;
    #endregion

    #region DUNGEON BUILD SETTINGS
    public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
    public const int maxDungeonBuildAttempts = 10;
    #endregion

    #region ROOM SETTINGS
    // Time to fade in the room
    public const float fadeInTime = 0.5f;
    // Max number of child corridors leading from a room. - maximum should be 3 although this is not recommended since it can cause the dungeon
    // building to fail since the rooms are more likely to not fit together;
    public const int maxChildCorridors = 3;
    public const float doorUnlockDelay = 1f;
    #endregion

    #region AUDIO
    public const float musicFadeOutTime = 0.5f; // Default music fade out transition
    public const float musicFadeInTime = 0.5f; // Default music fade in transition
    #endregion

    #region ANIMATOR PARAMETERS
    // Animator parameters - Player
    public static int aimUp = Animator.StringToHash("aimUp");
    public static int aimDown = Animator.StringToHash("aimDown");
    public static int aimUpRight = Animator.StringToHash("aimUpRight");
    public static int aimUpLeft = Animator.StringToHash("aimUpLeft");
    public static int aimRight = Animator.StringToHash("aimRight");
    public static int aimLeft = Animator.StringToHash("aimLeft");
    public static int isIdle = Animator.StringToHash("isIdle");
    public static int isMoving = Animator.StringToHash("isMoving");
    public static int rollUp = Animator.StringToHash("rollUp");
    public static int rollDown = Animator.StringToHash("rollDown");
    public static int rollRight = Animator.StringToHash("rollRight");
    public static int rollLeft = Animator.StringToHash("rollLeft");
    public static int use = Animator.StringToHash("use");
    public static int isAttacking = Animator.StringToHash("attack");
    public static int motionType = Animator.StringToHash("motionType");
    public static int axisX = Animator.StringToHash("axisX");
    public static int axisY = Animator.StringToHash("axisY");
    public const float baseSpeedForPlayerAnimations = 6f;

    // Animator parameters - Status
    public static int isStunned = Animator.StringToHash("isStunned");
    public static int isFrozen = Animator.StringToHash("isFrozen");

    // Animator parameters - Damage
    public static int death = Animator.StringToHash("death");
    public static int getHit = Animator.StringToHash("getHit");

    // Animator parameters - MeleeAttack
    public static int meleeAttackAtRightHand = Animator.StringToHash("AttackAtRightHand");
    public static int meleeAttackAtLeftHand = Animator.StringToHash("AttackAtLeftHand");
    public static int isLeft = Animator.StringToHash("isLeft");

    // Animator parameters - Shield
    public static int block = Animator.StringToHash("block");

    // Animator parameters - Enemy
    public const float baseSpeedForEnemyAnimations = 2.5f;

    // Animator parameters - Door
    public static int open = Animator.StringToHash("open");

    // Animator parameters - DamageableDecoration
    public static int destroy = Animator.StringToHash("destroy");
    public static string stateDestroyed = "Destroyed";

    // Animator parameters - DropItem
    public static int hovered = Animator.StringToHash("hovered");

    // Animator parameters - Book
    public static int turnPage = Animator.StringToHash("turnPage");
    public static int zoomIn = Animator.StringToHash("zoomIn");
    public static int zoomOut = Animator.StringToHash("zoomOut");
    #endregion

    #region GAMEOBJECT TAGS
    public const string playerTag = "Player";
    public const string playerWeapon = "playerWeapon";
    public const string enemyTag = "Enemy";
    public const string decoyTag = "Dummy";
    public const string summonedEnemyTag = "summonedEnemy";
    public const string chestItemTag = "Chest Item";
    public const string mainHandSlot = "mainHandSlot";
    public const string offHandSlot = "offHandSlot";
    public const string bookCover = "bookCover";
    public const string weaponSetButton = "weaponSetButton";
    public const string dropButton = "dropButton";
    public const string playerProjectile = "playerProjectile";
    public const string meteor = "meteor";
    public const string enemyProjectile = "enemyProjectile";
    #endregion

    #region FIRING CONTROL
    // If the target distance is less than this then the aim angle will be used (calculated from player), else the weapon aim angle
    // will be used (calculated from the weapon).
    public const float useAimAngleDistance = 3.5f;
    #endregion

    #region ASTAR PATHFINDING PARAMETERS
    public const int defaultAStarMovementPenalty = 30;
    public const int preferredPathAStarMovementPenalty = 1;
    public const int targetFrameRateToSpreadPathfindingOver = 40;
    public const float playerMoveDistanceToRebuildPath = 3f;
    public const float enemyPatrolPathRebuildCooldown = 1.5f;
    public const float enemyChasePathRebuildCooldown = 0.3f;
    #endregion

    #region ENEMY PARAMETERS
    public const int defaultEnemyHealth = 20;
    #endregion

    #region CHARACTER NAMES
    public const string astraeus = "Astraeus";
    public const string orion = "Orion";
    public const string erebus = "Erebus";
    public const string lyrisa = "Lyrisa";
    #endregion

    #region UI PARAMETERS
    public const float uiHeartSpacing = 16f;
    public const float uiProjectileIconSpacing = 4f;
    #endregion

    #region CONTACT DAMAGE PARAMETERS
    public const float contactDamageCollisionResetDelay = 0.5f;
    #endregion
}
