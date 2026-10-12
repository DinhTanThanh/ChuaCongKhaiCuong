using UnityEngine;
[CreateAssetMenu(fileName ="CharacterSO",menuName ="Data/CharactorSO")]
public class CharacterSO : ScriptableObject
{
    public float maxHp;
    public float defense;
    public float damage;
    public float critRate;
    public float critDamageMultiplier;
    public float speed;
    public float turnSpeed;
    public float jumpHeight;
}
