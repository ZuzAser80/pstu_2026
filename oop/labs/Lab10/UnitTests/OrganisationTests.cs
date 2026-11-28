namespace Lab10.Tests;

public class OrganisationTests
{
    #region Constructors

    [Fact]
    public void ParameterlessConstructor_SetsPlaceholderNameAndEmptyAddressAndZeroMoney()
    {
        var original = new Organisation();

        Assert.Equal("placeholder", original.Name);
        Assert.Equal("", original.Address);
        Assert.Equal(0, original.AnnualMoneyEarned);
        Assert.Empty(original.Workers);
    }

    [Fact]
    public void FullConstructor_AssignsAllFields()
    {
        var workers = new[] { new Worker("Ivan", 5, "Welding", "Welder") };
        var original = new Organisation("Saint Petersburg", "MoyDock", 500_000, workers);

        Assert.Equal("Saint Petersburg", original.Address);
        Assert.Equal("MoyDock", original.Name);
        Assert.Equal(500_000, original.AnnualMoneyEarned);
        Assert.Equal(workers, original.Workers);
    }

    [Fact]
    public void CopyConstructor_CopiesAllFields()
    {
        var workers = new[] { new Worker("Ivan", 5, "Welding", "Welder") };
        var mold = new Organisation("address", "name", 100, workers);
        var copy = new Organisation(mold);

        Assert.Equal(mold.Address, copy.Address);
        Assert.Equal(mold.Name, copy.Name);
        Assert.Equal(mold.AnnualMoneyEarned, copy.AnnualMoneyEarned);
        Assert.Equal(mold.Workers, copy.Workers);
        Assert.NotSame(mold.Workers, copy.Workers);
    }

    [Fact]
    public void CopyConstructor_CreatesDistinctInstance()
    {
        var mold = new Organisation("address", "name", 100);
        var copy = new Organisation(mold);

        Assert.NotSame(mold, copy);
        Assert.True(mold.Equals(copy));
    }

    #endregion

    #region Name validation

    [Fact]
    public void EmptyName_IsRejected_AndLeavesNameUnchanged()
    {
        var original = new Organisation("address", "", 100);

        Assert.Equal("", original.Name);
    }

    [Fact]
    public void EmptyName_DoesNotPreventAssigningOtherFields()
    {
        var original = new Organisation("address", "", 100);

        Assert.Equal("address", original.Address);
        Assert.Equal(100, original.AnnualMoneyEarned);
    }

    [Fact]
    public void NonEmptyName_IsAssigned()
    {
        var original = new Organisation("address", "name", 100);

        Assert.Equal("name", original.Name);
    }

    #endregion

    #region Equals

    [Fact]
    public void Equals_WithIdenticalValues_ReturnsTrue()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("address", "name", 100);

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void AssertEqualOnTwoOrganisations_ComparesByMoneyOnly_NotByEquals()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("other", "other", 100);

