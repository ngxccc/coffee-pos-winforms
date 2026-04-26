using AntdUI;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.ShiftReport;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageShiftReports : UserControl
{
    private readonly IShiftReportQueryService _shiftRepostQueryService;
    private List<ShiftReportDto> _rawList = [];

    public UC_ManageShiftReports(IShiftReportQueryService shiftRepostQueryService)
    {
        _shiftRepostQueryService = shiftRepostQueryService;
        InitializeComponent();
        SetupTable();

        _txtSearch.OnDebouncedTextChanged(300, () => Invoke(HandleSearch));
        Load += (s, e) => { _ = LoadDataAsync(); };
    }

    private void SetupTable()
    {
        _tableShiftReports.Columns =
        [
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.Id), c =>
                c.Align = ColumnAlign.Center),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.CashierName), c =>
                c.SortOrder = true),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.StartTime), c => {
                c.DisplayFormat = "dd/MM HH:mm";
                c.Align = ColumnAlign.Center;
            }),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.EndTime), c => {
                c.DisplayFormat = "dd/MM HH:mm";
                c.Align = ColumnAlign.Center;
            }),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.StartingCash), c => {
                c.DisplayFormat = "N0";
                c.Align = ColumnAlign.Right;
            }),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.ExpectedCash), c => {
                c.DisplayFormat = "N0";
                c.Align = ColumnAlign.Right;
            }),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.ActualCash), c => {
                c.DisplayFormat = "N0";
                c.Align = ColumnAlign.Right;
            }),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.Difference), c => {
                c.SortOrder = true;
                c.Align = ColumnAlign.Right;
                c.Render = (value, record, rowIndex) =>
                {
                    if (record is not ShiftReportDto dto) return value;

                    var label = new CellText($"{dto.Difference:N0}");
                    if (dto.Difference < 0) label.Fore = Color.Red;
                    else if (dto.Difference > 0) label.Fore = Color.Green;

                    return label;
                };
            }),
            DtoHelper.CreateCol<ShiftReportDto>(nameof(ShiftReportDto.Note))
        ];
    }

    private async Task LoadDataAsync()
    {
        await Spin.open(this, async cfg =>
        {
            try
            {
                _rawList = await _shiftRepostQueryService.GetAllShiftReportsAsync();
                Invoke(HandleSearch);
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", owner: this));
            }
        });
    }

    private void HandleSearch()
    {
        string kw = _txtSearch.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(kw))
        {
            _tableShiftReports.DataSource = _rawList;
            return;
        }

        // Tìm kiếm theo tên thu ngân hoặc ghi chú
        _tableShiftReports.DataSource = _rawList.Where(x =>
            x.CashierName.Contains(kw, StringComparison.CurrentCultureIgnoreCase) ||
            (x.Note != null && x.Note.Contains(kw, StringComparison.CurrentCultureIgnoreCase))
        ).ToList();
    }
}
