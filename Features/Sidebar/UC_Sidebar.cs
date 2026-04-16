using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public class UC_Sidebar : UserControl
{
    private const int CollapsedWidth = 72;
    private const int ExpandedWidth = 220;

    public event EventHandler? OnHomeClicked;
    public event EventHandler? OnBillHistoryClicked;
    public event EventHandler? OnSettingsClicked;
    public event EventHandler? OnLogoutClicked;

    private readonly AntdUI.Button _btnCollapse;
    private readonly AntdUI.Button _btnLogout;
    private readonly AntdUI.Menu _menu;

    public UC_Sidebar()
    {
        _btnCollapse = new AntdUI.Button
        {
            Dock = DockStyle.Top,
            Height = 56,
            Type = AntdUI.TTypeMini.Default,
            Radius = 0,
            Cursor = Cursors.Hand,
            IconSvg = "MenuUnfoldOutlined",
            ToggleIconSvg = "MenuFoldOutlined",
            ForeColor = UiTheme.SidebarText,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            WaveSize = 0
        };

        _btnLogout = new AntdUI.Button
        {
            Dock = DockStyle.Bottom,
            Height = 64,
            Type = AntdUI.TTypeMini.Default,
            Radius = 0,
            Cursor = Cursors.Hand,
            IconSvg = "LogoutOutlined",
            ForeColor = UiTheme.SidebarText,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            WaveSize = 0
        };

        _menu = new AntdUI.Menu
        {
            Dock = DockStyle.Fill,
            Radius = 0,
            Round = false,
            ForeColor = UiTheme.SidebarText,
            ForeActive = UiTheme.SidebarTextActive,
            BackHover = UiTheme.SidebarHover,
            BackActive = UiTheme.SidebarHover,
            Collapsed = true,
            AutoCollapse = false
        };

        InitializeUI();
    }

    private void InitializeUI()
    {
        SuspendLayout();

        Width = (int)(CollapsedWidth * AntdUI.Config.Dpi);
        Dock = DockStyle.Left;

        _btnCollapse.Click += ButtonCollapse_Click;
        _btnLogout.Click += (_, _) => OnLogoutClicked?.Invoke(this, EventArgs.Empty);
        _menu.SelectChanged += Menu_SelectChanged;

        _menu.Items.Add(new AntdUI.MenuItem
        {
            Text = "Home",
            IconSvg = "HomeOutlined",
            Tag = "home"
        });

        _menu.Items.Add(new AntdUI.MenuItem
        {
            Text = "Bills",
            IconSvg = "FileTextOutlined",
            Tag = "bills"
        });

        _menu.Items.Add(new AntdUI.MenuItem
        {
            Text = "Settings",
            IconSvg = "SettingOutlined",
            Tag = "settings"
        });

        Controls.Add(_menu);
        Controls.Add(_btnLogout);
        Controls.Add(_btnCollapse);

        ApplyCollapsedVisualState();
        _menu.SelectIndex(0, false);

        ResumeLayout(false);
    }

    private void ButtonCollapse_Click(object? sender, EventArgs e)
    {
        if (_menu.Collapsed)
        {
            Width = (int)(ExpandedWidth * AntdUI.Config.Dpi);
        }
        else
        {
            Width = (int)(CollapsedWidth * AntdUI.Config.Dpi);
        }

        _btnCollapse.Toggle = !_btnCollapse.Toggle;
        _menu.Collapsed = !_menu.Collapsed;

        if (_menu.Collapsed)
        {
            ApplyCollapsedVisualState();
        }
        else
        {
            ApplyExpandedVisualState();
        }
    }

    private void ApplyCollapsedVisualState()
    {
        _btnCollapse.Text = string.Empty;
        _btnCollapse.TextAlign = ContentAlignment.MiddleCenter;
        _btnCollapse.Padding = Padding.Empty;

        _btnLogout.Text = string.Empty;
        _btnLogout.TextAlign = ContentAlignment.MiddleCenter;
        _btnLogout.Padding = Padding.Empty;
    }

    private void ApplyExpandedVisualState()
    {
        _btnCollapse.Text = "  Thu gon";
        _btnCollapse.TextAlign = ContentAlignment.MiddleLeft;
        _btnCollapse.Padding = new Padding(14, 0, 0, 0);

        _btnLogout.Text = "  Logout";
        _btnLogout.TextAlign = ContentAlignment.MiddleLeft;
        _btnLogout.Padding = new Padding(14, 0, 0, 0);
    }

    private void Menu_SelectChanged(object? sender, AntdUI.MenuSelectEventArgs e)
    {
        string tag = e.Value.Tag?.ToString() ?? string.Empty;
        switch (tag)
        {
            case "home":
                OnHomeClicked?.Invoke(this, EventArgs.Empty);
                break;
            case "bills":
                OnBillHistoryClicked?.Invoke(this, EventArgs.Empty);
                break;
            case "settings":
                OnSettingsClicked?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
