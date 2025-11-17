namespace PascalLexer;


public enum StateTypes
{
    Error,
    SpecialWord,
    Identifier,
    PermationMark,
    Digit,
    Real,
}

public struct OutWord
{
    public StateTypes State;

    public string Word;


    public OutWord(StateTypes state, string word)
    {
        State = state;
        Word = word;
    }
}

public enum SymbolTypes
{
    Letter,
    DigitNotZero,
    Zero,
    Punctuation,
    Space,
}

public enum NextStateTypes
{
    None,
    Error,
    Start,
    Digit,
    Real,
    Identifier,
    Punctuation,
}


public class NextStateTypeOrString
{

    public NextStateTypes Type { get; private init; }
    public string TypeStr { get; private init; }
    public bool NeedClear { get; private init; }
    public bool IsCreate { get; private init; }
    public StateTypes StateTypes { get; private init; }

    public NextStateTypeOrString(
        NextStateTypes type,
        bool needClear = false,
        bool isCreate = false,
        StateTypes stateTypes = StateTypes.Error)
    {
        Type = type;
        TypeStr = null;
        IsCreate = isCreate;
        StateTypes = stateTypes;
        NeedClear = needClear;
    }
    public NextStateTypeOrString(
        string typeStr,
        bool needClear = false,
        bool isCreate = false,
        StateTypes stateTypes = StateTypes.Error)
    {
        Type = NextStateTypes.None;
        TypeStr = typeStr;
        IsCreate = isCreate;
        StateTypes = stateTypes;
        NeedClear = needClear;
    }
}

public class State
{
    public string Name { get; private init; }
    private readonly Automata _automata;
    public readonly Dictionary<char, NextStateTypeOrString> MapToNextState;
    private readonly Dictionary<SymbolTypes, NextStateTypeOrString> _mapOfSymbols;

    public State(
        string name,
        Automata automata,
        Dictionary<char, NextStateTypeOrString> map1,
        Dictionary<SymbolTypes, NextStateTypeOrString> map2)
    {
        Name = name;
        _automata = automata;
        MapToNextState = map1;
        _mapOfSymbols = map2;
    }

    public void Handle(char symbol)
    {
        if (MapToNextState.ContainsKey(symbol))
        {
            _automata.Next(MapToNextState[symbol]);
        }
        else if (char.IsDigit(symbol))
        {
            if (symbol == '0')
            {
                _automata.Next(_mapOfSymbols[SymbolTypes.Zero]);
            }
            else
            {
                _automata.Next(_mapOfSymbols[SymbolTypes.DigitNotZero]);
            }
        }
        else if (('A' <= symbol && symbol <= 'Z') || ('a' <= symbol && symbol <= 'z') || symbol == '_')
        {
            _automata.Next(_mapOfSymbols[SymbolTypes.Letter]);
        }
        else if (Automata.Punctuations.Contains(symbol))
        {
            _automata.Next(_mapOfSymbols[SymbolTypes.Punctuation]);
        }
        else if (symbol == ' ')
        {
            _automata.Next(_mapOfSymbols[SymbolTypes.Space]);
        }
        else
        {
            _automata.Next(new(NextStateTypes.Error));
        }
    }
    public void AddTransation(char key, NextStateTypeOrString value)
    {
        MapToNextState.Add(key, value);
    }
}

public class Automata
{
    public static char[] Punctuations =
    {
        '+',
        '-',
        '*',
        '/',
        ':',
        ';',
        ',',
        '(',
        ')',
        '[',
        ']',
        '=',
        '>',
        '<',
        '.',
        '{',
        '}',
        '/',
    };
    public static char[] EndOfGroups =
    {
        '}',
        ']',
        ')',
    };

    private State _start;
    private State _error;
    private State _digit;
    private State _real;
    private State _identifier;
    private State _punctuation;

    private Dictionary<string, State> _mapOfStates = [];

    private State _current;

    private OutWord _output;

    private string _currentWord = "";

    public Automata()
    {
        InitDefaultStates();
        InitPunctuationMarks();
        //InitReservedWords();
    }
    public OutWord GetOutWord()
    {
        return _output;
    }
    public void Handle(char symbol)
    {
        _output = new(StateTypes.Error, "");
        _current.Handle(char.ToLower(symbol));
        _currentWord += symbol;
    }

