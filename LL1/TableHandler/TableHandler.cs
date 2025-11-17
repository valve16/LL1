using Interfaces;
using Model.Table;

namespace TableHandler;

public class TableHandler : ITableHandler
{
    private Table _storedTable = null;

    private bool _isSentenceFromGrammar = false;

    public TableHandler()
    {
    }

    public TableHandler(Table table)
    {
        ImportTable(table);
    }

    public void ImportTable(Table table)
    {
        _storedTable = table;
    }

    public bool CheckSentenceByTable(string sentence, Table table)
    {
        return IsSentenceFromGrammar(sentence, table);
    }

    public bool CheckSentence(string sentence)
    {
        if (_storedTable == null)
        {
            throw new ArgumentNullException("No table found");
        }

        return IsSentenceFromGrammar(sentence, _storedTable);
    }

    private bool IsSentenceFromGrammar(string sentence, Table table)
    {
        //Инициализация перед работой
        int sentencePos = 0;
        int tableRecordPos = 0;

        Stack<int> stack = new();

        while (sentencePos != sentence.Length)
        {
            if (!CheckLine(sentence, table.Records[tableRecordPos], ref sentencePos, ref tableRecordPos, stack))
            {
                return false;
            }
        }

        return _isSentenceFromGrammar;
    }

    private bool CheckLine(
        string sentence, 
        TableData tableData, 
        ref int sentencePos, 
        ref int tableRecordPos, 
        Stack<int> stack
        )
    {
        bool isInGuidingSet = false;
        bool isRuleSkipPresent = false;

        int shiftLength = 0;

        foreach (string check in tableData.GuidingSet)
        {
            if (sentence.Length >= check.Length + sentencePos) //Дополнительная проверка, чтобы не вылететь за границы
            {
                if (sentence.Substring(sentencePos, check.Length) == check) //Часть строки совпала с направляющим множеством
                {
                    shiftLength = check.Length;
                    isInGuidingSet = true;
                    break;
                }
            }
        }

        if (!isInGuidingSet) //Часть строки не совпала с направляющим множеством
        {
            if (tableData.Error)//В правиле указано выдать ошибку
            {
                return false;
            }
            else //В правиле не указано выдать ошибку
            {
                //WARNING: Следует иметь в виду, что при подобном переходе подразумевается,
                //что правила грамматики с одинаковыми именами идут друг за другом,
                //а последнее из них имеет error = true. Если это не так,
                //то необходимо будет менять логику программы и искать правила по всей таблице.

                tableRecordPos += 1;
                isRuleSkipPresent = true;
            }
        }

        if (!isRuleSkipPresent) //Часть строки совпала с направляющим множеством
        {
            if (!ProcessLine(sentence, tableData, ref sentencePos, ref tableRecordPos, shiftLength, stack))
            {
                return false;
            }
        }

        return true;
    }

    private bool ProcessLine(
        string sentence, 
        TableData tableData, 
        ref int sentencePos, 
        ref int tableRecordPos, 
        int shiftLength, 
        Stack<int> stack
        )
    {
        if (tableData.Shift) //В правиле указан сдвиг
        {
            sentencePos += shiftLength;

            if (!tableData.End)
            {
                sentencePos += 1;

                //WARNING: +1 отвечает за пробелы между словами.
                //На данный момент перед конечным символом "#" тоже должен существовать пробел
                //Если мы хотим это изменить, необходимо будет прописать дополнительную проверку и логику
                //А вот после "#" пробела нету, за что и отвечает условие выше

                if (sentencePos > sentence.Length) //Предложение заканчивается сразу после последнего обработанного нетерминала (без пробела)
                {
                    return false;
                }

                if (sentence[sentencePos - 1] != ' ') //Следующий символ в предложении после последнего обработанного нетерминала не является пробелом
                {
                    return false;
                }
            }
        }

        if (tableData.Stack.HasValue) //В правиле указан новый переход для стэка
        {
            stack.Push(tableData.Stack.Value);
        }

        if (tableData.End) //В правиле указано, что оно конечное
        {
            if (sentencePos == sentence.Length) //Курсор дошёл до конца правила
            {
                _isSentenceFromGrammar = true;
                return true;
            }
            else //Курсор не дошёл до конца правила
            {
                return false;
            }
        }

        if (tableData.Transition.HasValue) //В правиле указан переход
        {
            tableRecordPos = tableData.Transition.Value;
        }
        else //В правиле не указан переход
        {
            if (stack.Count != 0) //В стэке есть переход/ы
            {
                tableRecordPos = stack.Pop();
            }
            else //В стэке нет переходов
            {
                return false;
            }
        }

        return true;
    }
}
