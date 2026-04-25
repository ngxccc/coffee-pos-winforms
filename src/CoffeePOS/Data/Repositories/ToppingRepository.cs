using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ToppingRepository(NpgsqlDataSource dataSource) : IToppingRepository
{
    private readonly NpgsqlDataSource _dataSource = dataSource;

    private static readonly string SqlGetAll = SqlFileLoader.Load(SqlKeys.Topping.GetAll);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.Topping.Insert);
    private static readonly string SqlUpdate = SqlFileLoader.Load(SqlKeys.Topping.Update);
    private static readonly string SqlSoftDelete = SqlFileLoader.Load(SqlKeys.Topping.SoftDelete);
    private static readonly string SqlRestore = SqlFileLoader.Load(SqlKeys.Topping.Restore);

    public async Task<List<ToppingDto>> GetToppingsAsync(bool isDeleted)
    {
        var result = new List<ToppingDto>();

        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        cmd.Parameters.Add(new NpgsqlParameter<bool>("is_deleted", isDeleted));
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new ToppingDto
            {
                Id = reader.GetRequired<int>("id"),
                Name = reader.GetRequired<string>("name"),
                Price = reader.GetRequired<decimal>("price"),
                CreatedAt = reader.GetRequired<DateTime>("created_at"),
                UpdatedAt = reader.GetNullable<DateTime>("updated_at"),
                DeletedAt = reader.GetNullable<DateTime>("deleted_at")
            });
        }
        return result;
    }

    public async Task<int> InsertToppingAsync(UpsertToppingDto dto)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlInsert, conn);

        cmd.Parameters.Add(new NpgsqlParameter<string>("name", dto.Name));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("price", dto.Price));

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateToppingAsync(UpsertToppingDto dto)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlUpdate, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("id", dto.Id));
        cmd.Parameters.Add(new NpgsqlParameter<string>("name", dto.Name));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("price", dto.Price));

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> SoftDeleteToppingAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlSoftDelete, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> RestoreToppingAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlRestore, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
