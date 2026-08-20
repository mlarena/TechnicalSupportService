# Шаг 1. Введение, структура решения и общие требования

## 1.1. Назначение

Разработать корпоративное веб-приложение для автоматизации процессов приёма, учёта, обработки и хранения заявок в службе технической поддержки компании, которая разрабатывает ПО и технические изделия.

## 1.2. Цели

- Централизованное хранение заявок и сопутствующих документов.
- Разграничение доступа по ролям.
- Полная история изменений по каждой заявке.
- Уникальная, человеко-читаемая нумерация заявок с ежемесячным сбросом.

---

## 1.3. Архитектура

- **Бэкенд:** .NET 10 (Core), ASP.NET Core MVC.
- **ORM:** Entity Framework Core 10.
- **БД:** PostgreSQL 16+.
- **Фронтенд:** Razor Pages / MVC Views + Bootstrap (допускается использование HTMX для динамики, но не обязательно).
- **Аутентификация:** ASP.NET Core Identity с кастомизацией.
- **Общий вид:** возможность изменять темы — светлая, тёмная.

---

## 1.4. Структура решения (проекты)

Решение (`TechnicalSupportService.sln`) состоит из трёх проектов:

| Проект | Тип | Назначение |
|--------|-----|------------|
| **TechnicalSupportService.Data** | Class Library (.NET 10) | Сущности (Entities), DbContext, миграции EF Core, enum'ы, конфигурации маппинга (Fluent API) |
| **TechnicalSupportService.Core** | Class Library (.NET 10) | Интерфейсы сервисов (IService), DTO/ViewModels, бизнес-правила и валидация, вспомогательные утилиты. **Не зависит** от Data и SUTP |
| **TechnicalSupportService.SUTP** | ASP.NET Core MVC Web App (.NET 10) | Контроллеры, Razor Views, реализации сервисов, wwwroot (CSS/JS), конфигурация DI, Identity, запуск приложения |

### Зависимости между проектами

```
TechnicalSupportService.Core  (нет зависимостей)
        ↑
TechnicalSupportService.Data  (зависит от Core — использует интерфейсы и DTO)
        ↑
TechnicalSupportService.SUTP  (зависит от Data и Core — подключает DI, реализует сервисы)
```

### Структура папок внутри проектов

```
TechnicalSupportService.Core/
├── Interfaces/           # IService-интерфейсы (ITicketService, IFileStorageService и т.д.)
├── DTOs/                 # Объекты передачи данных (TicketDto, TicketCreateDto и т.д.)
├── Enums/                # Общие enum'ы (при дублировании с Data — источник истины в Core)
├── Constants/            # Константы (роли, лимиты файлов, допустимые расширения)
├── Validation/           # FluentValidation-валидаторы (опционально)
└── Helpers/              # Вспомогательные утилиты (форматирование номера и т.д.)

TechnicalSupportService.Data/
├── Entities/             # Классы сущностей (Ticket, TicketHistory, Attachment, Product, Comment, ...)
├── Enums/                # Enum'ы, специфичные для БД (если не вынесены в Core)
├── Configurations/       # Fluent API-конфигурации (IEntityTypeConfiguration<T>)
├── Migrations/           # Миграции EF Core
├── Context/              # ApplicationDbContext
└── Extensions/           # Методы расширения для DbContext

TechnicalSupportService.SUTP/
├── Controllers/          # MVC-контроллеры (TicketsController, DashboardController, ...)
├── Services/             # Реализации сервисов из Core.Interfaces
├── ViewModels/           # View-модели (если отличаются от DTO)
├── Views/                # Razor Views (по папкам контроллеров + Shared/)
│   ├── Shared/           # _Layout, _ValidationScriptsPartial, Error
│   ├── Dashboard/
│   ├── Tickets/
│   ├── Account/
│   └── Admin/
├── wwwroot/
│   ├── css/              # Кастомные стили + темы (light/dark)
│   ├── js/               # Кастомные скрипты (drag-and-drop загрузки, HTMX и т.д.)
│   ├── lib/              # Bootstrap, jQuery (через LibMan или npm)
│   └── images/           # Логотип, иконки
├── Filters/              # Action filters (аудит, обработка ошибок)
├── Middleware/            # Кастомные middleware (логирование, обработка исключений)
├── Infrastructure/       # Регистрация DI, конфигурация Identity
├── appsettings.json
├── Program.cs
└── appsettings.Development.json
```

### Ключевые NuGet-пакеты

| Пакет | Назначение |
|-------|------------|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity + EF Core интеграция |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Провайдер EF Core для PostgreSQL |
| `Microsoft.EntityFrameworkCore.Tools` | Миграции (dotnet ef) |
| `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` | Горячая перезагрузка Razor при разработке |
| `Minio` или `AWSSDK.S3` (опционально) | S3-хранилище для файлов (если не локальный диск) |

---

## 1.5. Требования к развёртыванию

- Linux Debian (контейнеризация Docker — опционально).
- Конфигурация через `appsettings.json` и переменные окружения.

---

## 1.6. Этапы разработки (ориентировочно)

| Этап | Описание |
|------|----------|
| 1 | Проектирование БД, настройка окружения, Identity |
| 2 | Реализация ролевой модели и авторизации |
| 3 | CRUD заявок + генерация номеров |
| 4 | Версионность и история |
| 5 | Работа с файлами |
| 6 | Интерфейс (все экраны) |
| 7 | Тестирование, отладка, нагрузочное тестирование |
| 8 | Документация, развёртывание |
