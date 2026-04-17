using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Shared.Constants;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Features.Sidebar;

public partial class UC_Sidebar : UserControl
{
    private IUserSession? _session;

    public event Action<string>? OnNavigate;
    public event EventHandler? OnHomeClicked;
    public event EventHandler? OnBillHistoryClicked;
    public event EventHandler? OnSettingsClicked;
    public event EventHandler? OnLogoutClicked;

    public UC_Sidebar()
    {
        InitializeComponent();
        Load += OnComponentLoad;
        _btnToggle.Click += OnToggleClicked;
    }

    public void Setup(IUserSession session)
    {
        _session = session;
        RenderNavigationTree();
    }

    private void OnComponentLoad(object? sender, EventArgs e)
    {
        if (DesignMode) return;
        RenderNavigationTree();
    }

    private void OnToggleClicked(object? sender, EventArgs e)
    {
        _menuMain.Collapsed = !_menuMain.Collapsed;

        if (_menuMain.Collapsed)
        {
            Width = 80;
            _btnToggle.IconSvg = SvgAssets.MenuUnfoldOutlined;
        }
        else
        {
            Width = _sidebarWidth;
            _btnToggle.IconSvg = SvgAssets.MenuFoldOutlined;
        }
    }

    // PERF: Time/Space Complexity is O(N) where N is the number of menu items.
    // Array reallocation is avoided by rendering synchronously on init.
    private void RenderNavigationTree()
    {
        _menuMain.Items.Clear();

        var currentRole = _session?.CurrentUser?.Role ?? UserRole.Cashier;

        AddMenuItem("Bán hàng", "Billing", SvgAssets.Cart);

        if (currentRole == UserRole.Cashier)
        {
            AddMenuItem("Lịch sử hóa đơn", "BillHistory", SvgAssets.FileText);
        }

        if (currentRole == UserRole.Admin)
        {
            AddMenuItem("Thống kê", "Dashboard", SvgAssets.Dashboard);
            AddMenuItem("Hóa đơn", "ManageBills", SvgAssets.FileText);
            AddMenuItem("Sản phẩm", "ManageProducts", SvgAssets.Box);
            AddMenuItem("Danh mục", "ManageCategories", SvgAssets.Tags);
            AddMenuItem("Nhân sự", "ManageUsers", SvgAssets.Users);
        }

        AddMenuItem("Cài đặt", "Settings", SvgAssets.Setting);
        AddMenuItem("Đăng xuất", "Logout", SvgAssets.Logout);

        if (_menuMain.Items.Count > 0)
        {
            _menuMain.SelectIndex(0, false);
        }

        _menuMain.SelectChanged -= HandleRouteSelection;
        _menuMain.SelectChanged += HandleRouteSelection;
    }

    private void AddMenuItem(string displayText, string routeTag, string svgIcon)
    {
        _menuMain.Items.Add(new MenuItem
        {
            Text = displayText,
            Tag = routeTag,
            IconSvg = svgIcon
        });
    }

    private void HandleRouteSelection(object sender, MenuSelectEventArgs e)
    {
        if (e.Value.Tag is not string targetRoute)
        {
            return;
        }

        OnNavigate?.Invoke(targetRoute);

        switch (targetRoute)
        {
            case "Billing":
                OnHomeClicked?.Invoke(this, EventArgs.Empty);
                break;
            case "BillHistory":
                OnBillHistoryClicked?.Invoke(this, EventArgs.Empty);
                break;
            case "Settings":
                OnSettingsClicked?.Invoke(this, EventArgs.Empty);
                break;
            case "Logout":
                OnLogoutClicked?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
