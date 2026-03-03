using UnityEngine;

public class WartheonDatabase : SingletonMonobehaviour<WartheonDatabase>
{
    [SerializeField] EnemyDatabaseSO mobDatabase;
    [SerializeField] ProjectileDatabaseSO projectileDatabase;
    [SerializeField] ProjectileHitEffectDatabaseSO projectileHitFxDatabase;

    protected override void Awake()
    {
        base.Awake();

        mobDatabase.Initialize();
        projectileDatabase.Initialize();
        projectileHitFxDatabase.Initialize();
    }

    public EnemyDetailsSO GetEnemy(EnemyCategory category)
    {
        return mobDatabase.GetEnemy(category);
    }

    public ProjectileDetailsSO GetProjectile(int index)
    {
        return projectileDatabase.GetProjectile(index);
    }

    public int GetProjectileId(ProjectileDetailsSO details)
    {
        return projectileDatabase.GetProjectileId(details);
    }

    public ProjectileHitEffectSO GetProjectileHitFx(int index)
    {
        return projectileHitFxDatabase.GetProjectileHitFx(index);
    }

    public int GetProjectileHitFxId(ProjectileHitEffectSO details)
    {
        return projectileHitFxDatabase.GetProjectileHitFxId(details);
    }
}
