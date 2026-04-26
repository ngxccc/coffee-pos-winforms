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

    [Required(ErrorMessage = "Vui lòng nhập số tiền thực tế.")]
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền thực tế không hợp lệ.")]
    public decimal StartingCash { get; init; }

    public decimal ExpectedCash { get; init; }

    [Required(ErrorMessage = "Vui lòng nhập số tiền thực tế.")]
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền thực tế không hợp lệ.")]
    public decimal ActualCash { get; init; }

    public decimal Difference => ActualCash - (ExpectedCash + StartingCash);

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

        _lblHeader.Text = $"Nhân viên: {_session.CurrentUser?.FullName}";
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (_loaded) return;

        _loaded = true;
        await LoadDataAsync();
    }

    public bool ValidateInput()
    {
        if (!_loaded)
        {
            MessageBoxHelper.Warning("Dữ liệu chốt ca chưa sẵn sàng, vui lòng đợi thêm.", owner: this, type: FeedbackType.Message);
            return false;
        }

        ShiftReportPayload payload = GetPayload();

        if (payload.Difference != 0 && string.IsNullOrWhiteSpace(payload.Note))
        {
            MessageBoxHelper.Warning("Phát hiện lệch tiền. Bắt buộc nhập ghi chú (VD: Rớt tiền, trả tiền rác...).", owner: this, type: FeedbackType.Message);
            _txtNote.Focus();
            return false;
        }

        return true;
    }

    public ShiftReportPayload GetPayload()
    {
        return new ShiftReportPayload
        {
            EndTime = _endTime,
            TotalBills = _totalBills,
            ExpectedCash = _expectedCash,
            StartingCash = _numStartingCash.Value,
            ActualCash = _numActualCash.Value,
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
            // _lblExpectedCash.Text = "Tiền trên hệ thống: [ĐÃ ẨN]";
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu chốt ca: {ex.Message}", owner: this);
        }
    }
}
