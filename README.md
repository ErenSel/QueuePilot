# QueuePilot<

## Project Purpose & Design Goals

QueuePilot was built as a deliberately engineered backend system rather than a feature demo.

The goal of this project is to demonstrate how a real-world backend service should behave under failure, concurrency, and change — not just under happy paths.

Key design goals:

- **Strict Clean Architecture**  
  The domain and application layers are kept free from infrastructure concerns to enforce long-term maintainability and testability.

- **Explicit failure semantics**  
  Business and infrastructure failures (authentication errors, messaging failures, validation issues) are surfaced intentionally with correct HTTP semantics rather than generic 500 responses.

- **Event-driven reliability**  
  Ticket lifecycle events are published via RabbitMQ with idempotent consumers, retry backoff, and effective dead-lettering to prevent message loss or infinite retry loops.

- **Security-first authentication**  
  JWT access and refresh token flows are implemented with rotation, revocation, and role-based access control, reflecting production-grade auth expectations.

- **Deterministic integration testing**  
  Integration tests use Testcontainers to spin up isolated PostgreSQL instances, ensuring tests are repeatable and environment-independent.

This project is intentionally scoped as a single service to focus on correctness, architectural discipline, and operational behavior rather than breadth of features.


## Prerequisites
- .NET SDK 8.0
- Docker (for running PostgreSQL and RabbitMQ via compose)

## Configuration
QueuePilot requires a JWT signing secret and a database connection string.

### Required environment variables
| Variable | Description | Example |
| --- | --- | --- |
| `Jwt__Secret` | JWT signing secret used by the API. | `replace-with-strong-secret` |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string. | `Host=localhost;Port=5433;Database=QueuePilotDb;Username=postgres;Password=postgres` |

### Development helpers
Copy `.env.example` to `.env` as a reminder of required settings. When using Docker Compose, `.env` is also used to populate the `Jwt__Secret` value into the API container.

```bash
cp .env.example .env
```

## Docker Compose workflows
### 1) Infra only (PostgreSQL + RabbitMQ)
Use this flow when you want to run the API via `dotnet run` while relying on Docker for infra.

```bash
docker-compose up -d postgres rabbitmq
export Jwt__Secret="replace-with-strong-secret"
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5433;Database=QueuePilotDb;Username=postgres;Password=postgres"
dotnet run --project src/QueuePilot.Api
```

### 2) Full compose (optional)
Use this flow to run the API, PostgreSQL, and RabbitMQ in containers.

```bash
docker-compose up -d
```

## Ports
| Service | Port | Notes |
| --- | --- | --- |
| API | `5000` | Docker Compose maps `localhost:5000` → container port `8080`.
| API (dotnet run) | `5132` | Default HTTP port from `launchSettings.json`.
| PostgreSQL | `5433` | Docker Compose maps `localhost:5433` → container port `5432`.
| RabbitMQ broker | `5672` | AMQP broker port.
| RabbitMQ management UI | `15672` | Visit `http://localhost:15672` (user: `user`, password: `password`). |

## Run (local)
```bash
export Jwt__Secret="replace-with-strong-secret"
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5433;Database=QueuePilotDb;Username=postgres;Password=postgres"
dotnet run --project src/QueuePilot.Api
```
## Tests

Integration tests use Testcontainers and require Docker to be running.

Ensure Docker is running before executing:

```bash
dotnet test
```


## Troubleshooting
- **Port already in use**: Stop the conflicting service or change the port mapping in `docker-compose.yml` (for example, map `5001:8080` for the API).
- **Database migrations**: If the API fails due to missing tables, run:
  ```bash
  dotnet ef database update --project src/QueuePilot.Infrastructure --startup-project src/QueuePilot.Api
  ```
- **Docker not running**: Ensure Docker Desktop / the Docker daemon is running, then retry `docker-compose up -d`.
