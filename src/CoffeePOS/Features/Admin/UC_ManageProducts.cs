using AntdUI;
using CoffeePOS.Features.Admin.Controls;
using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin;

public partial class UC_ManageProducts : UserControl
{
    private readonly IProductService _productService;
    private readonly IProductQueryService _productQueryService;
    private readonly ICategoryQueryService _categoryQueryService;

    private List<ProductGridDto> _allProducts = [];
    private List<ProductGridDto> _filteredProducts = [];

    public UC_ManageProducts(IProductService productService, IProductQueryService productQueryService, ICategoryQueryService categoryQueryService)
    {
        _productService = productService;
        _productQueryService = productQueryService;
        _categoryQueryService = categoryQueryService;

        InitializeComponent();
        SetupTable();
        SetupEvents();

        _ = LoadDataAsync();
    }

    private void SetupTable()
    {
        _tableProducts.Columns =
        [
            new Column(nameof(ProductGridDto.Id), DtoInfo.GetName<ProductGridDto>(nameof(ProductGridDto.Id)))
            {
                Width = "80",
                Align = ColumnAlign.Center
            },
            new Column("Name", "TÊN SẢN PHẨM"),
            new Column("CategoryName", "DANH MỤC") { Width = "200" },
            new Column("Price", "ĐƠN GIÁ") { Width = "150", DisplayFormat = "N0", Align = ColumnAlign.Right }
        ];
    }

