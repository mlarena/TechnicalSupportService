# Прогресс выполнения проекта СУТП

**Проект:** TechnicalSupportService
**Корневая директория:** `C:\dev\TechnicalSupportService\`
**Дата начала:** ___
**Текущий статус:** 🔴 Не начат

---

## Легенда статусов

| Символ | Статус | Описание |
|--------|--------|----------|
| 🔴 | Не начат | Задача не выполнялась |
| 🟡 | В работе | Задача выполняется |
| 🟢 | Выполнено | Задача завершена и проверена |
| 🔵 | Проверено | Задача протестирована |
| ⚪ | Пропущено | Задача не требуется |

---

## Этап 1. Создание решения и проектов

| # | Задача | Файл/Папка | Статус |
|---|--------|------------|--------|
| 1.1 | Создать solution `TechnicalSupportService.sln` | `TechnicalSupportService.sln` | 🔴 |
| 1.2 | Создать проект `TechnicalSupportService.Core` (Class Library) | `TechnicalSupportService.Core/` | 🔴 |
| 1.3 | Создать проект `TechnicalSupportService.Data` (Class Library) | `TechnicalSupportService.Data/` | 🔴 |
| 1.4 | Создать проект `TechnicalSupportService.SUTP` (MVC Web App) | `TechnicalSupportService.SUTP/` | 🔴 |
| 1.5 | Добавить проекты в solution | `TechnicalSupportService.sln` | 🔴 |
| 1.6 | Добавить зависимости между проектами (Data→Core, SUTP→Data+Core) | `*.csproj` | 🔴 |
| 1.7 | Установить NuGet-пакеты (см. 01-project-structure.md §1.7) | `*.csproj` | 🔴 |
| 1.8 | Создать структуру папок в каждом проекте | См. 01-project-structure.md §1.4 | 🔴 |

---

## Этап 2. Сущности и БД

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 2.1 | Создать enum'ы в `Core/Enums/` | `Core/Enums/*.cs` (7 файлов) | 🔴 |
| 2.2 | Создать константы ролей | `Core/Constants/Roles.cs` | 🔴 |
| 2.3 | Создать сущности в `Data/Entities/` | `Data/Entities/*.cs` (8 файлов) | 🔴 |
| 2.4 | Создать `ApplicationDbContext` | `Data/Context/ApplicationDbContext.cs` | 🔴 |
| 2.5 | Создать Fluent API-конфигурации | `Data/Configurations/*.cs` (8 файлов) | 🔴 |
| 2.6 | Создать миграцию `InitialCreate` | `Data/Migrations/` | 🔴 |
| 2.7 | Применить миграцию к БД | PostgreSQL | 🔴 |

---

## Этап 3. Identity, DI, конфигурация

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 3.1 | Настроить `appsettings.json` | `SUTP/appsettings.json` | 🔴 |
| 3.2 | Настроить `appsettings.Development.json` | `SUTP/appsettings.Development.json` | 🔴 |
| 3.3 | Настроить `Program.cs` (Identity, DI, Middleware) | `SUTP/Program.cs` | 🔴 |
| 3.4 | Создать `Infrastructure/ServiceCollectionExtensions.cs` | `SUTP/Infrastructure/ServiceCollectionExtensions.cs` | 🔴 |
| 3.5 | Создать `Infrastructure/SeedData.cs` с 4 пользователями | `SUTP/Infrastructure/SeedData.cs` | 🔴 |

**Seed-пользователи для тестирования:**

| Роль | Email | Пароль | FullName |
|------|-------|--------|----------|
| Admin | `admin@company.com` | `Admin@123` | Администратор Системы |
| Engineer | `engineer@company.com` | `Engineer@123` | Инженер Техподдержки |
| Manager | `manager@company.com` | `Manager@123` | Менеджер Проектов |
| Applicant | `applicant@company.com` | `Applicant@123` | Иван Заявитель |

---

## Этап 4. DTO и интерфейсы сервисов

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 4.1 | Создать DTO для Tickets | `Core/DTOs/Ticket*.cs` | 🔴 |
| 4.2 | Создать DTO для Comments, Attachments | `Core/DTOs/Comment*.cs`, `Core/DTOs/Attachment*.cs` | 🔴 |
| 4.3 | Создать DTO для Products, Departments, Users, Dashboard | `Core/DTOs/*.cs` | 🔴 |
| 4.4 | Создать `PagedResult<T>` | `Core/DTOs/PagedResult.cs` | 🔴 |
| 4.5 | Создать интерфейсы сервисов | `Core/Interfaces/*.cs` (8 файлов) | 🔴 |
| 4.6 | Создать сервисные исключения | `Core/Exceptions/*.cs` (3 файла) | 🔴 |

---

## Этап 5. Реализация сервисов

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 5.1 | Реализовать `NumberGeneratorService` | `SUTP/Services/NumberGeneratorService.cs` | 🔴 |
| 5.2 | Реализовать `TicketService` | `SUTP/Services/TicketService.cs` | 🔴 |
| 5.3 | Реализовать `CommentService` | `SUTP/Services/CommentService.cs` | 🔴 |
| 5.4 | Реализовать `AttachmentService` | `SUTP/Services/AttachmentService.cs` | 🔴 |
| 5.5 | Реализовать `LocalFileStorageService` | `SUTP/Services/LocalFileStorageService.cs` | 🔴 |
| 5.6 | Реализовать `AuditService` | `SUTP/Services/AuditService.cs` | 🔴 |
| 5.7 | Реализовать `ProductService` | `SUTP/Services/ProductService.cs` | 🔴 |
| 5.8 | Реализовать `DepartmentService` | `SUTP/Services/DepartmentService.cs` | 🔴 |
| 5.9 | Реализовать `UserService` | `SUTP/Services/UserService.cs` | 🔴 |
| 5.10 | Реализовать `DashboardService` | `SUTP/Services/DashboardService.cs` | 🔴 |

---

## Этап 6. Контроллеры

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 6.1 | Создать `AccountController` | `SUTP/Controllers/AccountController.cs` | 🔴 |
| 6.2 | Создать `DashboardController` | `SUTP/Controllers/DashboardController.cs` | 🔴 |
| 6.3 | Создать `TicketsController` | `SUTP/Controllers/TicketsController.cs` | 🔴 |
| 6.4 | Создать `AdminController` | `SUTP/Controllers/AdminController.cs` | 🔴 |
| 6.5 | Создать `FilesController` | `SUTP/Controllers/FilesController.cs` | 🔴 |
| 6.6 | Создать `ExceptionHandlingMiddleware` | `SUTP/Middleware/ExceptionHandlingMiddleware.cs` | 🔴 |
| 6.7 | Создать `AuditActionFilter` | `SUTP/Filters/AuditActionFilter.cs` | 🔴 |

---

## Этап 7. UI — Views, CSS, JS

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 7.1 | Создать `_Layout.cshtml` | `SUTP/Views/Shared/_Layout.cshtml` | 🔴 |
| 7.2 | Создать `_ValidationScriptsPartial.cshtml` | `SUTP/Views/Shared/_ValidationScriptsPartial.cshtml` | 🔴 |
| 7.3 | Создать `_TicketStatusBadge.cshtml` | `SUTP/Views/Shared/_TicketStatusBadge.cshtml` | 🔴 |
| 7.4 | Создать `_PriorityBadge.cshtml` | `SUTP/Views/Shared/_PriorityBadge.cshtml` | 🔴 |
| 7.5 | Создать Views для Account | `SUTP/Views/Account/*.cshtml` (5 файлов) | 🔴 |
| 7.6 | Создать Views для Dashboard | `SUTP/Views/Dashboard/Index.cshtml` | 🔴 |
| 7.7 | Создать Views для Tickets | `SUTP/Views/Tickets/*.cshtml` (4 файла) | 🔴 |
| 7.8 | Создать Views для Admin | `SUTP/Views/Admin/*.cshtml` (5 файлов) | 🔴 |
| 7.9 | Создать CSS (темы, компоненты) | `SUTP/wwwroot/css/*.css` | 🔴 |
| 7.10 | Создать JS (тема, загрузка файлов, комментарии) | `SUTP/wwwroot/js/*.js` | 🔴 |
| 7.11 | Установить Bootstrap, jQuery через LibMan | `SUTP/wwwroot/lib/` | 🔴 |

---

## Этап 8. Тестирование

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 8.1 | Создать тестовый проект UnitTests | `tests/TechnicalSupportService.UnitTests/` | 🔴 |
| 8.2 | Создать тестовый проект IntegrationTests | `tests/TechnicalSupportService.IntegrationTests/` | 🔴 |
| 8.3 | Написать unit-тесты нумерации | `UnitTests/Services/NumberGeneratorServiceTests.cs` | 🔴 |
| 8.4 | Написать unit-тесты бизнес-логики | `UnitTests/Services/TicketServiceTests.cs` | 🔴 |
| 8.5 | Написать integration-тесты контроллеров | `IntegrationTests/Controllers/*.cs` | 🔴 |
| 8.6 | Запустить все тесты, убедиться что проходят | `dotnet test` | 🔴 |

---

## Этап 9. Развёртывание

| # | Задача | Файл | Статус |
|---|--------|------|--------|
| 9.1 | Создать `Dockerfile` | `TechnicalSupportService.SUTP/Dockerfile` | 🔴 |
| 9.2 | Создать `docker-compose.yml` | `docker-compose.yml` | 🔴 |
| 9.3 | Создать `deploy.sh` | `deploy.sh` | 🔴 |
| 9.4 | Создать `.dockerignore` | `.dockerignore` | 🔴 |
| 9.5 | Создать `.gitignore` | `.gitignore` | 🔴 |

---

## Этап 10. Проверка (Smoke Test)

| # | Задача | Статус |
|---|--------|--------|
| 10.1 | Приложение запускается без ошибок | 🔴 |
| 10.2 | Страница входа отображается | 🔴 |
| 10.3 | Вход под Admin работает | 🔴 |
| 10.4 | Вход под Engineer работает | 🔴 |
| 10.5 | Вход под Manager работает | 🔴 |
| 10.6 | Вход под Applicant работает | 🔴 |
| 10.7 | Создание заявки работает | 🔴 |
| 10.8 | Номер заявки генерируется корректно | 🔴 |
| 10.9 | Смена статуса работает | 🔴 |
| 10.10 | Комментарии работают | 🔴 |
| 10.11 | Загрузка файлов работает | 🔴 |
| 10.12 | История отображается | 🔴 |
| 10.13 | Дашборд отображается | 🔴 |
| 10.14 | Админ-панель работает | 🔴 |
| 10.15 | Переключение темы работает | 🔴 |

---

## Сводка

| Этап | Всего задач | Выполнено | Прогресс |
|------|-------------|-----------|----------|
| 1. Решение и проекты | 8 | 0 | 0% |
| 2. Сущности и БД | 7 | 0 | 0% |
| 3. Identity, DI | 5 | 0 | 0% |
| 4. DTO и интерфейсы | 6 | 0 | 0% |
| 5. Сервисы | 10 | 0 | 0% |
| 6. Контроллеры | 7 | 0 | 0% |
| 7. UI | 11 | 0 | 0% |
| 8. Тестирование | 6 | 0 | 0% |
| 9. Развёртывание | 5 | 0 | 0% |
| 10. Smoke Test | 15 | 0 | 0% |
| **ИТОГО** | **80** | **0** | **0%** |
