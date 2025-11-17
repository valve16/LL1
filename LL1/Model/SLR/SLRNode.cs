using System.Diagnostics.CodeAnalysis;

namespace Model.SLR;

public struct NodePos
{
    public int Row;
    public int Num;

    public NodePos(int row, int num) : this()
    {
        Row = row;
        Num = num;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is NodePos pos)
        {
            return Row == pos.Row && Num == pos.Num;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return (Row, Num).GetHashCode();
    }
}

public struct SLRNode
{
    public string Symbol;
    public List<NodePos> Positions = [];
    public bool IsR;
    public bool IsOK;

    public SLRNode()
    {
        IsOK = true;
    }
    public SLRNode(string symbol, int row)
    {
        Symbol = symbol;
        Positions = [new(row, 0)];
        IsR = true;
        IsOK = false;
    }
    public SLRNode(string symbol, List<NodePos> positions) : this()
    {
        Symbol = symbol;
        Positions = positions;
        IsR = false;
        IsOK = false;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
        if (obj is SLRNode node)
        {
            return Symbol == node.Symbol && Positions.SequenceEqual(node.Positions);
        }

        return false;
    }
    public override int GetHashCode()
    {
        int hash = 17;

        foreach (NodePos pos in Positions)
        {
            hash = hash * 23 + pos.GetHashCode();
        }

        return hash;
    }
}
