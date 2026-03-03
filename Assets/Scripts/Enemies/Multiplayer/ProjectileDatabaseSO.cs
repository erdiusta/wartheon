using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProjectileDatabase_", menuName = "Scriptable Objects/Weapons/Projectile Database")]
public class ProjectileDatabaseSO : ScriptableObject
{
    [SerializeField] List<ProjectileDetailsSO> projectiles;

    Dictionary<int, ProjectileDetailsSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<int, ProjectileDetailsSO>();

        for (int i = 0; i < projectiles.Count; i++)
        {
            if (projectiles[i] == null)
            {
                Debug.LogError($"Null projectile at index {i}");
                continue;
            }

            lookup.Add(i, projectiles[i]);
        }
    }

    public ProjectileDetailsSO GetProjectile(int id)
    {
        if (lookup == null)
        {
            Debug.LogError("ProjectileDatabaseSO not initialized!");
            return null;
        }

        if (!lookup.TryGetValue(id, out ProjectileDetailsSO projectile))
        {
            Debug.LogError($"Projectile ID not found: {id}");
            return null;
        }

        return projectile;
    }

    public int GetProjectileId(ProjectileDetailsSO details)
    {
        for (int i = 0; i < projectiles.Count; i++)
        {
            if (projectiles[i] == details)
            {
                return i;
            }
        }

        Debug.LogError("Projectile not found in database!");
        return -1;
    }
}
