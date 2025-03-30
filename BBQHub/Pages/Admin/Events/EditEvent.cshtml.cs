using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;

namespace BBQHub.Pages.Admin.Events
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Event Input { get; set; } = new();

        public List<SelectListItem> Managers { get; set; } = new();
        public List<Durchgang> Durchgaenge { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound();

            Input = ev;

            Durchgaenge = await _context.Durchgaenge
                .Where(d => d.EventId == id)
                .Include(d => d.Kriterien)
                .ToListAsync();

            Managers = await _userManager.Users
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Events.FindAsync(Input.Id);
            if (existing == null)
                return NotFound();

            existing.Name = Input.Name;
            existing.Address = Input.Address;
            existing.Description = Input.Description;
            existing.ManagerId = Input.ManagerId;

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/Index");
        }
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var existing = await _context.Events.FindAsync(Input.Id);
            if (existing == null)
                return NotFound();

            _context.Events.Remove(existing);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/Index");
        }

    }
}
