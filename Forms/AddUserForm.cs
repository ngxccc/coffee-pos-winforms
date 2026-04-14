using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public class AddUserForm : BaseUserAccountForm
{
    // WHY: Match the refactored base constructor signature.
    // Increased height to 460 to accommodate the repositioned buttons in the base class.
    public AddUserForm() : base("THÊM NHÂN VIÊN", new Size(420, 460), "Mật khẩu")
    {
        // WHY: UI overrides for specific subclass needs without polluting base constructor
        BtnSave.Text = "LƯU";
        BtnSave.BackColor = Color.FromArgb(46, 204, 113);
    }

    public CreateUserDto BuildCommand()
        => new(
            TxtUsername.Text.Trim(),
            TxtFullName.Text.Trim(),
            SelectedRole,
            TxtPassword.Text,
            TxtConfirmPassword.Text);

    // WHY: Required implementation of Template Method to hook into the save process
    // Time Complexity: O(1) string length checks
    protected override bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(TxtUsername.Text))
        {
            MessageBoxHelper.Warning("Ét o ét! Tên đăng nhập không được để trống!", owner: this);
            TxtUsername.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(TxtPassword.Text))
        {
            MessageBoxHelper.Warning("Chưa nhập mật khẩu sao mà lưu?", owner: this);
            TxtPassword.Focus();
            return false;
        }

        if (TxtPassword.Text != TxtConfirmPassword.Text)
        {
            MessageBoxHelper.Warning("Mật khẩu xác nhận không khớp, check lại nha!", owner: this);
            TxtConfirmPassword.Focus();
            return false;
        }

        return true;
    }
}
