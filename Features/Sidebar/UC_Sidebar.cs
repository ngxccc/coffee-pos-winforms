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

        var btnHome = CreateSidebarButton("HOME");
        btnHome.Dock = DockStyle.Top;
        btnHome.Click += (s, e) => OnHomeClicked?.Invoke(this, EventArgs.Empty);

        var btnLogout = CreateSidebarButton("LOGOUT");
        btnLogout.Dock = DockStyle.Bottom;
        btnLogout.Click += (s, e) => OnLogoutClicked?.Invoke(this, EventArgs.Empty);

        var btnSettings = CreateSidebarButton("SETTING");
        btnSettings.Dock = DockStyle.Bottom;
        btnSettings.Click += (s, e) => OnSettingsClicked?.Invoke(this, EventArgs.Empty);

        var btnBillHistory = CreateSidebarButton("BILL");
        btnBillHistory.Dock = DockStyle.Bottom;
        btnBillHistory.Click += (s, e) => OnBillHistoryClicked?.Invoke(this, EventArgs.Empty);

        Controls.Add(btnHome);
        Controls.Add(btnBillHistory);
        Controls.Add(btnSettings);
        Controls.Add(btnLogout);
    }

    private static AntdUI.Button CreateSidebarButton(string text)
    {
        return new AntdUI.Button
        {
            Text = text,
            Type = AntdUI.TTypeMini.Default,
            Height = 80,
            Radius = 0,
            Cursor = Cursors.Hand,
            ForeColor = Color.White,
            BackColor = Color.Transparent
        };
    }
}
