using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_Menu
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Panel _pnlHeader = null!;
    private AntdUI.Menu _menuCategories = null!;
    private AntdUI.Input _txtSearch = null!;
    private AntdUI.Panel _pnlBody = null!;
    private FlowLayoutPanel _flowProducts = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        _pnlHeader = new()
        {
            Dock = DockStyle.Top,
            Height = 55,
            Padding = new Padding(20, 5, 20, 5),
        };

        _txtSearch = new()
        {
            Dock = DockStyle.Right,
            Width = 250,
            PlaceholderText = "Tìm món...",
            AllowClear = true,
            Radius = 8,
            Margin = new Padding(10, 15, 0, 15)
        };

        _menuCategories = new()
        {
            Dock = DockStyle.Fill,
            Mode = TMenuMode.Horizontal,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeActive = UiTheme.BrandPrimary,
            Radius = 10
        };

        AntdUI.Divider divider1 = new()
        {
            Dock = DockStyle.Top,
            Size = new Size(1000, 1),
            Thickness = 1F,
            Margin = new Padding(0)
        };

        _pnlHeader.Controls.Add(_menuCategories);
        _pnlHeader.Controls.Add(_txtSearch);

        _pnlBody = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            Back = Color.Transparent
        };

        _flowProducts = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        _pnlBody.Controls.Add(_flowProducts);

        Controls.Add(_pnlBody);
        Controls.Add(divider1);
        Controls.Add(_pnlHeader);
    }
}
