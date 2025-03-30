using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using BBQHub.Infrastructure.Data;
using BBQHub.Domain.Entities;

namespace BBQHub.Pages.Admin.Events
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Event Input { get; set; } = new();

        public List<SelectListItem> Managers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Managers = _userManager.Users
                .Select(u => new SelectListItem { Value = u.Id, Text = u.Email })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Events.Add(Input);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Events/Index");
        }
    }
}
