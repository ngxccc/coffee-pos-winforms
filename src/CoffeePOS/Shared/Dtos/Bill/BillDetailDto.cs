using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos.Bill;

public record BillDetailDto
{
    [property: DisplayName("Mã món")] public int ProductId { get; set; }
    [property: DisplayName("Tên món")] public string ProductName { get; set; } = "";
    [property: DisplayName("Số lượng")] public int Quantity { get; set; }
    [property: DisplayName("Đơn giá")] public decimal Price { get; set; }
    [property: DisplayName("Thành tiền")] public decimal LineTotal => Quantity * Price;
    [property: DisplayName("Ghi chú")] public string Note { get; set; } = "";
};
