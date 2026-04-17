using AntdUI;
namespace CoffeePOS.Features.Sidebar;

partial class UC_Sidebar
{
    private System.ComponentModel.IContainer components = null!;
    private AntdUI.Menu _menuMain = null!;
    private AntdUI.Panel _panelHeader = null!;
    private AntdUI.Label _lblTitle = null!;

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
        AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        BackColor = System.Drawing.Color.White;
        Margin = new System.Windows.Forms.Padding(0);
        Name = "UC_Sidebar";
        Size = new System.Drawing.Size(200, 800);
        Dock = DockStyle.Left;
        Padding = new Padding(5);
        SuspendLayout();

        _panelHeader = new AntdUI.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Top,
            Location = new System.Drawing.Point(0, 0),
            Name = "panelHeader",
            Size = new System.Drawing.Size(260, 80),
            TabIndex = 0,
        };
        _panelHeader.SuspendLayout();

        _lblTitle = new AntdUI.Label
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold),
            Location = new System.Drawing.Point(0, 0),
            Name = "lblTitle",
            Size = new System.Drawing.Size(260, 80),
            TabIndex = 0,
            Text = "EPOS",
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
        };

        _menuMain = new Menu
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Font = new System.Drawing.Font("Segoe UI", 11F),
            Location = new System.Drawing.Point(0, 80),
            Name = "menuMain",
            Size = new System.Drawing.Size(260, 720),
            TabIndex = 1
        };

        _panelHeader.Controls.Add(_lblTitle);
        Controls.Add(_menuMain);
        Controls.Add(_panelHeader);

        _panelHeader.ResumeLayout(false);
        ResumeLayout(false);
    }
}
