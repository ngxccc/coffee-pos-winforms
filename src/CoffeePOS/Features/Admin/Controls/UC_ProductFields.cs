using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record ProductPayload(string Name, decimal Price, int CategoryId, string ImageUrl);

public class UC_ProductFields : UserControl, IValidatableComponent<ProductPayload>
{
    private readonly AntdUI.Input _txtName;
    private readonly NumericUpDown _nudPrice;
    private readonly ComboBox _cboCategory;
    private readonly AntdUI.Input _txtImageUrl;
    private readonly PictureBox _picImage;
    private readonly AntdUI.Button _btnChooseImage;

    public UC_ProductFields(IReadOnlyList<CategoryOptionDto> categories, ProductDetailDto? existingProduct = null)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        Controls.Add(new AntdUI.Label
        {
            Text = "Tên món",
            Location = new Point(20, 20),
            AutoSize = true
        });
        _txtName = new AntdUI.Input
        {
            Location = new Point(20, 45),
            Width = 420,
            Font = new Font("Segoe UI", 11),
            PlaceholderText = "Nhập tên món",
            AllowClear = true
        };
        Controls.Add(_txtName);

        Controls.Add(new AntdUI.Label
        {
            Text = "Giá bán (VNĐ)",
            Location = new Point(20, 85),
            AutoSize = true
        });
        _nudPrice = new NumericUpDown
        {
            Location = new Point(20, 110),
            Width = 200,
            Font = new Font("Segoe UI", 11),
            Maximum = 10000000,
            Increment = 1000,
            ThousandsSeparator = true
        };
        Controls.Add(_nudPrice);

        Controls.Add(new AntdUI.Label
        {
            Text = "Danh mục",
            Location = new Point(240, 85),
            AutoSize = true
        });
        _cboCategory = new ComboBox
        {
            Location = new Point(240, 110),
            Width = 200,
            Font = new Font("Segoe UI", 11),
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(CategoryOptionDto.Name),
            ValueMember = nameof(CategoryOptionDto.Id),
            DataSource = categories.ToList()
        };
        Controls.Add(_cboCategory);

        Controls.Add(new AntdUI.Label
        {
            Text = "Link Hình Ảnh (URL) hoặc Upload",
            Location = new Point(20, 150),
            AutoSize = true
        });

        _txtImageUrl = new AntdUI.Input
        {
            Location = new Point(20, 175),
            Width = 340,
            Font = new Font("Segoe UI", 11),
            PlaceholderText = "https://i.imgur.com/...",
            AllowClear = true
        };
        // PERF: Async loading preview when user manually pastes a URL
        _txtImageUrl.TextChanged += async (_, _) => await PreviewImageAsync(_txtImageUrl.Text);
        Controls.Add(_txtImageUrl);

        _btnChooseImage = new AntdUI.Button
        {
            Text = "Up ảnh",
            Location = new Point(370, 175),
            Size = new Size(70, 38),
            Type = AntdUI.TTypeMini.Primary,
            Cursor = Cursors.Hand
        };
        _btnChooseImage.Click += HandleLocalImageUploadAsync;
        Controls.Add(_btnChooseImage);

        _picImage = new PictureBox
        {
            Location = new Point(20, 220),
            Size = new Size(150, 150),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        Controls.Add(_picImage);

        if (existingProduct != null)
        {
            _txtName.Text = existingProduct.Name;
            _nudPrice.Value = existingProduct.Price;
            _cboCategory.SelectedValue = existingProduct.CategoryId;
            _txtImageUrl.Text = existingProduct.ImageUrl;

            if (!string.IsNullOrWhiteSpace(existingProduct.ImageUrl))
            {
                _ = PreviewImageAsync(existingProduct.ImageUrl);
            }
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
        using OpenFileDialog ofd = new();
        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";
        if (ofd.ShowDialog() != DialogResult.OK) return;

        _btnChooseImage.Loading = true;
        _btnChooseImage.Enabled = false;

        try
        {
            // Network I/O execution
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
        catch
        {
            // BUG: Silent fail on preview to prevent UI crashing if user pastes a broken URL
        }
    }
}
