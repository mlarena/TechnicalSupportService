# Шаг 9. Развёртывание

## 9.1. Окружения

| Окружение | Назначение | Конфигурация |
|-----------|------------|---------------|
| **Development** | Локальная разработка | `appsettings.Development.json`, PostgreSQL локальная |
| **Staging** | Предпродакшен | `appsettings.Staging.json`, отдельная БД |
| **Production** | Продакшен | `appsettings.Production.json`, переменные окружения |

---

## 9.2. Dockerfile (опционально)

```dockerfile
# TechnicalSupportService.SUTP/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем .csproj и восстанавливаем зависимости
COPY ["TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj", "SUTP/"]
COPY ["TechnicalSupportService.Data/TechnicalSupportService.Data.csproj", "Data/"]
COPY ["TechnicalSupportService.Core/TechnicalSupportService.Core.csproj", "Core/"]
RUN dotnet restore "SUTP/TechnicalSupportService.SUTP.csproj"

# Копируем исходники и собираем
COPY . .
WORKDIR "/src/SUTP"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Точка монтирования для файлов
VOLUME ["/var/data/support-files"]

ENTRYPOINT ["dotnet", "TechnicalSupportService.SUTP.dll"]
```

---

## 9.3. docker-compose.yml

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: support_db
      POSTGRES_USER: app_user
      POSTGRES_PASSWORD: ${DB_PASSWORD:-secret}
    volumes:
      - pgdata:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U app_user -d support_db"]
      interval: 5s
      timeout: 5s
      retries: 5

  app:
    build:
      context: .
      dockerfile: TechnicalSupportService.SUTP/Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=support_db;Username=app_user;Password=${DB_PASSWORD:-secret}
      - ASPNETCORE_ENVIRONMENT=Production
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

## 9.4. Переменные окружения (Production)

| Переменная | Описание | Пример |
|-----------|----------|--------|
| `ConnectionStrings__DefaultConnection` | Строка подключения к PostgreSQL | `Host=db;Database=support_db;...` |
| `ASPNETCORE_ENVIRONMENT` | Окружение | `Production` |
| `ASPNETCORE_URLS` | Адреса прослушивания | `http://+:8080` |
| `FileStorage__LocalPath` | Путь к файлам | `/var/data/support-files` |
| `DB_PASSWORD` | Пароль БД (для docker-compose) | `StrongP@ssw0rd!` |

---

## 9.5. Скрипт развёртывания (Linux Debian)

```bash
#!/bin/bash
# deploy.sh

set -e

echo "=== Развёртывание СУТП ==="

# 1. Установка .NET 10 Runtime (если не Docker)
# wget https://dot.net/v1/dotnet-install.sh
# chmod +x dotnet-install.sh
# ./dotnet-install.sh --channel 10.0 --runtime aspnetcore

# 2. Создание пользователя и директорий
sudo useradd -r -s /bin/false support-app || true
sudo mkdir -p /opt/support-app
sudo mkdir -p /var/data/support-files
sudo chown support-app:support-app /var/data/support-files

# 3. Копирование сборки
sudo cp -r publish/* /opt/support-app/

# 4. Создание systemd-сервиса
sudo tee /etc/systemd/system/support-app.service > /dev/null <<EOF
[Unit]
Description=Technical Support Service
After=network.target postgresql.service

[Service]
Type=notify
User=support-app
WorkingDirectory=/opt/support-app
ExecStart=/usr/bin/dotnet /opt/support-app/TechnicalSupportService.SUTP.dll
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://+:8080
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF

# 5. Запуск
sudo systemctl daemon-reload
sudo systemctl enable support-app
sudo systemctl start support-app

echo "=== Развёртывание завершено ==="
echo "Приложение доступно на http://localhost:8080"
```

---

## 9.6. Reverse proxy (nginx)

```nginx
server {
    listen 80;
    server_name support.company.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        client_max_body_size 60M;  # для загрузки файлов
    }
}
```

---

## 9.7. Миграции при развёртывании

### Вариант 1: Автоматические миграции при старте (для dev/staging)

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}
```

### Вариант 2: Ручные миграции (для production)

```bash
dotnet ef database update \
    --project TechnicalSupportService.Data \
    --startup-project TechnicalSupportService.SUTP \
    --connection "Host=...;Database=support_db;..."
```

---

## 9.8. Резервное копирование PostgreSQL

```bash
#!/bin/bash
# backup.sh — запускать по cron (ежедневно)

BACKUP_DIR="/var/backups/support-db"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
FILENAME="support_db_${TIMESTAMP}.sql.gz"

pg_dump -U app_user -h localhost support_db | gzip > "${BACKUP_DIR}/${FILENAME}"

# Удаление бэкапов старше 30 дней
find ${BACKUP_DIR} -name "*.sql.gz" -mtime +30 -delete
```

---

## 9.9. Мониторинг (базовый)

- **Health check endpoint:** `GET /health` — проверка подключения к БД
- **Логирование:** Serilog или встроенный `ILogger` → файл + stdout
- **Метрики (опционально):** Prometheus + Grafana

```csharp
// Program.cs
app.MapHealthChecks("/health");
```
