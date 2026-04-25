using CoffeePOS.Data.Repositories.Contracts;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;

namespace CoffeePOS.Services;

// WHY: Inject IProductQueryService to fetch base product price for domain rule validation.
public class ProductSizeService(
    IProductSizeRepository sizeRepository,
    IProductQueryService productQueryService) : IProductSizeService
{
    private readonly IProductSizeRepository _sizeRepository = sizeRepository;
    private readonly IProductQueryService _productQueryService = productQueryService;

    public async Task<int> AddProductSizeAsync(UpsertProductSizeDto dto)
    {
        await ValidatePriceAdjustmentAsync(dto.ProductId, dto.PriceAdjustment);
        return await _sizeRepository.InsertSizeAsync(dto);
    }

    public async Task<bool> UpdateProductSizeAsync(UpsertProductSizeDto dto)
    {
        await ValidatePriceAdjustmentAsync(dto.ProductId, dto.PriceAdjustment);
        return await _sizeRepository.UpdateSizeAsync(dto);
    }

    public async Task<bool> DeleteProductSizeAsync(int id)
    {
        return await _sizeRepository.DeleteSizeAsync(id);
    }

    // PERF: Time Complexity O(1).
    // HACK: Single point of truth for price calculation rule.
    private async Task ValidatePriceAdjustmentAsync(int productId, decimal priceAdjustment)
    {
        if (priceAdjustment >= 0) return;

        var product = await _productQueryService.GetProductByIdAsync(productId)
            ?? throw new ArgumentException($"Sản phẩm với mã {productId} không tồn tại.");

        if (product.Price + priceAdjustment < 0)
        {
            throw new InvalidOperationException($"Giá điều chỉnh giảm ({priceAdjustment:N0}đ) đã vượt quá giá gốc của món ({product.Price:N0}đ).");
        }
    }
}
