using System;
using UnityEngine;

public abstract class BaseDameSender : BaseLoadComponent
{
    [Header("Damage Settings")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected GameObject attacker;
    [Header("Collider")]
    [SerializeField] protected Collider col;
    public event Action<BaseDameReceiver, float> OnSendDamageEvent;

    public virtual float Damage => this.damage;
    public virtual GameObject Attacker => this.attacker;

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

    public virtual void SetAttacker(GameObject newAttacker)
    {
        this.attacker = newAttacker;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        this.Send(other);
    }

    public virtual void Send(Collider targetCollider)
    {
        if (targetCollider == null) return;
        BaseDameReceiver receiver = this.GetDameReceiver(targetCollider);
        if (receiver == null) return;

        this.Send(receiver);
    }

    public virtual void Send(BaseDameReceiver receiver)
    {
        if (!this.CanSendDamage(receiver)) return;

        receiver.Deduct(this.damage, this.GetAttacker());
        this.OnHit(receiver, this.damage);
        this.OnSendDamageEvent?.Invoke(receiver, this.damage);
    }

    protected virtual BaseDameReceiver GetDameReceiver(Collider targetCollider)
    {
        if (targetCollider == null) return null;
        return targetCollider.GetComponentInChildren<BaseDameReceiver>();
    }

    protected virtual bool CanSendDamage(BaseDameReceiver receiver)
    {
        if (receiver == null) return false;
        if (receiver.IsDead) return false;

        // Tránh tự gây sát thương cho bản thân hoặc đối tượng cùng nguồn gốc
        if (receiver.transform.root == this.transform.root) return false;
        if (this.attacker != null && receiver.transform.root == this.attacker.transform.root) return false;

        return true;
    }

    protected virtual GameObject GetAttacker()
    {
        if (this.attacker != null) return this.attacker;
        return this.transform.root.gameObject;
    }

    protected virtual void OnHit(BaseDameReceiver receiver, float damageDealt)
    {
        // Class con ghi đè nếu cần xử lý hiệu ứng riêng khi trúng đích
    }
}
