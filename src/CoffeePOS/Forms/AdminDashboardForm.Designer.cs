using System.Drawing;
using System.Windows.Forms;
using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public partial class AdminDashboardForm
{
    private System.ComponentModel.IContainer components = null!;

    // UI COMPONENTS
    private PageHeader windowBar = null!;
    private AntdUI.Panel pnlContent = null!;
    private AntdUI.Label lblPlaceholder = null!;
    private AntdUI.Label lblUserInfo = null!;
    private LabelTime lblTime = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        Text = "Hệ Thống Quản Trị - CoffeePOS Admin";
        ClientSize = new Size(1366, 768);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Surface;

        windowBar = new PageHeader
        {
            Dock = DockStyle.Top,
            Height = 40,
            Text = "CoffeePOS Admin",
            ShowButton = true,
            ShowIcon = false,
            DividerShow = true,
            BackColor = UiTheme.Surface
        };

        lblUserInfo = new AntdUI.Label
        {
            Dock = DockStyle.Right,
            Width = 260,
            ForeColor = UiTheme.BrandPrimary,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 8, 0)
        };

        lblTime = new LabelTime
        {
            Dock = DockStyle.Right,
            Width = 150,
            ShowTime = true,
            AutoWidth = false,
            DragMove = false,
            ForeColor = UiTheme.BrandPrimary,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(0, 2, 8, 2)
        };

        windowBar.Controls.Add(lblUserInfo);
        windowBar.Controls.Add(lblTime);

        pnlContent = new AntdUI.Panel
        {
            Dock = DockStyle.Fill,
            Radius = 0,
            Back = UiTheme.Surface,
            Padding = new Padding(20)
        };
    }

    private void AssembleLayout(Control sidebar)
    {
        SuspendLayout();

        // Z-Order: Content (Nằm giữa) -> Sidebar (Bám trái) -> WindowBar (Phủ đỉnh)
        Controls.Add(pnlContent);
        Controls.Add(sidebar);
        Controls.Add(windowBar);

        ResumeLayout(false);
    }
}
