using TMPro;
using UnityEngine;

public class DamagePopup : BaseLoadComponent
{
    [Header("Components & Settings")]
    [SerializeField] protected TextMeshPro textMesh;
    [SerializeField] protected float moveYSpeed = 2.5f;
    [SerializeField] protected float disappearSpeed = 3f;
    [SerializeField] protected float disappearTimer = 0.6f;

    protected Color textColor;
    protected Vector3 moveVector;
    protected bool isRecycled = false;

    public static DamagePopup Create(Vector3 position, float damageAmount, bool isCritical = false, bool isHeal = false)
    {
        return DamagePopupPool.Instance.Get(position, damageAmount, isCritical, isHeal);
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.InitTextMesh();
    }

    protected virtual void InitTextMesh()
    {
        if (this.textMesh == null)
        {
            this.textMesh = GetComponent<TextMeshPro>();
            if (this.textMesh == null)
            {
                this.textMesh = this.gameObject.AddComponent<TextMeshPro>();
            }
        }

        if (this.textMesh.font == null && TMP_Settings.defaultFontAsset != null)
        {
            this.textMesh.font = TMP_Settings.defaultFontAsset;
        }

        this.textMesh.alignment = TextAlignmentOptions.Center;
        this.textMesh.rectTransform.sizeDelta = new Vector2(10f, 2f);
        this.textMesh.textWrappingMode = TextWrappingModes.NoWrap;
        this.textMesh.fontStyle = FontStyles.Bold;
        this.textMesh.outlineWidth = 0.22f;
        this.textMesh.outlineColor = new Color32(0, 0, 0, 230);
        this.textMesh.sortingOrder = 1000;                 
    }

    public virtual void Setup(float amount, bool isCritical = false, bool isHeal = false)
    {
        this.isRecycled = false;
        this.InitTextMesh();

        string amountStr = amount >= 1f ? Mathf.RoundToInt(amount).ToString() : amount.ToString("0.#");

        if (isHeal)
        {
            this.textMesh.text = $"+{amountStr}";
            this.textColor = new Color(0.2f, 0.95f, 0.3f, 1f); 
            this.textMesh.fontSize = 5f;
            this.transform.localScale = Vector3.one;
        }
        else if (isCritical)
        {
            this.textMesh.text = $"CRIT! -{amountStr}";
            this.textColor = new Color(1f, 0.88f, 0.05f, 1f); 
            this.textMesh.fontSize = 6.5f;
            this.transform.localScale = Vector3.one * 1.3f;   
        }
        else
        {
            this.textMesh.text = $"-{amountStr}";
            this.textColor = new Color(1f, 0.22f, 0.22f, 1f); 
            this.textMesh.fontSize = 5f;
            this.transform.localScale = Vector3.one;
        }

        this.textMesh.color = this.textColor;
        this.disappearTimer = 0.6f;
        this.moveVector = new Vector3(Random.Range(-0.4f, 0.4f), 1.2f, 0f) * this.moveYSpeed;

        this.LookAtCamera();
    }

    protected virtual void Update()
    {
        this.transform.position += this.moveVector * Time.deltaTime;
        this.moveVector -= this.moveVector * 1.5f * Time.deltaTime;

        this.LookAtCamera();

        if (this.disappearTimer > 0.45f)
        {
            this.transform.localScale += Vector3.one * 1.2f * Time.deltaTime;
        }
        else if (this.disappearTimer > 0.2f)
        {
            this.transform.localScale -= Vector3.one * 0.6f * Time.deltaTime;
        }

        this.disappearTimer -= Time.deltaTime;
        if (this.disappearTimer < 0f)
        {
            this.textColor.a -= this.disappearSpeed * Time.deltaTime;
            this.textMesh.color = this.textColor;

            if (this.textColor.a <= 0f)
            {
                this.ReturnToPool();
            }
        }
    }

    public virtual void ReturnToPool()
    {
        if (this.isRecycled) return;
        this.isRecycled = true;

        if (DamagePopupPool.Instance != null)
        {
            DamagePopupPool.Instance.ReturnToPool(this);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    protected virtual void OnDisable()
    {
        this.isRecycled = true;
    }

    protected virtual void LookAtCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) cam = Camera.current;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            this.transform.rotation = cam.transform.rotation;
        }
    }
}
