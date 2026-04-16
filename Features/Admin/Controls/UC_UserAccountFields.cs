using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

// WHY: Pure UI State Record. Decouples UI component from Backend DTOs (CreateUserDto/UpdateUserAccountDto).
public record UserAccountPayload(string Username, string FullName, UserRole Role, string Password, string ConfirmPassword);

// HACK: Strongly typed implementation of the generic interface.
public class UC_UserAccountFields : UserControl, IValidatableComponent<UserAccountPayload>
{
    private AntdUI.Input _txtUsername = null!;
    private AntdUI.Input _txtFullName = null!;
    private ComboBox _cboRole = null!;
    private AntdUI.Input _txtPassword = null!;
    private AntdUI.Input _txtConfirm = null!;

    private readonly UserGridDto? _existingUser;

    // WHY: Nullable DTO injection. If null -> Add Mode. If has data -> Edit Mode.
    public UC_UserAccountFields(UserGridDto? existingUser = null)
    {
        _existingUser = existingUser;
        BuildUI();
        LoadExistingData();
    }

    private void BuildUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        int yPos = 10;
        int left = 20;
        int width = 370;

        // HACK: Stack-based manual layout computation
        Controls.Add(new AntdUI.Label { Text = "Tên đăng nhập", Location = new Point(left, yPos), AutoSize = true });
        _txtUsername = new AntdUI.Input
        {
            Location = new Point(left, yPos += 25),
            Width = width,
            Font = new Font("Segoe UI", 10),
            PlaceholderText = "Nhập tên đăng nhập",
            AllowClear = true
        };
        Controls.Add(_txtUsername);

        Controls.Add(new AntdUI.Label { Text = "Họ tên", Location = new Point(left, yPos += 45), AutoSize = true });
        _txtFullName = new AntdUI.Input
        {
            Location = new Point(left, yPos += 25),
            Width = width,
            Font = new Font("Segoe UI", 10),
            PlaceholderText = "Nhập họ tên",
            AllowClear = true
        };
        Controls.Add(_txtFullName);

        Controls.Add(new AntdUI.Label { Text = "Vai trò", Location = new Point(left, yPos += 45), AutoSize = true });
        _cboRole = new ComboBox
        {
            Location = new Point(left, yPos += 25),
            Width = width,
            Font = new Font("Segoe UI", 10),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(RoleOptionItem.Name),
            ValueMember = nameof(RoleOptionItem.Value),
            DataSource = UserRoleOptions.CreateDefault()
        };
        Controls.Add(_cboRole);

        // WHY: Passwords might be optional during Edit mode, indicated by label text
        string passLabel = _existingUser == null ? "Mật khẩu" : "Mật khẩu mới (Để trống nếu không đổi)";
        Controls.Add(new AntdUI.Label
        {
            Text = passLabel,
            Location = new Point(left, yPos += 45),
            AutoSize = true
        });
        _txtPassword = new AntdUI.Input
        {
            Location = new Point(left, yPos += 25),
            Width = width,
            Font = new Font("Segoe UI", 10),
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu mới"
        };
        Controls.Add(_txtPassword);

        Controls.Add(new AntdUI.Label
        {
            Text = "Xác nhận mật khẩu",
            Location = new Point(left, yPos += 45),
            AutoSize = true
        });
        _txtConfirm = new AntdUI.Input
        {
            Location = new Point(left, yPos += 25),
            Width = width,
            Font = new Font("Segoe UI", 10),
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập lại mật khẩu"
        };
        Controls.Add(_txtConfirm);
    }

    private void LoadExistingData()
    {
        if (_existingUser == null) return;

        _txtUsername.Text = _existingUser.Username;
        _txtFullName.Text = _existingUser.FullName;
        _cboRole.SelectedValue = _existingUser.Role;

        // WHY: Prevent changing username during edit to avoid cascading DB conflicts
        _txtUsername.Enabled = false;
    }

    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtUsername.Text))
        {
            MessageBoxHelper.Warning("Tên đăng nhập không được để trống!", owner: this);
            return false;
        }

        // HACK: Password is required ONLY in Add Mode. In Edit Mode, blank means keeping the old password.
        if (_existingUser == null && string.IsNullOrWhiteSpace(_txtPassword.Text))
        {
            MessageBoxHelper.Warning("Vui lòng nhập mật khẩu!", owner: this);
            return false;
        }

        if (_txtPassword.Text != _txtConfirm.Text)
        {
            MessageBoxHelper.Warning("Mật khẩu xác nhận không khớp!", owner: this);
            return false;
        }

        return true;
    }

    public UserAccountPayload GetPayload()
    {
        var role = _cboRole.SelectedValue is UserRole r ? r : UserRole.Cashier;

        // PERF: O(1) allocation. Returning pure state.
        return new UserAccountPayload(
            _txtUsername.Text.Trim(),
            _txtFullName.Text.Trim(),
            role,
            _txtPassword.Text,
            _txtConfirm.Text
        );
    }
}
