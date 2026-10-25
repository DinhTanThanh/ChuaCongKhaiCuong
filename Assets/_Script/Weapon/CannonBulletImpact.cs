using UnityEngine;

public class CannonBulletImpact : BaseLoadComponent
{
    [Header("Impact References")]
    [SerializeField] protected CannonBulletController controller;
    [SerializeField] protected CannonBulletDameSender dameSender;

    [Header("Explosion VFX & Lifetime")]
    [SerializeField] protected GameObject explosionVfxPrefab;
    [SerializeField] protected float lifeTime = 7f;
    [SerializeField] protected GameObject shooter;

    protected bool hasExploded = false;

    public virtual CannonBulletController Controller => this.controller;
    public virtual CannonBulletDameSender DameSender => this.dameSender;
    public virtual GameObject ExplosionVfxPrefab => this.explosionVfxPrefab;
    public virtual float LifeTime => this.lifeTime;
    public virtual bool HasExploded => this.hasExploded;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadController();
        this.LoadDameSender();
        this.LoadExplosionVfx();
    }

    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<CannonBulletController>();
    }

    protected virtual void LoadDameSender()
    {
        if (this.dameSender != null) return;
        if (this.controller != null && this.controller.DameSender != null)
        {
            this.dameSender = this.controller.DameSender;
            return;
        }
        this.dameSender = GetComponentInParent<CannonBulletDameSender>();
    }

    protected virtual void LoadExplosionVfx()
    {
        if (this.explosionVfxPrefab != null) return;
        if (this.controller != null && this.controller.BulletSO != null)
        {
            this.explosionVfxPrefab = this.controller.BulletSO.explosionVfxPrefab;
        }
    }

    protected virtual void Start()
    {
        // Tự động tiêu hủy cả viên đạn sau khoảng thời gian lifeTime nếu không va chạm
        Destroy(this.GetRootBullet(), this.lifeTime);
    }

    public virtual void SetShooter(GameObject newShooter)
    {
        this.shooter = newShooter;
    }

    public virtual void SetLifeTime(float newLifeTime)
    {
        if (newLifeTime <= 0f) return;
        this.lifeTime = newLifeTime;
    }

    public virtual void SetExplosionVfxPrefab(GameObject newVfxPrefab)
    {
        this.explosionVfxPrefab = newVfxPrefab;
    }

    public virtual void OnImpactTrigger(Collider other)
    {
        if (other == null) return;
        Vector3 hitPoint = other.ClosestPoint(this.transform.position);
        this.HandleImpact(other, hitPoint);
    }

    public virtual void HandleImpact(Collider hitCollider, Vector3 hitPoint)
    {
        if (this.hasExploded) return;
        if (hitCollider == null) return;
        if (this.shooter != null && hitCollider.transform.root == this.shooter.transform.root) return;
        if (hitCollider.transform.root == this.transform.root) return;

        this.Explode(hitPoint);
    }

    public virtual void Explode(Vector3 hitPoint)
    {
        if (this.hasExploded) return;
        this.hasExploded = true;

        this.SpawnExplosionVfx(hitPoint);

        CannonBulletDameSender sender = this.GetDameSender();
        if (sender != null)
        {
            sender.ExplodeDamage(hitPoint);
        }

        // Tiêu hủy toàn bộ đối tượng đạn
        Destroy(this.GetRootBullet());
    }

    protected virtual void SpawnExplosionVfx(Vector3 hitPoint)
    {
        if (this.explosionVfxPrefab == null) return;

        GameObject vfxInstance = Instantiate(this.explosionVfxPrefab, hitPoint, Quaternion.identity);
        Destroy(vfxInstance, 3.5f);
    }

    protected virtual CannonBulletDameSender GetDameSender()
    {
        if (this.dameSender != null) return this.dameSender;
        if (this.controller != null && this.controller.DameSender != null)
        {
            this.dameSender = this.controller.DameSender;
            return this.dameSender;
        }
        this.dameSender = GetComponentInParent<CannonBulletDameSender>();
        return this.dameSender;
    }

    protected virtual GameObject GetRootBullet()
    {
        if (this.controller != null) return this.controller.gameObject;
        return this.transform.root.gameObject;
    }
}
