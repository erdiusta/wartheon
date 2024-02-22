using System;

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
