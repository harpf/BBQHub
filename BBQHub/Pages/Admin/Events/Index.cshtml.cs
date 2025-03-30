using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BBQHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Admin.Events
{
    [Authorize(Roles = "Admin,EventOrganizer")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<BBQHub.Domain.Entities.Event> Events { get; set; } = new();

        public async Task OnGetAsync()
        {
            Events = await _context.Events.ToListAsync();
        }
        public async Task<IActionResult> OnPostToggleStatusAsync(int id, EventStatus newStatus)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();

            ev.Status = newStatus;
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

    }

}
