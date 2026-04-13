using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ToppingRepository(NpgsqlDataSource dataSource) : IToppingRepository
{
    private static readonly string SqlGet = SqlFileLoader.Load(SqlKeys.Topping.Get);

    public async Task<List<ToppingGridDto>> GetAllToppingsAsync(bool isDeleted = false)
    {
        var list = new List<ToppingGridDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGet, conn);
        cmd.Parameters.AddWithValue("is_deleted", isDeleted);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new ToppingGridDto(
                Id: reader.GetInt32(0),
                Name: reader.GetString(1),
                Price: reader.GetDecimal(2)
            ));
        }
        return list;
    }
}
