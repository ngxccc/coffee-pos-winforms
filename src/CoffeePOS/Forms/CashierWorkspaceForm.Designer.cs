using AntdUI;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public partial class CashierWorkspaceForm
{
    // UI Structural Components
    private PageHeader _windowBar = null!;
    private AntdUI.Label _lblUserInfo = null!;
    private LabelTime _lblTime = null!;
    private AntdUI.Splitter _mainSplitter = null!;

    private void InitializeComponent()
    {
        Text = "CoffeePOS";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Surface;

        _windowBar = new PageHeader
        {
            Dock = DockStyle.Top,
            Height = 40,
            Text = "CoffeePOS",
            ShowButton = true,
            ShowIcon = false,
            DividerShow = true,
            BackColor = UiTheme.Surface
        };

        _lblUserInfo = new AntdUI.Label
        {
            Dock = DockStyle.Right,
            Width = 260,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            BackColor = Color.Transparent,
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
            ForeColor = UiTheme.BrandPrimary,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 8, 2)
        };

        _mainSplitter = new AntdUI.Splitter
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            FixedPanel = FixedPanel.Panel2,
            SplitterWidth = 10,
            Lazy = true,
            Panel1MinSize = 0,
            Panel2MinSize = 0,
            CollapsePanel = AntdUI.Splitter.ADCollapsePanel.Panel2,
            SplitterDistance = 860,
            SplitPanelState = false,
            SplitterSize = 60,
        };

        _windowBar.Controls.Add(_lblUserInfo);
        _windowBar.Controls.Add(_lblTime);
    }

    private void AssembleLayout(Control sidebar, Control billing, Control history, Control menu)
    {
        SuspendLayout();

        menu.Dock = DockStyle.Fill;
        history.Dock = DockStyle.Fill;
        billing.Dock = DockStyle.Fill;

        _mainSplitter.Panel1.Controls.Add(menu);
        _mainSplitter.Panel1.Controls.Add(history);
        _mainSplitter.Panel2.Controls.Add(billing);

        Controls.Add(_mainSplitter);
        Controls.Add(sidebar);
        Controls.Add(_windowBar);

        menu.BringToFront();

        ResumeLayout(false);
    }
}
