using ConverterToCorrectGrammar;
using GrammerToSLR;
using Model.Grammer;
using Model.SLR;
using SLRHandler;

internal class Program
{
    private static void Main(string[] args)
    {
        Grammer grammar = new();

        //grammar.AddRule("Z", [["S", "#"]]);
        //grammar.AddRule("S", [["E", "#"], ["E", "*", "i"], ["E", "+", "i"]]);
        //grammar.AddRule("E", [["E", "a"], ["b"], ["@"]]);
        //grammar.AddRule("S", [["A", "B", "C", "#"]]);
        //grammar.AddRule("A", [["A", "a"], ["@"]]);
        //grammar.AddRule("B", [["B", "b"], ["@"]]);
        //grammar.AddRule("C", [["C", "c"], ["@"]]);
        //grammar.AddRule("Z", [["S", "#"]]);
        //grammar.AddRule("S", [["S", "*", "i"], ["i"]]);
        //grammar.AddRule("S", [["a", "A", "b"], ["c", "B", "b"]]);
        //grammar.AddRule("A", [["x", "A", "y"], ["a"]]);
        //grammar.AddRule("B", [["y", "B", "x"], ["b"]]);

        grammar.AddRule("<PROGRAM>", [["PROGRAM", "IDENTIFIER", "SEMICOLON", "<BLOCK>", "DOT"],
            ["PROGRAM", "IDENTIFIER", "SEMICOLON", "<VAR-SECTION>", "<BLOCK>", "DOT"]]);
        grammar.AddRule("<VAR-SECTION>", [["VAR", "<VAR-DECL-LIST>"]]);
        grammar.AddRule("<VAR-DECL-LIST>", [["<VAR-DECL>", "SEMICOLON", "<VAR-DECL-LIST>"], ["<VAR-DECL>", "SEMICOLON"]]);
        grammar.AddRule("<VAR-DECL>", [["<VAR-LIST>", "COLON", "<VAR-TYPE>"]]);
        grammar.AddRule("<VAR-LIST>", [["IDENTIFIER", "COMMA", "<VAR-LIST>"], ["IDENTIFIER"]]);
        grammar.AddRule("<VAR-TYPE>", [["INTEGER"], ["BOOLEAN"], ["CHAR"], ["REAL"]]);
        grammar.AddRule("<BLOCK>", [["BEGIN", "<STATEMENT-LIST>", "END"]]);
        grammar.AddRule("<STATEMENT-LIST>", [["<STATEMENT>", "SEMICOLON", "<STATEMENT-LIST>"], ["<STATEMENT>"]]);
        grammar.AddRule("<STATEMENT>", [["<ASSIGNMENT>"], ["<IF-STATEMENT>"], ["<WHILE-STATEMENT>"], ["<BLOCK>"]]);
        grammar.AddRule("<ASSIGNMENT>", [["IDENTIFIER", "ASSIGNMENT", "<EXPRESSION>"]]);
        grammar.AddRule("<IF-STATEMENT>", [["IF", "<CONDITION>", "THEN", "<STATEMENT>", "ELSE", "<STATEMENT>"],
            ["IF", "<CONDITION>", "THEN", "<STATEMENT>"]]);
        grammar.AddRule("<WHILE-STATEMENT>", [["WHILE", "<CONDITION>", "DO", "<STATEMENT>"]]);
        grammar.AddRule("<CONDITION>", [["<EXPRESSION>", "<REL-OP>", "<EXPRESSION>"]]);
        grammar.AddRule("<REL-OP>", [["EQUALS"],
            ["NOT_EQUAL"], ["LESS_THAN"], ["GREATER_THAN"], ["LESS_THAN_OR_EQUAL"], ["GREATER_THAN_OR_EQUAL"]]);
        grammar.AddRule("<EXPRESSION>", [["<TERM>", "<ADD-OP>", "<EXPRESSION>"], ["<TERM>"]]);
        grammar.AddRule("<TERM>", [["<FACTOR>", "<MUL-OP>", "<TERM>"], ["<FACTOR>"]]);
        grammar.AddRule("<FACTOR>", [["FLOAT"], ["DIGIT"], ["DOUBLE"], ["IDENTIFIER"],
            ["LEFT_PARENTHESIS", "<EXPRESSION>", "RIGHT_PARENTHESIS"]]);
        grammar.AddRule("<ADD-OP>", [["PLUS"], ["MINUS"]]);
        grammar.AddRule("<MUL-OP>", [["ASTERISK"], ["SLASH"]]);

        try
        {
            grammar = grammar.ConvertToCorrectSLR();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        SLRTable slr = GrammerToSLR.GrammerToSLR.ConvertToSLR(grammar);

        foreach (KeyValuePair<SLRNode, Dictionary<string, SLRNode>> item in slr.Nodes)
        {
            Console.Write($"{item.Key.Symbol}:");

            foreach (NodePos pos in item.Key.Positions)
            {
                Console.Write($" ({pos.Row + 1}, {pos.Num + 1})");
            }
            Console.WriteLine();
            Console.Write("--> ");
            foreach (KeyValuePair<string, SLRNode> transition in item.Value)
            {
                Console.Write($" {transition.Key}");
                if (transition.Value.IsOK)
                {
                    Console.Write(" OK");
                }
                else if (transition.Value.IsR)
                {
                    Console.Write($" R-{transition.Value.Symbol}-{transition.Value.Positions[0].Row + 1}");
                }
                else
                {
                    foreach (NodePos pos in transition.Value.Positions)
                    {
                        Console.Write($" ({pos.Row + 1}, {pos.Num + 1})");
                    }
                }
                Console.Write(";");
            }
            Console.WriteLine();
        }

        SLRTableHandler slrTableHandler = new(slr);

        if (Lexer.TryHandle("../../../../Test.txt", out List<Token> tokens))
        {
            List<string> tokensStrs = tokens.Select(t => t.TokenType).ToList();
            tokensStrs.Add("#");
            bool isSuccess = slrTableHandler.CheckSentence(tokensStrs, out int successCount);

            if (isSuccess)
            {
                Console.WriteLine("OK");
            }
            else
            {
                Console.WriteLine("BAD");
                Token token = tokens[successCount];
                Console.WriteLine($"{token.TokenType}, ({token.Line}, {token.Position})-{token.Value}");
            }
        }
        else
        {
            Console.WriteLine("Lexer has errors");
        }

        //string line = Console.ReadLine();

        //while (line != null && line != "...")
        //{
        //    Console.WriteLine(slrTableHandler.CheckSentence(line));

        //    line = Console.ReadLine();
        //}

        //foreach (KeyValuePair<string, List<List<string>>> rule in grammar.Rules)
        //{
        //    Console.Write($"{rule.Key} ->");

        //    foreach (List<string> transition in rule.Value)
        //    {
        //        foreach (string item in transition)
        //        {
        //            Console.Write($" {item}");
        //        }

        //        if (transition != rule.Value.Last())
        //        {
        //            Console.Write(" |");
        //        }
        //    }

        //    Console.WriteLine();
        //}

        // GrammerToTable.GrammerToTable gToT = new(new SearcherGuidingSet.SearcherGuidingSet());


        // Table table = gToT.Convert(grammar);

        // foreach (TableData record in table.Records)
        // {
        //     Console.Write($"Num - {record.Num}; name - {record.Name}; set:");

        //     foreach (string symbol in record.GuidingSet)
        //     {
        //         Console.Write($" {symbol}");
        //     }

        //     string nullStr = "null";
        //     Console.Write($"; transition - {(record.Transition != null ? record.Transition : nullStr)}; error - {record.Error}; ");
        //     Console.WriteLine($"shift - {record.Shift}; stack - {(record.Stack != null ? record.Stack : nullStr)}; end - {record.End}");
        // }

        // TableHandler.TableHandler tableHandler = new(table);

        // string line = Console.ReadLine();

        // while (line != null)
        // {
        //     Console.WriteLine(tableHandler.CheckSentence(line));

        //     line = Console.ReadLine();
        // }
    }
}
