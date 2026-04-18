using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public partial class LoginForm
{
    // PERF: Enforce strict nullability context to pass compiler checks
    private Input _txtUsername = null!;
    private Input _txtPassword = null!;
    private AntdUI.Button _btnLogin = null!;
    private PageHeader _windowBar = null!;

    private void InitializeComponent()
    {
        Text = "CoffeePOS - Đăng Nhập";
        ClientSize = new Size(400, 350);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Surface;

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
            MaximizeBox = false
        };

        // WHY: Deterministic 1D Matrix mapping. Replaces reverse Z-order Stack.
        TableLayoutPanel tlpInputs = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(40, 20, 40, 20),
        };
        tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        AntdUI.Label lblUser = new()
        {
            Text = "Tên đăng nhập:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            Dock = DockStyle.Fill,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(0, 0, 0, 5) // Tạo gap nhỏ giữa Label và Input
        };

        _txtUsername = new Input
        {
            Font = new Font("Segoe UI", 14),
            Dock = DockStyle.Fill,
            Height = 44,
            PlaceholderText = "Nhập tên đăng nhập",
            TabIndex = 1,
            Margin = new Padding(0, 0, 0, 10) // Tạo gap lớn giữa 2 cụm User/Pass
        };

        AntdUI.Label lblPass = new()
        {
            Text = "Mật khẩu:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            Dock = DockStyle.Fill,
            Height = 30,
            TextAlign = ContentAlignment.BottomLeft,
            Margin = new Padding(0, 0, 0, 5)
        };

        _txtPassword = new Input
        {
            Font = new Font("Segoe UI", 14),
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu",
            Dock = DockStyle.Fill,
            Height = 44,
            TabIndex = 2,
            Margin = new Padding(0)
        };

        // HACK: Explicit (Control, Column, Row) assignment. Readability restored.
        tlpInputs.Controls.Add(lblUser, 0, 0);
        tlpInputs.Controls.Add(_txtUsername, 0, 1);
        tlpInputs.Controls.Add(lblPass, 0, 2);
        tlpInputs.Controls.Add(_txtPassword, 0, 3);

        AntdUI.Panel pnlFooter = new()
        {
            Dock = DockStyle.Bottom,
            Height = 100,
            Padding = new Padding(40, 20, 40, 20),
            BackColor = Color.White
        };

        _btnLogin = new AntdUI.Button
        {
            Text = "ĐĂNG NHẬP",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Type = TTypeMini.Primary,
            Shape = TShape.Round,
            Dock = DockStyle.Fill,
            Cursor = Cursors.Hand,
            TabIndex = 3
        };
        _btnLogin.Click += HandleLoginAsync;

        pnlFooter.Controls.Add(_btnLogin);

        // Nối dây chuẩn thứ tự Z-Order
        Controls.Add(tlpInputs);
        Controls.Add(_windowBar);
        Controls.Add(pnlFooter);

        ResumeLayout(false);
    }
}
