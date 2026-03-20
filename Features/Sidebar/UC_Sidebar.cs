using FontAwesome.Sharp;

namespace CoffeePOS.Features.Sidebar;

public class UC_Sidebar : UserControl
{
    public event EventHandler? OnHomeClicked;
    public event EventHandler? OnBillHistoryClicked;
    public event EventHandler? OnSettingsClicked;
    public event EventHandler? OnLogoutClicked;

    public UC_Sidebar()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        Width = 80;
        Dock = DockStyle.Left;
        BackColor = Color.FromArgb(30, 30, 30);

        IconButton btnHome = CreateSidebarButton(IconChar.Home);
        btnHome.Dock = DockStyle.Top;
        btnHome.Click += (s, e) => OnHomeClicked?.Invoke(this, EventArgs.Empty);

        IconButton btnLogout = CreateSidebarButton(IconChar.SignOutAlt);
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Click += (s, e) => OnLogoutClicked?.Invoke(this, EventArgs.Empty);

        IconButton btnSettings = CreateSidebarButton(IconChar.Cog);
        btnSettings.Dock = DockStyle.Bottom;
        btnSettings.Click += (s, e) => OnSettingsClicked?.Invoke(this, EventArgs.Empty);

        IconButton btnBillHistory = CreateSidebarButton(IconChar.History);
        btnBillHistory.Dock = DockStyle.Bottom;
        btnBillHistory.Click += (s, e) => OnBillHistoryClicked?.Invoke(this, EventArgs.Empty);

        Controls.Add(btnHome);
        Controls.Add(btnBillHistory);
        Controls.Add(btnSettings);
        Controls.Add(btnLogout);
    }

    private static IconButton CreateSidebarButton(IconChar icon)
    {
        return new IconButton
        {
            IconChar = icon,
            IconColor = Color.White,
            IconSize = 32,
            Height = 80,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Transparent,
            TextImageRelation = TextImageRelation.Overlay,
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(50, 50, 50) }
        };
    }
}
