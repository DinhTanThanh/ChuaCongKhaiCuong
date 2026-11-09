using UnityEngine;

[DisallowMultipleComponent]
public class PooledObject : BaseLoadComponent
{
    [SerializeField] protected string poolKey;

    public string PoolKey
    {
        get => this.poolKey;
        set => this.poolKey = value;
    }
}
