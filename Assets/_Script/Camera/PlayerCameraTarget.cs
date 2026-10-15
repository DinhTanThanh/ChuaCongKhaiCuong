using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerCameraTarget : MonoBehaviour
{
    [Header("Camera Anchor Points")]
    [Tooltip("Điểm đặt mắt ở góc nhìn thứ nhất (FPP). Nếu để trống, hệ thống sẽ tự tìm xương Head/Eye hoặc tạo anchor theo eyeOffset.")]
    [SerializeField] protected Transform eyePoint;

    [Tooltip("Offset vị trí mắt nếu eyePoint chưa được chỉ định sẵn.")]
    [SerializeField] protected Vector3 eyeOffset = new Vector3(0f, 1.6f, 0.15f);

    [Tooltip("Điểm camera theo dõi ở góc nhìn thứ 3 (TPP). Nếu để trống, mặc định dùng Transform của nhân vật.")]
    [SerializeField] protected Transform tppTarget;

    [Header("First-Person Mesh Hiding")]
    [Tooltip("Ẩn toàn bộ cơ thể nhân vật khi vào góc nhìn thứ nhất (như GTA V, PUBG, Skyrim) để không bao giờ che mắt camera.")]
    [SerializeField] protected bool hideEntireCharacterInFPP = true;

    [Tooltip("Danh sách Renderer cụ thể cần ẩn (chỉ dùng nếu hideEntireCharacterInFPP = false).")]
    [SerializeField] protected Renderer[] renderersToHideInFPP;

    [Tooltip("Tự động tìm kiếm các mesh đầu/mặt/tóc nếu danh sách trên đang để trống.")]
    [SerializeField] protected bool autoFindHeadMeshesIfEmpty = true;

    [Header("Behavior Settings")]
    [Tooltip("Tự động đăng ký mục tiêu này với CameraPerspectiveManager khi GameObject được kích hoạt (OnEnable).")]
    [SerializeField] protected bool autoRegisterOnEnable = true;

    [Tooltip("Ở góc nhìn thứ nhất, xoay thân nhân vật theo hướng quay ngang (Yaw) của camera.")]
    [SerializeField] protected bool rotateWithCameraInFPP = true;

    protected Transform autoEyePointAnchor;

    public virtual Transform EyePoint => this.GetEyeTransform();
    public virtual Transform TppTarget => this.tppTarget != null ? this.tppTarget : this.transform;
    public virtual bool RotateWithCameraInFPP => this.rotateWithCameraInFPP;

    protected virtual void Awake()
    {
        this.EnsureEyePoint();
        this.EnsureRenderers();
    }

    protected virtual void Start()
    {
        if (this.autoRegisterOnEnable && CameraPerspectiveManager.Instance != null)
        {
            if (CameraPerspectiveManager.Instance.CurrentTarget == null)
            {
                CameraPerspectiveManager.Instance.SetTarget(this);
            }
        }
    }

    protected virtual void OnEnable()
    {
        if (this.autoRegisterOnEnable && CameraPerspectiveManager.Instance != null)
        {
            CameraPerspectiveManager.Instance.SetTarget(this);
        }
    }

    protected virtual void OnDisable()
    {
        // Tránh chạy dọn dẹp khi ứng dụng đang thoát hoặc dừng Play mode
        if (!Application.isPlaying) return;

        if (CameraPerspectiveManager.Instance != null && CameraPerspectiveManager.Instance.CurrentTarget == this)
        {
            CameraPerspectiveManager.Instance.ClearTarget(this);
        }
    }

    public virtual Transform GetEyeTransform()
    {
        if (this.eyePoint != null) return this.eyePoint;
        if (this.autoEyePointAnchor != null) return this.autoEyePointAnchor;

        this.EnsureEyePoint();
        return this.autoEyePointAnchor != null ? this.autoEyePointAnchor : this.transform;
    }

    protected virtual void EnsureEyePoint()
    {
        if (this.eyePoint != null) return;

        // Tự tìm xương Head hoặc Eye trong hierarchy
        Transform[] allChildren = this.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            string childName = child.name.ToLower();
            if (childName.Contains("head") || childName.Contains("eye") || childName.Equals("eyes"))
            {
                this.eyePoint = child;
                return;
            }
        }

        if (this.autoEyePointAnchor == null)
        {
            GameObject anchorObj = new GameObject("[AutoEyePoint]");
            anchorObj.transform.SetParent(this.transform, false);
            anchorObj.transform.localPosition = this.eyeOffset;
            anchorObj.transform.localRotation = Quaternion.identity;
            this.autoEyePointAnchor = anchorObj.transform;
        }
    }

    protected virtual void EnsureRenderers()
    {
        if (this.hideEntireCharacterInFPP) return;
        if (!this.autoFindHeadMeshesIfEmpty) return;
        if (this.renderersToHideInFPP != null && this.renderersToHideInFPP.Length > 0) return;

        Renderer[] allRenderers = this.GetComponentsInChildren<Renderer>(true);
        List<Renderer> foundList = new List<Renderer>();

        foreach (Renderer r in allRenderers)
        {
            string rName = r.gameObject.name.ToLower();
            if (rName.Contains("head") || rName.Contains("hair") || rName.Contains("face") ||
                rName.Contains("eye") || rName.Contains("hat") || rName.Contains("helmet"))
            {
                foundList.Add(r);
            }
        }

        if (foundList.Count > 0)
        {
            this.renderersToHideInFPP = foundList.ToArray();
        }
    }

    public virtual void SetMeshVisibility(bool visible)
    {
        if (this.hideEntireCharacterInFPP)
        {
            Renderer[] all = this.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null) all[i].enabled = visible;
            }
            return;
        }

        if (this.renderersToHideInFPP == null) return;
        for (int i = 0; i < this.renderersToHideInFPP.Length; i++)
        {
            Renderer r = this.renderersToHideInFPP[i];
            if (r != null)
            {
                r.enabled = visible;
            }
        }
    }

    public virtual void RotateBodyYaw(float yawAngle)
    {
        if (!this.rotateWithCameraInFPP) return;
        Vector3 currentEuler = this.transform.eulerAngles;
        this.transform.rotation = Quaternion.Euler(currentEuler.x, yawAngle, currentEuler.z);
    }
}
