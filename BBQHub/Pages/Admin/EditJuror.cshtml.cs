using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Admin
{
    public class EditJurorModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditJurorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public BBQHub.Domain.Entities.Juror Juror { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Juror = await _context.Juroren.FindAsync(id);

            if (Juror == null)
                return NotFound();

            return Page();
        }
        public List<string> AvailableCountries { get; set; } = new()
        {
            "Schweiz", "Deutschland", "Österreich", "Italien", "Frankreich"
        };

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Attach(Juror).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Juroren.Any(j => j.Id == Juror.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("/Admin/Juroren");
        }
    }
}
