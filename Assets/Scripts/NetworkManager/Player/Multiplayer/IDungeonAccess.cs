using System.Collections.Generic;

public interface IDungeonAccess
{
    IEnumerable<Room> GetRooms();
}
