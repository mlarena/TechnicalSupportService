# Шаг 2. Проектирование базы данных — сущности, связи, таблицы

## 2.1. Сущность «Заявка» (Ticket)

### Поля заявки

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **Id** | UUID (PK) | Да | Внутренний идентификатор |
| **Number** | String(50) | Да | Внешний номер формата `ГГГГ_ММ_XXX` |
| **Title** | String(200) | Да | Краткий заголовок заявки |
| **Description** | Text | Да | Полное описание проблемы |
| **ProductId** | UUID (FK -> Products) | Да | Ссылка на продукт |
| **Version** | String(20) | Нет | Версия продукта (может отличаться от CurrentVersion) |
| **Priority** | Enum | Да | `Low` / `Medium` / `High` / `Critical` |
| **Status** | Enum | Да | `New` / `Assigned` / `InProgress` / `Resolved` / `Closed` / `Reopened` |
| **Category** | Enum | Да | `Bug` / `Feature` / `Support` / `Incident` |
| **Impact** | Enum | Нет | `Individual` / `Team` / `Department` / `Company` |
| **Source** | Enum | Да | `Email` / `Phone` / `Portal` / `Internal` |
| **AssignedToUserId** | UUID (FK -> Users) | Нет | Исполнитель (инженер) |
| **CreatedByUserId** | UUID (FK -> Users) | Да | Кто создал заявку (автозаполнение) |
| **CreatedAt** | Timestamp | Да | Дата создания (UTC) |
| **UpdatedByUserId** | UUID (FK -> Users) | Да | Кто последним изменял (автозаполнение) |
| **UpdatedAt** | Timestamp | Да | Дата последнего изменения (UTC) |
| **ClosedAt** | Timestamp | Нет | Дата закрытия (заполняется при статусе `Closed`) |
| **Resolution** | Text | Нет | Описание решения (при `Resolved` / `Closed`) |
| **TimeSpentMinutes** | Integer | Нет | Затраченное время в минутах |
| **ParentTicketId** | UUID (FK -> Tickets) | Нет | Ссылка на родительскую заявку (эпик) |
| **IsDeleted** | Boolean | Да | Мягкое удаление (по умолчанию `false`) |
| **DeletedAt** | Timestamp | Нет | Дата удаления |
| **DeletedByUserId** | UUID (FK -> Users) | Нет | Кто удалил |

> Примечание: `ProductName` (String) и `ProductType` (Enum) заменены на `ProductId` (FK -> Products) для целостности справочника.

---

## 2.2. Нумерация заявок (ключевое требование)

**Формат номера:** `ГГГГ_ММ_ПОРЯДКОВЫЙ_НОМЕР`

Примеры:
- `2026_08_001`
- `2026_08_102`
- `2026_09_001` (сброс в новом месяце)

**Правила генерации:**
1. Номер формируется **автоматически** при создании заявки.
2. Счётчик сбрасывается **каждый месяц** (независимо для каждого месяца).
3. Счётчик хранится в отдельной таблице `TicketNumberCounter`:
   - `YearMonth` (например, `2026-08`) — PK.
   - `LastNumber` — последний использованный номер.
4. При создании заявки в транзакции:
   - Блокируем строку `SELECT ... FOR UPDATE`.
   - Инкрементируем `LastNumber`.
   - Формируем номер: `$"{Year:0000}_{Month:00}_{LastNumber:D3}"`.
   - Если строка отсутствует — создаём со значением `1`.
5. Допустимо использование `SEQUENCE` с условием, но рекомендуемый подход — таблица с блокировкой для простоты и ясности.

---

## 2.3. Версионность и история изменений (TicketHistory)

Каждое изменение заявки фиксируется в отдельной таблице.

### Поля

