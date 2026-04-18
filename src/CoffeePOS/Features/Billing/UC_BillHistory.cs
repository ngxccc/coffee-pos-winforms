using AntdUI;
using CoffeePOS.Shared.Dtos;
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
            new Column(nameof(BillHistoryViewModel.Id), DtoInfo.GetName<BillHistoryDto>(nameof(BillHistoryDto.Id)))
            {
                Width = "100",
                Align = ColumnAlign.Center
            },
            new Column(nameof(BillHistoryViewModel.CreatedAt), DtoInfo.GetName<BillHistoryDto>(nameof(BillHistoryDto.CreatedAt)))
            {
                Width = "120",
                DisplayFormat = "HH:mm:ss",
                Align = ColumnAlign.Center
            },
            new Column(nameof(BillHistoryViewModel.TotalAmount), DtoInfo.GetName<BillHistoryDto>(nameof(BillHistoryDto.TotalAmount)))
            {
                Width = "150",
                Align = ColumnAlign.Right,
                DisplayFormat = "N0"
            },
            new Column("Actions", "Thao tác")
            {
                Width = "180",
                Align = ColumnAlign.Center
            }
        ];

        _tableBills.CellButtonClick += TableBills_CellButtonClick;
    }

    public void BindData(List<BillHistoryDto> bills)
    {
        // PERF: Mapping O(N). Nếu N > 10,000 cần cân nhắc Virtualization thay vì init toàn bộ mảng CellButton.
        var viewModels = bills.Select(b => new BillHistoryViewModel(b)).ToList();

        _tableBills.DataSource = viewModels;

        // HACK: Tận dụng Dictionary để render Summary row ở bottom grid theo key của Column
        _tableBills.Summary = new Dictionary<string, object>
        {
            { nameof(BillHistoryViewModel.Id), $"Tổng: {bills.Count} đơn" },
            { nameof(BillHistoryViewModel.TotalAmount), bills.Sum(x => x.TotalAmount) }
        };
    }

    private void TableBills_CellButtonClick(object? sender, TableButtonEventArgs e)
    {
        if (e.Record is not BillHistoryViewModel vm) return;

        switch (e.Btn.Id)
        {
            case "btnReprint":
                OnReprintClicked?.Invoke(this, vm.OriginalDto);
                break;
            case "btnView":
                OnDetailsRequested?.Invoke(this, vm.OriginalDto);
                break;
        }
    }

    // WHY: DTO là record bất biến, không được chứa UI Logic.
    // ViewModel này đóng vai trò cầu nối để cung cấp AntdUI.CellLink[] cho Column "Actions"
    private class BillHistoryViewModel : NotifyProperty
    {
        public BillHistoryDto OriginalDto { get; }

        public BillHistoryViewModel(BillHistoryDto dto)
        {
            OriginalDto = dto;

            Actions =
            [
                new CellButton("btnView", "Xem", TTypeMini.Primary) { Radius = 4 },
                new CellButton("btnReprint", "In lại", TTypeMini.Default) { Radius = 4 }
            ];
        }

        public int Id => OriginalDto.Id;
        public DateTime CreatedAt => OriginalDto.CreatedAt;
        public decimal TotalAmount => OriginalDto.TotalAmount;

        public CellLink[] Actions { get; set; }
    }
}
