using TechnicalSupportService.Core.Enums;

namespace TechnicalSupportService.Core.Helpers;

public static class EnumDisplayHelper
{
    private static readonly Dictionary<TicketStatus, string> TicketStatusNames = new()
    {
        [TicketStatus.New] = "Новая",
        [TicketStatus.Assigned] = "Назначена",
        [TicketStatus.InProgress] = "В работе",
        [TicketStatus.Resolved] = "Решена",
        [TicketStatus.Closed] = "Закрыта",
        [TicketStatus.Reopened] = "Переоткрыта"
    };

    private static readonly Dictionary<Priority, string> PriorityNames = new()
    {
        [Priority.Low] = "Низкий",
        [Priority.Medium] = "Средний",
        [Priority.High] = "Высокий",
        [Priority.Critical] = "Критический"
    };

    private static readonly Dictionary<Category, string> CategoryNames = new()
    {
        [Category.Bug] = "Ошибка",
        [Category.Feature] = "Функция",
        [Category.Support] = "Поддержка",
        [Category.Incident] = "Инцидент"
    };

    private static readonly Dictionary<Source, string> SourceNames = new()
    {
        [Source.Email] = "Email",
        [Source.Phone] = "Телефон",
        [Source.Portal] = "Портал",
        [Source.Internal] = "Внутренний"
    };

    private static readonly Dictionary<Impact, string> ImpactNames = new()
    {
        [Impact.Individual] = "Отдельный сотрудник",
        [Impact.Team] = "Команда",
        [Impact.Department] = "Отдел",
        [Impact.Company] = "Компания"
    };

    private static readonly Dictionary<ChangeType, string> ChangeTypeNames = new()
    {
        [ChangeType.Creation] = "Создание",
        [ChangeType.Update] = "Изменение",
        [ChangeType.StatusChange] = "Смена статуса",
        [ChangeType.Assignment] = "Назначение",
        [ChangeType.Comment] = "Комментарий",
        [ChangeType.FileAttach] = "Файл"
    };

    private static readonly Dictionary<ProductType, string> ProductTypeNames = new()
    {
        [ProductType.Software] = "ПО",
        [ProductType.Hardware] = "Оборудование",
        [ProductType.Embedded] = "Встраиваемое"
    };

    public static string ToDisplayString(this TicketStatus status) =>
        TicketStatusNames.GetValueOrDefault(status, status.ToString());

    public static string ToDisplayString(this Priority priority) =>
        PriorityNames.GetValueOrDefault(priority, priority.ToString());

    public static string ToDisplayString(this Category category) =>
        CategoryNames.GetValueOrDefault(category, category.ToString());

    public static string ToDisplayString(this Source source) =>
        SourceNames.GetValueOrDefault(source, source.ToString());

    public static string ToDisplayString(this Impact impact) =>
        ImpactNames.GetValueOrDefault(impact, impact.ToString());

    public static string ToDisplayString(this ChangeType changeType) =>
        ChangeTypeNames.GetValueOrDefault(changeType, changeType.ToString());

    public static string ToDisplayString(this ProductType productType) =>
        ProductTypeNames.GetValueOrDefault(productType, productType.ToString());

    // ─── Роли (строковые) ────────────────────────────────────────────────

    private static readonly Dictionary<string, string> RoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = "Администратор",
        ["Engineer"] = "Инженер",
        ["Manager"] = "Менеджер",
        ["Applicant"] = "Заявитель"
    };

    public static string RoleToDisplayString(string? role) =>
        string.IsNullOrEmpty(role) ? "—" : RoleNames.GetValueOrDefault(role, role);
}
