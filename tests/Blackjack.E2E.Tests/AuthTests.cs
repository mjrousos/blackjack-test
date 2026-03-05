namespace Blackjack.E2E.Tests;

using Blackjack.E2E.Tests.Infrastructure;
using Microsoft.Playwright;
using Xunit;

[Collection("Playwright")]
[Trait("Category", "E2E")]
public class AuthTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public AuthTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task UnauthenticatedUser_RedirectsToLogin()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);

        // Should redirect to login
        Assert.Contains("Account/Login", page.Url);
    }

    [Fact]
    public async Task LoginPage_HasRequiredFields()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Login");

        var emailInput = page.Locator("input[name='Email']");
        var passwordInput = page.Locator("input[name='Password']");
        var submitButton = page.Locator("button[type='submit']");

        Assert.True(await emailInput.IsVisibleAsync());
        Assert.True(await passwordInput.IsVisibleAsync());
        Assert.True(await submitButton.IsVisibleAsync());
    }

    [Fact]
    public async Task RegisterPage_HasRequiredFields()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Register");

        var emailInput = page.Locator("input[name='Email']");
        var passwordInput = page.Locator("input[name='Password']");
        var confirmInput = page.Locator("input[name='ConfirmPassword']");
        var submitButton = page.Locator("button[type='submit']");

        Assert.True(await emailInput.IsVisibleAsync());
        Assert.True(await passwordInput.IsVisibleAsync());
        Assert.True(await confirmInput.IsVisibleAsync());
        Assert.True(await submitButton.IsVisibleAsync());
    }

    [Fact]
    public async Task Register_WithValidCredentials_RedirectsToHome()
    {
        var page = await _fixture.CreatePageAsync();
        var email = $"test-{Guid.NewGuid():N}@blackjack.com";

        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Register");
        await page.FillAsync("input[name='Email']", email);
        await page.FillAsync("input[name='Password']", "TestPass123!");
        await page.FillAsync("input[name='ConfirmPassword']", "TestPass123!");
        await page.ClickAsync("button[type='submit']");

        await page.WaitForURLAsync($"{_fixture.BaseUrl}/");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShowsError()
    {
        var page = await _fixture.CreatePageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/Account/Login");
        await page.FillAsync("input[name='Email']", "nonexistent@blackjack.com");
        await page.FillAsync("input[name='Password']", "WrongPassword1!");
        await page.ClickAsync("button[type='submit']");

        // Should stay on login page with error
        Assert.Contains("Account/Login", page.Url);
    }
}
