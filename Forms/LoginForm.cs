using CoffeePOS.Core;
using CoffeePOS.Data.Repositories;

namespace CoffeePOS.Forms;

public partial class LoginForm : Form
{
    private readonly IUserRepository _userRepo;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSession _session;

    private TextBox txtUsername = null!;
    private TextBox txtPassword = null!;
    private Button btnLogin = null!;

    public LoginForm(IUserRepository userRepo, IServiceProvider serviceProvider, IUserSession session)
    {
        _userRepo = userRepo;
        _serviceProvider = serviceProvider;
        _session = session;

        InitializeUI();

        btnLogin.Click += async (s, e) => await btnLogin_Click(s, e);
        AcceptButton = btnLogin;
    }

    private void InitializeUI()
    {
        Text = "CoffeePOS - Đăng Nhập";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 450);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        Label lblTitle = new()
        {
            Text = "ĐĂNG NHẬP",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Top,
            Height = 120,
            TextAlign = ContentAlignment.MiddleCenter
        };

        Panel pnlInputs = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 20, 40, 20)
        };

        Label lblUser = new()
        {
            Text = "Tên đăng nhập:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
        };

        txtUsername = new TextBox
        {
            Font = new Font("Segoe UI", 14),
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 20),
            TabIndex = 0
        };

        Panel spacer1 = new() { Dock = DockStyle.Top, Height = 20 };

        Label lblPass = new()
        {
            Text = "Mật khẩu:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft
        };

        txtPassword = new TextBox
        {
            Font = new Font("Segoe UI", 14),
            PasswordChar = '●',
            Dock = DockStyle.Top,
            TabIndex = 1
        };

        Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 120,
            Padding = new Padding(40, 20, 40, 40)
        };

        btnLogin = new Button
        {
            Text = "ĐĂNG NHẬP",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(0, 122, 204),
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand
        };
        btnLogin.FlatAppearance.BorderSize = 0;

        pnlInputs.Controls.Add(txtPassword);
        pnlInputs.Controls.Add(lblPass);
        pnlInputs.Controls.Add(spacer1);
        pnlInputs.Controls.Add(txtUsername);
        pnlInputs.Controls.Add(lblUser);

        pnlFooter.Controls.Add(btnLogin);

        Controls.Add(pnlInputs);  // Fill ở giữa
        Controls.Add(lblTitle);   // Top
        Controls.Add(pnlFooter);  // Bottom
    }

    private async Task btnLogin_Click(object? sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Vui lòng nhập đủ Username và Password!");
            return;
        }

        btnLogin.Enabled = false;
        btnLogin.Text = "Đang xử lý...";

        try
        {
            var user = await _userRepo.AuthenticateAsync(username, password);

            if (user != null)
            {
                _session.Login(user);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi kết nối CSDL: {ex.Message}");
        }
        finally
        {
            btnLogin.Enabled = true;
            btnLogin.Text = "ĐĂNG NHẬP";
        }
    }
}
