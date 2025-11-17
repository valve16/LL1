namespace Model.SLR;

public class SLRTable
{
    Dictionary<SLRNode, Dictionary<string, SLRNode>> _nodes = [];
    List<int> _rowToCount = [];

    public IReadOnlyDictionary<SLRNode, Dictionary<string, SLRNode>> Nodes => _nodes;
    public IReadOnlyList<int> RowToCount => _rowToCount;

    public void AddNode(SLRNode node, Dictionary<string, SLRNode> transactions)
    {
        _nodes[node] = transactions;

        List<string> symbols = transactions.Keys.ToList();
    }
    public void SetRowToCount(List<int> rowToCount)
    {
        _rowToCount = rowToCount;
    }
}
