using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services;

public interface IShiftReportService
{
    Task SaveReportAsync(SaveShiftReportDto command);
}
