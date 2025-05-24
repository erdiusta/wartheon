using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

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

    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnDynamicCameraToggled += StaticEventHandler_OnDynamicCameraToggled;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnDynamicCameraToggled -= StaticEventHandler_OnDynamicCameraToggled;
    }

    private void Start()
    {
        SetCinemachineTargetGroup();
    }

    /// <summary>
    /// Set the cinemachine camera target group
    /// </summary>
    private void SetCinemachineTargetGroup()
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

        DynamicCameraFollowToggle(InterScenesSingleton.dynamicCameraFollowEnabled);
    }

    private void StaticEventHandler_OnDynamicCameraToggled(DynamicCameraFollowArgs dynamicCameraFollowArgs)
    {
        DynamicCameraFollowToggle(dynamicCameraFollowArgs.isOn);
    }

    private void DynamicCameraFollowToggle(bool isOn)
    {
        if (isOn)
        {
            List<CinemachineTargetGroup.Target> cinemachineTargetList = new List<CinemachineTargetGroup.Target> { cinemachineGroupTarget_player,
                cinemachineGroupTarget_cursor };
            cinemachineTargetGroup.Targets = cinemachineTargetList;
        }
        else
        {
            List<CinemachineTargetGroup.Target> cinemachineTargetList = new List<CinemachineTargetGroup.Target> { cinemachineGroupTarget_player };
            cinemachineTargetGroup.Targets = cinemachineTargetList;
        }
    }

    private void Update()
    {
        cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
    }
}
