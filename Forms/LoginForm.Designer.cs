using AntdUI;

namespace CoffeePOS.Forms;

public partial class LoginForm
{
    private Input _txtUsername = null!;
    private Input _txtPassword = null!;
    private AntdUI.Button _btnLogin = null!;
    private PageHeader _windowBar = null!;

    private void InitializeComponent()
    {
        Text = "CoffeePOS - Đăng Nhập";
        ClientSize = new Size(400, 400);
        StartPosition = FormStartPosition.CenterScreen;

        SuspendLayout();

        _windowBar = new PageHeader
        {
            Dock = DockStyle.Top,
            Height = 40,
            Text = "CoffeePOS",
            SubText = "Đăng Nhập Hệ Thống",
            ShowButton = true,
            ShowIcon = false,
            DividerShow = true,
        };

        AntdUI.Panel pnlInputs = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(40, 30, 40, 20),
            BackColor = Color.White
        };

        AntdUI.Label lblUser = new()
        {
            Text = "Tên đăng nhập:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft
        };
        _txtUsername = new Input
        {
            Font = new Font("Segoe UI", 14),
            Dock = DockStyle.Top,
            Height = 44,
            Margin = new Padding(0, 0, 0, 20),
            PlaceholderText = "Nhập tên đăng nhập",
            AllowClear = true
        };

        AntdUI.Label lblPass = new()
        {
            Text = "Mật khẩu:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
            ForeColor = Color.FromArgb(31, 30, 68)
        };
        _txtPassword = new Input
        {
            Font = new Font("Segoe UI", 14),
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu",
            Dock = DockStyle.Top,
            Height = 44
        };

        // WHY: Dock.Top stacks controls in reverse order of addition.
        pnlInputs.Controls.Add(_txtPassword);
        pnlInputs.Controls.Add(lblPass);
        pnlInputs.Controls.Add(_txtUsername);
        pnlInputs.Controls.Add(lblUser);

        AntdUI.Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 100,
            Padding = new Padding(40, 0, 40, 40),
            BackColor = Color.White
        };

        _btnLogin = new AntdUI.Button
        {
            Text = "ĐĂNG NHẬP",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Type = TTypeMini.Primary,
            Shape = TShape.Round,
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand
        };
        _btnLogin.Click += HandleLoginAsync;

        pnlFooter.Controls.Add(_btnLogin);

        Controls.Add(pnlInputs);
        Controls.Add(_windowBar);
        Controls.Add(pnlFooter);

        ResumeLayout(false);
    }
}
