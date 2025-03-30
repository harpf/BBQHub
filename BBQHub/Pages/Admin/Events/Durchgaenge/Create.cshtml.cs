using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BBQHub.Pages.Admin.Events.Durchgaenge
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Durchgang Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public IActionResult OnGet()
        {
            Input.EventId = EventId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var letzteNummer = await _context.Durchgaenge
                .Where(d => d.EventId == Input.EventId)
                .Select(d => d.Durchgangsnummer)
                .ToListAsync();

            Input.Durchgangsnummer = letzteNummer.Any() ? letzteNummer.Max() + 1 : 1;


            _context.Durchgaenge.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/EditEvent", new { id = Input.EventId });
        }
    }
}