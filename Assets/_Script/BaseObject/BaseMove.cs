using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class BaseMove : BaseLoadComponent
{
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float turnSpeed = 10f;
    [SerializeField] protected float jumpHeight = 0.65f;
    [SerializeField] protected float gravity = -9.81f;
    [SerializeField] protected bool isGrounded = false;

    [SerializeField] protected LayerMask groundLayer = ~0;
    [SerializeField] protected Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] protected float groundRayDistance = 0.2f;
    [SerializeField] protected Vector3 velocity;
    [SerializeField] protected Vector3 moveDirection;
    [SerializeField] protected Rigidbody rb;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadRigibody();
    }
    protected virtual void LoadRigibody()
    {
        if (this.rb != null) return;
        this.rb = GetComponentInParent<Rigidbody>();
    }

    protected virtual void Update()
    {
        this.CheckGrounded();
    }

    protected virtual void FixedUpdate()
    {
        this.HandleMovement();
        this.HandleMoveDirection();
    }

    protected virtual void ApplyDisplacement(Vector3 displacement)
    {
        if (this.rb != null && !this.rb.isKinematic)
        {
            this.rb.MovePosition(this.rb.position + displacement);
        }
    }
    protected virtual void Move(Vector3 moveDirection)
    {
        Vector3 displacement = moveDirection.normalized * (this.speed * Time.fixedDeltaTime);
        this.ApplyDisplacement(displacement);
    }
    protected virtual void HandleMovement()
    {
        this.moveDirection = GetMoveDirection();
        this.Move(this.moveDirection);
    }
    protected virtual void HandleMoveDirection()
    {
        if (this.moveDirection.sqrMagnitude < 0.001f) return;
        Transform moveTransform = this.GetMoveTransform();
        if (moveTransform == null) return;
        Quaternion targetRotation = Quaternion.LookRotation(this.moveDirection);
        moveTransform.rotation = Quaternion.Slerp(moveTransform.rotation, targetRotation, this.turnSpeed * Time.fixedDeltaTime);
    }

    public virtual void Jump()
    {
        if (this.isGrounded && this.rb != null)
        {
            // Vận tốc đầu cần đạt để lên độ cao jumpHeight: v = sqrt(-2 * g * h)
            float g = Mathf.Abs(Physics.gravity.y != 0 ? Physics.gravity.y : this.gravity);
            float jumpSpeed = Mathf.Sqrt(2f * g * this.jumpHeight);

            // Đặt trực tiếp vận tốc hướng lên cho Rigidbody (tương thích cả Unity mới linearVelocity và velocity cũ)
            Vector3 currentVel = this.rb.linearVelocity;
            currentVel.y = jumpSpeed;
            this.rb.linearVelocity = currentVel;

            this.isGrounded = false;
        }
    }
    protected virtual void CheckGrounded()
    {
        Transform moveTransform = GetMoveTransform();
        if (moveTransform == null) return;
        Vector3 rayOrigin = moveTransform.position + this.groundCheckOffset;
        this.isGrounded = Physics.Raycast(rayOrigin, Vector3.down, this.groundRayDistance, this.groundLayer);
        if(this.isGrounded && this.velocity.y < 0)
        {
            this.velocity.y = -2f;
        }
    }
    protected abstract Vector3 GetMoveDirection();
    protected virtual Transform GetMoveTransform()
    {
        return this.rb.transform;
    }
}
