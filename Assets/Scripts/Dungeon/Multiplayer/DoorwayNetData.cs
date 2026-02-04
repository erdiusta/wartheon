using UnityEngine;

[System.Serializable]
public struct DoorwayNetData
{
    public Vector2Int position;
    public Orientation orientation;

    public int doorwayCopyTileWidth;
    public int doorwayCopyTileHeight;
    public Vector2Int doorwayStartCopyPosition;

    public bool isConnected;
    public bool isUnavailable;

    // Visual-only info
    public bool isBossDoor;
}
