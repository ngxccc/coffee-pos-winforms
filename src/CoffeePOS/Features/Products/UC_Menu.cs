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

    private int _currentCatId = 0;
    private bool _isChangingCategory = false;

    public event Action<int, string, decimal, string>? OnProductSelected;

    public UC_Menu(IProductQueryService productQueryService, ICategoryQueryService categoryQueryService)
    {
        _productQueryService = productQueryService;
        _categoryQueryService = categoryQueryService;

        InitializeComponent();
        WireEvents();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadDataFromDatabaseAsync();
    }

    private void WireEvents()
    {
        _txtSearch.TextChanged += TxtSearch_TextChanged;

        _menuCategories.SelectChanged += (s, e) =>
        {
            if (e.Value.Tag is int categoryId)
            {
                _isChangingCategory = true;
                _currentCatId = categoryId;

                if (!string.IsNullOrEmpty(_txtSearch.Text))
                {
                    _txtSearch.Text = "";
                }

                FilterProducts(categoryId);
                _isChangingCategory = false;
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

        ClearProductFlow();
        RenderProducts();
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        if (_isChangingCategory) return;

        string keyword = _txtSearch.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(keyword))
        {
            FilterProducts(_currentCatId);
            return;
        }

        _currentFilteredList = [.. _allProducts.Where(p => p.Name.Contains(keyword, StringComparison.CurrentCultureIgnoreCase) || p.Price.ToString().Contains(keyword))];

        ClearProductFlow();
        RenderProducts();
    }

    private void RenderProducts()
    {
        if (_currentFilteredList.Count == 0) return;

        var itemsToLoad = new List<UC_ProductItem>(_currentFilteredList.Count);

        foreach (var p in _currentFilteredList)
        {
            var pItem = new UC_ProductItem(p.Id, p.Name, p.Price, p.ImageUrl);
            pItem.OnProductClicked += (s, e) =>
                OnProductSelected?.Invoke(p.Id, p.Name, p.Price, p.ImageUrl);

            itemsToLoad.Add(pItem);
        }

        _flowProducts.Visible = false;

        _flowProducts.Controls.AddRange([.. itemsToLoad]);

        _flowProducts.Visible = true;

        foreach (var pItem in itemsToLoad)
        {
            _ = pItem.LoadImageAsync();
        }
    }

    private void ClearProductFlow()
    {
        _flowProducts.SuspendLayout();

        var controlsToDispose = _flowProducts.Controls.Cast<Control>().ToArray();

        _flowProducts.Visible = false;

        _flowProducts.Controls.Clear();

        _flowProducts.Visible = true;

        foreach (var c in controlsToDispose)
        {
            c.Dispose();
        }

        _flowProducts.ResumeLayout(true);
        PerformLayout();
        _flowProducts.Refresh();
    }
}
