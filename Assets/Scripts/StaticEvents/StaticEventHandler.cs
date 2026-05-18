using System;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEventHandler
{
    // Main menu scene loaded for transition from tutorial
    public static event Action OnMainMenuLoaded;

    public static void CallLoadMainMenuScene()
    {
        OnMainMenuLoaded?.Invoke();
    }

    // Dungen completed event
    public static event Action OnDungeonBuilt;

    public static void CallDungeonBuiltEvent()
    {
        OnDungeonBuilt?.Invoke();
    }

    // Additive scene removed
    public static event Action OnAdditiveSceneRemoved;

    public static void CallAdditiveSceneRemoveEvent()
    {
        OnAdditiveSceneRemoved?.Invoke();
    }

    // Character button selected
    public static event Action<CharacterButtonArgs> OnCharacterButtonSelected;

    public static void CallCharacterButtonSelectedEvent(Character charIndex)
    {
        OnCharacterButtonSelected?.Invoke(new CharacterButtonArgs { charIndex = charIndex });
    }

    // Character buttons deselected
    public static event Action OnCharacterButtonDeselected;

    public static void CallCharacterButtonDeselectedEvent()
    {
        OnCharacterButtonDeselected?.Invoke();
    }

    // Stat point increased
    public static event Action OnStatPointChanged;

    public static void CallStatPointChangedEvent()
    {
        OnStatPointChanged?.Invoke();
    }

    // Dynamic camera follow toggle changed
    public static event Action<DynamicCameraFollowArgs> OnDynamicCameraToggled;

    public static void CallDynamicCameraToggled(bool isOn)
    {
        OnDynamicCameraToggled?.Invoke(new DynamicCameraFollowArgs { isOn = isOn });
    }

    // Overview camera enabled
    public static event Action<OverviewCameraFollowArgs> OnOverviewCameraToggled;

    public static void CallOverviewCameraToggled(bool isOn)
    {
        OnOverviewCameraToggled?.Invoke(new OverviewCameraFollowArgs { isOn = isOn });
    }

    // Cheat code activated
    public static event Action OnCheatActivated;

    public static void CallCheatActivatedEvent()
    {
        OnCheatActivated?.Invoke();
    }

    // Inner Path Info Hovered
    public static event Action<SkillPointsArgs> OnInnerPathInfoHovered;

    public static void CallInnerPathInfoHoveredEvent(InnerPathDetailsSO innerPathDetails)
    {
        OnInnerPathInfoHovered?.Invoke(new SkillPointsArgs { innerPathDetails = innerPathDetails });
    }

    // Inner Path Info Unhovered
    public static event Action<SkillPointsArgs> OnInnerPathInfoUnhovered;

    public static void CallInnerPathInfoUnhoveredEvent(InnerPathDetailsSO innerPathDetails)
    {
        OnInnerPathInfoUnhovered?.Invoke(new SkillPointsArgs { innerPathDetails = innerPathDetails });
    }

    // Inner path point used
    public static event Action<SkillPointsArgs> OnSkillPointUsed;

    public static void CallSkillPointsUsed(InnerPathDetailsSO innerPathDetails)
    {
        OnSkillPointUsed?.Invoke(new SkillPointsArgs { innerPathDetails = innerPathDetails });
    }

    // Unique Skill Info Hovered
    public static event Action<SkillPointsArgs> OnUniqueSkillInfoHovered;

    public static void CallUniqueSkillInfoHoveredEvent(ActiveUniqueSkillDetailsSO activeUniqueSkillContainer)
    {
        OnUniqueSkillInfoHovered?.Invoke(new SkillPointsArgs { activeUniqueSkillContainer = activeUniqueSkillContainer });
    }

    // Unique Skill Info Unhovered
    public static event Action<SkillPointsArgs> OnUniqueSkillInfoUnhovered;

    public static void CallUniqueSkillInfoUnhoveredEvent(ActiveUniqueSkillDetailsSO activeUniqueSkillContainer)
    {
        OnUniqueSkillInfoUnhovered?.Invoke(new SkillPointsArgs { activeUniqueSkillContainer = activeUniqueSkillContainer });
    }

    // Skill point boost used
    public static event Action<SkillBoostArgs> OnSkillBoostUsed;

    public static void CallSkillBoostUsed(int skillBoostLevel)
    {
        OnSkillBoostUsed?.Invoke(new SkillBoostArgs { skillBoostLevel = skillBoostLevel });
    }

    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room, RoomNetData roomNetData = default)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs { room = room,  roomNetData = roomNetData});
    }

    // Npc interaction start
    public static event Action<NpcInteractionStartedArgs> OnNPCInteractionStarted;

    public static void CallNPCInteractionStartedEvent(NpcType npcType)
    {
        OnNPCInteractionStarted?.Invoke(new NpcInteractionStartedArgs { npcType = npcType });
    }

    // Npc interaction end
    public static event Action OnNPCInteractionEnded;


    public static void CallNPCInteractionEndedEvent()
    {
        OnNPCInteractionEnded?.Invoke();
    }

    // All enemies cleared in the room
    public static event Action OnEnemiesCleared;

    public static void CallEnemiesClearedEvent()
    {
        OnEnemiesCleared?.Invoke();
    }

    // Stats changed event
    public static event Action OnPrimaryStatsChanged;

    public static void CallStatsChangedOnTheBookEvent()
    {
        OnPrimaryStatsChanged?.Invoke();
    }

    // Room enemies defeated event
    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;

    public static void CallRoomEnemiesDefeatedEvent(Room room, RoomNetData roomNetData, List<GameObject> summonedEnemies)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs { room = room, data = roomNetData, summonedEnemies = summonedEnemies });
    }

    // Room enemies defeated event
    public static event Action<RoomEnemiesDefeatedArgs> OnRoomMPEnemiesDefeated;

    public static void CallRoomEnemiesDefeatedEventMP(RoomNetData data, List<GameObject> summonedEnemies)
    {
        OnRoomMPEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs { data = data, summonedEnemies = summonedEnemies });
    }

    // Camera shaken event
    public static event Action<CameraShakeArgs> OnCameraShaken;

    public static void CallCameraShakeEvent(float shakeIntensity, float shakeDuration)
    {
        OnCameraShaken?.Invoke(new CameraShakeArgs { shakeIntensity = shakeIntensity, shakeDuration = shakeDuration });
    }

    // Place new skill to slot
    public static event Action<ActiveUniqueSkillPlacedArgs> OnActiveUniqueSkillPlaced;

    public static void CallActiveUniqueSkillPlacedEvent(int placedSlotIndex, ActiveUniqueSkillDetailsSO activeUniqueSkillDetails, bool slotDrop)
    {
        OnActiveUniqueSkillPlaced?.Invoke(new ActiveUniqueSkillPlacedArgs { placedSlotIndex = placedSlotIndex, activeUniqueSkillDetails = activeUniqueSkillDetails,
            slotDrop = slotDrop});
    }

    // Open book's stat page
    public static event Action OnStatPageOpened;

    public static void CallOpenStatPageEvent()
    {
        OnStatPageOpened?.Invoke();
    }

    // Open book's build page
    public static event Action OnBuildPageOpened;

    public static void CallOpenBuildPageEvent()
    {
        OnBuildPageOpened?.Invoke();
    }

    // Book weapon switch event
    public static event Action OnWeaponSwitched;

    public static void CallWeaponSwitchedEventForBook()
    {
        OnWeaponSwitched?.Invoke();
    }

    // Gamble completed event
    public static event Action<GambleArgs> OnGambleCompleted;

    public static void CallGambleCompletedEvent(Player caller)
    {
        OnGambleCompleted?.Invoke(new GambleArgs { caller = caller});
    }

    // Book weapon pick-up event
    public static event Action<WeaponAddedToBookArgs> OnWeaponPickedUp;

    public static void CallWeaponPickedUpEventForBook(Weapon weapon, bool pickedUpByOffHand = false)
    {
        OnWeaponPickedUp?.Invoke(new WeaponAddedToBookArgs { weapon = weapon, pickedUpByOffHand = pickedUpByOffHand });
    }

    // Book weapon removed from slot
    public static event Action<WeaponAddedToBookArgs> OnWeaponRemoved;

    public static void CallWeaponRemovedFromEquippedSlot(SlotType slotType)
    {
        OnWeaponRemoved?.Invoke(new WeaponAddedToBookArgs { slotType = slotType });
    }

    // Book weapon pick-up and tranport to inventory event
    public static event Action<WeaponAddedToBookArgs> OnWeaponAddedToInventory;

    public static void CallOnWeaponAddedToInventoryEventForBook(Weapon weapon, int inventoryIndexNumber)
    {
        OnWeaponAddedToInventory?.Invoke(new WeaponAddedToBookArgs { weapon = weapon, inventoryIndexNumber = inventoryIndexNumber});
    }

    // Book weapon 
    public static event Action<WeaponAddedToBookArgs> OnInventoryWeaponDropped;

    public static void CallInventoryWeaponDroppedEventForBook(int inventoryIndexNumber)
    {
        OnInventoryWeaponDropped?.Invoke(new WeaponAddedToBookArgs { inventoryIndexNumber = inventoryIndexNumber });
    }

    public static event Action<WeaponAddedToBookArgs> OnInventoryItemRemoved;

    public static void CallInventoryItemRemovedForBook(int inventoryIndexNumber)
    {
        OnInventoryItemRemoved?.Invoke(new WeaponAddedToBookArgs { inventoryIndexNumber = inventoryIndexNumber });
    }

    // Book weapon drop event
    public static event Action<WeaponAddedToBookArgs> OnWeaponDropped;

    public static void CallWeaponDroppedEventForBook(SlotType slotType)
    {
        OnWeaponDropped?.Invoke(new WeaponAddedToBookArgs { slotType = slotType });
    }

    // Book weapon upgraded (in-place, inventory)
    public static event Action<InventoryWeaponUpgradedArgs> OnInventoryWeaponUpgraded;
    public static void CallInventoryWeaponUpgradedEventForBook(Weapon weapon, int inventoryIndexNumber)
    {
        OnInventoryWeaponUpgraded?.Invoke(new InventoryWeaponUpgradedArgs
        {
            inventoryIndexNumber = inventoryIndexNumber,
            weapon = weapon
        });
    }

    // Book passive upgraded (in-place, inventory)
    public static event Action<InventoryPassiveUpgradedArgs> OnInventoryPassiveUpgraded;
    public static void CallInventoryPassiveUpgradedEventForBook(PassiveItem passiveItem, int inventoryIndexNumber)
    {
        OnInventoryPassiveUpgraded?.Invoke(new InventoryPassiveUpgradedArgs
        {
            inventoryIndexNumber = inventoryIndexNumber,
            passiveItem = passiveItem
        });
    }

    // Item added to passiveitem slot on book event
    public static event Action<PassiveItemAddedToBookArgs> OnItemAddedToPassiveItemSlot;

    public static void CallItemAddedToPassiveItemSlot(PassiveItem passiveItem, PassiveItemSlotName itemSlotName)
    {
        OnItemAddedToPassiveItemSlot?.Invoke(new PassiveItemAddedToBookArgs { passiveItem = passiveItem, itemSlotName = itemSlotName });
    }

    // Item added to passiveitem inventory slot on book event
    public static event Action<PassiveItemAddedToBookArgs> OnPassiveItemAddedToInventorySlot;

    public static void CallPassiveItemAddedToInventorySlot(PassiveItem passiveItem, int inventoryIndexNumber)
    {
        OnPassiveItemAddedToInventorySlot?.Invoke(new PassiveItemAddedToBookArgs { passiveItem = passiveItem, inventoryIndexNumber = inventoryIndexNumber});
    }

    // Item removed from passiveitem inventory slot on book event
    public static event Action<PassiveItemAddedToBookArgs> OnInventoryPassiveItemDropped;

    public static void CallInventoryPassiveItemDroppedEventForBook(int inventoryIndexNumber)
    {
        OnInventoryPassiveItemDropped?.Invoke(new PassiveItemAddedToBookArgs { inventoryIndexNumber = inventoryIndexNumber });
    }

    // Item removed from passive item slot on book event
    public static event Action<PassiveItemRemovedFromBookArgs> OnItemRemovedFromPassiveItemSlot;

    public static void CallItemRemovedFromPassiveItemSlot(PassiveItemSlotName itemSlotName)
    {
        OnItemRemovedFromPassiveItemSlot?.Invoke(new PassiveItemRemovedFromBookArgs { itemSlotName = itemSlotName});
    }

    // Passive items swapped between passive slot and inventory
    public static event Action<PassiveItemAddedToBookArgs> OnPassiveItemsSwapped;

    public static void CallPassiveItemsSwappedEvent(PassiveItem slotPassiveItem, PassiveItem inventoryPassiveItem, int inventoryIndexNumber)
    {
        OnPassiveItemsSwapped?.Invoke(new PassiveItemAddedToBookArgs { passiveItem = slotPassiveItem, inventoryPassiveItem = inventoryPassiveItem, 
            inventoryIndexNumber = inventoryIndexNumber
        });
    }

    // Weapons swapped between weapon slot and inventory
    public static event Action<WeaponAddedToBookArgs> OnWeaponsSwappedWithInventory;

    public static void CallWeaponsSwappedWithInventoryEvent(Weapon slotWeapon, Weapon inventoryWeapon, int inventoryIndexNumber, int weaponSetNumber, bool onMainHand, 
        DraggableItem slotWeaponDraggableItem, DraggableItem inventoryWeaponDraggableItem)
    {
        OnWeaponsSwappedWithInventory?.Invoke(new WeaponAddedToBookArgs { weapon = slotWeapon, intentoryWeapon = inventoryWeapon, inventoryIndexNumber = inventoryIndexNumber,
            weaponSetNumber = weaponSetNumber, onMainHand = onMainHand, slotWeaponDraggableItem = slotWeaponDraggableItem, inventoryWeaponDraggableItem = inventoryWeaponDraggableItem});
    }

    // Items placed to another inventory slot
    public static event Action<ItemGenericPlacedArgs> OnGenericItemPlacedToEmptyInventory;

    public static void CallGenericItemPlacedToEmptyInInventory(ItemGeneric item,int fromIndex, int toIndex)
    {
        OnGenericItemPlacedToEmptyInventory?.Invoke(new ItemGenericPlacedArgs{ item = item, fromIndex = fromIndex, toIndex = toIndex});
    }

    // Items swapped between inventory
    public static event Action<ItemGenericSwappedArgs> OnGenericItemsSwappedInInventory;

    public static void CallGenericItemsSwappedInInventory(DraggableItem draggableItem, DraggableItem targetItem, int draggableItemInventoryIndex, int targetItemInventoryIndex)
    {
        OnGenericItemsSwappedInInventory?.Invoke(new ItemGenericSwappedArgs { draggableItem = draggableItem, targetItem = targetItem, 
            draggableItemInventoryIndex = draggableItemInventoryIndex, targetItemInventoryIndex = targetItemInventoryIndex});
    }

    // Swap failed event
    public static event Action<SwapFailedArgs> OnSwapFailed;

    public static void CallSwapFailedEvent(PopUpReason reason)
    {
        GameManager.Instance.OpenPopUpLog(reason);

        OnSwapFailed?.Invoke(new SwapFailedArgs { reason = reason });
    }

    // Health change on book event
    public static event Action<HealthChangedArgs> OnBookHealthChanged;

    public static void CallBookHealthChangedEvent(int currentHealth)
    {
        OnBookHealthChanged?.Invoke(new HealthChangedArgs { currentHealth = currentHealth });
    }

    // Mana change on book event
    public static event Action<ManaChangedArgs> OnBookManaChanged;

    public static void CallBookManaChangedEvent(int currentMana)
    {
        OnBookManaChanged?.Invoke(new ManaChangedArgs { currentMana = currentMana });
    }

    // Introduction ui screen opened event
    public static event Action<IntroductionPopUpUIArgs> OnDropPickedUp;

    public static void CallIntroductionPopUpEvent(ItemType dropType, ItemGeneric receivable)
    {
        OnDropPickedUp?.Invoke(new IntroductionPopUpUIArgs { dropType = dropType, receivable = receivable });
    }

    // Decoy added to the room event
    public static event Action<DecoySpawnedArgs> OnDecoySpawned;

    public static void CallDecoySpawned(Dummy decoy)
    {
        OnDecoySpawned?.Invoke(new DecoySpawnedArgs { decoy = decoy });
    }

    // Hourglass added to the room event
    public static event Action OnHourglassSpawned;

    public static void CallHourglassSpawned()
    {
        OnHourglassSpawned?.Invoke();
    }

    // Hourglass effect wore out
    public static event Action OnHourglasExpired;

    public static void CallHourglassExpired()
    {
        OnHourglasExpired?.Invoke();
    }

    // Compass enabled
    public static event Action OnCompassEnabled;

    public static void CallCompassEnabled()
    {
        OnCompassEnabled?.Invoke();
    }

    // Compass disabled
    public static event Action OnCompassDisabled;

    public static void CallCompassDisabled()
    {
        OnCompassDisabled?.Invoke();
    }

    // Level up
    public static event Action OnLevelUp;

    public static void CallLevelUp()
    {
        OnLevelUp?.Invoke();
    }

    // Exp point Gained
    public static event Action OnExpGained;

    public static void CallExpGained()
    {
        OnExpGained?.Invoke();
    }

    // Mob hovered
    public static event Action<MobHoverArgs> OnMobHovered;

    public static void CallMobHoveredEvent(EnemyCategory mobCategory, bool isBoss = false)
    {
        OnMobHovered?.Invoke(new MobHoverArgs { mobCategory = mobCategory, isBoss = isBoss});
    }

    // Mob unhovered
    public static event Action<MobHoverArgs> OnMobUnhovered;

    public static void CallMobUnhoveredEvent(bool isBoss = false)
    {
        OnMobUnhovered?.Invoke(new MobHoverArgs { isBoss = isBoss});
    }

    // Mob unlocked
    public static event Action<MobUnlockArgs> OnMobUnlocked;

    public static void CallMobUnlockedEvent(EnemyCategory mobCategory, bool isBoss)
    {
        OnMobUnlocked?.Invoke(new MobUnlockArgs { mobCategory = mobCategory, isBoss = isBoss });
    }

    // Weapon hovered
    public static event Action<WeaponHoverArgs> OnWeaponHovered;

    public static void CallWeaponHoveredEvent(WeaponTitle weaponTitle)
    {
        OnWeaponHovered?.Invoke(new WeaponHoverArgs { weaponTitle = weaponTitle });
    }

    // Weapon unhovered
    public static event Action OnWeaponUnhovered;

    public static void CallWeaponUnhoveredEvent()
    {
        OnWeaponUnhovered?.Invoke();
    }

    // Weapon unlocked
    public static event Action<WeaponUnlockArgs> OnWeaponUnlocked;

    public static void CallWeaponUnlockedEvent(WeaponTitle weaponTitle)
    {
        OnWeaponUnlocked?.Invoke(new WeaponUnlockArgs { weaponTitle = weaponTitle });
    }


    // Passive hovered
    public static event Action<PassiveHoverArgs> OnPassiveHovered;

    public static void CallPassiveHoveredEvent(PassiveItemType passiveItemType)
    {
        OnPassiveHovered?.Invoke(new PassiveHoverArgs { passiveItemType = passiveItemType });
    }

    // Passive unhovered
    public static event Action OnPassiveUnhovered;

    public static void CallPassiveUnhoveredEvent()
    {
        OnPassiveUnhovered?.Invoke();
    }

    // Passive unlocked
    public static event Action<PassiveUnlockArgs> OnPassiveUnlocked;

    public static void CallPassiveUnlockedEvent(PassiveItemType passiveItemType)
    {
        OnPassiveUnlocked?.Invoke(new PassiveUnlockArgs { passiveItemType = passiveItemType });
    }

    // Enemy killed
    public static event Action<EnemyKilledArgs> OnEnemyKilled;

    public static void CallEnemyKilledEvent(Enemy enemy)
    {
        OnEnemyKilled?.Invoke(new EnemyKilledArgs { enemy = enemy });
    }

    public static void ClearAll()
    {
        OnActiveUniqueSkillPlaced = null;
    }
}

