using UnityEngine;

[CreateAssetMenu(fileName = "BulletSO", menuName = "Data/BulletSO")]
public class BulletSO : WeaponSO
{
    [Header("Bullet Prefab")]
    public CannonBulletController bulletPrefab;

    [Header("Bullet Physics")]
    public float launchForce = 45f;
    public float mass = 1.5f;
    public float gravityScale = 1f;

    [Header("Explosion Settings")]
    public float explosionRadius = 3.5f;

    [Header("Lifetime & VFX")]
    public float lifeTime = 7f;
    public GameObject explosionVfxPrefab;

    // Getter tiện ích tương thích ngược (không hiển thị trên Inspector)
    public string bulletName => this.weaponName;
    public float damage => this.bonusDamage;
    public float critRate => this.bonusCritRate;
    public float critDamageMultiplier => this.bonusCritDamage;
}
