using CoffeePOS.Data.Repositories;
using CoffeePOS.Features.Billing;
using CoffeePOS.Features.Sidebar;
using CoffeePOS.Features.Tables;
using Panel = System.Windows.Forms.Panel;

namespace CoffeePOS.Forms;

public partial class MainForm : Form
{
    private readonly IBillRepository _billRepo;
    private UC_Sidebar? _ucSidebar;
    private UC_Billing _ucBilling = new();
    private Panel? _pnlMainWorkspace;
    private readonly Dictionary<int, bool> _tableStates = [];

    private void InitializeComponent()
    {
        Text = "CoffeePOS - Code Chay Edition";
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 800);
        StartPosition = FormStartPosition.CenterScreen;
    }

    public MainForm(IBillRepository billRepo)
    {
        InitializeComponent();
        _billRepo = billRepo;
        InitLayout();
    }

    private new void InitLayout()
    {
        // SIDE PANEL
        _ucSidebar = new UC_Sidebar();
        _ucSidebar.OnHomeClicked += (s, e) => SwitchToHome();

        // BILLING PANEL
        // _ucBilling = new UC_Billing();

        _ucBilling.AddItemToBill(101, "Cafe Đen Đá", 1, 20000, "Ít đường");
        _ucBilling.AddItemToBill(101, "Cafe Đen Đá", 1, 20000, "Ít đường");
        _ucBilling.AddItemToBill(102, "Cafe Đen Đá XL", 1, 30000, "Nhiều đá");

        _ucBilling.OnPayClicked += (s, e) => ProcessPayment();

        // WORKSPACE
        _pnlMainWorkspace = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        // CONTROLS
        Controls.Add(_ucSidebar);

        Controls.Add(_ucBilling);

        LoadTableMap();

        Controls.Add(_pnlMainWorkspace);

        _pnlMainWorkspace.BringToFront();
    }

    private void LoadTableMap()
    {
        FlowLayoutPanel flowTableList = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20),
        };

        for (int i = 1; i <= 20; i++)
        {
            _tableStates[i] = false;

            UC_Table table = new(i, $"Bàn {i:00}", TableStatus.Empty);

            table.Click += (s, e) => HandleTableClick(table);

            flowTableList.Controls.Add(table);
        }

        _pnlMainWorkspace?.Controls.Add(flowTableList);
    }

    private void HandleTableClick(UC_Table table)
    {
        bool isOccupied = _tableStates[table.TableId];

        if (isOccupied)
        {
            // KỊCH BẢN BÀN ĐỎ (ĐANG CÓ KHÁCH)
            DialogResult result = MessageBox.Show(
                $"Bàn {table.TableId} đang có khách.\n\n" +
                "- YES: Order thêm món (Bill mới)\n" +
                "- NO: Khách đã về (Dọn bàn)",
                "Quản lý bàn",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                // Dọn bàn -> Xanh
                _tableStates[table.TableId] = false;
                table.Status = TableStatus.Empty;
                table.UpdateColor();
                _ucBilling.ClearOrder();
                return;
            }
            else if (result == DialogResult.Yes)
            {
                // Order thêm -> Vẫn giữ bàn đỏ, nhưng reset bill bên phải để nhập món mới
                _ucBilling.SetTableInfo(table.TableId, $"Bàn {table.TableId} (Gọi thêm)");
            }
        }
        else
        {
            // KỊCH BẢN BÀN XANH (KHÁCH MỚI)
            _ucBilling.SetTableInfo(table.TableId, $"Order cho Bàn {table.TableId}");
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

        // 1. Lưu Bill xuống DB (Status = Paid)
        // _billRepo.CreateBill(...)

        // 2. Cập nhật trạng thái bàn -> ĐỎ
        _tableStates[tableId] = true;

        // 3. Update màu trên giao diện (Tìm cái UCTable tương ứng để đổi màu)
        // (Đây là điểm yếu của Code Chay: Phải loop tìm control để update)
        if (_pnlMainWorkspace?.Controls.Count > 0)
        {
            foreach (Control c in _pnlMainWorkspace.Controls[0].Controls) // Controls[0] là cái FlowLayout
            {
                if (c is UC_Table t && t.TableId == tableId)
                {
                    t.Status = TableStatus.Occupied;
                    t.UpdateColor();
                    break;
                }
            }
        }

        MessageBox.Show("Thanh toán thành công! In hóa đơn...");

        // 4. Reset Billing Panel để đón khách tiếp theo
        _ucBilling.ClearOrder();
    }

    private static void SwitchToHome()
    {
        MessageBox.Show("Đã chuyển sang trang Home");
    }
}
