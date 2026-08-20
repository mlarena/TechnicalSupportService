# Шаг 4. UI — Views, CSS, JS — точный код

> Все файлы в проекте `TechnicalSupportService.SUTP`.

---

## 4.1. Views/Shared/_Layout.cshtml

```html
@using TechnicalSupportService.Core.Constants
<!DOCTYPE html>
<html lang="ru" data-theme="light">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - СУТП</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" />
    <link rel="stylesheet" href="~/css/themes/variables-light.css" />
    <link rel="stylesheet" href="~/css/themes/variables-dark.css" />
</head>
<body>
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
        <div class="container">
            <a class="navbar-brand" href="/">СУТП</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                @if (User.Identity?.IsAuthenticated == true)
                {
                    <ul class="navbar-nav me-auto">
                        <li class="nav-item">
                            <a class="nav-link" asp-controller="Dashboard" asp-action="Index">Дашборд</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-controller="Tickets" asp-action="Index">Заявки</a>
                        </li>
                        @if (User.IsInRole(Roles.Admin))
                        {
                            <li class="nav-item dropdown">
                                <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">Админ</a>
                                <ul class="dropdown-menu">
                                    <li><a class="dropdown-item" asp-controller="Admin" asp-action="Users">Пользователи</a></li>
                                    <li><a class="dropdown-item" asp-controller="Admin" asp-action="Products">Продукты</a></li>
                                    <li><a class="dropdown-item" asp-controller="Admin" asp-action="Departments">Отделы</a></li>
                                    <li><a class="dropdown-item" asp-controller="Admin" asp-action="AuditLog">Аудит</a></li>
                                </ul>
                            </li>
                        }
                    </ul>
                    <ul class="navbar-nav">
                        <li class="nav-item">
                            <button class="btn btn-outline-light btn-sm me-2" onclick="toggleTheme()">🌙</button>
                        </li>
                        <li class="nav-item dropdown">
                            <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                                @User.Identity.Name
                            </a>
                            <ul class="dropdown-menu dropdown-menu-end">
                                <li><a class="dropdown-item" asp-controller="Account" asp-action="Profile">Профиль</a></li>
                                <li><a class="dropdown-item" asp-controller="Account" asp-action="ChangePassword">Сменить пароль</a></li>
                                <li><hr class="dropdown-divider" /></li>
                                <li>
                                    <form asp-controller="Account" asp-action="Logout" method="post">
                                        <button type="submit" class="dropdown-item">Выход</button>
                                    </form>
                                </li>
                            </ul>
                        </li>
                    </ul>
                }
                else
                {
                    <ul class="navbar-nav ms-auto">
                        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Login">Вход</a></li>
                        <li class="nav-item"><a class="nav-link" asp-controller="Account" asp-action="Register">Регистрация</a></li>
                    </ul>
                }
            </div>
        </div>
    </nav>

    <div class="container mt-4">
        @if (TempData["SuccessMessage"] != null)
        {
            <div class="alert alert-success alert-dismissible fade show">
                @TempData["SuccessMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger alert-dismissible fade show">
                @TempData["ErrorMessage"]
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        }
        @RenderBody()
    </div>

    <footer class="text-center text-muted py-4 mt-5">
        <small>© @DateTime.Now.Year Система управления технической поддержкой</small>
    </footer>

    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

---

## 4.2. Views/Shared/_ValidationScriptsPartial.cshtml

```html
<script src="~/lib/jquery-validation/dist/jquery.validate.min.js"></script>
<script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js"></script>
```

---

## 4.3. Views/Shared/_TicketStatusBadge.cshtml

```html
@using TechnicalSupportService.Core.Enums
@model TicketStatus

@{
    var cssClass = Model switch
    {
        TicketStatus.New => "bg-info",
        TicketStatus.Assigned => "bg-primary",
        TicketStatus.InProgress => "bg-warning text-dark",
        TicketStatus.Resolved => "bg-success",
        TicketStatus.Closed => "bg-secondary",
        TicketStatus.Reopened => "bg-danger",
        _ => "bg-secondary"
    };
    var text = Model switch
    {
        TicketStatus.New => "Новая",
        TicketStatus.Assigned => "Назначена",
        TicketStatus.InProgress => "В работе",
        TicketStatus.Resolved => "Решена",
        TicketStatus.Closed => "Закрыта",
        TicketStatus.Reopened => "Переоткрыта",
        _ => Model.ToString()
    };
}
<span class="badge @cssClass">@text</span>
```

---

## 4.4. Views/Shared/_PriorityBadge.cshtml

```html
@using TechnicalSupportService.Core.Enums
@model Priority

