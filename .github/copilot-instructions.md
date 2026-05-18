# Copilot Instructions

## Build & Run

```bash
# Build solution
dotnet build Sannel.House.Sprinklers.sln

# Run the web API
dotnet run --project src/Sannel.House.Sprinklers/Sannel.House.Sprinklers.csproj

# Publish self-contained single-file for linux-arm64
make arm64
```

### EF Core Migrations

Always target `Sannel.House.Sprinklers.Infrastructure` with `Sannel.House.Sprinklers` as the startup project:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Sannel.House.Sprinklers.Infrastructure \
  --startup-project src/Sannel.House.Sprinklers

dotnet ef database update \
  --project src/Sannel.House.Sprinklers.Infrastructure \
  --startup-project src/Sannel.House.Sprinklers
```

### Docker

CI builds multi-arch images (`linux/amd64`, `linux/arm64`) using `src/Sannel.House.Sprinklers/Dockerfile`. The `calculateRID.sh` script is used inside the Dockerfile to derive the correct RID for the target architecture.

---

## Architecture

Four projects with a strict dependency direction: **Web API → Core ← Infrastructure**, with **Shared** referenced by all layers.

| Project | Role |
|---|---|
| `Sannel.House.Sprinklers` | ASP.NET Core Web API host — DI wiring, auth, controllers, Swagger |
| `Sannel.House.Sprinklers.Core` | Domain layer — interfaces, business services, no infrastructure dependencies |
| `Sannel.House.Sprinklers.Infrastructure` | EF Core (SQLite), hardware drivers, MQTT, SignalR hub implementation |
| `Sannel.House.Sprinklers.Shared` | DTOs, message types, `SprinklersClient` HTTP client — published as a NuGet package |

**Key runtime components:**
- `SprinklerService` — `BackgroundService` that manages the active zone timer (polls every 500 ms). Registered as both a singleton and hosted service.
- `ScheduleService` — `BackgroundService` that polls for due `ZoneRun` records every 10 seconds and triggers `SprinklerService`. Regenerates daily schedules at 01:00 using NCrontab.
- `MQTTManager` — Manages the MQTT connection with exponential-backoff reconnect logic.
- `MultiMessageClient` — Fan-out implementation of `IMessageClient` that publishes to both SignalR (`HubMessageClient`) and MQTT (`MQTTMessageClient`) simultaneously.

---

## Key Conventions

### Object Mapping — Riok.Mapperly
All model↔DTO mapping uses source-generated mappers. Declare a `partial` class with `[Mapper]` and partial method signatures; do not write manual mapping code.

```csharp
[Mapper]
public partial class ZoneInfoMapper
{
    public partial ZoneInfoDto ModelToDto(ZoneMetaData zoneInfo);
    public partial ZoneMetaData DtoToModel(ZoneInfoDto zoneInfo);
}
```

### API Versioning
Controllers live in `Controllers/v1.0/`, use namespace `Controllers.v1_0`, and carry `[ApiVersion(1.0)]`. Routes follow the pattern:
```
sprinkler/api/v{version:apiVersion}/[controller]
```
All application routes (API, Swagger, SignalR hub) are prefixed with `/sprinkler/`.

### Authorization
Authentication is Azure AD JWT via `Microsoft.Identity.Web`. Authorization policies are constants on `AuthPolicy`. Roles come from the `Sannel.House.Core` NuGet package (`Roles.Sprinklers.*`, `Roles.ADMIN`). Always apply `[Authorize(AuthPolicy.XYZ)]` on controller actions rather than `[Authorize]` alone.

### Hardware Abstraction
`ISprinklerHardware` has two implementations:
- `OpenSprinklerHardware` — real Raspberry Pi GPIO shift-register driver
- `FakeHardware` — no-op for non-Pi environments

The correct implementation is chosen at startup by checking for `/dev/gpiomem`. The zone count comes from the `Sprinkler:Zones` configuration key (required for `OpenSprinklerHardware`).

### Database — SQLite / EF Core
- Database file lives at `Data/schedule.db` (created on startup).
- **`DateTimeOffset` limitation**: SQLite doesn't support `DateTimeOffset` natively. `SprinklerDbContext.OnModelCreating` automatically applies `DateTimeOffsetToBinaryConverter` to all such properties — do not work around this.
- `ScheduleProgram.StationTimes` is stored as a JSON column (not a related table).
- `ZoneRun` has a composite primary key: `(StartDateTime, ZoneId)`.

### Configuration
At runtime, additional config is loaded from `app_config/appsettings.json` (intended for a Docker volume mount). Required keys:
- `AzureAd` — tenant/client for JWT validation
- `Sprinkler:Zones` — number of hardware zones (byte)
- `MQTT` — broker connection settings (`MqttOptions`)
- `AllowedOrigins` — CORS origins list

### Versioning
`Major`, `Minor`, `Patch` are defined in `Directory.Build.props` and injected as MSBuild properties (`/p:Major=...`). Do not hardcode version numbers in project files.

### Shared NuGet Package
`Sannel.House.Sprinklers.Shared` is published to NuGet with Source Link and `.snupkg` symbols. Treat its public API as a versioned contract — breaking changes require a version bump.
