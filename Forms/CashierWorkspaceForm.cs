using CoffeePOS.Core;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Products;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CoffeePOS.Forms;

public class CashierWorkspaceForm : Form
{
    // DEPENDENCIES & CONTROLS
    private readonly IBillService _billService;
    private readonly IBillQueryService _billQueryService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFormFactory _formFactory;
    private readonly IUserSession _session;
    private readonly PdfPrintQueue _pdfQueue;

    // UI Components
    private readonly UC_Sidebar _ucSidebar = null!;
    private readonly UC_Billing _ucBilling = null!;
    private readonly UC_BillHistory _ucBillHistory = null!;
    private readonly UC_Menu _ucMenu = null!;
    private readonly Label _lblUserInfo = new();
    private CancellationTokenSource? _clockCts;

    // Logic Components
    private bool _isProcessingPayment = false;
    private bool _isLoggingOut = false;

    // CONSTRUCTOR & INIT
    public CashierWorkspaceForm(IServiceProvider serviceProvider, IUserSession session,
                    IBillService billService, IBillQueryService billQueryService, PdfPrintQueue pdfQueue, IFormFactory formFactory)
    {
        InitializeUI();

        _serviceProvider = serviceProvider;
        _formFactory = formFactory;
        _session = session;
        _billService = billService;
        _billQueryService = billQueryService;
        _pdfQueue = pdfQueue;
        _ucSidebar = _serviceProvider.GetRequiredService<UC_Sidebar>();
        _ucBilling = _serviceProvider.GetRequiredService<UC_Billing>();
        _ucMenu = _serviceProvider.GetRequiredService<UC_Menu>();
        _ucBillHistory = _serviceProvider.GetRequiredService<UC_BillHistory>();


        // Setup các thành phần giao diện
        SetupSidebar();
        SetupBilling();
        SetupHeader();
        SetupHistory();
        SetupMenu();

        // Ráp nối Layout
        AssembleLayout();
    }

    // UI SETUP METHODS

    private void AssembleLayout()
    {
        Controls.Add(_ucSidebar); // Trái
        Controls.Add(_ucBilling); // Phải
        Controls.Add(_lblUserInfo); // Top
        Controls.Add(_ucBillHistory); // Fill
        Controls.Add(_ucMenu); // Fill

        _ucMenu.BringToFront();
    }

    private void InitializeUI()
    {
        Text = "CoffeePOS - Code Chay Edition";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
    }

    private void SetupHeader()
    {
        _lblUserInfo.Dock = DockStyle.Top;
        _lblUserInfo.Height = 25;
        _lblUserInfo.BackColor = Color.FromArgb(245, 245, 245);
        _lblUserInfo.ForeColor = Color.FromArgb(0, 122, 204);
        _lblUserInfo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _lblUserInfo.TextAlign = ContentAlignment.MiddleLeft;
        _lblUserInfo.Padding = new Padding(0, 0, 20, 0);

        _clockCts = new CancellationTokenSource();

        _ = StartRealTimeClockAsync(_clockCts.Token);
    }

    private void SetupSidebar()
    {
        _ucSidebar.OnHomeClicked += (s, e) =>
        {
            _ucBilling.ClearOrder();

            _ucBillHistory.Visible = false;
            _ucMenu.Visible = true;
            _ucMenu.BringToFront();
        };

        _ucSidebar.OnBillHistoryClicked += async (s, e) =>
        {
            var todayBills = await _billQueryService.GetTodayBillsByUserAsync(_session.CurrentUser!.Id);
            _ucBillHistory.BindData(todayBills);

            _ucMenu.Visible = false;
            _ucBillHistory.Visible = true;
            _ucBillHistory.BringToFront();
        };

        _ucSidebar.OnSettingsClicked += (s, e) =>
        {
            using var settingForm = _formFactory.CreateForm<SettingForm>();
            if (settingForm.ShowDialog() == DialogResult.OK)
            {
                _session.Logout();
                DialogResult = DialogResult.Abort;
                _isLoggingOut = true;
                Close();
            }
        };

        _ucSidebar.OnLogoutClicked += (s, e) =>
        {
            if (_ucBilling.HasUnpaidItems)
            {
                MessageBoxHelper.Warning("Giỏ hàng đang có món chưa thanh toán!\nVui lòng hoàn tất hoặc xóa giỏ hàng trước khi đăng xuất.", "CẢNH BÁO", this);
                return;
            }

            using var shiftForm = _formFactory.CreateForm<ShiftReportForm>();
            if (shiftForm.ShowDialog() == DialogResult.OK)
            {
                _session.Logout();
                DialogResult = DialogResult.Abort;
                _isLoggingOut = true;
                Close();
            }
        };
    }

    private void SetupBilling()
    {
        _ucBilling.OnPayClicked += async (s, e) => await ProcessPaymentAsync();
    }

