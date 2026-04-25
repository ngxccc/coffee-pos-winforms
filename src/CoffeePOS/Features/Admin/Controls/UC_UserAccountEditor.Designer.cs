using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

partial class UC_UserAccountEditor
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Input _txtUsername = null!;
    private AntdUI.Input _txtFullName = null!;
    private AntdUI.Select _cboRole = null!;
    private AntdUI.Label _lblPassword = null!;
    private AntdUI.Input _txtPassword = null!;
    private AntdUI.Input _txtConfirm = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Size = new Size(420, 450);

        AntdUI.Label lblUsername = new()
        {
            Text = "Tên đăng nhập",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _txtUsername = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Nhập tên đăng nhập",
            AllowClear = true,
            Margin = new Padding(0, 0, 0, 15),
            TabIndex = 1,
        };

        AntdUI.Label lblFullName = new()
        {
            Text = "Họ và tên",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _txtFullName = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Nhập họ tên",
            AllowClear = true,
            Margin = new Padding(0, 0, 0, 15),
            TabIndex = 2,
        };

        AntdUI.Label lblRole = new()
        {
            Text = "Vai trò",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _cboRole = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            List = true,
            Margin = new Padding(0, 0, 0, 15),
            TabIndex = 3,
        };

        _lblPassword = new()
        {
            Text = "Mật khẩu",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _txtPassword = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập mật khẩu",
            AllowClear = true,
            Margin = new Padding(0, 0, 0, 15),
            TabIndex = 4,
        };

        AntdUI.Label lblConfirm = new()
        {
            Text = "Xác nhận mật khẩu",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _txtConfirm = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            UseSystemPasswordChar = true,
            PlaceholderText = "Nhập lại mật khẩu",
            AllowClear = true,
            Margin = new Padding(0, 0, 0, 10),
            TabIndex = 5,
        };

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Padding = new Padding(20)
        };

        mainLayout.Controls.Add(_txtConfirm);
        mainLayout.Controls.Add(lblConfirm);
        mainLayout.Controls.Add(_txtPassword);
        mainLayout.Controls.Add(_lblPassword);
        mainLayout.Controls.Add(_cboRole);
        mainLayout.Controls.Add(lblRole);
        mainLayout.Controls.Add(_txtFullName);
        mainLayout.Controls.Add(lblFullName);
        mainLayout.Controls.Add(_txtUsername);
        mainLayout.Controls.Add(lblUsername);

        Controls.Add(mainLayout);
        ResumeLayout(false);
    }
}
