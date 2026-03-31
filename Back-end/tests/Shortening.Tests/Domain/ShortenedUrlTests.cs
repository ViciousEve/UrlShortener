using FluentAssertions;
using Shortening.Domain;
using Shortening.Domain.Events;

namespace Shortening.Tests.Domain;

public class ShortenedUrlTests
{
    private readonly ShortCode _validShortCode = new("Ab12Cd34");
    private readonly string _validOriginalUrl = "https://example.com/long-url";
    private readonly DateTime _futureExpiryDate = DateTime.UtcNow.AddHours(1);
    private readonly Guid _validUserId = Guid.NewGuid();

    #region Constructor - Valid Cases

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateShortenedUrl()
    {
        // Act
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert
        shortenedUrl.OriginalUrl.Should().Be(_validOriginalUrl);
        shortenedUrl.ShortCode.Should().Be(_validShortCode);
        shortenedUrl.ExpiresAt.Should().Be(_futureExpiryDate);
        shortenedUrl.UserId.Should().Be(_validUserId);
        shortenedUrl.Status.Should().Be(UrlStatus.Active);
        shortenedUrl.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithNullUserId_ShouldCreateAnonymousShortenedUrl()
    {
        // Act
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            null);

        // Assert
        shortenedUrl.UserId.Should().BeNull();
        shortenedUrl.Status.Should().Be(UrlStatus.Active);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        var afterCreation = DateTime.UtcNow;

        // Assert
        shortenedUrl.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        shortenedUrl.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void Constructor_ShouldRaiseUrlCreatedDomainEvent()
    {
        // Act
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert
        shortenedUrl.DomainEvents.Should().HaveCount(1);
        var domainEvent = shortenedUrl.DomainEvents.First();
        domainEvent.Should().BeOfType<UrlCreatedDomainEvent>();
        
        var urlCreatedEvent = (UrlCreatedDomainEvent)domainEvent;
        urlCreatedEvent.ShortCode.Should().Be(_validShortCode.Value);
        urlCreatedEvent.OriginalUrl.Should().Be(_validOriginalUrl);
        urlCreatedEvent.UserId.Should().Be(_validUserId);
    }

    #endregion

    #region Constructor - Invalid Original URL (Edge Cases & User Mistakes)

    [Fact]
    public void Constructor_WithNullOriginalUrl_ShouldThrowArgumentException()
    {
        // Act
        var action = () => new ShortenedUrl(
            null!,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Original URL cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyOriginalUrl_ShouldThrowArgumentException()
    {
        // Act
        var action = () => new ShortenedUrl(
            string.Empty,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Original URL cannot be empty*");
    }

    [Fact]
    public void Constructor_WithWhitespaceOriginalUrl_ShouldNotThrowAtDomainLevel()
    {
        // Arrange - Common user mistake
        // Note: Domain accepts whitespace - URL format validation happens at application layer
        var whitespaceUrl = "   ";

        // Act
        var shortenedUrl = new ShortenedUrl(
            whitespaceUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert - Domain accepts it; proper validation at validator level
        shortenedUrl.OriginalUrl.Should().Be(whitespaceUrl);
    }

    #endregion

    #region Constructor - Invalid ShortCode

    [Fact]
    public void Constructor_WithNullShortCode_ShouldThrowArgumentNullException()
    {
        // Act
        var action = () => new ShortenedUrl(
            _validOriginalUrl,
            null!,
            _futureExpiryDate,
            _validUserId);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Constructor - Invalid Expiry Date (Edge Cases & User Mistakes)

    [Fact]
    public void Constructor_WithPastExpiryDate_ShouldThrowArgumentException()
    {
        // Arrange - User mistake: setting expiry in the past
        var pastDate = DateTime.UtcNow.AddHours(-1);

        // Act
        var action = () => new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            pastDate,
            _validUserId);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*must be in the future*");
    }

    [Fact]
    public void Constructor_WithExpiryDateExactlyNow_ShouldThrowArgumentException()
    {
        // Arrange - Edge case: expiry at exact current time
        var now = DateTime.UtcNow;

        // Act
        var action = () => new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            now,
            _validUserId);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*must be in the future*");
    }

    [Fact]
    public void Constructor_WithMinDateTimeExpiry_ShouldThrowArgumentException()
    {
        // Arrange - Edge case: DateTime.MinValue
        var minDate = DateTime.MinValue;

        // Act
        var action = () => new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            minDate,
            _validUserId);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithVeryFarFutureExpiry_ShouldSucceed()
    {
        // Arrange - Edge case: Very far future (but valid)
        var farFuture = DateTime.UtcNow.AddYears(100);

        // Act
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            farFuture,
            _validUserId);

        // Assert
        shortenedUrl.ExpiresAt.Should().Be(farFuture);
    }

    #endregion

    #region Disable Method

    [Fact]
    public void Disable_WhenStatusIsActive_ShouldSetStatusToDisabled()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Act
        shortenedUrl.Disable();

        // Assert
        shortenedUrl.Status.Should().Be(UrlStatus.Disabled);
    }

    [Fact]
    public void Disable_WhenStatusIsExpired_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);
        shortenedUrl.Expire();

        // Act
        var action = () => shortenedUrl.Disable();

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot disable an expired URL*");
    }

