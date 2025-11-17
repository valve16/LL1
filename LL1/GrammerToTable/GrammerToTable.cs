using Interfaces;
using Model.Grammer;
using Model.Table;

namespace GrammerToTable;

public class GrammerToTable : IGramerToTable
{
    const string EMPTY_TRANSATION = "@";

    private readonly ISearcherGuidingSet _searcherGuidingSet;

    public GrammerToTable(ISearcherGuidingSet searcherGuidingSet)
    {
        _searcherGuidingSet = searcherGuidingSet;
    }

    public Table Convert(Grammer grammer)
    {
        (Dictionary<int, List<int>> rules, List<string> nameOfRules) = SetNumForSymbols(grammer);
        List<List<string>> guidingSets = _searcherGuidingSet.Search(rules, nameOfRules);

        return CreateTable(rules, nameOfRules, guidingSets);
    }

    private (Dictionary<int, List<int>> rules, List<string> nameOfRules) SetNumForSymbols(Grammer grammer)
    {
        List<string> keys = grammer.GetKeys();
        List<string> nameOfRules = [];
        Dictionary<int, List<int>> rules = [];
        Dictionary<int, List<string>> newGrammer = [];

        foreach (string key in keys)
        {
            List<List<string>> transitions = grammer[key];

            for (int i = 0; i < transitions.Count; i++)
            {
                nameOfRules.Add(key);
                newGrammer[nameOfRules.Count - 1] = transitions[i];
            }
        }

        foreach (KeyValuePair<int, List<string>> rule in newGrammer)
        {
            rules[rule.Key] = [];
            foreach (string value in rule.Value)
            {
                nameOfRules.Add(value);
                rules[rule.Key].Add(nameOfRules.Count - 1);
            }
        }

        return (rules, nameOfRules);
    }

    private Table CreateTable(Dictionary<int, List<int>> rules, List<string> names, List<List<string>> guidingSets)
    {
        Table table = new();

        for (int i = 0; i < names.Count; i++)
        {
            int num = i;
            string name = names[i];
            List<string> guidingSet = guidingSets[i];
            (int? transition, int? stack) = FindTransitionAndStack(i, rules, names);
            bool error = IsError(i, rules, names);
            bool shift = IsShift(i, rules, names);
            bool end = name == "#";

            table.AddRecord(TableData.Create(
                    num,
                    name,
                    guidingSet,
                    transition,
                    error,
                    shift,
                    stack,
                    end
                ));
        }

        return table;
    }
    private (int? transition, int? stack) FindTransitionAndStack(int num, Dictionary<int, List<int>> rules, List<string> names)
    {
        int? transition;
        int? stack;

        if (rules.ContainsKey(num))
        {
            return (rules[num][0], null);
        }
        else
        {
            List<int> currentRule = rules.FirstOrDefault(r => r.Value.Contains(num)).Value;
            bool isLastInRule = currentRule.IndexOf(num) == currentRule.Count - 1;

            int? nextRuleNum = FindRuleWithEqualName(names[num], rules, names);

            if (nextRuleNum == null)
            {
                stack = null;
                if (isLastInRule)
                {
                    transition = null;
                }
                else
                {
                    transition = num + 1;
                }
            }
            else
            {
                transition = nextRuleNum;

                if (isLastInRule)
                {
                    stack = null;
                }
                else
                {
                    stack = num + 1;
                }
            }
        }

        return (transition, stack);
    }
    private bool IsError(int num, Dictionary<int, List<int>> rules, List<string> names)
    {
        int next = num + 1;
        if (!rules.ContainsKey(num) || !rules.ContainsKey(next))
        {
            return true;
        }

        return names[num] != names[next];
    }
    private bool IsShift(int num, Dictionary<int, List<int>> rules, List<string> names)
    {
        if (names[num] == EMPTY_TRANSATION)
        {
            return false;
        }

        int? rule = FindRuleWithEqualName(names[num], rules, names);

        return rule == null;
    }
    private int? FindRuleWithEqualName(string name, Dictionary<int, List<int>> rules, List<string> names)
    {
        List<int> nums = names.Select((name, index) => new { name, index })
                              .Where(p => p.name == name)
                              .Select(p => p.index)
                              .ToList();
        nums.Sort();

        for (int i = 0; i < nums.Count; i++)
        {
            if (rules.ContainsKey(nums[i]))
            {
                return nums[i];
            }
        }

        return null;
    }
}
