# EasyStock

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

## Tech stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core + PostgreSQL
- Docker

## License

MIT