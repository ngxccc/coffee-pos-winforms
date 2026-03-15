using CoffeePOS.Core;
using CoffeePOS.Data.Repositories;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Products;
using CoffeePOS.Features.Sidebar;
using Microsoft.Extensions.DependencyInjection;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Forms;

public partial class MainForm : Form
{
    // DEPENDENCIES & CONTROLS
    private readonly IBillRepository _billRepo;
    private readonly IServiceProvider _serviceProvider;
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

    // CONSTRUCTOR & INIT
    public MainForm(IServiceProvider serviceProvider, IUserSession session,
                    IBillRepository billRepo, PdfPrintQueue pdfQueue)
    {
        InitializeUI();

        _serviceProvider = serviceProvider;
        _session = session;
        _billRepo = billRepo;
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
            var todayBills = await _billRepo.GetTodayBillsByUserAsync(_session.CurrentUser!.Id);
            _ucBillHistory.BindData(todayBills);

            _ucMenu.Visible = false;
            _ucBillHistory.Visible = true;
            _ucBillHistory.BringToFront();
        };

        _ucSidebar.OnSettingsClicked += (s, e) =>
        {
            var settingForm = _serviceProvider.GetRequiredService<SettingForm>();
            if (settingForm.ShowDialog() == DialogResult.OK)
            {
                _session.Logout();
                DialogResult = DialogResult.Abort;
                Close();
            }
        };

        _ucSidebar.OnLogoutClicked += (s, e) =>
        {
            if (_ucBilling.HasUnpaidItems)
            {
                MessageBox.Show("Giỏ hàng đang có món chưa thanh toán!\nVui lòng hoàn tất hoặc xóa giỏ hàng trước khi đăng xuất.",
                    "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var shiftForm = _serviceProvider.GetRequiredService<ShiftReportForm>();
            if (shiftForm.ShowDialog() == DialogResult.OK)
            {
                _session.Logout();
                DialogResult = DialogResult.Abort;
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
            if (MessageBox.Show($"Bạn muốn in lại hóa đơn #{bill.Id}?", "In lại", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    var billItems = await _billRepo.GetBillDetailsAsync(bill.Id);

                    await _pdfQueue.EnqueueJobAsync(new PdfJobPayload
                    {
                        BillId = bill.Id,
                        BuzzerNumber = bill.BuzzerNumber,
                        TotalAmount = bill.TotalAmount,
                        Details = billItems,
                        IsReprint = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi in lại hóa đơn: {ex.Message}", "LỖI HỆ THỐNG", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        };
    }

    private void SetupMenu()
    {
        _ucMenu.OnProductSelected += (prodId, prodName, price) =>
        {
            _ucBilling.AddItemToBill(prodId, prodName, 1, price);
        };
    }

    // BUSINESS LOGIC

    private async Task ProcessPaymentAsync()
    {
        if (_isProcessingPayment) return;

        var cartItems = _ucBilling.GetCartItems();
        if (cartItems.Count == 0)
        {
            MessageBox.Show("Chưa có món nào trong giỏ!");
            return;
        }

        string input = Microsoft.VisualBasic.Interaction.InputBox("Nhập số Thẻ Rung / Số thứ tự:", "Xác nhận Order", "1");
        if (!int.TryParse(input, out int buzzerNumber)) return;

        decimal finalAmount = _ucBilling.GrandTotal;
        if (MessageBox.Show($"Thu của khách {finalAmount:N0} đ?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.No)
            return;

        try
        {
            _isProcessingPayment = true;

            int billId = await _billRepo.ProcessFullOrderAsync(buzzerNumber, finalAmount, cartItems);

            await _pdfQueue.EnqueueJobAsync(new PdfJobPayload
            {
                BillId = billId,
                BuzzerNumber = buzzerNumber,
                TotalAmount = finalAmount,
                Details = cartItems
            });

            _ucBilling.ClearOrder();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi thanh toán: {ex.Message}");
        }
        finally
        {
            _isProcessingPayment = false;
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            MessageBox.Show("Vui lòng thoát bằng nút đăng xuất để chốt ca làm việc!",
                    "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.Cancel = true;
            return;

            // if (_ucBilling.HasUnpaidItems)
            // {
            //     MessageBox.Show("Không thể tắt phần mềm khi đang có hóa đơn chưa thanh toán!",
            //         "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);

            //     // Thuộc tính Cancel = true sẽ HỦY BỎ lệnh tắt Form
            //     e.Cancel = true;
            //     return;
            // }
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
            Console.WriteLine($"Clock Error: {ex.Message}");
        }
    }
}
