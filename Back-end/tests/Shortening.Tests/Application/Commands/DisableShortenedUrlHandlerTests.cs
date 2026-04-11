using App.Exceptions;
using FluentAssertions;
using Moq;
using Shortening.Application.Commands.DisableShortenedUrl;
using Shortening.Application.Contracts;
using Shortening.Domain;

namespace Shortening.Tests.Application.Commands;

public class DisableShortenedUrlHandlerTests
{
    private readonly Mock<IShortenedUrlRepository> _repoMock;
    private readonly DisableShortenedUrlCommandHandler _handler;

    private static readonly Guid OwnerId   = Guid.NewGuid();
    private static readonly Guid StrangerId = Guid.NewGuid();

    public DisableShortenedUrlHandlerTests()
    {
        _repoMock = new Mock<IShortenedUrlRepository>();
        _handler  = new DisableShortenedUrlCommandHandler(_repoMock.Object);
    }

    // ─── Happy Path ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenOwnerDisablesOwnUrl_ShouldSetStatusDisabled()
    {
        // Arrange
        var url = BuildActiveUrl(OwnerId);
        SetupRepo("abc123", url);

        var command = new DisableShortenedUrlCommand("abc123", OwnerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: entity state changed
        url.Status.Should().Be(UrlStatus.Disabled);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOwnerDisablesOwnUrl_ShouldRaiseDomainEvent()
    {
        // Arrange
        var url = BuildActiveUrl(OwnerId);
        SetupRepo("abc123", url);

        // Act
        await _handler.Handle(new DisableShortenedUrlCommand("abc123", OwnerId), CancellationToken.None);

        // Assert: Disable() sets status (domain event is raised inside entity)
        url.Status.Should().Be(UrlStatus.Disabled);
    }

    // ─── Short code not found ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenShortCodeNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByShortCodeAsync(It.IsAny<string>()))
                 .ReturnsAsync((ShortenedUrl?)null);

        var command = new DisableShortenedUrlCommand("missing", OwnerId);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Url not found!");
    }

    [Fact]
    public async Task Handle_WhenShortCodeNotFound_ShouldNotCallSaveChanges()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByShortCodeAsync(It.IsAny<string>()))
                 .ReturnsAsync((ShortenedUrl?)null);

        // Act
        try { await _handler.Handle(new DisableShortenedUrlCommand("missing", OwnerId), CancellationToken.None); }
        catch { }

        // Assert
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ─── Wrong user ───────────────────────────────────────────────


    [Fact]
    public async Task Handle_WhenCallerIsNotOwner_ShouldThrowForbiddenAccessException()
    {
        // Arrange 
        var url = BuildActiveUrl(OwnerId);
        SetupRepo("abc123", url);

        var command = new DisableShortenedUrlCommand("abc123", StrangerId);

        // Act / Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
                      .Should().ThrowAsync<ForbiddenAccessException>();
    }

    // ─── Anonymous URL (null UserId) ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUrlIsAnonymousAndCallerHasAnyId_ShouldNotDisableAndThrowForbiddenAccessException()
    {
        // Arrange
        var url = BuildActiveUrl(userId: null);
        SetupRepo("anon01", url);

        var command = new DisableShortenedUrlCommand("anon01", OwnerId);

        // Act / Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
                      .Should().ThrowAsync<ForbiddenAccessException>();
        url.Status.Should().Be(UrlStatus.Active);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ─── Already expired URL ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenUrlIsAlreadyExpired_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var url = BuildExpiredUrl(OwnerId);
        SetupRepo("exp01", url);

        var command = new DisableShortenedUrlCommand("exp01", OwnerId);

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert: ShortenedUrl.Disable() throws on Expired
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot disable an expired URL");
    }

    // ─── Repository failure propagates ────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldPropagateException()
    {
        // Arrange
        var url = BuildActiveUrl(OwnerId);
        SetupRepo("abc123", url);
        _repoMock.Setup(r => r.SaveChangesAsync())
                 .ThrowsAsync(new Exception("DB write failed"));

        var command = new DisableShortenedUrlCommand("abc123", OwnerId);

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("DB write failed");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static ShortenedUrl BuildActiveUrl(Guid? userId)
        => new("https://example.com/long", new ShortCode("Ab12Cd34"), DateTime.UtcNow.AddHours(1), userId);

    private ShortenedUrl BuildExpiredUrl(Guid? userId)
    {
        var url = BuildActiveUrl(userId);
        url.Expire();   // transitions to Expired via domain method
        return url;
    }

    private void SetupRepo(string shortCode, ShortenedUrl url)
    {
        _repoMock.Setup(r => r.GetByShortCodeAsync(shortCode)).ReturnsAsync(url);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }
}
