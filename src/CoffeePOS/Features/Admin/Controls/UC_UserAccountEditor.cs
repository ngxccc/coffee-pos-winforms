using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Dtos.User;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record UserAccountPayload(string Username, string FullName, UserRole Role, string Password, string ConfirmPassword);

public partial class UC_UserAccountEditor : UserControl, IValidatableComponent<UserAccountPayload>
{
    private readonly UserGridDto? _existingUser;

    public UC_UserAccountEditor(UserGridDto? existingUser = null)
    {
        _existingUser = existingUser;

        InitializeComponent();
        SetupRoleDropdown();
        LoadExistingData();
    }

    private void SetupRoleDropdown()
    {
        _cboRole.Items.Clear();

        var roles = UserRoleOptions.CreateDefault()
            .Select(r => new AntdUI.SelectItem(r.Name, r.Value))
            .ToArray();

        _cboRole.Items.AddRange(roles);

        if (_cboRole.Items.Count > 0)
        {
            _cboRole.SelectedIndex = 0;
        }
    }

    private void LoadExistingData()
    {
        if (_existingUser == null) return;

        _txtUsername.Text = _existingUser.Username;
        _txtFullName.Text = _existingUser.FullName;
        _cboRole.SelectedValue = _existingUser.Role;

        // WHY: Immutable identity to prevent cascading foreign key anomalies or unhandled conflicts
        _txtUsername.Enabled = false;

        _lblPassword.Text = "Mật khẩu mới (Để trống nếu không đổi)";
        _txtPassword.PlaceholderText = "Nhập mật khẩu mới";
    }

    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtUsername.Text))
        {
            MessageBoxHelper.Warning("Tên đăng nhập không được để trống!", owner: this);
            _txtUsername.Focus();
            return false;
        }

        if (_existingUser == null && string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            MessageBoxHelper.Warning("Vui lòng nhập mật khẩu!", owner: this);
            _txtPassword.Focus();
            return false;
        }

        if (_txtPassword.Text != _txtConfirm.Text)
        {
            MessageBoxHelper.Warning("Mật khẩu xác nhận không khớp!", owner: this);
            _txtConfirm.Focus();
            return false;
        }

        return true;
    }

    public UserAccountPayload GetPayload()
    {
        var role = _cboRole.SelectedValue is UserRole r ? r : UserRole.Cashier;

        return new UserAccountPayload(
            _txtUsername.Text.Trim(),
            _txtFullName.Text.Trim(),
            role,
            _txtPassword.Text,
            _txtConfirm.Text
        );
    }
}
