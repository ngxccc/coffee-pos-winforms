using System.Drawing;
using System.Windows.Forms;

namespace CoffeePOS.Features.Admin.Controls;

partial class UC_ProductEditor
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Input _txtName = null!;
    private NumericUpDown _nudPrice = null!;
    private ComboBox _cboCategory = null!;
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
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.White;
        Size = new Size(450, 400);

        AntdUI.Label lblName = new()
        {
            Text = "Tên món",
            Location = new Point(20, 20),
            AutoSize = true
        };
        AntdUI.Label lblPrice = new()
        {
            Text = "Giá bán (VNĐ)",
            Location = new Point(20, 85),
            AutoSize = true
        };
        AntdUI.Label lblCategory = new()
        {
            Text = "Danh mục",
            Location = new Point(240, 85),
            AutoSize = true
        };
        AntdUI.Label lblImage = new()
        {
            Text = "Link Hình Ảnh (URL) hoặc Upload",
            Location = new Point(20, 150),
            AutoSize = true
        };

        SuspendLayout();

        _txtName = new()
        {
            Location = new Point(20, 45),
            Size = new Size(420, 38),
            Font = new Font("Segoe UI", 11F),
            PlaceholderText = "Nhập tên món",
            AllowClear = true,
        };

        _nudPrice = new()
        {
            Location = new Point(20, 110),
            Size = new Size(200, 32),
            Font = new Font("Segoe UI", 11F),
            Maximum = 10000000m,
            Increment = 1000m,
            ThousandsSeparator = true,
        };

        _cboCategory = new()
        {
            Location = new Point(240, 110),
            Size = new Size(200, 32),
            Font = new Font("Segoe UI", 11F),
            DropDownStyle = ComboBoxStyle.DropDownList,
        };

        _txtImageUrl = new()
        {
            Location = new Point(20, 175),
            Size = new Size(340, 38),
            Font = new Font("Segoe UI", 11F),
            PlaceholderText = "https://i.imgur.com/...",
            AllowClear = true,
        };

        _btnChooseImage = new()
        {
            Text = "Up ảnh",
            Location = new Point(370, 175),
            Size = new Size(70, 38),
            Type = AntdUI.TTypeMini.Primary,
            Cursor = Cursors.Hand,
        };

        _picImage = new()
        {
            Location = new Point(20, 220),
            Size = new Size(150, 150),
        };

        Controls.Add(lblName);
        Controls.Add(_txtName);
        Controls.Add(lblPrice);
        Controls.Add(_nudPrice);
        Controls.Add(lblCategory);
        Controls.Add(_cboCategory);
        Controls.Add(lblImage);
        Controls.Add(_txtImageUrl);
        Controls.Add(_btnChooseImage);
        Controls.Add(_picImage);

        ResumeLayout(false);
    }
}
