using AntdUI;

using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Enums;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillHistory : UserControl
{
    public event EventHandler<BillHistoryDto>? OnReprintClicked;
    public event EventHandler<BillHistoryDto>? OnDetailsRequested;
    public event EventHandler<BillHistoryDto>? OnCancelRequested;
    public event EventHandler<BillHistoryDto>? OnRestoreRequested;

    public UC_BillHistory()
    {
        InitializeComponent();
        SetupTable();
    }

    private void SetupTable()
    {
        _tableBills.Columns =
        [
            DtoHelper.CreateCol<BillHistoryDto>(nameof(BillHistoryDto.Id), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillHistoryDto>(nameof(BillHistoryDto.TotalItems), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillHistoryDto>(nameof(BillHistoryDto.CreatedAt), c =>
            {
                c.DisplayFormat = "HH:mm:ss";
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillHistoryDto>(nameof(BillHistoryDto.TotalAmount), c =>
            {
                c.Align = ColumnAlign.Right;
                c.DisplayFormat = "N0";
                c.SortOrder = true;
            }),
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Render = (value, record, rowIndex) => {
                    if (record is not BillHistoryDto bill) return null;

                    var buttons = new List<CellButton>
                    {
                        new("view", "Xem", TTypeMini.Primary),
                        new("reprint", "In lại", TTypeMini.Default)
                    };

                    if (bill.Status == BillStatus.Canceled)
                    {
                        buttons.Add(new("restore", "Khôi phục", TTypeMini.Success));
                    }
                    else
                    {
                        buttons.Add(new("cancel", "Hủy", TTypeMini.Error));
                    }

                    return buttons.ToArray();
                }
            }
        ];

        _tableBills.CellButtonClick += TableBills_CellButtonClick;
    }

    public void BindData(List<BillHistoryDto> bills)
    {
        _tableBills.DataSource = bills;

        _tableBills.Summary = new Dictionary<string, object>
        {
            { nameof(BillHistoryDto.Id), $"Tổng: {bills.Count} đơn" },
            { nameof(BillHistoryDto.TotalAmount), bills.Sum(x => x.TotalAmount) }
        };
    }

    private void TableBills_CellButtonClick(object? sender, TableButtonEventArgs e)
    {
        if (e.Record is not BillHistoryDto selectedBill) return;

        switch (e.Btn.Id)
        {
            case "view":
                OnDetailsRequested?.Invoke(this, selectedBill);
                break;
            case "reprint":
                OnReprintClicked?.Invoke(this, selectedBill);
                break;
            case "cancel":
                OnCancelRequested?.Invoke(this, selectedBill);
                break;
            case "restore":
                OnRestoreRequested?.Invoke(this, selectedBill);
                break;
        }
    }
}
