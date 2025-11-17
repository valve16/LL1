namespace Model.Grammer;

public class Grammer
{
    Dictionary<string, List<List<string>>> _rules = new();

    public List<List<string>> this[string rule] // Позволяет получать элемент grammer["S"]
    {
        get => _rules[rule];
    }

    public IReadOnlyDictionary<string, List<List<string>>> Rules => _rules;

    public List<string> GetKeys()
    {
        return _rules.Keys.ToList();
    }

    public void Clear()
    {
        _rules.Clear();
    }

    public void AddRule(string terminal, List<List<string>> rules)
    {
        if (_rules.ContainsKey(terminal))
        {
            return;
        }

        _rules[terminal] = rules;
    }
}
