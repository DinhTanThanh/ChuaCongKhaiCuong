using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDameSender : BaseLoadComponent
{
    [SerializeField] protected float damage;
    [Range(0f, 1f)]
    [SerializeField] protected float critRate;
    [SerializeField] protected float critDamageMultiplier;
    [SerializeField] protected float minDamage = 1f;
    [SerializeField] protected WeaponSO equippedWeaponSO;

    [Header("Collider & Hit Settings")]
    [SerializeField] protected Collider col;
    [SerializeField] protected GameObject attacker;
    [SerializeField] protected bool preventDuplicateHit = true;
    [SerializeField] protected List<BaseDameReceiver> hitReceivers = new List<BaseDameReceiver>();

    // Events cho hệ thống khác (UI, Sound, VFX, Combo counter...) đăng ký lắng nghe
    public event Action<BaseDameReceiver, float> OnSendDamageEvent;
    public event Action<WeaponSO> OnWeaponChangedEvent;

    public virtual float Damage => this.damage;
    public virtual float CritRate => this.critRate;
    public virtual float CritDamageMultiplier => this.critDamageMultiplier;
    public virtual WeaponSO EquippedWeaponSO => this.equippedWeaponSO;
    public virtual GameObject Attacker => this.attacker;

    protected virtual void OnEnable()
    {
        this.ResetHitTargets();
    }

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadCollider();
        this.LoadAttacker();
    }

    protected virtual void LoadCollider()
    {
        if (this.col != null) return;
        this.col = GetComponent<Collider>();
    }

    protected virtual void LoadAttacker()
    {
        if (this.attacker != null) return;
        this.attacker = this.transform.root.gameObject;
    }

    public virtual void SetDamage(float newDamage)
    {
        if (newDamage < 0f) return;
        this.damage = newDamage;
    }

    public virtual void SetCritRate(float newCritRate)
    {
        this.critRate = Mathf.Clamp01(newCritRate);
    }

    public virtual void SetCritDamageMultiplier(float newMultiplier)
    {
        this.critDamageMultiplier = Mathf.Max(1f, newMultiplier);
    }
    //trang bị vũ khí
    public virtual void EquipWeapon(WeaponSO weaponSO)
    {
        this.equippedWeaponSO = weaponSO;
        this.OnWeaponChangedEvent?.Invoke(weaponSO);
    }
    //gỡ bỏ vũ khí
    public virtual void UnequipWeapon()
    {
        this.equippedWeaponSO = null;
        this.OnWeaponChangedEvent?.Invoke(null);
    }
    public virtual void ResetHitTargets()
    {
        this.hitReceivers.Clear();
    }

    public virtual void Send(BaseDameReceiver receiver)
    {
        this.Send(receiver, this.equippedWeaponSO);
    }

    public virtual void Send(BaseDameReceiver receiver, WeaponSO weapon)
    {
        if (!this.CanSendDamage(receiver)) return;

        float finalDamage = this.CalculateFinalDamage(this.damage, weapon, out bool isCritical);
        receiver.Deduct(finalDamage, this.GetAttacker(), isCritical);

        if (this.preventDuplicateHit)
        {
            this.hitReceivers.Add(receiver);
        }

        this.OnHit(receiver, finalDamage, isCritical);
        this.OnSendDamageEvent?.Invoke(receiver, finalDamage);
    }

    public virtual void Send(Collider targetCollider)
    {
        this.Send(targetCollider, this.equippedWeaponSO);
    }

    public virtual void Send(Collider targetCollider, WeaponSO weapon)
    {
        if (targetCollider == null) return;
        BaseDameReceiver receiver = this.GetDameReceiver(targetCollider);
        if (receiver == null) return;

        this.Send(receiver, weapon);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        this.Send(other, this.equippedWeaponSO);
    }

    protected virtual BaseDameReceiver GetDameReceiver(Collider targetCollider)
    {
        if (targetCollider == null) return null;
        BaseDameReceiver receiver = targetCollider.GetComponentInChildren<BaseDameReceiver>();
        return receiver;
    }

    protected virtual bool CanSendDamage(BaseDameReceiver receiver)
    {
        if (receiver == null) return false;
        if (receiver.IsDead) return false;

        if (receiver.transform.root == this.transform.root) return false;
        if (this.attacker != null && receiver.transform.root == this.attacker.transform.root) return false;

        if (this.preventDuplicateHit && this.hitReceivers.Contains(receiver)) return false;

        return true;
    }

    public virtual float CalculateFinalDamage(float baseDamage, WeaponSO weapon, out bool isCritical)
    {
        if (baseDamage <= 0f && (weapon == null || weapon.bonusDamage <= 0f))
        {
            isCritical = false;
            return 0f;
        }

        float flatDamage = baseDamage;
        float weaponMultiplier = 1f;
        float extraCritRate = 0f;
        float extraCritDamage = 0f;

        if (weapon != null)
        {
            flatDamage += weapon.bonusDamage;
            weaponMultiplier = Mathf.Max(0f, weapon.damageMultiplier);
            extraCritRate = weapon.bonusCritRate;
            extraCritDamage = weapon.bonusCritDamage;
        }

        float rawDamage = Mathf.Max(0f, flatDamage * weaponMultiplier);
        float totalCritRate = Mathf.Clamp01(this.critRate + extraCritRate);
        isCritical = UnityEngine.Random.value < totalCritRate;

        float damageAfterCrit = rawDamage;
        if (isCritical)
        {
            float totalCritMultiplier = Mathf.Max(1f, this.critDamageMultiplier + extraCritDamage);
            damageAfterCrit = rawDamage * totalCritMultiplier;
        }
        float finalDamage = Mathf.Max(this.minDamage, damageAfterCrit);
        return finalDamage;
    }
    protected virtual GameObject GetAttacker()
    {
        if (this.attacker != null) return this.attacker;
        return this.transform.root.gameObject;
    }
    protected virtual void OnHit(BaseDameReceiver receiver, float damageDealt, bool isCritical)
    {
        // Class con ghi đè để xử lý hiệu ứng riêng
    }
}
