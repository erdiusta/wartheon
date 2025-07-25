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

    // Build Info Hovered
    public static event Action<SkillPointsArgs> OnBuildInfoHovered;

    public static void CallInnerPathInfoHoveredEvent(InnerPathDetailsSO innerPathDetails)
    {
        OnBuildInfoHovered?.Invoke(new SkillPointsArgs { innerPathDetails = innerPathDetails });
    }

    // Build Info Unhovered
    public static event Action<SkillPointsArgs> OnBuildInfoUnhovered;

    public static void CallInnerPathInfoUnhoveredEvent(InnerPathDetailsSO innerPathDetails)
    {
        OnBuildInfoUnhovered?.Invoke(new SkillPointsArgs { innerPathDetails = innerPathDetails });
    }

    // Build point used
    public static event Action<SkillPointsArgs> OnSkillPointUsed;

    public static void CallSkillPointsUsed(InnerPathDetailsSO innerPathDetails)
    {
        OnSkillPointUsed?.Invoke(new SkillPointsArgs { innerPathDetails = innerPathDetails });
    }

    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs { room = room });
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

    public static void CallRoomEnemiesDefeatedEvent(Room room, List<GameObject> summonedEnemies)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs { room = room, summonedEnemies = summonedEnemies });
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
    public static event Action OnGambleCompleted;

    public static void CallGambleCompletedEvent()
    {
        OnGambleCompleted?.Invoke();
    }

    // Book weapon pick-up event
    public static event Action<WeaponAddedToBookArgs> OnWeaponPickedUp;

    public static void CallWeaponPickedUpEventForBook(Weapon weapon, bool pickedUpByOffHand = false)
    {
        OnWeaponPickedUp?.Invoke(new WeaponAddedToBookArgs { weapon = weapon, pickedUpByOffHand = pickedUpByOffHand });
    }

    // Book weapon pick-up and tranport to inventory event
    public static event Action<WeaponAddedToBookArgs> OnWeaponAddedToInventory;

    public static void CallOnWeaponAddedToInventoryEventForBook(Weapon weapon, int inventoryIndexNumber)
    {
        OnWeaponAddedToInventory?.Invoke(new WeaponAddedToBookArgs { weapon = weapon, inventoryIndexNumber = inventoryIndexNumber });
    }

    // Book weapon 
    public static event Action<WeaponAddedToBookArgs> OnInventoryWeaponDropped;

    public static void CallInventoryWeaponDroppedEventForBook(int inventoryIndexNumber)
    {
        OnInventoryWeaponDropped?.Invoke(new WeaponAddedToBookArgs { inventoryIndexNumber = inventoryIndexNumber });
    }

    // Book weapon drop event
    public static event Action<WeaponAddedToBookArgs> OnWeaponDropped;

    public static void CallWeaponDroppedEventForBook(SlotType slotType)
    {
        OnWeaponDropped?.Invoke(new WeaponAddedToBookArgs { slotType = slotType });
    }

    // Item added to active item slot on book event
    public static event Action<SetSelectedActiveItemArgs> OnItemAddedToActiveItemSlot;

    public static void CallItemAddedToActiveItemSlot(ActiveItem activeItem)
    {
        OnItemAddedToActiveItemSlot?.Invoke(new SetSelectedActiveItemArgs { activeItem = activeItem });
    }

    // Item removed from active item slot on book event

    public static event Action OnItemRemovedFromActiveItemSlot;

    public static void CallItemRemovedFromActiveItemSlot()
    {
        OnItemRemovedFromActiveItemSlot?.Invoke();
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
        OnPassiveItemAddedToInventorySlot?.Invoke(new PassiveItemAddedToBookArgs { passiveItem = passiveItem, inventoryIndexNumber = inventoryIndexNumber });
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

    public static void CallIntroductionPopUpEvent(DropType dropType, ItemGeneric receivable)
    {
        OnDropPickedUp?.Invoke(new IntroductionPopUpUIArgs { dropType = dropType, receivable = receivable });
    }

    // Decoy added to the room event
    public static event Action<DecoySpawnedArgs> OnDecoySpawned;

    public static void CallDecoySpawned(Decoy decoy)
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

    // Active hovered
    public static event Action<ActiveHoverArgs> OnActiveHovered;

    public static void CallActiveHoveredEvent(ActiveItemType activeItemType)
    {
        OnActiveHovered?.Invoke(new ActiveHoverArgs { activeItemType = activeItemType });
    }

    // Active unhovered
    public static event Action OnActiveUnhovered;

    public static void CallActiveUnhoveredEvent()
    {
        OnActiveUnhovered?.Invoke();
    }

    // Active unlocked
    public static event Action<ActiveUnlockArgs> OnActiveUnlocked;

    public static void CallActiveUnlockedEvent(ActiveItemType activeItemType)
    {
        OnActiveUnlocked?.Invoke(new ActiveUnlockArgs { activeItemType = activeItemType });
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

public class GameplayUToggledEventArgs : EventArgs
{
    public bool isActive = false;
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class NpcInteractionStartedArgs : EventArgs
{
    public NpcType npcType;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
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
    public Decoy decoy;
}

public class IntroductionPopUpUIArgs : EventArgs
{
    public DropType dropType;
    public ItemGeneric receivable;
}

public class SkillPointsArgs : EventArgs
{
    public InnerPathDetailsSO innerPathDetails;
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
public class ActiveHoverArgs : EventArgs
{
    public ActiveItemType activeItemType;
}

public class ActiveUnlockArgs : EventArgs
{
    public ActiveItemType activeItemType;
}

public class EnemyKilledArgs : EventArgs
{
    public Enemy enemy;
}