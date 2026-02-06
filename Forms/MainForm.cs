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
    public MainForm(IServiceProvider serviceProvider, IBillRepository billRepo)
    {
        InitializeFormProperties();

        _serviceProvider = serviceProvider;
        _billRepo = billRepo;

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

        for (int i = 1; i <= 60; i++)
        {
            UC_Table table = new(i, $"Bàn {i:00}", TableStatus.Empty);

            table.Click += (s, e) => HandleTableClick(table);

            _tableMap.Add(i, table);

            _flowTableList.Controls.Add(table);
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
            ShowMenu();
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

        // 1. Lưu DB (Giả lập)
        // _billRepo.CreateBill(...)

        if (_tableMap.TryGetValue(tableId, out UC_Table? targetTable))
        {
            targetTable.Status = TableStatus.Occupied;
            targetTable.StartTime = DateTime.Now;

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
            // DI CONTAINER: "Cho tao xin một cái UC_Menu đầy đủ phụ kiện!"
            _ucMenu = _serviceProvider.GetRequiredService<UC_Menu>();

            _ucMenu.OnProductSelected += (id, name, price) =>
            {
                _ucBilling.AddItemToBill(id, name, 1, price);
            };
            _ucMenu.OnBackClicked += (s, e) => ShowTableMap();
        }

        _pnlMainWorkspace.Controls.Clear();
        _pnlMainWorkspace.Controls.Add(_ucMenu);
    }

    private void ShowTableMap()
    {
        _pnlMainWorkspace.Controls.Clear();
        _pnlMainWorkspace.Controls.Add(_flowTableList);
    }
}
