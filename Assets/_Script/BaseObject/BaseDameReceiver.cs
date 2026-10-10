using System;
using UnityEngine;

public abstract class BaseDameReceiver : BaseLoadComponent
{
    [Header("Health Settings")]
    [SerializeField] protected float hp = 100f;
    [SerializeField] protected float maxHp = 100f;
    [SerializeField] protected bool isDead = false;
    [SerializeField] protected bool isImmortal = false; //bât tử

    [Header("Components")]
    [SerializeField] protected Collider col;

    // Events cho UI thanh máu, âm thanh hoặc hệ thống khác đăng ký lắng nghe
    public event Action<float, float> OnHpChangedEvent; // (currentHp, maxHp)
    public event Action OnDeadEvent;
    public event Action<float> OnTakeDamageEvent;       // (damageTaken)

    public virtual float CurrentHp => this.hp;
    public virtual float MaxHp => this.maxHp;
    public virtual bool IsDead => this.isDead;
    public virtual bool IsImmortal 
    { 
        get => this.isImmortal; 
        set => this.isImmortal = value; 
    }
    public virtual float HpPercent => this.maxHp > 0f ? Mathf.Clamp01(this.hp / this.maxHp) : 0f;

    protected virtual void OnEnable()
    {
        this.Reborn();
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadCollider();
    }

    protected virtual void LoadCollider()
    {
        if (this.col != null) return;
        this.col = GetComponent<Collider>();
    }
    public virtual void Deduct(float damage, GameObject attacker = null)
    {
        if (this.isDead || this.isImmortal || damage <= 0f) return;

        this.hp -= damage;
        this.OnTakeDamage(damage, attacker);
        this.OnTakeDamageEvent?.Invoke(damage);

        if (this.hp <= 0f)
        {
            this.hp = 0f;
            this.isDead = true;
            this.OnDead();
            this.OnDeadEvent?.Invoke();
        }

        this.OnHpChanged(this.hp, this.maxHp);
        this.OnHpChangedEvent?.Invoke(this.hp, this.maxHp);
    }
    public virtual void Add(float healAmount)
    {
        if (this.isDead || healAmount <= 0f) return;

        this.hp = Mathf.Min(this.hp + healAmount, this.maxHp);

        this.OnHpChanged(this.hp, this.maxHp);
        this.OnHpChangedEvent?.Invoke(this.hp, this.maxHp);
    }
    public virtual void Reborn()
    {
        this.isDead = false;
        this.hp = this.maxHp;
        this.OnHpChanged(this.hp, this.maxHp);
        this.OnHpChangedEvent?.Invoke(this.hp, this.maxHp);
    }
    public virtual void SetMaxHp(float newMaxHp, bool healToFull = false)
    {
        if (newMaxHp <= 0f) return;
        this.maxHp = newMaxHp;
        if (healToFull || this.hp > this.maxHp)
        {
            this.hp = this.maxHp;
        }

        this.OnHpChanged(this.hp, this.maxHp);
        this.OnHpChangedEvent?.Invoke(this.hp, this.maxHp);
    }

    protected virtual void OnDead()
    {
        // Class con ghi đè logic xử lý cái chết (ví dụ: play Die animation, disable movement)
    }
    protected virtual void OnTakeDamage(float damage, GameObject attacker)
    {
        // Class con ghi đè hiệu ứng nhận đòn
    }
    protected virtual void OnHpChanged(float currentHp, float maxHp)
    {
        // Class con có thể cập nhật HealthBar
    }
}
