using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_ProductItem
{
    private System.ComponentModel.IContainer components = null!;

    private AntdUI.Panel _pnlCard = null!;
    private AntdUI.Avatar _picImage = null!;
    private AntdUI.Label _lblName = null!;
    private AntdUI.Label _lblPrice = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Size = new Size(150, 210);
        Margin = new Padding(8);
        BackColor = Color.Transparent;

        SuspendLayout();

        _pnlCard = new AntdUI.Panel
        {
            Dock = DockStyle.Fill,
            Back = UiTheme.Surface,
            Radius = 12,
            Shadow = 6,
            ShadowColor = Color.FromArgb(20, 0, 0, 0),
            Padding = new Padding(10),
            Cursor = Cursors.Hand
        };
        _pnlCard.SuspendLayout();

        _picImage = new AntdUI.Avatar
        {
            Dock = DockStyle.Top,
            Height = 110,
            Radius = 8,
            Round = false,
            ImageFit = TFit.Cover,
            BackColor = UiTheme.SurfaceAlt,
        };

        _lblName = new AntdUI.Label
        {
            Text = "Tên sản phẩm",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            AutoEllipsis = true,
            Margin = new Padding(0, 8, 0, 0)
        };

        _lblPrice = new AntdUI.Label
        {
            Text = "0 đ",
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Dock = DockStyle.Bottom,
            Height = 25,
            TextAlign = ContentAlignment.MiddleCenter
        };

        _pnlCard.Controls.Add(_lblName);
        _pnlCard.Controls.Add(_picImage);
        _pnlCard.Controls.Add(_lblPrice);

        AttachHoverBorderEffect(_pnlCard, _pnlCard);

        Controls.Add(_pnlCard);

        _pnlCard.ResumeLayout(false);
        ResumeLayout(false);
    }

    private void AttachHoverBorderEffect(Control container, AntdUI.Panel targetCard)
    {
        container.MouseEnter += (s, e) =>
        {
            targetCard.BorderWidth = 1;
            targetCard.BorderColor = UiTheme.BrandPrimary;
        };

        container.MouseLeave += (s, e) =>
        {
            var mousePos = targetCard.PointToClient(Cursor.Position);
            if (!targetCard.ClientRectangle.Contains(mousePos))
            {
                targetCard.BorderWidth = 0;
            }
        };

        foreach (Control child in container.Controls)
        {
            AttachHoverBorderEffect(child, targetCard);
        }
    }
}
