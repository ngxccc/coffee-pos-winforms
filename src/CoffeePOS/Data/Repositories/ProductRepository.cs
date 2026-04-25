using System.Data.Common;
using CoffeePOS.Data.Repositories.Contracts;

using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;
using Npgsql;

namespace CoffeePOS.Data.Repositories;

public class ProductRepository(NpgsqlDataSource dataSource) : IProductRepository
{
    private static readonly string SqlGetAll = SqlFileLoader.Load(SqlKeys.Product.GetAll);
    private static readonly string SqlGetById = SqlFileLoader.Load(SqlKeys.Product.GetById);
    private static readonly string SqlInsert = SqlFileLoader.Load(SqlKeys.Product.Insert);
    private static readonly string SqlUpdate = SqlFileLoader.Load(SqlKeys.Product.Update);
    private static readonly string SqlSoftDelete = SqlFileLoader.Load(SqlKeys.Product.SoftDelete);
    private static readonly string SqlRestore = SqlFileLoader.Load(SqlKeys.Product.Restore);

    public async Task<List<ProductDetailDto>> GetAllProductsAsync()
        => await GetProductsByDeletedStateAsync(isDeleted: false);

    public async Task<ProductDetailDto?> GetProductByIdAsync(int productId)
        => await GetProductByIdAndDeletedStateAsync(productId, isDeleted: false);

    public async Task AddProductAsync(UpsertProductDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlInsert, conn);
        cmd.Parameters.Add(new NpgsqlParameter<string>("name", command.Name));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("price", command.Price));
        cmd.Parameters.Add(new NpgsqlParameter<int>("category_id", command.CategoryId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("image_url", command.ImageUrl ?? string.Empty));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateProductAsync(UpsertProductDto command)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlUpdate, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", command.Id));
        cmd.Parameters.Add(new NpgsqlParameter<string>("name", command.Name));
        cmd.Parameters.Add(new NpgsqlParameter<decimal>("price", command.Price));
        cmd.Parameters.Add(new NpgsqlParameter<int>("category_id", command.CategoryId));
        cmd.Parameters.Add(new NpgsqlParameter<string>("image_url", command.ImageUrl ?? string.Empty));

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();

        using var cmd = new NpgsqlCommand(SqlSoftDelete, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", productId));

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<List<ProductDetailDto>> GetDeletedProductsAsync()
        => await GetProductsByDeletedStateAsync(isDeleted: true);

    public async Task<ProductDetailDto?> GetDeletedProductByIdAsync(int productId)
    {
        return await GetProductByIdAndDeletedStateAsync(productId, isDeleted: true);
    }

    public async Task<bool> RestoreProductAsync(int productId)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlRestore, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", productId));
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    #region Helpers

    private async Task<List<ProductDetailDto>> GetProductsByDeletedStateAsync(bool isDeleted)
    {
        var dict = new Dictionary<int, ProductDetailDto>();

        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlGetAll, conn);
        cmd.Parameters.Add(new NpgsqlParameter<bool>("is_deleted", isDeleted));
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            int id = reader.GetRequired<int>("id");
            if (!dict.TryGetValue(id, out var product))
            {
                product = MapProductFromReader(reader);
                dict.Add(id, product);
            }
            MapAndAddSize(reader, product);
        }

        return [.. dict.Values];
    }

    private async Task<ProductDetailDto?> GetProductByIdAndDeletedStateAsync(int productId, bool isDeleted)
    {
        using var conn = await dataSource.OpenConnectionAsync();
        using var cmd = new NpgsqlCommand(SqlGetById, conn);
        cmd.Parameters.Add(new NpgsqlParameter<int>("id", productId));
        cmd.Parameters.Add(new NpgsqlParameter<bool>("is_deleted", isDeleted));

        using var reader = await cmd.ExecuteReaderAsync();
        ProductDetailDto? product = null;

        while (await reader.ReadAsync())
        {
            product ??= MapProductFromReader(reader);
            MapAndAddSize(reader, product);
        }

        return product;
    }

    private static ProductDetailDto MapProductFromReader(DbDataReader reader)
    {
        return new ProductDetailDto(
            reader.GetRequired<int>("id"),
            reader.GetRequired<string>("name"),
            reader.GetRequired<decimal>("price"),
            reader["category_id"] is DBNull ? 0 : reader.GetRequired<int>("category_id"),
            reader["image_url"] is DBNull ? string.Empty : reader.GetRequired<string>("image_url"),
            []
        );
    }

    private static void MapAndAddSize(DbDataReader reader, ProductDetailDto product)
    {
        if (product.Sizes == null)
            return;

        // Tránh throw exception nếu câu SQL không Select cột size_name
        if (!HasColumn(reader, "size_name") || reader["size_name"] is DBNull)
            return;

        string sizeName = reader.GetRequired<string>("size_name");

        // Tránh lặp size nếu câu truy vấn SQL bị nổ số lượng dòng
        if (!product.Sizes.Any(s => s.SizeName == sizeName))
        {
            product.Sizes.Add(new ProductSizeDto
            (
                product.Id,
                sizeName,
                reader.GetRequired<decimal>("price_adjustment")
            ));
        }
    }

    // Tool hỗ trợ check cột tồn tại trong DataReader an toàn
    private static bool HasColumn(DbDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    #endregion
}
