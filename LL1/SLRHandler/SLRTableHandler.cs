using Model.SLR;

namespace SLRHandler;

public class SLRTableHandler
{
    SLRTable _table;

    public SLRTableHandler(SLRTable table)
    {
        _table = table;
    }

    public bool CheckSentence(string sentence, out int successCount)
    {
        if (_table == null)
        {
            throw new ArgumentException("Table is empty");
        }

        List<string> sentences = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        return IsSentencesFromGrammer(sentences, _table, out successCount);
    }
    public bool CheckSentence(List<string> sentences, out int successCount)
    {
        return IsSentencesFromGrammer(sentences, _table, out successCount);
    }
    private static bool IsSentencesFromGrammer(List<string> sentences, SLRTable table, out int successCount)
    {
        successCount = 0;
        int generalCount = sentences.Count;
        Stack<string> symbols = new();
        Stack<SLRNode> nodes = new();
        nodes.Push(table.Nodes.First().Key);

        while (sentences.Count > 0)
        {
            if (nodes.Count == 0)
            {
                return false;
            }
            string sentence = sentences[0];
            SLRNode currentNode = nodes.Peek();

            if (!table.Nodes[currentNode].ContainsKey(sentence))
            {
                return false;
            }

            SLRNode next = table.Nodes[currentNode][sentence];

            if (next.IsOK)
            {
                if (symbols.Count == 0 && nodes.Count == 1 && (sentences.Count == 1 || sentences.Count == 2))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (next.IsR)
            {
                int count = table.RowToCount[next.Positions[0].Row];
                string symbol = next.Symbol;

                sentences.Insert(0, symbol);

                for (int i = 0; i < count; i++)
                {
                    symbols.Pop();
                    nodes.Pop();
                }
            }
            else
            {
                symbols.Push(sentence);
                nodes.Push(next);
                sentences.RemoveAt(0);

                successCount = generalCount - sentences.Count;
            }
        }

        return false;
    }
}
