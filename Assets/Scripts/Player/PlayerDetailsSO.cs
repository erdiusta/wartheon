using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDetails_", menuName = "Scriptable Objects/Player/Player Details")]
public class PlayerDetailsSO : ScriptableObject
{
    #region Header PLAYER BASE DETAILS
    [Space(10)]
    [Header("PLAYER BASE DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Player character name")]
    #endregion
    public string playerCharacterName;
    #region Tooltip
    [Tooltip("Player character name index")]
    #endregion
    public Character playerCharacterIndex;
    #region Tooltip
    [Tooltip("Prefab gameobject for the player")]
    #endregion
    public GameObject playerPrefab;
    #region Tooltip
    [Tooltip("Collectible weapons array for the specific selected character")]
    #endregion
    public WeaponDetailsSO[] collectibleWeaponsArray;
    #region Tooltip
    [Tooltip("Player runtime animator controller - ONE HAND")]
    #endregion
    public RuntimeAnimatorController bodyRuntimeAnimatorController;

    #region Header
    [Space(10)]
    [Header("PRIMARY STATS")]
    #endregion
    #region Tooltip
    [Tooltip("Primary stats of the player")]
    #endregion
    public PrimaryStats primaryStats;
    [Space(10)]

    #region Header HEALTH
    [Space(10)]
    [Header("HEALTH")]

    #endregion
    #region Tooltip
    [Tooltip("Select if has immunity period immediately after being hit. If so specify the immunity time in seconds in the other field")]
    #endregion
    public bool isImmuneAfterHit = false;
    #region Tooltip
    [Tooltip("Immunity time in seconds after being hit")]
    #endregion
    public float hitImmunityTime;
    #region Tooltip
    [Tooltip("Get hit sound effect")]
    #endregion
    public SoundEffectSO getHitSoundEffect;
    #region Tooltip
    [Tooltip("Death sound effect")]
    #endregion
    public SoundEffectSO deathSoundEffect;
    #region Tooltip
    [Tooltip("Block sound effect")]
    #endregion
    public SoundEffectSO blockSoundEffect;
    #region Tooltip
    [Tooltip("Dodge sound effect")]
    #endregion
    public SoundEffectSO dodgeSoundEffect;
    #region Tooltip
    [Tooltip("Parry sound effect")]
    #endregion
    public SoundEffectSO parrySoundEffect;

    #region UNIQUE SKILL SETTINGS
    [Header("UNIQUE SKILL SETTINGS")]
    [Header("Passive Skill Details")]
    #endregion
    #region Passive Skill
    [Tooltip("Passive skill image")]
    #endregion
    public Sprite passiveSkillImage;
    #region Passive Skill
    [Tooltip("Passive skill sound effect")]
    #endregion
    public SoundEffectSO passiveSkillSoundEffect;

    #region Active Skill One
    [Header("Active Skill Details")]
    #endregion
    public ActiveUniqueSkillDetailsSO firstActiveSkillDetails;
    public ActiveUniqueSkillDetailsSO secondActiveSkillDetails;
    public ActiveUniqueSkillDetailsSO thirdActiveSkillDetails;
    public ActiveUniqueSkillDetailsSO fourthActiveSkillDetails;
    public ActiveUniqueSkillDetailsSO fifthActiveSkillDetails;

    #region Misc
    [Space(10)]
    [Header("Misc")]
    #endregion
    #region
    [Tooltip("Throwing axe details")]
    #endregion
    public ProjectileDetailsSO throwingAxeDetails;
    #region
    [Tooltip("Shiruken details")]
    #endregion
    public ProjectileDetailsSO shirukenDetails;
    #region
    [Tooltip("Grapple details")]
    #endregion
    public ProjectileDetailsSO grappleDetails;
    #region
    [Tooltip("Ice Breaker details")]
    #endregion
    public ProjectileDetailsSO iceBreakerDetails;
    #region
    [Tooltip("Absolute Zero details")]
    #endregion
    public ProjectileDetailsSO absoluteZeroDetails;
    #region
    [Tooltip("Fire Blast details")]
    #endregion
    public ProjectileDetailsSO fireBlastDetails;
    #region
    [Tooltip("Blazing Cyclone details")]
    #endregion
    public ProjectileDetailsSO blazingCycloneDetails;
    #region
    [Tooltip("Chain Lightning details")]
    #endregion
    public ProjectileDetailsSO chainLightningDetails;
    #region
    [Tooltip("Standard material")]
    #endregion
    public Material standardMaterial;
    #region
    [Tooltip("Penetrate material")]
    #endregion
    public Material penetrateMaterial;

    #region SCREEN SHAKE SETTINGS
    [Space(10)]
    [Header("Screen Shake Settings")]
    #endregion
    #region
    [Tooltip("Check if player can shake the camera")]
    #endregion
    public bool applyScreenShake;
    #region
    [Tooltip("Camera shake intenstiy")]
    #endregion
    public float shakeIntensity = 1f;
    #region
    [Tooltip("Camera shake duration")]
    #endregion
    public float shakeDuration = 0.5f;

    #region Header PASSIVE
    [Space(10)]
    [Header("PASSIVE")]
    #endregion
    #region Tooltip
    [Tooltip("Player's passive items list")]
    #endregion
    public List<PassiveItemDetailsSO> passiveItemsList;

    #region Header WEAPON
    [Space(10)]
    [Header("WEAPON")]
    #endregion
    #region Tooltip
    [Tooltip("Player  initial starting weapon")]
    #endregion
    public WeaponDetailsSO startingWeapon;
    #region Tooltip
    [Tooltip("Player initial starting weapon - Right hand animator controller")]
    #endregion
    public RuntimeAnimatorController mainHandAnimatorController;
    #region Tooltip
    [Tooltip("Player initial starting weapon - Left hand animator controller")]
    #endregion
    public RuntimeAnimatorController offHandAnimatorController;
    #region Tooltip
    [Tooltip("Populate with the list of starting weapons")]
    #endregion
    public List<WeaponDetailsSO> startingWeaponList;

    #region Header OTHER
    [Space(10)]
    [Header("OTHER")]
    #endregion
    #region Tooltip
    [Tooltip("Player icon sprite to be used in the minimap")]
    #endregion
    public Sprite playerMiniMapIcon;
    #region Tooltip
    [Tooltip("Player hand sprite")]
    #endregion
    public Sprite playerHandSprite;
    #region Tooltip
    [Tooltip("Player book sprite")]
    #endregion
    public Sprite playerBookSprite;
    #region Tooltip
    [Tooltip("Level-up sound effect")]
    #endregion
    public SoundEffectSO levelUpSoundEffect;
    #region Tooltip
    [Tooltip("Build activation sound effect")]
    #endregion
    public SoundEffectSO buildActivationSoundEffect;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(playerCharacterName), playerCharacterName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerPrefab), playerPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(startingWeapon), startingWeapon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerMiniMapIcon), playerMiniMapIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandSprite), playerHandSprite);
        HelperUtilities.ValidateCheckNullValue(this, nameof(bodyRuntimeAnimatorController), bodyRuntimeAnimatorController);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(startingWeaponList), startingWeaponList);

        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmunityTime), hitImmunityTime, false);
        }
    }
#endif
    #endregion
}
