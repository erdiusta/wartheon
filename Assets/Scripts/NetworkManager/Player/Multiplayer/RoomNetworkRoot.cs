using Mirror;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomNetworkRoot : NetworkBehaviour
{
    [SyncVar] public RoomNetData roomNetData;
    [SyncVar] public int randomNum;
    [SyncVar] public int dungeonGenerationId;

    bool built;
    BoxCollider2D rootCollider2D;
    Vector2 entryDirection;
    InstantiatedRoom instantiatedRoom;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        transform.SetParent(DungeonNetworkController.Instance.transform, worldPositionStays: true);

        NetworkClient.RegisterPrefab(GameResources.Instance.dummyMPPrefab);
        NetworkClient.RegisterPrefab(GameResources.Instance.crateMPPrefab);
        NetworkClient.RegisterPrefab(GameResources.Instance.barrelMPPrefab);
        NetworkClient.RegisterPrefab(GameResources.Instance.vaseMPPrefab);

        StartCoroutine(DelayedBuild());

        RemoveSinglePlayerProps();
    }

    [Server]
    public void Server_UnlockDoors()
    {
        RpcUnlockDoors();
    }

    [ClientRpc]
    private void RpcUnlockDoors()
    {
        instantiatedRoom.roomNetData = roomNetData;

        // Room cleared
        instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);
    }

    [Server]
    public void Server_RefreshMusic()
    {
        RpcRefreshMusic();
    }

    [ClientRpc]
    private void RpcRefreshMusic()
    {
        MusicTrackSO music = WartheonDatabase.Instance.GetMusic(GameSessionManager.Instance.selectedDungeonLevelIndex, MusicType.Ambient);
        MusicManager.Instance.PlayMusic(music);
    }

    IEnumerator DelayedBuild()
    {
        float timeout = 5f;
        float timer = 0f;

        while (string.IsNullOrEmpty(roomNetData.templateId))
        {
            timer += Time.deltaTime;

            if (timer >= timeout)
            {
                Debug.LogError($"TemplateId failed to sync for RoomNetworkRoot: {netId}");
            }

            yield return null;
        }

        BuildVisuals(randomNum);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameSessionManager.Instance.levelTransitionInProgress) return;

        if (!collision.CompareTag(Settings.playerTag)) return;

        if (collision is PolygonCollider2D) return;
        if (collision is CapsuleCollider2D) return;

        Player player = GameManager.Instance.GetLocalPlayer();
        if (player == null || !player.IsLocal) return;

        if (roomNetData.roomId == GameSessionManager.Instance.GetCurrentRoomNetData().roomId) return;

        if (!roomNetData.isEntrance)
        {
            if (player != null && player.IsLocal && player.isBattleReadyActive && !roomNetData.isPreviouslyVisited && roomNetData.isCombatRoom) // Battle Ready Mechanic
            {
                IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(player.gameObject);
                healthAuthority.ApplyDamage(-3, default);

                //player.health.AddShield(5);
            }
        }

        MusicManager.Instance.RefreshMusicMP(roomNetData);

        entryDirection = (transform.position - collision.transform.position).normalized;

        if(!roomNetData.isEntrance) player.NetAuth.CmdEnteredRoom(roomNetData.roomId, entryDirection); // Entrance is handled by GameSessionManager on transition separately
    }

    public void BuildVisuals(int randomNum)
    {
        if (built) return;

        built = true;

        RoomTemplateSO template = GameSessionManager.Instance.GetRoomTemplateSO(roomNetData.templateId);

        Vector3 roomPosition = new Vector3(roomNetData.lowerBounds.x - roomNetData.templateLowerBounds.x, roomNetData.lowerBounds.y - roomNetData.templateLowerBounds.y, 0f);
        GameObject roomGameObject = Instantiate(template.prefab, roomPosition, Quaternion.identity);

        BoxCollider2D roomCollider = roomGameObject.GetComponent<BoxCollider2D>();
        rootCollider2D = GetComponent<BoxCollider2D>();

        ApplyRoomColliderToRoot(roomCollider, rootCollider2D);

        instantiatedRoom = roomGameObject.GetComponent<InstantiatedRoom>();
        instantiatedRoom.roomNetData = roomNetData;
        instantiatedRoom.InitializeMultiplayer(roomGameObject);

        if(!isServer) DungeonRuntime.RoomNetDataDict.Add(roomNetData.roomId, roomNetData);

        // Instantiate room
        roomGameObject.transform.SetParent(transform);
        roomGameObject.transform.localPosition = Vector3.zero;

        // Instantiate npc object to the shop room
        if (roomNetData.isShopRoom)
        {
            int npcIndex = 0;

            if (randomNum < 60) npcIndex = 0;
            else if (randomNum < 80) npcIndex = 1;
            else npcIndex = 2;

            // Instantiate NPC into the shop room
            GameObject npcGameObject = Instantiate(GameResources.Instance.npcPrefabs[npcIndex], instantiatedRoom.transform.position,
                Quaternion.identity, instantiatedRoom.transform);
            npcGameObject.transform.localPosition = new Vector3(4f, 9f, 0f);
        }
    }

    private void ApplyRoomColliderToRoot(BoxCollider2D source, BoxCollider2D target)
    {
        if (source == null || target == null) return;

        // World-space bounds of room collider
        Bounds bounds = source.bounds;

        // Convert to root local space
        Vector2 localCenter = (Vector2)transform.InverseTransformPoint(bounds.center);

        target.offset = localCenter;
        target.size = bounds.size;

        source.enabled = false;
    }

    private void RemoveSinglePlayerProps()
    {
        var spProps = GetComponentsInChildren<MPPropReplaceMarker>(true);

        foreach (var sp in spProps)
        {
            Destroy(sp.gameObject);
        }
    }
}
