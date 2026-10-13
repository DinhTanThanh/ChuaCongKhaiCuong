using System.Collections.Generic;
using UnityEngine;

public class DamagePopupPool : BaseLoadComponent
{
    private static DamagePopupPool instance;
    public static DamagePopupPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DamagePopupPool>();
                if (instance == null)
                {
                    GameObject poolObj = new GameObject("_DamagePopupPool");
                    instance = poolObj.AddComponent<DamagePopupPool>();
                }
            }
            return instance;
        }
    }

    [Header("Pool Configuration")]
    [Tooltip("Prefab DamagePopup tùy chọn. Nếu để trống, hệ thống sẽ tự động khởi tạo linh hoạt bằng code.")]
    [SerializeField] protected DamagePopup popupPrefab;
    [SerializeField] protected int defaultPoolSize = 15;
    [SerializeField] protected Transform poolContainer;

    protected readonly Queue<DamagePopup> pool = new Queue<DamagePopup>();

    protected override void Awake()
    {
        base.Awake();
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        if (this.poolContainer == null)
        {
            this.poolContainer = this.transform;
        }

        this.PrewarmPool();
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadPoolContainer();
    }

    protected virtual void LoadPoolContainer()
    {
        if (this.poolContainer != null) return;
        this.poolContainer = this.transform;
    }

    protected virtual void PrewarmPool()
    {
        for (int i = 0; i < this.defaultPoolSize; i++)
        {
            DamagePopup popup = this.CreateNewPopup();
            popup.gameObject.SetActive(false);
            this.pool.Enqueue(popup);
        }
    }

    protected virtual DamagePopup CreateNewPopup()
    {
        DamagePopup popup;
        if (this.popupPrefab != null)
        {
            popup = Instantiate(this.popupPrefab, this.poolContainer);
        }
        else
        {
            GameObject popupObj = new GameObject("DamagePopup");
            popupObj.transform.SetParent(this.poolContainer);
            popup = popupObj.AddComponent<DamagePopup>();
        }

        return popup;
    }

    public virtual DamagePopup Get(Vector3 position, float damageAmount, bool isCritical = false, bool isHeal = false)
    {
        DamagePopup popup = null;

        // Lấy từ pool, kiểm tra trường hợp object có thể đã bị hủy ngoài ý muốn
        while (this.pool.Count > 0)
        {
            popup = this.pool.Dequeue();
            if (popup != null) break;
        }

        // Nếu pool hết hoặc chưa có object hợp lệ thì tạo mới (dynamic expand)
        if (popup == null)
        {
            popup = this.CreateNewPopup();
        }

        Vector3 offset = new Vector3(
            Random.Range(-0.35f, 0.35f),
            Random.Range(0f, 0.35f),
            Random.Range(-0.35f, 0.35f)
        );

        popup.transform.position = position + offset;
        popup.gameObject.SetActive(true);
        popup.Setup(damageAmount, isCritical, isHeal);

        return popup;
    }

    public virtual void ReturnToPool(DamagePopup popup)
    {
        if (popup == null) return;
        if (!popup.gameObject.activeSelf) return; // Tránh thu hồi 2 lần

        popup.gameObject.SetActive(false);
        if (this.poolContainer != null)
        {
            popup.transform.SetParent(this.poolContainer);
        }

        this.pool.Enqueue(popup);
    }
}
