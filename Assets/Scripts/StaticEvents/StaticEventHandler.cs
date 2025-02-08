using System;
using System.Collections.Generic;
using UnityEngine;

public static class StaticEventHandler
{
    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs { room = room });
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

    #region Book Weapon Events
    //// Weapon added to main hand on book event
    //public static event Action<WeaponAddedToBookArgs> OnWeaponAddedToMainHandBook;

    //public static void CallWeaponAddedToMainHandBook(Weapon weapon, bool onlySwitch)
    //{
    //    OnWeaponAddedToMainHandBook?.Invoke(new WeaponAddedToBookArgs { weapon = weapon, onlySwitch = onlySwitch});
    //}

    //// Main hand weapon removed from main hand on book event
    //public static event Action OnWeaponRemovedFromMainHandBook;

    //public static void CallWeaponRemovedFromMainHandBook()
    //{
    //    OnWeaponRemovedFromMainHandBook?.Invoke();
    //}

    //// Weapon added to off-hand on book event
    //public static event Action<WeaponAddedToBookArgs> OnWeaponAddedToOffHandBook;

    //public static void CallWeaponAddedToOffHandBook(Weapon weapon)
    //{
    //    OnWeaponAddedToOffHandBook?.Invoke(new WeaponAddedToBookArgs { weapon = weapon});
    //}

    //// Weapon swapped at main hand
    //public static event Action<WeaponAddedToBookArgs> OnWeaponSwappedAtOffHand;

    //public static void CallWeaponSwappedAtOffHand(Weapon weapon)
    //{
    //    OnWeaponSwappedAtOffHand?.Invoke(new WeaponAddedToBookArgs { weapon = weapon });
    //}

    //// Off-hand weapon removed from off-hand on book event
    //public static event Action OnWeaponRemovedFromOffHandBook;

    //public static void CallWeaponRemovedFromOffHandBook()
    //{
    //    OnWeaponRemovedFromOffHandBook?.Invoke();
    //}
    #endregion

    // Book weapon switch event
    public static event Action OnWeaponSwitched;

    public static void CallWeaponSwitchedEventForBook()
    {
        OnWeaponSwitched?.Invoke();
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

    // Build point used
    public static event Action<BuildPointsArgs> OnBuildPointUsed;

    public static void CallBuildPointsUsed(int unlockedBuildIconIndexNumber)
    {
        OnBuildPointUsed?.Invoke(new BuildPointsArgs { unlockedBuildIconIndexNumber = unlockedBuildIconIndexNumber });
    }

    // Build point gained
    public static event Action OnBuildPointsGained;

    public static void CallBuildPointsGained()
    {
        OnBuildPointsGained?.Invoke();
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
    public int unlockedBuildIconIndexNumber;
}
