using CoffeePOS.Shared.Helpers;
using FontAwesome.Sharp;

namespace CoffeePOS.Features.Admin.Controls;

public class UC_BillsHeaderToolbar : UserControl
{
    private readonly DateTimePicker _dtpFrom;
    private readonly DateTimePicker _dtpTo;
    private readonly IconButton _btnCancelBill;
    private readonly Label _lblSummary;

    public event EventHandler? LoadClicked;
    public event EventHandler? CancelClicked;
    public event EventHandler? ExportClicked;

    public DateOnly FromDate => DateOnly.FromDateTime(_dtpFrom.Value.Date);

    public DateOnly ToDate => DateOnly.FromDateTime(_dtpTo.Value.Date);

    public string SummaryText
    {
        get => _lblSummary.Text;
        set => _lblSummary.Text = value;
    }

    public bool CanCancel
    {
        set => _btnCancelBill.Enabled = value;
    }

    public void SetBillActionMode(bool restoreMode)
    {
        if (restoreMode)
        {
            _btnCancelBill.Text = "Khôi phục đơn";
            _btnCancelBill.IconChar = IconChar.Undo;
            _btnCancelBill.BackColor = Color.FromArgb(46, 204, 113);
            return;
        }

        _btnCancelBill.Text = "Huỷ hoá đơn";
        _btnCancelBill.IconChar = IconChar.Ban;
        _btnCancelBill.BackColor = Color.FromArgb(231, 76, 60);
    }

    public UC_BillsHeaderToolbar()
    {
        Dock = DockStyle.Top;
        Height = 134;
        BackColor = Color.White;

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ HOÁ ĐƠN & BÁO CÁO",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204),
            Dock = DockStyle.Top,
            Height = 42,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var pnlFilters = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 6, 0, 0)
        };

        _dtpFrom = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Width = 150,
            Value = DateTime.Today.AddDays(-6)
        };

        _dtpTo = new DateTimePicker
        {
            Format = DateTimePickerFormat.Custom,
            CustomFormat = "dd/MM/yyyy",
            Width = 150,
            Value = DateTime.Today
        };

        var btnLoad = UIHelper.CreateActionButton("Tải dữ liệu", IconChar.Search, Color.FromArgb(52, 152, 219), (_, _) => LoadClicked?.Invoke(this, EventArgs.Empty));
        _btnCancelBill = UIHelper.CreateActionButton("Huỷ hoá đơn", IconChar.Ban, Color.FromArgb(231, 76, 60), (_, _) => CancelClicked?.Invoke(this, EventArgs.Empty));
        var btnExport = UIHelper.CreateActionButton("Xuất báo cáo", IconChar.FileCsv, Color.FromArgb(46, 204, 113), (_, _) => ExportClicked?.Invoke(this, EventArgs.Empty));

        _btnCancelBill.Enabled = false;

        pnlFilters.Controls.Add(CreateFilterLabel("Từ ngày:"));
        pnlFilters.Controls.Add(_dtpFrom);
        pnlFilters.Controls.Add(CreateFilterLabel("Đến ngày:"));
        pnlFilters.Controls.Add(_dtpTo);
        pnlFilters.Controls.Add(btnLoad);
        pnlFilters.Controls.Add(_btnCancelBill);
        pnlFilters.Controls.Add(btnExport);

        _lblSummary = new Label
        {
            Dock = DockStyle.Fill,
            Height = 34,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 30, 68),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };

        Controls.Add(_lblSummary);
        Controls.Add(pnlFilters);
        Controls.Add(lblTitle);
    }

    private static Label CreateFilterLabel(string text)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 30, 68),
            Width = 80,
            Height = 36,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(10, 0, 0, 0)
        };
    }
}
