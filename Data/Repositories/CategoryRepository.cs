using System.Data.Common;
using CoffeePOS.Models;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class CategoryRepository(NpgsqlDataSource dataSource) : ICategoryRepository
{
    private const string CategoryColumns = """
        id, name, is_deleted, created_at, updated_at, deleted_at
        """;

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = $"""
            SELECT {CategoryColumns}
            FROM categories
            WHERE is_deleted = false
            ORDER BY id
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapCategoryFromReader(reader));
        }
        return list;
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = $"""
            SELECT {CategoryColumns}
            FROM categories
            WHERE id = @id AND is_deleted = false
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategoryFromReader(reader) : null;
    }

    public async Task AddCategoryAsync(Category category)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            INSERT INTO categories (name)
            VALUES (@name)
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", category.Name);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            UPDATE categories
            SET name = @name,
                updated_at = NOW()
            WHERE id = @id
              AND is_deleted = false
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
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
            const string sqlCat = """
                UPDATE categories
                SET is_deleted = true,
                    updated_at = NOW(),
                    deleted_at = NOW()
                WHERE id = @id
                  AND is_deleted = false
            """;

            using var cmdCat = new NpgsqlCommand(sqlCat, conn, tx);
            cmdCat.Parameters.AddWithValue("id", id);

            int rowsAffected = await cmdCat.ExecuteNonQueryAsync();
            if (rowsAffected == 0) return false;

            const string sqlProd = """
                UPDATE products
                SET is_deleted = true,
                    updated_at = NOW(),
                    deleted_at = NOW()
                WHERE category_id = @id
                  AND is_deleted = false
            """;

            using var cmdProd = new NpgsqlCommand(sqlProd, conn, tx);
            cmdProd.Parameters.AddWithValue("id", id);
            await cmdProd.ExecuteNonQueryAsync();

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Category>> GetDeletedCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = $"""
            SELECT {CategoryColumns}
            FROM categories
            WHERE is_deleted = true
            ORDER BY id
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapCategoryFromReader(reader));
        }
        return list;
    }

    public async Task<Category?> GetDeletedCategoryByIdAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        string sql = $"""
            SELECT {CategoryColumns}
            FROM categories
            WHERE id = @id AND is_deleted = true
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategoryFromReader(reader) : null;
    }

    public async Task<bool> RestoreCategoryAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        const string sql = """
            UPDATE categories
            SET is_deleted = false,
                updated_at = NOW(),
                deleted_at = NULL
            WHERE id = @id
              AND is_deleted = true
        """;

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static Category MapCategoryFromReader(DbDataReader reader)
    {
        return new Category
        {
            Id = Convert.ToInt32(reader["id"]),
            Name = Convert.ToString(reader["name"]) ?? string.Empty,
            IsDeleted = Convert.ToBoolean(reader["is_deleted"]),
            CreatedAt = Convert.ToDateTime(reader["created_at"]),
            UpdatedAt = Convert.ToDateTime(reader["updated_at"]),
            DeletedAt = reader["deleted_at"] is DBNull ? DateTime.MinValue : Convert.ToDateTime(reader["deleted_at"])
        };
    }
}
