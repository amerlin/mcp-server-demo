using McpDemo.Tools;
using ModelContextProtocol;
using Xunit;

namespace McpDemo.Tests;

public sealed class DemoToolsTests
{
    [Fact]
    public void Saluta_TrimsName() => Assert.Contains("Ciao, Mario!", DemoTools.Saluta(" Mario "));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Mario\nRossi")]
    public void Saluta_RejectsInvalidNames(string name) =>
        Assert.Throws<McpException>(() => DemoTools.Saluta(name));

    [Fact]
    public void Saluta_RejectsLongNames() =>
        Assert.Throws<McpException>(() => DemoTools.Saluta(new string('a', 81)));

    [Fact]
    public void CalcolaIva_UsesDefaultRate() =>
        Assert.Equal(new VatResult(100m, 22m, 22m, 122m, "EUR"), DemoTools.CalcolaIva(100m));

    [Fact]
    public void CalcolaIva_RoundsMidpointsAwayFromZero() =>
        Assert.Equal(0.01m, DemoTools.CalcolaIva(0.05m, 10m).Iva);

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1000000, 100)]
    public void CalcolaIva_AcceptsBoundaryValues(int amount, int rate) =>
        Assert.Equal(amount + amount * (decimal)rate / 100m, DemoTools.CalcolaIva(amount, rate).Totale);

    [Theory]
    [InlineData(-1, 22)]
    [InlineData(1000001, 22)]
    [InlineData(100, -1)]
    [InlineData(100, 101)]
    public void CalcolaIva_RejectsOutOfRangeValues(int amount, int rate) =>
        Assert.Throws<McpException>(() => DemoTools.CalcolaIva(amount, rate));

    [Fact]
    public void CalcolaIva_RejectsFractionalCents() =>
        Assert.Throws<McpException>(() => DemoTools.CalcolaIva(1.001m));
}