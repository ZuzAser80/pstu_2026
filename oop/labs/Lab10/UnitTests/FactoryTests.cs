namespace Lab10.Tests;

public class FactoryTests
{
    private static Worker[] CreateWorkers()
    {
        return
        [
            new Worker("Ivan", 5, "Welding", "Welder"),
            new Worker("Petr", 2, "Assembly", "Mechanic"),
            new Worker("Anna", 10, "Welding", "Engineer"),
        ];
    }

    private static Factory CreateFactory()
    {
        return new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY, CreateWorkers(), 7);
    }

    [Fact]
    public void ParameterlessConstructor_SetsEveryFieldToZeroValue()
    {
        var factory = new Factory();

        Assert.Equal(ProductTypeEnum.NONE, factory.ProductType);
        Assert.Empty(factory.Workers);
        Assert.Equal(0, factory.WorkerCount);
        Assert.Equal(0, factory.EngineerCount);
        Assert.Equal("placeholder", factory.Name);
        Assert.Equal("", factory.Address);
    }

    [Fact]
    public void FullConstructor_AssignsBaseAndDerivedFields()
    {
        var workers = CreateWorkers();
        var factory = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY, workers, 7);

        Assert.Equal("Saint Petersburg", factory.Address);
        Assert.Equal("MoyZavod", factory.Name);
        Assert.Equal(750_000, factory.AnnualMoneyEarned);
        Assert.Equal(ProductTypeEnum.MACHINERY, factory.ProductType);
        Assert.Equal(workers, factory.Workers);
        Assert.Equal(3, factory.WorkerCount);
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
        Assert.Equal(mold.EngineerCount, copy.EngineerCount);
        Assert.Equal(mold.Workers, copy.Workers);
        Assert.NotSame(mold.Workers, copy.Workers);
    }

    [Fact]
    public void Equals_WithIdenticalFactory_ReturnsTrue()
    {
        var first = CreateFactory();
        var second = CreateFactory();

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentWorkers_ReturnsFalse()
    {
        var first = CreateFactory();
        var second = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY,
            [new Worker("Ivan", 5, "Welding", "Welder"), new Worker("Petr", 2, "Assembly", "Mechanic"), new Worker("Anna", 11, "Welding", "Engineer")], 7);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentProductType_ReturnsFalse()
    {
        var first = CreateFactory();
        var second = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.CHEMICALS, CreateWorkers(), 7);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentEngineerCount_ReturnsFalse()
    {
        var first = CreateFactory();
        var second = new Factory("Saint Petersburg", "MoyZavod", 750_000, ProductTypeEnum.MACHINERY, CreateWorkers(), 8);

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

    [Fact]
    public void GetWorkerNamesWithMinExperience_ReturnsNamesWithExperienceNotLessThanGiven()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesWithMinExperience(5);

        Assert.Equal(["Ivan", "Anna"], names);
    }

    [Fact]
    public void GetWorkerNamesWithMinExperience_ReturnsEmpty_WhenNoWorkerMatches()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesWithMinExperience(11);

        Assert.Empty(names);
    }

    [Fact]
    public void GetWorkerNamesWithMinExperience_ReturnsAllNames_WhenThresholdIsZero()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesWithMinExperience(0);

        Assert.Equal(["Ivan", "Petr", "Anna"], names);
    }

    [Fact]
    public void GetWorkerNamesByWorkshop_ReturnsNamesOfWorkersInGivenWorkshop()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesByWorkshop("Welding");

        Assert.Equal(["Ivan", "Anna"], names);
    }

    [Fact]
    public void GetWorkerNamesByWorkshop_ReturnsEmpty_WhenNoWorkerMatches()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesByWorkshop("Painting");

        Assert.Empty(names);
    }

    [Fact]
    public void GetWorkerNamesByProfession_ReturnsNamesOfWorkersWithGivenProfession()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesByProfession("Mechanic");

        Assert.Equal(["Petr"], names);
    }

    [Fact]
    public void GetWorkerNamesByProfession_ReturnsEmpty_WhenNoWorkerMatches()
    {
        var factory = CreateFactory();

        var names = factory.GetWorkerNamesByProfession("Painter");

        Assert.Empty(names);
    }
}
