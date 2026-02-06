using CoffeePOS.Core;
using FontAwesome.Sharp;

namespace CoffeePOS.Features.Tables;

public enum TableStatus { Empty, Occupied, Reserved }

public class UC_Table : UserControl
{
    // UI COMPONENTS
    private readonly Label _lblTime;
    private readonly IconPictureBox _iconTable;
    private readonly Label _lblTableName;

    // DATA PROPERTIES
    public int TableId { get; set; }
    public TableStatus Status { get; set; }
    public DateTime? StartTime { get; set; }

    // CONSTRUCTOR
    public UC_Table(int id, string name, TableStatus status)
    {
        TableId = id;
        Status = status;

        SetupMainContainer();

        _lblTime = BuildTimeLabel();
        _lblTableName = BuildNameLabel(name);
        _iconTable = BuildTableIcon();

        // Add cái Fill trước, Dock=Top/Bottom sau cùng
        Controls.Add(_iconTable);      // Fill (Nằm giữa)
        Controls.Add(_lblTime);        // Top
        Controls.Add(_lblTableName);   // Bottom

        UpdateColor();

        BindClickEvent(this);
    }

    // HELPER METHODS (UI)

    private void SetupMainContainer()
    {
        Size = new Size(120, 120);
        Margin = new Padding(10);
        Cursor = Cursors.Hand;
    }

    private static Label BuildTimeLabel()
    {
        return new Label
        {
            Text = "",
            AutoSize = false,
            Height = 20,
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.Yellow,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 5, 0)
        };
    }

    private static Label BuildNameLabel(string name)
    {
        return new Label
        {
            Text = name,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            Dock = DockStyle.Bottom,
            Height = 40,
            TextAlign = ContentAlignment.TopCenter,
            BackColor = Color.Transparent
        };
    }

    private static IconPictureBox BuildTableIcon()
    {
        return new IconPictureBox
        {
            IconChar = IconChar.Table,
            IconSize = 50,
            IconColor = Color.White,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            SizeMode = PictureBoxSizeMode.CenterImage
        };
    }

    // LOGIC METHODS

    private void BindClickEvent(Control control)
    {
        if (control != this)
        {
            // Khi click vào label/icon -> Kích hoạt sự kiện Click của UC_Table
            control.Click += (s, e) => OnClick(e);
        }

        foreach (Control child in control.Controls)
        {
            child.Click += (s, e) => OnClick(e);
        }
    }

    private static Color GetColorByStatus(TableStatus status)
    {
        return status switch
        {
            TableStatus.Empty => Color.FromArgb(46, 204, 113),   // Green
            TableStatus.Occupied => Color.FromArgb(231, 76, 60), // Red
            TableStatus.Reserved => Color.FromArgb(241, 196, 15),// Yellow
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
            if (_lblTime.Text != "") _lblTime.Text = "";
            return;
        }

        TimeSpan duration = TimeKeeper.Now - StartTime.Value;

        // Cập nhật text
        string newText = duration.TotalMinutes < 60
            ? $"{duration.TotalMinutes:0}p"
            : $"{duration.Hours}h {duration.Minutes}p";

        if (_lblTime.Text != newText)
            _lblTime.Text = newText;

        Color targetColor = duration.TotalMinutes > 120 ? Color.OrangeRed : Color.Yellow;
        if (_lblTime.ForeColor != targetColor)
            _lblTime.ForeColor = targetColor;
    }
}
