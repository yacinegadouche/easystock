# EasyStock
[![CI](https://github.com/yacinegadouche/easystock/actions/workflows/ci.yml/badge.svg)](https://github.com/yacinegadouche/easystock/actions/workflows/ci.yml)

IT asset and inventory management web application built with ASP.NET Core MVC (.NET 10) and PostgreSQL.

> Re-implementation of the EasyStock team project from the INM5151 course (Software Engineering Project) at UQAM, rebuilt from scratch with PostgreSQL instead of SQL Server.

## Getting started

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run locally

```bash
cp .env.example .env            # then set POSTGRES_PASSWORD
docker compose up -d
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Port=5432;Database=easystock;Username=easystock;Password=<your-password>" \
  --project src/EasyStock.Web
dotnet user-secrets set "Seed:DemoPassword" "<choose-a-password>" --project src/EasyStock.Web
dotnet run --project src/EasyStock.Web
```

### Demo accounts

| Username | Role | Access |
|---|---|---|
| `admin` | Admin | Everything, including user management |
| `manager` | Manager | Inventory, history and reports |
| `operator` | Operator | Inventory only |

All demo accounts use the password set in `Seed:DemoPassword`.

Health check: `GET /health`

### Emails in development

`docker compose up -d` also starts [Mailpit](https://mailpit.axllent.org), a local SMTP server that catches every email.
Low stock alerts are visible at http://localhost:8025. No real email is ever sent in development.

In production, configure SMTP with environment variables, for example:
`Email__Enabled=true`, `Email__Host=smtp.example.com`, `Email__Port=587`, `Email__Security=StartTls`,
`Email__Username=...`, `Email__Password=...`, `Email__SenderEmail=...`, `Email__RecipientEmail=...`.

## Tests

```bash
dotnet test
```

Docker must be running: integration tests start a disposable PostgreSQL 17 container with [Testcontainers](https://dotnet.testcontainers.org), so they run against the same database engine as production. Each test gets its own fresh database.

- **Unit tests**: domain rules, CSV export (escaping, BOM, formula injection), time zone conversion
- **Integration tests**: stock movements, assignments (including transaction rollback and concurrent assignment of the last unit), authentication, user and category rules
- **Web tests**: anonymous redirect to login, public health endpoint

## Tech stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core + PostgreSQL
- Docker

## License

MIT