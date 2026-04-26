using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_ProductCustomization
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Segmented _segSize = null!;
    private AntdUI.Segmented _segOrderType = null!;
    private AntdUI.InputNumber _numQuantity = null!;
    private AntdUI.Label _lblTotalPrice = null!;
    private AntdUI.Table _tableToppings = null!;
    private AntdUI.SelectMultiple _cboNote = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = UiTheme.Surface;
        Size = new Size(450, 650);

        AntdUI.StackPanel mainLayout = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 0, 20, 0),
            Gap = 2,
            Vertical = true
        };
        mainLayout.SuspendLayout();

        var lblSizeTitle = CreateSectionLabel("CHỌN SIZE");
        AntdUI.FlowPanel pnlSizes = new()
        {
            Gap = 10,
            Height = 50,
            Dock = DockStyle.Top
        };
        pnlSizes.SuspendLayout();

        _segSize = new()
        {
            Width = 350,
            Height = 40,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeActive = UiTheme.BrandPrimary,
            BackActive = UiTheme.SurfaceAlt,
            Radius = 8,
            Full = false,
            Gap = 10,
        };
        pnlSizes.Controls.Add(_segSize);

        var lblToppingTitle = CreateSectionLabel("THÊM TOPPING");

        _tableToppings = new()
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            EmptyText = "Không có topping",
            Bordered = false,
            Padding = new Padding(0),
            MultipleRows = true,
            Height = 250,
            MaximumSize = new Size(500, 400),
            AutoSizeColumnsMode = ColumnsMode.Fill
        };

        var lblOrderTypeTitle = CreateSectionLabel("HÌNH THỨC");
        _segOrderType = new AntdUI.Segmented
        {
            Height = 40,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeActive = UiTheme.BrandPrimary,
            BackActive = UiTheme.SurfaceAlt,
            Radius = 8,
            Full = true,
            Gap = 5,
            Padding = new Padding(0, 0, 0, 10),
        };

        var lblNoteTitle = CreateSectionLabel("GHI CHÚ");
        _cboNote = new AntdUI.SelectMultiple
        {
            PlaceholderText = "Chọn hoặc nhập ghi chú (VD: Ít đá...)",
            Dock = DockStyle.Top,
            Height = 40,
            AllowClear = true,
            Font = UiTheme.BodyFont,
            Margin = new Padding(0, 0, 0, 10),
        };
        _cboNote.Items.AddRange(new object[] {
            "Ít đá",
            "Không đá",
            "Nhiều đá",
            "Đá riêng",
            "Ít đường",
            "Không đường",
            "Nhiều đường",
            "Nóng (Không đá)"
        });

        var lblQtyTitle = CreateSectionLabel("SỐ LƯỢNG");
        _numQuantity = new AntdUI.InputNumber
        {
            Minimum = 1,
            Maximum = 99,
            Value = 1,
            Size = new Size(140, 45),
            Font = new Font("Segoe UI", 12F, FontStyle.Bold)
        };

        AntdUI.Divider divider1 = new()
        {
            Dock = DockStyle.Bottom,
            Thickness = 1F,
        };

        _lblTotalPrice = new()
        {
            Text = "Tổng: 0 đ",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Height = 30,
            TextAlign = ContentAlignment.MiddleRight
        };

        mainLayout.Controls.Add(_numQuantity);
        mainLayout.Controls.Add(lblQtyTitle);

        mainLayout.Controls.Add(_cboNote);
        mainLayout.Controls.Add(lblNoteTitle);

        mainLayout.Controls.Add(_segOrderType);
        mainLayout.Controls.Add(lblOrderTypeTitle);

        mainLayout.Controls.Add(_lblTotalPrice);

        mainLayout.Controls.Add(divider1);

        mainLayout.Controls.Add(_tableToppings);
        mainLayout.Controls.Add(lblToppingTitle);

        mainLayout.Controls.Add(pnlSizes);
        mainLayout.Controls.Add(lblSizeTitle);

        mainLayout.Controls.Add(new AntdUI.Divider());

        Controls.Add(mainLayout);

        pnlSizes.ResumeLayout(false);
        mainLayout.ResumeLayout(false);
        ResumeLayout(false);
    }

    private AntdUI.Label CreateSectionLabel(string text)
    {
        return new AntdUI.Label
        {
            Text = text,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Color.Gray,
            Height = 40,
            AutoSizeMode = TAutoSize.Height
        };
    }
}
