# QueuePilot

## Prerequisites
- .NET SDK 8.0
- PostgreSQL (or update `ConnectionStrings:DefaultConnection`)
- RabbitMQ (optional for event processing)

## Configuration
QueuePilot requires a JWT signing secret. Provide it via environment variables or user-secrets.

### Environment variable
```bash
export Jwt__Secret="replace-with-strong-secret"
```

### User-secrets (development)
```bash
dotnet user-secrets init --project src/QueuePilot.Api
dotnet user-secrets set "Jwt:Secret" "replace-with-strong-secret" --project src/QueuePilot.Api
```

Copy `.env.example` to `.env` as a reminder of required settings.

## Run
```bash
dotnet run --project src/QueuePilot.Api
```

## Tests
```bash
dotnet test
```
