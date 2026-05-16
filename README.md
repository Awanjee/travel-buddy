# Travel Buddy (Phase 1)

Flutter mobile + web client with an ASP.NET Core API and PostgreSQL (SQLite in local dev).

## Features

- Email/password auth and profile
- Trip questionnaire (destination, budget, energy, visa intent, interests)
- Visa checklists and estimated timelines (informational only — not legal advice)
- Approve/decline discovery cards for places, hotels, and activities
- Itinerary generation (Markdown + PDF export, share sheet)
- Deep links to search hotels and activities

Seeded visa/content pairs: **PK → JP**, **PK → TR** (plus generic fallback).

Phase 2 social tables (`TravelerProfile`, `ConnectionRequest`, `SwipeEvent`) exist in the schema without UI.

## Quick start

### API

```bash
cd backend/src/TravelBuddy.Api
dotnet run
```

Swagger: http://localhost:5280/swagger

Development uses **SQLite** (`travelbuddy.db`) by default. For PostgreSQL:

```bash
docker compose up -d
```

Set in `appsettings.Development.json`:

```json
"Database": { "UseSqlite": false },
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=travelbuddy;Username=travelbuddy;Password=travelbuddy"
}
```

### Flutter

```bash
cd app
flutter pub get
flutter run -d chrome
```

API base URL: `lib/config/api_config.dart` (localhost:5280; Android emulator uses `10.0.2.2`).

## Project layout

```
travel-buddy/
├── app/                 # Flutter (iOS, Android, Web, Windows)
├── backend/             # .NET 8 API
├── docker-compose.yml   # PostgreSQL
└── .github/workflows/   # CI
```

## Disclaimer

Visa and travel content is for planning only. Always confirm requirements with official government sources before applying or booking.
