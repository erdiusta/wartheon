using System.Collections.Generic;

public class SinglePlayerDungeonAccess : IDungeonAccess
{
    public IEnumerable<Room> GetRooms()
    {
        return DungeonBuilder.Instance.dungeonBuilderRoomDictionary.Values;
    }
}
