using FluentAssertions;
using Moq;
using Shortening.Application.Contracts;
using Shortening.Application.Queries.GetUserUrls;
using Shortening.Domain;

namespace Shortening.Tests.Application.Queries;

public class GetUserUrlsHandlerTests
{
    private readonly Mock<IShortenedUrlRepository> _repositoryMock;
    private readonly GetUserUrlsHandler _handler;

    public GetUserUrlsHandlerTests()
    {
        _repositoryMock = new Mock<IShortenedUrlRepository>();
        _handler = new GetUserUrlsHandler(_repositoryMock.Object);
    }

    #region Successful Retrieval

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnUserUrls()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        var urls = new List<ShortenedUrl>
        {
            CreateTestShortenedUrl("Ab12Cd34", "https://example.com/1", userId),
            CreateTestShortenedUrl("Xy98Zw76", "https://example.com/2", userId)
        };

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(urls);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.ShortCode == "Ab12Cd34");
        result.Should().Contain(r => r.ShortCode == "Xy98Zw76");
    }

    [Fact]
    public async Task Handle_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);
        var originalUrl = "https://example.com/test";

        var shortenedUrl = CreateTestShortenedUrl("Ab12Cd34", originalUrl, userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<ShortenedUrl> { shortenedUrl });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var urlResponse = result.First();
        urlResponse.ShortCode.Should().Be(shortenedUrl.ShortCode.Value);
        urlResponse.OriginalUrl.Should().Be(originalUrl);
        urlResponse.Status.Should().Be(UrlStatus.Active.ToString());
        urlResponse.CreatedAt.Should().BeCloseTo(shortenedUrl.CreatedAt, TimeSpan.FromSeconds(1));
        urlResponse.ExpiresAt.Should().Be(shortenedUrl.ExpiresAt);
    }

    #endregion

    #region Empty Results (Edge Cases)

    [Fact]
    public async Task Handle_WhenUserHasNoUrls_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<ShortenedUrl>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsNull_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((IEnumerable<ShortenedUrl>?)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNewUserGuid_ShouldReturnEmptyCollection()
    {
        // Arrange - Edge case: brand new user with no URLs
        var newUserId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(newUserId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(newUserId))
            .ReturnsAsync(new List<ShortenedUrl>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetByUserIdAsync(newUserId), Times.Once);
    }

    #endregion

    #region Various URL States

    [Fact]
    public async Task Handle_ShouldReturnUrlsInAllStates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        var activeUrl = CreateTestShortenedUrl("Active01", "https://example.com/1", userId);
        var disabledUrl = CreateTestShortenedUrl("Disabled", "https://example.com/2", userId);
        disabledUrl.Disable();
        var expiredUrl = CreateTestShortenedUrl("Expired1", "https://example.com/3", userId);
        expiredUrl.Expire();

        var urls = new List<ShortenedUrl> { activeUrl, disabledUrl, expiredUrl };

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(urls);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Status == UrlStatus.Active.ToString());
        result.Should().Contain(r => r.Status == UrlStatus.Disabled.ToString());
        result.Should().Contain(r => r.Status == UrlStatus.Expired.ToString());
    }

    #endregion

    #region Large Result Sets (Performance Edge Cases)

    [Fact]
    public async Task Handle_WithManyUrls_ShouldReturnAllUrls()
    {
        // Arrange - Edge case: user with many URLs
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        var urls = Enumerable.Range(1, 100)
            .Select(i => CreateTestShortenedUrl($"Code{i:D4}", $"https://example.com/{i}", userId))
            .ToList();

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(urls);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(100);
    }

    #endregion

    #region Security

    [Fact]
    public async Task Handle_ShouldOnlyQueryForSpecificUserId()
    {
        // Arrange - Security: ensure handler only queries for the specified user
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<ShortenedUrl>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert - Verify the exact userId was used
        _repositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
        _repositoryMock.Verify(x => x.GetByUserIdAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldStillQueryRepository()
    {
        // Arrange - Edge case: Guid.Empty (could indicate a bug in calling code)
        var emptyGuid = Guid.Empty;
        var query = new GetUserUrlsQuery(emptyGuid);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(emptyGuid))
            .ReturnsAsync(new List<ShortenedUrl>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetByUserIdAsync(emptyGuid), Times.Once);
    }

    #endregion

    #region Repository Failures

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ThrowsAsync(new Exception("Database connection lost"));

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Database connection lost*");
    }

    [Fact]
    public async Task Handle_WhenRepositoryTimesOut_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserUrlsQuery(userId);

        _repositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ThrowsAsync(new TimeoutException("Query timed out"));

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<TimeoutException>();
    }

    #endregion

    #region Helper Methods

    private static ShortenedUrl CreateTestShortenedUrl(string shortCode, string originalUrl, Guid userId)
    {
        return new ShortenedUrl(
            originalUrl: originalUrl,
            shortCode: new ShortCode(shortCode),
            expiresAt: DateTime.UtcNow.AddHours(1),
            userId: userId);
    }

    #endregion
}
