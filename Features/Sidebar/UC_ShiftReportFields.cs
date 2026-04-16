using CoffeePOS.Core;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Sidebar;

public record ShiftReportPayload(DateTime EndTime, int TotalBills, decimal ExpectedCash, decimal ActualCash, decimal Variance, string Note);

public class UC_ShiftReportFields : UserControl, IValidatableComponent<ShiftReportPayload>
{
    private readonly IUserSession _session;
    private readonly IShiftReportQueryService _shiftReportQueryService;

    private AntdUI.Label _lblTotalBills = null!;
    private AntdUI.Label _lblExpectedCash = null!;
    private AntdUI.Input _txtActualCash = null!;
    private AntdUI.Input _txtNote = null!;

    private int _totalBills;
    private decimal _expectedCash;
    private DateTime _endTime;
    private bool _loaded;

    public UC_ShiftReportFields(IUserSession session, IShiftReportQueryService shiftReportQueryService)
    {
        _session = session;
        _shiftReportQueryService = shiftReportQueryService;
        _endTime = DateTime.Now;

        InitializeUI();
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

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(20)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var lblHeader = new AntdUI.Label
        {
            Text = $"Nhân viên: {_session.CurrentUser?.FullName}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };

        _lblTotalBills = new AntdUI.Label
        {
            Text = "Tổng hoá đơn: Đang tải...",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };

        _lblExpectedCash = new AntdUI.Label
        {
            Text = "Tiền trên hệ thống: [ĐÃ ẨN]",
            Font = new Font("Segoe UI", 11),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };

        var lblInputTitle = new AntdUI.Label
        {
            Text = "Nhập số tiền mặt thực tế trong két:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };

        _txtActualCash = new AntdUI.Input
        {
            Font = new Font("Segoe UI", 13),
            Margin = new Padding(0, 0, 0, 10),
            PlaceholderText = "VD: 2500000",
            AllowClear = true,
            Dock = DockStyle.Fill
        };

        var lblNote = new AntdUI.Label
        {
            Text = "Ghi chú (lý do lệch tiền nếu có):",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 5)
        };

        _txtNote = new AntdUI.Input
        {
            Multiline = true,
            Font = new Font("Segoe UI", 10),
            Dock = DockStyle.Fill,
            AllowClear = true,
            Margin = new Padding(0)
        };

        layout.Controls.Add(lblHeader, 0, 0);
        layout.Controls.Add(_lblTotalBills, 0, 1);
        layout.Controls.Add(_lblExpectedCash, 0, 2);
        layout.Controls.Add(lblInputTitle, 0, 3);
        layout.Controls.Add(_txtActualCash, 0, 4);
        layout.Controls.Add(lblNote, 0, 5);
        layout.Controls.Add(_txtNote, 0, 6);

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        Controls.Add(layout);
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
