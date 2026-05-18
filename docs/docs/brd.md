# Business Requirements Document: Sannel.House.Sprinklers

## Document Info

| Field        | Value                        |
|---|---|
| Version      | 1.0                          |
| Status       | Draft                        |
| Author       | Sannel Software, L.L.C.      |
| Last Updated | 2026-05-11                   |

---

## Executive Summary

Sannel.House.Sprinklers is a self-hosted residential irrigation controller designed to run on a Raspberry Pi with an OpenSprinkler-compatible GPIO-driven hardware board. It replaces proprietary, cloud-dependent irrigation timers with a locally hosted, privacy-first alternative that integrates cleanly into the Sannel House home automation ecosystem.

The system exposes a versioned REST API secured by Azure AD (Microsoft Entra ID) JWT tokens, a real-time SignalR hub, and an optional MQTT event channel. A companion NuGet package (`Sannel.House.Sprinklers.Shared`) provides a typed .NET client and shared DTOs for consumer applications. Automated watering schedules are defined via day-of-week or interval-based patterns and are evaluated and queued daily without cloud dependency.

---

## Business Objectives

- **BO-01** — Provide fully autonomous, on-device irrigation scheduling that operates without an internet connection after initial configuration.
- **BO-02** — Eliminate dependency on proprietary cloud irrigation services, keeping all watering history and schedule data on-premises.
- **BO-03** — Integrate with the Sannel House home automation platform using standard role-based access control (Azure AD app roles).
- **BO-04** — Expose hardware control and scheduling via a documented REST API so that other Sannel House components and third-party tools can interoperate.
- **BO-05** — Publish a typed .NET client library (`Sannel.House.Sprinklers.Shared`) so that consumer applications can integrate without hand-rolling HTTP calls.
- **BO-06** — Deliver real-time zone activity notifications over both SignalR and MQTT so that dashboards and automation rules can react to watering events without polling.

---

## Scope

### In Scope

- Residential irrigation zone control via OpenSprinkler-compatible GPIO hardware on Raspberry Pi.
- Day-of-week and interval-based automated watering schedule creation, management, and daily execution.
- Manual zone start/stop triggered via REST API.
- Persistent watering history (station run log).
- Per-zone metadata (name, display colour) settable via API.
- Real-time event fan-out over SignalR and MQTT (start, stop, progress, zone-update events).
- Azure AD JWT authentication and role-based authorization enforced on all API endpoints.
- Multi-architecture Docker image (`linux/amd64`, `linux/arm64`) for container-based deployment.
- Self-contained single-file binary for direct deployment on Raspberry Pi (`make arm64`).
- A non-hardware "fake" mode for development and testing on non-Pi hosts.
- Shared NuGet package (`Sannel.House.Sprinklers.Shared`) containing typed HTTP client, DTOs, and message types.
- A browser-based management UI (`Sannel.House.Sprinklers.Web`) built with Blazor WebAssembly and MudBlazor, providing real-time zone status, manual zone start/stop, schedule management, zone metadata editing, and run history. In development it is orchestrated alongside the API by the Aspire AppHost; for production it may be bundled with the API or deployed independently.
- A .NET Aspire AppHost project for orchestrated local development, spinning up the API host, Blazor app, and an MQTT broker container.

### Out of Scope

- Multi-controller or multi-site irrigation management.
- Weather-based watering adjustments (rain sensors, evapotranspiration calculations).
- Native mobile applications.
- Integration with proprietary irrigation ecosystems (Rachio, Orbit, etc.).
- User account management (delegated to Azure AD / Microsoft Entra).
- Direct hardware support for irrigation controllers other than OpenSprinkler-compatible GPIO-driven boards.

---

## Stakeholders

| Role                   | Name / Team             | Responsibility                                                            |
|---|---|---|
| Product Owner          | Sannel Software, L.L.C. | Defines requirements, approves releases                                   |
| Developer              | Sannel Software, L.L.C. | Implements and maintains the service and shared client library             |
| System Administrator   | End user / Homeowner     | Deploys and configures the Docker container; manages Azure AD app registration |
| Consumer Applications  | Sannel House ecosystem   | Use the REST API and `SprinklersClient` NuGet package                     |
| Identity Provider      | Microsoft Entra (Azure AD) | Issues JWT tokens; manages app roles assigned to users                  |
| External Broker        | MQTT server operator     | Receives and distributes irrigation event messages                        |

---

## Functional Requirements

