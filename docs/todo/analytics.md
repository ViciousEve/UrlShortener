# Analytics Module — TODO

> Status: **~50% complete** — Domain enrichment done, handler updated. Missing presentation layer, stats queries, and dashboard.

---

## ✅ Completed

### Domain Layer
- [x] `ClickEvent.cs` — Entity tracking individual click events
- [x] `ShortenedUrlStats.cs` — Entity aggregating stats per shortened URL

### Application Layer
- [x] `IClickEventRepository.cs` — Repository contract
- [x] `ClickEventResponse.cs` — DTO for click event data
- [x] `UrlClickedIntegrationEventHandler.cs` — Handles `UrlClickedIntegrationEvent` from Shortening module (upserts stats + records click)
- [x] `GetClicksByShortCodeQuery/Handler` — Fetches click events by short code

### Infrastructure Layer
- [x] `AnalyticsDbContext.cs` — DbContext with schema separation
- [x] `ClickEventRepository.cs` — Repository implementation
- [x] `ClickEventConfiguration.cs` — EF Core config for `ClickEvent`
- [x] `ShortenedUrlStatsConfiguration.cs` — EF Core config for `ShortenedUrlStats`

### Module Registration
- [x] `AnalyticsModule.cs` — DI registration
- [x] Registered in `Program.cs` via `AddAnalyticsModule()`

---

## ❌ Remaining TODO

### Presentation — Endpoints
- [ ] **Create `AnalyticsEndpoints.cs`** — No presentation layer exists yet:
  - [ ] `GET /api/analytics/{code}/clicks` → `GetClicksByShortCodeQuery` (requires auth)
  - [ ] `GET /api/analytics/{code}/stats` → Total clicks, unique visitors, time-series data
  - [ ] `GET /api/analytics/dashboard` → User's aggregated stats across all URLs

### Application — Queries
- [ ] **`GetUrlStatsQuery/Handler`** — Returns aggregated stats (total clicks, last click time, etc.)
- [ ] **`GetUserDashboardQuery/Handler`** — Returns dashboard overview for authenticated user
- [x] **Click metadata enrichment** — Capture IP and user-agent in `ClickEvent` (Referrer/Country deferred)

### Domain
- [x] Add metadata fields to `ClickEvent`:
  - `IpAddress`, `UserAgent`
- [x] Add computed properties to `ShortenedUrlStats`:
  - `TotalClicks`, `LastClickedAtUtc`, `RecordClick()` method

### API Wiring
- [ ] Add `app.MapAnalyticsModule()` in `Program.cs` (endpoints not mapped yet)

### Database
- [ ] Run EF Core migration for Analytics schema:
  ```
  dotnet ef migrations add AnalyticsInitial -p src/Modules/Analytics -s src/UrlShortener.API
  ```

### Testing
- [ ] Unit tests for `UrlClickedIntegrationEventHandler` 
- [ ] Unit tests for `GetClicksByShortCodeHandler`
- [ ] Integration test for click tracking flow (shorten → resolve → verify click recorded)
