namespace Blackjack.E2E.Tests;

using Blackjack.E2E.Tests.Infrastructure;
using Microsoft.Playwright;
using Xunit;

[Collection("Playwright")]
[Trait("Category", "E2E")]
public class DashboardTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public DashboardTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_ShowsBalance()
    {
        var email = $"dash-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        // Wait for loading to complete
        var balanceAmount = page.Locator(".balance-amount-large");
        await balanceAmount.WaitForAsync(new() { Timeout = 5000 });
        var text = await balanceAmount.TextContentAsync();
        Assert.Contains("1,000", text);
    }

    [Fact]
    public async Task Dashboard_ShowsStatistics()
    {
        var email = $"dash-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        // Wait for stats
        var statsGrid = page.Locator(".stats-grid");
        await statsGrid.WaitForAsync(new() { Timeout = 5000 });

        // Should have stat cards
        var statCards = page.Locator(".stat-card");
        var count = await statCards.CountAsync();
        Assert.True(count > 0);
    }

    [Fact]
    public async Task History_IsAccessible()
    {
        var email = $"hist-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);

        await page.GotoAsync($"{_fixture.BaseUrl}/history");

        // Should show the history page title
        var heading = page.Locator("h1");
        await heading.WaitForAsync();
        var text = await heading.TextContentAsync();
        Assert.Contains("History", text);
    }
}
