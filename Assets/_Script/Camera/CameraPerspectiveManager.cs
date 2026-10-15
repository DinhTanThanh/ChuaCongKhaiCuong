using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public enum CameraPerspective
{
    ThirdPerson,
    FirstPerson
}

public enum PerspectiveSwitchMode
{
    [Tooltip("Khuyên dùng: TPP tận dụng Cinemachine (Orbital/Collisions), FPP điều khiển Camera.main trực tiếp theo góc nhìn mắt mượt mà không bị giật lag.")]
    CinemachineHybrid,

    [Tooltip("Cả TPP và FPP đều dùng CinemachineCamera và hoán đổi Priority để Cinemachine tự động hòa trộn (Blend).")]
    CinemachinePriority,

    [Tooltip("Không dùng Cinemachine, script tự điều khiển Camera.main cho cả 2 góc nhìn.")]
    Standalone
}

public class CameraPerspectiveManager : MonoBehaviour
{
    private static CameraPerspectiveManager instance;
    public static CameraPerspectiveManager Instance => instance;

    [Header("Switch Mode & Perspective")]
    [SerializeField] protected PerspectiveSwitchMode switchMode = PerspectiveSwitchMode.CinemachineHybrid;
    [SerializeField] protected CameraPerspective currentPerspective = CameraPerspective.ThirdPerson;

    [Header("Target Management (Decoupled)")]
    [Tooltip("Mục tiêu nhân vật hiện tại mà Camera đang theo dõi.")]
    [SerializeField] protected PlayerCameraTarget currentTarget;
    [Tooltip("Tự động tìm kiếm nhân vật có PlayerCameraTarget hoặc tag 'Player' nếu currentTarget bị trống.")]
    [SerializeField] protected bool autoFindTargetIfNull = true;

    [Header("Cinemachine References")]
    [Tooltip("CinemachineBrain trên Main Camera.")]
    [SerializeField] protected CinemachineBrain cinemachineBrain;
    [Tooltip("Virtual Camera góc nhìn thứ 3 (TPP).")]
    [SerializeField] protected CinemachineCamera tppCinemachineCamera;
    [Tooltip("Virtual Camera góc nhìn thứ nhất (FPP - chỉ dùng khi ở chế độ CinemachinePriority).")]
    [SerializeField] protected CinemachineCamera fppCinemachineCamera;
    [SerializeField] protected int activePriority = 20;
    [SerializeField] protected int inactivePriority = 10;

    [Header("Camera & FOV Settings")]
    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected float fppFOV = 75f;
    [SerializeField] protected float tppFOV = 60f;
    [SerializeField] protected float fovTransitionSpeed = 8f;

    [Header("First-Person Mouse Look Settings")]
    [SerializeField] protected float mouseSensitivity = 2.0f;
    [SerializeField] protected float minPitch = -75f;
    [SerializeField] protected float maxPitch = 80f;

    [Header("Standalone TPP Settings (Nếu không dùng Cinemachine)")]
    [SerializeField] protected Vector3 standaloneTppOffset = new Vector3(0f, 1.8f, -4.5f);
    [SerializeField] protected float standaloneFollowSpeed = 12f;

    protected float currentPitch = 15f;
    protected float currentYaw = 0f;
    protected float lastSwitchTime = -1f;
    protected float switchCooldown = 0.25f;
    protected bool isApplicationQuitting = false;

    public event Action<CameraPerspective> OnPerspectiveChanged;
    public event Action<PlayerCameraTarget> OnTargetChanged;

