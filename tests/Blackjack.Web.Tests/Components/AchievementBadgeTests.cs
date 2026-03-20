using Bunit;
using Xunit;
using Blackjack.Domain.Models;
using Blackjack.Web.Components;

namespace Blackjack.Web.Tests.Components;

public class AchievementBadgeTests : BunitContext
{
    private static readonly Achievement TestAchievement = new("first_win", "First Win", "Win your first game", "🎯");

    [Fact]
    public void Earned_ShowsAchievementIcon()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, true));

        var icon = cut.Find(".achievement-icon");
        Assert.Equal("🎯", icon.TextContent);
    }

    [Fact]
    public void Locked_ShowsLockIcon()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, false));

        var icon = cut.Find(".achievement-icon");
        Assert.Equal("🔒", icon.TextContent);
    }

    [Fact]
    public void Earned_HasEarnedCssClass()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, true));

        var badge = cut.Find(".achievement-badge");
        Assert.Contains("earned", badge.ClassList);
    }

    [Fact]
    public void Locked_HasLockedCssClass()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, false));

        var badge = cut.Find(".achievement-badge");
        Assert.Contains("locked", badge.ClassList);
    }

    [Fact]
    public void ShowsAchievementName()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, true));

        var name = cut.Find(".achievement-name");
        Assert.Equal("First Win", name.TextContent);
    }

    [Fact]
    public void ShowsAchievementDescription()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, true));

        var desc = cut.Find(".achievement-desc");
        Assert.Equal("Win your first game", desc.TextContent);
    }

    [Fact]
    public void Earned_WithEarnedAt_ShowsDate()
    {
        var earnedAt = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, true)
            .Add(x => x.EarnedAt, earnedAt));

        var date = cut.Find(".achievement-date");
        Assert.Contains("Jun 15, 2024", date.TextContent);
    }

    [Fact]
    public void Earned_WithoutEarnedAt_DoesNotShowDate()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, true));

        Assert.Throws<ElementNotFoundException>(() => cut.Find(".achievement-date"));
    }

    [Fact]
    public void Locked_DoesNotShowDate()
    {
        var cut = Render<AchievementBadge>(p => p
            .Add(x => x.Achievement, TestAchievement)
            .Add(x => x.Earned, false)
            .Add(x => x.EarnedAt, DateTime.UtcNow));

        Assert.Throws<ElementNotFoundException>(() => cut.Find(".achievement-date"));
    }
}
