# Шаг 9. Развёртывание

---

## 9.1. Dockerfile

**Файл:** `TechnicalSupportService.SUTP/Dockerfile`

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj", "SUTP/"]
COPY ["TechnicalSupportService.Data/TechnicalSupportService.Data.csproj", "Data/"]
COPY ["TechnicalSupportService.Core/TechnicalSupportService.Core.csproj", "Core/"]
RUN dotnet restore "SUTP/TechnicalSupportService.SUTP.csproj"

COPY TechnicalSupportService.SUTP/ SUTP/
COPY TechnicalSupportService.Data/ Data/
COPY TechnicalSupportService.Core/ Core/

WORKDIR /src/SUTP
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
VOLUME ["/var/data/support-files"]
ENTRYPOINT ["dotnet", "TechnicalSupportService.SUTP.dll"]
```

---

## 9.2. docker-compose.yml

**Файл:** корень решения

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: support_db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - pgdata:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d support_db"]
      interval: 5s
      timeout: 5s
      retries: 5

  app:
    build:
      context: .
      dockerfile: TechnicalSupportService.SUTP/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=support_db;Username=postgres;Password=postgres
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - FileStorage__LocalPath=/var/data/support-files
    ports:
      - "8080:8080"
    volumes:
      - files:/var/data/support-files
    depends_on:
      postgres:
        condition: service_healthy

volumes:
  pgdata:
  files:
```

---

## 9.3. .gitignore

**Файл:** корень решения

```
bin/
obj/
.vs/
*.user
*.suo
appsettings.Development.json
files/
TestResults/
```

---

## 9.4. Локальный запуск (без Docker)

```powershell
# 1. Убедитесь что PostgreSQL запущен и БД создана
# 2. Настройте connection string в appsettings.json
# 3. Создайте миграцию и примените
dotnet ef migrations add InitialCreate --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
dotnet ef database update --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP

# 4. Запустите
dotnet run --project TechnicalSupportService.SUTP

# 5. Откройте http://localhost:5000
# 6. Войдите под admin@company.com / Admin@123
```
