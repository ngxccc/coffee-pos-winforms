using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

public record BillDetailRowDto(
    [property: DisplayName("Tên Món")] string ProductName,
    [property: DisplayName("SL")] int Quantity,
    [property: DisplayName("Đơn Giá (đ)")] decimal Price,
    [property: DisplayName("Thành Tiền (đ)")] decimal LineTotal,
    [property: DisplayName("Ghi Chú")] string Note
);
