using CoffeePOS.Core;
using CoffeePOS.Features.Admin;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public partial class AdminDashboardForm : AntdUI.Window
{
    private readonly IUserSession _session;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, UserControl> _viewCache = [];

    private readonly UC_Sidebar _ucSidebar;

    public AdminDashboardForm(IServiceProvider serviceProvider, IUserSession session)
    {
        _session = session;
        _serviceProvider = serviceProvider;

        _ucSidebar = _serviceProvider.GetRequiredService<UC_Sidebar>();

        InitializeComponent();
        AssembleLayout(_ucSidebar);

        BindData();
        WireEvents();

        NavigateTo<UC_Dashboard>("Dashboard");
    }

    private void BindData()
    {
        lblUserInfo.Text = $"Quản trị viên: {_session.CurrentUser?.FullName ?? "N/A"}";
    }

    private void WireEvents()
    {
        _ucSidebar.OnNavigate += (routeTag) =>
        {
            switch (routeTag)
            {
                case "Dashboard":
                    NavigateTo<UC_Dashboard>(routeTag);
                    break;
                case "ManageProducts":
                    NavigateTo<UC_ManageProducts>(routeTag);
                    break;
                case "ManageCategories":
                    NavigateTo<UC_ManageCategories>(routeTag);
                    break;
                case "ManageUsers":
                    NavigateTo<UC_ManageUsers>(routeTag);
                    break;
                case "ManageBills":
                    NavigateTo<UC_ManageBills>(routeTag);
                    break;
            }
        };

        _ucSidebar.OnProfilesClicked += async (s, e) =>
        {
            var uiFactory = _serviceProvider.GetRequiredService<IUiFactory>();
            var profilesControl = uiFactory.CreateControl<UC_Profiles>();
            using var shell = new DynamicModalShell<ChangePasswordPayload>("THÔNG TIN CÁ NHÂN", profilesControl, new Size(450, 550), saveButtonText: "CẬP NHẬT");

            if (shell.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var payload = shell.ExtractData();
                var userService = _serviceProvider.GetRequiredService<IUserService>();

                await userService.ChangePasswordAsync(
                    userId: _session.CurrentUser!.Id,
                    username: _session.CurrentUser.Username,
                    currentPassword: payload.CurrentPassword,
                    newPassword: payload.NewPassword,
                    confirmPassword: payload.ConfirmPassword
                );

                MessageBoxHelper.Info("Đổi mật khẩu thành công! Vui lòng đăng nhập lại.", "Thành công", this);
                _session.Logout();
                DialogResult = DialogResult.Abort;
                Close();
            }
            catch (ArgumentException ex) // Bắt lỗi validation (pass ngắn, không khớp)
            {
                MessageBoxHelper.Warning(ex.Message, owner: this);
            }
            catch (InvalidOperationException ex) // Bắt lỗi sai pass hiện tại
            {
                MessageBoxHelper.Error(ex.Message, owner: this);
            }
            catch (Exception ex) // Bắt lỗi DB/Network
            {
                MessageBoxHelper.Error($"Lỗi hệ thống: {ex.Message}", owner: this);
            }
        };

        _ucSidebar.OnLogoutClicked += BtnLogout_Click;
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