    private void InitDefaultStates()
    {
        _start = new("start", this, [], new()
        {
            {SymbolTypes.DigitNotZero, new(NextStateTypes.Digit,true) },
            {SymbolTypes.Zero, new(NextStateTypes.Digit, true) },
            {SymbolTypes.Letter, new(NextStateTypes.Identifier, true) },
            {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true) },
            {SymbolTypes.Space, new(NextStateTypes.Start, true) },
        });
        _error = new("error", this, [], new()
        {
            {SymbolTypes.DigitNotZero, new(NextStateTypes.Error) },
            {SymbolTypes.Zero, new(NextStateTypes.Error) },
            {SymbolTypes.Letter, new(NextStateTypes.Error) },
            {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.Error) },
            {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.Error) },
        });
        _digit = new("digit", this, new()
        {
            {'.', new(NextStateTypes.Real) }
        },
        new()
        {
            {SymbolTypes.DigitNotZero, new(NextStateTypes.Digit) },
            {SymbolTypes.Zero, new(NextStateTypes.Digit) },
            {SymbolTypes.Letter, new(NextStateTypes.Error) },
            {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.Digit) },
            {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.Digit) },
        });
        _real = new("real", this, [], new()
        {
            {SymbolTypes.DigitNotZero, new(NextStateTypes.Real) },
            {SymbolTypes.Zero, new(NextStateTypes.Real) },
            {SymbolTypes.Letter, new(NextStateTypes.Error) },
            {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.Real) },
            {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.Real) },
        });
        _identifier = new("identifier", this, [], new()
        {
            {SymbolTypes.DigitNotZero, new(NextStateTypes.Identifier) },
            {SymbolTypes.Zero, new(NextStateTypes.Identifier) },
            {SymbolTypes.Letter, new(NextStateTypes.Identifier) },
            {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.Identifier) },
            {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.Identifier) },
        });
        _punctuation = new("punctuation", this, [], new()
        {
            {SymbolTypes.DigitNotZero, new(NextStateTypes.Digit, true, true, StateTypes.PermationMark) },
            {SymbolTypes.Zero, new(NextStateTypes.Digit, true, true, StateTypes.PermationMark) },
            {SymbolTypes.Letter, new(NextStateTypes.Identifier, true, true, StateTypes.PermationMark) },
            {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.PermationMark) },
            //{SymbolTypes.Punctuation, new(NextStateTypes.Punctuation) },
            {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.PermationMark) },
        });


        _current = _start;
    }

    private void InitReservedWords()
    {
        //List<char> punctuationKeys = _start.MapToNextState.Keys.ToList();

        foreach (string reservedWord in Lexems.ReservedWords)
        {
            string word = "" + reservedWord[0];
            string oldWord = "";

            if (!_mapOfStates.ContainsKey(word))
            {
                _start.AddTransation(reservedWord[0], new(word, true));
                _punctuation.AddTransation(reservedWord[0], new(word, true, true, StateTypes.PermationMark));

                //Dictionary<char, NextStateTypeOrString> map = [];
                //foreach (char key in punctuationKeys)
                //{
                //    map.Add(key, new("" + key, true, true, reservedWord.Length == 1 ? StateTypes.SpecialWord : StateTypes.Identifier));
                //}

                _mapOfStates[word] = new(word, this, [], new()
                {
                    {SymbolTypes.DigitNotZero, new(NextStateTypes.Identifier) },
                    {SymbolTypes.Zero, new(NextStateTypes.Identifier) },
                    {SymbolTypes.Letter, new(NextStateTypes.Identifier) },
                    {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.Identifier) },
                    {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.Identifier) },
                });
            }

            for (int i = 1; i < reservedWord.Length; i++)
            {
                oldWord = word;
                word += reservedWord[i];

                if (!_mapOfStates.ContainsKey(word))
                {
                    //Dictionary<char, NextStateTypeOrString> map = [];

                    //foreach (char key in punctuationKeys)
                    //{
                    //    map.Add(key, new("" + key, true, true, i == reservedWord.Length - 1 ? StateTypes.SpecialWord : StateTypes.Identifier));
                    //}

                    _mapOfStates[word] = new(word, this, [], new()
                    {
                        {SymbolTypes.DigitNotZero, new(NextStateTypes.Identifier) },
                        {SymbolTypes.Zero, new(NextStateTypes.Identifier) },
                        {SymbolTypes.Letter, new(NextStateTypes.Identifier) },
                        {SymbolTypes.Punctuation, new(
                                NextStateTypes.Punctuation,
                                true,
                                true,
                                i == reservedWord.Length - 1 ? StateTypes.SpecialWord: StateTypes.Identifier
                            ) },
                        {SymbolTypes.Space, new(
                                NextStateTypes.Start,
                                true,
                                true,
                                i == reservedWord.Length - 1 ? StateTypes.SpecialWord: StateTypes.Identifier
                            ) },
                    });

                    _mapOfStates[oldWord].AddTransation(reservedWord[i], new(word));
                }
            }
        }
    }

    private void InitPunctuationMarks()
    {
        foreach (string punctuationMark in Lexems.PunctuationMarks)
        {
            string mark = "" + punctuationMark[0];
            string oldMark;

            if (!_mapOfStates.ContainsKey(mark))
            {
                _start.AddTransation(punctuationMark[0], new(mark, true));
                _identifier.AddTransation(punctuationMark[0], new(mark, true, true, StateTypes.Identifier));
                _real.AddTransation(punctuationMark[0], new(mark, true, true, StateTypes.Real));

                _mapOfStates[mark] = new(mark, this, [], new()
                {
                    {SymbolTypes.DigitNotZero, new(NextStateTypes.Digit, true, true, StateTypes.PermationMark) },
                    {SymbolTypes.Zero, new(NextStateTypes.Real, true, true, StateTypes.PermationMark) },
                    {SymbolTypes.Letter, new(NextStateTypes.Identifier, true, true, StateTypes.PermationMark) },
                    //{SymbolTypes.Punctuation, new(NextStateTypes.Punctuation) },
                    {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.PermationMark) },
                    {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.PermationMark) },
                });
            }


            for (int i = 1; i < punctuationMark.Length; i++)
            {
                oldMark = mark;
                mark += punctuationMark[i];

                if (!_mapOfStates.ContainsKey(punctuationMark))
                {
                    _mapOfStates[mark] = new(mark, this, [], new()
                    {
                        {SymbolTypes.DigitNotZero, new(NextStateTypes.Digit, true, true, StateTypes.PermationMark) },
                        {SymbolTypes.Zero, new(NextStateTypes.Digit, true, true, StateTypes.PermationMark) },
                        {SymbolTypes.Letter, new(NextStateTypes.Identifier, true, true, StateTypes.PermationMark) },
                        //{SymbolTypes.Punctuation, new(NextStateTypes.Punctuation) },
                        {SymbolTypes.Punctuation, new(NextStateTypes.Punctuation, true, true, StateTypes.PermationMark) },
                        {SymbolTypes.Space, new(NextStateTypes.Start, true, true, StateTypes.PermationMark) },
                    });

                    _mapOfStates[oldMark].AddTransation(punctuationMark[i], new(mark));
                }
            }
        }
    }

    public void Next(NextStateTypeOrString next)
    {
        //if (_currentWord.Length == 1 && EndOfGroups.Contains(_currentWord[0]) && next.Type == NextStateTypes.Punctuation)
        //{
        //    _output = new(StateTypes.PermationMark, _currentWord);
        //    _currentWord = "";
        //}
        if (_currentWord.Length > 1 && _currentWord.Last() == '.' && _current.Name == _real.Name && next.Type != NextStateTypes.Real)
        {
            //if (next.Type == NextStateTypes.Punctuation)
            if (next.TypeStr != null && next.TypeStr == ".")
            {
                _output = new(StateTypes.Digit, _currentWord.Substring(0, _currentWord.Length - 1));
                _currentWord = ".";

                _current = _mapOfStates[".."];
                //_current = _punctuation;

                return;
            }
            else
            {
                _output = new(StateTypes.Error, _currentWord);

                _currentWord = "";
            }
        }
        else if (next.IsCreate)
        {
            _output = new(next.StateTypes, _currentWord);
        }

        if (next.TypeStr != null)
        {
            _current = _mapOfStates[next.TypeStr];
        }
        else
        {
            switch (next.Type)
            {
                case NextStateTypes.Error:
                    _current = _error;
                    break;
                case NextStateTypes.Start:
                    _current = _start;
                    break;
                case NextStateTypes.Digit:
                    if (_currentWord == "0" && _current.Name == _digit.Name)
                    {
                        _current = _error;
                    }
                    else
                    {
                        _current = _digit;
                    }
                    break;
                case NextStateTypes.Real:
                    _current = _real;
                    break;
                case NextStateTypes.Identifier:
                    _current = _identifier;
                    break;
                case NextStateTypes.Punctuation:
                    _current = _punctuation;
                    break;
                case NextStateTypes.None:
                default:
                    break;
            }
        }
        if (next.NeedClear)
        {
            _currentWord = "";
        }
    }
}
