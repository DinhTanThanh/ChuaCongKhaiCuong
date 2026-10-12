using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] protected TextMeshPro textMesh;
    [SerializeField] protected float moveYSpeed = 2.5f;
    [SerializeField] protected float disappearSpeed = 3f;
    [SerializeField] protected float disappearTimer = 0.5f;

    protected Color textColor;
    protected Vector3 moveVector;

    /// <summary>
    /// Tạo nhanh một số sát thương nổi lên tại vị trí position
    /// </summary>
    public static DamagePopup Create(Vector3 position, float damageAmount, bool isCritical = false, bool isHeal = false)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        // Thêm độ lệch ngẫu nhiên nhẹ để các số không bị đè lên nhau
        Vector3 offset = new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(0f, 0.3f), Random.Range(-0.35f, 0.35f));
        popupObj.transform.position = position + offset;

        DamagePopup damagePopup = popupObj.AddComponent<DamagePopup>();
        damagePopup.Setup(damageAmount, isCritical, isHeal);

        return damagePopup;
    }

    protected virtual void Awake()
    {
        if (this.textMesh == null)
        {
            this.textMesh = this.gameObject.AddComponent<TextMeshPro>();
            if (TMP_Settings.defaultFontAsset != null)
            {
                this.textMesh.font = TMP_Settings.defaultFontAsset;
            }
            this.textMesh.alignment = TextAlignmentOptions.Center;
            this.textMesh.fontSize = 5f;
            this.textMesh.sortingOrder = 500; // Hiển thị đè lên trên các layer khác
        }
    }

    public virtual void Setup(float amount, bool isCritical = false, bool isHeal = false)
    {
        if (this.textMesh == null)
        {
            this.textMesh = GetComponent<TextMeshPro>();
        }

        if (isHeal)
        {
            this.textMesh.text = $"+{Mathf.RoundToInt(amount)}";
            this.textColor = new Color(0.2f, 0.95f, 0.3f); // Xanh lá cây hồi máu
            this.textMesh.fontSize = 4.5f;
        }
        else if (isCritical)
        {
            this.textMesh.text = $"CRIT! -{Mathf.RoundToInt(amount)}";
            this.textColor = new Color(1f, 0.85f, 0.1f); // Vàng rực chí mạng
            this.textMesh.fontSize = 6.5f;
        }
        else
        {
            this.textMesh.text = $"-{Mathf.RoundToInt(amount)}";
            this.textColor = new Color(1f, 0.3f, 0.15f); // Đỏ cam sát thương thường
            this.textMesh.fontSize = 5f;
        }

        this.textMesh.color = this.textColor;
        this.disappearTimer = 0.5f;
        this.moveVector = new Vector3(Random.Range(-0.5f, 0.5f), 1f, 0f) * this.moveYSpeed;

        // Xoay mặt về hướng Camera
        this.LookAtCamera();
    }

    protected virtual void Update()
    {
        // Bay lên nhẹ nhàng
        this.transform.position += this.moveVector * Time.deltaTime;
        this.moveVector -= this.moveVector * 1.5f * Time.deltaTime;

        // Luôn xoay mặt theo Camera
        this.LookAtCamera();

        // Hiệu ứng phóng to nhẹ lúc mới xuất hiện
        if (this.disappearTimer > 0.35f)
        {
            this.transform.localScale += Vector3.one * 1.2f * Time.deltaTime;
        }
        else
        {
            this.transform.localScale -= Vector3.one * 0.8f * Time.deltaTime;
        }

        this.disappearTimer -= Time.deltaTime;
        if (this.disappearTimer < 0f)
        {
            // Mờ dần rồi tự hủy
            this.textColor.a -= this.disappearSpeed * Time.deltaTime;
            this.textMesh.color = this.textColor;

            if (this.textColor.a <= 0f)
            {
                Destroy(this.gameObject);
            }
        }
    }

    protected virtual void LookAtCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            this.transform.forward = mainCam.transform.forward;
        }
    }
}
