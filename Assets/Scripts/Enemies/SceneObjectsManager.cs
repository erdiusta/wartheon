using System.Collections.Generic;
using UnityEngine;

public class SceneObjectsManager : SingletonMonobehaviour<SceneObjectsManager>
{
    [HideInInspector] public static List<GameObject> dynamicGameObjectsInScene = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
    }
}