public class PlayerReadyEventArgs : EventArgs
{
    public Player player;
}

public class GameplayUToggledEventArgs : EventArgs
{
    public bool isActive = false;
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
    public RoomNetData roomNetData;
}

public class NpcInteractionStartedArgs : EventArgs
{
    public NpcType npcType;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
    public RoomNetData data;
    public List<GameObject> summonedEnemies;
}

public class CharacterButtonArgs : EventArgs
{
    public Character charIndex;
}

public class DynamicCameraFollowArgs : EventArgs
{
    public bool isOn;
}

public class OverviewCameraFollowArgs : EventArgs
{
    public bool isOn;
}

public class CameraShakeArgs : EventArgs
{
    public float shakeIntensity;
    public float shakeDuration;
}

public class ActiveUniqueSkillPlacedArgs : EventArgs
{
    public int placedSlotIndex;
    public ActiveUniqueSkillDetailsSO activeUniqueSkillDetails;
    public bool slotDrop;
}

public class StatChangedArgs : EventArgs
{
    public PrimaryStatName statName;
}

public class ItemGenericPlacedArgs : EventArgs
{
    public ItemGeneric item;
    public int fromIndex;
    public int toIndex;
}

public class ItemGenericSwappedArgs : EventArgs
{
    public DraggableItem draggableItem;
    public DraggableItem targetItem;
    public int draggableItemInventoryIndex;
    public int targetItemInventoryIndex;
}

