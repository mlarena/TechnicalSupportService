using System.ComponentModel.DataAnnotations;

namespace TechnicalSupportService.Data.Entities;

public class TicketNumberCounter
{
    [Key, MaxLength(7)]
    public string YearMonth { get; set; } = string.Empty;

    public int LastNumber { get; set; } = 0;
}
