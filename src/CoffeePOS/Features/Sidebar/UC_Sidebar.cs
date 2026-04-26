using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Shared.Constants;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Features.Sidebar;

public partial class UC_Sidebar : UserControl
{
    private readonly IUserSession _session;

    public event Action<string>? OnNavigate;
    public event EventHandler? OnHomeClicked;
    public event EventHandler? OnBillHistoryClicked;
    public event EventHandler? OnProfilesClicked;
    public event EventHandler? OnLogoutClicked;

    public UC_Sidebar(IUserSession session)
    {
        _session = session;
        InitializeComponent();
        Load += OnComponentLoad;
        _btnToggle.Click += OnToggleClicked;

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
            Width = _sidebarWidthCollaped;
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

        if (currentRole == UserRole.Cashier)
        {
            AddMenuItem("Bán hàng", "Billing", SvgAssets.Cart);
            AddMenuItem("Lịch sử hóa đơn", "BillHistory", SvgAssets.FileText);
        }

        if (currentRole == UserRole.Admin)
        {
            AddMenuItem("Thống kê", "Dashboard", SvgAssets.Dashboard);
            AddMenuItem("Hóa đơn", "ManageBills", SvgAssets.FileText);
            AddMenuItem("Sản phẩm", "ManageProducts", SvgAssets.Box);
            AddMenuItem("Danh mục", "ManageCategories", SvgAssets.Tags);
            AddMenuItem("Topping", "ManageToppings", SvgAssets.Topping);
            AddMenuItem("Nhân sự", "ManageUsers", SvgAssets.Users);
            AddMenuItem("Chốt ca", "ManageShiftReports", SvgAssets.Repost);
        }

        AddMenuItem("Cá nhân", "Profiles", SvgAssets.User);
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
            case "Profiles":
                OnProfilesClicked?.Invoke(this, EventArgs.Empty);
                break;
            case "Logout":
                OnLogoutClicked?.Invoke(this, EventArgs.Empty);
                break;
        }
    }
}
