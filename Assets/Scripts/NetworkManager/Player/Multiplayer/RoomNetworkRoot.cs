using Mirror;
using UnityEngine;

[DisallowMultipleComponent]
public class RoomNetworkRoot : NetworkBehaviour
{
    [SyncVar] public string templateId;
    [SyncVar] public RoomNetData roomNetData;
    [SyncVar] public int randomNum;

    bool built;
    bool roomChangeTriggered = false;
    BoxCollider2D rootCollider2D;
    Player player;

    Vector2 entryDirection;

    public override void OnStartClient()
    {
        base.OnStartClient();

        BuildVisuals(roomNetData, randomNum);
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(Settings.playerTag)) return;

        Player player = GameManager.Instance.GetPlayer();
        if (player == null || !player.IsLocal) return;

        if (roomNetData.roomId == GameSessionManager.Instance.GetCurrentRoomNetData().roomId) return;

        if (!roomNetData.isEntrance)
        {
            if (player != null && player.IsLocal && player.isBattleReadyActive && !roomNetData.isPreviouslyVisited && roomNetData.isCombatRoom) // Battle Ready Mechanic
            {
                player.health.AddHealth(3);
                player.health.AddShield(5);
            }
        }

        // Set room as visited
        roomNetData.isPreviouslyVisited = true;

        entryDirection = (transform.position - collision.transform.position).normalized;
        player.NetAuth.CmdEnteredRoom(roomNetData.roomId, entryDirection);
    }

    public void BuildVisuals(RoomNetData data, int randomNum)
    {
        if (built) return;

        built = true;

        RoomTemplateSO template = GameSessionManager.Instance.GetRoomTemplateSO(templateId);

        Vector3 roomPosition = new Vector3(data.lowerBounds.x - data.templateLowerBounds.x, data.lowerBounds.y - data.templateLowerBounds.y, 0f);
        GameObject roomGameObject = Instantiate(template.prefab, roomPosition, Quaternion.identity);

        BoxCollider2D roomCollider = roomGameObject.GetComponent<BoxCollider2D>();
        rootCollider2D = GetComponent<BoxCollider2D>();

        ApplyRoomColliderToRoot(roomCollider, rootCollider2D);

        InstantiatedRoom instantiatedRoom = roomGameObject.GetComponent<InstantiatedRoom>();
        instantiatedRoom.roomNetData = data;
        instantiatedRoom.InitializeMultiplayer(roomGameObject);

        // Instantiate room
        roomGameObject.transform.SetParent(transform);
        roomGameObject.transform.localPosition = Vector3.zero;

        // Instantiate npc object to the shop room
        if (data.isShopRoom)
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
}
