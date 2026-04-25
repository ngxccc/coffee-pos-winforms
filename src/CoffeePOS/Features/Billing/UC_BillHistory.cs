using AntdUI;

using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillHistory : UserControl
{
    public event EventHandler<BillHistoryDto>? OnReprintClicked;
    public event EventHandler<BillHistoryDto>? OnDetailsRequested;

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
                    return new CellButton[] {
                        new("view", "Xem", TTypeMini.Primary),
                        new("reprint", "In lại", TTypeMini.Default)
                    };
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

        if (e.Btn.Id == "view") OnDetailsRequested?.Invoke(this, selectedBill);
        if (e.Btn.Id == "reprint") OnReprintClicked?.Invoke(this, selectedBill);
    }
}
