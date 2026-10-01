namespace Lab10.Tests;

/// <summary>
/// InitClass is the class that does not belong to the Organisation hierarchy but implements
/// IInit, which is what Part 3 of the lab asks for.
/// </summary>
public class InitClassTests
{
    [Fact]
    public void ParameterlessConstructor_SetsEmptyCaptionAndZeroValue()
    {
        var item = new InitClass();

        Assert.Equal("", item.Caption);
        Assert.Equal(0, item.Value);
    }

    [Fact]
    public void InitClass_IsNotPartOfTheOrganisationHierarchy()
    {
        var item = new InitClass();

        // The compiler proves at compile time that InitClass never is an Organisation, so this
        // is asserted through the base type instead of an "is Organisation" pattern.
        Assert.Equal(typeof(object), item.GetType().BaseType);
        Assert.True(item is IInit);
    }

    [Fact]
    public void InitClass_CanShareAnArrayWithOrganisationSubclasses()
    {
        IInit[] array = [new InitClass(), new Dock(), new Organisation()];

        // The interface is the only common type, which is the point of Part 3.
        Assert.Equal(3, array.Length);
        Assert.All(array, item => Assert.NotNull(item));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(200)]
    public void RandomInit_AlwaysSetsAGuidCaptionAndAValueInRange(int iterations)
    {
        var item = new InitClass();

        for (var i = 0; i < iterations; i++)
        {
            item.RandomInit();

            Assert.True(Guid.TryParse(item.Caption, out _));
            Assert.InRange(item.Value, 0, 999);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    public void RandomInit_ProducesADifferentCaptionEachCall(int iterations)
    {
        var item = new InitClass();
        var seenCaptions = new List<string>();

        for (var i = 0; i < iterations; i++)
        {
            item.RandomInit();
            seenCaptions.Add(item.Caption);
        }

        Assert.Equal(iterations, seenCaptions.Distinct().Count());
    }
}