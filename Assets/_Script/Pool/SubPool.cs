using System;
using System.Collections.Generic;
using UnityEngine;
//IPoolable két thừa từ object rồi override lại để khi spawn thì nó làm gì và khi return thì nó làm gì
//PooledObject kêt thừa object khi sopawn sẽ load tới để gán id
[Serializable]
public class SubPool
{
    [SerializeField] protected string poolKey;
    [SerializeField] protected GameObject prefab;
    [SerializeField] protected int defaultSize = 10;
    [SerializeField] protected bool canGrow = true;
    [SerializeField] protected Transform container;
    protected readonly Queue<GameObject> inactiveQueue = new Queue<GameObject>();

    public string PoolKey => this.poolKey;
    public GameObject Prefab => this.prefab;
    public int DefaultSize => this.defaultSize;
    public bool CanGrow => this.canGrow;
    public Transform Container => this.container;

    public SubPool(string key, GameObject prefab, int defaultSize, bool canGrow = true)
    {
        this.poolKey = key;
        this.prefab = prefab;
        this.defaultSize = defaultSize;
        this.canGrow = canGrow;
    }

    public virtual void Initialize()
    {
        if (this.container == null)
        {
            GameObject subContainer = new GameObject($"[SubPool_{this.poolKey}]");
            this.container = subContainer.transform;
        }
        this.Prewarm();
    }

    public virtual void Prewarm()
    {
        if (this.prefab == null)
        {
            Debug.LogWarning("Prefab chưa được gán, không thể Prewarm!");
            return;
        }

        while (this.inactiveQueue.Count < this.defaultSize)
        {
            GameObject obj = this.CreateNewInstance();
            obj.SetActive(false);
            this.inactiveQueue.Enqueue(obj);
        }
    }

    protected virtual GameObject CreateNewInstance()
    {
        GameObject obj = GameObject.Instantiate(this.prefab, this.container);
        obj.name = this.prefab.name+ "_" +this.inactiveQueue.Count + 1;
        PooledObject pooledTracker= obj.GetComponent<PooledObject>();
        
        pooledTracker.PoolKey = this.poolKey;
        return obj;
    }

    public virtual GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null; 
        if (this.inactiveQueue.Count > 0)
        {
            obj = this.inactiveQueue.Dequeue();
        }
        if (obj == null)
        {
            if (!this.canGrow)
            {
                Debug.LogWarning($"[SubPool: Không được phép mở rộng");
                return null;
            }
            obj = this.CreateNewInstance();
        }
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public virtual void Despawn(GameObject obj)
    {
        if (obj == null) return;
        IPoolable poolables = obj.GetComponentInChildren<IPoolable>(true);
        if (poolables !=null)
        {
            poolables.OnReturnToPool();
        }
        obj.SetActive(false);
        this.inactiveQueue.Enqueue(obj);
    }
}
