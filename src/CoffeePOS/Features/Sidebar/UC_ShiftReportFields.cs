using System.ComponentModel.DataAnnotations;
using AntdUI;
using CoffeePOS.Core;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public record ShiftReportPayload
{
    public DateTime EndTime { get; init; } = DateTime.Now;

    public int TotalBills { get; init; }

    public decimal ExpectedCash { get; init; }

    [Required(ErrorMessage = "Vui lòng nhập số tiền thực tế.")]
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền thực tế không hợp lệ.")]
    public decimal ActualCash { get; init; }

    public decimal Variance => ActualCash - ExpectedCash;

    public string Note { get; init; } = string.Empty;
}

public partial class UC_ShiftReportFields : UserControl, IValidatableComponent<ShiftReportPayload>
{
    private readonly IUserSession _session;
    private readonly IShiftReportQueryService _shiftReportQueryService;

    private int _totalBills;
    private decimal _expectedCash;
    private DateTime _endTime;
    private bool _loaded;

    public UC_ShiftReportFields(IUserSession session, IShiftReportQueryService shiftReportQueryService)
    {
        _session = session;
        _shiftReportQueryService = shiftReportQueryService;
        _endTime = DateTime.Now;

        InitializeComponent();

        // WHY: Bind dynamic session data after the UI DOM is constructed
        _lblHeader.Text = $"Nhân viên: {_session.CurrentUser?.FullName}";
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        await LoadDataAsync();
    }

    public bool ValidateInput()
    {
        Target target = new(this);

        if (!_loaded)
        {
            AntdUI.Message.warn(target, "Dữ liệu chốt ca chưa sẵn sàng, vui lòng đợi thêm.");
            return false;
        }

        ShiftReportPayload payload = GetPayload();

        if (!ValidationHelper.TryValidate(payload, out string error))
        {
            AntdUI.Message.warn(target, error);
            return false;
        }

        decimal variance = payload.ActualCash - _expectedCash;
        if (variance != 0 && string.IsNullOrWhiteSpace(_txtNote.Text))
        {
            AntdUI.Message.warn(target, "Phát hiện lệch tiền. Bắt buộc nhập ghi chú để quản lý kiểm tra.");
            _txtNote.Focus();
            return false;
        }

        return true;
    }

    public ShiftReportPayload GetPayload()
    {
        decimal actualCash = decimal.Parse(_txtActualCash.Text);

        return new ShiftReportPayload
        {
            EndTime = _endTime,
            TotalBills = _totalBills,
            ExpectedCash = _expectedCash,
            ActualCash = actualCash,
            Note = _txtNote.Text.Trim()
        };
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _endTime = DateTime.Now;
            var (TotalBills, ExpectedCash) = await _shiftReportQueryService.GetShiftSummaryAsync(_session.CurrentUser!.Id, _session.LoginTime!.Value, _endTime);
            _totalBills = TotalBills;
            _expectedCash = ExpectedCash;

            _lblTotalBills.Text = $"Tổng số hoá đơn: {_totalBills}";
            _lblExpectedCash.Text = "Tiền trên hệ thống: [ĐÃ ẨN]";
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu chốt ca: {ex.Message}", owner: this);
        }
    }
}
