using UnityEngine;

[DisallowMultipleComponent]
public class PoolIdentity : MonoBehaviour
{
    [SerializeField] int poolId;

    public int PoolId => poolId;
}
