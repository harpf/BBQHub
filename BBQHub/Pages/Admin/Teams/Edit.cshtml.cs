using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBQHub.Pages.Admin.Teams
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Team Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
                return NotFound();

            Input = team;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var existing = await _context.Teams.FindAsync(Input.Id);
            if (existing == null)
                return NotFound();

            existing.Name = Input.Name;
            existing.Ansprechpartner = Input.Ansprechpartner;
            existing.KontaktEmail = Input.KontaktEmail;

            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Teams/Index");
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var team = await _context.Teams.FindAsync(Input.Id);
            if (team == null)
                return NotFound();

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Teams/Index");
        }
    }
}
