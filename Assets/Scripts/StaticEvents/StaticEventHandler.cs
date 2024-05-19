using System;
using UnityEditor;
using UnityEngine;

public static class StaticEventHandler
{
    // Room changed event
    public static event Action<RoomChangedEventArgs> OnRoomChanged;

    public static void CallRoomChangedEvent(Room room)
    {
        OnRoomChanged?.Invoke(new RoomChangedEventArgs { room = room });
    }

    // Room enemies defeated event
    public static event Action<RoomEnemiesDefeatedArgs> OnRoomEnemiesDefeated;

    public static void CallRoomEnemiesDefeatedEvent(Room room)
    {
        OnRoomEnemiesDefeated?.Invoke(new RoomEnemiesDefeatedArgs { room = room });
    }

    // Camera shaken event
    public static event Action<CameraShakeArgs> OnCameraShaken;

    public static void CallCameraShakeEvent(float shakeIntensity, float shakeDuration)
    {
        OnCameraShaken?.Invoke(new CameraShakeArgs { shakeIntensity = shakeIntensity, shakeDuration = shakeDuration });
    }

    // Weapon added to main hand on book event
    public static event Action<WeaponAddedToBookArgs> OnWeaponAddedToMainHandBook;

    public static void CallWeaponAddedToMainHandBook(Sprite weaponSprite)
    {
        OnWeaponAddedToMainHandBook?.Invoke(new WeaponAddedToBookArgs { weaponSprite = weaponSprite});
    }

    // Weapon added to off-hand on book event
    public static event Action<WeaponAddedToBookArgs> OnWeaponAddedToOffHandBook;

    public static void CallWeaponAddedToOffHandBook(Sprite weaponSprite)
    {
        OnWeaponAddedToOffHandBook?.Invoke(new WeaponAddedToBookArgs { weaponSprite = weaponSprite});
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

    // Health change on book event
    public static event Action<HealthChangedArgs> OnBookHealthChanged;

    public static void CallBookHealthChangedEvent(int currentHealth)
    {
        OnBookHealthChanged?.Invoke(new HealthChangedArgs { currentHealth = currentHealth });
    }

    // Decoy added to the room event
    public static event Action<DecoySpawnedArgs> OnDecoySpawned;

    public static void CallDecoySpawned(Decoy decoy)
    {
        OnDecoySpawned?.Invoke(new DecoySpawnedArgs { decoy = decoy });
    }
}

public class RoomChangedEventArgs : EventArgs
{
    public Room room;
}

public class RoomEnemiesDefeatedArgs : EventArgs
{
    public Room room;
}

public class CameraShakeArgs : EventArgs
{
    public float shakeIntensity;
    public float shakeDuration;
}

public class WeaponAddedToBookArgs : EventArgs
{
    public Sprite weaponSprite;
}

public class ItemAddedToBookArgs : EventArgs
{
    public Sprite itemSprite;
}

public class HealthChangedArgs : EventArgs
{
    public int currentHealth;
}

public class DecoySpawnedArgs : EventArgs 
{
    public Decoy decoy;
}
