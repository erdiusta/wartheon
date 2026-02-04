using Mirror;
using UnityEngine;

public static class HealthAuthorityResolver
{
    public static IHealthAuthority GetAuthority(GameObject target)
    {
        if (target.TryGetComponent(out RoomProp prop) && (NetworkServer.active || NetworkClient.active))
        {
            return prop.Room.GetComponentInParent<RoomPropHealthAuthority>();
        }

        return target.GetComponent<IHealthAuthority>();
    }
}
