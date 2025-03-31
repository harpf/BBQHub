using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace BBQHub.Pages.Ranglisten
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Event> Events { get; set; } = new();

        public async Task OnGetAsync()
        {
            Events = await _context.Events
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }
    }
}
