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
    public RuntimeAnimatorController oneHandRuntimeAnimatorController;
    #region Tooltip
    [Tooltip("Player runtime animator controller - TWO HAND")]
    #endregion
    public RuntimeAnimatorController twoHandRuntimeAnimatorController;
    #region Tooltip
    [Tooltip("Player runtime animator controller - BOW")]
    #endregion
    public RuntimeAnimatorController bowRuntimeAnimatorController;
    #region Tooltip
    [Tooltip("Player runtime animator controller - STAFF")]
    #endregion
    public RuntimeAnimatorController staffRuntimeAnimatorController;

    #region Header HEALTH
    [Space(10)]
    [Header("HEALTH")]
    #endregion
    #region Tooltip
    [Tooltip("Player starting health amount")]
    #endregion
    public int playerHealthAmount;
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


    #region SPECIAL MOVE SETTINGS
    [Space(10)]
    [Header("Special Move Settings")]
    #endregion
    #region
    [Tooltip("Spacial move name")]
    #endregion
    public string specialMoveName = "";
    #region Tooltip
    [Tooltip("Special move sound effect")]
    #endregion
    public SoundEffectSO specialMoveOneSoundEffect;
    #region
    [Tooltip("Spacial move cooldown duration")]
    #endregion
    public float specialMoveOneCooldownDuration = 20f;
    #region
    [Tooltip("Spacial move duration")]
    #endregion
    public float specialMoveOneDuration = 0f;
    #region Tooltip
    [Tooltip("Special move sound effect")]
    #endregion
    public SoundEffectSO specialMoveTwoSoundEffect;
    #region
    [Tooltip("Spacial move cooldown duration")]
    #endregion
    public float specialMoveTwoCooldownDuration = 20f;
    #region
    [Tooltip("Spacial move duration")]
    #endregion
    public float specialMoveTwoDuration = 0f;
    #region Tooltip
    [Tooltip("Special move sound effect")]
    #endregion
    public SoundEffectSO specialMoveThreeSoundEffect;
    #region
    [Tooltip("Spacial move cooldown duration")]
    #endregion
    public float specialMoveThreeCooldownDuration = 20f;
    #region
    [Tooltip("Spacial move duration")]
    #endregion
    public float specialMoveThreeDuration = 0f;
    #region
    [Tooltip("Check if on stealth mode")]
    #endregion
    public bool onStealth = false;

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

    #region Header ACTIVE
    [Space(10)]
    [Header("ACTIVE")]
    #endregion
    #region Tooltip
    [Tooltip("Player's current active item")]
    #endregion
    public ActiveItemDetailsSO selectedActiveItem;
    #region Tooltip
    [Tooltip("Player's active items list")]
    #endregion
    public List<ActiveItemDetailsSO> activeItemsList;

    #region Header PASSIVE
    [Space(10)]
    [Header("PASSIVE")]
    #endregion
    #region Tooltip
    [Tooltip("Player starting armor amount")]
    #endregion
    public int playerArmorValue = 0;
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(playerCharacterName), playerCharacterName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerPrefab), playerPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(playerHealthAmount), playerHealthAmount, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(startingWeapon), startingWeapon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerMiniMapIcon), playerMiniMapIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerHandSprite), playerHandSprite);
        HelperUtilities.ValidateCheckNullValue(this, nameof(oneHandRuntimeAnimatorController), oneHandRuntimeAnimatorController);
        HelperUtilities.ValidateCheckNullValue(this, nameof(twoHandRuntimeAnimatorController), twoHandRuntimeAnimatorController);
        HelperUtilities.ValidateCheckNullValue(this, nameof(bowRuntimeAnimatorController), bowRuntimeAnimatorController);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(playerArmorValue), playerArmorValue, true);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(startingWeaponList), startingWeaponList);

        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmunityTime), hitImmunityTime, false);
        }
    }
#endif
    #endregion
}
