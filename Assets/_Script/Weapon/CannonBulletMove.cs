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
    public virtual void Launch(Vector3 direction, float force)
    {
        if (this.rb == null)
        {
            return;
        }
        if (direction == Vector3.zero)
        {
            direction = this.transform.parent.forward;
        }
        if (force <= 0f)
        {
            force = 45f;
        }
        this.rb.isKinematic = false;
        this.rb.WakeUp();
        Vector3 launchVelocity = direction.normalized * force;
        this.rb.linearVelocity = launchVelocity;
        this.SetFacingDirection(direction.normalized);
    }

    protected virtual void FixedUpdate()
    {
        this.UpdateFacingDirection();
    }
    protected virtual void UpdateFacingDirection()
    {
        if (this.rb != null && this.rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            this.SetFacingDirection(this.rb.linearVelocity.normalized);
        }
    }
    protected virtual void SetFacingDirection(Vector3 forwardDirection)
    {
        if (forwardDirection == Vector3.zero) return;

        Transform targetTransform = this.controller.transform;
        if (targetTransform != null)
        {
            targetTransform.forward = forwardDirection;
        }
    }

    public virtual void ResetMove()
    {
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();
        }
    }

    public virtual void StopMove()
    {
        if (this.rb != null)
        {
            this.rb.linearVelocity = Vector3.zero;
            this.rb.angularVelocity = Vector3.zero;
            this.rb.isKinematic = true;
        }
    }
}
