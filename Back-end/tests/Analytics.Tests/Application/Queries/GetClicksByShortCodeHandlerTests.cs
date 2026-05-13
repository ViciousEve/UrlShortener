using Analytics.Application.Contracts;
using Analytics.Application.DTOs;
using Analytics.Application.Queries.GetClicksByShortCode;
using Analytics.Domain;
using FluentAssertions;
using Moq;

namespace Analytics.Tests.Application.Queries;

public class GetClicksByShortCodeHandlerTests
{
    private readonly Mock<IClickEventRepository> _repositoryMock;
    private readonly GetClicksByShortCodeHandler _handler;

    // Shared test data
    private static readonly Guid StatsId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private const string ShortCode = "abc123";
    private const string OriginalUrl = "https://example.com";

    public GetClicksByShortCodeHandlerTests()
    {
        _repositoryMock = new Mock<IClickEventRepository>();
        _handler = new GetClicksByShortCodeHandler(_repositoryMock.Object);
    }

    // ─── Helper: build a ClickEvent with a populated navigation property ──────────
    // GetClicksByShortCodeHandler maps from c.ShortenedUrlStats.ShortCode etc.,
    // so the navigation property must be set for the projection to work.
    private static ClickEvent BuildClickEvent(Guid statsId, string shortCode, string originalUrl, Guid? userId,
        string? ipAddress = null, string? userAgent = null)
    {
        var stats = new ShortenedUrlStats(statsId, shortCode, originalUrl, userId);
        // ClickEvent's navigation property is private-set; we create it normally
        // and rely on the public nav property being set by the repository (EF Core does this).
        // In tests we call the constructor directly — ShortenedUrlStats is stored separately.
        var clickEvent = new ClickEvent(statsId, DateTime.UtcNow, ipAddress, userAgent);
        // Inject the navigation property via reflection so the handler projection works.
        typeof(ClickEvent)
            .GetProperty(nameof(ClickEvent.ShortenedUrlStats))!
            .SetValue(clickEvent, stats);
        return clickEvent;
    }

    // ─── Happy Path: returns mapped DTOs ─────────────────────────────────────────

    [Fact]
    public async Task Handle_WithExistingShortCode_ShouldReturnMappedClickEventResponses()
    {
        // Arrange
        var click1 = BuildClickEvent(StatsId, ShortCode, OriginalUrl, UserId, "1.1.1.1", "Chrome");
        var click2 = BuildClickEvent(StatsId, ShortCode, OriginalUrl, UserId, "2.2.2.2", "Firefox");

        _repositoryMock
            .Setup(r => r.GetClicksByShortCodeAsync(ShortCode))
            .ReturnsAsync(new List<ClickEvent> { click1, click2 });

        var query = new GetClicksByShortCodeQuery(ShortCode);

        // Act
        var result = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(2);

        result[0].ShortCode.Should().Be(ShortCode);
        result[0].OriginalUrl.Should().Be(OriginalUrl);
        result[0].UserId.Should().Be(UserId);
        result[0].ShortenedUrlId.Should().Be(StatsId);

        result[1].ShortCode.Should().Be(ShortCode);
    }

    // ─── Empty result ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithShortCodeThatHasNoClicks_ShouldReturnEmptyCollection()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetClicksByShortCodeAsync("notfound"))
            .ReturnsAsync(Enumerable.Empty<ClickEvent>());

        var query = new GetClicksByShortCodeQuery("notfound");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    // ─── Repository called with correct short code ────────────────────────────────

    [Fact]
    public async Task Handle_ShouldInvokeRepositoryWithExactShortCode()
    {
        // Arrange
        const string specificCode = "xyz789";

        _repositoryMock
            .Setup(r => r.GetClicksByShortCodeAsync(specificCode))
            .ReturnsAsync(Enumerable.Empty<ClickEvent>());

        var query = new GetClicksByShortCodeQuery(specificCode);

        // Act
        _ = await _handler.Handle(query, CancellationToken.None);

        // Assert: repository was called once with exactly the right code
        _repositoryMock.Verify(
            r => r.GetClicksByShortCodeAsync(specificCode),
            Times.Once);
    }

    // ─── DTO field mapping ────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_ShouldMapAllClickEventFieldsCorrectly()
    {
        // Arrange
        var statsId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var clickedAt = new DateTime(2025, 6, 15, 10, 30, 0, DateTimeKind.Utc);

        var stats = new ShortenedUrlStats(statsId, "maptest", "https://map.com", userId);
        var clickEvent = new ClickEvent(statsId, clickedAt, "9.9.9.9", "Safari");
        typeof(ClickEvent)
            .GetProperty(nameof(ClickEvent.ShortenedUrlStats))!
            .SetValue(clickEvent, stats);

        _repositoryMock
            .Setup(r => r.GetClicksByShortCodeAsync("maptest"))
            .ReturnsAsync(new List<ClickEvent> { clickEvent });

        var query = new GetClicksByShortCodeQuery("maptest");

        // Act
        var results = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert: every field maps correctly
        var dto = results.Single();
        dto.Id.Should().Be(clickEvent.Id);
        dto.ShortenedUrlId.Should().Be(statsId);
        dto.ShortCode.Should().Be("maptest");
        dto.OriginalUrl.Should().Be("https://map.com");
        dto.UserId.Should().Be(userId);
        dto.ClickedAtUtc.Should().Be(clickedAt);
    }

    // ─── Null UserId (anonymous URL) ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldReturnResponseWithNullUserId()
    {
        // Arrange
        var statsId = Guid.NewGuid();
        var clickEvent = BuildClickEvent(statsId, "anon2", "https://anon2.com", null);

        _repositoryMock
            .Setup(r => r.GetClicksByShortCodeAsync("anon2"))
            .ReturnsAsync(new List<ClickEvent> { clickEvent });

        var query = new GetClicksByShortCodeQuery("anon2");

        // Act
        var results = (await _handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        results.Single().UserId.Should().BeNull();
    }

    // ─── Return type is IEnumerable (lazy-safe) ───────────────────────────────────

    [Fact]
    public async Task Handle_ResultShouldBeEnumerableOfClickEventResponse()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetClicksByShortCodeAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<ClickEvent>());

        var query = new GetClicksByShortCodeQuery("any");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeAssignableTo<IEnumerable<ClickEventResponse>>();
    }
}
