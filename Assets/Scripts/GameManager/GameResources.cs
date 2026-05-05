using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Tilemaps;

public class GameResources : MonoBehaviour
{
    private static GameResources instance;

    public static GameResources Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<GameResources>("GameResources");
            }

            return instance;
        }
    }

    #region Header DUNGEON
    [Space(10)]
    [Header("DUNGEON")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
    #endregion
    public RoomNodeTypeListSO roomNodeTypeList;
    #region Tooltip
    [Tooltip("Loading manager prefab")]
    #endregion
    public GameObject loadingManager;

    #region Header DOOR
    [Space(10)]
    [Header("DOOR")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with DoorNS prefab")]
    #endregion
    public GameObject doorNSPrefab;
    #region Tooltip
    [Tooltip("Populate with DoorEW prefab")]
    #endregion
    public GameObject doorEWPrefab;

    #region Header MULTIPLAYER
    [Space(10)]
    [Header("MULTIPLAYER")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the DummyMP prefab")]
    #endregion
    public GameObject dummyMPPrefab;
    #region Tooltip
    [Tooltip("Populate with the CrateMP prefab")]
    #endregion
    public GameObject crateMPPrefab;
    #region Tooltip
    [Tooltip("Populate with the BarrelMP prefab")]
    #endregion
    public GameObject barrelMPPrefab;
    #region Tooltip
    [Tooltip("Populate with the VaseMP prefab")]
    #endregion
    public GameObject vaseMPPrefab;

    #region Header PLAYER SELECTION
    [Space(10)]
    [Header("PLAYER SELECTION")]
    #endregion
    #region Tooltip
    [Tooltip("The PlayerSelection prefab")]
    #endregion
    public GameObject playerSelectionPrefab;

    #region Header PLAYER
    [Space(10)]
    [Header("PLAYER")]
    #endregion Header PLAYER
    #region Tooltip
    [Tooltip("Player details list - populate the list with the playerdetails scriptable object")]
    #endregion
    public PlayerDetailsSO[] playerDetailsArray;
    #region Tooltip
    [Tooltip("The current player scriptable object - this is used to reference the current player between scenes")]
    #endregion Tooltip
    public CurrentPlayerSO currentPlayer;

    #region Header ENEMY
    [Space(10)]
    [Header("ENEMY")]
    #endregion Heade
    #region Tooltip
    [Tooltip("The parent transform holding all spawnpoints for enemies")]
    #endregion Tooltip
    public Transform enemyPatrolPointsParent;
    #region Tooltip
    [Tooltip("The parent transform holding all spawnpoints for enemies - MP")]
    #endregion Tooltip
    public Transform enemyPatrolPointsParentMP;

    #region Header MUSIC
    [Space(10)]
    [Header("MUSIC")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the music master mixer group")]
    #endregion
    public AudioMixerGroup musicMasterMixerGroup;
    #region Tooltip
    [Tooltip("Main menu music scriptable object")]
    #endregion
    public MusicTrackSO mainMenuMusic;
    #region Tooltip
    [Tooltip("Cutscene music scriptable object")]
    #endregion
    public MusicTrackSO cutsceneMusic;
    #region Tooltip
    [Tooltip("Ambient music used for tutorial")]
    #endregion
    public MusicTrackSO ambientMusic;
    #region Tooltip
    [Tooltip("Combat music used for tutorial")]
    #endregion
    public MusicTrackSO combatMusic;
    #region Tooltip
    [Tooltip("Music on full snapshot")]
    #endregion
    public AudioMixerSnapshot musicOnFullSnaphot;
    #region Tooltip
    [Tooltip("Music low snapshot")]
    #endregion
    public AudioMixerSnapshot musicLowSnapshot;
    #region Tooltip
    [Tooltip("Music off snapshot")]
    #endregion
    public AudioMixerSnapshot musicOffSnapshot;

    #region Header SOUNDS
    [Space(10)]
    [Header("SOUNDS")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the sounds master mixer group")]
    #endregion
    public AudioMixerGroup soundMasterMixerGroup;
    #region Tooltip
    [Tooltip("Tutorial phase pass soundEffect")]
    #endregion
    public SoundEffectSO tutorialPhasePassSoundEffect;
    #region Tooltip
    [Tooltip("CNext level musics")]
    #endregion
    public SoundEffectSO nextLevelSoundEffect;
    #region Tooltip
    [Tooltip("Open book sound effect")]
    #endregion Tooltip
    public SoundEffectSO openBookSoundEffect;
    #region Tooltip
    [Tooltip("Close book sound effect")]
    #endregion Tooltip
    public SoundEffectSO closeBookSoundEffect;
    #region Tooltip
    [Tooltip("Invalid action sound effect")]
    #endregion Tooltip
    public SoundEffectSO invalidActionSoundEffect;
    #region Tooltip
    [Tooltip("Door open close sound effect")]
    #endregion Tooltip
    public SoundEffectSO doorOpenCloseSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the chest open sound effect")]
    #endregion
    public SoundEffectSO chestOpen;
    #region Tooltip
    [Tooltip("Populate with the chest lock sound effect")]
    #endregion
    public SoundEffectSO chestLock;
    #region Tooltip
    [Tooltip("Populate with the coin pickup sound effect")]
    #endregion
    public SoundEffectSO coinPickup;
    #region Tooltip
    [Tooltip("Populate with the health pickup sound effect")]
    #endregion
    public SoundEffectSO healthPickup;
    #region Tooltip
    [Tooltip("Populate with the item pickup sound effect")]
    #endregion
    public SoundEffectSO itemPickup;
    #region Tooltip
    [Tooltip("Populate with the weapon pickup sound effect")]
    #endregion
    public SoundEffectSO weaponPickup;

    #region Header SOUNDS
    [Space(10)]
    [Header("STATUS EFFECT SOUNDS")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the root status sound effect")]
    #endregion
    public SoundEffectSO rootSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the heal status sound effect")]
    #endregion
    public SoundEffectSO healSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the stun status sound effect")]
    #endregion
    public SoundEffectSO stunSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the paralyze status sound effect")]
    #endregion
    public SoundEffectSO paralyzeSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the freeze status sound effect")]
    #endregion
    public SoundEffectSO freezeSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the poison status sound effect")]
    #endregion
    public SoundEffectSO poisonSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the bleeding status sound effect")]
    #endregion
    public SoundEffectSO bleedingSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the burn status sound effect")]
    #endregion
    public SoundEffectSO burnSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the curse status sound effect")]
    #endregion
    public SoundEffectSO curseSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the blind status sound effect")]
    #endregion
    public SoundEffectSO blindSoundEffect;
    #region Tooltip
    [Tooltip("Populate with the fear status sound effect")]
    #endregion
    public SoundEffectSO fearSoundEffect;

    #region Header PRIMARY PASSIVE ITEMS
    [Space(10)]
    [Header("PRIMARY PASSIVES")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the health passive item")]
    #endregion
    public PassiveItemDetailsSO healthPassiveItem;
    #region Tooltip
    [Tooltip("Populate with the coin passive item")]
    #endregion
    public PassiveItemDetailsSO coinPassiveItem;

    #region Header PRIMARY SECONDARY ITEMS
    [Space(10)]
    [Header("SECONDARY PASSIVES")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the secondary passive item - For Tutorial")]
    #endregion
    public PassiveItemDetailsSO secondaryPassiveItem;

    #region Header MATERIALS
    [Space(10)]
    [Header("MATERIALS")]
    #endregion
    #region Tooltip
    [Tooltip("Dimmed Material")]
    #endregion
    public Material dimmedMaterial;
    #region Tooltip
    [Tooltip("Sprite-Lit-Default Material")]
    #endregion
    public Material litMaterial;
    #region Tooltip
    [Tooltip("Populate with the Materialize Shader")]
    #endregion
    public Shader materializeShader;

    #region Header SPECIAL TILEMAP TILES
    [Space(10)]
    [Header("SPECIAL TILEMAP TILES")]
    #endregion Header SPECIAL TILEMAP TILES
    #region Tooltip
    [Tooltip("Collision tiles that the enemies can navigate to")]
    #endregion Tooltip
    public TileBase[] enemyUnwalkableCollisionTilesArray;
    #region Tooltip
    [Tooltip("Preferred path tile for enemy navigation")]
    #endregion Tooltip
    public TileBase preferredEnemyPathTile;

    #region Header UI
    [Space(10)]
    [Header("Level UI")]
    #endregion
    public Sprite levelOneFrameSprite;
    public Sprite levelTwoFrameSprite;
    public Sprite levelThreeFrameSprite;

    #region Header UI
    [Space(10)]
    [Header("Book UI")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with book item image prefab")]
    #endregion
    public GameObject bookWeaponSlot;
    #region
    [Tooltip("Populate with the lock slot image")]
    #endregion
    public Sprite lockSlotIcon;

    #region Header
    [Space(10)]
    [Header("DUST")]
    #endregion
    #region Tooltip
    [Tooltip("Dust trail prefab")]
    #endregion
    public GameObject dustTrailPrefab;

    #region Header NPC
    [Space(10)]
    [Header("NPC")]
    #endregion
    #region Tooltip
    [Tooltip("NPC arrays")]
    #endregion
    public GameObject[] npcPrefabs;
    #region Tooltip
    [Tooltip("Gamble dice animator")]
    #endregion
    public RuntimeAnimatorController gambleDiceAnimatorController;

    #region Header CHESTS
    [Space(10)]
    [Header("CHESTS")]
    #endregion
    #region Tooltip
    [Tooltip("Chest item prefab")]
    #endregion
    public GameObject chestItemPrefab;
    #region Tooltip
    [Tooltip("Chest item prefab - Multiplayer")]
    #endregion
    public GameObject chestItemNetworkPrefab;
    #region Tooltip
    [Tooltip("Populate with heart icon sprite")]
    #endregion
    public Sprite heartIcon;
    #region Tooltip
    [Tooltip("Populate with ammo hover animator controller")]
    #endregion
    public RuntimeAnimatorController ammoHoverAnimatorController;
    #region Tooltip
    [Tooltip("Populate with lock icon sprite")]
    #endregion
    public Sprite lockIcon;

    #region Header STATUS EFFECT
    [Space(10)]
    [Header("STATUS EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("Status effect prefab")]
    #endregion
    public GameObject statusEffectPrefab;

    #region Header NETWORK SKILL PREFABS
    [Space(10)]
    [Header("NETWORK SKILL PREFABS")]
    #endregion
    #region Tooltip
    [Tooltip("Umbral Mist prefab")]
    #endregion
    public GameObject umbralMistNetworkPrefab;
    #region Tooltip
    [Tooltip("Blizzard prefab")]
    #endregion
    public GameObject blizzardNetworkPrefab;
    #region Tooltip
    [Tooltip("Absolute Zero prefab")]
    #endregion
    public GameObject absoluteZeroPrefab;
    #region Tooltip
    [Tooltip("Flame lotus prefab")]
    #endregion
    public GameObject flameLotusPrefab;
    #region Tooltip
    [Tooltip("Mist of Disruption prefab")]
    #endregion
    public GameObject mistOfDisruptionPrefab;
    #region Tooltip
    [Tooltip("Eye of the storm prefab")]
    #endregion
    public GameObject eyeOfTheStormPrefab;

    #region Header MINIMAP
    [Space(10)]
    [Header("MINIMAP")]
    #endregion
    #region Tooltip
    [Tooltip("Minimap boss prefab")]
    #endregion
    public GameObject minimapBossPrefab;

    public GameObject GetMPPrefab(MPReplaceType type)
    {
        return type switch
        {
            MPReplaceType.Dummy => dummyMPPrefab,
            MPReplaceType.Barrel => barrelMPPrefab,
            MPReplaceType.Crate => crateMPPrefab,
            MPReplaceType.Vase => vaseMPPrefab,
            _ => null
        };
    }

    public void SavePlayerData()
    {
        if (currentPlayer.playerDetails != null)
        {
            PlayerPrefs.SetString("CurrentPlayerDetails", JsonUtility.ToJson(currentPlayer.playerDetails));
            PlayerPrefs.Save();
        }
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("CurrentPlayerDetails"))
        {
            currentPlayer.playerDetails = JsonUtility.FromJson<PlayerDetailsSO>(PlayerPrefs.GetString("CurrentPlayerDetails"));
        }
        else if (playerDetailsArray.Length > 0)
        {
            currentPlayer.playerDetails = playerDetailsArray[0]; // Default character
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeTypeList), roomNodeTypeList);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerSelectionPrefab), playerSelectionPrefab);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(playerDetailsArray), playerDetailsArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(currentPlayer), currentPlayer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundMasterMixerGroup), soundMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorOpenCloseSoundEffect), doorOpenCloseSoundEffect);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestOpen), chestOpen);
        HelperUtilities.ValidateCheckNullValue(this, nameof(coinPickup), coinPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(healthPickup), healthPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(itemPickup), itemPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponPickup), weaponPickup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(litMaterial), litMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(dimmedMaterial), dimmedMaterial);
        HelperUtilities.ValidateCheckNullValue(this, nameof(materializeShader), materializeShader);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyUnwalkableCollisionTilesArray), enemyUnwalkableCollisionTilesArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(preferredEnemyPathTile), preferredEnemyPathTile);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicMasterMixerGroup), musicMasterMixerGroup);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOnFullSnaphot), musicOnFullSnaphot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicLowSnapshot), musicLowSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicOffSnapshot), musicOffSnapshot);
        HelperUtilities.ValidateCheckNullValue(this, nameof(chestItemPrefab), chestItemPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(heartIcon), heartIcon);
        HelperUtilities.ValidateCheckNullValue(this, nameof(minimapBossPrefab), minimapBossPrefab);
    }
#endif
    #endregion
}