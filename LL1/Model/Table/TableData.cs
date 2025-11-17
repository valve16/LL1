namespace Model.Table;

public class TableData
{
    public int Num { get; private init; }
    public string Name { get; private init; }
    public List<string> GuidingSet { get; private init; }
    public int? Transition { get; private init; }
    public bool Error { get; private init; }
    public bool Shift { get; private init; }
    public int? Stack { get; private init; }
    public bool End { get; private init; }

    public TableData(
            int num,
            string name,
            List<string> guidingSet,
            int? transition,
            bool error,
            bool shift,
            int? stack,
            bool end
        )
    {
        Num = num;
        Name = name;
        GuidingSet = guidingSet;
        Transition = transition;
        Error = error;
        Shift = shift;
        Stack = stack;
        End = end;
    }

    public static TableData Create(
            int num,
            string name,
            List<string> guidingSet,
            int? transition,
            bool error,
            bool shift,
            int? stack,
            bool end)
    {
        return new(num, name, guidingSet, transition, error, shift, stack, end);
    }
}
