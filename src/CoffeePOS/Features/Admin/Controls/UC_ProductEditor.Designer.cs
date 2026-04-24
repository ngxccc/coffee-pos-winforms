using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

partial class UC_ProductEditor
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Input _txtName = null!;
    private AntdUI.InputNumber _nudPrice = null!;
    private AntdUI.Select _cboCategory = null!;
    private AntdUI.Input _txtImageUrl = null!;
    private AntdUI.Avatar _picImage = null!;
    private AntdUI.Button _btnChooseImage = null!;

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
        Size = new Size(450, 500);

        AntdUI.Label lblName = new()
        {
            Text = "Tên món",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _txtName = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Nhập tên món",
            AllowClear = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        AntdUI.Label lblPrice = new()
        {
            Text = "Giá bán (VNĐ)",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _nudPrice = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            Maximum = 10000000m,
            Increment = 1000m,
            ThousandsSeparator = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        AntdUI.Label lblCategory = new()
        {
            Text = "Danh mục",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _cboCategory = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            List = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        AntdUI.Label lblImage = new()
        {
            Text = "Link Hình Ảnh (URL) hoặc Upload",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _txtImageUrl = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            PlaceholderText = "https://i.imgur.com/...",
            AllowClear = true
        };
        _btnChooseImage = new()
        {
            Height = 40,
            Text = "Up ảnh",
            Font = UiTheme.BodyFont,
            Type = AntdUI.TTypeMini.Primary,
            Cursor = Cursors.Hand
        };

        _picImage = new()
        {
            Size = new Size(120, 120),
            Margin = new Padding(0, 10, 0, 0),
            ImageFit = AntdUI.TFit.Fill,
        };

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Padding = new Padding(10)
        };

        mainLayout.Controls.Add(_picImage);
        mainLayout.Controls.Add(_btnChooseImage);
        mainLayout.Controls.Add(_txtImageUrl);
        mainLayout.Controls.Add(lblImage);

        mainLayout.Controls.Add(_cboCategory);
        mainLayout.Controls.Add(lblCategory);

        mainLayout.Controls.Add(_nudPrice);
        mainLayout.Controls.Add(lblPrice);

        mainLayout.Controls.Add(_txtName);
        mainLayout.Controls.Add(lblName);

        Controls.Add(mainLayout);
        ResumeLayout(false);
    }
}
