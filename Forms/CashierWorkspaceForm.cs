using CoffeePOS.Core;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Products;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public partial class CashierWorkspaceForm : AntdUI.Window
{
    // DEPENDENCIES
    private readonly IBillService _billService;
    private readonly IBillQueryService _billQueryService;
    private readonly IUserService _userService;
    private readonly IShiftReportService _shiftReportService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUiFactory _formFactory;
    private readonly IUserSession _session;
    private readonly PdfPrintQueue _pdfQueue;

    // INJECTED UI COMPONENTS
    private readonly UC_Sidebar _ucSidebar;
    private readonly UC_Billing _ucBilling;
    private readonly UC_BillHistory _ucBillHistory;
    private readonly UC_Menu _ucMenu;

    // STATE
    private bool _isProcessingPayment = false;
    private bool _isLoggingOut = false;

    public CashierWorkspaceForm(
        IServiceProvider serviceProvider,
        IUserSession session,
        IBillService billService,
        IBillQueryService billQueryService,
        IUserService userService,
        IShiftReportService shiftReportService,
        PdfPrintQueue pdfQueue,
        IUiFactory formFactory)
    {
        _serviceProvider = serviceProvider;
        _session = session;
        _billService = billService;
        _billQueryService = billQueryService;
        _userService = userService;
        _shiftReportService = shiftReportService;
        _pdfQueue = pdfQueue;
        _formFactory = formFactory;

        // 1. Resolve ruột UI từ DI Container
        _ucSidebar = _serviceProvider.GetRequiredService<UC_Sidebar>();
        _ucBilling = _serviceProvider.GetRequiredService<UC_Billing>();
        _ucMenu = _serviceProvider.GetRequiredService<UC_Menu>();
        _ucBillHistory = _serviceProvider.GetRequiredService<UC_BillHistory>();

        // 2. Gọi file Designer để dựng khung sườn và lắp ráp
        InitializeComponent();
        AssembleLayout(_ucSidebar, _ucBilling, _ucBillHistory, _ucMenu);

        // 3. Khởi tạo dữ liệu và luồng sự kiện
        BindInitialData();
        SetupSidebarEvents();
        SetupBillingEvents();
        SetupHistoryEvents();
        SetupMenuEvents();
    }

    private void BindInitialData()
    {
        _lblUserInfo.Text = $"Ca trực: {_session.CurrentUser?.FullName ?? "N/A"}";
    }

    private void SetupSidebarEvents()
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

        _ucSidebar.OnProfilesClicked += async (s, e) =>
        {
            var profilesControl = _formFactory.CreateControl<UC_Profiles>();
            using var shell = new DynamicModalShell<ChangePasswordPayload>("THÔNG TIN CÁ NHÂN", profilesControl, new Size(450, 550), saveButtonText: "CẬP NHẬT");

            if (shell.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var payload = shell.ExtractData();
                await _userService.ChangePasswordAsync(
                    _session.CurrentUser!.Id,
                    _session.CurrentUser.Username,
                    payload.CurrentPassword,
                    payload.NewPassword,
                    payload.ConfirmPassword);

                MessageBoxHelper.Info("Đổi mật khẩu thành công!", "Thành công", this);
                _session.Logout();
                DialogResult = DialogResult.Abort;
                _isLoggingOut = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error($"Lỗi đổi mật khẩu: {ex.Message}", owner: this);
            }
        };

        _ucSidebar.OnLogoutClicked += async (s, e) =>
        {
            if (_ucBilling.HasUnpaidItems)
            {
                MessageBoxHelper.Warning("Giỏ hàng đang có món chưa thanh toán!\nVui lòng hoàn tất hoặc xóa giỏ hàng trước khi đăng xuất.", "CẢNH BÁO", this);
                return;
            }

            var shiftControl = _formFactory.CreateControl<UC_ShiftReportFields>();
            using var shell = new DynamicModalShell<ShiftReportPayload>(
                "CHỐT CA LÀM VIỆC",
                shiftControl,
                new Size(450, 500),
                saveButtonText: "XÁC NHẬN CHỐT CA");

            if (shell.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var payload = shell.ExtractData();
                var command = new SaveShiftReportDto(
                    _session.CurrentUser!.Id, _session.LoginTime!.Value, payload.EndTime,
                    payload.TotalBills, payload.ExpectedCash, payload.ActualCash,
                    payload.Variance, payload.Note);

                await _shiftReportService.SaveReportAsync(command);
                await _pdfQueue.EnqueueJobAsync(new ShiftReportPrintPayload
                {
                    CashierName = _session.CurrentUser!.FullName,
                    StartTime = _session.LoginTime!.Value,
                    EndTime = payload.EndTime,
                    TotalBills = payload.TotalBills,
                    ExpectedCash = payload.ExpectedCash,
                    ActualCash = payload.ActualCash,
                    Variance = payload.Variance,
                    Note = payload.Note
                });

                _session.Logout();
                DialogResult = DialogResult.Abort;
                _isLoggingOut = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error($"Lỗi chốt ca: {ex.Message}", owner: this);
            }
        };
    }

    private void SetupBillingEvents()
    {
        _ucBilling.OnPayClicked += async (s, e) => await ProcessPaymentAsync();

        _ucBilling.OnEditCartItem += async (s, cartItem) =>
        {
            var productQueryService = _serviceProvider.GetRequiredService<IProductQueryService>();
            var product = await productQueryService.GetProductByIdAsync(cartItem.ProductId);

            if (product == null)
            {
                MessageBoxHelper.Error("Không tìm thấy thông tin sản phẩm!", owner: this);
                return;
            }

            var customizationControl = new UC_ProductCustomization(cartItem, product, productQueryService);
            using var shell = new DynamicModalShell<CartItemDto>($"TUỲ CHỈNH - {product.Name}", customizationControl, new Size(500, 750), saveButtonText: "LƯU");

            if (shell.ShowDialog(this) == DialogResult.OK)
            {
                var updatedItem = shell.ExtractData();
                _ucBilling.UpdateCustomizedItem(cartItem, updatedItem);
            }
        };
    }

    private void SetupHistoryEvents()
    {
        _ucBillHistory.OnReprintClicked += async (s, bill) =>
        {
            if (!MessageBoxHelper.ConfirmYesNo($"Bạn muốn in lại hóa đơn #{bill.Id}?", "In lại", this)) return;

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
                MessageBoxHelper.Error($"Lỗi in hóa đơn: {ex.Message}", "LỖI HỆ THỐNG", this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        };

        _ucBillHistory.OnDetailsRequested += async (s, historyBill) =>
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
                    ?? new BillReportDto(historyBill.Id, historyBill.BuzzerNumber, historyBill.TotalAmount, historyBill.CreatedAt, _session.CurrentUser?.FullName ?? "N/A", false, null);

                var detailControl = new UC_BillDetail(reportBill, details);
                using var shell = new DynamicModalShell<bool>($"CHI TIẾT HOÁ ĐƠN #{reportBill.Id}", detailControl, new Size(900, 620), showSaveButton: false, cancelButtonText: "ĐÓNG");
                shell.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error($"Lỗi tải chi tiết: {ex.Message}", owner: this);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        };
    }

    private void SetupMenuEvents()
    {
        _ucMenu.OnProductSelected += async (prodId, prodName, price, imageIdentifier) =>
        {
            var productQueryService = _serviceProvider.GetRequiredService<IProductQueryService>();
            var product = await productQueryService.GetProductByIdAsync(prodId);

            if (product == null)
            {
                MessageBoxHelper.Error("Không tìm thấy thông tin sản phẩm!", owner: this);
                return;
            }

            var customizationControl = new UC_ProductCustomization(product, productQueryService);
            using var shell = new DynamicModalShell<CartItemDto>($"TUỲ CHỈNH - {product.Name}", customizationControl, new Size(500, 750), saveButtonText: "LƯU");

            if (shell.ShowDialog(this) == DialogResult.OK)
            {
                var cartItem = shell.ExtractData();
                _ucBilling.AddCustomizedItemToBill(cartItem);
            }
        };
    }

    private async Task ProcessPaymentAsync()
    {
        if (_isProcessingPayment) return;

        var cartItems = _ucBilling.GetCartItems();
        if (cartItems.Count == 0)
        {
            MessageBoxHelper.Warning("Chưa có món nào trong giỏ!", owner: this);
            return;
        }

        // HACK: VisualBasic InputBox in a modern AntdUI app. Should be refactored to a native custom modal.
        string input = Microsoft.VisualBasic.Interaction.InputBox("Nhập số Thẻ Rung / Số thứ tự:", "Xác nhận Order", "1");
        if (!int.TryParse(input, out int buzzerNumber)) return;

        decimal finalAmount = _ucBilling.GrandTotal;
        if (!MessageBoxHelper.ConfirmYesNo($"Thu của khách {finalAmount:N0} đ?", "Xác nhận", this))
            return;

        try
        {
            _isProcessingPayment = true;
            var command = new CreateBillDto(buzzerNumber, _session.CurrentUser!.Id, finalAmount, [.. cartItems]);
            int billId = await _billService.ProcessFullOrderAsync(command);

            await _pdfQueue.EnqueueJobAsync(new BillPrintPayload
            {
                BillId = billId,
                BuzzerNumber = buzzerNumber,
                TotalAmount = finalAmount,
                Details = [.. cartItems.Select(i => new BillDetailDto(i.ProductId, i.ProductName, i.Quantity, i.Price, i.Note))]
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
        base.OnFormClosing(e);
    }
}
