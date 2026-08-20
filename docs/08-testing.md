# Шаг 8. Тестирование

## 8.1. Стратегия тестирования

| Уровень | Что тестируем | Инструмент | Покрытие |
|---------|---------------|------------|----------|
| **Unit** | Бизнес-логика сервисов, валидация, нумерация | xUnit + Moq | Критичные методы |
| **Integration** | Контроллеры + БД (in-memory или Testcontainers) | xUnit + WebApplicationFactory | Все эндпоинты |
| **E2E** | Пользовательские сценарии | Playwright (опционально) | Критичные флоу |

---

## 8.2. Структура тестовых проектов

```
tests/
├── TechnicalSupportService.UnitTests/
│   ├── Services/
│   │   ├── TicketServiceTests.cs
│   │   ├── NumberGeneratorServiceTests.cs
│   │   ├── CommentServiceTests.cs
│   │   ├── AttachmentServiceTests.cs
│   │   └── FileStorageServiceTests.cs
│   ├── Validation/
│   │   └── TicketCreateDtoValidationTests.cs
│   └── Helpers/
│       └── TestDataBuilder.cs
│
└── TechnicalSupportService.IntegrationTests/
    ├── Controllers/
    │   ├── TicketsControllerTests.cs
    │   ├── AccountControllerTests.cs
    │   ├── AdminControllerTests.cs
    │   └── DashboardControllerTests.cs
    ├── Infrastructure/
    │   ├── CustomWebApplicationFactory.cs
    │   └── TestDatabaseFixture.cs
    └── Fixtures/
        └── TestDataSeeder.cs
```

---

## 8.3. Unit-тесты — примеры

### NumberGeneratorServiceTests

```csharp
public class NumberGeneratorServiceTests
{
    [Fact]
    public async Task GenerateNextNumber_FirstTicketOfMonth_Returns001()
    {
        // Arrange
        var db = CreateInMemoryDb();
        var service = new NumberGeneratorService(db);

        // Act
        var number = await service.GenerateNextNumberAsync();

        // Assert
        var expectedPrefix = DateTime.UtcNow.ToString("yyyy_MM");
        Assert.StartsWith(expectedPrefix, number);
        Assert.EndsWith("_001", number);
    }

    [Fact]
    public async Task GenerateNextNumber_Sequential_Returns002()
    {
        var db = CreateInMemoryDb();
        var service = new NumberGeneratorService(db);

        await service.GenerateNextNumberAsync();
        var number = await service.GenerateNextNumberAsync();

        Assert.EndsWith("_002", number);
    }
}
```

### TicketServiceTests (Business Rules)

```csharp
public class TicketServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidDto_TicketCreatedWithCorrectStatus()
    {
        // Arrange
        var service = CreateService();
        var dto = new TicketCreateDto { Title = "Test", ... };

        // Act
        var result = await service.CreateAsync(dto, userId);

        // Assert
        Assert.Equal("New", result.Status);
        Assert.NotNull(result.Number);
    }

    [Fact]
    public async Task ChangeStatusAsync_InvalidTransition_ThrowsBusinessRuleException()
    {
        // Arrange
        var service = CreateService();
        var ticket = await CreateTicketInDb(status: TicketStatus.New);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.ChangeStatusAsync(ticket.Id, TicketStatus.Resolved, engineerId));
    }

    [Fact]
    public async Task ChangeStatusAsync_EngineerCanNotAssign_ThrowsForbidden()
    {
        // Arrange
        var service = CreateService();
        var ticket = await CreateTicketInDb(status: TicketStatus.New);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.ChangeStatusAsync(ticket.Id, TicketStatus.Assigned, engineerId));
    }

    [Fact]
    public async Task DeleteAsync_NonAdmin_ThrowsForbidden()
    {
        var service = CreateService();
        var ticket = await CreateTicketInDb();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => service.DeleteAsync(ticket.Id, applicantId));
    }
}
```

---

## 8.4. Integration-тесты — примеры

### CustomWebApplicationFactory

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Заменяем реальную БД на InMemory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));

            // Заменяем файловое хранилище на in-memory
            services.AddScoped<IFileStorageService, InMemoryFileStorageService>();
        });
    }
}
```

### TicketsControllerTests

```csharp
public class TicketsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TicketsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Index_Authorized_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/Tickets");

        // Assert
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Заявки", html);
    }

    [Fact]
    public async Task Create_Post_ValidData_RedirectsToDetails()
    {
        // Arrange
        var formData = new Dictionary<string, string>
        {
            ["Title"] = "Тестовая заявка",
            ["Description"] = "Описание",
            ["ProductId"] = "...",
            ["Priority"] = "Medium",
            ["Category"] = "Bug",
            ["Source"] = "Portal"
        };

        // Act
        var response = await _client.PostAsync("/Tickets/Create",
            new FormUrlEncodedContent(formData));

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Details_UnauthorizedUser_ReturnsRedirect()
    {
        // Проверка что неавторизованный перенаправляется на Login
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync($"/Tickets/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
```

---

## 8.5. Тестовые сценарии (checklist)

### Авторизация
- [ ] Успешный вход
- [ ] Неверный пароль → ошибка
- [ ] Заблокированный пользователь → ошибка
- [ ] Регистрация нового пользователя
- [ ] Сброс пароля

### Заявки (CRUD)
- [ ] Создание заявки с валидными данными → номер генерируется
- [ ] Создание с пустым заголовком → ошибка валидации
- [ ] Просмотр деталей — владелец видит свою заявку
- [ ] Просмотр деталей — заявитель не видит чужую заявку (403)
- [ ] Редактирование — инженер может редактировать назначенную заявку
- [ ] Редактирование — заявитель не может редактировать (403)
- [ ] Удаление — только Admin
- [ ] Мягкое удаление — заявка не отображается в списке

### Статусы
- [ ] New → Assigned (Admin/Manager)
- [ ] Assigned → InProgress (Engineer)
- [ ] InProgress → Resolved (Engineer, заполняет Resolution)
- [ ] Resolved → Closed (Applicant/Manager)
- [ ] Resolved → Reopened (Applicant/Manager)
- [ ] New → Resolved — запрещён (недопустимый переход)
- [ ] Engineer → New → Assigned — запрещён (нет прав)

### Нумерация
- [ ] Первая заявка месяца → 2026_08_001
- [ ] Вторая заявка месяца → 2026_08_002
- [ ] 100-я заявка → 2026_08_100
- [ ] Конкурентное создание — номера не дублируются

### Файлы
- [ ] Загрузка допустимого файла
- [ ] Загрузка файла > 50 МБ → ошибка
- [ ] Загрузка .exe файла → ошибка
- [ ] Скачивание файла
- [ ] Удаление файла — только загрузивший / Admin

### Комментарии
- [ ] Добавление комментария
- [ ] Внутренний комментарий — заявитель не видит
- [ ] Редактирование — только автор
- [ ] Удаление — автор / Admin

### История
- [ ] Создание заявки → запись Creation
- [ ] Изменение статуса → запись StatusChange
- [ ] Изменение поля → запись Update
- [ ] Добавление комментария → запись Comment
- [ ] Загрузка файла → запись FileAttach
- [ ] Назначение → запись Assignment

### Аудит
- [ ] Каждое действие записывается в AuditLog
- [ ] Поля UserId, Action, EntityName, EntityId заполнены
- [ ] IP-адрес записывается
