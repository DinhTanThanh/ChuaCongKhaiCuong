using UnityEngine;
using UnityEngine.InputSystem;

public class WukongFly : BaseFly
{
    [Header("Wukong References")]
    [SerializeField] protected WukongController controller;
    [SerializeField] protected Camera mainCamera;
    [SerializeField] protected Animator animator;

    [Header("Somersault Cloud (Cân Đẩu Vân)")]
    [SerializeField] protected GameObject cloudVfx;
    [SerializeField] protected Vector3 cloudOffset = new Vector3(0f, -0.955f, -0.6f);
    [SerializeField] protected Vector3 cloudRotation = new Vector3(0f, -90f, 0f);
    [SerializeField] protected Vector3 cloudScale = new Vector3(0.75f, 0.75f, 0.75f);
    [SerializeField] protected bool enableHoverBobbing = true;
    [SerializeField] protected float bobbingSpeed = 2.5f;
    [SerializeField] protected float bobbingAmount = 0.04f;

    [Header("Take-off & Altitude (Độ cao cất cánh & bay)")]
    [SerializeField] protected float takeOffAltitude = 2.2f;
    [SerializeField] protected float minFlightAltitude = 1.6f;

    protected int isFlyingAnimParamHash;
    protected bool hasFlyingAnimParam = false;
    protected Vector3 initialCloudLocalPos;
    protected Animator cloudAnimator;

