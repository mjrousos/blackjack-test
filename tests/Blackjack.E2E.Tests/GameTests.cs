namespace Blackjack.E2E.Tests;

using Blackjack.E2E.Tests.Infrastructure;
using Microsoft.Playwright;
using Xunit;

[Collection("Playwright")]
[Trait("Category", "E2E")]
public class GameTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public GameTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GamePage_ShowsBettingArea_WhenAuthenticated()
    {
        var email = $"game-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);

        // Should show balance and betting area
        var balanceDisplay = page.Locator(".balance-value");
        await balanceDisplay.WaitForAsync();
        var balance = await balanceDisplay.TextContentAsync();
        Assert.Contains("1,000", balance);

        // Should show chip selector
        var chipButtons = page.Locator(".chip-btn");
        var count = await chipButtons.CountAsync();
        Assert.True(count > 0);
    }

    private async Task WaitForBlazorCircuit(IPage page)
    {
        // Wait for the Blazor Server circuit to connect.
        // After page load, Blazor establishes a SignalR connection.
        // We detect this by waiting for network idle and then attempting
        // a click-and-verify cycle to ensure handlers are attached.
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Additionally, retry clicking a chip button until the Deal button appears,
        // confirming the interactive circuit is handling events.
        var chipBtn = page.Locator(".chip-btn").First;
        await chipBtn.WaitForAsync(new() { Timeout = 10000 });

        var dealBtn = page.Locator("button:has-text('Deal')");
        for (int attempt = 0; attempt < 5; attempt++)
        {
            await chipBtn.ClickAsync();
            try
            {
                await dealBtn.WaitForAsync(new() { Timeout = 3000 });
                return; // Circuit is active, Deal button appeared
            }
            catch (TimeoutException)
            {
                // Circuit not ready yet, retry
            }
        }
    }

    [Fact]
    public async Task PlaceBetAndDeal_ShowsCards()
    {
        var email = $"game-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);

        // Wait for Blazor circuit and select chip (helper clicks chip + waits for Deal)
        await WaitForBlazorCircuit(page);

        // Deal button should be visible now — click it
        var dealBtn = page.Locator("button:has-text('Deal')");
        await dealBtn.ClickAsync();

        // Should show cards
        var cards = page.Locator(".playing-card");
        await cards.First.WaitForAsync(new() { Timeout = 5000 });
        var cardCount = await cards.CountAsync();
        Assert.True(cardCount >= 4); // At least 4 cards (2 player + 2 dealer)
    }

    [Fact]
    public async Task GamePage_ShowsActionButtons_DuringPlayerTurn()
    {
        var email = $"game-{Guid.NewGuid():N}@blackjack.com";
        var page = await _fixture.CreateAuthenticatedPageAsync(email);

        // Wait for Blazor circuit and select chip (helper clicks chip + waits for Deal)
        await WaitForBlazorCircuit(page);

        // Deal button should be visible now — click it
        var dealBtn = page.Locator("button:has-text('Deal')");
        await dealBtn.ClickAsync();

        // Wait for action buttons (may not appear if game resolved immediately)
        try
        {
            var actionButtons = page.Locator(".action-buttons .btn");
            await actionButtons.First.WaitForAsync(new() { Timeout = 3000 });

            // Should have at least Hit and Stand
            var hitButton = page.Locator("text=Hit");
            var standButton = page.Locator("text=Stand");
            Assert.True(await hitButton.IsVisibleAsync() || await standButton.IsVisibleAsync());
        }
        catch (TimeoutException)
        {
            // Game may have resolved immediately (natural blackjack)
            // That's OK, the result overlay should be visible
            var resultOverlay = page.Locator(".result-overlay.visible");
            Assert.True(await resultOverlay.IsVisibleAsync());
        }
    }
}
