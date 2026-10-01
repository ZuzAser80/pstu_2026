namespace Lab10.Tests;

/// <summary>
/// Covers the sorting and searching parts of the lab. The hand-written BinarySearch in
/// Program.cs is private and therefore unreachable from this assembly, so the same three
/// requirements are exercised through Array.Sort / Array.BinarySearch and the two comparers.
/// </summary>
public class SortingAndSearchTests
{
    private readonly NameComparer comparer = new();

    // Distinct names and distinct earnings, because CompareTo only looks at earnings and
    // Array.Sort is not stable: ties would make the resulting order unspecified.
    private static Organisation[] CreateArray()
    {
        return
        [
            new Organisation("address", "Charlie", 300),
            new Organisation("address", "Alpha", 100),
            new Organisation("address", "Echo", 500),
            new Organisation("address", "Bravo", 200),
            new Organisation("address", "Delta", 400),
        ];
    }

    private static bool IsSortedByMoney(Organisation[] array)
    {
        for (var i = 1; i < array.Length; i++)
        {
            if (array[i - 1].AnnualMoneyEarned > array[i].AnnualMoneyEarned)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsSortedByName(Organisation[] array)
    {
        for (var i = 1; i < array.Length; i++)
        {
            if (new NameComparer().Compare(array[i - 1], array[i]) > 0)
            {
                return false;
            }
        }
        return true;
    }

    [Fact]
    public void ArraySort_UsesIComparable_AndOrdersByAnnualMoneyEarned()
    {
        var array = CreateArray();

        Array.Sort(array);

        Assert.True(IsSortedByMoney(array));
        Assert.Equal([100, 200, 300, 400, 500], array.Select(item => item.AnnualMoneyEarned));
    }

    [Fact]
    public void ArraySort_WithNoComparer_OrdersSoThatEveryAdjacentPairAlreadyComparesInOrder()
    {
        var array = CreateArray();

        Array.Sort(array);

        // Array.Sort used IComparable.CompareTo, so no adjacent pair can still compare greater.
        Assert.True(array[0].CompareTo(array[1]) < 0);
        Assert.True(array[1].CompareTo(array[2]) < 0);
        Assert.True(array[2].CompareTo(array[3]) < 0);
        Assert.True(array[3].CompareTo(array[4]) < 0);
    }

    [Fact]
    public void ArraySort_WithNameComparer_OrdersByName()
    {
        var array = CreateArray();

        Array.Sort(array, comparer);

        Assert.True(IsSortedByName(array));
        Assert.Equal(["Alpha", "Bravo", "Charlie", "Delta", "Echo"], array.Select(item => item.Name));
    }

    [Fact]
    public void ArraySort_WithNameComparer_IgnoresAnnualMoneyEarned()
    {
        Organisation[] array =
        [
            new Organisation("address", "Bravo", 999),
            new Organisation("address", "Alpha", 1),
        ];

        Array.Sort(array, comparer);

        Assert.Equal(["Alpha", "Bravo"], array.Select(item => item.Name));
    }

    [Fact]
    public void ArraySort_OnAMixedSubtypeArray_OrdersEveryElementByTheSameRule()
    {
        Organisation[] array =
        [
            new Library("address", "Charlie", 300, BookGenresEnum.FICTION, 10, 2),
            new Dock("address", "Alpha", 100, ShipTypeEnum.CARGO, 3, 900.0),
            new Factory("address", "Bravo", 200, ProductTypeEnum.FOOD, 5, 1),
        ];

        Array.Sort(array, comparer);

        // CompareTo reads only the base field, so subclasses take part in the sort normally.
        Assert.Equal(["Alpha", "Bravo", "Charlie"], array.Select(item => item.Name));
    }

    [Fact]
    public void BinarySearch_FindsAnElementThatExistsInTheArray()
    {
        var array = CreateArray();
        Array.Sort(array, comparer);

        var index = Array.BinarySearch(array, new Organisation("", "Charlie", 0), comparer);

        Assert.True(index >= 0);
        Assert.Equal("Charlie", array[index].Name);
    }

    [Fact]
    public void BinarySearch_FindsEveryElementOfTheArray()
    {
        var array = CreateArray();
        Array.Sort(array, comparer);

        foreach (var name in new[] { "Alpha", "Bravo", "Charlie", "Delta", "Echo" })
        {
            var index = Array.BinarySearch(array, new Organisation("", name, 0), comparer);

            Assert.True(index >= 0);
            Assert.Equal(name, array[index].Name);
        }
    }

    [Fact]
    public void BinarySearch_ForAMissingName_ReturnsTheBitwiseComplementOfTheInsertionPoint()
    {
        var array = CreateArray();
        Array.Sort(array, comparer);

        var index = Array.BinarySearch(array, new Organisation("", "Zulu", 0), comparer);

        // A negative result encodes the insertion position the way Program.cs does it:
        // ~insertionPoint, which is always negative. "Zulu" sorts last, so it belongs at 5.
        Assert.True(index < 0);
        Assert.Equal(~5, index);
        Assert.Equal(5, ~index);
    }

    [Fact]
    public void BinarySearch_ForANameThatSortsBeforeEverything_ReportsIndexMinusOne()
    {
        var array = CreateArray();
        Array.Sort(array, comparer);

        var index = Array.BinarySearch(array, new Organisation("", "Aaa", 0), comparer);

        Assert.True(index < 0);
        // Insertion point 0, so ~0 == -1.
        Assert.Equal(-1, index);
    }

    [Fact]
    public void BinarySearch_ReportsTheInsertionPointWhereTheNameWouldBelong()
    {
        var array = CreateArray();
        Array.Sort(array, comparer);

        // Ordinal comparison puts "Bison" between "Alpha" and "Bravo", because "Bi" < "Br".
        // That is insertion point 1, so the returned value is ~1 == -2.
        var index = Array.BinarySearch(array, new Organisation("", "Bison", 0), comparer);

        Assert.Equal(~1, index);
        Assert.Equal(1, ~index);
    }

    [Fact]
    public void BinarySearch_MatchesTheArrayEvenThoughOnlyTheNameIsCompared()
    {
        var array = CreateArray();
        Array.Sort(array, comparer);

        // The search key differs in both base fields; only Name matters to the comparer.
        var key = new Organisation("unrelated", "Delta", -1);
        var index = Array.BinarySearch(array, key, comparer);

        Assert.True(index >= 0);
        Assert.Equal("Delta", array[index].Name);
    }
}