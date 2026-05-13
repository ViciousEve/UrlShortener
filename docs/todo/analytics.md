# Analytics Module — TODO

> Status: **~85% complete** — Domain, Application, Infrastructure, and Presentation fully implemented. Missing a few advanced queries, dashboard endpoint, and tests.

---

## ✅ Completed

### Domain Layer
- [x] `ClickEvent.cs` — Entity tracking individual click events (`Id`, `ShortenedUrlStatsId`, `ClickedAtUtc`, `IpAddress`, `UserAgent`)
- [x] `ShortenedUrlStats.cs` — Entity aggregating stats per shortened URL (`TotalClicks`, `LastClickedAtUtc`, `RecordClick()`)

### Application Layer
- [x] `IClickEventRepository.cs` — Full repository contract (`AddClick`, `GetClicksByShortCode`, `GetTotalClickForUserInPeriod`, `GetStatsByShortCode`, `GetTotalClickForUserRanked`, `GetTopBrowserForUser`, `AddStats`, `SaveChanges`)
- [x] `ClickEventResponse.cs` — DTO for click event data
- [x] `UrlClickedIntegrationEventHandler.cs` — Handles `UrlClickedIntegrationEvent` from Shortening module (upserts stats + records click + saves metadata)
- [x] `GetClicksByShortCodeQuery/Handler` — Fetches click events by short code
- [x] `GetUrlStatsQuery` — Directory exists (empty)
- [x] `GetUserClicksInPeriodQuery/Handler` — Total clicks for a user in a date range
- [x] `GetTopUrlQuery/Handler` — User's best performing URL by total clicks
- [x] `GetTopBrowserQuery/Handler` — Top N browsers by click count for a user
- [x] **Click metadata enrichment** — `IpAddress` and `UserAgent` captured in `ClickEvent`

### Infrastructure Layer
- [x] `AnalyticsDbContext.cs` — DbContext with schema separation
- [x] `ClickEventRepository.cs` — Full repository implementation
- [x] `ClickEventConfiguration.cs` — EF Core config for `ClickEvent`
- [x] `ShortenedUrlStatsConfiguration.cs` — EF Core config for `ShortenedUrlStats`

### Presentation Layer
- [x] `AnalyticsEndpoints.cs` — Presentation layer active with 4 endpoints:
  - [x] `GET /api/analytics/{code}/clicks` → `GetClicksByShortCodeQuery` (requires auth)
  - [x] `GET /api/analytics/clicks-in-period?from=&to=` → `GetUserClicksInPeriodQuery` (requires auth)
  - [x] `GET /api/analytics/top-url` → `GetTopUrlQuery` (requires auth)
  - [x] `GET /api/analytics/top-browsers?topN=5` → `GetTopBrowserQuery` (requires auth)

### Module Registration & Wiring
- [x] `AnalyticsModule.cs` — DI registration
- [x] `builder.Services.AddAnalyticsModule()` in `Program.cs`
- [x] `app.MapAnalyticsModule()` in `Program.cs`

---

## ❌ Remaining TODO

### Application — Queries
- [ ] **`GetUrlStatsQuery/Handler`** — Returns aggregated stats for a short code (total clicks, last click time). Directory scaffolded but handler not yet implemented.
- [ ] **`GetUserDashboardQuery/Handler`** — Returns dashboard overview for authenticated user across all URLs

### Presentation — Missing Endpoints
- [ ] `GET /api/analytics/{code}/stats` → `GetUrlStatsQuery` (total clicks, last click time, etc.)
- [ ] `GET /api/analytics/dashboard` → User's aggregated stats across all URLs

### Database
- [ ] Run EF Core migration for Analytics schema:
  ```
  dotnet ef migrations add AnalyticsInitial -p src/Modules/Analytics -s src/UrlShortener.API
  ```

### Testing
- [ ] Unit tests for `UrlClickedIntegrationEventHandler` — new stats, existing stats, metadata capture
- [ ] Unit tests for `GetClicksByShortCodeHandler` — valid code, empty result
- [ ] Integration test for click tracking flow (shorten → resolve → verify click recorded)

---

### Design Note — Dashboard Refresh Strategy
> **Polling over SSE.** A `setInterval` poll every 15–30s from the frontend is sufficient for dashboard freshness. SSE/WebSocket would add backend complexity (channels, connection management) for negligible UX gain — nobody watches click counts tick up in real-time. Revisit if sub-second latency becomes a real requirement.
