namespace Lab10.Tests;

/// <summary>
/// Covers the parts of the lab that depend on inheritance rather than on one concrete class:
/// dynamic type identification (Part 2) and polymorphism through the IInit interface (Part 3).
/// </summary>
public class HierarchyTests
{
    private static Organisation[] CreateMixedArray()
    {
        return
        [
            new Dock("Petersburg", "MoyDock", 500_000, ShipTypeEnum.CARGO, 12, 40_000.0),
            new Factory("Petersburg", "MoyZavod", 400_000, ProductTypeEnum.FOOD, 40, 7),
            new InsuranceCompany("Petersburg", "MoyStrahovka", 300_000, InsuranceTypeEnum.CAR, 1200, 4_500_000),
            new Library("Petersburg", "MoyBiblioteka", 200_000, BookGenresEnum.SCIENCE, 8500, 320),
            new Organisation("Petersburg", "BaseFirm", 100_000),
        ];
    }

    [Fact]
    public void OrganisationArray_HoldsOneObjectOfEveryTypeInTheHierarchy()
    {
        var array = CreateMixedArray();

        // Distinct() here builds a HashSet of Type objects, whose GetHashCode is fine.
        // Hashing the Organisation elements themselves would throw, because
        // Organisation.GetHashCode is not implemented.
        var distinctTypes = array.Select(item => item.GetType()).Distinct().ToList();

        Assert.Equal(5, distinctTypes.Count);
        Assert.Contains(typeof(Dock), distinctTypes);
        Assert.Contains(typeof(Factory), distinctTypes);
        Assert.Contains(typeof(InsuranceCompany), distinctTypes);
        Assert.Contains(typeof(Library), distinctTypes);
        Assert.Contains(typeof(Organisation), distinctTypes);
    }

    [Fact]
    public void IsOperator_IdentifiesTheExactSubtypeAtRuntime()
    {
        var array = CreateMixedArray();

        Assert.True(array[0] is Dock);
        Assert.True(array[1] is Factory);
        Assert.True(array[2] is InsuranceCompany);
        Assert.True(array[3] is Library);

        // A base-class instance is not any of the derived types.
        Assert.False(array[4] is Dock);
    }

    [Fact]
    public void IsOperator_WithPatternVariable_UnwrapsTheDerivedType()
    {
        var array = CreateMixedArray();

        var shipsInDock = 0;
        foreach (Organisation item in array)
        {
            if (item is Dock dock)
            {
                shipsInDock += dock.ShipsInDock;
            }
        }

        Assert.Equal(12, shipsInDock);
    }

    [Fact]
    public void AsOperator_ReturnsTheInstance_WhenTheCastIsValid()
    {
        var array = CreateMixedArray();

        var dock = array[0] as Dock;

        Assert.NotNull(dock);
        Assert.Equal(12, dock!.ShipsInDock);
    }

    [Fact]
    public void AsOperator_ReturnsNull_WhenTheCastIsInvalid()
    {
        var array = CreateMixedArray();

        // as never throws, which is exactly how it differs from a hard cast.
        Assert.Null(array[1] as Dock);
        Assert.Null(array[4] as Library);
    }

    [Fact]
    public void RandomInit_CalledThroughABaseReference_RunsTheDerivedOverride()
    {
        Organisation reference = new Dock();

        reference.RandomInit();

        // Organisation.RandomInit alone would leave ShipsInDock at 0, so a value inside
        // [1, 14] proves that Dock.RandomInit ran through dynamic dispatch.
        var dock = Assert.IsType<Dock>(reference);
        Assert.InRange(dock.ShipsInDock, 1, 14);
    }

    [Fact]
    public void RandomInit_CalledThroughABaseReference_RunsEveryDerivedOverride()
    {
        Organisation dockReference = new Dock();
        Organisation factoryReference = new Factory();
        Organisation libraryReference = new Library();
        Organisation insuranceReference = new InsuranceCompany();

        dockReference.RandomInit();
        factoryReference.RandomInit();
        libraryReference.RandomInit();
        insuranceReference.RandomInit();

        Assert.InRange(((Dock)dockReference).ShipsInDock, 1, 14);
        Assert.InRange(((Factory)factoryReference).EngineerCount, 0, 19);
        Assert.InRange(((Library)libraryReference).TextbookCount, 0, 499);
        Assert.InRange(((InsuranceCompany)insuranceReference).ClientCount, 1, 14);
    }

    [Fact]
    public void RandomInit_ThroughIInitInterface_FillsEveryElementOfAMixedArray()
    {
        IInit[] array =
        [
            new Dock(),
            new Library(),
            new Factory(),
            new InsuranceCompany(),
            new InitClass(),
        ];

        foreach (var item in array)
        {
            item.RandomInit();
        }

        // Each element took the override belonging to its own runtime type.
        Assert.InRange(((Dock)array[0]).ShipsInDock, 1, 14);
        Assert.InRange(((Library)array[1]).BookCount, 1, 14);
        Assert.InRange(((Factory)array[2]).WorkerCount, 1, 14);
        Assert.InRange(((InsuranceCompany)array[3]).ClientCount, 1, 14);
        Assert.InRange(((InitClass)array[4]).Value, 0, 999);
    }

    [Fact]
    public void IInitArray_HoldsBothHierarchyMembersAndTheOutsideClass()
    {
        IInit[] array = [new Dock(), new Organisation(), new InitClass()];

        var distinctTypes = array.Select(item => item.GetType()).Distinct().ToList();

        Assert.Equal(3, distinctTypes.Count);
        Assert.All(array, item => Assert.True(item is IInit));
    }

    [Fact]
    public void GetType_ReturnsTheDerivedType_EvenWhenTheVariableIsTypedAsOrganisation()
    {
        Organisation reference = new Library();

        // Dynamic type identification: the compiler type is Organisation, the runtime type
        // is Library.
        Assert.Equal(typeof(Organisation), reference.GetType().BaseType);
        Assert.Equal(typeof(Library), reference.GetType());
    }
}