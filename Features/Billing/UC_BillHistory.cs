using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public class UC_BillHistory : UserControl
{
    private DataGridView _dgvBills = null!;

    public event EventHandler<BillHistoryDto>? OnReprintClicked;
    public event EventHandler<BillHistoryDto>? OnDetailsRequested;

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
            Dock = DockStyle.Fill
        };
        _dgvBills.ApplyStandardAdminStyle();
        _dgvBills.CellClick += DgvBills_CellClick;
        _dgvBills.CellDoubleClick += DgvBills_CellDoubleClick;

        Controls.Add(_dgvBills);
        Controls.Add(lblTitle);
    }

    public void BindData(List<BillHistoryDto> bills)
    {
        _dgvBills.DataSource = null;
        _dgvBills.Columns.Clear();

        _dgvBills.DataSource = bills;

        _dgvBills.Columns[nameof(BillHistoryDto.TotalAmount)].DefaultCellStyle.Format = "N0";
        _dgvBills.Columns[nameof(BillHistoryDto.CreatedAt)].DefaultCellStyle.Format = "HH:mm:ss";

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
                var selectedBill = (BillHistoryDto)_dgvBills.Rows[e.RowIndex].DataBoundItem;
                OnReprintClicked?.Invoke(this, selectedBill);
            }
        }
    }

    private void DgvBills_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
        {
            return;
        }

        if (e.ColumnIndex >= 0 && _dgvBills.Columns[e.ColumnIndex].Name == "ReprintCol")
        {
            return;
        }

        if (_dgvBills.Rows[e.RowIndex].DataBoundItem is BillHistoryDto selectedBill)
        {
            OnDetailsRequested?.Invoke(this, selectedBill);
        }
    }
}