@{
    var cssClass = Model switch
    {
        Priority.Low => "bg-success",
        Priority.Medium => "bg-info",
        Priority.High => "bg-warning text-dark",
        Priority.Critical => "bg-danger",
        _ => "bg-secondary"
    };
    var text = Model switch
    {
        Priority.Low => "Низкий",
        Priority.Medium => "Средний",
        Priority.High => "Высокий",
        Priority.Critical => "Критический",
        _ => Model.ToString()
    };
}
<span class="badge @cssClass">@text</span>
```

---

## 4.5. Views/Account/Login.cshtml

```html
@{
    ViewData["Title"] = "Вход";
}

<div class="row justify-content-center">
    <div class="col-md-5">
        <div class="card shadow">
            <div class="card-body">
                <h3 class="card-title text-center mb-4">Вход в систему</h3>
                <div asp-validation-summary="All" class="text-danger mb-3"></div>

                <form asp-action="Login" method="post">
                    <input type="hidden" name="returnUrl" value="@ViewData["ReturnUrl"]" />

                    <div class="mb-3">
                        <label class="form-label">Email</label>
                        <input type="email" name="email" class="form-control" required autofocus />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Пароль</label>
                        <input type="password" name="password" class="form-control" required />
                    </div>
                    <button type="submit" class="btn btn-primary w-100">Войти</button>
                </form>

                <div class="text-center mt-3">
                    <a asp-action="Register">Регистрация</a>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 4.6. Views/Account/Register.cshtml

```html
@{
    ViewData["Title"] = "Регистрация";
}

<div class="row justify-content-center">
    <div class="col-md-6">
        <div class="card shadow">
            <div class="card-body">
                <h3 class="card-title text-center mb-4">Регистрация</h3>
                <div asp-validation-summary="All" class="text-danger mb-3"></div>

                <form asp-action="Register" method="post">
                    <div class="mb-3">
                        <label class="form-label">ФИО *</label>
                        <input type="text" name="fullName" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Email *</label>
                        <input type="email" name="email" class="form-control" required />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Пароль *</label>
                        <input type="password" name="password" class="form-control" required minlength="8" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Подтверждение пароля *</label>
                        <input type="password" name="confirmPassword" class="form-control" required />
                    </div>
                    <button type="submit" class="btn btn-primary w-100">Зарегистрироваться</button>
                </form>

                <div class="text-center mt-3">
                    <a asp-action="Login">Уже есть аккаунт? Войти</a>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## 4.7. Views/Account/Profile.cshtml

```html
@model TechnicalSupportService.Data.Entities.ApplicationUser
@{
    ViewData["Title"] = "Профиль";
}

<h2>Мой профиль</h2>

@if (ViewBag.Message != null)
{
    <div class="alert alert-success">@ViewBag.Message</div>
}

<div class="row">
    <div class="col-md-6">
        <form asp-action="Profile" method="post">
            <div class="mb-3">
                <label class="form-label">Email</label>
                <input type="email" class="form-control" value="@Model.Email" disabled />
            </div>
            <div class="mb-3">
                <label class="form-label">ФИО *</label>
                <input type="text" name="fullName" class="form-control" value="@Model.FullName" required />
            </div>
            <div class="mb-3">
                <label class="form-label">Должность</label>
                <input type="text" name="position" class="form-control" value="@Model.Position" />
            </div>
            <div class="mb-3">
                <label class="form-label">Телефон</label>
                <input type="tel" name="phoneNumber" class="form-control" value="@Model.PhoneNumber" />
            </div>
            <button type="submit" class="btn btn-primary">Сохранить</button>
            <a asp-action="ChangePassword" class="btn btn-outline-secondary">Сменить пароль</a>
        </form>
    </div>
</div>
```

---

## 4.8. Views/Account/ChangePassword.cshtml

```html
@{
    ViewData["Title"] = "Смена пароля";
}

<h2>Смена пароля</h2>
<div asp-validation-summary="All" class="text-danger"></div>

