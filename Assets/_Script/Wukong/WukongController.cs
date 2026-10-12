using UnityEngine;
using UnityEngine.InputSystem;

public class WukongController : BaseController
{
    [SerializeField] protected bool enableKeyboardWeaponSwitch = true;

    protected virtual void Update()
    {
        this.HandleWeaponSwitchInput();
    }

    protected virtual void HandleWeaponSwitchInput()
    {
        if (!this.enableKeyboardWeaponSwitch) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Phím 1, 2, 3, 4 chọn trực tiếp vũ khí theo index trong danh sách
        if (keyboard.digit1Key.wasPressedThisFrame) this.ChangeWeapon(0);
        else if (keyboard.digit2Key.wasPressedThisFrame) this.ChangeWeapon(1);
        else if (keyboard.digit3Key.wasPressedThisFrame) this.ChangeWeapon(2);
        else if (keyboard.digit4Key.wasPressedThisFrame) this.ChangeWeapon(3);

        // Phím Q để đổi sang vũ khí kế tiếp
        if (keyboard.qKey.wasPressedThisFrame)
        {
            this.NextWeapon();
        }
    }
}
