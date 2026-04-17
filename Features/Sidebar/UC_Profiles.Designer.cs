using System.Drawing;
using System.Windows.Forms;

namespace CoffeePOS.Features.Sidebar;

partial class UC_Profiles
{
    private System.ComponentModel.IContainer components = null!;
    private AntdUI.Input _txtOldPass = null!;
    private AntdUI.Input _txtNewPass = null!;
    private AntdUI.Input _txtConfirmPass = null!;

    private AntdUI.Label _lblRoleValue = null!;
    private AntdUI.Label _lblFullNameValue = null!;
    private AntdUI.Label _lblUsernameValue = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Name = "UC_Profiles";
        Size = new Size(600, 800);

        AntdUI.Panel pnlMain = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30),
            Radius = 0,
            Back = Color.White
        };

        AntdUI.Divider divTitle = new()
        {
            Text = "THÔNG TIN CÁ NHÂN",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            ColorSplit = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Top,
            Height = 40,
            Margin = new Padding(0, 10, 0, 10)
        };

        // --- SECTION 1: THÔNG TIN (Matrix 3x2) ---
        TableLayoutPanel tlpInfoSection = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.White
        };
        tlpInfoSection.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F)); // Cột Key
        tlpInfoSection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // Cột Value

        AntdUI.Label lblRoleKey = new()
        {
            Text = "Vai trò:",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblRoleValue = new AntdUI.Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 40
        };

        AntdUI.Label lblFullNameKey = new()
        {
            Text = "Họ tên:",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblFullNameValue = new AntdUI.Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 40
        };

        AntdUI.Label lblUsernameKey = new()
        {
            Text = "Tài khoản:",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _lblUsernameValue = new AntdUI.Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Height = 40
        };

        tlpInfoSection.Controls.Add(lblRoleKey, 0, 0);
        tlpInfoSection.Controls.Add(_lblRoleValue, 1, 0);
        tlpInfoSection.Controls.Add(lblFullNameKey, 0, 1);
        tlpInfoSection.Controls.Add(_lblFullNameValue, 1, 1);
        tlpInfoSection.Controls.Add(lblUsernameKey, 0, 2);
        tlpInfoSection.Controls.Add(_lblUsernameValue, 1, 2);

        AntdUI.Divider divider = new()
        {
            Dock = DockStyle.Top,
            Margin = new Padding(0, 20, 0, 20),
            Thickness = 1F,
            ColorSplit = Color.FromArgb(230, 230, 230)
        };

        // --- SECTION 2: MẬT KHẨU (Matrix 3x2) ---
        AntdUI.Divider divPassTitle = new()
        {
            Text = "ĐỔI MẬT KHẨU",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60),
            ColorSplit = Color.FromArgb(231, 76, 60),
            Dock = DockStyle.Top,
            Height = 40,
            Margin = new Padding(0, 10, 0, 10)
        };

        TableLayoutPanel tlpPassSection = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            BackColor = Color.White
        };
        tlpPassSection.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
        tlpPassSection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // PERF: Dùng RowStyles ép các dòng cao 50px để Input không bị dính vào nhau
        tlpPassSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        tlpPassSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        tlpPassSection.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

        AntdUI.Label lblOldPassKey = new()
        {
            Text = "Mật khẩu hiện tại:",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtOldPass = new AntdUI.Input
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12F),
            UseSystemPasswordChar = true,
            Margin = new Padding(0, 5, 0, 5)
        };

        AntdUI.Label lblNewPassKey = new()
        {
            Text = "Mật khẩu mới:",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtNewPass = new AntdUI.Input
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12F),
            UseSystemPasswordChar = true,
            Margin = new Padding(0, 5, 0, 5)
        };

        AntdUI.Label lblConfirmPassKey = new()
        {
            Text = "Xác nhận mật khẩu:",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.MiddleLeft
        };
        _txtConfirmPass = new AntdUI.Input
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12F),
            UseSystemPasswordChar = true,
            Margin = new Padding(0, 5, 0, 5)
        };

        tlpPassSection.Controls.Add(lblOldPassKey, 0, 0);
        tlpPassSection.Controls.Add(_txtOldPass, 1, 0);
        tlpPassSection.Controls.Add(lblNewPassKey, 0, 1);
        tlpPassSection.Controls.Add(_txtNewPass, 1, 1);
        tlpPassSection.Controls.Add(lblConfirmPassKey, 0, 2);
        tlpPassSection.Controls.Add(_txtConfirmPass, 1, 2);

        // HACK: Z-Order stacking for DockStyle.Top (Phải Add ngược từ dưới lên trên)
        pnlMain.Controls.Add(tlpPassSection);
        pnlMain.Controls.Add(divPassTitle);
        pnlMain.Controls.Add(divider);
        pnlMain.Controls.Add(tlpInfoSection);
        pnlMain.Controls.Add(divTitle);

        Controls.Add(pnlMain);
    }
}
