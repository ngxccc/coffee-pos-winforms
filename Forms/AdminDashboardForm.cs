using CoffeePOS.Core;
using CoffeePOS.Features.Admin;
using CoffeePOS.Shared.Helpers;
using FontAwesome.Sharp;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public partial class AdminDashboardForm : Form
{
    private readonly IUserSession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, UserControl> _viewCache = [];

    // UI COMPONENTS
    private Panel pnlSidebar = null!;
    private Panel pnlHeader = null!;
    private Panel pnlContent = null!;

    private Label lblPlaceholder = null!;

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
        BackColor = Color.FromArgb(245, 245, 255);

        pnlSidebar = new Panel
        {
            Dock = DockStyle.Left,
            Width = 220,
            BackColor = Color.FromArgb(31, 30, 68)
        };
        pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.White
        };
        pnlContent = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };

        Label lblUserInfo = new()
        {
            Text = $"Quản trị viên: {_session.CurrentUser?.FullName} | Đăng nhập: {_session.LoginTime:HH:mm}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Right,
            AutoSize = false,
            Width = 500,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 20, 0)
        };
        pnlHeader.Controls.Add(lblUserInfo);

        lblPlaceholder = new Label
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
        Label lblLogo = new()
        {
            Text = "QUẢN TRỊ",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter
        };

        pnlSidebar.Controls.Add(CreateMenuButton("Đăng xuất", IconChar.SignOutAlt, BtnLogout_Click));
        pnlSidebar.Controls.Add(CreateMenuButton("Hóa đơn & Báo cáo", IconChar.FileInvoiceDollar, (s, e) => ShowPlaceholder("TÍNH NĂNG: HÓA ĐƠN")));
        pnlSidebar.Controls.Add(CreateMenuButton("Nhân sự", IconChar.Users, (s, e) => NavigateTo<UC_ManageUsers>("USERS")));
        pnlSidebar.Controls.Add(CreateMenuButton("Danh mục", IconChar.Tags, (s, e) => NavigateTo<UC_ManageCategories>("CATEGORIES")));
        pnlSidebar.Controls.Add(CreateMenuButton("Sản phẩm", IconChar.Coffee, (s, e) => NavigateTo<UC_ManageProducts>("PRODUCTS")));
        pnlSidebar.Controls.Add(CreateMenuButton("Tổng quan", IconChar.ChartBar, (s, e) => NavigateTo<UC_Dashboard>("DASHBOARD")));

        pnlSidebar.Controls.Add(lblLogo);
    }

    private static IconButton CreateMenuButton(string text, IconChar icon, EventHandler clickEvent)
    {
        IconButton btn = new()
        {
            Text = "  " + text,
            UseMnemonic = false,
            IconChar = icon,
            IconSize = 32,
            IconColor = Color.Gainsboro,
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            Dock = DockStyle.Top,
            Height = 60,
            FlatStyle = FlatStyle.Flat,
            ImageAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            Padding = new Padding(10, 0, 0, 0),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;

        btn.MouseEnter += (s, e) =>
        {
            btn.BackColor = Color.FromArgb(41, 40, 78); btn.ForeColor = Color.White; btn.IconColor = Color.White;
        };
        btn.MouseLeave += (s, e) =>
        {
            btn.BackColor = Color.Transparent; btn.ForeColor = Color.Gainsboro; btn.IconColor = Color.Gainsboro;
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
