using System.Data.Common;
using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class CategoryRepository(NpgsqlDataSource dataSource) : ICategoryRepository
{
    private static readonly string SqlGetAll = SqlFileLoader.Load(SqlKeys.Category.GetAll);
    private static readonly string SqlGetById = SqlFileLoader.Load(SqlKeys.Category.GetById);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.Category.Insert);
    private static readonly string SqlUpdate = SqlFileLoader.Load(SqlKeys.Category.Update);
    private static readonly string SqlSoftDelete = SqlFileLoader.Load(SqlKeys.Category.SoftDelete);
    private static readonly string SqlSoftDeleteProductsByCategory = SqlFileLoader.Load(SqlKeys.Category.SoftDeleteProductsByCategory);
    private static readonly string SqlGetDeleted = SqlFileLoader.Load(SqlKeys.Category.GetDeleted);
    private static readonly string SqlGetDeletedById = SqlFileLoader.Load(SqlKeys.Category.GetDeletedById);
    private static readonly string SqlRestore = SqlFileLoader.Load(SqlKeys.Category.Restore);

    public async Task<List<CategoryDetailDto>> GetAllCategoriesAsync()
    {
        var list = new List<CategoryDetailDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapCategoryFromReader(reader));
        }
        return list;
    }

    public async Task<CategoryDetailDto?> GetCategoryByIdAsync(int id)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetById, conn);
        cmd.Parameters.AddWithValue("id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCategoryFromReader(reader) : null;
    }

    public async Task AddCategoryAsync(UpsertCategoryDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsert, conn);
        cmd.Parameters.AddWithValue("name", command.Name);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateCategoryAsync(UpsertCategoryDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlUpdate, conn);
        cmd.Parameters.AddWithValue("id", command.Id);
        cmd.Parameters.AddWithValue("name", command.Name);

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

    public async Task<List<CategoryDetailDto>> GetDeletedCategoriesAsync()
    {
        var list = new List<CategoryDetailDto>();
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlGetDeleted, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(MapCategoryFromReader(reader));
        }
        return list;
    }

    public async Task<CategoryDetailDto?> GetDeletedCategoryByIdAsync(int id)
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

    private static CategoryDetailDto MapCategoryFromReader(DbDataReader reader)
    {
        return new CategoryDetailDto(
            reader.GetRequiredInt("id"),
            reader.GetRequiredString("name"));
    }
}