public class WeaponAddedToBookArgs : EventArgs
{
    public Weapon weapon;
    public Weapon intentoryWeapon;
    public bool pickedUpByOffHand;
    public SlotType slotType;
    public bool onStart;
    public bool onlySwitch;
    public int inventoryIndexNumber;
    public int weaponSetNumber;
    public bool onMainHand;
    public DraggableItem slotWeaponDraggableItem;
    public DraggableItem inventoryWeaponDraggableItem;
}

public class PassiveItemAddedToBookArgs : EventArgs
{
    public PassiveItem passiveItem;
    public PassiveItem inventoryPassiveItem;
    public PassiveItemSlotName itemSlotName;
    public int inventoryIndexNumber;
    public int targetItemIndexNumber;
}

public class PassiveItemRemovedFromBookArgs : EventArgs
{
    public Sprite itemSprite;
    public PassiveItemSlotName itemSlotName;
    public int inventoryIndexNumber;
}

public class SwapFailedArgs : EventArgs
{
    public PopUpReason reason;
}

public class HealthChangedArgs : EventArgs
{
    public int currentHealth;
}

public class ManaChangedArgs : EventArgs
{
    public int currentMana;
}

public class DecoySpawnedArgs : EventArgs 
{
    public Dummy decoy;
}

public class InventoryWeaponUpgradedArgs : EventArgs
{
    public int inventoryIndexNumber;
    public Weapon weapon;
}

