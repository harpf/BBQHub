using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    public Event? NaechstesEvent { get; set; }

    public List<Event> AktiveSpontaneEvents { get; set; } = new();

    public async Task OnGetAsync()
    {
        NaechstesEvent = await _context.Events
            .Where(e => e.StartDate > DateTime.Now)
            .OrderBy(e => e.StartDate)
            .FirstOrDefaultAsync();

        AktiveSpontaneEvents = await _context.Events
            .Where(e => e.Status == EventStatus.Aktiv && e.Typ == EventType.SpontanTeilnahme)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

}
