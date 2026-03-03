using System;
using UnityEngine;

public class EnemyAIEvent : MonoBehaviour
{
    public event Action<EnemyAIEvent> OnEnemyHitTheWall;

    public void CallEnemyHitTheWallEvent()
    {
        OnEnemyHitTheWall?.Invoke(this);
    }
}