| ID     | Requirement                                                                                                                   | Priority |
|---|---|---|
| FR-001 | The system SHALL allow authorized users to manually enqueue one or more zones to run sequentially via REST API, specifying a zone ID and duration per zone. Zones SHALL execute in order; if a zone is currently running, the new zones SHALL be appended to the end of the queue. | High     |
| FR-002 | The system SHALL allow authorized users to stop all active zones and clear the zone queue immediately via REST API.          | High     |
| FR-003 | The system SHALL report current sprinkler status (running state, active zone, time remaining, total run time, and count of queued zones) via REST API. | High     |
| FR-004 | The system SHALL allow authorized users to create named irrigation schedule programs with a start time, a day-of-week selection OR an interval-in-days pattern (with an anchor start date), and per-zone run durations. Exactly one scheduling mode (day-of-week or interval) must be specified per program. | High     |
| FR-005 | The system SHALL allow authorized users to update existing schedule programs.                                                | High     |
| FR-006 | The system SHALL allow authorized users to enable or disable individual schedule programs independently.                     | High     |
| FR-007 | The system SHALL evaluate enabled schedules daily at 01:00 and pre-generate `ZoneRun` records for that day based on each schedule's configured day-of-week set or interval-in-days pattern and start time. | High     |
| FR-008 | The system SHALL poll pending `ZoneRun` records every 10 seconds and execute the next due run if no zone is currently active. | High    |
| FR-009 | The system SHALL execute only one zone at a time; concurrent zone activation SHALL be rejected.                              | High     |
| FR-010 | The system SHALL automatically stop a running zone when its configured run duration elapses (checked every 500 ms).         | High     |
| FR-011 | The system SHALL persist a log of all zone actions (start, stop, finish) including the initiating user identity. For schedule-triggered runs where no authenticated user is involved, the `Username` field SHALL be set to `"system"` and `UserId` SHALL be left null. | High     |
| FR-012 | The system SHALL allow authorized users to retrieve station run history for a specified date range via REST API.             | Medium   |
| FR-013 | The system SHALL allow authorized users to read and update per-zone metadata (name, display colour).                        | Medium   |
| FR-014 | The system SHALL broadcast zone start, stop, and progress events to all connected SignalR clients in real time.              | High     |
| FR-015 | The system SHALL publish zone start, stop, progress, and zone-update events to a configured MQTT broker.                    | Medium   |
| FR-016 | The system SHALL automatically reconnect to the MQTT broker after disconnection using exponential back-off (capped at 30 s). | Medium   |
| FR-017 | The system SHALL automatically apply pending EF Core database migrations on startup.                                         | High     |
| FR-018 | The system SHALL detect whether GPIO hardware is available (`/dev/gpiomem`) and select the real or fake hardware driver accordingly at startup. | High |
| FR-019 | The system SHALL expose a Swagger/OpenAPI UI at `/sprinkler/swagger` for API exploration.                                   | Low      |
| FR-020 | The `Sannel.House.Sprinklers.Shared` NuGet package SHALL provide a typed `SprinklersClient` with methods covering all v1 API operations and SignalR event subscriptions. | Medium |
| FR-021 | The system SHALL provide a browser-based management UI (`Sannel.House.Sprinklers.Web`) enabling authenticated users to view real-time zone status, manually start and stop zones, manage schedule programs, edit zone metadata, and review run history. | High |
| FR-022 | The browser-based UI SHALL authenticate users against the same Azure AD tenant and enforce the same role-based access policies as the REST API. | High |
| FR-023 | The non-hardware fake driver SHALL read zone count from the `Sprinkler:Zones` configuration key, consistent with the real hardware driver. | High |

---

## Non-Functional Requirements

| ID      | Requirement                                                                                                              | Category        |
|---|---|---|
| NFR-001 | All REST API endpoints SHALL require a valid Azure AD JWT bearer token; unauthenticated requests SHALL be rejected with HTTP 401. | Security   |
| NFR-002 | Authorization SHALL be enforced via Azure AD app roles (`Roles.Sprinklers.*`, `Roles.ADMIN`); role absence SHALL return HTTP 403. | Security  |
| NFR-003 | TLS SHALL be supported for both the HTTPS Kestrel endpoint and the MQTT broker connection; certificate paths SHALL be configurable via `appsettings.json`. | Security |
| NFR-004 | All sensitive configuration (credentials, certificate passwords) SHALL be supplied via environment-mounted `appsettings.json` and SHALL NOT be committed to source control. | Security |
| NFR-005 | The zone polling loop SHALL check for due runs every 10 seconds with negligible CPU overhead during idle periods.        | Performance     |
| NFR-006 | Zone timer accuracy SHALL be within up to 500 ms after the configured run duration elapses (governed by the 500 ms poll interval; the timer can only fire late, never early).       | Performance     |
| NFR-007 | The Docker image SHALL support `linux/amd64` and `linux/arm64` architectures.                                            | Portability     |
| NFR-008 | The self-contained single-file publish SHALL run on `linux-arm64` without a pre-installed .NET runtime.                  | Portability     |
| NFR-009 | The SQLite database SHALL be persisted to a volume-mounted `Data/` directory; data SHALL survive container restarts.     | Reliability     |
| NFR-010 | Data protection keys SHALL be persisted to the `Data/` directory so that token validation survives restarts.             | Reliability     |
| NFR-011 | The `ScheduleWorker` and `SprinklerWorker` background workers SHALL log errors and the exception SHALL propagate to cause a host restart on unhandled failure. | Reliability |
| NFR-012 | The API SHALL be versioned; the current version is `v1.0`, and routes SHALL use the `/sprinkler/api/v{version}/` prefix. | Maintainability |
| NFR-013 | Application Insights telemetry SHALL be supported when a connection string is configured.                                | Observability   |

