using Mirror;
using UnityEngine;

public static class HealthAuthorityResolver
{
    public static IHealthAuthority GetAuthority(GameObject target) => target.GetComponent<IHealthAuthority>();
}
