using UnityEngine;

[DisallowMultipleComponent]
public class PoolIdentity : MonoBehaviour
{
    [SerializeField] int poolId;
    [SerializeField] int slotIndex;

    public int PoolId => poolId;
    public int SlotIndex => slotIndex;

    public void InitializeSlotIndex(int index)
    {
        slotIndex = index;
    }
}
