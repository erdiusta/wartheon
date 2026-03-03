using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProjectileDatabase_", menuName = "Scriptable Objects/Weapons/ProjectileHitFx Database")]
public class ProjectileHitEffectDatabaseSO : ScriptableObject
{
    [SerializeField] List<ProjectileHitEffectSO> projectilesHitFxList;

    Dictionary<int, ProjectileHitEffectSO> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<int, ProjectileHitEffectSO>();

        for (int i = 0; i < projectilesHitFxList.Count; i++)
        {
            if (projectilesHitFxList[i] == null)
            {
                Debug.LogError($"Null projectile at index {i}");
                continue;
            }

            lookup.Add(i, projectilesHitFxList[i]);
        }
    }

    public ProjectileHitEffectSO GetProjectileHitFx(int id)
    {
        if (lookup == null)
        {
            Debug.LogError("ProjectileDatabaseSO not initialized!");
            return null;
        }

        if (!lookup.TryGetValue(id, out ProjectileHitEffectSO projectileHitFx))
        {
            Debug.LogError($"Projectile ID not found: {id}");
            return null;
        }

        return projectileHitFx;
    }

    public int GetProjectileHitFxId(ProjectileHitEffectSO details)
    {
        for (int i = 0; i < projectilesHitFxList.Count; i++)
        {
            if (projectilesHitFxList[i] == details)
            {
                return i;
            }
        }

        Debug.LogError("Projectile not found in database!");
        return -1;
    }
}
