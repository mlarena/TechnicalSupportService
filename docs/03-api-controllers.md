# Шаг 3. Контроллеры, маршруты и авторизация

## 3.1. Список контроллеров

| Контроллер | Маршрут (базовый) | Авторизация | Описание |
|------------|-------------------|-------------|----------|
| `AccountController` | `/Account` | Анонимный (Login/Register) | Вход, регистрация, восстановление пароля, профиль |
| `DashboardController` | `/Dashboard` | Авторизованные | Главная страница (дашборд) |
| `TicketsController` | `/Tickets` | Авторизованные | CRUD заявок, список, детали |
| `AdminController` | `/Admin` | `Admin` | Управление пользователями, ролями, справочниками |
| `ProductsController` | `/Admin/Products` | `Admin` | CRUD справочника продуктов |
| `DepartmentsController` | `/Admin/Departments` | `Admin` | CRUD справочника отделов |
| `FilesController` | `/Files` | Авторизованные | Загрузка, скачивание, удаление файлов |

---

## 3.2. Детализация эндпоинтов

### AccountController

| Метод | Маршрут | HTTP | Описание | Авторизация |
|-------|---------|------|----------|-------------|
| `Login` | `/Account/Login` | GET/POST | Форма входа | Анонимный |
| `Register` | `/Account/Register` | GET/POST | Регистрация нового пользователя | Анонимный |
| `Logout` | `/Account/Logout` | POST | Выход | Авторизованный |
| `Profile` | `/Account/Profile` | GET/POST | Просмотр/редактирование профиля | Авторизованный |
| `ForgotPassword` | `/Account/ForgotPassword` | GET/POST | Запрос сброса пароля | Анонимный |
| `ResetPassword` | `/Account/ResetPassword` | GET/POST | Сброс пароля по токену | Анонимный |
| `ChangePassword` | `/Account/ChangePassword` | GET/POST | Смена пароля | Авторизованный |
| `AccessDenied` | `/Account/AccessDenied` | GET | Страница «Доступ запрещён» | Авторизованный |

### DashboardController

| Метод | Маршрут | HTTP | Описание | Авторизация |
|-------|---------|------|----------|-------------|
| `Index` | `/Dashboard` | GET | Дашборд: сводка по статусам, последние заявки | Авторизованный |

### TicketsController

| Метод | Маршрут | HTTP | Описание | Авторизация |
|-------|---------|------|----------|-------------|
| `Index` | `/Tickets` | GET | Список заявок (с фильтрами, пагинацией) | Авторизованный |
| `Create` | `/Tickets/Create` | GET/POST | Создание новой заявки | Авторизованный |
| `Details` | `/Tickets/{id}` | GET | Детали заявки (вкладки: информация, файлы, история) | Авторизованный |
| `Edit` | `/Tickets/{id}/Edit` | GET/POST | Редактирование заявки | Авторизованный (роль + владелец/назначен) |
| `ChangeStatus` | `/Tickets/{id}/ChangeStatus` | POST | Изменение статуса | Авторизованный (роль-зависимый) |
| `Assign` | `/Tickets/{id}/Assign` | POST | Назначение исполнителя | `Admin`, `Manager` |
| `AddComment` | `/Tickets/{id}/Comments` | POST | Добавление комментария | Авторизованный |
| `Close` | `/Tickets/{id}/Close` | POST | Закрытие заявки | `Admin`, `Manager`, Заявитель (если своя) |
| `Reopen` | `/Tickets/{id}/Reopen` | POST | Переоткрытие заявки | `Admin`, `Manager`, Заявитель |
| `Delete` | `/Tickets/{id}/Delete` | POST | Мягкое удаление | `Admin` |
| `Export` | `/Tickets/Export` | GET | Экспорт в Excel/CSV | `Admin`, `Manager` |

### AdminController

