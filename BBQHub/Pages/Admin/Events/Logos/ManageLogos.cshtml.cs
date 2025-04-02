using BBQHub.Domain.Entities;
using BBQHub.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using BBQHub.Pages.Admin.Events.Logos.ViewModels;

namespace BBQHub.Pages.Admin.Events
{
    public class ManageLogosModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ManageLogosModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty(SupportsGet = true)]
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? Upload { get; set; }
        [BindProperty]
        public List<LogoSelectionModel> LogoSelection { get; set; } = new();


        [BindProperty]
        public LogoType SelectedType { get; set; }

        public List<EventLogo> ExistingLogos { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var ev = await _context.Events.FindAsync(EventId);
            if (ev == null) return NotFound();

            EventName = ev.Name;

            ExistingLogos = await _context.EventLogos
                .Where(e => e.EventId == EventId)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Upload != null && Upload.Length > 0)
            {
                // Validierung: Ist SelectedType ein gültiger LogoType?
                if (!Enum.IsDefined(typeof(LogoType), SelectedType))
                {
                    ModelState.AddModelError("", "Ungültiger Logo-Typ ausgewählt.");
                    return Page();
                }

                // Pfade vorbereiten
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(Upload.FileName)}";
                var uploadPath = Path.Combine(_env.WebRootPath, "images", "eventlogos");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Upload.CopyToAsync(stream);
                }

                // Logo speichern
                var logo = new EventLogo
                {
                    EventId = EventId,
                    Type = SelectedType,
                    FilePath = $"/images/eventlogos/{fileName}"
                };

                _context.EventLogos.Add(logo);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { eventId = EventId });
        }
        public async Task<IActionResult> OnPostDeleteLogoAsync(int logoId)
        {
            var logo = await _context.EventLogos.FirstOrDefaultAsync(l => l.Id == logoId && l.EventId == EventId);
            if (logo != null)
            {
                var path = Path.Combine(_env.WebRootPath, logo.FilePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

                _context.EventLogos.Remove(logo);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { eventId = EventId });
        }
        public async Task<IActionResult> OnPostSetActiveAsync()
        {
            var logos = await _context.EventLogos.Where(l => l.EventId == EventId).ToListAsync();

            foreach (var item in LogoSelection)
            {
                var logo = logos.FirstOrDefault(l => l.Id == item.Id);
                if (logo != null)
                {
                    logo.IsActive = item.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { eventId = EventId });
        }

    }
}