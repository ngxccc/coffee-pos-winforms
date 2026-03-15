using CoffeePOS.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Core;

public class AppStateManager : ApplicationContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserSession _session;

    public AppStateManager(IServiceProvider serviceProvider, IUserSession session)
    {
        _serviceProvider = serviceProvider;
        _session = session;

        ShowLoginForm();
    }

    private void ShowLoginForm()
    {
        var loginForm = _serviceProvider.GetRequiredService<LoginForm>();

        loginForm.FormClosed += OnFormClosed;

        MainForm = loginForm;
        loginForm.Show();
    }

    private void ShowMainWorkspace()
    {
        Form nextForm;
        if (_session.CurrentUser!.Role == 0)
        {
            nextForm = _serviceProvider.GetRequiredService<AdminDashboardForm>();
        }
        else
        {
            nextForm = _serviceProvider.GetRequiredService<CashierWorkspaceForm>();
        }

        nextForm.FormClosed += OnFormClosed;
        MainForm = nextForm;
        nextForm.Show();
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is Form closedForm)
        {
            // Unsubscribe event để Garbage Collector dọn dẹp Form cũ (Chống Memory Leak)
            closedForm.FormClosed -= OnFormClosed;

            bool isLoginSuccess = closedForm is LoginForm && closedForm.DialogResult == DialogResult.OK;
            bool isLogout = (closedForm is CashierWorkspaceForm || closedForm is AdminDashboardForm) && closedForm.DialogResult == DialogResult.Abort;

            if (isLoginSuccess)
            {
                ShowMainWorkspace();
            }
            else if (isLogout)
            {
                ShowLoginForm();
            }
            else
            {
                ExitThread();
            }
        }
    }
}
