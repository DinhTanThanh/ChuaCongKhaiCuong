using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Data/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public string weaponName = "New Weapon";
    public float bonusDamage;
    public float damageMultiplier = 1f;
    public float bonusCritRate;
    public float bonusCritDamage;
}
