using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories.Impl;

public class CategoryRepository(NpgsqlDataSource dataSource) : ICategoryRepository
{

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var list = new List<Category>
        {
            // Luôn thêm mục "Tất cả" đầu tiên
            new() { Id = 0, Name = "Tất cả" }
        };

        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand("SELECT id, name FROM categories ORDER BY id", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }
        return list;
    }
}
