namespace CoffeePOS.Shared.Helpers;

public sealed class StatefulSortableGrid<T>(DataGridView grid, string idColumnName = "Id")
{
    private readonly DataGridView _grid = grid;
    private readonly string _idColumnName = idColumnName;

    private string? _sortColumnName;
    private bool _sortAscending = true;
    private int _savedScrollPosition = -1;
    private int _savedSelectedRowId = -1;

    public event Action? SortChanged;

    public void AttachSortHandler()
    {
        _grid.ColumnHeaderMouseClick += OnColumnHeaderMouseClick;
    }

    public void CapturePosition()
    {
        _grid.SavePosition(ref _savedScrollPosition, ref _savedSelectedRowId, _idColumnName);
    }

    public void RestorePosition()
    {
        _grid.RestorePosition(_savedScrollPosition, _savedSelectedRowId, _idColumnName);
    }

    public void Bind(IEnumerable<T> rows)
    {
        _grid.DataSource = rows.DynamicSort(_sortColumnName, _sortAscending).ToList();
        _grid.UpdateSortGlyphs(_sortColumnName, _sortAscending);
    }

    private void OnColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        _grid.ToggleSortState(e.ColumnIndex, ref _sortColumnName, ref _sortAscending);
        SortChanged?.Invoke();
    }
}
