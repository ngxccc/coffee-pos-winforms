using CoffeePOS.Models;

namespace CoffeePOS.Features.Products;

public class UC_Menu : UserControl
{
    // UI Components
    private FlowLayoutPanel _flowProducts = null!;
    private FlowLayoutPanel _flowCategories = null!;

    // Data Cache
    private readonly List<Product> _allProducts = [];
    private List<Category> _allCategories = [];

    // Events
    public event Action<int, string, decimal>? OnProductSelected;
    public event EventHandler? OnBackClicked;

    public UC_Menu()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(245, 245, 245);

        InitializeComponents();

        LoadDataFromDatabase();

        RenderCategories();
        RenderProducts(_allProducts);
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
            Padding = new Padding(20),
            BackColor = Color.FromArgb(245, 245, 245),
        };

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
        _allCategories =
        [
            new Category { Id = 0, Name = "Tất cả" },
            new Category { Id = 1, Name = "Cà phê" },
            new Category { Id = 2, Name = "Trà trái cây" },
            new Category { Id = 3, Name = "Đá xay" },
            new Category { Id = 4, Name = "Bánh ngọt" }
        ];

        _allProducts.Clear();
        // Cafe (CatId 1)
        for (int i = 1; i <= 5; i++) _allProducts.Add(new Product { Id = i, Name = $"Cafe {i}", Price = 25000, CategoryId = 1 });
        // Trà (CatId 2)
        for (int i = 6; i <= 10; i++) _allProducts.Add(new Product { Id = i, Name = $"Trà {i}", Price = 30000, CategoryId = 2 });
        // Bánh (CatId 4)
        for (int i = 11; i <= 15; i++) _allProducts.Add(new Product { Id = i, Name = $"Bánh {i}", Price = 15000, CategoryId = 4 });
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
        List<Product> filteredList;

        if (categoryId == 0)
        {
            filteredList = _allProducts;
        }
        else
        {
            filteredList = _allProducts.Where(p => p.CategoryId == categoryId).ToList();
        }

        RenderProducts(filteredList);
    }

    private void RenderProducts(List<Product> products)
    {
        _flowProducts.SuspendLayout();
        _flowProducts.Controls.Clear();

        foreach (var p in products)
        {
            var pItem = new UC_ProductItem(p.Id, p.Name, p.Price);

            pItem.OnProductClicked += (s, e) =>
            {
                OnProductSelected?.Invoke(p.Id, p.Name, p.Price);
            };

            _flowProducts.Controls.Add(pItem);
        }

        _flowProducts.ResumeLayout();
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
