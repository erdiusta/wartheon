using UnityEngine;

public class WartheonDatabase : SingletonMonobehaviour<WartheonDatabase>
{
    [SerializeField] EnemyDatabaseSO mobDatabase;
    [SerializeField] ProjectileDatabaseSO projectileDatabase;
    [SerializeField] ProjectileHitEffectDatabaseSO projectileHitFxDatabase;
    [SerializeField] WeaponDatabaseSO weaponDatabase;
    [SerializeField] PassiveItemDatabaseSO passiveItemDatabase;

    protected override void Awake()
    {
        base.Awake();

        mobDatabase.Initialize();
        projectileDatabase.Initialize();
        projectileHitFxDatabase.Initialize();
        weaponDatabase.Initialize();
        passiveItemDatabase.Initialize();
    }

    public WeaponDetailsSO GetWeaponDetails(WeaponTitle title)
    {
        return weaponDatabase.GetWeapon(title);
    }

    public WeaponTitle GetWeaponDetailsTitle(WeaponDetailsSO weaponDetails)
    {
        return weaponDatabase.GetWeaponTitle(weaponDetails);
    }

    public PassiveItemDetailsSO GetPassiveItemDetails(PassiveItemType type)
    {
        return passiveItemDatabase.GetPassiveItem(type);
    }

    public PassiveItemType GetPassiveItemDetailsType(PassiveItemDetailsSO passiveItemDetails)
    {
        return passiveItemDatabase.GetPassiveItemType(passiveItemDetails);
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