<div class="row">
    <div class="col-md-6">
        <form asp-action="ChangePassword" method="post">
            <div class="mb-3">
                <label class="form-label">Текущий пароль</label>
                <input type="password" name="currentPassword" class="form-control" required />
            </div>
            <div class="mb-3">
                <label class="form-label">Новый пароль</label>
                <input type="password" name="newPassword" class="form-control" required minlength="8" />
            </div>
            <div class="mb-3">
                <label class="form-label">Подтверждение</label>
                <input type="password" name="confirmPassword" class="form-control" required />
            </div>
            <button type="submit" class="btn btn-primary">Изменить пароль</button>
        </form>
    </div>
</div>
```

---

## 4.9. Views/Dashboard/Index.cshtml

```html
@model TechnicalSupportService.Core.DTOs.DashboardDto
@{
    ViewData["Title"] = "Дашборд";
}

<h2>Дашборд</h2>

<div class="row mb-4">
    <div class="col-md-3">
        <div class="card text-white bg-info">
            <div class="card-body text-center">
                <h4>@(Model.TicketsByStatus.GetValueOrDefault("New", 0))</h4>
                <small>Новые</small>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-white bg-warning">
            <div class="card-body text-center">
                <h4>@(Model.TicketsByStatus.GetValueOrDefault("InProgress", 0))</h4>
                <small>В работе</small>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-white bg-success">
            <div class="card-body text-center">
                <h4>@(Model.TicketsByStatus.GetValueOrDefault("Resolved", 0))</h4>
                <small>Решены</small>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-white bg-danger">
            <div class="card-body text-center">
                <h4>@Model.CriticalCount</h4>
                <small>Критические</small>
            </div>
        </div>
    </div>
</div>

<h4>Последние заявки</h4>
<table class="table table-hover">
    <thead>
        <tr>
            <th>Номер</th>
            <th>Заголовок</th>
            <th>Статус</th>
            <th>Приоритет</th>
            <th>Продукт</th>
            <th>Дата</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var t in Model.RecentTickets)
        {
            <tr onclick="location.href='@Url.Action("Details", "Tickets", new { id = t.Id })'" style="cursor:pointer">
                <td>@t.Number</td>
                <td>@t.Title</td>
                <td>@await Html.PartialAsync("_TicketStatusBadge", t.Status)</td>
                <td>@await Html.PartialAsync("_PriorityBadge", t.Priority)</td>
                <td>@t.ProductName</td>
                <td>@t.CreatedAt.ToString("dd.MM.yyyy HH:mm")</td>
            </tr>
        }
    </tbody>
</table>
```

---

## 4.10. Views/Tickets/Index.cshtml

```html
@model TechnicalSupportService.Core.DTOs.PagedResult<TechnicalSupportService.Core.DTOs.TicketListItemDto>
@using TechnicalSupportService.Core.DTOs
@{
    ViewData["Title"] = "Заявки";
    var filter = ViewBag.Filter as TicketFilterDto ?? new TicketFilterDto();
}

<div class="d-flex justify-content-between align-items-center mb-3">
    <h2>Заявки</h2>
    <a asp-action="Create" class="btn btn-primary">+ Создать заявку</a>
</div>

<form method="get" class="card mb-4">
    <div class="card-body">
        <div class="row g-2">
            <div class="col-md-3">
                <select name="Status" class="form-select form-select-sm">
                    <option value="">Все статусы</option>
                    @foreach (var s in Enum.GetValues<TechnicalSupportService.Core.Enums.TicketStatus>())
                    {
                        <option value="@s" selected="@(filter.Status == s)">@s</option>
                    }
                </select>
            </div>
            <div class="col-md-3">
                <select name="Priority" class="form-select form-select-sm">
                    <option value="">Все приоритеты</option>
                    @foreach (var p in Enum.GetValues<TechnicalSupportService.Core.Enums.Priority>())
                    {
                        <option value="@p" selected="@(filter.Priority == p)">@p</option>
                    }
                </select>
            </div>
            <div class="col-md-4">
                <input type="text" name="Search" class="form-control form-control-sm" placeholder="Поиск..." value="@filter.Search" />
            </div>
            <div class="col-md-2">
                <button type="submit" class="btn btn-outline-primary btn-sm w-100">Найти</button>
            </div>
        </div>
    </div>
</form>

