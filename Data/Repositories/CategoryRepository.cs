using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class CategoryRepository(NpgsqlDataSource dataSource) : ICategoryRepository
{
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT id, name, is_deleted FROM categories WHERE is_deleted = false ORDER BY id", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsDeleted = reader.GetBoolean(2)
            });
        }
        return list;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT id, name FROM categories WHERE id = @id AND is_deleted = false", conn);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            };
        return null;
    }

    public async Task AddCategoryAsync(Category category)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("INSERT INTO categories (name) VALUES (@name)", conn);
        cmd.Parameters.AddWithValue("name", category.Name);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("UPDATE categories SET name = @name WHERE id = @id AND is_deleted = false", conn);
        cmd.Parameters.AddWithValue("id", category.Id);
        cmd.Parameters.AddWithValue("name", category.Name);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var tx = await conn.BeginTransactionAsync();

        try
        {
            string sqlCat = "UPDATE categories SET is_deleted = true WHERE id = @id AND is_deleted = false";
            using var cmdCat = new NpgsqlCommand(sqlCat, conn, tx);
            cmdCat.Parameters.AddWithValue("id", id);
            int rowsAffected = await cmdCat.ExecuteNonQueryAsync();

            if (rowsAffected == 0) return false;

            string sqlProd = "UPDATE products SET is_deleted = true WHERE category_id = @id AND is_deleted = false";
            using var cmdProd = new NpgsqlCommand(sqlProd, conn, tx);
            cmdProd.Parameters.AddWithValue("id", id);
            await cmdProd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            Console.WriteLine($"[Lỗi Transaction Xóa Danh Mục]: {ex.Message}");
            throw;
        }
    }

    public async Task<List<Category>> GetDeletedCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("SELECT id, name, is_deleted FROM categories WHERE is_deleted = true ORDER BY id", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                IsDeleted = reader.GetBoolean(2)
            });
        }
        return list;
    }

    public async Task<Category?> GetDeletedCategoryByIdAsync(int categoryId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        string sql = "SELECT id, name FROM categories WHERE id = @id AND is_deleted = true";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", categoryId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
            return new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
            };

        return null;
    }

    public async Task<bool> RestoreCategoryAsync(int categoryId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand("UPDATE categories SET is_deleted = false WHERE id = @id AND is_deleted = true", conn);
        cmd.Parameters.AddWithValue("id", categoryId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
