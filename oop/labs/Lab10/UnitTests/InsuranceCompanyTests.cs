namespace Lab10.Tests;

public class InsuranceCompanyTests
{
    private static InsuranceCompany CreateInsuranceCompany()
    {
        return new InsuranceCompany("Kazan", "Rosgosstrakh", 300_000, InsuranceTypeEnum.CAR, 1200, 4_500_000);
    }

    [Fact]
    public void ParameterlessConstructor_SetsEveryFieldToZeroValue()
    {
        var insurance = new InsuranceCompany();

        Assert.Equal(InsuranceTypeEnum.NONE, insurance.InsuranceType);
        Assert.Equal(0, insurance.ClientCount);
        Assert.Equal(0, insurance.InsuranceFund);
        Assert.Equal("placeholder", insurance.Name);
        Assert.Equal("", insurance.Address);
    }

    [Fact]
    public void FullConstructor_AssignsBaseAndDerivedFields()
    {
        var insurance = CreateInsuranceCompany();

        Assert.Equal("Kazan", insurance.Address);
        Assert.Equal("Rosgosstrakh", insurance.Name);
        Assert.Equal(300_000, insurance.AnnualMoneyEarned);
        Assert.Equal(InsuranceTypeEnum.CAR, insurance.InsuranceType);
        Assert.Equal(1200, insurance.ClientCount);
        Assert.Equal(4_500_000, insurance.InsuranceFund);
    }

    [Fact]
    public void CopyConstructor_CopiesEveryFieldAndCreatesNewInstance()
    {
        var mold = CreateInsuranceCompany();

        var copy = new InsuranceCompany(mold);

        Assert.True(mold.Equals(copy));
        Assert.NotSame(mold, copy);
        Assert.Equal(mold.InsuranceType, copy.InsuranceType);
        Assert.Equal(mold.ClientCount, copy.ClientCount);
        Assert.Equal(mold.InsuranceFund, copy.InsuranceFund);
    }

    [Fact]
    public void Equals_WithIdenticalInsuranceCompany_ReturnsTrue()
    {
        var first = CreateInsuranceCompany();
        var second = CreateInsuranceCompany();

        Assert.True(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentClientCount_ReturnsFalse()
    {
        var first = CreateInsuranceCompany();
        var second = new InsuranceCompany("Kazan", "Rosgosstrakh", 300_000, InsuranceTypeEnum.CAR, 1201, 4_500_000);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentInsuranceType_ReturnsFalse()
    {
        var first = CreateInsuranceCompany();
        var second = new InsuranceCompany("Kazan", "Rosgosstrakh", 300_000, InsuranceTypeEnum.HOUSE, 1200, 4_500_000);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithDifferentInsuranceFund_ReturnsFalse()
    {
        var first = CreateInsuranceCompany();
        var second = new InsuranceCompany("Kazan", "Rosgosstrakh", 300_000, InsuranceTypeEnum.CAR, 1200, 4_500_001);

        Assert.False(first.Equals(second));
    }

    [Fact]
    public void Equals_WithAnotherSubtypeHoldingTheSameBaseFields_ReturnsFalse()
    {
        var insurance = CreateInsuranceCompany();
        var dock = new Dock("Kazan", "Rosgosstrakh", 300_000, ShipTypeEnum.CARGO, 1200, 4_500_000.0);

        Assert.False(insurance.Equals(dock));
        Assert.False(dock.Equals(insurance));
    }

    [Fact]
    public void Clone_ReturnsInsuranceCompanyThatIsEqualToOriginal_AndKeepsTheName()
    {
        var original = CreateInsuranceCompany();

        var clone = Assert.IsType<InsuranceCompany>(original.Clone());

        Assert.Equal(original.Name, clone.Name);
        Assert.True(original.Equals(clone));
        Assert.NotSame(original, clone);
    }

    [Fact]
    public void ShallowCopy_ReturnsInsuranceCompanyThatIsEqualToOriginal()
    {
        var original = CreateInsuranceCompany();

        var shallow = Assert.IsType<InsuranceCompany>(original.ShallowCopy());

        Assert.True(original.Equals(shallow));
        Assert.NotSame(original, shallow);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(200)]
    public void RandomInit_KeepsEveryDerivedFieldInRange(int iterations)
    {
        var insurance = new InsuranceCompany();

        for (var i = 0; i < iterations; i++)
        {
            insurance.RandomInit();

            Assert.InRange(insurance.ClientCount, 1, 14);
            Assert.InRange(insurance.InsuranceFund, 1000, 99_999);
            Assert.True(Enum.IsDefined(insurance.InsuranceType));
        }
    }

    [Fact]
    public void RandomInit_AlsoFillsTheBaseFields()
    {
        var insurance = new InsuranceCompany();

        insurance.RandomInit();

        Assert.InRange(insurance.AnnualMoneyEarned, 100, 9999);
        Assert.True(Guid.TryParse(insurance.Name, out _));
        Assert.True(Guid.TryParse(insurance.Address, out _));
    }
}