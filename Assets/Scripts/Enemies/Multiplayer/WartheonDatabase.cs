using UnityEngine;

public class WartheonDatabase : SingletonMonobehaviour<WartheonDatabase>
{
    [SerializeField] EnemyDatabaseSO mobDatabase;
    [SerializeField] ProjectileDatabaseSO projectileDatabase;
    [SerializeField] ProjectileHitEffectDatabaseSO projectileHitFxDatabase;
    [SerializeField] WeaponDatabaseSO weaponDatabase;
    [SerializeField] PassiveItemDatabaseSO passiveItemDatabase;
    [SerializeField] SoundDatabaseSO soundDatabase;

    protected override void Awake()
    {
        base.Awake();

        mobDatabase.Initialize();
        projectileDatabase.Initialize();
        projectileHitFxDatabase.Initialize();
        weaponDatabase.Initialize();
        passiveItemDatabase.Initialize();
        soundDatabase.Initialize();
    }

    public WeaponDetailsSO GetWeaponDetails(WeaponTitle title)
    {
        return weaponDatabase.GetWeapon(title);
    }

    public PassiveItemDetailsSO GetPassiveItemDetails(PassiveItemType type)
    {
        return passiveItemDatabase.GetPassiveItem(type);
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

    public int GetProjectileHitFxId(ProjectileHitEffectSO details)
    {
        return projectileHitFxDatabase.GetProjectileHitFxId(details);
    }

    public SoundEffectSO GetSound(SoundName soundName)
    {
        return soundDatabase.GetSound(soundName);
    }
}
