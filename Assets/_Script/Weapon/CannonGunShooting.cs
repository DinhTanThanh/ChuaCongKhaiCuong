using UnityEngine;
using UnityEngine.InputSystem;

public class CannonGunShooting : BaseLoadComponent
{
    [SerializeField] protected CannonGunController controller;
    [SerializeField] protected GameObject bulletPrefab;
    [Header("Shooting Stats")]
    [SerializeField] protected float launchForce = 45f;
    [SerializeField] protected float fireRate = 0.8f;
    [SerializeField] protected bool canShoot = true;
    protected float lastFireTime;

    public virtual CannonGunController Controller => this.controller;
    public virtual GameObject BulletPrefab => this.bulletPrefab;
    public virtual float LaunchForce => this.launchForce;
    public virtual float FireRate => this.fireRate;
    public virtual bool CanShoot
    {
        get => this.canShoot;
        set => this.canShoot = value;
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadController();
        this.LoadBulletPrefab();
    }

    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<CannonGunController>();
    }
    protected virtual void LoadBulletPrefab()
    {
        if (this.bulletPrefab != null) return;
        this.bulletPrefab = GameObject.Find("Bullet");
    }
    protected virtual void Update()
    {
        this.HandleShootInput();
    }
    protected virtual void HandleShootInput()
    {
        if (!this.canShoot) return;
        if (InputManager.Instance == null) return;

        if (InputManager.Instance.LeftClickReleased)
        {
            if (Time.time >= this.lastFireTime + this.fireRate)
            {
                //ở đây truyền vào spawn bullet và thêm lực launchForce cho viên đạn bay đi
                this.lastFireTime = Time.time;
            }
        }
    }
}