    private void SetupEvents()
    {
        _toolbar.SearchChanged += ApplyFilterAndSort;
        _toolbar.AddClicked += HandleAddProduct;
        _toolbar.EditClicked += HandleEditProduct;
        _toolbar.DeleteClicked += HandleDeleteProduct;
        _toolbar.TrashModeChanged += HandleTrashModeChanged;

        _tableProducts.CellDoubleClick += HandleEditProduct;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _allProducts = await _productQueryService.GetProductGridAsync(_toolbar.IsTrashMode);
            ApplyFilterAndSort();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", type: FeedbackType.Message);
        }
    }

    private void ApplyFilterAndSort()
    {
        string keyword = _toolbar.SearchText.Trim();

        _filteredProducts = string.IsNullOrEmpty(keyword)
            ? [.. _allProducts]
            : [.. _allProducts.Where(c => c.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))];

        _tableProducts.DataSource = _filteredProducts;
    }

    private async void HandleTrashModeChanged(object? sender, EventArgs e)
    {
        _tableProducts.BackColor = _toolbar.IsTrashMode ? Color.MistyRose : UiTheme.Surface;
        await LoadDataAsync();
    }

    private async void HandleDeleteProduct(object? sender, EventArgs e)
    {
        if (_tableProducts.SelectedIndexs == null || _tableProducts.SelectedIndexs.Length == 0) return;

        var selectedProduct = _filteredProducts[_tableProducts.SelectedIndexs[0]];

        try
        {
            if (_toolbar.IsTrashMode)
            {
                if (MessageBoxHelper.ConfirmWarning($"Khôi phục '{selectedProduct.Name}' trở lại Menu bán hàng?", "Xác nhận", this))
                {
                    await _productService.RestoreProductAsync(selectedProduct.Id);
                }
            }
            else
            {
                if (MessageBoxHelper.ConfirmWarning($"Xóa món '{selectedProduct.Name}' khỏi Menu?\n(Báo cáo cũ vẫn giữ nguyên)", "Xác nhận", this))
                {
                    await _productService.DeleteProductAsync(selectedProduct.Id);
                }
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Warning(ex.Message, "Cảnh báo", this);
        }
    }

    private async void HandleAddProduct(object? sender, EventArgs e)
    {
        try
        {
            var categories = await _categoryQueryService.GetSelectableCategoriesAsync();
            var uiFields = new UC_ProductFields(categories);

            Target target = new(this);
            var config = new Modal.Config(target, "THÊM SẢN PHẨM MỚI", uiFields)
            {
                OkText = "LƯU",
                CancelText = "HUỶ",
                OnOk = (cfg) =>
                {
                    if (!uiFields.ValidateInput()) return false;
                    var payload = uiFields.GetPayload();

                    _ = ExecuteSaveProductAsync(payload, isUpdate: false, productId: 0);
                    return false;
                }
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    private async void HandleEditProduct(object? sender, EventArgs e)
    {
        if (_tableProducts.SelectedIndexs == null || _tableProducts.SelectedIndexs.Length == 0) return;
        var selectedItem = _filteredProducts[_tableProducts.SelectedIndexs[0]];

        try
        {
            var product = await _productQueryService.GetProductByIdAsync(selectedItem.Id);
            if (product == null)
            {
                MessageBoxHelper.Error("Không tìm thấy sản phẩm!", type: FeedbackType.Message);
                return;
            }

            var categories = await _categoryQueryService.GetSelectableCategoriesAsync();
            var uiFields = new UC_ProductFields(categories, product);

            Target target = new(this);
            var config = new Modal.Config(target, $"CẬP NHẬT: {product.Name}", uiFields)
            {
                OkText = "CẬP NHẬT",
                CancelText = "HUỶ",
                OnOk = (cfg) =>
                {
                    if (!uiFields.ValidateInput()) return false;
                    var payload = uiFields.GetPayload();

                    _ = ExecuteSaveProductAsync(payload, isUpdate: true, productId: product.Id, product.ImageUrl);
                    return false;
                }
            };
            Modal.open(config);
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi mở form: {ex.Message}", owner: this);
        }
    }

    // BACKGROUND TASK (API Call + File I/O)
    private async Task ExecuteSaveProductAsync(ProductPayload payload, bool isUpdate, int productId, string? oldImageUrl = null)
    {
        Form form = new Target(this).GetForm ?? new Form();
        AntdUI.Message.loading(form, "Đang xử lý...", async msg =>
        {
            msg.ID = "save_prod";
            bool isSuccess = false;

            try
            {
                // WHY: File I/O nên đẩy ra background để không block UI thread
                string finalFileName = await Task.Run(() => SaveSelectedImageAndResolveName(payload.ImageUrl, oldImageUrl ?? string.Empty));

                var dto = new UpsertProductDto(productId, payload.Name, payload.Price, payload.CategoryId, finalFileName);

                if (isUpdate)
                    await _productService.UpdateProductAsync(dto);
                else
                    await _productService.AddProductAsync(dto);

                if (isUpdate && !string.IsNullOrWhiteSpace(payload.ImageUrl))
                {
                    _ = Task.Run(() => TryDeletePreviousImage(oldImageUrl));
                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => MessageBoxHelper.Error(ex.Message, type: FeedbackType.Message)));
            }
            finally
            {
                Invoke(new Action(() => AntdUI.Message.close_id("save_prod")));
            }

            if (isSuccess)
            {
                Invoke(new Action(() => MessageBoxHelper.Info("Lưu thành công!", type: FeedbackType.Message)));
                await Task.Delay(500);
                Invoke(new Action(() =>
                {
                    if (form != null && !form.IsDisposed) form.Close();
                    _ = LoadDataAsync();
                }));
            }
        });
    }

    private static string SaveSelectedImageAndResolveName(string selectedImagePath, string currentSavedImage)
    {
        // WHY: Bỏ qua disk I/O nếu user không chọn ảnh mới hoặc đường dẫn ảo.
        // PERF: O(1) Time complexity.
        if (string.IsNullOrWhiteSpace(selectedImagePath) || !File.Exists(selectedImagePath))
        {
            return currentSavedImage;
        }

        // WHY: Sử dụng Local App Directory để đảm bảo tính di động (Portable) khi deploy app sang máy khác.
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var extension = Path.GetExtension(selectedImagePath);

        string finalFileName = $"prod_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}{extension}";
        var destinationPath = Path.Combine(directory, finalFileName);

        File.Copy(selectedImagePath, destinationPath, true);

        return finalFileName;
    }

    private static void TryDeletePreviousImage(string? currentSavedImage)
    {
        // WHY: Graceful exit nếu sản phẩm cũ chưa từng có ảnh.
        if (string.IsNullOrWhiteSpace(currentSavedImage))
        {
            return;
        }

        try
        {
            var oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", currentSavedImage);

            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }
        catch
        {
            // HACK: Silently swallow (nuốt lỗi) I/O Exceptions (như file đang bị khóa, không có quyền truy cập).
            // WHY: Việc xóa ảnh cũ thất bại KHÔNG ĐƯỢC PHÉP làm crash luồng lưu Database chính của hệ thống.
            // TODO: Ghi log tên file bị kẹt này vào Serilog để chạy Cron-job dọn rác (Garbage Collector) vào nửa đêm.
        }
    }
}
