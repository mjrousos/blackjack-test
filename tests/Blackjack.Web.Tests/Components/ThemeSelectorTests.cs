using Bunit;
using Xunit;
using Blackjack.Web.Components;

namespace Blackjack.Web.Tests.Components;

public class ThemeSelectorTests : BunitContext
{
    private void SetupJSInterop()
    {
        JSInterop.Setup<string>("getTheme", _ => true).SetResult("");
        JSInterop.SetupVoid("setTheme", _ => true);
    }

    [Fact]
    public void Renders_ThemeToggleButton()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle");
    }

    [Fact]
    public void Dropdown_InitiallyHidden()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        Assert.Throws<ElementNotFoundException>(() => cut.Find(".theme-dropdown"));
    }

    [Fact]
    public void ClickToggle_ShowsDropdown()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();

        cut.Find(".theme-dropdown");
    }

    [Fact]
    public void Dropdown_ContainsThreeOptions()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();

        var options = cut.FindAll(".theme-option");
        Assert.Equal(3, options.Count);
    }

    [Fact]
    public void Dropdown_ContainsCorrectThemeNames()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();

        var options = cut.FindAll(".theme-option");
        Assert.Contains("Classic Casino", options[0].TextContent);
        Assert.Contains("Midnight Blue", options[1].TextContent);
        Assert.Contains("Royal Purple", options[2].TextContent);
    }

    [Fact]
    public void Dropdown_ContainsSwatches()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();

        cut.Find(".swatch-classic");
        cut.Find(".swatch-midnight");
        cut.Find(".swatch-purple");
    }

    [Fact]
    public void ClassicOption_ActiveByDefault()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();

        var options = cut.FindAll(".theme-option");
        Assert.Contains("active", options[0].ClassList);
    }

    [Fact]
    public void SelectingTheme_CallsJSInterop()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();
        var options = cut.FindAll(".theme-option");
        options[1].Click();

        var invocation = JSInterop.VerifyInvoke("setTheme");
        Assert.Equal("midnight-blue", invocation.Arguments[0]);
    }

    [Fact]
    public void SelectingTheme_ClosesDropdown()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();
        cut.Find(".theme-dropdown"); // confirm open

        var options = cut.FindAll(".theme-option");
        options[1].Click();

        Assert.Throws<ElementNotFoundException>(() => cut.Find(".theme-dropdown"));
    }

    [Fact]
    public void ClickToggle_TwiceClosesDropdown()
    {
        SetupJSInterop();
        var cut = Render<ThemeSelector>();

        cut.Find("button.theme-toggle").Click();
        cut.Find(".theme-dropdown"); // confirm open

        cut.Find("button.theme-toggle").Click();

        Assert.Throws<ElementNotFoundException>(() => cut.Find(".theme-dropdown"));
    }
}
