namespace Blackjack.E2E.Tests.Infrastructure;

using Microsoft.Playwright;
using Xunit;

public class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    // Base URL — configurable via environment variable
    public string BaseUrl => Environment.GetEnvironmentVariable("APP_URL") ?? "http://localhost:8080";

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    public async Task<IPage> CreatePageAsync()
    {
        var context = await Browser.NewContextAsync();
        return await context.NewPageAsync();
    }

    public async Task<IPage> CreateAuthenticatedPageAsync(string email = "test@blackjack.com", string password = "TestPass123!")
    {
        var page = await CreatePageAsync();

        // Register if needed, then login
        await page.GotoAsync($"{BaseUrl}/Account/Register");
        await page.FillAsync("input[name='Email']", email);
        await page.FillAsync("input[name='Password']", password);
        await page.FillAsync("input[name='ConfirmPassword']", password);
        await page.ClickAsync("button[type='submit']");

        // If registration fails (user exists), try login
        if (page.Url.Contains("Register"))
        {
            await page.GotoAsync($"{BaseUrl}/Account/Login");
            await page.FillAsync("input[name='Email']", email);
            await page.FillAsync("input[name='Password']", password);
            await page.ClickAsync("button[type='submit']");
        }

        // Wait for redirect to home
        await page.WaitForURLAsync($"{BaseUrl}/");
        return page;
    }
}
