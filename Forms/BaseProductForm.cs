using CoffeePOS.Services.Contracts.Commands;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using Serilog;

namespace CoffeePOS.Forms;

public abstract class BaseProductForm : BaseCrudForm
{
    private int _productId;
    private int _initialCategoryId;
    private string _selectedImagePath = "";
    private string _currentSavedImage = "";

    private readonly ICategoryQueryService _categoryQueryService;
    private readonly Button _btnSave;

    protected readonly IProductService ProductService;
    protected readonly TextBox TxtName;
    protected readonly NumericUpDown NudPrice;
    protected readonly ComboBox CboCategory;
    protected readonly PictureBox PicImage;

    protected BaseProductForm(
        IProductService productService,
        ICategoryQueryService categoryQueryService,
        string title,
        string saveButtonText,
        Color saveButtonColor)
        : base(title, new Size(500, 530))
    {
        ProductService = productService;
        _categoryQueryService = categoryQueryService;

        Load += ProductForm_Load;

        TxtName = CreateTextBox(new Point(30, 55), 420, 12);

        NudPrice = new NumericUpDown
        {
            Location = new Point(30, 125),
            Width = 200,
            Font = new Font("Segoe UI", 12),
            Maximum = 10000000,
            Increment = 1000,
            ThousandsSeparator = true
        };

        CboCategory = new ComboBox
        {
            Location = new Point(250, 125),
            Width = 200,
            Font = new Font("Segoe UI", 12),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        PicImage = new PictureBox
        {
            Location = new Point(30, 195),
            Size = new Size(150, 150),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        var btnChooseImage = new Button
        {
            Text = "Chọn Ảnh",
            Location = new Point(200, 195),
            Size = new Size(100, 35),
            Cursor = Cursors.Hand
        };
        btnChooseImage.Click += (_, _) => ChooseImage();

        _btnSave = CreatePrimaryButton(saveButtonText, new Point(270, 440), new Size(100, 32), saveButtonColor);
        _btnSave.Click += async (_, _) => await SaveAsync();

        var btnCancel = CreateCancelButton(new Point(380, 440));
        btnCancel.Click += (_, _) => Close();

        Controls.AddRange(
        [
            CreateLabel("Tên món:", new Point(30, 30)),
            TxtName,
            CreateLabel("Giá bán (VNĐ):", new Point(30, 100)),
            NudPrice,
            CreateLabel("Danh mục:", new Point(250, 100)),
            CboCategory,
            CreateLabel("Hình ảnh:", new Point(30, 170)),
            PicImage,
            btnChooseImage,
            _btnSave,
            btnCancel
        ]);

        AcceptButton = _btnSave;
        CancelButton = btnCancel;
    }

    protected void LoadProductInternal(ProductDetailDto product, string title)
    {
        _productId = product.Id;
        TxtName.Text = product.Name;
        NudPrice.Value = product.Price;
        _initialCategoryId = product.CategoryId;
        Text = title;

        if (string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            _currentSavedImage = "";
            PicImage.Image?.Dispose();
            PicImage.Image = null;
            return;
        }

        _currentSavedImage = product.ImageUrl;
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", product.ImageUrl);
        LoadImageSafely(fullPath);
    }

    protected bool HasNewImage => !string.IsNullOrEmpty(_selectedImagePath);
    protected string CurrentSavedImage => _currentSavedImage;

    private async void ProductForm_Load(object? sender, EventArgs e)
    {
        try
        {
            var categories = await _categoryQueryService.GetSelectableCategoriesAsync();
            CboCategory.DataSource = categories;
            CboCategory.DisplayMember = nameof(CategoryOptionDto.Name);
            CboCategory.ValueMember = nameof(CategoryOptionDto.Id);

            if (_initialCategoryId > 0)
            {
                CboCategory.SelectedValue = _initialCategoryId;
            }
        }
        catch (Exception ex)
        {
            ShowLoadError(ex);
        }
    }

    private void ChooseImage()
    {
        using OpenFileDialog ofd = new();
        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";
        if (ofd.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        _selectedImagePath = ofd.FileName;
        LoadImageSafely(_selectedImagePath);
    }

    private void LoadImageSafely(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            return;
        }

        PicImage.Image?.Dispose();
        PicImage.Image = null;

        using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var img = Image.FromStream(fs);
        PicImage.Image = new Bitmap(img);
    }

    private async Task SaveAsync()
    {
        await ExecuteSaveAsync(_btnSave, async () =>
        {
            var finalFileName = SaveSelectedImageAndResolveName();

            var command = new UpsertProductDto(
                _productId,
                TxtName.Text.Trim(),
                NudPrice.Value,
                (int)CboCategory.SelectedValue!,
                finalFileName);

            await PersistAsync(command);
            await AfterPersistAsync();
        }, SuccessMessage);
    }

    private string SaveSelectedImageAndResolveName()
    {
        var finalFileName = _currentSavedImage;
        if (string.IsNullOrEmpty(_selectedImagePath))
        {
            return finalFileName;
        }

        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var extension = Path.GetExtension(_selectedImagePath);
        finalFileName = $"prod_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
        var destinationPath = Path.Combine(directory, finalFileName);

        File.Copy(_selectedImagePath, destinationPath, true);
        return finalFileName;
    }

    protected void TryDeletePreviousImage()
    {
        if (!HasNewImage || string.IsNullOrEmpty(_currentSavedImage))
        {
            return;
        }

        try
        {
            var oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", _currentSavedImage);
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"[Cảnh báo Dọn Rác]: Không thể xóa ảnh cũ '{_currentSavedImage}'. Chi tiết: {ex.Message}");
        }
    }

    protected virtual void ShowLoadError(Exception ex)
    {
        Shared.Helpers.MessageBoxHelper.Error($"Lỗi tải danh mục: {ex.Message}", owner: this);
    }

    protected virtual Task AfterPersistAsync() => Task.CompletedTask;

    protected abstract Task PersistAsync(UpsertProductDto command);
    protected abstract string SuccessMessage { get; }
}
