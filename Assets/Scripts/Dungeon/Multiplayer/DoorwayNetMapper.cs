public static class DoorwayNetMapper
{
    public static DoorwayNetData ToNetData(Doorway doorway)
    {
        return new DoorwayNetData
        {
            position = doorway.position,
            orientation = doorway.orientation,
            doorwayStartCopyPosition = doorway.doorwayStartCopyPosition,
            doorwayCopyTileWidth = doorway.doorwayCopyTileWidth,
            doorwayCopyTileHeight = doorway.doorwayCopyTileHeight,
            isConnected = doorway.isConnected,
            isUnavailable = doorway.isUnavailable
        };
    }
}
