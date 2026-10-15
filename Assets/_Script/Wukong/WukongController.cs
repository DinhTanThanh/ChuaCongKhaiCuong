using UnityEngine;

public class WukongController : BaseController
{
    [Header("Wukong Components")]
    [SerializeField] protected WukongFly wukongFly;

    public virtual WukongFly WukongFly => this.wukongFly;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadWukongFly();
    }

    protected virtual void LoadWukongFly()
    {
        if (this.wukongFly != null) return;
        this.wukongFly = GetComponentInChildren<WukongFly>();
    }
}
