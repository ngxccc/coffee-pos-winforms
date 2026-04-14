using CoffeePOS.Forms.Core;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Admin.Controls;

public record ProductPayload(string Name, decimal Price, int CategoryId, string SelectedImagePath);

public class UC_ProductFields : UserControl, IValidatableComponent<ProductPayload>
{
    private readonly TextBox _txtName;
    private readonly NumericUpDown _nudPrice;
    private readonly ComboBox _cboCategory;
    private readonly PictureBox _picImage;
    private string _selectedImagePath = string.Empty;

    public UC_ProductFields(IReadOnlyList<CategoryOptionDto> categories, ProductDetailDto? existingProduct = null)
    {
        Dock = DockStyle.Fill;
        BackColor = Color.White;

        Controls.Add(new Label
        {
            Text = "Tên món",
            Location = new Point(20, 20),
            AutoSize = true
        });
        _txtName = new TextBox
        {
            Location = new Point(20, 45),
            Width = 420,
            Font = new Font("Segoe UI", 11)
        };
        Controls.Add(_txtName);

        Controls.Add(new Label
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

        Controls.Add(new Label
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

        Controls.Add(new Label
        {
            Text = "Hình ảnh",
            Location = new Point(20, 150),
            AutoSize = true
        });
        _picImage = new PictureBox
        {
            Location = new Point(20, 175),
            Size = new Size(150, 150),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };
        Controls.Add(_picImage);

        var btnChooseImage = new Button
        {
            Text = "Chọn ảnh",
            Location = new Point(190, 175),
            Size = new Size(120, 35),
            Cursor = Cursors.Hand
        };
        btnChooseImage.Click += (_, _) => ChooseImage();
        Controls.Add(btnChooseImage);

        if (existingProduct != null)
        {
            _txtName.Text = existingProduct.Name;
            _nudPrice.Value = existingProduct.Price;
            _cboCategory.SelectedValue = existingProduct.CategoryId;

            if (!string.IsNullOrWhiteSpace(existingProduct.ImageUrl))
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", existingProduct.ImageUrl);
                LoadImageSafely(fullPath);
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
            _selectedImagePath);

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

        _picImage.Image?.Dispose();
        _picImage.Image = null;

        using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var img = Image.FromStream(fs);
        _picImage.Image = new Bitmap(img);
    }
}
