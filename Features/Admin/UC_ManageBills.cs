using System.ComponentModel;
using System.Globalization;
using System.Text;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public class UC_ManageBills : UserControl
{
    private readonly IBillService _billService;
    private readonly IBillQueryService _billQueryService;

    private UC_BillsHeaderToolbar _toolbar = null!;
    private DataGridView _dgvBills = null!;
    private StatefulSortableGrid<BillReportDto> _billsGrid = null!;

    private static readonly (string Header, Func<BillReportDto, string> ValueSelector)[] CsvColumns =
    [
        (GetDisplayName(nameof(BillReportDto.Id)), b => b.Id.ToString(CultureInfo.InvariantCulture)),
        (GetDisplayName(nameof(BillReportDto.BuzzerNumber)), b => b.BuzzerNumber.ToString(CultureInfo.InvariantCulture)),
        (GetDisplayName(nameof(BillReportDto.TotalAmount)), b => b.TotalAmount.ToString(CultureInfo.InvariantCulture)),
        (GetDisplayName(nameof(BillReportDto.CreatedAt)), b => b.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss")),
        (GetDisplayName(nameof(BillReportDto.CreatedByName)), b => b.CreatedByName),
        (GetDisplayName(nameof(BillReportDto.IsCanceled)), b => b.IsCanceled ? "1" : "0"),
        (GetDisplayName(nameof(BillReportDto.CanceledAt)), b => b.CanceledAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? string.Empty)
    ];

    private List<BillReportDto> _bills = [];

    public UC_ManageBills(IBillService billService, IBillQueryService billQueryService)
    {
        _billService = billService;
        _billQueryService = billQueryService;

        InitializeUI();
        _ = LoadBillsAsync();
    }

    private void InitializeUI()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        _toolbar = new UC_BillsHeaderToolbar();
        _toolbar.LoadClicked += async (_, _) => await LoadBillsAsync();
        _toolbar.CancelClicked += ToggleSelectedBillStatusAsync;
        _toolbar.ExportClicked += ExportReportToCsv;

        _dgvBills = new DataGridView
        {
            Dock = DockStyle.Fill
        };
        _dgvBills.ApplyStandardAdminStyle();
        _dgvBills.SelectionChanged += (_, _) => RefreshCancelButtonState();

        _billsGrid = new StatefulSortableGrid<BillReportDto>(_dgvBills);
        _billsGrid.AttachSortHandler();
        _billsGrid.SortChanged += ApplySort;

        Controls.Add(_dgvBills);
        Controls.Add(_toolbar);
    }

    private async Task LoadBillsAsync()
    {
        try
        {
            _billsGrid.CapturePosition();

            var fromDate = _toolbar.FromDate;
            var toDate = _toolbar.ToDate;

            _bills = await _billQueryService.GetBillsByDateRangeAsync(fromDate, toDate);
            BindGrid();
            UpdateSummary();

            _billsGrid.RestorePosition();
        }
        catch (ArgumentException ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Lỗi nhập liệu", this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải hóa đơn: {ex.Message}", owner: this);
        }
    }

    private void BindGrid()
    {
        _dgvBills.DataSource = null;
        _dgvBills.Columns.Clear();

        _billsGrid.Bind(_bills);

        _dgvBills.Columns[nameof(BillReportDto.TotalAmount)].DefaultCellStyle.Format = "N0";
        _dgvBills.Columns[nameof(BillReportDto.CreatedAt)].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        _dgvBills.Columns[nameof(BillReportDto.CanceledAt)].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";

        _dgvBills.Columns[nameof(BillReportDto.IsCanceled)].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

        RefreshCancelButtonState();
    }

    private void ApplySort()
    {
        _billsGrid.Bind(_bills);
        RefreshCancelButtonState();
    }

    private void RefreshCancelButtonState()
    {
        var selected = GetSelectedBill();
        _toolbar.CanCancel = selected is not null;
        _toolbar.SetBillActionMode(selected?.IsCanceled == true);
    }

    private BillReportDto? GetSelectedBill()
    {
        if (_dgvBills.SelectedRows.Count == 0)
        {
            return null;
        }

        return _dgvBills.SelectedRows[0].DataBoundItem as BillReportDto;
    }

    private async void ToggleSelectedBillStatusAsync(object? sender, EventArgs e)
    {
        var selectedBill = GetSelectedBill();
        if (selectedBill is null)
        {
            MessageBoxHelper.Warning("Vui lòng chọn hóa đơn cần thao tác.", owner: this);
            return;
        }

        string actionText = selectedBill.IsCanceled ? "khôi phục" : "hủy";
        string title = selectedBill.IsCanceled ? "Xác nhận khôi phục hóa đơn" : "Xác nhận hủy hóa đơn";
        bool confirmed = MessageBoxHelper.ConfirmWarning(
            $"Bạn có chắc muốn {actionText} hóa đơn #{selectedBill.Id} (Thẻ rung {selectedBill.BuzzerNumber})?",
            title,
            this);

        if (!confirmed)
        {
            return;
        }

        try
        {
            if (selectedBill.IsCanceled)
            {
                await _billService.RestoreBillAsync(selectedBill.Id);
                MessageBoxHelper.Info($"Đã khôi phục hóa đơn #{selectedBill.Id}.", owner: this);
            }
            else
            {
                await _billService.CancelBillAsync(selectedBill.Id);
                MessageBoxHelper.Info($"Đã hủy hóa đơn #{selectedBill.Id}.", owner: this);
            }

            await LoadBillsAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi thao tác hóa đơn: {ex.Message}", owner: this);
        }
    }

    private void UpdateSummary()
    {
        int totalBills = _bills.Count;
        int canceledBills = _bills.Count(x => x.IsCanceled);
        int validBills = totalBills - canceledBills;
        decimal netRevenue = _bills.Where(x => !x.IsCanceled).Sum(x => x.TotalAmount);

        string revenueText = netRevenue.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        _toolbar.SummaryText = $"Tổng đơn: {totalBills} | Đơn hợp lệ: {validBills} | Đơn hủy: {canceledBills} | Doanh thu thực thu: {revenueText} đ";
    }

    private async void ExportReportToCsv(object? sender, EventArgs e)
    {
        if (_bills.Count == 0)
        {
            MessageBoxHelper.Warning("Không có dữ liệu để xuất báo cáo.", owner: this);
            return;
        }

        using SaveFileDialog saveDialog = new()
        {
            Filter = "CSV file (*.csv)|*.csv",
            Title = "Lưu báo cáo hóa đơn",
            FileName = $"BaoCaoHoaDon_{_toolbar.FromDate:yyyyMMdd}_{_toolbar.ToDate:yyyyMMdd}.csv"
        };

        if (saveDialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            using var stream = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            using var writer = new StreamWriter(stream, new UTF8Encoding(true));

            await writer.WriteLineAsync("Báo cáo hoá đơn");
            await writer.WriteLineAsync($"Khoảng thời gian,{_toolbar.FromDate:dd/MM/yyyy} - {_toolbar.ToDate:dd/MM/yyyy}");
            await writer.WriteLineAsync($"Thời gian xuất,{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            await writer.WriteLineAsync(string.Empty);

            string headerLine = string.Join(",", CsvColumns.Select(c => EscapeCsvField(c.Header)));
            await writer.WriteLineAsync(headerLine);

            foreach (var bill in _bills)
            {
                string line = string.Join(",", CsvColumns.Select(c => EscapeCsvField(c.ValueSelector(bill))));
                await writer.WriteLineAsync(line);
            }

            MessageBoxHelper.Info($"Xuất báo cáo thành công:\n{saveDialog.FileName}", owner: this);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi xuất báo cáo: {ex.Message}", owner: this);
        }
    }

    private static string EscapeCsvField(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string GetDisplayName(string propertyName)
    {
        var property = typeof(BillReportDto).GetProperty(propertyName);
        if (property is null)
        {
            return propertyName;
        }

        var displayName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
            .OfType<DisplayNameAttribute>()
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(displayName?.DisplayName)
            ? propertyName
            : displayName.DisplayName;
    }
}
