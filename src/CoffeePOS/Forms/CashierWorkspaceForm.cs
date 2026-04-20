using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Products;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Dtos.ShiftReport;
using CoffeePOS.Shared.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeePOS.Forms;

public partial class CashierWorkspaceForm : Window
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

        _ucSidebar = _serviceProvider.GetRequiredService<UC_Sidebar>();
        _ucBilling = _serviceProvider.GetRequiredService<UC_Billing>();
        _ucMenu = _serviceProvider.GetRequiredService<UC_Menu>();
        _ucBillHistory = _serviceProvider.GetRequiredService<UC_BillHistory>();

        InitializeComponent();
        AssembleLayout(_ucSidebar, _ucBilling, _ucBillHistory, _ucMenu);

        BindInitialData();
        SetupSidebarEvents();
        SetupBillingEvents();
        SetupHistoryEvents();
        SetupMenuEvents();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        AntdUI.Message.info(this, $"Chào mừng {_session.CurrentUser!.FullName} đến với ca làm việc của mình!");
    }

    private void BindInitialData()
    {
        _lblUserInfo.Text = $"Ca trực: {_session.CurrentUser?.FullName ?? "N/A"}";
    }

    private void SetupSidebarEvents()
    {
        // HANDLER ON HOME CLICK
        _ucSidebar.OnHomeClicked += (s, e) => _ucMenu.BringToFront();

        // HANDLER ON BILL HISTORY CLICK
        _ucSidebar.OnBillHistoryClicked += async (s, e) =>
        {
            var todayBills = await _billQueryService.GetTodayBillsByUserAsync(_session.CurrentUser!.Id);
            _ucBillHistory.BindData(todayBills);

            _ucBillHistory.BringToFront();
        };

        // HANDLER ON PROFILES CLICK
        _ucSidebar.OnProfilesClicked += (s, e) =>
        {
            var profilesControl = _formFactory.CreateControl<UC_Profiles>();

            var config = new Modal.Config(this, "THÔNG TIN CÁ NHÂN", profilesControl)
            {
                OkText = "CẬP NHẬT",
                CancelText = "HUỶ",
                BtnHeight = 50,

                OnOk = (cfg) =>
                {
                    if (!profilesControl.ValidateInput()) return false;

                    var payload = profilesControl.GetPayload();

                    _ = ExecutePasswordChangeAsync(payload);

                    return _isLoggingOut;
                }
            };

            AntdUI.Modal.open(config);
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
        _ucBilling.OnPayClicked += async (s, e) => ProcessPaymentAsync();

        _ucBilling.OnEditCartItem += async (s, cartItem) =>
        {
            try
            {
                var productQueryService = _serviceProvider.GetRequiredService<IProductQueryService>();
                var product = await productQueryService.GetProductByIdAsync(cartItem.ProductId);

                if (product == null)
                {
                    MessageBoxHelper.Error("Không tìm thấy thông tin sản phẩm!", owner: this);
                    return;
                }

                var hostForm = FindForm();
                if (hostForm == null) return;

                var customizationControl = new UC_ProductCustomization(cartItem, product, productQueryService);
                var drawerShell = new DynamicDrawerShell<CartItemDto>($"SỬA: {product.Name.ToUpper()}", customizationControl, 480);

                drawerShell.OnSaved += (updatedItem) =>
                {
                    _ucBilling.UpdateItem(cartItem, updatedItem);
                };

                Drawer.open(new Drawer.Config(hostForm, drawerShell)
                {
                    Align = TAlignMini.Right,
                    MaskClosable = true,
                    Padding = 0
                });
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error($"Lỗi tải tuỳ chỉnh: {ex.Message}", owner: this);
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

                var config = new Modal.Config(this, $"CHI TIẾT HOÁ ĐƠN #{reportBill.Id}", detailControl)
                {
                    OkText = "Đóng",
                    CancelText = null,
                };
                AntdUI.Modal.open(config);
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
            try
            {
                var productQueryService = _serviceProvider.GetRequiredService<IProductQueryService>();
                var product = await productQueryService.GetProductByIdAsync(prodId);

                if (product == null)
                {
                    MessageBoxHelper.Error("Không tìm thấy thông tin sản phẩm!", owner: this);
                    return;
                }

                var hostForm = FindForm();
                if (hostForm == null) return;

                var customizationControl = new UC_ProductCustomization(product, productQueryService);

                var drawerShell = new DynamicDrawerShell<CartItemDto>(
                    $"TÙY CHỈNH: {product.Name.ToUpper()}",
                    customizationControl,
                    width: 480
                );

                drawerShell.OnSaved += (cartItemPayload) =>
                {
                    if (_mainSplitter.SplitPanelState == false)
                        _mainSplitter.SplitPanelState = true;
                    _ucBilling.AddItem(cartItemPayload);
                };

                Drawer.open(new Drawer.Config(hostForm, drawerShell)
                {
                    Align = TAlignMini.Right,
                    MaskClosable = true,
                    Padding = 0
                });
            }
            catch (Exception ex)
            {
                MessageBoxHelper.Error($"Lỗi mở tuỳ chỉnh món: {ex.Message}", owner: this);
            }
        };
    }

    private void ProcessPaymentAsync()
    {
        if (_isProcessingPayment) return;

        var cartItems = _ucBilling.GetCartItems();
        if (cartItems.Count == 0)
        {
            MessageBoxHelper.Warning("Chưa có món nào trong giỏ!", owner: this);
            return;
        }

        decimal finalAmount = _ucBilling.GrandTotal;
        var confirmControl = new UC_PaymentConfirm(finalAmount);

        var config = new Modal.Config(this, "XÁC NHẬN THANH TOÁN", confirmControl)
        {
            OkText = "Xác nhận",
            CancelText = "Huỷ",
            MaskClosable = false,
            OnOk = (cfg) =>
            {
                int buzzerNumber = confirmControl.BuzzerNumber;

                if (buzzerNumber <= 0)
                {
                    AntdUI.Message.warn(this, "Số thẻ rung không hợp lệ!");
                    return false;
                }

                _ = ExecutePaymentTransactionAsync(buzzerNumber, finalAmount, cartItems);

                return true;
            }
        };

        AntdUI.Modal.open(config);
    }

    private async Task ExecutePaymentTransactionAsync(int buzzerNumber, decimal finalAmount, List<CreateBillItemDto> cartItems)
    {
        try
        {
            _isProcessingPayment = true;

            AntdUI.Message.loading(this, "Đang xử lý đơn hàng...", async config =>
            {
                var command = new CreateBillDto(buzzerNumber, _session.CurrentUser!.Id, finalAmount, cartItems);
                int billId = await _billService.ProcessFullOrderAsync(command);

                config.ID = billId.ToString();

                await _pdfQueue.EnqueueJobAsync(new BillPrintPayload
                {
                    BillId = billId,
                    BuzzerNumber = buzzerNumber,
                    TotalAmount = finalAmount,
                    Details = [.. cartItems.Select(i => new BillDetailDto{
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Price = i.Price,
                        Note = i.Note
                    })]
                });

                Invoke(new Action(() =>
                {
                    _ucBilling.ClearOrder();
                    AntdUI.Message.close_id(billId.ToString());
                    AntdUI.Message.success(this, "Thanh toán và tạo hóa đơn thành công!");
                }));
            });
        }
        catch (Exception ex)
        {
            AntdUI.Message.error(this, $"Lỗi thanh toán: {ex.Message}");
        }
        finally
        {
            _isProcessingPayment = false;
        }
    }

    private async Task ExecutePasswordChangeAsync(ChangePasswordPayload payload)
    {
        AntdUI.Message.loading(this, "Đang đổi mật khẩu...", async config =>
        {
            config.ID = "change_pass";

            try
            {
                await _userService.ChangePasswordAsync(
                    _session.CurrentUser!.Id,
                    _session.CurrentUser.Username,
                    payload
                );

                Invoke(new Action(() =>
                {
                    _session.Logout();
                    DialogResult = DialogResult.Abort;
                    _isLoggingOut = true;
                    _ucBilling.ClearOrder();
                    AntdUI.Message.close_id("change_pass");
                    MessageBoxHelper.Info("Đổi mật khẩu thành công! Hệ thống sẽ đăng xuất.");
                    Close();
                }));
            }
            catch (Exception ex)
            {
                AntdUI.Message.close_id("change_pass");
                AntdUI.Message.error(this, $"Lỗi đổi mật khẩu: {ex.Message}");
            }
        });
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
