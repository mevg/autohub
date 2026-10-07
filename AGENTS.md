# AGENTS.md — AutoHub

Monorepo with two independent stacks. No root workspace: root `package.json` is only husky + commitlint.
- Backend: `src/backend/` — .NET 10 clean architecture (`Api` / `Application` / `Domain` / `Infrastructure`).
- Frontend: `src/frontend/` — Angular 22 standalone app (`src/main.ts` → `app/app.ts`), SCSS + Tailwind.

## Engineering Philosophy
- **Avoid Over-Engineering:** Do not implement design patterns just for the sake of using them. Keep solutions pragmatic and strictly aligned with the actual business needs.
- **Strict Library Evaluation:** Do not add third-party libraries or extra abstraction layers unless they provide undeniable, immediate value to the specific feature being developed.

## Backend (`src/backend/`)

- **API Design:** Expose endpoints strictly using **Minimal APIs** in `src/Api/`. Do not generate or use traditional MVC Controllers.
- **Data Access Strategy (CQRS approach):** 
  - **Reads (Dapper):** Always use Dapper with raw SQL queries to fetch data. Return lightweight C# `records` or DTOs.
  - **Writes (EF Core):** Use EF Core + Npgsql exclusively for state mutations (Commands: INSERT, UPDATE, DELETE) to leverage domain entities and change tracking.
- Solution file is `AutoHub.slnx` (new XML format), not `.sln`. Always pass it explicitly:
  - `dotnet restore AutoHub.slnx`
  - `dotnet build AutoHub.slnx -c Release`
  - `dotnet test AutoHub.slnx -c Release`
- Central Package Management is on (`Directory.Packages.props`). Add/update versions only there; `.csproj` files use bare `<PackageReference Include="..." />`.
- Wiring: `src/Api/Program.cs` calls `AddApplication()` + `AddInfrastructure(config)`; DI extensions live in `Application/ApplicationDependencyInjection.cs` and `Infrastructure/InfrastructureDependencyInjection.cs`. `Api` references `Application` + `Infrastructure`.
- Application pattern: Feature folders under `Application/Features/<Area>/...`. Use MediatR command/handler + FluentValidation ONLY IF cross-cutting pipelines justify it. If a feature is simple, inject handlers directly into the Minimal API to avoid unnecessary abstraction layers.
- EF Core + Npgsql Postgres. `AppDbContext` auto-applies `IEntityTypeConfiguration<>` from its assembly — add entity config in `Infrastructure/Persistence/Configurations/`. Migrations live in `Infrastructure/Migrations/` (created via `dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Api`).
- `dotnet ef` uses `Infrastructure/Persistence/AppDbContextFactory.cs`, which reads `src/Api/appsettings.json` (`ConnectionStrings:DefaultConnection`).
- `IntegrationTests` use Testcontainers.PostgreSQL — require a running Docker daemon.
- Serilog is configured in code (`Program.cs`) and writes `logs/autohub-*.log`; don't commit log files.

## Frontend (`src/frontend/`)

- Requires Node 22 + pnpm 11.5.2, `pnpm install --frozen-lockfile`. Angular CLI `packageManager` is pnpm.
- **Architecture & Best Practices:**
  - **Standalone Only:** Use Standalone components exclusively. `NgModules` are strictly forbidden.
  - **Smart/Dumb (Container/Presentational) Pattern:** Strictly separate logic. Smart components inject services, manage state, and pass data down. Dumb/Presentational components ONLY receive data and emit events.
  - **Reactivity (Signals):** Default to **Angular Signals** (`signal`, `computed`, `effect`) for local component state. Use the modern signal-based inputs/outputs (`input()`, `output()`) instead of the old `@Input()`/`@Output()` decorators. Limit RxJS only to HTTP calls or complex asynchronous streams.
  - **Modern Control Flow:** Use the built-in control flow (`@if`, `@for`, `@switch`) instead of structural directives (`*ngIf`, `*ngFor`).
- **UI & Styling:**
  - **Tailwind CSS:** Use Tailwind utility classes directly in the HTML templates for styling. Keep `.scss` files empty unless writing highly custom, non-utility animations or complex scoped overrides.
  - **UX / Loading States:** Prefer **Skeleton Loaders** built with Tailwind (e.g., `animate-pulse`) over traditional loading spinners to improve perceived performance during async operations.
- Commands (run with `workdir` = `src/frontend`):
  - `pnpm start` — dev server; `pnpm build` / `pnpm build -- --configuration production`
  - `pnpm test` / `pnpm test:ci` (`test:ci` = `ng test --watch=false`, Vitest via `@angular/build:unit-test`, not Karma)
  - `pnpm lint` is `prettier --check .`, not ESLint; fix with `pnpm lint:fix`. Config: `.prettierrc` (100 col, single quotes).
- New components default to SCSS (`angular.json` schematic). Tests are `*.spec.ts` picked up via `tsconfig.spec.json` (`vitest/globals` types).
- CI generates `src/environments/environment.prod.ts` at build time from GitVersion — the file is not in the repo; don't create or depend on it locally.

## Commits / CI

- Commitlint enforced via `commit-msg` hook: `type(scope): subject`, subject lower-case. Allowed types: `feat fix docs style refactor perf test chore ci revert`; scopes: `backend frontend core auth inventory deps`.
- CI is path-filtered per stack (`.github/workflows/backend-ci.yml`, `frontend-ci.yml`). Versioning is GitVersion `ContinuousDelivery` (`GitVersion.yml`); branches `main dev feat/* hotfix/*`.