using UnityEngine;
using UnityEngine.InputSystem;

public class WukongMove : BaseMove
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

    protected override Vector3 GetMoveDirection()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector3.zero;

        float horizontal = 0f;
        float vertical = 0f;

        // Phím A / D hoặc mũi tên Trái / Phải
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;

        // Phím W / S hoặc mũi tên Lên / Xuống
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;

        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDir.sqrMagnitude < 0.001f)
        {
            return Vector3.zero;
        }

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

        return inputDir;
    }

    protected virtual void HandleJumpInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("bam phim space");
            this.Jump();
        }
    }
}
