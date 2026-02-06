using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories.Impl;

public class ProductRepository(string connStr) : IProductRepository
{
    private readonly string _connStr = connStr;

    public List<Category> GetCategories()
    {
        var list = new List<Category>
        {
            // Luôn thêm mục "Tất cả" đầu tiên
            new() { Id = 0, Name = "Tất cả" }
        };

        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, name FROM categories ORDER BY id", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            list.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }
        return list;
    }

    public List<Product> GetProducts()
    {
        var list = new List<Product>();
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();

        string sql = "SELECT id, name, price, category_id FROM products ORDER BY name";
        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
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
}
