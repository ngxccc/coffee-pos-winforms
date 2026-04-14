using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public class EditUserForm : BaseUserAccountForm
{
    private int _targetUserId;
    private UserRole _initialRole;

    public EditUserForm() : base("SỬA TÀI KHOẢN", new Size(430, 460), "Mật khẩu mới (để trống nếu không đổi)")
    {
        BtnSave.Text = "CẬP NHẬT";
        BtnSave.BackColor = Color.FromArgb(243, 156, 18);
    }

    public void LoadUser(UserGridDto user)
    {
        _targetUserId = user.Id;
        TxtUsername.Text = user.Username;
        TxtFullName.Text = user.FullName;
        _initialRole = user.Role;

        Text = $"SỬA TÀI KHOẢN - {user.Username}";
        TxtPassword.Clear();
        TxtConfirmPassword.Clear();
    }

    public UpdateUserAccountDto BuildCommand()
        => new(
            _targetUserId,
            TxtUsername.Text,
            TxtFullName.Text,
            SelectedRole,
            TxtPassword.Text,
            TxtConfirmPassword.Text);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        CboRole.SelectedValue = _initialRole;
    }

    protected override bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(TxtUsername.Text))
        {
            MessageBoxHelper.Warning("Tên đăng nhập không được để trống!", owner: this);
            TxtUsername.Focus();
            return false;
        }

        bool hasAnyPasswordInput = !string.IsNullOrWhiteSpace(TxtPassword.Text) || !string.IsNullOrWhiteSpace(TxtConfirmPassword.Text);
        if (!hasAnyPasswordInput)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(TxtPassword.Text) || string.IsNullOrWhiteSpace(TxtConfirmPassword.Text))
        {
            MessageBoxHelper.Warning("Nếu đổi mật khẩu, vui lòng nhập đầy đủ mật khẩu mới và xác nhận.", owner: this);
            return false;
        }

        if (TxtPassword.Text != TxtConfirmPassword.Text)
        {
            MessageBoxHelper.Warning("Mật khẩu xác nhận không khớp!", owner: this);
            TxtConfirmPassword.Focus();
            return false;
        }

        return true;
    }
}
