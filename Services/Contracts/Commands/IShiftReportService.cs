using CoffeePOS.Shared.Dtos;

namespace CoffeePOS.Services.Contracts.Commands;

public interface IShiftReportService
{
    Task SaveReportAsync(SaveShiftReportDto command);
}
