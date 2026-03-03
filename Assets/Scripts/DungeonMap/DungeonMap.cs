using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonMap : MonoBehaviour
{
    Player player;
    Camera dungeonMapCamera;
    CinemachineCamera dungeonMapCinemachine;

    private void Awake()
    {
        dungeonMapCamera = GetComponentInChildren<Camera>(true);
        dungeonMapCinemachine = GetComponentInChildren<CinemachineCamera>(true);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        InitializeDungeonMapCamera();
    }

    private void InitializeDungeonMapCamera()
    {
        dungeonMapCinemachine.Follow = player.transform;

        // Register camera to CameraManager (Local only)
        StaticEventHandler.CallOverviewCameraToggled(false);
    }

    private void Update()
    {
        if (player == null || !player.IsLocal) return;

        // If mouse button pressed and gamestate is dungeon overview map then get the room clicked
        if (InputManager.Instance.click.action.WasPerformedThisFrame() && GameManager.Instance.isOverviewCameraClicked)
        {
            GetRoomClicked();
        }
    }

    /// <summary>
    /// Get the room clicked on the map
    /// </summary>
    private void GetRoomClicked()
    {
        // Convert screen position to world position
        Vector3 worldPosition = dungeonMapCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        // Check for collisions at cursor position
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(new Vector2(worldPosition.x, worldPosition.y), 1f);

        if (collider2DArray == null || collider2DArray.Length == 0) return;

        // Check if any of the colliders are a room
        foreach (Collider2D collider2D in collider2DArray)
        {
            if (collider2D.GetComponent<InstantiatedRoom>() != null)
            {
                InstantiatedRoom instantiatedRoom = collider2D.GetComponent<InstantiatedRoom>();

                // If clicked room is clear of enemies and previously visited then move player to the room
                if (instantiatedRoom.room.isClearedOfEnemies && instantiatedRoom.room.isPreviouslyVisited)
                {
                    // Move player to room
                    StartCoroutine(MovePlayerToRoom(worldPosition, instantiatedRoom.room));
                    break;
                }
            }
        }

        player.meleeAttackMainHand.IsAttacking = false;
    }

    /// <summary>
    /// Move the player to the selected room
    /// </summary>
    IEnumerator MovePlayerToRoom(Vector3 worldPosition, Room room)
    {
        // Call room changed event
        StaticEventHandler.CallRoomChangedEvent(room);

        // Fade out screen to black immediately
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 0f, Color.black));

        // Clear dungeon overview
        ClearDungeonOverViewMap();

        // Disable player during the fade
        player.DisablePlayer();

        // Get nearest spawn point in room nearest to player
        Vector3 spawnPosition = HelperUtilities.GetSpawnPositionNearestToPlayer(worldPosition);

        // Move player to new location - spawning them at the closest spawn point
        player.transform.position = spawnPosition;

        // Fade the screen back in
        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));

        // Enable player
        player.EnablePlayer();
    }

    /// <summary>
    /// Display dungeon overview map UI
    /// </summary>
    public void DisplayDungeonOverViewMap()
    {
        player.cameraManager.ShowDungeonOverview();

        StaticEventHandler.CallOverviewCameraToggled(true);

        // Ensure all rooms are active so they can be displayed
        ActivateRoomsForDisplay();
    }

    /// <summary>
    /// Clear the dungeon overview map UI
    /// </summary>
    public void ClearDungeonOverViewMap()
    {
        // Enable player
        player.EnablePlayer();

        player.cameraManager.ShowGameplay();

        // Enable main camera and disable dungeon overview camera
        StaticEventHandler.CallOverviewCameraToggled(false);
    }

    /// <summary>
    /// Ensure all rooms are active so they can be displayed
    /// </summary>
    private void ActivateRoomsForDisplay()
    {
        // Iterate through dungeon rooms
        foreach (RoomActivationData data in GetRoomsForActivation())
        {
            data.instance.gameObject.SetActive(true);
        }
    }

    IEnumerable<RoomActivationData> GetRoomsForActivation()
    {
        // Single player
        if (!NetworkServer.active && !NetworkClient.active)
        {
            foreach (Room room in DungeonBuilder.Instance.dungeonBuilderRoomDictionary.Values)
            {
                if (room == null || room.instantiatedRoom == null) continue;

                yield return new RoomActivationData
                {
                    lower = room.lowerBounds,
                    upper = room.upperBounds,
                    instance = room.instantiatedRoom
                };
            }

            yield break;
        }

        // Multiplayer
        foreach (RoomNetData roomNet in DungeonRuntime.RoomNetDataDict.Values)
        {
            InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(roomNet.roomId);
            if (instantiatedRoom == null) continue;

            yield return new RoomActivationData
            {
                lower = roomNet.lowerBounds,
                upper = roomNet.upperBounds,
                instance = instantiatedRoom
            };
        }
    }
}
