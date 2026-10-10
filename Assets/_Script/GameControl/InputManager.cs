using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private static InputManager instance;
    public static InputManager Instance => instance;

    [Header("Cursor Settings")]
    [SerializeField] protected bool lockCursor = true;

    protected Vector2 moveInput;
    protected bool jumpPressed;

    public Vector2 MoveInput => this.moveInput;
    public bool JumpPressed => this.jumpPressed;

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    protected virtual void Start()
    {
        if (this.lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked; //khóa chết con chuột ở giữa màn hình
            Cursor.visible = false; //ẩn con chuột đi nếu muốn bật lại thì true
        }
    }

    protected virtual void Update()
    {
        this.ReadMovementInput();
        this.ReadJumpInput();
        this.HandleCursorToggle();
    }

    protected virtual void ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            this.moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;

        this.moveInput = new Vector2(horizontal, vertical).normalized;
    }

    protected virtual void ReadJumpInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            this.jumpPressed = false;
            return;
        }
        this.jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
    }

    protected virtual void HandleCursorToggle()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.leftAltKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
