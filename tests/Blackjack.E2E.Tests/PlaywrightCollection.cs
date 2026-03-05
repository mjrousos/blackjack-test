namespace Blackjack.E2E.Tests;

using Blackjack.E2E.Tests.Infrastructure;
using Xunit;

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}
