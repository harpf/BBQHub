using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BBQHub.Pages.Admin.Teams
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Team Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public void OnGet()
        {
            Input.EventId = EventId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Teams.Add(Input);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Admin/Teams/Index");
        }
    }
}
