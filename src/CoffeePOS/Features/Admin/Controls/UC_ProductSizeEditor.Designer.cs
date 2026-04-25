using System.Drawing;
using System.Windows.Forms;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

partial class UC_ProductSizeEditor
{
    private System.ComponentModel.IContainer components = null!;
    private AntdUI.Select _cboSizeName = null!;
    private AntdUI.InputNumber _nudPriceAdjustment = null!;

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
        Size = new Size(300, 200);

        AntdUI.Label lblSize = new()
        {
            Text = "Tên Size (Kích cỡ)",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _cboSizeName = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            List = true,
            Margin = new Padding(0, 0, 0, 15)
        };

        AntdUI.Label lblPrice = new()
        {
            Text = "Giá điều chỉnh (VNĐ) - Có thể nhập âm",
            AutoSize = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 5)
        };
        _nudPriceAdjustment = new()
        {
            Height = 40,
            Font = UiTheme.BodyFont,
            Minimum = -1000000m, // Cho phép giá âm (VD: Size S rẻ hơn Size M mặc định 5k)
            Maximum = 1000000m,
            Increment = 1000m,
            ThousandsSeparator = true
        };

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Padding = new Padding(20)
        };

        mainLayout.Controls.Add(_nudPriceAdjustment);
        mainLayout.Controls.Add(lblPrice);
        mainLayout.Controls.Add(_cboSizeName);
        mainLayout.Controls.Add(lblSize);

        Controls.Add(mainLayout);
        ResumeLayout(false);
    }
}