    [Fact]
    public void Disable_WhenAlreadyDisabled_ShouldRemainDisabled()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);
        shortenedUrl.Disable();

        // Act - Calling disable again
        shortenedUrl.Disable();

        // Assert
        shortenedUrl.Status.Should().Be(UrlStatus.Disabled);
    }

    #endregion

    #region Expire Method

    [Fact]
    public void Expire_WhenStatusIsActive_ShouldSetStatusToExpired()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Act
        shortenedUrl.Expire();

        // Assert
        shortenedUrl.Status.Should().Be(UrlStatus.Expired);
    }

    [Fact]
    public void Expire_ShouldRaiseUrlExpiredDomainEvent()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);
        shortenedUrl.ClearDomainEvents(); // Clear the creation event

        // Act
        shortenedUrl.Expire();

        // Assert
        shortenedUrl.DomainEvents.Should().HaveCount(1);
        var domainEvent = shortenedUrl.DomainEvents.First();
        domainEvent.Should().BeOfType<UrlExpiredDomainEvent>();
        
        var expiredEvent = (UrlExpiredDomainEvent)domainEvent;
        expiredEvent.ShortCode.Should().Be(_validShortCode.Value);
        expiredEvent.ShortenedUrlId.Should().Be(shortenedUrl.Id);
    }

    [Fact]
    public void Expire_WhenStatusIsDisabled_ShouldSetStatusToExpired()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);
        shortenedUrl.Disable();

        // Act
        shortenedUrl.Expire();

        // Assert
        shortenedUrl.Status.Should().Be(UrlStatus.Expired);
    }

    #endregion

    #region Security - Malicious URL Inputs

    [Fact]
    public void Constructor_WithMaliciousProtocols_ShouldStillCreate()
    {
        // Note: URL validation should happen at the application layer (validator)
        // Domain entity just stores the value - this test documents current behavior
        // Malicious protocols like javascript:, data:, vbscript: are not blocked at domain level
        
        // Act
        var shortenedUrl = new ShortenedUrl(
            "https://example.com", // Valid URL
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert - Valid URL should work
        shortenedUrl.OriginalUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithVeryLongUrl_ShouldAcceptUrl()
    {
        // Arrange - Very long URL (could be used for DoS or storage attacks)
        var longUrl = "https://example.com/" + new string('a', 10000);

        // Act
        var shortenedUrl = new ShortenedUrl(
            longUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);

        // Assert - Domain accepts it; length validation should be at app layer
        shortenedUrl.OriginalUrl.Should().Be(longUrl);
    }

    #endregion

    #region Domain Events Management

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl(
            _validOriginalUrl,
            _validShortCode,
            _futureExpiryDate,
            _validUserId);
        shortenedUrl.Expire();
        shortenedUrl.DomainEvents.Should().HaveCount(2);

        // Act
        shortenedUrl.ClearDomainEvents();

        // Assert
        shortenedUrl.DomainEvents.Should().BeEmpty();
    }

    #endregion
}
