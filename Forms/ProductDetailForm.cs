using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Forms;

public partial class ProductDetailForm : Form
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;

    private int _productId = 0;
    private string _selectedImagePath = "";
    private string _currentSavedImage = "";

    // UI Controls
    private TextBox txtName = null!;
    private NumericUpDown nudPrice = null!;
    private ComboBox cboCategory = null!;
    private PictureBox picImage = null!;
    private Button btnChooseImage = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    public ProductDetailForm(IProductRepository productRepo, ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        InitializeUI();
    }

    public void LoadProductDetails(Product product)
    {
        _productId = product.Id;
        txtName.Text = product.Name;
        nudPrice.Value = product.Price;
        cboCategory.SelectedValue = product.CategoryId;

        Text = $"CẬP NHẬT SẢN PHẨM: {product.Name}";
        btnSave.Text = "CẬP NHẬT";

        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            _currentSavedImage = product.ImageUrl;
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products", product.ImageUrl);
            LoadImageSafely(fullPath);
        }
    }

    private async void ProductDetailForm_Load(object? sender, EventArgs e)
    {
        try
        {
            var categories = (await _categoryRepo.GetAllCategoriesAsync())
                .Where(c => c.Id > 0)
                .ToList();
            cboCategory.DataSource = categories;
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "Id";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải danh mục: {ex.Message}");
        }
    }

    private void InitializeUI()
    {
        Text = "THÊM SẢN PHẨM MỚI";
        Size = new Size(500, 550);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.White;

        Load += ProductDetailForm_Load;

        Label lblName = new()
        {
            Text = "Tên món:",
            Location = new Point(30, 30),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        txtName = new TextBox
        {
            Location = new Point(30, 55),
            Width = 420,
            Font = new Font("Segoe UI", 12)
        };

        Label lblPrice = new()
        {
            Text = "Giá bán (VNĐ):",
            Location = new Point(30, 100),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        nudPrice = new NumericUpDown
        {
            Location = new Point(30, 125),
            Width = 200,
            Font = new Font("Segoe UI", 12),
            Maximum = 10000000,
            Increment = 1000,
            ThousandsSeparator = true
        };

        Label lblCategory = new()
        {
            Text = "Danh mục:",
            Location = new Point(250, 100),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        cboCategory = new ComboBox
        {
            Location = new Point(250, 125),
            Width = 200,
            Font = new Font("Segoe UI", 12),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        Label lblImage = new()
        {
            Text = "Hình ảnh:",
            Location = new Point(30, 170),
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        picImage = new PictureBox
        {
            Location = new Point(30, 195),
            Size = new Size(150, 150),
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom
        };

        btnChooseImage = new Button
        {
            Text = "Chọn Ảnh",
            Location = new Point(200, 195),
            Size = new Size(100, 35),
            Cursor = Cursors.Hand
        };
        btnChooseImage.Click += BtnChooseImage_Click;

        btnSave = new Button
        {
            Text = "LƯU MỚI",
            Location = new Point(240, 440),
            Size = new Size(120, 45),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSave.Click += async (s, e) => await SaveProductAsync();

        btnCancel = new Button
        {
            Text = "HỦY BỎ",
            Location = new Point(370, 440),
            Size = new Size(80, 45),
            BackColor = Color.Silver,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnCancel.Click += (s, e) => Close();

        Controls.AddRange([lblName, txtName, lblPrice, nudPrice, lblCategory, cboCategory, lblImage, picImage, btnChooseImage, btnSave, btnCancel]);
    }

    private void BtnChooseImage_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog ofd = new();
        ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            _selectedImagePath = ofd.FileName;
            LoadImageSafely(_selectedImagePath);
        }
    }

    private void LoadImageSafely(string imagePath)
    {
        if (!File.Exists(imagePath)) return;

        picImage.Image?.Dispose();

        using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        picImage.Image = Image.FromStream(fs);
    }

    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text) || nudPrice.Value <= 0 || cboCategory.SelectedValue == null)
        {
            MessageBox.Show("Vui lòng điền đầy đủ Tên, Giá và Chọn danh mục!");
            return;
        }

        if ((int)cboCategory.SelectedValue <= 0)
        {
            MessageBox.Show("Vui lòng chọn danh mục hợp lệ!");
            return;
        }

        btnSave.Enabled = false;

        try
        {
            string finalFileName = _currentSavedImage;

            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Products");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string extension = Path.GetExtension(_selectedImagePath);
                finalFileName = $"prod_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                string destinationPath = Path.Combine(directory, finalFileName);

                File.Copy(_selectedImagePath, destinationPath, true);
            }

            var product = new Product
            {
                Id = _productId,
                Name = txtName.Text.Trim(),
                Price = nudPrice.Value,
                CategoryId = (int)cboCategory.SelectedValue,
                ImageUrl = finalFileName
            };

            if (_productId == 0)
            {
                await _productRepo.AddProductAsync(product);
                MessageBox.Show("Thêm món mới thành công!");
            }
            else
            {
                await _productRepo.UpdateProductAsync(product);
                MessageBox.Show("Cập nhật món thành công!");
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu dữ liệu: {ex.Message}");
            btnSave.Enabled = true;
        }
    }
}
