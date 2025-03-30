using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBQHub.Pages.Admin.Events.Kriterien
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int DurchgangId { get; set; }

        [BindProperty]
        public Kriterium Input { get; set; } = new();

        public IActionResult OnGet()
        {
            Input.DurchgangId = DurchgangId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Input.Durchgang");
            if (!ModelState.IsValid) return Page();

            _context.Kriterien.Add(Input);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Events/Index");
        }
    }
}