    public virtual void ApplyCloudTransform()
    {
        if (this.cloudVfx == null) return;
        this.cloudVfx.transform.localPosition = this.cloudOffset;
        this.cloudVfx.transform.localRotation = Quaternion.Euler(this.cloudRotation);
        this.cloudVfx.transform.localScale = this.cloudScale;
        this.initialCloudLocalPos = this.cloudOffset;
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.minGroundHeight = this.minFlightAltitude;
        this.LoadController();
        this.LoadMainCamera();
        this.LoadAnimator();
        this.InitCloud();
    }

    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<WukongController>();
        if (this.controller == null) this.controller = GetComponent<WukongController>();
    }

    protected virtual void LoadMainCamera()
    {
        if (this.mainCamera != null) return;
        this.mainCamera = Camera.main;
    }

    protected virtual void LoadAnimator()
    {
        if (this.animator == null)
        {
            if (this.transform.parent != null)
            {
                this.animator = this.transform.parent.GetComponentInChildren<Animator>();
            }
            if (this.animator == null)
            {
                this.animator = GetComponentInChildren<Animator>();
            }
        }

        if (this.animator != null)
        {
            this.isFlyingAnimParamHash = Animator.StringToHash("IsFlying");
            this.hasFlyingAnimParam = false;
            foreach (var param in this.animator.parameters)
            {
                if (param.nameHash == this.isFlyingAnimParamHash)
                {
                    this.hasFlyingAnimParam = true;
                    break;
                }
            }
        }
    }

    protected virtual void InitCloud()
    {
        if (this.cloudVfx == null || (this.cloudVfx != null && this.cloudVfx.name.Contains("SomersaultCloud")))
        {
            this.CreateDefaultCloud();
        }

        if (this.cloudVfx != null)
        {
            this.ApplyCloudTransform();
            this.cloudAnimator = this.cloudVfx.GetComponentInChildren<Animator>();
            this.cloudVfx.SetActive(this.isFlying);
        }
    }

    protected virtual void CreateDefaultCloud()
    {
        Transform canDauVanTrans = this.transform.Find("CanDauVan");
        if (canDauVanTrans != null)
        {
            this.cloudVfx = canDauVanTrans.gameObject;
            this.ApplyCloudTransform();
            this.HideOldCloud();
            return;
        }
        if (this.transform.parent != null)
        {
            Transform parentCloud = this.transform.parent.Find("CanDauVan");
            if (parentCloud != null)
            {
                this.cloudVfx = parentCloud.gameObject;
                this.ApplyCloudTransform();
                this.HideOldCloud();
                return;
            }
        }

        GameObject prefab = Resources.Load<GameObject>("CanDauVan");
        if (prefab != null)
        {
            this.HideOldCloud();
            GameObject newCloud = Instantiate(prefab, this.transform);
            newCloud.name = "CanDauVan";
            this.cloudVfx = newCloud;
            this.ApplyCloudTransform();
            return;
        }

        Transform oldCloud = this.transform.Find("SomersaultCloud_VFX");
        if (oldCloud != null)
        {
            this.cloudVfx = oldCloud.gameObject;
        }
    }

    protected virtual void HideOldCloud()
    {
        Transform oldCloud = this.transform.Find("SomersaultCloud_VFX");
        if (oldCloud != null)
        {
            oldCloud.gameObject.SetActive(false);
        }
        if (this.transform.parent != null)
        {
            Transform parentOld = this.transform.parent.Find("SomersaultCloud_VFX");
            if (parentOld != null)
            {
                parentOld.gameObject.SetActive(false);
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        this.HandleFlyInputToggle();
    }

    protected virtual void HandleFlyInputToggle()
    {
        bool toggleRequested = false;

        if (InputManager.Instance != null)
        {
            toggleRequested = InputManager.Instance.FlyTogglePressed;
        }

        if (!toggleRequested)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                toggleRequested = true;
            }
        }

        if (toggleRequested)
        {
            this.ToggleFly();
        }
    }

    protected override void OnFlyingUpdate()
    {
        base.OnFlyingUpdate();
        if (this.enableHoverBobbing && this.cloudVfx != null && this.cloudVfx.activeSelf)
        {
            float bob = Mathf.Sin(Time.time * this.bobbingSpeed) * this.bobbingAmount;
            this.cloudVfx.transform.localPosition = this.initialCloudLocalPos + new Vector3(0f, bob, 0f);
        }
    }

    protected override Vector3 GetHorizontalFlyDirection()
    {
        Vector2 input = Vector2.zero;

        if (InputManager.Instance != null)
        {
            input = InputManager.Instance.MoveInput;
        }
        if (input.sqrMagnitude < 0.001f) return Vector3.zero;

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

    protected override float GetVerticalFlyInput()
    {
        if (InputManager.Instance != null)
        {
            float vertical = 0f;
            if (InputManager.Instance.FlyUpPressed) vertical += 1f;
            if (InputManager.Instance.FlyDownPressed) vertical -= 1f;
            return vertical;
        }
        return 0f;
    }

    protected override bool IsBoostFlying()
    {
        if (InputManager.Instance != null)
        {
            return InputManager.Instance.FlyBoostPressed;   
        }
        return false;
    }

    protected override void OnStartFlying()
    {
        base.OnStartFlying();

        this.LiftOffGround();

        if (this.cloudVfx != null)
        {
            this.ApplyCloudTransform();
            this.cloudVfx.SetActive(true);

            if (this.cloudAnimator == null)
            {
                this.cloudAnimator = this.cloudVfx.GetComponentInChildren<Animator>();
            }

            if (this.cloudAnimator != null)
            {
                this.cloudAnimator.enabled = true;
                this.cloudAnimator.Play("SwayAndBob", 0, 0f);
            }
        }

        if (this.animator != null && this.hasFlyingAnimParam)
        {
            this.animator.SetBool(this.isFlyingAnimParamHash, true);
        }
    }

    protected virtual void LiftOffGround()
    {
        Transform moveTransform = this.GetMoveTransform();
        if (moveTransform == null) return;

        Vector3 currentPos = moveTransform.position;
        Vector3 rayOrigin = currentPos + Vector3.up * 1.5f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 50f, this.groundLayer, QueryTriggerInteraction.Ignore))
        {
            float groundY = hit.point.y;
            float currentAltitude = currentPos.y - groundY;

            if (currentAltitude < this.takeOffAltitude)
            {
                Vector3 targetPos = new Vector3(currentPos.x, groundY + this.takeOffAltitude, currentPos.z);
                if (this.rb != null && !this.rb.isKinematic)
                {
                    this.rb.position = targetPos;
                    this.rb.linearVelocity = Vector3.zero;
                }
                moveTransform.position = targetPos;
            }
        }
        else
        {
            Vector3 targetPos = currentPos + Vector3.up * this.takeOffAltitude;
            if (this.rb != null && !this.rb.isKinematic)
            {
                this.rb.position = targetPos;
                this.rb.linearVelocity = Vector3.zero;
            }
            moveTransform.position = targetPos;
        }

        this.minGroundHeight = this.minFlightAltitude;
    }

    protected override void OnStopFlying()
    {
        base.OnStopFlying();

        if (this.cloudVfx != null)
        {
            this.cloudVfx.SetActive(false);
        }

        if (this.animator != null && this.hasFlyingAnimParam)
        {
            this.animator.SetBool(this.isFlyingAnimParamHash, false);
        }
    }
}
