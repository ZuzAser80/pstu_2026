namespace Lab10.Tests;

public class LibraryTests
{
    private static Library CreateLibrary()
    {
        return new Library("Moscow", "RayonnayaBiblioteka", 120_000, BookGenresEnum.SCIENCE, 8500, 320);
    }

    [Fact]
    public void ParameterlessConstructor_SetsEveryFieldToZeroValue()
    {
        var library = new Library();

        Assert.Equal(BookGenresEnum.NONE, library.BookGenres);
        Assert.Equal(0, library.BookCount);
        Assert.Equal(0, library.TextbookCount);
        Assert.Equal("placeholder", library.Name);
        Assert.Equal("", library.Address);
    }

    [Fact]
    public void FullConstructor_AssignsBaseAndDerivedFields()
    {
        var library = CreateLibrary();

        Assert.Equal("Moscow", library.Address);
        Assert.Equal("RayonnayaBiblioteka", library.Name);
        Assert.Equal(120_000, library.AnnualMoneyEarned);
        Assert.Equal(BookGenresEnum.SCIENCE, library.BookGenres);
        Assert.Equal(8500, library.BookCount);
        Assert.Equal(320, library.TextbookCount);
    }

    [Fact]
    public void CopyConstructor_CopiesEveryFieldAndCreatesNewInstance()
    {
        var mold = CreateLibrary();

        var copy = new Library(mold);

        Assert.True(mold.Equals(copy));
        Assert.NotSame(mold, copy);
        Assert.Equal(mold.BookGenres, copy.BookGenres);
        Assert.Equal(mold.BookCount, copy.BookCount);
        Assert.Equal(mold.TextbookCount, copy.TextbookCount);
    }

    [Fact]
    public void Equals_WithIdenticalLibrary_ReturnsTrue()
    {
        var first = CreateLibrary();
        var second = CreateLibrary();

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentBookCount_ReturnsFalse()
    {
        var first = CreateLibrary();
        var second = new Library("Moscow", "RayonnayaBiblioteka", 120_000, BookGenresEnum.SCIENCE, 8501, 320);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentBookGenres_ReturnsFalse()
    {
        var first = CreateLibrary();
        var second = new Library("Moscow", "RayonnayaBiblioteka", 120_000, BookGenresEnum.FICTION, 8500, 320);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentTextbookCount_ReturnsFalse()
    {
        var first = CreateLibrary();
        var second = new Library("Moscow", "RayonnayaBiblioteka", 120_000, BookGenresEnum.SCIENCE, 8500, 321);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithAnotherSubtypeHoldingTheSameBaseFields_ReturnsFalse()
    {
        var library = CreateLibrary();
        var insurance = new InsuranceCompany("Moscow", "RayonnayaBiblioteka", 120_000, InsuranceTypeEnum.CAR, 8500, 320);

        Assert.False(library.Equals(insurance));
        Assert.False(insurance.Equals(library));
    }

    [Fact]
    public void Clone_ReturnsLibraryThatIsEqualToOriginal_AndKeepsTheName()
    {
        var original = CreateLibrary();

        var clone = Assert.IsType<Library>(original.Clone());

        Assert.Equal(original.Name, clone.Name);
        Assert.True(original.Equals(clone));
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void ShallowCopy_ReturnsLibraryThatIsEqualToOriginal()
    {
        var original = CreateLibrary();

        var shallow = Assert.IsType<Library>(original.ShallowCopy());

        Assert.True(original.Equals(shallow));
        Assert.NotSame(original, shallow);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(200)]
    public void RandomInit_KeepsEveryDerivedFieldInRange(int iterations)
    {
        var library = new Library();

        for (var i = 0; i < iterations; i++)
        {
            library.RandomInit();

            Assert.InRange(library.BookCount, 1, 14);
            Assert.InRange(library.TextbookCount, 0, 499);
            Assert.True(Enum.IsDefined(library.BookGenres));
        }
    }

    [Fact]
    public void RandomInit_AlsoFillsTheBaseFields()
    {
        var library = new Library();

        library.RandomInit();

        Assert.InRange(library.AnnualMoneyEarned, 100, 9999);
        Assert.True(Guid.TryParse(library.Name, out _));
        Assert.True(Guid.TryParse(library.Address, out _));
    }
}