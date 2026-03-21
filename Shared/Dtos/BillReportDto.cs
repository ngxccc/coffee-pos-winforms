using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos;

public record BillReportDto(
    [property: DisplayName("Mã Đơn")] int Id,
    [property: DisplayName("Thẻ Rung")] int BuzzerNumber,
    [property: DisplayName("Tổng Tiền (đ)")] decimal TotalAmount,
    [property: DisplayName("Thời Gian Tạo")] DateTime CreatedAt,
    [property: DisplayName("Nhân Viên Tạo")] string CreatedByName,
    [property: DisplayName("Đã Hủy")] bool IsCanceled,
    [property: DisplayName("Thời Gian Hủy")] DateTime? CanceledAt
);
