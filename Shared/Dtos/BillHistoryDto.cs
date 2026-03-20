using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

public record BillHistoryDto(
    [property: DisplayName("Mã Đơn")] int Id,
    [property: DisplayName("Thẻ Rung")] int BuzzerNumber,
    [property: DisplayName("Tổng Tiền (đ)")] decimal TotalAmount,
    [property: DisplayName("Thời Gian")] DateTime CreatedAt
);
