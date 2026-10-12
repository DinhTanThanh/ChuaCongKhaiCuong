using UnityEngine;

public class WukongDameSender : BaseDameSender
{
    [SerializeField] protected WukongController controller;

    public virtual WukongController Controller => this.controller;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadWukongController();
    }

    protected virtual void LoadWukongController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<WukongController>();
    }

    protected override void OnEnable()
    {
        this.SyncCharacterStats();
        base.OnEnable();
        this.RegisterControllerWeaponEvent();
    }

    protected virtual void OnDisable()
    {
        this.UnregisterControllerWeaponEvent();
    }

    protected virtual void RegisterControllerWeaponEvent()
    {
        if (this.controller == null) return;
        this.controller.OnWeaponChangedEvent += this.OnControllerWeaponChanged;

        // Tự động đồng bộ vũ khí ban đầu từ Controller nếu đã có sẵn
        if (this.controller.CurrentWeapon != null)
        {
            this.EquipWeapon(this.controller.CurrentWeapon);
        }
    }

    protected virtual void UnregisterControllerWeaponEvent()
    {
        if (this.controller == null) return;
        this.controller.OnWeaponChangedEvent -= this.OnControllerWeaponChanged;
    }

    protected virtual void OnControllerWeaponChanged(WeaponSO newWeapon)
    {
        this.EquipWeapon(newWeapon);
    }

    protected virtual void SyncCharacterStats()
    {
        if (this.controller == null || this.controller.CharacterSO == null) return;

        this.SetDamage(this.controller.CharacterSO.damage);
        this.SetCritRate(this.controller.CharacterSO.critRate);
        this.SetCritDamageMultiplier(this.controller.CharacterSO.critDamageMultiplier);
    }

    public override void EquipWeapon(WeaponSO weaponSO)
    {
        base.EquipWeapon(weaponSO);

        float previewDamage = this.CalculateFinalDamage(this.damage, weaponSO, out bool _);
        string weaponTitle = weaponSO != null ? weaponSO.weaponName : "Tay không";
        Debug.Log($"[{this.gameObject.name}] Cập nhật vũ khí thành công: '{weaponTitle}'. Sát thương cơ bản mới: {previewDamage}");
    }

    protected override void OnHit(BaseDameReceiver receiver, float damageDealt, bool isCritical)
    {
        base.OnHit(receiver, damageDealt, isCritical);

        // Logic xử lý khi Wukong đánh trúng mục tiêu (VFX va chạm, âm thanh gậy trúng đòn, rung camera...)
        if (isCritical)
        {
            // Hiệu ứng đặc biệt khi đòn đánh bạo kích (VFX nổ chí mạng vàng/đỏ, âm thanh uy lực...)
        }
    }
}
