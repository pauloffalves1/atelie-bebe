# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

This is a monorepo with two independent projects, no shared tooling or root package manifest:

- `server/` — .NET 10 backend (Clean Architecture), ASP.NET Core Minimal APIs, SQLite via EF Core.
- `client/` — Angular 22 frontend (standalone components, Vitest for tests).

There is no root-level build; each project is built/run/tested from its own directory.

Git repo hosted at `github.com/pauloffalves1/atelie-bebe` (remote `origin`, branch `master`). Ignore rules are split per project: root `.gitignore` (OS cruft only), `client/.gitignore` (Angular defaults: `node_modules`, `dist`, `.angular/cache`), `server/.gitignore` (.NET: `bin/`, `obj/`, `.vs/`, `*.user`, the local SQLite `*.db*` files).

`README.md` at the repo root is the canonical, detailed reference for architecture diagrams (Mermaid), the full business-rule catalog, and numbered functional/non-functional requirements (RF01–RF25, RNF01–RNF08) — read it for anything beyond the condensed architecture notes below rather than re-deriving it from source.

## Commands

### Backend (`server/`)

```bash
dotnet build AtelieBebe.slnx                       # build the whole solution
dotnet user-secrets set "Jwt:Secret" "<random-secret>" --project src/AtelieBebe.Api  # one-time, see below
dotnet run --project src/AtelieBebe.Api             # run the API (http://localhost:5120)
dotnet ef migrations add <Name> --project src/AtelieBebe.Infrastructure --startup-project src/AtelieBebe.Api
dotnet ef database update --project src/AtelieBebe.Infrastructure --startup-project src/AtelieBebe.Api
dotnet test test/AtelieBebe.Domain.Tests/AtelieBebe.Domain.Tests.csproj   # domain unit tests (xUnit)
```

