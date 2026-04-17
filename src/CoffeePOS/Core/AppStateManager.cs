using CoffeePOS.Forms;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Core;

public class AppStateManager : ApplicationContext
{
    private readonly IUiFactory _formFactory;
    private readonly IUserSession _session;

    public AppStateManager(IUiFactory formFactory, IUserSession session)
    {
        _formFactory = formFactory;
        _session = session;

        ShowLoginForm();
    }

    private void ShowLoginForm()
    {
        var loginForm = _formFactory.CreateForm<LoginForm>();

        loginForm.FormClosed += OnFormClosed;

        MainForm = loginForm;
        // loginForm.Show();
    }

    private void ShowMainWorkspace()
    {
        Form nextForm;
        if (_session.CurrentUser!.Role == UserRole.Admin)
        {
            nextForm = _formFactory.CreateForm<AdminDashboardForm>();
        }
        else
        {
            nextForm = _formFactory.CreateForm<CashierWorkspaceForm>();
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
