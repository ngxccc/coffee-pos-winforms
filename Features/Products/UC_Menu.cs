using CoffeePOS.Data.Repositories;
using CoffeePOS.Models;

namespace CoffeePOS.Features.Products;

public class UC_Menu : UserControl
{
    // UI Components
    private FlowLayoutPanel _flowProducts = null!;
    private FlowLayoutPanel _flowCategories = null!;
    private List<Product> _currentFilteredList = [];
    private int _currentPage = 0;
    private const int PAGE_SIZE = 20;
    private bool _isLoading = false;

    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;

    // Data Cache
    private List<Product> _allProducts = [];
    private List<Category> _allCategories = [];

    // Events
    public event Action<int, string, decimal>? OnProductSelected;
    public event EventHandler? OnBackClicked;

    public UC_Menu(IProductRepository productRepo, ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;

        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 245, 245);

        InitializeComponents();
        LoadDataFromDatabase();

        RenderCategories();
        FilterProducts(0);
    }

    private void InitializeComponents()
    {
        Panel pnlHeader = BuildHeaderPanel();

        _flowCategories = new()
        {
            Dock = DockStyle.Top,
            Height = 60,
            BackColor = Color.White,
            Padding = new Padding(10, 5, 10, 5),
            AutoScroll = true,
            WrapContents = false
        };

        _flowProducts = new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(20, 0, 0, 0),
            BackColor = Color.FromArgb(245, 245, 245),
        };

        _flowProducts.Scroll += (s, e) => CheckScrollBottom();
        // WinForms FlowLayout đôi khi không fire event Scroll khi dùng chuột lăn (MouseWheel)
        // Nên cần bắt thêm cái này cho chắc cốp:
        _flowProducts.MouseWheel += (s, e) => CheckScrollBottom();

        Controls.Add(_flowProducts);    // Fill
        Controls.Add(_flowCategories);  // Top 2
        Controls.Add(pnlHeader);        // Top 1
    }

    private Panel BuildHeaderPanel()
    {
        Panel pnl = new()
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.White,
            Padding = new Padding(10)
        };

        Button btnBack = new()
        {
            Text = "Quay lại",
            Dock = DockStyle.Right,
            Width = 100,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(230, 230, 230),
            Cursor = Cursors.Hand
        };
        btnBack.Click += (s, e) => OnBackClicked?.Invoke(this, EventArgs.Empty);
        pnl.Controls.Add(btnBack);

        return pnl;
    }

    private void LoadDataFromDatabase()
    {
        try
        {
            _allCategories = _categoryRepo.GetCategories();
            _allProducts = _productRepo.GetProducts();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
            _allCategories = [];
            _allProducts = [];
        }
    }

    private void RenderCategories()
    {
        _flowCategories.Controls.Clear();

        foreach (var cat in _allCategories)
        {
            Button btnCat = new()
            {
                Text = cat.Name,
                Width = 120,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = (cat.Id == 0) ? Color.FromArgb(0, 122, 204) : Color.WhiteSmoke,
                ForeColor = (cat.Id == 0) ? Color.White : Color.Black,
                Cursor = Cursors.Hand,
                Tag = cat.Id,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnCat.FlatAppearance.BorderSize = 0;

            btnCat.Click += (s, e) =>
            {
                HighlightCategoryButton(btnCat);

                FilterProducts(cat.Id);
            };

            _flowCategories.Controls.Add(btnCat);
        }
    }

    private void FilterProducts(int categoryId)
    {
        if (categoryId == 0)
            _currentFilteredList = _allProducts;
        else
            _currentFilteredList = [.. _allProducts.Where(p => p.CategoryId == categoryId)];

        _currentPage = 0;
        _flowProducts.VerticalScroll.Value = 0;
        _flowProducts.Controls.Clear();
        _flowProducts.PerformLayout(); // Force layout update

        LoadNextPage();
    }

    private void LoadNextPage()
    {
        if (_isLoading) return;
        _isLoading = true;

        // Tính toán: Lấy từ đâu, lấy bao nhiêu
        // Skip: Bỏ qua những thằng đã load
        // Take: Lấy 20 thằng tiếp theo
        var productsToRender = _currentFilteredList
                                .Skip(_currentPage * PAGE_SIZE)
                                .Take(PAGE_SIZE)
                                .ToList();

        // Nếu hết hàng để lấy thì thôi
        if (productsToRender.Count == 0)
        {
            _isLoading = false;
            return;
        }

        _flowProducts.SuspendLayout(); // Tạm dừng vẽ để đỡ giật

        foreach (var p in productsToRender)
        {
            var pItem = new UC_ProductItem(p.Id, p.Name, p.Price);
            pItem.OnProductClicked += (s, e) =>
                OnProductSelected?.Invoke(p.Id, p.Name, p.Price);

            pItem.LoadImageAsync(p.Id);

            _flowProducts.Controls.Add(pItem);
        }

        _flowProducts.ResumeLayout();
        _currentPage++;
        _isLoading = false;
    }

    private void CheckScrollBottom()
    {
        // Công thức kiểm tra đã cuộn xuống đáy chưa
        // VerticalScroll.Value: Vị trí hiện tại của thanh cuộn
        // ClientSize.Height: Chiều cao vùng nhìn thấy
        // VerticalScroll.Maximum: Tổng chiều cao thực tế của nội dung

        // Hack: Cộng thêm 50px dung sai (buffer) để load sớm hơn xíu cho mượt
        if (_flowProducts.VerticalScroll.Value + _flowProducts.ClientSize.Height >= _flowProducts.VerticalScroll.Maximum - 100)
        {
            LoadNextPage();
        }
    }

    private void HighlightCategoryButton(Button activeBtn)
    {
        foreach (Control c in _flowCategories.Controls)
        {
            if (c is Button btn)
            {
                if (btn == activeBtn)
                {
                    btn.BackColor = Color.FromArgb(0, 122, 204); // Xanh dương
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.BackColor = Color.WhiteSmoke;
                    btn.ForeColor = Color.Black;
                }
            }
        }
    }
}
