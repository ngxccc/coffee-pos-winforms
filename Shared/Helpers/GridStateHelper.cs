namespace CoffeePOS.Shared.Helpers;

public static class GridStateHelper
{
    public static void SavePosition(this DataGridView dgv, ref int scrollPos, ref int selectedId, string idColName = "Id")
    {
        if (dgv.Rows.Count == 0) return;
        scrollPos = dgv.FirstDisplayedScrollingRowIndex;
        selectedId = dgv.SelectedRows.Count > 0 ? (int)dgv.SelectedRows[0].Cells[idColName].Value : -1;
    }

    public static void RestorePosition(this DataGridView dgv, int scrollPos, int selectedId, string idColName = "Id")
    {
        if (dgv.Rows.Count == 0) return;
        try
        {
            dgv.ClearSelection();
            if (selectedId > 0)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if ((int)row.Cells[idColName].Value == selectedId)
                    {
                        row.Selected = true; break;
                    }
                }
            }
            if (scrollPos >= 0)
            {
                dgv.FirstDisplayedScrollingRowIndex = Math.Min(scrollPos, dgv.Rows.Count - 1);
            }
        }
        catch { }
    }
}
