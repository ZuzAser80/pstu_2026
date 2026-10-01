namespace Lab10.Tests;

public class NameComparerTests
{
    private readonly NameComparer comparer = new();

    [Fact]
    public void Compare_WithEqualNames_ReturnsZero()
    {
        var first = new Organisation("address", "same", 100);
        var second = new Organisation("other", "same", 200);

        Assert.Equal(0, comparer.Compare(first, second));
    }

    [Fact]
    public void Compare_WithSmallerName_ReturnsNegative()
    {
        var smaller = new Organisation("address", "Alpha", 100);
        var bigger = new Organisation("address", "Beta", 100);

        Assert.True(comparer.Compare(smaller, bigger) < 0);
        Assert.True(comparer.Compare(bigger, smaller) > 0);
    }

    [Fact]
    public void Compare_IsCaseSensitive_BecauseComparisonIsOrdinal()
    {
        var uppercase = new Organisation("address", "Zebra", 100);
        var lowercase = new Organisation("address", "apple", 100);

        // StringComparison.Ordinal sorts by code point, so 'Z' (90) lands before 'a' (97).
        Assert.True(comparer.Compare(uppercase, lowercase) < 0);
    }

    [Fact]
    public void Compare_IgnoresAddressAndMoney()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("other", "name", 999);

        Assert.Equal(0, comparer.Compare(first, second));
    }

    [Fact]
    public void Compare_WithBothNull_ReturnsZero()
    {
        Assert.Equal(0, comparer.Compare(null, null));
    }

    [Fact]
    public void Compare_WithFirstArgumentNull_ReturnsZero()
    {
        var original = new Organisation("address", "name", 100);

        // Organisation.cs:120 treats any null as "equal", so this reports 0 even though the
        // names differ. Documented rather than endorsed: an IComparer must return a negative
        // value when x sorts before y, and null does not.
        Assert.Equal(0, comparer.Compare(null, original));
    }

    [Fact]
    public void Compare_WithSecondArgumentNull_ReturnsZero()
    {
        var original = new Organisation("address", "name", 100);

        Assert.Equal(0, comparer.Compare(original, null));
    }
}