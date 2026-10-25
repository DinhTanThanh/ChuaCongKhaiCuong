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

    /// <summary>
    /// Đăng ký thêm hoặc tạo động một Sub-Pool mới khi game đang chạy
    /// </summary>
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

    /// <summary>
    /// Lấy một component từ Sub-Pool thông qua Key (ví dụ: PoolManager.Instance.Spawn<CannonBulletController>("Bullet", pos, rot))
    /// </summary>
    public virtual T Spawn<T>(string poolKey, Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject obj = this.Spawn(poolKey, position, rotation);
        if (obj == null) return null;

        return obj.GetComponent<T>();
    }

    /// <summary>
    /// Lấy object từ Pool dựa trên Prefab (Tự động tạo Sub-Pool nếu chưa tồn tại)
    /// </summary>
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

    /// <summary>
    /// Lấy component từ Pool dựa trên Prefab Component
    /// </summary>
    public virtual T Spawn<T>(T prefabComponent, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefabComponent == null) return null;

        GameObject obj = this.Spawn(prefabComponent.gameObject, position, rotation);
        if (obj == null) return null;

        return obj.GetComponent<T>();
    }

    /// <summary>
    /// Thu hồi object về đúng Sub-Pool quản lý nó
    /// </summary>
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

        // Fallback: nếu object không thuộc pool nào
        Debug.LogWarning($"[PoolManager] Object '{obj.name}' không có thông tin Sub-Pool hợp lệ. Đang dùng fallback Destroy!");
        Destroy(obj);
    }

    /// <summary>
    /// Thu hồi object về Sub-Pool sau một khoảng thời gian
    /// </summary>
    public virtual void Despawn(GameObject obj, float delay)
    {
        if (obj == null) return;

        PooledObject tracker = obj.GetComponent<PooledObject>();
        if (tracker != null)
        {
            tracker.ReturnToPool(delay);
        }
        else
        {
            tracker = obj.AddComponent<PooledObject>();
            tracker.ReturnToPool(delay);
        }
    }

    /// <summary>
    /// Lấy tham chiếu đến một Sub-Pool theo Key nếu cần can thiệp trực tiếp
    /// </summary>
    public virtual SubPool GetSubPool(string poolKey)
    {
        this.poolDictionary.TryGetValue(poolKey, out SubPool subPool);
        return subPool;
    }
}
