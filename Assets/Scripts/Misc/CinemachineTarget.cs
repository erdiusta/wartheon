using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    CinemachineTargetGroup cinemachineTargetGroup;

    #region Tooltip
    [Tooltip("Populate with the CursorTarget gameobject")]
    #endregion Tooltip
    [SerializeField] Transform cursorTarget;

    CinemachineTargetGroup.Target cinemachineGroupTarget_player;
    CinemachineTargetGroup.Target cinemachineGroupTarget_cursor;
    CinemachineTargetGroup.Target cinemachineGroupTarget_vendor;

    List<CinemachineTargetGroup.Target> cinemachineTargetList = new List<CinemachineTargetGroup.Target>();

    NPC npcVendor;
    bool npcZoomTriggered = false;

    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnDynamicCameraToggled += StaticEventHandler_OnDynamicCameraToggled;
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnNPCInteractionStarted += StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded += StaticEventHandler_OnNPCInteractionEnded;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnDynamicCameraToggled -= StaticEventHandler_OnDynamicCameraToggled;
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnNPCInteractionStarted -= StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded -= StaticEventHandler_OnNPCInteractionEnded;
    }

    private void Start()
    {
        SetCinemachineTargetGroup(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
    }

    /// <summary>
    /// Set the cinemachine camera target group
    /// </summary>
    private void SetCinemachineTargetGroup(bool isNpcZoomTriggered, bool npcZoomTriggered)
    {
        // Create target group for cinemachine for the cinemachine camera to follow  - group will include the player and screen cursor
        cinemachineGroupTarget_player = new CinemachineTargetGroup.Target
        {
            Weight = 1f,
            Radius = 2.5f,
            Object = GameManager.Instance.GetPlayer().transform
        };

        cinemachineGroupTarget_cursor = new CinemachineTargetGroup.Target
        {
            Weight = 1f,
            Radius = 1f,
            Object = cursorTarget
        };

        if (npcZoomTriggered)
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
        if (roomChangedEventArgs.room.roomNodeType.isShopRoom)
        {
            // Get npc vendor
            npcVendor = roomChangedEventArgs.room.instantiatedRoom.GetComponentInChildren<NPC>();

        }
    }

    private void StaticEventHandler_OnNPCInteractionStarted(NpcInteractionStartedArgs npcInteractionStartedArgs)
    {
        npcZoomTriggered = true;

        // Cache npc vendor object to added cinemachine target reference
        cinemachineGroupTarget_vendor = new CinemachineTargetGroup.Target
        {
            Weight = 1.5f,
            Radius = 1f,
            Object = npcVendor.transform
        };

        ApplyCameraTargets(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
    }

    private void StaticEventHandler_OnNPCInteractionEnded()
    {
        npcZoomTriggered = false;

        ApplyCameraTargets(InterScenesSingleton.dynamicCameraFollowEnabled, npcZoomTriggered);
    }

    private void Update()
    {
        cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
    }
}
