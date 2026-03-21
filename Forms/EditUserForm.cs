using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public class EditUserForm() : BaseUserAccountForm(
            "SỬA TÀI KHOẢN",
            new Size(430, 440),
            "Mật khẩu mới (để trống nếu không đổi)",
            "CẬP NHẬT",
            Color.FromArgb(243, 156, 18),
            new Point(210, 355),
            new Point(320, 355))
{
    private int _targetUserId;
    private int _initialRoleValue;

    public void LoadUser(UserGridDto user)
    {
        _targetUserId = user.Id;
        TxtUsername.Text = user.Username;
        TxtFullName.Text = user.FullName;
        _initialRoleValue = user.Role;

        Text = $"SỬA TÀI KHOẢN - {user.Username}";
        TxtPassword.Clear();
        TxtConfirmPassword.Clear();
    }

    public UpdateUserAccountDto BuildCommand()
        => new(
            _targetUserId,
            TxtUsername.Text,
            TxtFullName.Text,
            SelectedRoleValue,
            TxtPassword.Text,
            TxtConfirmPassword.Text);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        CboRole.SelectedValue = _initialRoleValue;
    }
}
