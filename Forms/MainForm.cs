using CoffeePOS.Core;
using CoffeePOS.Data.Repositories;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Products;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Features.Tables;
using Microsoft.Extensions.DependencyInjection;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Forms;

public partial class MainForm : Form
{
    // DEPENDENCIES & CONTROLS
    private readonly IBillRepository _billRepo;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITableRepository _tableRepo;

    // UI Components
    private readonly UC_Sidebar _ucSidebar = new();
    private readonly UC_Billing _ucBilling = new();
    private UC_Menu? _ucMenu;
    private Panel _pnlMainWorkspace = new();
    private FlowLayoutPanel _flowTableList = new();

    // Logic Components
    private System.Windows.Forms.Timer? _masterTimer;

    private readonly Dictionary<int, UC_Table> _tableMap = [];
    private readonly List<UC_Table> _activeTables = [];

    // CONSTRUCTOR & INIT
    public MainForm(IServiceProvider serviceProvider,
                    IBillRepository billRepo,
                    ITableRepository tableRepo)
    {
        InitializeFormProperties();

        _serviceProvider = serviceProvider;
        _billRepo = billRepo;
        _tableRepo = tableRepo;

        // Setup các thành phần giao diện
        SetupTimer();
        SetupSidebar();
        SetupBilling();
        SetupWorkspace();

        // Ráp nối Layout
        AssembleLayout();

        // Load dữ liệu bàn
        LoadTableMap();
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

    private void SetupTimer()
    {
        _masterTimer = new System.Windows.Forms.Timer { Interval = 60000 };
        _masterTimer.Tick += MasterTimer_Tick;
        _masterTimer.Start();
    }

    private void SetupSidebar()
    {
        _ucSidebar.OnHomeClicked += (s, e) => SwitchToHome();
    }

    private void SetupBilling()
    {
        _ucBilling.OnPayClicked += (s, e) => ProcessPayment();
    }

    private void SetupWorkspace()
    {
        _pnlMainWorkspace = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        _flowTableList = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
        };

        _pnlMainWorkspace.Controls.Add(_flowTableList);
    }

    private void AssembleLayout()
    {
        Controls.Add(_ucSidebar);
        Controls.Add(_ucBilling);
        Controls.Add(_pnlMainWorkspace);

        _pnlMainWorkspace.BringToFront();
    }

    private void LoadTableMap()
    {
        _flowTableList.Controls.Clear();
        _tableMap.Clear();
        _activeTables.Clear();

        var tables = _tableRepo.GetAllTables();

        foreach (var t in tables)
        {
            UC_Table ucTable = new(t.Id, t.Name, TableStatus.Empty);

            int paidBillId = _billRepo.GetCurrentPaidBillId(t.Id);

            if (paidBillId > 0)
            {
                ucTable.Status = TableStatus.Occupied;
                var startTime = _billRepo.GetBillStartTime(paidBillId);
                ucTable.StartTime = startTime;

                ucTable.UpdateColor();
                ucTable.UpdateDuration();
                _activeTables.Add(ucTable);
            }

            ucTable.Click += (s, e) => HandleTableClick(ucTable);
            _tableMap.Add(t.Id, ucTable);
            _flowTableList.Controls.Add(ucTable);
        }
    }

    // BUSINESS LOGIC

    private void HandleTableClick(UC_Table table)
    {
        bool isOccupied = table.Status == TableStatus.Occupied;

        if (isOccupied)
        {
            // --- BÀN CÓ KHÁCH ---
            DialogResult result = MessageBox.Show(
                $"Bàn {table.TableId} đang có khách.\n\n" +
                "- YES: Order thêm món (Bill mới)\n" +
                "- NO: Khách đã về (Dọn bàn)",
                "Quản lý bàn",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                // Dọn bàn
                _billRepo.ClearTable(table.TableId);
                ResetTableStatus(table);
                _ucBilling.ClearOrder();
            }
            else if (result == DialogResult.Yes)
            {
                // Gọi thêm
                _ucBilling.SetTableInfo(table.TableId, $"Bàn {table.TableId} (Gọi thêm)");
                ShowMenu();
            }

        }
        else
        {
            // --- BÀN TRỐNG ---
            _ucBilling.SetTableInfo(table.TableId, $"Order cho Bàn {table.TableId}");
            int draftBillId = _billRepo.GetCurrentUnpaidBillId(table.TableId);

            if (draftBillId > 0)
            {
                LoadExistingBillToUI(table.TableId);
            }
            ShowMenu();
        }
    }

