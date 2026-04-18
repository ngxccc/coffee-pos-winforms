using CoffeePOS.Core;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public record ShiftReportPayload(DateTime EndTime, int TotalBills, decimal ExpectedCash, decimal ActualCash, decimal Variance, string Note);

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
        if (!_loaded)
        {
            MessageBoxHelper.Warning("Dữ liệu chốt ca chưa sẵn sàng, vui lòng đợi thêm.", owner: this);
            return false;
        }

        if (!decimal.TryParse(_txtActualCash.Text, out decimal actualCash))
        {
            MessageBoxHelper.Warning("Vui lòng nhập số tiền thực tế hợp lệ.", owner: this);
            return false;
        }

        decimal variance = actualCash - _expectedCash;
        if (variance != 0 && string.IsNullOrWhiteSpace(_txtNote.Text))
        {
            MessageBoxHelper.Warning("Phát hiện lệch tiền. Bắt buộc nhập ghi chú để quản lý kiểm tra.", owner: this);
            _txtNote.Focus();
            return false;
        }

        return true;
    }

    public ShiftReportPayload GetPayload()
    {
        decimal actualCash = decimal.Parse(_txtActualCash.Text);
        decimal variance = actualCash - _expectedCash;

        return new ShiftReportPayload(
            _endTime,
            _totalBills,
            _expectedCash,
            actualCash,
            variance,
            _txtNote.Text.Trim());
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
