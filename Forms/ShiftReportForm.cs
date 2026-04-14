using CoffeePOS.Core;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public class ShiftReportForm : Form
{
    private readonly IUserSession _session;
    private readonly IShiftReportQueryService _shiftReportQueryService;
    private readonly IShiftReportService _shiftReportService;
    private readonly PdfPrintQueue _pdfQueue;

    // UI Controls
    private Label lblTotalBills = null!;
    private Label lblExpectedCash = null!;
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
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadDataAsync();
    }

    private void InitializeUI()
    {
        Text = "CHỐT CA LÀM VIỆC";
        Size = new Size(450, 500); // WHY: Thu gọn bề ngang lại cho gọn gàng vì giờ nó đã tự co giãn
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        // PERF: TableLayoutPanel handles internal resizing math automatically, preventing manual pixel-pushing
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(20)
        };
        // HACK: Force the single column to take 100% of available width
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // 0. Header
        Label lblHeader = new()
        {
            Text = $"Nhân viên: {_session.CurrentUser?.FullName}",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };

        // 1 & 2. Info
        lblTotalBills = new Label
        {
            Text = "Tổng số hóa đơn: Đang tải...",
            Font = new Font("Segoe UI", 12),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };
        lblExpectedCash = new Label
        {
            Text = "Tiền trên hệ thống: [ĐÃ ẨN]",
            Font = new Font("Segoe UI", 12),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };

        // 3 & 4. Input Actual Cash
        Label lblInputTitle = new()
        {
            Text = "Nhập số tiền mặt thực tế trong két:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 5)
        };
        txtActualCash = new TextBox
        {
            Font = new Font("Segoe UI", 14),
            Margin = new Padding(0, 0, 0, 10),
            Dock = DockStyle.Fill
        };

        // 5 & 6. Note
        Label lblNote = new()
        {
            Text = "Ghi chú (Lý do lệch tiền nếu có):",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Margin = new Padding(0, 5, 0, 5)
        };
        txtNote = new TextBox
        {
            Multiline = true,
            Font = new Font("Segoe UI", 10),
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 20)
        };

        // 7. Button
        btnConfirm = new Button
        {
            Text = "XÁC NHẬN CHỐT CA && ĐĂNG XUẤT",
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        btnConfirm.FlatAppearance.BorderSize = 0;
        btnConfirm.Click += async (s, e) => await ConfirmShiftAsync();

        // Map controls to matrix (Column 0, Row N)
        layout.Controls.Add(lblHeader, 0, 0);
        layout.Controls.Add(lblTotalBills, 0, 1);
        layout.Controls.Add(lblExpectedCash, 0, 2);
        layout.Controls.Add(lblInputTitle, 0, 3);
        layout.Controls.Add(txtActualCash, 0, 4);
        layout.Controls.Add(lblNote, 0, 5);
        layout.Controls.Add(txtNote, 0, 6);
        layout.Controls.Add(btnConfirm, 0, 7);

        // Map sizing rules for each row
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Total Bills
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Expected Cash
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Input Title
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Actual Cash
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Note Title
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // WHY: Note Input automatically absorbs all remaining vertical space
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Button Height

        Controls.Add(layout);
    }

    private static Label CreateLabelRow(string text) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", 12),
        AutoSize = true,
        Margin = new Padding(0, 5, 0, 5)
    };

    private async Task LoadDataAsync()
    {
        try
        {
            btnConfirm.Enabled = false;
            var (TotalBills, ExpectedCash) = await _shiftReportQueryService.GetShiftSummaryAsync(_session.CurrentUser!.Id, _session.LoginTime!.Value, _endTime);
            _totalBills = TotalBills;
            _expectedCash = ExpectedCash;

            lblTotalBills.Text = $"Tổng số hóa đơn: {_totalBills}";
            lblExpectedCash.Text = $"Tiền trên hệ thống: [ĐÃ ẨN]";
            btnConfirm.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu chốt ca: {ex.Message}", owner: this);
            Close();
        }
        finally
        {
            btnConfirm.Enabled = true;
        }
    }

    private async Task ConfirmShiftAsync()
    {
        if (!decimal.TryParse(txtActualCash.Text, out decimal actualCash))
        {
            MessageBoxHelper.Warning("Ét o ét! Vui lòng nhập số tiền thực tế hợp lệ!", owner: this);
            return;
        }

        decimal variance = actualCash - _expectedCash;
        if (variance != 0 && string.IsNullOrWhiteSpace(txtNote.Text))
        {
            // WHY: True Blind Close implementation. We only signal a mismatch, preventing reverse-engineering of ExpectedCash.
            MessageBoxHelper.Warning(
                "Hệ thống phát hiện số tiền trong két KHÔNG KHỚP với doanh thu!\n" +
                "Vui lòng lôi hết tiền ra đếm lại thật kỹ.\n" +
                "Nếu đếm lại vẫn thấy lệch, bắt buộc nhập lý do chi tiết vào ô Ghi chú để quản lý kiểm tra.",
                owner: this);

            txtNote.Focus();

            // HACK: Block submission. Force them to recount or formally confess via the Note.
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
                variance,
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
                Variance = variance,
                Note = txtNote.Text
            });

            // Bắn tín hiệu chốt ca thành công
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi lưu báo cáo: {ex.Message}", owner: this);
            btnConfirm.Enabled = true;
        }
    }
}
