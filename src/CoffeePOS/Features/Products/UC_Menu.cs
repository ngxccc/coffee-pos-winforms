using AntdUI;
using CoffeePOS.Services.Contracts.Queries;
using CoffeePOS.Shared.Dtos;
using CoffeePOS.Shared.Helpers;

namespace CoffeePOS.Features.Products;

public partial class UC_Menu : UserControl
{
    private readonly IProductQueryService _productQueryService;
    private readonly ICategoryQueryService _categoryQueryService;

    private List<ProductDetailDto> _allProducts = [];
    private List<CategoryOptionDto> _allCategories = [];
    private List<ProductDetailDto> _currentFilteredList = [];

    private int _currentPage = 0;
    private const int PAGE_SIZE = 20;
    private bool _isLoading = false;
    private int _currentCatId = 0;

    public event Action<int, string, decimal, string>? OnProductSelected;

    public UC_Menu(IProductQueryService productQueryService, ICategoryQueryService categoryQueryService)
    {
        _productQueryService = productQueryService;
        _categoryQueryService = categoryQueryService;

        InitializeComponent();
        WireEvents();

        _ = LoadDataFromDatabaseAsync();
    }

    private void WireEvents()
    {
        _txtSearch.TextChanged += TxtSearch_TextChanged;

        _menuCategories.SelectChanged += (s, e) =>
        {
            if (e.Value.Tag is int categoryId)
            {
                _txtSearch.Text = "";
                FilterProducts(categoryId);
                _currentCatId = categoryId;
            }
        };

        _flowProducts.MouseWheel += (s, e) => CheckScrollBottom();
        _flowProducts.Scroll += (s, e) =>
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                CheckScrollBottom();
            }
        };
    }

    private async Task LoadDataFromDatabaseAsync()
    {
        try
        {
            var categoryTask = _categoryQueryService.GetSelectableCategoriesAsync();
            var productTask = _productQueryService.GetAllProductsAsync();

            await Task.WhenAll(categoryTask, productTask);

            _allCategories = categoryTask.Result;
            _allProducts = productTask.Result;

            RenderCategories();
        }
        catch (Exception ex)
        {
            MessageBoxHelper.Error($"Lỗi nạp thực đơn: {ex.Message}", owner: this);
        }
    }

    private void RenderCategories()
    {
        _menuCategories.Items.Clear();

        _menuCategories.Items.Add(new MenuItem { Text = "TẤT CẢ", Tag = 0 });

        foreach (var cat in _allCategories)
        {
            _menuCategories.Items.Add(new MenuItem { Text = cat.Name.ToUpper(), Tag = cat.Id });
        }

        if (_menuCategories.Items.Count > 0)
        {
            _menuCategories.SelectIndex(0);
        }
    }

    private void FilterProducts(int categoryId)
    {
        if (categoryId == 0)
        {
            _currentFilteredList = [.. _allProducts];
        }
        else
        {
            _currentFilteredList = [.. _allProducts.Where(p => p.CategoryId == categoryId)];
        }

        _currentPage = 0;
        _flowProducts.Controls.Clear();
        LoadNextPage();
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        string keyword = _txtSearch.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(keyword))
        {
            FilterProducts(_currentCatId);
            return;
        }

        _currentFilteredList = [.. _allProducts.Where(p => p.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) || p.Price.ToString().Contains(keyword))];

        _currentPage = 0;
        _flowProducts.Controls.Clear();
        LoadNextPage();
    }

    private void LoadNextPage()
    {
        if (_isLoading) return;

        int totalItems = _currentFilteredList.Count;
        int startIndex = _currentPage * PAGE_SIZE;

        if (startIndex >= totalItems) return;

        _isLoading = true;
        _flowProducts.SuspendLayout();

        var pageItems = _currentFilteredList.Skip(startIndex).Take(PAGE_SIZE).ToList();

        foreach (var p in pageItems)
        {
            var pItem = new UC_ProductItem(p.Id, p.Name, p.Price, p.ImageUrl);
            pItem.OnProductClicked += (s, e) =>
                OnProductSelected?.Invoke(p.Id, p.Name, p.Price, p.ImageUrl);

            pItem.LoadImageAsync();
            _flowProducts.Controls.Add(pItem);
        }

        _flowProducts.ResumeLayout();
        _currentPage++;
        _isLoading = false;
    }

    private void CheckScrollBottom()
    {
        if (_flowProducts.VerticalScroll.Value + _flowProducts.ClientSize.Height >= _flowProducts.VerticalScroll.Maximum - 100)
        {
            LoadNextPage();
        }
    }
}
