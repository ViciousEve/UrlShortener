using FluentAssertions;
using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Application.Queries.LoginUser;
using Identity.Domain;
using Moq;
using App.Exceptions;

namespace Identity.Tests.Application.Queries;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtProvider> _jwtMock;
    private readonly LoginUserHandler _handler;

    private const string FakeHash = "hashedPw1";
    private static readonly TokenResult FakeToken = new()
    {
        AccessToken = "valid.jwt.token",
        ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
    };

    public LoginUserHandlerTests()
    {
        _repoMock   = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock    = new Mock<IJwtProvider>();
        _handler    = new LoginUserHandler(_repoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    // ─── Happy Path ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = BuildUser();
        _repoMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("Secret1!", FakeHash)).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns(FakeToken);

        var query = new LoginUserQuery("user@example.com", "Secret1!");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be(FakeToken.AccessToken);
        result.ExpiresAtUtc.Should().Be(FakeToken.ExpiresAtUtc);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldCallJwtProviderWithCorrectUser()
    {
        // Arrange
        var user = BuildUser();
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtMock.Setup(j => j.GenerateToken(user)).Returns(FakeToken);

        var query = new LoginUserQuery("user@example.com", "Secret1!");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _jwtMock.Verify(j => j.GenerateToken(user), Times.Once);
    }

    // ─── Email not found ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEmailNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var query = new LoginUserQuery("ghost@example.com", "Secret1!");

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WhenEmailNotFound_ShouldNotCallHasherOrJwt()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var query = new LoginUserQuery("ghost@example.com", "Secret1!");

        // Act
        try { await _handler.Handle(query, CancellationToken.None); } catch { }

        // Assert: short-circuit
        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    // ─── Wrong password ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = BuildUser();
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("WrongPass1!", FakeHash)).Returns(false);

        var query = new LoginUserQuery("user@example.com", "WrongPass1!");

        // Act
        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        // Assert: same generic message — no leaking which part was wrong
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldNotCallJwtProvider()
    {
        // Arrange
        var user = BuildUser();
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var query = new LoginUserQuery("user@example.com", "WrongPass1!");

        // Act
        try { await _handler.Handle(query, CancellationToken.None); } catch { }

        // Assert: token was never generated
        _jwtMock.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    // ─── Security — error message must be identical for both failure modes ────────

    [Fact]
    public async Task Handle_BothFailureModesReturnIdenticalMessage_PreventingUserEnumeration()
    {
        // Arrange
        var wrongEmailQuery    = new LoginUserQuery("nobody@example.com", "AnyPass1!");
        var wrongPasswordQuery = new LoginUserQuery("user@example.com",   "BadPass1!");

        var user = BuildUser();
        _repoMock.Setup(r => r.GetByEmailAsync("nobody@example.com")).ReturnsAsync((User?)null);
        _repoMock.Setup(r => r.GetByEmailAsync("user@example.com")).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify("BadPass1!", FakeHash)).Returns(false);

        string? msgEmailNotFound = null;
        string? msgWrongPassword = null;

        try { await _handler.Handle(wrongEmailQuery, CancellationToken.None); }
        catch (UnauthorizedException ex) { msgEmailNotFound = ex.Message; }

        try { await _handler.Handle(wrongPasswordQuery, CancellationToken.None); }
        catch (UnauthorizedException ex) { msgWrongPassword = ex.Message; }

        // Assert: attacker cannot distinguish between the two scenarios
        msgEmailNotFound.Should().Be(msgWrongPassword);
    }

    // ─── Repository failure propagates ────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                 .ThrowsAsync(new Exception("Connection timeout"));

        var query = new LoginUserQuery("user@example.com", "Secret1!");

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("Connection timeout");
    }

    // ─── Helper ──────────────────────────────────────────────────────────────────

    private static User BuildUser()
        => new(new Email("user@example.com"), "testuser1", FakeHash);
}
