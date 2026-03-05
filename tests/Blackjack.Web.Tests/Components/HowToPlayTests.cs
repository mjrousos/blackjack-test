using Bunit;
using Xunit;
using Blackjack.Web.Pages;

namespace Blackjack.Web.Tests.Components;

public class HowToPlayTests : BunitContext
{
    [Fact]
    public void HowToPlay_RendersPageTitle()
    {
        var cut = Render<HowToPlay>();

        var heading = cut.Find("h1");
        Assert.Contains("How to Play", heading.TextContent);
    }

    [Fact]
    public void HowToPlay_ContainsObjectiveSection()
    {
        var cut = Render<HowToPlay>();

        var headings = cut.FindAll("h2");
        Assert.Contains(headings, h => h.TextContent.Contains("Objective"));
    }

    [Fact]
    public void HowToPlay_ContainsCardValuesSection()
    {
        var cut = Render<HowToPlay>();

        var headings = cut.FindAll("h2");
        Assert.Contains(headings, h => h.TextContent.Contains("Card Values"));
    }

    [Fact]
    public void HowToPlay_ContainsGameplaySection()
    {
        var cut = Render<HowToPlay>();

        var headings = cut.FindAll("h2");
        Assert.Contains(headings, h => h.TextContent.Contains("Gameplay"));
    }

    [Fact]
    public void HowToPlay_ContainsPayoutsSection()
    {
        var cut = Render<HowToPlay>();

        var headings = cut.FindAll("h2");
        Assert.Contains(headings, h => h.TextContent.Contains("Payouts"));
    }

    [Fact]
    public void HowToPlay_HasStartPlayingLink()
    {
        var cut = Render<HowToPlay>();

        var link = cut.Find("a[href='/']");
        Assert.NotNull(link);
    }
}
