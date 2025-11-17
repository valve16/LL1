using Interfaces;

namespace SearcherGuidingSet;

public class SearcherGuidingSet : ISearcherGuidingSet
{
    const string EMPTY_TRANSATION = "@";

    public List<List<string>> Search(Dictionary<int, List<int>> rules, List<string> nameOfRules)
    {
        Dictionary<int, List<string>> sets = new();
        List<int> emptyRules = new();
        Stack<int> stack = new();

        for (int i = 0; i < nameOfRules.Count; i++)
        {
            stack.Push(i);

            while (stack.TryPeek(out int value))
            {
                if (sets.ContainsKey(value))
                {
                    stack.Pop();
                    continue;
                }

                if (nameOfRules[value] == EMPTY_TRANSATION)
                {
                    HandleEmptySymbol(value, rules, sets, stack);
                    continue;
                }

                if (IsValueTerminal(value, rules, nameOfRules))
                {
                    sets[value] = [nameOfRules[value]];
                    stack.Pop();
                }
                else
                {
                    if (rules.ContainsKey(value))
                    {
                        HandleRule(value, rules[value][0], stack, sets, rules, nameOfRules, emptyRules);
                    }
                    else
                    {
                        HandleNotTerminal(value, stack, rules, nameOfRules, emptyRules, sets);
                    }
                }
            }
        }

        return DictionartToList(sets, nameOfRules.Count);
    }
    private void HandleEmptySymbol(int value, Dictionary<int, List<int>> rules, Dictionary<int, List<string>> sets, Stack<int> stack)
    {
        KeyValuePair<int, List<int>> rule = rules.FirstOrDefault(p => p.Value.Contains(value));

        if (sets.ContainsKey(rule.Key))
        {
            sets[value] = sets[rule.Key];
            stack.Pop();
        }
        else
        {
            stack.Push(rule.Key);
        }
    }
    private void HandleRule(
        int value,
        int next,
        Stack<int> stack,
        Dictionary<int, List<string>> sets,
        Dictionary<int, List<int>> rules,
        List<string> nameOfRules,
        List<int> emptyRules
    )
    {
        if (nameOfRules[next] == EMPTY_TRANSATION)
        {
            HanldeEmptyRule(value, stack, rules, nameOfRules, emptyRules, sets);
            emptyRules.Add(value);
        }
        else if (sets.ContainsKey(next))
        {
            sets[value] = sets[next];
            stack.Pop();
        }
        else
        {
            stack.Push(next);
        }
    }
    private void HanldeEmptyRule(
        int value,
        Stack<int> stack,
        Dictionary<int, List<int>> rules,
        List<string> nameOfRules,
        List<int> emptyRules,
        Dictionary<int, List<string>> sets
    )
    {
        List<string> set = new();
        bool canAdd = true;

        Queue<int> queue = new();
        FindAllNumsOfValue(value, nameOfRules).ForEach(queue.Enqueue);

        List<int> used = new();

        while (queue.TryDequeue(out int result))
        {
            if (rules.ContainsKey(result))
            {
                continue;
            }
            KeyValuePair<int, List<int>> rule = rules.FirstOrDefault(p => p.Value.Contains(result));
            used.Add(result);

            int index = rule.Value.IndexOf(result);
            if (index < rule.Value.Count - 1)
            {
                int next = result + 1;

                if (sets.ContainsKey(next))
                {
                    set.AddRange(sets[next]);
                }
                else
                {
                    canAdd = false;
                    stack.Push(next);
                }
            }
            else
            {
                List<int> generalRools = FindAllNumsOfValue(rule.Key, nameOfRules);

                foreach (int i in generalRools)
                {
                    if (!rules.ContainsKey(i) && !used.Contains(i))
                    {
                        queue.Enqueue(i);
                    }
                }
            }
        }

        if (canAdd)
        {
            stack.Pop();

            sets[value] = set;
        }
    }
    private void HandleNotTerminal(
        int value,
        Stack<int> stack,
        Dictionary<int, List<int>> rules,
        List<string> nameOfRules,
        List<int> emptyRules,
        Dictionary<int, List<string>> sets
    )
    {
        List<int> list = FindAllNumsOfValue(value, nameOfRules);
        list.Sort();

        bool canAdd = true;

        List<string> newSet = new();
        foreach (int rule in list)
        {
            if (!rules.ContainsKey(rule))
            {
                continue;
            }

            if (emptyRules.Contains(rule))
            {
                List<string> nextSet = HandleNotTermainToEmptyRule(value, stack, rules, nameOfRules, sets);

                if (nextSet != null && nextSet.Count > 0)
                {
                    newSet.AddRange(nextSet);
                }
                else
                {
                    canAdd = false;
                }
            }
            else if (sets.ContainsKey(rule))
            {
                newSet.AddRange(sets[rule]);
            }
            else
            {
                stack.Push(rule);
                canAdd = false;
            }
        }

        if (canAdd)
        {
            sets[value] = newSet;
            stack.Pop();
        }
    }
    private List<string> HandleNotTermainToEmptyRule(
        int value,
        Stack<int> stack,
        Dictionary<int, List<int>> rules,
        List<string> nameOfRules,
        Dictionary<int, List<string>> sets
    )
    {
        List<string> set = new();
        bool canAdd = true;

        List<int> used = new();
        Queue<int> queue = new();
        queue.Enqueue(value);
        while (queue.TryDequeue(out int result))
        {
            KeyValuePair<int, List<int>> rule = rules.FirstOrDefault(p => p.Value.Contains(result));
            used.Add(result);

            int index = rule.Value.IndexOf(result);
            if (index < rule.Value.Count - 1)
            {
                int next = result + 1;

                if (sets.ContainsKey(next))
                {
                    set.AddRange(sets[next]);
                }
                else
                {
                    canAdd = false;
                    stack.Push(next);
                }
            }
            else
            {
                List<int> generalRools = FindAllNumsOfValue(rule.Key, nameOfRules);

                foreach (int i in generalRools)
                {
                    if (!rules.ContainsKey(i) && !used.Contains(i))
                    {
                        queue.Enqueue(i);
                    }
                }
            }
        }

        return canAdd ? set : null;
    }

    private bool IsValueTerminal(int value, Dictionary<int, List<int>> rules, List<string> nameOfRules)
    {
        if (rules.ContainsKey(value))
        {
            return false;
        }
        List<int> otherNums = FindAllNumsOfValue(value, nameOfRules);

        return !otherNums.Any(rules.ContainsKey);
    }
    private List<int> FindAllNumsOfValue(int value, List<string> nameOfRules)
    {
        string str = nameOfRules[value];

        return nameOfRules.Select((str, index) => new { value = str, index })
                          .Where(i => i.value == str && i.index != value)
                          .Select(i => i.index)
                          .ToList();
    }

    private List<List<string>> DictionartToList(Dictionary<int, List<string>> dictionary, int count)
    {
        List<List<string>> result = new();
        for (int i = 0; i < count; i++)
        {
            if (!dictionary.ContainsKey(i))
            {
                throw new ArgumentException("All values must has guiding set");
            }

            result.Add(dictionary[i]);
        }

        return result;
    }
}