public class InventoryPassiveUpgradedArgs : EventArgs
{
    public int inventoryIndexNumber;
    public PassiveItem passiveItem;
}


public class IntroductionPopUpUIArgs : EventArgs
{
    public ItemType dropType;
    public ItemGeneric receivable;
}

public class SkillPointsArgs : EventArgs
{
    public InnerPathDetailsSO innerPathDetails;
    public ActiveUniqueSkillDetailsSO activeUniqueSkillContainer;
}

public class SkillBoostArgs : EventArgs
{
    public int skillBoostLevel;
}

public class CoinAndShardArgs : EventArgs
{
    public int updatedCoinAmount;
    public int updatedShardAmount;
}

public class GambleArgs : EventArgs
{
    public Player caller;
}

public class MobHoverArgs : EventArgs
{
    public EnemyCategory mobCategory;
    public bool isBoss;
}

public class MobUnlockArgs : EventArgs
{
    public EnemyCategory mobCategory;
    public bool isBoss;
}

public class WeaponHoverArgs : EventArgs
{
    public WeaponTitle weaponTitle;
}

public class WeaponUnlockArgs : EventArgs
{
    public WeaponTitle weaponTitle;
}

public class PassiveHoverArgs : EventArgs
{
    public PassiveItemType passiveItemType;
}

public class PassiveUnlockArgs : EventArgs
{
    public PassiveItemType passiveItemType;
}

public class EnemyKilledArgs : EventArgs
{
    public Enemy enemy;
}

