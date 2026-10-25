using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Data/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName = "New Weapon";

    [Header("Damage Stats")]
    public float bonusDamage;
    public float damageMultiplier = 1f;
    public float bonusCritRate;
    public float bonusCritDamage;
}
