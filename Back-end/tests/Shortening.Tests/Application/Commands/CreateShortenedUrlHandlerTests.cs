using FluentAssertions;
using Moq;
using Shortening.Application.Commands.CreateShortenedUrl;
using Shortening.Application.Contracts;
using Shortening.Domain;

namespace Shortening.Tests.Application.Commands;

public class CreateShortenedUrlHandlerTests
{
    private readonly Mock<IShortenedUrlRepository> _repositoryMock;
    private readonly Mock<IShortCodeGenerator> _shortCodeGeneratorMock;
    private readonly CreateShortenedUrlHandler _handler;

    public CreateShortenedUrlHandlerTests()
    {
        _repositoryMock = new Mock<IShortenedUrlRepository>();
        _shortCodeGeneratorMock = new Mock<IShortCodeGenerator>();
        _handler = new CreateShortenedUrlHandler(
            _repositoryMock.Object,
            _shortCodeGeneratorMock.Object);
    }

    #region Successful Creation

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndPersistShortenedUrl()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com/long-url",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShortenedUrl>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShortCode.Should().Be("Ab12Cd34");
        result.OriginalUrl.Should().Be(command.OriginalUrl);
        result.Status.Should().Be(UrlStatus.Active.ToString());

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<ShortenedUrl>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSetCorrectExpirationTime()
    {
        // Arrange
        var ttlMinutes = 30;
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: ttlMinutes,
            UserId: Guid.NewGuid());

        var beforeExecution = DateTime.UtcNow;

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterExecution = DateTime.UtcNow;

        // Assert
        result.ExpiresAt.Should().BeOnOrAfter(beforeExecution.AddMinutes(ttlMinutes));
        result.ExpiresAt.Should().BeOnOrBefore(afterExecution.AddMinutes(ttlMinutes));
    }

    [Fact]
    public async Task Handle_WithAnonymousUser_ShouldCreateUrlWithNullUserId()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 15,
            UserId: null);

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        ShortenedUrl? capturedUrl = null;
        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShortenedUrl>()))
            .Callback<ShortenedUrl>(url => capturedUrl = url)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUrl.Should().NotBeNull();
        capturedUrl!.UserId.Should().BeNull();
    }

    #endregion

    #region Repository Interaction

    [Fact]
    public async Task Handle_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.AddAsync(It.Is<ShortenedUrl>(
            url => url.OriginalUrl == command.OriginalUrl &&
                   url.ShortCode.Value == "Ab12Cd34")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShortenedUrl>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Database connection failed*");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception("Transaction failed"));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Transaction failed*");
    }

    #endregion

    #region Short Code Generator Interaction

    [Fact]
    public async Task Handle_ShouldUseGeneratedShortCode()
    {
        // Arrange
        var expectedCode = "XyZ12345";
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns(expectedCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShortCode.Should().Be(expectedCode);
        _shortCodeGeneratorMock.Verify(x => x.GenerateShortCode(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenGeneratorReturnsInvalidCode_ShouldThrowException()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("invalid"); // Too short, will fail ShortCode validation

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Edge Cases - TTL Values

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(180)]
    public async Task Handle_WithAllowedTtlValues_ShouldSucceed(int ttlMinutes)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: ttlMinutes,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // The expiration should be approximately ttlMinutes from now
        result.ExpiresAt.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(ttlMinutes),
            TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Cancellation Token

    [Fact]
    public async Task Handle_WithCancelledToken_ShouldStillComplete()
    {
        // Note: Current implementation doesn't check cancellation token
        // This test documents the current behavior
        
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        _shortCodeGeneratorMock
            .Setup(x => x.GenerateShortCode())
            .Returns("Ab12Cd34");

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _handler.Handle(command, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion
}
