using CoffeePOS.Core;
using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;
using Serilog;

namespace CoffeePOS.Features.Sidebar;

public record ChangePasswordPayload(string CurrentPassword, string NewPassword, string ConfirmPassword);

public partial class UC_Settings : UserControl, IValidatableComponent<ChangePasswordPayload>
{
    private readonly IUserSession _session;

    public UC_Settings(IUserSession session)
    {
        _session = session;
        InitializeComponent();

        HydrateData();
    }

    private void HydrateData()
    {
        Log.Debug($"{_session?.CurrentUser?.Role}");
        _lblRoleValue.Text = FormatRole(_session?.CurrentUser?.Role);
        _lblFullNameValue.Text = _session?.CurrentUser?.FullName ?? "N/A";
        _lblUsernameValue.Text = _session?.CurrentUser?.Username ?? "N/A";
    }

    // PERF: O(1) Time Complexity. Simple string comparisons.
    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtOldPass.Text))
        {
            MessageBoxHelper.Warning("Vui lòng nhập mật khẩu hiện tại.", owner: this);
            _txtOldPass.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_txtNewPass.Text))
        {
            MessageBoxHelper.Warning("Vui lòng nhập mật khẩu mới.", owner: this);
            _txtNewPass.Focus();
            return false;
        }

        if (_txtNewPass.Text != _txtConfirmPass.Text)
        {
            MessageBoxHelper.Warning("Mật khẩu xác nhận không khớp.", owner: this);
            _txtConfirmPass.Focus();
            return false;
        }

        return true;
    }

    public ChangePasswordPayload GetPayload()
        => new(_txtOldPass.Text, _txtNewPass.Text, _txtConfirmPass.Text);

    private static string FormatRole(UserRole? role)
        => role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Cashier => "Thu ngân",
            _ => "N/A"
        };
}
