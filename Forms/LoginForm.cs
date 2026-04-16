using CoffeePOS.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public class LoginForm : AntdUI.Window
{
    private readonly IUserService _userService;
    private readonly IUserSession _session;

    private AntdUI.Input txtUsername = null!;
    private AntdUI.Input txtPassword = null!;
    private AntdUI.Button btnLogin = null!;
    private AntdUI.PageHeader? windowBar = null!;

    public LoginForm(IUserService userService, IUserSession session)
    {
        _userService = userService;
        _session = session;

        InitializeUI();

        btnLogin.Click += async (s, e) => await BtnLogin_Click(s, e);
        AcceptButton = btnLogin;
    }

    private void InitializeUI()
    {
        Text = "CoffeePOS - Đăng Nhập";
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        ClientSize = new Size(400, 400);
        windowBar?.SuspendLayout();
        SuspendLayout();

        // PageHeader Bar
        windowBar = new AntdUI.PageHeader
        {
            Dock = DockStyle.Top,
            Height = 40,
            Text = "CoffeePOS",
            SubText = "Đăng Nhập Hệ Thống",
            ShowButton = true,
            ShowIcon = false,
            DividerShow = true,
            Location = new Point(0, 0),
        };

        AntdUI.Panel pnlInputs = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 20, 40, 20),
            BackColor = Color.White
        };

        AntdUI.Label lblUser = new()
        {
            Text = "Tên đăng nhập:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
        };

        txtUsername = new AntdUI.Input
        {
            Font = new Font("Segoe UI", 14),
            Dock = DockStyle.Top,
            Height = 44,
            Margin = new Padding(0, 0, 0, 20),
            PlaceholderText = "Nhập tên đăng nhập",
            AllowClear = true,
            TabIndex = 0
        };

        AntdUI.Panel spacer1 = new() { Dock = DockStyle.Top, Height = 20, Back = Color.White };

        AntdUI.Label lblPass = new()
        {
            Text = "Mật khẩu:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = Color.FromArgb(31, 30, 68)
        };

        txtPassword = new AntdUI.Input
        {
            Font = new Font("Segoe UI", 14),
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu",
            Dock = DockStyle.Top,
            Height = 44,
            TabIndex = 1
        };

        AntdUI.Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 120,
            Padding = new Padding(40, 20, 40, 40),
            BackColor = Color.White
        };

        btnLogin = new AntdUI.Button
        {
            Text = "ĐĂNG NHẬP",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Type = AntdUI.TTypeMini.Primary,
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand
        };

        pnlInputs.Controls.Add(txtPassword);
        pnlInputs.Controls.Add(lblPass);
        pnlInputs.Controls.Add(spacer1);
        pnlInputs.Controls.Add(txtUsername);
        pnlInputs.Controls.Add(lblUser);

        pnlFooter.Controls.Add(btnLogin);

        Controls.Add(pnlInputs);  // Fill ở giữa
        Controls.Add(windowBar);  // Top
        Controls.Add(pnlFooter);  // Bottom
        windowBar.ResumeLayout(false);
        ResumeLayout(false);
    }

    private async Task BtnLogin_Click(object? sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBoxHelper.Warning("Vui lòng nhập đủ Username và Password!", owner: this);
            return;
        }

        btnLogin.Enabled = false;
        btnLogin.Text = "Đang xử lý...";

        try
        {
            var user = await _userService.AuthenticateAsync(username, password);

            if (user != null)
            {
                _session.Login(user);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBoxHelper.Warning("Sai tài khoản hoặc mật khẩu!", owner: this);
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Tài khoản bị khóa", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi kết nối CSDL: {ex.Message}", owner: this);
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text = "ĐĂNG NHẬP";
        }
    }
}
