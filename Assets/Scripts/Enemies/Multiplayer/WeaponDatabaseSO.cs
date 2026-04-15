using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase_", menuName = "Scriptable Objects/Weapons/Weapon Database")]
public class WeaponDatabaseSO : ScriptableObject
{
    [SerializeField] List<WeaponDetailsSO> weapons;

    Dictionary<WeaponTitle, WeaponDetailsSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<WeaponTitle, WeaponDetailsSO>();

        foreach (WeaponDetailsSO weapon in weapons)
        {
            if (lookup.ContainsKey(weapon.weaponTitle))
            {
                Debug.LogError($"Duplicate WeaponTitle in database: {weapon.weaponTitle}");
                continue;
            }

            lookup.Add(weapon.weaponTitle, weapon);
        }
    }

    public WeaponDetailsSO GetWeapon(WeaponTitle title)
    {
        if (lookup == null)
        {
            Debug.LogError("WeaponDatabaseSO not initialized!");
            return null;
        }

        if (!lookup.TryGetValue(title, out WeaponDetailsSO weapon))
        {
            Debug.LogError($"WeaponTitle not found in database {title}");
            return null;
        }

        return weapon;
    }

    public WeaponTitle GetWeaponTitle(WeaponDetailsSO details)
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weaponTitle == details.weaponTitle)
            {
                return details.weaponTitle;
            }
        }

        Debug.LogError("Weapon not found in database!");
        return 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var categories = new HashSet<WeaponTitle>();

        foreach (var weapon in weapons)
        {
            if (!categories.Add(weapon.weaponTitle))
            {
                Debug.LogError($"Duplicate weapon title: {weapon.weaponTitle}", weapon);
            }
        }
    }
#endif
}
