using App.Exceptions;
using FluentAssertions;
using Moq;
using Shortening.Application.Commands.DeleteShortenedUrl;
using Shortening.Application.Contracts;
using Shortening.Domain;

namespace Shortening.Tests.Application.Commands;

public class DeleteShortenedUrlHandlerTests
{
    private readonly Mock<IShortenedUrlRepository> _repositoryMock;
    private readonly DeleteShortenedUrlHandler _handler;

    public DeleteShortenedUrlHandlerTests()
    {
        _repositoryMock = new Mock<IShortenedUrlRepository>();
        _handler = new DeleteShortenedUrlHandler(_repositoryMock.Object);
    }

    #region Successful Deletion

    [Fact]
    public async Task Handle_WithValidOwner_ShouldDeleteUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, userId);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, userId);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        _repositoryMock
            .Setup(x => x.DeleteAsync(shortCode))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.DeleteAsync(shortCode), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region Not Found (Edge Cases)

    [Fact]
    public async Task Handle_WhenShortCodeNotFound_ShouldThrowException()
    {
        // Arrange
        var command = new DeleteShortenedUrlCommand("NotExist", Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");

        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyShortCode_ShouldCallRepositoryWithEmptyString()
    {
        // Arrange - Edge case: empty short code passed
        var command = new DeleteShortenedUrlCommand(string.Empty, Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(string.Empty))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Authorization (Security)

    [Fact]
    public async Task Handle_WhenUserIsNotOwner_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, attackerId);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, ownerId);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*permission*");

        _repositoryMock.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ShouldThrowUnauthorizedException()
    {
        // Arrange - Security: anonymous user trying to delete
        var ownerId = Guid.NewGuid();
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, null);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, ownerId);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public async Task Handle_AnonymousUrlWithAnonymousUser_ShouldStillThrowUnauthorized()
    {
        // Arrange - Edge case: URL has no owner, anonymous trying to delete
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, null);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, null);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*permission*");
    }

    [Fact]
    public async Task Handle_AnonymousUrlWithAuthenticatedUser_ShouldThrowUnauthorized()
    {
        // Arrange - Security: authenticated user trying to delete anonymous URL
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, Guid.NewGuid());

        var shortenedUrl = CreateTestShortenedUrl(shortCode, null); // Anonymous URL

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("*permission*");
    }

    #endregion

    #region Repository Failures

    [Fact]
    public async Task Handle_WhenDeleteAsyncFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, userId);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, userId);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        _repositoryMock
            .Setup(x => x.DeleteAsync(shortCode))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Database error*");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shortCode = "Ab12Cd34";
        var command = new DeleteShortenedUrlCommand(shortCode, userId);

        var shortenedUrl = CreateTestShortenedUrl(shortCode, userId);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        _repositoryMock
            .Setup(x => x.DeleteAsync(shortCode))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception("Transaction rollback"));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*Transaction rollback*");
    }

    #endregion

    #region Security - Malicious Input

    [Theory]
    [InlineData("'; DROP TABLE ShortenedUrls; --")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("../../../etc/passwd")]
    [InlineData("{{7*7}}")]
    [InlineData("%00nullbyte")]
    public async Task Handle_WithMaliciousShortCode_ShouldQueryRepositorySafely(string maliciousCode)
    {
        // Arrange - These should just result in "not found" if repository handles safely
        var command = new DeleteShortenedUrlCommand(maliciousCode, Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(maliciousCode))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");

        // Verify the repository was called with the exact input
        _repositoryMock.Verify(x => x.GetByShortCodeAsync(maliciousCode), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static ShortenedUrl CreateTestShortenedUrl(string shortCode, Guid? userId)
    {
        return new ShortenedUrl(
            originalUrl: "https://example.com",
            shortCode: new ShortCode(shortCode),
            expiresAt: DateTime.UtcNow.AddHours(1),
            userId: userId);
    }

    #endregion
}
