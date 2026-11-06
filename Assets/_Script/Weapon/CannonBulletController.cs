using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CannonBulletController : BaseLoadComponent, IPoolable
{
    [Header("Bullet Components")]
    [SerializeField] protected BulletSO bulletSO;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider bulletCollider;
    [SerializeField] protected CannonBulletMove bulletMove;
    [SerializeField] protected CannonBulletDameSender dameSender;
    [SerializeField] protected CannonBulletImpact bulletImpact;

    protected Coroutine lifeTimerCoroutine;

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
        this.bulletSO = Resources.Load<BulletSO>(transform.name + "SO");
        this.ApplyBulletSO();
    }

    public virtual void ApplyBulletSO()
    {
        if (this.bulletSO == null) return;

        if (this.dameSender != null)
        {
            this.dameSender.SetDamage(this.bulletSO.damage);
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

    public virtual void Launch(Vector3 direction, float force, GameObject shooter)
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

        this.StartLifeTimer();
    }

    public virtual void Despawn()
    {
        this.StopLifeTimer();
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Despawn(this.gameObject);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    protected virtual void StartLifeTimer()
    {
        this.StopLifeTimer();
        float life = (this.bulletSO != null && this.bulletSO.lifeTime > 0f) ? this.bulletSO.lifeTime : 7f;
        this.lifeTimerCoroutine = this.StartCoroutine(this.LifeTimerRoutine(life));
    }

    protected virtual void StopLifeTimer()
    {
        if (this.lifeTimerCoroutine != null)
        {
            this.StopCoroutine(this.lifeTimerCoroutine);
            this.lifeTimerCoroutine = null;
        }
    }

    protected virtual IEnumerator LifeTimerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        this.Despawn();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (this.bulletImpact != null)
        {
            this.bulletImpact.OnImpactTrigger(other);
        }
    }

    #region IPoolable Implementation

    public virtual void OnSpawnFromPool()
    {
        if (this.bulletMove != null) this.bulletMove.ResetMove();
        if (this.bulletImpact != null) this.bulletImpact.ResetImpact();
        this.ApplyBulletSO();
    }

    public virtual void OnReturnToPool()
    {
        this.StopLifeTimer();
        if (this.bulletMove != null) this.bulletMove.StopMove();
    }

    #endregion
}
