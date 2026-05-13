# UrlShortener.API — TODO

> Status: **~30% complete** — Basic setup with Swagger, exception handlers, and Shortening/Analytics module registration. Missing auth, Identity wiring, CORS, and production config.

---

## ✅ Completed

### `Program.cs`
- [x] `GlobalExceptionHandler` — Catches unhandled exceptions → 500 ProblemDetails
- [x] `ValidationExceptionHandler` — Catches FluentValidation exceptions → 400 ProblemDetails
- [x] `AddEndpointsApiExplorer()` + `AddSwaggerGen()` — Swagger/OpenAPI
- [x] `AddShorteningModule()` / `MapShorteningModule()` — Shortening module registered
- [x] `AddAnalyticsModule()` — Analytics module services registered

### Configuration
- [x] `appsettings.json` — Basic logging config
- [x] `appsettings.Development.json` — Dev overrides

---

## ❌ Remaining TODO

### Module Wiring
- [ ] **Add Identity module**:
  - [ ] `builder.Services.AddIdentityModule(builder.Configuration)`
  - [ ] `app.MapIdentityModule()`
- [ ] **Map Analytics endpoints**:
  - [ ] `app.MapAnalyticsModule()` (Analytics services are registered but endpoints are not mapped)

### Authentication & Authorization
- [ ] `builder.Services.AddAuthentication().AddJwtBearer(...)` — Configure JWT Bearer validation
- [ ] `app.UseAuthentication()` — Add authentication middleware
- [ ] `app.UseAuthorization()` — Add authorization middleware
- [ ] Add `[Authorize]` / `.RequireAuthorization()` to protected endpoints

### Exception Handling
- [ ] Handle custom exceptions with proper HTTP status codes:
  - `NotFoundException` → 404
  - `ExpiredUrlException` → 410
  - `UnauthorizedException` → 403
- [ ] Add dedicated `IExceptionHandler` implementations or switch on exception type

### Configuration / `appsettings.json`
- [ ] Add `ConnectionStrings:DefaultConnection` for PostgreSQL
- [ ] Add `JwtSettings` section (SecretKey, Issuer, Audience, ExpirationMinutes)
- [ ] Add Redis connection string (when caching is implemented)

### CORS
- [ ] Configure CORS policy for the front-end origin:
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddDefaultPolicy(policy =>
          policy.WithOrigins("http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod());
  });
  ```
- [ ] `app.UseCors()`

### Docker / Deployment
- [x] `docker-compose.yml` exists at project root (API + PostgreSQL)
- [ ] Verify Dockerfile builds and runs correctly
- [ ] Add health check endpoint (`/health`)
- [ ] Environment variable substitution for secrets (connection strings, JWT key)

### Database Migrations
- [ ] Run all pending migrations:
  ```
  dotnet ef migrations add InitialCreate -p src/Modules/Shortening -s src/UrlShortener.API
  dotnet ef migrations add IdentityInitial -p src/Modules/Identity -s src/UrlShortener.API
  dotnet ef migrations add AnalyticsInitial -p src/Modules/Analytics -s src/UrlShortener.API
  ```
- [ ] Add `EnsureCreated` or migration auto-apply for development

### Testing
- [ ] Integration tests for the full API pipeline
- [ ] Endpoint smoke tests (health checks, Swagger reachable)
