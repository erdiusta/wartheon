using UnityEngine;

public class RoomPropIndexer : MonoBehaviour
{
    public RoomProp[] Props { get; private set; }

    private void Awake()
    {
        BuildIndex();
    }

    public void BuildIndex()
    {
        Props = GetComponentsInChildren<RoomProp>();

        for (int i = 0; i < Props.Length; i++)
        {
            Props[i].propId = i;
        }
    }

    public RoomProp Get(int propId)
    {
        for (int i = 0; i < Props.Length; i++)
        {
            if (propId == i) return Props[i];
        }

        return null;
    }
}