---

## Assumptions & Constraints

- The target hardware is a Raspberry Pi (any model with GPIO support) running the OpenSprinkler board. The presence of `/dev/gpiomem` is the sole runtime indicator of hardware availability.
- The Docker container user must belong to the host `gpio` group (GID typically `993`) for GPIO access.
- Azure AD (Microsoft Entra ID) is used as the sole identity provider; no local user database is maintained.
- The number of zones is fixed at deployment time via the `Sprinkler:Zones` configuration key and cannot be changed without restarting the service.
- Only one zone can be active at any given time; the hardware does not support concurrent zone operation.
- Sequential zone runs have an inherent gap of up to 10 seconds between them, governed by the `ScheduleWorker` polling interval. This is by design.
- Each schedule program uses exactly one scheduling mode: either a set of days-of-week (e.g., Monday, Wednesday, Friday) combined with a fixed start time, or an interval-in-days with an anchor start date (e.g., every 3 days starting from a given date). Mixed use of both modes within a single program is not permitted.
- The MQTT broker is external and must be provisioned separately; the system does not include or manage an MQTT broker.
- All times are local time on the host; the `DateTimeOffset` binary storage convention is a SQLite limitation workaround.
- .NET Aspire is used exclusively for local development orchestration and is not part of the production Docker deployment.

---

## Success Criteria

- **SC-01** — A schedule program can be created, enabled, and will automatically water the correct zones at the correct times the following day without manual intervention.
- **SC-02** — A manual zone start via the REST API activates GPIO output, and the zone stops automatically when the run duration elapses (within 500 ms tolerance).
- **SC-03** — A connected SignalR client receives start, progress (every ~500 ms), and stop events for every zone run without requiring polling.
- **SC-04** — The Docker image deploys and passes application startup (including DB migration) on both `linux/amd64` and `linux/arm64` hosts.
- **SC-05** — All API endpoints return HTTP 401 for requests without a valid JWT and HTTP 403 for requests with an insufficient role.
- **SC-06** — The service recovers from an MQTT broker disconnection and re-establishes the connection without a service restart.
- **SC-07** — An authenticated browser user can view real-time zone status, start/stop zones, and manage schedules using the Blazor WASM management UI served directly from the API host.

---

## Decisions & Resolved Questions

| # | Question | Decision |
|---|---|---|
| OQ-1 | Should `FakeHardware` simulate a configurable zone count? | **Yes** — `FakeHardware` shall read zone count from `Sprinkler:Zones` configuration, consistent with `OpenSprinklerHardware`. (FR-023) |
| OQ-2 | Is rain delay / schedule skip logic required? | **Future scope** — weather-based skip logic is planned for a future version but is not in the current release. |
| OQ-3 | Should watering history be purgeable via API? | **No** — manual database operations are acceptable for retention management; no DELETE endpoint is required. |
| OQ-4 | Is the ~10-second gap between sequential zone runs by design? | **Yes, by design** — `ScheduleWorker` operates on a fixed 10-second polling cycle. Immediate zone chaining is not a requirement. |
| OQ-A | Should the SignalR hub enforce authentication server-side? | **Yes** — `MessageHub` shall be decorated with `[Authorize]` to reject unauthenticated WebSocket connections at the server. |
| OQ-B | Should the manual Start API support queuing multiple zones? | **Yes** — `POST /Start` accepts a list of zone+duration pairs; `SprinklerWorker` maintains an internal queue. (FR-001 updated) |
| OQ-C | Should `FakeHardware` zone count be configurable? | **Yes** — reads from `Sprinkler:Zones` config. Resolved as FR-023. |
| OQ-D | Should `ScheduleWorker`'s generation loop be `Task`-returning with cancellation? | **Yes** — restructured as a `Task`-returning method with `CancellationToken` support so unhandled exceptions propagate to the host rather than being silently swallowed. |
