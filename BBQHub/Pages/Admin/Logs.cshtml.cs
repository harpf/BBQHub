using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BBQHub.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LogsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<LogEntry> Logs { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedLevel { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public List<string> AvailableLevels { get; set; } = new();
        public List<string> CustomKeywords { get; set; } = new()
        {
            "E-Mail", "Warnung", "Info", "Datenbank", "Verbindung", "Authentifizierung", "Timeout", "Server", "Benutzer"
        };

        // Pagination - add page number
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync()
        {
            var query = _context.Logs.AsQueryable();

            // 📆 Filter nach Datum
            if (StartDate.HasValue)
                query = query.Where(l => l.TimeStamp >= StartDate.Value);

            if (EndDate.HasValue)
                query = query.Where(l => l.TimeStamp <= EndDate.Value);

            // 🔍 Suche
            if (!string.IsNullOrWhiteSpace(SearchTerm))
                query = query.Where(l =>
                    (l.Message ?? "").Contains(SearchTerm) ||
                    (l.Exception ?? "").Contains(SearchTerm) ||
                    (l.Properties ?? "").Contains(SearchTerm)
                    );

            // 🧮 Level-Filter
            if (!string.IsNullOrWhiteSpace(SelectedLevel))
                query = query.Where(l => l.Level == SelectedLevel);

            // Verfügbare Level (inkl. "Alle" für alle Level anzeigen)
            AvailableLevels = await _context.Logs
                .Select(l => l.Level)
                .Distinct()
                .ToListAsync();

            // Pagination: Berechne die Anzahl der Einträge pro Seite
            const int pageSize = 20;
            var skip = (PageNumber - 1) * pageSize;

            Logs = await query
                .OrderByDescending(l => l.TimeStamp)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        // Hilfsmethode für das Setzen der Seite
        public string SetPageUrl(int pageNumber)
        {
            return Url.Page("./Logs", new { SearchTerm, SelectedLevel, StartDate, EndDate, PageNumber = pageNumber });
        }
    }
}