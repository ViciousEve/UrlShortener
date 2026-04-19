using FluentAssertions;
using FluentValidation.TestHelper;
using Shortening.Application.Commands.CreateShortenedUrl;

namespace Shortening.Tests.Application.Validators;

public class CreateShortenedUrlValidatorTests
{
    private readonly CreateShortenedUrlValidator _validator;

    public CreateShortenedUrlValidatorTests()
    {
        _validator = new CreateShortenedUrlValidator();
    }

    #region Valid Commands

    [Fact]
    public void Validate_WithValidHttpsUrl_ShouldPass()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com/page",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidHttpUrl_ShouldPass()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "http://example.com/page",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(180)]
    public void Validate_WithAllowedTtlValues_ShouldPass(int ttlMinutes)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: ttlMinutes,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TtlInMinutes);
    }

    #endregion

    #region Invalid URL (User Mistakes)

    [Fact]
    public void Validate_WithEmptyUrl_ShouldFail()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: string.Empty,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL is required.");
    }

    [Fact]
    public void Validate_WithNullUrl_ShouldFail()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: null!,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyUrl_ShouldFail()
    {
        // Arrange - Common user mistake
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "   ",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("example.com")] // Missing scheme
    [InlineData("www.example.com")] // Missing scheme
    [InlineData("just some text")]
    public void Validate_WithInvalidUrlFormat_ShouldFail(string invalidUrl)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: invalidUrl,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL must be a valid URL.");
    }

    [Theory]
    [InlineData("ftp://example.com/file")] // FTP not allowed
    [InlineData("file:///c:/path/to/file")] // File scheme
    [InlineData("mailto:user@example.com")] // Mailto
    public void Validate_WithNonHttpScheme_ShouldFail(string nonHttpUrl)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: nonHttpUrl,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl)
            .WithErrorMessage("Original URL must be a valid URL.");
    }

    #endregion

    #region Invalid TTL (User Mistakes & Edge Cases)

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(120)]
    [InlineData(240)]
    [InlineData(1440)]
    public void Validate_WithNonAllowedTtl_ShouldFail(int invalidTtl)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: invalidTtl,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TtlInMinutes);
    }

    [Fact]
    public void Validate_WithNegativeTtl_ShouldFail()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: -1,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TtlInMinutes);
    }

    [Fact]
    public void Validate_WithVeryLargeTtl_ShouldFail()
    {
        // Arrange - Edge case: extremely large value
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: int.MaxValue,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TtlInMinutes);
    }

    #endregion

    #region Anonymous User Restrictions

    [Fact]
    public void Validate_AnonymousUserWith15MinTtl_ShouldPass()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 15,
            UserId: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(180)]
    public void Validate_AnonymousUserWithNon15MinTtl_ShouldFail(int ttlMinutes)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: ttlMinutes,
            UserId: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TtlInMinutes)
            .WithErrorMessage("Anonymous users can only create 15-minutes links");
    }

    [Fact]
    public void Validate_AuthenticatedUserWith60MinTtl_ShouldPass()
    {
        // Arrange - Authenticated users can use any allowed TTL
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Security - Malicious URL Inputs

    [Theory]
    [InlineData("javascript:alert('xss')")]
    [InlineData("data:text/html,<script>alert('xss')</script>")]
    [InlineData("vbscript:msgbox('xss')")]
    public void Validate_WithJavascriptProtocol_ShouldFail(string maliciousUrl)
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: maliciousUrl,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Theory]
    [InlineData("https://evil.com/<script>alert('xss')</script>")]
    [InlineData("https://example.com/page?q=<img src=x onerror=alert(1)>")]
    [InlineData("https://example.com/page?redirect=javascript:alert(1)")]
    public void Validate_WithXssInQueryParams_ShouldStillPassValidation(string xssUrl)
    {
        // Note: These are technically valid URLs - XSS prevention should happen at rendering
        // The validator only checks URL format, not content safety
        
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: xssUrl,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Theory]
    [InlineData("https://localhost/admin")]
    [InlineData("https://127.0.0.1/internal")]
    [InlineData("https://192.168.1.1/config")]
    [InlineData("https://10.0.0.1/secrets")]
    public void Validate_WithLocalNetworkUrls_ShouldPass(string localUrl)
    {
        // Note: SSRF protection might need additional checks beyond basic validation
        // Current validator allows these - document behavior
        
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: localUrl,
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Currently passes (potential SSRF vector - document for awareness)
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Fact]
    public void Validate_WithSqlInjectionInUrl_ShouldStillBeValidUrl()
    {
        // Arrange - SQL injection in URL (should be parameterized in storage)
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com/search?q='; DROP TABLE users; --",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert - It's a valid URL format; SQL protection is at storage layer
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }

    #endregion

    #region Edge Cases - URL Formats

    [Fact]
    public void Validate_WithUrlContainingPort_ShouldPass()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com:8080/page",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUrlContainingQueryAndFragment_ShouldPass()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com/page?param=value&other=123#section",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithUrlContainingEncodedCharacters_ShouldPass()
    {
        // Arrange
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://example.com/path%20with%20spaces?q=hello%20world",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithVeryLongUrl_ShouldPass()
    {
        // Arrange - Long but valid URL (DOS consideration - might want length limit)
        var longPath = new string('a', 2000);
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: $"https://example.com/{longPath}",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert - Currently no length limit in validator
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }

    [Fact]
    public void Validate_WithInternationalizedDomain_ShouldPass()
    {
        // Arrange - IDN domain
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "https://例え.jp/page",
            TtlInMinutes: 60,
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OriginalUrl);
    }

    #endregion

    #region Multiple Validation Errors

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReportAll()
    {
        // Arrange - Anonymous user with invalid URL and wrong TTL
        var command = new CreateShortenedUrlCommand(
            OriginalUrl: "not-a-valid-url",
            TtlInMinutes: 60, // Invalid for anonymous
            UserId: null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OriginalUrl);
        result.ShouldHaveValidationErrorFor(x => x.TtlInMinutes);
    }

    #endregion
}
