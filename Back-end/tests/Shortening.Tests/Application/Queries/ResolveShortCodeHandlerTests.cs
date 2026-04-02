using App.Exceptions;
using FluentAssertions;
using MediatR;
using Moq;
using Shortening.Application.Contracts;
using Shortening.Application.IntegrationEvents;
using Shortening.Application.Queries.ResolveShortCode;
using Shortening.Domain;

namespace Shortening.Tests.Application.Queries;

public class ResolveShortCodeHandlerTests
{
    private readonly Mock<IShortenedUrlRepository> _repositoryMock;
    private readonly Mock<IPublisher> _publisherMock;
    private readonly ResolveShortCodeHandler _handler;

    public ResolveShortCodeHandlerTests()
    {
        _repositoryMock = new Mock<IShortenedUrlRepository>();
        _publisherMock = new Mock<IPublisher>();
        _handler = new ResolveShortCodeHandler(
            _repositoryMock.Object,
            _publisherMock.Object);
    }

    #region Successful Resolution

    [Fact]
    public async Task Handle_WithValidActiveShortCode_ShouldReturnOriginalUrl()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var originalUrl = "https://example.com/original-page";
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(
            shortCode,
            originalUrl,
            expiresAt: DateTime.UtcNow.AddHours(1));

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(originalUrl);
    }

    [Fact]
    public async Task Handle_WithValidShortCode_ShouldPublishClickEvent()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, "https://example.com");

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(
                It.Is<UrlClickedIntegrationEvent>(e =>
                    e.ShortCode == shortCode &&
                    e.OriginalUrl == shortenedUrl.OriginalUrl),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Not Found (Edge Cases)

    [Fact]
    public async Task Handle_WhenShortCodeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new ResolveShortCodeQuery("NotExist");

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");

        _publisherMock.Verify(
            x => x.Publish(It.IsAny<UrlClickedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyShortCode_ShouldThrowNotFoundException()
    {
        // Arrange - Edge case: empty string
        var query = new ResolveShortCodeQuery(string.Empty);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(string.Empty))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Expired URLs

    [Fact]
    public async Task Handle_WhenUrlStatusIsExpired_ShouldThrowException()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, "https://example.com");
        shortenedUrl.Expire();

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ExpiredUrlException>()
            .WithMessage("*expired*");

        _publisherMock.Verify(
            x => x.Publish(It.IsAny<UrlClickedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExpiryDateHasPassed_ShouldThrowException()
    {
        // Arrange - Edge case: TTL check catches expired URLs before background job updates status
        var shortCode = "Ab12Cd34";
        var query = new ResolveShortCodeQuery(shortCode);

        // Create with past expiry (simulating a URL where the background job hasn't run yet)
        var shortenedUrl = CreateTestShortenedUrlWithPastExpiry(shortCode, "https://example.com");

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ExpiredUrlException>()
            .WithMessage("*expired*");
    }

    #endregion

    #region Disabled URLs

    [Fact]
    public async Task Handle_WhenUrlIsDisabled_ShouldThrowException()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, "https://example.com");
        shortenedUrl.Disable();

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");

        _publisherMock.Verify(
            x => x.Publish(It.IsAny<UrlClickedIntegrationEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Publisher Failures

    [Fact]
    public async Task Handle_WhenPublisherFails_ShouldPropagateException()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, "https://example.com");

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<UrlClickedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Message bus unavailable"));

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Message bus unavailable*");
    }

    #endregion

    #region Security - Malicious Input

    [Theory]
    [InlineData("'; DROP TABLE--")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("../../../etc/passwd")]
    [InlineData("{{constructor.constructor('return this')()}}")]
    [InlineData("%00%00%00%00")]
    public async Task Handle_WithMaliciousShortCode_ShouldQuerySafelyAndReturnNotFound(string maliciousCode)
    {
        // Arrange
        var query = new ResolveShortCodeQuery(maliciousCode);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(maliciousCode))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(x => x.GetByShortCodeAsync(maliciousCode), Times.Once);
    }

    #endregion

    #region Edge Cases - URLs with Various Content

    [Fact]
    public async Task Handle_WithUrlContainingQueryParameters_ShouldReturnFullUrl()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var complexUrl = "https://example.com/page?param1=value1&param2=value2#section";
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, complexUrl);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(complexUrl);
    }

    [Fact]
    public async Task Handle_WithVeryLongOriginalUrl_ShouldReturnFullUrl()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var longUrl = "https://example.com/" + new string('a', 5000);
        var query = new ResolveShortCodeQuery(shortCode);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, longUrl);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(longUrl);
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToPublisher()
    {
        // Arrange
        var shortCode = "Ab12Cd34";
        var query = new ResolveShortCodeQuery(shortCode);
        var cts = new CancellationTokenSource();

        var shortenedUrl = CreateTestShortenedUrl(shortCode, "https://example.com");

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        await _handler.Handle(query, cts.Token);

        // Assert
        _publisherMock.Verify(
            x => x.Publish(It.IsAny<UrlClickedIntegrationEvent>(), cts.Token),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static ShortenedUrl CreateTestShortenedUrl(
        string shortCode, 
        string originalUrl, 
        DateTime? expiresAt = null)
    {
        return new ShortenedUrl(
            originalUrl: originalUrl,
            shortCode: new ShortCode(shortCode),
            expiresAt: expiresAt ?? DateTime.UtcNow.AddHours(1),
            userId: Guid.NewGuid());
    }

    private static ShortenedUrl CreateTestShortenedUrlWithPastExpiry(
        string shortCode, 
        string originalUrl)
    {
        // We need to create a URL that's active but has a past expiry
        // This simulates the race condition where background job hasn't run
        // Since we can't create with past expiry directly, we mock the repository
        // to return a URL with modified expiry time using reflection or different approach
        
        // For testing purposes, create with future expiry then modify 
        // In real scenario, this would be set by the database
        var url = new ShortenedUrl(
            originalUrl: originalUrl,
            shortCode: new ShortCode(shortCode),
            expiresAt: DateTime.UtcNow.AddHours(1),
            userId: Guid.NewGuid());

        // Use reflection to set expiry to past for testing
        var expiresAtProperty = typeof(ShortenedUrl).GetProperty("ExpiresAt");
        expiresAtProperty?.SetValue(url, DateTime.UtcNow.AddHours(-1));

        return url;
    }

    #endregion
}
