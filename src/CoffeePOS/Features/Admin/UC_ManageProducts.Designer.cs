using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

partial class UC_ManageProducts
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Table _tableProducts = null!;
    private UC_ProductsHeaderToolbar _toolbar = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Surface;

        _toolbar = new UC_ProductsHeaderToolbar { Dock = DockStyle.Top };

        _tableProducts = new AntdUI.Table
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Bordered = true,
            EmptyHeader = true,
        };

        var hostPanel = new AntdUI.Panel
        {
            Dock = DockStyle.Fill,
            Radius = 8,
            Back = UiTheme.Surface,
            Padding = new Padding(UiTheme.BlockGap)
        };
        hostPanel.Controls.Add(_tableProducts);

        Controls.Add(hostPanel);
        Controls.Add(_toolbar);
    }
}
