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
    private readonly PdfPrintQueue _pdfQueue;

    // UI Components
    private readonly UC_Sidebar _ucSidebar = new();
    private readonly UC_Billing _ucBilling = new();
    private UC_Menu _ucMenu = null!;

    // Logic Components

    private bool _isProcessingPayment = false;

    // CONSTRUCTOR & INIT
    public MainForm(IServiceProvider serviceProvider,
                    IBillRepository billRepo, PdfPrintQueue pdfQueue)
    {
        InitializeFormProperties();

        _serviceProvider = serviceProvider;
        _billRepo = billRepo;
        _pdfQueue = pdfQueue;

        // Setup các thành phần giao diện
        SetupSidebar();
        SetupBilling();

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

    private void SetupSidebar()
    {
        _ucSidebar.OnHomeClicked += (s, e) => _ucBilling.ClearOrder();

        // _ucSidebar.OnLogoutClicked += (s, e) =>
        // {
        //     if (MessageBox.Show("Bạn muốn đăng xuất ca làm việc?", "Đăng xuất", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //     {
        //         this.DialogResult = DialogResult.Abort; // Quăng cờ Đăng xuất
        //         this.Close(); // Đóng MainForm
        //     }
        // };
    }

    private void SetupBilling()
    {
        _ucBilling.OnPayClicked += async (s, e) => await ProcessPaymentAsync();
    }

    private void AssembleLayout()
    {
        Controls.Add(_ucSidebar);
        Controls.Add(_ucBilling);
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
}
