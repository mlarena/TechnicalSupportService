using Microsoft.EntityFrameworkCore;
using TechnicalSupportService.Core.Interfaces;
using TechnicalSupportService.Data.Context;
using TechnicalSupportService.Data.Entities;

namespace TechnicalSupportService.SUTP.Services;

public class NumberGeneratorService : INumberGeneratorService
{
    private readonly ApplicationDbContext _db;
    public NumberGeneratorService(ApplicationDbContext db) => _db = db;

    public async Task<string> GenerateNextNumberAsync()
    {
        var yearMonth = DateTime.UtcNow.ToString("yyyy-MM");
        var counter = await _db.TicketNumberCounters.FirstOrDefaultAsync(c => c.YearMonth == yearMonth);

        if (counter == null)
        {
            counter = new TicketNumberCounter { YearMonth = yearMonth, LastNumber = 1 };
            _db.TicketNumberCounters.Add(counter);
        }
        else
        {
            counter.LastNumber++;
        }

        await _db.SaveChangesAsync();
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        return $"{year:0000}_{month:00}_{counter.LastNumber:D3}";
    }
}
