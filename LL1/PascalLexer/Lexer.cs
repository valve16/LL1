using PascalLexer;
using static PascalLexer.Lexems;

public enum Comment
{
    None,
    Block,
    Line
}
public class Token
{
    public string TokenType { get; set; }
    public string Value { get; set; }
    public int Line { get; set; }
    public int Position { get; set; }
}

public static class Lexer
{
    public static bool TryHandle(string fileName, out List<Token> tokens)
    {
        tokens = [];
        Automata automata = new();
        List<string> errors = [];

        using (StreamReader streamReader = new(fileName))
        {
            int lineNum = 0;
            string line;
            Comment comment = Comment.None;
            bool isString = false;
            int stringLine = 0;
            int stringPos = 0;
            string stringText = "";
            int commentLine = 0;
            int commentPos = 0;
            string commentText = "";

            while ((line = streamReader.ReadLine()) != null)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    if (comment == Comment.None && !isString)
                    {
                        HandleSymbol(automata, line[i], tokens, i, lineNum, out comment, out isString);

                        if (comment == Comment.Block)
                        {
                            commentLine = lineNum;
                            commentPos = i;
                            commentText += line[i];
                        }

                        if (isString)
                        {
                            stringLine = lineNum;
                            stringPos = i;
                            stringText += line[i];
                        }
                    }
                    else if (isString)
                    {
                        stringText += line[i];

                        if (line[i] == '\'')
                        {
                            isString = false;

                            tokens.Add(new()
                            {
                                TokenType = "STRING_TEXT",
                                Line = stringLine,
                                Position = stringPos,
                                Value = stringText,
                            });

                            stringText = "";
                        }
                    }
                    else if (comment == Comment.Block)
                    {
                        commentText += line[i];

                        if (line[i] == '}')
                        {
                            comment = Comment.None;
                            commentText = "";
                        }
                    }
                }

                if (comment == Comment.None)
                {
                    HandleSymbol(automata, ' ', tokens, line.Length, lineNum, out Comment comment1, out bool isString1);
                }

                if (comment == Comment.Line)
                {
                    comment = Comment.None;
                }

                lineNum++;
            }

            if (isString)
            {
                isString = false;

                tokens.Add(new()
                {
                    TokenType = "ERROR",
                    Line = stringLine,
                    Position = stringPos,
                    Value = stringText,
                });

                stringText = "";

                errors.Add("Not closed string in " + (stringLine + 1).ToString() + " line");
            }

