using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public RoomNetworkRoot root;
    [HideInInspector] public RoomNetData roomNetData; // For Multiplayer
    [HideInInspector] public Room room; // For Single Player
    [HideInInspector] public bool isMultiplayer;
    [HideInInspector] public Dictionary<int, Door> spawnedDoors = new();

    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap sideTilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap poolTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public int[,] aStarMovementPenalty; // use this 2d array to store movement penalties from the tilemaps to be used in AStar pathfinding
    [HideInInspector] public Bounds roomColliderBounds;


    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the environment child placeholder gameobject ")]
    #endregion Tooltip
    public GameObject environmentGameObject;

    [HideInInspector] public List<GameObject> roomObstaclesList = new List<GameObject>();

    #region Header MASK REFERENCES
    [Space(10)]
    [Header("MASK REFERENCES")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the child mask gameObjects")]
    #endregion Tooltip
    [SerializeField] SpriteMask northMask;
    [SerializeField] SpriteMask southMask;
    [SerializeField] SpriteMask eastMask;
    [SerializeField] SpriteMask westMask;

    BoxCollider2D boxCollider2D;
    Player player;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    // Trigger room changed event when player enters a room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkServer.active && !NetworkClient.active) // Single Player
        {
            // If the player triggered the collider
            if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
            {
                if (!room.roomNodeType.isEntrance)
                {
                    player = GameManager.Instance.GetLocalPlayer();

                    if (player != null && player.isBattleReadyActive && !room.isPreviouslyVisited && room.roomNodeType.isCombatRoom) // Battle Ready Mechanic
                    {
                        player.health.AddHealth(3);
                        player.health.AddShield(5);
                    }
                }

                // Set room as visited
                room.isPreviouslyVisited = true;

                // Call room changed event
                StaticEventHandler.CallRoomChangedEvent(room);
            }
        }
    }

    // Delete chest items when exiting rooms
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (collision.CompareTag(Settings.playerTag))
            {
                if (IsCorridor()) return;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterRoomVisit(this);
                }
            }
        }
    }

    public bool IsCorridor()
    {
        if (isMultiplayer) return roomNetData.isCorridor || roomNetData.isCorridorEW || roomNetData.isCorridorNS;
        else
        {
            if(room != null)
            {
                string typeName = room.roomNodeType.roomNodeTypeName;
                return typeName == "Corridor" || typeName == "Corridor NS" || typeName == "Corridor EW";
            }

            return true;
        }
    }

    private void OnDestroy()
    {
        if (roomNetData.roomId != null) DungeonRuntime.UnregisterRoom(roomNetData.roomId);
    }

    /// <summary>
    /// Initialize The Instantiated Room - SP
    /// </summary>
    public void InitializeSinglePlayer(GameObject roomGameObject)
    {
        isMultiplayer = false;

        InitializeCommon(roomGameObject);
        InitializeSinglePlayerOnly();
    }

    /// <summary>
    /// Initialize The Instantiated Room - MP
    /// </summary>
    public void InitializeMultiplayer(GameObject roomGameObject)
    {
        isMultiplayer = true;

        InitializeCommon(roomGameObject);
        InitializeMultiplayerOnly();
        DungeonRuntime.RegisterRoom(roomNetData.roomId, this);
    }

    private void InitializeCommon(GameObject roomGameObject)
    {
        PopulateEnvironmentObjects();
        PopulateTilemapMemberVariables(roomGameObject);
        DisableCollisionTilemapRenderer();
    }

    private void InitializeSinglePlayerOnly()
    {
        BlockOffUnusedDoorWaysSP();
        AddObstaclesAndPreferredPaths();
        AddDoorsToRoomsSP();
    }

    private void InitializeMultiplayerOnly()
    {
        ReplaceSPPropsWithMP();
        BlockOffUnusedDoorWaysMP();
        AddObstaclesAndPreferredPaths();
        AddDoorsToRoomsMP();
    }

    private void PopulateEnvironmentObjects()
    {
        // Fill obstacles list
        foreach (Transform child in environmentGameObject.transform)
        {
            if (child.TryGetComponent(out Environment obstacle))
            {
                roomObstaclesList.Add(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Populate the tilemap and grid memeber variables.
    /// </summary>
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        // Get the grid component
        grid = roomGameobject.GetComponentInChildren<Grid>();

        // Get tilemaps in children
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            if (tilemap.gameObject.tag == Settings.groundTilemap) groundTilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.decoration1Tilemap) decoration1Tilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.decoration2Tilemap) decoration2Tilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.sideTilemap) sideTilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.frontTilemap) frontTilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.collisionTilemap) collisionTilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.poolTilemap) poolTilemap = tilemap;
            else if (tilemap.gameObject.tag == Settings.minimapTilemap) minimapTilemap = tilemap;
        }
    }

    /// <summary>
    /// Block Off Unused Doorways In The Room - SP
    /// </summary>
    private void BlockOffUnusedDoorWaysSP()
    {
        // Loop through all doorways
        foreach (Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected) continue;

            // Block unconnected doorways using tiles on tilemaps
            if (collisionTilemap != null) BlockADoorwayOnTilemapLayer(collisionTilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (poolTilemap != null) BlockADoorwayOnTilemapLayer(poolTilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (minimapTilemap != null) BlockADoorwayOnTilemapLayer(minimapTilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (groundTilemap != null) BlockADoorwayOnTilemapLayer(groundTilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (decoration1Tilemap != null) BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (decoration2Tilemap != null) BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (sideTilemap != null) BlockADoorwayOnTilemapLayer(sideTilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
            if (frontTilemap != null) BlockADoorwayOnTilemapLayer(frontTilemap, doorway.orientation, doorway.doorwayStartCopyPosition, doorway.doorwayCopyTileWidth, doorway.doorwayCopyTileHeight);
        }
    }

    /// <summary>
    /// Replace single player props and dummies with multiplayer ones - Multiplayer Only
    /// </summary>
    private void ReplaceSPPropsWithMP()
    {
        if (!NetworkServer.active) return;

        var markers = GetComponentsInChildren<MPPropReplaceMarker>(true);

        foreach (var marker in markers)
        {
            GameObject mpPrefab = GameResources.Instance.GetMPPrefab(marker.replaceType);
            if (mpPrefab == null) continue;

            Transform t = marker.transform;
            GameObject mpInstance = Instantiate(mpPrefab, t.position, t.rotation);

            var propNet = mpInstance.GetComponent<PropNetwork>();
            propNet.roomId = roomNetData.roomId;

            NetworkServer.Spawn(mpInstance);
            Destroy(marker.gameObject);
        }
    }

    /// <summary>
    /// Block Off Unused Doorways In The Room - MP
    /// </summary>
    private void BlockOffUnusedDoorWaysMP()
    {   
        foreach (DoorwayNetData doorwayData in roomNetData.doorways)
        {
            if (doorwayData.isConnected) continue;

            // Block unconnected doorways using tiles on tilemaps
            if (collisionTilemap != null) BlockADoorwayOnTilemapLayer(collisionTilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (poolTilemap != null) BlockADoorwayOnTilemapLayer(poolTilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (minimapTilemap != null) BlockADoorwayOnTilemapLayer(minimapTilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (groundTilemap != null) BlockADoorwayOnTilemapLayer(groundTilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (decoration1Tilemap != null) BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (decoration2Tilemap != null) BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (sideTilemap != null) BlockADoorwayOnTilemapLayer(sideTilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
            if (frontTilemap != null) BlockADoorwayOnTilemapLayer(frontTilemap, doorwayData.orientation, doorwayData.doorwayStartCopyPosition, doorwayData.doorwayCopyTileWidth, doorwayData.doorwayCopyTileHeight);
        }
    }

    /// <summary>
    /// Block a doorway on a tilemap layer
    /// </summary>
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Orientation orientation, Vector2Int doorwayStartCopyPosition, int doorwayCopyTileWidth, int doorwayCopyTileHeight)
    {
        switch (orientation)
        {
            case Orientation.North:
                BlockDoorwayHorizontally(tilemap, doorwayStartCopyPosition, doorwayCopyTileWidth, doorwayCopyTileHeight);
                if (northMask != null) DisableMask(northMask);
                break;
            case Orientation.South:
                BlockDoorwayHorizontally(tilemap, doorwayStartCopyPosition, doorwayCopyTileWidth, doorwayCopyTileHeight);
                if (southMask != null) DisableMask(southMask);
                break;
            case Orientation.East:
                BlockDoorwayVertically(tilemap, doorwayStartCopyPosition, doorwayCopyTileWidth, doorwayCopyTileHeight);
                if (eastMask != null) DisableMask(eastMask);
                break;
            case Orientation.West:
                BlockDoorwayVertically(tilemap, doorwayStartCopyPosition, doorwayCopyTileWidth, doorwayCopyTileHeight);
                if (westMask != null) DisableMask(westMask);
                break;
            case Orientation.None:
                break;
        }
    }

    /// <summary>
    /// Block doorway horizontally - for North and South doorways
    /// </summary>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Vector2Int doorwayStartCopyPosition, int doorwayCopyTileWidth, int doorwayCopyTileHeight)
    {
        Vector2Int startPosition = doorwayStartCopyPosition;

        // Loop through all tiles to copy
        for (int xPos = 0; xPos < doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorwayCopyTileHeight; yPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(
                    new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Block doorway vertically - for East and West doorways
    /// </summary>
    private void BlockDoorwayVertically(Tilemap tilemap, Vector2Int doorwayStartCopyPosition, int doorwayCopyTileWidth, int doorwayCopyTileHeight)
    {
        Vector2Int startPosition = doorwayStartCopyPosition;

        // Loop through all tiles to copy
        for (int yPos = 0; yPos < doorwayCopyTileHeight; yPos++)
        {
            for (int xPos = 0; xPos < doorwayCopyTileWidth; xPos++)
            {
                // Get rotation of tile being copied
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                // Copy tile
                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(
                    new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                // Set rotation of tile copied
                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Disable unconnected doorway's sprite mask
    /// </summary>
    public void DisableMask(SpriteMask mask)
    {
        mask.gameObject.SetActive(false);
    }

    /// <summary>
    /// Update obstacles used by AStar pathfinmding
    /// </summary>
    private void AddObstaclesAndPreferredPaths()
    {
        Vector2Int templateUpperBounds = isMultiplayer ? roomNetData.templateUpperBounds : room.templateUpperBounds;
        Vector2Int templateLowerBounds = isMultiplayer ? roomNetData.templateLowerBounds : room.templateLowerBounds;

        // This array will be populated with wall obstacles
        aStarMovementPenalty = new int[templateUpperBounds.x - templateLowerBounds.x + 1, templateUpperBounds.y - templateLowerBounds.y + 1];

        // Loop thorugh all grid squares
        for (int x = 0; x < (templateUpperBounds.x - templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (templateUpperBounds.y - templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                // Add obstacles for collision tiles the enemy can't walk on
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + templateLowerBounds.x, y + templateLowerBounds.y, 0));

                // Add obstacles for pool tiles the enemy can't walk on
                TileBase checkedPoolTile = poolTilemap.GetTile(new Vector3Int(x + templateLowerBounds.x, y + templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }

                    if (checkedPoolTile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                // Add preferred path for enemies (1 is the preferred path value, default value for a grid location is specified in the Settings)
                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }
            }
        }
    }

    /// <summary>
    /// Add opening doors if this is not a corridor room
    /// </summary>
    private void AddDoorsToRoomsSP()
    {
        // If the room is a corridor then return
        if (IsCorridor()) return;

        float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

        // Instantiate door prefabs at doorway positions
        foreach (Doorway doorway in room.doorwayList)
        {
            // If the doorway prefab isn't null and the doorway is connected
            if (!doorway.isConnected) continue;

            GameObject prefab = (doorway.orientation == Orientation.North || doorway.orientation == Orientation.South) ?
                GameResources.Instance.doorNSPrefab : GameResources.Instance.doorEWPrefab;

            Vector3 localPos = doorway.orientation switch
            {
                Orientation.North => new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance - 1, 0f),
                Orientation.South => new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y - 1, 0f),
                Orientation.East => new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance + 0.4f, 0f),
                Orientation.West => new Vector3(doorway.position.x + 1, doorway.position.y + tileDistance + 0.4f, 0f),
                _ => Vector3.zero
            };

            // Get door component
            GameObject door = Instantiate(prefab, transform);
            door.transform.localPosition = localPos;
            Door doorComponent = door.GetComponent<Door>();

            // Set if door is part of a boss room
            if (roomNetData.isBossRoom) doorComponent.isBossRoomDoor = true;
        }
    }

    /// <summary>
    /// Add opening doors if this is not a corridor room - MP
    /// </summary>
    private void AddDoorsToRoomsMP()
    {
        // If the room is a corridor then return
        if (IsCorridor()) return;

        float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

        // Instantiate door prefabs at doorway positions
        for (int i = 0; i < roomNetData.doorways.Length; i++)
        {
            DoorwayNetData doowayData = roomNetData.doorways[i];

            // If the doorway prefab isn't null and the doorway is connected
            if (!doowayData.isConnected) continue;

            // Door already exists - skip
            if (spawnedDoors.ContainsKey(i)) continue;

            GameObject prefab = (doowayData.orientation == Orientation.North || doowayData.orientation == Orientation.South) ?
                GameResources.Instance.doorNSPrefab : GameResources.Instance.doorEWPrefab;

            Vector3 localPos = doowayData.orientation switch
            {
                Orientation.North => new Vector3(doowayData.position.x + tileDistance / 2f, doowayData.position.y + tileDistance - 1, 0f),
                Orientation.South => new Vector3(doowayData.position.x + tileDistance / 2f, doowayData.position.y - 1, 0f),
                Orientation.East => new Vector3(doowayData.position.x + tileDistance, doowayData.position.y + tileDistance + 0.4f, 0f),
                Orientation.West => new Vector3(doowayData.position.x + 1, doowayData.position.y + tileDistance + 0.4f, 0f),
                _ => Vector3.zero
            };

            // Get door component
            GameObject doorGO = Instantiate(prefab, transform);
            doorGO.transform.localPosition = localPos;
            Door door = doorGO.GetComponent<Door>();

            // Set if door is part of a boss room
            if (roomNetData.isBossRoom) door.isBossRoomDoor = true;

            spawnedDoors[i] = door;
        }
    }

    /// <summary>
    /// Disable collision tilemap renderer
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        // Disable collision tilemap renderer
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    /// <summary>
    /// Disable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    /// <summary>
    /// Enable the room trigger collider that is used to trigger when the player enters a room
    /// </summary>
    public void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }

    public void ActivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(true);
    }

    public void DeactivateEnvironmentGameObjects()
    {
        if (environmentGameObject != null)
            environmentGameObject.SetActive(false);
    }

    /// <summary>
    /// Lock the room doors
    /// </summary>
    public void LockDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        // Trigger lock doors
        foreach (Door door in doorArray)
        {
            door.LockDoor();
        }

        // Disable room trigger collider
        DisableRoomCollider();
    }

    /// <summary>
    /// Unlock the room doors
    /// </summary>
    public void UnlockDoors(float doorUnlockDelay)
    {
        StartCoroutine(UnlockDoorsRoutine(doorUnlockDelay));
    }

    public void DestroyAllDroppedItems()
    {
        DropItem[] dropItems = GetComponentsInChildren<DropItem>(true);

        if (dropItems != null)
        {
            foreach (DropItem item in dropItems)
            {
                if (item.GetComponentInParent<Player>() != null) continue;
                if (item.GetComponentInParent<Counter>() != null) continue;

                Destroy(item.gameObject);
            }
        }
    }

    /// <summary>
    /// 
    /// the room doors routine
    /// </summary>
    IEnumerator UnlockDoorsRoutine(float doorUnlockDelay)
    {
        if (doorUnlockDelay > 0f)
            yield return new WaitForSeconds(doorUnlockDelay);

        Door[] doorArray = GetComponentsInChildren<Door>();

        // Trigger open doors
        foreach (Door door in doorArray)
        {
            door.UnlockDoor();
        }

        // Enable room trigger collider
        EnableRoomCollider();
    }

    public int GetRoomTilePenaltyValue(Vector3Int enemyZeroBasedCellPosition)
    {
        try
        {
            return aStarMovementPenalty[enemyZeroBasedCellPosition.x, enemyZeroBasedCellPosition.y];
        }
        catch (System.IndexOutOfRangeException)
        {
            return Settings.defaultAStarMovementPenalty;
        }
    }

    public InstantiatedRoom GetBossRoom()
    {
        if (room.roomNodeType.isBossRoom)
        {
            return room.instantiatedRoom;
        }
        else
        {
            return null;
        }
    }

    public Transform[] CreatePatrolTargets(Vector2Int[] spawnPositions, Transform parent = null, bool isMultiplayer = false)
    {
        if (spawnPositions == null || spawnPositions.Length == 0) return System.Array.Empty<Transform>();

        Transform[] patrolTargets = new Transform[spawnPositions.Length];

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            Vector3Int cell = new Vector3Int(spawnPositions[i].x, spawnPositions[i].y, 0);

            Vector3 worldPos = grid.CellToWorld(cell) + grid.cellSize * 0.5f;

            PatrolPoint point;

            if (isMultiplayer) point = (PatrolPoint)PoolManager.Instance.Reuse(GameResources.Instance.enemyPatrolPointsParentMP.gameObject, worldPos, Quaternion.identity);
            else point = (PatrolPoint)PoolManager.Instance.Reuse(GameResources.Instance.enemyPatrolPointsParent.gameObject, worldPos, Quaternion.identity);

            if (parent != null) point.transform.SetParent(parent);

            patrolTargets[i] = point.transform;
        }

        return patrolTargets;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(environmentGameObject), environmentGameObject);
    }
#endif
    #endregion Validation
}
