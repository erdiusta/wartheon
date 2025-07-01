using System.Collections.Generic;
using UnityEngine;

public class SceneObjectsManager : SingletonMonobehaviour<SceneObjectsManager>
{
    [HideInInspector] public List<GameObject> dynamicGameObjectsInScene = new List<GameObject>();

    Player player;

    protected override void Awake()
    {
        base.Awake();
    }
}
