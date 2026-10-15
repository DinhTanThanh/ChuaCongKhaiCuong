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

    [Header("Flight Inputs")]
    protected bool flyTogglePressed;
    protected bool flyUpPressed;
    protected bool flyDownPressed;
    protected bool flyBoostPressed;

    public Vector2 MoveInput => this.moveInput;
    public bool JumpPressed => this.jumpPressed;
    public bool FlyTogglePressed => this.flyTogglePressed;
    public bool FlyUpPressed => this.flyUpPressed;
    public bool FlyDownPressed => this.flyDownPressed;
    public bool FlyBoostPressed => this.flyBoostPressed;

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
        this.ReadFlyInput();
        this.HandleCursorToggle();
    }

    protected virtual void ReadFlyInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            this.flyTogglePressed = false;
            this.flyUpPressed = false;
            this.flyDownPressed = false;
            this.flyBoostPressed = false;
            return;
        }

        this.flyTogglePressed = keyboard.fKey.wasPressedThisFrame;
        this.flyUpPressed = keyboard.spaceKey.isPressed || keyboard.eKey.isPressed;
        this.flyDownPressed = keyboard.cKey.isPressed || keyboard.leftCtrlKey.isPressed;
        this.flyBoostPressed = keyboard.leftShiftKey.isPressed;
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
