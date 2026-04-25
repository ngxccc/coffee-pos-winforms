using AntdUI;
using CoffeePOS.Forms.Core;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos.Category;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record ProductPayload(string Name, decimal Price, int CategoryId, string ImageUrl);

public partial class UC_ProductEditor : UserControl, IValidatableComponent<ProductPayload>
{
    private readonly int _productId;
    private readonly ICategoryQueryService _categoryQueryService;
    private readonly IProductQueryService _productQueryService;

    public string? OriginalImageUrl { get; private set; }

    public UC_ProductEditor(
        int productId,
        ICategoryQueryService categoryQueryService,
        IProductQueryService productQueryService)
    {
        _productId = productId;
        _categoryQueryService = categoryQueryService;
        _productQueryService = productQueryService;

        InitializeComponent();
        SetupEvents();

        Load += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await Spin.open(this, async cfg =>
        {
            try
            {
                var categories = await _categoryQueryService.GetSelectableCategoriesAsync();

                ProductDetailDto? product = null;
                if (_productId > 0)
                    product = await _productQueryService.GetProductByIdAsync(_productId);

                Invoke(() =>
                {
                    SetupBindings(categories);
                    if (product != null)
                    {
                        LoadExistingData(product);
                    }
                });
            }
            catch (Exception ex)
            {
                Invoke(() => MessageBoxHelper.Error($"Lỗi tải dữ liệu: {ex.Message}", owner: this));
            }
        });
    }

    private void SetupBindings(IReadOnlyList<CategoryOptionDto> categories)
    {
        _cboCategory.Items.Clear();

        var selectedItems = categories.
            Select(item => new SelectItem(item.Name, item.Id))
            .ToArray();

        _cboCategory.Items.AddRange(selectedItems);

        if (_cboCategory.Items.Count > 0)
            _cboCategory.SelectedIndex = 0;
    }

    private void SetupEvents()
    {
        _btnChooseImage.Click += HandleLocalImageUploadAsync;
        _txtImageUrl.TextChanged += async (_, _) => await PreviewImageAsync(_txtImageUrl.Text);

        _picImage.Click += HandleImageClick;
    }

    private void LoadExistingData(ProductDetailDto product)
    {
        OriginalImageUrl = product.ImageUrl;

        _txtName.Text = product.Name;
        _nudPrice.Value = product.Price;
        _cboCategory.SelectedValue = product.CategoryId;
        _txtImageUrl.Text = product.ImageUrl;

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            _ = PreviewImageAsync(product.ImageUrl);
        }
    }

    public bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBoxHelper.Warning("Tên món không được để trống!", owner: this);
            _txtName.Focus();
            return false;
        }

        if (_nudPrice.Value <= 0)
        {
            MessageBoxHelper.Warning("Giá bán phải lớn hơn 0.", owner: this);
            _nudPrice.Focus();
            return false;
        }

        if (_cboCategory.SelectedValue is not int)
        {
            MessageBoxHelper.Warning("Vui lòng chọn danh mục hợp lệ.", owner: this);
            _cboCategory.Focus();
            return false;
        }

        return true;
    }

    public ProductPayload GetPayload()
        => new(
            _txtName.Text.Trim(),
            _nudPrice.Value,
            (int)_cboCategory.SelectedValue!,
            _txtImageUrl.Text.Trim());

    private async void HandleLocalImageUploadAsync(object? sender, EventArgs e)
    {
        using OpenFileDialog ofd = new() { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp" };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        _btnChooseImage.Loading = true;
        _btnChooseImage.Enabled = false;

        try
        {
            string cloudUrl = await ImgbbHelper.UploadImageAsync(ofd.FileName);
            _txtImageUrl.Text = cloudUrl;
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Up ảnh xịt: {ex.Message}", owner: this, type: FeedbackType.Message);
        }
        finally
        {
            _btnChooseImage.Loading = false;
            _btnChooseImage.Enabled = true;
        }
    }

    private async Task PreviewImageAsync(string pathOrUrl)
    {
        if (string.IsNullOrWhiteSpace(pathOrUrl))
        {
            _picImage.Image?.Dispose();
            _picImage.Image = null;
            return;
        }

        string finalUrlToLoad = pathOrUrl;

        bool isWebUrl = Uri.TryCreate(pathOrUrl, UriKind.Absolute, out Uri? uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        if (!isWebUrl)
        {
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", pathOrUrl);

            if (File.Exists(localPath))
            {
                finalUrlToLoad = localPath;
            }
            else
            {
                _picImage.Image?.Dispose();
                _picImage.Image = null;
                return;
            }
        }

        try
        {
            await ImageHelper.LoadImageAsync(_picImage, finalUrlToLoad, "Preview", 0);
        }
        catch { /* Nuốt lỗi im lặng để không crash app */ }
    }

    private void HandleImageClick(object? sender, EventArgs e)
    {
        if (_picImage.Image != null)
        {
            Form form = FindForm() ?? throw new InvalidOperationException("Lỗi UI: UserControl chưa được gắn vào Form chính.");
            var config = new AntdUI.Preview.Config(form, _picImage.Image);

            AntdUI.Preview.open(config);
        }
    }
}
