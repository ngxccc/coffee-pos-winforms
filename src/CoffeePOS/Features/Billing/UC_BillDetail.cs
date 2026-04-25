using System.Drawing;
using CoffeePOS.Shared.Dtos.Bill;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Billing;

public partial class UC_BillDetail : UserControl
{
    private readonly BillReportDto _bill;
    private readonly IReadOnlyList<BillDetailDto> _items;

    public UC_BillDetail(BillReportDto bill, IReadOnlyList<BillDetailDto> items)
    {
        _bill = bill;
        _items = items;

        InitializeComponent();
        SetupTable();
    }

    private void SetupTable()
    {
        _tableItems.Columns =
        [
            DtoHelper.CreateCol<BillDetailDto>(nameof(BillDetailDto.ProductName), c =>
            {
                c.Align = AntdUI.ColumnAlign.Left;
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillDetailDto>(nameof(BillDetailDto.Quantity), c =>
            {
                c.Align = AntdUI.ColumnAlign.Center;
                c.DisplayFormat = "{0:N0}";
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillDetailDto>(nameof(BillDetailDto.Price), c =>
            {
                c.Align = AntdUI.ColumnAlign.Right;
                c.DisplayFormat = "{0:N0}";
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillDetailDto>(nameof(BillDetailDto.LineTotal), c =>
            {
                c.Align = AntdUI.ColumnAlign.Right;
                c.DisplayFormat = "{0:N0}";
                c.SortOrder = true;
            }),
            DtoHelper.CreateCol<BillDetailDto>(nameof(BillDetailDto.Note), c =>
            {
                c.Align = AntdUI.ColumnAlign.Left;
                c.SortOrder = true;
            }),
        ];

        _lblTitle.Text = $"CHI TIẾT HOÁ ĐƠN #{_bill.Id}";

        _lblBuzzer.Text = $"Thẻ rung: {_bill.BuzzerNumber}";
        _lblStaff.Text = $"Nhân viên: {_bill.CreatedByName}";
        _lblCreatedAt.Text = $"Tạo lúc: {_bill.CreatedAt:dd/MM/yyyy HH:mm}";

        string status = _bill.IsCanceled ? "Đã huỷ" : "Hợp lệ";
        _lblStatus.Text = $"Trạng thái: {status}";

        if (_bill.IsCanceled && _bill.CanceledAt.HasValue)
        {
            _lblCanceledAt.Text = $"Huỷ lúc: {_bill.CanceledAt.Value:dd/MM/yyyy HH:mm}";
            _lblCanceledAt.ForeColor = Color.Red;
            _lblCanceledAt.Visible = true;
        }
        else
        {
            _lblCanceledAt.Visible = false;
        }

        _lblTotal.Text = $"TỔNG TIỀN: {_bill.TotalAmount:N0} đ";

        _tableItems.DataSource = _items;
    }
}
