using UnityEngine;

public class BaseLoadComponent : MonoBehaviour
{
    protected virtual void Reset()
    {
        this.LoadComponent();
    }
    protected virtual void Awake()
    {
        this.LoadComponent();
    }
    protected virtual void LoadComponent() { }
}
