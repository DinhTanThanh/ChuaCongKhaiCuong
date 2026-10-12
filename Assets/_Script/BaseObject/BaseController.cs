using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : BaseLoadComponent
{
    [Header("Character Stats")]
    [SerializeField] protected CharacterSO characterSO;

    [Header("Weapon Management")]
    [SerializeField] protected WeaponSO currentWeapon;
    [SerializeField] protected List<WeaponSO> weaponList = new List<WeaponSO>();
    [SerializeField] protected BaseDameSender dameSender;

    // Sự kiện khi đổi vũ khí để UI hoặc các hệ thống khác lắng nghe
    public event Action<WeaponSO> OnWeaponChangedEvent;

    public virtual CharacterSO CharacterSO => this.characterSO;
    public virtual WeaponSO CurrentWeapon => this.currentWeapon;
    public virtual List<WeaponSO> WeaponList => this.weaponList;
    public virtual BaseDameSender DameSender => this.dameSender;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadCharacterSO();
        this.LoadDameSender();
    }

    protected virtual void LoadCharacterSO()
    {
        if (this.characterSO != null) return;
        this.characterSO = Resources.Load<CharacterSO>(name + "SO");
        Debug.LogWarning(name + " : LoadCharacterSO");
    }

    protected virtual void LoadDameSender()
    {
        if (this.dameSender != null) return;
        this.dameSender = GetComponent<BaseDameSender>();
    }

    protected virtual void Start()
    {
        // Khởi tạo trang bị vũ khí ban đầu cho damage sender
        if (this.currentWeapon != null)
        {
            this.UpdateWeaponToDamageSender(this.currentWeapon);
        }
        else if (this.weaponList.Count > 0 && this.weaponList[0] != null)
        {
            this.ChangeWeapon(this.weaponList[0]);
        }
    }

    public virtual void ChangeWeapon(WeaponSO newWeapon)
    {
        this.currentWeapon = newWeapon;
        this.UpdateWeaponToDamageSender(newWeapon);
        this.OnWeaponChangedEvent?.Invoke(newWeapon);

        string weaponName = newWeapon != null ? newWeapon.weaponName : "Tay không";
        Debug.Log($"[{this.gameObject.name}] Đã đổi vũ khí thành: '{weaponName}'");
    }

    public virtual void ChangeWeapon(int index)
    {
        if (this.weaponList == null || this.weaponList.Count == 0) return;
        if (index < 0 || index >= this.weaponList.Count) return;

        this.ChangeWeapon(this.weaponList[index]);
    }
    public virtual void NextWeapon()
    {
        if (this.weaponList == null || this.weaponList.Count <= 1) return;

        int currentIndex = this.weaponList.IndexOf(this.currentWeapon);
        int nextIndex = (currentIndex + 1) % this.weaponList.Count;
        this.ChangeWeapon(nextIndex);
    }
    public virtual void PreviousWeapon()
    {
        if (this.weaponList == null || this.weaponList.Count <= 1) return;

        int currentIndex = this.weaponList.IndexOf(this.currentWeapon);
        int prevIndex = (currentIndex - 1 + this.weaponList.Count) % this.weaponList.Count;
        this.ChangeWeapon(prevIndex);
    }
    public virtual void UpdateWeaponToDamageSender(WeaponSO weapon)
    {
        if (this.dameSender == null)
        {
            this.LoadDameSender();
        }

        if (this.dameSender != null)
        {
            this.dameSender.EquipWeapon(weapon);
        }
    }
}