| Метод | Маршрут | HTTP | Описание | Авторизация |
|-------|---------|------|----------|-------------|
| `Users` | `/Admin/Users` | GET | Список пользователей | `Admin` |
| `CreateUser` | `/Admin/Users/Create` | GET/POST | Создание пользователя | `Admin` |
| `EditUser` | `/Admin/Users/{id}/Edit` | GET/POST | Редактирование пользователя | `Admin` |
| `BlockUser` | `/Admin/Users/{id}/Block` | POST | Блокировка/разблокировка | `Admin` |
| `DeleteUser` | `/Admin/Users/{id}/Delete` | POST | Удаление пользователя | `Admin` |
| `Roles` | `/Admin/Roles` | GET | Управление ролями | `Admin` |
| `AuditLog` | `/Admin/AuditLog` | GET | Просмотр лога аудита | `Admin` |

### FilesController

| Метод | Маршрут | HTTP | Описание | Авторизация |
|-------|---------|------|----------|-------------|
| `Upload` | `/Files/Upload` | POST | Загрузка файла к заявке | Авторизованный |
| `Download` | `/Files/{id}/Download` | GET | Скачивание файла | Авторизованный |
| `Delete` | `/Files/{id}/Delete` | POST | Удаление файла | `Admin`, Инженер (если загрузил), Менеджер |

---

## 3.3. Матрица доступа к действиям

| Действие | Admin | Engineer | Manager | Applicant |
|----------|-------|----------|---------|-----------|
| Создать заявку | ✅ | ✅ | ✅ | ✅ |
| Просмотреть все заявки | ✅ | ✅ | ✅ | ❌ (только свои) |
| Редактировать заявку | ✅ | ✅ (назначенная) | ✅ | ❌ |
| Назначить исполнителя | ✅ | ❌ | ✅ | ❌ |
| Изменить статус | ✅ | ✅ (ограниченно) | ✅ | ❌ |
| Закрыть заявку | ✅ | ❌ | ✅ | ✅ (если Resolved) |
| Переоткрыть заявку | ✅ | ❌ | ✅ | ✅ (если Closed) |
| Удалить заявку | ✅ | ❌ | ❌ | ❌ |
| Загрузить файл | ✅ | ✅ | ✅ | ✅ (к своей) |
| Скачать файл | ✅ | ✅ | ✅ | ✅ (к своей) |
| Удалить файл | ✅ | ✅ (загруженный) | ❌ | ❌ |
| Добавить комментарий | ✅ | ✅ | ✅ | ✅ (к своей) |
| Внутренний комментарий | ✅ | ✅ | ✅ | ❌ |
| Управление пользователями | ✅ | ❌ | ❌ | ❌ |
| Управление продуктами | ✅ | ❌ | ❌ | ❌ |
| Экспорт в Excel | ✅ | ❌ | ✅ | ❌ |
| Просмотр аудита | ✅ | ❌ | ❌ | ❌ |

---

## 3.4. Фильтрация списка заявок

Параметры GET `/Tickets`:

| Параметр | Тип | Описание |
|----------|-----|----------|
| `status` | Enum | Фильтр по статусу |
| `priority` | Enum | Фильтр по приоритету |
| `category` | Enum | Фильтр по категории |
| `productId` | UUID | Фильтр по продукту |
| `assignedToUserId` | UUID | Фильтр по исполнителю |
| `search` | String | Поиск по номеру, заголовку, описанию |
| `dateFrom` | Date | Дата создания от |
| `dateTo` | Date | Дата создания до |
| `page` | Int | Номер страницы (по умолчанию 1) |
| `pageSize` | Int | Размер страницы (по умолчанию 20) |
| `sortBy` | String | Поле сортировки (по умолчанию `CreatedAt`) |
| `sortDir` | String | Направление (`asc` / `desc`, по умолчанию `desc`) |

**Ограничение:** Заявитель видит только свои заявки (`CreatedByUserId == currentUserId`).

---

## 3.5. Атрибуты авторизации

```csharp
// На уровне контроллера
[Authorize]
public class TicketsController : Controller

// На уровне действия
[Authorize(Roles = "Admin")]
public IActionResult Users()

// Проверка владения — через сервис (не через атрибут)
// В сервисе: if (ticket.CreatedByUserId != currentUserId && !isManagerOrAdmin) throw Forbidden();
```

---

## 3.6. Анти-CSRF

- Все POST-формы используют `@Html.AntiForgeryToken()`
- Контроллеры с POST-методами помечаются `[AutoValidateAntiforgeryToken]`
- AJAX-запросы передают токен в заголовке `RequestVerificationToken`
