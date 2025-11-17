namespace Model.Table;

public class Table
{
    List<TableData> _records = new();

    public IReadOnlyList<TableData> Records { get => _records; }

    public TableData this[int i] => _records[i];

    public void AddRecord(
            string name,
            List<string> guidingSet,
            int? transition,
            bool error,
            bool shift,
            int? stack,
            bool end)
    {
        AddRecord(TableData.Create(_records.Count, name, guidingSet, transition, error, shift, stack, end));
    }
    public void AddRecord(TableData data)
    {
        _records.Add(data);
    }
}
