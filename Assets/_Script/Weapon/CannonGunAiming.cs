using UnityEngine;

public class CannonGunAiming : BaseLoadComponent
{
    [Header("Components")]
    [SerializeField] protected CannonGunController controller;

    [Header("Trajectory (Quỹ đạo ngắm bắn)")]
    [SerializeField] protected bool showTrajectoryLine = false;
    [SerializeField] protected LineRenderer trajectoryLine;
    [SerializeField] protected int trajectorySteps = 30;
    [SerializeField] protected float trajectoryTimeStep = 0.08f;

    public virtual CannonGunController Controller => this.controller;
    public virtual bool ShowTrajectoryLine
    {
        get => this.showTrajectoryLine;
        set => this.showTrajectoryLine = value;
    }
    public virtual LineRenderer TrajectoryLine => this.trajectoryLine;

    protected override void LoadComponent()
    {
        base.LoadComponent();
        this.LoadController();
        this.LoadTrajectoryLine();
    }

    protected virtual void LoadController()
    {
        if (this.controller != null) return;
        this.controller = GetComponentInParent<CannonGunController>();
    }

    protected virtual void LoadTrajectoryLine()
    {
        if (this.trajectoryLine != null) return;
        this.trajectoryLine = GetComponent<LineRenderer>();
    }

    protected virtual void Start()
    {
        if (this.trajectoryLine != null)
        {
            this.trajectoryLine.enabled = false;
        }
    }

    protected virtual void Update()
    {
        this.UpdateTrajectory();
    }

    /// <summary>
    /// Lấy hướng bắn thẳng theo góc xoay của nòng súng (firePoint) hoặc súng
    /// </summary>
    public virtual Vector3 GetShootDirection()
    {
        if (this.controller != null && this.controller.FirePoint != null)
        {
            return this.controller.FirePoint.forward;
        }
        return this.transform.forward;
    }

    /// <summary>
    /// Vẽ đường cong parabol dự đoán quỹ đạo bay của đạn pháo
    /// </summary>
    protected virtual void UpdateTrajectory()
    {
        if (this.trajectoryLine == null || !this.showTrajectoryLine)
        {
            if (this.trajectoryLine != null && this.trajectoryLine.enabled)
            {
                this.trajectoryLine.enabled = false;
            }
            return;
        }

        if (!this.trajectoryLine.enabled)
        {
            this.trajectoryLine.enabled = true;
        }

        Vector3 startPos = this.controller != null && this.controller.FirePoint != null
            ? this.controller.FirePoint.position
            : this.transform.position;

        Vector3 shootDir = this.GetShootDirection();
        float force = (this.controller != null && this.controller.Shooting != null)
            ? this.controller.Shooting.LaunchForce
            : 45f;

        Vector3 startVel = shootDir * force;
        Vector3 gravity = Physics.gravity;

        this.trajectoryLine.positionCount = this.trajectorySteps;
        Vector3 currentPos = startPos;
        Vector3 currentVel = startVel;

        for (int i = 0; i < this.trajectorySteps; i++)
        {
            this.trajectoryLine.SetPosition(i, currentPos);

            // Dự đoán rơi theo trọng lực môi trường
            currentVel += gravity * this.trajectoryTimeStep;
            currentPos += currentVel * this.trajectoryTimeStep;

            // Dừng vẽ nếu va chạm vào vật cản (bỏ qua bản thân người bắn)
            if (i > 0 && Physics.Linecast(this.trajectoryLine.GetPosition(i - 1), currentPos, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.root != this.transform.root)
                {
                    this.trajectoryLine.positionCount = i + 1;
                    this.trajectoryLine.SetPosition(i, hit.point);
                    break;
                }
            }
        }
    }
}

