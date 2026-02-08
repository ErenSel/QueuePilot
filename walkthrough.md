# QueuePilot API Walkthrough

This walkthrough exercises the API end-to-end: register a user, log in, create a ticket, and confirm the event consumer logs.

## 0) Start services
Pick one of the workflows from the README (infra only or full compose). For infra only, ensure both PostgreSQL and RabbitMQ are running, then start the API with `dotnet run`.

## 1) Set the base URL
Use the base URL for your chosen workflow:

```bash
# Docker Compose API
export BASE_URL="http://localhost:5000"

# Or, if you're running `dotnet run` locally
# export BASE_URL="http://localhost:5132"
```

## 2) Register a user
```bash
curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@example.com","password":"P@ssword123!","role":"Customer"}'
```

## 3) Log in and capture the access token
```bash
curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@example.com","password":"P@ssword123!"}'
```

Copy the `accessToken` from the response and export it:

```bash
export ACCESS_TOKEN="<paste-access-token>"
```

## 4) Create a ticket
```bash
curl -s -X POST "$BASE_URL/api/tickets" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -d '{"title":"Printer is offline","description":"The front desk printer stopped responding."}'
```

## 5) Confirm event consumer logs
Creating a ticket publishes an event that is consumed by the hosted service. Look for log lines similar to:

```
>>> NOTIFICATION: New Ticket Created! ID: <ticketId>, Title: <title>
```

How to view logs:
- **Docker Compose**: `docker-compose logs -f api`
- **dotnet run**: watch the API console output directly.
