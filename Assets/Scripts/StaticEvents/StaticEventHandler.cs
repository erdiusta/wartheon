using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class StaticEventHandler
{
    // Additive scene removed
    public static event Action OnAdditiveSceneRemoved;

    public static void CallAdditiveSceneRemoveEvent()
    {
        OnAdditiveSceneRemoved?.Invoke();
    }

    // Cheat code activated
    public static event Action OnCheatActivated;

    public static void CallCheatActivatedEvent()
    {
        OnCheatActivated?.Invoke();
    }

    // Build Info Hovered
    public static event Action<BuildPointsArgs> OnBuildInfoHovered;

    public static void CallBuildInfoHoveredEvent(int buildIndex)
    {
        OnBuildInfoHovered?.Invoke(new BuildPointsArgs { buildIndex = buildIndex});
    }

    // Build Info Unhovered
    public static event Action<BuildPointsArgs> OnBuildInfoUnhovered;

    public static void CallBuildInfoUnhoveredEvent(int buildIndex)
    {
        OnBuildInfoUnhovered?.Invoke(new BuildPointsArgs { buildIndex = buildIndex });
    }

    // Build point used
    public static event Action<BuildPointsArgs> OnBuildPointUsed;

    public static void CallBuildPointsUsed(int buildIndex)
    {
        OnBuildPointUsed?.Invoke(new BuildPointsArgs { buildIndex = buildIndex });
    }

    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs { room = room });
    }

    // All enemies cleared in the room
    public static event Action OnEnemiesCleared;

    public static void CallEnemiesClearedEvent()
    {
        OnEnemiesCleared?.Invoke();
    }

    // Stats changed event
    public static event Action OnPrimaryStatsChanged;

    public static void CallPrimaryStatsChangedEvent()
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

    // Book weapon drop event
    public static event Action<WeaponAddedToBookArgs> OnWeaponDropped;

    public static void CallWeaponDroppedEventForBook(SlotType slotType)
    {
        OnWeaponDropped?.Invoke(new WeaponAddedToBookArgs { slotType = slotType });
    }

    // Item added to active item slot on book event
    public static event Action<ItemAddedToBookArgs> OnItemAddedToActiveItemSlot;

    public static void CallItemAddedToActiveItemSlot(Sprite itemSprite)
    {
        OnItemAddedToActiveItemSlot?.Invoke(new ItemAddedToBookArgs { itemSprite = itemSprite });
    }

    // Item removed from active item slot on book event

    public static event Action OnItemRemovedFromActiveItemSlot;

    public static void CallItemRemovedFromActiveItemSlot()
    {
        OnItemRemovedFromActiveItemSlot?.Invoke();
    }

    // Item added to passiveitem slot on book event
    public static event Action<ItemAddedToBookArgs> OnItemAddedToPassiveItemSlot;

    public static void CallItemAddedToPassiveItemSlot(Sprite itemSprite, PassiveItemSlotName itemSlotName)
    {
        OnItemAddedToPassiveItemSlot?.Invoke(new ItemAddedToBookArgs { itemSprite = itemSprite, itemSlotName = itemSlotName });
    }

    // Item removed from passive item slot on book event
    public static event Action<ItemRemovedFromBookArgs> OnItemRemovedFromPassiveItemSlot;

    public static void CallItemRemovedFromPassiveItemSlot(PassiveItemSlotName itemSlotName)
    {
        OnItemRemovedFromPassiveItemSlot?.Invoke(new ItemRemovedFromBookArgs { itemSlotName = itemSlotName});
    }

    // Health change on book event
    public static event Action<HealthChangedArgs> OnBookHealthChanged;

    public static void CallBookHealthChangedEvent(int currentHealth)
    {
        OnBookHealthChanged?.Invoke(new HealthChangedArgs { currentHealth = currentHealth });
    }

    // Introduction ui screen opened event
    public static event Action<IntroductionPopUpUIArgs> OnDropPickedUp;

    public static void CallIntroductionPopUpEvent(DropType dropType, IReceivable receivable)
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
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
    public List<GameObject> summonedEnemies;
}

public class CameraShakeArgs : EventArgs
{
    public float shakeIntensity;
    public float shakeDuration;
}

public class WeaponAddedToBookArgs : EventArgs
{
    public Weapon weapon;
    public bool pickedUpByOffHand;
    public SlotType slotType;
    public bool onStart;
    public bool onlySwitch;
}

public class ItemAddedToBookArgs : EventArgs
{
    public Sprite itemSprite;
    public PassiveItemSlotName itemSlotName;
}

public class ItemRemovedFromBookArgs : EventArgs
{
    public Sprite itemSprite;
    public PassiveItemSlotName itemSlotName;
}

public class HealthChangedArgs : EventArgs
{
    public int currentHealth;
}

public class DecoySpawnedArgs : EventArgs 
{
    public Decoy decoy;
}

public class IntroductionPopUpUIArgs : EventArgs
{
    public DropType dropType;
    public IReceivable receivable;
}

public class BuildPointsArgs : EventArgs
{
    public int buildIndex;
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