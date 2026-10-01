namespace Lab10.Tests;

public class DockTests
{
    private static Dock CreateDock()
    {
        return new Dock("Saint Petersburg", "MoyDock", 500_000, ShipTypeEnum.CARGO, 12, 40_000.0);
    }

    [Fact]
    public void ParameterlessConstructor_SetsEveryFieldToZeroValue()
    {
        var dock = new Dock();

        Assert.Equal(ShipTypeEnum.NONE, dock.ShipType);
        Assert.Equal(0, dock.ShipsInDock);
        Assert.Equal(0.0, dock.ShipDisplacement);
        Assert.Equal("placeholder", dock.Name);
        Assert.Equal("", dock.Address);
    }

    [Fact]
    public void FullConstructor_AssignsBaseAndDerivedFields()
    {
        var dock = CreateDock();

        Assert.Equal("Saint Petersburg", dock.Address);
        Assert.Equal("MoyDock", dock.Name);
        Assert.Equal(500_000, dock.AnnualMoneyEarned);
        Assert.Equal(ShipTypeEnum.CARGO, dock.ShipType);
        Assert.Equal(12, dock.ShipsInDock);
        Assert.Equal(40_000.0, dock.ShipDisplacement);
    }

    [Fact]
    public void CopyConstructor_CopiesEveryFieldAndCreatesNewInstance()
    {
        var mold = CreateDock();

        var copy = new Dock(mold);

        Assert.True(mold.Equals(copy));
        Assert.NotSame(mold, copy);
        Assert.Equal(mold.ShipType, copy.ShipType);
        Assert.Equal(mold.ShipsInDock, copy.ShipsInDock);
        Assert.Equal(mold.ShipDisplacement, copy.ShipDisplacement);
    }

    [Fact]
    public void Equals_WithIdenticalDock_ReturnsTrue()
    {
        var first = CreateDock();
        var second = CreateDock();

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentShipsInDock_ReturnsFalse()
    {
        var first = CreateDock();
        var second = new Dock("Saint Petersburg", "MoyDock", 500_000, ShipTypeEnum.CARGO, 13, 40_000.0);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentShipType_ReturnsFalse()
    {
        var first = CreateDock();
        var second = new Dock("Saint Petersburg", "MoyDock", 500_000, ShipTypeEnum.TANKER, 12, 40_000.0);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentDisplacement_ReturnsFalse()
    {
        var first = CreateDock();
        var second = new Dock("Saint Petersburg", "MoyDock", 500_000, ShipTypeEnum.CARGO, 12, 40_001.0);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithAnotherSubtypeHoldingTheSameBaseFields_ReturnsFalse()
    {
        var dock = CreateDock();
        var factory = new Factory("Saint Petersburg", "MoyDock", 500_000, ProductTypeEnum.FOOD, 12, 3);

        Assert.False(dock.Equals(factory));
        Assert.False(factory.Equals(dock));
    }

    [Fact]
    public void Clone_ReturnsDockThatIsEqualToOriginal_AndKeepsTheName()
    {
        var original = CreateDock();

        var clone = Assert.IsType<Dock>(original.Clone());

        // Unlike Organisation.Clone(), Dock.Clone() goes through the copy constructor, so the
        // name survives untouched and there is no "Клон " prefix anywhere.
        Assert.Equal(original.Name, clone.Name);
        Assert.True(original.Equals(clone));
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void ShallowCopy_ReturnsDockThatIsEqualToOriginal()
    {
        var original = CreateDock();

        var shallow = Assert.IsType<Dock>(original.ShallowCopy());

        // MemberwiseClone preserves the exact runtime type, and every field is a value or a
        // string, so the shallow copy still compares equal.
        Assert.True(original.Equals(shallow));
        Assert.NotSame(original, shallow);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(200)]
    public void RandomInit_KeepsEveryDerivedFieldInRange(int iterations)
    {
        var dock = new Dock();

        for (var i = 0; i < iterations; i++)
        {
            dock.RandomInit();

            Assert.InRange(dock.ShipsInDock, 1, 14);
            Assert.InRange(dock.ShipDisplacement, 1000.0, 49_999.0);
            Assert.True(Enum.IsDefined(dock.ShipType));
        }
    }

    [Fact]
    public void RandomInit_AlsoFillsTheBaseFields()
    {
        var dock = new Dock();

        dock.RandomInit();

        Assert.InRange(dock.AnnualMoneyEarned, 100, 9999);
        Assert.True(Guid.TryParse(dock.Name, out _));
        Assert.True(Guid.TryParse(dock.Address, out _));
    }
}