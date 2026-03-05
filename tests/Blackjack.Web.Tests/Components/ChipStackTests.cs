using Bunit;
using Xunit;
using Blackjack.Web.Components;

namespace Blackjack.Web.Tests.Components;

public class ChipStackTests : BunitContext
{
    [Fact]
    public void DisplaysAmountInChipValue()
    {
        var cut = Render<ChipStack>(p => p
            .Add(x => x.Amount, 50m));

        var value = cut.Find(".chip-value");
        Assert.Equal("$50", value.TextContent);
    }

    [Theory]
    [InlineData(5, "chip-white")]
    [InlineData(10, "chip-white")]
    [InlineData(15, "chip-red")]
    [InlineData(25, "chip-red")]
    [InlineData(50, "chip-green")]
    [InlineData(100, "chip-green")]
    [InlineData(250, "chip-black")]
    [InlineData(500, "chip-black")]
    [InlineData(1000, "chip-gold")]
    public void ChipColor_BasedOnAmount(int amount, string expectedClass)
    {
        var cut = Render<ChipStack>(p => p
            .Add(x => x.Amount, (decimal)amount));

        var chip = cut.Find(".chip");
        Assert.Contains(expectedClass, chip.ClassList);
    }
}
