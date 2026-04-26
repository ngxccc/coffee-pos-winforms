using System.ComponentModel;

namespace CoffeePOS.Shared.Dtos.ShiftReport;

public record ShiftReportDto(
    [property: DisplayName("Mã")]
    int Id,
    [property: DisplayName("Tên nhân viên")]
    string CashierName,
    [property: DisplayName("Thời gian vào làm")]
    DateTime StartTime,
    [property: DisplayName("Thời gian chốt ca")]
    DateTime EndTime,
    [property: DisplayName("Tổng số đơn")]
    int TotalBills,
    [property: DisplayName("Tiền vốn")]
    decimal StartingCash,
    [property: DisplayName("Tiền hệ thống")]
    decimal ExpectedCash,
    [property: DisplayName("Tiền thực tế")]
    decimal ActualCash,
    [property: DisplayName("Độ lệch")]
    decimal Difference,
    [property: DisplayName("Ghi chú")]
    string? Note
);
