using Analytics.Domain;
using FluentAssertions;

namespace Analytics.Tests.Domain;

public class ShortenedUrlStatsTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesShortenedUrlStats()
    {
        // Arrange
        var Id = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;
        var shortCode = "shortCode";
        var originalUrl = "originalUrl";
        var userId = Guid.NewGuid();

        // Act
        var stats = new ShortenedUrlStats(Id, shortCode, originalUrl, userId);

        // Assert
        stats.Id.Should().Be(Id);
        stats.ShortCode.Should().Be(shortCode);
        stats.OriginalUrl.Should().Be(originalUrl);
        stats.UserId.Should().Be(userId);
        stats.TotalClicks.Should().Be(0);
        stats.LastClickedAtUtc.Should().BeNull();
    }

    [Fact]
    public void RecordClick_WithValidClickEvent_UpdatesStats()
    {
        // Arrange
        var Id = Guid.NewGuid();
        var shortCode = "shortCode";
        var originalUrl = "originalUrl";
        var userId = Guid.NewGuid();
        var stats = new ShortenedUrlStats(Id, shortCode, originalUrl, userId);

        // Act
        stats.RecordClick();

        // Assert
        stats.TotalClicks.Should().Be(1);
        stats.LastClickedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyShortCode_ThrowsArgumentException()
    {
        // Arrange
        var Id = Guid.NewGuid();
        var shortCode = "";
        var originalUrl = "originalUrl";
        var userId = Guid.NewGuid();

        // Act
        Action act = () => new ShortenedUrlStats(Id, shortCode, originalUrl, userId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Short code cannot be empty (Parameter 'shortCode')");
    }

    [Fact]
    public void Constructor_WithEmptyOriginalUrl_ThrowsArgumentException()
    {
        // Arrange
        var Id = Guid.NewGuid();
        var shortCode = "shortCode";
        var originalUrl = "";
        var userId = Guid.NewGuid();

        // Act
        Action act = () => new ShortenedUrlStats(Id, shortCode, originalUrl, userId);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Original URL cannot be empty (Parameter 'originalUrl')");
    }
}