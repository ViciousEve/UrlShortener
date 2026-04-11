# Identity Module — TODO

> Status: **~95% complete** — Fully implemented and wired. Only DB migration and security hardening remain.

---

## ✅ Completed

### Domain Layer
- [x] `User.cs` — Aggregate Root (`Id`, `Email`, `Username`, `PasswordHash`, `Role`, `IsActive`, `CreatedAtUtc`)
- [x] `Email.cs` — Value Object (validates format, normalises to lowercase)
- [x] `Role.cs` — Role enum
- [x] `UserCreatedDomainEvent.cs` — Fired on registration

### Application Layer
- [x] `RegisterUserCommand/Handler/Validator` — Full CQRS with FluentValidation
- [x] `LoginUserQuery/Handler/Validator` — Authentication with secure error messages (identical message for email-not-found and wrong-password)
- [x] `IJwtProvider` / `IPasswordHasher` / `IUserRepository` — Contracts
- [x] `RegisterRequest` / `LoginRequest` / `AuthResponse` / `TokenResult` — DTOs

### Infrastructure Layer
- [x] `IdentityDbContext.cs` — DbContext with schema separation
- [x] `UserConfiguration.cs` — EF Core entity configuration (activated, all columns mapped)
- [x] `UserRepository.cs` — Repository implementation
- [x] `JwtProvider.cs` — JWT token generation (uses `JwtSettings` Options from `appsettings.Development.json`)
- [x] `PasswordHasher.cs` — BCrypt password hashing

### Presentation Layer
- [x] `IdentityEndpoints.cs` — Both endpoints active:
  - [x] `POST /api/identity/register` → `RegisterUserCommand`
  - [x] `POST /api/identity/login` → `LoginUserQuery`

### API Wiring (`Program.cs`)
- [x] `builder.Services.AddIdentityModule(builder.Configuration)`
- [x] `app.MapIdentityModule()`
- [x] JWT bearer authentication configured (`AddAuthentication().AddJwtBearer(...)`)
- [x] `app.UseAuthentication()` + `app.UseAuthorization()` in middleware pipeline

### Module Registration
- [x] `IdentityModule.cs` — DI registration (DbContext, IUserRepository, IPasswordHasher, IJwtProvider, MediatR, FluentValidation)

### Testing
- [x] Unit tests for `Email` value object (valid/invalid, normalisation, equality)
- [x] Unit tests for `RegisterUserHandler` (happy path, duplicate email, hash enforcement, JWT delegation, repo failure)
- [x] Unit tests for `LoginUserHandler` (happy path, email-not-found, wrong password, user enumeration prevention, repo failure)

---

## ❌ Remaining TODO

### Database
- [ ] Run EF Core migration for Identity schema:
  ```
  dotnet ef migrations add IdentityInitial -p src/Modules/Identity -s src/UrlShortener.API
  ```

### Security Hardening
- [ ] Add rate limiting to login endpoint (prevent brute-force)
- [ ] Use custom `AuthenticationException` instead of `InvalidOperationException` in `LoginUserHandler` (maps to 401 rather than 500)

### Future Endpoints
- [ ] `GET /api/identity/me` — Get current user profile (requires auth)
- [ ] `POST /api/identity/change-password` — Change password (requires auth)

### Testing
- [ ] Integration test for register → login flow (end-to-end with real DB)
