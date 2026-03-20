namespace Blackjack.E2E.Tests;

using Blackjack.E2E.Tests.Infrastructure;
using Microsoft.Playwright;
using Xunit;

[Collection("Playwright")]
[Trait("Category", "E2E")]
public class ThemeTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public ThemeTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ThemeSelector_IsVisible_WhenAuthenticated()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var themeToggle = page.Locator(".theme-toggle");
        await themeToggle.WaitForAsync(new() { Timeout = 5000 });
        Assert.True(await themeToggle.IsVisibleAsync());
    }

    [Fact]
    public async Task ThemeSelector_OpensDropdown_OnClick()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var themeToggle = page.Locator(".theme-toggle");
        await themeToggle.WaitForAsync(new() { Timeout = 5000 });
        await themeToggle.ClickAsync();

        var dropdown = page.Locator(".theme-dropdown");
        await dropdown.WaitForAsync(new() { Timeout = 3000 });
        Assert.True(await dropdown.IsVisibleAsync());

        var options = page.Locator(".theme-option");
        var count = await options.CountAsync();
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task ThemeSelector_DefaultsToClassicTheme()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dataTheme = await page.EvaluateAsync<string?>("document.documentElement.getAttribute('data-theme')");
        Assert.True(string.IsNullOrEmpty(dataTheme), $"Expected no data-theme attribute for classic theme, but got '{dataTheme}'");
    }

    [Fact]
    public async Task ThemeSelector_SwitchesToMidnightBlue()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await SelectThemeAsync(page, "Midnight Blue");

        var dataTheme = await page.EvaluateAsync<string?>("document.documentElement.getAttribute('data-theme')");
        Assert.Equal("midnight-blue", dataTheme);
    }

    [Fact]
    public async Task ThemeSelector_SwitchesToRoyalPurple()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await SelectThemeAsync(page, "Royal Purple");

        var dataTheme = await page.EvaluateAsync<string?>("document.documentElement.getAttribute('data-theme')");
        Assert.Equal("royal-purple", dataTheme);
    }

    [Fact]
    public async Task ThemeSelector_PersistsAcrossNavigation()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await SelectThemeAsync(page, "Midnight Blue");

        // Navigate to dashboard
        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dataTheme = await page.EvaluateAsync<string?>("document.documentElement.getAttribute('data-theme')");
        Assert.Equal("midnight-blue", dataTheme);
    }

    [Fact]
    public async Task ThemeSelector_PersistsAcrossReload()
    {
        var email = $"theme-test-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await SelectThemeAsync(page, "Royal Purple");

        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dataTheme = await page.EvaluateAsync<string?>("document.documentElement.getAttribute('data-theme')");
        Assert.Equal("royal-purple", dataTheme);
    }

    [Fact]
    public async Task ThemeSelector_AppliesOnLoginPage()
    {
        var page = await _fixture.CreatePageAsync();

        // Set theme in localStorage before navigating to login
        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Login");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.EvaluateAsync("localStorage.setItem('blackjack-theme', 'midnight-blue')");

        // Reload to pick up the localStorage value
        await page.ReloadAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dataTheme = await page.EvaluateAsync<string?>("document.documentElement.getAttribute('data-theme')");
        Assert.Equal("midnight-blue", dataTheme);
    }

    private async Task SelectThemeAsync(IPage page, string themeName)
    {
        var themeToggle = page.Locator(".theme-toggle");
        await themeToggle.WaitForAsync(new() { Timeout = 5000 });
        await themeToggle.ClickAsync();

        var option = page.Locator($".theme-option:has-text('{themeName}')");
        await option.WaitForAsync(new() { Timeout = 3000 });
        await option.ClickAsync();
    }
}
