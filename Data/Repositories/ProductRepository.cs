using System.Data.Common;
using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{
    private const string ProductColumns = """
        id, name, price, category_id, image_url,
        is_deleted, created_at, updated_at, deleted_at
        """;

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = $"""
            SELECT {ProductColumns}
            FROM products
            WHERE is_deleted = false
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
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

        string sql = $"""
            SELECT {ProductColumns}
            FROM products
            WHERE id = @id AND is_deleted = false
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProductFromReader(reader) : null;
    }

    public async Task AddProductAsync(Product product)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            INSERT INTO products (name, price, category_id, image_url)
            VALUES (@name, @price, @categoryId, @imageUrl);
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", product.Name);
        cmd.Parameters.AddWithValue("price", product.Price);
        cmd.Parameters.AddWithValue("categoryId", product.CategoryId > 0 ? product.CategoryId : DBNull.Value);
        cmd.Parameters.AddWithValue("imageUrl", (object?)product.ImageUrl ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            UPDATE products
            SET name = @name,
                price = @price,
                category_id = @categoryId,
                image_url = @imageUrl,
                updated_at = NOW()
            WHERE id = @id
              AND is_deleted = false;
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
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

        const string sql = """
            UPDATE products
            SET is_deleted = true,
                updated_at = NOW(),
                deleted_at = NOW()
            WHERE id = @id
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<List<Product>> GetDeletedProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = $"""
            SELECT {ProductColumns}
            FROM products
            WHERE is_deleted = true
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
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

        string sql = $"""
            SELECT {ProductColumns}
            FROM products
            WHERE id = @id
              AND is_deleted = true
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapProductFromReader(reader) : null;
    }

    public async Task<bool> RestoreProductAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            UPDATE products
            SET is_deleted = false,
                updated_at = NOW(),
                deleted_at = NULL
            WHERE id = @id
              AND is_deleted = true
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static Product MapProductFromReader(DbDataReader reader)
    {
        return new Product
        {
            Id = Convert.ToInt32(reader["id"]),
            Name = Convert.ToString(reader["name"]) ?? string.Empty,
            Price = Convert.ToDecimal(reader["price"]),

            CategoryId = reader["category_id"] is DBNull ? 0 : Convert.ToInt32(reader["category_id"]),
            ImageUrl = reader["image_url"] is DBNull ? string.Empty : Convert.ToString(reader["image_url"]) ?? string.Empty,

            IsDeleted = Convert.ToBoolean(reader["is_deleted"]),
            CreatedAt = Convert.ToDateTime(reader["created_at"]),
            UpdatedAt = Convert.ToDateTime(reader["updated_at"]),
            DeletedAt = reader["deleted_at"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(reader["deleted_at"])
        };
    }
}
