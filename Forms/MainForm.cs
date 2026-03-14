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
    private readonly UC_Sidebar _ucSidebar = new();
    private readonly UC_Billing _ucBilling = new();
    private UC_Menu _ucMenu = null!;
    private readonly Label _lblUserInfo = new();

    // Logic Components

    private bool _isProcessingPayment = false;

    // CONSTRUCTOR & INIT
    public MainForm(IServiceProvider serviceProvider, IUserSession session,
                    IBillRepository billRepo, PdfPrintQueue pdfQueue)
    {
        InitializeFormProperties();

        _serviceProvider = serviceProvider;
        _session = session;
        _billRepo = billRepo;
        _pdfQueue = pdfQueue;

        // Setup các thành phần giao diện
        SetupSidebar();
        SetupBilling();
        SetupHeader();

        // Ráp nối Layout
        AssembleLayout();

        LoadMenuDirectly();
    }

    // UI SETUP METHODS

    private void InitializeFormProperties()
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

        UpdateUserInfoUI();
    }

    private void SetupSidebar()
    {
        _ucSidebar.OnHomeClicked += (s, e) => _ucBilling.ClearOrder();

        _ucSidebar.OnSettingsClicked += (s, e) =>
        {
            var settingForm = _serviceProvider.GetRequiredService<SettingForm>();
            settingForm.ShowDialog();
        };

        _ucSidebar.OnLogoutClicked += (s, e) =>
        {
            if (_ucBilling.HasUnpaidItems)
            {
                MessageBox.Show("Giỏ hàng đang có món chưa thanh toán!\nVui lòng hoàn tất hoặc xóa giỏ hàng trước khi đăng xuất.",
                    "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn chắc chắn muốn đăng xuất?", "Đăng xuất",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        };
    }

    private void SetupBilling()
    {
        _ucBilling.OnPayClicked += async (s, e) => await ProcessPaymentAsync();
    }

    private void AssembleLayout()
    {
        Controls.Add(_ucSidebar);
        Controls.Add(_ucBilling);
        Controls.Add(_lblUserInfo);
    }

    private void LoadMenuDirectly()
    {
        _ucMenu = _serviceProvider.GetRequiredService<UC_Menu>();

        _ucMenu.OnProductSelected += (prodId, prodName, price) =>
        {
            _ucBilling.AddItemToBill(prodId, prodName, 1, price);
        };

        _ucMenu.Dock = DockStyle.Fill;
        Controls.Add(_ucMenu);
        _ucMenu.BringToFront();
    }

    private void UpdateUserInfoUI()
    {
        if (_session.IsLoggedIn)
        {
            _lblUserInfo.Text = $"Ca trực: {_session.CurrentUser!.FullName}";
        }
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
            if (_ucBilling.HasUnpaidItems)
            {
                MessageBox.Show("Không thể tắt phần mềm khi đang có hóa đơn chưa thanh toán!",
                    "CẢNH BÁO", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Thuộc tính Cancel = true sẽ HỦY BỎ lệnh tắt Form
                e.Cancel = true;
                return;
            }
        }

        base.OnFormClosing(e);
    }
}