    private void LoadExistingBillToUI(int tableId)
    {
        int billId = _billRepo.GetCurrentUnpaidBillId(tableId);
        if (billId == 0) return;

        var details = _billRepo.GetBillDetails(billId);

        foreach (var d in details)
        {
            _ucBilling.AddItemToBill(d.ProductId, d.ProductName, d.Quantity, d.Price, d.Note);
        }
    }

    private void ProcessPayment()
    {
        int tableId = _ucBilling.CurrentTableId;

        if (tableId == 0)
        {
            MessageBox.Show("Vui lòng chọn bàn trước!");
            return;
        }

        int billId = _billRepo.GetCurrentUnpaidBillId(tableId);
        if (billId == 0)
        {
            MessageBox.Show("Bàn này không có hóa đơn nào để thanh toán!");
            return;
        }

        decimal finalAmount = _ucBilling.GrandTotal;

        if (MessageBox.Show($"Xác nhận thanh toán {finalAmount:N0} đ?", "Thanh toán", MessageBoxButtons.YesNo) == DialogResult.No)
        {
            return;
        }

        _billRepo.Checkout(billId, finalAmount);

        if (_tableMap.TryGetValue(tableId, out UC_Table? targetTable))
        {
            targetTable.Status = TableStatus.Occupied;
            targetTable.StartTime = TimeKeeper.Now;

            targetTable.UpdateColor();
            targetTable.UpdateDuration();

            // tránh add trùng
            if (!_activeTables.Contains(targetTable))
            {
                _activeTables.Add(targetTable);
            }

            MessageBox.Show($"Thanh toán Bàn {tableId} thành công!");

            ShowTableMap();
        }
        else
        {
            MessageBox.Show("Lỗi: Không tìm thấy bàn này trong bản đồ!");
        }

        _ucBilling.ClearOrder();
    }

    private void ResetTableStatus(UC_Table table)
    {
        table.Status = TableStatus.Empty;
        table.StartTime = null;
        table.UpdateColor();
        table.UpdateDuration();

        _activeTables.Remove(table);
    }

    private void SwitchToHome()
    {
        ShowTableMap();
    }

    private void MasterTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var table in _activeTables)
        {
            table.UpdateDuration();
        }
    }

    private void ShowMenu()
    {
        if (_ucMenu == null || _ucMenu.IsDisposed)
        {
            _ucMenu = _serviceProvider.GetRequiredService<UC_Menu>();
            _ucMenu.OnBackClicked += (s, e) => ShowTableMap();

            _ucMenu.OnProductSelected += (prodId, prodName, price) =>
            {
                int tableId = _ucBilling.CurrentTableId;
                if (tableId == 0) return;

                // A. Kiểm tra xem bàn đã có Bill chưa? Chưa thì tạo mới.
                int billId = _billRepo.GetCurrentUnpaidBillId(tableId);
                if (billId == 0)
                {
                    billId = _billRepo.CreateBill(tableId);
                }

                // B. Lưu món vào DB (Bill Details)
                // Lưu ý: Hàm này sẽ tự cộng dồn số lượng nếu trùng
                _billRepo.AddBillDetail(billId, prodId, prodName, 1, price);

                // C. Hiển thị lên UI (UC_Billing)
                _ucBilling.AddItemToBill(prodId, prodName, 1, price);
            };
        }

        _pnlMainWorkspace.Controls.Clear();
        _pnlMainWorkspace.Controls.Add(_ucMenu);
    }

    private void ShowTableMap()
    {
        _ucBilling.ClearOrder();
        _pnlMainWorkspace.Controls.Clear();
        _pnlMainWorkspace.Controls.Add(_flowTableList);
    }
}
