using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Features.Admin;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public partial class AdminDashboardForm : Window
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
        _ucSidebar.OnNavigate += HandleNavigation;
        _ucSidebar.OnProfilesClicked += HandleProfilesClicked;
        _ucSidebar.OnLogoutClicked += HandleLogoutClicked;
    }

    private void HandleNavigation(string routeTag)
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
            case "ManageToppings":
                NavigateTo<UC_ManageToppings>(routeTag);
                break;
            case "ManageUsers":
                NavigateTo<UC_ManageUsers>(routeTag);
                break;
            case "ManageBills":
                NavigateTo<UC_ManageBills>(routeTag);
                break;
        }
    }

    private void HandleProfilesClicked(object? sender, EventArgs e)
    {
        var uiFactory = _serviceProvider.GetRequiredService<IUiFactory>();
        var profilesControl = uiFactory.CreateControl<UC_Profiles>();

        var config = new Modal.Config(this, "THÔNG TIN CÁ NHÂN", profilesControl)
        {
            Font = UiTheme.BodyFont,
            OkText = "Cập nhật",
            CancelText = "Huỷ",
            BtnHeight = 45,
            OnOk = (cfg) =>
            {
                var payload = profilesControl.GetPayload();
                ExecutePasswordChange(payload);
                return false;
            }
        };

        AntdUI.Modal.open(config);
    }

    private void HandleLogoutClicked(object? sender, EventArgs e)
    {
        if (MessageBoxHelper.ConfirmYesNo("Bạn muốn đăng xuất khỏi hệ thống?", "Xác nhận", this))
        {
            _session.Logout();
            DialogResult = DialogResult.Abort;
            Close();
        }
    }

    private void BtnLogout_Click(object? sender, EventArgs e)
    {
        if (MessageBoxHelper.ConfirmYesNo("Bạn muốn đăng xuất khỏi hệ thống?", "Xác nhận", this))
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

        value.Visible = true;
        value.BringToFront();
    }

    private void ExecutePasswordChange(ChangePasswordPayload payload)
    {
        AntdUI.Message.loading(this, "Đang đổi mật khẩu...", async config =>
        {
            config.ID = "change_pass";

            try
            {
                var userService = _serviceProvider.GetRequiredService<IUserService>();
                await userService.ChangePasswordAsync(
                    _session.CurrentUser!.Id,
                    _session.CurrentUser.Username,
                    payload
                );

                Invoke(() =>
                {
                    _session.Logout();
                    DialogResult = DialogResult.Abort;
                    AntdUI.Message.close_id("change_pass");
                    MessageBoxHelper.Info("Đổi mật khẩu thành công! Hệ thống sẽ đăng xuất.");
                    Close();
                });
            }
            catch (Exception ex)
            {
                string errMsg = ex is InvalidOperationException or ArgumentException
                    ? ex.Message
                    : $"Lỗi đổi mật khẩu: {ex.Message}";

                Invoke(() => MessageBoxHelper.Error(errMsg, owner: this, type: FeedbackType.Message));
            }
            finally
            {
                if (!IsDisposed)
                {
                    Invoke(() => AntdUI.Message.close_id("change_pass"));
                }
            }
        });
    }
}
