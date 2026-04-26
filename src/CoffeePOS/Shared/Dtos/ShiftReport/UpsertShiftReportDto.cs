namespace CoffeePOS.Shared.Dtos.ShiftReport;

public record UpsertShiftReportDto(
    int UserId,
    DateTime StartTime,
    DateTime EndTime,
    int TotalBills,
    decimal StartingCash,
    decimal ExpectedCash,
    decimal ActualCash,
    decimal Difference,
    string? Note
);
