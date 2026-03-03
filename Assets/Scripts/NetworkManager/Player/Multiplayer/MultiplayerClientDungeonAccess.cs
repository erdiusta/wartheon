using System.Collections.Generic;

public class MultiplayerClientDungeonAccess : IDungeonAccess
{
    readonly List<Room> rooms = new List<Room>();

    public bool IsDungeonReady => rooms.Count > 0;

    public void RegisterRoom(Room room)
    {
        if (room == null) return;
        if (!rooms.Contains(room)) rooms.Add(room);
    }

    public IEnumerable<Room> GetRooms() => rooms;
}
