using UnityEngine;
using UnityEngine.InputSystem;

public class CannonGunController : BaseLoadComponent
{
    [Header("Gun References")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected Transform visorTransform;
    [SerializeField] protected BulletSO bulletSO;
    [SerializeField] protected CannonBulletController bulletPrefab;

    [Header("Shooting Stats")]
    [Tooltip("Lực bắn mặc định (sẽ ưu tiên lấy từ bulletSO nếu có)")]
    [SerializeField] protected float launchForce = 45f;
    [SerializeField] protected float fireRate = 0.8f;
    [SerializeField] protected bool canShoot = true;

    [Header("Trajectory (Quỹ đạo đạn)")]
    [Tooltip("Vẽ đường cong dự đoán quỹ đạo đạn pháo (mặc định tắt)")]
    [SerializeField] protected bool showTrajectoryLine = false;
    [SerializeField] protected LineRenderer trajectoryLine;
    [SerializeField] protected int trajectorySteps = 30;
    [SerializeField] protected float trajectoryTimeStep = 0.08f;

    [Header("Audio & VFX")]
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip shootSound;

    protected float lastFireTime;

    // Giữ thuộc tính IsAiming để tương thích với các script khác nếu có gọi tới
    public virtual bool IsAiming => false;
    public virtual Transform FirePoint => this.firePoint;
    public virtual Transform VisorTransform => this.visorTransform;
    public virtual BulletSO BulletSO => this.bulletSO;
    public virtual float LaunchForce => this.bulletSO != null ? this.bulletSO.launchForce : this.launchForce;
    public virtual Vector3 BulletGravity => Physics.gravity * (this.bulletSO != null ? this.bulletSO.gravityScale : 1f);
    public virtual CannonBulletController BulletPrefab => (this.bulletSO != null && this.bulletSO.bulletPrefab != null) ? this.bulletSO.bulletPrefab : this.bulletPrefab;

    public virtual void SetBulletSO(BulletSO newBulletSO)
    {
        this.bulletSO = newBulletSO;
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadFirePoint();
        this.LoadVisor();
        this.LoadTrajectoryLine();
    }

    protected virtual void LoadFirePoint()
    {
        if (this.firePoint != null) return;
        Transform fp = transform.Find("FirePoint");
        if (fp != null) this.firePoint = fp;
    }

    protected virtual void LoadVisor()
    {
        if (this.visorTransform != null) return;
        Transform v = transform.Find("Visor");
        if (v != null) this.visorTransform = v;
    }

    protected virtual void LoadTrajectoryLine()
    {
        if (this.trajectoryLine != null) return;
        this.trajectoryLine = GetComponent<LineRenderer>();
    }

    protected virtual void Start()
    {
        if (this.trajectoryLine != null)
        {
            this.trajectoryLine.enabled = false;
        }
    }

    protected virtual void Update()
    {
        this.HandleShootInput();
        this.UpdateTrajectory();
    }

    /// <summary>
    /// Xử lý phím bắn (Chuột trái) - Bắn tự do
    /// </summary>
    protected virtual void HandleShootInput()
    {
        if (!this.canShoot) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            // Thực hiện bắn nếu hết thời gian nạp
            if (Time.time >= this.lastFireTime + this.fireRate)
            {
                this.Shoot();
                this.lastFireTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Lấy hướng bắn thẳng theo góc xoay của nòng súng / súng
    /// </summary>
    public virtual Vector3 GetShootDirection()
    {
        return this.firePoint != null ? this.firePoint.forward : this.transform.forward;
    }

    /// <summary>
    /// Bắn đạn pháo thẳng theo đúng góc xoay và hướng của súng
    /// </summary>
    public virtual void Shoot()
    {
        CannonBulletController prefab = this.BulletPrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[CannonGunController] Chưa gán bulletPrefab vào súng hoặc BulletSO!");
            return;
        }

        Vector3 spawnPos = this.firePoint != null ? this.firePoint.position : this.transform.position;
        Vector3 shootDir = this.GetShootDirection();
        Quaternion spawnRot = this.firePoint != null ? this.firePoint.rotation : this.transform.rotation;

        CannonBulletController bullet = Instantiate(prefab, spawnPos, spawnRot);

        bullet.Launch(shootDir, this.LaunchForce, this.transform.root.gameObject);

        // Hiệu ứng lửa đầu nòng
        if (this.muzzleFlash != null)
        {
            this.muzzleFlash.Play();
        }

        // Âm thanh bắn pháo
        if (this.audioSource != null && this.shootSound != null)
        {
            this.audioSource.PlayOneShot(this.shootSound);
        }
    }

    /// <summary>
    /// Vẽ đường cong parabol dự đoán quỹ đạo bay của đạn pháo (nếu được bật trong Inspector)
    /// </summary>
    protected virtual void UpdateTrajectory()
    {
        if (this.trajectoryLine == null || !this.showTrajectoryLine)
        {
            if (this.trajectoryLine != null && this.trajectoryLine.enabled)
            {
                this.trajectoryLine.enabled = false;
            }
            return;
        }

        if (!this.trajectoryLine.enabled) this.trajectoryLine.enabled = true;

        Vector3 startPos = this.firePoint != null ? this.firePoint.position : this.transform.position;
        Vector3 shootDir = this.GetShootDirection();
        Vector3 startVel = shootDir * this.LaunchForce;
        Vector3 gravity = this.BulletGravity;

        this.trajectoryLine.positionCount = this.trajectorySteps;
        Vector3 currentPos = startPos;
        Vector3 currentVel = startVel;

        for (int i = 0; i < this.trajectorySteps; i++)
        {
            this.trajectoryLine.SetPosition(i, currentPos);

            // Dự đoán rơi theo trọng lực của viên đạn đang nạp
            currentVel += gravity * this.trajectoryTimeStep;
            currentPos += currentVel * this.trajectoryTimeStep;

            // Dừng vẽ nếu va chạm vào vật cản (bỏ qua bản thân người bắn)
            if (i > 0 && Physics.Linecast(this.trajectoryLine.GetPosition(i - 1), currentPos, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root != this.transform.root)
                {
                    this.trajectoryLine.positionCount = i + 1;
                    this.trajectoryLine.SetPosition(i, hit.point);
                    break;
                }
            }
        }
    }
}
