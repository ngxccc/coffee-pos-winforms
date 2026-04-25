using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ProductSizeRepository(NpgsqlDataSource dataSource) : IProductSizeRepository
{
    private static readonly string SqlGetByProductId = SqlFileLoader.Load(SqlKeys.ProductSize.GetByProductId);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.ProductSize.Insert);
    private static readonly string SqlUpdate = SqlFileLoader.Load(SqlKeys.ProductSize.Update);
    private static readonly string SqlDelete = SqlFileLoader.Load(SqlKeys.ProductSize.Delete);

    private readonly NpgsqlDataSource _dataSource = dataSource;

    public async Task<List<ProductSizeDto>> GetSizesByProductIdAsync(int productId)
    {
        var result = new List<ProductSizeDto>();

        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlGetByProductId, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("product_id", productId));

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ProductSizeDto(
                reader.GetRequiredInt("id"),
                // reader.GetRequiredInt("product_id"),
                // reader.GetRequiredString("product_name"),
                reader.GetRequiredString("size_name"),
                reader.GetRequiredDecimal("price_adjustment")
            ));
        }

        return result;
    }

    public async Task<int> InsertSizeAsync(UpsertProductSizeDto dto)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlInsert, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("product_id", dto.ProductId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("size_name", dto.SizeName));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("price_adjustment", dto.PriceAdjustment));

        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> UpdateSizeAsync(UpsertProductSizeDto dto)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlUpdate, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("id", dto.Id));
        cmd.Parameters.Add(new NpgsqlParameter<int>("product_id", dto.ProductId));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("price_adjustment", dto.PriceAdjustment));

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteSizeAsync(int id)
    {
        await using var conn = await _dataSource.OpenConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlDelete, conn);

        cmd.Parameters.Add(new NpgsqlParameter<int>("id", id));

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
