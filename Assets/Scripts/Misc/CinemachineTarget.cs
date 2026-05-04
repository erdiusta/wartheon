using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the CursorTarget gameobject")]
    #endregion
    [SerializeField] Transform cursorTarget;

    CinemachineTargetGroup cinemachineTargetGroup;

    CinemachineTargetGroup.Target cinemachineGroupTarget_player;
    CinemachineTargetGroup.Target cinemachineGroupTarget_cursor;
    CinemachineTargetGroup.Target cinemachineGroupTarget_vendor;

    List<CinemachineTargetGroup.Target> cinemachineTargetList = new List<CinemachineTargetGroup.Target>();

    Player player;
    NPC npcVendor;
    bool npcZoomTriggered = false;

    private void Awake()
    {
        player = GetComponentInParent<Player>();
        cinemachineTargetGroup = player.cameraManager.GetCinemachineTargetGroup();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnDynamicCameraToggled += StaticEventHandler_OnDynamicCameraToggled;
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnNPCInteractionStarted += StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded += StaticEventHandler_OnNPCInteractionEnded;

        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        StaticEventHandler.OnDynamicCameraToggled -= StaticEventHandler_OnDynamicCameraToggled;
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnNPCInteractionStarted -= StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded -= StaticEventHandler_OnNPCInteractionEnded;
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal) yield return null;

        BuildCinemachineTargets(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
    }

    /// <summary>
    /// Set the cinemachine camera target group
    /// </summary>
    public void BuildCinemachineTargets(bool isNpcZoomTriggered, bool npcZoomTriggered)
    {
        if (player.IsLocal)
        {
            // Create target group for cinemachine for the cinemachine camera to follow  - group will include the player and screen cursor
            cinemachineGroupTarget_player = new CinemachineTargetGroup.Target
            {
                Weight = 1f,
                Radius = 2.5f,
                Object = player.transform
            };

            cinemachineGroupTarget_cursor = new CinemachineTargetGroup.Target
            {
                Weight = 1f,
                Radius = 1f,
                Object = cursorTarget
            };

            if (npcZoomTriggered)
            {
                if (npcVendor != null)
                {
                    cinemachineGroupTarget_vendor = new CinemachineTargetGroup.Target
                    {
                        Weight = 1.5f,
                        Radius = 1f,
                        Object = npcVendor.transform
                    };
                }
            }

            ApplyCameraTargets(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
        }
    }

    private void StaticEventHandler_OnDynamicCameraToggled(DynamicCameraFollowArgs dynamicCameraFollowArgs)
    {
        ApplyCameraTargets(dynamicCameraFollowArgs.isOn, npcZoomTriggered);
    }

    private void ApplyCameraTargets(bool dynamicCameraIsOn, bool isZoomTriggered)
    {
        cinemachineTargetList = new List<CinemachineTargetGroup.Target> { cinemachineGroupTarget_player };

        if (dynamicCameraIsOn)
        {
            cinemachineTargetList.Add(cinemachineGroupTarget_cursor);
            cinemachineTargetGroup.Targets = cinemachineTargetList;
        }
        else
        {
            cinemachineTargetList.Remove(cinemachineGroupTarget_cursor);
            cinemachineTargetGroup.Targets = cinemachineTargetList;
        }

        if (isZoomTriggered && cinemachineGroupTarget_vendor.Object != null)
        {
            cinemachineTargetList.Add(cinemachineGroupTarget_vendor);
            cinemachineTargetGroup.Targets = cinemachineTargetList;
        }
        else
        {
            cinemachineTargetList.Remove(cinemachineGroupTarget_vendor);
            cinemachineTargetGroup.Targets = cinemachineTargetList;
        }
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetNpcOnRoomChanged(roomChangedEventArgs.room, roomChangedEventArgs.roomNetData);
    }

    private void SetNpcOnRoomChanged(Room room, RoomNetData roomNetData)
    {
        if (room != null)
        {
            if (room.roomNodeType.isShopRoom)
            {
                // Get npc vendor
                npcVendor = room.instantiatedRoom.GetComponentInChildren<NPC>();
            }

            return;
        }

        if (roomNetData.isShopRoom)
        {
            InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(roomNetData.roomId);

            // Get npc vendor
            npcVendor = instantiatedRoom.GetComponentInChildren<NPC>();
        }
    }

    private void StaticEventHandler_OnNPCInteractionStarted(NpcInteractionStartedArgs npcInteractionStartedArgs)
    {
        npcZoomTriggered = true;

        // Cache npc vendor object to added cinemachine target reference
        if (npcVendor != null)
        {
            cinemachineGroupTarget_vendor = new CinemachineTargetGroup.Target
            {
                Weight = 1.5f,
                Radius = 1f,
                Object = npcVendor.transform
            };
        }

        ApplyCameraTargets(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
    }

    private void StaticEventHandler_OnNPCInteractionEnded()
    {
        npcZoomTriggered = false;

        ApplyCameraTargets(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
    }

    private void Update()
    {
        if (player == null) return;
        if (!player.IsLocal) return;
        if (player.cameraManager == null) return;

        cursorTarget.position = HelperUtilities.GetMouseWorldPosition(player);
    }
}
