# Shortly — URL Shortener

A full-stack URL shortening platform built with a modular, production-ready architecture. Users can shorten links, track click analytics, and manage their URLs through a sleek dashboard — all secured behind JWT authentication.

## Features

- **URL Shortening** — Generate short codes for any URL, with redirect support
- **Analytics** — Track click counts and per-URL statistics
- **Authentication** — Secure registration & login via JWT + ASP.NET Identity
- **Dashboard** — View and manage all shortened URLs in one place
- **Containerized Deployment** — Docker support for both frontend and backend

## Tech Stack

### Backend
- **ASP.NET Core** — Minimal API, modular architecture with Domain-Driven Design (DDD)
- **MediatR** — CQRS pattern for clean separation of commands and queries
- **Entity Framework Core** — ORM with SQL Server / Supabase (PostgreSQL)
- **JWT + ASP.NET Identity** — Secure authentication and authorization
- **xUnit + Moq** — Unit tests with ~90% coverage on business logic
- **Swagger / OpenAPI** — Auto-generated API documentation

### Frontend
- **React + TypeScript** — Component-based SPA
- **Tailwind CSS** — Glassmorphism-inspired design system
- **TanStack Query** — Data fetching and cache management
- **Recharts** — Analytics data visualization
- **Vite** — Fast development tooling with environment variable support

### Infrastructure
- **Docker** — Multi-stage builds (Node.js → Nginx for frontend, containerized backend)
- **GitHub Actions** — CI/CD pipeline running tests on every pull request
- **Render** — Backend container hosting
- **Vercel** — Frontend hosting
- **Supabase** — Managed PostgreSQL database

## Project Structure

```
├── Back-end/         # ASP.NET Core API
│   └── src/
│       ├── UrlShortener.API/        # Entry point, endpoint registration
│       ├── UrlShortener.Shortening/ # Core URL shortening module
│       ├── UrlShortener.Analytics/  # Click tracking module
│       └── UrlShortener.Identity/   # Auth module (register/login)
├── Front-end/
│   └── url-shortener/  # React + TypeScript SPA
├── docs/               # Architecture and API wiring docs
├── docker-compose.yml  # Production compose config
└── docker-compose-dev.yml  # Local development config
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker (optional)

### Run Locally (without Docker)

**Backend:**
```bash
cd Back-end/src/UrlShortener.API
dotnet run
```

**Frontend:**
```bash
cd Front-end/url-shortener
npm install
npm run dev
```

### Run with Docker Compose

```bash
docker-compose -f docker-compose-dev.yml up --build
```

## Links

- **GitHub:** [github.com/ViciousEve/UrlShortener](https://github.com/ViciousEve/UrlShortener)
- **Live Frontend:** Deployed on Vercel
- **Backend API:** Deployed on Render