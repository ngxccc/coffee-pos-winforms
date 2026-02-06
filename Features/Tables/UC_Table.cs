using FontAwesome.Sharp;

namespace CoffeePOS.Features.Tables;

public enum TableStatus { Empty, Occupied, Reserved }

public class UC_Table : UserControl
{
    // Data Properties
    public int TableId { get; set; }
    public TableStatus Status { get; set; }
    public DateTime? StartTime { get; set; }

    // UI Components
    private readonly Label _lblTime;
    private readonly IconPictureBox iconTable;
    private readonly Label lblTableName;

    public UC_Table(int id, string name, TableStatus status)
    {
        TableId = id;
        Status = status;

        Size = new Size(120, 120);
        BackColor = GetColorByStatus(status);
        Margin = new Padding(10);
        Cursor = Cursors.Hand;

        _lblTime = new Label
        {
            Text = "",
            AutoSize = false,
            Size = new Size(60, 20),
            Dock = DockStyle.Top, // Tạm dock Top, hoặc dùng Anchor
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.Yellow,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 5, 0)
        };

        iconTable = new IconPictureBox
        {
            IconChar = IconChar.Table,
            IconSize = 50,
            IconColor = Color.White,
            Dock = DockStyle.Fill,
            Height = 80,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.CenterImage
        };
        iconTable.Click += (s, e) => OnClick(e);

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

        Controls.Add(_lblTime);
        Controls.Add(iconTable);
        Controls.Add(lblTableName);

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

    public void UpdateDuration()
    {
        if (Status == TableStatus.Empty || StartTime == null)
        {
            _lblTime.Text = "";
            return;
        }

        // Tính khoảng thời gian chênh lệch
        TimeSpan duration = DateTime.Now - StartTime.Value;

        // Format đẹp: "1h 5p" hoặc "45p"
        if (duration.TotalMinutes < 60)
        {
            _lblTime.Text = $"{duration.TotalMinutes:0}p";
        }
        else
        {
            _lblTime.Text = $"{duration.Hours}h {duration.Minutes}p";
        }

        // Cảnh báo: Nếu ngồi quá 2 tiếng (120p) -> Đổi màu chữ sang Cam để nhắc nhân viên
        if (duration.TotalMinutes > 120) _lblTime.ForeColor = Color.OrangeRed;
        else _lblTime.ForeColor = Color.Yellow;
    }
}
