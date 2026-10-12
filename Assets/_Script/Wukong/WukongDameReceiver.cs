using UnityEngine;

public class WukongDameReceiver : BaseDameReceiver
{
    [SerializeField] protected WukongController controller;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadWukongController();
    }
    protected virtual void LoadWukongController()
    {
        if (this.controller != null) return;
        this.controller=GetComponentInParent<WukongController>();   
    }
    protected override void OnEnable()
    {
        if (this.controller == null) return;
        this.SetMaxHp(this.controller.CharacterSO.maxHp);
        this.SetDefense(this.controller.CharacterSO.defense);
        base.OnEnable();
    }
    protected override void OnTakeDamage(float damage, GameObject attacker)
    {
        base.OnTakeDamage(damage, attacker);
        // Logic xử lý khi Wukong nhận sát thương (hiệu ứng, âm thanh, animation...)
    }

    protected override void OnDead()
    {
        base.OnDead();
        // Logic xử lý khi Wukong chết (animation tử trận, tắt điều khiển...)
    }

    protected override void OnHpChanged(float currentHp, float maxHp)
    {
        base.OnHpChanged(currentHp, maxHp);
        // Logic khi máu thay đổi (cập nhật UI thanh máu...)
    }
}
