using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories.Impl;

public class TableRepository(NpgsqlDataSource dataSource) : ITableRepository
{
    public List<Table> GetAllTables()
    {
        var list = new List<Table>();
        using var conn = dataSource.OpenConnection();
        using var cmd = new NpgsqlCommand("SELECT id, name FROM tables ORDER BY id", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Table { Id = reader.GetInt32(0), Name = reader.GetString(1) });
        }
        return list;
    }
}
