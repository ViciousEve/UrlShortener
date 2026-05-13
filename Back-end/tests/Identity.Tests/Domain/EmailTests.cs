using FluentAssertions;
using Identity.Domain;

namespace Identity.Tests.Domain;

public class EmailTests
{
    // ─── Valid emails ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]          // uppercase — stored as lowercase
    [InlineData("user+tag@sub.domain.org")]
    [InlineData("a@b.co")]                    // minimal valid TLD
    public void Constructor_WithValidEmail_ShouldCreateEmailAndNormalisedToLowerCase(string raw)
    {
        var email = new Email(raw);

        email.Value.Should().Be(raw.ToLowerInvariant());
    }

    // ─── Invalid emails ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]                       // whitespace-only
    [InlineData("notanemail")]                // missing @
    [InlineData("@nodomain.com")]             // missing local part
    [InlineData("user@")]                     // missing domain
    [InlineData("user@domain")]               // missing TLD
    [InlineData("user @domain.com")]          // inline space — rejected by \s guard
    [InlineData("  user@example.com  ")]      // leading/trailing whitespace — rejected (no auto-trim)
    public void Constructor_WithInvalidEmail_ShouldThrowArgumentException(string raw)
    {
        Action act = () => new Email(raw);

        act.Should().Throw<ArgumentException>().WithMessage("Invalid email address");
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentException()
    {
        Action act = () => new Email(null!);

        act.Should().Throw<ArgumentException>().WithMessage("Invalid email address");
    }

    // ─── Normalisation ────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldStoreLowerCaseValue()
    {
        var email = new Email("Hello@World.COM");

        email.Value.Should().Be("hello@world.com");
    }

    // ─── Equality ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        var a = new Email("user@example.com");
        var b = new Email("USER@EXAMPLE.COM");  // same after normalisation

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        var a = new Email("alice@example.com");
        var b = new Email("bob@example.com");

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var email = new Email("user@example.com");

        email.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNonEmailObject_ShouldReturnFalse()
    {
        var email = new Email("user@example.com");

        email.Equals("user@example.com").Should().BeFalse();
    }
}
