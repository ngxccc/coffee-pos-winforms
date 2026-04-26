
using CoffeePOS.Shared.Dtos.ShiftReport;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IShiftReportService
{
    Task SaveReportAsync(UpsertShiftReportDto command);
}
