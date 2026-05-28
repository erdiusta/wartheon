using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("GAMEPLAY")]
    public Camera gameplayCamera;
    public CinemachineCamera gameplayCinemachine;
    public CinemachineTargetGroup cinemachineTargetGroup;

    [Header("MINIMAP")]
    public Minimap minimap;
    public Camera minimapCamera;

    [Header("DUNGEON OVERVIEW")]
    public DungeonMap dungeonMap;
    public Camera dungeonOverviewCamera;

    Player player;

    private void Awake()
    {
        player = GetComponentInParent<Player>();

        // Hard off for everyone by default
        gameplayCamera.gameObject.SetActive(false);
        minimapCamera.gameObject.SetActive(false);
        dungeonOverviewCamera.gameObject.SetActive(false);
    }

    public void ShowGameplay(bool onStart = false)
    {
        gameplayCamera?.gameObject.SetActive(true);
        minimapCamera?.gameObject.SetActive(true);
        dungeonOverviewCamera?.gameObject.SetActive(false);

        if (onStart) gameplayCamera.GetComponentInChildren<AudioListener>().enabled = true;

        if (player.IsLocal) gameplayCinemachine.Priority = 15;
    }

    public void ShowDungeonOverview()
    {
        gameplayCamera?.gameObject.SetActive(false);
        minimapCamera?.gameObject.SetActive(false);
        dungeonOverviewCamera?.gameObject.SetActive(true);
    }

    public Camera GetGameplayCamera() => gameplayCamera;
    public CinemachineCamera GetCinemachineCamera() => gameplayCinemachine;
    public CinemachineTargetGroup GetCinemachineTargetGroup() => cinemachineTargetGroup;
    public Camera GetMinimapCamera() => minimapCamera;
    public Camera GetDungeonOverviewCamera() => dungeonOverviewCamera;
}
