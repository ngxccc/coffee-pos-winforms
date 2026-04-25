using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

partial class UC_ToppingEditor
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Input _txtName = null!;
    private AntdUI.InputNumber _numPrice = null!;

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
        Size = new Size(400, 180);

        AntdUI.Label lblName = new()
        {
            Text = "Tên Topping",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };

        _txtName = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Nhập tên topping (VD: Trân châu trắng)",
            AllowClear = true
        };

        AntdUI.Label lblPrice = new()
        {
            Text = "Giá bán (đ)",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 10, 0, 5)
        };

        _numPrice = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            Minimum = 0,
            Maximum = 1000000,
            ThousandsSeparator = true
        };

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Padding = new Padding(10)
        };

        mainLayout.Controls.Add(_numPrice);
        mainLayout.Controls.Add(lblPrice);
        mainLayout.Controls.Add(_txtName);
        mainLayout.Controls.Add(lblName);

        Controls.Add(mainLayout);
        ResumeLayout(false);
    }
}