| Поле | Тип данных | Описание |
|------|-----------|----------|
| **Id** | UUID (PK) | Уникальный идентификатор записи истории |
| **TicketId** | UUID (FK) | Ссылка на заявку |
| **ChangedByUserId** | UUID (FK) | Кто изменил |
| **ChangedAt** | Timestamp | Дата изменения (UTC) |
| **FieldName** | String(50) | Название изменённого поля |
| **OldValue** | Text | Старое значение (строка). Null для создания |
| **NewValue** | Text | Новое значение (строка) |
| **ChangeType** | Enum | `Creation` / `Update` / `StatusChange` / `Assignment` / `Comment` / `FileAttach` |
| **CommentId** | UUID (FK -> Comments) | Ссылка на комментарий (если `ChangeType = Comment`) |
| **AttachmentId** | UUID (FK -> Attachments) | Ссылка на вложение (если `ChangeType = FileAttach`) |

### Правила фиксации

- Изменение **любого поля** → новая запись в `TicketHistory`.
- Добавление комментария → запись с `ChangeType = Comment`.
- Прикрепление/удаление файла → запись в истории.
- Создание заявки → запись с `ChangeType = Creation`.
- Назначение исполнителя → запись с `ChangeType = Assignment`.

---

## 2.4. Хранение документов (файлы)

### Требования

- Каждый файл прикрепляется к конкретной заявке.
- Поддерживаемые форматы: `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.jpg`, `.png`, `.zip`.
- Максимальный размер одного файла — **50 МБ**.
- Общий лимит на заявку — **500 МБ**.
- Физические файлы хранятся на файловом сервере (или в S3-совместимом хранилище).

### Таблица Attachments

| Поле | Тип данных | Описание |
|------|-----------|----------|
| **Id** | UUID (PK) | Идентификатор |
| **TicketId** | UUID (FK) | Заявка |
| **FileName** | String(255) | Оригинальное имя файла |
| **StoredFileName** | String(255) | Уникальное имя на диске (GUID + расширение) |
| **FilePath** | String(500) | Относительный путь к файлу |
| **FileSizeBytes** | BigInt | Размер в байтах |
| **MimeType** | String(100) | MIME-тип |
| **UploadedByUserId** | UUID (FK) | Кто загрузил |
| **UploadedAt** | Timestamp | Дата загрузки |
| **IsDeleted** | Boolean | Мягкое удаление (по умолчанию `false`) |
| **DeletedAt** | Timestamp | Дата удаления |

---

## 2.5. Статусная модель

### Диаграмма состояний

```
[New] → [Assigned] → [InProgress] → [Resolved] → [Closed]
  ↑         ↓              ↓            ↓
  └────[Reopened] ←─────────────────────┘
```

### Правила переходов

| Переход | Кто может выполнить |
|---------|---------------------|
| `New` → `Assigned` | Менеджер, Администратор |
| `Assigned` → `InProgress` | Инженер |
| `InProgress` → `Resolved` | Инженер (заполняет Resolution) |
| `Resolved` → `Closed` | Заявитель, Менеджер |
| `Resolved` → `Reopened` | Заявитель, Менеджер |
| `Closed` → `Reopened` | Заявитель, Менеджер |
| Любой → `Closed` | Администратор, Менеджер (с комментарием) |

---

## 2.6. Полное описание всех таблиц БД

### 2.6.1. Таблица `Users` (расширение ASP.NET Identity)

Наследуется от `IdentityUser<Guid>`. Стандартные поля Identity (Id, UserName, Email, PasswordHash, PhoneNumber и др.) дополняются кастомными.

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **Id** | UUID (PK) | Да | Уникальный идентификатор (наследуется от IdentityUser) |
| **FullName** | String(200) | Да | ФИО пользователя |
| **DepartmentId** | UUID (FK) | Нет | Ссылка на отдел (таблица `Departments`) |
| **Position** | String(100) | Нет | Должность |
| **IsActive** | Boolean | Да | Активен/заблокирован (по умолчанию `true`) |
| **CreatedAt** | Timestamp | Да | Дата создания учётной записи (UTC) |
| **UpdatedAt** | Timestamp | Да | Дата последнего обновления профиля (UTC) |

