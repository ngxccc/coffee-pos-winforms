using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Dtos.Category;
using CoffeePOS.Shared.Dtos.Product;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record ProductPayload(string Name, decimal Price, int CategoryId, string ImageUrl);

public partial class UC_ProductEditor : UserControl, IValidatableComponent<ProductPayload>
{
    public UC_ProductEditor(IReadOnlyList<CategoryOptionDto> categories, ProductDetailDto? existingProduct = null)
    {
        InitializeComponent();
        SetupBindings(categories);
        SetupEvents();

        if (existingProduct != null)
        {
            LoadExistingData(existingProduct);
        }
    }

    private void SetupBindings(IReadOnlyList<CategoryOptionDto> categories)
    {
        _cboCategory.DisplayMember = nameof(CategoryOptionDto.Name);
        _cboCategory.ValueMember = nameof(CategoryOptionDto.Id);
        _cboCategory.DataSource = categories.ToList();
    }

    private void SetupEvents()
    {
        _btnChooseImage.Click += HandleLocalImageUploadAsync;
        _txtImageUrl.TextChanged += async (_, _) => await PreviewImageAsync(_txtImageUrl.Text);
    }

    private void LoadExistingData(ProductDetailDto product)
    {
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
            string cloudUrl = await ImgurHelper.UploadImageAsync(ofd.FileName);
            _txtImageUrl.Text = cloudUrl;
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Up ảnh xịt: {ex.Message}", owner: this);
        }
        finally
        {
            _btnChooseImage.Loading = false;
            _btnChooseImage.Enabled = true;
        }
    }

    private async Task PreviewImageAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            _picImage.Image?.Dispose();
            _picImage.Image = null;
            return;
        }

        try
        {
            await ImageHelper.LoadImageAsync(_picImage, url, "Preview", 0);
        }
        catch { /* Nuốt lỗi im lặng để không crash app */ }
    }
}
