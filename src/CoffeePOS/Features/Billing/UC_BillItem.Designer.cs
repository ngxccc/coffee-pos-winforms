using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillItem
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Avatar _picFood = null!;
    private AntdUI.Label _lblCount = null!;
    private AntdUI.Label _lblPrice = null!;
    private AntdUI.Label _lblNote = null!;
    private AntdUI.Label _lblName = null!;
    private AntdUI.Button _btnMinus = null!;
    private AntdUI.Button _btnPlus = null!;
    private AntdUI.Button _btnDelete = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_picFood?.Image != null) _picFood.Image.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Size = new Size(400, 90);
        MinimumSize = new Size(400, 90);
        BackColor = UiTheme.Surface;
        Margin = new Padding(0, 0, 0, 10);

        SuspendLayout();

        // CHIA LƯỚI 4 CỘT: Ảnh(90) | Qty(90) | Info(Fill) | Tiền & Xóa(80)
        var mainGrid = new AntdUI.GridPanel
        {
            Dock = DockStyle.Fill,
            Span = "90 90 100% 90",
            Gap = 10,
            Padding = new Padding(5)
        };

        _picFood = new AntdUI.Avatar
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Cursor = Cursors.Hand
        };

        var pnlQty = new AntdUI.StackPanel
        {
            Dock = DockStyle.Fill,
            Vertical = false,
            Gap = 4
        };

        _btnMinus = new AntdUI.Button { Text = "-", Type = TTypeMini.Default, Size = new Size(24, 30), Radius = 4, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
        _lblCount = new AntdUI.Label { Text = "1", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = UiTheme.TextPrimary, TextAlign = ContentAlignment.MiddleCenter, Width = 30 };
        _btnPlus = new AntdUI.Button { Text = "+", Type = TTypeMini.Default, Size = new Size(24, 30), Radius = 4, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 12, FontStyle.Bold) };


        var pnlInfo = new AntdUI.StackPanel
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Cursor = Cursors.Hand
        };

        _lblName = new AntdUI.Label { Height = 25, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = UiTheme.BrandPrimary };
        _lblNote = new AntdUI.Label { Height = 40, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray };


        // --- CỘT 4: XÓA & GIÁ TIỀN ---
        var pnlRight = new AntdUI.StackPanel
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Gap = 5
        };

        _btnDelete = new AntdUI.Button { Text = "X", Type = TTypeMini.Error, Size = new Size(30, 30), Radius = 6, Cursor = Cursors.Hand };
        _lblPrice = new AntdUI.Label { Height = 25, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(64, 64, 64), TextAlign = ContentAlignment.MiddleRight };

        pnlQty.Controls.Add(_btnPlus);
        pnlQty.Controls.Add(_lblCount);
        pnlQty.Controls.Add(_btnMinus);
        pnlInfo.Controls.Add(_lblName);
        pnlInfo.Controls.Add(_lblNote);
        pnlRight.Controls.Add(_btnDelete);
        pnlRight.Controls.Add(_lblPrice);

        mainGrid.Controls.Add(pnlRight);
        mainGrid.Controls.Add(pnlInfo);
        mainGrid.Controls.Add(pnlQty);
        mainGrid.Controls.Add(_picFood);

        Controls.Add(mainGrid);
        ResumeLayout(false);
    }
}
