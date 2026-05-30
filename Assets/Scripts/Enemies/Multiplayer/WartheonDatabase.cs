using UnityEngine;

public class WartheonDatabase : SingletonMonobehaviour<WartheonDatabase>
{
    [SerializeField] EnemyDatabaseSO mobDatabase;
    [SerializeField] ProjectileDatabaseSO projectileDatabase;
    [SerializeField] ProjectileHitEffectDatabaseSO projectileHitFxDatabase;
    [SerializeField] WeaponDatabaseSO weaponDatabase;
    [SerializeField] PassiveItemDatabaseSO passiveItemDatabase;
    [SerializeField] SoundDatabaseSO soundDatabase;
    [SerializeField] MusicDatabaseSO musicDatabase;

    protected override void Awake()
    {
        base.Awake();

        mobDatabase.Initialize();
        projectileDatabase.Initialize();
        projectileHitFxDatabase.Initialize();
        weaponDatabase.Initialize();
        passiveItemDatabase.Initialize();
        soundDatabase.Initialize();
        musicDatabase.Initialize();
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

    public SoundEffectSO GetSound(SoundName soundName)
    {
        return soundDatabase.GetSound(soundName);
    }

    public MusicTrackSO GetMusic(int level, MusicType musicType)
    {
        return musicDatabase.GetMusic(level, musicType);
    }
}
