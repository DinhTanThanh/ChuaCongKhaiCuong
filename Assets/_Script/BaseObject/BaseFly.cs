using System;
using UnityEngine;

public abstract class BaseFly : BaseLoadComponent
{
    [Header("RigidBody & Components")]
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected BaseMove baseMove;
    [SerializeField] protected Collider col;

    [Header("Flight Speeds")]
    [SerializeField] protected float normalFlySpeed = 12f;
    [SerializeField] protected float fastFlySpeed = 24f;
    [SerializeField] protected float ascendSpeed = 8f;
    [SerializeField] protected float descendSpeed = 8f;
    [SerializeField] protected float turnSpeed = 10f;
    [SerializeField] protected float acceleration = 6f;
    [SerializeField] protected float deceleration = 7f;

    [Header("Flight Banking")]
    [SerializeField] protected bool enableTilt = true;
    [SerializeField] protected float maxTiltAngle = 25f;
    [SerializeField] protected float tiltSpeed = 6f;

    [Header("Flight Altitude, Constraints")]
    [SerializeField] protected float minGroundHeight = 0.8f;
    [SerializeField] protected float maxHeight = 200f;
    [SerializeField] protected LayerMask groundLayer = ~0;

    [Header("Flight State")]
    [SerializeField] protected bool isFlying = false;
    [SerializeField] protected bool canFly = true;

    public event Action OnStartFlyingEvent;
    public event Action OnStopFlyingEvent;

    protected Vector3 currentVelocity;
    protected float currentTilt = 0f;
    protected bool previousUseGravity = true;

