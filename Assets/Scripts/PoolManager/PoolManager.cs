using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    public bool IsInitialized { get; set; }
    public static event Action OnPoolSystemReady;

    #region Tooltip
    [Tooltip("Populate this array with prefabs that you want to add to the pool, and specify the number of gameobjects to be created for each.")]
    #endregion
    public Pool[] poolArray = null;

    Transform objectPoolTransform;
    Dictionary<uint, Queue<Component>> poolDictionary = new Dictionary<uint, Queue<Component>>();

    Dictionary<uint, Transform> poolAnchorDictionary = new Dictionary<uint, Transform>();

    [Serializable]
    public struct Pool
    {
        public uint poolSize;
        public GameObject prefab;
        public string componentType;
    }

    protected override void Awake()
    {
        base.Awake();

        // This singleton gameobject will be the object pool parent
        objectPoolTransform = transform;
    }

    private void Start()
    {
        InitializePoolSP();
    }

    private void InitializePoolSP()
    {
        // Create object pools on start - Only at scene 0, 1 and 2 for only SP sessions
        if (!NetworkServer.active && !NetworkClient.active)
        {
            for (int i = 0; i < poolArray.Length; i++)
            {
                CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType, out NetworkIdentity ni);
            }

            IsInitialized = true;
            OnPoolSystemReady?.Invoke();
        }
    }

    public void InitializePoolMP()
    {
        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePoolMP(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType, out NetworkIdentity ni);

            if (ni != null)
            {
                Debug.Log("Server projectile assetId: " + ni.assetId);
            }
        }

        IsInitialized = true;
        OnPoolSystemReady?.Invoke();
    }

    /// <summary>
    /// Create the object pool with the specified prefabs and the specified pool size for each
    /// </summary>
    private void CreatePool(GameObject prefab, uint poolSize, string componentType, out NetworkIdentity ni)
    {
        ni = prefab.GetComponent<NetworkIdentity>();
        if (ni != null) return; // Don't add NetworkIdentity components to Non-Networked PoolManager

        uint poolKey = GetPoolKey(prefab);

        string prefabName = prefab.name;

        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolAnchorDictionary.ContainsKey(poolKey)) poolAnchorDictionary.Add(poolKey, parentGameObject.transform);

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform);
                newObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    /// <summary>
    /// Create the object pool with the specified prefabs and the specified pool size for each - MP
    /// </summary>
    private void CreatePoolMP(GameObject prefab, uint poolSize, string componentType, out NetworkIdentity ni)
    {
        ni = prefab.GetComponent<NetworkIdentity>();
        if (ni == null) return; // Don't add Non-NetworkIdentity components into Networked Pool Manager

        NetworkClient.RegisterPrefab(prefab);

        uint poolKey = GetPoolKey(prefab);

        string prefabName = prefab.name;

        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolAnchorDictionary.ContainsKey(poolKey)) poolAnchorDictionary.Add(poolKey, parentGameObject.transform);

        if (!poolDictionary.ContainsKey(poolKey))
        {   
            poolDictionary.Add(poolKey, new Queue<Component>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform);
                newObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    /// <summary>
    /// Reuse a gameobject component in the pool.  'prefab' is the prefab gameobject containing the component. 'position' is the world position for the 
    /// gameobject where it should appear when enabled. 'rotation' should be set if the gameobject needs to be rotated.
    /// </summary>
    public Component Reuse(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        uint poolKey = GetPoolKey(prefab);

        if (poolDictionary.ContainsKey(poolKey))
        {
            // Get object from pool queue
            Component componentToReuse = GetComponentFromPool(poolKey);
       
            ResetObject(position, rotation, componentToReuse, prefab);

            if (parent != null) componentToReuse.transform.SetParent(parent);

            if (NetworkServer.active) NetworkServer.Spawn(componentToReuse.gameObject);

            return componentToReuse;
        }
        else
        {
            Debug.Log("No object pool for " + prefab);
            return null;
        }
    }

    IEnumerator DelayReuse(GameObject go, string componentType,uint poolKey)
    {
        yield return null;

        go.SetActive(false);
        poolDictionary[poolKey].Enqueue(go.GetComponent(Type.GetType(componentType)));
    }

    /// <summary>
    /// Get a gameobject component from the pool using the 'poolKey'
    /// </summary>
    private Component GetComponentFromPool(uint poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);

        if (componentToReuse.gameObject.activeSelf == true)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        if (componentToReuse.tag == Settings.enemyTag)
        {
            componentToReuse.gameObject.SetActive(true);
        }

        return componentToReuse;
    }

    /// <summary>
    /// Reset the gameobject
    /// </summary>
    public void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.transform.localScale = prefab.transform.localScale;
    }

    private uint GetPoolKey(GameObject prefab)
    {
        PoolIdentity identity = prefab.GetComponent<PoolIdentity>();

        if (identity == null)
        {
            Debug.LogError($"Prefab {prefab.name} has no PoolIdentity.");
            return 0;
        }

        return (uint)identity.PoolId;
    }

    public Transform GetAnchorParent(GameObject prefab)
    {
        uint poolKey = GetPoolKey(prefab);

        if (poolAnchorDictionary.TryGetValue(poolKey, out Transform anchor)) return anchor;

        return objectPoolTransform; // fallback
    }
}