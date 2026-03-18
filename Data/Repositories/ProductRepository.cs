using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"SELECT id, name, price, category_id, image_url, is_deleted
                    FROM products
                    WHERE is_deleted = false";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsDeleted = reader.GetBoolean(5)
            });
        }
        return list;
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"SELECT id, name, price, category_id, image_url
                    FROM products
                    WHERE id = @id AND is_deleted = false";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);
        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }
        return null;
    }

    public async Task AddProductAsync(Product product)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = @"
            INSERT INTO products (name, price, category_id, image_url)
            VALUES (@name, @price, @categoryId, @imageUrl);";

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

        const string sql = @"
            UPDATE products
            SET name = @name,
                price = @price,
                category_id = @categoryId,
                image_url = @imageUrl
            WHERE id = @id
              AND is_deleted = false;";

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

        string sql = "UPDATE products SET is_deleted = true WHERE id = @id";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<List<Product>> GetDeletedProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"SELECT id, name, price, category_id, image_url, is_deleted
                    FROM products
                    WHERE is_deleted = true";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsDeleted = reader.GetBoolean(5)
            });
        }
        return list;
    }

    public async Task<Product?> GetDeletedProductByIdAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        string sql = "SELECT id, name, price, category_id, image_url FROM products WHERE id = @id AND is_deleted = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", productId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                CategoryId = reader.GetInt32(3)
            };

        return null;
    }

    public async Task<bool> RestoreProductAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("UPDATE products SET is_deleted = false WHERE id = @id AND is_deleted = true", conn);
        cmd.Parameters.AddWithValue("id", productId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
