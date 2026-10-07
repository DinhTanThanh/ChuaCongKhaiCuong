using UnityEngine;

public class BaseController : BaseLoadComponent
{
    [SerializeField] protected CharacterSO characterSO;
    public CharacterSO CharacterSO => characterSO;
    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadCharacterSO();
    }
    protected virtual void LoadCharacterSO()
    {
        if (this.characterSO != null) return;
        this.characterSO = Resources.Load<CharacterSO>(name+"SO");
        Debug.LogWarning(name + " : LoadCharacterSO");
    }
}
