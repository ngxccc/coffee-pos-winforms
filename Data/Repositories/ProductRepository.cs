using System.Data.Common;
using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{
    private static readonly string SqlGetAll = SqlFileLoader.Load(SqlKeys.Product.GetAll);
    private static readonly string SqlGetById = SqlFileLoader.Load(SqlKeys.Product.GetById);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.Product.Insert);
    private static readonly string SqlUpdate = SqlFileLoader.Load(SqlKeys.Product.Update);
    private static readonly string SqlSoftDelete = SqlFileLoader.Load(SqlKeys.Product.SoftDelete);
    private static readonly string SqlGetDeleted = SqlFileLoader.Load(SqlKeys.Product.GetDeleted);
    private static readonly string SqlGetDeletedById = SqlFileLoader.Load(SqlKeys.Product.GetDeletedById);
    private static readonly string SqlRestore = SqlFileLoader.Load(SqlKeys.Product.Restore);

    public async Task<List<ProductDetailDto>> GetAllProductsAsync()
    {
        var list = new List<ProductDetailDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapProductFromReader(reader));
        }
        return list;
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetById, conn);
        cmd.Parameters.AddWithValue("id", productId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProductFromReader(reader) : null;
    }

    public async Task AddProductAsync(UpsertProductDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsert, conn);
        cmd.Parameters.AddWithValue("name", command.Name);
        cmd.Parameters.AddWithValue("price", command.Price);
        cmd.Parameters.AddWithValue("categoryId", command.CategoryId > 0 ? command.CategoryId : DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrl", (object?)command.ImageUrl ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateProductAsync(UpsertProductDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlUpdate, conn);
        cmd.Parameters.AddWithValue("id", command.Id);
        cmd.Parameters.AddWithValue("name", command.Name);
        cmd.Parameters.AddWithValue("price", command.Price);
        cmd.Parameters.AddWithValue("categoryId", command.CategoryId > 0 ? command.CategoryId : DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrl", (object?)command.ImageUrl ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlSoftDelete, conn);
        cmd.Parameters.AddWithValue("id", productId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<List<ProductDetailDto>> GetDeletedProductsAsync()
    {
        var list = new List<ProductDetailDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetDeleted, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapProductFromReader(reader));
        }
        return list;
    }

    public async Task<ProductDetailDto?> GetDeletedProductByIdAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetDeletedById, conn);
        cmd.Parameters.AddWithValue("id", productId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProductFromReader(reader) : null;
    }

    public async Task<bool> RestoreProductAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlRestore, conn);
        cmd.Parameters.AddWithValue("id", productId);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static ProductDetailDto MapProductFromReader(DbDataReader reader)
    {
        return new ProductDetailDto(
            reader.GetRequiredInt("id"),
            reader.GetRequiredString("name"),
            reader.GetRequiredDecimal("price"),
            reader["category_id"] is DBNull ? 0 : reader.GetRequiredInt("category_id"),
            reader["image_url"] is DBNull ? string.Empty : reader.GetRequiredString("image_url"));
    }
}
