namespace Interfaces;

public interface ISearcherGuidingSet
{
    // Например:
    // S -> AB#
    // A -> aAc | @
    // B -> b
    // @ - пустой

    // rules = 
    // [
    //      0, [4, 5, 6]
    //      1, [7, 8, 9]
    //      2, [10]
    //      3, [11]
    // ]

    // nameOfRules = 
    // [
    // 0:     S,
    // 1:     A,
    // 2:     A,
    // 3:     B,
    // 4:     A,
    // 5:     B,
    // 6:     #,
    // 7:     a,
    // 8:     A,
    // 9:    c,
    // 10:    @,
    // 11:    b,
    // ]

    // Return
    // [
    // 1:     [a, b],
    // 2:     [a],
    // 3:     [b, c],
    // 4:     [b],
    // 5:     [a, b],
    // 6:     [b],
    // 7:     [#],
    // 8:     [a],
    // 9:     [a, c],
    // 10:    [c],
    // 11:    [b, c],
    // 12:    [b],
    // ]

    List<List<string>> Search(Dictionary<int, List<int>> rules, List<string> nameOfRules);
}