`Jwt:Secret` is intentionally blank in `appsettings.json` — it is never committed. Set it locally via `dotnet user-secrets` (stored outside the repo, under the project's `UserSecretsId`) before running the API; without it, token generation throws at runtime (the key is too short). `dotnet test AtelieBebe.slnx` runs every test project; to run a single test, use xUnit's filter: `dotnet test --filter "FullyQualifiedName~OrderTests.ChangeStatus_AllowedTransition_Succeeds"`.

The API applies EF Core migrations and seeds the admin user + demo products automatically on startup (`DbInitializer.InitializeAsync`, called from `Program.cs`). The SQLite file lives at `src/AtelieBebe.Api/atelie-bebe.db`. Default seeded admin login is `admin@ateliebebe.com.br` / `admin123` (overridable via `AdminSeed:Email` / `AdminSeed:Password` config).

### Frontend (`client/`)

```bash
npm start            # ng serve, http://localhost:4200
npm run build         # ng build -> dist/
npm test              # ng test (Vitest)
```

To run a single test file/suite, use Vitest's own filtering, e.g. `npx vitest run path/to/file.spec.ts` or `npx vitest -t "test name"`.

`client/src/environments/environment.ts` points `apiUrl` at `http://localhost:5120/api` — keep the backend's `Cors:AllowedOrigins` (appsettings) and the frontend's dev port in sync when changing either.

## Backend architecture

Clean Architecture with four projects, dependencies flowing inward (`Api` → `Application`/`Infrastructure` → `Domain`; `Domain` has no project references):

- **`AtelieBebe.Domain`** — entities (`Product`, `Order`, `OrderItem`, `Customer`, `Admin`, `ContactMessage`), value objects (`Money`, `Email`), enums (`OrderStatus`, `OrderType`), and domain events. `Entity` (in `Common/`) is the base class holding a private `_domainEvents` list; entities call `AddDomainEvent` internally when their state changes meaningfully (e.g. `Order` created, stock going low). `IAggregateRoot` marks entities that repositories are allowed to expose directly — only aggregate roots get a repository.
- **`AtelieBebe.Application`** — one folder per feature (`Products`, `Orders`, `Auth`, `Contact`, `Dashboard`), each with an `I<Name>Service` interface, DTOs, and the implementation. Services depend on `Domain` repository interfaces and `Application/Abstractions` (`IUnitOfWork`, `IPasswordHasher`, `IJwtTokenGenerator`, `INotificationSender`) — never on `Infrastructure` directly. Custom exceptions (`NotFoundException`, `ConflictException`, `UnauthorizedAppException`) are thrown from services and translated to HTTP responses centrally.
- **`AtelieBebe.Infrastructure`** — EF Core (`AppDbContext`, per-entity `IEntityTypeConfiguration` classes, SQLite), repository implementations, `UnitOfWork`, JWT generation/password hashing, and the outbox (below). Registers everything via `AddInfrastructure(configuration)`; `Application` registers its services via `AddApplication()`.
- **`AtelieBebe.Api`** — Minimal API endpoints only, no controllers. Each feature has a static `Map<Feature>Endpoints(this WebApplication app)` extension in `Endpoints/`, called from `Program.cs`. Public routes live under `/api/{feature}`; admin-only routes live under `/api/admin/{feature}` behind `RequireAuthorization("AdminOnly")`. `Program.cs` is the composition root: registers auth (JWT bearer, `AdminOnly`/`CustomerOnly` policies), CORS, exception handling (`AppExceptionHandler` + `ProblemDetails`), then maps endpoints and runs `DbInitializer` before `app.Run()`.
- **`test/AtelieBebe.Domain.Tests`** — xUnit tests covering `Domain` invariants: the `Order` status state machine, `Product` stock/reservation rules, `Money`/`Email` value objects, and `Customer` registration. Only references `AtelieBebe.Domain` — no `Application`/`Infrastructure` tests exist yet, so mocking a repository or `IUnitOfWork` isn't a pattern established in this repo.

**Domain events → outbox → notifications**: entities raise domain events; `DomainEventsToOutboxInterceptor` (an EF Core `SaveChanges` interceptor) intercepts them at persistence time and writes each one as a serialized `OutboxMessage` row in the same transaction as the entity change — this is what makes the write atomic with the event capture. `OutboxProcessor` (a `BackgroundService`) polls unprocessed rows every 5s, deserializes each by its stored CLR type name, and dispatches to `INotificationSender` (currently `LoggingNotificationSender`, which just logs — no real email/SMS integration yet) based on a `switch` over the event type in `OutboxProcessor.DispatchAsync`. Delivery is at-least-once with a retry cap (`MaxAttempts = 5`); failures are recorded on the message row (`Attempts`, `Error`) rather than crashing the poller. When adding a new domain event that should trigger a notification, add a case to that switch and a corresponding method on `INotificationSender`.

There are two independent auth flows sharing the same JWT bearer scheme but different roles/policies: **Admin** (`AdminAuthService`, `AdminOnly` policy, backs the `/admin` area) and **Customer** (`CustomerAuthService`, `CustomerOnly` policy, backs storefront login/registration). Role names come from `JwtTokenGenerator.AdminRole`/equivalent constants.

## Frontend architecture

Angular 22, standalone components (no `NgModule`s), lazy-loaded routes (`loadComponent`), Bootstrap 5 for styling. Bootstrapped via `app.config.ts` (`ApplicationConfig`) rather than a root module.

`src/app/` is split into:
- **`core/`** — singletons: `services/` (one `HttpClient` wrapper per backend feature, mirroring the Application-layer split — e.g. `product.service.ts` calls both the public `/api/products` and admin `/api/admin/products` endpoints), `models/` (DTO interfaces matching the backend's), `guards/` (`adminGuard`, `customerGuard`, checked in `app.routes.ts`), `interceptors/` (`auth.interceptor.ts` attaches a Bearer token — picks the admin or customer token based on whether the request URL contains `/admin/`).
- **`features/`** — route-level components, split into `public/` (storefront: home, shop, product detail, cart, checkout, auth, account, contact, gallery) and `admin/` (dashboard, product/order/contact-message management, all behind `admin/` routes gated by `adminGuard`).
- **`shared/components/`** — reserved for cross-feature reusable components; currently empty.

Two parallel auth services (`auth.service.ts` for customers, `admin-auth.service.ts` for admins) each hold their own token, matching the backend's two independent auth flows.

Routing convention: public-facing route paths are in Portuguese (`/loja`, `/produto/:slug`, `/carrinho`, `/minha-conta`, etc.) while admin routes are under `/admin/...` also in Portuguese (`/admin/produtos`, `/admin/encomendas`). `LOCALE_ID` is set to `pt-BR` in `app.config.ts`. Match this locale/language convention for any new user-facing route, label, or seed data — the whole app (including seeded product data and validation/error messages in the backend) is in Brazilian Portuguese.

`features/public/contact` (`/contato`) merges general contact and custom-order requests into one page: a toggle reveals the piece-detail fields, and on submit the component builds a message client-side and opens `https://wa.me/<number>?text=...` — it does **not** call `OrderService.createCustomOrder` or `ContactService.submit` (both still exist and work, just unused by this page). The old `/encomenda-personalizada` route now `redirectTo: 'contato'` in `app.routes.ts` for backward-compat links. The WhatsApp number (`WHATSAPP_NUMBER` in `contact.ts`) is the atelier's real number, confirmed by the client.

Code style: Prettier config (`.prettierrc`) enforces 100-char width, single quotes, and the Angular parser for `.html` templates. New components use `style: "scss"` per `angular.json` schematics defaults, with the `app` selector prefix.
