using UnityEngine;

public class CannonGunController : BaseLoadComponent
{
    [Header("Gun Components")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected CannonGunShooting shooting;
    [SerializeField] protected CannonGunAiming aiming;
    public virtual Transform FirePoint => this.firePoint;
    public virtual CannonGunShooting Shooting => this.shooting;
    public virtual CannonGunAiming Aiming => this.aiming;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadFirePoint();
        this.LoadShooting();
        this.LoadAiming();
    }

    protected virtual void LoadFirePoint()
    {
        if (this.firePoint != null) return;
        Transform fp = transform.Find("FirePoint");
        if (fp != null) this.firePoint = fp;
    }

    protected virtual void LoadShooting()
    {
        if (this.shooting != null) return;
        this.shooting = GetComponentInChildren<CannonGunShooting>();
    }

    protected virtual void LoadAiming()
    {
        if (this.aiming != null) return;
        this.aiming = GetComponentInChildren<CannonGunAiming>();
    }
}
