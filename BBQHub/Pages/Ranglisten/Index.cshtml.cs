using BBQHub.Application.Events;
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

        //public List<Event> Events { get; set; } = new();
        public List<EventViewModel> Events { get; set; } = new();

        public async Task OnGetAsync()
        {
            Events = await _context.Events
                .Join(_context.Users,
                      e => e.ManagerId,
                      u => u.Id,
                      (e, u) => new EventViewModel
                      {
                          Id = e.Id,
                          Name = e.Name,
                          ManagerName = u.UserName,
                          Status = e.Status
                      })
                .ToListAsync();
                    }
    }
}