<table class="table table-hover">
    <thead>
        <tr>
            <th>Номер</th>
            <th>Заголовок</th>
            <th>Статус</th>
            <th>Приоритет</th>
            <th>Продукт</th>
            <th>Исполнитель</th>
            <th>Дата</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var t in Model.Items)
        {
            <tr onclick="location.href='@Url.Action("Details", new { id = t.Id })'" style="cursor:pointer">
                <td>@t.Number</td>
                <td>@t.Title</td>
                <td>@await Html.PartialAsync("_TicketStatusBadge", t.Status)</td>
                <td>@await Html.PartialAsync("_PriorityBadge", t.Priority)</td>
                <td>@t.ProductName</td>
                <td>@(t.AssignedToUserName ?? "—")</td>
                <td>@t.CreatedAt.ToString("dd.MM.yyyy")</td>
            </tr>
        }
    </tbody>
</table>

@if (Model.TotalPages > 1)
{
    <nav>
        <ul class="pagination justify-content-center">
            @for (int i = 1; i <= Model.TotalPages; i++)
            {
                <li class="page-item @(i == Model.Page ? "active" : "")">
                    <a class="page-link" href="?page=@i&status=@filter.Status&priority=@filter.Priority&search=@filter.Search">@i</a>
                </li>
            }
        </ul>
    </nav>
}
```

---

## 4.11. Views/Tickets/Create.cshtml

```html
@model TechnicalSupportService.Core.DTOs.TicketCreateDto
@{
    ViewData["Title"] = "Создать заявку";
}

<h2>Создать заявку</h2>
<div asp-validation-summary="All" class="text-danger"></div>

<form asp-action="Create" method="post">
    <div class="row">
        <div class="col-md-8">
            <div class="mb-3">
                <label class="form-label">Заголовок *</label>
                <input asp-for="Title" class="form-control" maxlength="200" required />
            </div>
            <div class="mb-3">
                <label class="form-label">Описание *</label>
                <textarea asp-for="Description" class="form-control" rows="5" required></textarea>
            </div>
        </div>
        <div class="col-md-4">
            <div class="mb-3">
                <label class="form-label">Продукт *</label>
                <select asp-for="ProductId" asp-items="@(ViewBag.Products as SelectList)" class="form-select" required>
                    <option value="">Выберите продукт</option>
                </select>
            </div>
            <div class="mb-3">
                <label class="form-label">Версия</label>
                <input asp-for="Version" class="form-control" maxlength="20" />
            </div>
            <div class="mb-3">
                <label class="form-label">Категория *</label>
                <select asp-for="Category" asp-items="@(ViewBag.Categories as SelectList)" class="form-select" required></select>
            </div>
            <div class="mb-3">
                <label class="form-label">Приоритет *</label>
                <select asp-for="Priority" asp-items="@(ViewBag.Priorities as SelectList)" class="form-select" required></select>
            </div>
            <div class="mb-3">
                <label class="form-label">Источник *</label>
                <select asp-for="Source" asp-items="@(ViewBag.Sources as SelectList)" class="form-select" required></select>
            </div>
            <div class="mb-3">
                <label class="form-label">Влияние</label>
                <select asp-for="Impact" asp-items="@(ViewBag.Impacts as SelectList)" class="form-select">
                    <option value="">Не указано</option>
                </select>
            </div>
            <div class="mb-3">
                <label class="form-label">Исполнитель</label>
                <select asp-for="AssignedToUserId" asp-items="@(ViewBag.Engineers as SelectList)" class="form-select">
                    <option value="">Не назначен</option>
                </select>
            </div>
        </div>
    </div>
    <button type="submit" class="btn btn-primary">Создать</button>
    <a asp-action="Index" class="btn btn-outline-secondary">Отмена</a>
</form>

@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
```

---

## 4.12. Views/Tickets/Details.cshtml

```html
@model TechnicalSupportService.Core.DTOs.TicketDto
@using TechnicalSupportService.Core.DTOs
@using TechnicalSupportService.Core.Enums
@using TechnicalSupportService.Core.Constants
@{
    ViewData["Title"] = $"Заявка {Model.Number}";
    var comments = ViewBag.Comments as List<CommentDto> ?? new();
    var attachments = ViewBag.Attachments as List<AttachmentDto> ?? new();
    var history = ViewBag.History as List<TicketHistoryDto> ?? new();
    var currentRole = ViewBag.CurrentRole as string ?? "";
    var currentUserId = (Guid)ViewBag.CurrentUserId;
}

