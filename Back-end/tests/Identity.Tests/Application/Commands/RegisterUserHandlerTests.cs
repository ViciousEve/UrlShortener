using FluentAssertions;
using Identity.Application.Commands.RegisterUser;
using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Identity.Domain;
using Moq;
using App.Exceptions;

namespace Identity.Tests.Application.Commands;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtProvider> _jwtMock;
    private readonly RegisterUserHandler _handler;

    // Minimum valid password hash (≥8 chars, ≤128 chars)
    private const string FakeHash = "hashedPw1";
    private static readonly TokenResult FakeToken = new()
    {
        AccessToken = "header.payload.sig",
        ExpiresAtUtc = DateTime.UtcNow.AddHours(1)
    };

    public RegisterUserHandlerTests()
    {
        _repoMock  = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _jwtMock   = new Mock<IJwtProvider>();
        _handler   = new RegisterUserHandler(_repoMock.Object, _hasherMock.Object, _jwtMock.Object);
    }

    // ─── Happy Path ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnAuthResponse()
    {
        // Arrange
        SetupDefaultMocks();
        var command = new RegisterUserCommand("test@example.com", "alice123", "Secret1!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(FakeToken.AccessToken);
        result.ExpiresAtUtc.Should().Be(FakeToken.ExpiresAtUtc);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldHashPasswordBeforePersisting()
    {
        // Arrange
        SetupDefaultMocks();
        var command = new RegisterUserCommand("test@example.com", "alice123", "Secret1!");

        User? capturedUser = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                 .Callback<User>(u => capturedUser = u)
                 .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: raw password never stored, hash is
        capturedUser!.PasswordHash.Should().Be(FakeHash);
        _hasherMock.Verify(h => h.Hash("Secret1!"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldPersistUserWithNormalisedEmail()
    {
        // Arrange
        SetupDefaultMocks();
        User? capturedUser = null;
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
                 .Callback<User>(u => capturedUser = u)
                 .Returns(Task.CompletedTask);

        var command = new RegisterUserCommand("TEST@EXAMPLE.COM", "alice123", "Secret1!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: Email value object normalises to lowercase
        capturedUser!.Email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallAddAsyncAndSaveChanges()
    {
        // Arrange
        SetupDefaultMocks();
        var command = new RegisterUserCommand("test@example.com", "alice123", "Secret1!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert: repository methods called in order
        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ─── Duplicate email ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenEmailAlreadyRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _repoMock.Setup(r => r.ExistsByEmailAsync("existing@example.com")).ReturnsAsync(true);
        var command = new RegisterUserCommand("existing@example.com", "alice123", "Secret1!");

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email already registered");
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyRegistered_ShouldNotCallHasherOrRepo()
    {
        // Arrange
        _repoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(true);
        var command = new RegisterUserCommand("existing@example.com", "alice123", "Secret1!");

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert: short-circuit — no hashing, no persistence
        _hasherMock.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    // ─── User entity invariants (caught here before reaching DB) ─────────────────

    [Theory]
    [InlineData("ab")]          // too short (< MinUsernameLength 3)
    [InlineData("")]            // empty
    public async Task Handle_WithInvalidUsername_ShouldThrowArgumentException(string username)
    {
        // Arrange
        _repoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns(FakeHash);

        var command = new RegisterUserCommand("test@example.com", username, "Secret1!");

        // Act
        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        // Assert: User constructor throws
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenHashIsTooShort_ShouldThrowArgumentException()
    {
        // The User constructor enforces passwordHash.Length >= MinPasswordLength (8).
        // If a hasher returns a suspiciously short hash this guards invariant.
        _repoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("short"); // < 8 chars

        var command = new RegisterUserCommand("test@example.com", "alice123", "Secret1!");

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ─── Token is returned without extra transformation ────────────────────────────

    [Fact]
    public async Task Handle_ShouldCallJwtProviderWithCreatedUser()
    {
        // Arrange
        SetupDefaultMocks();
        var command = new RegisterUserCommand("test@example.com", "alice123", "Secret1!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _jwtMock.Verify(j => j.GenerateToken(It.Is<User>(u =>
            u.Email.Value == "test@example.com" &&
            u.Username == "alice123"
        )), Times.Once);
    }

    // ─── Repository failure propagates ────────────────────────────────────────────

    [Fact]
    public async Task Handle_WhenSaveChangesFails_ShouldPropagateException()
    {
        // Arrange
        SetupDefaultMocks(saveThrows: true);
        var command = new RegisterUserCommand("test@example.com", "alice123", "Secret1!");

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("DB error");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private void SetupDefaultMocks(bool saveThrows = false)
    {
        _repoMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>())).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns(FakeHash);
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns(FakeToken);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        if (saveThrows)
            _repoMock.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new Exception("DB error"));
        else
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
    }
}
