# Шаг 1. Создание решения и структура проекта

**Корневая директория:** `C:\dev\TechnicalSupportService\`

---

## 1.1. Создание решения и проектов

Выполнить команды в PowerShell из корневой директории:

```powershell
# Создание решения
dotnet new sln -n TechnicalSupportService -o . --force

# Создание проектов
dotnet new classlib -n TechnicalSupportService.Core -o TechnicalSupportService.Core --framework net10.0
dotnet new classlib -n TechnicalSupportService.Data -o TechnicalSupportService.Data --framework net10.0
dotnet new mvc -n TechnicalSupportService.SUTP -o TechnicalSupportService.SUTP --framework net10.0 --no-https

# Добавление проектов в решение
dotnet sln add TechnicalSupportService.Core/TechnicalSupportService.Core.csproj
dotnet sln add TechnicalSupportService.Data/TechnicalSupportService.Data.csproj
dotnet sln add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj

# Зависимости между проектами
dotnet add TechnicalSupportService.Data/TechnicalSupportService.Data.csproj reference TechnicalSupportService.Core/TechnicalSupportService.Core.csproj
dotnet add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj reference TechnicalSupportService.Data/TechnicalSupportService.Data.csproj
dotnet add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj reference TechnicalSupportService.Core/TechnicalSupportService.Core.csproj
```

---

## 1.2. Установка NuGet-пакетов

```powershell
# Data проект
dotnet add TechnicalSupportService.Data/TechnicalSupportService.Data.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0-*
dotnet add TechnicalSupportService.Data/TechnicalSupportService.Data.csproj package Microsoft.EntityFrameworkCore.Tools --version 10.0.0-*
dotnet add TechnicalSupportService.Data/TechnicalSupportService.Data.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.0-*

# SUTP проект
dotnet add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 10.0.0-*
dotnet add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0-*
dotnet add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj package Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation --version 10.0.0-*
dotnet add TechnicalSupportService.SUTP/TechnicalSupportService.SUTP.csproj package Microsoft.EntityFrameworkCore.Tools --version 10.0.0-*
```

> Примечание: если .NET 10 preview, версии пакетов могут быть `10.0.0-preview.X`. Используйте `--prerelease` или укажите конкретную версию.

---

## 1.3. Удаление лишних файлов

```powershell
# Удалить auto-generated Class1.cs из classlib проектов
Remove-Item TechnicalSupportService.Core/Class1.cs -ErrorAction SilentlyContinue
Remove-Item TechnicalSupportService.Data/Class1.cs -ErrorAction SilentlyContinue
```

---

## 1.4. Создание структуры папок

```powershell
# Core
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Core/Interfaces
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Core/DTOs
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Core/Enums
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Core/Constants
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Core/Exceptions

# Data
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Data/Entities
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Data/Configurations
New-Item -ItemType Directory -Force -Path TechnicalSupportService.Data/Context

# SUTP
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Services
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Infrastructure
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Middleware
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Filters
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Views/Shared
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Views/Dashboard
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Views/Tickets
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Views/Account
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/Views/Admin
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/wwwroot/css/themes
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/wwwroot/js
New-Item -ItemType Directory -Force -Path TechnicalSupportService.SUTP/wwwroot/images