> Поля `Email`, `PhoneNumber`, `UserName`, `EmailConfirmed`, `PasswordHash` и др. наследуются от `IdentityUser<Guid>` и не дублируются.

---

### 2.6.2. Таблица `Departments` (справочник отделов)

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **Id** | UUID (PK) | Да | Уникальный идентификатор |
| **Name** | String(200) | Да | Наименование отдела (уникальное) |
| **Description** | String(500) | Нет | Описание отдела |
| **IsActive** | Boolean | Да | Активен/архив (по умолчанию `true`) |
| **CreatedAt** | Timestamp | | Дата создания записи |

---

### 2.6.3. Таблица `Products` (справочник продуктов)

Вынесена из Tickets для нормализации.

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **Id** | UUID (PK) | Да | Уникальный идентификатор |
| **Name** | String(200) | Да | Наименование продукта |
| **ProductType** | Enum | Да | `Software` / `Hardware` / `Embedded` |
| **CurrentVersion** | String(20) | Нет | Текущая актуальная версия продукта |
| **Description** | String(500) | Нет | Краткое описание продукта |
| **IsActive** | Boolean | Да | Активен/снят с поддержки (по умолчанию `true`) |
| **CreatedAt** | Timestamp | Да | Дата создания записи |
| **UpdatedAt** | Timestamp | Да | Дата последнего обновления |

---

### 2.6.4. Таблица `Tickets` (заявки) — основная сущность

Полное описание полей см. в §2.1 выше.

---

### 2.6.5. Таблица `Comments` (комментарии к заявкам)

Вынесена из `TicketHistory` в отдельную сущность.

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **Id** | UUID (PK) | Да | Уникальный идентификатор |
| **TicketId** | UUID (FK -> Tickets) | Да | Заявка |
| **AuthorUserId** | UUID (FK -> Users) | Да | Автор комментария |
| **Content** | Text | Да | Текст комментария |
| **IsInternal** | Boolean | Да | Внутренний (скрыт от заявителя). По умолчанию `false` |
| **IsEdited** | Boolean | Да | Был ли отредактирован. По умолчанию `false` |
| **EditedAt** | Timestamp | Нет | Дата последнего редактирования |
| **CreatedAt** | Timestamp | Да | Дата создания (UTC) |
| **IsDeleted** | Boolean | Да | Мягкое удаление. По умолчанию `false` |

> При добавлении комментария в `TicketHistory` создаётся запись с `ChangeType = Comment` (ссылка на Id комментария в `NewValue`), но сам текст хранится в `Comments`.

---

### 2.6.6. Таблица `TicketHistory` (история изменений)

Полное описание полей см. в §2.3 выше.

---

### 2.6.7. Таблица `Attachments` (вложения/файлы)

Полное описание полей см. в §2.4 выше.

---

### 2.6.8. Таблица `TicketNumberCounter` (счётчик нумерации)

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **YearMonth** | String(7) (PK) | Да | Ключ формата `ГГГГ-ММ` (например, `2026-08`) |
| **LastNumber** | Integer | Да | Последний использованный порядковый номер (по умолчанию 0) |

---

### 2.6.9. Таблица `AuditLog` (аудит действий пользователей)

Логирование **все** действий пользователей (не только изменения заявок).

