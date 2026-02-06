using CoffeePOS.Models;
namespace CoffeePOS.Data.Repositories;

public interface ITableRepository
{
    List<Table> GetAllTables();
}
