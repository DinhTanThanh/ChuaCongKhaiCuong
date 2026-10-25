using System;
using UnityEngine;

public abstract class BaseDameReceiver : BaseLoadComponent
{
    [Header("Health Settings")]
    [SerializeField] protected float hp;
    [SerializeField] protected float maxHp;
    [SerializeField] protected bool isDead = false;
    [SerializeField] protected bool isImmortal = false; // Bất tử

    [Header("Defense Settings")]
    [SerializeField] protected float defense = 0f;

    [Header("Damage Popup Settings")]
    [SerializeField] protected bool showDamagePopup = true;
    [SerializeField] protected Vector3 popupOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Components")]
    [SerializeField] protected Collider col;

    // Events cho UI thanh máu, âm thanh hoặc hệ thống khác đăng ký lắng nghe
    public event Action<float, float> OnHpChangedEvent; // (currentHp, maxHp)
    public event Action OnDeadEvent;
    public event Action<float> OnTakeDamageEvent;       // (damageTaken)

    public virtual float CurrentHp => this.hp;
    public virtual float MaxHp => this.maxHp;
    public virtual float Defense => this.defense;
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
        this.col = GetComponentInParent<Collider>();
    }

    public virtual void Deduct(float damage, GameObject attacker = null, bool isCritical = false)
    {
        if (this.isDead || this.isImmortal || damage <= 0f) return;

        float finalDamage = this.ApplyDefenseFormula(damage, this.defense);
        this.hp -= finalDamage;
        this.OnTakeDamage(finalDamage, attacker);
        this.OnTakeDamageEvent?.Invoke(finalDamage);

        // Hiển thị số sát thương (Màu đỏ nếu thường, Màu vàng nếu chí mạng)
        if (this.showDamagePopup)
        {
            this.ShowDamagePopup(finalDamage, isCritical: isCritical, isHeal: false);
        }

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

    protected virtual float ApplyDefenseFormula(float incomingDamage, float defense)
    {
        if (defense <= 0f) return incomingDamage;
        float finalDamage = incomingDamage - defense;
        return Mathf.Max(1f, finalDamage);
    }

    public virtual void Add(float healAmount)
    {
        if (this.isDead || healAmount <= 0f) return;

        float prevHp = this.hp;
        this.hp = Mathf.Min(this.hp + healAmount, this.maxHp);
        float actualHealed = this.hp - prevHp;

        // Hiển thị số hồi máu (Màu xanh lá cây)
        if (this.showDamagePopup && actualHealed > 0f)
        {
            this.ShowDamagePopup(actualHealed, isCritical: false, isHeal: true);
        }

        this.OnHpChanged(this.hp, this.maxHp);
        this.OnHpChangedEvent?.Invoke(this.hp, this.maxHp);
    }

    protected virtual void ShowDamagePopup(float amount, bool isCritical = false, bool isHeal = false)
    {
        Vector3 spawnPos = this.GetDamagePopupPosition();
        DamagePopup.Create(spawnPos, amount, isCritical, isHeal);
    }

    protected virtual Vector3 GetDamagePopupPosition()
    {
        if (this.col != null)
        {
            return this.col.bounds.center + Vector3.up * (this.col.bounds.extents.y * 0.7f);
        }
        return this.transform.position + this.popupOffset;
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

    public virtual void SetDefense(float newDefense)
    {
        this.defense = Mathf.Max(0f, newDefense);
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

#if UNITY_EDITOR
    [ContextMenu("Test Take Damage - Normal (60 - Màu đỏ)")]
    protected virtual void EditorTestNormalDamage()
    {
        this.Deduct(60f, null, isCritical: false);
    }

    [ContextMenu("Test Take Damage - Critical (100 - Màu vàng)")]
    protected virtual void EditorTestCriticalDamage()
    {
        this.Deduct(100f, null, isCritical: true);
    }

    [ContextMenu("Test Heal (30 - Màu xanh lá)")]
    protected virtual void EditorTestHeal()
    {
        this.Add(30f);
    }
#endif
}
