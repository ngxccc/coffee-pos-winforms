using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories.Impl;

public class ProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var list = new List<Product>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = @"SELECT id, name, price, category_id
                    FROM products
                    WHERE is_deleted = false
                    ORDER BY name";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
            });
        }
        return list;
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
}
