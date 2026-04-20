using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos.Bill;

public record BillDetailDto(
    [property: DisplayName("Mã món")] int ProductId,
    [property: DisplayName("Tên món")] string ProductName,
    [property: DisplayName("Số lượng")] int Quantity,
    [property: DisplayName("Đơn giá")] decimal Price,
    [property: DisplayName("Ghi chú")] string Note
);
