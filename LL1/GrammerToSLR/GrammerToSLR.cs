using Model.Grammer;
using Model.SLR;

namespace GrammerToSLR;

public static class GrammerToSLR
{
    public static SLRTable ConvertToSLR(Grammer grammer)
    {
        List<KeyValuePair<string, List<string>>> anotherView = CreateAnotherGrammerView(grammer);

        for (int i = 0; i < anotherView.Count; i++)
        {
            Console.Write($"{i}: {anotherView[i].Key} ->");

            foreach (string symbols in anotherView[i].Value)
            {
                Console.Write($" {symbols}");
            }

            Console.WriteLine();
        }

        return CreateSLRTable(anotherView, FindFirstValueForTerminals(anotherView, grammer.GetKeys()));
    }

    private static List<KeyValuePair<string, List<string>>> CreateAnotherGrammerView(Grammer grammer)
    {
        List<KeyValuePair<string, List<string>>> result = [];

        foreach (KeyValuePair<string, List<List<string>>> rule in grammer.Rules)
        {
            string key = rule.Key;

            for (int i = 0; i < rule.Value.Count; i++)
            {
                result.Add(new(key, new(rule.Value[i])));
            }
        }

        return result;
    }
    private static Dictionary<string, Dictionary<string, SLRNode>> FindFirstValueForTerminals(
        List<KeyValuePair<string, List<string>>> grammer,
        List<string> terminals)
    {
        Dictionary<string, Dictionary<string, SLRNode>> result = [];
        List<string> terminalAll = [];

        foreach (string terminal in terminals)
        {
            bool isAll = true;
            Dictionary<string, SLRNode> newNode = [];

            for (int i = 0; i < grammer.Count; i++)
            {
                if (grammer[i].Key != terminal)
                {
                    continue;
                }
                string first = grammer[i].Value[0];
                if (terminals.Contains(first) && first != terminal)
                {
                    isAll = false;
                }

                if (newNode.ContainsKey(first))
                {
                    NodePos nodePos = new(i, 0);
                    if (!newNode[first].Positions.Contains(nodePos))
                    {
                        newNode[first].Positions.Add(nodePos);
                    }
                }
                else
                {
                    newNode[first] = new(first, [new(i, 0)]);
                }
            }

            if (isAll)
            {
                terminalAll.Add(terminal);
            }
            result[terminal] = newNode;
        }

        Stack<string> stack = new();

        foreach (string firstTerminal in terminals)
        {
            stack.Push(firstTerminal);

            while (stack.Count > 0)
            {
                Queue<string> newForStack = new();
                string terminal = stack.Peek();
                if (terminalAll.Contains(terminal))
                {
                    stack.Pop();
                    continue;
                }
                for (int i = 0; i < grammer.Count; i++)
                {
                    if (grammer[i].Key != terminal)
                    {
                        continue;
                    }
                    string first = grammer[i].Value[0];

                    if (terminals.Contains(first) && !newForStack.Contains(first))
                    {
                        Dictionary<string, SLRNode> fromFirst = result[first];
                        Dictionary<string, SLRNode> fromTerminal = result[terminal];

                        foreach (KeyValuePair<string, SLRNode> item in fromFirst)
                        {
                            if (fromTerminal.ContainsKey(item.Key))
                            {
                                foreach (NodePos pos in item.Value.Positions)
                                {
                                    if (!fromTerminal[item.Key].Positions.Contains(pos))
                                    {
                                        fromTerminal[item.Key].Positions.Add(pos);
                                    }
                                }
                            }
                            else
                            {
                                fromTerminal.Add(item.Key, item.Value);
                            }
                        }

                        if (!terminalAll.Contains(first))
                        {
                            if (stack.Contains(first))
                            {
                                if (!terminalAll.Contains(terminal))
                                {
                                    terminalAll.Add(terminal);
                                }
                            }
                            else if (!newForStack.Contains(first))
                            {
                                newForStack.Enqueue(first);
                            }
                        }
                    }
                }

                if (newForStack.Count == 0)
                {
                    terminalAll.Add(stack.Pop());
                }
                else
                {
                    while (newForStack.Count > 0)
                    {
                        stack.Push(newForStack.Dequeue());
                    }
                }
            }
        }

        return result;
    }
    private static SLRTable CreateSLRTable(
        List<KeyValuePair<string, List<string>>> grammer,
        Dictionary<string, Dictionary<string, SLRNode>> terminalToStartValues)
    {
        Queue<SLRNode> queue = new();
        List<SLRNode> used = [];

        string firstKey = grammer[0].Key;
        SLRTable result = new();

        foreach (KeyValuePair<string, SLRNode> item in terminalToStartValues[firstKey])
        {
            queue.Enqueue(item.Value);
        }
        terminalToStartValues[firstKey][firstKey] = new SLRNode();
        if (grammer[1].Key == firstKey)
        {
            terminalToStartValues[firstKey]["#"] = new SLRNode();
        }
        result.AddNode(new(firstKey, [new(0, 0)]), terminalToStartValues[firstKey]);

        while (queue.Count > 0)
        {
            SLRNode record = queue.Peek();
            if (record.Symbol == "#")
            {
                queue.Dequeue();
                continue;
            }

            Dictionary<string, SLRNode> transitions = [];

            foreach (NodePos pos in record.Positions)
            {
                if (grammer[pos.Row].Value.Count - 1 == pos.Num)
                {
                    List<string> list = FindNextForR(pos, grammer, terminalToStartValues);

                    foreach (string next in list)
                    {
                        if (!transitions.ContainsKey(next))
                        {
                            transitions.Add(next, new(grammer[pos.Row].Key, pos.Row));
                        }
                    }
                }
                else
                {
                    string next = grammer[pos.Row].Value[pos.Num + 1];
                    if (next == "#")
                    {
                        transitions.Add("#", new(grammer.First().Key, pos.Row));
                        continue;
                    }

                    SLRNode nextNode = new(next, [new(pos.Row, pos.Num + 1)]);
                    if (transitions.ContainsKey(next))
                    {
                        if (!transitions[next].Positions.Contains(nextNode.Positions[0]))
                        {
                            transitions[next].Positions.Add(nextNode.Positions[0]);
                        }
                    }
                    else
                    {
                        transitions[next] = nextNode;
                    }

                    if (terminalToStartValues.ContainsKey(next))
                    {
                        Dictionary<string, SLRNode> fromNext = terminalToStartValues[next];

                        foreach (KeyValuePair<string, SLRNode> item in fromNext)
                        {
                            if (transitions.ContainsKey(item.Key))
                            {
                                foreach (NodePos posNext in item.Value.Positions)
                                {
                                    if (!transitions[item.Key].Positions.Contains(posNext))
                                    {
                                        transitions[item.Key].Positions.Add(posNext);
                                    }
                                }
                            }
                            else
                            {
                                transitions[item.Key] = item.Value;
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, SLRNode> item in transitions)
            {
                if (item.Value.IsOK || item.Value.IsR)
                {
                    continue;
                }
                if (!used.Contains(item.Value))
                {
                    used.Add(item.Value);
                    queue.Enqueue(item.Value);
                }
            }

            result.AddNode(queue.Dequeue(), transitions);
        }

        result.SetRowToCount(grammer.Select(list =>
            { return list.Value.Count - (list.Value.Contains("#") ? 1 : 0); }).ToList());

        return result;
    }
    private static List<string> FindNextForR(
        NodePos pos,
        List<KeyValuePair<string, List<string>>> grammer,
        Dictionary<string, Dictionary<string, SLRNode>> terminalToStartValues)
    {
        List<string> result = [];
        Queue<string> queue = new();
        List<string> usedInQueue = [];
        queue.Enqueue(grammer[pos.Row].Key);
        usedInQueue.Add(grammer[pos.Row].Key);

        while (queue.Count > 0)
        {
            string word = queue.Peek();
            for (int i = 0; i < grammer.Count; i++)
            {
                for (int j = 0; j < grammer[i].Value.Count; j++)
                {
                    if (grammer[i].Value[j] == word)
                    {
                        if (j == grammer[i].Value.Count - 1)
                        {
                            if (!usedInQueue.Contains(grammer[i].Key))
                            {
                                usedInQueue.Add(grammer[i].Key);
                                queue.Enqueue(grammer[i].Key);
                            }
                        }
                        else
                        {
                            string next = grammer[i].Value[j + 1];
                            if (!result.Contains(next))
                            {
                                result.Add(next);
                            }
                            if (terminalToStartValues.ContainsKey(next))
                            {
                                foreach (KeyValuePair<string, SLRNode> item in terminalToStartValues[next])
                                {
                                    if (!result.Contains(item.Key))
                                    {
                                        result.Add(item.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            queue.Dequeue();
        }

        return result;
    }
}
