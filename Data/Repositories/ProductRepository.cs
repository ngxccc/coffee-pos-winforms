using System.Data.Common;
using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Models;
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

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapProductFromReader(reader));
        }
        return list;
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetById, conn);
        cmd.Parameters.AddWithValue("id", productId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProductFromReader(reader) : null;
    }

    public async Task AddProductAsync(Product product)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsert, conn);
        cmd.Parameters.AddWithValue("name", product.Name);
        cmd.Parameters.AddWithValue("price", product.Price);
        cmd.Parameters.AddWithValue("categoryId", product.CategoryId > 0 ? product.CategoryId : DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrl", (object?)product.ImageUrl ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlUpdate, conn);
        cmd.Parameters.AddWithValue("id", product.Id);
        cmd.Parameters.AddWithValue("name", product.Name);
        cmd.Parameters.AddWithValue("price", product.Price);
        cmd.Parameters.AddWithValue("categoryId", product.CategoryId > 0 ? product.CategoryId : DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrl", (object?)product.ImageUrl ?? DBNull.Value);

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

    public async Task<List<Product>> GetDeletedProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetDeleted, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapProductFromReader(reader));
        }
        return list;
    }

    public async Task<Product?> GetDeletedProductByIdAsync(int productId)
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

    private static Product MapProductFromReader(DbDataReader reader)
    {
        return new Product
        {
            Id = reader.GetRequiredInt("id"),
            Name = reader.GetRequiredString("name"),
            Price = reader.GetRequiredDecimal("price"),

            CategoryId = reader["category_id"] is DBNull ? 0 : reader.GetRequiredInt("category_id"),
            ImageUrl = reader["image_url"] is DBNull ? string.Empty : reader.GetRequiredString("image_url"),

            IsDeleted = reader.GetRequiredBool("is_deleted"),
            CreatedAt = reader.GetDateOnlyAsDateTime("created_at"),
            UpdatedAt = reader.GetDateOnlyAsDateTime("updated_at"),
            DeletedAt = reader.GetNullableDateTime("deleted_at") ?? DateTime.MinValue
        };
    }
}
