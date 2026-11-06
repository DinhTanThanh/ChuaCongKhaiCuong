using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : BaseLoadComponent
{
    private static PoolManager instance;
    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PoolManager>();
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("_PoolManager");
                    instance = managerObj.AddComponent<PoolManager>();
                }
            }
            return instance;
        }
    }

    [SerializeField] protected List<SubPool> configuredPools = new List<SubPool>();

    protected readonly Dictionary<string, SubPool> poolDictionary = new Dictionary<string, SubPool>();

    protected override void Awake()
    {
        base.Awake();
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        this.InitializeAllSubPools();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void InitializeAllSubPools()
    {
        foreach (SubPool subPool in this.configuredPools)
        {
            if (string.IsNullOrEmpty(subPool.PoolKey)) continue;

            if (this.poolDictionary.ContainsKey(subPool.PoolKey))
            {
                Debug.LogWarning($"[PoolManager] Trùng lặp PoolKey: '{subPool.PoolKey}'. Bỏ qua cấu hình trùng!");
                continue;
            }

            subPool.Initialize(this.transform);
            this.poolDictionary.Add(subPool.PoolKey, subPool);
        }
    }

    public virtual SubPool CreateSubPool(string poolKey, GameObject prefab, int defaultSize = 10, bool canGrow = true)
    {
        if (string.IsNullOrEmpty(poolKey) || prefab == null) return null;

        if (this.poolDictionary.TryGetValue(poolKey, out SubPool existingPool))
        {
            return existingPool;
        }

        SubPool newSubPool = new SubPool(poolKey, prefab, defaultSize, canGrow);
        newSubPool.Initialize(this.transform);

        this.poolDictionary.Add(poolKey, newSubPool);
        this.configuredPools.Add(newSubPool);

        return newSubPool;
    }

    public virtual GameObject Spawn(string poolKey, Vector3 position, Quaternion rotation)
    {
        if (!this.poolDictionary.TryGetValue(poolKey, out SubPool subPool))
        {
            Debug.LogError($"[PoolManager] Không tìm thấy Sub-Pool với Key: '{poolKey}'!");
            return null;
        }

        return subPool.Spawn(position, rotation);
    }


    public virtual T Spawn<T>(string poolKey, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject obj = this.Spawn(poolKey, position, rotation);
        if (obj == null) return null;

        return obj.GetComponent<T>();
    }


    public virtual GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        string key = prefab.name;
        if (!this.poolDictionary.ContainsKey(key))
        {
            this.CreateSubPool(key, prefab, 10, true);
        }

        return this.Spawn(key, position, rotation);
    }

    public virtual T Spawn<T>(T prefabComponent, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefabComponent == null) return null;

        GameObject obj = this.Spawn(prefabComponent.gameObject, position, rotation);
        if (obj == null) return null;

        return obj.GetComponent<T>();
    }


    public virtual void Despawn(GameObject obj)
    {
        if (obj == null) return;

        PooledObject tracker = obj.GetComponent<PooledObject>();
        if (tracker != null && !string.IsNullOrEmpty(tracker.PoolKey))
        {
            if (this.poolDictionary.TryGetValue(tracker.PoolKey, out SubPool subPool))
            {
                subPool.Despawn(obj);
                return;
            }
        }

        Debug.LogWarning($"[PoolManager] Object '{obj.name}' không có thông tin Sub-Pool hợp lệ. Đang dùng fallback Destroy!");
        Destroy(obj);
    }

    public virtual void Despawn(GameObject obj, float delay)
    {
        if (obj == null) return;
        this.StartCoroutine(this.DespawnDelayRoutine(obj, delay));
    }

    protected virtual IEnumerator DespawnDelayRoutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && obj.activeSelf)
        {
            this.Despawn(obj);
        }
    }

    public virtual SubPool GetSubPool(string poolKey)
    {
        this.poolDictionary.TryGetValue(poolKey, out SubPool subPool);
        return subPool;
    }
}
