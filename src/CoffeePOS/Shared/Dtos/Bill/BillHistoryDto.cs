using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos.Bill;

public record BillHistoryDto(
    [property: DisplayName("Mã Đơn")] int Id,
    [property: DisplayName("Thẻ Rung")] int BuzzerNumber,
    [property: DisplayName("Số Món")] int TotalItems,
    [property: DisplayName("Tổng Tiền (đ)")] decimal TotalAmount,
    [property: DisplayName("Thời Gian")] DateTime CreatedAt
);