<div class="d-flex justify-content-between align-items-start mb-3">
    <div>
        <h2>@Model.Number — @Model.Title</h2>
        @await Html.PartialAsync("_TicketStatusBadge", Model.Status)
        @await Html.PartialAsync("_PriorityBadge", Model.Priority)
    </div>
    <div>
        @if (Model.Status == TicketStatus.Resolved && (currentRole == Roles.Applicant || currentRole == Roles.Manager || currentRole == Roles.Admin))
        {
            <form asp-action="Close" asp-route-id="@Model.Id" method="post" class="d-inline">
                <button type="submit" class="btn btn-success btn-sm">Закрыть</button>
            </form>
        }
        @if ((Model.Status == TicketStatus.Closed || Model.Status == TicketStatus.Resolved) && (currentRole == Roles.Applicant || currentRole == Roles.Manager || currentRole == Roles.Admin))
        {
            <form asp-action="Reopen" asp-route-id="@Model.Id" method="post" class="d-inline">
                <button type="submit" class="btn btn-warning btn-sm">Переоткрыть</button>
            </form>
        }
        @if (currentRole == Roles.Admin || currentRole == Roles.Manager)
        {
            <form asp-action="Assign" asp-route-id="@Model.Id" method="post" class="d-inline">
                <select name="assigneeId" class="form-select form-select-sm d-inline" style="width:auto">
                    <option value="">Снять исполнителя</option>
                </select>
                <button type="submit" class="btn btn-outline-primary btn-sm">Назначить</button>
            </form>
        }
        @if (currentRole == Roles.Admin)
        {
            <form asp-action="Delete" asp-route-id="@Model.Id" method="post" class="d-inline"
                  onsubmit="return confirm('Удалить заявку?')">
                <button type="submit" class="btn btn-danger btn-sm">Удалить</button>
            </form>
        }
        <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-outline-secondary btn-sm">Редактировать</a>
    </div>
</div>

<ul class="nav nav-tabs" id="ticketTabs">
    <li class="nav-item"><a class="nav-link active" data-bs-toggle="tab" href="#info">Информация</a></li>
    <li class="nav-item"><a class="nav-link" data-bs-toggle="tab" href="#files">Файлы (@attachments.Count)</a></li>
    <li class="nav-item"><a class="nav-link" data-bs-toggle="tab" href="#comments">Комментарии (@comments.Count)</a></li>
    <li class="nav-item"><a class="nav-link" data-bs-toggle="tab" href="#history">История</a></li>
</ul>

