using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public class UC_BillsHeaderToolbar : UserControl
{
    private readonly DateTimePicker _dtpFrom;
    private readonly DateTimePicker _dtpTo;
    private readonly AntdUI.Button _btnCancelBill;
    private readonly AntdUI.Label _lblSummary;

    public event EventHandler? LoadClicked;
    public event EventHandler? CancelClicked;
    public event EventHandler? ExportClicked;

    public DateOnly FromDate => DateOnly.FromDateTime(_dtpFrom.Value.Date);

    public DateOnly ToDate => DateOnly.FromDateTime(_dtpTo.Value.Date);

    public string SummaryText
    {
        get => _lblSummary.Text ?? string.Empty;
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
            _btnCancelBill.Type = UiTheme.AddButtonType;
            return;
        }

        _btnCancelBill.Text = "Huỷ hoá đơn";
        _btnCancelBill.Type = UiTheme.DeleteButtonType;
    }

    public UC_BillsHeaderToolbar()
    {
        Dock = DockStyle.Top;
        Height = 134;
        BackColor = UiTheme.Surface;

        var lblTitle = new AntdUI.Label
        {
            Text = "QUẢN LÝ HOÁ ĐƠN & BÁO CÁO",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = UiTheme.BrandPrimary,
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

        var btnLoad = CreateActionButton("Tải dữ liệu", UiTheme.PrimaryButtonType, (_, _) => LoadClicked?.Invoke(this, EventArgs.Empty));
        _btnCancelBill = CreateActionButton("Huỷ hoá đơn", UiTheme.DeleteButtonType, (_, _) => CancelClicked?.Invoke(this, EventArgs.Empty));
        var btnExport = CreateActionButton("Xuất báo cáo", UiTheme.AddButtonType, (_, _) => ExportClicked?.Invoke(this, EventArgs.Empty));

        _btnCancelBill.Enabled = false;

        pnlFilters.Controls.Add(CreateFilterLabel("Từ ngày:"));
        pnlFilters.Controls.Add(_dtpFrom);
        pnlFilters.Controls.Add(CreateFilterLabel("Đến ngày:"));
        pnlFilters.Controls.Add(_dtpTo);
        pnlFilters.Controls.Add(btnLoad);
        pnlFilters.Controls.Add(_btnCancelBill);
        pnlFilters.Controls.Add(btnExport);

        _lblSummary = new AntdUI.Label
        {
            Dock = DockStyle.Fill,
            Height = 34,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };

        Controls.Add(_lblSummary);
        Controls.Add(pnlFilters);
        Controls.Add(lblTitle);
    }

    private static AntdUI.Button CreateActionButton(string text, AntdUI.TTypeMini type, EventHandler click)
    {
        var btn = new AntdUI.Button
        {
            Text = text,
            Type = type,
            Size = new Size(120, 38),
            Cursor = Cursors.Hand,
            Margin = new Padding(8, 0, 0, 0)
        };
        btn.Click += click;
        return btn;
    }

    private static AntdUI.Label CreateFilterLabel(string text)
    {
        return new AntdUI.Label
        {
            Text = text,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = UiTheme.TextPrimary,
            Width = 80,
            Height = 36,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(10, 0, 0, 0)
        };
    }
}
