using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using BBQHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
    }

}
