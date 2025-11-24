---
applyTo: `**/*.*`
description: `Implementation.`
---
# Implementation Plan: Medical Equipment CRUD (Razor Pages, .NET 8, PostgreSQL)

Phases Overview
- Total phases: 5
- Follow sequentially; pause for side tasks from user; resume afterwards

Phase 1 — Project setup and dependencies
- Ensure ASP.NET Core Razor Pages project (NET 8) scaffolded
- Add NuGet: `Npgsql`, `Npgsql.DependencyInjection`
- Replace factory pattern with `AddNpgsqlDataSource` in `Program.cs`
- Initialize User Secrets: `dotnet user-secrets init`
- Set secret connection string: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=crud_test;Username=postgres;Password=YOUR_SECRET"`
- Keep `appsettings.json` placeholder password (no real secrets)
- Add navigation link to `Equipment` in `_Layout.cshtml`
- Commit: "Phase 1: deps + AddNpgsqlDataSource + secrets init"

Phase 2 — Domain model, data access, and schema
- Create `Models/Equipment.cs` with validation
- Create `Data/EquipmentRepository.cs` using injected `NpgsqlDataSource`
- Add `Database/schema.sql` and apply to PostgreSQL
- (Remove any obsolete factory if present)
- Commit: "Phase 2: model + repository + schema"

Phase 3 — Razor Pages scaffolding (UI)
- Create folder `Pages/Equipment`
- Add pages: `Index`, `Create`, `Edit`, `Details`, `Delete` (cshtml + page models)
- Inject `EquipmentRepository` and implement async handlers
- Add Bootstrap forms + validation tags
- Commit: "Phase 3: CRUD pages"

Phase 4 — Validation, UX polish, and error handling
- Add search by Name on Index
- Add TempData success/error alerts
- Handle not found (return 404) on invalid id
- Improve layout link styling for Equipment
- Commit: "Phase 4: UX + validation + errors"

Phase 5 — Configuration, testing, and deployment readiness
- Verify connection via running basic CRUD manually
- README: usage + applying schema + secret management instructions (User Secrets, env vars, GitHub Actions secrets)
- Optional: add GitHub Actions workflow (with env `ConnectionStrings__DefaultConnection: ${{ secrets.DEFAULT_CONNECTION }}`)
- Final manual test (create/edit/delete)
- Commit: "Phase 5: finalize + docs"