            if (comment != Comment.None)
            {
                tokens.Add(new()
                {
                    TokenType = "ERROR",
                    Line = commentLine,
                    Position = commentPos,
                    Value = commentText,
                });
                errors.Add("Comment block not closed");
            }
        }

        if (tokens.Count == 0)
        {
            tokens.Add(new()
            {
                TokenType = "ERRPR",
                Line = 0,
                Position = 0,
                Value = "File is empty"
            });
        }

        foreach (Token item in tokens)
        {
            Console.WriteLine(item.TokenType + "(" +
                (item.Line).ToString() +
                ", " + (item.Position).ToString() + ")-\"" + item.Value + "\"");
        }

        Console.WriteLine();

        foreach (string error in errors)
        {
            Console.WriteLine(error);
        }

        return errors.Count == 0;
    }

    private static void HandleSymbol(
        Automata automata,
        char symbol,
        List<Token> tokens,
        int position,
        int lineNum,
        out Comment comment,
        out bool isString)
    {
        if (symbol == '{')
        {
            comment = Comment.Block;
            isString = false;
            return;
        }

        if (symbol == '\'')
        {
            comment = Comment.None;
            isString = true;
            return;
        }

        automata.Handle(symbol);
        OutWord outWord = automata.GetOutWord();
        if (!string.IsNullOrWhiteSpace(outWord.Word))
        {
            if (outWord.Word == "//")
            {
                comment = Comment.Line;
                isString = false;
                return;
            }
            else
            {
                if (outWord.Word[0] == '.' && tokens.Last().TokenType == "DIGIT")
                {
                    tokens.Last().Position--;
                }

                if (outWord.State == StateTypes.Error && tokens.Last().Value == "." && IsFloatPart(outWord.Word))
                {
                    tokens[tokens.Count - 1].Value = "." + outWord.Word;
                    tokens[tokens.Count - 1].TokenType = "ERROR";

                    comment = Comment.None;
                    isString = false;

                    return;
                }

                string type = GetTokenType(outWord);


                if (outWord.State == StateTypes.Digit && tokens.Last().Value == "."
                    && tokens.Last().Position == position - outWord.Word.Length - 1)
                {
                    tokens[tokens.Count - 1].Value = "." + outWord.Word;
                    tokens[tokens.Count - 1].TokenType = "DOUBLE";
                }
                else if (type == "DIGIT" &&
                    (tokens[tokens.Count - 1].Value == "+" || tokens[tokens.Count - 1].Value == "-")
                    && tokens[tokens.Count - 2].TokenType == "ERROR")
                {
                    if (CheckIsFloatPoint(tokens[tokens.Count - 2].Value, tokens[tokens.Count - 1].Value, outWord.Word))
                    {
                        int line = tokens[tokens.Count - 2].Line;
                        int pos = tokens[tokens.Count - 2].Position;

                        string real = tokens[tokens.Count - 2].Value;
                        string sign = tokens[tokens.Count - 1].Value;
                        string degree = outWord.Word;

                        tokens.RemoveRange(tokens.Count - 2, 2);

                        tokens.Add(new()
                        {
                            TokenType = "FLOAT",
                            Line = line,
                            Position = pos,
                            Value = real + sign + degree,
                        });
                    }
                    else
                    {
                        tokens.Add(new()
                        {
                            TokenType = type,
                            Line = lineNum,
                            Position = position - outWord.Word.Length,
                            Value = outWord.Word,
                        });
                    }
                }
                else
                {
                    tokens.Add(new()
                    {
                        TokenType = type,
                        Line = lineNum,
                        Position = position - outWord.Word.Length,
                        Value = outWord.Word,
                    });
                }
            }
        }

        comment = Comment.None;
        isString = false;
    }
    private static string GetTokenType(OutWord word)
    {
        if (word.State == StateTypes.Digit)
        {
            if (IsStrDigit(word.Word))
            {
                return "DIGIT";
            }
            else
            {
                return "ERROR";
            }

        }
        else if (word.State == StateTypes.Real)
        {
            return "DOUBLE";
        }
        else if (word.State == StateTypes.Identifier || word.State == StateTypes.SpecialWord)
        {
            if (word.Word.Length > 256)
            {
                return "ERROR";
            }

            string needWord = word.Word.ToLower();
            string reservedWord = ReservedWords.FirstOrDefault(w => w == needWord);

            if (reservedWord == null)
            {
                return "IDENTIFIER";
            }
            else
            {
                return reservedWord.ToUpper();
            }
        }
        else if (word.State == StateTypes.PermationMark)
        {
            string mark = PunctuationMarks.FirstOrDefault(w => w == word.Word);

            if (mark != null)
            {
                return GetTokenTypeOfMark(word.Word);
            }
            else
            {
                return "ERROR";
            }
        }
        else
        {
            return "ERROR";
        }
    }

    private static bool IsStrDigit(string str)
    {
        if (int.TryParse(str, out int value))
        {
            return -32768 <= value && value <= 32767;
        }

        return false;
    }
    private static bool CheckIsFloatPoint(string real, string sign, string degree)
    {
        if (sign != "+" && sign != "-")
        {
            return false;
        }

        if (degree.Length > 2)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < degree.Length; i++)
            {
                if (!char.IsDigit(degree[i]))
                {
                    return false;
                }
            }
        }

        int index = 0;

        while (real[index] != '.' && real[index] != 'e' && index < real.Length)
        {
            if (!char.IsDigit(real[index]))
            {
                return false;
            }

            index++;
        }

        if (index == real.Length - 1 && real.Last() == 'e')
        {
            return true;
        }

        if (index > real.Length - 2)
        {
            return false;
        }

        for (int i = index + 1; i < real.Length - 1; i++)
        {
            if (!char.IsDigit(real[i]))
            {
                return false;
            }
        }

        if (real.Last() != 'e')
        {
            return false;
        }

        return true;
    }
    private static bool IsFloatPart(string str)
    {
        if (str.Last() != 'e')
        {
            return false;
        }

        for (int i = 0; i < str.Length - 1; i++)
        {
            if (!char.IsDigit(str[i]))
            {
                return false;
            }
        }

        return true;
    }
}
