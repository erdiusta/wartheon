using UnityEngine;

public class EnemyDatabase : SingletonMonobehaviour<EnemyDatabase>
{
    [SerializeField] EnemyDatabaseSO mobDatabase;

    protected override void Awake()
    {
        base.Awake();

        mobDatabase.Initialize();
    }

    public EnemyDetailsSO GetEnemy(EnemyCategory category)
    {
        return mobDatabase.GetEnemy(category);
    }
}
