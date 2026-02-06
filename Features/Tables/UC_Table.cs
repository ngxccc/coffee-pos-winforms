using FontAwesome.Sharp;

namespace CoffeePOS.Features.Tables;

public enum TableStatus { Empty, Occupied, Reserved }

public class UC_Table : UserControl
{
    // Data Properties
    public int TableId { get; set; }
    public TableStatus Status { get; set; }

    // UI Components
    private readonly IconPictureBox iconTable;
    private readonly Label lblTableName;

    public UC_Table(int id, string name, TableStatus status)
    {
        TableId = id;
        Status = status;

        // 1. Setup Container (Cái thẻ bàn)
        Size = new Size(120, 120);
        BackColor = GetColorByStatus(status);
        Margin = new Padding(10); // Cách nhau ra
        Cursor = Cursors.Hand; // Hover vào hiện tay

        // 2. Icon Bàn (FontAwesome)
        iconTable = new IconPictureBox
        {
            IconChar = IconChar.Table,
            IconSize = 50,
            IconColor = Color.White,
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        // Quan trọng: Click vào icon thì cũng tính là click vào bàn
        iconTable.Click += (s, e) => OnClick(e);

        // 3. Tên bàn
        lblTableName = new Label
        {
            Text = name,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Bottom,
            Height = 40,
            TextAlign = ContentAlignment.TopCenter
        };
        lblTableName.Click += (s, e) => OnClick(e);

        Controls.Add(lblTableName);
        Controls.Add(iconTable);

        UpdateColor();
    }

    private static Color GetColorByStatus(TableStatus status)
    {
        return status switch
        {
            TableStatus.Empty => Color.FromArgb(46, 204, 113),   // Xanh lá (Trống)
            TableStatus.Occupied => Color.FromArgb(231, 76, 60), // Đỏ (Có khách)
            _ => Color.Gray
        };
    }

    public void UpdateColor()
    {
        BackColor = GetColorByStatus(Status);
    }
}