    public virtual bool IsFlying => this.isFlying;
    public virtual bool CanFly
    {
        get => this.canFly;
        set => this.canFly = value;
    }
    public virtual float CurrentFlySpeed => this.currentVelocity.magnitude;
    public virtual Rigidbody Rigidbody => this.rb;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadRigidbody();
        this.LoadBaseMove();
        this.LoadCollider(); 
    }

    protected virtual void LoadRigidbody()
    {
        if (this.rb != null) return;
        this.rb = GetComponentInParent<Rigidbody>();
    }

    protected virtual void LoadBaseMove()
    {
        if (this.baseMove != null) return;
        if (this.baseMove == null && this.transform.parent != null)
        {
            this.baseMove = this.transform.parent.GetComponentInChildren<BaseMove>();
        }
    }

    protected virtual void LoadCollider()
    {
        if (this.col != null) return;
        if (this.col == null && this.transform.parent != null)
        {
            this.col = this.transform.parent.GetComponentInChildren<Collider>();
        }
    }

    protected virtual void Update()
    {
        if (!this.isFlying) return;
        this.OnFlyingUpdate();
    }

    protected virtual void FixedUpdate()
    {
        if (!this.isFlying) return;
        this.HandleFlyMovement();
        this.HandleFlyRotation();
    }

    public virtual void ToggleFly()
    {
        if (this.isFlying)
        {
            this.StopFlying();
        }
        else
        {
            this.StartFlying();
        }
    }

    public virtual void StartFlying()
    {
        if (!this.canFly || this.isFlying) return;

        this.isFlying = true;

        if (this.rb != null)
        {
            this.previousUseGravity = this.rb.useGravity;
            this.rb.useGravity = false;
            this.rb.linearVelocity = Vector3.zero;
        }

        if (this.baseMove != null)
        {
            this.baseMove.IsMoveEnabled = false;
            this.baseMove.enabled = false;
        }

        this.currentVelocity = Vector3.zero;
        this.currentTilt = 0f;

        this.OnStartFlying();
        this.OnStartFlyingEvent?.Invoke();
    }

    public virtual void StopFlying()
    {
        if (!this.isFlying) return;

        this.isFlying = false;

        if (this.rb != null)
        {
            this.rb.useGravity = this.previousUseGravity;
        }

        if (this.baseMove != null)
        {
            this.baseMove.enabled = true;
            this.baseMove.IsMoveEnabled = true;
        }

        this.ResetTiltRotation();

        this.OnStopFlying();
        this.OnStopFlyingEvent?.Invoke();
    }

    protected virtual void HandleFlyMovement()
    {
        if (this.rb == null) return;

        Vector3 horizDir = this.GetHorizontalFlyDirection();
        bool isBoost = this.IsBoostFlying();
        float targetHorizSpeed = isBoost ? this.fastFlySpeed : this.normalFlySpeed;

        Vector3 targetHorizVelocity = horizDir.sqrMagnitude > 0.001f ? horizDir.normalized * targetHorizSpeed: Vector3.zero;

        float vertInput = this.GetVerticalFlyInput();
        float targetVertSpeed = vertInput * (vertInput > 0f ? this.ascendSpeed : this.descendSpeed);

        Transform moveTransform = this.GetMoveTransform();
        Vector3 currentPos = moveTransform.position;

        if (targetVertSpeed < 0f)
        {
            Vector3 rayOrigin = currentPos + Vector3.up * 0.2f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, (this.minGroundHeight + 0.2f) * 2f, this.groundLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != this.col && hit.distance <= (this.minGroundHeight + 0.2f))
                {
                    targetVertSpeed = Mathf.Max(0f, targetVertSpeed);
                }
            }
        }
        else if (targetVertSpeed > 0f && currentPos.y >= this.maxHeight)
        {
            targetVertSpeed = 0f;
        }

        Vector3 targetVelocity = new Vector3(targetHorizVelocity.x, targetVertSpeed, targetHorizVelocity.z);

        float accelRate = targetVelocity.sqrMagnitude > 0.001f ? this.acceleration : this.deceleration;
        this.currentVelocity = Vector3.Lerp(this.currentVelocity, targetVelocity, accelRate * Time.fixedDeltaTime);

        this.ApplyFlyDisplacement(this.currentVelocity * Time.fixedDeltaTime);
        this.rb.linearVelocity = Vector3.zero;
    }

    protected virtual void ApplyFlyDisplacement(Vector3 displacement)
    {
        if (this.rb != null && !this.rb.isKinematic)
        {
            this.rb.MovePosition(this.rb.position + displacement);
        }
    }

    protected virtual void HandleFlyRotation()
    {
        Transform moveTransform = this.GetMoveTransform();
        if (moveTransform == null) return;

        Vector3 horizDir = this.GetHorizontalFlyDirection();

        if (horizDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetYaw = Quaternion.LookRotation(horizDir.normalized);

            if (this.enableTilt)
            {
                float turnAngle = Vector3.SignedAngle(moveTransform.forward, horizDir.normalized, Vector3.up);
                float targetTilt = Mathf.Clamp(-turnAngle * 1.5f, -this.maxTiltAngle, this.maxTiltAngle);
                this.currentTilt = Mathf.Lerp(this.currentTilt, targetTilt, this.tiltSpeed * Time.fixedDeltaTime);

                Quaternion tiltRotation = Quaternion.Euler(0f, 0f, this.currentTilt);
                Quaternion finalRotation = targetYaw * tiltRotation;

                moveTransform.rotation = Quaternion.Slerp(moveTransform.rotation, finalRotation, this.turnSpeed * Time.fixedDeltaTime);
            }
            else
            {
                moveTransform.rotation = Quaternion.Slerp(moveTransform.rotation, targetYaw, this.turnSpeed * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (this.enableTilt && Mathf.Abs(this.currentTilt) > 0.05f)
            {
                this.currentTilt = Mathf.Lerp(this.currentTilt, 0f, this.tiltSpeed * Time.fixedDeltaTime);
                Vector3 euler = moveTransform.rotation.eulerAngles;
                euler.z = this.currentTilt;
                moveTransform.rotation = Quaternion.Euler(euler);
            }
        }
    }

    protected virtual void ResetTiltRotation()
    {
        Transform moveTransform = this.GetMoveTransform();
        if (moveTransform == null) return;
        Vector3 euler = moveTransform.rotation.eulerAngles;
        euler.z = 0f;
        moveTransform.rotation = Quaternion.Euler(euler);
        this.currentTilt = 0f;
    }

    protected virtual Transform GetMoveTransform()
    {
        return this.rb != null ? this.rb.transform : this.transform;
    }

    protected abstract Vector3 GetHorizontalFlyDirection();
    protected abstract float GetVerticalFlyInput();
    protected abstract bool IsBoostFlying();

    protected virtual void OnStartFlying() { }
    protected virtual void OnStopFlying() { }
    protected virtual void OnFlyingUpdate() { }
}
