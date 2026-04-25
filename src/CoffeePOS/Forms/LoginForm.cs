using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public partial class LoginForm : Window
{
    private readonly IUserService _userService;
    private readonly IUserSession _session;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource? _cts;
    private bool _isSystemReady = false;

    public LoginForm(IServiceProvider serviceProvider, IUserService userService, IUserSession session)
    {
        _serviceProvider = serviceProvider;
        _userService = userService;
        _session = session;

        InitializeComponent();

        KeyPreview = true;
        KeyDown += HandleGlobalKeyDown;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);

        if (_isSystemReady) return;

        _btnLogin.Enabled = false;
        _btnLogin.Loading = true;
        _btnLogin.Text = "Hệ Thống Đang Khởi Động...";

        try
        {
            var config = _serviceProvider.GetRequiredService<IConfiguration>();
            var initPdfTask = Task.Run(InvoiceGenerator.Initialize);

            await initPdfTask;

            _isSystemReady = true;
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Initialization Failed: {ex.Message}", "CRITICAL ERROR");
            Application.Exit();
        }
        finally
        {
            if (!IsDisposed && !Disposing && _isSystemReady)
            {
                _btnLogin.Enabled = true;
                _btnLogin.Loading = false;
                _btnLogin.Text = "ĐĂNG NHẬP";
                _txtUsername.Focus();
            }
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void HandleGlobalKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !_btnLogin.Loading && _isSystemReady)
        {
            e.SuppressKeyPress = true;
            HandleLoginAsync(this, EventArgs.Empty);
        }
    }

    private async void HandleLoginAsync(object? sender, EventArgs e)
    {
        if (!_isSystemReady) return;

        string username = _txtUsername.Text.Trim();
        string password = _txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBoxHelper.Warning("Vui lòng nhập đủ Username và Password!", owner: this, type: FeedbackType.Message);
            return;
        }

        _btnLogin.Enabled = false;
        _btnLogin.Loading = true;
        _btnLogin.Text = "Đang xử lý...";

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            var user = await _userService.AuthenticateAsync(username, password, _cts.Token);

            if (user != null)
            {
                _session.Login(user);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBoxHelper.Error("Sai tài khoản hoặc mật khẩu!", owner: this, type: FeedbackType.Message);
            }
        }
        catch (OperationCanceledException)
        {
            // BUG: Silent catch intentional during Form disposal
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Error($"{ex.Message}", owner: this, type: FeedbackType.Message);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Connection Error: {ex.Message}", owner: this, type: FeedbackType.Message);
        }
        finally
        {
            if (!IsDisposed && !Disposing)
            {
                _btnLogin.Enabled = true;
                _btnLogin.Loading = false;
                _btnLogin.Text = "ĐĂNG NHẬP";
            }
        }
    }
}
