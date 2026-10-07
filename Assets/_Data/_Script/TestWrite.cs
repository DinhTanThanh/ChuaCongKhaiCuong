using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class TestWrite : BaseLoadComponent
{
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float turnSpeed = 10f;
    [SerializeField] protected float jumpHeight = 1.5f;
    [SerializeField] protected float gravity = -9.81f;
    [SerializeField] protected bool isGrounded = false;

    [SerializeField] protected Vector3 moveDirection;
    [SerializeField] protected Vector3 velocity;

    [SerializeField] protected LayerMask groundLayer = -0;
    [SerializeField] protected float groundRayDistance = 0.2f;
    [SerializeField] protected Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    [SerializeField] protected BaseController controller;
    [SerializeField] Rigidbody rb;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadController();
        this.LoadRigibody();
    }
    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<BaseController>();
    }
    protected virtual void LoadRigibody()
    {
        if (this.rb != null) return;
        this.rb = GetComponentInParent<Rigidbody>();
    }
    protected virtual void ApplyDisplacement(Vector3 displacement)
    {
        if(this.rb!=null && !this.rb.isKinematic)
        {
            this.rb.MovePosition(this.rb.position + displacement);
        }
    }
    protected virtual void Move(Vector3 direction)
    {
        Vector3 displacement = direction.normalized * (this.speed * Time.deltaTime);
        this.ApplyDisplacement(displacement);
    }
    protected virtual void HandleMovement()
    {
        this.moveDirection = this.GetMoveDirection();
        this.Move(this.moveDirection);
    }
    protected abstract Vector3 GetMoveDirection();
    protected virtual void HandleRotation()
    {
        if (this.moveDirection.sqrMagnitude < 0.001f) return;
        Transform moveTransform=this.GetMoveTransform();
        Quaternion targetRotation = Quaternion.LookRotation(this.moveDirection);
        moveTransform.rotation = Quaternion.Slerp(moveTransform.rotation,targetRotation,this.turnSpeed*Time.deltaTime);
    }
    public virtual void Jump()
    {
        //v^2-v0^2=2as
        if (this.isGrounded == false) return;
        this.velocity.y = Mathf.Sqrt(this.jumpHeight * -2f * this.gravity);
    }
    protected virtual void CheckGrounded()
    {
        Transform moveTransform = this.GetMoveTransform();
        Vector3 rayOrigin = moveTransform.position + this.groundCheckOffset;
        this.isGrounded = Physics.Raycast(rayOrigin, Vector3.down, this.groundRayDistance, groundLayer);
        if (this.isGrounded && this.velocity.y < 0)
        {
            this.velocity.y = -2f;
        }
    }

    protected virtual Transform GetMoveTransform()
    {
        return this.rb.transform;
    }
}
