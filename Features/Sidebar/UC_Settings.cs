using CoffeePOS.Core;
using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public record ChangePasswordPayload(string CurrentPassword, string NewPassword, string ConfirmPassword);

public class UC_Settings : UserControl, IValidatableComponent<ChangePasswordPayload>
{
    private readonly IUserSession _session;

    private AntdUI.Input _txtOldPass = null!;
    private AntdUI.Input _txtNewPass = null!;
    private AntdUI.Input _txtConfirmPass = null!;

    public UC_Settings(IUserSession session)
    {
        _session = session;
        InitializeUI();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        var pnlMain = new AntdUI.Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            Radius = 0,
            Back = Color.White
        };

        var lblTitle = new AntdUI.Label
        {
            Text = "THÔNG TIN CÁ NHÂN",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Top,
            Height = 40
        };

        var pnlRole = CreateInfoRow("Vai trò:", FormatRole(_session.CurrentUser?.Role));
        var pnlFullName = CreateInfoRow("Họ tên:", _session.CurrentUser?.FullName ?? "N/A");
        var pnlUsername = CreateInfoRow("Tài khoản:", _session.CurrentUser?.Username ?? "N/A");

        var pnlDivider = new AntdUI.Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            Radius = 0,
            Back = Color.White
        };
        pnlDivider.Controls.Add(new AntdUI.Divider
        {
            Dock = DockStyle.Bottom,
            Height = 2,
            Text = string.Empty
        });

        var lblPassTitle = new AntdUI.Label
        {
            Text = "ĐỔI MẬT KHẨU",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60),
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.BottomLeft
        };

        var pnlConfirmPass = CreateInputRow("Xác nhận mật khẩu mới:", out _txtConfirmPass);
        var pnlNewPass = CreateInputRow("Mật khẩu mới:", out _txtNewPass);
        var pnlOldPass = CreateInputRow("Mật khẩu hiện tại:", out _txtOldPass);

        pnlMain.Controls.Add(pnlConfirmPass);
        pnlMain.Controls.Add(pnlNewPass);
        pnlMain.Controls.Add(pnlOldPass);
        pnlMain.Controls.Add(lblPassTitle);
        pnlMain.Controls.Add(pnlDivider);
        pnlMain.Controls.Add(pnlRole);
        pnlMain.Controls.Add(pnlFullName);
        pnlMain.Controls.Add(pnlUsername);
        pnlMain.Controls.Add(lblTitle);

        Controls.Add(pnlMain);
    }

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

    private static AntdUI.Panel CreateInfoRow(string labelText, string valueText)
    {
        var pnl = new AntdUI.Panel
        {
            Dock = DockStyle.Top,
            Height = 35,
            Radius = 0,
            Back = Color.White
        };
        var lblVal = new AntdUI.Label
        {
            Text = valueText,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        var lblKey = new AntdUI.Label
        {
            Text = labelText,
            Dock = DockStyle.Left,
            Width = 120,
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleLeft
        };

        pnl.Controls.Add(lblVal);
        pnl.Controls.Add(lblKey);
        return pnl;
    }

    private static AntdUI.Panel CreateInputRow(string labelText, out AntdUI.Input txt)
    {
        var pnl = new AntdUI.Panel
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 5, 0, 5),
            Radius = 0,
            Back = Color.White
        };
        txt = new AntdUI.Input
        {
            Dock = DockStyle.Bottom,
            Font = new Font("Segoe UI", 12),
            UseSystemPasswordChar = true
        };
        var lblKey = new AntdUI.Label
        {
            Text = labelText,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.DimGray
        };

        pnl.Controls.Add(txt);
        pnl.Controls.Add(lblKey);
        return pnl;
    }

    private static string FormatRole(UserRole? role)
        => role switch
        {
            UserRole.Admin => "Admin",
            UserRole.Cashier => "Thu ngân",
            _ => "N/A"
        };
}
