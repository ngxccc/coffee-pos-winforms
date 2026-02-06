using FontAwesome.Sharp;

namespace CoffeePOS.Features.Products;

public class UC_ProductItem : UserControl
{
    // UI Components
    private readonly IconPictureBox _iconFood;
    private readonly Label _lblName;
    private readonly Label _lblPrice;

    // Data
    public int ProductId { get; private set; }
    public new string ProductName { get; private set; }
    public decimal Price { get; private set; }

    public event EventHandler? OnProductClicked;

    public UC_ProductItem(int id, string name, decimal price)
    {
        ProductId = id;
        ProductName = name;
        Price = price;

        Size = new Size(130, 180);
        BackColor = Color.White;
        Cursor = Cursors.Hand;
        BorderStyle = BorderStyle.FixedSingle;

        _iconFood = BuildFoodIcon();
        _lblName = BuildNameLabel(name);
        _lblPrice = BuildPriceLabel(price);

        Controls.Add(_lblName);
        Controls.Add(_iconFood);
        Controls.Add(_lblPrice);

        BindClickRecursive(this);
    }

    private static Label BuildNameLabel(string name)
    {
        return new()
        {
            Text = name,
            Dock = DockStyle.Top,
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(5)
        };
    }

    private static Label BuildPriceLabel(decimal price)
    {
        return new()
        {
            Text = $"{price:N0} đ",
            Dock = DockStyle.Bottom,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(231, 76, 60)
        };
    }

    private static IconPictureBox BuildFoodIcon()
    {
        return new IconPictureBox
        {
            IconChar = IconChar.Spinner,
            IconSize = 40,
            IconColor = Color.Gray,
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = Color.FromArgb(245, 245, 245),
            SizeMode = PictureBoxSizeMode.CenterImage
        };
    }

    public async void LoadImageAsync(int colorSeed)
    {
        // Chạy task ngầm để không đơ UI
        await Task.Run(async () =>
        {
            // 1. Giả lập độ trễ mạng (Network Delay) từ 100ms - 500ms
            // Nếu không có dòng này, code chạy nhanh quá ông sẽ không thấy hiệu ứng loading
            await Task.Delay(new Random().Next(100, 500));

            // 2. Tạo ảnh thật (Trong thực tế đoạn này là tải từ URL hoặc Disk)
            Bitmap realImage = new(100, 100);
            using (Graphics g = Graphics.FromImage(realImage))
            {
                // Random màu nền dựa trên ID món
                Random rnd = new(colorSeed);
                Color randomColor = Color.FromArgb(rnd.Next(200, 255), rnd.Next(200, 255), rnd.Next(200, 255));

                g.Clear(randomColor);

                // Vẽ chữ cái đầu
                g.DrawString(ProductName[..1],
                    new Font("Arial", 30, FontStyle.Bold),
                    Brushes.DimGray,
                    35, 25);
            }

            // 3. Cập nhật UI (Bắt buộc phải Invoke vì đang ở thread khác)
            if (!IsDisposed && IsHandleCreated)
            {
                Invoke(() =>
                {
                    _iconFood.IconChar = IconChar.None; // Tắt icon loading
                    _iconFood.Image = realImage;        // Gắn ảnh thật
                    _iconFood.SizeMode = PictureBoxSizeMode.StretchImage; // Full ảnh
                });
            }
        });
    }

    private void BindClickRecursive(Control ctrl)
    {
        ctrl.Click += (s, e) => OnProductClicked?.Invoke(this, EventArgs.Empty);
        foreach (Control child in ctrl.Controls) BindClickRecursive(child);
    }
}
