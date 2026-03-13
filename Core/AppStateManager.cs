using CoffeePOS.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Core;

public class AppStateManager : ApplicationContext
{
    private readonly IServiceProvider _serviceProvider;

    public AppStateManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

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
        var mainForm = _serviceProvider.GetRequiredService<MainForm>();
        mainForm.FormClosed += OnFormClosed;
        MainForm = mainForm;
        mainForm.Show();
    }

    private void OnFormClosed(object? sender, FormClosedEventArgs e)
    {
        if (sender is Form closedForm)
        {
            // Unsubscribe event để Garbage Collector dọn dẹp Form cũ (Chống Memory Leak)
            closedForm.FormClosed -= OnFormClosed;

            bool isLoginSuccess = closedForm is LoginForm && closedForm.DialogResult == DialogResult.OK;
            bool isLogout = closedForm is MainForm && closedForm.DialogResult == DialogResult.Abort;

            closedForm.Dispose();

            // 4. CHUYỂN CẢNH
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
                ExitThread(); // Tắt App
            }
        }
    }
}
