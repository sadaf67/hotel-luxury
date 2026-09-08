# HotelLuxury

A hotel booking and management website built with ASP.NET Core, using Clean Architecture
(Domain / Application / Infrastructure / Web layers) and Entity Framework Core.

## Features

- **Public site**: room catalog with photo galleries, room reservations (with a public
  "track my reservation" lookup), pool session booking, café menu, homepage slider/gallery,
  discounts, and a contact form.
- **Admin panel** (`/Admin`): manage rooms, reservations, pool sessions, café menu/categories,
  discounts, site settings, contact messages, and SMS delivery logs. Cookie-based admin login
  (ASP.NET Core Identity) with rate-limited login attempts.
- **SMS notifications** via Kavenegar (configurable — can run in a "Mock" provider mode for
  local development with no real SMS sent).
- **Health check** endpoint (`/health`) and a startup configuration validator that refuses to
  boot in Production with missing SMS/admin-seed configuration.
- **Reverse proxy support** (optional, for hosting behind IIS/nginx that terminates HTTPS).

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 8/9), Clean Architecture |
| Data | Entity Framework Core, SQL Server (LocalDB for development) |
| Auth | ASP.NET Core Identity (cookie-based, admin area only) |
| SMS | Kavenegar API (optional, mockable) |
| Frontend | Razor views + Bootstrap-based admin/public UI |

## Project structure

```
Hotel.Domain/          → entities (Room, Reservation, PoolBooking/PoolSession, CafeMenuItem,
                          Gallery, Slider, Discount, ContactMessage, SmsLog, SiteSetting)
Hotel.Application/      → application services / business logic
Hotel.Infrastructure/   → EF Core DbContext, migrations, Kavenegar SMS service
Hotel.Web/              → MVC controllers (public + Admin area), Razor views, Program.cs
```

## Local setup

1. Requires .NET SDK and SQL Server / LocalDB.
2. Copy the connection string and settings pattern from `appsettings.json` — no real secrets
   are checked in; `appsettings.Development.json` / `appsettings.Production.json` are
   gitignored and read from environment variables in production (see `DEPLOYMENT.md`).
3. `dotnet ef database update --project Hotel.Infrastructure --startup-project Hotel.Web`
4. `dotnet run --project Hotel.Web`

See `DEPLOYMENT.md` for the full production deployment guide (required environment variables,
publish steps, and post-deployment verification checklist).

## Disclaimer

This is a software-engineering demo project.
