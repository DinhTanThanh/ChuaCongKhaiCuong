using UnityEngine;

public class StoneArchGateDoor : MonoBehaviour
{
    private static readonly int IsOpenHash = Animator.StringToHash("isOpen");
    private static readonly int OpenTriggerHash = Animator.StringToHash("Open");
    private static readonly int CloseTriggerHash = Animator.StringToHash("Close");

    [SerializeField] private Animator gateAnimator;
    [SerializeField] private bool isOpen = false;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (gateAnimator == null)
        {
            gateAnimator = GetComponentInChildren<Animator>();
        }
    }

    [ContextMenu("Mở Cửa (Open Door)")]
    public void OpenDoor()
    {
        if (gateAnimator == null)
            gateAnimator = GetComponentInChildren<Animator>();

        isOpen = true;
        if (gateAnimator != null)
        {
            gateAnimator.SetBool(IsOpenHash, true);
            gateAnimator.SetTrigger(OpenTriggerHash);
        }
    }

    [ContextMenu("Đóng Cửa (Close Door)")]
    public void CloseDoor()
    {
        if (gateAnimator == null)
            gateAnimator = GetComponentInChildren<Animator>();

        isOpen = false;
        if (gateAnimator != null)
        {
            gateAnimator.SetBool(IsOpenHash, false);
            gateAnimator.SetTrigger(CloseTriggerHash);
        }
    }

    [ContextMenu("Đảo Trạng Thái Cửa (Toggle Door)")]
    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }
}

