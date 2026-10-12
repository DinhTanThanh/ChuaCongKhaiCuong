using UnityEngine;

public class PlayerMove : BaseMove
{
    [SerializeField] protected Camera mainCamera;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadMainCamera();
    }

    protected virtual void LoadMainCamera()
    {
        if (this.mainCamera != null) return;
        this.mainCamera = Camera.main;
    }

    protected override void Update()
    {
        base.Update();
        this.HandleJumpInput();
    }

    protected virtual void HandleJumpInput()
    {
        if (InputManager.Instance != null && InputManager.Instance.JumpPressed)
        {
            this.Jump();
        }
    }

    protected override Vector3 GetMoveDirection()
    {
        if (InputManager.Instance == null) return Vector3.zero;

        Vector2 input = InputManager.Instance.MoveInput;
        if (input.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

        Vector3 inputDir = new Vector3(input.x, 0f, input.y);

        if (this.mainCamera != null)
        {
            Vector3 camForward = this.mainCamera.transform.forward;
            Vector3 camRight = this.mainCamera.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            return (camForward * inputDir.z + camRight * inputDir.x).normalized;
        }

        return inputDir.normalized;
    }
}
