using CoffeePOS.Core;
using CoffeePOS.Services;

namespace CoffeePOS.Forms;

public partial class SettingForm : Form
{
    private readonly IUserSession _session;
    private readonly IUserService _userService;

    private TextBox txtOldPass = null!;
    private TextBox txtNewPass = null!;
    private TextBox txtConfirmPass = null!;
    private Button btnSave = null!;

    public SettingForm(IUserSession session, IUserService userService)
    {
        _session = session;
        _userService = userService;

        InitializeUI();

        AcceptButton = btnSave;
    }

    private void InitializeUI()
    {
        Text = "Cài đặt Tài khoản";
        Size = new Size(450, 550);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        Panel pnlMain = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30)
        };

        Label lblTitle = new()
        {
            Text = "THÔNG TIN CÁ NHÂN",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Top,
            Height = 40
        };

        var pnlRole = CreateInfoRow("Vai trò:", _session.CurrentUser?.Role == 0 ? "Admin" : "Thu ngân");
        var pnlFullName = CreateInfoRow("Họ tên:", _session.CurrentUser?.FullName ?? "N/A");
        var pnlUsername = CreateInfoRow("Tài khoản:", _session.CurrentUser?.Username ?? "N/A");

        Panel pnlDivider = new()
        {
            Dock = DockStyle.Top,
            Height = 30
        };
        Label lblLine = new()
        {
            Dock = DockStyle.Bottom,
            Height = 2,
            BackColor = Color.LightGray
        };
        pnlDivider.Controls.Add(lblLine);

        Label lblPassTitle = new()
        {
            Text = "ĐỔI MẬT KHẨU",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60),
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.BottomLeft
        };

        var pnlConfirmPass = CreateInputRow("Xác nhận mật khẩu mới:", out txtConfirmPass);
        var pnlNewPass = CreateInputRow("Mật khẩu mới:", out txtNewPass);
        var pnlOldPass = CreateInputRow("Mật khẩu hiện tại:", out txtOldPass);

        pnlOldPass.TabIndex = 1;
        pnlNewPass.TabIndex = 2;
        pnlConfirmPass.TabIndex = 3;

        Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 60,
            Padding = new Padding(0, 20, 0, 0)
        };
        btnSave = new Button
        {
            Text = "CẬP NHẬT MẬT KHẨU",
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += async (s, e) => await ChangePasswordAsync();
        pnlFooter.Controls.Add(btnSave);

        // Thằng nào add trước sẽ nằm trên cùng
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
        Controls.Add(pnlFooter);

        pnlFooter.Padding = new Padding(30, 0, 30, 30);
    }

    private static Panel CreateInfoRow(string labelText, string valueText)
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Top,
            Height = 35
        };
        Label lblVal = new()
        {
            Text = valueText,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        Label lblKey = new()
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

    private static Panel CreateInputRow(string labelText, out TextBox txt)
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Top,
            Height = 60,
            Padding = new Padding(0, 5, 0, 5)
        };
        txt = new TextBox
        {
            Dock = DockStyle.Bottom,
            Font = new Font("Segoe UI", 12),
            PasswordChar = '●'
        };
        Label lblKey = new()
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

    private async Task ChangePasswordAsync()
    {
        string oldPass = txtOldPass.Text;
        string newPass = txtNewPass.Text;
        string confirmPass = txtConfirmPass.Text;

        btnSave.Enabled = false;
        btnSave.Text = "ĐANG XỬ LÝ...";

        try
        {
            await _userService.ChangePasswordAsync(
                _session.CurrentUser!.Id,
                _session.CurrentUser.Username,
                oldPass,
                newPass,
                confirmPass);

            MessageBox.Show("Đổi mật khẩu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
        }
        finally
        {
            btnSave.Enabled = true;
            btnSave.Text = "CẬP NHẬT MẬT KHẨU";
        }
    }
}
