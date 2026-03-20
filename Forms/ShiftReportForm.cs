using CoffeePOS.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Forms;

public partial class ShiftReportForm : Form
{
    private readonly IUserSession _session;
    private readonly IShiftReportQueryService _shiftReportQueryService;
    private readonly IShiftReportService _shiftReportService;
    private readonly PdfPrintQueue _pdfQueue;

    // UI Controls
    private Label lblTotalBills = null!;
    private Label lblExpectedCash = null!;
    private Label lblVariance = null!;
    private TextBox txtActualCash = null!;
    private TextBox txtNote = null!;
    private Button btnConfirm = null!;

    // Data State
    private int _totalBills = 0;
    private decimal _expectedCash = 0;
    private readonly DateTime _endTime;

    public ShiftReportForm(IUserSession session, IShiftReportService shiftReportService, IShiftReportQueryService shiftReportQueryService, PdfPrintQueue pdfQueue)
    {
        _session = session;
        _shiftReportService = shiftReportService;
        _shiftReportQueryService = shiftReportQueryService;
        _endTime = DateTime.Now;
        _pdfQueue = pdfQueue;

        InitializeUI();
        LoadDataAsync();
    }

    private void InitializeUI()
    {
        Text = "CHỐT CA LÀM VIỆC";
        Size = new Size(500, 480);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        Panel pnlMain = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };

        FlowLayoutPanel flowLayout = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        // Header
        Label lblHeader = new()
        {
            Text = $"Nhân viên: {_session.CurrentUser?.FullName}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };

        // Info
        lblTotalBills = CreateLabelRow("Tổng số hóa đơn: Đang tải...");
        lblExpectedCash = CreateLabelRow("Tiền trên hệ thống: Đang tải...");

        Label lblInputTitle = new()
        {
            Text = "Nhập số tiền mặt thực tế trong két:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 20, 0, 5)
        };
        txtActualCash = new TextBox
        {
            Width = 300,
            Font = new Font("Segoe UI", 14),
            Margin = new Padding(0, 0, 0, 10)
        };
        txtActualCash.TextChanged += TxtActualCash_TextChanged;

        // Độ lệch
        lblVariance = CreateLabelRow("Độ lệch: 0 đ");
        lblVariance.ForeColor = Color.DimGray;

        // Ghi chú
        Label lblNote = new()
        {
            Text = "Ghi chú (Lý do lệch tiền):",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Margin = new Padding(0, 10, 0, 5)
        };
        txtNote = new TextBox
        {
            Width = 420,
            Height = 60,
            Multiline = true,
            Font = new Font("Segoe UI", 10)
        };

        // Button Lưu
        btnConfirm = new Button
        {
            Text = "XÁC NHẬN CHỐT CA && ĐĂNG XUẤT",
            Width = 420,
            Height = 50,
            Margin = new Padding(0, 20, 0, 0),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Click += async (s, e) => await ConfirmShiftAsync();

        flowLayout.Controls.AddRange([lblHeader, lblTotalBills, lblExpectedCash, lblInputTitle, txtActualCash, lblVariance, lblNote, txtNote, btnConfirm]);
        pnlMain.Controls.Add(flowLayout);
        Controls.Add(pnlMain);
    }

    private static Label CreateLabelRow(string text) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 12),
        AutoSize = true,
        Margin = new Padding(0, 5, 0, 5)
    };

    private async void LoadDataAsync()
    {
        try
        {
            btnConfirm.Enabled = false;
            var (TotalBills, ExpectedCash) = await _shiftReportQueryService.GetShiftSummaryAsync(_session.CurrentUser!.Id, _session.LoginTime!.Value, _endTime);
            _totalBills = TotalBills;
            _expectedCash = ExpectedCash;

            lblTotalBills.Text = $"Tổng số hóa đơn: {_totalBills}";
            lblExpectedCash.Text = $"Tiền trên hệ thống: {_expectedCash:N0} đ";
            btnConfirm.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu chốt ca: {ex.Message}");
            Close();
        }
    }

    private void TxtActualCash_TextChanged(object? sender, EventArgs e)
    {
        // Tính toán độ lệch Live
        if (decimal.TryParse(txtActualCash.Text, out decimal actualCash))
        {
            decimal variance = actualCash - _expectedCash;
            lblVariance.Text = $"Độ lệch: {variance:N0} đ";

            if (variance < 0) lblVariance.ForeColor = Color.Red; // Hụt tiền
            else if (variance > 0) lblVariance.ForeColor = Color.Green; // Dư tiền
            else lblVariance.ForeColor = Color.Blue; // Hoàn hảo
        }
        else
        {
            lblVariance.Text = "Độ lệch: ---";
            lblVariance.ForeColor = Color.DimGray;
        }
    }

    private async Task ConfirmShiftAsync()
    {
        if (!decimal.TryParse(txtActualCash.Text, out decimal actualCash))
        {
            MessageBox.Show("Vui lòng nhập số tiền thực tế hợp lệ!");
            return;
        }

        btnConfirm.Enabled = false;
        try
        {
            var command = new SaveShiftReportDto(
                _session.CurrentUser!.Id,
                _session.LoginTime!.Value,
                _endTime,
                _totalBills,
                _expectedCash,
                actualCash,
                actualCash - _expectedCash,
                txtNote.Text);

            await _shiftReportService.SaveReportAsync(command);

            await _pdfQueue.EnqueueJobAsync(new ShiftReportPrintPayload
            {
                CashierName = _session.CurrentUser!.FullName,
                StartTime = _session.LoginTime!.Value,
                EndTime = _endTime,
                TotalBills = _totalBills,
                ExpectedCash = _expectedCash,
                ActualCash = actualCash,
                Variance = actualCash - _expectedCash,
                Note = txtNote.Text
            });

            // Bắn tín hiệu chốt ca thành công
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu báo cáo: {ex.Message}");
            btnConfirm.Enabled = true;
        }
    }
}
