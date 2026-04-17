using AntdUI;

namespace CoffeePOS.Forms;

public partial class CashierWorkspaceForm
{
    // UI Structural Components
    private PageHeader _windowBar = null!;
    private AntdUI.Label _lblUserInfo = null!;
    private LabelTime _lblTime = null!;

    private void InitializeComponent()
    {
        Text = "CoffeePOS";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;

        _windowBar = new PageHeader
        {
            Dock = DockStyle.Top,
            Height = 40,
            Text = "CoffeePOS",
            ShowButton = true,
            ShowIcon = false,
            DividerShow = true,
        };

        _lblUserInfo = new AntdUI.Label
        {
            Dock = DockStyle.Right,
            Width = 260,
            ForeColor = Color.FromArgb(0, 122, 204),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 8, 0)
        };

        _lblTime = new LabelTime
        {
            Dock = DockStyle.Right,
            Width = 150,
            ShowTime = true,
            AutoWidth = false,
            DragMove = false,
            ForeColor = Color.FromArgb(0, 122, 204),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(0, 2, 8, 2)
        };

        _windowBar.Controls.Add(_lblUserInfo);
        _windowBar.Controls.Add(_lblTime);
    }

    // PERF: Centralized method to stitch together DI-provided UserControls
    private void AssembleLayout(Control sidebar, Control billing, Control history, Control menu)
    {
        SuspendLayout();

        // WHY: Reverse Z-Order mapping for standard WinForms layout resolution
        // Adding Top/Left/Right first ensures Fill objects fit perfectly in the remaining center void.
        Controls.Add(menu);         // Center Fill (Active)
        Controls.Add(history);      // Center Fill (Hidden by default)
        Controls.Add(billing);      // Right Dock
        Controls.Add(sidebar);      // Left Dock
        Controls.Add(_windowBar);   // Top Dock

        menu.BringToFront();

        ResumeLayout(false);
    }
}
