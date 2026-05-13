using FluentAssertions;
using Shortening.Domain;

namespace Shortening.Tests.Domain;

public class ShortCodeTests
{
    #region Valid Short Codes

    [Fact]
    public void Constructor_WithValidAlphanumericCode_ShouldCreateShortCode()
    {
        // Arrange
        var validCode = "Ab12Cd34";

        // Act
        var shortCode = new ShortCode(validCode);

        // Assert
        shortCode.Value.Should().Be(validCode);
    }

    [Theory]
    [InlineData("abcdefgh")] // All lowercase
    [InlineData("ABCDEFGH")] // All uppercase
    [InlineData("12345678")] // All numbers
    [InlineData("aB1cD2eF")] // Mixed
    public void Constructor_WithValidEightCharacterCodes_ShouldSucceed(string validCode)
    {
        // Act
        var shortCode = new ShortCode(validCode);

        // Assert
        shortCode.Value.Should().Be(validCode);
    }

    #endregion

    #region Empty/Null Input (Edge Cases & User Mistakes)

    [Fact]
    public void Constructor_WithNullValue_ShouldThrowArgumentException()
    {
        // Arrange
        string? nullCode = null;

        // Act
        var action = () => new ShortCode(nullCode!);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyCode = string.Empty;

        // Act
        var action = () => new ShortCode(emptyCode);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void Constructor_WithWhitespaceOnly_ShouldThrowArgumentException()
    {
        // Arrange - Common user mistake: entering spaces
        var whitespaceCode = "        "; // 8 spaces

        // Act
        var action = () => new ShortCode(whitespaceCode);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Invalid Length (Edge Cases)

    [Theory]
    [InlineData("abc1234")] // 7 characters - too short
    [InlineData("abcde12")] // 7 characters
    [InlineData("a")] // 1 character
    public void Constructor_WithLessThanEightCharacters_ShouldThrowArgumentException(string shortCode)
    {
        // Act
        var action = () => new ShortCode(shortCode);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*8 characters*");
    }

    [Theory]
    [InlineData("abc123456")] // 9 characters - too long
    [InlineData("abcdefghij")] // 10 characters
    [InlineData("abcdefghijklmnop")] // 16 characters
    public void Constructor_WithMoreThanEightCharacters_ShouldThrowArgumentException(string shortCode)
    {
        // Act
        var action = () => new ShortCode(shortCode);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*8 characters*");
    }

    #endregion

    #region Invalid Characters (User Mistakes & Security)

    [Theory]
    [InlineData("abc-1234")] // Hyphen
    [InlineData("abc_1234")] // Underscore
    [InlineData("abc.1234")] // Dot
    [InlineData("abc 1234")] // Space in middle
    public void Constructor_WithSpecialCharacters_ShouldThrowArgumentException(string invalidCode)
    {
        // Act
        var action = () => new ShortCode(invalidCode);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*invalid characters*");
    }

    [Theory]
    [InlineData("абвгдежз")] // Cyrillic characters (8 chars)
    [InlineData("中文测试测试中文")] // Chinese characters
    [InlineData("αβγδεζηθ")] // Greek characters
    public void Constructor_WithNonAsciiCharacters_ShouldThrowArgumentException(string unicodeCode)
    {
        // Act
        var action = () => new ShortCode(unicodeCode);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Security - Malicious Input Attempts

    [Theory]
    [InlineData("'; DROP")] // SQL Injection attempt (partial, 7 chars + space = 8)
    [InlineData("<script")] // XSS attempt start
    [InlineData("..\\..\\.")] // Path traversal attempt
    [InlineData("{{7*7}}")] // Template injection
    public void Constructor_WithMaliciousPatterns_ShouldThrowArgumentException(string maliciousCode)
    {
        // Act
        var action = () => new ShortCode(maliciousCode);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullByteInjection_ShouldThrowArgumentException()
    {
        // Arrange - Null byte injection attempt
        var nullByteCode = "abc\0defg";

        // Act
        var action = () => new ShortCode(nullByteCode);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithControlCharacters_ShouldThrowArgumentException()
    {
        // Arrange - Control characters that could cause issues
        var controlCharCode = "abc\tdefg"; // Tab character

        // Act
        var action = () => new ShortCode(controlCharCode);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNewlineCharacters_ShouldThrowArgumentException()
    {
        // Arrange - Newline injection (HTTP response splitting potential)
        var newlineCode = "abc\r\nXYZ";

        // Act
        var action = () => new ShortCode(newlineCode);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Boundary Cases

    [Fact]
    public void Constructor_WithExactlyEightDigits_ShouldSucceed()
    {
        // Arrange
        var numericCode = "00000001";

        // Act
        var shortCode = new ShortCode(numericCode);

        // Assert
        shortCode.Value.Should().Be(numericCode);
    }

    [Fact]
    public void Constructor_WithMixedCasePreserved_ShouldPreserveCase()
    {
        // Arrange
        var mixedCaseCode = "AbCdEfGh";

        // Act
        var shortCode = new ShortCode(mixedCaseCode);

        // Assert
        shortCode.Value.Should().Be(mixedCaseCode);
        shortCode.Value.Should().NotBe(mixedCaseCode.ToLower());
        shortCode.Value.Should().NotBe(mixedCaseCode.ToUpper());
    }

    #endregion
}
