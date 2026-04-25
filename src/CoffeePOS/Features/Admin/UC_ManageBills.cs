using System.ComponentModel;
using System.Globalization;
using System.Text;
using AntdUI;
using CoffeePOS.Features.Billing;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageBills : UserControl
{
    private readonly IBillService _billService;
    private readonly IBillQueryService _billQueryService;

    private List<BillReportDto> _bills = [];

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

    public UC_ManageBills(IBillService billService, IBillQueryService billQueryService)
    {
        _billService = billService;
        _billQueryService = billQueryService;

        InitializeComponent();
        SetupTable();
        SetupEvents();

        _ = LoadBillsAsync();
    }

    private void SetupTable()
    {
        _tableBills.Columns =
        [
            DtoHelper.CreateCol<BillReportDto>(nameof(BillReportDto.Id), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillReportDto>(nameof(BillReportDto.BuzzerNumber), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillReportDto>(nameof(BillReportDto.TotalAmount), c =>
            {
                c.Align = ColumnAlign.Right;
                c.DisplayFormat = "N0";
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillReportDto>(nameof(BillReportDto.CreatedAt), c =>
            {
                c.Align = ColumnAlign.Center;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillReportDto>(nameof(BillReportDto.CreatedByName), c => c.SortOrder = true),
            new Column("Status", "Trạng thái")
            {
                Align = ColumnAlign.Center,
                Render = (value, record, rowIndex) =>
                {
                    var isCanceled = ((BillReportDto)record).IsCanceled;
                    return new CellBadge(isCanceled ? TState.Error : TState.Success,
                                        isCanceled ? "Đã huỷ" : "Hợp lệ");
                }
            },
            new Column("action", "Thao tác")
            {
                Align = ColumnAlign.Center,
                Fixed = true,
                Width = "180",
                Render = (value, record, rowIndex) =>
                {
                    var isCanceled = ((BillReportDto)record).IsCanceled;

                    return new CellButton[] {
                        new("view", "Chi tiết") { Type = TTypeMini.Primary },
                        new("toggle", isCanceled ? "Khôi phục" : "Hủy HĐ") {
                            Type = isCanceled ? TTypeMini.Success : TTypeMini.Error
                        }
                    };
                }
            }
        ];

        _tableBills.CellButtonClick += TableBills_CellButtonClick;
    }

    private void SetupEvents()
    {
        _btnLoad.Click += async (_, _) => await LoadBillsAsync();
        _btnExport.Click += ExportReportToCsv;
    }

    private async Task LoadBillsAsync()
    {
        try
        {
            await Spin.open(_tableBills, async cfg =>
        {
            var rawFrom = _dpFrom.Value ?? DateTime.Today;
            var rawTo = _dpTo.Value ?? DateTime.Today;

            var fromDate = DateOnly.FromDateTime(rawFrom);
            var toDate = DateOnly.FromDateTime(rawTo);

            _bills = await _billQueryService.GetBillsByDateRangeAsync(fromDate, toDate);

            Invoke(() =>
            {
                _tableBills.DataSource = _bills;
                UpdateSummary();
            });
        });
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải hóa đơn: {ex.Message}", owner: this, type: FeedbackType.Message);
        }
    }

    private void TableBills_CellButtonClick(object sender, TableButtonEventArgs e)
    {
        if (e.Record is not BillReportDto selectedBill) return;

        if (e.Btn.Id == "view")
        {
            _ = ShowBillDetailsAsync(selectedBill);
        }
        if (e.Btn.Id == "toggle")
        {
            ToggleSelectedBillStatusAsync(selectedBill);
        }
    }

    private async Task ShowBillDetailsAsync(BillReportDto selectedBill)
    {
        try
        {
            var details = await _billQueryService.GetBillDetailsAsync(selectedBill.Id);
            if (details.Count == 0)
            {
                MessageBoxHelper.Warning($"Hóa đơn #{selectedBill.Id} chưa có chi tiết món.", owner: this);
                return;
            }

            var detailControl = new UC_BillDetail(selectedBill, details);
            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

            var config = new Modal.Config(form, $"CHI TIẾT HOÁ ĐƠN #{selectedBill.Id}", detailControl)
            {
                Font = UiTheme.BodyFont,
                OkText = "ĐÓNG",
                CancelText = null
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải chi tiết: {ex.Message}", owner: this);
        }
    }

    private async void ToggleSelectedBillStatusAsync(BillReportDto selectedBill)
    {
        if (selectedBill.IsCanceled)
        {
            if (!MessageBoxHelper.ConfirmWarning($"Bạn có chắc muốn khôi phục hóa đơn #{selectedBill.Id}?", "Xác nhận khôi phục", this))
                return;

            ExecuteRestoreBillAsync(selectedBill.Id);
        }
        else
        {
            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI.");

            var txtReason = new Input
            {
                PlaceholderText = "Nhập lý do hủy hóa đơn (Bắt buộc)...",
                AllowClear = true,
                Font = UiTheme.BodyFont,
                Height = 40,
                Dock = DockStyle.Fill,
                Multiline = true
            };

            var panel = new AntdUI.Panel
            {
                Size = LogicalToDeviceUnits(new Size(300, 100)),
                Padding = new Padding(10, 15, 10, 10)
            };
            panel.Controls.Add(txtReason);

            var config = new Modal.Config(form, $"HỦY HÓA ĐƠN #{selectedBill.Id}", panel)
            {
                Font = UiTheme.BodyFont,
                OkText = "Xác nhận hủy",
                CancelText = "Đóng",
                OnOk = (cfg) =>
                {
                    bool isValid = false;
                    string reason = string.Empty;

                    Invoke(() =>
                    {
                        reason = txtReason.Text.Trim();

                        if (string.IsNullOrWhiteSpace(reason))
                        {
                            MessageBoxHelper.Warning("Bắt buộc nhập lý do hủy.", owner: this);
                            txtReason.Focus();
                        }
                        else
                        {
                            isValid = true;
                        }
                    });

                    if (!isValid) return false;

                    ExecuteCancelBillAsync(selectedBill.Id, reason);
                    return true;
                }
            };

            Modal.open(config);
        }
    }

    private void ExecuteCancelBillAsync(int billId, string reason)
    {
        Target target = new(this);
        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "cancel_bill";
            try
            {
                await _billService.CancelBillAsync(billId, reason);
                Invoke(() => MessageBoxHelper.Success($"Đã hủy hóa đơn #{billId}.", owner: this, type: FeedbackType.Message));
                await LoadBillsAsync();
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error($"Lỗi hủy hóa đơn: {ex.Message}", owner: this));
            }
            finally
            {
                Invoke(() => AntdUI.Message.close_id("cancel_bill"));
            }
        });
    }

    private void ExecuteRestoreBillAsync(int billId)
    {
        Target target = new(this);
        AntdUI.Message.loading(target, "Đang xử lý...", async msg =>
        {
            msg.ID = "restore_bill";
            try
            {
                await _billService.RestoreBillAsync(billId);
                Invoke(() => MessageBoxHelper.Success($"Đã khôi phục hóa đơn #{billId}.", owner: this, type: FeedbackType.Message));
                await LoadBillsAsync();
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error($"Lỗi khôi phục: {ex.Message}", owner: this));
            }
            finally
            {
                Invoke(() => AntdUI.Message.close_id("restore_bill"));
            }
        });
    }

    private void UpdateSummary()
    {
        int totalBills = _bills.Count;
        int canceledBills = _bills.Count(x => x.IsCanceled);
        int validBills = totalBills - canceledBills;
        decimal netRevenue = _bills.Where(x => !x.IsCanceled).Sum(x => x.TotalAmount);

        string revenueText = netRevenue.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));
        _lblSummary.Text = $"Tổng đơn: {totalBills}  |  Đơn hợp lệ: {validBills}  |  Đơn hủy: {canceledBills}  |  Doanh thu: {revenueText} đ";
    }

    private async void ExportReportToCsv(object? sender, EventArgs e)
    {
        if (_bills.Count == 0)
        {
            MessageBoxHelper.Warning("Không có dữ liệu để xuất.", owner: this);
            return;
        }

        DateTime fromDate = _dpFrom.Value ?? DateTime.Today;
        DateTime toDate = _dpTo.Value ?? DateTime.Today;

        using SaveFileDialog saveDialog = new()
        {
            Filter = "CSV file (*.csv)|*.csv",
            Title = "Lưu báo cáo hóa đơn",
            FileName = $"BaoCaoHoaDon_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.csv"
        };

        if (saveDialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            using var stream = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            using var writer = new StreamWriter(stream, new UTF8Encoding(true)); // UTF8 w/ BOM for Excel compatibility

            await writer.WriteLineAsync("Báo cáo hoá đơn");
            await writer.WriteLineAsync($"Khoảng thời gian,{fromDate:dd/MM/yyyy} - {toDate:dd/MM/yyyy}");
            await writer.WriteLineAsync($"Thời gian xuất,{DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            await writer.WriteLineAsync(string.Empty);

            string headerLine = string.Join(",", CsvColumns.Select(c => EscapeCsvField(c.Header)));
            await writer.WriteLineAsync(headerLine);

            foreach (var bill in _bills)
            {
                string line = string.Join(",", CsvColumns.Select(c => EscapeCsvField(c.ValueSelector(bill))));
                await writer.WriteLineAsync(line);
            }

            MessageBoxHelper.Success($"Xuất báo cáo thành công:\n{saveDialog.FileName}", owner: this, type: FeedbackType.Message);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi xuất báo cáo: {ex.Message}", owner: this);
        }
    }

    private static string EscapeCsvField(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n')) return value;
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string GetDisplayName(string propertyName)
    {
        var property = typeof(BillReportDto).GetProperty(propertyName);
        if (property is null) return propertyName;

        var displayName = property.GetCustomAttributes(typeof(DisplayNameAttribute), true)
            .OfType<DisplayNameAttribute>()
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(displayName?.DisplayName) ? propertyName : displayName.DisplayName;
    }
}