    public CameraPerspective CurrentPerspective => this.currentPerspective;
    public PlayerCameraTarget CurrentTarget => this.currentTarget;
    public Camera MainCamera => this.mainCamera;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitializeOnLoad()
    {
        if (FindFirstObjectByType<CameraPerspectiveManager>() == null)
        {
            GameObject camSystem = new GameObject("[CameraSystem]");
            camSystem.AddComponent<CameraPerspectiveManager>();
            Debug.Log("<color=cyan>[CameraPerspectiveManager] Hệ thống Camera FPP/TPP đã tự động khởi tạo!</color>");
        }
    }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        this.LoadComponents();
    }

    protected virtual void LoadComponents()
    {
        if (this.mainCamera == null)
        {
            this.mainCamera = Camera.main;
            if (this.mainCamera == null)
            {
                this.mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        if (this.cinemachineBrain == null && this.mainCamera != null)
        {
            this.cinemachineBrain = this.mainCamera.GetComponent<CinemachineBrain>();
        }

        if (this.tppCinemachineCamera == null)
        {
            this.tppCinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (this.currentTarget == null && this.autoFindTargetIfNull)
        {
            this.AutoFindTarget();
        }
    }

    protected virtual void Start()
    {
        this.LoadComponents();
        this.ApplyInitialPerspective();
    }

    protected virtual void OnApplicationQuit()
    {
        this.isApplicationQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    protected virtual void Update()
    {
        this.HandleInput();
    }

    protected virtual void LateUpdate()
    {
        if (this.isApplicationQuitting) return;

        if (this.currentTarget == null)
        {
            if (this.autoFindTargetIfNull)
            {
                this.AutoFindTarget();
            }
            if (this.currentTarget == null) return;
        }

        if (this.mainCamera == null)
        {
            this.LoadComponents();
            if (this.mainCamera == null) return;
        }

        this.HandleCameraUpdate();
    }

    protected virtual void HandleInput()
    {
        if (this.IsViewSwitchPressed())
        {
            this.TogglePerspective();
        }
    }

    protected virtual bool IsViewSwitchPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.vKey.wasPressedThisFrame || keyboard[Key.V].wasPressedThisFrame)
            {
                return true;
            }
        }

        if (InputManager.Instance != null && InputManager.Instance.ViewSwitchPressed)
        {
            return true;
        }

        return false;
    }

    public virtual void TogglePerspective()
    {
        if (Time.unscaledTime - this.lastSwitchTime < this.switchCooldown) return;
        this.lastSwitchTime = Time.unscaledTime;

        CameraPerspective newPerspective = (this.currentPerspective == CameraPerspective.ThirdPerson)
            ? CameraPerspective.FirstPerson
            : CameraPerspective.ThirdPerson;

        this.SetPerspective(newPerspective);
    }

    public virtual void SetPerspective(CameraPerspective newPerspective)
    {
        this.currentPerspective = newPerspective;

        if (this.currentPerspective == CameraPerspective.FirstPerson)
        {
            this.EnterFirstPerson();
        }
        else
        {
            this.EnterThirdPerson();
        }

        this.OnPerspectiveChanged?.Invoke(this.currentPerspective);
    }

    protected virtual void EnterFirstPerson()
    {
        this.LoadComponents();

        if (this.currentTarget != null)
        {
            this.currentTarget.SetMeshVisibility(false);
        }

        if (this.mainCamera != null)
        {
            this.currentYaw = this.mainCamera.transform.eulerAngles.y;
            float rawPitch = this.mainCamera.transform.eulerAngles.x;
            if (rawPitch > 180f) rawPitch -= 360f;
            this.currentPitch = Mathf.Clamp(rawPitch, this.minPitch, this.maxPitch);

            if (this.currentTarget != null)
            {
                this.mainCamera.transform.position = this.currentTarget.GetEyeTransform().position;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        switch (this.switchMode)
        {
            case PerspectiveSwitchMode.CinemachineHybrid:
                if (this.tppCinemachineCamera != null)
                {
                    this.tppCinemachineCamera.gameObject.SetActive(false);
                }
                if (this.cinemachineBrain != null)
                {
                    this.cinemachineBrain.enabled = false;
                }
                break;

            case PerspectiveSwitchMode.CinemachinePriority:
                if (this.fppCinemachineCamera != null)
                {
                    this.fppCinemachineCamera.Priority.Value = this.activePriority;
                }
                if (this.tppCinemachineCamera != null)
                {
                    this.tppCinemachineCamera.Priority.Value = this.inactivePriority;
                }
                break;

            case PerspectiveSwitchMode.Standalone:
                break;
        }
    }

    protected virtual void EnterThirdPerson()
    {
        this.LoadComponents();

        if (this.currentTarget != null)
        {
            this.currentTarget.SetMeshVisibility(true);
        }

        switch (this.switchMode)
        {
            case PerspectiveSwitchMode.CinemachineHybrid:
                if (this.tppCinemachineCamera != null)
                {
                    this.tppCinemachineCamera.gameObject.SetActive(true);
                }
                if (this.cinemachineBrain != null)
                {
                    this.cinemachineBrain.enabled = true;
                }
                break;

            case PerspectiveSwitchMode.CinemachinePriority:
                if (this.tppCinemachineCamera != null)
                {
                    this.tppCinemachineCamera.Priority.Value = this.activePriority;
                }
                if (this.fppCinemachineCamera != null)
                {
                    this.fppCinemachineCamera.Priority.Value = this.inactivePriority;
                }
                break;

            case PerspectiveSwitchMode.Standalone:
                break;
        }
    }

    protected virtual void ApplyInitialPerspective()
    {
        if (this.currentPerspective == CameraPerspective.FirstPerson)
        {
            this.EnterFirstPerson();
        }
        else
        {
            this.EnterThirdPerson();
        }
    }

    protected virtual void HandleCameraUpdate()
    {
        if (this.currentPerspective == CameraPerspective.FirstPerson)
        {
            this.UpdateFirstPersonView();
        }
        else
        {
            this.UpdateThirdPersonView();
        }
    }

    protected virtual void UpdateFirstPersonView()
    {
        Vector2 mouseDelta = Vector2.zero;
        if (Mouse.current != null)
        {
            mouseDelta = Mouse.current.delta.ReadValue() * (this.mouseSensitivity * 0.1f);
        }

        this.currentYaw += mouseDelta.x;
        this.currentPitch -= mouseDelta.y;
        this.currentPitch = Mathf.Clamp(this.currentPitch, this.minPitch, this.maxPitch);

        if (this.currentTarget != null)
        {
            this.currentTarget.RotateBodyYaw(this.currentYaw);
        }

        if (this.switchMode == PerspectiveSwitchMode.CinemachineHybrid || this.switchMode == PerspectiveSwitchMode.Standalone)
        {
            if (this.mainCamera != null && this.currentTarget != null)
            {
                Transform eye = this.currentTarget.GetEyeTransform();
                this.mainCamera.transform.position = eye.position;
                this.mainCamera.transform.rotation = Quaternion.Euler(this.currentPitch, this.currentYaw, 0f);

                this.mainCamera.fieldOfView = Mathf.Lerp(this.mainCamera.fieldOfView, this.fppFOV, Time.deltaTime * this.fovTransitionSpeed);
            }
        }
    }

    protected virtual void UpdateThirdPersonView()
    {
        if (this.mainCamera != null)
        {
            this.mainCamera.fieldOfView = Mathf.Lerp(this.mainCamera.fieldOfView, this.tppFOV, Time.deltaTime * this.fovTransitionSpeed);
        }

        if (this.switchMode == PerspectiveSwitchMode.Standalone && this.mainCamera != null && this.currentTarget != null)
        {
            Vector2 mouseDelta = Vector2.zero;
            if (Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue() * (this.mouseSensitivity * 0.1f);
            }

            this.currentYaw += mouseDelta.x;
            this.currentPitch -= mouseDelta.y;
            this.currentPitch = Mathf.Clamp(this.currentPitch, this.minPitch, this.maxPitch);

            Quaternion targetRotation = Quaternion.Euler(this.currentPitch, this.currentYaw, 0f);
            Vector3 targetPosition = this.currentTarget.TppTarget.position + targetRotation * this.standaloneTppOffset;

            this.mainCamera.transform.position = Vector3.Lerp(this.mainCamera.transform.position, targetPosition, Time.deltaTime * this.standaloneFollowSpeed);
            this.mainCamera.transform.rotation = targetRotation;
        }
    }

    public virtual void SetTarget(PlayerCameraTarget newTarget)
    {
        if (this.currentTarget == newTarget) return;

        if (this.currentTarget != null)
        {
            this.currentTarget.SetMeshVisibility(true);
        }

        this.currentTarget = newTarget;

        if (this.currentTarget != null)
        {
            this.UpdateCinemachineTargets();

            if (this.currentPerspective == CameraPerspective.FirstPerson)
            {
                this.currentYaw = this.currentTarget.transform.eulerAngles.y;
                this.currentTarget.SetMeshVisibility(false);
            }

            Debug.Log($"[CameraPerspectiveManager] Đã gắn Camera sang nhân vật: '{this.currentTarget.gameObject.name}'");
        }

        this.OnTargetChanged?.Invoke(this.currentTarget);
    }

    public virtual void SetTarget(Transform targetTransform, Transform eyePoint = null)
    {
        if (targetTransform == null) return;

        PlayerCameraTarget targetComponent = targetTransform.GetComponent<PlayerCameraTarget>();
        if (targetComponent == null)
        {
            targetComponent = targetTransform.gameObject.AddComponent<PlayerCameraTarget>();
        }

        this.SetTarget(targetComponent);
    }

    public virtual void ClearTarget(PlayerCameraTarget target)
    {
        if (this.currentTarget == target)
        {
            this.currentTarget = null;
        }
    }

    protected virtual void UpdateCinemachineTargets()
    {
        if (this.currentTarget == null) return;

        if (this.tppCinemachineCamera != null)
        {
            this.tppCinemachineCamera.Target.TrackingTarget = this.currentTarget.TppTarget;
        }

        if (this.fppCinemachineCamera != null)
        {
            this.fppCinemachineCamera.Target.TrackingTarget = this.currentTarget.EyePoint;
        }
    }

    public virtual void AutoFindTarget()
    {
        if (this.isApplicationQuitting || !Application.isPlaying) return;

        PlayerCameraTarget found = FindFirstObjectByType<PlayerCameraTarget>();
        if (found != null && found.gameObject.activeInHierarchy)
        {
            this.SetTarget(found);
            return;
        }

        BaseController controller = FindFirstObjectByType<BaseController>();
        if (controller != null && controller.gameObject.activeInHierarchy)
        {
            this.SetTarget(controller.transform);
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            this.SetTarget(playerObj.transform);
            return;
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    protected virtual void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = this.currentPerspective == CameraPerspective.FirstPerson ? Color.green : Color.white;
        string text = $"[Góc nhìn: {this.currentPerspective}] - Bấm 'V' để đổi";
        GUI.Label(new Rect(15, 15, 270, 32), text, style);
    }
#endif
}
