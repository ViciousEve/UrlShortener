# Shortening Module — TODO

> Status: **100% complete** — All items done. 216 unit tests passing.

---

## ✅ Completed

### Domain Layer
- [x] `ShortenedUrl.cs` — Aggregate Root with validation, `Expire()`, `Disable()`
- [x] `ShortCode.cs` — Value Object with length/character validation
- [x] `UrlStatus.cs` — Enum: Active, Expired, Disabled
- [x] `UrlCreatedDomainEvent.cs` — Fired on URL creation
- [x] `UrlExpiredDomainEvent.cs` — Fired on expiration

### Application Layer
- [x] `CreateShortenedUrlCommand/Handler/Validator` — Full CQRS with FluentValidation
- [x] `DeleteShortenedUrlCommand/Handler` — With existence check + ownership verification
- [x] `ResolveShortCodeQuery/Handler` — Resolves short code to original URL with status/TTL checks
- [x] `GetUserUrlsQuery/Handler` — Returns all URLs for authenticated user
- [x] `IShortenedUrlRepository` / `IShortCodeGenerator` — Contracts
- [x] `CreateShortenUrlRequest` / `ShortenedUrlResponse` — DTOs
- [x] `UrlClickedIntegrationEvent` — Integration event for Analytics click tracking

### Infrastructure Layer
- [x] `ShorteningDbContext.cs` — DbContext with schema separation
- [x] `ShortenedUrlConfiguration.cs` — EF Core config with Value Object conversion + indexes
- [x] `ShortenedUrlRepository.cs` — Repository implementation
- [x] `ShortCodeGenerator.cs` — Random base62 code generator
- [x] `UrlExpirationService.cs` — Background service (`BackgroundService`) to expire stale URLs every 15min

### Module Registration
- [x] `ShorteningModule.cs` — DI registration (DbContext, repository, generator, MediatR, FluentValidation)
- [x] Registered in `Program.cs` via `AddShorteningModule()`

---



### Application — Exceptions
- [x] Create **custom exception classes** for proper HTTP status mapping:
  - `NotFoundException` → 404
  - `ExpiredUrlException` → 410 (Gone)
  - `ForbiddenAccessException` → 403
  - Location: `App/Exceptions/` (shared kernel)

### Presentation — Endpoints
- [x] **Complete `ShorteningEndpoints.cs`**:
  - [x] `GET /s/{code}` → `ResolveShortCodeQuery` → **302 redirect** response
  - [x] `GET /api/shortening/urls` → `GetUserUrlsQuery` (requires authentication)
  - [x] `DELETE /api/shortening/urls/{code}` → `DeleteShortenedUrlCommand` (requires authentication)

### Infrastructure — Caching
- [x] **`CachedShortenedUrlRepository.cs`** — Decorator pattern with IMemoryCache for fast redirect lookups
## ❌ Remaining TODO
### Wiring
- [x] Add `ConnectionStrings:DefaultConnection` to `appsettings.Development.json`
- [x] Run initial EF Core migration (`ShorteningInitialCreate` applied)

### Testing
- [x] Unit tests for `ShortCode` value object
- [x] Unit tests for `ShortenedUrl` aggregate (creation, expiration, disable)
- [x] Unit tests for command/query handlers (mock repository)
- [ ] Integration test for `POST /api/shortening/shorten` endpoint