| Поле | Тип данных | Обязательное | Описание |
|------|-----------|--------------|----------|
| **Id** | UUID (PK) | Да | Уникальный идентификатор |
| **UserId** | UUID (FK -> Users) | Да | Кто выполнил действие |
| **Action** | String(100) | Да | Тип действия (`Ticket.Create`, `User.Block`, `File.Upload` и т.д.) |
| **EntityName** | String(100) | Нет | Имя сущности (`Ticket`, `User`, `Attachment`) |
| **EntityId** | UUID | Нет | Идентификатор затронутой сущности |
| **Details** | Text | Нет | Дополнительные детали (JSON или текст) |
| **IpAddress** | String(45) | Нет | IP-адрес пользователя |
| **UserAgent** | String(500) | Нет | User-Agent браузера |
| **CreatedAt** | Timestamp | Да | Дата и время действия (UTC) |

---

### 2.6.10. Перечень enum-типов

| Enum | Значения | Используется в |
|------|----------|----------------|
| **ProductType** | `Software`, `Hardware`, `Embedded` | Products |
| **Priority** | `Low`, `Medium`, `High`, `Critical` | Tickets |
| **TicketStatus** | `New`, `Assigned`, `InProgress`, `Resolved`, `Closed`, `Reopened` | Tickets |
| **Category** | `Bug`, `Feature`, `Support`, `Incident` | Tickets |
| **Impact** | `Individual`, `Team`, `Department`, `Company` | Tickets |
| **Source** | `Email`, `Phone`, `Portal`, `Internal` | Tickets |
| **ChangeType** | `Creation`, `Update`, `StatusChange`, `Assignment`, `Comment`, `FileAttach` | TicketHistory |

---

### 2.6.11. ER-диаграмма (связи)

```
┌─────────────┐       ┌──────────────┐       ┌─────────────────┐
│ Departments  │1────N│    Users      │1────N│   AuditLog      │
└─────────────┘       │ (IdentityUser)│       └─────────────────┘
                      └──────┬───────┘
                             │
                 ┌───────────┼───────────┐
                 │1          │1          │1
                 │           │           │
          ┌──────┴──────┐    │    ┌──────┴──────┐
          │  Comments   │    │    │ Attachments │
          └──────┬──────┘    │    └──────┬──────┘
                 │N          │           │N
                 │           │           │
                 └─────┐N    │    N┌─────┘
                       │     │     │
                 ┌─────┴─────┴─────┴──────┐
                 │        Tickets          │
                 │  (PK: Id, Unique: Number)│
                 └─────┬───────────────┬───┘
                       │1              │1
                       │               │
                  N┌───┴────┐    N┌────┴───────┐
                   │Ticket  │    │TicketNumber │
                   │History │    │Counter      │
                   └────────┘    └─────────────┘

┌─────────────┐
│  Products   │1────N│ Tickets │ (через ProductId FK)
└─────────────┘
```

---

## 2.7. Технические требования к БД (PostgreSQL)

### Индексы

| Таблица | Индекс | Тип | Поля |
|---------|--------|-----|------|
| `Tickets` | `IX_Ticket_Number` | UNIQUE | `Number` |
| `Tickets` | `IX_Ticket_CreatedAt` | | `CreatedAt DESC` |
| `Tickets` | `IX_Ticket_Status` | | `Status` |
| `Tickets` | `IX_Ticket_AssignedToUserId` | | `AssignedToUserId` |
| `Tickets` | `IX_Ticket_CreatedByUserId` | | `CreatedByUserId` |
| `Tickets` | `IX_Ticket_ProductId` | | `ProductId` |
| `TicketHistory` | `IX_TicketHistory_TicketId_ChangedAt` | | `(TicketId, ChangedAt DESC)` |
| `Attachments` | `IX_Attachments_TicketId` | | `TicketId` |
| `Comments` | `IX_Comments_TicketId` | | `TicketId` |
| `AuditLog` | `IX_AuditLog_UserId_CreatedAt` | | `(UserId, CreatedAt DESC)` |
| `AuditLog` | `IX_AuditLog_EntityName_EntityId` | | `(EntityName, EntityId)` |

### Триггеры (опционально)

- Автоматическое заполнение `UpdatedAt` при изменении строки.
- Автоматическое заполнение `CreatedAt` при вставке.
