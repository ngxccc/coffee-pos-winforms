using System.ComponentModel;
using CoffeePOS.Shared.Enums;

namespace CoffeePOS.Shared.Dtos.Bill;

public record BillReportDto(
    [property: DisplayName("Mã Đơn")] int Id,
    [property: DisplayName("Thẻ Rung")] int BuzzerNumber,
    [property: DisplayName("Tổng Tiền (đ)")] decimal TotalAmount,
    [property: DisplayName("Trạng Thái")] BillStatus Status,
    [property: DisplayName("Nhân Viên Tạo")] string CreatedByName,
    [property: DisplayName("Người Đã Huỷ")] string? CanceledByName,
    [property: DisplayName("Thời Gian Tạo")] DateTime CreatedAt,
    [property: DisplayName("Thời Gian Hủy")] DateTime? CanceledAt
);
