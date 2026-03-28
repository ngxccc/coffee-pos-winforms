using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Forms;

public class BillDetailForm : Form
{
    private readonly BillReportDto _bill;
    private readonly IReadOnlyList<BillDetailDto> _items;

    public BillDetailForm(BillReportDto bill, IReadOnlyList<BillDetailDto> items)
    {
        _bill = bill;
        _items = items;

        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = $"Chi tiết hóa đơn #{_bill.Id}";
        Size = new Size(900, 620);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = CreateHeaderPanel();
        var grid = CreateItemsGrid();
        var footer = CreateFooterPanel();

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(grid, 0, 1);
        root.Controls.Add(footer, 0, 2);

        Controls.Add(root);
    }

    private Control CreateHeaderPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 150,
            Padding = new Padding(8, 0, 8, 8)
        };

        var title = new Label
        {
            AutoSize = true,
            Text = $"HÓA ĐƠN #{_bill.Id}",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 30, 68),
            Location = new Point(0, 0)
        };

        string status = _bill.IsCanceled ? "Đã hủy" : "Hợp lệ";
        string canceledAt = _bill.CanceledAt.HasValue
            ? _bill.CanceledAt.Value.ToString("dd/MM/yyyy HH:mm:ss")
            : "-";

        var details = new Label
        {
            AutoSize = true,
            Location = new Point(0, 42),
            Font = new Font("Segoe UI", 10),
            Text =
                $"Thẻ rung: {_bill.BuzzerNumber}\n" +
                $"Nhân viên tạo: {_bill.CreatedByName}\n" +
                $"Thời gian tạo: {_bill.CreatedAt:dd/MM/yyyy HH:mm:ss}\n" +
                $"Trạng thái: {status}\n" +
                $"Thời gian hủy: {canceledAt}"
        };

        panel.Controls.Add(title);
        panel.Controls.Add(details);

        return panel;
    }

    private Control CreateItemsGrid()
    {
        var dgv = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false
        };
        dgv.ApplyStandardAdminStyle();

        dgv.Columns.Add(CreateTextColumn(nameof(BillDetailRowDto.ProductName), "Tên món", 42));
        dgv.Columns.Add(CreateTextColumn(
            nameof(BillDetailRowDto.Quantity),
            "SL",
            10,
            alignment: DataGridViewContentAlignment.MiddleCenter));
        dgv.Columns.Add(CreateTextColumn(
            nameof(BillDetailRowDto.Price),
            "Đơn giá",
            18,
            format: "N0",
            alignment: DataGridViewContentAlignment.MiddleRight));
        dgv.Columns.Add(CreateTextColumn(
            nameof(BillDetailRowDto.LineTotal),
            "Thành tiền",
            20,
            format: "N0",
            alignment: DataGridViewContentAlignment.MiddleRight));
        dgv.Columns.Add(CreateTextColumn(nameof(BillDetailRowDto.Note), "Ghi chú", 25));

        var rows = _items
            .Select(x => new BillDetailRowDto(
                x.ProductName,
                x.Quantity,
                x.Price,
                x.Quantity * x.Price,
                x.Note))
            .ToList();

        dgv.DataSource = rows;

        return dgv;
    }

    private static DataGridViewTextBoxColumn CreateTextColumn(
        string propertyName,
        string headerText,
        float fillWeight,
        string? format = null,
        DataGridViewContentAlignment? alignment = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = headerText,
            FillWeight = fillWeight
        };

        if (!string.IsNullOrWhiteSpace(format) || alignment.HasValue)
        {
            column.DefaultCellStyle = new DataGridViewCellStyle();

            if (!string.IsNullOrWhiteSpace(format))
            {
                column.DefaultCellStyle.Format = format;
            }

            if (alignment.HasValue)
            {
                column.DefaultCellStyle.Alignment = alignment.Value;
            }
        }

        return column;
    }

    private Control CreateFooterPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 56,
            Padding = new Padding(8, 10, 8, 0)
        };

        var total = new Label
        {
            AutoSize = true,
            Text = $"Tổng tiền hóa đơn: {_bill.TotalAmount:N0} đ",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(39, 174, 96),
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var btnClose = new Button
        {
            Text = "Đóng",
            Width = 120,
            Height = 36,
            Dock = DockStyle.Right,
            BackColor = Color.FromArgb(31, 30, 68),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => Close();

        panel.Controls.Add(btnClose);
        panel.Controls.Add(total);

        return panel;
    }
}
