using Model.Grammer;

namespace ConverterToCorrectGrammar;

public static class ConverterToCorrectGrammerSLR
{
    public static Grammer ConvertToCorrectSLR(this Grammer grammer)
    {
        ConverterToCerrectGrammerLL.CheckIsGrammerProductiveAndReachable(grammer);
        Grammer newGrammer = RemoveEndSymbol(grammer);
        newGrammer = RemoveEmptyTransaction(newGrammer);

        if (newGrammer.Rules.First().Value.Count == 2 && newGrammer.Rules.First().Value[1].SequenceEqual(["@"]))
        {
            newGrammer.Rules.First().Value[1] = ["#"];
        }
        else
        {
            if (newGrammer.Rules.First().Value.Count > 1)
            {
                newGrammer = AddNewBeginRule(newGrammer);

                if (newGrammer[newGrammer.GetKeys()[1]].Any(l => l.SequenceEqual(["@"])))
                {
                    newGrammer.Rules.First().Value.Add(["#"]);
                    newGrammer[newGrammer.GetKeys()[1]].RemoveAll(l => l.SequenceEqual(["@"]));
                }
            }
            else
            {
                newGrammer.Rules.First().Value[0].Add("#");
            }
        }

        return newGrammer;
    }
    private static Grammer RemoveEndSymbol(Grammer grammer)
    {
        Dictionary<string, List<List<string>>> dictionary = grammer.Rules.CreateNewDictionary();
        foreach (List<string> list in dictionary.First().Value)
        {
            list.Remove("#");
        }

        Grammer newGrammer = new();
        foreach (KeyValuePair<string, List<List<string>>> item in dictionary)
        {
            newGrammer.AddRule(item.Key, item.Value);
        }
        return newGrammer;
    }
    private static Grammer AddNewBeginRule(Grammer grammer)
    {
        string firstKey = grammer.Rules.Keys.First();
        string key = $"0_{firstKey}";

        Grammer newGrammer = new();
        newGrammer.AddRule(key, [[firstKey, "#"]]);

        foreach (KeyValuePair<string, List<List<string>>> item in grammer.Rules)
        {
            newGrammer.AddRule(item.Key, item.Value);
        }

        return newGrammer;
    }
    private static Grammer RemoveEmptyTransaction(Grammer grammer)
    {
        List<string> emptyTerminals = FindTerminalWithEmptyTransaction(grammer.Rules);
        Dictionary<string, List<List<string>>> rules = grammer.Rules.CreateNewDictionary();

        while (emptyTerminals.Count > 0)
        {
            foreach (string terminal in emptyTerminals)
            {
                if (terminal == rules.First().Key)
                {
                    continue;
                }
                foreach (KeyValuePair<string, List<List<string>>> rule in rules)
                {
                    for (int i = 0; i < rule.Value.Count; i++)
                    {
                        if (rule.Value[i].Contains(terminal))
                        {
                            List<string> list = new(rule.Value[i]);

                            list.Remove(terminal);
                            if (list.Count == 0)
                            {
                                list = ["@"];
                            }
                            if (!rule.Value.Any(l => l.SequenceEqual(list)))
                            {
                                rule.Value.Add(list);
                            }
                        }
                    }
                }
                RemoveEmptySymbols(rules, terminal);
            }

            emptyTerminals = FindTerminalWithEmptyTransaction(rules);
            emptyTerminals.RemoveAll(t => t == rules.First().Key);
        }

        Grammer newGrammer = new();
        foreach (KeyValuePair<string, List<List<string>>> item in rules)
        {
            newGrammer.AddRule(item.Key, item.Value);
        }

        return newGrammer;
    }
    private static void RemoveEmptySymbols(this Dictionary<string, List<List<string>>> dictionary, string terminal)
    {
        dictionary[terminal].RemoveAll(r => r.Contains("@"));
    }
    private static List<string> FindTerminalWithEmptyTransaction(
        IReadOnlyDictionary<string, List<List<string>>> rules)
    {
        List<string> result = [];

        foreach (KeyValuePair<string, List<List<string>>> rule in rules)
        {
            for (int i = 0; i < rule.Value.Count; i++)
            {
                if (rule.Value[i].Contains("@"))
                {
                    result.Add(rule.Key);
                    continue;
                }
            }
        }

        return result;
    }
    private static Dictionary<string, List<List<string>>> CreateNewDictionary(
        this IReadOnlyDictionary<string, List<List<string>>> dictionary)
    {
        Dictionary<string, List<List<string>>> newDictinary = [];

        foreach (KeyValuePair<string, List<List<string>>> item in dictionary)
        {
            newDictinary.Add(item.Key, item.Value.CreateNewList());
        }

        return newDictinary;
    }
}
