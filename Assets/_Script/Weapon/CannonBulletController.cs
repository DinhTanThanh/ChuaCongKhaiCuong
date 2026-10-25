using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CannonBulletController : BaseLoadComponent
{
    [Header("Bullet Components")]
    [SerializeField] protected BulletSO bulletSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider bulletCollider;
    [SerializeField] protected CannonBulletMove bulletMove;
    [SerializeField] protected CannonBulletDameSender dameSender;
    [SerializeField] protected CannonBulletImpact bulletImpact;

    public virtual BulletSO BulletSO => this.bulletSO;
    public virtual Rigidbody Rigidbody => this.rb;
    public virtual Collider BulletCollider => this.bulletCollider;
    public virtual CannonBulletMove BulletMove => this.bulletMove;
    public virtual CannonBulletDameSender DameSender => this.dameSender;
    public virtual CannonBulletImpact BulletImpact => this.bulletImpact;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadRigidbody();
        this.LoadCollider();
        this.LoadBulletMove();
        this.LoadDameSender();
        this.LoadBulletImpact();
        this.LoadBulletSO();
    }

    protected virtual void LoadRigidbody()
    {
        if (this.rb != null) return;
        this.rb = GetComponent<Rigidbody>();
    }

    protected virtual void LoadCollider()
    {
        if (this.bulletCollider != null) return;
        this.bulletCollider = GetComponent<Collider>();
    }

    protected virtual void LoadBulletMove()
    {
        if (this.bulletMove != null) return;
        this.bulletMove = GetComponentInChildren<CannonBulletMove>();
    }

    protected virtual void LoadDameSender()
    {
        if (this.dameSender != null) return;
        this.dameSender = GetComponentInChildren<CannonBulletDameSender>();
    }

    protected virtual void LoadBulletImpact()
    {
        if (this.bulletImpact != null) return;
        this.bulletImpact = GetComponentInChildren<CannonBulletImpact>();
    }

    protected virtual void Start()
    {
        if (this.bulletSO != null)
        {
            this.ApplyBulletSO();
        }
    }

    public virtual void LoadBulletSO()
    {
        if (this.bulletSO != null) return;
        this.bulletSO = Resources.Load<BulletSO>(transform.name+"SO");
        this.ApplyBulletSO();
    }

    public virtual void ApplyBulletSO()
    {
        if (this.bulletSO == null) return;

        if (this.dameSender != null)
        {
            this.dameSender.SetDamage(this.bulletSO.damage);
            this.dameSender.SetCritRate(this.bulletSO.critRate);
            this.dameSender.SetCritDamageMultiplier(this.bulletSO.critDamageMultiplier);
            this.dameSender.SetExplosionRadius(this.bulletSO.explosionRadius);
        }

        if (this.bulletImpact != null)
        {
            this.bulletImpact.SetLifeTime(this.bulletSO.lifeTime);
            this.bulletImpact.SetExplosionVfxPrefab(this.bulletSO.explosionVfxPrefab);
        }

        if (this.rb != null)
        {
            this.rb.mass = this.bulletSO.mass;
        }
    }

    public virtual void Launch(Vector3 direction, float force, GameObject shooter = null)
    {
        if (shooter != null)
        {
            if (this.dameSender != null)
            {
                this.dameSender.SetAttacker(shooter);
            }

            if (this.bulletImpact != null)
            {
                this.bulletImpact.SetShooter(shooter);
            }
 
            if (this.bulletCollider == null) this.LoadCollider();
            if (this.bulletCollider != null)
            {
                Collider[] shooterColliders = shooter.GetComponentsInChildren<Collider>();
                foreach (Collider sc in shooterColliders)
                {
                    Physics.IgnoreCollision(this.bulletCollider, sc, true);
                }
            }
        }

        if (this.bulletMove == null) this.LoadBulletMove();
        if (this.bulletMove != null)
        {
            this.bulletMove.Launch(direction, force);
        }
        else
        {
            if (this.rb == null) this.LoadRigidbody();
            if (this.rb != null)
            {
                this.rb.isKinematic = false;
                this.rb.WakeUp();
                this.rb.linearVelocity = (direction != Vector3.zero ? direction.normalized : this.transform.forward) * (force > 0f ? force : 45f);
            }
            else
            {
                Debug.LogWarning($"[{this.gameObject.name}] CannonBulletController: Không tìm thấy component CannonBulletMove lẫn Rigidbody để phóng đạn!");
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (this.bulletImpact != null)
        {
            this.bulletImpact.OnImpactTrigger(other);
        }
    }
}
