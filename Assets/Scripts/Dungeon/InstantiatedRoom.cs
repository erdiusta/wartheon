using System.Collections;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
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
    [SerializeField] GameObject environmentGameObject;

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

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();

        // Save room collider bounds
        roomColliderBounds = boxCollider2D.bounds;
    }

    // Trigger room changed event when player enters a room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player triggered the collider
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {
            // If the player entering the room is a clone, ignore room visit interactions
            if (collision.GetComponent<Player>().isClone) return;

            // Set room as visited
            room.isPreviouslyVisited = true;

            // Call room changed event
            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

    // Delete chest items when exiting rooms
    private void OnTriggerExit2D(Collider2D collision)
    {
        // If the player triggered the collider
        if (collision.tag == Settings.playerTag && room == GameManager.Instance.GetCurrentRoom())
        {
            ChestItem[] chestItems = FindObjectsByType<ChestItem>(FindObjectsSortMode.None);

            for (int i = 0; i < chestItems.Length; i++)
            {
                if (chestItems[i].GetComponentInParent<Player>() != null) continue;

                if(chestItems[i].GetComponentInParent<Counter>() != null) continue;

                Destroy(chestItems[i].gameObject);
            }
        }
    }

    /// <summary>
    /// Initialize The Instantiated Room
    /// </summary>
    public void Initialize(GameObject roomGameObject)
    {
        PopulateTilemapMemberVariables(roomGameObject);
        BlockOffUnusedDoorWays();
        AddObstaclesAndPreferredPaths();
        AddDoorsToRooms();
        DisableCollisionTilemapRenderer();
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
            if (tilemap.gameObject.tag == "groundTilemap")
            {
                groundTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration1Tilemap")
            {
                decoration1Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "decoration2Tilemap")
            {
                decoration2Tilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "sideTilemap")
            {
                sideTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "frontTilemap")
            {
                frontTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "collisionTilemap")
            {
                collisionTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "poolTilemap")
            {
                poolTilemap = tilemap;
            }
            else if (tilemap.gameObject.tag == "minimapTilemap")
            {
                minimapTilemap = tilemap;
            }
        }
    }

    /// <summary>
    /// Block Off Unused Doorways In The Room
    /// </summary>
    private void BlockOffUnusedDoorWays()
    {
        // Loop through all doorways
        foreach (Doorway doorway in room.doorwayList)
        {
            if (doorway.isConnected) continue;

            // Block unconnected doorways using tiles on tilemaps
            if (collisionTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(collisionTilemap, doorway);
            }

            if (poolTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(poolTilemap, doorway);
            }

            if (minimapTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(minimapTilemap, doorway);
            }

            if (groundTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(groundTilemap, doorway);
            }

            if (decoration1Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration1Tilemap, doorway);
            }

            if (decoration2Tilemap != null)
            {
                BlockADoorwayOnTilemapLayer(decoration2Tilemap, doorway);
            }

            if (sideTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(sideTilemap, doorway);
            }

            if (frontTilemap != null)
            {
                BlockADoorwayOnTilemapLayer(frontTilemap, doorway);
            }
        }
    }

    /// <summary>
    /// Block a doorway on a tilemap layer
    /// </summary>
    private void BlockADoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.North:
                BlockDoorwayHorizontally(tilemap, doorway);
                if (northMask != null)
                {
                    DisableMask(northMask);
                }
                break;
            case Orientation.South:
                BlockDoorwayHorizontally(tilemap, doorway);
                if (southMask != null)
                {
                    DisableMask(southMask);
                }
                break;

            case Orientation.East:
                BlockDoorwayVertically(tilemap, doorway);
                if (eastMask != null)
                {
                    DisableMask(eastMask);
                }
                break;
            case Orientation.West:
                BlockDoorwayVertically(tilemap, doorway);
                if (westMask != null)
                {
                    DisableMask(westMask);
                }
                break;

            case Orientation.None:
                break;
        }
    }

    /// <summary>
    /// Block doorway horizontally - for North and South doorways
    /// </summary>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // Loop through all tiles to copy
        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
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
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        // Loop through all tiles to copy
        for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
        {

            for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
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
        // This array will be populated with wall obstacles
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1, room.templateUpperBounds.y -
            room.templateLowerBounds.y + 1];

        // Loop thorugh all grid squares
        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                // Set default movement penalty for grid sqaures
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                // Add obstacles for collision tiles the enemy can't walk on
                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                // Add obstacles for pool tiles the enemy can't walk on
                TileBase checkedPoolTile = poolTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

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
    private void AddDoorsToRooms()
    {
        // If the room is a corridor then return
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS) return;

        // Instantiate door prefabs at doorway positions
        foreach (Doorway doorway in room.doorwayList)
        {
            // If the doorway prefab isn't null and the doorway is connected
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

                GameObject door = null;

                if (doorway.orientation == Orientation.North)
                {
                    // Create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance - 1, 0f);
                }
                else if (doorway.orientation == Orientation.South)
                {
                    // Create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y - 1, 0f);
                }
                else if (doorway.orientation == Orientation.East)
                {
                    // Create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance + 0.4f, 0f);
                }
                else if (doorway.orientation == Orientation.West)
                {
                    // Create door with parent as the room
                    door = Instantiate(doorway.doorPrefab, gameObject.transform);
                    door.transform.localPosition = new Vector3(doorway.position.x + 1, doorway.position.y + tileDistance + 0.4f, 0f);
                }


                // Get door component
                Door doorComponent = door.GetComponent<Door>();

                // Set if door is part of a boss room
                if (room.roomNodeType.isBossRoom)
                {
                    doorComponent.isBossRoomDoor = true;

                    // Instantiate boss icon for minimap by door
                    GameObject bossIcon = Instantiate(GameResources.Instance.minimapBossPrefab, gameObject.transform);
                    bossIcon.transform.localPosition = door.transform.localPosition;
                }
            }
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

    /// <summary>
    /// Unlock the room doors routine
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(environmentGameObject), environmentGameObject);
    }
#endif
    #endregion Validation
}
