# Identity Module — TODO

> Status: **~60% complete** — Fully scaffolded, handlers/services implemented, but endpoints are commented out and auth middleware isn't wired.

---

## ✅ Completed

### Domain Layer
- [x] `User.cs` — Aggregate Root
- [x] `Email.cs` — Value Object
- [x] `Role.cs` — Role enum/entity
- [x] `UserCreatedDomainEvent.cs` — Fired on registration

### Application Layer
- [x] `RegisterUserCommand/Handler/Validator` — Full CQRS with FluentValidation
- [x] `LoginUserQuery/Handler/Validator` — Authentication with secure error messages
- [x] `IJwtProvider` / `IPasswordHasher` / `IUserRepository` — Contracts
- [x] `RegisterRequest` / `LoginRequest` / `AuthResponse` / `TokenResult` — DTOs

### Infrastructure Layer
- [x] `IdentityDbContext.cs` — DbContext with schema separation
- [x] `UserConfiguration.cs` — EF Core entity configuration
- [x] `UserRepository.cs` — Repository implementation
- [x] `JwtProvider.cs` — JWT token generation (uses Options pattern + `TokenResult`)
- [x] `PasswordHasher.cs` — BCrypt password hashing

### Module Registration
- [x] `IdentityModule.cs` — DI registration

---

## ❌ Remaining TODO

### Presentation — Endpoints
- [ ] **Uncomment & activate `IdentityEndpoints.cs`** — both endpoints currently commented out:
  - [ ] `POST /api/identity/register` → `RegisterUserCommand`
  - [ ] `POST /api/identity/login` → `LoginUserQuery`
- [ ] **Future endpoints** (mentioned in code comments):
  - [ ] `GET /api/identity/me` — Get current user profile (requires auth)
  - [ ] `POST /api/identity/refresh` — Refresh JWT token
  - [ ] `POST /api/identity/change-password`

### API Wiring (in `Program.cs`)
- [ ] Add `builder.Services.AddIdentityModule(builder.Configuration)` 
- [ ] Add `app.MapIdentityModule()` to register endpoints
- [ ] Add `builder.Services.AddAuthentication().AddJwtBearer(...)` — configure JWT validation
- [ ] Add `app.UseAuthentication()` + `app.UseAuthorization()` middleware

### Configuration
- [ ] Add JWT settings to `appsettings.json`:
  ```json
  "JwtSettings": {
    "SecretKey": "...",
    "Issuer": "UrlShortener",
    "Audience": "UrlShortener",
    "ExpirationMinutes": 60
  }
  ```
- [ ] Add Identity connection string (or share the Shortening one)

### Database
- [ ] Run EF Core migration for Identity schema:
  ```
  dotnet ef migrations add IdentityInitial -p src/Modules/Identity -s src/UrlShortener.API
  ```

### Security Hardening
- [ ] Add rate limiting to login endpoint
- [ ] Use custom exceptions instead of `InvalidOperationException` in `LoginUserHandler`
- [ ] Add refresh token support (token rotation)

### Testing
- [ ] Unit tests for `Email` value object
- [ ] Unit tests for `RegisterUserHandler` / `LoginUserHandler`
- [ ] Integration test for register → login flow