    private void SetupHistory()
    {
        _ucBillHistory.OnReprintClicked += async (s, bill) =>
        {
            if (MessageBoxHelper.ConfirmYesNo($"Bạn muốn in lại hóa đơn #{bill.Id}?", "In lại", this))
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    var billItems = await _billQueryService.GetBillDetailsAsync(bill.Id);

                    await _pdfQueue.EnqueueJobAsync(new BillPrintPayload
                    {
                        BillId = bill.Id,
                        BuzzerNumber = bill.BuzzerNumber,
                        TotalAmount = bill.TotalAmount,
                        Details = [.. billItems],
                        IsReprint = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBoxHelper.Error($"Lỗi khi in lại hóa đơn: {ex.Message}", "LỖI HỆ THỐNG", this);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        };

        _ucBillHistory.OnDetailsRequested += async (s, bill) => await ShowBillHistoryDetailsAsync(bill);
    }

    private async Task ShowBillHistoryDetailsAsync(BillHistoryDto historyBill)
    {
        try
        {
            Cursor.Current = Cursors.WaitCursor;

            var details = await _billQueryService.GetBillDetailsAsync(historyBill.Id);
            if (details.Count == 0)
            {
                MessageBoxHelper.Warning($"Hóa đơn #{historyBill.Id} chưa có chi tiết món.", owner: this);
                return;
            }

            var billDate = DateOnly.FromDateTime(historyBill.CreatedAt);
            var reports = await _billQueryService.GetBillsByDateRangeAsync(billDate, billDate);

            var reportBill = reports.FirstOrDefault(x => x.Id == historyBill.Id)
                ?? new BillReportDto(
                    historyBill.Id,
                    historyBill.BuzzerNumber,
                    historyBill.TotalAmount,
                    historyBill.CreatedAt,
                    _session.CurrentUser?.FullName ?? "N/A",
                    false,
                    null);

            using var detailForm = new BillDetailForm(reportBill, details);
            detailForm.ShowDialog(this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải chi tiết hóa đơn: {ex.Message}", owner: this);
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    private void SetupMenu()
    {
        _ucMenu.OnProductSelected += (prodId, prodName, price, imageIdentifier) =>
        {
            _ucBilling.AddItemToBill(prodId, prodName, 1, price, imageIdentifier);
        };
    }

    // BUSINESS LOGIC

    private async Task ProcessPaymentAsync()
    {
        if (_isProcessingPayment) return;

        var cartItems = _ucBilling.GetCartItems();
        if (cartItems.Count == 0)
        {
            MessageBoxHelper.Warning("Chưa có món nào trong giỏ!", owner: this);
            return;
        }

        string input = Microsoft.VisualBasic.Interaction.InputBox("Nhập số Thẻ Rung / Số thứ tự:", "Xác nhận Order", "1");
        if (!int.TryParse(input, out int buzzerNumber)) return;

        decimal finalAmount = _ucBilling.GrandTotal;
        if (!MessageBoxHelper.ConfirmYesNo($"Thu của khách {finalAmount:N0} đ?", "Xác nhận", this))
            return;

        try
        {
            _isProcessingPayment = true;

            var command = new CreateBillDto(
                buzzerNumber,
                _session.CurrentUser!.Id,
                finalAmount,
                [.. cartItems]);

            int billId = await _billService.ProcessFullOrderAsync(command);

            await _pdfQueue.EnqueueJobAsync(new BillPrintPayload
            {
                BillId = billId,
                BuzzerNumber = buzzerNumber,
                TotalAmount = finalAmount,
                Details = [.. cartItems.Select(i => new BillDetailDto(
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    i.Price,
                    i.Note))]
            });

            _ucBilling.ClearOrder();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi thanh toán: {ex.Message}", owner: this);
        }
        finally
        {
            _isProcessingPayment = false;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && !_isLoggingOut)
        {
            MessageBoxHelper.Error("Vui lòng thoát bằng nút 'Đăng xuất' để hệ thống Chốt ca làm việc!", "CẢNH BÁO", this);
            e.Cancel = true;
            return;
        }

        if (!e.Cancel && _clockCts != null)
        {
            _clockCts.Cancel();
            _clockCts.Dispose();
        }

        base.OnFormClosing(e);
    }

    private async Task StartRealTimeClockAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var user = _session.CurrentUser;

                if (user != null)
                {
                    var now = DateTime.Now;
                    _lblUserInfo.Text = $"Ca trực: {user.FullName}   |   🕒 {now:dd/MM/yyyy HH:mm:ss}";

                    int msUntilNextSecond = 1000 - now.Millisecond;
                    await Task.Delay(msUntilNextSecond + 1, token);
                }
                else
                {
                    _lblUserInfo.Text = string.Empty;
                    await Task.Delay(1000, token);
                }
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            Log.Error($"Clock Error: {ex.Message}");
        }
    }
}
