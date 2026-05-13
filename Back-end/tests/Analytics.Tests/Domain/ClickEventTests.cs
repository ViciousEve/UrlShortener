using Analytics.Domain;
using FluentAssertions;

namespace Analytics.Tests.Domain;

public class ClickEventTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesClickEvent()
    {
        // Arrange
        var shortenedUrlStatsId = Guid.NewGuid();
        var clickedAtUtc = DateTime.UtcNow;
        var ipAddress = "[IP_ADDRESS]";
        var userAgent = "Mozilla/5.0";

        // Act
        var clickEvent = new ClickEvent(shortenedUrlStatsId, clickedAtUtc, ipAddress, userAgent);

        // Assert
        clickEvent.Id.Should().NotBeEmpty();
        clickEvent.ShortenedUrlStatsId.Should().Be(shortenedUrlStatsId);
        clickEvent.ClickedAtUtc.Should().Be(clickedAtUtc);
        clickEvent.IpAddress.Should().Be(ipAddress);
        clickEvent.UserAgent.Should().Be(userAgent);
    }

    [Fact]
    public void Constructor_WithEmptyIpAddress_CreatesClickEventWithEmptyIpAddress()
    {
        // Arrange
        var shortenedUrlStatsId = Guid.NewGuid();
        var clickedAtUtc = DateTime.UtcNow;
        var ipAddress = "";
        var userAgent = "Mozilla/5.0";

        // Act
        var clickEvent = new ClickEvent(shortenedUrlStatsId, clickedAtUtc, ipAddress, userAgent);

        // Assert
        clickEvent.Id.Should().NotBeEmpty();
        clickEvent.ShortenedUrlStatsId.Should().Be(shortenedUrlStatsId);
        clickEvent.ClickedAtUtc.Should().Be(clickedAtUtc);
        clickEvent.IpAddress.Should().Be(ipAddress);
        clickEvent.UserAgent.Should().Be(userAgent);
    }

    [Fact]
    public void Constructor_WithEmptyUserAgent_CreatesClickEventWithEmptyUserAgent()
    {
        // Arrange
        var shortenedUrlStatsId = Guid.NewGuid();
        var clickedAtUtc = DateTime.UtcNow;
        var ipAddress = "[IP_ADDRESS]";
        var userAgent = "";

        // Act
        var clickEvent = new ClickEvent(shortenedUrlStatsId, clickedAtUtc, ipAddress, userAgent);

        // Assert
        clickEvent.Id.Should().NotBeEmpty();
        clickEvent.ShortenedUrlStatsId.Should().Be(shortenedUrlStatsId);
        clickEvent.ClickedAtUtc.Should().Be(clickedAtUtc);
        clickEvent.IpAddress.Should().Be(ipAddress);
        clickEvent.UserAgent.Should().Be(userAgent);
    }
}