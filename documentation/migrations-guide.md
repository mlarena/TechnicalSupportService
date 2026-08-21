# Миграции EF Core — краткая инструкция

## Предварительные требования

1. **PostgreSQL запущен** и доступен по `localhost:5432`
2. **База данных создана** (или будет создана автоматически):
   ```sql
   CREATE DATABASE "SUTP";
   ```
3. **Connection string** настроен в `TechnicalSupportService.SUTP/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=SUTP;Username=postgres;Password=12345678"
   }
   ```

---

## Шаг 1. Создание миграции

Все команды выполняются из корневой директории решения (`C:\dev\TechnicalSupportService\`).

```powershell
dotnet ef migrations add InitialCreate --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
```

- `InitialCreate` — имя миграции (можно любое, например `AddTicketTables`)
- `--project` — проект, где лежит `DbContext` (Data)
- `--startup-project` — проект с `Program.cs` (SUTP)

В папке `TechnicalSupportService.Data/Migrations/` появятся файлы:
- `YYYYMMDDHHMMSS_InitialCreate.cs` — код миграции (создание таблиц)
- `YYYYMMDDHHMMSS_InitialCreate.Designer.cs` — метаданные
- `ApplicationDbContextModelSnapshot.cs` — снимок текущей модели

---

## Шаг 2. Применение миграции на пустую базу

### Вариант A — через CLI (рекомендуется)

```powershell
dotnet ef database update --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
```

Это создаст все таблицы в базе `SUTP` и запишет в таблицу `__EFMigrationsHistory` историю миграций.

### Вариант B — автоматически при запуске приложения

В `Program.cs` уже есть вызов `SeedData.InitializeAsync()`, который содержит:

```csharp
await db.Database.EnsureCreatedAsync();
```

> **Важно:** `EnsureCreated()` создаёт таблицы напрямую из модели **без миграций**. Это удобно для разработки, но **не подходит для продакшена** — нельзя добавить новые миграции к уже созданной базе.

Для продакшена замените `EnsureCreatedAsync()` на:

```csharp
await db.Database.MigrateAsync();
```

---

## Шаг 3. Проверка

```powershell
# Показать список применённых миграций
dotnet ef migrations list --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
```

Или подключитесь к PostgreSQL и проверьте:

```sql
SELECT * FROM "__EFMigrationsHistory";
```

---

## Типичные сценарии

### Добавить новую таблицу / изменить существующую

1. Измените сущность в `TechnicalSupportService.Data/Entities/`
2. Создайте миграцию:
   ```powershell
   dotnet ef migrations add AddNewTable --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
   ```
3. Примените:
   ```powershell
   dotnet ef database update --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
   ```

### Откатить последнюю миграцию

```powershell
dotnet ef database update PreviousMigrationName --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
```

### Удалить последнюю неприменённую миграцию

```powershell
dotnet ef migrations remove --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
```

### Полный сброс базы (удалить всё и создать заново)

```powershell
dotnet ef database drop --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
dotnet ef database update --project TechnicalSupportService.Data --startup-project TechnicalSupportService.SUTP
```

---

## Частые ошибки

| Ошибка | Причина | Решение |
|--------|---------|---------|
| `Unable to create an object of type 'ApplicationDbContext'` | Не найдена строка подключения | Проверьте `appsettings.json` в SUTP |
| `relation "__EFMigrationsHistory" does not exist` | База создана через `EnsureCreated` | Выполните `dotnet ef database update` |
| `Build failed` | Ошибка компиляции | Исправьте ошибки, затем повторите |
| `No project was found` | Неверный путь к проекту | Убедитесь, что `.csproj` файлы существуют |

---

## Структура папки миграций

```
TechnicalSupportService.Data/
├── Migrations/
│   ├── 20260820000000_InitialCreate.cs
│   ├── 20260820000000_InitialCreate.Designer.cs
│   └── ApplicationDbContextModelSnapshot.cs
├── Context/
│   └── ApplicationDbContext.cs
├── Entities/
│   └── ... (все сущности)
└── Configurations/
    └── ... (Fluent API конфигурации)
```