<div class="tab-content mt-3">
    <!-- Информация -->
    <div class="tab-pane fade show active" id="info">
        <div class="row">
            <div class="col-md-6">
                <dl class="row">
                    <dt class="col-sm-4">Продукт</dt>
                    <dd class="col-sm-8">@Model.ProductName (@Model.ProductType)</dd>
                    <dt class="col-sm-4">Версия</dt>
                    <dd class="col-sm-8">@(Model.Version ?? "—")</dd>
                    <dt class="col-sm-4">Категория</dt>
                    <dd class="col-sm-8">@Model.Category</dd>
                    <dt class="col-sm-4">Источник</dt>
                    <dd class="col-sm-8">@Model.Source</dd>
                    <dt class="col-sm-4">Влияние</dt>
                    <dd class="col-sm-8">@(Model.Impact?.ToString() ?? "—")</dd>
                </dl>
            </div>
            <div class="col-md-6">
                <dl class="row">
                    <dt class="col-sm-4">Создал</dt>
                    <dd class="col-sm-8">@Model.CreatedByUserName (@Model.CreatedAt.ToString("dd.MM.yyyy HH:mm"))</dd>
                    <dt class="col-sm-4">Исполнитель</dt>
                    <dd class="col-sm-8">@(Model.AssignedToUserName ?? "Не назначен")</dd>
                    <dt class="col-sm-4">Обновлён</dt>
                    <dd class="col-sm-8">@Model.UpdatedAt.ToString("dd.MM.yyyy HH:mm")</dd>
                    @if (Model.ClosedAt.HasValue)
                    {
                        <dt class="col-sm-4">Закрыта</dt>
                        <dd class="col-sm-8">@Model.ClosedAt.Value.ToString("dd.MM.yyyy HH:mm")</dd>
                    }
                    @if (!string.IsNullOrWhiteSpace(Model.Resolution))
                    {
                        <dt class="col-sm-4">Решение</dt>
                        <dd class="col-sm-8">@Model.Resolution</dd>
                    }
                </dl>
            </div>
        </div>
        <h5>Описание</h5>
        <p>@Model.Description</p>
    </div>

    <!-- Файлы -->
    <div class="tab-pane fade" id="files">
        <form asp-controller="Files" asp-action="Upload" method="post" enctype="multipart/form-data" class="mb-3">
            <input type="hidden" name="ticketId" value="@Model.Id" />
            <div class="input-group">
                <input type="file" name="file" class="form-control" required />
                <button type="submit" class="btn btn-outline-primary">Загрузить</button>
            </div>
        </form>
        @foreach (var f in attachments)
        {
            <div class="d-flex justify-content-between align-items-center border-bottom py-2">
                <div>
                    <strong>@f.FileName</strong>
                    <small class="text-muted">(@($"{f.FileSizeBytes / 1024.0:F1}") КБ, @f.UploadedByName, @f.UploadedAt.ToString("dd.MM.yyyy"))</small>
                </div>
                <div>
                    <a asp-controller="Files" asp-action="Download" asp-route-id="@f.Id" class="btn btn-sm btn-outline-success">⬇</a>
                    <form asp-controller="Files" asp-action="Delete" method="post" class="d-inline">
                        <input type="hidden" name="id" value="@f.Id" />
                        <input type="hidden" name="ticketId" value="@Model.Id" />
                        <button type="submit" class="btn btn-sm btn-outline-danger">🗑</button>
                    </form>
                </div>
            </div>
        }
    </div>

    <!-- Комментарии -->
    <div class="tab-pane fade" id="comments">
        <form asp-action="AddComment" asp-route-id="@Model.Id" method="post" class="mb-4">
            <div class="mb-2">
                <textarea name="Content" class="form-control" rows="3" placeholder="Добавить комментарий..." required></textarea>
            </div>
            @if (currentRole != Roles.Applicant)
            {
                <div class="form-check mb-2">
                    <input type="checkbox" name="IsInternal" value="true" class="form-check-input" id="internalCheck" />
                    <label class="form-check-label" for="internalCheck">Внутренний комментарий</label>
                </div>
            }
            <button type="submit" class="btn btn-primary btn-sm">Отправить</button>
        </form>
        @foreach (var c in comments)
        {
            <div class="border-start border-3 @(c.IsInternal ? "border-warning" : "border-primary") ps-3 mb-3">
                <div class="d-flex justify-content-between">
                    <strong>@c.AuthorName</strong>
                    <small class="text-muted">@c.CreatedAt.ToString("dd.MM.yyyy HH:mm")</small>
                </div>
                @if (c.IsInternal)
                {
                    <span class="badge bg-warning text-dark">Внутренний</span>
                }
                <p class="mb-0">@c.Content</p>
            </div>
        }
    </div>

    <!-- История -->
    <div class="tab-pane fade" id="history">
        @foreach (var h in history)
        {
            <div class="d-flex mb-2">
                <small class="text-muted me-3" style="min-width:140px">@h.ChangedAt.ToString("dd.MM.yyyy HH:mm")</small>
                <div>
                    <strong>@h.ChangedByName</strong>
                    <span class="badge bg-secondary">@h.ChangeType</span>
                    @if (!string.IsNullOrWhiteSpace(h.FieldName))
                    {
                        <span>@h.FieldName: @(h.OldValue ?? "—") → @(h.NewValue ?? "—")</span>
                    }
                    else
                    {
                        <span>@h.NewValue</span>
                    }
                </div>
            </div>
        }
    </div>
</div>
```

---

## 4.13. Views/Tickets/Edit.cshtml

```html
@model TechnicalSupportService.Core.DTOs.TicketUpdateDto
@{
    ViewData["Title"] = $"Редактирование {ViewBag.TicketNumber}";
}

<h2>Редактирование @ViewBag.TicketNumber</h2>
<div asp-validation-summary="All" class="text-danger"></div>

