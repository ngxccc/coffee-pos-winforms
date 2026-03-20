using System.Data.Common;
using CoffeePOS.Models;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class CategoryRepository(NpgsqlDataSource dataSource) : ICategoryRepository
{
    private static readonly string SqlGetAll = SqlFileLoader.Load("Category.get_all.sql");
    private static readonly string SqlGetById = SqlFileLoader.Load("Category.get_by_id.sql");
    private static readonly string SqlInsert = SqlFileLoader.Load("Category.insert.sql");
    private static readonly string SqlUpdate = SqlFileLoader.Load("Category.update.sql");
    private static readonly string SqlSoftDelete = SqlFileLoader.Load("Category.soft_delete.sql");
    private static readonly string SqlSoftDeleteProductsByCategory = SqlFileLoader.Load("Category.soft_delete_products_by_category.sql");
    private static readonly string SqlGetDeleted = SqlFileLoader.Load("Category.get_deleted.sql");
    private static readonly string SqlGetDeletedById = SqlFileLoader.Load("Category.get_deleted_by_id.sql");
    private static readonly string SqlRestore = SqlFileLoader.Load("Category.restore.sql");

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var list = new List<Category>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
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

        using var cmd = new NpgsqlCommand(SqlGetById, conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategoryFromReader(reader) : null;
    }

    public async Task AddCategoryAsync(Category category)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsert, conn);
        cmd.Parameters.AddWithValue("name", category.Name);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlUpdate, conn);
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
            using var cmdCat = new NpgsqlCommand(SqlSoftDelete, conn, tx);
            cmdCat.Parameters.AddWithValue("id", id);

            int rowsAffected = await cmdCat.ExecuteNonQueryAsync();
            if (rowsAffected == 0) return false;

            using var cmdProd = new NpgsqlCommand(SqlSoftDeleteProductsByCategory, conn, tx);
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

        using var cmd = new NpgsqlCommand(SqlGetDeleted, conn);
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

        using var cmd = new NpgsqlCommand(SqlGetDeletedById, conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategoryFromReader(reader) : null;
    }

    public async Task<bool> RestoreCategoryAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlRestore, conn);
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
