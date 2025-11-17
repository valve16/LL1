using Model.Grammer;

namespace ConverterToCorrectGrammar;

public static class ConverterToCerrectGrammerLL
{
    public static Grammer ConvertToCorrectLL(this Grammer grammer)
    {
        CheckIsGrammerProductiveAndReachable(grammer);
        Grammer newGrammer = CheckAndRemoveLeftRecursion(grammer);

        return ChackAndRemoveDuplicateRules(newGrammer);
    }

    private static Grammer CheckAndRemoveLeftRecursion(Grammer grammer)
    {
        Grammer newGrammer = new();

        foreach (KeyValuePair<string, List<List<string>>> rule in grammer.Rules)
        {
            List<int> leftRecursion = FindLeftRecursion(rule.Key, rule.Value);

            if (leftRecursion.Count != 0)
            {
                RemoveLeftRecursion(newGrammer, rule.Key, rule.Value.CreateNewList(), leftRecursion);
            }
            else
            {
                newGrammer.AddRule(rule.Key, rule.Value.CreateNewList());
            }
        }

        return newGrammer;
    }

    internal static void CheckIsGrammerProductiveAndReachable(Grammer grammer)
    {
        bool isReachable = false;
        List<string> keys = grammer.Rules.Keys.ToList();
        keys.RemoveAt(0);

        foreach (KeyValuePair<string, List<List<string>>> rule in grammer.Rules)
        {
            for (int i = 0; i < rule.Value.Count; i++)
            {
                bool isEnd = true;
                foreach (string symbol in rule.Value[i])
                {
                    if (grammer.Rules.ContainsKey(symbol))
                    {
                        isEnd = false;
                        keys.Remove(symbol);
                    }
                }

                if (isEnd)
                {
                    isReachable = true;
                }
            }
        }

        if (!isReachable)
        {
            throw new Exception("Grammer not reachable");
        }
        if (keys.Count > 0)
        {
            throw new Exception("Grammer not productive");
        }
    }

    // Left recursion
    private static List<int> FindLeftRecursion(
        string key,
        List<List<string>> rules)
    {
        List<int> list = [];

        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i].Count == 0)
            {
                continue;
            }

            if (rules[i][0] == key)
            {
                list.Add(i);
            }
        }

        return list;
    }
    private static void RemoveLeftRecursion(
        Grammer grammer,
        string key,
        List<List<string>> rules,
        List<int> leftRecursion)
    {
        string newKey = key + "_left";

        List<List<string>> rulesForNew = [];
        foreach (int ruleNum in leftRecursion)
        {
            List<string> list = [];

            for (int i = 1; i < rules[ruleNum].Count; i++)
            {
                list.Add(rules[ruleNum][i]);
            }
            list.Add(newKey);

            rulesForNew.Add(list);
        }
        rulesForNew.Add(["@"]);

        rules.RemoveAll(r => r[0] == key);

        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i][0] == "@")
            {
                rules[i] = [newKey];
            }
            else
            {
                rules[i].Add(newKey);
            }
        }

        grammer.AddRule(key, rules);
        grammer.AddRule(newKey, rulesForNew);
    }

    internal static List<List<string>> CreateNewList(this List<List<string>> list)
    {
        List<List<string>> newList = list.Select(l => new List<string>(l)).ToList();

        return newList;
    }

    private static Grammer ChackAndRemoveDuplicateRules(Grammer grammer)
    {
        Grammer newGrammer = new();

        foreach (KeyValuePair<string, List<List<string>>> rule in grammer.Rules)
        {
            Dictionary<string, List<int>> duplicateRules = FindDuplicateRules(rule.Value);

            if (duplicateRules.Count != 0)
            {
                RemoveDuplicateRules(newGrammer, rule.Key, rule.Value.CreateNewList(), duplicateRules);
            }
            else
            {
                newGrammer.AddRule(rule.Key, rule.Value.CreateNewList());
            }
        }

        return newGrammer;
    }
    private static Dictionary<string, List<int>> FindDuplicateRules(
        List<List<string>> rules)
    {
        Dictionary<string, List<int>> duplicates = [];

        for (int i = 0; i < rules.Count; i++)
        {
            string beginStr = rules[i][0];
            if (!duplicates.ContainsKey(beginStr))
            {
                duplicates[beginStr] = [];
            }

            duplicates[beginStr].Add(i);
        }

        return duplicates.Where(i => i.Value.Count > 1).ToDictionary();
    }
    private static void RemoveDuplicateRules(
        Grammer grammer,
        string key,
        List<List<string>> rules,
        Dictionary<string, List<int>> duplicateRules)
    {

        Dictionary<string, List<List<string>>> newSymbols = [];
        List<List<string>> newRules = [];

        foreach (KeyValuePair<string, List<int>> item in duplicateRules)
        {
            string newKey = $"{key}_{item.Key}";
            List<List<string>> rulesForNew = [];

            foreach (int ruleNum in item.Value)
            {
                List<string> list = [];

                if (rules[ruleNum].Count == 1)
                {
                    list.Add("@");
                }
                else
                {
                    for (int i = 1; i < rules[ruleNum].Count; i++)
                    {
                        list.Add(rules[ruleNum][i]);
                    }
                }

                rulesForNew.Add(list);
            }

            newSymbols[newKey] = rulesForNew;
            newRules.Add([item.Key, newKey]);
        }

        rules.RemoveAll(r => duplicateRules.ContainsKey(r[0]));

        for (int i = 0; i < rules.Count; i++)
        {
            newRules.Add(rules[i]);
        }

        grammer.AddRule(key, newRules);

        foreach (KeyValuePair<string, List<List<string>>> item in newSymbols)
        {
            grammer.AddRule(item.Key, item.Value);
        }
    }
}
