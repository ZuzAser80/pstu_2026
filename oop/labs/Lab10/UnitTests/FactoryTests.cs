namespace Lab10.Tests;

public class FactoryTests
{
    private static Factory CreateFactory()
    {
        return new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY, 40, 7);
    }

    [Fact]
    public void ParameterlessConstructor_SetsEveryFieldToZeroValue()
    {
        var factory = new Factory();

        Assert.Equal(ProductTypeEnum.NONE, factory.ProductType);
        Assert.Equal(0, factory.WorkerCount);
        Assert.Equal(0, factory.EngineerCount);
        Assert.Equal("placeholder", factory.Name);
        Assert.Equal("", factory.Address);
    }

    [Fact]
    public void FullConstructor_AssignsBaseAndDerivedFields()
    {
        var factory = CreateFactory();

        Assert.Equal("Saint Petersburg", factory.Address);
        Assert.Equal("MoyZavod", factory.Name);
        Assert.Equal(750_000, factory.AnnualMoneyEarned);
        Assert.Equal(ProductTypeEnum.MACHINERY, factory.ProductType);
        Assert.Equal(40, factory.WorkerCount);
        Assert.Equal(7, factory.EngineerCount);
    }

    [Fact]
    public void CopyConstructor_CopiesEveryFieldAndCreatesNewInstance()
    {
        var mold = CreateFactory();

        var copy = new Factory(mold);

        Assert.True(mold.Equals(copy));
        Assert.NotSame(mold, copy);
        Assert.Equal(mold.ProductType, copy.ProductType);
        Assert.Equal(mold.WorkerCount, copy.WorkerCount);
        Assert.Equal(mold.EngineerCount, copy.EngineerCount);
    }

    [Fact]
    public void Equals_WithIdenticalFactory_ReturnsTrue()
    {
        var first = CreateFactory();
        var second = CreateFactory();

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentWorkerCount_ReturnsFalse()
    {
        var first = CreateFactory();
        var second = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY, 41, 7);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentProductType_ReturnsFalse()
    {
        var first = CreateFactory();
        var second = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.CHEMICALS, 40, 7);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentEngineerCount_ReturnsFalse()
    {
        var first = CreateFactory();
        var second = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY, 40, 8);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithAnotherSubtypeHoldingTheSameBaseFields_ReturnsFalse()
    {
        var factory = CreateFactory();
        var library = new Library("Saint Petersburg", "MoyZavod", 750_000, BookGenresEnum.SCIENCE, 40, 7);

        Assert.False(factory.Equals(library));
        Assert.False(library.Equals(factory));
    }

    [Fact]
    public void Clone_ReturnsFactoryThatIsEqualToOriginal_AndKeepsTheName()
    {
        var original = CreateFactory();

        var clone = Assert.IsType<Factory>(original.Clone());

        Assert.Equal(original.Name, clone.Name);
        Assert.True(original.Equals(clone));
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void ShallowCopy_ReturnsFactoryThatIsEqualToOriginal()
    {
        var original = CreateFactory();

        var shallow = Assert.IsType<Factory>(original.ShallowCopy());

        Assert.True(original.Equals(shallow));
        Assert.NotSame(original, shallow);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(200)]
    public void RandomInit_KeepsEveryDerivedFieldInRange(int iterations)
    {
        var factory = new Factory();

        for (var i = 0; i < iterations; i++)
        {
            factory.RandomInit();

            Assert.InRange(factory.WorkerCount, 1, 14);
            Assert.InRange(factory.EngineerCount, 0, 19);
            Assert.True(Enum.IsDefined(factory.ProductType));
        }
    }

    [Fact]
    public void RandomInit_AlsoFillsTheBaseFields()
    {
        var factory = new Factory();

        factory.RandomInit();

        Assert.InRange(factory.AnnualMoneyEarned, 100, 9999);
        Assert.True(Guid.TryParse(factory.Name, out _));
        Assert.True(Guid.TryParse(factory.Address, out _));
    }
}