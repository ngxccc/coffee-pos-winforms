using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillItem
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Avatar _picFood = null!;
    private AntdUI.InputNumber _numQty = null!;
    private AntdUI.Label _lblPrice = null!;
    private AntdUI.Label _lblNote = null!;
    private AntdUI.Label _lblName = null!;
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

        AntdUI.GridPanel mainGrid = new()
        {
            Dock = DockStyle.Fill,
            Span = "100 80 100 80",
            Gap = 2,
        };
        mainGrid.SuspendLayout();

        _picFood = new AntdUI.Avatar
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Cursor = Cursors.Hand,
        };

        _numQty = new AntdUI.InputNumber
        {
            Dock = DockStyle.Fill,
            Minimum = 1,
            Maximum = 999,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Height = 20,
        };

        AntdUI.StackPanel pnlInfo = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Cursor = Cursors.Hand
        };
        pnlInfo.SuspendLayout();

        _lblName = new AntdUI.Label
        {
            Height = 25,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary
        };
        _lblNote = new AntdUI.Label
        {
            Height = 40,
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            ForeColor = Color.Gray
        };

        AntdUI.StackPanel pnlRight = new()
        {
            Dock = DockStyle.Fill,
            Vertical = true,
            Gap = 5
        };
        pnlRight.SuspendLayout();

        _btnDelete = new AntdUI.Button
        {
            Text = "X",
            Type = TTypeMini.Error,
            Size = new Size(30, 30),
            Radius = 6,
            Cursor = Cursors.Hand
        };
        _lblPrice = new AntdUI.Label
        {
            Height = 25,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(64, 64, 64),
            TextAlign = ContentAlignment.MiddleRight
        };

        pnlInfo.Controls.Add(_lblNote);
        pnlInfo.Controls.Add(_lblName);

        pnlRight.Controls.Add(_btnDelete);
        pnlRight.Controls.Add(_lblPrice);

        mainGrid.Controls.Add(pnlRight);
        mainGrid.Controls.Add(pnlInfo);
        mainGrid.Controls.Add(_numQty);
        mainGrid.Controls.Add(_picFood);

        Controls.Add(mainGrid);

        pnlRight.ResumeLayout(false);
        pnlInfo.ResumeLayout(false);
        mainGrid.ResumeLayout(false);
        ResumeLayout(false);
    }
}
