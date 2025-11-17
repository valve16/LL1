using Model.Grammer;
using Model.Table;

namespace Interfaces;

public interface IGramerToTable
{
    Table Convert(Grammer grammer);
}
