using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Features.Billing;

public class UC_BillHistory : UserControl
{
    private DataGridView _dgvBills = null!;

    public event EventHandler<Bill>? OnReprintClicked;

    public UC_BillHistory()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;
        Padding = new Padding(20);
        Visible = false;

        Label lblTitle = new()
        {
            Text = "LỊCH SỬ HÓA ĐƠN TRONG NGÀY",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Top,
            Height = 50
        };

        _dgvBills = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.WhiteSmoke,
            RowTemplate = { Height = 40 },
            Font = new Font("Segoe UI", 11),
            RowHeadersVisible = false,
            ColumnHeadersHeight = 40
        };

        _dgvBills.CellClick += DgvBills_CellClick;

        Controls.Add(_dgvBills);
        Controls.Add(lblTitle);
    }

    public void BindData(List<Bill> bills)
    {
        _dgvBills.DataSource = null;
        _dgvBills.Columns.Clear();

        _dgvBills.DataSource = bills;

        _dgvBills.Columns["Id"].HeaderText = "Mã Đơn";
        _dgvBills.Columns["BuzzerNumber"].HeaderText = "Thẻ Rung";
        _dgvBills.Columns["TotalAmount"].HeaderText = "Tổng Tiền (đ)";
        _dgvBills.Columns["TotalAmount"].DefaultCellStyle.Format = "N0";
        _dgvBills.Columns["CreatedAt"].HeaderText = "Thời Gian";
        _dgvBills.Columns["CreatedAt"].DefaultCellStyle.Format = "HH:mm:ss";

        string[] nonVisibleColumns = [
            "UserId",
            "Status",
            "IsDeleted",
            "UpdatedAt",
            "OrderType",
            nameof(Bill.DeletedAt)
        ];
        foreach (var col in nonVisibleColumns)
            if (_dgvBills.Columns[col] != null) _dgvBills.Columns[col].Visible = false;

        DataGridViewButtonColumn btnReprint = new()
        {
            Name = "ReprintCol",
            HeaderText = "Thao tác",
            Text = "🖨️ In lại",
            UseColumnTextForButtonValue = true,
            FlatStyle = FlatStyle.Flat
        };
        _dgvBills.Columns.Add(btnReprint);
    }

    private void DgvBills_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
        {
            if (_dgvBills.Columns[e.ColumnIndex].Name == "ReprintCol")
            {
                var selectedBill = (Bill)_dgvBills.Rows[e.RowIndex].DataBoundItem;
                OnReprintClicked?.Invoke(this, selectedBill);
            }
        }
    }
}
