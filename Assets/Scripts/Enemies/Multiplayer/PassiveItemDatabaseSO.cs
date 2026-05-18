using Mirror.Examples.Tanks;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveItemDatabase_", menuName = "Scriptable Objects/Passive Items/Passive Item Database")]
public class PassiveItemDatabaseSO : ScriptableObject
{
    [SerializeField] List<PassiveItemDetailsSO> passiveItems;

    Dictionary<PassiveItemType, PassiveItemDetailsSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<PassiveItemType, PassiveItemDetailsSO>();

        foreach (PassiveItemDetailsSO passiveItem in passiveItems)
        {
            if (lookup.ContainsKey(passiveItem.passiveItemType))
            {
                Debug.LogError($"Duplicate PassiveItem type in database: {passiveItem.passiveItemType}");
                continue;
            }

            lookup.Add(passiveItem.passiveItemType, passiveItem);
        }
    }

    public PassiveItemDetailsSO GetPassiveItem(PassiveItemType passiveItemType)
    {
        if (lookup == null)
        {
            Debug.LogError("PassiveItemDatabaseSO not initialized!");
            return null;
        }

        if (!lookup.TryGetValue(passiveItemType, out PassiveItemDetailsSO passiveItem))
        {
            Debug.LogError($"PassiveItemType not found in database {passiveItemType}");
            return null;
        }

        if (passiveItemType == PassiveItemType.None)
        {
            Debug.Log("PASSIVE ITEM TYPE IS NONE!");
        }

        return passiveItem;
    }

    public PassiveItemType GetPassiveItemType(PassiveItemDetailsSO details)
    {
        for (int i = 0; i < passiveItems.Count; i++)
        {
            if (passiveItems[i].passiveItemType == details.passiveItemType)
            {
                return details.passiveItemType;
            }
        }

        Debug.LogError("Passive item not found in database!");
        return 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var categories = new HashSet<PassiveItemType>();

        foreach (var passiveItem in passiveItems)
        {
            if (!categories.Add(passiveItem.passiveItemType))
            {
                Debug.LogError($"Duplicate passive item title: {passiveItem.passiveItemType}", passiveItem);
            }
        }
    }
#endif
}
