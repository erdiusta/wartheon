using UnityEngine;

public class RoomProp : MonoBehaviour
{
    [HideInInspector] public int propId;
    [HideInInspector] public Health health;
    [HideInInspector] public InstantiatedRoom Room;

    private void Awake()
    {
        health = GetComponent<Health>();
    }
}
