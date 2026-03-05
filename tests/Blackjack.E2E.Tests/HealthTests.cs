namespace Blackjack.E2E.Tests;

using Blackjack.E2E.Tests.Infrastructure;
using Xunit;

[Collection("Playwright")]
[Trait("Category", "E2E")]
public class HealthTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public HealthTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var page = await _fixture.CreatePageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/health");

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
    }
}