        // Organisation implements IComparable, and xUnit's equality comparer prefers
        // IComparable.CompareTo over object.Equals. CompareTo only reads AnnualMoneyEarned, so
        // Assert.Equal calls these equal even though Equals correctly calls them different.
        // Consequence for this suite: always assert with .Equals(...) directly.
        Assert.Equal(first, second);
        Assert.False(first.Equals(second));
        Assert.Equal(0, first.CompareTo(second));
    }

    [Fact]
    public void Equals_WithItself_ReturnsTrue()
    {
        var original = new Organisation("address", "name", 100);

        Assert.True(original.Equals(original));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        var original = new Organisation("address", "name", 100);

        Assert.False(original.Equals(null!));
    }

    [Fact]
    public void Equals_WithDifferentSubtype_ReturnsFalse()
    {
        var original = new Organisation("address", "name", 100);
        var derived = new Dock("address", "name", 100, ShipTypeEnum.CARGO, 3, 1500.0);

        // Organisation.cs:78 compares GetType(), so a subtype is never equal to its base.
        Assert.False(original.Equals(derived));
        Assert.False(derived.Equals(original));
    }

    [Fact]
    public void Equals_WithDifferentName_ReturnsFalse()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("address", "other", 100);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentAddress_ReturnsFalse()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("other", "name", 100);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentMoney_ReturnsFalse()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("address", "name", 200);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentWorkers_ReturnsFalse()
    {
        var first = new Organisation("address", "name", 100, [new Worker("Ivan", 5, "Welding", "Welder")]);
        var second = new Organisation("address", "name", 100, [new Worker("Ivan", 6, "Welding", "Welder")]);

        Assert.False(first.Equals(second));
    }

    #endregion

    #region IComparable

    [Fact]
    public void CompareTo_WithLowerMoney_ReturnsNegative()
    {
        var smaller = new Organisation("address", "name", 100);
        var bigger = new Organisation("address", "name", 200);

        Assert.True(smaller.CompareTo(bigger) < 0);
    }

    [Fact]
    public void CompareTo_WithHigherMoney_ReturnsPositive()
    {
        var bigger = new Organisation("address", "name", 200);
        var smaller = new Organisation("address", "name", 100);

        Assert.True(bigger.CompareTo(smaller) > 0);
    }

    [Fact]
    public void CompareTo_WithEqualMoney_ReturnsZero_EvenWhenOtherFieldsDiffer()
    {
        var first = new Organisation("address", "name", 100);
        var second = new Organisation("other", "other", 100);

        // CompareTo only looks at money, so it reports these as equal.
        Assert.Equal(0, first.CompareTo(second));
    }

    [Fact]
    public void CompareTo_WithNull_ThrowsArgumentException()
    {
        var original = new Organisation("address", "name", 100);

        Assert.Throws<ArgumentException>(() => { original.CompareTo(null); });
    }

    [Fact]
    public void CompareTo_WithNonOrganisation_ThrowsArgumentException()
    {
        var original = new Organisation("address", "name", 100);

        Assert.Throws<ArgumentException>(() => { original.CompareTo("not an organisation"); });
    }

    #endregion

    #region Clone vs ShallowCopy

    [Fact]
    public void Clone_SwapsAddressAndName_BecauseArgumentOrderDoesNotMatchConstructor()
    {
        var original = new Organisation("address", "name", 100);

        var clone = Assert.IsType<Organisation>(original.Clone());

        // SUSPECTED BUG in Organisation.cs:111. The constructor is declared
        // (address, name, money) but Clone passes ("Клон " + Name, Address, Money), so the
        // prefix ends up in Address and the original Address ends up in Name.
        // The assertions below describe the real behaviour, not the intended behaviour.
        Assert.Equal("Клон name", clone.Address);
        Assert.Equal("address", clone.Name);
    }

    [Fact]
    public void Clone_KeepsMoney_BecauseOnlyTheStringsAreSwapped()
    {
        var original = new Organisation("address", "name", 100);

        var clone = Assert.IsType<Organisation>(original.Clone());

        Assert.Equal(original.AnnualMoneyEarned, clone.AnnualMoneyEarned);
    }

    [Fact]
    public void Clone_IsNotEqualToOriginal_BecauseBothStringsEndUpDifferent()
    {
        var original = new Organisation("address", "name", 100);

        var clone = Assert.IsType<Organisation>(original.Clone());

        Assert.False(original.Equals(clone));
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void ShallowCopy_KeepsNameUnchanged()
    {
        var original = new Organisation("address", "name", 100);

        var shallow = original.ShallowCopy();

        Assert.Equal("name", shallow.Name);
    }

    [Fact]
    public void ShallowCopy_IsEqualButNotSameReference()
    {
        var original = new Organisation("address", "name", 100);

        var shallow = original.ShallowCopy();

        Assert.NotSame(original, shallow);
        // Every field here is a value or a string, so a shallow copy still compares equal.
        Assert.True(original.Equals(shallow));
    }

    [Fact]
    public void CloneAndShallowCopy_DifferInBothStringFields_ButAgreeOnMoney()
    {
        var original = new Organisation("address", "name", 100);

        var clone = Assert.IsType<Organisation>(original.Clone());
        var shallow = original.ShallowCopy();

        // MemberwiseClone reproduces both strings verbatim; Clone reassigns them.
        Assert.Equal(original.Name, shallow.Name);
        Assert.Equal(original.Address, shallow.Address);
        Assert.NotEqual(shallow.Name, clone.Name);
        Assert.NotEqual(shallow.Address, clone.Address);
        Assert.Equal(original.AnnualMoneyEarned, clone.AnnualMoneyEarned);
        Assert.Equal(original.AnnualMoneyEarned, shallow.AnnualMoneyEarned);
    }

    #endregion

    #region Worker queries

    [Fact]
    public void GetWorkerNamesWithMinExperience_ReturnsMatchingNames()
    {
        var organisation = new Organisation("address", "name", 100,
        [
            new Worker("Ivan", 5, "Welding", "Welder"),
            new Worker("Petr", 2, "Assembly", "Mechanic"),
            new Worker("Anna", 10, "Welding", "Engineer"),
        ]);

        var names = organisation.GetWorkerNamesWithMinExperience(5);

        Assert.Equal(["Ivan", "Anna"], names);
    }

    [Fact]
    public void GetWorkerNamesByWorkshop_ReturnsMatchingNames()
    {
        var organisation = new Organisation("address", "name", 100,
        [
            new Worker("Ivan", 5, "Welding", "Welder"),
            new Worker("Petr", 2, "Assembly", "Mechanic"),
            new Worker("Anna", 10, "Welding", "Engineer"),
        ]);

        var names = organisation.GetWorkerNamesByWorkshop("Welding");

        Assert.Equal(["Ivan", "Anna"], names);
    }

    [Fact]
    public void GetWorkerNamesByProfession_ReturnsMatchingNames()
    {
        var organisation = new Organisation("address", "name", 100,
        [
            new Worker("Ivan", 5, "Welding", "Welder"),
            new Worker("Petr", 2, "Assembly", "Mechanic"),
            new Worker("Anna", 10, "Welding", "Engineer"),
        ]);

        var names = organisation.GetWorkerNamesByProfession("Mechanic");

        Assert.Equal(["Petr"], names);
    }

    #endregion
}
