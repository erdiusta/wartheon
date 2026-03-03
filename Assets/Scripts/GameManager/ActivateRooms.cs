using Mirror;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ActivateRooms : MonoBehaviour
{
    protected IDungeonAccess dungeon;

    Player player;
    Camera mainCamera;
    Camera minimapCamera;

    private void OnEnable()
    {
        StartCoroutine(PlayerReadyUtility.WaitForLocalPlayer(OnLocalPlayerReady));
    }

    private void OnLocalPlayerReady(Player player)
    {
        this.player = player;

        dungeon = new SinglePlayerDungeonAccess();
        StaticEventHandler.OnDungeonBuilt += OnDungeonBuilt;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnDungeonBuilt -= OnDungeonBuilt;
    }

    private void OnDungeonBuilt()
    {
        InvokeRepeating(nameof(EnableRooms), 0.5f, 0.75f);
    }

    private void EnableRooms()
    {
        if (player == null || !player.IsLocal) return;

        mainCamera = player.cameraManager.GetGameplayCamera();
        minimapCamera = player.cameraManager.GetMinimapCamera();

        if (GameManager.Instance.isOverviewCameraClicked) return;

        HelperUtilities.CameraWorldPositionBounds(out Vector2Int miniLow, out Vector2Int miniHigh, minimapCamera);
        HelperUtilities.CameraWorldPositionBounds(out Vector2Int camLow, out Vector2Int camHigh, mainCamera);

        // Iterate through dungeon rooms
        foreach (RoomActivationData room in GetRoomsForActivation())
        {
            bool inMinimap = room.lower.x <= miniHigh.x && room.lower.y <= miniHigh.y &&
                room.upper.x >= miniLow.x && room.upper.y >= miniLow.y;

            if (!inMinimap)
            {
                room.instance.gameObject.SetActive(false);
                continue;
            }

            room.instance.gameObject.SetActive(true);

            bool inMainCamera = room.lower.x <= camHigh.x && room.lower.y <= camHigh.y && 
                room.upper.x >= camLow.x && room.upper.y >= camLow.y;

            if (inMainCamera) room.instance.ActivateEnvironmentGameObjects();
            else room.instance.DeactivateEnvironmentGameObjects();
        }
    }

    IEnumerable<RoomActivationData> GetRoomsForActivation()
    {
        // Single player
        if (!NetworkServer.active && !NetworkClient.active)
        {
            foreach (Room room in dungeon.GetRooms())
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
