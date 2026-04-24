namespace CoffeePOS.Shared.Dtos.ShiftReport;

public record SaveShiftReportDto(
    int UserId,
    DateTime StartTime,
    DateTime EndTime,
    int TotalBills,
    decimal ExpectedCash,
    decimal ActualCash,
    decimal Variance,
    string Note
);
