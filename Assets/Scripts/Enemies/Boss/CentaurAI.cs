using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CentaurAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    public bool PassedToWait { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void HandleWaitPhase()
    {
        throw new NotImplementedException();
    }

    public void PlayerStealthCheck()
    {
        throw new NotImplementedException();
    }
}
