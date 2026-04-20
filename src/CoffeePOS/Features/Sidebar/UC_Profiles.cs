using System.ComponentModel.DataAnnotations;
using CoffeePOS.Core;
using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public record ChangePasswordPayload
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự!")]
    public string NewPassword { get; init; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public partial class UC_Profiles : UserControl, IValidatableComponent<ChangePasswordPayload>
{
    private readonly IUserSession _session;

    public UC_Profiles(IUserSession session)
    {
        _session = session;
        InitializeComponent();

        HydrateData();
    }

    private void HydrateData()
    {
        _lblRoleValue.Text = FormatRole(_session?.CurrentUser?.Role);
        _lblFullNameValue.Text = _session?.CurrentUser?.FullName ?? "N/A";
        _lblUsernameValue.Text = _session?.CurrentUser?.Username ?? "N/A";
    }

    public bool ValidateInput()
    {
        if (InvokeRequired) return Invoke(new Func<bool>(ValidateInput));

        ChangePasswordPayload payload = GetPayload();

        if (!ValidationHelper.TryValidate(payload, out string error))
        {
            AntdUI.Message.warn(new AntdUI.Target(this), error);
            return false;
        }

        return true;
    }

    public ChangePasswordPayload GetPayload()
        => new()
        {
            CurrentPassword = _txtCurrPass.Text,
            NewPassword = _txtNewPass.Text,
            ConfirmPassword = _txtConfirmPass.Text
        };

    private static string FormatRole(UserRole? role)
        => role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Cashier => "Thu ngân",
            _ => "N/A"
        };
}
