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
