using UnityEngine;

public class CannonBulletMove : BaseLoadComponent
{
    [Header("Bullet Move Components")]
    [SerializeField] protected CannonBulletController controller;
    [SerializeField] protected Rigidbody rb;

    public virtual CannonBulletController Controller => this.controller;
    public virtual Rigidbody Rigidbody => this.rb;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadController();
        this.LoadRigidbody();
    }

    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<CannonBulletController>();
    }

    protected virtual void LoadRigidbody()
    {
        if (this.rb != null) return;
        this.rb = GetComponentInParent<Rigidbody>();
    }
    /// Cấp vận tốc ban đầu cho đạn
    public virtual void Launch(Vector3 direction, float force)
    {
        if (this.rb == null) this.LoadRigidbody();
        if (this.rb == null)
        {
            Debug.LogWarning($"[{this.gameObject.name}] CannonBulletMove: Không tìm thấy Rigidbody trên viên đạn để phóng đi!");
            return;
        }

        // Đảm bảo hướng bay hợp lệ
        if (direction == Vector3.zero)
        {
            direction = this.transform.forward;
        }

        // Đảm bảo lực phóng tối thiểu
        if (force <= 0f)
        {
            force = 45f;
        }

        // Đảm bảo Rigidbody không bị kinematic và đánh thức vật lý
        this.rb.isKinematic = false;
        this.rb.WakeUp();

        // Gán vận tốc bay tức thời cho Rigidbody
        Vector3 launchVelocity = direction.normalized * force;
        this.rb.linearVelocity = launchVelocity;

        // Xoay đầu đạn theo hướng bắn ngay lập tức
        this.SetFacingDirection(direction.normalized);
    }

    protected virtual void FixedUpdate()
    {
        this.UpdateFacingDirection();
    }

    /// <summary>
    /// Xoay đầu đạn chúc theo đường cong quỹ đạo rơi của Rigidbody
    /// </summary>
    protected virtual void UpdateFacingDirection()
    {
        if (this.rb != null && this.rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            this.SetFacingDirection(this.rb.linearVelocity.normalized);
        }
    }

    /// <summary>
    /// Cập nhật hướng quay an toàn cho GameObject viên đạn (tránh lỗi null parent)
    /// </summary>
    protected virtual void SetFacingDirection(Vector3 forwardDirection)
    {
        if (forwardDirection == Vector3.zero) return;

        Transform targetTransform = this.controller != null ? this.controller.transform : (this.transform.parent != null ? this.transform.parent : this.transform);
        if (targetTransform != null)
        {
            targetTransform.forward = forwardDirection;
        }
    }
}