<form asp-action="Edit" asp-route-id="@ViewBag.TicketId" method="post">
    <div class="mb-3">
        <label class="form-label">Заголовок *</label>
        <input asp-for="Title" class="form-control" required />
    </div>
    <div class="mb-3">
        <label class="form-label">Описание *</label>
        <textarea asp-for="Description" class="form-control" rows="5" required></textarea>
    </div>
    <div class="row">
        <div class="col-md-4 mb-3">
            <label class="form-label">Продукт</label>
            <select asp-for="ProductId" asp-items="@(ViewBag.Products as Microsoft.AspNetCore.Mvc.Rendering.SelectList)" class="form-select"></select>
        </div>
        <div class="col-md-4 mb-3">
            <label class="form-label">Приоритет</label>
            <select asp-for="Priority" asp-items="@(ViewBag.Priorities as Microsoft.AspNetCore.Mvc.Rendering.SelectList)" class="form-select"></select>
        </div>
        <div class="col-md-4 mb-3">
            <label class="form-label">Категория</label>
            <select asp-for="Category" asp-items="@(ViewBag.Categories as Microsoft.AspNetCore.Mvc.Rendering.SelectList)" class="form-select"></select>
        </div>
    </div>
    <div class="mb-3">
        <label class="form-label">Версия</label>
        <input asp-for="Version" class="form-control" maxlength="20" />
    </div>
    <button type="submit" class="btn btn-primary">Сохранить</button>
    <a asp-action="Details" asp-route-id="@ViewBag.TicketId" class="btn btn-outline-secondary">Отмена</a>
</form>

@section Scripts {
    @await Html.PartialAsync("_ValidationScriptsPartial")
}
```

---

## 4.14. CSS — wwwroot/css/site.css

```css
body {
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
}

.card {
    border-radius: 0.5rem;
}

.table tr[onclick]:hover {
    background-color: rgba(0, 0, 0, 0.03);
    cursor: pointer;
}

.nav-tabs .nav-link.active {
    font-weight: 600;
}

.badge {
    font-size: 0.75em;
    padding: 0.35em 0.65em;
}
```

---

## 4.15. CSS — wwwroot/css/themes/variables-light.css

```css
:root, [data-theme="light"] {
    --bg-body: #ffffff;
    --bg-card: #ffffff;
    --text-primary: #212529;
    --text-secondary: #6c757d;
    --border-color: #dee2e6;
}
```

---

## 4.16. CSS — wwwroot/css/themes/variables-dark.css

```css
[data-theme="dark"] {
    --bg-body: #1a1a2e;
    --bg-card: #16213e;
    --text-primary: #e0e0e0;
    --text-secondary: #a0a0a0;
    --border-color: #2a2a4a;
}

[data-theme="dark"] body {
    background-color: var(--bg-body);
    color: var(--text-primary);
}

[data-theme="dark"] .card {
    background-color: var(--bg-card);
    border-color: var(--border-color);
}

[data-theme="dark"] .table {
    color: var(--text-primary);
}

[data-theme="dark"] .form-control,
[data-theme="dark"] .form-select {
    background-color: #2a2a4a;
    border-color: #3a3a5a;
    color: var(--text-primary);
}

[data-theme="dark"] .navbar-dark {
    background-color: #0f3460 !important;
}
```

---

## 4.17. JS — wwwroot/js/site.js

```javascript
// Переключение темы
function toggleTheme() {
    const html = document.documentElement;
    const current = html.getAttribute('data-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    html.setAttribute('data-theme', next);
    localStorage.setItem('theme', next);
}

// Восстановление темы при загрузке
(function () {
    const saved = localStorage.getItem('theme');
    if (saved) {
        document.documentElement.setAttribute('data-theme', saved);
    }
})();
```

---

## 4.18. Примечание по LibMan

Для установки Bootstrap и jQuery создайте файл `wwwroot/libman.json`:

```json
{
  "version": "1.0",
  "defaultProvider": "cdnjs",
  "libraries": [
    {
      "library": "twitter-bootstrap@5.3.3",
      "destination": "wwwroot/lib/bootstrap/"
    },
    {
      "library": "jquery@3.7.1",
      "destination": "wwwroot/lib/jquery/"
    },
    {
      "library": "jquery-validate@1.21.0",
      "destination": "wwwroot/lib/jquery-validation/"
    },
    {
      "library": "jquery-validation-unobtrusive@4.0.0",
      "destination": "wwwroot/lib/jquery-validation-unobtrusive/"
    }
  ]
}
```

Или вручную скачайте Bootstrap 5, jQuery, jQuery Validation в `wwwroot/lib/`.
