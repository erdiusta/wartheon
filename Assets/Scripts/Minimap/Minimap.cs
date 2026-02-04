using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using Mirror;

[DisallowMultipleComponent]
public class Minimap : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the child minimap icon container gameobject")]
    #endregion Tooltip
    [SerializeField] Transform iconContainer;
    #region Tooltip
    [Tooltip("Populate with the child icon prefab gameobject")]
    #endregion Tooltip
    [SerializeField] MinimapIcon iconPrefab;
    #region Tooltip
    [Tooltip("Populate with the minimapCinemachine gameobject")]
    #endregion
    [SerializeField] CinemachineCamera minimapCinemachine;
    #region Tooltip
    [Tooltip("Populate with the light2d gameobject")]
    #endregion
    [SerializeField] Light2D minimapLight2D;

    Player player;
    Dictionary<Player, MinimapIcon> icons = new Dictionary<Player, MinimapIcon>();

    private void Awake()
    {
        minimapCinemachine = GetComponentInChildren<CinemachineCamera>(true);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    IEnumerator WaitForPlayerInitialization()
    {
        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        while (player == null || !player.IsLocal)
        {
            if (isMultiplayer && FindObjectsByType<Player>(FindObjectsSortMode.None).Length != NetworkServer.connections.Count)
            {
                yield return null;
            }

            player = GameManager.Instance.GetPlayer();
            yield return null;
        }

        InitializeMinimap(isMultiplayer);
    }

    private void InitializeMinimap(bool isMultiplayer)
    {
        MinimapIcon localPlayerIcon = new MinimapIcon();
        MinimapIcon remotePlayerIcon;

        if (isMultiplayer)
        {
            foreach (Player player in FindObjectsByType<Player>(FindObjectsSortMode.None))
            {
                if (player.IsLocal)
                {
                    localPlayerIcon = Instantiate(iconPrefab, iconContainer);
                    PopulatePlayerIcon(localPlayerIcon, player);
                }
                else
                {
                    remotePlayerIcon = Instantiate(iconPrefab, iconContainer);
                    PopulatePlayerIcon(remotePlayerIcon, player);
                }
            }
        }
        else
        {
            // Create for SP
            localPlayerIcon = Instantiate(iconPrefab, iconContainer);
            PopulatePlayerIcon(localPlayerIcon, player);
        }

        if (player.IsLocal && icons.TryGetValue(player, out MinimapIcon localIcon))
        {
            // Follow local player
            minimapCinemachine.Follow = player.transform;

            // Anchor
            localPlayerIcon.transform.position = player.transform.position;
        }
    }

    private void PopulatePlayerIcon(MinimapIcon icon, Player player)
    {
        icon.target = player.transform;
        icon.GetComponent<SpriteRenderer>().sprite = player.playerDetails.playerMiniMapIcon;
        icons.Add(player, icon);
    }
}
