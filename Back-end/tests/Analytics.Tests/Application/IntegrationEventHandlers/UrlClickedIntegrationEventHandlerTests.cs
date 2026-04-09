using Analytics.Application.Contracts;
using Analytics.Application.IntegrationEventHandlers;
using Analytics.Domain;
using FluentAssertions;
using Moq;
using Shortening.Application.IntegrationEvents;

namespace Analytics.Tests.Application.IntegrationEventHandlers;

public class UrlClickedIntegrationEventHandlerTests
{
    private readonly Mock<IClickEventRepository> _repositoryMock;
    private readonly UrlClickedIntegrationEventHandler _handler;

    public UrlClickedIntegrationEventHandlerTests()
    {
        _repositoryMock = new Mock<IClickEventRepository>();
        _handler = new UrlClickedIntegrationEventHandler(_repositoryMock.Object);
    }

    // ─── Happy Path: Stats do not yet exist ──────────────────────────────────────

    [Fact]
    public async Task Handle_WhenStatsDoNotExist_ShouldCreateStatsAndAddClickEvent()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = new UrlClickedIntegrationEvent(
            shortenedUrlId, "abc123", "https://example.com", userId, "1.2.3.4", "Mozilla/5.0");

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync((ShortenedUrlStats?)null);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert: a new ShortenedUrlStats was created and persisted
        _repositoryMock.Verify(r => r.AddStatsAsync(It.Is<ShortenedUrlStats>(s =>
            s.Id == shortenedUrlId &&
            s.ShortCode == "abc123" &&
            s.OriginalUrl == "https://example.com" &&
            s.UserId == userId
        )), Times.Once);

        // Assert: a ClickEvent was persisted
        _repositoryMock.Verify(r => r.AddClickAsync(It.IsAny<ClickEvent>()), Times.Once);

        // Assert: changes were saved
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ─── Happy Path: Stats already exist ─────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenStatsAlreadyExist_ShouldNotCreateNewStats_AndShouldAddClickEvent()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();
        var existingStats = new ShortenedUrlStats(shortenedUrlId, "abc123", "https://example.com", Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync(existingStats);

        var notification = new UrlClickedIntegrationEvent(
            shortenedUrlId, "abc123", "https://example.com", existingStats.UserId, "10.0.0.1", "Chrome");

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert: AddStatsAsync was NOT called because stats already exist
        _repositoryMock.Verify(r => r.AddStatsAsync(It.IsAny<ShortenedUrlStats>()), Times.Never);

        // Assert: TotalClicks was incremented via RecordClick()
        existingStats.TotalClicks.Should().Be(1);

        // Assert: ClickEvent was persisted
        _repositoryMock.Verify(r => r.AddClickAsync(It.IsAny<ClickEvent>()), Times.Once);

        // Assert: changes were saved
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ─── Multiple clicks on same URL ─────────────────────────────────────────────

    [Fact]
    public async Task Handle_CalledMultipleTimes_ShouldAccumulateClickCount()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();
        var stats = new ShortenedUrlStats(shortenedUrlId, "test1", "https://test.com", null);

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync(stats);

        var notification = new UrlClickedIntegrationEvent(shortenedUrlId, "test1", "https://test.com", null, null, null);

        // Act
        await _handler.Handle(notification, CancellationToken.None);
        await _handler.Handle(notification, CancellationToken.None);
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        stats.TotalClicks.Should().Be(3);
        _repositoryMock.Verify(r => r.AddClickAsync(It.IsAny<ClickEvent>()), Times.Exactly(3));
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(3));
    }

    // ─── Metadata capture ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldPassIpAddressAndUserAgentToClickEvent()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();
        var stats = new ShortenedUrlStats(shortenedUrlId, "meta1", "https://meta.com", null);

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync(stats);

        const string expectedIp = "192.168.1.100";
        const string expectedAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";

        var notification = new UrlClickedIntegrationEvent(
            shortenedUrlId, "meta1", "https://meta.com", null, expectedIp, expectedAgent);

        ClickEvent? capturedEvent = null;
        _repositoryMock
            .Setup(r => r.AddClickAsync(It.IsAny<ClickEvent>()))
            .Callback<ClickEvent>(e => capturedEvent = e);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.IpAddress.Should().Be(expectedIp);
        capturedEvent.UserAgent.Should().Be(expectedAgent);
        capturedEvent.ShortenedUrlStatsId.Should().Be(shortenedUrlId);
    }

    // ─── Null metadata (anonymous click) ─────────────────────────────────────────

    [Fact]
    public async Task Handle_WithNullIpAndUserAgent_ShouldStoreNullsGracefully()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();
        var stats = new ShortenedUrlStats(shortenedUrlId, "anon1", "https://anon.com", null);

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync(stats);

        var notification = new UrlClickedIntegrationEvent(
            shortenedUrlId, "anon1", "https://anon.com", null, null, null);

        ClickEvent? capturedEvent = null;
        _repositoryMock
            .Setup(r => r.AddClickAsync(It.IsAny<ClickEvent>()))
            .Callback<ClickEvent>(e => capturedEvent = e);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert: null metadata accepted without exception
        capturedEvent.Should().NotBeNull();
        capturedEvent!.IpAddress.Should().BeNull();
        capturedEvent.UserAgent.Should().BeNull();
    }

    // ─── Anonymous URL (no userId) ────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldCreateStatsWithNullUserId()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync((ShortenedUrlStats?)null);

        var notification = new UrlClickedIntegrationEvent(
            shortenedUrlId, "nouser", "https://nouser.com", null, null, null);

        ShortenedUrlStats? capturedStats = null;
        _repositoryMock
            .Setup(r => r.AddStatsAsync(It.IsAny<ShortenedUrlStats>()))
            .Callback<ShortenedUrlStats>(s => capturedStats = s);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        capturedStats.Should().NotBeNull();
        capturedStats!.UserId.Should().BeNull();
    }

    // ─── Verify ClickedAtUtc matches event timestamp ──────────────────────────────

    [Fact]
    public async Task Handle_ShouldUseOccurredOnUtcAsClickedAtUtc()
    {
        // Arrange
        var shortenedUrlId = Guid.NewGuid();
        var stats = new ShortenedUrlStats(shortenedUrlId, "time1", "https://time.com", null);

        _repositoryMock
            .Setup(r => r.GetStatsByIdAsync(shortenedUrlId))
            .ReturnsAsync(stats);

        var notification = new UrlClickedIntegrationEvent(
            shortenedUrlId, "time1", "https://time.com", null, null, null);

        // The event captures OccurredOnUtc at construction time
        var expectedTime = notification.OccurredOnUtc;

        ClickEvent? capturedEvent = null;
        _repositoryMock
            .Setup(r => r.AddClickAsync(It.IsAny<ClickEvent>()))
            .Callback<ClickEvent>(e => capturedEvent = e);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        capturedEvent!.ClickedAtUtc.Should().Be(expectedTime);
    }
}
