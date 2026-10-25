using UnityEngine;

public class CannonBulletDameSender : BaseDameSender
{
    [Header("Cannon Damage Settings")]
    [SerializeField] protected CannonBulletController controller;
    [SerializeField] protected float explosionRadius = 3.5f;

    public virtual CannonBulletController Controller => this.controller;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadController();
        this.LoadBulletSO();
    }

    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<CannonBulletController>();
    }

    protected virtual void Start()
    {
        this.LoadBulletSO();
    }

    public virtual void LoadBulletSO()
    {
        if (this.controller == null) this.LoadController();
        if (this.controller == null || this.controller.BulletSO == null) return;

        BulletSO bulletSO = this.controller.BulletSO;
        this.SetDamage(bulletSO.damage);
        this.SetCritRate(bulletSO.critRate);
        this.SetCritDamageMultiplier(bulletSO.critDamageMultiplier);
        this.SetExplosionRadius(bulletSO.explosionRadius);
    }

    public virtual void SetAttacker(GameObject newAttacker)
    {
        this.attacker = newAttacker;
    }

    public virtual void SetExplosionRadius(float newRadius)
    {
        if (newRadius < 0f) return;
        this.explosionRadius = newRadius;
    }

    public virtual void ExplodeDamage(Vector3 hitPoint)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPoint, this.explosionRadius);
        foreach (Collider col in colliders)
        {
            if (this.attacker != null && col.transform.root == this.attacker.transform.root) continue;
            this.Send(col);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, this.explosionRadius);
    }
}
