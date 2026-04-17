using CoffeePOS.Core;
using CoffeePOS.Features.Admin;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public class AdminDashboardForm : AntdUI.Window
{
    private readonly IUserSession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, UserControl> _viewCache = [];

    // UI COMPONENTS
    private AntdUI.Panel pnlSidebar = null!;
    private AntdUI.Panel pnlHeader = null!;
    private AntdUI.Panel pnlContent = null!;

    private AntdUI.Label lblPlaceholder = null!;

    public AdminDashboardForm(IServiceProvider serviceProvider, IUserSession session)
    {
        _session = session;
        _serviceProvider = serviceProvider;

        InitializeUI();

        SetupSidebarMenu();

        NavigateTo<UC_Dashboard>("DASHBOARD");
    }

    private void InitializeUI()
    {
        Text = "Hệ Thống Quản Trị - CoffeePOS Admin";
        ClientSize = new Size(1366, 768);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiTheme.Surface;

        pnlSidebar = new AntdUI.Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            Radius = 0,
            Back = UiTheme.TextPrimary
        };
        pnlHeader = new AntdUI.Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            Radius = 0,
            Back = UiTheme.Surface
        };
        pnlContent = new AntdUI.Panel
        {
            Dock = DockStyle.Fill,
            Radius = 0,
            Back = UiTheme.Surface,
            Padding = new Padding(20)
        };

        AntdUI.Label lblUserInfo = new()
        {
            Text = $"Quản trị viên: {_session.CurrentUser?.FullName} | Đăng nhập: {_session.LoginTime:HH:mm}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
            Dock = DockStyle.Right,
            AutoSize = false,
            Width = 500,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 20, 0)
        };
        pnlHeader.Controls.Add(lblUserInfo);

        lblPlaceholder = new AntdUI.Label
        {
            Text = "CHÀO MỪNG ADMIN!\nHãy chọn một chức năng bên Menu.",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Color.Silver,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        pnlContent.Controls.Add(lblPlaceholder);

        Controls.Add(pnlContent); // Giữa
        Controls.Add(pnlHeader);  // Trên
        Controls.Add(pnlSidebar); // Trái
    }

    private void SetupSidebarMenu()
    {
        AntdUI.Label lblLogo = new()
        {
            Text = "QUẢN TRỊ",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter
        };

        pnlSidebar.Controls.Add(CreateMenuButton("Đăng xuất", BtnLogout_Click));
        pnlSidebar.Controls.Add(CreateMenuButton("Hóa đơn & Báo cáo", (s, e) => NavigateTo<UC_ManageBills>("BILLS")));
        pnlSidebar.Controls.Add(CreateMenuButton("Nhân sự", (s, e) => NavigateTo<UC_ManageUsers>("USERS")));
        pnlSidebar.Controls.Add(CreateMenuButton("Danh mục", (s, e) => NavigateTo<UC_ManageCategories>("CATEGORIES")));
        pnlSidebar.Controls.Add(CreateMenuButton("Sản phẩm", (s, e) => NavigateTo<UC_ManageProducts>("PRODUCTS")));
        pnlSidebar.Controls.Add(CreateMenuButton("Tổng quan", (s, e) => NavigateTo<UC_Dashboard>("DASHBOARD")));

        pnlSidebar.Controls.Add(lblLogo);
    }

    private static AntdUI.Button CreateMenuButton(string text, EventHandler clickEvent)
    {
        AntdUI.Button btn = new()
        {
            Text = text,
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            Type = AntdUI.TTypeMini.Default,
            Radius = 0,
            Dock = DockStyle.Top,
            Height = 60,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Cursor = Cursors.Hand
        };

        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = Color.FromArgb(41, 40, 78); btn.ForeColor = Color.White;
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = Color.Transparent; btn.ForeColor = Color.Gainsboro;
        };

        btn.Click += clickEvent;
        return btn;
    }

    private void ShowPlaceholder(string moduleName)
    {
        foreach (var view in _viewCache.Values)
        {
            view.Visible = false;
        }

        lblPlaceholder.Text = $"{moduleName}\n\nĐang được các Kỹ sư xây dựng...";
        lblPlaceholder.ForeColor = Color.FromArgb(0, 122, 204);
        lblPlaceholder.Visible = true;
        lblPlaceholder.BringToFront();
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        if (MessageBoxHelper.ConfirmYesNo("Sếp muốn đăng xuất khỏi hệ thống?", "Xác nhận", this))
        {
            _session.Logout();
            DialogResult = DialogResult.Abort;
            Close();
        }
    }

    private void NavigateTo<T>(string viewKey) where T : UserControl
    {
        if (!_viewCache.TryGetValue(viewKey, out UserControl? value))
        {
            var newView = _serviceProvider.GetRequiredService<T>();
            newView.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(newView);
            value = newView;
            _viewCache[viewKey] = value;
        }

        foreach (var view in _viewCache.Values)
        {
            view.Visible = false;
        }

        lblPlaceholder.Visible = false;
        value.Visible = true;
        value.BringToFront();
    }
}