# Хранилище файлов
New-Item -ItemType Directory -Force -Path files
```

---

## 1.5. Итоговая структура файлов

```
C:\dev\TechnicalSupportService\
├── TechnicalSupportService.sln
├── task.txt
├── docs/
│   ├── 00-progress.md
│   ├── 01-project-structure.md
│   ├── 02-database-design.md
│   ├── 03-api-controllers.md
│   ├── 04-ui-views.md
│   ├── 05-auth-identity.md
│   ├── 06-services-logic.md
│   ├── 07-config-migrations.md
│   ├── 08-testing.md
│   ├── 09-deployment.md
│   └── 10-validation.md
├── files/                              # Хранилище загруженных файлов
├── TechnicalSupportService.Core/
│   ├── TechnicalSupportService.Core.csproj
│   ├── Constants/
│   │   └── Roles.cs
│   ├── DTOs/
│   │   ├── PagedResult.cs
│   │   ├── TicketDto.cs
│   │   ├── TicketListItemDto.cs
│   │   ├── TicketCreateDto.cs
│   │   ├── TicketUpdateDto.cs
│   │   ├── TicketFilterDto.cs
│   │   ├── TicketHistoryDto.cs
│   │   ├── CommentDto.cs
│   │   ├── CommentCreateDto.cs
│   │   ├── AttachmentDto.cs
│   │   ├── ProductDto.cs
│   │   ├── ProductCreateDto.cs
│   │   ├── DepartmentDto.cs
│   │   ├── DepartmentCreateDto.cs
│   │   ├── UserDto.cs
│   │   ├── UserFilterDto.cs
│   │   ├── UserCreateDto.cs
│   │   ├── UserUpdateDto.cs
│   │   └── DashboardDto.cs
│   ├── Enums/
│   │   ├── ProductType.cs
│   │   ├── Priority.cs
│   │   ├── TicketStatus.cs
│   │   ├── Category.cs
│   │   ├── Impact.cs
│   │   ├── Source.cs
│   │   └── ChangeType.cs
│   ├── Exceptions/
│   │   ├── NotFoundException.cs
│   │   ├── BusinessRuleException.cs
│   │   └── ForbiddenException.cs
│   └── Interfaces/
│       ├── ITicketService.cs
│       ├── ICommentService.cs
│       ├── IAttachmentService.cs
│       ├── IFileStorageService.cs
│       ├── INumberGeneratorService.cs
│       ├── IAuditService.cs
│       ├── IDashboardService.cs
│       ├── IProductService.cs
│       ├── IDepartmentService.cs
│       └── IUserService.cs
├── TechnicalSupportService.Data/
│   ├── TechnicalSupportService.Data.csproj
│   ├── Context/
│   │   └── ApplicationDbContext.cs
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   ├── ApplicationRole.cs
│   │   ├── Department.cs
│   │   ├── Product.cs
│   │   ├── Ticket.cs
│   │   ├── TicketHistory.cs
│   │   ├── Comment.cs
│   │   ├── Attachment.cs
│   │   ├── TicketNumberCounter.cs
│   │   └── AuditLog.cs
│   ├── Configurations/
│   │   ├── TicketConfiguration.cs
│   │   ├── TicketHistoryConfiguration.cs
│   │   ├── CommentConfiguration.cs
│   │   ├── AttachmentConfiguration.cs
│   │   ├── ProductConfiguration.cs
│   │   ├── DepartmentConfiguration.cs
│   │   ├── TicketNumberCounterConfiguration.cs
│   │   └── AuditLogConfiguration.cs
│   └── Migrations/
└── TechnicalSupportService.SUTP/
    ├── TechnicalSupportService.SUTP.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Controllers/
    │   ├── AccountController.cs
    │   ├── DashboardController.cs
    │   ├── TicketsController.cs
    │   ├── AdminController.cs
    │   └── FilesController.cs
    ├── Services/
    │   ├── TicketService.cs
    │   ├── CommentService.cs
    │   ├── AttachmentService.cs
    │   ├── LocalFileStorageService.cs
    │   ├── NumberGeneratorService.cs
    │   ├── AuditService.cs
    │   ├── ProductService.cs
    │   ├── DepartmentService.cs
    │   ├── UserService.cs
    │   └── DashboardService.cs
    ├── Infrastructure/
    │   ├── ServiceCollectionExtensions.cs
    │   └── SeedData.cs
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    ├── Filters/
    │   └── AuditActionFilter.cs
    ├── Views/
    │   ├── Shared/
    │   │   ├── _Layout.cshtml
    │   │   ├── _ValidationScriptsPartial.cshtml
    │   │   ├── _TicketStatusBadge.cshtml
    │   │   └── _PriorityBadge.cshtml
    │   ├── Account/
    │   │   ├── Login.cshtml
    │   │   ├── Register.cshtml
    │   │   ├── Profile.cshtml
    │   │   └── ChangePassword.cshtml
    │   ├── Dashboard/
    │   │   └── Index.cshtml
    │   ├── Tickets/
    │   │   ├── Index.cshtml
    │   │   ├── Create.cshtml
    │   │   ├── Details.cshtml
    │   │   └── Edit.cshtml
    │   └── Admin/
    │       ├── Users.cshtml
    │       ├── CreateUser.cshtml
    │       ├── EditUser.cshtml
    │       ├── Products.cshtml
    │       ├── CreateProduct.cshtml
    │       ├── EditProduct.cshtml
    │       ├── Departments.cshtml
    │       └── AuditLog.cshtml
    └── wwwroot/
        ├── css/
        │   ├── site.css
        │   └── themes/
        │       ├── variables-light.css
        │       └── variables-dark.css
        ├── js/
        │   ├── site.js
        │   ├── drag-drop-upload.js
        │   └── comments.js
        └── images/
```

---

## 1.6. Сводка NuGet-пакетов

| Проект | Пакет | Назначение |
|--------|-------|------------|
| Data | `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core провайдер PostgreSQL |
| Data | `Microsoft.EntityFrameworkCore.Tools` | dotnet ef миграции |
| Data | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity + EF Core |
| SUTP | `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core провайдер |
| SUTP | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Identity |
| SUTP | `Microsoft.EntityFrameworkCore.Tools` | dotnet ef |
| SUTP | `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` | Hot reload Razor |
