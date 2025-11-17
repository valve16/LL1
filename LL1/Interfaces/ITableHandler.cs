using Model.Table;

namespace Interfaces;

public interface ITableHandler
{
    void ImportTable(Table table);
    bool CheckSentenceByTable(string sentence, Table table);
    bool CheckSentence(string sentence);
}
