using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

// WHY: This file handles strictly Business Logic and State Mutations. Zero UI rendering code allowed here.
public partial class LoginForm : Window
{
    private readonly IUserService _userService;
    private readonly IUserSession _session;
    private CancellationTokenSource? _cts;

    public LoginForm(IUserService userService, IUserSession session)
    {
        _userService = userService;
        _session = session;

        // HACK: Physically located in LoginForm.Designer.cs
        InitializeComponent();

        // PERF: Global KeyHook replaces standard AcceptButton to work flawlessly with AntdUI custom GDI+ controls
        KeyPreview = true;
        KeyDown += HandleGlobalKeyDown;
    }

    // HACK: Bắt sự kiện Window bị tắt (bấm nút X)
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);

        // Gửi tín hiệu Hủy xuống tất cả các Task đang dùng Token này
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void HandleGlobalKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !_btnLogin.Loading)
        {
            e.SuppressKeyPress = true;
            HandleLoginAsync(this, EventArgs.Empty);
        }
    }

    private async void HandleLoginAsync(object? sender, EventArgs e)
    {
        string username = _txtUsername.Text.Trim();
        string password = _txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            MessageBoxHelper.Warning("Vui lòng nhập đủ Username và Password!", owner: this);
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
                MessageBoxHelper.Warning("Sai tài khoản hoặc mật khẩu!", owner: this);
            }
        }
        catch (OperationCanceledException)
        {
            // PERF: Bắt đúng lỗi Hủy Task. Không làm gì cả vì Form đang bị đóng rồi, văng MessageBox lên lúc này sẽ gây lỗi UI.
        }
        catch (InvalidOperationException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Tài khoản bị khóa", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi kết nối CSDL: {ex.Message}", owner: this);
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
